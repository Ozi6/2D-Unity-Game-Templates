using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class JumpingBehaviour : MonoBehaviour
{
    [Header("Jump")]
    public float JumpForce = 12f;
    public float JumpHoldMultiplier = 0.5f;
    public float MaxJumpHoldTime = 0.2f;

    private MovementComponents core;
    private WallClingBehaviour wallCling;

    private bool isJumping = false;
    private float jumpHoldTimer = 0f;

    void Start()
    {
        core = GetComponent<MovementComponents>();
        wallCling = GetComponent<WallClingBehaviour>();
    }

    void Update()
    {
        HandleJumping();
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (wallCling != null && wallCling.IsWallClinging)
            {
                wallCling.PerformWallJump();
            }
            else if (core.IsGrounded && !isJumping)
            {
                core.Rb.linearVelocity = new Vector2(core.Rb.linearVelocity.x, JumpForce);
                isJumping = true;
                jumpHoldTimer = 0f;
            }
        }

        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < MaxJumpHoldTime && (wallCling == null || !wallCling.IsWallClinging))
        {
            core.Rb.AddForce(Vector2.up * JumpForce * JumpHoldMultiplier * Time.deltaTime * 50f);
            jumpHoldTimer += Time.deltaTime;
        }
        else if (Input.GetButtonUp("Jump"))
            isJumping = false;

        if (core.Rb.linearVelocity.y < 0)
            isJumping = false;
    }
}