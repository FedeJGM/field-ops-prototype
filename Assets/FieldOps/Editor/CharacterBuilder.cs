using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace FieldOps.Editor {
 // The practice target: the Quaternius "Swat" soldier (CC0, see CREDITS.md) with an idle animation and a hit reaction.
 // Hit zones are capsules on the bones, so impact marks (parented to the collider's transform) move with the animation.
 static class CharacterBuilder {
  const string ModelPath="Assets/FieldOps/ThirdParty/QuaterniusMen/Swat.fbx";
  const string AnimationFolder="Assets/FieldOps/Animation";
  const string ControllerPath=AnimationFolder+"/SwatTarget.controller";

  // The pack ships 24 animations; only two are used, so the importer keeps just those (smaller APK).
  static void ConfigureClips() {
   var importer=(ModelImporter)AssetImporter.GetAtPath(ModelPath);if(importer==null)throw new FileNotFoundException("Missing soldier model "+ModelPath);
   var current=importer.clipAnimations;
   if(importer.avatarSetup==ModelImporterAvatarSetup.CreateFromThisModel&&current.Length==2&&current.Any(c=>c.name=="Idle"&&c.loopTime)&&current.Any(c=>c.name=="HitRecieve"&&!c.loopTime))return;
   importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;   // the Animator needs the rig's avatar
   var defaults=importer.defaultClipAnimations;var idle=defaults.First(c=>c.name.EndsWith("|Idle"));var hit=defaults.First(c=>c.name.EndsWith("|HitRecieve"));
   idle.name="Idle";idle.loopTime=true;hit.name="HitRecieve";hit.loopTime=false;
   importer.clipAnimations=new[]{idle,hit};importer.SaveAndReimport();
  }

  static AnimatorController BuildController(AnimationClip idle,AnimationClip hit) {
   Directory.CreateDirectory(AnimationFolder);
   var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
   if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
   else {   // keep the asset (and its GUID) stable across regenerations
    for(int i=controller.parameters.Length-1;i>=0;i--)controller.RemoveParameter(i);
    var existing=controller.layers[0].stateMachine;foreach(var t in existing.anyStateTransitions.ToArray())existing.RemoveAnyStateTransition(t);foreach(var s in existing.states.ToArray())existing.RemoveState(s.state);
   }
   controller.AddParameter("Hit",AnimatorControllerParameterType.Trigger);
   var machine=controller.layers[0].stateMachine;var idleState=machine.AddState("Idle");idleState.motion=idle;var hitState=machine.AddState("Hit");hitState.motion=hit;machine.defaultState=idleState;
   var toHit=machine.AddAnyStateTransition(hitState);toHit.AddCondition(AnimatorConditionMode.If,0,"Hit");toHit.hasExitTime=false;toHit.duration=.05f;toHit.canTransitionToSelf=false;
   var back=hitState.AddTransition(idleState);back.hasExitTime=true;back.exitTime=.9f;back.duration=.1f;
   EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();return controller;
  }

  static Transform Find(Transform root,string name) { return root.GetComponentsInChildren<Transform>(true).First(t=>t.name==name); }

  // A capsule from one bone to the next, in world-sized meters converted to the bone's own scale.
  static void Limb(Transform root,string from,string to,float radius) {
   var bone=Find(root,from);var end=Find(root,to);var a=bone.position;var b=end.position;float length=Vector3.Distance(a,b);float scale=Mathf.Max(1e-4f,bone.lossyScale.x);
   var localEnd=bone.InverseTransformPoint(b);int axis=0;float best=Mathf.Abs(localEnd.x);if(Mathf.Abs(localEnd.y)>best){axis=1;best=Mathf.Abs(localEnd.y);}if(Mathf.Abs(localEnd.z)>best)axis=2;
   var capsule=bone.gameObject.AddComponent<CapsuleCollider>();capsule.direction=axis;capsule.radius=radius/scale;capsule.height=(length+radius)/scale;capsule.center=bone.InverseTransformPoint((a+b)*.5f);
  }

  public static void Build(Transform idleRig,TrainingTarget target) {
   ConfigureClips();
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
   var soldier=(GameObject)PrefabUtility.InstantiatePrefab(prefab);soldier.name="Swat soldier";soldier.transform.SetParent(idleRig,false);
   // The model faces +Z; the player is at -Z, so the soldier turns around.
   soldier.transform.localPosition=Vector3.zero;soldier.transform.localRotation=Quaternion.Euler(0,180,0);
   var root=soldier.transform;
   foreach(var spec in new[]{("Hips","Abdomen",.18f),("Abdomen","Torso",.18f),("Torso","Chest",.2f),("Chest","Neck",.19f),("Neck","Head",.08f),("Head","Head_end",.13f),
    ("UpperArm.L","LowerArm.L",.075f),("LowerArm.L","Wrist.L",.065f),("UpperArm.R","LowerArm.R",.075f),("LowerArm.R","Wrist.R",.065f),
    ("UpperLeg.L","LowerLeg.L",.11f),("LowerLeg.L","Foot.L",.085f),("UpperLeg.R","LowerLeg.R",.11f),("LowerLeg.R","Foot.R",.085f)})Limb(root,spec.Item1,spec.Item2,spec.Item3);
   var clips=AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>().ToArray();
   var controller=BuildController(clips.First(c=>c.name=="Idle"),clips.First(c=>c.name=="HitRecieve"));
   var animator=soldier.GetComponent<Animator>();if(animator==null){animator=soldier.AddComponent<Animator>();animator.avatar=AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().First();}
   animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;target.animator=animator;
  }
 }
}
