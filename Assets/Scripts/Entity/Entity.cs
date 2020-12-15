using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDamageable
{
    [Header("Entity")]
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
    public bool IsDead => isDead;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        health = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int damage) {
        int clampedDamage = Mathf.Clamp(damage - armor, 1, damage); // Guarantee at least one damage.
        health = health - clampedDamage;

        health = Mathf.Clamp(health, 0, maxHealth); // Force health to be between 0 and maxHealth

        if(health == 0) {
            Die();
        }
    }

    protected virtual void Die() {
        isDead = true;
        // TODO: Add animation for death
        Destroy(this.gameObject, 0.1f);
    }
}
