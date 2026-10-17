namespace FieldOps {
 // Sprint by joystick: pushing the stick to its edge sprints; the release threshold is lower so a wobbling thumb does not flicker.
 public sealed class SprintRule {
  public const float Engage=.92f,Release=.8f;
  public bool Engaged { get; private set; }
  public bool Update(float magnitude) {
   if(float.IsNaN(magnitude)||float.IsInfinity(magnitude)||magnitude<0)magnitude=0;
   Engaged=Engaged?magnitude>=Release:magnitude>=Engage;
   return Engaged;
  }
  public void Reset() { Engaged=false; }
 }
}
