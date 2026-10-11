using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace FieldOps.Editor {
 public static class PrototypeBuilder {
  const string ScenePath="Assets/FieldOps/Scenes/TrainingRange.unity";
  static Material sand,concrete,olive,dark,skin,metal,impact,orange,uiMaterial;
  static Font font;
  [MenuItem("Field Ops/Create or Rebuild Training Range")]
  public static void Generate() {
   if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
   Directory.CreateDirectory("Assets/FieldOps/Scenes");Directory.CreateDirectory("Assets/FieldOps/Materials");
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); uiMaterial=EnsureUiMaterial();
   sand=Mat("Sand",new Color(.53f,.48f,.34f));concrete=Mat("Concrete",new Color(.5f,.51f,.46f));olive=Mat("Olive",new Color(.25f,.3f,.18f));dark=Mat("Dark",new Color(.07f,.08f,.075f));skin=Mat("Skin",new Color(.65f,.5f,.36f));metal=Mat("Metal",new Color(.2f,.22f,.23f));impact=Mat("Impact",new Color(.025f,.017f,.012f));orange=Mat("Signal",new Color(.95f,.45f,.1f));
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.58f,.62f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.67f,.73f,.77f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=30;RenderSettings.fogEndDistance=90;
   var sun=new GameObject("Afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.1f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-35,0);
   var world=new GameObject("Training field — editable geometry").transform;
   Shape("Ground",PrimitiveType.Cube,new Vector3(0,-.15f,0),new Vector3(44,.3f,56),sand,world);
   Shape("Firing lane",PrimitiveType.Cube,new Vector3(0,.006f,-1),new Vector3(5,.012f,24),concrete,world);
   for(int i=0;i<6;i++)Shape("Lane marking",PrimitiveType.Cube,new Vector3(0,.015f,-10+i*4),new Vector3(.12f,.02f,1.5f),orange,world);
   Shape("Backstop",PrimitiveType.Cube,new Vector3(0,1.5f,12),new Vector3(20,3,.7f),concrete,world);
   Shape("Boundary west",PrimitiveType.Cube,new Vector3(-22,1,0),new Vector3(.5f,2,56),concrete,world);Shape("Boundary east",PrimitiveType.Cube,new Vector3(22,1,0),new Vector3(.5f,2,56),concrete,world);Shape("Boundary south",PrimitiveType.Cube,new Vector3(0,1,-28),new Vector3(44,2,.5f),concrete,world);Shape("Boundary north",PrimitiveType.Cube,new Vector3(0,1,28),new Vector3(44,2,.5f),concrete,world);
   for(int i=0;i<9;i++) {float x=i%2==0?-8:9;float z=-18+i*4.6f;Shape("Supply crate "+i,PrimitiveType.Cube,new Vector3(x,.7f,z),new Vector3(1.7f,1.4f,1.5f),olive,world);Shape("Crate strap",PrimitiveType.Cube,new Vector3(x,.7f,z-.76f),new Vector3(.16f,1.45f,.025f),metal,world);if(i%3==0)Shape("Stacked supply",PrimitiveType.Cube,new Vector3(x,1.95f,z),new Vector3(1.3f,1.1f,1.3f),olive,world); }
   for(int i=0;i<5;i++) {Shape("Concrete barrier",PrimitiveType.Cube,new Vector3(-14+i*7, .6f,19),new Vector3(3,1.2f,.65f),concrete,world);Shape("Safety post",PrimitiveType.Cylinder,new Vector3(-4+i*2, .55f,7),new Vector3(.13f,.55f,.13f),orange,world);}
   for(int i=0;i<10;i++) {float x=i%2==0?-19:19;var canopy=Shape("Low-poly tree",PrimitiveType.Sphere,new Vector3(x,3,-22+i*5),new Vector3(3,3.5f,3),olive,world);Object.DestroyImmediate(canopy.GetComponent<Collider>());Shape("Tree trunk",PrimitiveType.Cylinder,new Vector3(x,1,-22+i*5),new Vector3(.4f,1,.4f),dark,world);}
   var targetRoot=new GameObject("Training target — indestructible");targetRoot.transform.position=new Vector3(0,0,2);
   var target=targetRoot.AddComponent<TrainingTarget>();target.impactMaterial=impact;var rig=new GameObject("Idle rig").transform;rig.SetParent(targetRoot.transform,false);target.idleRig=rig;
   Shape("Left boot",PrimitiveType.Cube,new Vector3(-.16f,.12f,0),new Vector3(.24f,.24f,.35f),dark,rig);Shape("Right boot",PrimitiveType.Cube,new Vector3(.16f,.12f,0),new Vector3(.24f,.24f,.35f),dark,rig);
   Shape("Left leg",PrimitiveType.Capsule,new Vector3(-.16f,.6f,0),new Vector3(.23f,.4f,.23f),olive,rig);Shape("Right leg",PrimitiveType.Capsule,new Vector3(.16f,.6f,0),new Vector3(.23f,.4f,.23f),olive,rig);
   Shape("Torso armor",PrimitiveType.Cube,new Vector3(0,1.22f,0),new Vector3(.65f,.65f,.32f),olive,rig);Shape("Vest plate",PrimitiveType.Cube,new Vector3(0,1.25f,-.185f),new Vector3(.48f,.43f,.08f),metal,rig);
   Shape("Left arm",PrimitiveType.Capsule,new Vector3(-.44f,1.14f,0),new Vector3(.2f,.34f,.2f),olive,rig);Shape("Right arm",PrimitiveType.Capsule,new Vector3(.44f,1.14f,0),new Vector3(.2f,.34f,.2f),olive,rig);
   Shape("Head",PrimitiveType.Sphere,new Vector3(0,1.77f,0),new Vector3(.34f,.4f,.34f),skin,rig);Shape("Helmet",PrimitiveType.Sphere,new Vector3(0,1.92f,0),new Vector3(.39f,.25f,.39f),olive,rig);
   Shape("Target platform",PrimitiveType.Cube,new Vector3(0,.015f,2),new Vector3(2,.03f,2),dark,world);
   var player=new GameObject("Player");player.transform.position=new Vector3(0,.06f,-12);var controller=player.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.3f;controller.center=new Vector3(0,.9f,0);controller.stepOffset=.3f;
   var cameraObject=new GameObject("First-person camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(player.transform,false);cameraObject.transform.localPosition=new Vector3(0,1.65f,0);var cam=cameraObject.AddComponent<Camera>();cam.fieldOfView=68;cam.nearClipPlane=.03f;cam.farClipPlane=100;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.67f,.73f,.77f);cameraObject.AddComponent<AudioListener>();
   var weapon=new GameObject("Original training rifle").transform;weapon.SetParent(cameraObject.transform,false);weapon.localPosition=new Vector3(.28f,-.24f,.46f);
   GunPart("Receiver",new Vector3(0,0,0),new Vector3(.11f,.12f,.38f),metal,weapon);GunPart("Handguard",new Vector3(0,.015f,.23f),new Vector3(.1f,.1f,.21f),olive,weapon);GunPart("Barrel",new Vector3(0,.01f,.4f),new Vector3(.035f,.035f,.18f),dark,weapon);GunPart("Magazine",new Vector3(0,-.1f,.04f),new Vector3(.065f,.17f,.09f),dark,weapon);GunPart("Stock",new Vector3(0,-.02f,-.28f),new Vector3(.085f,.11f,.22f),olive,weapon);GunPart("Rear sight",new Vector3(0,.09f,-.08f),new Vector3(.06f,.04f,.035f),dark,weapon);GunPart("Front sight",new Vector3(0,.09f,.29f),new Vector3(.025f,.05f,.025f),dark,weapon);GunPart("Support glove",new Vector3(-.03f,-.065f,.19f),new Vector3(.14f,.085f,.13f),olive,weapon);
   var flash=GunPart("Muzzle flash",new Vector3(0,.01f,.51f),new Vector3(.09f,.09f,.08f),orange,weapon);flash.SetActive(false);
   var input=player.AddComponent<PrototypeInput>();var motor=player.AddComponent<FirstPersonMotor>();motor.input=input;motor.cameraPivot=cameraObject.transform;var rifle=player.AddComponent<Rifle>();rifle.input=input;rifle.view=cam;rifle.weapon=weapon;rifle.muzzleFlash=flash;
   var session=player.AddComponent<TrainingSession>();session.input=input;session.rifle=rifle;session.target=target;input.session=session;rifle.session=session;target.session=session;
   var hud=CreateHud(target,session,rifle);input.hud=hud;
   Configure();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("FIELD_OPS_SCENE_GENERATED "+ScenePath);
  }
  static void Configure() {
   PlayerSettings.companyName="FPS Prototype contributors";PlayerSettings.productName="Field Ops Prototype";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.fieldops.academicprototype");PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});PlayerSettings.colorSpace=ColorSpace.Gamma;
   QualitySettings.vSyncCount=0;QualitySettings.shadows=ShadowQuality.HardOnly;QualitySettings.shadowDistance=25;QualitySettings.antiAliasing=2;QualitySettings.pixelLightCount=1;
  }
  static Material EnsureUiMaterial() {
   // Runtime default Canvas material is not a serialized scene shader dependency.
   var shader=Shader.Find("UI/Default");if(shader==null)throw new System.Exception("Required UI/Default shader is unavailable.");
   var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);var shaders=settings.FindProperty("m_AlwaysIncludedShaders");bool included=false;
   for(int i=0;i<shaders.arraySize;i++)if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader)included=true;
   if(!included){int index=shaders.arraySize;shaders.InsertArrayElementAtIndex(index);shaders.GetArrayElementAtIndex(index).objectReferenceValue=shader;settings.ApplyModifiedPropertiesWithoutUndo();}
   const string path="Assets/FieldOps/Materials/UiDefault.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(shader);material.name="UiDefault";AssetDatabase.CreateAsset(material,path);}material.shader=shader;material.color=Color.white;EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();return material;
  }
  static Material Mat(string name,Color color) {string path="Assets/FieldOps/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Glossiness",.05f);return m;}
  static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Transform parent) {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}
  static GameObject GunPart(string name,Vector3 p,Vector3 s,Material m,Transform parent) {var go=Shape(name,PrimitiveType.Cube,p,s,m,parent);Object.DestroyImmediate(go.GetComponent<Collider>());go.layer=2;var r=go.GetComponent<Renderer>();r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return go;}
  static PrototypeHud CreateHud(TrainingTarget target,TrainingSession session,Rifle rifle) {
   font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");var canvas=new GameObject("HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
   var safe=Rect("Safe area",canvas.transform,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;var hud=canvas.AddComponent<PrototypeHud>();hud.safeRoot=safe;hud.target=target;hud.session=session;hud.rifle=rifle;
   var header=Panel("Header",safe,new Vector2(0,1),new Vector2(310,-65),new Vector2(580,100),new Color(.05f,.08f,.09f,.8f));hud.title=Label("Title",header,new Vector2(.5f,.5f),new Vector2(0,20),new Vector2(550,35),26,TextAnchor.MiddleLeft);hud.title.color=new Color(.9f,.76f,.42f);hud.status=Label("Status",header,new Vector2(.5f,.5f),new Vector2(0,-18),new Vector2(550,28),16,TextAnchor.MiddleLeft);
   hud.hitLabel=Label("Hit counter",safe,new Vector2(.5f,1),new Vector2(0,-145),new Vector2(800,40),26);
   hud.joystick=Panel("Movement joystick",safe,Vector2.zero,new Vector2(175,165),new Vector2(190,190),new Color(.08f,.13f,.14f,.6f));hud.stick=Panel("Joystick thumb",hud.joystick,new Vector2(.5f,.5f),Vector2.zero,new Vector2(70,70),new Color(.62f,.69f,.56f,.8f));hud.moveLabel=Label("Movement label",safe,Vector2.zero,new Vector2(175,48),new Vector2(250,40),22);
   hud.fire=Panel("Fire touch button",safe,new Vector2(1,0),new Vector2(-160,170),new Vector2(175,130),new Color(.7f,.32f,.12f,.8f));hud.fireLabel=Label("Fire label",hud.fire,new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,50),26);
   hud.reload=Panel("Reload touch button",safe,new Vector2(1,0),new Vector2(-160,300),new Vector2(175,85),new Color(.12f,.2f,.28f,.85f));hud.reloadLabel=Label("Reload label",hud.reload,new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,40),23);
   hud.ammoLabel=Label("Ammo counter",safe,new Vector2(1,0),new Vector2(-160,385),new Vector2(300,40),26);
   hud.sprint=Panel("Sprint touch button",safe,new Vector2(1,0),new Vector2(-365,115),new Vector2(180,85),new Color(.15f,.22f,.15f,.85f));hud.sprintLabel=Label("Sprint label",hud.sprint,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,40),23);
   hud.lookLabel=Label("Look hint",safe,new Vector2(1,.5f),new Vector2(-245,0),new Vector2(400,40),20);hud.lookLabel.color=new Color(1,1,1,.55f);
   hud.languageButton=MenuButton("Language",safe,new Vector2(1,1),new Vector2(-130,-48),new Vector2(210,60));hud.resetButton=MenuButton("Reset",safe,new Vector2(1,1),new Vector2(-130,-123),new Vector2(210,60));hud.resetLabel=hud.resetButton.GetComponentInChildren<Text>();
   hud.crosshair=CreateReticle(canvas.transform);
   hud.help=Label("Editor help",safe,new Vector2(.5f,0),new Vector2(0,25),new Vector2(950,40),18);
   CreateRoundMenu(hud,safe);
   hud.languageButton.transform.SetAsLastSibling();
   new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));return hud;
  }
  static void CreateRoundMenu(PrototypeHud hud,RectTransform safe) {
   hud.timerLabel=Label("Round timer",safe,new Vector2(.5f,1),new Vector2(0,-205),new Vector2(450,55),32);
   var blocker=Panel("Round menu blocker",safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,new Color(.025f,.04f,.05f,.88f));blocker.anchorMin=Vector2.zero;blocker.anchorMax=Vector2.one;blocker.offsetMin=blocker.offsetMax=Vector2.zero;blocker.GetComponent<Image>().raycastTarget=true;hud.menuBlocker=blocker.gameObject;
   var configuration=Panel("Round configuration",blocker,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,500),new Color(.09f,.14f,.15f,.98f));hud.configurationPanel=configuration.gameObject;
   hud.roundTitle=Label("Round title",configuration,new Vector2(.5f,.5f),new Vector2(0,185),new Vector2(720,55),32);
   hud.durationLabel=Label("Duration label",configuration,new Vector2(.5f,.5f),new Vector2(0,110),new Vector2(710,40),24);
   var field=Panel("Duration input",configuration,new Vector2(.5f,.5f),new Vector2(0,45),new Vector2(300,65),new Color(.2f,.27f,.27f,1));field.GetComponent<Image>().raycastTarget=true;
   // Keep pasted text intact: the rules reject invalid input instead of silently truncating/filtering it.
   var input=field.gameObject.AddComponent<InputField>();input.targetGraphic=field.GetComponent<Image>();input.textComponent=Label("Duration value",field,new Vector2(.5f,.5f),Vector2.zero,new Vector2(270,60),32);input.contentType=InputField.ContentType.Standard;input.keyboardType=TouchScreenKeyboardType.NumberPad;input.lineType=InputField.LineType.SingleLine;input.characterLimit=0;input.text="60";hud.durationInput=input;
   hud.durationError=Label("Duration error",configuration,new Vector2(.5f,.5f),new Vector2(0,-25),new Vector2(710,60),22);hud.durationError.color=new Color(1,.65f,.45f,1);
   hud.scoreRule=Label("Scoring rule",configuration,new Vector2(.5f,.5f),new Vector2(0,-90),new Vector2(710,45),22);
   hud.startButton=MenuButton("Start round",configuration,new Vector2(.5f,.5f),new Vector2(0,-175),new Vector2(420,65));
   var results=Panel("Round results",blocker,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,500),new Color(.09f,.14f,.15f,.98f));hud.resultsPanel=results.gameObject;
   hud.resultTitle=Label("Results title",results,new Vector2(.5f,.5f),new Vector2(0,175),new Vector2(710,55),32);
   hud.resultScore=Label("Final score",results,new Vector2(.5f,.5f),new Vector2(0,60),new Vector2(720,120),25);
   hud.replayButton=MenuButton("Replay round",results,new Vector2(.5f,.5f),new Vector2(0,-65),new Vector2(430,65));
   hud.configureButton=MenuButton("Configure round",results,new Vector2(.5f,.5f),new Vector2(0,-155),new Vector2(430,65));
   results.gameObject.SetActive(false);hud.resetButton.gameObject.SetActive(false);
  }
  static GameObject CreateReticle(Transform canvas) {
   // Aim uses the camera viewport center, not the potentially asymmetric safe area.
   var root=Rect("Crosshair",canvas,new Vector2(.5f,.5f),Vector2.zero,new Vector2(40,40));
   ReticleBar("ReticleLeft",root,new Vector2(-10,0),new Vector2(10,3));
   ReticleBar("ReticleRight",root,new Vector2(10,0),new Vector2(10,3));
   ReticleBar("ReticleTop",root,new Vector2(0,10),new Vector2(3,10));
   ReticleBar("ReticleBottom",root,new Vector2(0,-10),new Vector2(3,10));
   return root.gameObject;
  }
  static void ReticleBar(string name,Transform parent,Vector2 position,Vector2 size) {
   var bar=Panel(name,parent,new Vector2(.5f,.5f),position,size,Color.white);
   var outline=bar.gameObject.AddComponent<Outline>();outline.effectColor=Color.black;outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=true;
  }
  static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size) {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;}
  static RectTransform Panel(string name,Transform parent,Vector2 a,Vector2 p,Vector2 s,Color c) {var r=Rect(name,parent,a,p,s);var image=r.gameObject.AddComponent<Image>();image.material=uiMaterial;image.color=c;image.raycastTarget=false;return r;}
  static Text Label(string name,Transform parent,Vector2 a,Vector2 p,Vector2 s,int fontSize,TextAnchor align=TextAnchor.MiddleCenter) {var r=Rect(name,parent,a,p,s);var text=r.gameObject.AddComponent<Text>();text.material=uiMaterial;text.font=font;text.fontSize=fontSize;text.alignment=align;text.color=Color.white;text.raycastTarget=false;return text;}
  static Button MenuButton(string name,Transform parent,Vector2 a,Vector2 p,Vector2 s) {var r=Panel(name,parent,a,p,s,new Color(.12f,.18f,.2f,.9f));r.GetComponent<Image>().raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();Label("Label",r,new Vector2(.5f,.5f),Vector2.zero,s,23);return b;}
  [MenuItem("Field Ops/Build Android APK")]
  public static void BuildAndroid() { Configure();Directory.CreateDirectory("Builds");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/FieldOpsPrototype.apk",target=BuildTarget.Android,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Android build failed: "+report.summary.result);Debug.Log("FIELD_OPS_APK_BUILT "+report.summary.totalSize); }
 }
}



