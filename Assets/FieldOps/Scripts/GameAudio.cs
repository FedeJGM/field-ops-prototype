using UnityEngine;
namespace FieldOps {
 public enum Sfx { Shot, Reload, DryFire, Hit }
 // Plays the equipped weapon's recorded shot and reload sounds plus two small generated sounds (empty click, target hit).
 // Counters and last-played clips work without an audio device, which is how batch-mode tests observe playback.
 public sealed class GameAudio : MonoBehaviour {
  readonly int[] counts=new int[4];
  readonly AudioClip[] last=new AudioClip[4];
  AudioClip dryFire,hit;AudioSource effects,reload;WeaponDefinition weapon;
  public int StopCount { get; private set; }
  void Awake() {
   int rate=AudioSettings.outputSampleRate;if(rate<8000)rate=SoundSynth.DefaultSampleRate;
   dryFire=Make("DryFire",SoundSynth.DryFire(rate),rate);hit=Make("Hit",SoundSynth.Hit(rate),rate);
   effects=NewSource();reload=NewSource();
  }
  AudioSource NewSource() {var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;return source;}
  static AudioClip Make(string name,float[] data,int rate) {var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;}
  public void SetWeapon(WeaponDefinition definition) { weapon=definition; }
  public int Count(Sfx sfx) { return counts[(int)sfx]; }
  public AudioClip LastClip(Sfx sfx) { return last[(int)sfx]; }
  AudioClip Resolve(Sfx sfx) {
   switch(sfx) {
    case Sfx.Shot:return weapon!=null?weapon.shotClip:null;
    case Sfx.Reload:return weapon!=null?weapon.reloadClip:null;
    case Sfx.DryFire:return dryFire;
    default:return hit;
   }
  }
  public void Play(Sfx sfx) {
   counts[(int)sfx]++;var clip=Resolve(sfx);last[(int)sfx]=clip;if(clip==null||effects==null)return;
   if(sfx==Sfx.Reload) {reload.Stop();reload.clip=clip;reload.volume=.8f;reload.Play();return;}
   effects.PlayOneShot(clip,sfx==Sfx.Shot?1f:(sfx==Sfx.DryFire?.6f:.7f));
  }
  public void StopAll() {StopCount++;if(effects!=null){effects.Stop();reload.Stop();}}
 }
}
