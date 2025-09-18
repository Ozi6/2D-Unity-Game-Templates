using UnityEngine;

[RequireComponent(typeof(MovementControllerCore))]
public class MovementBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float MaxSpeed = 7f;
    public float GroundAccel = 25f;
    public float GroundDecel = 30f;
    public float TurnaroundDecel = 35f;
    public float AirAccel = 15f;
    public float AirDecel = 10f;
    public float CrouchSpeedMultiplier = 0.6f;

    private MovementControllerCore core;
    private WallClingBehaviour wallCling;
    private CrouchingBehaviour crouch;

    public float FacingDirection { get; set; } = 1f;
    public float CurrentPushDirection { get; private set; } = 0f;

    void Start()
    {
        core = GetComponent<MovementControllerCore>();
        wallCling = GetComponent<WallClingBehaviour>();
        crouch = GetComponent<CrouchingBehaviour>();
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizInput = Input.GetAxis("Horizontal");
        CurrentPushDirection = horizInput;

        bool isWallClinging = wallCling != null ? wallCling.IsWallClinging : false;
        float wallJumpTimer = wallCling != null ? wallCling.WallJumpTimer : 0f;

        if (isWallClinging && wallJumpTimer <= 0f)
        {
            if (wallCling != null && ((horizInput < 0 && wallCling.IsTouchingWallRight) || (horizInput > 0 && wallCling.IsTouchingWallLeft)))
            {
                wallCling.WallJumpInputPressed = true;
                wallCling.WallJumpTimer = wallCling.WallJumpInputBuffer;
            }
            return;
        }

        bool isCrouching = crouch != null ? crouch.IsCrouching : false;

        bool changingDirection = (horizInput > 0 && core.Rb.linearVelocity.x < 0) || (horizInput < 0 && core.Rb.linearVelocity.x > 0);
        float targetSpeed = horizInput * MaxSpeed * (isCrouching ? CrouchSpeedMultiplier : 1f);
        float accelRate;
        if (changingDirection && core.IsGrounded)
            accelRate = TurnaroundDecel;
        else if (core.IsGrounded)
            accelRate = Mathf.Abs(horizInput) > 0 ? GroundAccel : GroundDecel;
        else
            accelRate = Mathf.Abs(horizInput) > 0 ? AirAccel : AirDecel;
        float newVelX = Mathf.MoveTowards(core.Rb.linearVelocity.x, targetSpeed, accelRate * Time.deltaTime);
        core.Rb.linearVelocity = new Vector2(newVelX, core.Rb.linearVelocity.y);

        if (horizInput != 0)
            FacingDirection = Mathf.Sign(horizInput);

        if (wallCling != null && wallCling.WallJumpTimer > 0f)
            wallCling.WallJumpTimer -= Time.deltaTime;
    }
}