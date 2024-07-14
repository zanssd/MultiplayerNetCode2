using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Match
{
    public string matchID;
    public bool matchFull;
    public List<Player> players = new List<Player>();


    public Match(string matchID, Player player)
    {
        matchFull = false;
        this.matchID = matchID;
        players.Add(player);

    }

    public Match() { }
}
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }
    public List<Match> matches = new List<Match>();
    public List<ulong> connectedPlayers = new List<ulong>();
    public List<Player> connectedPlayersSc= new List<Player>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            if(IsHost) AddHostPlayer(); 
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

    }

    private void AddHostPlayer()
    {
        ulong hostClientId = NetworkManager.Singleton.LocalClientId;
        AddPlayerToList(hostClientId);
    }

    private void AddPlayerToList(ulong clientId)
    {
        connectedPlayers.Add(clientId);
        connectedPlayersSc.Add(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject.GetComponent<Player>());
        Debug.Log($"Client {clientId} connected.");
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            connectedPlayers.Add(clientId);
            connectedPlayersSc.Add(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject.GetComponent<Player>());
            Debug.Log($"Client {clientId} connected.");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            connectedPlayers.Remove(clientId);
            connectedPlayersSc.Remove(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject.GetComponent<Player>());
            Debug.Log($"Client {clientId} disconnected.");
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            foreach (Player item in connectedPlayersSc)
            {
                Debug.Log("player ID " + item.clientId + " player name" + item.playerNameText);
            }
        }
    }

    public List<ulong> GetConnectedPlayers()
    {
        return new List<ulong>(connectedPlayers);
    }

    public void GetPlayerOnMainLobby(ulong clientID, out List<Player> player)
    {
        player = null;
        player = connectedPlayersSc;
    }

    public void GetPlayerOnRoom(string matchId,out List<Player> playerSc, out bool isFound)
    {
        Debug.Log("TRY GET PLAYER " + matchId);
        isFound = false;
        playerSc = null;
        //playerSc = connectedPlayersSc;

        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchId)
            {
                Debug.Log("FOUNDED " + matchId);
                isFound = true;
                playerSc = matches[i].players;
            }
        }
    }
    public void InvitedPlayer(ulong clientID, string matchID, string fromPlayer)
    {
        foreach (Player player in connectedPlayersSc)
        {
            if(player.clientId == clientID)
            {
                player.InvitePlayerClientRpc(clientID,matchID, fromPlayer, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } } });
            }
        }
    }

    //ROOM
    public void PlayerCreateRoom(ulong clientId,string roomname)
    {
        foreach (Player item in connectedPlayersSc)
        {
            Debug.Log("SERVER SIDE PLAYER " + clientId + " " + item.clientId);
            if(clientId == item.clientId)
            {
                Debug.Log("BERHASIL NEMU MAKE ROOM");
                item.CreateRoomClientRpc(roomname, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } });
                item.playerRoom = roomname;
                Match match = new Match(roomname, item);
                matches.Add(match);
            }
        }
    }

    public void JoinGame(string matchId, ulong clientID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchId)
            {
                foreach (Player player in connectedPlayersSc)
                {
                    if (player.clientId == clientID)
                    {
                        {
                            matches[i].players.Add(player);
                            UpdatedRoom(matchId, clientID);
                            player.JoinRoomClientRpc(matchId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } } });
                        }
                    }
                }
            }
        }
    }

    private void UpdatedRoom(string matchID, ulong clientID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchID)
            {
                foreach (Player player in matches[i].players)
                {
                    if(player.clientId != clientID)
                    {
                        player.UpdateRoomClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player.clientId } } });
                    }
                }
            }
        }
    }

    public void AssginPrefabCardAnotherPLayer(ulong clientID, string matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchID)
            {
                foreach (Player player in matches[i].players)
                {
                    player.AssignPrefabCardClientRpc();
                }
            }
        }
    }

    public void CheckerBeginGame(string matchID)
    {
        int playerReady = 0;
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchID)
            {
                foreach (Player player in matches[i].players)
                {
                    if(player.isReady.Value)
                    {
                        playerReady++;
                    }
                }
                if(playerReady == matches[i].players.Count)
                {
                    BeginGame(matchID);
                }
            }
        }
    }
    public void BeginGame(string matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].matchID == matchID)
            {
                foreach (Player player in matches[i].players)
                {
                    player.BeginGameClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player.clientId } } });
                }
            }
        }
    }
}
