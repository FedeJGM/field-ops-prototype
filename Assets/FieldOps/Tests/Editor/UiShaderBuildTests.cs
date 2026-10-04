using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps.Tests {
 public class UiShaderBuildTests {
  [Test] public void UiDefaultShaderIsExplicitlyIncludedForAllBuilds() {
   var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);var shaders=settings.FindProperty("m_AlwaysIncludedShaders");bool found=false;
   for(int i=0;i<shaders.arraySize;i++) { var shader=shaders.GetArrayElementAtIndex(i).objectReferenceValue as Shader;if(shader!=null&&shader.name=="UI/Default")found=true; }
   Assert.That(found,Is.True,"Runtime default UGUI shader was omitted from Android shader compile list; pin UI/Default explicitly.");
  }
  [Test] public void EverySavedUiGraphicReferencesBuildVisibleUiMaterial() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);var hud=GameObject.Find("HUD");Assert.That(hud,Is.Not.Null);var graphics=hud.GetComponentsInChildren<Graphic>(true);Assert.That(graphics.Length,Is.GreaterThan(10));
   foreach(var graphic in graphics) {
    var material=new SerializedObject(graphic).FindProperty("m_Material").objectReferenceValue as Material;
    Assert.That(material,Is.Not.Null,graphic.name+" must not rely on unreferenced runtime default material.");Assert.That(AssetDatabase.GetAssetPath(material),Is.EqualTo("Assets/FieldOps/Materials/UiDefault.mat"));Assert.That(material.shader.name,Is.EqualTo("UI/Default"));
    Assert.That(material.HasProperty("_Stencil"),Is.True);Assert.That(material.HasProperty("_ColorMask"),Is.True);
   }
  }
 }
}
