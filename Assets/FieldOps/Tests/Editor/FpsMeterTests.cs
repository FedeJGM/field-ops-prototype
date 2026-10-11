using System;
using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public class FpsMeterTests {
  static int Feed(FpsMeter m,float dt,int frames) { int updates=0;for(int i=0;i<frames;i++)if(m.Add(dt))updates++;return updates; }
  [Test] public void StartsAtZeroUntilTheFirstWindowCompletes() { var m=new FpsMeter();Assert.AreEqual(0,m.Fps);Assert.IsFalse(m.Add(.016f));Assert.AreEqual(0,m.Fps); }
  [Test] public void ConstantSixtyFpsReadsSixtyAfterHalfASecond() { var m=new FpsMeter();int updates=Feed(m,1f/60f,31);Assert.AreEqual(1,updates);Assert.AreEqual(60f,m.Fps,.5f); }
  [Test] public void ConstantThirtyFpsReadsThirty() { var m=new FpsMeter();Feed(m,1f/30f,16);Assert.AreEqual(30f,m.Fps,.5f); }
  [Test] public void TheValueRefreshesEveryWindowAndFollowsTheLatestOne() { var m=new FpsMeter();int updates=Feed(m,1f/60f,31);Assert.AreEqual(1,updates);Assert.AreEqual(60f,m.Fps,.5f);updates=Feed(m,1f/30f,16);Assert.AreEqual(1,updates);Assert.Less(m.Fps,40f,"The reading follows the latest window.");Assert.Greater(m.Fps,28f); }
  [Test] public void ASlowFrameLowersTheReadingOfItsWindow() { var m=new FpsMeter();Feed(m,.016f,20);m.Add(.2f);Feed(m,.016f,10);Assert.Less(m.Fps,55f);Assert.Greater(m.Fps,30f); }
  [Test] public void InvalidFrameTimesAreIgnored() { var m=new FpsMeter();Assert.IsFalse(m.Add(0));Assert.IsFalse(m.Add(-1));Assert.IsFalse(m.Add(float.NaN));Assert.IsFalse(m.Add(float.PositiveInfinity));Feed(m,1f/60f,31);Assert.AreEqual(60f,m.Fps,.5f); }
  [Test] public void ARejectedIntervalIsAnError() { Assert.Throws<ArgumentOutOfRangeException>(()=>new FpsMeter(0));Assert.Throws<ArgumentOutOfRangeException>(()=>new FpsMeter(-1));Assert.Throws<ArgumentOutOfRangeException>(()=>new FpsMeter(float.NaN)); }
  [TestCase(60f,FpsBand.Good)][TestCase(50f,FpsBand.Good)][TestCase(49.9f,FpsBand.Fair)][TestCase(30f,FpsBand.Fair)][TestCase(29.9f,FpsBand.Poor)][TestCase(0f,FpsBand.Poor)]
  public void BandsFollowTheThresholds(float fps,FpsBand band) { Assert.AreEqual(band,FpsMeter.Band(fps)); }
 }
 public class FpsSettingTests {
  [SetUp] public void Clean() {PlayerPrefs.DeleteKey("showFps");}
  [TearDown] public void CleanAfter() {PlayerPrefs.DeleteKey("showFps");}
  [Test] public void TheCounterIsShownByDefault() { Assert.IsTrue(GameSettings.ShowFps); }
  [Test] public void TheChoiceIsStoredLocally() { GameSettings.ShowFps=false;Assert.AreEqual(0,PlayerPrefs.GetInt("showFps",-1));Assert.IsFalse(GameSettings.ShowFps);GameSettings.ShowFps=true;Assert.IsTrue(GameSettings.ShowFps); }
 }
}
