using UnityEngine;

[RequireComponent(typeof(MovementControllerCore))]
public class WallClingBehaviour : MonoBehaviour
{
    [Header("Wall Cling System")]
    public bool EnableWallCling = true;
    public LayerMask WallMask;
    public float WallCheckDistance = 0.6f;
    public float WallSlideSpeed = 2f;
    public float WallJumpForce = 8f;
    public float WallJumpHorizontalForce = 6f;
    public float WallClingTime = 2f;
    public float WallJumpInputBuffer = 0.2f;

    private MovementControllerCore core;
    private MovementBehaviour movement;

    public bool IsWallClinging { get; private set; } = false;
    public bool IsTouchingWallLeft { get; private set; } = false;
    public bool IsTouchingWallRight { get; private set; } = false;
    private float wallClingTimer = 0f;
    public float WallJumpTimer { get; set; } = 0f;
    public bool WallJumpInputPressed { get; set; } = false;

    void Start()
    {
        core = GetComponent<MovementControllerCore>();
        movement = GetComponent<MovementBehaviour>();
        if (WallMask.value == 0)
            WallMask = core.GroundMask;
    }

    void Update()
    {
        if (!EnableWallCling) return;
        CheckWallCollision();
        HandleWallCling();
    }

    private void CheckWallCollision()
    {
        Vector2 leftRayOrigin = new Vector2(core.Coll.bounds.min.x, core.Coll.bounds.center.y);
        Vector2 rightRayOrigin = new Vector2(core.Coll.bounds.max.x, core.Coll.bounds.center.y);
        RaycastHit2D leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.left, WallCheckDistance, WallMask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.right, WallCheckDistance, WallMask);
        IsTouchingWallLeft = leftHit.collider != null;
        IsTouchingWallRight = rightHit.collider != null;
    }

    private void HandleWallCling()
    {
        bool canWallCling = !core.IsGrounded && core.Rb.linearVelocity.y <= 0 && (IsTouchingWallLeft || IsTouchingWallRight);
        if (canWallCling && !IsWallClinging && wallClingTimer < WallClingTime)
        {
            float horizInput = Input.GetAxis("Horizontal");
            if ((IsTouchingWallLeft && horizInput < 0) || (IsTouchingWallRight && horizInput > 0))
            {
                IsWallClinging = true;
                core.Rb.linearVelocity = new Vector2(0f, 0f);
            }
        }
        if (IsWallClinging)
        {
            wallClingTimer += Time.deltaTime;
            if (core.Rb.linearVelocity.y > -WallSlideSpeed)
                core.Rb.linearVelocity = new Vector2(0f, Mathf.Max(core.Rb.linearVelocity.y - WallSlideSpeed * Time.deltaTime * 10f, -WallSlideSpeed));
            if (core.IsGrounded || (!IsTouchingWallLeft && !IsTouchingWallRight) || wallClingTimer >= WallClingTime)
                StopWallCling();
            float horizInput = Input.GetAxis("Horizontal");
            if ((IsTouchingWallLeft && horizInput > 0) || (IsTouchingWallRight && horizInput < 0))
                if (!WallJumpInputPressed)
                    StopWallCling();
        }
        if (core.IsGrounded)
            wallClingTimer = 0f;
    }

    public void PerformWallJump()
    {
        StopWallCling();
        Vector2 jumpDirection;
        if (IsTouchingWallLeft)
        {
            jumpDirection = new Vector2(1f, 1f).normalized;
            if (movement != null)
                movement.FacingDirection = 1f;
        }
        else
        {
            jumpDirection = new Vector2(-1f, 1f).normalized;
            if (movement != null)
                movement.FacingDirection = -1f;
        }
        core.Rb.linearVelocity = new Vector2(jumpDirection.x * WallJumpHorizontalForce, jumpDirection.y * WallJumpForce);
        WallJumpTimer = WallJumpInputBuffer;
    }

    private void StopWallCling()
    {
        IsWallClinging = false;
        WallJumpInputPressed = false;
    }

    void OnDrawGizmosSelected()
    {
        if (core != null && core.Coll != null && EnableWallCling)
        {
            Bounds bounds = core.Coll.bounds;
            Vector2 leftRayOrigin = new Vector2(bounds.min.x, bounds.center.y);
            Vector2 rightRayOrigin = new Vector2(bounds.max.x, bounds.center.y);
            Gizmos.color = IsTouchingWallLeft ? Color.blue : Color.cyan;
            Gizmos.DrawLine(leftRayOrigin, leftRayOrigin + Vector2.left * WallCheckDistance);
            Gizmos.color = IsTouchingWallRight ? Color.blue : Color.cyan;
            Gizmos.DrawLine(rightRayOrigin, rightRayOrigin + Vector2.right * WallCheckDistance);
            if (IsWallClinging)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(transform.position, bounds.size);
            }
        }
    }

    public bool IsTouchingWall() => IsTouchingWallLeft || IsTouchingWallRight;
    public float GetWallClingTimer() => wallClingTimer;
}