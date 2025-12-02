using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public FrameInput Current { get; private set; }

    private void Update()
    {
        Current = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            ShootDown = Input.GetMouseButtonDown(0),
            ShootHeld = Input.GetMouseButton(0),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            
        };
    }
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool ShootDown;
    public bool ShootHeld;
    public Vector2 Move;
}