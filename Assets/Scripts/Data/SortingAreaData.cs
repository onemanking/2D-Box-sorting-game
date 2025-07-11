using UnityEngine;

[CreateAssetMenu(fileName = "SortingAreaData", menuName = "Scriptable Objects/SortingAreaData")]
public class SortingAreaData : ScriptableObject
{
    public BoxData[] AcceptedBoxes; // Array of accepted box data

    internal bool IsBoxAccepted(Box box)
    {
        foreach (var acceptedBox in AcceptedBoxes)
        {
            if (box.BoxData == acceptedBox)
            {
                return true;
            }
        }
        return false;
    }
}
