using System;
namespace FieldOps {
 // Pure ammunition rules: infinite reserve, manual reload, progress advances only on active-play time.
 public sealed class Magazine {
  public const int DefaultCapacity=30;
  public const double DefaultReloadSeconds=1.5;
  readonly double reloadSeconds;
  double reloadRemaining;
  public int Capacity { get; }
  public int Count { get; private set; }
  public bool IsReloading { get; private set; }
  public bool IsEmpty => Count<=0;
  public Magazine(int capacity=DefaultCapacity,double reloadSeconds=DefaultReloadSeconds) {
   if(capacity<=0)throw new ArgumentOutOfRangeException(nameof(capacity));
   if(double.IsNaN(reloadSeconds)||double.IsInfinity(reloadSeconds)||reloadSeconds<=0)throw new ArgumentOutOfRangeException(nameof(reloadSeconds));
   Capacity=capacity;Count=capacity;this.reloadSeconds=reloadSeconds;
  }
  public bool TryConsume() {
   if(IsReloading||IsEmpty)return false;
   Count--;return true;
  }
  public bool BeginReload() {
   if(IsReloading||Count>=Capacity)return false;
   IsReloading=true;reloadRemaining=reloadSeconds;return true;
  }
  public void Tick(double seconds) {
   if(!IsReloading||seconds<0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   reloadRemaining-=seconds;
   if(reloadRemaining<=0) {IsReloading=false;Count=Capacity;}
  }
  public void Reset() {Count=Capacity;IsReloading=false;reloadRemaining=0;}
 }
}
