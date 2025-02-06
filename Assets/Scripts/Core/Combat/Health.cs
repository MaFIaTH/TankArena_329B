using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Health : NetworkBehaviour
{ 
    public NetworkVariable<int> currentHealth = new();
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    
    private bool _isDead;
    
    public Action<Health> OnDeath;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        currentHealth.Value = MaxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        ModifyHealth(-damage);
    }
    
    public void RestoreHealth(int health)
    {
        ModifyHealth(health);
    }
    
    private void ModifyHealth(int amount)
    {
       if (_isDead) return;
       int newHealth = Mathf.Clamp(currentHealth.Value + amount, 0, MaxHealth);
       currentHealth.Value = newHealth;
       Debug.Log("Health: " + currentHealth.Value);
       if (currentHealth.Value == 0)
       {
           _isDead = true;
           OnDeath?.Invoke(this);
       }
    }
}
