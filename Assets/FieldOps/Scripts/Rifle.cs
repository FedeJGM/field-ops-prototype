using UnityEngine;
namespace FieldOps {
 // Weapon controller (kept under its original name). Stats, model and aim values come from the equipped WeaponDefinition.
 public sealed class Rifle : MonoBehaviour {
  public TrainingSession session;public PrototypeInput input;public Camera view;public Transform weapon;public GameObject muzzleFlash;
  public WeaponDefinition[] weapons;public Transform[] models;
  public GameAudio sfx;public Magazine Ammo {get;private set;}=new Magazine();public AimControl Aim {get;}=new AimControl();
  public WeaponDefinition Current {get;private set;}public int CurrentIndex {get;private set;}=-1;
  ShotGate gate=new ShotGate(.12);Vector3 rest;float flashUntil,kick,baseFov;
  void Awake() { rest=weapon.localPosition;baseFov=view.fieldOfView;Aim.Toggle=GameSettings.AimToggle;Equip(GameSettings.SelectedWeapon); }
  public void Equip(int index) {
   if(weapons==null||weapons.Length==0)return;
   index=Mathf.Clamp(index,0,weapons.Length-1);var def=weapons[index];
   CurrentIndex=index;Current=def;if(sfx!=null)sfx.SetWeapon(def);Ammo=new Magazine(def.magazineSize,def.reloadSeconds);gate=new ShotGate(def.fireInterval);Aim.ZoomFov=def.aimFov;Aim.Reset();
   for(int i=0;i<models.Length;i++)models[i].gameObject.SetActive(i==index);
   muzzleFlash.transform.localPosition=def.muzzleLocal;ClearVisuals();
  }
  public void ClearVisuals() {flashUntil=0;kick=0;if(sfx!=null)sfx.StopAll();if(muzzleFlash!=null)muzzleFlash.SetActive(false);}
  public void ResetShots() {gate.Reset();Ammo.Reset();Aim.Reset();ClearVisuals();}
  // One trigger pull: every pellet damages and marks the target, but only the first accepted hit scores a point.
  void Shoot() {
   var def=Current;var eye=view.transform;bool scored=false;
   for(int p=0;p<def.pellets;p++) {
    var ray=view.ViewportPointToRay(new Vector3(.5f,.5f));
    if(def.pellets>1) {var o=PelletPattern.Offset(p,def.pellets,def.spreadDegrees);ray.direction=(ray.direction+eye.right*o.x+eye.up*o.y).normalized;}
    if(!Physics.Raycast(ray,out var hit,def.range,~(1<<2),QueryTriggerInteraction.Ignore)) {if(Debug.isDebugBuild)Debug.Log("FIELD_OPS_SHOT miss");continue;}
    var target=hit.collider.GetComponentInParent<TrainingTarget>();
    bool accepted=target!=null&&target.ReceiveHit(hit,def.damage,!scored);if(accepted)scored=true;
    if(Debug.isDebugBuild)Debug.Log("FIELD_OPS_SHOT hit="+hit.collider.name+" y="+hit.point.y.ToString("0.00")+" dist="+hit.distance.ToString("0.0")+" target="+(target!=null)+" accepted="+accepted);
   }
  }
  void LateUpdate() {
   session.Synchronize();
   if(session.GameplayEnabled)Aim.Step(input.AimHeld,input.AimPressed,input.Sprint,true,Time.deltaTime);else Aim.Reset();
   view.fieldOfView=Aim.Fov(baseFov);input.LookScale=Aim.LookScale;
   if(session.GameplayEnabled) {Ammo.Tick(Time.deltaTime);if(input.Reload&&Ammo.BeginReload()&&sfx!=null)sfx.Play(Sfx.Reload);}
   if(session.GameplayEnabled&&input.Fire&&Ammo.IsEmpty&&!Ammo.IsReloading&&gate.TryFire(Time.timeAsDouble)&&sfx!=null)sfx.Play(Sfx.DryFire);
   if(session.GameplayEnabled&&input.Fire&&!Ammo.IsReloading&&!Ammo.IsEmpty&&gate.TryFire(Time.timeAsDouble)) {Ammo.TryConsume();if(sfx!=null)sfx.Play(Sfx.Shot);kick=Current.kick;flashUntil=Time.time+.04f;Shoot();}
   kick=Mathf.MoveTowards(kick,0,Time.deltaTime*.25f);float calm=1f-.7f*Aim.Blend;weapon.localPosition=rest+Vector3.Lerp(Vector3.zero,Current.aimOffset,Aim.Blend)+new Vector3(Mathf.Sin(Time.time*7)*input.Move.magnitude*.006f,Mathf.Abs(Mathf.Cos(Time.time*7))*input.Move.magnitude*.008f,-kick)*calm;muzzleFlash.SetActive(Time.time<flashUntil);
  }
 }
}
