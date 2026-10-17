using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public sealed class SprintRuleTests {
  [SetUp] public void Clean() { PlayerPrefs.DeleteKey("sprintMode"); }
  [TearDown] public void CleanAfter() { PlayerPrefs.DeleteKey("sprintMode"); }
  [Test] public void ThresholdsKeepAGapSoTheThumbCanWobble() { Assert.Less(SprintRule.Release,SprintRule.Engage);Assert.Greater(SprintRule.Release,.5f);Assert.LessOrEqual(SprintRule.Engage,1f); }
  [Test] public void StartsNotSprinting() { Assert.IsFalse(new SprintRule().Engaged); }
  [Test] public void EngagesAtTheEdgeOfTheStick() { var r=new SprintRule();Assert.IsFalse(r.Update(.5f));Assert.IsFalse(r.Update(.91f));Assert.IsTrue(r.Update(.92f));Assert.IsTrue(r.Engaged);Assert.IsTrue(r.Update(1f)); }
  [Test] public void StaysEngagedBetweenTheTwoThresholds() { var r=new SprintRule();r.Update(1f);Assert.IsTrue(r.Update(.85f));Assert.IsTrue(r.Update(.81f)); }
  [Test] public void ReleasesBelowTheLowerThresholdAndNeedsTheEdgeAgain() { var r=new SprintRule();r.Update(1f);Assert.IsFalse(r.Update(.79f));Assert.IsFalse(r.Update(.85f),"A partial push does not re-engage.");Assert.IsTrue(r.Update(.95f)); }
  [Test] public void LettingGoStopsTheSprint() { var r=new SprintRule();r.Update(1f);Assert.IsFalse(r.Update(0f)); }
  [Test] public void ResetClearsTheState() { var r=new SprintRule();r.Update(1f);r.Reset();Assert.IsFalse(r.Engaged);Assert.IsFalse(r.Update(.85f)); }
  [Test] public void InvalidMagnitudesNeverSprint() { var r=new SprintRule();Assert.IsFalse(r.Update(float.NaN));Assert.IsFalse(r.Update(-1f));Assert.IsFalse(r.Update(float.PositiveInfinity)); }
  [Test] public void JoystickSprintIsTheDefault() { Assert.IsFalse(PlayerPrefs.HasKey("sprintMode"));Assert.IsTrue(GameSettings.SprintByJoystick); }
  [Test] public void TheChoiceIsStoredLocally() { GameSettings.SprintByJoystick=false;Assert.IsFalse(GameSettings.SprintByJoystick);Assert.AreEqual(0,PlayerPrefs.GetInt("sprintMode",-1));GameSettings.SprintByJoystick=true;Assert.IsTrue(GameSettings.SprintByJoystick);Assert.AreEqual(1,PlayerPrefs.GetInt("sprintMode",-1)); }
 }
}
