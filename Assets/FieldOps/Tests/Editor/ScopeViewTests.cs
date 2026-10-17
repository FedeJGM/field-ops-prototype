using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps.Tests {
 public class ScopeViewTests {
  const string MaskPath="Assets/FieldOps/Textures/ScopeMask.png";
  static Rifle OpenRifle() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);return Object.FindFirstObjectByType<Rifle>(); }
  static Texture2D Mask() { var t=new Texture2D(2,2,TextureFormat.RGBA32,false);Assert.IsTrue(t.LoadImage(File.ReadAllBytes(MaskPath)),"The scope mask is a valid image.");return t; }
  [Test] public void OnlyTheSniperUsesTheScopeView() { var rifle=OpenRifle();foreach(var w in rifle.weapons)Assert.AreEqual(w.nameKey=="weaponSniper",w.scopeView,w.nameKey); }
  [Test] public void TheMaskIsASquareSpriteWithAClearCircleAndSolidCorners() {
   var importer=(TextureImporter)AssetImporter.GetAtPath(MaskPath);Assert.IsNotNull(importer,"The mask exists in the project.");Assert.AreEqual(TextureImporterType.Sprite,importer.textureType);Assert.IsFalse(importer.mipmapEnabled);Assert.IsTrue(importer.alphaIsTransparency);
   var t=Mask();Assert.AreEqual(t.width,t.height);int n=t.width;Assert.GreaterOrEqual(n,256);
   float A(float dx,float dy)=>t.GetPixel(Mathf.Clamp(Mathf.RoundToInt(n*.5f+dx*n),0,n-1),Mathf.Clamp(Mathf.RoundToInt(n*.5f+dy*n),0,n-1)).a;
   Assert.Less(A(0,0),.02f,"Centre is clear.");Assert.Less(A(.36f,0),.02f,"Well inside the hole is clear.");Assert.Less(A(0,-.36f),.02f);Assert.Greater(A(.47f,0),.97f,"Just outside the rim is solid.");Assert.Greater(A(-.49f,.49f),.99f,"Corners are solid.");Assert.Greater(A(0,.49f),.99f);
   float rim=A(.43f,0);Assert.That(rim,Is.InRange(.02f,.98f),"The rim has a soft edge.");
  }
  [Test] public void TheOverlayIsBehindTheHudDoesNotBlockTouchesAndUsesTheUiMaterial() {
   OpenRifle();var hudT=GameObject.Find("HUD").transform;var overlay=hudT.Find("Scope overlay");Assert.IsNotNull(overlay);Assert.AreEqual(0,overlay.GetSiblingIndex(),"Drawn first, so buttons and labels stay on top.");
   var comp=overlay.GetComponent<ScopeOverlay>();Assert.IsNotNull(comp);Assert.IsNotNull(comp.circle);Assert.IsNotNull(comp.left);Assert.IsNotNull(comp.right);Assert.IsNotNull(comp.horizontal);Assert.IsNotNull(comp.vertical);Assert.IsNotNull(comp.group);Assert.AreEqual(0f,comp.group.alpha,"Hidden until the sniper aims.");
   foreach(var g in overlay.GetComponentsInChildren<Graphic>(true)){Assert.IsFalse(g.raycastTarget,g.name+" must not catch touches.");Assert.IsNotNull(g.material,g.name);Assert.AreEqual("UI/Default",g.material.shader.name,g.name+" uses the serialized UI shader, never magenta.");}
   Assert.AreSame(Object.FindFirstObjectByType<PrototypeHud>().scope,comp);
   var sprite=comp.circle.GetComponent<Image>().sprite;Assert.IsNotNull(sprite);Assert.AreEqual(MaskPath,AssetDatabase.GetAssetPath(sprite));
  }
 }
}
