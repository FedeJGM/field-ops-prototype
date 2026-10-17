using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace FieldOps.Editor {
 // Military training-base scenery made of simple shapes and the credited CC0 textures (see CREDITS.md).
 // The firing corridor (|x| < 3 from the player start to the target) is kept clear, and the 44 x 56 m field walls stay where the tests expect them.
 static class EnvironmentBuilder {
  const string TextureFolder="Assets/FieldOps/ThirdParty/PolyHaven/";
  const string MaterialFolder="Assets/FieldOps/Materials/";
  static readonly Dictionary<string,Material> cache=new Dictionary<string,Material>();

  // ---- textures and materials
  // Textures are capped at 1024 px and compressed with ASTC on Android so the scenery stays light on the phone.
  static Texture2D LoadTexture(string file,bool normalMap=false,bool sky=false) {
   var path=TextureFolder+file;var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   if(importer==null)throw new FileNotFoundException("Missing texture "+path);
   var android=importer.GetPlatformTextureSettings("Android");
   int cap=sky?2048:1024;   // surfaces are 1K; the sky panorama is 2K so distant trees stay sharp on the phone
   bool needsType=normalMap&&importer.textureType!=TextureImporterType.NormalMap;
   if(!android.overridden||android.maxTextureSize!=cap||android.format!=TextureImporterFormat.ASTC_6x6||importer.maxTextureSize!=cap||needsType||(sky&&(importer.wrapModeU!=TextureWrapMode.Repeat||importer.mipmapEnabled))) {
    importer.maxTextureSize=cap;if(normalMap)importer.textureType=TextureImporterType.NormalMap;
    // The panorama is only ever magnified; mipmaps make the texture filter jump where the image wraps, which shows as a vertical line in the sky.
    if(sky){importer.wrapModeU=TextureWrapMode.Repeat;importer.wrapModeV=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;}
    android.name="Android";android.overridden=true;android.maxTextureSize=cap;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
   }
   return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  static Material LoadOrCreate(string name,string shader) {
   if(cache.TryGetValue(name,out var cached))return cached;
   var path=MaterialFolder+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
   m.shader=Shader.Find(shader);cache[name]=m;return m;
  }
  static Material Surface(string name,string diffuse,string normal,Color tint,Vector2 tiling,float smoothness=.12f) {
   var m=LoadOrCreate(name,"Standard");m.color=tint;m.SetTexture("_MainTex",LoadTexture(diffuse+"_diff_1k.jpg"));m.mainTextureScale=tiling;m.mainTextureOffset=Vector2.zero;m.SetFloat("_Glossiness",smoothness);m.SetFloat("_Metallic",0);
   if(normal!=null){m.SetTexture("_BumpMap",LoadTexture(normal+"_nor_gl_1k.jpg",true));m.SetFloat("_BumpScale",1f);m.EnableKeyword("_NORMALMAP");}
   else{m.SetTexture("_BumpMap",null);m.DisableKeyword("_NORMALMAP");}
   EditorUtility.SetDirty(m);return m;
  }
  static Material Flat(string name,Color color,float smoothness=.1f) {var m=LoadOrCreate(name,"Standard");m.color=color;m.SetFloat("_Glossiness",smoothness);EditorUtility.SetDirty(m);return m;}

  // ---- shapes
  static GameObject Prop(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Transform parent,float yaw=0,bool collider=true) {
   var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localRotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=scale;
   go.GetComponent<Renderer>().sharedMaterial=material;if(!collider)Object.DestroyImmediate(go.GetComponent<Collider>());
   GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);return go;
  }
  static Transform Group(string name,Transform parent,Vector3 position) {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}

  // ---- sky, light and fog: an overcast open field, sober like a real training ground
  public static void CreateAtmosphere() {
   var sky=LoadOrCreate("EnvSky","Skybox/Panoramic");sky.SetTexture("_MainTex",LoadTexture("belfast_open_field_sky_2k.jpg",false,true));
   sky.SetFloat("_Mapping",1);sky.SetFloat("_ImageType",0);sky.SetFloat("_Rotation",200);sky.SetFloat("_Exposure",1f);sky.SetColor("_Tint",new Color(.5f,.5f,.5f,.5f));sky.EnableKeyword("_MAPPING_LATITUDE_LONGITUDE_LAYOUT");EditorUtility.SetDirty(sky);
   RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.6f,.64f,.7f);RenderSettings.ambientEquatorColor=new Color(.48f,.48f,.46f);RenderSettings.ambientGroundColor=new Color(.28f,.26f,.23f);
   RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.62f,.64f,.64f);RenderSettings.fogStartDistance=32;RenderSettings.fogEndDistance=120;
   var sun=new GameObject("Overcast sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1f,.96f,.88f);sun.intensity=1f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-32,0);
  }

  public static void Build(Transform world) {
   cache.Clear();
   // surfaces
   var dirt=Surface("EnvDirt","brown_mud_dry",null,new Color(.92f,.9f,.86f),new Vector2(11,14),.04f);
   var asphalt=Surface("EnvAsphalt","asphalt_02",null,Color.white,new Vector2(1.25f,6),.15f);
   var pad=Surface("EnvPad","brushed_concrete_03","brushed_concrete_03",new Color(.95f,.95f,.95f),new Vector2(2.25f,1.25f),.1f);
   var wallEW=Surface("EnvWallEW","brushed_concrete_03","brushed_concrete_03",Color.white,new Vector2(14,.5f),.1f);
   var wallNS=Surface("EnvWallNS","brushed_concrete_03","brushed_concrete_03",Color.white,new Vector2(11,.5f),.1f);
   var backstopMat=Surface("EnvBackstop","brushed_concrete_03","brushed_concrete_03",new Color(.9f,.9f,.9f),new Vector2(5,.75f),.1f);
   var barrierMat=Surface("EnvBarrier","brushed_concrete_03","brushed_concrete_03",new Color(.95f,.95f,.95f),new Vector2(.8f,.3f),.1f);
   var containers=new[]{Surface("EnvContainerGreen","container_side","container_side",Color.white,new Vector2(2,1),.18f),Surface("EnvContainerGray","container_side","container_side",new Color(.78f,.86f,.74f),new Vector2(2,1),.18f),Surface("EnvContainerBrown","container_side","container_side",new Color(1f,.8f,.62f),new Vector2(2,1),.18f)};
   var sandbag=Surface("EnvSandbag","hessian_230","hessian_230",new Color(.86f,.8f,.65f),new Vector2(3,1),.03f);
   var rustGreen=Surface("EnvRustGreen","green_metal_rust",null,Color.white,new Vector2(1,1),.2f);
   var rustRed=Surface("EnvRustRed","green_metal_rust",null,new Color(1.25f,.72f,.5f),new Vector2(1,1),.2f);
   var rustDark=Surface("EnvRustDark","green_metal_rust",null,new Color(.45f,.5f,.45f),new Vector2(1,1),.2f);
   var iron=Surface("EnvIron","corrugated_iron_03",null,new Color(.9f,.95f,.9f),new Vector2(4,1),.25f);
   var rubber=Flat("EnvRubber",new Color(.05f,.05f,.055f),.05f);var paint=Flat("EnvPaint",new Color(.92f,.82f,.35f),.1f);var darkMetal=Flat("EnvDarkMetal",new Color(.1f,.11f,.11f),.2f);var canopy=Flat("EnvCanopy",new Color(.18f,.24f,.14f),.02f);var bark=Flat("EnvBark",new Color(.16f,.12f,.09f),.02f);

   // ground, firing lane and the pad where the player starts
   Prop("Ground",PrimitiveType.Cube,new Vector3(0,-.15f,0),new Vector3(44,.3f,56),dirt,world);
   Prop("Firing lane",PrimitiveType.Cube,new Vector3(0,.006f,-1),new Vector3(5,.012f,24),asphalt,world);
   Prop("Firing line pad",PrimitiveType.Cube,new Vector3(0,.004f,-12),new Vector3(9,.008f,5),pad,world);
   for(int i=0;i<6;i++)Prop("Lane marking",PrimitiveType.Cube,new Vector3(0,.015f,-9+i*3.4f),new Vector3(.12f,.02f,1.4f),paint,world,0,false);
   Prop("Target platform",PrimitiveType.Cube,new Vector3(0,.015f,2),new Vector3(2,.03f,2),darkMetal,world);

   // backstop with sandbag flanks, and the four field walls
   Prop("Backstop",PrimitiveType.Cube,new Vector3(0,1.5f,12),new Vector3(20,3,.7f),backstopMat,world);
   Prop("Sandbag wall backstop left",PrimitiveType.Cube,new Vector3(-7,.45f,10.7f),new Vector3(5,.9f,.8f),sandbag,world);Prop("Sandbag wall backstop right",PrimitiveType.Cube,new Vector3(7,.45f,10.7f),new Vector3(5,.9f,.8f),sandbag,world);
   Prop("Boundary west",PrimitiveType.Cube,new Vector3(-22,2,0),new Vector3(.5f,4,56),wallEW,world);Prop("Boundary east",PrimitiveType.Cube,new Vector3(22,2,0),new Vector3(.5f,4,56),wallEW,world);
   Prop("Boundary south",PrimitiveType.Cube,new Vector3(0,2,-28),new Vector3(44,4,.5f),wallNS,world);Prop("Boundary north",PrimitiveType.Cube,new Vector3(0,2,28),new Vector3(44,4,.5f),wallNS,world);

   // sandbag positions around the firing line and the target
   Prop("Sandbag wall firing left",PrimitiveType.Cube,new Vector3(-5.5f,.45f,-9.5f),new Vector3(4,.9f,.8f),sandbag,world);Prop("Sandbag wall firing right",PrimitiveType.Cube,new Vector3(5.5f,.45f,-9.5f),new Vector3(4,.9f,.8f),sandbag,world);
   Prop("Sandbag wall firing left top",PrimitiveType.Cube,new Vector3(-5.5f,1.1f,-9.5f),new Vector3(2.6f,.45f,.7f),sandbag,world);
   Prop("Sandbag wall target left",PrimitiveType.Cube,new Vector3(-6,.45f,6),new Vector3(3,.9f,.8f),sandbag,world,25);Prop("Sandbag wall target right",PrimitiveType.Cube,new Vector3(6,.45f,6),new Vector3(3,.9f,.8f),sandbag,world,-25);

   // shipping containers along both sides (some stacked)
   float[] leftZ={-18,-11.6f,-5.2f,1.2f},rightZ={-16,-9.6f,-3.2f,3.2f};
   for(int i=0;i<4;i++) {
    Prop("Shipping container left "+i,PrimitiveType.Cube,new Vector3(-17,1.3f,leftZ[i]),new Vector3(2.4f,2.6f,6),containers[i%3],world);
    Prop("Shipping container right "+i,PrimitiveType.Cube,new Vector3(17,1.3f,rightZ[i]),new Vector3(2.4f,2.6f,6),containers[(i+1)%3],world);
   }
   Prop("Shipping container left stacked",PrimitiveType.Cube,new Vector3(-17,3.9f,-5.2f),new Vector3(2.4f,2.6f,6),containers[1],world);Prop("Shipping container right stacked",PrimitiveType.Cube,new Vector3(17,3.9f,3.2f),new Vector3(2.4f,2.6f,6),containers[2],world);

   // supply crates (painted metal) kept from the original layout, and concrete barriers / safety posts
   for(int i=0;i<9;i++) {
    float x=i%2==0?-8:9,z=-18+i*4.6f;Prop("Supply crate "+i,PrimitiveType.Cube,new Vector3(x,.7f,z),new Vector3(1.7f,1.4f,1.5f),rustGreen,world);
    Prop("Crate strap",PrimitiveType.Cube,new Vector3(x,.7f,z-.76f),new Vector3(.16f,1.45f,.025f),darkMetal,world,0,false);
    if(i%3==0)Prop("Stacked supply",PrimitiveType.Cube,new Vector3(x,1.95f,z),new Vector3(1.3f,1.1f,1.3f),rustGreen,world);
   }
   for(int i=0;i<5;i++) {Prop("Concrete barrier",PrimitiveType.Cube,new Vector3(-14+i*7,.6f,19),new Vector3(3,1.2f,.65f),barrierMat,world);Prop("Safety post",PrimitiveType.Cylinder,new Vector3(-4+i*2,.55f,7),new Vector3(.13f,.55f,.13f),paint,world);}
   Prop("Concrete barrier",PrimitiveType.Cube,new Vector3(-4.5f,.6f,-15.5f),new Vector3(3,1.2f,.65f),barrierMat,world);Prop("Concrete barrier",PrimitiveType.Cube,new Vector3(4.5f,.6f,-15.5f),new Vector3(3,1.2f,.65f),barrierMat,world);

   // oil drums and tire stacks
   Vector3[] drums={new Vector3(-13,.45f,-21),new Vector3(-12.2f,.45f,-21.3f),new Vector3(-12.6f,.45f,-20.2f),new Vector3(-13.5f,.45f,-20.1f),new Vector3(12.5f,.45f,10),new Vector3(13.3f,.45f,10.4f),new Vector3(12.9f,.45f,9.3f)};
   var drumMats=new[]{rustGreen,rustRed,rustDark};
   for(int i=0;i<drums.Length;i++)Prop("Oil drum "+i,PrimitiveType.Cylinder,drums[i],new Vector3(.58f,.45f,.58f),drumMats[i%3],world);
   Vector3[] tires={new Vector3(-6,0,-22),new Vector3(-4.3f,0,-21.4f),new Vector3(6,0,-22.2f)};
   for(int s=0;s<tires.Length;s++)for(int k=0;k<3;k++)Prop(k==0?"Tire stack "+s:"Tire",PrimitiveType.Cylinder,tires[s]+new Vector3(0,.12f+k*.24f,0),new Vector3(.9f,.12f,.9f),rubber,world);

   // watchtower behind the right-hand containers
   var tower=Group("Watchtower",world,new Vector3(15,0,18));
   foreach(var corner in new[]{new Vector2(-1.3f,-1.3f),new Vector2(1.3f,-1.3f),new Vector2(-1.3f,1.3f),new Vector2(1.3f,1.3f)})Prop("Tower leg",PrimitiveType.Cube,new Vector3(corner.x,2.2f,corner.y),new Vector3(.2f,4.4f,.2f),rustGreen,tower);
   Prop("Tower beam low",PrimitiveType.Cube,new Vector3(0,1.6f,-1.3f),new Vector3(2.6f,.12f,.12f),rustGreen,tower,0,false);Prop("Tower beam low back",PrimitiveType.Cube,new Vector3(0,1.6f,1.3f),new Vector3(2.6f,.12f,.12f),rustGreen,tower,0,false);
   Prop("Tower platform",PrimitiveType.Cube,new Vector3(0,4.5f,0),new Vector3(3.4f,.2f,3.4f),rustDark,tower);
   Prop("Tower rail front",PrimitiveType.Cube,new Vector3(0,5.05f,-1.65f),new Vector3(3.4f,.9f,.08f),rustGreen,tower,0,false);Prop("Tower rail left",PrimitiveType.Cube,new Vector3(-1.65f,5.05f,0),new Vector3(.08f,.9f,3.4f),rustGreen,tower,0,false);Prop("Tower rail right",PrimitiveType.Cube,new Vector3(1.65f,5.05f,0),new Vector3(.08f,.9f,3.4f),rustGreen,tower,0,false);
   Prop("Tower roof",PrimitiveType.Cube,new Vector3(0,6.5f,0),new Vector3(4.2f,.18f,4.2f),iron,tower,0,false);
   foreach(var corner in new[]{new Vector2(-1.7f,-1.7f),new Vector2(1.7f,-1.7f),new Vector2(-1.7f,1.7f),new Vector2(1.7f,1.7f)})Prop("Tower roof post",PrimitiveType.Cube,new Vector3(corner.x,5.5f,corner.y),new Vector3(.12f,2f,.12f),rustDark,tower,0,false);

   // open-fronted hangar behind the backstop on the left
   var hangar=Group("Hangar",world,new Vector3(-11,0,22));
   Prop("Hangar back wall",PrimitiveType.Cube,new Vector3(0,2.5f,4),new Vector3(14,5,.4f),iron,hangar);Prop("Hangar left wall",PrimitiveType.Cube,new Vector3(-7,2.5f,0),new Vector3(.4f,5,8),iron,hangar);Prop("Hangar right wall",PrimitiveType.Cube,new Vector3(7,2.5f,0),new Vector3(.4f,5,8),iron,hangar);
   Prop("Hangar roof",PrimitiveType.Cube,new Vector3(0,5.15f,0),new Vector3(14.8f,.3f,8.8f),rustDark,hangar,0,false);
   Prop("Hangar crate",PrimitiveType.Cube,new Vector3(-3,.7f,2.5f),new Vector3(1.7f,1.4f,1.5f),rustGreen,hangar);Prop("Hangar crate",PrimitiveType.Cube,new Vector3(-1.2f,.7f,2.7f),new Vector3(1.7f,1.4f,1.5f),rustGreen,hangar,15);

   // trees beyond the walls, seen over them as a green treeline
   for(int i=0;i<12;i++) {
    float x=(i%2==0?-1:1)*(26+(i%3)*3),z=-27+i*4.9f;
    Prop("Tree canopy",PrimitiveType.Sphere,new Vector3(x,4.8f,z),new Vector3(4.2f,5.2f,4.2f),canopy,world,0,false);Prop("Tree trunk",PrimitiveType.Cylinder,new Vector3(x,1.8f,z),new Vector3(.5f,1.8f,.5f),bark,world,0,false);
   }
  }
 }
}
