using UnityEngine;

public class DamagingBlock : Block
{
    protected override void Start()
    {
        base.Start();
        gameObject.layer = LayerMask.NameToLayer("Ground");
        GetComponent<BoxCollider2D>().isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

        }
    }
}