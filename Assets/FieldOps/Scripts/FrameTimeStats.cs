using System;
using System.Collections.Generic;
using System.Linq;
namespace FieldOps {
 // Pure frame-time statistics (milliseconds) used to measure real performance on the phone.
 public sealed class FrameTimeStats {
  readonly List<float> samples=new List<float>();
  public int Count => samples.Count;
  public void Add(float seconds) { if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return;samples.Add(seconds*1000f); }
  public float AverageMs => Count==0?0:samples.Sum()/Count;
  public float AverageFps => AverageMs<=0?0:1000f/AverageMs;
  public float MaxMs => Count==0?0:samples.Max();
  // Nearest-rank percentile of the sorted frame times.
  public float PercentileMs(float percentile) {
   if(Count==0)return 0;
   var sorted=samples.OrderBy(x=>x).ToArray();int rank=(int)Math.Ceiling(percentile/100.0*sorted.Length);
   return sorted[Math.Max(0,Math.Min(sorted.Length-1,rank-1))];
  }
  public void Reset() { samples.Clear(); }
 }
}
