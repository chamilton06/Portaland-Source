using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour
{
    // Start is called before the first frame update
    public int maxHealth = 100;
    private int currentHealth;
    public UnityEvent onDeath = new UnityEvent();

    // enemy hunger drop
    public GameObject dropPrefab;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || damage <= 0)  // already dead or no damage, stop
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(gameObject.name + " health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " died");
            Debug.Log("Drop prefab: " + (dropPrefab != null ? dropPrefab.name : "NONE"));
            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            onDeath.Invoke();
        }
    }
}
