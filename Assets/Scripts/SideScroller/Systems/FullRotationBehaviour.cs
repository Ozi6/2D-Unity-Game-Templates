using UnityEngine;

[RequireComponent(typeof(MovementComponents))]
public class FullRotationBehaviour : MonoBehaviour
{
    private MovementComponents core;

    void Start()
    {
        core = GetComponent<MovementComponents>();
        core.Rb.constraints = RigidbodyConstraints2D.None;
    }
}