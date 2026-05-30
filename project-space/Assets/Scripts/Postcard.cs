using UnityEngine;

[CreateAssetMenu(fileName = "New Postcard", menuName = "SpaceGame/Postcard")]
public class Postcard : ScriptableObject
{
    [Tooltip("Unique identifier for this postcard.")]
    public string id;

    [Tooltip("Display title of the postcard.")]
    public string title;

    [Tooltip("The postcard image collected by the player.")]
    public Sprite image;

    [Tooltip("Short postcard description or lore text.")]
    [TextArea(3, 10)]
    public string description;
}
