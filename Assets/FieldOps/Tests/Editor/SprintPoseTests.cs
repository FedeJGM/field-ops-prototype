using NUnit.Framework;
namespace FieldOps.Tests {
 public sealed class SprintPoseTests {
  const double Dt=1.0/60;
  static void Run(SprintPose p,bool wants,bool firing,double seconds) { for(double t=0;t<seconds;t+=Dt)p.Step(wants,firing,Dt); }
  [Test] public void StartsInTheFiringPose() { Assert.AreEqual(0f,new SprintPose().Blend); }
  [Test] public void WantsThePoseOnlyWhenSprintingAndMovingAndNotAimingOrSliding() {
   var p=new SprintPose();Assert.IsTrue(p.Wants(true,true,false,false));
   Assert.IsFalse(p.Wants(false,true,false,false),"Not sprinting.");Assert.IsFalse(p.Wants(true,false,false,false),"Standing still with the sprint held.");Assert.IsFalse(p.Wants(true,true,true,false),"Aiming.");Assert.IsFalse(p.Wants(true,true,false,true),"Sliding.");
  }
  [Test] public void RisesInAboutAQuarterOfASecond() { var p=new SprintPose();Run(p,true,false,.11);Assert.That(p.Blend,Is.InRange(.4f,.6f));Run(p,true,false,.2);Assert.AreEqual(1f,p.Blend,.001f); }
  [Test] public void LowersFasterThanItRises() { var up=new SprintPose();Run(up,true,false,.22);Assert.AreEqual(1f,up.Blend,.001f);Run(up,false,false,.12);Assert.AreEqual(0f,up.Blend,.02f); }
  [Test] public void FiringDropsItEvenFasterAndKeepsItDownWhileFiring() { var p=new SprintPose();Run(p,true,false,.3);Run(p,true,true,.08);Assert.AreEqual(0f,p.Blend,.02f);Run(p,true,true,1);Assert.AreEqual(0f,p.Blend,.001f,"Holding fire keeps the firing pose even if the sprint continues."); }
  [Test] public void RaisesAgainAfterFiringStops() { var p=new SprintPose();Run(p,true,true,.3);Run(p,true,false,.3);Assert.AreEqual(1f,p.Blend,.001f); }
  [Test] public void BlendStaysInsideZeroAndOne() { var p=new SprintPose();Run(p,true,false,5);Assert.AreEqual(1f,p.Blend);Run(p,false,false,5);Assert.AreEqual(0f,p.Blend); }
  [Test] public void InvalidTimeStepsAreIgnored() { var p=new SprintPose();p.Step(true,false,double.NaN);p.Step(true,false,-1);p.Step(true,false,double.PositiveInfinity);Assert.AreEqual(0f,p.Blend); }
  [Test] public void ResetReturnsToTheFiringPose() { var p=new SprintPose();Run(p,true,false,.3);p.Reset();Assert.AreEqual(0f,p.Blend); }
 }
}
