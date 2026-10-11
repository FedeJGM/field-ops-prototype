using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace FieldOps.Tests {
 public class CharacterTests {
  const string ModelPath="Assets/FieldOps/ThirdParty/QuaterniusMen/Swat.fbx";
  static TrainingTarget OpenTarget() { EditorSceneManager.OpenScene("Assets/FieldOps/Scenes/TrainingRange.unity",OpenSceneMode.Single);return Object.FindFirstObjectByType<TrainingTarget>(); }
  static Transform Bone(TrainingTarget target,string name) { return target.GetComponentsInChildren<Transform>(true).First(t=>t.name==name); }
  [Test] public void TheTargetIsTheImportedSwatSoldierWithAnAnimator() {
   var target=OpenTarget();var smr=target.GetComponentInChildren<SkinnedMeshRenderer>(true);Assert.IsNotNull(smr,"The soldier is a skinned mesh.");
   var source=PrefabUtility.GetCorrespondingObjectFromOriginalSource(smr.gameObject);Assert.IsNotNull(source);StringAssert.Contains("QuaterniusMen/Swat.fbx",AssetDatabase.GetAssetPath(source));
   Assert.IsNotNull(target.animator,"TrainingTarget needs the animator for the hit reaction.");Assert.IsNotNull(target.animator.runtimeAnimatorController);Assert.AreSame(target.animator,target.GetComponentInChildren<Animator>(true));
  }
  [Test] public void ThePrimitiveHumanoidIsGone() { var target=OpenTarget();foreach(var name in new[]{"Left boot","Right boot","Torso armor","Vest plate","Helmet","Head"}) {var t=target.GetComponentsInChildren<Transform>(true).Where(x=>x.name==name&&x.GetComponent<MeshFilter>()!=null);Assert.IsEmpty(t,name+" was a primitive of the old target."); } }
  [Test] public void AnimatorHasALoopingIdleAndAHitReactionThatReturnsToIdle() {
   var target=OpenTarget();var controller=(AnimatorController)target.animator.runtimeAnimatorController;var states=controller.layers[0].stateMachine.states.Select(s=>s.state).ToArray();
   var idle=states.FirstOrDefault(s=>s.name=="Idle");var hit=states.FirstOrDefault(s=>s.name=="Hit");Assert.IsNotNull(idle);Assert.IsNotNull(hit);
   Assert.AreSame(idle,controller.layers[0].stateMachine.defaultState,"The soldier starts idling.");Assert.IsTrue(((AnimationClip)idle.motion).isLooping);Assert.Greater(((AnimationClip)idle.motion).length,1f);Assert.IsFalse(((AnimationClip)hit.motion).isLooping);
   Assert.IsTrue(controller.parameters.Any(p=>p.name=="Hit"&&p.type==AnimatorControllerParameterType.Trigger));
   Assert.IsTrue(controller.layers[0].stateMachine.anyStateTransitions.Any(t=>t.destinationState==hit&&!t.hasExitTime&&!t.canTransitionToSelf),"A hit interrupts idling but does not restart itself.");
   Assert.IsTrue(hit.transitions.Any(t=>t.destinationState==idle&&t.hasExitTime),"After the reaction the soldier goes back to idle.");
  }
  [Test] public void ModelImportKeepsOnlyTheTwoClipsThatAreUsedAndLoopsTheIdle() {
   var importer=(ModelImporter)AssetImporter.GetAtPath(ModelPath);Assert.AreEqual(ModelImporterAnimationType.Generic,importer.animationType);Assert.AreEqual(ModelImporterAvatarSetup.CreateFromThisModel,importer.avatarSetup,"The Animator needs the rig avatar.");var clips=importer.clipAnimations;Assert.AreEqual(2,clips.Length,"The other 22 animations would only add weight to the build.");
   Assert.IsTrue(clips.Any(c=>c.name.Contains("Idle")&&c.loopTime));Assert.IsTrue(clips.Any(c=>c.name.Contains("HitRecieve")&&!c.loopTime));
  }
  [Test] public void TheSoldierStandsOnTheGroundAtHumanHeightFacingThePlayer() {
   var target=OpenTarget();float head=Bone(target,"Head_end").position.y;Assert.That(head,Is.InRange(1.7f,1.9f),"Human height.");
   float lowest=target.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Foot")).Min(t=>t.position.y);Assert.That(lowest,Is.InRange(-.05f,.08f),"Feet on the ground.");
   var toes=Bone(target,"Foot.R_end").position-Bone(target,"Foot.R").position;Assert.Greater(Vector3.Dot(toes.normalized,Vector3.back),.5f,"The toes point toward the player at z = -12.");
   Assert.AreEqual(2f,target.transform.position.z,.01f);Assert.AreEqual(0f,target.transform.position.x,.01f);
  }
  [Test] public void BodyCollidersFollowTheBonesAndSpanFromTheFeetToTheHelmet() {
   var target=OpenTarget();var colliders=target.GetComponentsInChildren<Collider>(true);Assert.GreaterOrEqual(colliders.Length,14,"Torso, head, arms and legs are separate hit zones.");
   var skeleton=Bone(target,"Root");foreach(var c in colliders)Assert.IsTrue(c.transform.IsChildOf(skeleton),c.name+" must sit on a bone so impact marks move with the animation.");
   var bounds=colliders[0].bounds;foreach(var c in colliders)bounds.Encapsulate(c.bounds);Assert.Less(bounds.min.y,.15f);Assert.Greater(bounds.max.y,1.7f);Assert.Less(bounds.size.x,1.4f,"Hit zones stay close to the body.");
  }
  [Test] public void ImpactMarksContrastWithTheDarkSuitSoTheyCanBeSeen() {
   var target=OpenTarget();float Lum(Color c) { return .2126f*c.r+.7152f*c.g+.0722f*c.b; }
   var suit=target.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterials.Where(m=>m.name!="Skin").Max(m=>Lum(m.color));
   Assert.Less(suit,.15f,"The tactical suit is dark.");Assert.Greater(Lum(target.impactMaterial.color),.35f,"Near-black impact marks vanish on a near-black suit.");Assert.Greater(Lum(target.impactMaterial.color)-suit,.25f);
  }
  [Test] public void TheSoldierIsLightEnoughForAPhone() {
   var target=OpenTarget();var meshes=target.GetComponentsInChildren<SkinnedMeshRenderer>(true);int triangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3);Assert.Less(triangles,8000);Assert.Less(meshes.SelectMany(m=>m.sharedMaterials).Distinct().Count(),6);Assert.Less(target.GetComponentsInChildren<Transform>(true).Length,200,"Bone count stays small.");
  }
  [Test] public void TheSoldierIsCreditedWithItsLicenseNotice() {
   var credits=File.ReadAllText("CREDITS.md");StringAssert.Contains("Ultimate Modular Men",credits);StringAssert.Contains("Swat",credits);StringAssert.Contains("Assets/FieldOps/ThirdParty/QuaterniusMen/",credits);
   var notice=File.ReadAllText("Assets/FieldOps/ThirdParty/QuaterniusMen/NOTICE.txt");StringAssert.Contains("Quaternius",notice);StringAssert.Contains("CC0",notice);StringAssert.Contains("Swat.fbx",notice);StringAssert.Contains("clips",notice);
  }
 }
}
