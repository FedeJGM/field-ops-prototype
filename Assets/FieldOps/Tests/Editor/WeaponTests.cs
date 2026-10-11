using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace FieldOps.Tests {
 public class PelletPatternTests {
  [Test] public void ASinglePelletAndTheFirstPelletFlyStraight() { Assert.AreEqual(Vector2.zero,PelletPattern.Offset(0,1,4));Assert.AreEqual(Vector2.zero,PelletPattern.Offset(0,8,4)); }
  [Test] public void OtherPelletsSitOnARingOfTheRequestedSpread() { float r=Mathf.Tan(4*Mathf.Deg2Rad);for(int i=1;i<8;i++)Assert.AreEqual(r,PelletPattern.Offset(i,8,4).magnitude,1e-5f,"pellet "+i); }
  [Test] public void RingPelletsAreDistinctAndBalanced() { var points=Enumerable.Range(1,7).Select(i=>PelletPattern.Offset(i,8,4)).ToArray();for(int a=0;a<points.Length;a++)for(int b=a+1;b<points.Length;b++)Assert.Greater((points[a]-points[b]).magnitude,.001f);var sum=points.Aggregate(Vector2.zero,(s,p)=>s+p);Assert.AreEqual(0,sum.magnitude,1e-5f,"The ring is symmetric, so the average aim stays on target."); }
  [Test] public void NoSpreadOrInvalidIndicesGiveNoOffset() { Assert.AreEqual(Vector2.zero,PelletPattern.Offset(3,8,0));Assert.AreEqual(Vector2.zero,PelletPattern.Offset(-1,8,4));Assert.AreEqual(Vector2.zero,PelletPattern.Offset(8,8,4));Assert.AreEqual(Vector2.zero,PelletPattern.Offset(1,1,4)); }
 }
 public class WeaponSettingsTests {
  [SetUp] public void Clean() {PlayerPrefs.DeleteKey("weapon");}
  [TearDown] public void CleanAfter() {PlayerPrefs.DeleteKey("weapon");}
  [Test] public void TheAssaultRifleIsTheDefaultWeapon() { Assert.AreEqual(1,GameSettings.DefaultWeapon);Assert.AreEqual(GameSettings.DefaultWeapon,GameSettings.SelectedWeapon); }
  [Test] public void TheSelectedWeaponPersists() { GameSettings.SelectedWeapon=3;Assert.AreEqual(3,PlayerPrefs.GetInt("weapon",-1));Assert.AreEqual(3,GameSettings.SelectedWeapon); }
 }
 public class WeaponCatalogTests {
  static readonly (string key,int mag,float interval,int damage,int pellets,float fov,float range,float minLength,float maxLength)[] Design={
   ("weaponPistol",12,.30f,20,1,50f,60f,.15f,.4f),("weaponRifle",30,.12f,25,1,40f,80f,.7f,1.1f),("weaponSmg",32,.07f,15,1,45f,60f,.4f,.8f),("weaponShotgun",6,.85f,15,8,50f,40f,.8f,1.3f),("weaponSniper",5,1.2f,100,1,20f,120f,1f,1.5f)};
  static Rifle OpenRifle() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);return UnityEngine.Object.FindFirstObjectByType<Rifle>(); }
  [Test] public void SceneHasFiveWeaponsInOrderWithTheDesignedStats() {
   var rifle=OpenRifle();Assert.AreEqual(Design.Length,rifle.weapons.Length);
   for(int i=0;i<Design.Length;i++) {var w=rifle.weapons[i];var d=Design[i];Assert.AreEqual(d.key,w.nameKey);Assert.AreEqual(d.mag,w.magazineSize,d.key);Assert.AreEqual(d.interval,w.fireInterval,.001f,d.key);Assert.AreEqual(d.damage,w.damage,d.key);Assert.AreEqual(d.pellets,w.pellets,d.key);Assert.AreEqual(d.fov,w.aimFov,.001f,d.key);Assert.AreEqual(d.range,w.range,.001f,d.key);Assert.AreEqual(w.reloadClip.length,w.reloadSeconds,.01f,d.key+": the reload lasts as long as its sound.");}
   Assert.AreEqual(4f,rifle.weapons[3].spreadDegrees,.001f);
  }
  [Test] public void TheDefaultWeaponKeepsTheOriginalRifleBehavior() { var rifle=OpenRifle();var w=rifle.weapons[GameSettings.DefaultWeapon];Assert.AreEqual("weaponRifle",w.nameKey);Assert.AreEqual(30,w.magazineSize);Assert.AreEqual(.12f,w.fireInterval,.001f);Assert.AreEqual(25,w.damage);Assert.AreEqual(40f,w.aimFov,.001f);Assert.AreEqual(80f,w.range,.001f);Assert.AreEqual(w.reloadClip.length,w.reloadSeconds,.01f);Assert.AreEqual(-.28f,w.aimOffset.x,.001f,"Aiming centers the gun horizontally.");}
  [Test] public void AimingKeepsTheRearOfEveryGunAboutThirtyEightCentimetersFromTheCamera() { var rifle=OpenRifle();foreach(var w in rifle.weapons){float rear=.46f+w.aimOffset.z-w.modelLength*.5f;Assert.AreEqual(.38f,rear,.01f,w.nameKey+": a gun glued to the lens would fill the screen.");} }
  [Test] public void AimingPutsTheTopOfEveryGunJustUnderTheEyeLine() { var rifle=OpenRifle();foreach(var w in rifle.weapons)Assert.That(w.aimOffset.y,Is.InRange(-.05f,.2f),w.nameKey+": the ADS lift must bring the sights near eye level, not leave the gun off-screen."); }
  [Test] public void EveryWeaponHasADistinctLocalizedName() { var rifle=OpenRifle();var names=rifle.weapons.Select(w=>GameText.Get(w.nameKey,false)).ToArray();var spanish=rifle.weapons.Select(w=>GameText.Get(w.nameKey,true)).ToArray();Assert.AreEqual(names.Length,names.Distinct().Count());Assert.AreEqual(spanish.Length,spanish.Distinct().Count());Assert.IsTrue(names.All(n=>n.Length>0)&&spanish.All(n=>n.Length>0)); }
  [Test] public void EveryWeaponUsesAnImportedModelFromTheCreditedPack() {
   var rifle=OpenRifle();foreach(var w in rifle.weapons) {Assert.IsNotNull(w.modelPrefab,w.nameKey);var path=AssetDatabase.GetAssetPath(w.modelPrefab);StringAssert.StartsWith("Assets/FieldOps/ThirdParty/QuaterniusGuns/",path,w.nameKey);Assert.IsTrue(File.Exists(path),path);}
   var credits=File.ReadAllText("CREDITS.md");StringAssert.Contains("Quaternius",credits);StringAssert.Contains("CC0",credits);StringAssert.Contains("Assets/FieldOps/ThirdParty/QuaterniusGuns/",credits);Assert.IsTrue(File.Exists("Assets/FieldOps/ThirdParty/QuaterniusGuns/NOTICE.txt"));
  }
  [Test] public void ModelsHaveRealisticFirstPersonLengths() { var rifle=OpenRifle();for(int i=0;i<Design.Length;i++)Assert.That(rifle.weapons[i].modelLength,Is.InRange(Design[i].minLength,Design[i].maxLength),Design[i].key); }
  [Test] public void ModelsAreInstancedUnderTheHolderWithOnlyTheDefaultActiveAndNeverBlockRaycasts() {
   var rifle=OpenRifle();Assert.AreEqual(Design.Length,rifle.models.Length);
   for(int i=0;i<rifle.models.Length;i++) {
    var m=rifle.models[i];Assert.AreSame(rifle.weapon,m.parent,rifle.weapons[i].nameKey);Assert.AreEqual(i==GameSettings.DefaultWeapon,m.gameObject.activeSelf,rifle.weapons[i].nameKey);
    var renderers=m.GetComponentsInChildren<Renderer>(true);Assert.Greater(renderers.Length,0);
    foreach(var r in renderers) {Assert.AreEqual(2,r.gameObject.layer,"Ignore Raycast layer");Assert.AreEqual(ShadowCastingMode.Off,r.shadowCastingMode);Assert.IsFalse(r.receiveShadows);}
    Assert.AreEqual(0,m.GetComponentsInChildren<Collider>(true).Length,"A weapon model must not have colliders.");
   }
  }
  [Test] public void ModelsStayUprightAndTheMuzzlePointsForward() {
   var rifle=OpenRifle();for(int i=0;i<rifle.models.Length;i++) {
    var model=rifle.models[i].GetChild(0);
    // FBX axes: the gun's vertical axis is the mesh +Z (the import rotation turns it up) and its muzzle is the mesh -X.
    Assert.Less(Vector3.Angle(model.TransformDirection(Vector3.forward),Vector3.up),1f,rifle.weapons[i].nameKey+": the gun must stand upright, not lie on its side.");
    Assert.Less(Vector3.Angle(model.TransformDirection(Vector3.left),Vector3.forward),1f,rifle.weapons[i].nameKey+": the muzzle must point forward.");
   }
  }
  [Test] public void SavedSceneLetsYouChangeWeaponFromTheResultsScreenAndTheSettingsMenu() {
   OpenRifle();var hud=UnityEngine.Object.FindFirstObjectByType<PrototypeHud>();
   foreach(var pair in new[]{(hud.resultsWeapon,hud.resultsPanel),(hud.settingsWeapon,hud.settingsPanel)}) {
    var ui=pair.Item1;Assert.IsNotNull(ui,"selector missing");Assert.IsNotNull(ui.name);Assert.IsNotNull(ui.stats);Assert.IsNotNull(ui.prev);Assert.IsNotNull(ui.next);
    Assert.IsTrue(ui.name.transform.IsChildOf(pair.Item2.transform));Assert.IsTrue(ui.next.transform.IsChildOf(pair.Item2.transform));
   }
   foreach(var graphic in hud.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [Test] public void SavedSceneHasTheWeaponSelectorInTheConfigurationMenu() {
   OpenRifle();var hud=UnityEngine.Object.FindFirstObjectByType<PrototypeHud>();Assert.IsNotNull(hud.weaponNameLabel);Assert.IsNotNull(hud.weaponStatsLabel);Assert.IsNotNull(hud.weaponPrevButton);Assert.IsNotNull(hud.weaponNextButton);
   Assert.IsTrue(hud.weaponNameLabel.transform.IsChildOf(hud.configurationPanel.transform),"The selector lives in the pre-round menu.");
   foreach(var graphic in hud.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [Test] public void SavedSceneHasTheFpsCounterAndItsSettingsButton() {
   OpenRifle();var hud=UnityEngine.Object.FindFirstObjectByType<PrototypeHud>();Assert.IsNotNull(hud.fpsLabel);Assert.IsNotNull(hud.fpsButton);Assert.IsTrue(hud.fpsButton.transform.IsChildOf(hud.settingsPanel.transform),"The switch lives in the settings menu.");Assert.IsFalse(hud.fpsLabel.raycastTarget,"The counter must not intercept touches.");
   foreach(var graphic in hud.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [TestCase("fps","FPS","FPS")][TestCase("fpsCounter","FPS COUNTER","CONTADOR DE FPS")]
  public void FpsLabelsAreBilingual(string key,string en,string es) { Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true)); }
  [TestCase("weaponPistol","PISTOL","PISTOLA")][TestCase("weaponRifle","ASSAULT RIFLE","FUSIL DE ASALTO")][TestCase("weaponSmg","SMG","SUBFUSIL")][TestCase("weaponShotgun","SHOTGUN","ESCOPETA")][TestCase("weaponSniper","SNIPER RIFLE","FRANCOTIRADOR")]
  public void WeaponNamesAreBilingualAndGeneric(string key,string en,string es) { Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true)); }
 }
}
