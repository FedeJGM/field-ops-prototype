using UnityEngine;
namespace FieldOps {
 // Deterministic shotgun spread: pellet 0 flies straight, the others sit on a ring (tangent-plane offsets: x right, y up).
 public static class PelletPattern {
  public static Vector2 Offset(int index,int count,float spreadDegrees) {
   if(count<=1||index<=0||index>=count||spreadDegrees<=0f)return Vector2.zero;
   float angle=(index-1)*Mathf.PI*2f/(count-1),radius=Mathf.Tan(spreadDegrees*Mathf.Deg2Rad);
   return new Vector2(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);
  }
 }
}
