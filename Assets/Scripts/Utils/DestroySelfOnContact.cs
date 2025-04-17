using System;
using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (projectile.TeamIndex != 0)
        {
            if (other.TryGetComponent(out TankPlayer player))
            {
                if (player.TeamIndex.Value == projectile.TeamIndex) return;
            }
        }
        Destroy(gameObject);
    }
}
