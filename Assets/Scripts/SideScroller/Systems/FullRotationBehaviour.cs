using UnityEngine;

[RequireComponent(typeof(MovementControllerCore))]
public class FullRotationBehaviour : MonoBehaviour
{
    private MovementControllerCore core;

    void Start()
    {
        core = GetComponent<MovementControllerCore>();
        core.Rb.constraints = RigidbodyConstraints2D.None;
    }
}