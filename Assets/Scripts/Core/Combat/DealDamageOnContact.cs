using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private int damage = 5;

    private ulong _ownerClientId;
    
    public void SetOwner(ulong clientId)
    {
        _ownerClientId = clientId;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null)
        {
            return;
        }

        if (projectile.TeamIndex != 0)
        {
            if (other.TryGetComponent(out TankPlayer player))
            {
                if (player.TeamIndex.Value == projectile.TeamIndex) return;
            }
        }
        if (other.attachedRigidbody.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}
