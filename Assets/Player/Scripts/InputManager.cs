using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
   public static PlayerInput PlayerInput;

   public static Vector2 Movement;
   public static bool JumpPressed;
   public static bool JumpIsHeld;
   public static bool JumpReleased;
   public static bool RunIsHeld;

   private InputAction moveAction;
   private InputAction jumpAction;
   private InputAction runAction;

   private void Awake()
   {
      PlayerInput = GetComponent<PlayerInput>();
      
      moveAction = PlayerInput.actions["Move"];
      jumpAction = PlayerInput.actions["Jump"];
      runAction = PlayerInput.actions["Run"];
   }

   private void Update()
   {
      Movement = moveAction.ReadValue<Vector2>();

      JumpPressed = jumpAction.WasPressedThisFrame();
      JumpIsHeld = jumpAction.IsPressed();
      JumpReleased = jumpAction.WasReleasedThisFrame();
      
      RunIsHeld = runAction.IsPressed();
   }
   
   
   
   // public void JumpAction(InputAction.CallbackContext context){
   //    if (context.started || context.performed){
   //       Debug.Log("Recognizing jump");
   //       jumpPressed = true;
   //       jumpReleased = false;
   //    }else if (context.canceled){
   //       jumpPressed = false;
   //       jumpReleased = true;
   //    }
   // }

}
