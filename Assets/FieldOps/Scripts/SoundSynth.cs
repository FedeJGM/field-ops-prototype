using System;
namespace FieldOps {
 // Pure, deterministic generation of the two small sounds that have no recorded source: the empty-magazine click and the target hit.
 public static class SoundSynth {
  public const int DefaultSampleRate=44100;
  const double Tau=Math.PI*2;
  static void Check(int sampleRate) { if(sampleRate<8000)throw new ArgumentOutOfRangeException(nameof(sampleRate)); }
  static float Noise(ref uint state) { state^=state<<13;state^=state>>17;state^=state<<5;return state/(float)uint.MaxValue*2f-1f; }
  static void Normalize(float[] clip,float peak) {
   float max=0;foreach(var x in clip)max=Math.Max(max,Math.Abs(x));
   if(max>0)for(int i=0;i<clip.Length;i++)clip[i]*=peak/max;
  }
  // Short linear fade so a clip never ends on a click.
  static void FadeOut(float[] clip,int from,int to) {
   for(int i=from;i<to&&i<clip.Length;i++)clip[i]*=1f-(i-from)/(float)Math.Max(1,to-from);
  }
  public static float[] DryFire(int sampleRate=DefaultSampleRate) {
   Check(sampleRate);var clip=new float[(int)(.06*sampleRate)];uint seed=0x9E3779B9u;
   for(int i=0;i<clip.Length;i++) {
    double t=i/(double)sampleRate;
    clip[i]=Noise(ref seed)*(float)Math.Exp(-t*220)*.8f+(float)(Math.Sin(Tau*1900*t)*Math.Exp(-t*140))*.6f;
   }
   FadeOut(clip,clip.Length*8/10,clip.Length);Normalize(clip,.7f);return clip;
  }
  public static float[] Hit(int sampleRate=DefaultSampleRate) {
   Check(sampleRate);var clip=new float[(int)(.12*sampleRate)];uint seed=0xC0FFEE11u;
   for(int i=0;i<clip.Length;i++) {
    double t=i/(double)sampleRate;
    clip[i]=(float)(Math.Sin(Tau*620*t)*Math.Exp(-t*40))+.5f*(float)(Math.Sin(Tau*1480*t)*Math.Exp(-t*70))+Noise(ref seed)*(float)Math.Exp(-t*300)*.4f;
   }
   FadeOut(clip,clip.Length*9/10,clip.Length);Normalize(clip,.8f);return clip;
  }
 }
}
