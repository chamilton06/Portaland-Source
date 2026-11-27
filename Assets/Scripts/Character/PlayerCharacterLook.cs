// AI-generated snippets in this file were copied and integrated by Kartik (kartikeyapro1).
// Tool used: Claude Code in VS Code. Model used: Claude Opus 5.5, medium.
// The comments beside each snippet explain why I needed help and what it does.
// See README.md under Character creation > AI assistance for the declaration.

using UnityEngine;

// Builds the player's character from the look saved in the character creator. Goes on the
// Player. When the scene starts it swaps the default character model for the chosen body, shows
// the chosen look, points the animation scripts at the new model, sets health and stamina for
// the chosen class and puts the class weapon in the player's hand. On a new game it also gives the class and keepsake items, once. If
// nothing has been saved yet, the default character is left as it is.
// AI-generated snippet. I needed help getting the loading order right.
// Run this first so the other player scripts find the new model in Awake.
// Prompt used:
// Help me load the selected character before the other player components
// initialise so they use the new model
[DefaultExecutionOrder(-200)]
public class PlayerCharacterLook : MonoBehaviour
{
    public GameObject malePrefab;
    public GameObject femalePrefab;

    [Header("Starting items (from the class and keepsake)")]
    public Item sword;
    public Item axe;
    public Item healingPotion;
    public Item apple;
    public Item duskroot;
    public Item moonpetal;
    public Item wardleaf;

    private CharacterLook look;

    private void Awake()
    {
        look = CharacterLook.LoadSaved();
        if (look == null) return;

        // Class stats. PlayerStats fills health and stamina up to these in its Start.
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            Origins.Stats origin = Origins.For(look.startingClass);
            stats.maxHealth = origin.health;
            stats.maxStamina = origin.stamina;
        }

        // Start holding the class weapon. PlayerCombat equips this in its Start. The Wanderer
        // keeps whatever the scene gives the player.
        PlayerCombat combat = GetComponent<PlayerCombat>();
        Weapon classWeapon = look.startingClass == StartingClass.Knight ? sword as Weapon
            : look.startingClass == StartingClass.Ranger ? axe as Weapon : null;
        if (combat != null && classWeapon != null) combat.equippedWeapon = classWeapon;

        PlayerLocomotionAnimation locomotion = GetComponent<PlayerLocomotionAnimation>();
        Animator old = locomotion != null ? locomotion.characterAnimator : GetComponentInChildren<Animator>();
        GameObject prefab = look.body == BodyType.Female ? femalePrefab : malePrefab;
        if (old == null || prefab == null)
        {
            Debug.LogWarning("Player Character Look needs the character prefabs and a character model on the Player.", this);
            return;
        }

        // AI-generated model replacement snippet.
        // I needed help putting the selected model into gameplay without replacing Player.
        // Keep the old placement and controller then apply the saved size and appearance.
        // Prompt used:
        // Help me replace the default model with the selected character while keeping
        // the player controller animations and weapon attachment working
        Transform slot = old.transform;
        GameObject model = Instantiate(prefab, slot.parent, false);
        model.transform.localPosition = slot.localPosition;
        model.transform.localRotation = slot.localRotation;
        model.transform.localScale = Vector3.Scale(slot.localScale, new Vector3(look.build, look.height, look.build));
        model.name = prefab.name;
        Animator animator = model.GetComponent<Animator>();
        animator.runtimeAnimatorController = old.runtimeAnimatorController;
        animator.applyRootMotion = false;
        model.GetComponent<CharacterAppearance>().Apply(look);

        old.gameObject.SetActive(false);
        Destroy(old.gameObject);

        // Part of the same AI-generated model replacement solution.
        // Point movement, combat and the weapon at the new Animator.
        // Otherwise they'd still be using the old model.
        if (locomotion != null) locomotion.characterAnimator = animator;
        PlayerCombatAnimation combatAnimation = GetComponent<PlayerCombatAnimation>();
        if (combatAnimation != null) combatAnimation.characterAnimator = animator;
        WeaponHandSocket socket = GetComponent<WeaponHandSocket>();
        if (socket != null) socket.characterAnimator = animator;
    }

    // AI-generated snippet. I needed help waiting for the inventory to be ready.
    // Wait a frame for its Start, then give the class and keepsake items.
    // Remember they've been given so respawning doesn't give them again.
    // Prompt used:
    // Help me give the class and keepsake items after inventory initialisation
    // without giving them again every time the player respawns
    private System.Collections.IEnumerator Start()
    {
        if (look == null || look.startingItemsGiven) yield break;
        yield return null;
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) yield break;
        bool allGiven = true;
        foreach (var (item, amount) in Origins.ClassItems(look.startingClass))
            allGiven &= Give(inventory, item, amount);
        foreach (var (item, amount) in Origins.KeepsakeItems(look.keepsake))
            allGiven &= Give(inventory, item, amount);
        // A missing item reference leaves the flag unset so it can retry.
        // Earlier grants aren't rolled back so a partial failure can duplicate items.
        if (allGiven)
        {
            look.startingItemsGiven = true;
            look.Save();
        }
    }

    private bool Give(Inventory inventory, StartingItem which, int amount)
    {
        Item item = Find(which);
        if (item == null)
        {
            Debug.LogWarning("Player Character Look has no " + which + " set, so it couldn't be given.", this);
            return false;
        }
        inventory.AddItem(item, amount);
        return true;
    }

    private Item Find(StartingItem which)
    {
        switch (which)
        {
            case StartingItem.Sword: return sword;
            case StartingItem.Axe: return axe;
            case StartingItem.HealingPotion: return healingPotion;
            case StartingItem.Apple: return apple;
            case StartingItem.Duskroot: return duskroot;
            case StartingItem.Moonpetal: return moonpetal;
            default: return wardleaf;
        }
    }
}
