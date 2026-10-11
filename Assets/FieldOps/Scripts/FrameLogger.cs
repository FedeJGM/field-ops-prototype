using System.Globalization;
using UnityEngine;
namespace FieldOps {
 // Logs frame statistics every few seconds, only in development builds, so performance can be read with adb logcat.
 public sealed class FrameLogger : MonoBehaviour {
  public float intervalSeconds=10f;
  public FrameTimeStats Stats { get; }=new FrameTimeStats();
  float elapsed;
  static string Format(float value) { return value.ToString("0.0",CultureInfo.InvariantCulture); }
  public string Report() { return "FIELD_OPS_FPS avg="+Format(Stats.AverageFps)+" p95ms="+Format(Stats.PercentileMs(95))+" maxms="+Format(Stats.MaxMs)+" n="+Stats.Count; }
  void Update() {
   Stats.Add(Time.unscaledDeltaTime);elapsed+=Time.unscaledDeltaTime;
   if(elapsed<intervalSeconds)return;
   elapsed=0;if(Debug.isDebugBuild)Debug.Log(Report());Stats.Reset();
  }
 }
}
