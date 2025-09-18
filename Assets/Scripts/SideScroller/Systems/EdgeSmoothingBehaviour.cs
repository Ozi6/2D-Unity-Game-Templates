using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class EdgeSmoothingBehaviour : MonoBehaviour
{
    [Header("Edge Smoothing")]
    public bool EnableEdgeSmoothing = true;
    public float EdgeSmoothDistance = 0.1f;

    private MovementComponents core;
    private MovementBehaviour movement;
    private WallClingBehaviour wallCling;

    void Start()
    {
        core = GetComponent<MovementComponents>();
        movement = GetComponent<MovementBehaviour>();
        wallCling = GetComponent<WallClingBehaviour>();
    }

    void Update()
    {
        if (EnableEdgeSmoothing && (wallCling == null || !wallCling.IsWallClinging))
            HandleEdgeSmoothing();
    }

    private void HandleEdgeSmoothing()
    {
        if (!core.IsGrounded || Mathf.Abs(core.Rb.linearVelocity.x) < 0.1f)
            return;
        float facing = movement != null ? movement.FacingDirection : 1f;
        Vector2 rayOrigin = new Vector2(core.Coll.bounds.center.x + (facing * core.Coll.bounds.extents.x), core.Coll.bounds.max.y - EdgeSmoothDistance);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * facing, EdgeSmoothDistance, core.GroundMask);
        if (hit.collider != null)
        {
            Vector2 stepUpOrigin = new Vector2(hit.point.x + (facing * 0.01f), core.Coll.bounds.max.y + 0.1f);
            RaycastHit2D stepUpHit = Physics2D.Raycast(stepUpOrigin, Vector2.down, EdgeSmoothDistance + 0.1f, core.GroundMask);
            if (stepUpHit.collider != null)
            {
                float stepHeight = stepUpHit.point.y - core.Coll.bounds.min.y;
                if (stepHeight > 0 && stepHeight <= EdgeSmoothDistance)
                    core.Rb.AddForce(Vector2.up * stepHeight * 50f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (core != null && core.Coll != null && EnableEdgeSmoothing)
        {
            float facing = movement != null ? movement.FacingDirection : 1f;
            Bounds bounds = core.Coll.bounds;
            Vector2 edgeRayOrigin = new Vector2(bounds.center.x + (facing * bounds.extents.x), bounds.max.y - EdgeSmoothDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(edgeRayOrigin, edgeRayOrigin + Vector2.right * facing * EdgeSmoothDistance);
        }
    }
}