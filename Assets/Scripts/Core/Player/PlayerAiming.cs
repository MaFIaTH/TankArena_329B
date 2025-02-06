using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;
    
    private Vector2 _aimPosition;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.AimEvent += HandleAim;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        inputReader.AimEvent -= HandleAim;
    }
    
    private void HandleAim(Vector2 vector)
    {
        _aimPosition = vector;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!IsOwner) return;
        var aimScreenPosition = _aimPosition;
        var aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
        turretTransform.up = new Vector2(aimWorldPosition.x - turretTransform.position.x, aimWorldPosition.y - turretTransform.position.y);
    }
}
