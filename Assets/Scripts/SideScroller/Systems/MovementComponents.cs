using UnityEngine;

public class MovementComponents : Singleton<MovementComponents>
{
    [Header("Ground Check")]
    public LayerMask GroundMask;
    public float GroundCheckBuffer = 0.1f;

    protected override bool Persistent => false;

    public Rigidbody2D Rb { get; private set; }
    public BoxCollider2D Coll { get; private set; }
    public Vector2 StandingSize { get; private set; }
    public int PlayerLayer { get; private set; }
    public int PlatformsLayer { get; private set; }
    public bool IsGrounded { get; private set; }

    public Vector2 GetGroundCheckPoint()
    {
        Bounds bounds = Coll.bounds;
        return new Vector2(transform.position.x, transform.position.y - (bounds.extents.y + GroundCheckBuffer));
    }

    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Coll = GetComponent<BoxCollider2D>();
        StandingSize = Coll.size;
        PlayerLayer = LayerMask.NameToLayer("Player");
        PlatformsLayer = LayerMask.NameToLayer("Platforms");
    }

    void Update()
    {
        IsGrounded = CheckGroundWithRaycast();
    }

    private bool CheckGroundWithRaycast()
    {
        Vector2 rayOrigin = transform.position;
        float rayDistance = Coll.bounds.extents.y + GroundCheckBuffer;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, GroundMask);
        return hit.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        if (Coll != null)
        {
            Bounds bounds = Coll.bounds;
            Vector2 rayOrigin = transform.position;
            float rayDistance = bounds.extents.y + GroundCheckBuffer;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * rayDistance);
            Gizmos.DrawWireSphere(rayOrigin + Vector2.down * rayDistance, 0.1f);
            Vector2 groundCheckPoint = GetGroundCheckPoint();
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(groundCheckPoint, 0.05f);
        }
    }
}