using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")] 
    [SerializeField] private Image healPowerBar;

    [Header("Settings")] 
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healthPerTick = 10;
    
    private float _remainingCooldown;
    private float _tickTimer;
    
    private List<TankPlayer> _playersInZone = new();

    private NetworkVariable<int> HealPower = new();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }
        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (_remainingCooldown > 0f)
        {
            _remainingCooldown -= Time.deltaTime;
            if (_remainingCooldown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= 1 / healTickRate)
        {
            foreach(TankPlayer player in _playersInZone)
            {
                if(HealPower.Value==0) { break; }
                if(player.Health.currentHealth.Value == player.Health.MaxHealth) { continue; }
                if(player.Wallet.TotalCoins.Value < coinsPerTick) { continue; }
                player.Wallet.SpendCoins(coinsPerTick);
                player.Health.RestoreHealth(healthPerTick);
                HealPower.Value -= 1;
                if(HealPower.Value == 0)
                {
                    _remainingCooldown = healCooldown;
                }
            }
            _tickTimer %= (1 / healTickRate);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
        _playersInZone.Add(player);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
        _playersInZone.Remove(player);
    }
    
    private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
    {
        healPowerBar.fillAmount = (float) newHealPower / maxHealPower;
    }
}
