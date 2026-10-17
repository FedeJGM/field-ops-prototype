using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace FieldOps {
 public sealed class PrototypeHud : MonoBehaviour {
  public RectTransform safeRoot, joystick, stick, fire, sprint, reload, aim, jump, slide, crouch;public ScopeOverlay scope;public CanvasGroup headerGroup;public Text settingsSubtitle;public Button sprintSensButton;public Text sprintSensLabel;public Button sprintModeButton;public Text sprintModeLabel;public bool SprintByJoystick { get; private set; }public SprintSensitivity SprintLevel { get; private set; }
  public Button languageButton, resetButton, startButton, replayButton, configureButton;
  public Text jumpLabel, slideLabel, crouchLabel, title, status, moveLabel, lookLabel, fireLabel, sprintLabel, resetLabel, hitLabel, help;
  public Text roundTitle,durationLabel,durationError,scoreRule,timerLabel,resultTitle,resultScore;
  public GameObject menuBlocker,configurationPanel,resultsPanel,crosshair;
  public InputField durationInput;
  public Text reloadLabel,ammoLabel,aimLabel,settingsTitle,aimModeLabel,crossLabel;
  public Button settingsButton,aimModeButton,crossButton,closeSettingsButton;
  public GameObject settingsPanel;
  public Button editLayoutButton,saveLayoutButton,resetLayoutButton,cancelLayoutButton;
  public GameObject layoutPanel;public Text layoutHint;
  public Text weaponNameLabel,weaponStatsLabel;public Button weaponPrevButton,weaponNextButton;
  [System.Serializable] public sealed class WeaponSelectorUi {public Text name,stats;public Button prev,next;}
  public WeaponSelectorUi resultsWeapon,settingsWeapon;
  public Text fpsLabel;public Button fpsButton;
  readonly FpsMeter fpsMeter=new FpsMeter();bool showFps=true;
  public bool EditingLayout {get;private set;}
  Dictionary<string,RectTransform> movable;
  readonly Dictionary<string,Vector2> defaultPositions=new Dictionary<string,Vector2>();
  readonly Dictionary<string,Vector2> backupPositions=new Dictionary<string,Vector2>();
  struct LayoutDrag {public RectTransform rect;public Vector2 grab;}
  readonly Dictionary<int,LayoutDrag> drags=new Dictionary<int,LayoutDrag>();
  public Rifle rifle;
  public TrainingSession session;
  public TrainingTarget target;
  bool spanish,invalidDuration; Rect lastSafe; int lastWidth,lastHeight;
  void Start() {
   spanish=PlayerPrefs.GetInt("language",0)==1;languageButton.onClick.AddListener(ToggleLanguage);
   SprintByJoystick=GameSettings.SprintByJoystick;SprintLevel=GameSettings.SprintSensitivity;sprintModeButton.onClick.AddListener(ToggleSprintMode);sprintSensButton.onClick.AddListener(ToggleSprintSens);showFps=GameSettings.ShowFps;fpsButton.onClick.AddListener(ToggleFps);fpsLabel.text="-- FPS";
   settingsButton.onClick.AddListener(OpenSettings);closeSettingsButton.onClick.AddListener(CloseSettings);aimModeButton.onClick.AddListener(ToggleAimMode);crossButton.onClick.AddListener(ToggleCross);settingsPanel.SetActive(false);
   startButton.onClick.AddListener(StartSelectedRound);replayButton.onClick.AddListener(Replay);configureButton.onClick.AddListener(Configure);
   durationInput.onValueChanged.AddListener(_=>{if(invalidDuration){invalidDuration=false;Refresh();}});
   resetButton.gameObject.SetActive(false);durationInput.text=session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture);Refresh();UpdateSafeArea();
   movable=new Dictionary<string,RectTransform>{{"joystick",joystick},{"fire",fire},{"reload",reload},{"aim",aim},{"sprint",sprint},{"jump",jump},{"slide",slide},{"crouch",crouch},{"language",(RectTransform)languageButton.transform},{"fps",fpsLabel.rectTransform}};
   foreach(var pair in movable)defaultPositions[pair.Key]=pair.Value.anchoredPosition;
   editLayoutButton.onClick.AddListener(BeginEditLayout);saveLayoutButton.onClick.AddListener(()=>SaveLayout());foreach(var selector in Selectors()) {selector.prev.onClick.AddListener(()=>ChangeWeapon(-1));selector.next.onClick.AddListener(()=>ChangeWeapon(1));}resetLayoutButton.onClick.AddListener(ResetLayout);cancelLayoutButton.onClick.AddListener(CancelLayout);layoutPanel.SetActive(false);
   Canvas.ForceUpdateCanvases();ApplySavedLayout();
  }
  void Update() {UpdateSafeArea();RefreshRound();if(fpsMeter.Add(Time.unscaledDeltaTime))RefreshFpsText();fpsLabel.gameObject.SetActive(showFps||EditingLayout);languageButton.interactable=!EditingLayout;}   // while editing: the FPS counter can be placed even if hidden, and the language button is inert so a drag is never a click
  void StartSelectedRound() {invalidDuration=!session.StartRound(durationInput.text);Refresh();}
  void Replay() {session.StartRound(session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture));Refresh();}
  void Configure() {session.Configure();durationInput.text=session.Round.DurationSeconds.ToString(CultureInfo.InvariantCulture);invalidDuration=false;Refresh();}
  void UpdateSafeArea() {Rect safe=Screen.safeArea;if(safe==lastSafe&&lastWidth==Screen.width&&lastHeight==Screen.height)return;lastSafe=safe;lastWidth=Screen.width;lastHeight=Screen.height;safeRoot.anchorMin=new Vector2(safe.x/Screen.width,safe.y/Screen.height);safeRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);safeRoot.offsetMin=safeRoot.offsetMax=Vector2.zero;if(movable!=null&&!EditingLayout){Canvas.ForceUpdateCanvases();ApplySavedLayout();}}
  public bool Contains(RectTransform rect,Vector2 point) {return rect.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(rect,point,null);}
  public bool IsMenu(Vector2 point) {return !session.GameplayEnabled||Contains((RectTransform)languageButton.transform,point)||Contains((RectTransform)settingsButton.transform,point);}
  public Vector2 JoystickValue(Vector2 screenPoint) {RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick,screenPoint,null,out var p);var value=Vector2.ClampMagnitude(p/80f,1);stick.anchoredPosition=value*65;return value;}
  public void ResetStick() {stick.anchoredPosition=Vector2.zero;}
  public void ShowButtons(bool firing,bool sprinting) {stick.GetComponent<Image>().color=SprintByJoystick&&sprinting?new Color(.95f,.85f,.45f,.95f):new Color(.62f,.69f,.56f,.8f);fire.GetComponent<Image>().color=firing?new Color(1,.65f,.25f,.9f):new Color(.7f,.32f,.12f,.8f);sprint.GetComponent<Image>().color=sprinting?new Color(.5f,.75f,.35f,.9f):new Color(.15f,.22f,.15f,.85f);}
  // ---- Layout editor: saved offsets per movable button; untouched buttons keep their exact default place.
  void Place(RectTransform rect,Vector2 position) {rect.anchoredPosition=HudLayout.ClampInside(position,rect.anchorMin,rect.rect.size,safeRoot.rect.size);}
  void ApplySavedLayout() {
   var layout=GameSettings.HudLayout;
   foreach(var pair in movable) {if(layout.TryGet(pair.Key,out var saved))Place(pair.Value,saved);else pair.Value.anchoredPosition=defaultPositions[pair.Key];}
  }
  Vector2 CenterOf(RectTransform rect) {var area=safeRoot.rect.size;return new Vector2(rect.anchorMin.x*area.x,rect.anchorMin.y*area.y)+rect.anchoredPosition;}
  IEnumerable<RectTransform> FixedLayoutRects() {yield return (RectTransform)settingsButton.transform;yield return (RectTransform)layoutPanel.transform;}
  bool AnyLayoutOverlap() {
   var items=new List<RectTransform>();foreach(var rect in movable.Values)if(rect.gameObject.activeInHierarchy)items.Add(rect);   // a hidden button (sprint in joystick mode) cannot overlap anything
   for(int i=0;i<items.Count;i++) {
    for(int j=i+1;j<items.Count;j++)if(HudLayout.Overlaps(CenterOf(items[i]),items[i].rect.size,CenterOf(items[j]),items[j].rect.size))return true;
    foreach(var fixedRect in FixedLayoutRects())if(HudLayout.Overlaps(CenterOf(items[i]),items[i].rect.size,CenterOf(fixedRect),fixedRect.rect.size))return true;
   }
   return false;
  }
  void RefreshLayoutState() {
   bool overlap=EditingLayout&&AnyLayoutOverlap();saveLayoutButton.interactable=!overlap;
   layoutHint.text=GameText.Get(overlap?"layoutOverlap":"layoutHint",spanish);layoutHint.color=overlap?new Color(1,.55f,.4f):Color.white;
  }
  void BeginEditLayout() {
   backupPositions.Clear();foreach(var pair in movable)backupPositions[pair.Key]=pair.Value.anchoredPosition;
   EditingLayout=true;drags.Clear();settingsPanel.SetActive(false);layoutPanel.SetActive(true);RefreshLayoutState();Refresh();
  }
  void EndEditLayout() {EditingLayout=false;drags.Clear();layoutPanel.SetActive(false);session.SetSettingsOpen(false);Refresh();}
  public bool SaveLayout() {
   if(AnyLayoutOverlap())return false;
   var layout=new HudLayout();
   foreach(var pair in movable)if((pair.Value.anchoredPosition-defaultPositions[pair.Key]).sqrMagnitude>.0001f)layout.Set(pair.Key,pair.Value.anchoredPosition);
   GameSettings.HudLayout=layout;EndEditLayout();return true;
  }
  void CancelLayout() {foreach(var pair in movable)pair.Value.anchoredPosition=backupPositions[pair.Key];EndEditLayout();}
  void ResetLayout() {foreach(var pair in movable)pair.Value.anchoredPosition=defaultPositions[pair.Key];RefreshLayoutState();}
  Vector2 PointerToAnchored(Vector2 screen,RectTransform rect) {
   RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRoot,screen,null,out var local);var area=safeRoot.rect.size;
   return local-(new Vector2(rect.anchorMin.x*area.x,rect.anchorMin.y*area.y)-area*.5f);
  }
  bool IsLayoutUi(Vector2 screen) {return Contains((RectTransform)layoutPanel.transform,screen)||Contains((RectTransform)settingsButton.transform,screen);}
  public void LayoutPointer(int id,bool began,bool ended,Vector2 screen) {
   if(!EditingLayout)return;
   if(began) {
    if(IsLayoutUi(screen))return;
    RectTransform best=null;float bestDistance=float.MaxValue;
    foreach(var rect in movable.Values) {
     if(!Contains(rect,screen))continue;
     float distance=(RectTransformUtility.WorldToScreenPoint(null,rect.position)-screen).sqrMagnitude;if(distance<bestDistance){bestDistance=distance;best=rect;}
    }
    if(best!=null)drags[id]=new LayoutDrag{rect=best,grab=best.anchoredPosition-PointerToAnchored(screen,best)};
   } else if(drags.TryGetValue(id,out var drag)) Place(drag.rect,PointerToAnchored(screen,drag.rect)+drag.grab);
   if(ended)drags.Remove(id);
   RefreshLayoutState();
  }
  WeaponSelectorUi configSelector;
  // The same weapon choice is offered in the pre-round menu, on the results screen and in the settings menu.
  IEnumerable<WeaponSelectorUi> Selectors() {
   if(configSelector==null)configSelector=new WeaponSelectorUi{name=weaponNameLabel,stats=weaponStatsLabel,prev=weaponPrevButton,next=weaponNextButton};
   yield return configSelector;if(resultsWeapon!=null&&resultsWeapon.next!=null)yield return resultsWeapon;if(settingsWeapon!=null&&settingsWeapon.next!=null)yield return settingsWeapon;
  }
  void ChangeWeapon(int delta) {
   int count=rifle.weapons.Length,index=(rifle.CurrentIndex+delta+count)%count;
   GameSettings.SelectedWeapon=index;rifle.Equip(index);session.WeaponChanged();Refresh();
  }
  void RefreshWeaponText() {
   var def=rifle.Current;if(def==null)return;
   string weaponName=GameText.Get(def.nameKey,spanish);
   string stats=GameText.Get("ammo",spanish)+" "+def.magazineSize+"   |   "+GameText.Get("damage",spanish)+" "+def.damage+(def.pellets>1?" x"+def.pellets:"")+"   |   "+Mathf.RoundToInt(60f/def.fireInterval)+"/min";
   foreach(var selector in Selectors()) {selector.name.text=weaponName;selector.stats.text=stats;}
  }
  void OpenSettings() {settingsPanel.SetActive(true);session.SetSettingsOpen(true);Refresh();}
  void CloseSettings() {settingsPanel.SetActive(false);session.SetSettingsOpen(false);Refresh();}
  void ToggleAimMode() {GameSettings.AimToggle=!GameSettings.AimToggle;rifle.Aim.Toggle=GameSettings.AimToggle;rifle.Aim.Reset();Refresh();}
  void ToggleSprintSens() {SprintLevel=(SprintSensitivity)(((int)SprintLevel+1)%3);GameSettings.SprintSensitivity=SprintLevel;Refresh();}   // medium -> low -> high -> medium
  void ToggleSprintMode() {SprintByJoystick=!SprintByJoystick;GameSettings.SprintByJoystick=SprintByJoystick;Refresh();}
  void ToggleFps() {showFps=!GameSettings.ShowFps;GameSettings.ShowFps=showFps;Refresh();}
  void RefreshFpsText() {
   float fps=fpsMeter.Fps;fpsLabel.text=Mathf.RoundToInt(fps)+" "+GameText.Get("fps",spanish);
   switch(FpsMeter.Band(fps)) {case FpsBand.Good:fpsLabel.color=new Color(.55f,.9f,.55f);break;case FpsBand.Fair:fpsLabel.color=new Color(1f,.85f,.4f);break;default:fpsLabel.color=new Color(1f,.45f,.4f);break;}
  }
  void ToggleCross() {GameSettings.KeepCrossWhileAiming=!GameSettings.KeepCrossWhileAiming;Refresh();}
  void RefreshSettingsText() {
   aimLabel.text=GameText.Get("aim",spanish);settingsTitle.text=GameText.Get("settings",spanish);settingsButton.GetComponentInChildren<Text>().text=GameText.Get("settings",spanish);closeSettingsButton.GetComponentInChildren<Text>().text=GameText.Get("close",spanish);
   aimModeLabel.text=GameText.Get("aimMode",spanish)+": "+GameText.Get(GameSettings.AimToggle?"toggle":"hold",spanish);crossLabel.text=GameText.Get("keepCross",spanish)+": "+GameText.Get(GameSettings.KeepCrossWhileAiming?"on":"off",spanish);
   sprintModeLabel.text=GameText.Get("sprintMode",spanish)+": "+GameText.Get(SprintByJoystick?"stick":"button",spanish);
   sprintSensLabel.text=GameText.Get("sprintSens",spanish)+": "+GameText.Get(SprintLevel==SprintSensitivity.High?"sensHigh":SprintLevel==SprintSensitivity.Low?"sensLow":"sensMedium",spanish)+(SprintByJoystick?"":"  ·  "+GameText.Get("sensNote",spanish));sprintSensButton.interactable=SprintByJoystick;
   fpsButton.GetComponentInChildren<Text>().text=GameText.Get("fpsCounter",spanish)+": "+GameText.Get(showFps?"on":"off",spanish);
   editLayoutButton.GetComponentInChildren<Text>().text=GameText.Get("editLayout",spanish);saveLayoutButton.GetComponentInChildren<Text>().text=GameText.Get("save",spanish);resetLayoutButton.GetComponentInChildren<Text>().text=GameText.Get("resetLayout",spanish);cancelLayoutButton.GetComponentInChildren<Text>().text=GameText.Get("cancel",spanish);
   if(movable!=null)RefreshLayoutState();
  }
  void ToggleLanguage() {spanish=!spanish;PlayerPrefs.SetInt("language",spanish?1:0);Refresh();}
  void Refresh() {
   title.text=settingsSubtitle.text="FIELD OPS / PROTOTYPE";moveLabel.text=GameText.Get("move",spanish);lookLabel.text=GameText.Get("look",spanish);fireLabel.text=GameText.Get("fire",spanish);sprintLabel.text=GameText.Get("sprint",spanish);jumpLabel.text=GameText.Get("jump",spanish);slideLabel.text=GameText.Get("slide",spanish);crouchLabel.text=GameText.Get("crouch",spanish);resetLabel.text=GameText.Get("reset",spanish);help.text=GameText.Get("help",spanish);languageButton.GetComponentInChildren<Text>().text=spanish?"ENGLISH":"ESPAÑOL";
   roundTitle.text=GameText.Get("roundTitle",spanish);durationLabel.text=GameText.Get("duration",spanish);scoreRule.text=GameText.Get("scoreRule",spanish);durationError.text=invalidDuration?GameText.Get("durationError",spanish):"";startButton.GetComponentInChildren<Text>().text=GameText.Get("start",spanish);replayButton.GetComponentInChildren<Text>().text=GameText.Get("replay",spanish);configureButton.GetComponentInChildren<Text>().text=GameText.Get("configure",spanish);resultTitle.text=GameText.Get("finished",spanish);reloadLabel.text=GameText.Get("reload",spanish);RefreshSettingsText();RefreshWeaponText();RefreshRound();
  }
  void RefreshRound() {
   bool active=session.Round.Phase==RoundPhase.Active;bool editing=EditingLayout,show=active||editing;
   // The header also doubles as the paused notice, so it stays visible while the round is suspended.
   headerGroup.alpha=active&&session.Round.Suspended?1f:TitleTab.Alpha(session.Round.DurationSeconds-session.Round.RemainingSeconds,active);
   bool aiming=active&&rifle.Aim.Aiming;crosshair.SetActive(active&&(!aiming||GameSettings.KeepCrossWhileAiming)&&rifle.ScopeBlend<.5f);scope.SetBlend(active?rifle.ScopeBlend:0f);aim.gameObject.SetActive(show);menuBlocker.SetActive(!active&&!editing);configurationPanel.SetActive(session.Round.Phase==RoundPhase.Configuration&&!editing);resultsPanel.SetActive(session.Round.Phase==RoundPhase.Results&&!editing);
   joystick.gameObject.SetActive(show);moveLabel.gameObject.SetActive(show);fire.gameObject.SetActive(show);sprint.gameObject.SetActive(show&&!SprintByJoystick);jump.gameObject.SetActive(show);slide.gameObject.SetActive(show);crouch.gameObject.SetActive(show);lookLabel.gameObject.SetActive(active);help.gameObject.SetActive(active&&!Application.isMobilePlatform);hitLabel.gameObject.SetActive(active);timerLabel.gameObject.SetActive(active);reload.gameObject.SetActive(show);ammoLabel.gameObject.SetActive(show);
   ammoLabel.text=rifle.Ammo.IsReloading?GameText.Get("reloading",spanish):GameText.Get("ammo",spanish)+" "+rifle.Ammo.Count+" / ∞";reload.GetComponent<Image>().color=rifle.Ammo.IsReloading?new Color(.5f,.5f,.5f,.6f):(rifle.Ammo.IsEmpty?new Color(.85f,.25f,.2f,.9f):new Color(.12f,.2f,.28f,.85f));
   status.text=GameText.Get(session.Round.Suspended&&active?"paused":"status",spanish);
   hitLabel.text=GameText.Get("score",spanish)+": "+session.Round.Score+"   |   "+GameText.Get("damage",spanish)+": "+target.Damage.TotalDamage;
   timerLabel.text=GameText.Get("time",spanish)+"  "+session.Round.Clock;
   resultScore.text=GameText.Get("score",spanish)+": "+session.Round.Score+"\n"+GameText.Get("duration",spanish)+": "+session.Round.DurationSeconds;
  }
 }
}
