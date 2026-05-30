using UnityEngine;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Progress Tracking")]
    public List<string> albumPostcards = new List<string>();
    public List<string> backpackPostcards = new List<string>();

    private List<SlingShot> _sessionSlingshots = new List<SlingShot>();
    private List<SlingShot> _committedSlingshots = new List<SlingShot>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectPostcard(string id)
    {
        if (!backpackPostcards.Contains(id) && !albumPostcards.Contains(id))
        {
            backpackPostcards.Add(id);
            Debug.Log($"Postcard '{id}' added to backpack (Session).");
        }
    }

    public void RegisterSessionSlingshot(SlingShot ss)
    {
        if (ss != null && !_sessionSlingshots.Contains(ss))
        {
            _sessionSlingshots.Add(ss);
        }
    }

    public void CommitSessionProgress()
    {
        // 1. Move postcards from backpack to permanent album
        foreach (string id in backpackPostcards)
        {
            if (!albumPostcards.Contains(id))
            {
                albumPostcards.Add(id);
            }
        }
        backpackPostcards.Clear();

        // 2. Commit placed slingshots so they aren't lost on death
        foreach (SlingShot ss in _sessionSlingshots)
        {
            if (ss != null && !_committedSlingshots.Contains(ss))
            {
                _committedSlingshots.Add(ss);
            }
        }
        _sessionSlingshots.Clear();

        Debug.Log("Session progress successfully committed to Album/Save State.");
    }

    public void Die()
    {
        Debug.Log("Player Died! Resetting session progress...");

        // 1. Lose backpack postcards
        backpackPostcards.Clear();

        // 2. Destroy all session-placed slingshots
        foreach (SlingShot ss in _sessionSlingshots)
        {
            if (ss != null)
            {
                Destroy(ss.gameObject);
            }
        }
        _sessionSlingshots.Clear();

        // 3. Teleport player back home and refill slingshots
        TeleportPlayerHome();
    }

    private void TeleportPlayerHome()
    {
        Player.SlingshotPlacer placer = FindFirstObjectByType<Player.SlingshotPlacer>();
        HomePlanet home = FindFirstObjectByType<HomePlanet>();
        
        if (placer != null && home != null)
        {
            // Reset player velocity and position
            Rigidbody2D rb = placer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // Align placement offset upward from home planet
            placer.transform.position = home.transform.position + placer.transform.up * 1.5f;
            placer.RefillSlingshots(5);
        }
    }
}
