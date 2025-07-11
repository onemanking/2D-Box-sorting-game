using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class SortingArea : MonoBehaviour
{
    [SerializeField] private SortingAreaData m_sortingAreaData;

    private List<Box> sortedBoxes = new();

    internal void DepositBox(Box box)
    {
        if (IsBoxAccepted(box))
        {
            box.SetSorted();
            box.transform.SetParent(transform);

            sortedBoxes.Add(box);
            StackBox(box);
        }
        else
        {
            Debug.LogWarning($"Box {box.name} is not accepted in this sorting area.");
        }
    }

    private void StackBox(Box box)
    {
        if (sortedBoxes.Count > 1)
        {
            var previousBox = sortedBoxes[sortedBoxes.Count - 2];

            var stackHeight = previousBox.transform.localPosition.y + previousBox.Collider2D.bounds.size.y;

            box.transform.localPosition = new Vector2(0f, stackHeight);
        }
        else
        {
            box.transform.position = transform.position;
        }
    }

    internal bool IsBoxAccepted(Box box)
    {
        return m_sortingAreaData.IsBoxAccepted(box);
    }

    private Collider2D collider2D;
    internal Bounds GetBounds()
    {
        collider2D ??= GetComponent<Collider2D>();
        return collider2D.bounds;
    }
}
