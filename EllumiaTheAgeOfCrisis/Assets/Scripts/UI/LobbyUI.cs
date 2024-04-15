using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Lobbies.Models;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button JoinButton;
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private TMP_InputField lobbyNameInp;
    [SerializeField] private TMP_InputField maxPlayersInp;
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button ConfirmCreateLobbyButton;
    [SerializeField] private GameObject joinLobbyPanel;
    [SerializeField] private Button joinWithCodeButton;
    [SerializeField] private TMP_InputField joinCodeInp;
    [SerializeField] private Button joinCloseButton;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;
    private bool inJoinPanel = false;

    private void Awake()
    {
        ConfirmCreateLobbyButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyNameInp.text, Int32.Parse(maxPlayersInp.text), isPrivateToggle.isOn);
        });
        JoinButton.onClick.AddListener(() =>
        {
            OpenJoinLobbyPanel();
        });
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            OpenCreateLobbyPanel();
        });
        joinWithCodeButton.onClick.AddListener(() =>
        {
            JoinWithCode();
        });
        joinCloseButton.onClick.AddListener(() =>
        {
            Close();
        });

        lobbyNameInp.text = "Ellumia Lobby";
        maxPlayersInp.text = "10";

        lobbyTemplate.gameObject.SetActive(false);
    }

    public void OpenCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(true);
        inJoinPanel = false;
    }

    public void Close()
    {
        if (inJoinPanel)
        {
            joinLobbyPanel.SetActive(false);
        }
        else
        {
            createLobbyPanel.SetActive(false);
        }

    }

    public void OpenJoinLobbyPanel()
    {
        joinLobbyPanel.SetActive(true);
        inJoinPanel = true;
    }

    public void JoinWithCode()
    {
        if(joinCodeInp.text!=""){
            GameLobby.Instance.JoinWithCode(joinCodeInp.text);
        }
    }

    private void UpdateLobbyList (List<Lobby> lobbyList) {
        foreach(Transform child in lobbyContainer){
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList){
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void Start() {
        GameLobby.Instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void GameLobby_OnLobbyListChanged (object sender, GameLobby.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }
}
