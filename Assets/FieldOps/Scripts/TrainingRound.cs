using System;
using System.Globalization;
namespace FieldOps {
 public enum RoundPhase { Configuration, Active, Results }
 // Pure rules: time comes from the active-play clock, never wall-clock background time.
 public sealed class TrainingRound {
  public RoundPhase Phase { get; private set; } = RoundPhase.Configuration;
  public int DurationSeconds { get; private set; } = 60;
  public double RemainingSeconds { get; private set; }
  public int Score { get; private set; }
  public bool Suspended { get; private set; }
  public static bool TryDuration(string text,out int seconds) {
   seconds=0;if(string.IsNullOrEmpty(text))return false;
   foreach(char c in text)if(c<'0'||c>'9')return false;
   return int.TryParse(text,NumberStyles.None,CultureInfo.InvariantCulture,out seconds)&&seconds>=1&&seconds<=300;
  }
  public bool Start(string text) {
   if(Phase==RoundPhase.Active||!TryDuration(text,out int seconds))return false;
   DurationSeconds=seconds;RemainingSeconds=seconds;Score=0;Phase=RoundPhase.Active;return true;
  }
  public void Tick(double seconds) {
   if(Phase!=RoundPhase.Active||Suspended||seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   RemainingSeconds=Math.Max(0,RemainingSeconds-seconds);
   if(RemainingSeconds<=0)Phase=RoundPhase.Results;
  }
  public bool RecordHit() {if(Phase!=RoundPhase.Active||Suspended||RemainingSeconds<=0)return false;Score++;return true;}
  public void SetSuspended(bool value) {Suspended=value;}
  public void ResetScore() {if(Phase==RoundPhase.Active)Score=0;}
  public void Configure() {if(Phase!=RoundPhase.Active)Phase=RoundPhase.Configuration;}
  public string Clock {get {int seconds=(int)Math.Ceiling(RemainingSeconds);return (seconds/60).ToString("00",CultureInfo.InvariantCulture)+":"+(seconds%60).ToString("00",CultureInfo.InvariantCulture);}}
 }
}
