using System;
namespace FieldOps {
 public enum FpsBand { Good, Fair, Poor }
 // Pure on-screen FPS measurement: frames counted over a short window so the number is readable and does not flicker.
 public sealed class FpsMeter {
  readonly double interval;double elapsed;int frames;
  public FpsMeter(float intervalSeconds=.5f) {
   if(float.IsNaN(intervalSeconds)||float.IsInfinity(intervalSeconds)||intervalSeconds<=0)throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
   interval=intervalSeconds;
  }
  public float Fps { get; private set; }
  // Returns true when a window completed and Fps was refreshed.
  public bool Add(float seconds) {
   if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return false;
   frames++;elapsed+=seconds;
   if(elapsed<interval)return false;
   Fps=(float)(frames/elapsed);frames=0;elapsed=0;return true;
  }
  public static FpsBand Band(float fps) { return fps>=50f?FpsBand.Good:(fps>=30f?FpsBand.Fair:FpsBand.Poor); }
 }
}
