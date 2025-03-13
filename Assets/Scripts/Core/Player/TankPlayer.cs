using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")] 
    [SerializeField]
    private CinemachineCamera virtualCamera;

    [SerializeField] private Texture2D crosshair;
    
    [field: SerializeField] public Health Health { get; private set; }

    [Header("Settings")] 
    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName = new();
    public NetworkVariable<int> PlayerColorIndex = new();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            PlayerName.Value = userData.username;
            PlayerColorIndex.Value = userData.userColorIndex;
            OnPlayerSpawned?.Invoke(this);
        }
        if (IsOwner)
        {
            virtualCamera.Priority = ownerPriority;
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width/2, crosshair.height/2), CursorMode.Auto);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
