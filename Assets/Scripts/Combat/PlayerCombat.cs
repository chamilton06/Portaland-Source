using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public WeaponStats equippedWeapon;

    public bool weaponEquipped = true;
    public bool inventoryOpen = false;

    private float nextAttackTime = 0f;
    private bool swinging = false;

    void Update()
    {
        if (!weaponEquipped || inventoryOpen|| equippedWeapon == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + equippedWeapon.attackCooldown;
            if (!swinging)
            {
                StartCoroutine(SwingWeapon());
            }

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
    IEnumerator SwingWeapon()
{
    swinging = true;

    Transform weapon = equippedWeapon.transform;
    Quaternion startRotation = weapon.localRotation;
    Quaternion swingRotation = startRotation * Quaternion.Euler(60f, 0f, -40f);

    float duration = 0.15f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        weapon.localRotation = Quaternion.Slerp(
            startRotation, swingRotation, Mathf.Clamp01(elapsed / duration));

        yield return null;
    }

    elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        weapon.localRotation = Quaternion.Slerp(
            swingRotation, startRotation, Mathf.Clamp01(elapsed / duration));

        yield return null;
    }

    weapon.localRotation = startRotation;
    swinging = false;
}
}