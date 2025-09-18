using UnityEngine;

public class Platform : Block
{
    private BoxCollider2D platformCollider;

    protected override void Start()
    {
        base.Start();
        platformCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        Vector2 playerBottom = MovementComponents.Instance.GetGroundCheckPoint();
        float platformTop = gameObject.transform.position.y;
        bool playerAbovePlatform = playerBottom.y >= platformTop;
        bool playerPressingDown = Input.GetAxis("Vertical") < -0.1f;
        bool shouldCollide = playerAbovePlatform && !playerPressingDown;
        if (shouldCollide)
            Debug.Log("SHOULD COLLIDE");
        platformCollider.enabled = shouldCollide;
    }
}