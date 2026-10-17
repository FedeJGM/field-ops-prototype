using UnityEngine;
namespace FieldOps {
 // How far the joystick must be pushed before it sprints.
 public enum SprintSensitivity { High=0, Medium=1, Low=2 }
 // Sprint by joystick: pushing the stick far enough and mostly forward sprints (sideways and backward only walk, so a quick strafe never breaks the aim).
 // The release thresholds are looser than the engage ones so a wobbling thumb does not flicker.
 public sealed class SprintRule {
  public const float Engage=.92f,Release=.8f;   // the medium level, kept as the reference values
  public const float ForwardConeDegrees=40f,ReleaseConeDegrees=50f;
  const float ReleaseGap=.12f;
  SprintSensitivity level=SprintSensitivity.Medium;
  public bool Engaged { get; private set; }
  public SprintSensitivity Level { get => level; set { if(level!=value){level=value;Engaged=false;} } }
  public static float EngageFor(SprintSensitivity level) { switch(level){case SprintSensitivity.High:return .80f;case SprintSensitivity.Low:return .97f;default:return Engage;} }
  public static float ReleaseFor(SprintSensitivity level) { return EngageFor(level)-ReleaseGap; }
  // stick.x is right, stick.y is forward.
  public bool Update(Vector2 stick) {
   if(float.IsNaN(stick.x)||float.IsNaN(stick.y)||float.IsInfinity(stick.x)||float.IsInfinity(stick.y))stick=Vector2.zero;
   float magnitude=stick.magnitude;
   float angle=magnitude<1e-4f?180f:(stick.y<=0?90f+(stick.y<0?Mathf.Atan2(-stick.y,Mathf.Abs(stick.x))*Mathf.Rad2Deg:0f):Mathf.Atan2(Mathf.Abs(stick.x),stick.y)*Mathf.Rad2Deg);   // degrees away from straight forward
   Engaged=Engaged?magnitude>=ReleaseFor(level)&&angle<=ReleaseConeDegrees:magnitude>=EngageFor(level)&&angle<=ForwardConeDegrees;
   return Engaged;
  }
  // A plain magnitude means "pushed straight forward".
  public bool Update(float magnitude) {
   if(float.IsNaN(magnitude)||float.IsInfinity(magnitude)||magnitude<0)magnitude=0;
   return Update(new Vector2(0,magnitude));
  }
  public void Reset() { Engaged=false; }
 }
}
