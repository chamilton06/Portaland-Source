using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 2.5f;
    public int damage = 10;
    public float attackCooldown = 0.8f;
    private float nextAttackTime = 0f;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            
            nextAttackTime = Time.time + attackCooldown;
        
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
               Health targetHealth = hit.collider.GetComponentInParent<Health>();

if (targetHealth != null)
{
    targetHealth.TakeDamage(damage);
}
            }
        }
    }}
}