using UnityEngine;

[CreateAssetMenu(fileName = "BoxSpawnConfigData", menuName = "Scriptable Objects/BoxSpawnConfigData")]
public class BoxSpawnConfigData : ScriptableObject
{
    public BoxData[] BoxDataArray;
    public float BoxSpawnIntervalMin = 1.0f;
    public float BoxSpawnIntervalMax = 2.0f;
    public Vector2 BoxSpawnAreaMin = new(-10, 5f);
    public Vector2 BoxSpawnAreaMax = new(10, 5f);

    public BoxData GetRandomBoxData()
    {
        if (BoxDataArray == null || BoxDataArray.Length == 0)
        {
            Debug.LogError("No box data assigned in BoxSpawnConfigData.");
            return null;
        }
        int randomIndex = Random.Range(0, BoxDataArray.Length);

        return BoxDataArray[randomIndex];
    }

    public Vector2 GetRandomSpawnPosition()
    {
        float xPos = Random.Range(BoxSpawnAreaMin.x, BoxSpawnAreaMax.x);
        float yPos = Random.Range(BoxSpawnAreaMin.y, BoxSpawnAreaMax.y);
        return new Vector2(xPos, yPos);
    }
}
