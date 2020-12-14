using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [Tooltip("What team the Entity is on. Value of negative one denotes neutral/unaligned.")]
    [SerializeField]
    private int team = -1;
    public int Team => team;

    [SerializeField]
    private int maxHealth;

    [SerializeField]
    private int armor;

    private int health;

    private bool isDead;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        health = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damage) {
        int newHealth = health - (damage - armor);

        newHealth = Mathf.Clamp(newHealth, 1, health); // Guarantee to do atleast 1 damage.

        health = Mathf.Clamp(newHealth, 0, maxHealth); // Force health to be between 0 and maxHealth

        if(health == 0) {
            Die();
        }
    }

    protected virtual void Die() {
        isDead = true;
    }
}
