using UnityEngine;
using UnityEngine.UI;
namespace FieldOps {
 public sealed class PrototypeHud : MonoBehaviour {
  public RectTransform safeRoot, joystick, stick, fire, sprint;
  public Button languageButton, resetButton;
  public Text title, status, moveLabel, lookLabel, fireLabel, sprintLabel, resetLabel, hitLabel, help;
  public TrainingTarget target;
  bool spanish; Rect lastSafe; int lastWidth,lastHeight;
  void Start() { spanish=PlayerPrefs.GetInt("language",0)==1; languageButton.onClick.AddListener(ToggleLanguage); resetButton.onClick.AddListener(ResetTarget); Refresh(); UpdateSafeArea(); }
  void Update() { UpdateSafeArea(); if(target!=null)hitLabel.text=GameText.Get("hits",spanish)+": "+target.Damage.Hits+"   |   "+GameText.Get("damage",spanish)+": "+target.Damage.TotalDamage; }
  void UpdateSafeArea() { Rect safe=Screen.safeArea; if(safe==lastSafe&&lastWidth==Screen.width&&lastHeight==Screen.height)return; lastSafe=safe;lastWidth=Screen.width;lastHeight=Screen.height; safeRoot.anchorMin=new Vector2(safe.x/Screen.width,safe.y/Screen.height);safeRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);safeRoot.offsetMin=safeRoot.offsetMax=Vector2.zero; }
  public bool Contains(RectTransform rect, Vector2 point) { return RectTransformUtility.RectangleContainsScreenPoint(rect,point,null); }
  public bool IsMenu(Vector2 point) { return Contains((RectTransform)languageButton.transform,point)||Contains((RectTransform)resetButton.transform,point); }
  public Vector2 JoystickValue(Vector2 screenPoint) { RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick,screenPoint,null,out var p); var value=Vector2.ClampMagnitude(p/80f,1); stick.anchoredPosition=value*65;return value; }
  public void ResetStick() { stick.anchoredPosition=Vector2.zero; }
  public void ShowButtons(bool firing,bool sprinting) { fire.GetComponent<Image>().color=firing?new Color(1,.65f,.25f,.9f):new Color(.7f,.32f,.12f,.8f); sprint.GetComponent<Image>().color=sprinting?new Color(.5f,.75f,.35f,.9f):new Color(.15f,.22f,.15f,.85f); }
  void ToggleLanguage() { spanish=!spanish;PlayerPrefs.SetInt("language",spanish?1:0);Refresh(); }
  void ResetTarget() { target.ResetDamage(); }
  void Refresh() { title.text="FIELD OPS / PROTOTYPE";status.text=GameText.Get("status",spanish);moveLabel.text=GameText.Get("move",spanish);lookLabel.text=GameText.Get("look",spanish);fireLabel.text=GameText.Get("fire",spanish);sprintLabel.text=GameText.Get("sprint",spanish);resetLabel.text=GameText.Get("reset",spanish);help.text=GameText.Get("help",spanish);help.gameObject.SetActive(!Application.isMobilePlatform);languageButton.GetComponentInChildren<Text>().text=spanish?"ENGLISH":"ESPAÑOL"; }
 }
}
