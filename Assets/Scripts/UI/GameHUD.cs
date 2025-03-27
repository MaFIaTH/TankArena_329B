using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : NetworkBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;
    private NetworkVariable<FixedString32Bytes> _lobbyCode = new("");

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
            HandleLobbyCodeChanged(string.Empty, _lobbyCode.Value);
        }
        if (!IsHost) return;
        _lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;
        _lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
    }

    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        lobbyCodeText.SetText(newCode.ToString());
    }

    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }
        
        ClientSingleton.Instance.GameManager.Disconnect();
    }
}
