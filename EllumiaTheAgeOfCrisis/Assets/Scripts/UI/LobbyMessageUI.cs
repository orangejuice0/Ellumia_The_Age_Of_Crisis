using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private Button Close;
    [SerializeField] private TMP_Text Message;
    [SerializeField] private GameObject messagePanel;

    private void Awake()   {
        Close.onClick.AddListener(CloseButton);
    }

    public void CloseButton()
    {
        messagePanel.SetActive(false);
    }

    private void Start() {
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreateLobbyFailed;
        GameLobby.Instance.OnFailedToJoinGame += GameLobby_OnFailedToJoinGame;
        GameLobby.Instance.OnJoinLobbyStarted += GameLobby_OnJoinLobbyStarted;
        GameLobby.Instance.OnJoinLobbyWithCodeFailed += GameLobby_OnJoinLobbyWithCodeFailed;
        GameLobby.Instance.OnLobbyJoined += GameLobby_OnLobbyJoined;
        GameLobby.Instance.OnLobbyCreated += GameLobby_OnLobbyCreated;
    }

    private void GameLobby_OnCreateLobbyStarted (object sender, System.EventArgs e) {
        ShowMessage("Creating Lobby...");
    }

    private void GameLobby_OnCreateLobbyFailed (object sender, System.EventArgs e) {
        ShowMessage("Failed to Create Lobby!");
    }

    private void GameLobby_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        if(NetworkManager.Singleton.DisconnectReason==""){
            ShowMessage("Failed to connect!");
        }
        else{
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void GameLobby_OnJoinLobbyStarted(object sender, System.EventArgs e){
        ShowMessage("Joining Lobby...");
    }

    private void GameLobby_OnJoinLobbyWithCodeFailed (object sender, System.EventArgs e) {
        ShowMessage("Failed to Join Private Lobby!");
    }

    private void GameLobby_OnLobbyJoined(object sender, System.EventArgs e){
        messagePanel.SetActive(false);
    }

    private void GameLobby_OnLobbyCreated(object sender, System.EventArgs e)
    {
        messagePanel.SetActive(false);
    }

    private void ShowMessage(string message){
        Show();
        Message.text=message;
    }

    private void Show(){
        messagePanel.SetActive(true);
    }

}
