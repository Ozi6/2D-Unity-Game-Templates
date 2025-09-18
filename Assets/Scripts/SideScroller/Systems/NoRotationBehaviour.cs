using UnityEngine;

[RequireComponent(typeof(MovementControllerCore))]
public class NoRotationBehaviour : MonoBehaviour
{
    private MovementControllerCore core;

    void Start()
    {
        core = GetComponent<MovementControllerCore>();
        core.Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}