using UnityEngine;
namespace FieldOps {
 // Player options stored locally, like the language choice.
 public static class GameSettings {
  const string AimToggleKey="aimToggle",KeepCrossKey="keepCross";
  public static bool AimToggle { get => PlayerPrefs.GetInt(AimToggleKey,0)==1; set {PlayerPrefs.SetInt(AimToggleKey,value?1:0);PlayerPrefs.Save();} }
  const string HudLayoutKey="hudLayout";
  public static HudLayout HudLayout {
   get => HudLayout.FromJson(PlayerPrefs.GetString(HudLayoutKey,""));
   set {var json=value==null?"":value.ToJson();if(json.Length==0)PlayerPrefs.DeleteKey(HudLayoutKey);else PlayerPrefs.SetString(HudLayoutKey,json);PlayerPrefs.Save();}
  }
  public static bool ShowFps { get => PlayerPrefs.GetInt("showFps",1)==1; set {PlayerPrefs.SetInt("showFps",value?1:0);PlayerPrefs.Save();} }
  public static bool SprintByJoystick { get => PlayerPrefs.GetInt("sprintMode",1)==1; set {PlayerPrefs.SetInt("sprintMode",value?1:0);PlayerPrefs.Save();} }
  public const int DefaultWeapon=1;
  public static int SelectedWeapon { get => PlayerPrefs.GetInt("weapon",DefaultWeapon); set {PlayerPrefs.SetInt("weapon",value);PlayerPrefs.Save();} }
  public static bool KeepCrossWhileAiming { get => PlayerPrefs.GetInt(KeepCrossKey,0)==1; set {PlayerPrefs.SetInt(KeepCrossKey,value?1:0);PlayerPrefs.Save();} }
 }
}
