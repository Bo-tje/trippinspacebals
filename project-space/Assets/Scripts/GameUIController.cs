using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI slingsText;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Button openAlbumButton;

    [Header("Album UI Panel")]
    [SerializeField] private GameObject albumPanel;
    [SerializeField] private Button closeAlbumButton;
    [SerializeField] private Image[] postcardSlots; // 6 slots in the album UI

    [Header("Album Database")]
    [Tooltip("Assign the 6 Postcards in the order they correspond to the slots.")]
    [SerializeField] private List<Postcard> expectedPostcards = new List<Postcard>();
    [SerializeField] private Sprite emptySlotSprite;

    [Header("UI Tweak Settings")]
    [Tooltip("Delay in seconds before UI shows the restart/death prompt when airborne to prevent flicker on micro-hops.")]
    [SerializeField] private float ungroundedPromptDelay = 0.5f;

    private Player.PlayerController _player;
    private Player.SlingshotPlacer _placer;
    private bool _isAlbumOpen = false;
    private float _timeSinceUngrounded = 0f;

    private void Start()
    {
        // Find player components
        _player = FindFirstObjectByType<Player.PlayerController>();
        _placer = FindFirstObjectByType<Player.SlingshotPlacer>();

        // Wire up buttons
        if (openAlbumButton != null)
            openAlbumButton.onClick.AddListener(OpenAlbum);

        if (closeAlbumButton != null)
            closeAlbumButton.onClick.AddListener(CloseAlbum);

        // Ensure album is closed at start
        if (albumPanel != null)
            albumPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateHUD();

        // Handle Escape or clicking album button to close it
        if (_isAlbumOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAlbum();
        }
    }

    private void UpdateHUD()
    {
        // 1. Update Slings count
        if (_placer != null && slingsText != null)
        {
            slingsText.text = $"Slings: {_placer.SlingshotsRemaining}";
        }

        // 2. Update contextual prompt text
        if (promptText != null)
        {
            if (_player != null)
            {
                if (_player.IsGrounded)
                {
                    _timeSinceUngrounded = 0f;

                    if (_placer != null && _placer.SlingshotsRemaining > 0)
                    {
                        promptText.text = "• Press [SPACE] to build a slingshot";
                    }
                    else
                    {
                        promptText.text = "• Press [R] to hitchhike back home";
                    }
                }
                else
                {
                    _timeSinceUngrounded += Time.deltaTime;

                    if (_timeSinceUngrounded >= ungroundedPromptDelay)
                    {
                        promptText.text = "• Press [R] to restart (Wipes current session)";
                    }
                    else
                    {
                        // Show default grounded prompt during the brief transition period
                        if (_placer != null && _placer.SlingshotsRemaining > 0)
                        {
                            promptText.text = "• Press [SPACE] to build a slingshot";
                        }
                        else
                        {
                            promptText.text = "• Press [R] to hitchhike back home";
                        }
                    }
                }
            }
            else
            {
                // Fallback if player doesn't exist in scene
                promptText.text = "";
                _player = FindFirstObjectByType<Player.PlayerController>();
                _placer = FindFirstObjectByType<Player.SlingshotPlacer>();
            }
        }
    }

    public void OpenAlbum()
    {
        Debug.Log("[ALBUM UI] Opening album...");
        _isAlbumOpen = true;
        if (albumPanel != null)
            albumPanel.SetActive(true);

        PopulateAlbum();

        // Optionally pause player input or freeze movement when looking at album
        if (_player != null)
        {
            _player.enabled = false;
        }
    }

    public void CloseAlbum()
    {
        Debug.Log("[ALBUM UI] Closing album...");
        _isAlbumOpen = false;
        if (albumPanel != null)
            albumPanel.SetActive(false);

        // Resume player input
        if (_player != null)
        {
            _player.enabled = true;
        }
    }

    private void PopulateAlbum()
    {
        if (SessionManager.Instance == null)
        {
            Debug.LogError("[ALBUM UI] SessionManager.Instance is null!");
            return;
        }

        // Get the list of permanently saved postcards
        List<Postcard> album = SessionManager.Instance.albumPostcards;
        Debug.Log($"[ALBUM UI] Populating slots. Collected count in permanent Album: {album.Count}");

        for (int i = 0; i < postcardSlots.Length; i++)
        {
            if (postcardSlots[i] == null) continue;

            if (i < expectedPostcards.Count && expectedPostcards[i] != null)
            {
                Postcard targetPostcard = expectedPostcards[i];
                bool isUnlocked = album.Contains(targetPostcard);
                Debug.Log($"[ALBUM UI] Slot {i} checking postcard '{targetPostcard.title}' (ID: {targetPostcard.id}). Unlocked/Collected permanent: {isUnlocked}");

                // Check if this specific postcard is unlocked in the album
                if (isUnlocked)
                {
                    postcardSlots[i].sprite = targetPostcard.image;
                    postcardSlots[i].color = Color.white; // Solid color for collected postcard
                }
                else
                {
                    // Not collected yet: show the empty brown slot image
                    postcardSlots[i].sprite = emptySlotSprite;
                    // Optional: slightly dim or shade the empty slot
                    postcardSlots[i].color = new Color(1f, 1f, 1f, 0.4f);
                }
            }
            else
            {
                // Fallback for unconfigured slots
                postcardSlots[i].sprite = emptySlotSprite;
                postcardSlots[i].color = new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }
}
