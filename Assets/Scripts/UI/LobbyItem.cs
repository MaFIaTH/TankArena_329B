using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;

    [SerializeField] private TMP_Text lobbyPlayerText;
    private LobbyList _lobbyList;
    private Lobby _lobby;
    public void Intialize(LobbyList lobbyList, Lobby lobby)
    {
        _lobbyList = lobbyList;
        _lobby = lobby;
        lobbyNameText.text = lobby.Name;
        lobbyPlayerText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Join()
    {
        _lobbyList.JoinAsync(_lobby);
    }
}
