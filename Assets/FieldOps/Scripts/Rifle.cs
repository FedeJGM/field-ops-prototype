using UnityEngine;
namespace FieldOps {
 public sealed class Rifle : MonoBehaviour {
  public TrainingSession session;public PrototypeInput input;public Camera view;public Transform weapon;public GameObject muzzleFlash;public float range=80;
  public GameAudio sfx;public Magazine Ammo {get;}=new Magazine();public AimControl Aim {get;}=new AimControl();readonly ShotGate gate=new ShotGate(.12);Vector3 rest;float flashUntil,kick;
  static readonly Vector3 AimOffset=new Vector3(-.28f,-.03f,-.04f);float baseFov;
  void Awake() { rest=weapon.localPosition;baseFov=view.fieldOfView;Aim.Toggle=GameSettings.AimToggle; }
  public void ClearVisuals() {flashUntil=0;kick=0;if(sfx!=null)sfx.StopAll();if(muzzleFlash!=null)muzzleFlash.SetActive(false);}
  public void ResetShots() {gate.Reset();Ammo.Reset();Aim.Reset();ClearVisuals();}
  void LateUpdate() {
   session.Synchronize();
   if(session.GameplayEnabled)Aim.Step(input.AimHeld,input.AimPressed,input.Sprint,true,Time.deltaTime);else Aim.Reset();
   view.fieldOfView=Aim.Fov(baseFov);input.LookScale=Aim.LookScale;
   if(session.GameplayEnabled) {Ammo.Tick(Time.deltaTime);if(input.Reload&&Ammo.BeginReload()&&sfx!=null)sfx.Play(Sfx.Reload);}
   if(session.GameplayEnabled&&input.Fire&&Ammo.IsEmpty&&!Ammo.IsReloading&&gate.TryFire(Time.timeAsDouble)&&sfx!=null)sfx.Play(Sfx.DryFire);
   if(session.GameplayEnabled&&input.Fire&&!Ammo.IsReloading&&!Ammo.IsEmpty&&gate.TryFire(Time.timeAsDouble)) {Ammo.TryConsume();if(sfx!=null)sfx.Play(Sfx.Shot);kick=.045f;flashUntil=Time.time+.04f; if(Physics.Raycast(view.ViewportPointToRay(new Vector3(.5f,.5f)),out var hit,range,~(1<<2),QueryTriggerInteraction.Ignore)) {var target=hit.collider.GetComponentInParent<TrainingTarget>();if(target!=null)target.ReceiveHit(hit,25);} }
   kick=Mathf.MoveTowards(kick,0,Time.deltaTime*.25f);float calm=1f-.7f*Aim.Blend;weapon.localPosition=rest+Vector3.Lerp(Vector3.zero,AimOffset,Aim.Blend)+new Vector3(Mathf.Sin(Time.time*7)*input.Move.magnitude*.006f,Mathf.Abs(Mathf.Cos(Time.time*7))*input.Move.magnitude*.008f,-kick)*calm;muzzleFlash.SetActive(Time.time<flashUntil);
  }
 }
}
