using UnityEngine;

public class Block : MonoBehaviour
{
    protected virtual void Start()
    {
        if (GetComponent<BoxCollider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }
}