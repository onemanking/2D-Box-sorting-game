using UnityEngine;


[CreateAssetMenu(fileName = "BoxData", menuName = "Scriptable Objects/BoxData")]
public class BoxData : ScriptableObject
{
    public BoxColor BoxColor;
    public Color Color;
}

public enum BoxColor
{
    Red,
    Blue,
}
