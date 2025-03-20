using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")] 
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin bountyCoinPrefab;

    [Header("Settings")] 
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 0.5f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 10;
    [SerializeField] private LayerMask layerMask;

    private float coinRadius;
    private Collider2D[] coinBuffer = new Collider2D[1];
    
    public NetworkVariable<int> TotalCoins = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
        health.OnDeath += HandleDeath;
    }
    
    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        health.OnDeath -= HandleDeath;
    }

    private void HandleDeath(Health obj)
    {
        int bountyValue = Mathf.CeilToInt(TotalCoins.Value * bountyPercentage);
        int bountyCoinValue = bountyValue / bountyCoinCount;
        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin bountyCoin = Instantiate(bountyCoinPrefab, GetSpawnPoint(), Quaternion.identity);
            bountyCoin.SetValue(Mathf.Max(minBountyCoinValue, bountyCoinValue));
            bountyCoin.NetworkObject.Spawn();
        }
    }

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
    
    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            ContactFilter2D contactFilter2D = new ContactFilter2D();
            contactFilter2D.layerMask = layerMask;
            int numColliders = Physics2D.OverlapCircle(spawnPoint, coinRadius, contactFilter2D, coinBuffer);
            if (numColliders == 0) return spawnPoint;
        }
    }
}
