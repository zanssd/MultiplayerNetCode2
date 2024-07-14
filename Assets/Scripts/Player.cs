using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

using TMPro;
using System.Collections;

public class Player : NetworkBehaviour
{
    public GameObject playerPrefab;
    public NetworkObject playerCharPrefab;
    public static Player instance;
    public ulong clientId;
    public string playerRoom;
    public Room roomSc;
    public string playerName1 = "Player";
    public GameObject playerCard;

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>();



    public string playerNameText;

    private void Start()
    {

        if (IsOwner && string.IsNullOrEmpty(playerName.Value.ToString()))
        {
            instance = this;
            string randomName = GenerateRandomName();
            SetPlayerNameServerRpc(randomName);
            
        }
        clientId = OwnerClientId;
        UpdatePlayerNameUI();

        playerName.OnValueChanged += (oldValue, newValue) => UpdatePlayerNameUI();
        isReady.OnValueChanged += OnStateReadyChanged;
    }

    private void UpdatePlayerNameUI()
    {
        if (playerNameText != null)
        {
            playerNameText = playerName.Value.ToString();
        }
    }
    public void OnStateReadyChanged(bool previous, bool current)
    {
        if (isReady.Value)
        {
            playerCard.GetComponent<UIPlayerCard>().state.text = "Ready";
        }
        else
        {
            playerCard.GetComponent<UIPlayerCard>().state.text = "Not Ready";


        }
    }

    public void ReadyState()
    {
        if (!IsOwner) return;
        ToggleReadyServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc()
    {

        isReady.Value = !isReady.Value;
    }
    [ServerRpc]
    public void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    private string GenerateRandomName()
    {
        int randomNumber = Random.Range(100, 1000); 
        return "Player " + randomNumber;
    }

    public void OnNameChangeButtonClicked(string newName)
    {
        if (IsOwner)
        {
            SetPlayerNameServerRpc(newName);
        }
    }
    public void GetPlayerOnMainLobby()
    {
        CmdGetPlayerOnMainLobbyServerRpc(clientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void CmdGetPlayerOnMainLobbyServerRpc(ulong clientID)
    {
        Debug.Log("Get Players");
        LobbyManager.Instance.GetPlayerOnMainLobby(clientID, out List<Player> players);
        foreach(Player playerList in players)
        {
            Debug.Log("Player Name " + playerList.playerNameText + " PlayerID " + playerList.clientId);
            SendWelcomeMessageClientRpc(playerList.playerNameText, playerList.clientId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } });
        }

    }

    [ClientRpc]
    private void SendWelcomeMessageClientRpc(string playerName, ulong playerID, ClientRpcParams clientRpcParams = default)
    {
        if (playerID == clientId) return;

        Debug.Log("Add PLayer");
        UILobbyCanvasManager.instance.GetPlayerForInvList(playerName, playerID);
    }

    [ServerRpc]
    public void InvitePlayerServerRpc(ulong playerID, string matchID)
    {
        Debug.Log("INVITE PLAYER " + playerID + " FROM " + clientId);
        LobbyManager.Instance.InvitedPlayer(playerID, matchID, playerNameText);
    }
    [ClientRpc]
    public void InvitePlayerClientRpc(ulong playerID, string matchID, string fromPlayer,ClientRpcParams clientRpcParams = default)
    {

        Debug.Log("Halo "+ playerID + " " + playerNameText );
        UILobbyCanvasManager.instance.invitationPopUp.SetActive(true);
        InvitationPopUp invitationPopUp = UILobbyCanvasManager.instance.invitationPopUp.GetComponent<InvitationPopUp>();
        invitationPopUp.title.text = $"{fromPlayer} invited you play together";
        invitationPopUp.yesButton.onClick.AddListener(() =>
        {
            PlayerJoinGame(matchID);
        });
    }

    [ServerRpc]
    public void CreateLobbyServerRpc(ulong clientId, string matchID)
    {
        Debug.Log("Server Side Player " + clientId + " Make Room");

        LobbyManager.Instance.PlayerCreateRoom(clientId, matchID);
    }

    [ClientRpc]
    public void CreateRoomClientRpc(string roomName, ClientRpcParams clientRpcParams = default)
    {
        UILobbyCanvasManager.instance.CreateRoomPlayer(roomName);
    }

    public void PlayerJoinGame(string matchID)
    {
        playerRoom = matchID;
        PlayerJoinGameServerRpc(matchID, clientId);;
    }


    [ServerRpc]
    private void PlayerJoinGameServerRpc(string matchID, ulong clientID)
    {
        LobbyManager.Instance.JoinGame(matchID,clientID);
    }

    [ClientRpc]
    public void JoinRoomClientRpc(string roomName, ClientRpcParams clientRpcParams = default)
    {
        UILobbyCanvasManager.instance.JoinRoom(roomName);
    }

    [ClientRpc]
    public void UpdateRoomClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(GetPlayerInRoom());
    }
    public IEnumerator GetPlayerInRoom()
    {
        yield return new WaitForSeconds(0.3f);
        roomSc.ClearPlayer();
        yield return new WaitForSeconds(0.5f);
        GetPlayerInRoomServerRpc(clientId,playerRoom);
        
    }
    [ServerRpc]
    public void GetPlayerInRoomServerRpc(ulong clientID,string matchID)
    {
        LobbyManager.Instance.GetPlayerOnRoom(matchID,out List<Player> players, out bool isFound);
        if(isFound)
        {
            foreach (Player playerList in players)
            {
                Debug.Log("Player Name " + playerList.playerNameText + " PlayerID " + playerList.clientId);
                AddPlayerInRoomClientRpc(playerList.playerNameText, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientID } } });
            }
        }
      
    }
    [ClientRpc]
    private void AddPlayerInRoomClientRpc(string playerName,ClientRpcParams clientRpcParams = default)
    {
        roomSc.SpawnPlayerPrefabUI(playerName, this);
    }

    [ServerRpc]
    public void AssginPrefabCardServerRpc(ulong clientID, string matchID)
    {
        LobbyManager.Instance.AssginPrefabCardAnotherPLayer(clientID, matchID);
    }

    [ClientRpc]
    public void AssignPrefabCardClientRpc()
    {
        if(roomSc == null)
        {
            roomSc = Player.instance.roomSc;
        }
        roomSc.AssignAnotherPLayerPrefabUI(this);
    }
    public void BeginGame()
    {
        BeginGameServerRpc(playerRoom);
    }
    [ServerRpc]
    public void BeginGameServerRpc(string matchID)
    {
        LobbyManager.Instance.CheckerBeginGame(matchID);
    }
    [ClientRpc]
    public void BeginGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        SpawnPlayerServerRpc(clientId);
        UILobbyCanvasManager.instance.canvasLobby.SetActive(false);
    }

    [ServerRpc]
    public void SpawnPlayerServerRpc(ulong clientID)
    {
        NetworkObject playerInstance = Instantiate(playerCharPrefab);
        playerInstance.SpawnWithOwnership(clientID);
    }

}
