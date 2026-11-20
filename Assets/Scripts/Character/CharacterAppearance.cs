using System;
using UnityEngine;

// Shows a CharacterLook on one of the wardrobe characters (PlayerCharacterMale/Female): which
// clothes and hair are on, and the skin, eye, hair and clothing colours. All the pieces are
// already on the character, this just turns them on and off.
// It also works outside Play mode, so changing Look in the Inspector updates the model.
// Colours are set per character with property blocks, so the shared materials never change.
[ExecuteAlways]
public class CharacterAppearance : MonoBehaviour
{
    public CharacterLook look = new CharacterLook();

    [Serializable]
    public class OutfitParts
    {
        public Renderer[] top;
        public Renderer[] legs;
        public Renderer[] feet;
        public Renderer[] hood;
        public Renderer[] shoulderPads;
        public Texture[] schemes;   // The artist's colour schemes for this outfit.
    }

    [Header("Filled in when the wardrobe is built")]
    public BodyType body;
    public OutfitParts ranger = new OutfitParts();
    public OutfitParts peasant = new OutfitParts();
    [Tooltip("One per Hairstyle, in the same order. The first (None) stays empty.")]
    public Renderer[] hairstyles = new Renderer[0];
    public Renderer beard;
    public Renderer headSkin;
    public Renderer eyes;
    public Texture headLight, headDark, bodyLight, bodyDark;

    private static readonly int ColourId = Shader.PropertyToID("_Color");
    private static readonly int TextureId = Shader.PropertyToID("_MainTex");
    private MaterialPropertyBlock block;

    private void OnEnable() => Apply();
    private void OnValidate() => Apply();

    public void Apply(CharacterLook newLook)
    {
        look = newLook.Copy();
        Apply();
    }

    public void Apply()
    {
        if (look == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        ShowOutfit(ranger, OutfitStyle.Ranger);
        ShowOutfit(peasant, OutfitStyle.Peasant);

        bool hoodUp = look.hood && look.top == OutfitStyle.Ranger;
        for (int i = 0; i < hairstyles.Length; i++)
        {
            if (hairstyles[i] == null) continue;
            bool on = !hoodUp && i == (int)look.hair;
            hairstyles[i].enabled = on;
            if (on) Tint(hairstyles[i], look.hairColour, null);
        }
        if (beard != null)
        {
            beard.enabled = look.beard && body == BodyType.Male;
            Tint(beard, look.hairColour, null);
        }

        bool dark = look.skin == SkinTone.Dark;
        if (headSkin != null) Tint(headSkin, look.skinTint, dark ? headDark : headLight);
        if (eyes != null) Tint(eyes, look.eyeColour, null);
    }

    private void ShowOutfit(OutfitParts parts, OutfitStyle style)
    {
        Texture scheme = parts.schemes != null && parts.schemes.Length > 0
            ? parts.schemes[Mathf.Clamp(look.outfitScheme, 0, parts.schemes.Length - 1)] : null;
        Show(parts.top, look.top == style, scheme);
        Show(parts.legs, look.legs == style, scheme);
        Show(parts.feet, look.feet == style, scheme);
        // The hood and shoulder pads belong with the top, so they need that top on too.
        Show(parts.hood, look.hood && look.top == style, scheme);
        Show(parts.shoulderPads, look.shoulderPads && look.top == style, scheme);
    }

    private void Show(Renderer[] renderers, bool on, Texture scheme)
    {
        if (renderers == null) return;
        foreach (Renderer part in renderers)
        {
            if (part == null) continue;
            part.enabled = on;
            if (!on) continue;
            // Pieces can mix clothing and bare skin, so colour each material to match.
            Material[] materials = part.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                bool skin = IsSkin(materials[i]);
                Texture texture = skin ? (look.skin == SkinTone.Dark ? bodyDark : bodyLight) : scheme;
                // One-material pieces (most of them) use a whole-renderer block so the red hit
                // flash can still show over the tint.
                Tint(part, skin ? look.skinTint : look.outfitColour, texture, materials.Length > 1 ? i : -1);
            }
        }
    }

    private static bool IsSkin(Material material)
    {
        if (material == null || material.mainTexture == null) return false;
        string name = material.mainTexture.name;
        return name.Contains("Regular") || name.Contains("Superhero");
    }

    private void Tint(Renderer target, Color colour, Texture texture, int materialIndex = -1)
    {
        if (target == null) return;
        block.Clear();
        block.SetColor(ColourId, colour);
        if (texture != null) block.SetTexture(TextureId, texture);
        if (materialIndex < 0) target.SetPropertyBlock(block);
        else target.SetPropertyBlock(block, materialIndex);
    }
}
