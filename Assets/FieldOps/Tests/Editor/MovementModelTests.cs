using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public sealed class MovementModelTests {
  const float Dt=1f/60f;
  MovementTuning tuning;MovementModel m;float y;
  static readonly Vector3 Forward=Vector3.forward;
  [SetUp] public void Setup() { tuning=new MovementTuning();m=new MovementModel(tuning);y=0; }
  MoveInput Run(bool jump=false,bool held=false,bool slide=false,bool sprint=true,bool grounded=true,bool canStand=true,Vector3? wish=null,bool crouch=false) { return new MoveInput{Wish=wish??Forward,Sprint=sprint,JumpPressed=jump,JumpHeld=held,SlidePressed=slide,CrouchPressed=crouch,Grounded=grounded,CanStand=canStand}; }
  void Ground(int frames,bool sprint=true) { for(int i=0;i<frames;i++)m.Step(Run(sprint:sprint),Dt); }
  // Integrates height with the model's vertical velocity and reports ground contact like the motor does.
  void Fly(float seconds,bool jumpHeld=false) { int n=Mathf.RoundToInt(seconds/Dt);for(int i=0;i<n;i++) {bool grounded=y<=0&&m.VerticalVelocity<=0;m.Step(Run(held:jumpHeld,grounded:grounded),Dt);y+=m.VerticalVelocity*Dt;if(y<0)y=0;} }
  // Runs until the model lands, then returns.
  void FlyUntilLanded() { for(int i=0;i<600;i++) {bool grounded=y<=0&&m.VerticalVelocity<=0;m.Step(Run(grounded:grounded),Dt);y+=m.VerticalVelocity*Dt;if(y<0)y=0;if(m.JustLanded)return;} Assert.Fail("Never landed"); }

  [Test] public void DefaultTuningIsConsistent() { Assert.Greater(tuning.sprintSpeed,tuning.walkSpeed);Assert.Greater(tuning.maxHopSpeed,tuning.sprintSpeed);Assert.Greater(tuning.slideBoost,1f);Assert.Greater(tuning.jumpHeight,.3f);Assert.Greater(tuning.slideMaxSeconds,.3f); }

  [Test] public void StandingStillGroundedStaysGroundedWithNoVelocity() { m.Step(Run(sprint:false,wish:Vector3.zero),Dt);Assert.AreEqual(MoveState.Grounded,m.State);Assert.AreEqual(0f,m.Speed,.001f);Assert.LessOrEqual(m.VerticalVelocity,0f); }
  [Test] public void GroundSpeedReachesWalkAndSprintTargets() { Ground(30,false);Assert.AreEqual(tuning.walkSpeed,m.Speed,.01f);Ground(30,true);Assert.AreEqual(tuning.sprintSpeed,m.Speed,.01f); }

  [Test] public void JumpSetsTheVerticalSpeedForTheConfiguredApex() { Ground(30);m.Step(Run(jump:true),Dt);Assert.IsTrue(m.JustJumped);Assert.AreEqual(MoveState.Airborne,m.State);Assert.AreEqual(Mathf.Sqrt(2*tuning.gravity*tuning.jumpHeight),m.VerticalVelocity,.15f); }
  [Test] public void JumpApexIsAboutTheConfiguredHeight() { Ground(30);m.Step(Run(jump:true),Dt);float max=0;for(int i=0;i<200;i++) {y+=m.VerticalVelocity*Dt;max=Mathf.Max(max,y);m.Step(Run(grounded:false),Dt);} Assert.AreEqual(tuning.jumpHeight,max,.08f); }
  [Test] public void JumpPressedJustBeforeLandingIsBufferedAndJumpsOnLanding() {
   Ground(30);m.Step(Run(jump:true),Dt);
   for(int i=0;i<600&&!(y<=.05f&&m.VerticalVelocity<0);i++) {y+=m.VerticalVelocity*Dt;m.Step(Run(grounded:false),Dt);}
   m.Step(Run(jump:true,grounded:false),Dt);   // pressed a hair before touching down
   bool jumped=false;for(int i=0;i<30&&!jumped;i++) {y+=m.VerticalVelocity*Dt;bool grounded=y<=0&&m.VerticalVelocity<=0;if(y<0)y=0;m.Step(Run(grounded:grounded),Dt);jumped|=m.JustJumped;}
   Assert.IsTrue(jumped,"The buffered press jumps as soon as the player lands.");
  }
  [Test] public void JumpPressedLongBeforeLandingIsNotBuffered() {
   Ground(30);m.Step(Run(jump:true),Dt);for(int i=0;i<5;i++)m.Step(Run(grounded:false),Dt);
   m.Step(Run(jump:true,grounded:false),Dt);   // far too early
   bool jumped=false;for(int i=0;i<200;i++) {y+=m.VerticalVelocity*Dt;bool grounded=y<=0&&m.VerticalVelocity<=0;if(y<0)y=0;m.Step(Run(grounded:grounded),Dt);if(grounded&&m.JustJumped)jumped=true;}
   Assert.IsFalse(jumped);
  }
  [Test] public void WalkingOffALedgeStillAllowsAJumpForACoyoteMoment() { Ground(30);for(int i=0;i<4;i++)m.Step(Run(grounded:false),Dt);m.Step(Run(jump:true,grounded:false),Dt);Assert.IsTrue(m.JustJumped); }
  [Test] public void CoyoteTimeExpires() { Ground(30);for(int i=0;i<20;i++)m.Step(Run(grounded:false),Dt);m.Step(Run(jump:true,grounded:false),Dt);Assert.IsFalse(m.JustJumped); }
  [Test] public void ThereIsNoDoubleJump() { Ground(30);m.Step(Run(jump:true),Dt);for(int i=0;i<10;i++)m.Step(Run(grounded:false),Dt);float v=m.VerticalVelocity;m.Step(Run(jump:true,grounded:false),Dt);Assert.IsFalse(m.JustJumped);Assert.Less(m.VerticalVelocity,v); }
  [Test] public void HorizontalSpeedIsKeptInTheAir() { Ground(30);m.Step(Run(jump:true),Dt);for(int i=0;i<30;i++)m.Step(Run(grounded:false,wish:Vector3.zero),Dt);Assert.AreEqual(tuning.sprintSpeed,m.Speed,.05f); }
  [Test] public void AirSteeringBendsTheDirectionWithoutAddingSpeed() { Ground(30);m.Step(Run(jump:true),Dt);float before=m.Speed;for(int i=0;i<20;i++)m.Step(Run(grounded:false,wish:Vector3.right),Dt);Assert.Greater(m.HorizontalVelocity.x,.2f);Assert.LessOrEqual(m.Speed,before+.01f); }

  [Test] public void ChainedHopsGainSpeedUpToTheCap() {
   Ground(30);m.Step(Run(jump:true),Dt);Assert.AreEqual(0,m.HopCount);float last=m.Speed;
   for(int hop=1;hop<=6;hop++) {FlyUntilLanded();m.Step(Run(jump:true),Dt);Assert.IsTrue(m.JustJumped,"hop "+hop);Assert.AreEqual(hop,m.HopCount);Assert.GreaterOrEqual(m.Speed,last-.001f);Assert.LessOrEqual(m.Speed,tuning.maxHopSpeed+.001f);last=m.Speed;}
   Assert.AreEqual(tuning.maxHopSpeed,m.Speed,.001f,"The chain stops gaining at the cap.");
  }
  [Test] public void FirstChainedHopAddsExactlyTheConfiguredGain() { Ground(30);m.Step(Run(jump:true),Dt);float s=m.Speed;FlyUntilLanded();m.Step(Run(jump:true),Dt);Assert.AreEqual(s+tuning.hopGain,m.Speed,.01f); }
  [Test] public void JumpingAfterTheHopWindowLosesTheChain() {
   Ground(30);m.Step(Run(jump:true),Dt);FlyUntilLanded();m.Step(Run(jump:true),Dt);FlyUntilLanded();
   for(int i=0;i<30;i++)m.Step(Run(),Dt);   // wait well past the window with sprint held
   Assert.AreEqual(tuning.sprintSpeed,m.Speed,.05f,"Friction brings the speed back to sprint.");
   m.Step(Run(jump:true),Dt);Assert.IsTrue(m.JustJumped);Assert.AreEqual(0,m.HopCount);Assert.AreEqual(tuning.sprintSpeed,m.Speed,.05f);
  }
  [Test] public void LandingWithoutAJumpInsideTheWindowDropsToGroundSpeed() { Ground(30);m.Step(Run(jump:true),Dt);FlyUntilLanded();m.Step(Run(jump:true),Dt);FlyUntilLanded();for(int i=0;i<40;i++)m.Step(Run(sprint:false),Dt);Assert.AreEqual(tuning.walkSpeed,m.Speed,.05f); }

  [Test] public void SlideOnlyStartsWhenFastEnoughAndGrounded() {
   Ground(30,false);m.Step(Run(slide:true,sprint:false),Dt);Assert.AreNotEqual(MoveState.Sliding,m.State,"Walking pace is too slow.");
   m.Reset();Ground(30);m.Step(Run(slide:true,grounded:false),Dt);Assert.AreNotEqual(MoveState.Sliding,m.State,"No slide in the air.");
   m.Reset();Ground(30);m.Step(Run(slide:true),Dt);Assert.AreEqual(MoveState.Sliding,m.State);Assert.IsTrue(m.Crouched);
  }
  [Test] public void SlideStartsWithABoostAndKeepsTheDirection() { Ground(30);m.Step(Run(slide:true),Dt);Assert.AreEqual(tuning.sprintSpeed*tuning.slideBoost,m.Speed,.05f);Assert.Greater(Vector3.Dot(m.HorizontalVelocity.normalized,Forward),.99f); }
  [Test] public void SlideDecaysEveryFrameAndEndsWithinItsMaximumTime() {
   Ground(30);m.Step(Run(slide:true),Dt);Assert.AreEqual(MoveState.Sliding,m.State,"The slide really started.");float last=m.Speed;float t=0;int frames=0;
   while(m.State==MoveState.Sliding&&t<3f) {m.Step(Run(),Dt);t+=Dt;if(m.State==MoveState.Sliding) {frames++;Assert.LessOrEqual(m.Speed,last+.001f);}last=m.Speed;}
   Assert.Greater(frames,20,"It lasts a noticeable time.");Assert.AreEqual(MoveState.Grounded,m.State);Assert.LessOrEqual(t,tuning.slideMaxSeconds+.05f);
  }
  [Test] public void SlideHasACooldown() {
   Ground(30);m.Step(Run(slide:true),Dt);while(m.State==MoveState.Sliding)m.Step(Run(),Dt);
   Ground(20);m.Step(Run(slide:true),Dt);Assert.AreNotEqual(MoveState.Sliding,m.State,"Still cooling down.");
   for(int i=0;i<60;i++)m.Step(Run(),Dt);m.Step(Run(slide:true),Dt);Assert.AreEqual(MoveState.Sliding,m.State,"Available again after the cooldown.");
  }
  [Test] public void JumpingDuringASlideCancelsItAndKeepsTheSlideSpeed() {
   Ground(30);m.Step(Run(slide:true),Dt);for(int i=0;i<10;i++)m.Step(Run(),Dt);float slideSpeed=m.Speed;
   m.Step(Run(jump:true),Dt);
   Assert.IsTrue(m.JustJumped);Assert.AreEqual(MoveState.Airborne,m.State);Assert.IsFalse(m.Crouched);Assert.AreEqual(slideSpeed,m.Speed,.1f);Assert.AreEqual(1,m.HopCount,"The slide-cancel counts as the first hop.");
  }
  [Test] public void SlideCancelCanChainIntoHopGain() {
   Ground(30);m.Step(Run(slide:true),Dt);for(int i=0;i<10;i++)m.Step(Run(),Dt);m.Step(Run(jump:true),Dt);float s=m.Speed;
   FlyUntilLanded();m.Step(Run(jump:true),Dt);Assert.IsTrue(m.JustJumped);Assert.AreEqual(2,m.HopCount);Assert.Greater(m.Speed,s);
  }
  [Test] public void UnderALowCeilingTheSlideDoesNotStandUp() {
   Ground(30);m.Step(Run(slide:true),Dt);for(int i=0;i<90;i++)m.Step(Run(canStand:false),Dt);
   Assert.AreEqual(MoveState.Sliding,m.State);Assert.IsTrue(m.Crouched);Assert.LessOrEqual(m.Speed,tuning.slideEndSpeed+.05f);
   m.Step(Run(canStand:true),Dt);Assert.AreEqual(MoveState.Grounded,m.State);Assert.IsFalse(m.Crouched);
  }
  [Test] public void CannotJumpOutOfASlideUnderALowCeiling() { Ground(30);m.Step(Run(slide:true),Dt);m.Step(Run(jump:true,canStand:false),Dt);Assert.IsFalse(m.JustJumped);Assert.AreEqual(MoveState.Sliding,m.State); }
  [Test] public void SlidingOffAnEdgeBecomesAirborneAndKeepsTheSpeed() { Ground(30);m.Step(Run(slide:true),Dt);m.Step(Run(grounded:false),Dt);Assert.AreEqual(MoveState.Airborne,m.State);Assert.Greater(m.Speed,tuning.sprintSpeed); }
  [Test] public void HittingACeilingStopsTheRiseButNotTheFall() { Ground(30);m.Step(Run(jump:true),Dt);Assert.Greater(m.VerticalVelocity,0f);m.CancelRise();Assert.AreEqual(0f,m.VerticalVelocity);m.Step(Run(grounded:false),Dt);Assert.Less(m.VerticalVelocity,0f);float falling=m.VerticalVelocity;m.CancelRise();Assert.AreEqual(falling,m.VerticalVelocity,"Only an upward speed is cancelled."); }
  [Test] public void BeingBlockedByAWallReplacesTheHorizontalVelocity() { Ground(30);m.SetHorizontalVelocity(new Vector3(0,0,1.5f));Assert.AreEqual(1.5f,m.Speed,.001f);m.SetHorizontalVelocity(new Vector3(0,9,2f));Assert.AreEqual(0f,m.HorizontalVelocity.y,"Only the horizontal part is kept.");Assert.AreEqual(2f,m.Speed,.001f); }
  [Test] public void CrouchSpeedIsSlowerThanWalking() { Assert.Greater(tuning.crouchSpeed,0f);Assert.Less(tuning.crouchSpeed,tuning.walkSpeed); }
  [Test] public void CrouchTogglesOnAndOffOnTheGround() {
   Ground(30,false);Assert.IsFalse(m.Crouching);
   m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching);Assert.IsTrue(m.Crouched,"The body is lowered while crouching.");Assert.AreEqual(MoveState.Grounded,m.State);
   m.Step(Run(sprint:false),Dt);Assert.IsTrue(m.Crouching,"It stays crouched until the next press.");
   m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsFalse(m.Crouching);Assert.IsFalse(m.Crouched);
  }
  [Test] public void CrouchedWalkingRunsAtTheCrouchSpeed() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);for(int i=0;i<60;i++)m.Step(Run(sprint:false),Dt);Assert.AreEqual(tuning.crouchSpeed,m.Speed,.01f); }
  [Test] public void SprintIsIgnoredWhileCrouchedAndSlowsTheRunDown() { Ground(30);m.Step(Run(crouch:true),Dt);for(int i=0;i<60;i++)m.Step(Run(),Dt);Assert.IsTrue(m.Crouching);Assert.AreEqual(tuning.crouchSpeed,m.Speed,.01f,"The held sprint does not stand the player up or speed it up."); }
  [Test] public void StandingUpRestoresTheNormalSpeeds() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching);m.Step(Run(sprint:false,crouch:true),Dt);Ground(40,false);Assert.AreEqual(tuning.walkSpeed,m.Speed,.01f);Ground(40);Assert.AreEqual(tuning.sprintSpeed,m.Speed,.01f); }
  // Negative tests first prove that the crouch works on the ground, so they cannot pass just because the feature is missing.
  void ProveCrouchWorks() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching,"The crouch works on the ground.");m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsFalse(m.Crouching); }
  [Test] public void CannotCrouchInTheAir() { ProveCrouchWorks();Ground(30);m.Step(Run(jump:true),Dt);m.Step(Run(grounded:false,crouch:true),Dt);Assert.IsFalse(m.Crouching);Assert.AreEqual(MoveState.Airborne,m.State); }
  [Test] public void CannotCrouchDuringASlide() { ProveCrouchWorks();Ground(30);m.Step(Run(slide:true),Dt);Assert.AreEqual(MoveState.Sliding,m.State);m.Step(Run(crouch:true),Dt);Assert.IsFalse(m.Crouching);Assert.AreEqual(MoveState.Sliding,m.State,"The slide keeps going."); }
  [Test] public void CannotSlideWhileCrouched() { Ground(30);m.Step(Run(crouch:true),Dt);Assert.IsTrue(m.Crouching);m.Step(Run(slide:true),Dt);Assert.AreNotEqual(MoveState.Sliding,m.State);Assert.IsTrue(m.Crouching); }
  [Test] public void StandingUpNeedsRoomAboveTheHead() {
   Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);
   m.Step(Run(sprint:false,crouch:true,canStand:false),Dt);Assert.IsTrue(m.Crouching,"No room: it stays crouched.");
   m.Step(Run(sprint:false,crouch:true,canStand:true),Dt);Assert.IsFalse(m.Crouching,"A new press stands up once there is room.");
  }
  [Test] public void JumpingFromACrouchStandsUpAndJumps() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching);m.Step(Run(sprint:false,jump:true),Dt);Assert.IsTrue(m.JustJumped);Assert.IsFalse(m.Crouching);Assert.AreEqual(MoveState.Airborne,m.State); }
  [Test] public void CannotJumpFromACrouchUnderALowCeiling() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);m.Step(Run(sprint:false,jump:true,canStand:false),Dt);Assert.IsFalse(m.JustJumped);Assert.IsTrue(m.Crouching);Assert.AreEqual(MoveState.Grounded,m.State); }
  [Test] public void WalkingOffALedgeStandsTheCrouchedPlayerUp() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching);m.Step(Run(sprint:false,grounded:false),Dt);Assert.IsFalse(m.Crouching);Assert.AreEqual(MoveState.Airborne,m.State); }
  [Test] public void ResetClearsTheCrouch() { Ground(30,false);m.Step(Run(sprint:false,crouch:true),Dt);Assert.IsTrue(m.Crouching);m.Reset();Assert.IsFalse(m.Crouching);Assert.IsFalse(m.Crouched); }
  [Test] public void ResetClearsEverything() { Ground(30);m.Step(Run(jump:true),Dt);m.Reset();Assert.AreEqual(MoveState.Grounded,m.State);Assert.AreEqual(0f,m.Speed);Assert.AreEqual(0f,m.VerticalVelocity);Assert.AreEqual(0,m.HopCount);Assert.IsFalse(m.Crouched); }
 }
}
