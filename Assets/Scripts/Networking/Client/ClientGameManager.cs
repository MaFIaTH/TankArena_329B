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
    private MatchplayMatchmaker _matchmaker;
    public UserData UserData { get; private set; }
    private const string MenuSceneName = "Menu";
    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        _networkClient = new NetworkClient(NetworkManager.Singleton);
        _matchmaker = new MatchplayMatchmaker();
        AuthState authState = await AuthenticationWrapper.DoAuth();
        if (authState != AuthState.Authenticated) return false;
        UserData = new UserData
        {
            username = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"), 
            userAuthId = AuthenticationService.Instance.PlayerId,
            userColorIndex = PlayerPrefs.GetInt(ColorSelector.PlayerColorKey, 0)
        };
        return true;
    }

    public async void MatchmakeAsync(bool team, Action<MatchmakerPollingResult> onMatch)
    {
        if (_matchmaker.IsMatchmaking) return;
        UserData.userGamePreferences.gameQueue = team ? GameQueue.Team : GameQueue.Solo;
        var matchResult = await GetMatchAsync();
        onMatch?.Invoke(matchResult);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        var matchResult = await _matchmaker.Matchmake(UserData);

        if (matchResult.result is MatchmakerPollingResult.Success)
        {
            StartClient(matchResult.ip, matchResult.port);
        }

        return matchResult.result;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public void StartClient(string ip, int port)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        ConnectClient();
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
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);
        ConnectClient();
    }

    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(UserData);
        byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    public void Dispose()
    {
        _networkClient?.Dispose();
    }

    public void Disconnect()
    {
        _networkClient?.Disconnect();
    }

    public async Task CancelMatchmaking()
    {
        await _matchmaker.CancelMatchmaking();
    }
}
