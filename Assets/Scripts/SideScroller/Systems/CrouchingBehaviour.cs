using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class CrouchingBehaviour : MonoBehaviour
{
    [Header("Crouch")]
    public bool EnableCrouch = true;

    private MovementComponents core;
    public bool IsCrouching { get; private set; } = false;
    private Vector2 crouchSize;

    void Start()
    {
        core = GetComponent<MovementComponents>();
        crouchSize = new Vector2(core.StandingSize.x, core.StandingSize.y * 0.5f);
    }

    void Update()
    {
        if (!EnableCrouch) return;
        HandleCrouching();
    }

    private void HandleCrouching()
    {
        float vertInput = Input.GetAxis("Vertical");
        if (vertInput < -0.1f && !IsCrouching)
            Crouch();
        else if (vertInput >= -0.1f && IsCrouching)
            StandUp();
    }

    private void Crouch()
    {
        IsCrouching = true;
        core.Coll.size = crouchSize;
        core.Coll.offset = new Vector2(core.Coll.offset.x, -crouchSize.y * 0.5f + core.StandingSize.y * 0.5f);
        Physics2D.IgnoreLayerCollision(core.PlayerLayer, core.PlatformsLayer, true);
    }

    private void StandUp()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + crouchSize.y / 2);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, (core.StandingSize.y - crouchSize.y) / 2 + 0.1f, core.GroundMask);
        if (hit.collider == null)
        {
            IsCrouching = false;
            core.Coll.size = core.StandingSize;
            core.Coll.offset = Vector2.zero;
            Physics2D.IgnoreLayerCollision(core.PlayerLayer, core.PlatformsLayer, false);
        }
    }
}