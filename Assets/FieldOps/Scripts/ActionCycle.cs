using System;
namespace FieldOps {
 // The hand-worked action after a shot (bolt of the sniper rifle, pump of the shotgun), played during the fire cooldown.
 // Roll: the weapon turns out, stays turned and turns back. Pull: the bolt or pump goes back in the middle and returns forward.
 public sealed class ActionCycle {
  public const double MinSeconds=.25;
  double elapsed;bool soundDone;
  public bool Active { get; private set; }
  public double Duration { get; private set; }
  public float Progress { get { return Active?(float)Math.Min(1.0,elapsed/Duration):0f; } }
  static float Smooth(float from,float to,float x) { x=Math.Max(0f,Math.Min(1f,(x-from)/(to-from)));return x*x*(3f-2f*x); }
  public float Roll { get { if(!Active)return 0f;float u=Progress;return Smooth(.08f,.28f,u)*(1f-Smooth(.72f,.92f,u)); } }
  public float Pull { get { if(!Active)return 0f;float u=Progress;return Smooth(.30f,.42f,u)*(1f-Smooth(.46f,.60f,u)); } }
  public void Start(double seconds) {
   bool valid=!double.IsNaN(seconds)&&!double.IsInfinity(seconds);Duration=Math.Max(MinSeconds,valid?seconds:MinSeconds);elapsed=0;soundDone=false;Active=true;
  }
  public void Step(double seconds) {
   if(!Active||double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)return;
   elapsed+=seconds;if(elapsed>=Duration){Active=false;elapsed=0;}
  }
  // True once, the first time the cycle has gone past the given fraction of its length (used to start the sound).
  public bool SoundDue(float fraction) { if(!Active||soundDone||Progress<fraction)return false;soundDone=true;return true; }
  public void Cancel() { Active=false;elapsed=0; }
 }
}
