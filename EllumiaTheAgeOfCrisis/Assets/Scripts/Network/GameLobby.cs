using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }

    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinLobbyStarted;
    public event EventHandler OnJoinLobbyWithCodeFailed;
    public event EventHandler OnLobbyCreated;
    public event EventHandler OnLobbyJoined;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }


    private Lobby joinedLobby;
    private float heartBeatTimer = 0, lobbiesListTimer = 0;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitialiseUnityAuthentication();
    }


    private async void InitialiseUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });

            NetworkManager.Singleton.StartHost();
            OnLobbyCreated?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }

    }


    public async void QuickJoin()
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            NetworkManager.Singleton.StartClient();
            OnLobbyJoined?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        }

    }

    public async void JoinWithCode(string lobbyCode)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            NetworkManager.Singleton.StartClient();
            OnLobbyJoined?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinLobbyWithCodeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithID(string lobbyID)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);
            NetworkManager.Singleton.StartClient();
            OnLobbyJoined?.Invoke(this, EventArgs.Empty);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicListLobbies();
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0)
            {
                heartBeatTimer = 15;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private void HandlePeriodicListLobbies()
    {
        if (joinedLobby == null)
        {
            lobbiesListTimer -= Time.deltaTime;
            if (lobbiesListTimer <= 0)
            {
                lobbiesListTimer = 5;
                ListLobbies();
            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>{
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                )
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


}
