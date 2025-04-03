using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text queueStatusText;
    [SerializeField] private TMP_Text queueTimerText;
    [SerializeField] private TMP_Text findMatchButtonText;
    [SerializeField] private TMP_InputField joinCodeField;
    
    private bool isMatchmaking;
    private bool isCancelling;
    private bool isBusy;
    private float timeInQueue;
    
    
    private void Start()
    {
        if (ClientSingleton.Instance == null)
        {
            return;
        }
        Cursor.SetCursor(null,Vector2.zero,CursorMode.Auto);
        queueStatusText.text = string.Empty;
        queueTimerText.text = string.Empty;
    }

    private void Update()
    {
        if (isMatchmaking)
        {
            timeInQueue += Time.deltaTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInQueue);
            queueTimerText.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
    }

    public async void FindMatchPressed()
    {
        if(isCancelling) { return; }
        
        if (isMatchmaking)
        {
            queueStatusText.text = "Cancelling...";
            isCancelling = true;
            // Cancel matchmaking
            await ClientSingleton.Instance.GameManager.CancelMatchmaking();
            isCancelling = false;
            isMatchmaking = false;
            isBusy = false;
            findMatchButtonText.text = "Find Match";
            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
            return;
        }

        if (isBusy) return;
        // Start queue
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMode);
        findMatchButtonText.text = "Cancel";
        queueStatusText.text = "Searching...";
        timeInQueue = 0;
        isMatchmaking = true;
        isBusy = true;
    }

    private void OnMatchMode(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatusText.text = "Connecting...";
                break;
            case MatchmakerPollingResult.TicketCreationError:
                queueStatusText.text = "Ticket Creation Error";
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                queueStatusText.text = "Ticket Cancellation Error";
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                queueStatusText.text = "Ticket Retrieval Error";
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                queueStatusText.text = "Match Assignment Error";
                break;
        }
    }

    public async void StartHost()
    {
        if (isBusy) return;
        isBusy = true;
        await HostSingleton.Instance.GameManager.StartHostAsync();
        isBusy = false;
    }
    public async void StartClient()
    {
        if (isBusy) return;
        isBusy = true;
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        isBusy = false;
    }
    
    public async void JoinAsync(Lobby lobby)
    {
        if (isBusy) return;
        isBusy = true;
        try
        {
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
        }

        isBusy = false;
    }
}
