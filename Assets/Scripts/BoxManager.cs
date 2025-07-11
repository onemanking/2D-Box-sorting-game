using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] private BoxSpawnConfigData m_boxSpawnConfigData;

    private float m_nextSpawnTime;

    void Update()
    {
        if (ShouldSpawnBox(Time.time)) SpawnBox();
    }

    private bool ShouldSpawnBox(float currentTime)
    {
        var min = m_boxSpawnConfigData.BoxSpawnIntervalMin;
        var max = m_boxSpawnConfigData.BoxSpawnIntervalMax;
        if (currentTime >= m_nextSpawnTime)
        {
            m_nextSpawnTime = currentTime + Random.Range(min, max);
            return true;
        }
        return false;
    }

    private void SpawnBox()
    {
        var boxPrefab = m_boxSpawnConfigData.GetRandomBoxPrefab();
        var spawnPosition = m_boxSpawnConfigData.GetRandomSpawnPosition();

        _ = Instantiate(boxPrefab, spawnPosition, Quaternion.identity);
    }
}
