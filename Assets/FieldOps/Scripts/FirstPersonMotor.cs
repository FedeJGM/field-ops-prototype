using UnityEngine;
namespace FieldOps {
 [RequireComponent(typeof(CharacterController))]
 public sealed class FirstPersonMotor : MonoBehaviour {
  public PrototypeInput input;public Transform cameraPivot;public float walkSpeed=3.5f,sprintSpeed=6f;
  CharacterController controller;float pitch,verticalSpeed;
  void Awake() { controller=GetComponent<CharacterController>();Application.targetFrameRate=60; }
  void Update() {
   transform.Rotate(0,input.Look.x,0);pitch=Mathf.Clamp(pitch-input.Look.y,-75,75);cameraPivot.localRotation=Quaternion.Euler(pitch,0,0);
   if(controller.isGrounded&&verticalSpeed<0)verticalSpeed=-2;verticalSpeed+=Physics.gravity.y*Time.deltaTime;
   var horizontal=(transform.right*input.Move.x+transform.forward*input.Move.y)*(input.Sprint?sprintSpeed:walkSpeed);
   controller.Move((horizontal+Vector3.up*verticalSpeed)*Time.deltaTime);
  }
 }
}
