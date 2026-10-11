using System;
namespace FieldOps {
 // Pure aim-down-sights rules: hold or toggle, no aiming while sprinting or when play is not allowed.
 public sealed class AimControl {
  public const float ZoomedFov=40f;
  public const float SensitivityWhileAiming=.5f;
  public const double BlendSeconds=.15;
  public float ZoomFov { get; set; }=ZoomedFov;
  public bool Toggle { get; set; }
  public bool Aiming { get; private set; }
  public float Blend { get; private set; }
  public float LookScale => 1f+(SensitivityWhileAiming-1f)*Blend;
  public float Fov(float normalFov) { return normalFov+(ZoomFov-normalFov)*Blend; }
  public void Step(bool held,bool pressed,bool sprinting,bool allowed,double seconds) {
   if(!allowed||sprinting)Aiming=false;
   else if(Toggle) {if(pressed)Aiming=!Aiming;}
   else Aiming=held;
   if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   float step=(float)(seconds/BlendSeconds);
   Blend=Aiming?Math.Min(1f,Blend+step):Math.Max(0f,Blend-step);
  }
  public void Reset() {Aiming=false;Blend=0;}
 }
}
