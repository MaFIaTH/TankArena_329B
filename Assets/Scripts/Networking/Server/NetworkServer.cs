using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;
    private NetworkObject _playerPrefeb;
    
    private Dictionary<ulong, string> _clientIdToAuth = new();
    private Dictionary<string, UserData> _authIdToUserData = new();

    public static Action<string> OnClientLeft;
    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefeb)
    {
        _networkManager = networkManager;
        _playerPrefeb = playerPrefeb;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }
    
    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = _networkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);
        return _networkManager.StartServer();
    }

    private void OnNetworkReady()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong obj)
    {
        if(_clientIdToAuth.TryGetValue(obj, out string authId))
        {
            _clientIdToAuth.Remove(obj);
            OnUserLeft?.Invoke(_authIdToUserData[authId]);
            _authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);
        _clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        _authIdToUserData[userData.userAuthId] = userData;
        OnUserJoined?.Invoke(userData);
        //Debug.Log($"User {userData.username} is trying to connect");
        _ = SpawnPlayerDelayed(request.ClientNetworkId);
        response.Approved = true;
        response.CreatePlayerObject = false;

    }

    public void Dispose()
    {
        if (_networkManager == null) return;
        _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        _networkManager.OnServerStarted -= OnNetworkReady;
        _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        if (_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
    
    public UserData GetUserDataByClientId(ulong clientId)
    {
        if(_clientIdToAuth.TryGetValue(clientId,out string authId))
        {
            return _authIdToUserData.TryGetValue(authId,out UserData data) ? data : null;
        }
        return null;
    }
    
    private async Task SpawnPlayerDelayed(ulong clientId)
    {
        await Task.Delay(1000);
        NetworkObject playerInstance = GameObject.Instantiate(_playerPrefeb,
            SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.SpawnAsPlayerObject(clientId);

    }
}
