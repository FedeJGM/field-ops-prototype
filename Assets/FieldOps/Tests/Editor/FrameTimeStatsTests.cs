using NUnit.Framework;
namespace FieldOps.Tests {
 public class FrameTimeStatsTests {
  [Test] public void EmptyStatsAreAllZero() { var s=new FrameTimeStats();Assert.AreEqual(0,s.Count);Assert.AreEqual(0,s.AverageMs);Assert.AreEqual(0,s.AverageFps);Assert.AreEqual(0,s.MaxMs);Assert.AreEqual(0,s.PercentileMs(95)); }
  [Test] public void ConstantSixtyFpsGivesSixtyFpsAndSixteenPointSevenMs() { var s=new FrameTimeStats();for(int i=0;i<60;i++)s.Add(1f/60f);Assert.AreEqual(60,s.Count);Assert.AreEqual(60f,s.AverageFps,.01f);Assert.AreEqual(16.667f,s.AverageMs,.01f);Assert.AreEqual(16.667f,s.MaxMs,.01f); }
  [Test] public void PercentilesUseTheNearestRankOfTheSortedFrameTimes() { var s=new FrameTimeStats();for(int i=100;i>=1;i--)s.Add(i/1000f);Assert.AreEqual(50f,s.PercentileMs(50),.001f);Assert.AreEqual(95f,s.PercentileMs(95),.001f);Assert.AreEqual(100f,s.PercentileMs(100),.001f);Assert.AreEqual(1f,s.PercentileMs(0),.001f);Assert.AreEqual(100f,s.MaxMs,.001f); }
  [Test] public void OneSlowFrameShowsInTheMaxAndTheTailButBarelyInTheAverage() { var s=new FrameTimeStats();for(int i=0;i<99;i++)s.Add(.016f);s.Add(.2f);Assert.AreEqual(200f,s.MaxMs,.01f);Assert.AreEqual(200f,s.PercentileMs(100),.01f);Assert.AreEqual(16f,s.PercentileMs(95),.01f);Assert.Less(s.AverageMs,19f); }
  [Test] public void InvalidSamplesAreIgnored() { var s=new FrameTimeStats();s.Add(0);s.Add(-1);s.Add(float.NaN);s.Add(float.PositiveInfinity);s.Add(.02f);Assert.AreEqual(1,s.Count);Assert.AreEqual(20f,s.AverageMs,.001f); }
  [Test] public void ResetClearsEverything() { var s=new FrameTimeStats();s.Add(.02f);s.Reset();Assert.AreEqual(0,s.Count);Assert.AreEqual(0,s.MaxMs); }
 }
}
