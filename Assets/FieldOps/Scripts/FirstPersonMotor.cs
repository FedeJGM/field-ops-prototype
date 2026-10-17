using UnityEngine;
namespace FieldOps {
 [RequireComponent(typeof(CharacterController))]
 public sealed class FirstPersonMotor : MonoBehaviour {
  public PrototypeInput input;public Transform cameraPivot;
  public float standHeight=1.8f,slideHeight=1f,standEye=1.65f,slideEye=.9f,heightChangeSpeed=6f;
  public MovementTuning Tuning { get; }=new MovementTuning();
  public MovementModel Model { get; private set; }
  public float Height { get; private set; }
  CharacterController controller;float pitch;MoveState lastState;
  const int IgnoreRaycastLayer=1<<2;   // the player itself lives on this layer, so every physics query below skips it
  void Awake() {controller=GetComponent<CharacterController>();Application.targetFrameRate=60;Model=new MovementModel(Tuning);Height=standHeight;}
  void Update() {
   float dt=Time.deltaTime;
   transform.Rotate(0,input.Look.x,0);pitch=Mathf.Clamp(pitch-input.Look.y,-75,75);cameraPivot.localRotation=Quaternion.Euler(pitch,0,0);
   var wish=transform.right*input.Move.x+transform.forward*input.Move.y;
   Model.Step(new MoveInput{Wish=wish,Sprint=input.Sprint,JumpPressed=input.JumpPressed,JumpHeld=input.JumpHeld,SlidePressed=input.SlidePressed,Grounded=IsGrounded(),CanStand=HasRoomToStand()},dt);
   var before=transform.position;
   var flags=controller.Move((Model.HorizontalVelocity+Vector3.up*Model.VerticalVelocity)*dt);
   if((flags&CollisionFlags.Above)!=0)Model.CancelRise();
   if((flags&CollisionFlags.Sides)!=0&&dt>0) {var moved=transform.position-before;moved.y=0;Model.SetHorizontalVelocity(moved/dt);}   // pressed against a wall: do not keep building speed
   ApplyHeight(dt);LogChanges();
  }
  bool IsGrounded() {return controller.isGrounded||Physics.Raycast(transform.position+Vector3.up*.1f,Vector3.down,.25f,~IgnoreRaycastLayer,QueryTriggerInteraction.Ignore);}
  // Standing up needs the full-height capsule to be free of geometry (the player's own layer is excluded).
  bool HasRoomToStand() {
   if(Height>=standHeight-.01f)return true;
   float r=controller.radius;var bottom=transform.position+Vector3.up*r;var top=transform.position+Vector3.up*(standHeight-r);
   return !Physics.CheckCapsule(bottom,top,r*.95f,~IgnoreRaycastLayer,QueryTriggerInteraction.Ignore);
  }
  void ApplyHeight(float dt) {
   float target=Model.Crouched?slideHeight:standHeight;Height=Mathf.MoveTowards(Height,target,heightChangeSpeed*dt);
   controller.height=Height;controller.center=new Vector3(0,Height*.5f,0);
   float t=Mathf.InverseLerp(slideHeight,standHeight,Height);cameraPivot.localPosition=new Vector3(0,Mathf.Lerp(slideEye,standEye,t),0);
  }
  // Development builds print the moments that matter when tuning the feel with `adb logcat`.
  void LogChanges() {
   if(!Debug.isDebugBuild)return;
   if(Model.JustJumped)Debug.Log("FIELD_OPS_MOVE jump speed="+Model.Speed.ToString("0.00")+" hops="+Model.HopCount);
   if(Model.JustLanded)Debug.Log("FIELD_OPS_MOVE land speed="+Model.Speed.ToString("0.00"));
   if(Model.State!=lastState&&(Model.State==MoveState.Sliding||lastState==MoveState.Sliding))Debug.Log("FIELD_OPS_MOVE "+lastState+"->"+Model.State+" speed="+Model.Speed.ToString("0.00"));
   lastState=Model.State;
  }
 }
}
