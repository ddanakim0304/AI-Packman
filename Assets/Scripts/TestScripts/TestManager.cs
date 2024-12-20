using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TestManager : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints; 
    [SerializeField] private GameObject ghost;
    [SerializeField] private GameObject pacman;
    [SerializeField] private float ghostZOffset = -1f;
    [SerializeField] private float pacmanZOffset = -5f;
    [SerializeField] private TextMeshProUGUI counterText;
    private float timeoutTimer = 0f;
    public const float timeoutDuration = 40f;

    // Track how many times Pacman was caught
    private int pacmanCaughtCount = 0;      

        private void Update()
    {
        timeoutTimer += Time.deltaTime;
        
        if (timeoutTimer >= timeoutDuration)
        {
            timeoutTimer = 0f;
            SpawnActors();
        }
    }             

    private void Start()
    {
        SpawnActors();
        UpdateCounterText();
    }


    private void UpdateCounterText()
    {
        counterText.text = $"Count: {pacmanCaughtCount}";
    }
    private void SpawnActors()
    {
        if (spawnPoints == null || spawnPoints.Count < 2 || ghost == null || pacman == null)
        {
            Debug.LogWarning("Invalid setup: Ensure spawn points, ghost, and pacman are properly assigned and there are at least 2 spawn points.");
            return;
        }

        // Spawn Ghost
        int ghostSpawnIndex = GetRandomSpawnIndex();
        ghost.transform.position = GetSpawnPosition(ghostSpawnIndex, ghostZOffset);

        // Spawn Pacman at a different spawn point
        int pacmanSpawnIndex;
        do
        {
            pacmanSpawnIndex = GetRandomSpawnIndex();
        } while (pacmanSpawnIndex == ghostSpawnIndex);

        pacman.transform.position = GetSpawnPosition(pacmanSpawnIndex, pacmanZOffset);
    }

    private int GetRandomSpawnIndex()
    {
        return Random.Range(0, spawnPoints.Count);
    }

    private Vector3 GetSpawnPosition(int index, float zOffset)
    {
        Vector3 position = spawnPoints[index].position;
        return new Vector3(position.x, position.y, zOffset);
    }

    public void IncrementPacmanCaught()
    {
        pacmanCaughtCount++;
        UpdateCounterText();
        timeoutTimer = 0f;
        SpawnActors();
    }
}
