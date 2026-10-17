using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public sealed class SprintRuleTests {
  [SetUp] public void Clean() { PlayerPrefs.DeleteKey("sprintMode");PlayerPrefs.DeleteKey("sprintSens"); }
  [TearDown] public void CleanAfter() { PlayerPrefs.DeleteKey("sprintMode");PlayerPrefs.DeleteKey("sprintSens"); }
  [Test] public void ThresholdsKeepAGapSoTheThumbCanWobble() { Assert.Less(SprintRule.Release,SprintRule.Engage);Assert.Greater(SprintRule.Release,.5f);Assert.LessOrEqual(SprintRule.Engage,1f); }
  [Test] public void StartsNotSprinting() { Assert.IsFalse(new SprintRule().Engaged); }
  [Test] public void EngagesAtTheEdgeOfTheStick() { var r=new SprintRule();Assert.IsFalse(r.Update(.5f));Assert.IsFalse(r.Update(.91f));Assert.IsTrue(r.Update(.92f));Assert.IsTrue(r.Engaged);Assert.IsTrue(r.Update(1f)); }
  [Test] public void StaysEngagedBetweenTheTwoThresholds() { var r=new SprintRule();r.Update(1f);Assert.IsTrue(r.Update(.85f));Assert.IsTrue(r.Update(.81f)); }
  [Test] public void ReleasesBelowTheLowerThresholdAndNeedsTheEdgeAgain() { var r=new SprintRule();r.Update(1f);Assert.IsFalse(r.Update(.79f));Assert.IsFalse(r.Update(.85f),"A partial push does not re-engage.");Assert.IsTrue(r.Update(.95f)); }
  [Test] public void LettingGoStopsTheSprint() { var r=new SprintRule();r.Update(1f);Assert.IsFalse(r.Update(0f)); }
  [Test] public void ResetClearsTheState() { var r=new SprintRule();r.Update(1f);r.Reset();Assert.IsFalse(r.Engaged);Assert.IsFalse(r.Update(.85f)); }
  [Test] public void InvalidMagnitudesNeverSprint() { var r=new SprintRule();Assert.IsFalse(r.Update(float.NaN));Assert.IsFalse(r.Update(-1f));Assert.IsFalse(r.Update(float.PositiveInfinity)); }
  static Vector2 Dir(float degreesFromForward,float length=1f) { float r=degreesFromForward*Mathf.Deg2Rad;return new Vector2(Mathf.Sin(r),Mathf.Cos(r))*length; }
  [Test] public void ConesKeepAGapForHysteresis() { Assert.AreEqual(40f,SprintRule.ForwardConeDegrees);Assert.AreEqual(50f,SprintRule.ReleaseConeDegrees);Assert.Greater(SprintRule.ReleaseConeDegrees,SprintRule.ForwardConeDegrees); }
  [Test] public void OnlyAForwardPushSprints() { Assert.IsTrue(new SprintRule().Update(Dir(0)));Assert.IsTrue(new SprintRule().Update(Dir(30)));Assert.IsTrue(new SprintRule().Update(Dir(-30)));Assert.IsTrue(new SprintRule().Update(Dir(38))); }
  [Test] public void SidewaysDiagonalAndBackwardPushesNeverSprint() {
   Assert.IsFalse(new SprintRule().Update(Dir(90)),"Right.");Assert.IsFalse(new SprintRule().Update(Dir(-90)),"Left.");Assert.IsFalse(new SprintRule().Update(Dir(45)),"Forward-right diagonal.");Assert.IsFalse(new SprintRule().Update(Dir(-45)),"Forward-left diagonal.");
   Assert.IsFalse(new SprintRule().Update(Dir(42)));Assert.IsFalse(new SprintRule().Update(Dir(135)));Assert.IsFalse(new SprintRule().Update(Dir(180)),"Backwards.");Assert.IsFalse(new SprintRule().Update(Vector2.zero));
  }
  [Test] public void DirectionHysteresisKeepsASprintThroughASmallDriftButNotALargeOne() {
   var r=new SprintRule();Assert.IsTrue(r.Update(Dir(0)));Assert.IsTrue(r.Update(Dir(45)),"Still inside the 50 degree release cone.");Assert.IsTrue(r.Update(Dir(49)));
   Assert.IsFalse(r.Update(Dir(55)),"Drifted out of the cone.");Assert.IsFalse(r.Update(Dir(45)),"A diagonal does not re-engage.");Assert.IsTrue(r.Update(Dir(20)),"Back inside the engage cone.");
  }
  [Test] public void DistanceStillMattersForTheVectorOverload() { var r=new SprintRule();Assert.IsFalse(r.Update(Dir(0,.9f)));Assert.IsTrue(r.Update(Dir(0,.95f)));Assert.IsFalse(r.Update(Dir(0,.7f))); }
  [Test] public void SensitivityStillAppliesAlongsideTheDirectionRule() { var r=new SprintRule{Level=SprintSensitivity.High};Assert.IsTrue(r.Update(Dir(10,.82f)));Assert.IsFalse(new SprintRule{Level=SprintSensitivity.High}.Update(Dir(70,1f))); }
  [Test] public void InvalidVectorsNeverSprint() { var r=new SprintRule();Assert.IsFalse(r.Update(new Vector2(float.NaN,1)));Assert.IsFalse(r.Update(new Vector2(0,float.PositiveInfinity)));Assert.IsFalse(r.Update(new Vector2(float.NegativeInfinity,float.NaN))); }
  [Test] public void SensitivityLevelsMapToEngageThresholds() { Assert.AreEqual(.80f,SprintRule.EngageFor(SprintSensitivity.High),.001f);Assert.AreEqual(.92f,SprintRule.EngageFor(SprintSensitivity.Medium),.001f);Assert.AreEqual(.97f,SprintRule.EngageFor(SprintSensitivity.Low),.001f); }
  [Test] public void ReleaseIsAlwaysBelowEngageWithAGapAndStaysUsable() { foreach(SprintSensitivity level in System.Enum.GetValues(typeof(SprintSensitivity))) {float e=SprintRule.EngageFor(level),r=SprintRule.ReleaseFor(level);Assert.AreEqual(e-.12f,r,.001f,level.ToString());Assert.GreaterOrEqual(r,.5f);Assert.Less(r,e);} }
  [Test] public void MediumKeepsTheOriginalBehaviour() { var r=new SprintRule();Assert.AreEqual(SprintSensitivity.Medium,r.Level);Assert.IsFalse(r.Update(.91f));Assert.IsTrue(r.Update(.92f));Assert.IsTrue(r.Update(.81f));Assert.IsFalse(r.Update(.79f)); }
  [Test] public void HighSensitivitySprintsWithAShortPush() { var r=new SprintRule{Level=SprintSensitivity.High};Assert.IsFalse(r.Update(.79f));Assert.IsTrue(r.Update(.80f));Assert.IsTrue(r.Update(.69f),"Release is 0.68.");Assert.IsFalse(r.Update(.67f)); }
  [Test] public void LowSensitivityNeedsAnAlmostFullPush() { var r=new SprintRule{Level=SprintSensitivity.Low};Assert.IsFalse(r.Update(.95f));Assert.IsFalse(r.Update(.96f));Assert.IsTrue(r.Update(.97f));Assert.IsTrue(r.Update(.86f),"Release is 0.85.");Assert.IsFalse(r.Update(.84f)); }
  [Test] public void ChangingTheLevelStopsAnyCurrentSprint() { var r=new SprintRule();r.Update(1f);Assert.IsTrue(r.Engaged);r.Level=SprintSensitivity.Low;Assert.IsFalse(r.Engaged); }
  [Test] public void SensitivityDefaultsToMediumAndIsStoredLocally() {
   PlayerPrefs.DeleteKey("sprintSens");Assert.AreEqual(SprintSensitivity.Medium,GameSettings.SprintSensitivity);
   GameSettings.SprintSensitivity=SprintSensitivity.High;Assert.AreEqual(SprintSensitivity.High,GameSettings.SprintSensitivity);Assert.AreEqual(0,PlayerPrefs.GetInt("sprintSens",-1));
   GameSettings.SprintSensitivity=SprintSensitivity.Low;Assert.AreEqual(SprintSensitivity.Low,GameSettings.SprintSensitivity);PlayerPrefs.DeleteKey("sprintSens");
  }
  [Test] public void ACorruptStoredSensitivityFallsBackToMedium() { PlayerPrefs.SetInt("sprintSens",42);Assert.AreEqual(SprintSensitivity.Medium,GameSettings.SprintSensitivity);PlayerPrefs.SetInt("sprintSens",-3);Assert.AreEqual(SprintSensitivity.Medium,GameSettings.SprintSensitivity);PlayerPrefs.DeleteKey("sprintSens"); }
  [Test] public void JoystickSprintIsTheDefault() { Assert.IsFalse(PlayerPrefs.HasKey("sprintMode"));Assert.IsTrue(GameSettings.SprintByJoystick); }
  [Test] public void TheChoiceIsStoredLocally() { GameSettings.SprintByJoystick=false;Assert.IsFalse(GameSettings.SprintByJoystick);Assert.AreEqual(0,PlayerPrefs.GetInt("sprintMode",-1));GameSettings.SprintByJoystick=true;Assert.IsTrue(GameSettings.SprintByJoystick);Assert.AreEqual(1,PlayerPrefs.GetInt("sprintMode",-1)); }
 }
}
