using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class Box : MonoBehaviour
{
    internal BoxData BoxData { get; private set; }

    internal Collider2D Collider2D { get; private set; }

    private Rigidbody2D rigid2D;
    private SpriteRenderer renderer;

    internal void Setup(BoxData boxData)
    {
        renderer ??= GetComponent<SpriteRenderer>();
        Collider2D ??= GetComponent<Collider2D>();
        rigid2D ??= GetComponent<Rigidbody2D>();

        BoxData = boxData;
        renderer.color = boxData.Color;
    }

    internal void SetHolding(Transform handTransform)
    {
        Collider2D = Collider2D != null ? Collider2D : GetComponent<Collider2D>();
        Collider2D.enabled = false;

        rigid2D = rigid2D != null ? rigid2D : GetComponent<Rigidbody2D>();
        rigid2D.simulated = false;

        renderer.sortingOrder = 10;

        transform.SetParent(handTransform);
        transform.position = handTransform.position;
        transform.localRotation = Quaternion.identity;
    }

    internal bool IsOnGround()
    {
        if (rigid2D == null)
            rigid2D = GetComponent<Rigidbody2D>();

        return rigid2D.linearVelocityY == 0;
    }

    internal void SetSorted()
    {
        Collider2D.enabled = true;
        transform.localPosition = Vector3.zero;

        renderer.sortingOrder = 0;
        renderer.color = new Color(BoxData.Color.r * 0.5f, BoxData.Color.g * 0.5f, BoxData.Color.b * 0.5f, BoxData.Color.a);
    }
}
