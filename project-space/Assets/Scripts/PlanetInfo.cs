using UnityEngine;
using System.Collections.Generic;

public class PlanetInfo : MonoBehaviour
{
    [Header("Slingshot Restrictions")]
    [Tooltip("Maximum number of slingshots allowed to be placed on this planet.")]
    public int maxSlingshotsAllowed = 3;

    [Header("Postcard Settings")]
    public bool isCustomPlanet = false;
    public Postcard postcard;
    private bool _hasBeenVisited = false;

    private List<SlingShot> _placedSlingshots = new List<SlingShot>();

    public bool CanPlaceSlingshot => _placedSlingshots.Count < maxSlingshotsAllowed;
    public int PlacedCount => _placedSlingshots.Count;

    public void RegisterSlingshot(SlingShot slingshot)
    {
        if (slingshot != null && !_placedSlingshots.Contains(slingshot))
        {
            _placedSlingshots.Add(slingshot);
        }
    }

    public void CollectPostcard()
    {
        if (isCustomPlanet && postcard != null && SessionManager.Instance != null)
        {
            // If already permanently saved in the Album, do not show or collect again
            if (SessionManager.Instance.albumPostcards.Contains(postcard))
            {
                return;
            }

            // If not in the backpack (current session progress), collect and show it
            if (!SessionManager.Instance.backpackPostcards.Contains(postcard))
            {
                SessionManager.Instance.CollectPostcard(postcard);
            }
        }
    }
}
