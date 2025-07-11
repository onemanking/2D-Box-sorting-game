using UnityEngine;


[CreateAssetMenu(fileName = "BoxData", menuName = "Scriptable Objects/BoxData")]
public class BoxData : ScriptableObject
{
    public BoxType BoxType;
    public Color Color;
}

public enum BoxType
{
    Red,
    Blue,
}
