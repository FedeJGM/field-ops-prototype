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
   sand=Mat("Sand",new Color(.53f,.48f,.34f));concrete=Mat("Concrete",new Color(.5f,.51f,.46f));olive=Mat("Olive",new Color(.25f,.3f,.18f));dark=Mat("Dark",new Color(.07f,.08f,.075f));skin=Mat("Skin",new Color(.65f,.5f,.36f));metal=Mat("Metal",new Color(.2f,.22f,.23f));impact=Mat("Impact",new Color(1f,.72f,.2f));   // bright amber: the practice target wears a near-black suit
   orange=Mat("Signal",new Color(.95f,.45f,.1f));
   EnvironmentBuilder.CreateAtmosphere();
   var world=new GameObject("Training field — editable geometry").transform;
   EnvironmentBuilder.Build(world);
   var targetRoot=new GameObject("Training target — indestructible");targetRoot.transform.position=new Vector3(0,0,2);
   var target=targetRoot.AddComponent<TrainingTarget>();target.impactMaterial=impact;target.markMesh=Resources.GetBuiltinResource<Mesh>("Sphere.fbx");var rig=new GameObject("Idle rig").transform;rig.SetParent(targetRoot.transform,false);target.idleRig=rig;
   CharacterBuilder.Build(rig,target);
   var player=new GameObject("Player");player.layer=2;   // Ignore Raycast: the camera sits inside the capsule, which would otherwise block shots aimed downward
   player.transform.position=new Vector3(0,.06f,-12);var controller=player.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.3f;controller.center=new Vector3(0,.9f,0);controller.stepOffset=.3f;
   var cameraObject=new GameObject("First-person camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(player.transform,false);cameraObject.transform.localPosition=new Vector3(0,1.65f,0);var cam=cameraObject.AddComponent<Camera>();cam.fieldOfView=68;cam.nearClipPlane=.03f;cam.farClipPlane=100;cam.clearFlags=CameraClearFlags.Skybox;cameraObject.AddComponent<AudioListener>();
   var weapon=new GameObject("Weapon holder").transform;weapon.SetParent(cameraObject.transform,false);weapon.localPosition=new Vector3(.28f,-.24f,.46f);
   var flash=GunPart("Muzzle flash",Vector3.zero,new Vector3(.09f,.09f,.08f),orange,weapon);flash.SetActive(false);
   CreateWeapons(weapon,out var weaponDefinitions,out var weaponModels);
   var input=player.AddComponent<PrototypeInput>();var motor=player.AddComponent<FirstPersonMotor>();motor.input=input;motor.cameraPivot=cameraObject.transform;player.AddComponent<FrameLogger>();var rifle=player.AddComponent<Rifle>();var sfx=player.AddComponent<GameAudio>();rifle.sfx=sfx;target.sfx=sfx;rifle.input=input;rifle.view=cam;rifle.weapon=weapon;rifle.muzzleFlash=flash;rifle.weapons=weaponDefinitions;rifle.models=weaponModels;
   var session=player.AddComponent<TrainingSession>();session.input=input;session.rifle=rifle;session.target=target;input.session=session;rifle.session=session;target.session=session;
   var hud=CreateHud(target,session,rifle);input.hud=hud;
   Configure();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();Debug.Log("FIELD_OPS_SCENE_GENERATED "+ScenePath);
  }
  struct WeaponSpec {public string key,fbx,audio;public float length;public Vector3 hold,aim;public int mag;public float interval;public int damage,pellets;public float spread,range,kick,fov;}
  // Generic names and stats; models come from the credited CC0 pack (see CREDITS.md). The default is the assault rifle.
  static readonly WeaponSpec[] WeaponSpecs={
   new WeaponSpec{key="weaponPistol",fbx="Pistol_1",audio="pistol",length=.24f,hold=new Vector3(0,.02f,0),aim=new Vector3(-.28f,0,-.03f),mag=12,interval=.30f,damage=20,pellets=1,spread=0,range=60,kick=.06f,fov=50},
   new WeaponSpec{key="weaponRifle",fbx="AssaultRifle_2",audio="rifle",length=.9f,hold=Vector3.zero,aim=new Vector3(-.28f,-.03f,-.04f),mag=30,interval=.12f,damage=25,pellets=1,spread=0,range=80,kick=.045f,fov=40},
   new WeaponSpec{key="weaponSmg",fbx="SubmachineGun_1",audio="smg",length=.55f,hold=Vector3.zero,aim=new Vector3(-.28f,-.02f,-.03f),mag=32,interval=.07f,damage=15,pellets=1,spread=0,range=60,kick=.03f,fov=45},
   new WeaponSpec{key="weaponShotgun",fbx="Shotgun_4",audio="shotgun",length=1.05f,hold=Vector3.zero,aim=new Vector3(-.28f,-.02f,-.04f),mag=6,interval=.85f,damage=15,pellets=8,spread=4,range=40,kick=.12f,fov=50},
   new WeaponSpec{key="weaponSniper",fbx="SniperRifle_4",audio="sniper",length=1.3f,hold=Vector3.zero,aim=new Vector3(-.28f,-.02f,-.04f),mag=5,interval=1.2f,damage=100,pellets=1,spread=0,range=120,kick=.15f,fov=20},
  };
  const string WeaponFolder="Assets/FieldOps/Weapons",WeaponModelFolder="Assets/FieldOps/ThirdParty/QuaterniusGuns/";
  const string WeaponAudioFolder="Assets/FieldOps/Audio/Weapons/";
  // Short effects: decompress on load so playback has no start-up delay on the phone; mono keeps them small.
  static AudioClip ImportWeaponClip(string file) {
   var path=WeaponAudioFolder+file;var importer=(AudioImporter)AssetImporter.GetAtPath(path);if(importer==null)throw new FileNotFoundException("Missing weapon sound "+path);
   var settings=importer.defaultSampleSettings;
   if(settings.loadType!=AudioClipLoadType.DecompressOnLoad||importer.loadInBackground||!importer.forceToMono) {
    settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;
    importer.defaultSampleSettings=settings;importer.loadInBackground=false;importer.forceToMono=true;importer.SaveAndReimport();
   }
   return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
  }
  static Bounds RendererBounds(GameObject go) {var renderers=go.GetComponentsInChildren<Renderer>(true);var bounds=renderers[0].bounds;for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);return bounds;}
  static void CreateWeapons(Transform holder,out WeaponDefinition[] definitions,out Transform[] models) {
   Directory.CreateDirectory(WeaponFolder);definitions=new WeaponDefinition[WeaponSpecs.Length];models=new Transform[WeaponSpecs.Length];
   for(int i=0;i<WeaponSpecs.Length;i++) {
    var spec=WeaponSpecs[i];var assetPath=WeaponFolder+"/"+spec.key+".asset";
    var def=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(assetPath);if(def==null){def=ScriptableObject.CreateInstance<WeaponDefinition>();AssetDatabase.CreateAsset(def,assetPath);}
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(WeaponModelFolder+spec.fbx+".fbx");if(prefab==null)throw new FileNotFoundException("Missing weapon model "+spec.fbx+" under "+WeaponModelFolder);
    var wrapper=new GameObject("Weapon - "+spec.key).transform;wrapper.SetParent(holder,false);
    var model=(GameObject)PrefabUtility.InstantiatePrefab(prefab);model.transform.SetParent(wrapper,false);
    foreach(var collider in model.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
    foreach(var child in model.GetComponentsInChildren<Transform>(true))child.gameObject.layer=2;
    foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
    // The generator runs with identity transforms, so world bounds are holder-space bounds. Turn the long axis to point forward (+Z); Unity mirrors X on FBX import, so the muzzle ends up toward +Z with +90. The import rotation (270 deg about X) must be kept, or the gun lies on its side.
    var importRotation=model.transform.localRotation;var raw=RendererBounds(model);float yaw=raw.size.x>raw.size.z?90f:0f;model.transform.localRotation=Quaternion.Euler(0,yaw,0)*importRotation;
    var turned=RendererBounds(model);model.transform.localScale=model.transform.localScale*(spec.length/turned.size.z);
    var fitted=RendererBounds(model);model.transform.localPosition=spec.hold-(fitted.center-holder.position);
    var final=RendererBounds(model);Debug.Log("FIELD_OPS_WEAPON "+spec.key+" rawSize="+raw.size+" yaw="+yaw+" length="+final.size.z);
    def.nameKey=spec.key;def.modelPrefab=prefab;def.modelLength=final.size.z;def.muzzleLocal=spec.hold+new Vector3(0,final.size.y*.18f,final.size.z*.5f+.03f);// Aiming centers the gun horizontally, puts its top edge a little under the eye line and keeps its rear end 38 cm from the camera.
    def.aimOffset=new Vector3(-(holder.localPosition.x+spec.hold.x),-.075f-(holder.localPosition.y+spec.hold.y+final.size.y*.5f),.38f-(holder.localPosition.z+spec.hold.z-final.size.z*.5f));
    def.magazineSize=spec.mag;def.fireInterval=spec.interval;def.damage=spec.damage;def.pellets=spec.pellets;def.spreadDegrees=spec.spread;def.range=spec.range;def.shotClip=ImportWeaponClip(spec.audio+"_shot.wav");def.reloadClip=ImportWeaponClip(spec.audio+"_reload.wav");def.reloadSeconds=def.reloadClip.length;def.kick=spec.kick;def.aimFov=spec.fov;
    EditorUtility.SetDirty(def);definitions[i]=def;models[i]=wrapper;wrapper.gameObject.SetActive(i==GameSettings.DefaultWeapon);
   }
   AssetDatabase.SaveAssets();
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
   hud.fpsLabel=Label("FPS counter",safe,new Vector2(.5f,1),new Vector2(50,-30),new Vector2(180,36),24);hud.fpsLabel.raycastTarget=false;
   hud.hitLabel=Label("Hit counter",safe,new Vector2(.5f,1),new Vector2(0,-145),new Vector2(800,40),26);
   hud.joystick=Panel("Movement joystick",safe,Vector2.zero,new Vector2(175,165),new Vector2(190,190),new Color(.08f,.13f,.14f,.6f));hud.stick=Panel("Joystick thumb",hud.joystick,new Vector2(.5f,.5f),Vector2.zero,new Vector2(70,70),new Color(.62f,.69f,.56f,.8f));hud.moveLabel=Label("Movement label",hud.joystick,new Vector2(.5f,.5f),new Vector2(0,-117),new Vector2(250,40),22);
   hud.fire=Panel("Fire touch button",safe,new Vector2(1,0),new Vector2(-160,170),new Vector2(175,130),new Color(.7f,.32f,.12f,.8f));hud.fireLabel=Label("Fire label",hud.fire,new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,50),26);
   hud.reload=Panel("Reload touch button",safe,new Vector2(1,0),new Vector2(-160,300),new Vector2(175,85),new Color(.12f,.2f,.28f,.85f));hud.reloadLabel=Label("Reload label",hud.reload,new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,40),23);
   hud.aim=Panel("Aim touch button",safe,new Vector2(1,0),new Vector2(-365,215),new Vector2(180,85),new Color(.12f,.2f,.28f,.85f));hud.aimLabel=Label("Aim label",hud.aim,new Vector2(.5f,.5f),Vector2.zero,new Vector2(170,40),23);
   hud.settingsButton=MenuButton("Settings",safe,new Vector2(1,0),new Vector2(-110,55),new Vector2(190,70));
   hud.ammoLabel=Label("Ammo counter",hud.reload,new Vector2(.5f,.5f),new Vector2(0,85),new Vector2(300,40),26);
   hud.sprint=Panel("Sprint touch button",safe,new Vector2(1,0),new Vector2(-365,115),new Vector2(180,85),new Color(.15f,.22f,.15f,.85f));hud.sprintLabel=Label("Sprint label",hud.sprint,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,40),23);
   hud.lookLabel=Label("Look hint",safe,new Vector2(1,.5f),new Vector2(-245,0),new Vector2(400,40),20);hud.lookLabel.color=new Color(1,1,1,.55f);
   hud.languageButton=MenuButton("Language",safe,new Vector2(1,1),new Vector2(-130,-48),new Vector2(210,60));hud.resetButton=MenuButton("Reset",safe,new Vector2(1,1),new Vector2(-130,-123),new Vector2(210,60));hud.resetLabel=hud.resetButton.GetComponentInChildren<Text>();
   hud.crosshair=CreateReticle(canvas.transform);
   hud.help=Label("Editor help",safe,new Vector2(.5f,0),new Vector2(0,25),new Vector2(950,40),18);
   CreateRoundMenu(hud,safe);CreateSettingsMenu(hud,safe);CreateLayoutToolbar(hud,safe);
   var dot=Panel("Aim dot",canvas.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(8,8),new Color(1,.1f,.1f,1));dot.GetComponent<Image>().raycastTarget=false;hud.aimDot=dot.gameObject;dot.gameObject.SetActive(false);
   hud.languageButton.transform.SetAsLastSibling();hud.settingsButton.transform.SetAsLastSibling();
   new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));return hud;
  }
  static PrototypeHud.WeaponSelectorUi CreateWeaponSelector(RectTransform parent,float y) {
   var ui=new PrototypeHud.WeaponSelectorUi();
   ui.prev=MenuButton("Weapon previous",parent,new Vector2(.5f,.5f),new Vector2(-300,y),new Vector2(90,60));ui.prev.GetComponentInChildren<Text>().text="<";
   ui.next=MenuButton("Weapon next",parent,new Vector2(.5f,.5f),new Vector2(300,y),new Vector2(90,60));ui.next.GetComponentInChildren<Text>().text=">";
   ui.name=Label("Weapon name",parent,new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(460,50),30);ui.name.color=new Color(.9f,.76f,.42f);
   ui.stats=Label("Weapon stats",parent,new Vector2(.5f,.5f),new Vector2(0,y-50),new Vector2(700,40),22);
   return ui;
  }
  static void CreateLayoutToolbar(PrototypeHud hud,RectTransform safe) {
   var panel=Panel("Layout toolbar",safe,new Vector2(.5f,1),new Vector2(0,-105),new Vector2(560,170),new Color(.09f,.14f,.15f,.95f));panel.GetComponent<Image>().raycastTarget=true;hud.layoutPanel=panel.gameObject;
   hud.layoutHint=Label("Layout hint",panel,new Vector2(.5f,.5f),new Vector2(0,50),new Vector2(530,60),20);
   hud.saveLayoutButton=MenuButton("Save layout",panel,new Vector2(.5f,.5f),new Vector2(-180,-40),new Vector2(165,60));
   hud.resetLayoutButton=MenuButton("Reset layout",panel,new Vector2(.5f,.5f),new Vector2(0,-40),new Vector2(165,60));
   hud.cancelLayoutButton=MenuButton("Cancel layout",panel,new Vector2(.5f,.5f),new Vector2(180,-40),new Vector2(165,60));
   panel.gameObject.SetActive(false);
  }
  static void CreateSettingsMenu(PrototypeHud hud,RectTransform safe) {
   var panel=Panel("Settings panel",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,720),new Color(.09f,.14f,.15f,1f));panel.GetComponent<Image>().raycastTarget=true;hud.settingsPanel=panel.gameObject;
   hud.settingsTitle=Label("Settings title",panel,new Vector2(.5f,.5f),new Vector2(0,310),new Vector2(710,55),32);
   hud.aimModeButton=MenuButton("Aim mode",panel,new Vector2(.5f,.5f),new Vector2(0,225),new Vector2(680,70));hud.aimModeLabel=hud.aimModeButton.GetComponentInChildren<Text>();
   hud.crossButton=MenuButton("Keep cross",panel,new Vector2(.5f,.5f),new Vector2(0,145),new Vector2(680,70));hud.crossLabel=hud.crossButton.GetComponentInChildren<Text>();
   hud.fpsButton=MenuButton("FPS counter",panel,new Vector2(.5f,.5f),new Vector2(0,65),new Vector2(680,70));
   hud.editLayoutButton=MenuButton("Edit layout",panel,new Vector2(.5f,.5f),new Vector2(0,-15),new Vector2(680,70));
   hud.settingsWeapon=CreateWeaponSelector(panel,-90);
   hud.closeSettingsButton=MenuButton("Close settings",panel,new Vector2(.5f,.5f),new Vector2(0,-275),new Vector2(420,65));
   panel.gameObject.SetActive(false);
  }
  static void CreateRoundMenu(PrototypeHud hud,RectTransform safe) {
   hud.timerLabel=Label("Round timer",safe,new Vector2(.5f,1),new Vector2(0,-205),new Vector2(450,55),32);
   var blocker=Panel("Round menu blocker",safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero,new Color(.025f,.04f,.05f,.88f));blocker.anchorMin=Vector2.zero;blocker.anchorMax=Vector2.one;blocker.offsetMin=blocker.offsetMax=Vector2.zero;blocker.GetComponent<Image>().raycastTarget=true;hud.menuBlocker=blocker.gameObject;
   var configuration=Panel("Round configuration",blocker,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,600),new Color(.09f,.14f,.15f,.98f));hud.configurationPanel=configuration.gameObject;
   hud.roundTitle=Label("Round title",configuration,new Vector2(.5f,.5f),new Vector2(0,245),new Vector2(720,55),32);
   hud.durationLabel=Label("Duration label",configuration,new Vector2(.5f,.5f),new Vector2(0,190),new Vector2(710,40),24);
   var field=Panel("Duration input",configuration,new Vector2(.5f,.5f),new Vector2(0,125),new Vector2(300,65),new Color(.2f,.27f,.27f,1));field.GetComponent<Image>().raycastTarget=true;
   // Keep pasted text intact: the rules reject invalid input instead of silently truncating/filtering it.
   var input=field.gameObject.AddComponent<InputField>();input.targetGraphic=field.GetComponent<Image>();input.textComponent=Label("Duration value",field,new Vector2(.5f,.5f),Vector2.zero,new Vector2(270,60),32);input.contentType=InputField.ContentType.Standard;input.keyboardType=TouchScreenKeyboardType.NumberPad;input.lineType=InputField.LineType.SingleLine;input.characterLimit=0;input.text="60";hud.durationInput=input;
   hud.durationError=Label("Duration error",configuration,new Vector2(.5f,.5f),new Vector2(0,55),new Vector2(710,60),22);hud.durationError.color=new Color(1,.65f,.45f,1);
   hud.scoreRule=Label("Scoring rule",configuration,new Vector2(.5f,.5f),new Vector2(0,10),new Vector2(710,45),22);
   hud.weaponPrevButton=MenuButton("Weapon previous",configuration,new Vector2(.5f,.5f),new Vector2(-300,-50),new Vector2(90,60));hud.weaponPrevButton.GetComponentInChildren<Text>().text="<";
   hud.weaponNextButton=MenuButton("Weapon next",configuration,new Vector2(.5f,.5f),new Vector2(300,-50),new Vector2(90,60));hud.weaponNextButton.GetComponentInChildren<Text>().text=">";
   hud.weaponNameLabel=Label("Weapon name",configuration,new Vector2(.5f,.5f),new Vector2(0,-50),new Vector2(460,50),30);hud.weaponNameLabel.color=new Color(.9f,.76f,.42f);
   hud.weaponStatsLabel=Label("Weapon stats",configuration,new Vector2(.5f,.5f),new Vector2(0,-100),new Vector2(700,40),22);
   hud.startButton=MenuButton("Start round",configuration,new Vector2(.5f,.5f),new Vector2(0,-205),new Vector2(420,65));
   var results=Panel("Round results",blocker,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,620),new Color(.09f,.14f,.15f,.98f));hud.resultsPanel=results.gameObject;
   hud.resultTitle=Label("Results title",results,new Vector2(.5f,.5f),new Vector2(0,250),new Vector2(710,55),32);
   hud.resultScore=Label("Final score",results,new Vector2(.5f,.5f),new Vector2(0,130),new Vector2(720,120),25);
   hud.resultsWeapon=CreateWeaponSelector(results,15);
   hud.replayButton=MenuButton("Replay round",results,new Vector2(.5f,.5f),new Vector2(0,-120),new Vector2(430,65));
   hud.configureButton=MenuButton("Configure round",results,new Vector2(.5f,.5f),new Vector2(0,-210),new Vector2(430,65));
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



