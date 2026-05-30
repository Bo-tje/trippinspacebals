using UnityEngine;
using System.Collections.Generic;

public class PlanetSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Unique custom planets that should all be spawned once.")]
    public GameObject[] customPlanetPrefabs;

    [Tooltip("Filler planets that will be randomly chosen to populate space.")]
    public GameObject[] fillerPlanetPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("How many random filler planets to spawn.")]
    public int fillerPlanetCount = 10;

    [Tooltip("Minimum scale for randomized filler planets.")]
    public float minFillerScale = 0.6f;

    [Tooltip("Maximum scale for randomized filler planets.")]
    public float maxFillerScale = 2.0f;

    [Tooltip("If true, filler planets will be tinted with a random vibrant color.")]
    public bool randomizeFillerColor = true;

    [Tooltip("Size of the rectangular box in space where planets can spawn.")]
    public Vector2 spawnAreaSize = new Vector2(120f, 120f);

    [Tooltip("Minimum distance between any two spawned planets to prevent overlap.")]
    public float minimumSpacing = 18f;

    [Tooltip("Center of the spawning area.")]
    public Vector3 spawnAreaCenter = Vector3.zero;

    private List<Vector3> _spawnedPositions = new List<Vector3>();

    private void Start()
    {
        SpawnAllPlanets();
    }

    private void SpawnAllPlanets()
    {
        // Add Home Planet's position to prevent spawning anything on top of it
        HomePlanet home = FindFirstObjectByType<HomePlanet>();
        if (home != null)
        {
            _spawnedPositions.Add(home.transform.position);
        }

        // 1. Spawn Custom Planets (Ensure all of them are spawned once)
        foreach (GameObject prefab in customPlanetPrefabs)
        {
            if (prefab != null)
            {
                SpawnPlanet(prefab, false);
            }
        }

        // 2. Spawn Filler Planets
        if (fillerPlanetPrefabs != null && fillerPlanetPrefabs.Length > 0)
        {
            for (int i = 0; i < fillerPlanetCount; i++)
            {
                GameObject randomFiller = fillerPlanetPrefabs[Random.Range(0, fillerPlanetPrefabs.Length)];
                if (randomFiller != null)
                {
                    SpawnPlanet(randomFiller, true);
                }
            }
        }
    }

    private void SpawnPlanet(GameObject prefab, bool isFiller)
    {
        int maxAttempts = 100; // Increase attempts to find a valid path location
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 potentialPosition;

            // Connected Spawning: pick a random already-spawned planet and place near it
            if (_spawnedPositions.Count > 0)
            {
                Vector3 origin = _spawnedPositions[Random.Range(0, _spawnedPositions.Count)];
                float distance = Random.Range(minimumSpacing, minimumSpacing * 1.5f);
                float angle = Random.Range(0f, Mathf.PI * 2f);
                potentialPosition = origin + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
            }
            else
            {
                // Fallback for first planet if no home planet is found
                float randomX = Random.Range(-spawnAreaSize.x / 4f, spawnAreaSize.x / 4f);
                float randomY = Random.Range(-spawnAreaSize.y / 4f, spawnAreaSize.y / 4f);
                potentialPosition = spawnAreaCenter + new Vector3(randomX, randomY, 0f);
            }

            // Ensure the position fits inside boundaries and doesn't overlap existing planets
            if (IsWithinBounds(potentialPosition) && IsValidPosition(potentialPosition))
            {
                GameObject newPlanet = Instantiate(prefab, potentialPosition, Quaternion.identity);
                _spawnedPositions.Add(potentialPosition);

                // Apply customization if it is a filler planet
                if (isFiller)
                {
                    // 1. Randomize Scale
                    float randomScale = Random.Range(minFillerScale, maxFillerScale);
                    newPlanet.transform.localScale = Vector3.one * randomScale;

                    // Scale gravity properties proportionally
                    //PlanetGravity gravity = newPlanet.GetComponent<PlanetGravity>();
                    //if (gravity != null)
                    //{
                     //   gravity.gravityRadius *= randomScale;
                    //    gravity.gravityStrength *= randomScale;
                   // }

                    // 2. Randomize Color (HSV space for nice, vibrant tints)
                    if (randomizeFillerColor)
                    {
                        SpriteRenderer sr = newPlanet.GetComponent<SpriteRenderer>();
                        if (sr == null)
                        {
                            sr = newPlanet.GetComponentInChildren<SpriteRenderer>();
                        }
                        
                        if (sr != null)
                        {
                            // Generate vibrant hues with high saturation and brightness
                            sr.color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);
                        }
                    }
                }

                return;
            }
        }
        Debug.LogWarning($"Could not find a valid position to spawn planet: {prefab.name}. Try increasing spawnAreaSize or decreasing minimumSpacing.");
    }

    private bool IsWithinBounds(Vector3 position)
    {
        float minX = spawnAreaCenter.x - spawnAreaSize.x / 2f;
        float maxX = spawnAreaCenter.x + spawnAreaSize.x / 2f;
        float minY = spawnAreaCenter.y - spawnAreaSize.y / 2f;
        float maxY = spawnAreaCenter.y + spawnAreaSize.y / 2f;

        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    private bool IsValidPosition(Vector3 position)
    {
        foreach (Vector3 spawnedPos in _spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minimumSpacing)
            {
                return false;
            }
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawning boundaries in the editor
        Gizmos.color = new Color(0.2f, 0.5f, 0.9f, 0.15f);
        Gizmos.DrawCube(spawnAreaCenter, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 1f));
        
        Gizmos.color = new Color(0.2f, 0.5f, 0.9f, 0.8f);
        Gizmos.DrawWireCube(spawnAreaCenter, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 1f));
    }
}
