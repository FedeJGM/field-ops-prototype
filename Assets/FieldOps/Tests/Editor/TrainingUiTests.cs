using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps.Tests {
 public class TrainingUiTests {
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
  [TestCase("reload","RELOAD","RECARGAR")][TestCase("reloading","RELOADING","RECARGANDO")][TestCase("ammo","AMMO","MUNICIÓN")]
  public void AmmoLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
  [TestCase("start","START ROUND","INICIAR PARTIDA")][TestCase("finished","ROUND COMPLETE","PARTIDA TERMINADA")][TestCase("replay","PLAY AGAIN","VOLVER A JUGAR")][TestCase("configure","CHANGE DURATION","CAMBIAR DURACIÓN")][TestCase("time","TIME","TIEMPO")][TestCase("score","SCORE","PUNTOS")]
  public void RoundLabelsAreBilingual(string key,string en,string es) {Assert.AreEqual(en,GameText.Get(key,false));Assert.AreEqual(es,GameText.Get(key,true));}
 }
}
