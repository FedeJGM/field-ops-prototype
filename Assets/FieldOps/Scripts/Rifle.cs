using UnityEngine;
namespace FieldOps {
 public sealed class Rifle : MonoBehaviour {
  public TrainingSession session;public PrototypeInput input;public Camera view;public Transform weapon;public GameObject muzzleFlash;public float range=80;
  public Magazine Ammo {get;}=new Magazine();readonly ShotGate gate=new ShotGate(.12);Vector3 rest;float flashUntil,kick;
  void Awake() { rest=weapon.localPosition; }
  public void ClearVisuals() {flashUntil=0;kick=0;if(muzzleFlash!=null)muzzleFlash.SetActive(false);}
  public void ResetShots() {gate.Reset();Ammo.Reset();ClearVisuals();}
  void LateUpdate() {
   session.Synchronize();
   if(session.GameplayEnabled) {Ammo.Tick(Time.deltaTime);if(input.Reload)Ammo.BeginReload();}
   if(session.GameplayEnabled&&input.Fire&&!Ammo.IsReloading&&!Ammo.IsEmpty&&gate.TryFire(Time.timeAsDouble)) {Ammo.TryConsume();kick=.045f;flashUntil=Time.time+.04f; if(Physics.Raycast(view.ViewportPointToRay(new Vector3(.5f,.5f)),out var hit,range,~(1<<2),QueryTriggerInteraction.Ignore)) {var target=hit.collider.GetComponentInParent<TrainingTarget>();if(target!=null)target.ReceiveHit(hit,25);} }
   kick=Mathf.MoveTowards(kick,0,Time.deltaTime*.25f);weapon.localPosition=rest+new Vector3(Mathf.Sin(Time.time*7)*input.Move.magnitude*.006f,Mathf.Abs(Mathf.Cos(Time.time*7))*input.Move.magnitude*.008f,-kick);muzzleFlash.SetActive(Time.time<flashUntil);
  }
 }
}
