using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps.Tests {
 public class ReticleSceneTests {
  [Test] public void SavedSceneReticleHasFourVisibleCenteredNonBlockingGeometryBars() {
   EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);
   var root=GameObject.Find("Crosshair");Assert.That(root,Is.Not.Null);
   Assert.That(root.GetComponent<Text>(),Is.Null,"Reticle must not depend on font glyph availability.");
   Assert.That(root.transform.parent.GetComponent<Canvas>(),Is.Not.Null,"Reticle must align to camera viewport, not asymmetric safe-area center."); var rect=root.GetComponent<RectTransform>();Assert.That(rect.anchorMin,Is.EqualTo(new Vector2(.5f,.5f)));Assert.That(rect.anchorMax,Is.EqualTo(new Vector2(.5f,.5f)));Assert.That(rect.anchoredPosition,Is.EqualTo(Vector2.zero));
   var bars=root.GetComponentsInChildren<Image>();Assert.That(bars.Length,Is.EqualTo(4));
   foreach(var bar in bars) {
    Assert.That(bar.gameObject.activeInHierarchy,Is.True);Assert.That(bar.raycastTarget,Is.False);Assert.That(bar.color.a,Is.GreaterThan(.9f));
    Assert.That(bar.rectTransform.rect.width,Is.GreaterThan(0));Assert.That(bar.rectTransform.rect.height,Is.GreaterThan(0));
    Assert.That(bar.rectTransform.anchoredPosition.magnitude,Is.GreaterThan(4),"Keep a clear aiming gap.");
    var outline=bar.GetComponent<Outline>();Assert.That(outline,Is.Not.Null);Assert.That(outline.effectColor.a,Is.GreaterThan(.9f));
    using(var helper=new VertexHelper()) { typeof(Image).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic,null,new[]{typeof(VertexHelper)},null).Invoke(bar,new object[]{helper});Assert.That(helper.currentVertCount,Is.GreaterThanOrEqualTo(4),"Image must produce a nonempty geometry mesh."); }
   }
  }
 }
}


