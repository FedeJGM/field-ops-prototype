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
   PlayerPrefs.SetInt("language",0);
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
  [UnityTearDown] public IEnumerator Cleanup() {InputSystem.RemoveDevice(screen);PlayerPrefs.SetInt("language",0);yield return null;}
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
