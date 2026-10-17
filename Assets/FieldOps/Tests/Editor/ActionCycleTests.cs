using NUnit.Framework;
namespace FieldOps.Tests {
 public sealed class ActionCycleTests {
  const double Dt=1.0/60;
  static void Run(ActionCycle c,double seconds) { for(double t=0;t<seconds;t+=Dt)c.Step(Dt); }
  [Test] public void StartsInactiveWithNoMovement() { var c=new ActionCycle();Assert.IsFalse(c.Active);Assert.AreEqual(0f,c.Progress);Assert.AreEqual(0f,c.Roll);Assert.AreEqual(0f,c.Pull); }
  [Test] public void LastsExactlyTheCooldownItWasStartedWith() { var c=new ActionCycle();c.Start(1.2);Assert.IsTrue(c.Active);Assert.AreEqual(1.2,c.Duration,1e-6);Run(c,1.15);Assert.IsTrue(c.Active);Run(c,.1);Assert.IsFalse(c.Active);Assert.AreEqual(0f,c.Roll);Assert.AreEqual(0f,c.Pull); }
  [Test] public void ProgressGoesFromZeroToAlmostOne() { var c=new ActionCycle();c.Start(1.0);Run(c,.5);Assert.AreEqual(.5f,c.Progress,.03f);Run(c,.45);Assert.That(c.Progress,Is.InRange(.93f,1f)); }
  [Test] public void TheWeaponTurnsOutHoldsAndTurnsBack() {
   var c=new ActionCycle();c.Start(1.0);float start=c.Roll;Run(c,.5);float mid=c.Roll;Run(c,.45);float end=c.Roll;
   Assert.AreEqual(0f,start,.001f);Assert.AreEqual(1f,mid,.02f,"Fully turned in the middle of the cooldown.");Assert.Less(end,.15f,"Almost back to the firing pose at the end.");
  }
  [Test] public void ThePullHappensInTheMiddleAndReturnsForward() {
   var c=new ActionCycle();c.Start(1.0);float peak=0,peakAt=0;for(int i=0;i<60;i++){c.Step(1.0/60);if(c.Pull>peak){peak=c.Pull;peakAt=c.Progress;}}
   Assert.AreEqual(1f,peak,.02f);Assert.That(peakAt,Is.InRange(.38f,.58f),"The bolt is pulled back around the middle.");Assert.Less(c.Pull,.05f,"And is closed again by the end.");
  }
  [Test] public void WeightsStayBetweenZeroAndOne() { var c=new ActionCycle();c.Start(.85);for(int i=0;i<80;i++){c.Step(.011);Assert.That(c.Roll,Is.InRange(0f,1f));Assert.That(c.Pull,Is.InRange(0f,1f));} }
  [Test] public void TheSoundIsDueOnceWhenProgressPassesItsMoment() {
   var c=new ActionCycle();c.Start(1.0);int due=0;for(int i=0;i<70;i++){c.Step(1.0/60);if(c.SoundDue(.25f))due++;}Assert.AreEqual(1,due);
   c.Start(1.0);Assert.IsFalse(c.SoundDue(.25f),"A new cycle can sound again.");Run(c,.3);Assert.IsTrue(c.SoundDue(.25f));Assert.IsFalse(c.SoundDue(.25f));
  }
  [Test] public void CancelStopsEverythingAtOnce() { var c=new ActionCycle();c.Start(1.0);Run(c,.4);c.Cancel();Assert.IsFalse(c.Active);Assert.AreEqual(0f,c.Roll);Assert.AreEqual(0f,c.Pull);Assert.AreEqual(0f,c.Progress); }
  [Test] public void ATinyCooldownStillGetsAReadableCycle() { var c=new ActionCycle();c.Start(.05);Assert.AreEqual(ActionCycle.MinSeconds,c.Duration,1e-6);c.Start(-3);Assert.AreEqual(ActionCycle.MinSeconds,c.Duration,1e-6);c.Start(double.NaN);Assert.AreEqual(ActionCycle.MinSeconds,c.Duration,1e-6); }
  [Test] public void InvalidTimeStepsAreIgnored() { var c=new ActionCycle();c.Start(1.0);c.Step(double.NaN);c.Step(-1);c.Step(double.PositiveInfinity);Assert.IsTrue(c.Active);Assert.AreEqual(0f,c.Progress,.001f); }
 }
}
