using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Box : MonoBehaviour
{
    [SerializeField] private BoxData m_boxData;

    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        renderer.color = m_boxData.Color;
    }

    void Update()
    {

    }
}
