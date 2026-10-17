using UnityEngine;
namespace FieldOps {
 // Full-screen scope view: black sides that close in around a circular opening, with a thin reticle.
 // The circle sprite is a square as tall as the screen; two plain bars cover whatever is left on each side.
 public sealed class ScopeOverlay : MonoBehaviour {
  public RectTransform circle,left,right,horizontal,vertical;public CanvasGroup group;public float holeFraction=.86f;public float lineThickness=2f;
  public float Alpha { get; private set; }
  public void SetBlend(float blend) {
   Alpha=float.IsNaN(blend)?0f:Mathf.Clamp01(blend);if(group!=null)group.alpha=Alpha;
  }
  void LateUpdate() {
   var area=((RectTransform)transform).rect;float side=area.height;
   circle.sizeDelta=new Vector2(side,side);
   // Each bar is anchored to its half of the screen and stops at the circle's edge (one extra unit so no seam shows), whatever the aspect ratio.
   left.offsetMin=Vector2.zero;left.offsetMax=new Vector2(-(side*.5f-1f),0f);right.offsetMin=new Vector2(side*.5f-1f,0f);right.offsetMax=Vector2.zero;
   float opening=side*holeFraction;horizontal.sizeDelta=new Vector2(opening,lineThickness);vertical.sizeDelta=new Vector2(lineThickness,opening);
  }
 }
}
