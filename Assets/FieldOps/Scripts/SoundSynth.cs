using System;
namespace FieldOps {
 // Pure, deterministic sound generation: no audio files, no licenses.
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
  public static float[] Shot(int sampleRate=DefaultSampleRate) {
   Check(sampleRate);var clip=new float[(int)(.22*sampleRate)];uint seed=0x1234ABCDu;float previous=0;double phase=0;
   for(int i=0;i<clip.Length;i++) {
    double t=i/(double)sampleRate;float n=Noise(ref seed);
    float crack=(n-previous)*(float)Math.Exp(-t*110)*.7f;float body=n*(float)Math.Exp(-t*26)*.8f;
    phase+=Tau*(45+140*Math.Exp(-t*10))/sampleRate;float thump=(float)(Math.Sin(phase)*Math.Exp(-t*14))*.9f;
    clip[i]=crack+body+thump;previous=n;
   }
   FadeOut(clip,clip.Length*9/10,clip.Length);Normalize(clip,.9f);return clip;
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
  // A firearm reload as a sequence of physical events inside the 1.5 s reload:
  // release click, magazine sliding out and dropping, new magazine sliding in and seating, charging handle pulled back and bolt slamming forward.
  public static float[] Reload(int sampleRate=DefaultSampleRate) {
   Check(sampleRate);var clip=new float[(int)(Magazine.DefaultReloadSeconds*sampleRate)];uint seed=0xBADC0DE5u;
   Metal(clip,sampleRate,ref seed,.08,.06,new[]{2400.0,4100},new[]{.6f,.4f},new[]{220.0,300},.5f,500,.45f);
   Friction(clip,sampleRate,ref seed,.18,.12,700,3500,.3f,false);
   Thud(clip,sampleRate,ref seed,.34,.1,.5f);
   Friction(clip,sampleRate,ref seed,.60,.12,900,4200,.35f,true);
   Metal(clip,sampleRate,ref seed,.76,.15,new[]{140.0,780,1650,3100},new[]{1f,.6f,.45f,.25f},new[]{30.0,38,55,80},.5f,200,1f);
   Friction(clip,sampleRate,ref seed,.98,.1,1500,5500,.3f,false);
   Metal(clip,sampleRate,ref seed,1.16,.16,new[]{110.0,520,1230,2700,4800},new[]{.8f,.9f,.6f,.4f,.25f},new[]{35.0,28,45,70,110},.6f,160,.95f);
   FadeOut(clip,clip.Length*95/100,clip.Length);Normalize(clip,.85f);return clip;
  }
  // Metal impact: inharmonic ringing partials plus a short noise transient.
  static void Metal(float[] clip,int sampleRate,ref uint seed,double start,double length,double[] frequencies,float[] amplitudes,double[] decays,float noiseLevel,double noiseDecay,float level) {
   int first=(int)(start*sampleRate),end=Math.Min(clip.Length,first+(int)(length*sampleRate));
   for(int i=first;i<end;i++) {
    double t=(i-first)/(double)sampleRate;float v=Noise(ref seed)*noiseLevel*(float)Math.Exp(-t*noiseDecay);
    for(int m=0;m<frequencies.Length;m++)v+=amplitudes[m]*(float)(Math.Sin(Tau*frequencies[m]*t)*Math.Exp(-t*decays[m]));
    clip[i]+=v*level*Math.Min(1f,(end-i)/(.008f*sampleRate));
   }
  }
  // Soft low-frequency thud, like a magazine landing on dirt.
  static void Thud(float[] clip,int sampleRate,ref uint seed,double start,double length,float level) {
   int first=(int)(start*sampleRate),end=Math.Min(clip.Length,first+(int)(length*sampleRate));float low=0;
   for(int i=first;i<end;i++) {
    double t=(i-first)/(double)sampleRate;low+=.08f*(Noise(ref seed)-low);
    clip[i]+=(low*3f+(float)Math.Sin(Tau*90*t))*(float)Math.Exp(-t*40)*level*Math.Min(1f,(end-i)/(.008f*sampleRate));
   }
  }
  // Scraping metal: band-passed noise with a slow wobble; sliding in rises in pitch.
  static void Friction(float[] clip,int sampleRate,ref uint seed,double start,double length,double lowCut,double highCut,float level,bool rising) {
   int first=(int)(start*sampleRate),count=(int)(length*sampleRate),end=Math.Min(clip.Length,first+count);float slow=0,fast=0;
   for(int i=first;i<end;i++) {
    double u=(i-first)/(double)count,t=(i-first)/(double)sampleRate;double scale=rising?.5+.5*u:1;
    float n=Noise(ref seed);slow+=(float)(1-Math.Exp(-Tau*lowCut*scale/sampleRate))*(n-slow);fast+=(float)(1-Math.Exp(-Tau*highCut*scale/sampleRate))*(n-fast);
    double envelope=Math.Sin(Math.PI*u);envelope*=envelope;
    clip[i]+=(fast-slow)*(float)(envelope*(1+.5*Math.Sin(Tau*45*t)))*level*2f;
   }
  }
 }
}
