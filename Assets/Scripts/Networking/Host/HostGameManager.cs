using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string _joinCode;
    private string _lobbyId;
    public NetworkServer NetworkServer { get; private set; }
    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    private const string JoinCodeKey = "JoinCode";

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(_joinCode);
            PlayerPrefs.SetString(JoinCodeKey, _joinCode);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode)
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            _lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
            return;
        }
        NetworkServer = new NetworkServer(NetworkManager.Singleton);
        UserData userData = new UserData
        {
            username = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"), 
            userAuthId = AuthenticationService.Instance.PlayerId,
            userColorIndex = PlayerPrefs.GetInt(ColorSelector.PlayerColorKey, 0)
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartHost();
        NetworkServer.OnClientLeft += HandleClientLeft;
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private async void HandleClientLeft(string obj)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_lobbyId, obj);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }
    }

    private IEnumerator HeartbeatLobby(float waitTime)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void Dispose()
    {
        Shutdown();
    }

    public async void Shutdown()
    {
        HostSingleton.Instance.StopCoroutine(HeartbeatLobby(15));
        
        if (!string.IsNullOrEmpty(_lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            _lobbyId = string.Empty;
        }
        
        NetworkServer.OnClientLeft -= HandleClientLeft;
        NetworkServer?.Dispose();
    }
}