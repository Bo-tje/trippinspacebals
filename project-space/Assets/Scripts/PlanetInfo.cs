using UnityEngine;
using System.Collections.Generic;

public class PlanetInfo : MonoBehaviour
{
    [Header("Slingshot Restrictions")]
    [Tooltip("Maximum number of slingshots allowed to be placed on this planet.")]
    public int maxSlingshotsAllowed = 3;

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
}
