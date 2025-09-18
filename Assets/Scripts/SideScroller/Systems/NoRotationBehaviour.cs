using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class NoRotationBehaviour : MonoBehaviour
{
    private MovementComponents core;

    void Start()
    {
        core = GetComponent<MovementComponents  >();
        core.Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}