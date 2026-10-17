using System;
namespace FieldOps {
 // Pure aim-down-sights rules: hold or toggle, no aiming while sprinting or when play is not allowed.
 public sealed class AimControl {
  public const float ZoomedFov=40f;
  public const float SensitivityWhileAiming=.5f;
  public const double BlendSeconds=.15;
  public float ZoomFov { get; set; }=ZoomedFov;
  public bool Toggle { get; set; }
  public bool Suppressed { get; set; }   // held back (for example during a bolt cycle) while the wish to aim is kept
  public bool Aiming { get; private set; }
  public float Blend { get; private set; }
  public float LookScale => 1f+(SensitivityWhileAiming-1f)*Blend;
  public float Fov(float normalFov) { return normalFov+(ZoomFov-normalFov)*Blend; }
  bool wanted;   // what the player asks for (hold or toggle); Suppressed only hides it for a while
  public void Step(bool held,bool pressed,bool sprinting,bool allowed,double seconds) {
   if(!allowed||sprinting)wanted=false;
   else if(Toggle) {if(pressed)wanted=!wanted;}
   else wanted=held;
   Aiming=wanted&&!Suppressed;
   if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   float step=(float)(seconds/BlendSeconds);
   Blend=Aiming?Math.Min(1f,Blend+step):Math.Max(0f,Blend-step);
  }
  public void Reset() {Aiming=false;wanted=false;Blend=0;}
 }
}
