using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class SlightRotationBehaviour : MonoBehaviour
{
    [Header("Rotation")]
    public float MaxRotationAngle = 20f;
    public float ReturnRotationSpeed = 180f;

    private MovementComponents core;
    private MovementBehaviour movement;
    private WallClingBehaviour wallCling;

    void Start()
    {
        core = GetComponent<MovementComponents>();
        movement = GetComponent<MovementBehaviour>();
        wallCling = GetComponent<WallClingBehaviour>();
        core.Rb.constraints = RigidbodyConstraints2D.None;
    }

    void FixedUpdate()
    {
        float angle = core.Rb.rotation;
        bool wasClamped = false;
        float clampedAngle = Mathf.Clamp(angle, -MaxRotationAngle, MaxRotationAngle);
        if (Mathf.Abs(angle - clampedAngle) > 0.001f)
        {
            core.Rb.MoveRotation(clampedAngle);
            core.Rb.angularVelocity = 0f;
            wasClamped = true;
        }

        bool pushing = movement != null && Mathf.Abs(movement.CurrentPushDirection) > 0.1f && Mathf.Abs(core.Rb.linearVelocity.x) < 0.5f && (core.IsGrounded || (wallCling != null && wallCling.IsWallClinging));
        if (!pushing)
        {
            float newAngle = Mathf.MoveTowards(core.Rb.rotation, 0f, ReturnRotationSpeed * Time.fixedDeltaTime);
            core.Rb.MoveRotation(newAngle);
        }
        else if (wasClamped)
            core.Rb.angularVelocity = 0f;
    }
}