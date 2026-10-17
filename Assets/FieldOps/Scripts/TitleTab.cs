namespace FieldOps {
 // The title tab is shown for the first seconds of a round and then fades out.
 public static class TitleTab {
  public const float VisibleSeconds=5f,FadeSeconds=.5f;
  public static float Alpha(double elapsedSeconds,bool roundActive) {
   if(!roundActive||double.IsNaN(elapsedSeconds)||double.IsInfinity(elapsedSeconds))return 0f;
   if(elapsedSeconds<0)elapsedSeconds=0;
   if(elapsedSeconds>=VisibleSeconds)return 0f;
   double fadeStart=VisibleSeconds-FadeSeconds;
   return elapsedSeconds<=fadeStart?1f:(float)((VisibleSeconds-elapsedSeconds)/FadeSeconds);
  }
 }
}
