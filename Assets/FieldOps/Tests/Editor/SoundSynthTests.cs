using System;
using System.Linq;
using NUnit.Framework;
namespace FieldOps.Tests {
 public class SoundSynthTests {
  static double Energy(float[] s,int from,int to) { double e=0;for(int i=from;i<to&&i<s.Length;i++)e+=s[i]*s[i];return e; }
  static float[][] All() { return new[]{SoundSynth.Shot(),SoundSynth.DryFire(),SoundSynth.Hit(),SoundSynth.Reload()}; }
  [Test] public void EveryClipIsAudibleBoundedAndFinite() { foreach(var clip in All()) {Assert.Greater(clip.Length,1000);Assert.IsTrue(clip.All(x=>!float.IsNaN(x)&&!float.IsInfinity(x)));float peak=clip.Max(x=>Math.Abs(x));Assert.Greater(peak,.4f);Assert.LessOrEqual(peak,1f);} }
  [Test] public void ClipLengthsMatchTheirPurpose() { Assert.That(SoundSynth.Shot().Length/44100.0,Is.InRange(.12,.35));Assert.That(SoundSynth.DryFire().Length/44100.0,Is.InRange(.03,.1));Assert.That(SoundSynth.Hit().Length/44100.0,Is.InRange(.06,.2));Assert.AreEqual((int)(Magazine.DefaultReloadSeconds*44100),SoundSynth.Reload().Length,"Reload sound must end when the reload does."); }
  [Test] public void ShotAndHitDecayInsteadOfSustaining() { foreach(var clip in new[]{SoundSynth.Shot(),SoundSynth.Hit()}) {int q=clip.Length/4;Assert.Less(Energy(clip,clip.Length-q,clip.Length),Energy(clip,0,q)*.05);} }
  [Test] public void ShotAttackIsImmediate() { var s=SoundSynth.Shot();Assert.Greater(s.Take(441).Max(x=>Math.Abs(x)),.4f,"Peak must land in the first 10 ms."); }
  static double Window(float[] s,double from,double to) { return Energy(s,(int)(from*44100),(int)(to*44100)); }
  [Test] public void ReloadFollowsTheRealSequenceOfAFirearmReload() {
   var s=SoundSynth.Reload();
   Assert.Greater(Window(s,.08,.14),.0005,"Magazine release click.");Assert.Greater(Window(s,.18,.30),.0005,"Magazine sliding out.");Assert.Greater(Window(s,.34,.44),.0005,"Magazine dropping.");
   Assert.Greater(Window(s,.60,.72),.0005,"New magazine sliding in.");Assert.Greater(Window(s,.76,.91),.0005,"Magazine seating.");Assert.Greater(Window(s,.98,1.08),.0005,"Charging handle pulled back.");Assert.Greater(Window(s,1.16,1.32),.0005,"Bolt slamming forward.");
  }
  [Test] public void ReloadHasSilentGapsBetweenItsSteps() { var s=SoundSynth.Reload();Assert.AreEqual(0,Window(s,.46,.58),1e-9);Assert.AreEqual(0,Window(s,.93,.97),1e-9);Assert.AreEqual(0,Window(s,1.4,1.5),1e-9,"The reload ends in silence."); }
  [Test] public void HeavyMetalImpactsAreLouderThanTheLightReleaseClick() { var s=SoundSynth.Reload();double click=Window(s,.08,.14);Assert.Greater(Window(s,.76,.91),click*3,"Seating the magazine is a heavy hit.");Assert.Greater(Window(s,1.16,1.32),click*3,"The bolt slam is a heavy hit."); }
  [Test] public void ImpactsRingLikeMetalWithHighFrequencyContent() { var s=SoundSynth.Reload();double Roughness(double from,double to) {double e=0;for(int i=(int)(from*44100)+1;i<(int)(to*44100);i++){double d=s[i]-s[i-1];e+=d*d;}return e;}double slam=Window(s,1.16,1.32);Assert.Greater(Roughness(1.16,1.32)/slam,.05,"A metal impact is bright, not a dull thump."); }
  [Test] public void GenerationIsDeterministic() { Assert.AreEqual(SoundSynth.Shot(),SoundSynth.Shot());Assert.AreEqual(SoundSynth.Reload(),SoundSynth.Reload());Assert.AreEqual(SoundSynth.Hit(),SoundSynth.Hit()); }
  [Test] public void ClipsAreDistinctFromEachOther() { var clips=All();for(int a=0;a<clips.Length;a++)for(int b=a+1;b<clips.Length;b++)Assert.IsFalse(clips[a].Length==clips[b].Length&&clips[a].SequenceEqual(clips[b]),a+" vs "+b); }
  [Test] public void OtherSampleRatesKeepTheSameDuration() { Assert.AreEqual(SoundSynth.Shot(48000).Length/48000.0,SoundSynth.Shot(44100).Length/44100.0,.001);Assert.AreEqual(SoundSynth.Reload(22050).Length/22050.0,Magazine.DefaultReloadSeconds,.001); }
  [Test] public void InvalidSampleRateIsRejected() { foreach(int bad in new[]{0,-1,7999})Assert.Throws<ArgumentOutOfRangeException>(()=>SoundSynth.Shot(bad),"rate "+bad);Assert.Throws<ArgumentOutOfRangeException>(()=>SoundSynth.Reload(0));Assert.Throws<ArgumentOutOfRangeException>(()=>SoundSynth.Hit(-5));Assert.Throws<ArgumentOutOfRangeException>(()=>SoundSynth.DryFire(0)); }
 }
}
