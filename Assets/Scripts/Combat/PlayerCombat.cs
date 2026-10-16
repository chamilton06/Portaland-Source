using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public WeaponStats equippedWeapon;

    public bool weaponEquipped = true;
    public bool inventoryOpen = false;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (!weaponEquipped || inventoryOpen|| equippedWeapon == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + equippedWeapon.attackCooldown;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, equippedWeapon.attackRange))
            {
                Health targetHealth = hit.collider.GetComponentInParent<Health>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(equippedWeapon.damage);
                }
            }
        }
    }
}