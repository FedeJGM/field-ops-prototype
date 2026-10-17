using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public class AimControlTests {
  [Test] public void StartsNotAimingAtNormalFovAndFullSensitivity() { var a=new AimControl();Assert.IsFalse(a.Aiming);Assert.IsFalse(a.Toggle);Assert.AreEqual(0,a.Blend);Assert.AreEqual(68,a.Fov(68),.001f);Assert.AreEqual(1,a.LookScale,.001f); }
  [Test] public void HoldModeAimsOnlyWhileTheButtonIsHeld() { var a=new AimControl();a.Step(true,true,false,true,.016);Assert.IsTrue(a.Aiming);a.Step(true,false,false,true,.016);Assert.IsTrue(a.Aiming);a.Step(false,false,false,true,.016);Assert.IsFalse(a.Aiming); }
  [Test] public void ToggleModeFlipsOnEachPressAndIgnoresHolding() { var a=new AimControl{Toggle=true};a.Step(true,true,false,true,.016);Assert.IsTrue(a.Aiming);a.Step(false,false,false,true,.016);Assert.IsTrue(a.Aiming);a.Step(true,false,false,true,.016);Assert.IsTrue(a.Aiming,"Holding without a new press must not flip it.");a.Step(true,true,false,true,.016);Assert.IsFalse(a.Aiming); }
  [Test] public void SprintingCancelsAimingInBothModesAndBlocksNewAiming() { foreach(var toggle in new[]{false,true}) {var a=new AimControl{Toggle=toggle};a.Step(true,true,false,true,.016);Assert.IsTrue(a.Aiming);a.Step(true,false,true,true,.016);Assert.IsFalse(a.Aiming,"toggle="+toggle);a.Step(true,true,true,true,.016);Assert.IsFalse(a.Aiming);} }
  [Test] public void ToggleStaysOffAfterSprintEndsUntilPressedAgain() { var a=new AimControl{Toggle=true};a.Step(false,true,false,true,.016);a.Step(false,false,true,true,.016);a.Step(false,false,false,true,.016);Assert.IsFalse(a.Aiming); }
  [Test] public void NotAllowedCancelsAimingAndIgnoresPresses() { foreach(var toggle in new[]{false,true}) {var a=new AimControl{Toggle=toggle};a.Step(true,true,false,true,.016);a.Step(true,true,false,false,.016);Assert.IsFalse(a.Aiming,"toggle="+toggle);} }
  [Test] public void BlendMovesSmoothlyToFullZoomAndBack() { var a=new AimControl();a.Step(true,true,false,true,AimControl.BlendSeconds/2);Assert.AreEqual(.5f,a.Blend,.01f);Assert.AreEqual(54,a.Fov(68),.5f);a.Step(true,false,false,true,10);Assert.AreEqual(1,a.Blend,.001f);Assert.AreEqual(AimControl.ZoomedFov,a.Fov(68),.001f);a.Step(false,false,false,true,AimControl.BlendSeconds/2);Assert.AreEqual(.5f,a.Blend,.01f);a.Step(false,false,false,true,10);Assert.AreEqual(0,a.Blend,.001f); }
  [Test] public void LookSensitivityDropsToHalfWhenFullyAiming() { var a=new AimControl();a.Step(true,true,false,true,10);Assert.AreEqual(AimControl.SensitivityWhileAiming,a.LookScale,.001f);Assert.AreEqual(.5f,a.LookScale,.001f); }
  [Test] public void InvalidElapsedValuesDoNotMoveTheBlend() { var a=new AimControl();a.Step(true,true,false,true,-1);a.Step(true,false,false,true,double.NaN);a.Step(true,false,false,true,double.PositiveInfinity);Assert.AreEqual(0,a.Blend);Assert.IsTrue(a.Aiming); }
  [Test] public void ZoomFovIsConfigurablePerWeapon() { var a=new AimControl();Assert.AreEqual(AimControl.ZoomedFov,a.ZoomFov,.001f);a.ZoomFov=20f;a.Step(true,true,false,true,10);Assert.AreEqual(20f,a.Fov(68),.001f);a.Step(false,false,false,true,10);Assert.AreEqual(68f,a.Fov(68),.001f); }
  [Test] public void ResetStopsAimingAndSnapsBackToNormalView() { var a=new AimControl();a.Step(true,true,false,true,10);a.Reset();Assert.IsFalse(a.Aiming);Assert.AreEqual(0,a.Blend);Assert.AreEqual(68,a.Fov(68),.001f);Assert.AreEqual(1,a.LookScale,.001f); }
 }
 public class GameSettingsTests {
  [SetUp] public void Clean() {PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");}
  [TearDown] public void CleanAfter() {PlayerPrefs.DeleteKey("aimToggle");PlayerPrefs.DeleteKey("keepCross");}
  [Test] public void DefaultsAreHoldModeAndNoCrossWhileAiming() { Assert.IsFalse(GameSettings.AimToggle);Assert.IsFalse(GameSettings.KeepCrossWhileAiming); }
  [Test] public void ChoicesPersistInPlayerPrefs() { GameSettings.AimToggle=true;GameSettings.KeepCrossWhileAiming=true;Assert.AreEqual(1,PlayerPrefs.GetInt("aimToggle",0));Assert.AreEqual(1,PlayerPrefs.GetInt("keepCross",0));Assert.IsTrue(GameSettings.AimToggle);Assert.IsTrue(GameSettings.KeepCrossWhileAiming);GameSettings.AimToggle=false;Assert.IsFalse(GameSettings.AimToggle);Assert.IsTrue(GameSettings.KeepCrossWhileAiming); }
  [Test] public void SuppressingHidesTheAimButHoldModeComesBackByItself() { var a=new AimControl();for(int i=0;i<20;i++)a.Step(true,false,false,true,.02);Assert.IsTrue(a.Aiming);Assert.AreEqual(1f,a.Blend,.001f);
   a.Suppressed=true;for(int i=0;i<20;i++)a.Step(true,false,false,true,.02);Assert.IsFalse(a.Aiming);Assert.AreEqual(0f,a.Blend,.001f,"The zoom lets go.");
   a.Suppressed=false;a.Step(true,false,false,true,.02);Assert.IsTrue(a.Aiming,"Still holding: aiming resumes with no new press."); }
  [Test] public void SuppressingKeepsToggleModeSwitchedOn() { var a=new AimControl{Toggle=true};a.Step(false,true,false,true,.02);Assert.IsTrue(a.Aiming);a.Suppressed=true;for(int i=0;i<10;i++)a.Step(false,false,false,true,.02);Assert.IsFalse(a.Aiming);a.Suppressed=false;a.Step(false,false,false,true,.02);Assert.IsTrue(a.Aiming,"Toggle was on, so it comes back without another tap."); }
  [Test] public void SuppressionDoesNotRevealAnAimThatWasReleased() { var a=new AimControl();a.Step(true,false,false,true,.02);a.Suppressed=true;a.Step(false,false,false,true,.02);a.Suppressed=false;a.Step(false,false,false,true,.02);Assert.IsFalse(a.Aiming); }
  [Test] public void SprintingStillCancelsTheWishToAimEvenWhenSuppressed() { var a=new AimControl{Toggle=true};a.Step(false,true,false,true,.02);a.Suppressed=true;a.Step(false,false,true,true,.02);a.Suppressed=false;a.Step(false,false,false,true,.02);Assert.IsFalse(a.Aiming); }
 }
}
