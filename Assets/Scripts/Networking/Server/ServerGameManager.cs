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
using Unity.Services.Matchmaker.Models;

public class ServerGameManager : IDisposable
{
    
    private string serverIP;
    private int serverPort;
    private int queryPort;
    private MatchplayBackfiller backfiller;
    public NetworkServer NetworkServer { get; private set; }
    private MultiplayAllocationService multiplayAllocationService;
    private const string GameSceneName = "Game";
    public ServerGameManager(string serverIP,int serverPort,int queryPort, NetworkManager manager,NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        NetworkServer = new NetworkServer(manager, playerPrefab);
        multiplayAllocationService = new MultiplayAllocationService();
    }
#if UNITY_SERVER
    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            var payload = await GetMatchmakerPayload();
            if (payload != null)
            {
                await StartBackfill(payload);
                NetworkServer.OnUserJoined += UserJoined;
                NetworkServer.OnUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload timeout");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
        
        if (!NetworkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogError("Failed to start server");
            return;
        }
    }
    
    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        var payloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();
        
        if (await Task.WhenAny(payloadTask, Task.Delay(20000)) == payloadTask)
        {
            return payloadTask.Result;   
        }

        return null;
    }
    
    private void UserJoined(UserData user)
    {
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }
    
    private void UserLeft(UserData user)
    {
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();
        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }
        
        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
    }
    
    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }
    
    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 20);
        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }
    #endif

    
    public void Dispose()
    {   
        #if UNITY_SERVER
        NetworkServer.OnUserJoined -= UserJoined;
        NetworkServer.OnUserLeft -= UserLeft;
        backfiller?.Dispose();
        multiplayAllocationService?.Dispose();
        NetworkServer?.Dispose();
        #endif
    }
}