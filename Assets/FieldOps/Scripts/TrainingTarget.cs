using System.Collections.Generic;
using UnityEngine;
namespace FieldOps {
 public sealed class TrainingTarget : MonoBehaviour {
  public Animator animator;public GameAudio sfx;public TrainingSession session;static readonly int HitTrigger=Animator.StringToHash("Hit");float nextReaction; public Transform idleRig; public Material impactMaterial;
  public TargetDamage Damage { get; }=new TargetDamage();
  readonly Queue<GameObject> impacts=new Queue<GameObject>();Vector3 restPosition;Quaternion restRotation;
  void Awake() {restPosition=idleRig.localPosition;restRotation=idleRig.localRotation;}
  void Update() { idleRig.localPosition=restPosition+Vector3.up*(Mathf.Sin(Time.time*1.8f)*.012f);idleRig.localRotation=restRotation*Quaternion.Euler(0,Mathf.Sin(Time.time*.7f)*1.3f,Mathf.Sin(Time.time*1.1f)*.55f); }
  public bool ReceiveHit(RaycastHit hit,int amount,bool scores=true) {
   if(amount<=0||session==null)return false;
   if(scores) {if(!session.TryRecordHit())return false;} else if(!session.GameplayEnabled)return false;
   Damage.Apply(amount);if(scores&&sfx!=null)sfx.Play(Sfx.Hit);
   if(scores&&animator!=null&&Time.time>=nextReaction) {animator.SetTrigger(HitTrigger);nextReaction=Time.time+.5f;}
   var mark=GameObject.CreatePrimitive(PrimitiveType.Sphere);mark.name="Bullet impact";Destroy(mark.GetComponent<Collider>());mark.transform.position=hit.point+hit.normal*.006f;mark.transform.rotation=Quaternion.FromToRotation(Vector3.forward,hit.normal);mark.transform.localScale=new Vector3(.07f,.07f,.009f);mark.GetComponent<Renderer>().sharedMaterial=impactMaterial;mark.transform.SetParent(hit.collider.transform,true);impacts.Enqueue(mark);if(impacts.Count>60)Destroy(impacts.Dequeue());
   return true;
  }
  public void ResetDamage() {Damage.Reset();while(impacts.Count>0)Destroy(impacts.Dequeue());}
 }
}
