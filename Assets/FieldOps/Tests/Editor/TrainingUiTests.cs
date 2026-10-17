using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps.Tests {
 public class TrainingUiTests {
  // The canvas scales with the screen (reference 1600x900, match 0.5), so tall panels overflow on very wide phones.
  static float CanvasHeight(float screenWidth,float screenHeight) { float scale=Mathf.Pow(2f,.5f*Mathf.Log(screenWidth/1600f,2f)+.5f*Mathf.Log(screenHeight/900f,2f));return screenHeight/scale; }
  [Test] public void SettingsPanelFitsEvenOnAVeryWidePhoneAndItsRowsStayInside() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   Assert.AreEqual(804.7f,CanvasHeight(2712,1220),1f,"The helper matches the measured phone.");
   float worst=Mathf.Min(CanvasHeight(2712,1220),CanvasHeight(2400,1080),CanvasHeight(2520,1080));   // 20:9 and 21:9
   var panel=(RectTransform)GameObject.Find("HUD").transform.Find("Safe area/Settings panel");Assert.IsNotNull(panel,"The settings panel is inactive in the saved scene, so it is found by path.");Assert.LessOrEqual(panel.sizeDelta.y+20f,worst,"The settings panel is "+panel.sizeDelta.y+" tall; the narrowest phone canvas is "+worst+".");
   foreach(RectTransform child in panel) {if(!child.gameObject.activeSelf&&child.name!="Sprint sensitivity")continue;float top=child.anchoredPosition.y+child.sizeDelta.y*.5f,bottom=child.anchoredPosition.y-child.sizeDelta.y*.5f;Assert.LessOrEqual(top,panel.sizeDelta.y*.5f+.1f,child.name+" top");Assert.GreaterOrEqual(bottom,-panel.sizeDelta.y*.5f-.1f,child.name+" bottom");}
   Assert.IsNotNull(panel.Find("Sprint sensitivity"));
  }
  [Test] public void SavedSceneHasNumericDurationAndRoundActions() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var root=GameObject.Find("HUD");var field=root.GetComponentInChildren<InputField>(true);Assert.IsNotNull(field,"Duration must be editable in the actual saved mobile scene.");
   Assert.AreEqual(InputField.CharacterValidation.None,field.characterValidation);Assert.AreEqual(TouchScreenKeyboardType.NumberPad,field.keyboardType);Assert.AreEqual("60",field.text);Assert.AreEqual(0,field.characterLimit,"Do not turn 1000 into 100 by silent truncation.");
   foreach(var name in new[]{"Start round","Replay round","Configure round"})Assert.IsTrue(System.Array.Exists(root.GetComponentsInChildren<Button>(true),b=>b.name==name),name);
  }
  [Test] public void SavedSceneHasReloadButtonAndAmmoCounterWiredToRifle() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var hud=Object.FindFirstObjectByType<PrototypeHud>();Assert.IsNotNull(hud.reload,"Reload touch button must be saved in the scene.");Assert.IsNotNull(hud.reloadLabel);Assert.IsNotNull(hud.ammoLabel);Assert.IsNotNull(hud.rifle);
   foreach(var graphic in hud.GetComponentsInChildren<Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [Test] public void SavedSceneHasAimButtonAndSettingsMenuWithSerializedMaterials() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var hud=Object.FindFirstObjectByType<PrototypeHud>();Assert.IsNotNull(hud.aim,"ADS touch button must be saved in the scene.");Assert.IsNotNull(hud.aimLabel);Assert.IsNotNull(hud.settingsButton);Assert.IsNotNull(hud.settingsPanel);Assert.IsNotNull(hud.aimModeButton);Assert.IsNotNull(hud.crossButton);Assert.IsNotNull(hud.closeSettingsButton);Assert.IsNotNull(hud.aimDot);
   Assert.IsFalse(hud.settingsPanel.activeSelf,"Settings start closed.");Assert.IsFalse(hud.aimDot.GetComponent<Graphic>().raycastTarget,"The aiming dot must not intercept touches.");
   foreach(var graphic in hud.GetComponentsInChildren<Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [Test] public void SavedSceneWiresGameAudioToRifleAndTargetWithASingleListener() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var hud=Object.FindFirstObjectByType<PrototypeHud>();var target=Object.FindFirstObjectByType<TrainingTarget>();Assert.IsNotNull(hud.rifle.sfx,"Rifle needs the saved GameAudio.");Assert.AreSame(hud.rifle.sfx,target.sfx);
   Assert.AreEqual(1,Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length);
  }
  [Test] public void SavedSceneHasLayoutEditorControlsAndLabelsThatFollowTheirButtons() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var hud=Object.FindFirstObjectByType<PrototypeHud>();Assert.IsNotNull(hud.editLayoutButton);Assert.IsNotNull(hud.saveLayoutButton);Assert.IsNotNull(hud.resetLayoutButton);Assert.IsNotNull(hud.cancelLayoutButton);Assert.IsNotNull(hud.layoutPanel);Assert.IsNotNull(hud.layoutHint);
   Assert.IsFalse(hud.layoutPanel.activeSelf,"The layout toolbar starts hidden.");Assert.AreSame(hud.joystick,hud.moveLabel.transform.parent,"The joystick label must move with the joystick.");Assert.AreSame(hud.reload,hud.ammoLabel.transform.parent,"The ammo counter must move with the reload button.");
   foreach(var graphic in hud.GetComponentsInChildren<Graphic>(true))Assert.IsNotNull(graphic.material,graphic.name+" must keep the serialized UiDefault material.");
  }
  [TestCase("editLayout","EDIT LAYOUT","EDITAR BOTONES")][TestCase("save","SAVE","GUARDAR")][TestCase("resetLayout","RESET","RESTABLECER")][TestCase("cancel","CANCEL","CANCELAR")][TestCase("layoutHint","Drag the buttons where you want them.","Arrastra los botones adonde quieras.")][TestCase("layoutOverlap","Buttons overlap or cover a menu button. Move them apart.","Los botones se superponen o tapan un bot\u00f3n del men\u00fa. Sep\u00e1ralos.")]
  public void LayoutLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
  [TestCase("aim","AIM","APUNTAR")][TestCase("settings","SETTINGS","AJUSTES")][TestCase("aimMode","ADS MODE","MODO DE APUNTADO")][TestCase("hold","HOLD","MANTENER")][TestCase("toggle","TOGGLE","ALTERNAR")][TestCase("keepCross","CROSS WHILE AIMING","MIRA DE CRUZ AL APUNTAR")][TestCase("on","ON","SÍ")][TestCase("off","OFF","NO")][TestCase("close","CLOSE","CERRAR")]
  public void AimLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
  [TestCase("reload","RELOAD","RECARGAR")][TestCase("reloading","RELOADING","RECARGANDO")][TestCase("ammo","AMMO","MUNICIÓN")]
  public void AmmoLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
  [TestCase("start","START ROUND","INICIAR PARTIDA")][TestCase("finished","ROUND COMPLETE","PARTIDA TERMINADA")][TestCase("replay","PLAY AGAIN","VOLVER A JUGAR")][TestCase("configure","CHANGE DURATION","CAMBIAR DURACIÓN")][TestCase("time","TIME","TIEMPO")][TestCase("score","SCORE","PUNTOS")]
  public void RoundLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
 }
}
