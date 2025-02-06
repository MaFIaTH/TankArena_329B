using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
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
        if (other.attachedRigidbody.TryGetComponent(out NetworkObject networkIdentity))
        {
            if (networkIdentity.OwnerClientId == _ownerClientId)
            {
                return;
            }
        }
        if (other.transform.parent.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}
