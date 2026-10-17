using NUnit.Framework;
namespace FieldOps.Tests {
 public sealed class TitleTabTests {
  [Test] public void LastsFiveSecondsWithAHalfSecondFade() { Assert.AreEqual(5f,TitleTab.VisibleSeconds);Assert.AreEqual(.5f,TitleTab.FadeSeconds); }
  [Test] public void FullyVisibleAtTheStartAndDuringMostOfTheWindow() { Assert.AreEqual(1f,TitleTab.Alpha(0,true),.001f);Assert.AreEqual(1f,TitleTab.Alpha(2.5,true),.001f);Assert.AreEqual(1f,TitleTab.Alpha(4.5,true),.001f); }
  [Test] public void FadesLinearlyOverTheLastHalfSecond() { Assert.AreEqual(.5f,TitleTab.Alpha(4.75,true),.001f);Assert.Less(TitleTab.Alpha(4.9,true),TitleTab.Alpha(4.6,true)); }
  [Test] public void GoneAfterFiveSeconds() { Assert.AreEqual(0f,TitleTab.Alpha(5,true),.001f);Assert.AreEqual(0f,TitleTab.Alpha(5.01,true));Assert.AreEqual(0f,TitleTab.Alpha(120,true)); }
  [Test] public void NeverShownOutsideAnActiveRound() { Assert.AreEqual(0f,TitleTab.Alpha(0,false));Assert.AreEqual(0f,TitleTab.Alpha(3,false)); }
  [Test] public void BadValuesAreHandledSafely() { Assert.AreEqual(1f,TitleTab.Alpha(-1,true),"A negative elapsed time counts as the start.");Assert.AreEqual(0f,TitleTab.Alpha(double.NaN,true));Assert.AreEqual(0f,TitleTab.Alpha(double.PositiveInfinity,true)); }
 }
}
