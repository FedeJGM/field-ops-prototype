using UnityEngine;
namespace FieldOps {
 // Player options stored locally, like the language choice.
 public static class GameSettings {
  const string AimToggleKey="aimToggle",KeepCrossKey="keepCross";
  public static bool AimToggle { get => PlayerPrefs.GetInt(AimToggleKey,0)==1; set {PlayerPrefs.SetInt(AimToggleKey,value?1:0);PlayerPrefs.Save();} }
  public static bool KeepCrossWhileAiming { get => PlayerPrefs.GetInt(KeepCrossKey,0)==1; set {PlayerPrefs.SetInt(KeepCrossKey,value?1:0);PlayerPrefs.Save();} }
 }
}
