using System;
using NUnit.Framework;
namespace FieldOps.Tests {
 public class MagazineTests {
  [Test] public void StartsFullWithThirtyRounds() { var m=new Magazine();Assert.AreEqual(30,m.Capacity);Assert.AreEqual(30,m.Count);Assert.IsFalse(m.IsEmpty);Assert.IsFalse(m.IsReloading); }
  [Test] public void EachShotConsumesOneRoundUntilEmpty() { var m=new Magazine(3);Assert.IsTrue(m.TryConsume());Assert.AreEqual(2,m.Count);Assert.IsTrue(m.TryConsume());Assert.IsTrue(m.TryConsume());Assert.IsTrue(m.IsEmpty);Assert.IsFalse(m.TryConsume());Assert.AreEqual(0,m.Count); }
  [Test] public void EmptyMagazineDoesNotReloadByItself() { var m=new Magazine(1);m.TryConsume();m.Tick(100);Assert.IsTrue(m.IsEmpty);Assert.IsFalse(m.IsReloading); }
  [Test] public void ReloadRefillsToCapacityAfterTheReloadTime() { var m=new Magazine(30,1.5);for(int i=0;i<10;i++)m.TryConsume();Assert.IsTrue(m.BeginReload());Assert.IsTrue(m.IsReloading);m.Tick(1.49);Assert.IsTrue(m.IsReloading);Assert.AreEqual(20,m.Count);m.Tick(.02);Assert.IsFalse(m.IsReloading);Assert.AreEqual(30,m.Count); }
  [Test] public void CannotFireWhileReloading() { var m=new Magazine(30,1.5);m.TryConsume();m.BeginReload();Assert.IsFalse(m.TryConsume());Assert.AreEqual(29,m.Count); }
  [Test] public void ReloadIsRejectedWhenFullOrAlreadyReloading() { var m=new Magazine();Assert.IsFalse(m.BeginReload());m.TryConsume();Assert.IsTrue(m.BeginReload());Assert.IsFalse(m.BeginReload()); }
  [Test] public void ReloadWorksFromEmptyWithInfiniteReserve() { var m=new Magazine(2,1);m.TryConsume();m.TryConsume();for(int cycle=0;cycle<3;cycle++){Assert.IsTrue(m.BeginReload());m.Tick(1);Assert.AreEqual(2,m.Count);m.TryConsume();m.TryConsume();} }
  [Test] public void InvalidElapsedValuesDoNotAdvanceReload() { var m=new Magazine(30,1);m.TryConsume();m.BeginReload();m.Tick(-5);m.Tick(double.NaN);m.Tick(double.PositiveInfinity);Assert.IsTrue(m.IsReloading);Assert.AreEqual(29,m.Count); }
  [Test] public void ResetRestoresFullMagazineAndCancelsReload() { var m=new Magazine(30,1);for(int i=0;i<5;i++)m.TryConsume();m.BeginReload();m.Reset();Assert.AreEqual(30,m.Count);Assert.IsFalse(m.IsReloading);m.Tick(5);Assert.AreEqual(30,m.Count); }
  [Test] public void ConstructorRejectsInvalidSettings() { Assert.Throws<ArgumentOutOfRangeException>(()=>new Magazine(0));Assert.Throws<ArgumentOutOfRangeException>(()=>new Magazine(-1));Assert.Throws<ArgumentOutOfRangeException>(()=>new Magazine(30,0));Assert.Throws<ArgumentOutOfRangeException>(()=>new Magazine(30,double.NaN)); }
 }
}
