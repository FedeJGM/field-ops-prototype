using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace FieldOps.Tests {
 public class RuntimeSmokeTests {
  TrainingSession session; PrototypeInput input; FirstPersonMotor motor; TrainingTarget target; PrototypeHud hud; Touchscreen screen;
  [UnitySetUp] public IEnumerator LoadScene() {
   PlayerPrefs.SetInt("language",0);PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");PlayerPrefs.DeleteKey("hudLayout");PlayerPrefs.DeleteKey("weapon");
   yield return SceneManager.LoadSceneAsync("TrainingRange");
   yield return null; yield return null;
   input=Object.FindFirstObjectByType<PrototypeInput>();motor=Object.FindFirstObjectByType<FirstPersonMotor>();target=Object.FindFirstObjectByType<TrainingTarget>();hud=Object.FindFirstObjectByType<PrototypeHud>();
   InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
   InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
   session=Object.FindFirstObjectByType<TrainingSession>();session.SetPause(false);session.SetFocus(true);
   screen=InputSystem.AddDevice<Touchscreen>();InputSystem.EnableDevice(screen);Canvas.ForceUpdateCanvases();
  }
  [UnityTearDown] public IEnumerator Cleanup() {InputSystem.RemoveDevice(screen);PlayerPrefs.SetInt("language",0);PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");PlayerPrefs.DeleteKey("hudLayout");PlayerPrefs.DeleteKey("weapon");yield return null;}
  Vector2 Point(RectTransform rect) {return RectTransformUtility.WorldToScreenPoint(null,rect.position);}
  void Touch(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 point) {InputSystem.QueueStateEvent(screen,new TouchState {touchId=id,phase=phase,position=point});}
  [UnityTest] public IEnumerator SceneRendersWithConfiguredRuntimeObjects() {
   Assert.NotNull(session);Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.IsTrue(hud.configurationPanel.activeInHierarchy);Assert.IsFalse(hud.resultsPanel.activeInHierarchy);Assert.AreEqual("60",hud.durationInput.text);yield return CapturePreview("training-config-preview.bmp");Assert.IsTrue(session.StartRound("60"));yield return null;
   Assert.NotNull(input);Assert.NotNull(motor);Assert.NotNull(target);Assert.NotNull(Camera.main);Assert.NotNull(hud);
   Assert.AreEqual("FIRE",hud.fireLabel.text);Assert.IsTrue(motor.GetComponent<CharacterController>().enabled);
   var reticle=hud.transform.Find("Crosshair");Assert.NotNull(reticle);Canvas.ForceUpdateCanvases();var bars=reticle.GetComponentsInChildren<UnityEngine.UI.Image>();Assert.AreEqual(4,bars.Length);foreach(var bar in bars){Assert.IsTrue(bar.isActiveAndEnabled);Assert.IsFalse(bar.raycastTarget);Assert.Greater(bar.color.a,.9f);Assert.Greater(bar.rectTransform.rect.width,0);Assert.Greater(bar.rectTransform.rect.height,0);}Assert.Less(Vector2.Distance(Point((RectTransform)reticle),new Vector2(Screen.width*.5f,Screen.height*.5f)),1f);
   yield return new WaitForSeconds(.2f);
   yield return CapturePreview("training-active-preview.bmp");session.Round.Tick(60);yield return null;yield return CapturePreview("training-results-preview.bmp");
  }
  [UnityTest] public IEnumerator IndependentTouchesMoveSprintAndLookWithoutRoleSwitching() {
   Assert.IsTrue(session.StartRound("60"));yield return null;
   Vector2 move=Point(hud.joystick)+Vector2.up*(55*hud.GetComponent<Canvas>().scaleFactor);Vector2 sprint=Point(hud.sprint);Vector2 look=new Vector2(Screen.width*.65f,Screen.height*.6f);
   Touch(1,UnityEngine.InputSystem.TouchPhase.Began,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Began,sprint);Touch(3,UnityEngine.InputSystem.TouchPhase.Began,look);yield return null;yield return null;
   Debug.Log("SMOKE_INPUT touches="+UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count+" device="+screen.enabled+" move="+input.Move+" screen="+Screen.width+"x"+Screen.height+" point="+move+" contains="+hud.Contains(hud.joystick,move));Assert.Greater(input.Move.y,.1f);Assert.IsTrue(input.Sprint);Vector3 start=motor.transform.position;Quaternion rotation=motor.transform.rotation;
   Touch(3,UnityEngine.InputSystem.TouchPhase.Moved,look+Vector2.right*40);yield return null;yield return null;
   Assert.Greater(Quaternion.Angle(rotation,motor.transform.rotation),.01f);
   yield return new WaitForSeconds(.2f);Assert.Greater(Vector3.Distance(start,motor.transform.position),.4f);
   Touch(1,UnityEngine.InputSystem.TouchPhase.Ended,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Ended,sprint);Touch(3,UnityEngine.InputSystem.TouchPhase.Ended,look);yield return null;yield return null;
   Assert.AreEqual(Vector2.zero,input.Move);Assert.IsFalse(input.Sprint);
   var controller=motor.GetComponent<CharacterController>();controller.enabled=false;motor.transform.position=new Vector3(-21,.05f,0);motor.transform.rotation=Quaternion.Euler(0,-90,0);controller.enabled=true;
   Touch(5,UnityEngine.InputSystem.TouchPhase.Began,move);yield return new WaitForSeconds(.6f);Assert.Greater(motor.transform.position.x,-21.6f,"Character controller must not cross the west boundary.");Touch(5,UnityEngine.InputSystem.TouchPhase.Ended,move);yield return null;
  }
  [UnityTest] public IEnumerator ActualRifleRaycastDamagesTargetAndMarksSurface() {
   Assert.IsTrue(session.StartRound("60"));yield return null;
   motor.enabled=false;Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   Touch(4,UnityEngine.InputSystem.TouchPhase.Began,Point(hud.fire));yield return new WaitForSeconds(.3f);
   Assert.IsTrue(input.Fire);Assert.Greater(target.Damage.Hits,0);Assert.Greater(target.Damage.TotalDamage,0);
   Assert.IsTrue(target.GetComponentsInChildren<Renderer>().Any(r=>r.name=="Bullet impact"));
   Touch(4,UnityEngine.InputSystem.TouchPhase.Ended,Point(hud.fire));yield return null;yield return null;Assert.IsFalse(input.Fire);
  }
  [UnityTest] public IEnumerator ImpactCapIdleAnimationAndResetAreObservable() {
   Assert.IsTrue(session.StartRound("60"));yield return null;
   Vector3 initial=target.idleRig.localPosition;float displacement=0;float end=Time.time+.7f;while(Time.time<end){yield return null;displacement=Mathf.Max(displacement,Vector3.Distance(initial,target.idleRig.localPosition));}Assert.Greater(displacement,.0001f);
   var origin=target.transform.position+new Vector3(0,1.25f,-3);Assert.IsTrue(Physics.Raycast(origin,Vector3.forward,out var hit,5));Assert.AreEqual(target,hit.collider.GetComponentInParent<TrainingTarget>());
   for(int i=0;i<65;i++)target.ReceiveHit(hit,25);yield return null;
   Assert.AreEqual(60,target.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Bullet impact"));Assert.AreEqual(65,target.Damage.Hits);
   Assert.AreEqual(65,session.Round.Score);Assert.IsFalse(hud.resetButton.gameObject.activeInHierarchy);hud.resetButton.onClick.Invoke();Assert.AreEqual(65,session.Round.Score);
   session.Round.Tick(60);yield return null;hud.replayButton.onClick.Invoke();yield return null;Assert.AreEqual(0,session.Round.Score);Assert.AreEqual(0,target.Damage.Hits);Assert.AreEqual(0,target.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Bullet impact"));
  }
  [UnityTest] public IEnumerator FiringConsumesAmmoAndEmptyMagazineStopsScoring() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;motor.enabled=false;Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   Assert.AreEqual(30,rifle.Ammo.Count);while(rifle.Ammo.Count>1)rifle.Ammo.TryConsume();
   var fire=Point(hud.fire);Touch(4,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.5f);
   Assert.AreEqual(0,rifle.Ammo.Count);Assert.AreEqual(1,session.Round.Score,"Only the last round may score; an empty magazine must not fire.");Assert.AreEqual(1,target.Damage.Hits);Assert.IsFalse(rifle.Ammo.IsReloading,"Empty magazine must not reload by itself.");
   Touch(4,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;
  }
  [UnityTest] public IEnumerator ReloadButtonRefillsAfterReloadTimeAndBlocksFiringMeanwhile() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;motor.enabled=false;Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   for(int i=0;i<10;i++)rifle.Ammo.TryConsume();Assert.AreEqual(20,rifle.Ammo.Count);yield return Tap(6,hud.reload);
   Assert.IsTrue(rifle.Ammo.IsReloading);Assert.IsTrue(hud.ammoLabel.text.Contains(GameText.Get("reloading",false)));
   var fire=Point(hud.fire);Touch(7,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.4f);Assert.AreEqual(20,rifle.Ammo.Count,"Firing while reloading must not consume ammo.");Assert.AreEqual(0,target.Damage.Hits);
   Touch(7,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return new WaitForSeconds(1.4f);
   Assert.IsFalse(rifle.Ammo.IsReloading);Assert.AreEqual(30,rifle.Ammo.Count);Assert.IsTrue(hud.ammoLabel.text.Contains("30"));Assert.IsTrue(hud.ammoLabel.text.Contains("∞"));
  }
  [UnityTest] public IEnumerator ReloadKeyStartsReloadAndReloadButtonIsInertInMenus() {
   var rifle=hud.rifle;yield return Tap(11,hud.reload);Assert.IsFalse(rifle.Ammo.IsReloading);
   Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();var keyboard=InputSystem.AddDevice<Keyboard>();
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
   Assert.IsTrue(rifle.Ammo.IsReloading);InputSystem.RemoveDevice(keyboard);
  }
  [UnityTest] public IEnumerator NewRoundRestoresFullMagazineAndCancelsReload() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;for(int i=0;i<12;i++)rifle.Ammo.TryConsume();rifle.Ammo.BeginReload();
   session.Round.Tick(60);yield return null;hud.replayButton.onClick.Invoke();yield return null;
   Assert.AreEqual(30,rifle.Ammo.Count);Assert.IsFalse(rifle.Ammo.IsReloading);
  }
  [UnityTest] public IEnumerator PausedTimeDoesNotAdvanceReload() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();rifle.Ammo.BeginReload();
   session.SetPause(true);yield return new WaitForSeconds(1.7f);Assert.IsTrue(rifle.Ammo.IsReloading,"Reload must not progress while the round is suspended.");
   session.SetPause(false);yield return new WaitForSeconds(1.6f);Assert.IsFalse(rifle.Ammo.IsReloading);Assert.AreEqual(30,rifle.Ammo.Count);
  }
  [UnityTest] public IEnumerator HoldAimZoomsLowersSensitivityShowsDotAndMovesWeapon() {
   var rifle=hud.rifle;var cam=Camera.main;Assert.IsTrue(session.StartRound("60"));yield return null;yield return null;float restX=rifle.weapon.localPosition.x;Assert.AreEqual(68,cam.fieldOfView,.1f);Assert.IsTrue(hud.crosshair.activeSelf);Assert.IsFalse(hud.aimDot.activeSelf);
   var point=Point(hud.aim);Touch(12,UnityEngine.InputSystem.TouchPhase.Began,point);yield return new WaitForSeconds(.4f);
   Assert.IsTrue(rifle.Aim.Aiming);Assert.AreEqual(40,cam.fieldOfView,.5f);Assert.AreEqual(.5f,input.LookScale,.02f);Assert.IsTrue(hud.aimDot.activeSelf);Assert.IsFalse(hud.crosshair.activeSelf,"Cross hides while aiming unless the option is on.");Assert.Less(rifle.weapon.localPosition.x,restX-.15f);yield return CapturePreview("ads-aim-preview.bmp");
   Touch(12,UnityEngine.InputSystem.TouchPhase.Ended,point);yield return new WaitForSeconds(.4f);
   Assert.IsFalse(rifle.Aim.Aiming);Assert.AreEqual(68,cam.fieldOfView,.5f);Assert.AreEqual(1,input.LookScale,.02f);Assert.IsFalse(hud.aimDot.activeSelf);Assert.IsTrue(hud.crosshair.activeSelf);Assert.AreEqual(restX,rifle.weapon.localPosition.x,.02f);
  }
  [UnityTest] public IEnumerator ToggleModeFromSettingsKeepsAimingUntilPressedAgain() {
   var rifle=hud.rifle;hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.settingsPanel.activeSelf);yield return CapturePreview("ads-settings-preview.bmp");hud.aimModeButton.onClick.Invoke();yield return null;
   Assert.IsTrue(GameSettings.AimToggle);Assert.IsTrue(hud.aimModeLabel.text.Contains(GameText.Get("toggle",false)));hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.IsFalse(hud.settingsPanel.activeSelf);
   Assert.IsTrue(session.StartRound("60"));yield return null;
   yield return Tap(13,hud.aim);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Aim.Aiming,"Toggle must keep aiming after release.");
   yield return Tap(14,hud.aim);yield return new WaitForSeconds(.3f);Assert.IsFalse(rifle.Aim.Aiming);
  }
  [UnityTest] public IEnumerator KeepCrossOptionShowsBothReticlesWhileAiming() {
   var rifle=hud.rifle;hud.settingsButton.onClick.Invoke();yield return null;hud.crossButton.onClick.Invoke();yield return null;Assert.IsTrue(GameSettings.KeepCrossWhileAiming);hud.closeSettingsButton.onClick.Invoke();
   Assert.IsTrue(session.StartRound("60"));yield return null;var point=Point(hud.aim);Touch(15,UnityEngine.InputSystem.TouchPhase.Began,point);yield return new WaitForSeconds(.4f);
   Assert.IsTrue(rifle.Aim.Aiming);Assert.IsTrue(hud.aimDot.activeSelf);Assert.IsTrue(hud.crosshair.activeSelf);Touch(15,UnityEngine.InputSystem.TouchPhase.Ended,point);yield return null;
  }
  [UnityTest] public IEnumerator SettingsPauseTheRoundAndWorkInMenus() {
   hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.settingsPanel.activeSelf);hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.IsFalse(hud.settingsPanel.activeSelf);
   Assert.IsTrue(session.StartRound("60"));yield return null;hud.settingsButton.onClick.Invoke();yield return null;
   Assert.IsTrue(session.SettingsOpen);Assert.IsTrue(session.Round.Suspended);double remaining=session.Round.RemainingSeconds;yield return new WaitForSeconds(.3f);Assert.AreEqual(remaining,session.Round.RemainingSeconds);Assert.IsFalse(input.Fire);Assert.IsFalse(hud.rifle.Aim.Aiming);
   hud.closeSettingsButton.onClick.Invoke();yield return new WaitForSeconds(.2f);Assert.IsFalse(session.SettingsOpen);Assert.IsFalse(session.Round.Suspended);Assert.Less(session.Round.RemainingSeconds,remaining);
  }
  [UnityTest] public IEnumerator SprintCancelsAimingButReloadingKeepsIt() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();var aim=Point(hud.aim);var sprint=Point(hud.sprint);
   Touch(16,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Aim.Aiming);
   yield return Tap(17,hud.reload);Assert.IsTrue(rifle.Ammo.IsReloading);Assert.IsTrue(rifle.Aim.Aiming,"Reloading must not cancel aiming.");
   Touch(18,UnityEngine.InputSystem.TouchPhase.Began,sprint);yield return new WaitForSeconds(.3f);Assert.IsFalse(rifle.Aim.Aiming,"Sprinting cancels aiming.");
   Touch(18,UnityEngine.InputSystem.TouchPhase.Ended,sprint);Touch(16,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;yield return null;
  }
  [UnityTest] public IEnumerator RoundEndResetsAimingAndAimButtonIsInertInMenus() {
   var rifle=hud.rifle;var cam=Camera.main;yield return Tap(19,hud.aim);Assert.IsFalse(rifle.Aim.Aiming);
   Assert.IsTrue(session.StartRound("60"));yield return null;var aim=Point(hud.aim);Touch(20,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Aim.Aiming);
   session.Round.Tick(60);yield return new WaitForSeconds(.1f);Assert.IsFalse(rifle.Aim.Aiming);Assert.AreEqual(68,cam.fieldOfView,.1f);Touch(20,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
  }
  [UnityTest] public IEnumerator FiringPlaysOneShotSoundPerRoundAndHitSoundsOnlyOnTarget() {
   var rifle=hud.rifle;var sfx=rifle.sfx;Assert.NotNull(sfx);Assert.IsTrue(session.StartRound("60"));yield return null;motor.enabled=false;
   Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);var fire=Point(hud.fire);Touch(21,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.4f);Touch(21,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;
   int fired=30-rifle.Ammo.Count;Assert.Greater(fired,1);Assert.AreEqual(fired,sfx.Count(Sfx.Shot));Assert.AreEqual(target.Damage.Hits,sfx.Count(Sfx.Hit));Assert.AreEqual(0,sfx.Count(Sfx.DryFire));Assert.AreEqual(0,sfx.Count(Sfx.Reload));
   int hits=sfx.Count(Sfx.Hit);Camera.main.transform.rotation=Quaternion.Euler(-60,0,0);Touch(22,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.3f);Touch(22,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;
   Assert.Greater(sfx.Count(Sfx.Shot),fired,"Misses still make the shot sound.");Assert.AreEqual(hits,sfx.Count(Sfx.Hit),"A miss must not make the hit sound.");
  }
  [UnityTest] public IEnumerator EmptyMagazinePlaysDryClickInsteadOfShotOrHit() {
   var rifle=hud.rifle;var sfx=rifle.sfx;Assert.IsTrue(session.StartRound("60"));yield return null;motor.enabled=false;Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   while(!rifle.Ammo.IsEmpty)rifle.Ammo.TryConsume();var fire=Point(hud.fire);Touch(23,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.4f);Touch(23,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;
   Assert.Greater(sfx.Count(Sfx.DryFire),0);Assert.AreEqual(0,sfx.Count(Sfx.Shot));Assert.AreEqual(0,sfx.Count(Sfx.Hit));Assert.AreEqual(0,target.Damage.Hits);
  }
  [UnityTest] public IEnumerator ReloadSoundPlaysOncePerReloadAndSuspensionStopsIt() {
   var rifle=hud.rifle;var sfx=rifle.sfx;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();
   yield return Tap(24,hud.reload);Assert.AreEqual(1,sfx.Count(Sfx.Reload));yield return Tap(25,hud.reload);Assert.AreEqual(1,sfx.Count(Sfx.Reload),"A rejected reload must stay silent.");
   int stops=sfx.StopCount;session.SetPause(true);yield return null;Assert.Greater(sfx.StopCount,stops,"Pausing must cut the reload sound.");session.SetPause(false);
  }
  [UnityTest] public IEnumerator MenusAndSettingsAreSilent() {
   var rifle=hud.rifle;var sfx=rifle.sfx;var fire=Point(hud.fire);Touch(26,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.3f);Touch(26,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;
   yield return Tap(27,hud.reload);foreach(Sfx s in System.Enum.GetValues(typeof(Sfx)))Assert.AreEqual(0,sfx.Count(s),s.ToString());
  }
  IEnumerator Drag(int id,Vector2 from,Vector2 to) {
   Touch(id,UnityEngine.InputSystem.TouchPhase.Began,from);yield return null;yield return null;Touch(id,UnityEngine.InputSystem.TouchPhase.Moved,Vector2.Lerp(from,to,.5f));yield return null;Touch(id,UnityEngine.InputSystem.TouchPhase.Moved,to);yield return null;yield return null;Touch(id,UnityEngine.InputSystem.TouchPhase.Ended,to);yield return null;yield return null;
  }
  IEnumerator OpenLayoutEditor() {hud.settingsButton.onClick.Invoke();yield return null;hud.editLayoutButton.onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();}
  float CanvasScale() {return hud.GetComponent<Canvas>().scaleFactor;}
  [UnityTest] public IEnumerator DraggingAButtonMovesItAndOnlyItsNewSpotFires() {
   var defaultPosition=hud.fire.anchoredPosition;yield return OpenLayoutEditor();
   Assert.IsTrue(hud.EditingLayout);Assert.IsTrue(hud.layoutPanel.activeSelf);Assert.IsFalse(hud.settingsPanel.activeSelf);Assert.IsTrue(hud.fire.gameObject.activeInHierarchy,"Buttons show while editing, even from the menu.");Assert.IsFalse(hud.menuBlocker.activeSelf);Assert.IsFalse(hud.configurationPanel.activeSelf);
   var oldPoint=Point(hud.fire);yield return Drag(30,oldPoint,oldPoint+new Vector2(-600,250)*CanvasScale());
   var moved=hud.fire.anchoredPosition-defaultPosition;Assert.AreEqual(-600,moved.x,3f);Assert.AreEqual(250,moved.y,3f);
   hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsFalse(hud.EditingLayout);Assert.IsFalse(hud.layoutPanel.activeSelf);Assert.IsTrue(hud.menuBlocker.activeSelf,"The configuration menu returns after editing.");
   Assert.IsTrue(session.StartRound("60"));yield return null;var newPoint=Point(hud.fire);Touch(31,UnityEngine.InputSystem.TouchPhase.Began,newPoint);yield return null;yield return null;Assert.IsTrue(input.Fire,"The new spot must fire.");Touch(31,UnityEngine.InputSystem.TouchPhase.Ended,newPoint);yield return null;yield return null;
   Touch(32,UnityEngine.InputSystem.TouchPhase.Began,oldPoint);yield return null;yield return null;Assert.IsFalse(input.Fire,"The old spot must no longer fire.");Touch(32,UnityEngine.InputSystem.TouchPhase.Ended,oldPoint);yield return null;
  }
  [UnityTest] public IEnumerator SavedLayoutSurvivesAReloadOfTheScene() {
   yield return OpenLayoutEditor();var oldPoint=Point(hud.reload);yield return Drag(33,oldPoint,oldPoint+new Vector2(-500,200)*CanvasScale());var saved=hud.reload.anchoredPosition;hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(PlayerPrefs.HasKey("hudLayout"));
   yield return SceneManager.LoadSceneAsync("TrainingRange");yield return null;yield return null;var reloaded=Object.FindFirstObjectByType<PrototypeHud>();Canvas.ForceUpdateCanvases();
   Assert.AreEqual(saved.x,reloaded.reload.anchoredPosition.x,.5f);Assert.AreEqual(saved.y,reloaded.reload.anchoredPosition.y,.5f);Assert.AreEqual(new Vector2(-160,170),reloaded.fire.anchoredPosition,"Untouched buttons keep their default place.");
  }
  [UnityTest] public IEnumerator CancelDiscardsEditsAndStoresNothing() {
   var defaultPosition=hud.sprint.anchoredPosition;yield return OpenLayoutEditor();var from=Point(hud.sprint);yield return Drag(34,from,from+new Vector2(-400,300)*CanvasScale());Assert.AreNotEqual(defaultPosition,hud.sprint.anchoredPosition);
   hud.cancelLayoutButton.onClick.Invoke();yield return null;Assert.IsFalse(hud.EditingLayout);Assert.AreEqual(defaultPosition.x,hud.sprint.anchoredPosition.x,.01f);Assert.AreEqual(defaultPosition.y,hud.sprint.anchoredPosition.y,.01f);Assert.IsFalse(PlayerPrefs.HasKey("hudLayout"));
  }
  [UnityTest] public IEnumerator ResetThenSaveRestoresDefaultsAndClearsTheStoredLayout() {
   var defaultPosition=hud.aim.anchoredPosition;yield return OpenLayoutEditor();var from=Point(hud.aim);yield return Drag(35,from,from+new Vector2(-500,250)*CanvasScale());hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(PlayerPrefs.HasKey("hudLayout"));
   yield return OpenLayoutEditor();hud.resetLayoutButton.onClick.Invoke();yield return null;hud.saveLayoutButton.onClick.Invoke();yield return null;
   Assert.AreEqual(defaultPosition.x,hud.aim.anchoredPosition.x,.01f);Assert.AreEqual(defaultPosition.y,hud.aim.anchoredPosition.y,.01f);Assert.IsFalse(PlayerPrefs.HasKey("hudLayout"),"A layout equal to the defaults stores nothing.");
  }
  [UnityTest] public IEnumerator OverlappingButtonsBlockSavingUntilSeparated() {
   yield return OpenLayoutEditor();var aim=Point(hud.aim);var fire=Point(hud.fire);yield return Drag(36,aim,fire);
   Assert.IsFalse(hud.saveLayoutButton.interactable);Assert.AreEqual(GameText.Get("layoutOverlap",false),hud.layoutHint.text);hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.EditingLayout,"Saving must be refused while buttons overlap.");Assert.IsFalse(PlayerPrefs.HasKey("hudLayout"));
   var stuck=Point(hud.aim);yield return Drag(37,stuck,stuck+new Vector2(-500,200)*CanvasScale());Assert.IsTrue(hud.saveLayoutButton.interactable);Assert.AreEqual(GameText.Get("layoutHint",false),hud.layoutHint.text);
   hud.cancelLayoutButton.onClick.Invoke();yield return null;
  }
  [UnityTest] public IEnumerator ButtonsCannotBeDraggedOutsideTheScreen() {
   yield return OpenLayoutEditor();var from=Point(hud.fire);yield return Drag(38,from,from+new Vector2(5000,5000));
   var corners=new Vector3[4];hud.fire.GetWorldCorners(corners);foreach(var c in corners){Assert.GreaterOrEqual(c.x,-.5f);Assert.LessOrEqual(c.x,Screen.width+.5f);Assert.GreaterOrEqual(c.y,-.5f);Assert.LessOrEqual(c.y,Screen.height+.5f);}
   hud.cancelLayoutButton.onClick.Invoke();yield return null;
  }
  [UnityTest] public IEnumerator CorruptStoredLayoutFallsBackToTheDefaultPlaces() {
   PlayerPrefs.SetString("hudLayout","garbage{{");yield return SceneManager.LoadSceneAsync("TrainingRange");yield return null;yield return null;var reloaded=Object.FindFirstObjectByType<PrototypeHud>();Canvas.ForceUpdateCanvases();
   Assert.AreEqual(new Vector2(-160,170),reloaded.fire.anchoredPosition);Assert.AreEqual(new Vector2(175,165),reloaded.joystick.anchoredPosition);
  }
  [UnityTest] public IEnumerator EditingKeepsTheRoundPausedAndResumesAfterwards() {
   Assert.IsTrue(session.StartRound("60"));yield return null;yield return OpenLayoutEditor();Assert.IsTrue(session.Round.Suspended);double remaining=session.Round.RemainingSeconds;yield return new WaitForSeconds(.3f);Assert.AreEqual(remaining,session.Round.RemainingSeconds);
   hud.cancelLayoutButton.onClick.Invoke();yield return new WaitForSeconds(.2f);Assert.IsFalse(session.Round.Suspended);Assert.IsFalse(session.SettingsOpen);Assert.Less(session.Round.RemainingSeconds,remaining);
  }
  [UnityTest] public IEnumerator DefaultButtonPlacesDoNotOverlapEachOtherOrTheFixedMenuButtons() {
   yield return OpenLayoutEditor();Assert.IsTrue(hud.saveLayoutButton.interactable,"The default layout must be saveable.");Assert.AreEqual(GameText.Get("layoutHint",false),hud.layoutHint.text);yield return CapturePreview("hud-edit-preview.bmp");hud.languageButton.onClick.Invoke();yield return null;yield return CapturePreview("hud-edit-preview-es.bmp");hud.languageButton.onClick.Invoke();hud.cancelLayoutButton.onClick.Invoke();yield return null;
  }
  IEnumerator SelectWeapon(int index) {var rifle=hud.rifle;while(rifle.CurrentIndex!=index){hud.weaponNextButton.onClick.Invoke();yield return null;}}
  IEnumerator HoldFire(int id,float seconds) {var fire=Point(hud.fire);Touch(id,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(seconds);Touch(id,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;}
  IEnumerator FinishRound() {session.Round.Tick(60);yield return null;yield return null;session.Configure();yield return null;}
  void AimAtTarget() {Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);}
  [UnityTest] public IEnumerator ConfigurationMenuShowsTheSelectedWeaponAndCyclesThroughAllFive() {
   var rifle=hud.rifle;Assert.AreEqual(5,rifle.weapons.Length);Assert.AreEqual(1,rifle.CurrentIndex);string Name(int i) {return GameText.Get(rifle.weapons[i].nameKey,false);}
   Assert.AreEqual(Name(1),hud.weaponNameLabel.text);
   foreach(int idx in new[]{2,3,4,0,1}) {hud.weaponNextButton.onClick.Invoke();yield return null;Assert.AreEqual(idx,rifle.CurrentIndex);Assert.AreEqual(Name(idx),hud.weaponNameLabel.text);Assert.AreEqual(idx,GameSettings.SelectedWeapon);for(int m=0;m<rifle.models.Length;m++)Assert.AreEqual(m==idx,rifle.models[m].gameObject.activeSelf,"model "+m+" for weapon "+idx);}
   hud.weaponPrevButton.onClick.Invoke();yield return null;Assert.AreEqual(0,rifle.CurrentIndex);hud.weaponPrevButton.onClick.Invoke();yield return null;Assert.AreEqual(4,rifle.CurrentIndex,"Previous wraps to the last weapon.");
   hud.languageButton.onClick.Invoke();yield return null;Assert.AreEqual(GameText.Get(rifle.weapons[4].nameKey,true),hud.weaponNameLabel.text);
  }
  [UnityTest] public IEnumerator EachWeaponAppliesItsOwnStatsAndShowsThemInTheMenu() {
   var rifle=hud.rifle;for(int i=0;i<rifle.weapons.Length;i++) {
    var def=rifle.weapons[i];yield return SelectWeapon(i);StringAssert.Contains(def.magazineSize.ToString(),hud.weaponStatsLabel.text);StringAssert.Contains(def.damage.ToString(),hud.weaponStatsLabel.text);
    Assert.IsTrue(session.StartRound("60"));yield return null;Assert.AreSame(def,rifle.Current);Assert.AreEqual(def.magazineSize,rifle.Ammo.Capacity);Assert.AreEqual(def.magazineSize,rifle.Ammo.Count);Assert.AreEqual(def.aimFov,rifle.Aim.ZoomFov,.001f);Assert.IsTrue(hud.ammoLabel.text.Contains(def.magazineSize.ToString()));
    yield return FinishRound();
   }
  }
  [UnityTest] public IEnumerator FireRateDiffersPerWeapon() {
   var rifle=hud.rifle;motor.enabled=false;yield return SelectWeapon(0);Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();yield return HoldFire(40,.7f);int pistol=rifle.Ammo.Capacity-rifle.Ammo.Count;Assert.That(pistol,Is.InRange(2,3),"Pistol: one shot per 0.30 s.");
   yield return FinishRound();yield return SelectWeapon(2);Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();yield return HoldFire(41,.7f);int smg=rifle.Ammo.Capacity-rifle.Ammo.Count;Assert.GreaterOrEqual(smg,8,"Sub-machine gun: one shot per 0.07 s.");Assert.Greater(smg,pistol*2);
  }
  [UnityTest] public IEnumerator SniperDealsHeavyDamageWithOneShot() {
   var rifle=hud.rifle;motor.enabled=false;yield return SelectWeapon(4);Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();yield return HoldFire(42,.4f);
   Assert.AreEqual(4,rifle.Ammo.Count);Assert.AreEqual(1,session.Round.Score);Assert.AreEqual(1,target.Damage.Hits);Assert.AreEqual(100,target.Damage.TotalDamage);
  }
  [UnityTest] public IEnumerator ShotgunFiresPelletsButScoresOncePerShot() {
   var rifle=hud.rifle;motor.enabled=false;yield return SelectWeapon(3);Assert.IsTrue(session.StartRound("60"));yield return null;
   var controller=motor.GetComponent<CharacterController>();controller.enabled=false;motor.transform.position=new Vector3(0,.06f,-1);controller.enabled=true;AimAtTarget();yield return null;
   yield return HoldFire(43,.3f);Assert.AreEqual(5,rifle.Ammo.Count,"One trigger pull uses one shell.");Assert.AreEqual(1,session.Round.Score,"One point per shot that hits, however many pellets land.");Assert.GreaterOrEqual(target.Damage.Hits,3,"Several pellets hit at close range.");Assert.AreEqual(target.Damage.Hits*15,target.Damage.TotalDamage);
  }
  [UnityTest] public IEnumerator EachWeaponHasItsOwnAimZoom() {
   var cam=Camera.main;motor.enabled=false;float[] zoom=new float[2];int slot=0;
   foreach(int weapon in new[]{0,4}) {
    yield return SelectWeapon(weapon);Assert.IsTrue(session.StartRound("60"));yield return null;var aim=Point(hud.aim);Touch(44+slot,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.4f);zoom[slot]=cam.fieldOfView;Touch(44+slot,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;yield return null;slot++;yield return FinishRound();
   }
   Assert.AreEqual(50f,zoom[0],.5f);Assert.AreEqual(20f,zoom[1],.5f);Assert.Less(zoom[1],zoom[0]);
  }
  [UnityTest] public IEnumerator WeaponChoiceSurvivesAReloadOfTheScene() {
   yield return SelectWeapon(3);Assert.AreEqual(3,GameSettings.SelectedWeapon);
   yield return SceneManager.LoadSceneAsync("TrainingRange");yield return null;yield return null;var reloaded=Object.FindFirstObjectByType<PrototypeHud>();Assert.AreEqual(3,reloaded.rifle.CurrentIndex);Assert.AreEqual(GameText.Get("weaponShotgun",false),reloaded.weaponNameLabel.text);
  }
  [UnityTest] public IEnumerator WeaponShowcaseCapturesHipAndAimViews() {
   var rifle=hud.rifle;motor.enabled=false;for(int i=0;i<rifle.weapons.Length;i++) {
    yield return SelectWeapon(i);Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();yield return new WaitForSeconds(.2f);yield return CapturePreview("weapon-"+i+"-hip.bmp");yield return null;Canvas.ForceUpdateCanvases();
    var aim=Point(hud.aim);Touch(50+i,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.5f);Assert.IsTrue(rifle.Aim.Aiming,"weapon "+i+" must be aiming: input.AimHeld="+input.AimHeld+" gameplay="+session.GameplayEnabled);Assert.AreEqual(rifle.weapons[i].aimFov,Camera.main.fieldOfView,.5f,"weapon "+i);yield return CapturePreview("weapon-"+i+"-aim.bmp");Touch(50+i,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;yield return null;yield return FinishRound();
   }
  }
  [UnityTest] public IEnumerator WeaponCanBeChangedOnTheResultsScreenAndTheNextRoundUsesIt() {
   var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;session.Round.Tick(60);yield return null;yield return null;
   Assert.AreEqual(RoundPhase.Results,session.Round.Phase);Assert.IsTrue(hud.resultsPanel.activeInHierarchy);Assert.AreEqual(GameText.Get("weaponRifle",false),hud.resultsWeapon.name.text);Assert.IsTrue(hud.resultsWeapon.next.interactable);
   hud.resultsWeapon.next.onClick.Invoke();yield return null;Assert.AreEqual(2,rifle.CurrentIndex);Assert.AreEqual(GameText.Get("weaponSmg",false),hud.resultsWeapon.name.text);StringAssert.Contains("32",hud.resultsWeapon.stats.text);
   hud.replayButton.onClick.Invoke();yield return null;Assert.AreEqual(RoundPhase.Active,session.Round.Phase);Assert.AreSame(rifle.weapons[2],rifle.Current);Assert.AreEqual(32,rifle.Ammo.Capacity);
  }
  [UnityTest] public IEnumerator WeaponCanBeChangedFromSettingsWhileInTheMenus() {
   var rifle=hud.rifle;hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.settingsPanel.activeSelf);Assert.AreEqual(GameText.Get("weaponRifle",false),hud.settingsWeapon.name.text);Assert.IsTrue(hud.settingsWeapon.prev.interactable);
   hud.settingsWeapon.prev.onClick.Invoke();yield return null;Assert.AreEqual(0,rifle.CurrentIndex);Assert.AreEqual(GameText.Get("weaponPistol",false),hud.settingsWeapon.name.text);Assert.AreEqual(GameText.Get("weaponPistol",false),hud.weaponNameLabel.text,"All selectors show the same weapon.");Assert.AreEqual(0,GameSettings.SelectedWeapon);
   hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.IsTrue(session.StartRound("60"));yield return null;Assert.AreEqual(12,rifle.Ammo.Capacity);
  }
  [UnityTest] public IEnumerator ChangingWeaponMidRoundResetsTheScoreAndGivesAFullMagazine() {
   var rifle=hud.rifle;motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();yield return HoldFire(60,.3f);
   Assert.Greater(session.Round.Score,0);Assert.Greater(target.Damage.Hits,0);Assert.Less(rifle.Ammo.Count,30);
   hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.settingsWeapon.next.interactable,"Weapons can be swapped during a round.");double remaining=session.Round.RemainingSeconds;
   hud.settingsWeapon.next.onClick.Invoke();yield return null;
   Assert.AreEqual(2,rifle.CurrentIndex);Assert.AreEqual(RoundPhase.Active,session.Round.Phase);Assert.AreEqual(0,session.Round.Score);Assert.AreEqual(0,target.Damage.Hits);Assert.AreEqual(0,target.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Bullet impact"));Assert.AreEqual(32,rifle.Ammo.Count);Assert.AreEqual(remaining,session.Round.RemainingSeconds,"The timer does not restart.");
   hud.closeSettingsButton.onClick.Invoke();yield return null;yield return HoldFire(61,.3f);Assert.Greater(session.Round.Score,0,"The new weapon scores from zero.");Assert.Less(session.Round.RemainingSeconds,remaining);
  }
  IEnumerator CapturePreview(string fileName) {
   if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null) {
    var camera=Camera.main;var canvas=hud.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.2f;yield return null;Canvas.ForceUpdateCanvases();
    var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
    var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
    var previewPath=System.IO.Path.Combine(Application.dataPath,"../Logs/"+fileName);System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(previewPath));
    var pixels=image.GetPixels32();using(var writer=new System.IO.BinaryWriter(System.IO.File.Create(previewPath))) {
     writer.Write((short)0x4d42);writer.Write(54+1280*720*3);writer.Write(0);writer.Write(54);writer.Write(40);writer.Write(1280);writer.Write(720);writer.Write((short)1);writer.Write((short)24);writer.Write(0);writer.Write(1280*720*3);writer.Write(0);writer.Write(0);writer.Write(0);writer.Write(0);
     foreach(var pixel in pixels){writer.Write(pixel.b);writer.Write(pixel.g);writer.Write(pixel.r);}
    }
    camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(rt);Object.Destroy(image);canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   }
  }
  IEnumerator Tap(int id,RectTransform rect) {var point=Point(rect);Touch(id,UnityEngine.InputSystem.TouchPhase.Began,point);yield return null;yield return null;Touch(id,UnityEngine.InputSystem.TouchPhase.Ended,point);yield return null;yield return null;}
  [UnityTest] public IEnumerator ActualTouchSelectsNumericInputRejectsInvalidDurationAndStartsMaximumRound() {
   Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.IsFalse(input.Fire);
   yield return Tap(8,(RectTransform)hud.durationInput.transform);
   Assert.AreEqual(hud.durationInput.gameObject,UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);Assert.IsTrue(hud.durationInput.isFocused,"Actual touch must focus the saved input field.");
   hud.durationInput.text="301";yield return Tap(9,(RectTransform)hud.startButton.transform);
   Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.AreEqual(GameText.Get("durationError",false),hud.durationError.text);
   hud.languageButton.onClick.Invoke();Assert.AreEqual(GameText.Get("durationError",true),hud.durationError.text);
   hud.durationInput.text="";hud.startButton.onClick.Invoke();Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);
   foreach(var invalid in new[]{"1.5","1000","abc","0"}) {hud.durationInput.text=invalid;hud.startButton.onClick.Invoke();Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.AreEqual(GameText.Get("durationError",true),hud.durationError.text);}
   hud.durationInput.text="300";yield return Tap(10,(RectTransform)hud.startButton.transform);
   Assert.AreEqual(RoundPhase.Active,session.Round.Phase);Assert.AreEqual(300,session.Round.DurationSeconds);Assert.IsFalse(hud.menuBlocker.activeSelf);Assert.IsTrue(hud.timerLabel.text.EndsWith("05:00"));
   Assert.IsFalse(input.Fire);Assert.AreEqual(Vector2.zero,input.Move);Assert.AreEqual(0,session.Round.Score);
  }
  [UnityTest] public IEnumerator MenuAndHeldTouchesCannotLeakIntoNewRound() {
   var move=Point(hud.joystick)+Vector2.up*40;var fire=Point(hud.fire);var look=new Vector2(Screen.width*.85f,Screen.height*.6f);var position=motor.transform.position;var rotation=motor.transform.rotation;
   Touch(1,UnityEngine.InputSystem.TouchPhase.Began,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Began,fire);Touch(3,UnityEngine.InputSystem.TouchPhase.Began,look);yield return null;yield return null;
   Touch(3,UnityEngine.InputSystem.TouchPhase.Moved,look+Vector2.right*50);yield return null;
   Assert.AreEqual(Vector2.zero,input.Move);Assert.AreEqual(Vector2.zero,input.Look);Assert.IsFalse(input.Fire);Assert.AreEqual(0,target.Damage.Hits);Assert.Less(Vector3.Distance(position,motor.transform.position),.1f);Assert.Less(Quaternion.Angle(rotation,motor.transform.rotation),.01f);
   Assert.IsTrue(session.StartRound("60"));yield return null;yield return null;Assert.IsFalse(input.Fire);Assert.AreEqual(Vector2.zero,input.Move);Assert.AreEqual(0,session.Round.Score);
   Touch(1,UnityEngine.InputSystem.TouchPhase.Ended,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Ended,fire);Touch(3,UnityEngine.InputSystem.TouchPhase.Ended,look);yield return null;yield return null;
  }
  [UnityTest] public IEnumerator ExpiryFreezesScoreAndReplayClearsHitsMarksAndHeldFire() {
   Assert.IsTrue(session.StartRound("1"));yield return null;motor.enabled=false;Camera.main.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   var fire=Point(hud.fire);Touch(4,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(1.1f);yield return null;
   Assert.AreEqual(RoundPhase.Results,session.Round.Phase);Assert.AreEqual("00:00",session.Round.Clock);int score=session.Round.Score;Assert.Greater(score,0);Assert.AreEqual(score,target.Damage.Hits);Assert.IsTrue(hud.resultsPanel.activeInHierarchy);Assert.IsFalse(input.Fire);Assert.AreEqual(CursorLockMode.None,Cursor.lockState);
   yield return new WaitForSeconds(.15f);Assert.AreEqual(score,session.Round.Score);Assert.IsFalse(session.Round.RecordHit());
   hud.replayButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual(RoundPhase.Active,session.Round.Phase);Assert.AreEqual(1,session.Round.DurationSeconds);Assert.AreEqual(0,session.Round.Score);Assert.AreEqual(0,target.Damage.Hits);Assert.AreEqual(0,target.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Bullet impact"));Assert.IsFalse(input.Fire);
   Touch(4,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;yield return null;Touch(5,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.15f);Assert.Greater(session.Round.Score,0);Touch(5,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;
   session.Round.Tick(2);yield return null;hud.configureButton.onClick.Invoke();yield return null;Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.AreEqual("1",hud.durationInput.text);
  }
  [UnityTest] public IEnumerator PauseAndFocusLossSuspendActiveTimeAndClearHeldControls() {
   Assert.IsTrue(session.StartRound("60"));yield return null;var move=Point(hud.joystick)+Vector2.up*(55*hud.GetComponent<Canvas>().scaleFactor);var fire=Point(hud.fire);
   Touch(1,UnityEngine.InputSystem.TouchPhase.Began,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return null;yield return null;Assert.Greater(input.Move.y,0);Assert.IsTrue(input.Fire);
   session.SetPause(true);double remaining=session.Round.RemainingSeconds;yield return new WaitForSeconds(.2f);Assert.AreEqual(remaining,session.Round.RemainingSeconds);Assert.IsFalse(input.Fire);Assert.AreEqual(Vector2.zero,input.Move);Assert.IsTrue(hud.status.text.Contains("PAUSED"));
   session.SetFocus(false);session.SetPause(false);Assert.IsTrue(session.Round.Suspended);session.SetFocus(true);yield return new WaitForSeconds(.1f);Assert.Less(session.Round.RemainingSeconds,remaining);Assert.IsFalse(input.Fire);Assert.AreEqual(Vector2.zero,input.Move);
   Touch(1,UnityEngine.InputSystem.TouchPhase.Ended,move);Touch(2,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;
  }
  [UnityTest] public IEnumerator LanguageButtonUpdatesActualHudInBothLanguages() {
   Assert.AreEqual("START ROUND",hud.startButton.GetComponentInChildren<UnityEngine.UI.Text>().text);Assert.AreEqual("FIRE",hud.fireLabel.text);hud.languageButton.onClick.Invoke();yield return null;Assert.AreEqual("DISPARAR",hud.fireLabel.text);Assert.AreEqual("INICIAR PARTIDA",hud.startButton.GetComponentInChildren<UnityEngine.UI.Text>().text);Assert.IsTrue(session.StartRound("60"));yield return null;Assert.IsTrue(hud.timerLabel.text.StartsWith("TIEMPO"));session.Round.Tick(60);yield return null;Assert.AreEqual("PARTIDA TERMINADA",hud.resultTitle.text);hud.languageButton.onClick.Invoke();yield return null;Assert.AreEqual("FIRE",hud.fireLabel.text);
  }
 }
}
