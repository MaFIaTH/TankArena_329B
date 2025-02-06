using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthBar;
    
    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;
        health.currentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.currentHealth.Value);
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;
        health.currentHealth.OnValueChanged -= HandleHealthChanged;
    }
    
    private void HandleHealthChanged(int previousValue, int newValue)
    {
        healthBar.fillAmount = (float)newValue / health.MaxHealth;
    }
}
