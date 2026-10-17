using UnityEngine;
namespace FieldOps {
 // Every number that shapes the movement feel, in one place so it can be tuned on the phone.
 public sealed class MovementTuning {
  public float walkSpeed=3.5f,sprintSpeed=6f,crouchSpeed=2f;
  public float jumpHeight=1f,gravity=9.81f;
  public float jumpBuffer=.12f,coyoteTime=.1f;
  public float hopWindow=.15f,hopGain=.4f,maxHopSpeed=7.5f;
  public float groundAccel=30f,groundDecel=20f,airAccel=6f;
  public float slideBoost=1.15f,slideMinSpeedFactor=.9f,slideDecel=4.5f,slideEndSpeed=2.5f,slideMaxSeconds=1f,slideCooldown=.5f,slideSteerDegreesPerSecond=45f;
 }
 public enum MoveState {Grounded,Airborne,Sliding}
 public struct MoveInput {
  public Vector3 Wish;   // desired direction in world space, magnitude 0..1
  public bool Sprint,JumpPressed,JumpHeld,SlidePressed,CrouchPressed,Grounded,CanStand;
 }
 // Pure movement rules (no Unity physics): the motor feeds it ground contact and headroom, and applies the velocity it returns.
 public sealed class MovementModel {
  public MovementModel(MovementTuning tuning) { Tuning=tuning;Reset(); }
  public MovementTuning Tuning { get; }
  public MoveState State { get; private set; }
  public Vector3 HorizontalVelocity { get; private set; }
  public float VerticalVelocity { get; private set; }
  public float Speed { get { return HorizontalVelocity.magnitude; } }
  public int HopCount { get; private set; }
  public bool JustJumped { get; private set; }
  public bool JustLanded { get; private set; }
  public bool Crouching { get; private set; }
  public bool Crouched { get { return State==MoveState.Sliding||Crouching; } }
  float landedAgo,bufferLeft,coyoteLeft,slideTime,cooldownLeft;
  // Feedback from the physics step: a ceiling stops the rise, a wall replaces the speed the model assumed.
  public void CancelRise() { if(VerticalVelocity>0)VerticalVelocity=0; }
  public void SetHorizontalVelocity(Vector3 velocity) { HorizontalVelocity=new Vector3(velocity.x,0,velocity.z); }
  public void Reset() { State=MoveState.Grounded;HorizontalVelocity=Vector3.zero;VerticalVelocity=0;HopCount=0;JustJumped=JustLanded=false;landedAgo=999f;bufferLeft=coyoteLeft=slideTime=cooldownLeft=0;Crouching=false; }

  public void Step(MoveInput input,float dt) {
   JustJumped=JustLanded=false;landedAgo+=dt;cooldownLeft=Mathf.Max(0,cooldownLeft-dt);
   if(input.JumpPressed||input.JumpHeld)bufferLeft=Tuning.jumpBuffer;else bufferLeft=Mathf.Max(0,bufferLeft-dt);
   bool onGround=input.Grounded&&VerticalVelocity<=0;   // still rising right after a jump is not "landed"
   if(onGround) {
    coyoteLeft=Tuning.coyoteTime;
    if(State==MoveState.Airborne) {State=MoveState.Grounded;JustLanded=true;landedAgo=0;}
   } else {
    coyoteLeft=Mathf.Max(0,coyoteLeft-dt);
    if(State==MoveState.Grounded)State=MoveState.Airborne;
    Crouching=false;   // the body stands in the air
   }
   var wish=Vector3.ClampMagnitude(new Vector3(input.Wish.x,0,input.Wish.z),1f);

   if(bufferLeft>0&&CanJump(onGround,input)) {Jump();return;}
   if(input.CrouchPressed&&State==MoveState.Grounded&&onGround) {
    // A press crouches; the next press stands up again, but only when there is room above.
    if(!Crouching)Crouching=true;else if(input.CanStand)Crouching=false;
   }
   if(State==MoveState.Grounded&&!Crouching&&input.SlidePressed&&cooldownLeft<=0&&Speed>=Tuning.sprintSpeed*Tuning.slideMinSpeedFactor&&onGround) {
    State=MoveState.Sliding;slideTime=0;HorizontalVelocity=HorizontalVelocity.normalized*Tuning.sprintSpeed*Tuning.slideBoost;VerticalVelocity=-2f;return;
   }
   switch(State) {
    case MoveState.Sliding:Slide(wish,input,onGround,dt);break;
    case MoveState.Airborne:Air(wish,dt);break;
    default:Walk(wish,input,dt);break;
   }
  }

  bool CanJump(bool onGround,MoveInput input) {
   if(State==MoveState.Sliding)return input.CanStand;
   if(State==MoveState.Grounded)return onGround&&(!Crouching||input.CanStand);
   return coyoteLeft>0;   // walked off an edge a moment ago
  }

  void Jump() {
   bool fromSlide=State==MoveState.Sliding;Crouching=false;
   if(fromSlide) {HopCount=1;cooldownLeft=Tuning.slideCooldown;}   // the cancel itself counts as the first hop, with no extra gain
   else if(landedAgo<=Tuning.hopWindow) {
    HopCount++;float speed=Speed;
    if(speed>.1f)HorizontalVelocity=HorizontalVelocity.normalized*Mathf.Max(speed,Mathf.Min(Tuning.maxHopSpeed,speed+Tuning.hopGain));
   } else HopCount=0;
   VerticalVelocity=Mathf.Sqrt(2f*Tuning.gravity*Tuning.jumpHeight);State=MoveState.Airborne;coyoteLeft=0;bufferLeft=0;JustJumped=true;
  }

  void Walk(Vector3 wish,MoveInput input,float dt) {
   VerticalVelocity=-2f;
   if(landedAgo>Tuning.hopWindow) {   // inside the hop window the landing keeps its momentum; after it, friction applies
    HopCount=0;var target=wish*(Crouching?Tuning.crouchSpeed:input.Sprint?Tuning.sprintSpeed:Tuning.walkSpeed);
    HorizontalVelocity=Vector3.MoveTowards(HorizontalVelocity,target,(Speed>target.magnitude?Tuning.groundDecel:Tuning.groundAccel)*dt);
   }
  }

  void Air(Vector3 wish,float dt) {
   VerticalVelocity-=Tuning.gravity*dt;
   float cap=Mathf.Max(Speed,Tuning.walkSpeed);var v=HorizontalVelocity+wish*Tuning.airAccel*dt;   // steering bends the direction but never adds speed
   HorizontalVelocity=v.magnitude>cap?v.normalized*cap:v;
  }

  void Slide(Vector3 wish,MoveInput input,bool onGround,float dt) {
   slideTime+=dt;
   if(!onGround) {State=MoveState.Airborne;cooldownLeft=Tuning.slideCooldown;Air(wish,dt);return;}
   VerticalVelocity=-2f;
   var dir=HorizontalVelocity.sqrMagnitude>1e-4f?HorizontalVelocity.normalized:wish;float speed=Mathf.Max(0,Speed-Tuning.slideDecel*dt);
   if(wish.sqrMagnitude>.01f)dir=Vector3.RotateTowards(dir,wish.normalized,Tuning.slideSteerDegreesPerSecond*Mathf.Deg2Rad*dt,0).normalized;
   bool finished=slideTime>=Tuning.slideMaxSeconds||speed<=Tuning.slideEndSpeed;
   if(finished&&input.CanStand) {State=MoveState.Grounded;cooldownLeft=Tuning.slideCooldown;HorizontalVelocity=dir*speed;landedAgo=999f;return;}
   // Under a low ceiling the player stays crouched and crawls until there is room to stand.
   HorizontalVelocity=finished?wish*Tuning.slideEndSpeed:dir*speed;
  }
 }
}
