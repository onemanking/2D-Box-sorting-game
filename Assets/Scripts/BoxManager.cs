using UnityEngine;

public class BoxManager : MonoBehaviour
{
    [SerializeField] private BoxSpawnConfigData m_boxSpawnConfigData;

    void Update()
    {
        if (m_boxSpawnConfigData.ShouldSpawnBox(Time.time))
        {
            SpawnBox();
        }
    }

    private void SpawnBox()
    {
        var boxPrefab = m_boxSpawnConfigData.GetRandomBoxPrefab();
        var spawnPosition = m_boxSpawnConfigData.GetRandomSpawnPosition();

        _ = Instantiate(boxPrefab, spawnPosition, Quaternion.identity);
    }
}
