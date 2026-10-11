using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace FieldOps.Tests {
 public class WeaponAudioTests {
  const string Folder="Assets/FieldOps/Audio/Weapons/";
  static WeaponDefinition[] Weapons() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);return UnityEngine.Object.FindFirstObjectByType<Rifle>().weapons; }
  static float[] Samples(AudioClip clip) { var s=new float[clip.samples*clip.channels];clip.GetData(s,0);return s; }
  static float ZeroCrossingRate(AudioClip clip,float seconds) { var s=Samples(clip);int n=Math.Min(s.Length,(int)(seconds*clip.frequency));int crossings=0;for(int i=1;i<n;i++)if((s[i]>=0)!=(s[i-1]>=0))crossings++;return crossings/(float)n; }
  [Test] public void EveryWeaponHasItsOwnShotAndReloadClipFromTheCreditedFolder() {
   var weapons=Weapons();foreach(var w in weapons) {Assert.IsNotNull(w.shotClip,w.nameKey+" shot");Assert.IsNotNull(w.reloadClip,w.nameKey+" reload");StringAssert.StartsWith(Folder,AssetDatabase.GetAssetPath(w.shotClip));StringAssert.StartsWith(Folder,AssetDatabase.GetAssetPath(w.reloadClip));}
   Assert.AreEqual(weapons.Length,weapons.Select(w=>w.shotClip).Distinct().Count(),"A pistol must not reuse the sniper's shot.");Assert.AreEqual(weapons.Length,weapons.Select(w=>w.reloadClip).Distinct().Count());
  }
  [Test] public void ShotClipsDifferInContentAndAreAudible() {
   var weapons=Weapons();var data=weapons.Select(w=>Samples(w.shotClip)).ToArray();
   for(int a=0;a<data.Length;a++) {Assert.Greater(data[a].Max(Math.Abs),.5f,weapons[a].nameKey+" shot must be audible");for(int b=a+1;b<data.Length;b++)Assert.IsFalse(data[a].Length==data[b].Length&&data[a].SequenceEqual(data[b]),weapons[a].nameKey+" vs "+weapons[b].nameKey);}
  }
  [Test] public void ReloadDurationFollowsTheReloadSound() { foreach(var w in Weapons()) {Assert.AreEqual(w.reloadClip.length,w.reloadSeconds,.01f,w.nameKey+": the reload lasts as long as its sound.");Assert.That(w.reloadSeconds,Is.InRange(1.0f,3.0f),w.nameKey);} }
  [Test] public void ClipsAreMonoAtFortyFourKilohertzWithSaneLengths() { foreach(var w in Weapons()) {foreach(var clip in new[]{w.shotClip,w.reloadClip}) {Assert.AreEqual(1,clip.channels,clip.name);Assert.AreEqual(44100,clip.frequency,clip.name);}Assert.That(w.shotClip.length,Is.InRange(.3f,2f),w.nameKey+" shot");} }
  [Test] public void TheSubMachineGunShotIsAHigherPitchedShorterVersionOfTheRifleShot() { var w=Weapons();var rifle=w[1].shotClip;var smg=w[2].shotClip;Assert.Greater(ZeroCrossingRate(smg,.15f),ZeroCrossingRate(rifle,.15f)*1.15f,"Higher pitch means more zero crossings.");Assert.Less(smg.length,rifle.length); }
  [Test] public void ClipsAreImportedToPlayInstantlyOnAndroid() { foreach(var w in Weapons())foreach(var clip in new[]{w.shotClip,w.reloadClip}) {var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));Assert.AreEqual(AudioClipLoadType.DecompressOnLoad,importer.defaultSampleSettings.loadType,clip.name);Assert.IsFalse(importer.loadInBackground,clip.name);} }
  [Test] public void AudioAttributionIsRecordedInCreditsAndNotice() {
   var credits=File.ReadAllText("CREDITS.md");StringAssert.Contains("Vincent Sevedge",credits);StringAssert.Contains("CC BY 3.0",credits);StringAssert.Contains("Joaken",credits);StringAssert.Contains("Assets/FieldOps/Audio/Weapons/",credits);
   var notice=File.ReadAllText(Folder+"NOTICE.txt");StringAssert.Contains("Vincent Sevedge",notice);StringAssert.Contains("Changes made",notice);StringAssert.Contains("CC BY 3.0",notice);StringAssert.Contains("Joaken",notice);
  }
 }
}
