using UnityEngine;

// Builds the player's character from the look saved in the character creator. Goes on the
// Player. When the scene starts it swaps the default character model for the chosen body, shows
// the chosen look, points the animation scripts at the new model and sets health and stamina
// for the chosen class. On a new game it also gives the class and keepsake items, once. If
// nothing has been saved yet, the default character is left as it is.
// Runs before the other Player scripts so they pick up the new model in their own Awake.
[DefaultExecutionOrder(-200)]
public class PlayerCharacterLook : MonoBehaviour
{
    public GameObject malePrefab;
    public GameObject femalePrefab;

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

        PlayerLocomotionAnimation locomotion = GetComponent<PlayerLocomotionAnimation>();
        Animator old = locomotion != null ? locomotion.characterAnimator : GetComponentInChildren<Animator>();
        GameObject prefab = look.body == BodyType.Female ? femalePrefab : malePrefab;
        if (old == null || prefab == null)
        {
            Debug.LogWarning("Player Character Look needs the character prefabs and a character model on the Player.", this);
            return;
        }

        // Same place, size and controller as the model it replaces.
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

        if (locomotion != null) locomotion.characterAnimator = animator;
        PlayerCombatAnimation combatAnimation = GetComponent<PlayerCombatAnimation>();
        if (combatAnimation != null) combatAnimation.characterAnimator = animator;
        WeaponHandSocket socket = GetComponent<WeaponHandSocket>();
        if (socket != null) socket.characterAnimator = animator;
    }

    // The inventory sets itself up in Start, so wait a frame before adding to it.
    private System.Collections.IEnumerator Start()
    {
        if (look == null || look.startingItemsGiven) yield break;
        yield return null;
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) yield break;
        foreach (var (path, amount) in Origins.ClassItems(look.startingClass))
            Give(inventory, path, amount);
        foreach (var (path, amount) in Origins.KeepsakeItems(look.keepsake))
            Give(inventory, path, amount);
        look.startingItemsGiven = true;
        look.Save();
    }

    private void Give(Inventory inventory, string path, int amount)
    {
        Item item = Resources.Load<Item>(path);
        if (item != null) inventory.AddItem(item, amount);
        else Debug.LogWarning("Starting item not found: Resources/" + path, this);
    }
}
