using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace FieldOps.Tests {
 // The sights must sit on the gun, line up with the screen centre when aiming, and show at the hip.
 public class WeaponSightsTests {
  static readonly Vector3 HolderRest=new Vector3(.28f,-.24f,.46f);   // Weapon holder, relative to the camera
  static Rifle OpenRifle() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);return Object.FindFirstObjectByType<Rifle>(); }
  struct Tri { public Vector3 a,b,c; }
  static List<Tri> Triangles(Transform wrapper,Vector3 holderOrigin) {
   var list=new List<Tri>();var ours=wrapper.Find("Sights");
   foreach(var mf in wrapper.GetComponentsInChildren<MeshFilter>(true)) {
    if(ours!=null&&mf.transform.IsChildOf(ours))continue;   // only the original model
    var mesh=mf.sharedMesh;if(mesh==null)continue;var v=mesh.vertices;var t=mesh.triangles;
    for(int i=0;i<t.Length;i+=3)list.Add(new Tri{a=mf.transform.TransformPoint(v[t[i]])-holderOrigin,b=mf.transform.TransformPoint(v[t[i+1]])-holderOrigin,c=mf.transform.TransformPoint(v[t[i+2]])-holderOrigin});
   }
   return list;
  }
  static float TopAt(List<Tri> tris,float x,float z) {
   float best=float.NegativeInfinity;
   foreach(var t in tris) {
    float d=(t.b.z-t.c.z)*(t.a.x-t.c.x)+(t.c.x-t.b.x)*(t.a.z-t.c.z);if(Mathf.Abs(d)<1e-9f)continue;
    float l1=((t.b.z-t.c.z)*(x-t.c.x)+(t.c.x-t.b.x)*(z-t.c.z))/d,l2=((t.c.z-t.a.z)*(x-t.c.x)+(t.a.x-t.c.x)*(z-t.c.z))/d,l3=1-l1-l2;
    if(l1<-1e-4f||l2<-1e-4f||l3<-1e-4f)continue;float y=l1*t.a.y+l2*t.b.y+l3*t.c.y;if(y>best)best=y;
   }
   return best;
  }
  static readonly string[] IronSightWeapons={"weaponPistol","weaponRifle","weaponSmg","weaponShotgun"};

  [Test] public void TheSightLineOfEveryWeaponPassesThroughTheCameraAxisWhenAiming() {
   var rifle=OpenRifle();
   foreach(var w in rifle.weapons) {
    var rear=HolderRest+w.aimOffset+w.sightRear;var front=HolderRest+w.aimOffset+w.sightFront;
    Assert.AreEqual(0f,rear.x,.01f,w.nameKey+" rear x");Assert.AreEqual(0f,rear.y,.01f,w.nameKey+" rear y");Assert.AreEqual(0f,front.x,.01f,w.nameKey+" front x");Assert.AreEqual(0f,front.y,.01f,w.nameKey+" front y");
    Assert.GreaterOrEqual(front.z-rear.z,.12f,w.nameKey+": the front point is ahead of the rear point.");
   }
  }
  [Test] public void TheRearSightIsAComfortableDistanceFromTheEyeWhenAiming() { var rifle=OpenRifle();foreach(var w in rifle.weapons) {float d=HolderRest.z+w.aimOffset.z+w.sightRear.z;Assert.That(d,Is.InRange(.08f,.45f),w.nameKey+" rear sight distance "+d);} }
  [Test] public void AtTheHipTheRearSightIsInFrontOfTheCameraAndInsideTheView() {
   var rifle=OpenRifle();foreach(var w in rifle.weapons) {var p=HolderRest+w.sightRear;Assert.GreaterOrEqual(p.z,.15f,w.nameKey+" is behind or too close to the lens at the hip.");Assert.LessOrEqual(Mathf.Abs(p.x)/p.z,Mathf.Tan(50*Mathf.Deg2Rad),w.nameKey+" is outside the horizontal view at the hip.");}
  }
  [Test] public void TheFourGunsWithoutAScopeGetTheirOwnSightsGroupedUnderTheModel() {
   var rifle=OpenRifle();for(int i=0;i<rifle.models.Length;i++) {
    var sights=rifle.models[i].Find("Sights");
    if(!IronSightWeapons.Contains(rifle.weapons[i].nameKey)){Assert.IsNull(sights,rifle.weapons[i].nameKey+": the sniper keeps its own scope and gets no extra iron sights.");continue;}
    Assert.IsNotNull(sights,rifle.weapons[i].nameKey);var names=sights.GetComponentsInChildren<Transform>(true).Select(t=>t.name).ToArray();
    Assert.IsTrue(names.Any(n=>n.Contains("Front")),rifle.weapons[i].nameKey+" front sight");
   }
  }
  [Test] public void EverySightPartIsInertAndCheap() {
   var rifle=OpenRifle();foreach(var wrapper in rifle.models) {var sights=wrapper.Find("Sights");if(sights==null)continue;
    foreach(var r in sights.GetComponentsInChildren<Renderer>(true)){Assert.IsNull(r.GetComponent<Collider>(),r.name+" has no collider.");Assert.AreEqual(2,r.gameObject.layer,r.name+" is on Ignore Raycast.");Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off,r.shadowCastingMode,r.name);Assert.IsFalse(r.receiveShadows);}
    Assert.LessOrEqual(sights.GetComponentsInChildren<Renderer>(true).Length,24,"A handful of simple shapes per gun.");}
  }
  [Test] public void EverySightPartSitsOnTheGunWithNoGapUnderIt() {
   var rifle=OpenRifle();var holder=GameObject.Find("Weapon holder").transform;
   foreach(var wrapper in rifle.models) {
    var sights=wrapper.Find("Sights");if(sights==null)continue;var tris=Triangles(wrapper,holder.position);
    foreach(var part in sights.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.Contains("base")||r.name.Contains("Base"))) {
     float bottom=part.bounds.min.y-holder.position.y,top=TopAt(tris,0,part.bounds.center.z-holder.position.z);
     Assert.IsFalse(float.IsNegativeInfinity(top),part.name+": there is gun under it.");Assert.That(bottom,Is.InRange(top-.02f,top+.004f),wrapper.name+"/"+part.name+" floats "+(bottom-top)+" m from the surface.");
    }
   }
  }
  [Test] public void TheSniperSightLineIsTheScopeAxisOfItsOwnModel() {
   var rifle=OpenRifle();int i=System.Array.FindIndex(rifle.weapons,w=>w.nameKey=="weaponSniper");var w2=rifle.weapons[i];var holder=GameObject.Find("Weapon holder").transform;
   var pts=new List<Vector3>();foreach(var mf in rifle.models[i].GetComponentsInChildren<MeshFilter>(true)){var v=mf.sharedMesh.vertices;foreach(var p in v)pts.Add(mf.transform.TransformPoint(p)-holder.position);}
   float frontZ=pts.Where(p=>p.y>.07f&&p.z<w2.sightFront.z+.2f).Max(p=>p.z);var bell=pts.Where(p=>p.y>.06f&&p.z>frontZ-.08f&&p.z<=frontZ+.001f).ToList();float axisY=(bell.Min(p=>p.y)+bell.Max(p=>p.y))*.5f;
   float axisX=(bell.Min(p=>p.x)+bell.Max(p=>p.x))*.5f;Assert.AreEqual(axisX,w2.sightFront.x,.005f,"The scope sits off the middle of this model, so the sight line must follow it sideways too.");Assert.AreEqual(w2.sightRear.x,w2.sightFront.x,.003f,"The scope axis is straight.");
   Assert.AreEqual(axisY,w2.sightFront.y,.01f,"The front point is the middle of the objective lens.");Assert.AreEqual(w2.sightRear.y,w2.sightFront.y,.003f,"The scope axis is level.");
  }
  [Test] public void EveryWeaponHasADistinctAccentColour() {
   var rifle=OpenRifle();var colours=new List<Color>();foreach(var wrapper in rifle.models) {var sights=wrapper.Find("Sights");if(sights==null)continue;var accent=sights.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r=>r.name.Contains("Accent"));Assert.IsNotNull(accent,wrapper.name+" has an accent part.");colours.Add(accent.sharedMaterial.color);}
   Assert.AreEqual(4,colours.Count);for(int a=0;a<colours.Count;a++)for(int b=a+1;b<colours.Count;b++)Assert.Greater(Mathf.Abs(colours[a].r-colours[b].r)+Mathf.Abs(colours[a].g-colours[b].g)+Mathf.Abs(colours[a].b-colours[b].b),.35f,"Accent colours "+a+" and "+b+" are too similar.");
  }
  [Test] public void TheFrontSightNeverRisesAboveTheSightLineAndItsPostEndsOnIt() {
   var rifle=OpenRifle();var holder=GameObject.Find("Weapon holder").transform;
   for(int i=0;i<rifle.models.Length;i++) {
    var w=rifle.weapons[i];var sights=rifle.models[i].Find("Sights");if(sights==null)continue;var parts=sights.GetComponentsInChildren<Renderer>(true);
    foreach(var part in parts)Assert.Greater(part.transform.localScale.y,0f,w.nameKey+"/"+part.name+" has a negative height (its post is upside down).");
    float Top(Renderer r)=>r.bounds.max.y-holder.position.y;float Bottom(Renderer r)=>r.bounds.min.y-holder.position.y;
    var front=parts.Where(r=>r.name.StartsWith("Front sight")||r.name.StartsWith("Accent front")).ToList();Assert.IsTrue(front.Count>0,w.nameKey);
    Assert.LessOrEqual(front.Max(Top),w.sightFront.y+.004f,w.nameKey+": a front part rises above the sight line, so the eye aims too high.");
    Assert.AreEqual(w.sightFront.y,front.Max(Top),.004f,w.nameKey+": the front post ends on the sight line.");
    var rearBase=parts.FirstOrDefault(r=>r.name=="Rear sight base");if(rearBase==null)continue;Assert.LessOrEqual(Top(rearBase),w.sightRear.y,w.nameKey+": the rear aperture opens above its riser.");
    var bar=parts.First(r=>r.name.Contains("sight bar")||r.name.Contains("rear bar"));Assert.GreaterOrEqual(Bottom(bar),w.sightRear.y-.001f,w.nameKey+": the rear bar is above the sight line.");
   }
  }
  [Test] public void TheSightsStayOnTheReceiverAndBarrelAfterTheHipShift() {
   // The long guns sit further forward at the hip; their sights must move with the model, not stay at fixed holder coordinates.
   var rifle=OpenRifle();var holder=GameObject.Find("Weapon holder").transform;
   foreach(var name in new[]{"weaponRifle","weaponShotgun"}) {
    int i=System.Array.FindIndex(rifle.weapons,x=>x.nameKey==name);var w=rifle.weapons[i];var wrapper=rifle.models[i];var tris=Triangles(wrapper,holder.position);
    var model=wrapper.GetComponentsInChildren<MeshFilter>(true).First(m=>m.transform.IsChildOf(wrapper)&&(wrapper.Find("Sights")==null||!m.transform.IsChildOf(wrapper.Find("Sights"))));
    var b=model.GetComponent<Renderer>().bounds;float modelCentreZ=b.center.z-holder.position.z;
    float rearInModel=w.sightRear.z-modelCentreZ,frontInModel=w.sightFront.z-modelCentreZ;
    if(name=="weaponRifle"){Assert.AreEqual(-.12f,rearInModel,.01f,"The rear sight sits over the receiver.");Assert.AreEqual(.40f,frontInModel,.01f,"The front sight sits over the muzzle end.");}
    else{Assert.AreEqual(-.16f,rearInModel,.01f,"The ghost ring sits over the receiver.");Assert.AreEqual(.50f,frontInModel,.01f,"The bead sits over the muzzle end.");}
   }
  }
  [Test] public void TheAssaultRifleAndTheShotgunHaveNoSquareRearFrameWhilePistolAndSmgKeepTheirRearSight() {
   var rifle=OpenRifle();
   for(int i=0;i<rifle.models.Length;i++) {
    var key=rifle.weapons[i].nameKey;var sights=rifle.models[i].Find("Sights");if(sights==null)continue;var names=sights.GetComponentsInChildren<Renderer>(true).Select(r=>r.name).ToArray();
    bool hasRear=names.Any(n=>n.Contains("Rear")||n.Contains("rear"));
    if(key=="weaponRifle"||key=="weaponShotgun")Assert.IsFalse(hasRear,key+": no boxy rear frame; only the front sight remains. Found: "+string.Join(", ",names.Where(n=>n.ToLower().Contains("rear"))));
    else Assert.IsTrue(hasRear,key+" keeps its rear sight.");
    Assert.IsTrue(names.Any(n=>n.Contains("Front")),key+" still has its front sight.");
   }
  }
  // Two parts with a face in the same plane facing the same way "fight" for the pixel: the nearer-looking one flickers or vanishes.
  [Test] public void NoTwoSightPartsShareAFaceInTheSamePlane() {
   var rifle=OpenRifle();var problems=new List<string>();
   for(int w=0;w<rifle.models.Length;w++) {
    var sights=rifle.models[w].Find("Sights");if(sights==null)continue;var rs=sights.GetComponentsInChildren<Renderer>(true).ToArray();
    for(int i=0;i<rs.Length;i++)for(int j=i+1;j<rs.Length;j++) {
     var a=rs[i].bounds;var b=rs[j].bounds;
     for(int k=0;k<3;k++) {
      int o1=(k+1)%3,o2=(k+2)%3;float ov1=Mathf.Min(a.max[o1],b.max[o1])-Mathf.Max(a.min[o1],b.min[o1]),ov2=Mathf.Min(a.max[o2],b.max[o2])-Mathf.Max(a.min[o2],b.min[o2]);
      if(ov1<=.0002f||ov2<=.0002f)continue;   // the faces do not overlap on screen
      if(Mathf.Abs(a.min[k]-b.min[k])<.0002f)problems.Add(rifle.weapons[w].nameKey+": "+rs[i].name+" / "+rs[j].name+" share a "+"xyz"[k]+"-min face");
      if(Mathf.Abs(a.max[k]-b.max[k])<.0002f)problems.Add(rifle.weapons[w].nameKey+": "+rs[i].name+" / "+rs[j].name+" share a "+"xyz"[k]+"-max face");
     }
    }
   }
   Assert.IsEmpty(problems,"Coplanar faces flicker; make one part slightly larger than the other: "+string.Join(" | ",problems));
  }
  [Test] public void TheFloatingRedDotIsGone() { OpenRifle();var hud=GameObject.Find("HUD").transform;Assert.IsNull(hud.Find("Aim dot"),"The red dot is replaced by real sights on the gun."); }
  [Test] public void SightPointMarkersExistForEveryWeaponSoTheAlignmentCanBeChecked() { var rifle=OpenRifle();foreach(var wrapper in rifle.models){Assert.IsNotNull(wrapper.Find("Sight rear point"),wrapper.name);Assert.IsNotNull(wrapper.Find("Sight front point"),wrapper.name);} }
 }
}
