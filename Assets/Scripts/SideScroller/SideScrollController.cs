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

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Vector2 standingSize;
    private Vector2 crouchSize;
    private bool isCrouching = false;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumpHoldTimer = 0f;

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

        if (rotationMode == RotationMode.NoRotation)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    void Update()
    {
        isGrounded = CheckGroundWithRaycast();
        HandleMovement();
        HandleJumping();
        HandleCrouching();
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

            bool pushing = Mathf.Abs(currentPushDirection) > 0.1f && Mathf.Abs(rb.linearVelocity.x) < 0.5f && isGrounded;
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
    }

    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpHoldTimer = 0f;
        }
        if (Input.GetButton("Jump") && isJumping && jumpHoldTimer < maxJumpHoldTime)
        {
            rb.AddForce(Vector2.up * jumpForce * jumpHoldMultiplier * Time.deltaTime * 50f);
            jumpHoldTimer += Time.deltaTime;
        }
        else if (Input.GetButtonUp("Jump"))
            isJumping = false;
        if (rb.linearVelocity.y < 0)
            isJumping = false;
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
        }
    }

    public Vector2 GetGroundCheckPoint()
    {
        Bounds bounds = coll.bounds;
        return new Vector2(transform.position.x, transform.position.y - (bounds.extents.y + groundCheckBuffer));
    }
}