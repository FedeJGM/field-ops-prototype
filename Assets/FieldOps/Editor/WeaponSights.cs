using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace FieldOps.Editor {
 // Builds the iron sights of the weapons from simple shapes, resting on the real surface of each model, and finds the sight line
 // (rear aperture or eyepiece, front post or objective) the aim pose lines up with the screen centre.
 // The sniper already has a scope on its model, so its sight line is the axis of that scope and nothing is added.
 static class WeaponSights {
  public struct Result { public Vector3 rear,front; public float eyeDistance; }
  struct Tri { public Vector3 a,b,c; }
  const float Embed=.003f;   // parts sink a little into the gun so there is never a gap under them

  static List<Tri> Triangles(Transform wrapper,Vector3 origin) {
   var list=new List<Tri>();var ours=wrapper.Find("Sights");
   foreach(var mf in wrapper.GetComponentsInChildren<MeshFilter>(true)) {
    if(ours!=null&&mf.transform.IsChildOf(ours))continue;var mesh=mf.sharedMesh;if(mesh==null)continue;var v=mesh.vertices;var t=mesh.triangles;
    for(int i=0;i<t.Length;i+=3)list.Add(new Tri{a=mf.transform.TransformPoint(v[t[i]])-origin,b=mf.transform.TransformPoint(v[t[i+1]])-origin,c=mf.transform.TransformPoint(v[t[i+2]])-origin});
   }
   return list;
  }
  // Highest surface point above the column (x,z), by ray-triangle intersection (the models have large faces and few vertices).
  static float TopAt(List<Tri> tris,float x,float z) {
   float best=float.NegativeInfinity;
   foreach(var t in tris) {
    float d=(t.b.z-t.c.z)*(t.a.x-t.c.x)+(t.c.x-t.b.x)*(t.a.z-t.c.z);if(Mathf.Abs(d)<1e-9f)continue;
    float l1=((t.b.z-t.c.z)*(x-t.c.x)+(t.c.x-t.b.x)*(z-t.c.z))/d,l2=((t.c.z-t.a.z)*(x-t.c.x)+(t.a.x-t.c.x)*(z-t.c.z))/d,l3=1-l1-l2;
    if(l1<-1e-4f||l2<-1e-4f||l3<-1e-4f)continue;float y=l1*t.a.y+l2*t.b.y+l3*t.c.y;if(y>best)best=y;
   }
   return best;
  }
  // Lowest and highest surface over a short stretch of the barrel or rail (a toothed rail is not flat).
  static void Span(List<Tri> tris,float z,float depth,out float floor,out float ceil) {
   floor=float.PositiveInfinity;ceil=float.NegativeInfinity;
   for(int i=0;i<=4;i++){float y=TopAt(tris,0,z-depth*.5f+depth*i/4f);if(float.IsNegativeInfinity(y))continue;floor=Mathf.Min(floor,y);ceil=Mathf.Max(ceil,y);}
   if(float.IsPositiveInfinity(floor))throw new InvalidOperationException("No gun surface under the sight at z="+z);
  }

  static GameObject Part(Transform root,string name,PrimitiveType type,Vector3 center,Vector3 size,Material material) {
   var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=center;go.transform.localScale=size;
   UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.layer=2;var r=go.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;
  }
  // A block that stands on the gun at (0,z) and reaches up to topY.
  static void Stand(Transform root,List<Tri> tris,string name,float z,float width,float depth,float topY,Material material) {
   Span(tris,z,depth,out var floor,out _);float bottom=floor-Embed;float height=topY-bottom;
   Part(root,name,PrimitiveType.Cube,new Vector3(0,bottom+height*.5f,z),new Vector3(width,height,depth),material);
  }
  static void Box(Transform root,string name,Vector3 center,Vector3 size,Material material) { Part(root,name,PrimitiveType.Cube,center,size,material); }
  static void Marker(Transform wrapper,string name,Vector3 position) { var t=new GameObject(name).transform;t.SetParent(wrapper,false);t.localPosition=position; }

  // An open aperture: two uprights and a top bar over a riser; its opening is centred at sightY.
  static void Aperture(Transform root,string label,float z,float riserTop,float openHeight,float halfWidth,float thickness,Material dark,Material accent,float sightY) {
   float postTop=riserTop+openHeight;
   foreach(float side in new[]{-1f,1f})Box(root,"Rear sight "+label+(side<0?" left":" right"),new Vector3(side*halfWidth,riserTop+openHeight*.5f,z),new Vector3(thickness,openHeight+.001f,thickness*1.6f),dark);
   Box(root,(accent==dark?"Rear sight bar ":"Accent rear bar ")+label,new Vector3(0,postTop+thickness*.5f,z),new Vector3(halfWidth*2+thickness+.001f,thickness,thickness*1.6f+.001f),accent);   // a hair larger than the uprights so no faces share a plane
   if(Mathf.Abs(sightY-(riserTop+openHeight*.5f))>.002f)throw new InvalidOperationException("Aperture is not centred on the sight line.");
  }

  // holdZ is how far the model was moved forward in the holder; the sight positions below are measured on the model itself.
  public static Result Build(string key,Transform wrapper,Transform holder,Func<string,Color,Material> material,float holdZ) {
   var old=wrapper.Find("Sights");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var tris=Triangles(wrapper,holder.position);var root=new GameObject("Sights").transform;root.SetParent(wrapper,false);
   var dark=material("SightDark",new Color(.05f,.05f,.06f));var white=material("SightWhite",new Color(.95f,.95f,.92f));
   Result r;
   switch(key) {
    case "weaponPistol": r=Pistol(root,tris,dark,white,holdZ);break;
    case "weaponRifle": r=Rifle(root,tris,dark,material("SightOrange",new Color(1f,.55f,.12f)),holdZ);break;
    case "weaponSmg": r=Smg(root,tris,dark,material("SightCyan",new Color(.2f,.85f,1f)),holdZ);break;
    case "weaponShotgun": r=Shotgun(root,tris,dark,material("SightBrass",new Color(.82f,.68f,.28f)),holdZ);break;
    case "weaponSniper": UnityEngine.Object.DestroyImmediate(root.gameObject);r=Scope(wrapper,holder);break;
    default: throw new ArgumentException("No sights defined for "+key);
   }
   Marker(wrapper,"Sight rear point",r.rear);Marker(wrapper,"Sight front point",r.front);return r;
  }

  static Result Pistol(Transform root,List<Tri> tris,Material dark,Material white,float holdZ) {
   float zr=holdZ-.10f,zf=holdZ+.10f;Span(tris,zr,.012f,out _,out float ceilR);Span(tris,zf,.006f,out _,out _);
   float riserTop=ceilR+.003f,open=.010f,sightY=riserTop+open*.5f;
   Stand(root,tris,"Rear sight base",zr,.016f,.012f,riserTop,dark);
   Aperture(root,"notch",zr,riserTop,open,.0065f,.004f,dark,dark,sightY);
   foreach(float side in new[]{-1f,1f})Box(root,"Accent rear dot "+(side<0?"left":"right"),new Vector3(side*.0065f,sightY,zr-.0066f),new Vector3(.003f,.003f,.001f),white);
   Stand(root,tris,"Front sight base",zf,.004f,.006f,sightY,dark);
   Box(root,"Accent front dot",new Vector3(0,sightY-.002f,zf-.0035f),new Vector3(.003f,.003f,.001f),white);
   return new Result{rear=new Vector3(0,sightY,zr),front=new Vector3(0,sightY,zf),eyeDistance=.30f};
  }
  static Result Rifle(Transform root,List<Tri> tris,Material dark,Material accent,float holdZ) {
   float zr=holdZ-.12f,zf=holdZ+.40f;Span(tris,zr,.02f,out _,out float ceilR);Span(tris,zf,.014f,out _,out float ceilF);
   float riserTop=ceilR+.009f,open=.018f,sightY=riserTop+open*.5f;
   // No rear frame on this gun: only the front sight remains; the rear point is just where the sight line starts.
   float towerTop=ceilF+.009f;Stand(root,tris,"Front sight base",zf,.012f,.014f,towerTop,dark);
   Box(root,"Front sight post",new Vector3(0,(towerTop+sightY)*.5f,zf),new Vector3(.004f,sightY-towerTop,.006f),dark);
   foreach(float side in new[]{-1f,1f})Box(root,"Front sight guard "+(side<0?"left":"right"),new Vector3(side*.0065f,towerTop+.013f,zf),new Vector3(.003f,.026f,.008f),dark);
   Box(root,"Accent front tip",new Vector3(0,sightY-.0025f,zf),new Vector3(.005f,.007f,.0072f),accent);   // wraps the top of the post, a hair wider, deeper and taller
   return new Result{rear=new Vector3(0,sightY,zr),front=new Vector3(0,sightY,zf),eyeDistance=.34f};
  }
  static Result Smg(Transform root,List<Tri> tris,Material dark,Material accent,float holdZ) {
   float zr=holdZ-.18f,zf=holdZ+.14f;Span(tris,zr,.014f,out _,out float ceilR);Span(tris,zf,.012f,out _,out float ceilF);
   float riserTop=ceilR+.008f,open=.014f,sightY=riserTop+open*.5f;
   Stand(root,tris,"Rear sight base",zr,.018f,.014f,riserTop,dark);
   Aperture(root,"ring",zr,riserTop,open,.008f,.003f,dark,accent,sightY);
   float baseTop=ceilF+.006f;Stand(root,tris,"Front sight base",zf,.010f,.012f,baseTop,dark);
   Box(root,"Front sight post",new Vector3(0,(baseTop+sightY)*.5f,zf),new Vector3(.003f,sightY-baseTop,.005f),dark);
   Box(root,"Accent front tip",new Vector3(0,sightY-.0015f,zf),new Vector3(.004f,.005f,.006f),accent);
   return new Result{rear=new Vector3(0,sightY,zr),front=new Vector3(0,sightY,zf),eyeDistance=.30f};
  }
  static Result Shotgun(Transform root,List<Tri> tris,Material dark,Material brass,float holdZ) {
   float zr=holdZ-.16f,zf=holdZ+.50f;Span(tris,zr,.012f,out _,out float ceilR);Span(tris,zf,.010f,out _,out float ceilF);
   float riserTop=ceilR+.006f,open=.016f,sightY=riserTop+open*.5f;
   // No rear frame on this gun: only the front bead remains; the rear point is just where the sight line starts.
   float baseTop=ceilF+.004f;Stand(root,tris,"Front sight base",zf,.008f,.010f,baseTop,dark);
   Box(root,"Front sight post",new Vector3(0,(baseTop+sightY-.003f)*.5f,zf),new Vector3(.003f,sightY-.003f-baseTop,.004f),dark);
   Part(root,"Accent front bead",PrimitiveType.Sphere,new Vector3(0,sightY-.002f,zf),new Vector3(.007f,.007f,.007f),brass);
   return new Result{rear=new Vector3(0,sightY,zr),front=new Vector3(0,sightY,zf),eyeDistance=.36f};
  }
  // The sniper model carries its own scope; its sight line is the scope axis, from the eyepiece to the middle of the objective lens.
  static Result Scope(Transform wrapper,Transform holder) {
   var pts=new List<Vector3>();foreach(var mf in wrapper.GetComponentsInChildren<MeshFilter>(true)){var mesh=mf.sharedMesh;if(mesh==null)continue;foreach(var p in mesh.vertices)pts.Add(mf.transform.TransformPoint(p)-holder.position);}
   var scope=pts.Where(p=>p.y>.078f).ToList();if(scope.Count==0)throw new InvalidOperationException("The sniper model has no scope above the receiver.");
   float frontZ=scope.Max(p=>p.z),rearZ=scope.Min(p=>p.z);var bell=pts.Where(p=>p.y>.06f&&p.z>frontZ-.08f&&p.z<=frontZ+.001f).ToList();
   float axisY=(bell.Min(p=>p.y)+bell.Max(p=>p.y))*.5f;
   float axisX=(bell.Min(p=>p.x)+bell.Max(p=>p.x))*.5f;   // this model carries its scope a little off the middle line
   return new Result{rear=new Vector3(axisX,axisY,rearZ),front=new Vector3(axisX,axisY,frontZ),eyeDistance=.12f};
  }
 }
}
