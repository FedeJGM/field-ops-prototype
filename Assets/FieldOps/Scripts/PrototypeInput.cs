using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using Touch=UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace FieldOps {
 [DefaultExecutionOrder(-100)]
 public sealed class PrototypeInput : MonoBehaviour {
  public PrototypeHud hud;
  public TrainingSession session;
  bool requireMouseRelease;
  public Vector2 Move {get;private set;}
  public Vector2 Look {get;private set;}
  public bool Fire {get;private set;}
  public bool Sprint {get;private set;}
  public bool Reload {get;private set;}
  public bool AimHeld {get;private set;}
  public bool AimPressed {get;private set;}
  public bool JumpPressed {get;private set;}
  public bool JumpHeld {get;private set;}
  public bool SlidePressed {get;private set;}
  public float LookScale {get;set;}=1f;
  enum Role {Ignore,Move,Look,Fire,Sprint,Aim,Jump}
  readonly Dictionary<int,Role> roles=new Dictionary<int,Role>();
  int moveFinger=-1,lookFinger=-1;
  readonly SprintRule stickSprint=new SprintRule();
  void OnEnable() { EnhancedTouchSupport.Enable(); }
  void OnDisable() { EnhancedTouchSupport.Disable();Clear();Cursor.lockState=CursorLockMode.None;Cursor.visible=true; }
  void OnApplicationFocus(bool focus) { if(!focus)Clear(); }
  public void ClearGameplay() {Clear();requireMouseRelease=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
  void Clear() { stickSprint.Reset();roles.Clear();moveFinger=lookFinger=-1;Move=Look=Vector2.zero;Fire=Sprint=JumpPressed=JumpHeld=SlidePressed=false;if(hud!=null)hud.ResetStick(); }
  // While the layout editor is open, touches and the mouse drag the HUD buttons instead of playing.
  void ForwardLayoutPointers() {
   foreach(var touch in Touch.activeTouches)hud.LayoutPointer(touch.finger.index,touch.phase==UnityEngine.InputSystem.TouchPhase.Began,touch.phase==UnityEngine.InputSystem.TouchPhase.Ended||touch.phase==UnityEngine.InputSystem.TouchPhase.Canceled,touch.screenPosition);
   var mouse=Mouse.current;if(!Application.isMobilePlatform&&mouse!=null)hud.LayoutPointer(-1,mouse.leftButton.wasPressedThisFrame,mouse.leftButton.wasReleasedThisFrame,mouse.position.ReadValue());
  }
  void Update() {
   Move=Look=Vector2.zero;Fire=Sprint=Reload=AimHeld=AimPressed=JumpPressed=JumpHeld=SlidePressed=false;
   if(!session.GameplayEnabled) {Clear();Cursor.lockState=CursorLockMode.None;Cursor.visible=true;hud.ShowButtons(false,false);if(hud.EditingLayout)ForwardLayoutPointers();return;}
   foreach(var touch in Touch.activeTouches) {
    int id=touch.finger.index;
    if(touch.phase==UnityEngine.InputSystem.TouchPhase.Began) {
     Role role=Role.Ignore;var point=touch.screenPosition;
     if(hud.IsMenu(point))role=Role.Ignore;
     else if(hud.Contains(hud.reload,point)){role=Role.Ignore;Reload=true;}
     else if(hud.Contains(hud.slide,point)){role=Role.Ignore;SlidePressed=true;}
     else if(hud.Contains(hud.jump,point)){role=Role.Jump;JumpPressed=true;}
     else if(hud.Contains(hud.aim,point)){role=Role.Aim;AimPressed=true;}
     else if(hud.Contains(hud.fire,point))role=Role.Fire;
     else if(hud.Contains(hud.sprint,point))role=Role.Sprint;
     else if(hud.Contains(hud.joystick,point)&&moveFinger<0) {role=Role.Move;moveFinger=id;}
     else if(point.x>Screen.width*.42f&&lookFinger<0) {role=Role.Look;lookFinger=id;}
     roles[id]=role;
    }
    if(!roles.TryGetValue(id,out var active))continue;
    if(touch.phase==UnityEngine.InputSystem.TouchPhase.Ended||touch.phase==UnityEngine.InputSystem.TouchPhase.Canceled) { roles.Remove(id);if(moveFinger==id){moveFinger=-1;hud.ResetStick();}if(lookFinger==id)lookFinger=-1;continue; }
    switch(active) {case Role.Move:Move=hud.JoystickValue(touch.screenPosition);break;case Role.Look:Look+=touch.delta/Mathf.Max(1,Screen.height)*105f;break;case Role.Fire:Fire=true;break;case Role.Sprint:Sprint=true;break;case Role.Aim:AimHeld=true;break;case Role.Jump:JumpHeld=true;break;}
   }
   // In joystick mode the sprint comes from pushing the stick to its edge; the sprint button is hidden then.
   stickSprint.Level=hud.SprintLevel;
   if(hud.SprintByJoystick)Sprint=stickSprint.Update(Move);else stickSprint.Reset();
   if(!Application.isMobilePlatform) {
    var kb=Keyboard.current;var mouse=Mouse.current;
    if(mouse==null||!mouse.leftButton.isPressed)requireMouseRelease=false;
    if(kb!=null) { if(kb.escapeKey.wasPressedThisFrame){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;} Reload|=kb.rKey.wasPressedThisFrame;Move+=new Vector2((kb.dKey.isPressed?1:0)-(kb.aKey.isPressed?1:0),(kb.wKey.isPressed?1:0)-(kb.sKey.isPressed?1:0));Sprint|=kb.leftShiftKey.isPressed;JumpPressed|=kb.spaceKey.wasPressedThisFrame;JumpHeld|=kb.spaceKey.isPressed;SlidePressed|=kb.leftCtrlKey.wasPressedThisFrame||kb.cKey.wasPressedThisFrame; }
    if(mouse!=null) { if(Cursor.lockState==CursorLockMode.Locked) {Look+=mouse.delta.ReadValue()*.1f;Fire|=mouse.leftButton.isPressed&&!requireMouseRelease;AimHeld|=mouse.rightButton.isPressed;AimPressed|=mouse.rightButton.wasPressedThisFrame;}else if(mouse.leftButton.wasPressedThisFrame&&!hud.IsMenu(mouse.position.ReadValue())&&(EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject())) {Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;} }
   }
   Look*=LookScale;Move=Vector2.ClampMagnitude(Move,1);hud.ShowButtons(Fire,Sprint);
  }
 }
}
