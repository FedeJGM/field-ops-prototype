using NUnit.Framework;
using UnityEngine;
namespace FieldOps.Tests {
 public class HudLayoutTests {
  [Test] public void NewLayoutIsEmptyAndSerializesToNothing() { var l=new HudLayout();Assert.IsTrue(l.IsEmpty);Assert.AreEqual("",l.ToJson());Assert.IsFalse(l.TryGet("fire",out _)); }
  [Test] public void EveryMovableElementRoundTripsThroughJson() { var l=new HudLayout();for(int i=0;i<HudLayout.Ids.Length;i++)Assert.IsTrue(l.Set(HudLayout.Ids[i],new Vector2(-100.5f*i,40+i)));Assert.IsFalse(l.IsEmpty);var back=HudLayout.FromJson(l.ToJson());for(int i=0;i<HudLayout.Ids.Length;i++){Assert.IsTrue(back.TryGet(HudLayout.Ids[i],out var p));Assert.AreEqual(-100.5f*i,p.x,.001f);Assert.AreEqual(40+i,p.y,.001f);} }
  [Test] public void UnknownElementsCannotBeStored() { var l=new HudLayout();Assert.IsFalse(l.Set("settings",Vector2.zero));Assert.IsFalse(l.Set("",Vector2.zero));Assert.IsFalse(l.Set(null,Vector2.zero));Assert.IsFalse(l.Set("fire",new Vector2(float.NaN,0)));Assert.IsFalse(l.Set("fire",new Vector2(0,float.PositiveInfinity)));Assert.IsTrue(l.IsEmpty); }
  [Test] public void SetReplacesAndClearEmpties() { var l=new HudLayout();l.Set("fire",new Vector2(1,2));l.Set("fire",new Vector2(3,4));Assert.IsTrue(l.TryGet("fire",out var p));Assert.AreEqual(new Vector2(3,4),p);l.Clear();Assert.IsTrue(l.IsEmpty); }
  [TestCase(null)][TestCase("")][TestCase("not json")][TestCase("{}")][TestCase("{\"version\":99,\"entries\":[{\"id\":\"fire\",\"x\":1,\"y\":2}]}")][TestCase("{\"version\":1,\"entries\":[{\"id\":\"settings\",\"x\":1,\"y\":2}]}")][TestCase("{\"version\":1,\"entries\":[{\"id\":\"fire\",\"x\":1,\"y\":2},{\"id\":\"fire\",\"x\":3,\"y\":4}]}")][TestCase("{\"version\":1,\"entries\":[{\"id\":\"fire\",\"x\":1000000,\"y\":2}]}")][TestCase("{\"version\":1,\"entries\":[{\"id\":\"fire\",\"x\":1,\"y\":2},{\"id\":\"bogus\",\"x\":3,\"y\":4}]}")]
  public void CorruptOrUnsupportedDataGivesAnEmptyLayout(string json) { Assert.IsTrue(HudLayout.FromJson(json).IsEmpty); }
  [Test] public void ClampKeepsInsidePositionsUntouched() { Assert.AreEqual(new Vector2(-160,170),HudLayout.ClampInside(new Vector2(-160,170),new Vector2(1,0),new Vector2(175,130),new Vector2(1600,900))); }
  [Test] public void ClampPullsTheWholeButtonBackInsideEveryEdge() {
   var area=new Vector2(1600,900);var size=new Vector2(175,130);
   Assert.AreEqual(new Vector2(-87.5f,835),HudLayout.ClampInside(new Vector2(500,2000),new Vector2(1,0),size,area));
   Assert.AreEqual(new Vector2(87.5f-1600,65),HudLayout.ClampInside(new Vector2(-5000,-5000),new Vector2(1,0),size,area));
   Assert.AreEqual(new Vector2(87.5f,65),HudLayout.ClampInside(new Vector2(-100,-100),Vector2.zero,size,area));
   Assert.AreEqual(new Vector2(1600-87.5f,900-65),HudLayout.ClampInside(new Vector2(9000,9000),Vector2.zero,size,area));
  }
  [Test] public void ClampCentersAnElementLargerThanTheArea() { var p=HudLayout.ClampInside(new Vector2(-9999,9999),Vector2.zero,new Vector2(3000,2000),new Vector2(1600,900));Assert.AreEqual(new Vector2(800,450),p); }
  [Test] public void OverlapIsTrueForPartialAndContainedAndFalseForTouchingOrApart() {
   var size=new Vector2(100,100);
   Assert.IsTrue(HudLayout.Overlaps(new Vector2(0,0),size,new Vector2(50,50),size));Assert.IsTrue(HudLayout.Overlaps(new Vector2(0,0),new Vector2(300,300),new Vector2(10,10),size));
   Assert.IsFalse(HudLayout.Overlaps(new Vector2(0,0),size,new Vector2(100,0),size),"Touching edges do not overlap.");Assert.IsFalse(HudLayout.Overlaps(new Vector2(0,0),size,new Vector2(300,300),size));
   Assert.AreEqual(HudLayout.Overlaps(new Vector2(0,0),size,new Vector2(50,50),size),HudLayout.Overlaps(new Vector2(50,50),size,new Vector2(0,0),size));
  }
 }
 public class HudLayoutSettingsTests {
  [SetUp] public void Clean() {PlayerPrefs.DeleteKey("hudLayout");}
  [TearDown] public void CleanAfter() {PlayerPrefs.DeleteKey("hudLayout");}
  [Test] public void StoredLayoutDefaultsToEmpty() { Assert.IsTrue(GameSettings.HudLayout.IsEmpty); }
  [Test] public void LayoutPersistsInPlayerPrefsAndEmptyLayoutRemovesTheKey() { var l=new HudLayout();l.Set("fire",new Vector2(-300,200));GameSettings.HudLayout=l;Assert.IsTrue(PlayerPrefs.HasKey("hudLayout"));Assert.IsTrue(GameSettings.HudLayout.TryGet("fire",out var p));Assert.AreEqual(new Vector2(-300,200),p);GameSettings.HudLayout=new HudLayout();Assert.IsFalse(PlayerPrefs.HasKey("hudLayout"));Assert.IsTrue(GameSettings.HudLayout.IsEmpty); }
  [Test] public void CorruptStoredLayoutFallsBackToEmpty() { PlayerPrefs.SetString("hudLayout","garbage{{");Assert.IsTrue(GameSettings.HudLayout.IsEmpty); }
 }
}
