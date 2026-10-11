using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace FieldOps.Tests {
 public class MapTests {
  const string Folder="Assets/FieldOps/ThirdParty/PolyHaven/";
  const string WorldRoot="Training field — editable geometry";
  static readonly string[] Textures={"brown_mud_dry","brushed_concrete_03","asphalt_02","container_side","green_metal_rust","corrugated_iron_03","hessian_230"};
  static void Open() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single); }
  static Transform[] ByPrefix(string prefix) { return Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name.StartsWith(prefix)).ToArray(); }
  static string TextureName(Material m,string property="_MainTex") { var t=m.GetTexture(property);return t==null?"":Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(t)); }
  [Test] public void SkyIsAnOvercastPanoramaWithFogAndASkyboxCamera() {
   Open();Assert.IsNotNull(RenderSettings.skybox);Assert.AreEqual("Skybox/Panoramic",RenderSettings.skybox.shader.name);StringAssert.StartsWith("belfast_open_field_sky",TextureName(RenderSettings.skybox));
   Assert.AreEqual(CameraClearFlags.Skybox,Camera.main.clearFlags);Assert.IsTrue(RenderSettings.fog);Assert.AreEqual(AmbientMode.Trilight,RenderSettings.ambientMode,"Cheap, predictable ambient light on the phone.");
  }
  [Test] public void GroundAndFiringLaneUseTheCreditedTexturesTiledAtARealisticScale() {
   Open();var ground=GameObject.Find("Ground").GetComponent<Renderer>().sharedMaterial;var lane=GameObject.Find("Firing lane").GetComponent<Renderer>().sharedMaterial;
   StringAssert.StartsWith("brown_mud_dry",TextureName(ground));StringAssert.StartsWith("asphalt_02",TextureName(lane));Assert.Greater(ground.mainTextureScale.x,5f,"Dirt must repeat across the 44 m field.");Assert.Greater(lane.mainTextureScale.y,2f);
  }
  [Test] public void TheBaseHasContainersSandbagsATowerAHangarAndDrums() {
   Open();Assert.GreaterOrEqual(ByPrefix("Shipping container").Length,5);Assert.GreaterOrEqual(ByPrefix("Sandbag wall").Length,4);Assert.IsNotNull(GameObject.Find("Watchtower"));Assert.IsNotNull(GameObject.Find("Hangar"));Assert.GreaterOrEqual(ByPrefix("Oil drum").Length,6);Assert.GreaterOrEqual(ByPrefix("Tire stack").Length,2);
  }
  [Test] public void ContainersSandbagsAndWallsUseTexturesAndNormalMaps() {
   Open();var container=ByPrefix("Shipping container")[0].GetComponent<Renderer>().sharedMaterial;StringAssert.StartsWith("container_side",TextureName(container));StringAssert.StartsWith("container_side",TextureName(container,"_BumpMap"),"Containers carry a normal map.");
   var sandbag=ByPrefix("Sandbag wall")[0].GetComponent<Renderer>().sharedMaterial;StringAssert.StartsWith("hessian_230",TextureName(sandbag));StringAssert.StartsWith("hessian_230",TextureName(sandbag,"_BumpMap"));
   var wall=GameObject.Find("Boundary west").GetComponent<Renderer>().sharedMaterial;StringAssert.StartsWith("brushed_concrete_03",TextureName(wall));
   var hangar=GameObject.Find("Hangar").GetComponentInChildren<Renderer>().sharedMaterial;StringAssert.StartsWith("corrugated_iron_03",TextureName(hangar));
   var tower=GameObject.Find("Watchtower").GetComponentInChildren<Renderer>().sharedMaterial;StringAssert.StartsWith("green_metal_rust",TextureName(tower));
  }
  [Test] public void TexturesAreImportedSmallAndCompressedForAndroid() {
   var files=Directory.GetFiles(Folder,"*.jpg");Assert.GreaterOrEqual(files.Length,10);
   foreach(var file in files) {
    var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);int cap=path.Contains("_sky_")?2048:1024;Assert.LessOrEqual(importer.maxTextureSize,cap,path);
    var android=importer.GetPlatformTextureSettings("Android");Assert.IsTrue(android.overridden,path+": Android override");Assert.LessOrEqual(android.maxTextureSize,cap,path);Assert.AreEqual(TextureImporterFormat.ASTC_6x6,android.format,path);
    if(path.Contains("_nor_gl"))Assert.AreEqual(TextureImporterType.NormalMap,importer.textureType,path);
   }
  }
  [Test] public void SkyIsSharpEnoughForAPhoneScreen() {
   Open();var texture=RenderSettings.skybox.GetTexture("_MainTex");Assert.GreaterOrEqual(texture.width,2048,"A 1024 px panorama is stretched about 12x on the phone and the distant trees look blurry.");
   var path=AssetDatabase.GetAssetPath(texture);var importer=(TextureImporter)AssetImporter.GetAtPath(path);Assert.GreaterOrEqual(importer.maxTextureSize,2048);Assert.LessOrEqual(importer.maxTextureSize,2048,"2K is the agreed size: sharper than 1K without the memory cost of 4K.");
   var android=importer.GetPlatformTextureSettings("Android");Assert.IsTrue(android.overridden);Assert.AreEqual(2048,android.maxTextureSize);
  }
  [Test] public void SkyTextureHasNoMipmapsSoItsWrapSeamCannotShow() { var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+"belfast_open_field_sky_2k.jpg");Assert.IsFalse(importer.mipmapEnabled);Assert.AreEqual(TextureWrapMode.Repeat,importer.wrapModeU);Assert.AreEqual(TextureWrapMode.Clamp,importer.wrapModeV); }
  [Test] public void TheFiringLaneAndPlayerStartStayClear() {
   Open();Physics.SyncTransforms();var allowed=new[]{"Ground","Firing lane","Lane marking","Firing line pad","Target platform","Player"};
   var blockers=Physics.OverlapBox(new Vector3(0,1.2f,-5.5f),new Vector3(2.8f,1.1f,6.5f),Quaternion.identity).Select(c=>c.name).Where(n=>!allowed.Any(a=>n.StartsWith(a))).ToArray();
   Assert.IsEmpty(blockers,"Nothing may stand between the player and the target: "+string.Join(", ",blockers));
  }
  [Test] public void ThePlayFieldWallsStillBoundTheCharacterAt22Meters() {
   Open();foreach(var name in new[]{"Boundary west","Boundary east","Boundary south","Boundary north"}) {var go=GameObject.Find(name);Assert.IsNotNull(go,name);Assert.IsNotNull(go.GetComponent<Collider>(),name);}
   Assert.AreEqual(-22f,GameObject.Find("Boundary west").transform.position.x,.01f);Assert.AreEqual(22f,GameObject.Find("Boundary east").transform.position.x,.01f);
  }
  [Test] public void SceneryIsBatchedAndStaysInsideAMobileBudget() {
   Open();var root=GameObject.Find(WorldRoot).transform;var renderers=root.GetComponentsInChildren<MeshRenderer>(true);Assert.Less(renderers.Length,450);
   foreach(var r in renderers)Assert.IsTrue((GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.BatchingStatic)!=0,r.name+" must be batching-static.");
   int triangles=root.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3);Assert.Less(triangles,120000,"Triangle budget for the scenery.");
   var materials=renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count();Assert.Less(materials,40,"Few materials keep batching effective.");
  }
  [Test] public void SceneryCreditsAreRecorded() {
   var credits=File.ReadAllText("CREDITS.md");StringAssert.Contains("Poly Haven",credits);StringAssert.Contains("Assets/FieldOps/ThirdParty/PolyHaven/",credits);StringAssert.Contains("CC0",credits);
   var notice=File.ReadAllText(Folder+"NOTICE.txt");foreach(var t in Textures)StringAssert.Contains(t,notice);StringAssert.Contains("belfast_open_field",notice);StringAssert.Contains("derivative",notice);
  }
 }
}
