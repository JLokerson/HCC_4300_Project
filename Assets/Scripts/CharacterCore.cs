using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CharacterCore : MonoBehaviour
{
    [Tooltip("Used to modify the speed the player moves")]
    public float MovementSpeed = 5;
    private InputAction moveAction;
    private CharacterController controller;

    private void Start()
    {
        moveAction=InputSystem.actions.FindAction("Move"); //binds the "Move" actions from the Input Actions Asset
        controller = GetComponent<CharacterController>();

    }

    private void Update()
    {
        //moveAction is a Vector2, which looks like (x,y) with x and y being -1, 0, or 1 depending on the input
        //(movement along an axis. 0 is none, -1 is one direction, and 1 is the other)
        Vector2 moveValue= moveAction.ReadValue<Vector2>(); 
        Vector3 move = new Vector3(moveValue.x, 0, moveValue.y); //now we convert it to a Vector3 for 3D movement. the 0 is since we don't want to move up or down
        controller.Move(move * Time.deltaTime * MovementSpeed); //this is the actual movement
    }

}
