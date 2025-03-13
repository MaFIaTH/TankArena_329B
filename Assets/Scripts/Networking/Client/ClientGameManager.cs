using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;
    private NetworkClient _networkClient;
    private const string MenuSceneName = "Menu";
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        _networkClient = new NetworkClient(NetworkManager.Singleton);
        AuthState authState = await AuthenticationWrapper.DoAuth();
        return authState == AuthState.Authenticated;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return;
        }
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        UserData userData = new UserData
        {
            username = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"), 
            userAuthId = AuthenticationService.Instance.PlayerId,
            userColorIndex = PlayerPrefs.GetInt(ColorSelector.PlayerColorKey, 0)
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        _networkClient?.Dispose();
    }
}
