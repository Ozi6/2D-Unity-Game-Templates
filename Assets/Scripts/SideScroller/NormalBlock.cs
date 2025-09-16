using UnityEngine;

public class NormalBlock : Block
{
    protected override void Start()
    {
        base.Start();
        gameObject.layer = LayerMask.NameToLayer("Ground");
        GetComponent<BoxCollider2D>().isTrigger = false;
    }
}