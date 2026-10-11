using UnityEngine;
using UnityEngine.EventSystems;
namespace FieldOps {
 // Expire before input and re-check at the actual shot boundary in LateUpdate.
 [DefaultExecutionOrder(-200)]
 public sealed class TrainingSession : MonoBehaviour {
  public PrototypeInput input;
  public Rifle rifle;
  public TrainingTarget target;
  public TrainingRound Round { get; } = new TrainingRound();
  double lastTime;
  bool paused,unfocused;
  public bool SettingsOpen {get;private set;}
  public void SetSettingsOpen(bool value) {Synchronize();SettingsOpen=value;ApplySuspension();}
  public bool GameplayEnabled => Round.Phase==RoundPhase.Active&&!Round.Suspended;
  void Awake() {lastTime=Time.unscaledTimeAsDouble;}
  void Update() {Synchronize();}
  public void Synchronize() {
   double now=Time.unscaledTimeAsDouble;var before=Round.Phase;
   Round.Tick(now-lastTime);lastTime=now;
   if(before==RoundPhase.Active&&Round.Phase!=before)ClearControls();
  }
  public bool StartRound(string seconds) {
   if(!Round.Start(seconds))return false;
   lastTime=Time.unscaledTimeAsDouble;target.ResetDamage();rifle.ResetShots();ClearControls();
   if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
   return true;
  }
  public void Configure() {Round.Configure();ClearControls();}
  public bool TryRecordHit() {Synchronize();return Round.RecordHit();}
  void ClearControls() {if(input!=null)input.ClearGameplay();if(rifle!=null)rifle.ClearVisuals();}
  void OnApplicationPause(bool value) {SetPause(value);}
  void OnApplicationFocus(bool value) {SetFocus(value);}
  public void SetPause(bool value) {Synchronize();paused=value;ApplySuspension();}
  public void SetFocus(bool focused) {Synchronize();unfocused=!focused;ApplySuspension();}
  void ApplySuspension() {Round.SetSuspended(paused||unfocused||SettingsOpen);lastTime=Time.unscaledTimeAsDouble;ClearControls();}
 }
}
