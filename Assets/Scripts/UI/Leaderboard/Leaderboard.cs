using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderBoardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityDisplay;
    [SerializeField] private int entitiesToDisplay = 8;
    private NetworkList<LeaderboardEntityState> _leaderboardEntities = new();
    private readonly List<LeaderboardEntityDisplay> _entityDisplays = new();

    private void Awake()
    {
        _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _leaderboardEntities.OnListChanged += HandleLeaderboardEntityListChanged;
            foreach (var entity in _leaderboardEntities)
            {
                HandleLeaderboardEntityListChanged(new()
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }
        if (!IsServer) return;
        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _leaderboardEntities.OnListChanged -= HandleLeaderboardEntityListChanged;
        }
        if (!IsServer) return;
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }
    
    private void HandleLeaderboardEntityListChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        if (!gameObject.scene.isLoaded) return;
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (_entityDisplays.All(x => x.CliendId != changeEvent.Value.clientId))
                {
                    LeaderboardEntityDisplay leaderboardEntity =
                        Instantiate(leaderboardEntityDisplay, leaderBoardEntityHolder);
                    leaderboardEntity.Initialise(changeEvent.Value.clientId, changeEvent.Value.playerName, changeEvent.Value.coins);
                    _entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove =
                    _entityDisplays.FirstOrDefault(x => x.CliendId == changeEvent.Value.clientId);
                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    _entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate =
                    _entityDisplays.FirstOrDefault(x => x.CliendId == changeEvent.Value.clientId);
                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.coins);
                }
                break;
        }
        _entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        for (int i = 0; i < _entityDisplays.Count; i++)
        {
            _entityDisplays[i].transform.SetSiblingIndex(i);
            _entityDisplays[i].UpdateText();
            _entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }

        LeaderboardEntityDisplay myDisplay =
            _entityDisplays.FirstOrDefault(x => x.CliendId == NetworkManager.Singleton.LocalClientId);
        
        if (myDisplay == null) return;
        if (myDisplay.transform.GetSiblingIndex() < entitiesToDisplay) return;
        leaderBoardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
        myDisplay.gameObject.SetActive(true);
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        _leaderboardEntities.Add(new LeaderboardEntityState
        {
            clientId = player.OwnerClientId,
            playerName = player.PlayerName.Value,
            coins = 0
        });
        player.Wallet.TotalCoins.OnValueChanged += (_, newCoin) => HandleCoinChanged(player.OwnerClientId, newCoin);
    }

    private void HandleCoinChanged(ulong clientId, int newCoins)
    {
        for(int i = 0; i < _leaderboardEntities.Count; i++)
        {   
            if (_leaderboardEntities[i].clientId != clientId) { continue; }
            _leaderboardEntities[i] = new LeaderboardEntityState
            {
                clientId = _leaderboardEntities[i].clientId,
                playerName = _leaderboardEntities[i].playerName,
                coins = newCoins
            };
            return;
        }
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        if (NetworkManager.ShutdownInProgress) return;
        foreach(LeaderboardEntityState entity in _leaderboardEntities)
        {
            if(entity.clientId != player.OwnerClientId) { continue; }
            _leaderboardEntities.Remove(entity);
            break;
        }
        player.Wallet.TotalCoins.OnValueChanged -= (_, newCoin) => HandleCoinChanged(player.OwnerClientId, newCoin);
    }    
}
