namespace FieldOps {
 // The weapon is carried raised while sprinting; this is only the blend (0 = firing pose, 1 = sprint pose).
 public sealed class SprintPose {
  public const double InSeconds=.22,OutSeconds=.12,FireOutSeconds=.08;
  public float Blend { get; private set; }
  // Sprinting and actually moving, with no aiming (the zoom wins) and no slide (the slide keeps the normal grip).
  public bool Wants(bool sprinting,bool moving,bool aiming,bool sliding) { return sprinting&&moving&&!aiming&&!sliding; }
  public void Step(bool wantsPose,bool firing,double seconds) {
   if(seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   if(wantsPose&&!firing)Blend=System.Math.Min(1f,Blend+(float)(seconds/InSeconds));
   else Blend=System.Math.Max(0f,Blend-(float)(seconds/(firing?FireOutSeconds:OutSeconds)));
  }
  public void Reset() { Blend=0; }
 }
}
