using UnityEngine;

[CreateAssetMenu(fileName = "NPCBaseStateConfigData", menuName = "Scriptable Objects/NPCBaseStateConfigData")]
public class NPCBaseStateConfigData : ScriptableObject
{
    [Header("Idle Settings")]
    public float IdleTimeMin = 1f;
    public float IdleTimeMax = 3f;

    [Header("Movement Settings")]
    public float WanderSpeed = 2f;
    public float FoundSpeed = 3f;
    public float SortingSpeed = 5f;
}
