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
   PlayerPrefs.SetInt("language",0);PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");PlayerPrefs.DeleteKey("hudLayout");PlayerPrefs.DeleteKey("weapon");PlayerPrefs.DeleteKey("showFps");PlayerPrefs.DeleteKey("sprintMode");PlayerPrefs.DeleteKey("sprintSens");
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
  [UnityTearDown] public IEnumerator Cleanup() {InputSystem.RemoveDevice(screen);PlayerPrefs.SetInt("language",0);PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");PlayerPrefs.DeleteKey("hudLayout");PlayerPrefs.DeleteKey("weapon");PlayerPrefs.DeleteKey("showFps");PlayerPrefs.DeleteKey("sprintMode");PlayerPrefs.DeleteKey("sprintSens");yield return null;}
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
   hud.sprintModeButton.onClick.Invoke();Canvas.ForceUpdateCanvases();   // this test drives the sprint button
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
   session.SetPause(false);yield return new WaitForSeconds(2.0f);Assert.IsFalse(rifle.Ammo.IsReloading);Assert.AreEqual(30,rifle.Ammo.Count);
  }
  [UnityTest] public IEnumerator HoldAimZoomsLowersSensitivityShowsDotAndMovesWeapon() {
   var rifle=hud.rifle;var cam=Camera.main;Assert.IsTrue(session.StartRound("60"));yield return null;yield return null;float restX=rifle.weapon.localPosition.x;Assert.AreEqual(68,cam.fieldOfView,.1f);Assert.IsTrue(hud.crosshair.activeSelf);
   var point=Point(hud.aim);Touch(12,UnityEngine.InputSystem.TouchPhase.Began,point);yield return new WaitForSeconds(.4f);
   Assert.IsTrue(rifle.Aim.Aiming);Assert.AreEqual(40,cam.fieldOfView,.5f);Assert.AreEqual(.5f,input.LookScale,.02f);Assert.IsFalse(hud.crosshair.activeSelf,"Cross hides while aiming unless the option is on.");Assert.Less(rifle.weapon.localPosition.x,restX-.15f);yield return CapturePreview("ads-aim-preview.bmp");
   Touch(12,UnityEngine.InputSystem.TouchPhase.Ended,point);yield return new WaitForSeconds(.4f);
   Assert.IsFalse(rifle.Aim.Aiming);Assert.AreEqual(68,cam.fieldOfView,.5f);Assert.AreEqual(1,input.LookScale,.02f);Assert.IsTrue(hud.crosshair.activeSelf);Assert.AreEqual(restX,rifle.weapon.localPosition.x,.02f);
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
   Assert.IsTrue(rifle.Aim.Aiming);Assert.IsTrue(hud.crosshair.activeSelf);Touch(15,UnityEngine.InputSystem.TouchPhase.Ended,point);yield return null;
  }
  [UnityTest] public IEnumerator SettingsPauseTheRoundAndWorkInMenus() {
   hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.settingsPanel.activeSelf);hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.AreEqual(RoundPhase.Configuration,session.Round.Phase);Assert.IsFalse(hud.settingsPanel.activeSelf);
   Assert.IsTrue(session.StartRound("60"));yield return null;hud.settingsButton.onClick.Invoke();yield return null;
   Assert.IsTrue(session.SettingsOpen);Assert.IsTrue(session.Round.Suspended);double remaining=session.Round.RemainingSeconds;yield return new WaitForSeconds(.3f);Assert.AreEqual(remaining,session.Round.RemainingSeconds);Assert.IsFalse(input.Fire);Assert.IsFalse(hud.rifle.Aim.Aiming);
   hud.closeSettingsButton.onClick.Invoke();yield return new WaitForSeconds(.2f);Assert.IsFalse(session.SettingsOpen);Assert.IsFalse(session.Round.Suspended);Assert.Less(session.Round.RemainingSeconds,remaining);
  }
  [UnityTest] public IEnumerator SprintCancelsAimingButReloadingKeepsIt() {
   hud.sprintModeButton.onClick.Invoke();var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();var aim=Point(hud.aim);var sprint=Point(hud.sprint);
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
   hud.sprintModeButton.onClick.Invoke();var defaultPosition=hud.sprint.anchoredPosition;yield return OpenLayoutEditor();var from=Point(hud.sprint);yield return Drag(34,from,from+new Vector2(-400,300)*CanvasScale());Assert.AreNotEqual(defaultPosition,hud.sprint.anchoredPosition);
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
  [UnityTest] public IEnumerator EachWeaponPlaysItsOwnShotAndReloadClips() {
   var rifle=hud.rifle;var sfx=rifle.sfx;motor.enabled=false;
   for(int i=0;i<rifle.weapons.Length;i++) {
    var def=rifle.weapons[i];Assert.IsNotNull(def.shotClip,def.nameKey);Assert.IsNotNull(def.reloadClip,def.nameKey);yield return SelectWeapon(i);Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();
    yield return HoldFire(70+i,.15f);Assert.AreSame(def.shotClip,sfx.LastClip(Sfx.Shot),def.nameKey+" shot");
    yield return Tap(80+i,hud.reload);Assert.IsTrue(rifle.Ammo.IsReloading,def.nameKey);Assert.AreSame(def.reloadClip,sfx.LastClip(Sfx.Reload),def.nameKey+" reload");
    yield return FinishRound();
   }
  }
  [UnityTest] public IEnumerator ReloadTakesAsLongAsTheWeaponsReloadSound() {
   var rifle=hud.rifle;foreach(int i in new[]{0,3,4}) {
    var def=rifle.weapons[i];Assert.IsNotNull(def.reloadClip,def.nameKey);Assert.AreEqual(def.reloadClip.length,def.reloadSeconds,.01f,def.nameKey);yield return SelectWeapon(i);Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Ammo.TryConsume();yield return Tap(90+i,hud.reload);
    Assert.IsTrue(rifle.Ammo.IsReloading,def.nameKey);yield return new WaitForSeconds(def.reloadSeconds-.5f);Assert.IsTrue(rifle.Ammo.IsReloading,def.nameKey+" must still be reloading before its sound ends.");
    yield return new WaitForSeconds(.7f);Assert.IsFalse(rifle.Ammo.IsReloading,def.nameKey);Assert.AreEqual(def.magazineSize,rifle.Ammo.Count,def.nameKey);yield return FinishRound();
   }
  }
  [UnityTest] public IEnumerator MapShowcaseCapturesViewsFromSeveralAngles() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var cam=Camera.main;var player=motor.transform;
   Assert.AreEqual(CameraClearFlags.Skybox,cam.clearFlags);yield return CapturePreview("map-start.bmp");
   foreach(var view in new[]{("left",-70f),("right",70f),("back",180f)}) {player.rotation=Quaternion.Euler(0,view.Item2,0);yield return null;yield return CapturePreview("map-"+view.Item1+".bmp");}
   player.rotation=Quaternion.identity;cam.transform.position=new Vector3(0,24,-36);cam.transform.rotation=Quaternion.Euler(33,0,0);yield return null;yield return CapturePreview("map-overview.bmp");
  }
  [UnityTest] public IEnumerator FrameLoggerCollectsFrameTimesAndReportsInTheExpectedFormat() {
   var logger=Object.FindFirstObjectByType<FrameLogger>();Assert.IsNotNull(logger,"The player carries the frame logger.");
   yield return new WaitForSeconds(.5f);Assert.Greater(logger.Stats.Count,5);var report=logger.Report();StringAssert.IsMatch(@"^FIELD_OPS_FPS avg=[0-9.]+ p95ms=[0-9.]+ maxms=[0-9.]+ n=[0-9]+$",report);
  }
  // Renders only the sky from many yaw angles and returns, per angle, the strongest vertical-line step relative to the typical column step.
  float[] SkySeamScores(int stepDegrees,float pitch) {
   var go=new GameObject("Sky probe");var cam=go.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.Skybox;cam.cullingMask=0;cam.fieldOfView=40;cam.nearClipPlane=.1f;cam.farClipPlane=10;
   var rt=new RenderTexture(512,512,24);cam.targetTexture=rt;var tex=new Texture2D(512,512,TextureFormat.RGB24,false);var scores=new System.Collections.Generic.List<float>();var previous=RenderTexture.active;
   for(int yaw=0;yaw<360;yaw+=stepDegrees) {
    go.transform.rotation=Quaternion.Euler(-pitch,yaw,0);cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,512,512),0,0);tex.Apply();var px=tex.GetPixels32();
    var steps=new float[511];for(int x=0;x<511;x++){float sum=0;for(int y=0;y<512;y+=4){var a=px[y*512+x];var b=px[y*512+x+1];sum+=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);}steps[x]=sum/128f;}
    var sorted=(float[])steps.Clone();System.Array.Sort(sorted);float median=Mathf.Max(.5f,sorted[sorted.Length/2]);scores.Add(sorted[sorted.Length-1]/median);
   }
   RenderTexture.active=previous;cam.targetTexture=null;Object.Destroy(rt);Object.Destroy(tex);Object.Destroy(go);return scores.ToArray();
  }
  [UnityTest] public IEnumerator SkyShowsNoVerticalSeamLineFromAnyAngle() {
   yield return null;var scores=SkySeamScores(5,10);float worst=scores.Max();int worstYaw=System.Array.IndexOf(scores,worst)*5;
   Assert.Less(worst,8f,"A panorama wrap seam shows as one vertical line far stronger than normal cloud edges (worst at yaw "+worstYaw+", score "+worst.ToString("0.0")+").");
  }
  [UnityTest] public IEnumerator FpsCounterIsOnByDefaultAndShowsAReasonableNumber() {
   Assert.IsTrue(hud.fpsLabel.gameObject.activeInHierarchy,"Visible by default, even in the menu.");yield return new WaitForSeconds(1.2f);
   StringAssert.IsMatch(@"^[0-9]+ FPS$",hud.fpsLabel.text);int value=int.Parse(hud.fpsLabel.text.Split(' ')[0]);Assert.Greater(value,0);Assert.Less(value,2000);
  }
  [UnityTest] public IEnumerator SettingsSwitchHidesAndShowsTheCounterAndRemembersTheChoice() {
   hud.settingsButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.fpsButton.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(GameText.Get("on",false)));
   hud.fpsButton.onClick.Invoke();yield return null;Assert.IsFalse(GameSettings.ShowFps);Assert.IsFalse(hud.fpsLabel.gameObject.activeInHierarchy);Assert.IsTrue(hud.fpsButton.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(GameText.Get("off",false)));
   hud.languageButton.onClick.Invoke();yield return null;Assert.AreEqual(GameText.Get("fpsCounter",true)+": "+GameText.Get("off",true),hud.fpsButton.GetComponentInChildren<UnityEngine.UI.Text>().text);hud.languageButton.onClick.Invoke();
   yield return SceneManager.LoadSceneAsync("TrainingRange");yield return null;yield return null;var reloaded=Object.FindFirstObjectByType<PrototypeHud>();Assert.IsFalse(reloaded.fpsLabel.gameObject.activeInHierarchy,"The choice survives a restart.");
   reloaded.settingsButton.onClick.Invoke();yield return null;reloaded.fpsButton.onClick.Invoke();yield return null;Assert.IsTrue(GameSettings.ShowFps);Assert.IsTrue(reloaded.fpsLabel.gameObject.activeInHierarchy);
  }
  [UnityTest] public IEnumerator LegsCanBeHitFromTheRealPlayerEyeAtTheStartingPosition() {
   motor.enabled=false;yield return new WaitForSeconds(.3f);var eye=Camera.main.transform.position;var legBones=new[]{"UpperLeg.L","LowerLeg.L","Foot.L","UpperLeg.R","LowerLeg.R","Foot.R"};
   foreach(var name in legBones) {
    var bone=target.GetComponentsInChildren<Transform>().First(t=>t.name==name);var aim=bone.position+Vector3.up*.08f;var dir=(aim-eye).normalized;
    Assert.IsTrue(Physics.Raycast(eye,dir,out var hit,150f,~(1<<2),QueryTriggerInteraction.Ignore),name+": the ray hits something");
    Assert.AreSame(target,hit.collider.GetComponentInParent<TrainingTarget>(),name+": the ray hits the soldier, not "+hit.collider.name);
   }
  }
  [UnityTest] public IEnumerator EveryVisibleBodyVertexIsCoveredByAHitZone() {
   motor.enabled=false;yield return new WaitForSeconds(.3f);var eye=Camera.main.transform.position;int total=0,miss=0;var bins=new int[10];
   foreach(var smr in target.GetComponentsInChildren<SkinnedMeshRenderer>()) {
    var baked=new Mesh();smr.BakeMesh(baked,true);var verts=baked.vertices;
    for(int i=0;i<verts.Length;i+=2) {
     var w=smr.transform.TransformPoint(verts[i]);if(w.y<.03f)continue;total++;
     bool ok=Physics.Raycast(eye,(w-eye).normalized,out var hit,150f,~(1<<2),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<TrainingTarget>()==target;if(!ok){miss++;bins[Mathf.Clamp((int)(w.y*5),0,9)]++;}
    }
   }
   Debug.Log("BODYSCAN total="+total+" miss="+miss+" missPer20cm="+string.Join(",",bins));
   Assert.Less(miss,total*0.02f,"Visible vertices not covered by a hit zone: "+miss+"/"+total+" per 20 cm of height: "+string.Join(",",bins));
  }
  [UnityTest] public IEnumerator LegHitZonesFollowTheAnimatedLegsFrameByFrame() {
   motor.enabled=false;yield return new WaitForSeconds(.3f);var center=target.transform.position;var smrs=target.GetComponentsInChildren<SkinnedMeshRenderer>();int total=0,miss=0,fixedBySync=0;var meshes=smrs.Select(_=>new Mesh()).ToArray();
   for(int frame=0;frame<90;frame++) {
    yield return null;var eye=center+Quaternion.Euler(0,(frame*37)%360,0)*new Vector3(0,0,-4f)+Vector3.up*1.6f;
    for(int k=0;k<smrs.Length;k++) {smrs[k].BakeMesh(meshes[k],true);var verts=meshes[k].vertices;
     for(int i=frame%7;i<verts.Length;i+=7) {var w=smrs[k].transform.TransformPoint(verts[i]);if(w.y>.8f||w.y<.08f)continue;if(Physics.Linecast(eye,w,out var block,~(1<<2),QueryTriggerInteraction.Ignore)&&block.collider.GetComponentInParent<TrainingTarget>()!=target)continue;
      total++;bool ok=Physics.Raycast(eye,(w-eye).normalized,out var hit,150f,~(1<<2),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<TrainingTarget>()==target;if(!ok){miss++;Physics.SyncTransforms();if(Physics.Raycast(eye,(w-eye).normalized,out var h2,150f,~(1<<2),QueryTriggerInteraction.Ignore)&&h2.collider.GetComponentInParent<TrainingTarget>()==target)fixedBySync++;}}}
   }
   Debug.Log("LAGSCAN legs total="+total+" miss="+miss+" fixedBySync="+fixedBySync);
   Assert.Less(miss,total*0.02f,"Leg vertices missed while animating: "+miss+"/"+total);
  }
  [UnityTest] public IEnumerator ShotsAimedSteeplyDownwardAreNotBlockedByThePlayersOwnCollider() {
   motor.enabled=false;yield return new WaitForSeconds(.2f);var cam=Camera.main;var player=motor.gameObject;
   foreach(var pitch in new[]{20f,35f,50f,65f}) {
    cam.transform.rotation=Quaternion.Euler(pitch,0,0);
    bool hitSomething=Physics.Raycast(cam.transform.position,cam.transform.forward,out var hit,100f,~(1<<2),QueryTriggerInteraction.Ignore);
    Assert.IsTrue(hitSomething,"pitch "+pitch+": the ray reaches the ground");Assert.AreNotEqual("Player",hit.collider.name,"pitch "+pitch+": the ray starts inside the player capsule and must not hit it");
   }
   Assert.AreEqual(2,player.layer,"The player is on the Ignore Raycast layer so its own capsule never blocks shots.");
  }
  [UnityTest] public IEnumerator JumpAndSlideButtonsExistAreLabelledAndMovable() {
   Assert.IsNotNull(hud.jump);Assert.IsNotNull(hud.slide);Assert.AreEqual("JUMP",hud.jumpLabel.text);Assert.AreEqual("SLIDE",hud.slideLabel.text);
   Assert.AreEqual("Jump touch button",hud.jump.name);Assert.AreEqual("Slide touch button",hud.slide.name);
   Assert.AreSame(hud.jump.GetComponent<UnityEngine.UI.Image>().material,hud.sprint.GetComponent<UnityEngine.UI.Image>().material,"New buttons use the serialized UI material.");
   hud.languageButton.onClick.Invoke();yield return null;Assert.AreEqual("SALTAR",hud.jumpLabel.text);Assert.AreEqual("DESLIZAR",hud.slideLabel.text);hud.languageButton.onClick.Invoke();
   Assert.IsTrue(session.StartRound("60"));yield return null;Assert.IsTrue(hud.jump.gameObject.activeInHierarchy);Assert.IsTrue(hud.slide.gameObject.activeInHierarchy);
  }
  [UnityTest] public IEnumerator JumpButtonSetsPressedOnceAndHeldWhileTheFingerStays() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var jump=Point(hud.jump);
   Touch(60,UnityEngine.InputSystem.TouchPhase.Began,jump);yield return null;Assert.IsTrue(input.JumpPressed);Assert.IsTrue(input.JumpHeld);
   yield return null;yield return null;Assert.IsFalse(input.JumpPressed,"Pressed lasts a single frame.");Assert.IsTrue(input.JumpHeld);
   Touch(60,UnityEngine.InputSystem.TouchPhase.Ended,jump);yield return null;yield return null;Assert.IsFalse(input.JumpHeld);
  }
  [UnityTest] public IEnumerator SlideButtonSetsPressedForASingleFrame() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var slide=Point(hud.slide);
   Touch(61,UnityEngine.InputSystem.TouchPhase.Began,slide);yield return null;Assert.IsTrue(input.SlidePressed);yield return null;yield return null;Assert.IsFalse(input.SlidePressed);Assert.IsFalse(input.Fire,"The slide button is not a fire or look touch.");
   Touch(61,UnityEngine.InputSystem.TouchPhase.Ended,slide);yield return null;
  }
  [UnityTest] public IEnumerator PausedOrMenuStatesClearJumpAndSlide() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var jump=Point(hud.jump);Touch(62,UnityEngine.InputSystem.TouchPhase.Began,jump);yield return null;Assert.IsTrue(input.JumpHeld);
   session.SetSettingsOpen(true);yield return null;yield return null;Assert.IsFalse(input.JumpHeld);Assert.IsFalse(input.JumpPressed);Assert.IsFalse(input.SlidePressed);
   session.SetSettingsOpen(false);Touch(62,UnityEngine.InputSystem.TouchPhase.Ended,jump);yield return null;
  }
  Vector2 StickForward() {return Point(hud.joystick)+Vector2.up*(90*hud.GetComponent<Canvas>().scaleFactor);}   // full tilt
  void PlacePlayer(Vector3 position) {var controller=motor.GetComponent<CharacterController>();controller.enabled=false;motor.transform.position=position;motor.transform.rotation=Quaternion.identity;controller.enabled=true;}
  IEnumerator StartRunning() {Touch(70,UnityEngine.InputSystem.TouchPhase.Began,StickForward());yield return null;}   // the full-tilt stick sprints by itself
  void StopRunning() {Touch(70,UnityEngine.InputSystem.TouchPhase.Ended,StickForward());}
  IEnumerator TapButton(int id,RectTransform button) {var p=Point(button);Touch(id,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;Touch(id,UnityEngine.InputSystem.TouchPhase.Ended,p);yield return null;}
  IEnumerator OpenFieldRound(float z=-18f) {Assert.IsTrue(session.StartRound("120"));yield return null;PlacePlayer(new Vector3(0,.06f,z));yield return new WaitForSeconds(.4f);}
  [UnityTest] public IEnumerator JumpLiftsThePlayerAboutOneMeterAndItLandsAgain() {
   yield return OpenFieldRound();float start=motor.transform.position.y;float max=start;yield return TapButton(72,hud.jump);
   float end=Time.time+1.6f;while(Time.time<end) {max=Mathf.Max(max,motor.transform.position.y);yield return null;}
   Assert.AreEqual(1f,max-start,.2f,"The jump apex is about one meter.");Assert.AreEqual(start,motor.transform.position.y,.1f,"Back on the ground.");Assert.AreEqual(MoveState.Grounded,motor.Model.State);
  }
  [UnityTest] public IEnumerator HoldingJumpWhileSprintingChainsHopsAndBuildsSpeedWithoutLeavingTheField() {
   yield return OpenFieldRound();yield return StartRunning();yield return new WaitForSeconds(.5f);
   var jump=Point(hud.jump);Touch(73,UnityEngine.InputSystem.TouchPhase.Began,jump);float topSpeed=0;int hops=0;float end=Time.time+2.1f;while(Time.time<end) {topSpeed=Mathf.Max(topSpeed,motor.Model.Speed);hops=Mathf.Max(hops,motor.Model.HopCount);yield return null;}
   Touch(73,UnityEngine.InputSystem.TouchPhase.Ended,jump);StopRunning();yield return null;
   Assert.GreaterOrEqual(hops,1,"Landing and jumping again inside the window chains a hop.");Assert.Greater(topSpeed,6.2f,"The chain gains speed over the sprint.");Assert.LessOrEqual(topSpeed,7.51f);Assert.Greater(motor.transform.position.z,-18f);
  }
  [UnityTest] public IEnumerator SlideLowersTheViewAndTheBodyThenStandsUpAgain() {
   yield return OpenFieldRound();yield return StartRunning();yield return new WaitForSeconds(.6f);var camera=Camera.main.transform;var controller=motor.GetComponent<CharacterController>();
   yield return TapButton(74,hud.slide);yield return new WaitForSeconds(.3f);
   Assert.AreEqual(MoveState.Sliding,motor.Model.State);Assert.Less(controller.height,1.3f,"The body shrinks.");Assert.Less(camera.localPosition.y,1.2f,"The view drops.");Assert.Greater(motor.Model.Speed,2.5f);
   yield return new WaitForSeconds(1.5f);StopRunning();yield return new WaitForSeconds(.4f);
   Assert.AreEqual(MoveState.Grounded,motor.Model.State);Assert.AreEqual(1.8f,controller.height,.02f);Assert.AreEqual(.9f,controller.center.y,.02f);Assert.AreEqual(1.65f,camera.localPosition.y,.02f);
  }
  [UnityTest] public IEnumerator WalkingIsTooSlowToSlide() {
   yield return OpenFieldRound();Touch(70,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(45));yield return new WaitForSeconds(.6f);Assert.Less(motor.Model.Speed,5f,"A partial push walks.");yield return TapButton(75,hud.slide);yield return new WaitForSeconds(.2f);
   Assert.AreNotEqual(MoveState.Sliding,motor.Model.State);Touch(70,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(45));yield return null;
  }
  [UnityTest] public IEnumerator JumpingDuringASlideCancelsItAndTheBodyStandsBackUp() {
   yield return OpenFieldRound();yield return StartRunning();yield return new WaitForSeconds(.6f);yield return TapButton(76,hud.slide);yield return new WaitForSeconds(.2f);Assert.AreEqual(MoveState.Sliding,motor.Model.State);float slideSpeed=motor.Model.Speed;
   yield return TapButton(77,hud.jump);yield return new WaitForSeconds(.25f);StopRunning();
   Assert.AreEqual(MoveState.Airborne,motor.Model.State);Assert.AreEqual(1,motor.Model.HopCount);Assert.Greater(motor.Model.Speed,slideSpeed-.8f,"The slide speed carries into the jump.");Assert.Greater(motor.Height,1.6f,"The body is standing again.");
  }
  [UnityTest] public IEnumerator UnderALowCeilingTheSlideStaysCrouchedUntilThereIsRoomToStand() {
   var ceiling=GameObject.CreatePrimitive(PrimitiveType.Cube);ceiling.name="Test ceiling";ceiling.transform.position=new Vector3(0,1.4f,-9f);ceiling.transform.localScale=new Vector3(6,.2f,12);   // z -15 to -3, underside at 1.3 m: room to crouch, not to stand
   yield return OpenFieldRound(-20f);var controller=motor.GetComponent<CharacterController>();yield return StartRunning();
   float guard=Time.time+6f;while(motor.transform.position.z<-16.4f&&Time.time<guard)yield return null;
   yield return TapButton(78,hud.slide);bool crouchedUnder=false,stoodUnder=false;float end=Time.time+8f;
   while(Time.time<end&&motor.transform.position.z<-2f) {float z=motor.transform.position.z;if(z>-12f&&z<-4f) {if(controller.height<1.3f)crouchedUnder=true;else stoodUnder=true;}yield return null;}
   Assert.IsTrue(crouchedUnder,"It slides under the ceiling.");Assert.IsFalse(stoodUnder,"It never stands up while the ceiling is above, even after the slide time is over.");
   guard=Time.time+5f;while(motor.transform.position.z<-1.5f&&Time.time<guard)yield return null;yield return new WaitForSeconds(.6f);StopRunning();yield return new WaitForSeconds(.3f);
   Assert.AreEqual(1.8f,controller.height,.05f,"It stands up once past the ceiling.");Object.Destroy(ceiling);
  }
  [UnityTest] public IEnumerator BoundaryWallsAreTallerThanAnyJumpCanReach() {
   foreach(var name in new[]{"Boundary west","Boundary east","Boundary south","Boundary north"}) {var wall=GameObject.Find(name);Assert.IsNotNull(wall,name);Assert.GreaterOrEqual(wall.GetComponent<Collider>().bounds.max.y,2.2f,name+" is too low: a jump could clear it.");}
   yield return null;
  }
  Vector2 StickTilt(float canvasPixels) {return Point(hud.joystick)+Vector2.up*(canvasPixels*hud.GetComponent<Canvas>().scaleFactor);}
  [UnityTest] public IEnumerator FullTiltStickSprintsAndAPartialPushOnlyWalks() {
   motor.enabled=false;Assert.IsTrue(GameSettings.SprintByJoystick);Assert.IsTrue(session.StartRound("60"));yield return null;
   Touch(80,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(45));yield return null;yield return null;Assert.Greater(input.Move.magnitude,.4f);Assert.IsFalse(input.Sprint,"A partial push walks.");
   Touch(80,UnityEngine.InputSystem.TouchPhase.Moved,StickTilt(90));yield return null;yield return null;Assert.IsTrue(input.Sprint,"The edge of the stick sprints.");
   Touch(80,UnityEngine.InputSystem.TouchPhase.Moved,StickTilt(68));yield return null;yield return null;Assert.IsTrue(input.Sprint,"Easing a little does not drop the sprint (hysteresis).");
   Touch(80,UnityEngine.InputSystem.TouchPhase.Moved,StickTilt(50));yield return null;yield return null;Assert.IsFalse(input.Sprint,"Easing a lot does.");
   Touch(80,UnityEngine.InputSystem.TouchPhase.Moved,StickTilt(90));yield return null;yield return null;Assert.IsTrue(input.Sprint);Touch(80,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(90));yield return null;yield return null;Assert.IsFalse(input.Sprint,"Letting go stops it.");
  }
  [UnityTest] public IEnumerator InJoystickModeTheSprintButtonIsHiddenAndIgnored() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var button=Point(hud.sprint);Assert.IsFalse(hud.sprint.gameObject.activeInHierarchy);
   Touch(81,UnityEngine.InputSystem.TouchPhase.Began,button);yield return null;yield return null;Assert.IsFalse(input.Sprint);Touch(81,UnityEngine.InputSystem.TouchPhase.Ended,button);yield return null;
  }
  [UnityTest] public IEnumerator SprintModeSettingSwitchesBetweenJoystickAndButtonAndIsRemembered() {
   motor.enabled=false;Assert.IsNotNull(hud.sprintModeButton);hud.settingsButton.onClick.Invoke();yield return null;StringAssert.Contains("JOYSTICK",hud.sprintModeLabel.text);
   hud.sprintModeButton.onClick.Invoke();yield return null;Assert.IsFalse(GameSettings.SprintByJoystick);StringAssert.Contains("BUTTON",hud.sprintModeLabel.text);
   hud.languageButton.onClick.Invoke();yield return null;StringAssert.Contains("BOTÓN",hud.sprintModeLabel.text);hud.languageButton.onClick.Invoke();
   hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.IsTrue(session.StartRound("60"));yield return null;Assert.IsTrue(hud.sprint.gameObject.activeInHierarchy,"The button is back.");
   var button=Point(hud.sprint);Touch(82,UnityEngine.InputSystem.TouchPhase.Began,button);yield return null;yield return null;Assert.IsTrue(input.Sprint,"Holding the button sprints.");Touch(82,UnityEngine.InputSystem.TouchPhase.Ended,button);yield return null;yield return null;Assert.IsFalse(input.Sprint);
   Touch(83,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(90));yield return null;yield return null;Assert.IsFalse(input.Sprint,"In button mode a full push does not sprint.");Touch(83,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(90));yield return null;
  }
  [UnityTest] public IEnumerator TitleTabShowsForTheFirstFiveSecondsOfEachRoundOnly() {
   motor.enabled=false;Assert.IsNotNull(hud.headerGroup);Assert.AreEqual(0f,hud.headerGroup.alpha,.01f,"Not shown in the menu.");
   Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(.3f);Assert.AreEqual(1f,hud.headerGroup.alpha,.01f);StringAssert.Contains("FIELD OPS",hud.title.text);
   yield return new WaitForSeconds(5.2f);Assert.AreEqual(0f,hud.headerGroup.alpha,.01f,"Gone after five seconds.");Assert.IsFalse(hud.headerGroup.blocksRaycasts);
   hud.settingsButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual(1f,hud.headerGroup.alpha,.01f,"The paused notice shares the header, so it shows while the round is suspended.");
   hud.closeSettingsButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual(0f,hud.headerGroup.alpha,.01f);
   session.Round.Tick(60);yield return null;yield return null;Assert.AreEqual(0f,hud.headerGroup.alpha,.01f,"Not shown on the results screen.");
   hud.replayButton.onClick.Invoke();yield return new WaitForSeconds(.3f);Assert.AreEqual(1f,hud.headerGroup.alpha,.01f,"A new round shows it again.");
  }
  [UnityTest] public IEnumerator PausedTimeDoesNotUseUpTheTitleTab() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(1f);session.SetPause(true);yield return new WaitForSeconds(5f);session.SetPause(false);yield return new WaitForSeconds(.3f);
   Assert.Greater(hud.headerGroup.alpha,.9f,"Only about 1.3 s of play time has passed.");
  }
  [UnityTest] public IEnumerator SettingsShowTheGameTitleAsAHeaderLine() {
   Assert.IsNotNull(hud.settingsSubtitle);hud.settingsButton.onClick.Invoke();yield return null;Assert.AreEqual("FIELD OPS / PROTOTYPE",hud.settingsSubtitle.text);Assert.IsTrue(hud.settingsSubtitle.gameObject.activeInHierarchy);
   var panel=(RectTransform)hud.settingsPanel.transform;var corners=new Vector3[4];panel.GetWorldCorners(corners);var box=((RectTransform)hud.settingsSubtitle.transform);var sub=new Vector3[4];box.GetWorldCorners(sub);
   Assert.Greater(sub[0].y,corners[0].y);Assert.Less(sub[1].y,corners[1].y,"The subtitle sits inside the panel.");
   var title=(RectTransform)hud.settingsTitle.transform;var t=new Vector3[4];title.GetWorldCorners(t);Assert.GreaterOrEqual(t[0].y,sub[1].y-1f,"It does not overlap the settings title.");
   hud.closeSettingsButton.onClick.Invoke();yield return null;
  }
  RectTransform LanguageRect() {return (RectTransform)hud.languageButton.transform;}
  [UnityTest] public IEnumerator LanguageButtonAndFpsCounterCanBeDraggedSavedAndRestoredAfterAReload() {
   var langDefault=LanguageRect().anchoredPosition;var fpsDefault=hud.fpsLabel.rectTransform.anchoredPosition;yield return OpenLayoutEditor();
   var from=Point(LanguageRect());yield return Drag(90,from,from+new Vector2(-250,-400)*CanvasScale());var langMoved=LanguageRect().anchoredPosition-langDefault;Assert.AreEqual(-250,langMoved.x,3f);Assert.AreEqual(-400,langMoved.y,3f);
   from=Point(hud.fpsLabel.rectTransform);yield return Drag(91,from,from+new Vector2(-450,-120)*CanvasScale());var fpsMoved=hud.fpsLabel.rectTransform.anchoredPosition-fpsDefault;Assert.AreEqual(-450,fpsMoved.x,3f);Assert.AreEqual(-120,fpsMoved.y,3f);
   Assert.IsTrue(hud.saveLayoutButton.interactable);var savedLang=LanguageRect().anchoredPosition;var savedFps=hud.fpsLabel.rectTransform.anchoredPosition;hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(PlayerPrefs.HasKey("hudLayout"));
   yield return SceneManager.LoadSceneAsync("TrainingRange");yield return null;yield return null;var reloaded=Object.FindFirstObjectByType<PrototypeHud>();Canvas.ForceUpdateCanvases();
   Assert.AreEqual(savedLang.x,((RectTransform)reloaded.languageButton.transform).anchoredPosition.x,.5f);Assert.AreEqual(savedLang.y,((RectTransform)reloaded.languageButton.transform).anchoredPosition.y,.5f);
   Assert.AreEqual(savedFps.x,reloaded.fpsLabel.rectTransform.anchoredPosition.x,.5f);Assert.AreEqual(savedFps.y,reloaded.fpsLabel.rectTransform.anchoredPosition.y,.5f);
  }
  [UnityTest] public IEnumerator DraggingTheLanguageButtonNeverSwitchesTheLanguage() {
   yield return OpenLayoutEditor();Assert.IsFalse(hud.languageButton.interactable,"The button is inert while editing, so a drag is never a click.");
   var from=Point(LanguageRect());yield return Drag(92,from,from+new Vector2(-250,-400)*CanvasScale());Assert.AreEqual(0,PlayerPrefs.GetInt("language",0));Assert.AreEqual("FIRE",hud.fireLabel.text);
   hud.cancelLayoutButton.onClick.Invoke();yield return null;yield return null;Assert.IsTrue(hud.languageButton.interactable,"It works again after editing.");
  }
  [UnityTest] public IEnumerator FpsCounterCanBePlacedEvenWhileItIsHidden() {
   hud.fpsButton.onClick.Invoke();yield return null;Assert.IsFalse(GameSettings.ShowFps);Assert.IsFalse(hud.fpsLabel.gameObject.activeInHierarchy);
   yield return OpenLayoutEditor();Assert.IsTrue(hud.fpsLabel.gameObject.activeInHierarchy,"It shows while editing so it can be placed.");
   var from=Point(hud.fpsLabel.rectTransform);yield return Drag(93,from,from+new Vector2(-450,-120)*CanvasScale());Assert.IsTrue(hud.saveLayoutButton.interactable);hud.saveLayoutButton.onClick.Invoke();yield return null;yield return null;
   Assert.IsFalse(hud.fpsLabel.gameObject.activeInHierarchy,"Hidden again after editing because the option is off.");Assert.IsTrue(PlayerPrefs.HasKey("hudLayout"));
  }
  [UnityTest] public IEnumerator MovedLanguageButtonIsTheMenuSpotAndTheOldSpotPlaysAgain() {
   var oldPoint=Point(LanguageRect());yield return OpenLayoutEditor();yield return Drag(94,oldPoint,oldPoint+new Vector2(-250,-400)*CanvasScale());var newPoint=Point(LanguageRect());hud.saveLayoutButton.onClick.Invoke();yield return null;
   Assert.IsTrue(session.StartRound("60"));yield return null;Canvas.ForceUpdateCanvases();Assert.IsTrue(hud.IsMenu(newPoint),"The moved button is the menu spot.");Assert.IsFalse(hud.IsMenu(oldPoint),"The old spot is free again.");
  }
  [UnityTest] public IEnumerator PlacingTheFpsCounterOnTheLanguageButtonBlocksSavingAndResetRestoresBoth() {
   var langDefault=LanguageRect().anchoredPosition;var fpsDefault=hud.fpsLabel.rectTransform.anchoredPosition;yield return OpenLayoutEditor();
   var fps=Point(hud.fpsLabel.rectTransform);var lang=Point(LanguageRect());yield return Drag(95,fps,lang);Assert.IsFalse(hud.saveLayoutButton.interactable);hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.EditingLayout);
   hud.resetLayoutButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.saveLayoutButton.interactable);Assert.AreEqual(langDefault,LanguageRect().anchoredPosition);Assert.AreEqual(fpsDefault,hud.fpsLabel.rectTransform.anchoredPosition);
   hud.saveLayoutButton.onClick.Invoke();yield return null;Assert.IsFalse(PlayerPrefs.HasKey("hudLayout"),"Defaults store nothing.");
  }
  [UnityTest] public IEnumerator SprintSensitivityButtonCyclesLevelsAndChangesHowFarTheStickMustGo() {
   motor.enabled=false;Assert.IsNotNull(hud.sprintSensButton);hud.settingsButton.onClick.Invoke();yield return null;StringAssert.Contains("MEDIUM",hud.sprintSensLabel.text);
   hud.sprintSensButton.onClick.Invoke();yield return null;Assert.AreEqual(SprintSensitivity.Low,GameSettings.SprintSensitivity);StringAssert.Contains("LOW",hud.sprintSensLabel.text);
   hud.sprintSensButton.onClick.Invoke();yield return null;Assert.AreEqual(SprintSensitivity.High,GameSettings.SprintSensitivity);StringAssert.Contains("HIGH",hud.sprintSensLabel.text);
   hud.languageButton.onClick.Invoke();yield return null;StringAssert.Contains("ALTA",hud.sprintSensLabel.text);hud.languageButton.onClick.Invoke();
   hud.closeSettingsButton.onClick.Invoke();yield return null;Assert.IsTrue(session.StartRound("60"));yield return null;
   Touch(100,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(66));yield return null;yield return null;Assert.IsTrue(input.Sprint,"High sensitivity sprints with a short push (about 82 percent).");Touch(100,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(66));yield return null;yield return null;
   hud.settingsButton.onClick.Invoke();yield return null;hud.sprintSensButton.onClick.Invoke();yield return null;hud.sprintSensButton.onClick.Invoke();yield return null;Assert.AreEqual(SprintSensitivity.Low,GameSettings.SprintSensitivity);hud.closeSettingsButton.onClick.Invoke();yield return null;
   Touch(101,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(74));yield return null;yield return null;Assert.IsFalse(input.Sprint,"Low sensitivity needs about 97 percent.");Touch(101,UnityEngine.InputSystem.TouchPhase.Moved,StickTilt(80));yield return null;yield return null;Assert.IsTrue(input.Sprint);Touch(101,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(80));yield return null;
  }
  [UnityTest] public IEnumerator SensitivityRowIsInertWhenSprintIsOnTheButtonAndExplainsWhy() {
   hud.sprintModeButton.onClick.Invoke();hud.settingsButton.onClick.Invoke();yield return null;Assert.IsFalse(hud.sprintSensButton.interactable);StringAssert.Contains("JOYSTICK MODE ONLY",hud.sprintSensLabel.text);
   hud.sprintModeButton.onClick.Invoke();yield return null;Assert.IsTrue(hud.sprintSensButton.interactable);StringAssert.DoesNotContain("ONLY",hud.sprintSensLabel.text);
  }
  float MuzzleRise() {var rifle=hud.rifle;return rifle.muzzleFlash.transform.position.y-Camera.main.transform.position.y;}
  [UnityTest] public IEnumerator SprintingRaisesTheWeaponAndReleasingLowersItAgain() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(.4f);float hip=MuzzleRise();Assert.AreEqual(0f,rifle.Sprint.Blend,.001f);
   Touch(110,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(90));yield return new WaitForSeconds(.6f);
   Assert.IsTrue(input.Sprint);Assert.AreEqual(1f,rifle.Sprint.Blend,.05f);Assert.Greater(MuzzleRise()-hip,.2f,"The muzzle points up while sprinting.");yield return CapturePreview("sprint-pose.bmp");
   Touch(110,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(90));yield return new WaitForSeconds(.4f);
   Assert.AreEqual(0f,rifle.Sprint.Blend,.02f);Assert.AreEqual(hip,MuzzleRise(),.03f,"Back to the firing pose.");
  }
  [UnityTest] public IEnumerator FiringWhileSprintingDropsTheWeaponAndStillHitsTheTarget() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();
   Touch(111,UnityEngine.InputSystem.TouchPhase.Began,StickTilt(90));yield return new WaitForSeconds(.5f);Assert.Greater(rifle.Sprint.Blend,.9f);
   var fire=Point(hud.fire);Touch(112,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.35f);
   Assert.Less(rifle.Sprint.Blend,.05f,"The weapon drops to the firing pose.");Assert.Greater(rifle.Ammo.Capacity-rifle.Ammo.Count,0,"It fired.");Assert.Greater(target.Damage.Hits,0,"And it hit.");
   Touch(112,UnityEngine.InputSystem.TouchPhase.Ended,fire);Touch(111,UnityEngine.InputSystem.TouchPhase.Ended,StickTilt(90));yield return null;
  }
  [UnityTest] public IEnumerator HoldingTheSprintButtonWithoutMovingDoesNotRaiseTheWeapon() {
   motor.enabled=false;hud.sprintModeButton.onClick.Invoke();var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;Canvas.ForceUpdateCanvases();
   var button=Point(hud.sprint);Touch(113,UnityEngine.InputSystem.TouchPhase.Began,button);yield return new WaitForSeconds(.5f);Assert.IsTrue(input.Sprint);Assert.AreEqual(0f,rifle.Sprint.Blend,.001f,"Standing still.");Touch(113,UnityEngine.InputSystem.TouchPhase.Ended,button);yield return null;
  }
  [UnityTest] public IEnumerator SlidingKeepsTheNormalPoseInsteadOfTheSprintPose() {
   yield return OpenFieldRound();var rifle=hud.rifle;yield return StartRunning();yield return new WaitForSeconds(.8f);Assert.Greater(rifle.Sprint.Blend,.9f,"Raised while running.");
   yield return TapButton(114,hud.slide);yield return new WaitForSeconds(.4f);Assert.AreEqual(MoveState.Sliding,motor.Model.State);Assert.Less(rifle.Sprint.Blend,.1f,"Lowered during the slide.");StopRunning();yield return null;
  }
  Vector2 StickDir(float degreesFromForward,float canvasPixels=90f) {float r=degreesFromForward*Mathf.Deg2Rad;return Point(hud.joystick)+new Vector2(Mathf.Sin(r),Mathf.Cos(r))*(canvasPixels*CanvasScale());}
  [UnityTest] public IEnumerator StrafingHardToBothSidesWhileAimingKeepsTheAimAndOnlyWalks() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var rifle=hud.rifle;var aim=Point(hud.aim);Touch(120,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Aim.Aiming);
   Touch(121,UnityEngine.InputSystem.TouchPhase.Began,StickDir(90));yield return new WaitForSeconds(.4f);Assert.Greater(input.Move.x,.9f,"Full push to the right.");Assert.IsFalse(input.Sprint,"Sideways never sprints.");Assert.IsTrue(rifle.Aim.Aiming,"The aim stays.");
   Touch(121,UnityEngine.InputSystem.TouchPhase.Moved,StickDir(-90));yield return new WaitForSeconds(.4f);Assert.Less(input.Move.x,-.9f,"Full push to the left.");Assert.IsFalse(input.Sprint);Assert.IsTrue(rifle.Aim.Aiming,"Still aiming after switching sides.");
   Touch(121,UnityEngine.InputSystem.TouchPhase.Moved,StickDir(45));yield return new WaitForSeconds(.3f);Assert.IsFalse(input.Sprint,"A forward-right diagonal does not sprint either.");Assert.IsTrue(rifle.Aim.Aiming);
   Touch(121,UnityEngine.InputSystem.TouchPhase.Ended,StickDir(45));Touch(120,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
  }
  [UnityTest] public IEnumerator PushingStraightForwardToTheEdgeStillSprintsAndLeavesTheAim() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return null;var rifle=hud.rifle;var aim=Point(hud.aim);Touch(122,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Aim.Aiming);
   Touch(123,UnityEngine.InputSystem.TouchPhase.Began,StickDir(0));yield return new WaitForSeconds(.3f);Assert.IsTrue(input.Sprint);Assert.IsFalse(rifle.Aim.Aiming,"Sprinting forward exits the aim, as in CoD.");
   Touch(123,UnityEngine.InputSystem.TouchPhase.Ended,StickDir(0));Touch(122,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
  }
  [UnityTest] public IEnumerator StrafingAtTheEdgeMovesAtWalkSpeedNotSprintSpeed() {
   yield return OpenFieldRound();var start=motor.transform.position;Touch(124,UnityEngine.InputSystem.TouchPhase.Began,StickDir(90));yield return new WaitForSeconds(.8f);
   Assert.AreEqual(3.5f,motor.Model.Speed,.2f,"Walk speed sideways.");Assert.IsFalse(input.Sprint);Touch(124,UnityEngine.InputSystem.TouchPhase.Ended,StickDir(90));yield return null;
  }
  [UnityTest] public IEnumerator WhileAimingTheSightLineOfEveryWeaponSitsOnTheScreenCentre() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;var cam=Camera.main;
   for(int i=0;i<rifle.weapons.Length;i++) {
    rifle.Equip(i);yield return null;var aim=Point(hud.aim);Touch(130+i,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.5f);Assert.AreEqual(1f,rifle.Aim.Blend,.01f,rifle.Current.nameKey);
    var rear=cam.WorldToViewportPoint(rifle.models[i].Find("Sight rear point").position);var front=cam.WorldToViewportPoint(rifle.models[i].Find("Sight front point").position);
    Assert.AreEqual(.5f,rear.x,.02f,rifle.Current.nameKey+" rear x");Assert.AreEqual(.5f,rear.y,.02f,rifle.Current.nameKey+" rear y");Assert.AreEqual(.5f,front.x,.02f,rifle.Current.nameKey+" front x");Assert.AreEqual(.5f,front.y,.02f,rifle.Current.nameKey+" front y");Assert.Greater(rear.z,0f);Assert.Greater(front.z,rear.z);
    yield return CapturePreview("sights-"+i+"-aim.bmp");Touch(130+i,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return new WaitForSeconds(.3f);yield return CapturePreview("sights-"+i+"-hip.bmp");
   }
  }
  int SniperIndex() {var rifle=hud.rifle;return System.Array.FindIndex(rifle.weapons,w=>w.nameKey=="weaponSniper");}
  [UnityTest] public IEnumerator AimingTheSniperClosesTheSidesHidesTheGunAndReleasingBringsItBack() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;int i=SniperIndex();rifle.Equip(i);yield return null;
   Assert.AreEqual(0f,hud.scope.Alpha,.001f);Assert.IsTrue(rifle.models[i].gameObject.activeSelf);var aim=Point(hud.aim);Touch(140,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.6f);
   Assert.Greater(hud.scope.Alpha,.95f,"The scope view is fully on.");Assert.Greater(rifle.ScopeBlend,.95f);Assert.IsFalse(rifle.models[i].gameObject.activeSelf,"The gun is hidden: the player looks through the scope.");Assert.IsFalse(hud.crosshair.activeSelf,"The scope has its own reticle.");
   Assert.AreEqual(.3f,input.LookScale,.03f,"Slow, precise turning.");Assert.AreEqual(20f,Camera.main.fieldOfView,.5f);
   var rect=((RectTransform)hud.scope.transform).rect;float side=hud.scope.circle.rect.width;Assert.AreEqual(rect.height,side,1f,"The circle square is as tall as the screen.");Assert.GreaterOrEqual(hud.scope.left.rect.width+side+hud.scope.right.rect.width,rect.width-1f,"The sides close in: nothing is left uncovered.");
   Assert.AreEqual(hud.scope.holeFraction*side,hud.scope.horizontal.rect.width,2f,"The reticle spans the opening.");
   yield return CapturePreview("scope-view.bmp");Touch(140,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return new WaitForSeconds(.4f);
   Assert.AreEqual(0f,hud.scope.Alpha,.01f);Assert.IsTrue(rifle.models[i].gameObject.activeSelf,"The gun is back.");Assert.AreEqual(1f,input.LookScale,.02f);
  }
  [UnityTest] public IEnumerator OtherWeaponsNeverShowTheScopeView() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;
   for(int i=0;i<rifle.weapons.Length;i++) {if(i==SniperIndex())continue;rifle.Equip(i);yield return null;var aim=Point(hud.aim);Touch(141+i,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.4f);Assert.AreEqual(0f,hud.scope.Alpha,.001f,rifle.Current.nameKey);Assert.IsTrue(rifle.models[i].gameObject.activeSelf,rifle.Current.nameKey);Touch(141+i,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;}
  }
  [UnityTest] public IEnumerator SprintingForwardLeavesTheScopeViewAndFiringWorksWhileScoped() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;int i=SniperIndex();rifle.Equip(i);yield return null;AimAtTarget();var aim=Point(hud.aim);Touch(150,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.5f);Assert.Greater(hud.scope.Alpha,.95f);
   int before=rifle.Ammo.Count;var fire=Point(hud.fire);Touch(151,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(.15f);Touch(151,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;Assert.Less(rifle.Ammo.Count,before,"The overlay does not block the fire button.");
   Touch(152,UnityEngine.InputSystem.TouchPhase.Began,StickDir(0));yield return new WaitForSeconds(.5f);Assert.AreEqual(0f,hud.scope.Alpha,.05f);Assert.IsTrue(rifle.models[i].gameObject.activeSelf,"Sprinting leaves the scope and shows the gun.");
   Touch(152,UnityEngine.InputSystem.TouchPhase.Ended,StickDir(0));Touch(150,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
  }
  int WeaponIndex(string key) {return System.Array.FindIndex(hud.rifle.weapons,w=>w.nameKey==key);}
  IEnumerator TapFire(int id,float seconds=.08f) {var fire=Point(hud.fire);Touch(id,UnityEngine.InputSystem.TouchPhase.Began,fire);yield return new WaitForSeconds(seconds);Touch(id,UnityEngine.InputSystem.TouchPhase.Ended,fire);yield return null;}
  float WeaponTurn() {return Quaternion.Angle(hud.rifle.weapon.localRotation,Quaternion.identity);}
  [UnityTest] public IEnumerator TheSniperWorksItsBoltDuringTheCooldownAndSoundsOnceThenReturnsToTheFiringPose() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponSniper"));yield return null;AimAtTarget();
   Assert.IsFalse(rifle.Cycle.Active);Assert.Less(WeaponTurn(),.5f);yield return TapFire(170);yield return new WaitForSeconds(.1f);Assert.IsTrue(rifle.Cycle.Active,"The cycle starts right after the shot.");
   yield return new WaitForSeconds(.4f);Assert.Greater(WeaponTurn(),12f,"Mid-cycle the gun is turned for the bolt.");yield return CapturePreview("sniper-bolt.bmp");
   yield return new WaitForSeconds(.9f);Assert.IsFalse(rifle.Cycle.Active);Assert.Less(WeaponTurn(),1f,"Back in the firing pose when the cooldown ends.");Assert.AreEqual(1,rifle.sfx.Count(Sfx.Cycle),"One bolt sound per shot.");Assert.AreSame(rifle.Current.cycleClip,rifle.sfx.LastClip(Sfx.Cycle));
  }
  [UnityTest] public IEnumerator AimingTheSniperLeavesTheScopeDuringTheBoltAndReturnsWhileAimIsStillHeld() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponSniper"));yield return null;AimAtTarget();var aim=Point(hud.aim);Touch(171,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.5f);Assert.Greater(hud.scope.Alpha,.95f);
   yield return TapFire(172);yield return new WaitForSeconds(.45f);Assert.IsTrue(rifle.Aim.Suppressed);Assert.Less(hud.scope.Alpha,.2f,"Out of the scope while the bolt is worked.");Assert.IsTrue(rifle.models[WeaponIndex("weaponSniper")].gameObject.activeSelf,"The gun is visible, so the player sees the cycle.");
   yield return new WaitForSeconds(1.1f);Assert.IsFalse(rifle.Aim.Suppressed);Assert.Greater(hud.scope.Alpha,.9f,"Back in the scope with no new press, because AIM is still held.");
   Touch(171,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
  }
  [UnityTest] public IEnumerator ReleasingAimDuringTheBoltDoesNotBringTheScopeBack() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponSniper"));yield return null;AimAtTarget();var aim=Point(hud.aim);Touch(173,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.5f);
   yield return TapFire(174);yield return new WaitForSeconds(.3f);Touch(173,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return new WaitForSeconds(1.3f);Assert.AreEqual(0f,hud.scope.Alpha,.01f);Assert.IsFalse(rifle.Aim.Aiming);
  }
  [UnityTest] public IEnumerator TheShotgunPumpsAfterEachShotWithASmallerMotionThanTheBolt() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponShotgun"));yield return null;AimAtTarget();
   yield return TapFire(175);float peak=0;float end=Time.time+.9f;while(Time.time<end){peak=Mathf.Max(peak,WeaponTurn());yield return null;}
   Assert.That(peak,Is.InRange(1.5f,16f),"A smaller turn than the sniper bolt.");Assert.IsFalse(rifle.Cycle.Active);Assert.Less(WeaponTurn(),1f);Assert.AreEqual(1,rifle.sfx.Count(Sfx.Cycle));
  }
  [UnityTest] public IEnumerator PistolRifleAndSubMachineGunHaveNoHandCycle() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;AimAtTarget();
   foreach(var key in new[]{"weaponPistol","weaponRifle","weaponSmg"}) {rifle.Equip(WeaponIndex(key));yield return null;yield return TapFire(176);float peak=0;float end=Time.time+.5f;while(Time.time<end){peak=Mathf.Max(peak,WeaponTurn());Assert.IsFalse(rifle.Cycle.Active,key);yield return null;}Assert.Less(peak,.5f,key+" keeps its normal pose.");Assert.AreEqual(0,rifle.sfx.Count(Sfx.Cycle),key);}
  }
  [UnityTest] public IEnumerator SwappingWeaponOrReloadingEndsTheCycleAtOnce() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponSniper"));yield return null;AimAtTarget();var aim=Point(hud.aim);Touch(177,UnityEngine.InputSystem.TouchPhase.Began,aim);yield return new WaitForSeconds(.4f);
   yield return TapFire(178);yield return new WaitForSeconds(.3f);Assert.IsTrue(rifle.Cycle.Active);rifle.Equip(WeaponIndex("weaponRifle"));yield return null;yield return null;Assert.IsFalse(rifle.Cycle.Active);Assert.IsFalse(rifle.Aim.Suppressed,"The aim is no longer held back after a swap.");Assert.Less(WeaponTurn(),1f);
   Touch(177,UnityEngine.InputSystem.TouchPhase.Ended,aim);yield return null;
   rifle.Equip(WeaponIndex("weaponShotgun"));yield return null;yield return TapFire(179);yield return new WaitForSeconds(.2f);Assert.IsTrue(rifle.Cycle.Active);yield return Tap(180,hud.reload);yield return new WaitForSeconds(.1f);Assert.IsFalse(rifle.Cycle.Active,"Starting a reload ends the pump.");
  }
  [UnityTest] public IEnumerator PausingTheRoundStopsTheCycleAndItsSound() {
   motor.enabled=false;var rifle=hud.rifle;Assert.IsTrue(session.StartRound("60"));yield return null;rifle.Equip(WeaponIndex("weaponSniper"));yield return null;AimAtTarget();yield return TapFire(181);yield return new WaitForSeconds(.2f);Assert.IsTrue(rifle.Cycle.Active);
   int stops=rifle.sfx.StopCount;session.SetPause(true);yield return null;yield return null;Assert.IsFalse(rifle.Cycle.Active);Assert.Greater(rifle.sfx.StopCount,stops,"The bolt sound is cut.");session.SetPause(false);
  }
  [UnityTest] public IEnumerator ImpactMarksNeedNoRuntimePrimitiveSoTheBuildDoesNotStripTheColliderClass() {
   Assert.IsNotNull(target.markMesh,"The mark mesh is serialized in the scene.");
   var cam=Camera.main;motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(.2f);cam.transform.LookAt(target.transform.position+Vector3.up*1.25f);
   Assert.IsTrue(Physics.Raycast(cam.transform.position,cam.transform.forward,out var hit,150f,~(1<<2)));Assert.IsTrue(target.ReceiveHit(hit,25));
   var mark=target.GetComponentsInChildren<Renderer>().First(r=>r.name=="Bullet impact");Assert.IsNull(mark.GetComponent<Collider>());Assert.IsNotNull(mark.GetComponent<MeshFilter>().sharedMesh);
  }
  [UnityTest] public IEnumerator SoldierIdlesWithRealAnimationAndEveryBodyPartCanBeHit() {
   var animator=target.animator;Assert.IsNotNull(animator);var head=target.GetComponentsInChildren<Transform>().First(t=>t.name=="Head");yield return new WaitForSeconds(.4f);
   float min=float.MaxValue,max=float.MinValue;float end=Time.time+1.8f;while(Time.time<end){var y=head.position.y;min=Mathf.Min(min,y);max=Mathf.Max(max,y);yield return null;}Assert.Greater(max-min,.004f,"The idle animation moves the body (breathing).");
   Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
   var origin=new Vector3(0,0,-12);foreach(var height in new[]{1.65f,1.25f,1.0f,.9f}) {Assert.IsTrue(Physics.Raycast(new Vector3(0,height,-8),Vector3.forward,out var hit,20f,~(1<<2)),"ray at "+height);Assert.AreSame(target,hit.collider.GetComponentInParent<TrainingTarget>(),"ray at "+height);}
   bool leg=Physics.Raycast(new Vector3(.1f,.5f,-8),Vector3.forward,out var legHit,20f,~(1<<2))&&legHit.collider.GetComponentInParent<TrainingTarget>()==target||Physics.Raycast(new Vector3(-.1f,.5f,-8),Vector3.forward,out legHit,20f,~(1<<2))&&legHit.collider.GetComponentInParent<TrainingTarget>()==target;Assert.IsTrue(leg,"A leg can be hit.");
  }
  [UnityTest] public IEnumerator HitMakesTheSoldierReactAndThenReturnToIdleWhileStayingIndestructible() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(.3f);AimAtTarget();yield return HoldFire(95,.25f);
   Assert.Greater(target.Damage.Hits,0);var animator=target.animator;float until=Time.time+.5f;bool reacted=false;while(Time.time<until&&!reacted){reacted=animator.GetCurrentAnimatorStateInfo(0).IsName("Hit")||animator.IsInTransition(0)&&animator.GetNextAnimatorStateInfo(0).IsName("Hit");yield return null;}
   Assert.IsTrue(reacted,"The soldier plays the hit reaction.");yield return new WaitForSeconds(1.2f);Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"It returns to idle.");Assert.IsTrue(target.gameObject.activeInHierarchy);Assert.IsTrue(target.GetComponentInChildren<SkinnedMeshRenderer>().enabled,"Indestructible: it is never removed.");
  }
  [UnityTest] public IEnumerator SoldierShowcaseCapturesCloseAndFarViews() {
   motor.enabled=false;Assert.IsTrue(session.StartRound("60"));yield return new WaitForSeconds(.8f);var cam=Camera.main;AimAtTarget();yield return null;yield return CapturePreview("soldier-far.bmp");
   cam.transform.position=new Vector3(0,1.5f,-1.2f);cam.transform.LookAt(target.transform.position+Vector3.up*1.1f);yield return new WaitForSeconds(.2f);yield return CapturePreview("soldier-close.bmp");
   for(int i=0;i<4;i++)target.ReceiveHit(HitAt(new Vector3(0.05f*i,1.1f+0.1f*i,1.95f)),25);yield return new WaitForSeconds(.15f);yield return CapturePreview("soldier-hit.bmp");
   cam.transform.position=new Vector3(2.4f,1.3f,.2f);cam.transform.LookAt(target.transform.position+Vector3.up*1f);yield return new WaitForSeconds(1.0f);yield return CapturePreview("soldier-side.bmp");
  }
  RaycastHit HitAt(Vector3 point) {Physics.Raycast(point+Vector3.back*2f,Vector3.forward,out var hit,6f,~(1<<2));return hit;}
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
