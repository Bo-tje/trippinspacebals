using UnityEngine;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Progress Tracking")]
    public List<Postcard> albumPostcards = new List<Postcard>();
    public List<Postcard> backpackPostcards = new List<Postcard>();

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

    public void CollectPostcard(Postcard postcard)
    {
        if (postcard == null) return;

        if (!backpackPostcards.Contains(postcard) && !albumPostcards.Contains(postcard))
        {
            backpackPostcards.Add(postcard);
            Debug.Log($"[POSTCARD] Collected '{postcard.title}' (ID: {postcard.id}). Added to backpack! (Backpack Count: {backpackPostcards.Count})");
        }
        else
        {
            Debug.Log($"[POSTCARD] Already collected or in backpack: '{postcard.title}'");
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
        Debug.Log($"[SESSION] Committing progress. Backpack contains {backpackPostcards.Count} postcards.");
        // 1. Move postcards from backpack to permanent album
        foreach (Postcard postcard in backpackPostcards)
        {
            if (!albumPostcards.Contains(postcard))
            {
                albumPostcards.Add(postcard);
                Debug.Log($"[SESSION] Postcard '{postcard.title}' committed to permanent Album!");
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
            // Reset player position and rotation to stand upright on top of home planet
            placer.transform.position = home.transform.position + Vector3.up * 1.5f;
            placer.transform.rotation = Quaternion.identity;

            // Reset player velocity and angular spin
            Rigidbody2D rb = placer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            placer.RefillSlingshots(5);
        }
    }
}
