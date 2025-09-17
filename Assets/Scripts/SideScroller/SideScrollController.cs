using UnityEngine;

public class SideScrollController : Singleton<SideScrollController>
{
    public enum RotationMode { FullRotation, SlightRotation, NoRotation }

    [Header("Movement")]
    public float maxSpeed = 7f;
    public float groundAccel = 25f;
    public float groundDecel = 30f;
    public float turnaroundDecel = 35f;
    public float airAccel = 15f;
    public float airDecel = 10f;
    public float crouchSpeedMultiplier = 0.6f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float jumpHoldMultiplier = 0.5f;
    public float maxJumpHoldTime = 0.2f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckBuffer = 0.1f;

    [Header("Crouch")]
    public bool enableCrouch = true;

    [Header("Rotation")]
    public RotationMode rotationMode = RotationMode.FullRotation;
    public float maxRotationAngle = 20f;
    public float returnRotationSpeed = 180f;

    [Header("Wall Cling System")]
    public bool enableWallCling = false;
    public LayerMask wallMask;
    public float wallCheckDistance = 0.6f;
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 8f;
    public float wallJumpHorizontalForce = 6f;
    public float wallClingTime = 2f;
    public float wallJumpInputBuffer = 0.2f;

    [Header("Edge Smoothing")]
    public bool enableEdgeSmoothing = true;
    public float edgeSmoothDistance = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Vector2 standingSize;
    private Vector2 crouchSize;
    private bool isCrouching = false;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumpHoldTimer = 0f;

    private bool isWallClinging = false;
    private bool isTouchingWallLeft = false;
    private bool isTouchingWallRight = false;
    private float wallClingTimer = 0f;
    private float wallJumpTimer = 0f;
    private bool wallJumpInputPressed = false;

    private int playerLayer;
    private int platformsLayer;
    private float facingDirection = 1f;
    private float currentPushDirection = 0f;

    protected override bool Persistent => false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        standingSize = coll.size;
        crouchSize = new Vector2(standingSize.x, standingSize.y * 0.5f);
        playerLayer = LayerMask.NameToLayer("Player");
        platformsLayer = LayerMask.NameToLayer("Platforms");
        if (wallMask == 0)
            wallMask = groundMask;
        if (rotationMode == RotationMode.NoRotation)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        else
            rb.constraints = RigidbodyConstraints2D.None;
    }

    void Update()
    {
        isGrounded = CheckGroundWithRaycast();
        if (enableWallCling)
        {
            CheckWallCollision();
            HandleWallCling();
        }
        HandleMovement();
        HandleJumping();
        HandleCrouching();
        if (enableEdgeSmoothing && !isWallClinging)
            HandleEdgeSmoothing();
    }

    void FixedUpdate()
    {
        if (rotationMode == RotationMode.SlightRotation)
        {
            float angle = rb.rotation;
            bool wasClamped = false;
            float clampedAngle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle);
            if (Mathf.Abs(angle - clampedAngle) > 0.001f)
            {
                rb.MoveRotation(clampedAngle);
                rb.angularVelocity = 0f;
                wasClamped = true;
            }

            bool pushing = Mathf.Abs(currentPushDirection) > 0.1f && Mathf.Abs(rb.linearVelocity.x) < 0.5f && (isGrounded || isWallClinging);
            if (!pushing)
            {
                float newAngle = Mathf.MoveTowards(rb.rotation, 0f, returnRotationSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newAngle);
            }
            else if (wasClamped)
                rb.angularVelocity = 0f;
        }
    }

    private void HandleMovement()
    {
        float horizInput = Input.GetAxis("Horizontal");
        currentPushDirection = horizInput;
        if (isWallClinging && wallJumpTimer <= 0f)
        {
            if ((horizInput < 0 && isTouchingWallRight) || (horizInput > 0 && isTouchingWallLeft))
            {
                wallJumpInputPressed = true;
                wallJumpTimer = wallJumpInputBuffer;
            }
            return;
        }
        bool changingDirection = (horizInput > 0 && rb.linearVelocity.x < 0) || (horizInput < 0 && rb.linearVelocity.x > 0);
        float targetSpeed = horizInput * maxSpeed * (isCrouching ? crouchSpeedMultiplier : 1f);
        float accelRate;
        if (changingDirection && isGrounded)
            accelRate = turnaroundDecel;
        else if (isGrounded)
            accelRate = Mathf.Abs(horizInput) > 0 ? groundAccel : groundDecel;
        else
            accelRate = Mathf.Abs(horizInput) > 0 ? airAccel : airDecel;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.deltaTime);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        if (horizInput != 0)
            facingDirection = Mathf.Sign(horizInput);
        if (wallJumpTimer > 0f)
            wallJumpTimer -= Time.deltaTime;
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isWallClinging)
                PerformWallJump();
            else if (isGrounded && !isJumping)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isJumping = true;
                jumpHoldTimer = 0f;
            }
        }

        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < maxJumpHoldTime && !isWallClinging)
        {
            rb.AddForce(Vector2.up * jumpForce * jumpHoldMultiplier * Time.deltaTime * 50f);
            jumpHoldTimer += Time.deltaTime;
        }
        else if (Input.GetButtonUp("Jump"))
            isJumping = false;
        if (rb.linearVelocity.y < 0)
            isJumping = false;
    }

    private void CheckWallCollision()
    {
        Vector2 leftRayOrigin = new Vector2(coll.bounds.min.x, coll.bounds.center.y);
        Vector2 rightRayOrigin = new Vector2(coll.bounds.max.x, coll.bounds.center.y);
        RaycastHit2D leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.left, wallCheckDistance, wallMask);
        RaycastHit2D rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.right, wallCheckDistance, wallMask);
        isTouchingWallLeft = leftHit.collider != null;
        isTouchingWallRight = rightHit.collider != null;
    }

    private void HandleWallCling()
    {
        bool canWallCling = !isGrounded && rb.linearVelocity.y <= 0 && (isTouchingWallLeft || isTouchingWallRight);
        if (canWallCling && !isWallClinging && wallClingTimer < wallClingTime)
        {
            float horizInput = Input.GetAxis("Horizontal");
            if ((isTouchingWallLeft && horizInput < 0) || (isTouchingWallRight && horizInput > 0))
            {
                isWallClinging = true;
                rb.linearVelocity = new Vector2(0f, 0f);
            }
        }
        if (isWallClinging)
        {
            wallClingTimer += Time.deltaTime;
            if (rb.linearVelocity.y > -wallSlideSpeed)
                rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y - wallSlideSpeed * Time.deltaTime * 10f, -wallSlideSpeed));
            if (isGrounded || (!isTouchingWallLeft && !isTouchingWallRight) || wallClingTimer >= wallClingTime)
                StopWallCling();
            float horizInput = Input.GetAxis("Horizontal");
            if ((isTouchingWallLeft && horizInput > 0) || (isTouchingWallRight && horizInput < 0))
                if (!wallJumpInputPressed)
                    StopWallCling();
        }
        if (isGrounded)
            wallClingTimer = 0f;
    }

    private void PerformWallJump()
    {
        StopWallCling();
        Vector2 jumpDirection;
        if (isTouchingWallLeft)
        {
            jumpDirection = new Vector2(1f, 1f).normalized;
            facingDirection = 1f;
        }
        else
        {
            jumpDirection = new Vector2(-1f, 1f).normalized;
            facingDirection = -1f;
        }
        rb.linearVelocity = new Vector2(jumpDirection.x * wallJumpHorizontalForce, jumpDirection.y * wallJumpForce);
        wallJumpTimer = wallJumpInputBuffer;
    }

    private void StopWallCling()
    {
        isWallClinging = false;
        wallJumpInputPressed = false;
    }

    private void HandleEdgeSmoothing()
    {
        if (!isGrounded || Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            return;
        Vector2 rayOrigin = new Vector2(coll.bounds.center.x + (facingDirection * coll.bounds.extents.x), coll.bounds.max.y - edgeSmoothDistance);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * facingDirection, edgeSmoothDistance, groundMask);
        if (hit.collider != null)
        {
            Vector2 stepUpOrigin = new Vector2(hit.point.x + (facingDirection * 0.01f), coll.bounds.max.y + 0.1f);
            RaycastHit2D stepUpHit = Physics2D.Raycast(stepUpOrigin, Vector2.down, edgeSmoothDistance + 0.1f, groundMask);
            if (stepUpHit.collider != null)
            {
                float stepHeight = stepUpHit.point.y - coll.bounds.min.y;
                if (stepHeight > 0 && stepHeight <= edgeSmoothDistance)
                    rb.AddForce(Vector2.up * stepHeight * 50f);
            }
        }
    }

    private void HandleCrouching()
    {
        if (!enableCrouch) return;
        float vertInput = Input.GetAxis("Vertical");
        if (vertInput < -0.1f && !isCrouching)
            Crouch();
        else if (vertInput >= -0.1f && isCrouching)
            StandUp();
    }

    private bool CheckGroundWithRaycast()
    {
        Bounds bounds = coll.bounds;
        Vector2 rayOrigin = transform.position;
        float rayDistance = bounds.extents.y + groundCheckBuffer;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);
        return hit.collider != null;
    }

    private void Crouch()
    {
        isCrouching = true;
        coll.size = crouchSize;
        coll.offset = new Vector2(coll.offset.x, -crouchSize.y * 0.5f + standingSize.y * 0.5f);
        Physics2D.IgnoreLayerCollision(playerLayer, platformsLayer, true);
    }

    private void StandUp()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + crouchSize.y / 2);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, (standingSize.y - crouchSize.y) / 2 + 0.1f, groundMask);
        if (hit.collider == null)
        {
            isCrouching = false;
            coll.size = standingSize;
            coll.offset = Vector2.zero;
            Physics2D.IgnoreLayerCollision(playerLayer, platformsLayer, false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (coll != null)
        {
            Bounds bounds = coll.bounds;
            Vector2 rayOrigin = transform.position;
            float rayDistance = bounds.extents.y + groundCheckBuffer;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * rayDistance);
            Gizmos.DrawWireSphere(rayOrigin + Vector2.down * rayDistance, 0.1f);
            Vector2 groundCheckPoint = GetGroundCheckPoint();
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(groundCheckPoint, 0.05f);
            if (enableWallCling)
            {
                Vector2 leftRayOrigin = new Vector2(bounds.min.x, bounds.center.y);
                Vector2 rightRayOrigin = new Vector2(bounds.max.x, bounds.center.y);
                Gizmos.color = isTouchingWallLeft ? Color.blue : Color.cyan;
                Gizmos.DrawLine(leftRayOrigin, leftRayOrigin + Vector2.left * wallCheckDistance);
                Gizmos.color = isTouchingWallRight ? Color.blue : Color.cyan;
                Gizmos.DrawLine(rightRayOrigin, rightRayOrigin + Vector2.right * wallCheckDistance);
                if (isWallClinging)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(transform.position, bounds.size);
                }
            }
            if (enableEdgeSmoothing)
            {
                Vector2 edgeRayOrigin = new Vector2(bounds.center.x + (facingDirection * bounds.extents.x), bounds.max.y - edgeSmoothDistance);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(edgeRayOrigin, edgeRayOrigin + Vector2.right * facingDirection * edgeSmoothDistance);
            }
        }
    }

    public Vector2 GetGroundCheckPoint()
    {
        Bounds bounds = coll.bounds;
        return new Vector2(transform.position.x, transform.position.y - (bounds.extents.y + groundCheckBuffer));
    }

    public bool IsWallClinging() => isWallClinging;
    public bool IsTouchingWall() => isTouchingWallLeft || isTouchingWallRight;
    public float GetWallClingTimer() => wallClingTimer;
}