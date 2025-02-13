using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new();

    private void OnTriggerEnter2D(Collider2D col)
    {
         if (!col.TryGetComponent<Coin>(out var coin)) return;
         int coinValue = coin.Collect();
         if (!IsServer) return;
         TotalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }
}
