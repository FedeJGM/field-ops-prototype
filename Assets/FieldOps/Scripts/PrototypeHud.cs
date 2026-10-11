using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps {
 public sealed class PrototypeHud : MonoBehaviour {
  public RectTransform safeRoot, joystick, stick, fire, sprint, reload, aim;
  public Button languageButton, resetButton, startButton, replayButton, configureButton;
  public Text title, status, moveLabel, lookLabel, fireLabel, sprintLabel, resetLabel, hitLabel, help;
  public Text roundTitle,durationLabel,durationError,scoreRule,timerLabel,resultTitle,resultScore;
  public GameObject menuBlocker,configurationPanel,resultsPanel,crosshair;
  public InputField durationInput;
  public Text reloadLabel,ammoLabel,aimLabel,settingsTitle,aimModeLabel,crossLabel;
  public Button settingsButton,aimModeButton,crossButton,closeSettingsButton;
  public GameObject settingsPanel,aimDot;
  public Rifle rifle;
  public TrainingSession session;
  public TrainingTarget target;
  bool spanish,invalidDuration; Rect lastSafe; int lastWidth,lastHeight;
  void Start() {
   spanish=PlayerPrefs.GetInt("language",0)==1;languageButton.onClick.AddListener(ToggleLanguage);
   settingsButton.onClick.AddListener(OpenSettings);closeSettingsButton.onClick.AddListener(CloseSettings);aimModeButton.onClick.AddListener(ToggleAimMode);crossButton.onClick.AddListener(ToggleCross);settingsPanel.SetActive(false);
   startButton.onClick.AddListener(StartSelectedRound);replayButton.onClick.AddListener(Replay);configureButton.onClick.AddListener(Configure);
   durationInput.onValueChanged.AddListener(_=>{if(invalidDuration){invalidDuration=false;Refresh();}});
   resetButton.gameObject.SetActive(false);durationInput.text=session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture);Refresh();UpdateSafeArea();
  }
  void Update() {UpdateSafeArea();RefreshRound();}
  void StartSelectedRound() {invalidDuration=!session.StartRound(durationInput.text);Refresh();}
  void Replay() {session.StartRound(session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture));Refresh();}
  void Configure() {session.Configure();durationInput.text=session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture);invalidDuration=false;Refresh();}
  void UpdateSafeArea() {Rect safe=Screen.safeArea;if(safe==lastSafe&&lastWidth==Screen.width&&lastHeight==Screen.height)return;lastSafe=safe;lastWidth=Screen.width;lastHeight=Screen.height;safeRoot.anchorMin=new Vector2(safe.x/Screen.width,safe.y/Screen.height);safeRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);safeRoot.offsetMin=safeRoot.offsetMax=Vector2.zero;}
  public bool Contains(RectTransform rect,Vector2 point) {return rect.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(rect,point,null);}
  public bool IsMenu(Vector2 point) {return !session.GameplayEnabled||Contains((RectTransform)languageButton.transform,point)||Contains((RectTransform)settingsButton.transform,point);}
  public Vector2 JoystickValue(Vector2 screenPoint) {RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick,screenPoint,null,out var p);var value=Vector2.ClampMagnitude(p/80f,1);stick.anchoredPosition=value*65;return value;}
  public void ResetStick() {stick.anchoredPosition=Vector2.zero;}
  public void ShowButtons(bool firing,bool sprinting) {fire.GetComponent<Image>().color=firing?new Color(1,.65f,.25f,.9f):new Color(.7f,.32f,.12f,.8f);sprint.GetComponent<Image>().color=sprinting?new Color(.5f,.75f,.35f,.9f):new Color(.15f,.22f,.15f,.85f);}
  void OpenSettings() {settingsPanel.SetActive(true);session.SetSettingsOpen(true);Refresh();}
  void CloseSettings() {settingsPanel.SetActive(false);session.SetSettingsOpen(false);Refresh();}
  void ToggleAimMode() {GameSettings.AimToggle=!GameSettings.AimToggle;rifle.Aim.Toggle=GameSettings.AimToggle;rifle.Aim.Reset();Refresh();}
  void ToggleCross() {GameSettings.KeepCrossWhileAiming=!GameSettings.KeepCrossWhileAiming;Refresh();}
  void RefreshSettingsText() {
   aimLabel.text=GameText.Get("aim",spanish);settingsTitle.text=GameText.Get("settings",spanish);settingsButton.GetComponentInChildren<Text>().text=GameText.Get("settings",spanish);closeSettingsButton.GetComponentInChildren<Text>().text=GameText.Get("close",spanish);
   aimModeLabel.text=GameText.Get("aimMode",spanish)+": "+GameText.Get(GameSettings.AimToggle?"toggle":"hold",spanish);crossLabel.text=GameText.Get("keepCross",spanish)+": "+GameText.Get(GameSettings.KeepCrossWhileAiming?"on":"off",spanish);
  }
  void ToggleLanguage() {spanish=!spanish;PlayerPrefs.SetInt("language",spanish?1:0);Refresh();}
  void Refresh() {
   title.text="FIELD OPS / PROTOTYPE";moveLabel.text=GameText.Get("move",spanish);lookLabel.text=GameText.Get("look",spanish);fireLabel.text=GameText.Get("fire",spanish);sprintLabel.text=GameText.Get("sprint",spanish);resetLabel.text=GameText.Get("reset",spanish);help.text=GameText.Get("help",spanish);languageButton.GetComponentInChildren<Text>().text=spanish?"ENGLISH":"ESPAÑOL";
   roundTitle.text=GameText.Get("roundTitle",spanish);durationLabel.text=GameText.Get("duration",spanish);scoreRule.text=GameText.Get("scoreRule",spanish);durationError.text=invalidDuration?GameText.Get("durationError",spanish):"";startButton.GetComponentInChildren<Text>().text=GameText.Get("start",spanish);replayButton.GetComponentInChildren<Text>().text=GameText.Get("replay",spanish);configureButton.GetComponentInChildren<Text>().text=GameText.Get("configure",spanish);resultTitle.text=GameText.Get("finished",spanish);reloadLabel.text=GameText.Get("reload",spanish);RefreshSettingsText();RefreshRound();
  }
  void RefreshRound() {
   bool active=session.Round.Phase==RoundPhase.Active;bool aiming=active&&rifle.Aim.Aiming;crosshair.SetActive(active&&(!aiming||GameSettings.KeepCrossWhileAiming));aimDot.SetActive(aiming);aim.gameObject.SetActive(active);menuBlocker.SetActive(!active);configurationPanel.SetActive(session.Round.Phase==RoundPhase.Configuration);resultsPanel.SetActive(session.Round.Phase==RoundPhase.Results);
   joystick.gameObject.SetActive(active);moveLabel.gameObject.SetActive(active);fire.gameObject.SetActive(active);sprint.gameObject.SetActive(active);lookLabel.gameObject.SetActive(active);help.gameObject.SetActive(active&&!Application.isMobilePlatform);hitLabel.gameObject.SetActive(active);timerLabel.gameObject.SetActive(active);reload.gameObject.SetActive(active);ammoLabel.gameObject.SetActive(active);
   ammoLabel.text=rifle.Ammo.IsReloading?GameText.Get("reloading",spanish):GameText.Get("ammo",spanish)+" "+rifle.Ammo.Count+" / ∞";reload.GetComponent<Image>().color=rifle.Ammo.IsReloading?new Color(.5f,.5f,.5f,.6f):(rifle.Ammo.IsEmpty?new Color(.85f,.25f,.2f,.9f):new Color(.12f,.2f,.28f,.85f));
   status.text=GameText.Get(session.Round.Suspended&&active?"paused":"status",spanish);
   hitLabel.text=GameText.Get("score",spanish)+": "+session.Round.Score+"   |   "+GameText.Get("damage",spanish)+": "+target.Damage.TotalDamage;
   timerLabel.text=GameText.Get("time",spanish)+"  "+session.Round.Clock;
   resultScore.text=GameText.Get("score",spanish)+": "+session.Round.Score+"\n"+GameText.Get("duration",spanish)+": "+session.Round.DurationSeconds;
  }
 }
}
