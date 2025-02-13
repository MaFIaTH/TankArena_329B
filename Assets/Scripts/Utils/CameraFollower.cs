using Unity.Netcode;
using UnityEngine;

public class CameraFollower : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        base.OnNetworkSpawn();
        Camera.main.transform.SetParent(transform);
    }
}
