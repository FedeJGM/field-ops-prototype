using UnityEngine;
namespace FieldOps {
 public enum Sfx { Shot, Reload, DryFire, Hit }
 // Plays the generated sounds. Counters work even without an audio device (batch mode), so tests can observe them.
 public sealed class GameAudio : MonoBehaviour {
  readonly int[] counts=new int[4];
  AudioClip[] clips;AudioSource effects,reload;
  public int StopCount { get; private set; }
  void Awake() {
   int rate=AudioSettings.outputSampleRate;if(rate<8000)rate=SoundSynth.DefaultSampleRate;
   clips=new[]{Make("Shot",SoundSynth.Shot(rate),rate),Make("Reload",SoundSynth.Reload(rate),rate),Make("DryFire",SoundSynth.DryFire(rate),rate),Make("Hit",SoundSynth.Hit(rate),rate)};
   effects=NewSource();reload=NewSource();
  }
  AudioSource NewSource() {var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;return source;}
  static AudioClip Make(string name,float[] data,int rate) {var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;}
  public int Count(Sfx sfx) { return counts[(int)sfx]; }
  public void Play(Sfx sfx) {
   counts[(int)sfx]++;if(clips==null)return;
   if(sfx==Sfx.Reload) {reload.Stop();reload.clip=clips[(int)sfx];reload.volume=.7f;reload.Play();return;}
   effects.PlayOneShot(clips[(int)sfx],sfx==Sfx.Shot?.8f:(sfx==Sfx.DryFire?.6f:.7f));
  }
  public void StopAll() {StopCount++;if(effects!=null){effects.Stop();reload.Stop();}}
 }
}
