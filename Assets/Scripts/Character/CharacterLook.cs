using System;
using UnityEngine;

public enum BodyType { Male, Female }
public enum SkinTone { Light, Dark }
public enum OutfitStyle { Ranger, Peasant }
public enum Hairstyle { None, Buzzed, SimpleParted, Long, Buns }
public enum StartingClass { Knight, Ranger, Wanderer }
public enum Keepsake { None, HealingPotions, Apples, Herbs }

// Everything the player picks in the character creator. Saved between sessions, and the
// Player in Main Game is built from it.
[Serializable]
public class CharacterLook
{
    [Header("Origin")]
    public string characterName = "";
    public StartingClass startingClass = StartingClass.Wanderer;
    public Keepsake keepsake = Keepsake.None;
    [Tooltip("Set once the class and keepsake items have been given, so respawning doesn't give them again.")]
    public bool startingItemsGiven;

    [Header("Body")]
    public BodyType body = BodyType.Male;
    [Range(0.94f, 1.06f)] public float height = 1f;
    [Range(0.94f, 1.06f)] public float build = 1f;

    [Header("Skin and face")]
    public SkinTone skin = SkinTone.Light;
    [Tooltip("Multiplies the skin colour. White leaves it as it is.")]
    public Color skinTint = Color.white;
    public Color eyeColour = new Color(0.35f, 0.22f, 0.12f);

    [Header("Hair")]
    public Hairstyle hair = Hairstyle.SimpleParted;
    public Color hairColour = new Color(0.25f, 0.17f, 0.11f);
    [Tooltip("Only on the male body.")]
    public bool beard;

    [Header("Outfit")]
    public OutfitStyle top = OutfitStyle.Ranger;
    public OutfitStyle legs = OutfitStyle.Ranger;
    public OutfitStyle feet = OutfitStyle.Ranger;
    [Tooltip("The Ranger hood. Hides the hair while it's up.")]
    public bool hood = true;
    [Tooltip("The Ranger shoulder pads.")]
    public bool shoulderPads = true;
    [Tooltip("Each outfit comes in two colour schemes from the artist.")]
    [Range(0, 1)] public int outfitScheme;
    [Tooltip("Multiplies the clothing colours. White leaves them as they are.")]
    public Color outfitColour = Color.white;

    private const string SaveKey = "Portaland.CharacterLook";

    public static bool HasSaved => PlayerPrefs.HasKey(SaveKey);

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(this));
        PlayerPrefs.Save();
    }

    // The saved look, or null if the player hasn't made a character yet.
    public static CharacterLook LoadSaved()
    {
        if (!HasSaved) return null;
        try { return JsonUtility.FromJson<CharacterLook>(PlayerPrefs.GetString(SaveKey)); }
        catch (ArgumentException) { return null; }
    }

    public CharacterLook Copy() => JsonUtility.FromJson<CharacterLook>(JsonUtility.ToJson(this));
}
