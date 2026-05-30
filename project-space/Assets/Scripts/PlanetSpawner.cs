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
    public Vector2 spawnAreaSize = new Vector2(200f, 200f);

    [Tooltip("Minimum distance between any two spawned planets to prevent overlap.")]
    public float minimumSpacing = 30f;

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
        int maxAttempts = 50; // Prevent infinite loops if spacing is too tight
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            Vector3 potentialPosition = spawnAreaCenter + new Vector3(randomX, randomY, 0f);

            if (IsValidPosition(potentialPosition))
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
                    PlanetGravity gravity = newPlanet.GetComponent<PlanetGravity>();
                    if (gravity != null)
                    {
                        gravity.gravityRadius *= randomScale;
                        gravity.gravityStrength *= randomScale;
                    }

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
