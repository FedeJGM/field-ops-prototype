using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace FieldOps.Editor {
 // Fail before expensive Android compilation if runtime UI dependencies are removed.
 public sealed class UiBuildValidation : IPreprocessBuildWithReport {
  public int callbackOrder => 0;
  public void OnPreprocessBuild(BuildReport report) { if(report.summary.platform==BuildTarget.Android)Validate(); }
  public static void Validate() {
   var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);var shaders=settings.FindProperty("m_AlwaysIncludedShaders");bool included=false;
   for(int i=0;i<shaders.arraySize;i++){var shader=shaders.GetArrayElementAtIndex(i).objectReferenceValue as Shader;if(shader!=null&&shader.name=="UI/Default")included=true;}
   if(!included)throw new BuildFailedException("UI/Default shader is missing from Always Included Shaders. Rebuild the training range before Android delivery.");
   const string path="Assets/FieldOps/Scenes/TrainingRange.unity";var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;
   if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   try {
    int count=0;
    foreach(var root in scene.GetRootGameObjects())foreach(var graphic in root.GetComponentsInChildren<Graphic>(true)) {
     count++;var material=new SerializedObject(graphic).FindProperty("m_Material").objectReferenceValue as Material;
     if(material==null||AssetDatabase.GetAssetPath(material)!="Assets/FieldOps/Materials/UiDefault.mat"||material.shader==null||material.shader.name!="UI/Default")throw new BuildFailedException("UI shader dependency is invalid on "+graphic.name+". Rebuild the training range.");
     if(!material.HasProperty("_Stencil")||!material.HasProperty("_ColorMask"))throw new BuildFailedException("Required UI stencil/color-mask support is missing.");
    }
    if(count<10)throw new BuildFailedException("Training scene UI is incomplete.");
    Debug.Log("FIELD_OPS_UI_BUILD_VALIDATED UI/Default explicitly included; "+count+" graphics reference saved material.");
   }finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
  }
 }
}
