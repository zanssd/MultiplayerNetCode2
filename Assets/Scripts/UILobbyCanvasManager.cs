using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class UILobbyCanvasManager : MonoBehaviour
{
    public static UILobbyCanvasManager instance;
    public GameObject canvasLobby;
    public GameObject serverOwnedObjectPrefab;
    public TMP_Text lobbyName;
    public GameObject UIPlayerCardPrefab;
    [SerializeField]
    private GameObject[] playerPositions;

    [Header("INVITATION REFERENCE")]
    [SerializeField]
    private GameObject playerInvPrefab;
    public Transform parentPlayerInv;
    public GameObject invitationPopUp;

    [Header("ROOM REFERENCE")]
    public string matchID;
    [SerializeField]
    private GameObject roomPrefab;
    [SerializeField]
    private Transform parentRoom;

    [Header("SCENE REFERENCE")]
    public Scene mainScene;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
       
    }
  
    public void CreateLobby()
    {
        string randomName = GenerateRandomName();
        Player.instance.playerRoom = randomName;
        Player.instance.CreateLobbyServerRpc(Player.instance.clientId, randomName);
        Debug.Log("Client Side Player " + Player.instance.clientId + " Make Room");
    }
    

    public void CreateRoomPlayer(string roomName)
    {
        GameObject roomGO = Instantiate(roomPrefab, parentRoom);
        Room roomSc = roomGO.GetComponent<Room>();
        Player.instance.roomSc = roomSc;
        roomSc.lobbyText.text = roomName;
        roomSc.beginGameBtn.gameObject.SetActive(true);
        roomSc.beginGameBtn.onClick.AddListener(() =>
        {
            Player.instance.BeginGame();
        });
        roomSc.readyBtn.onClick.AddListener(() =>
        {
            Player.instance.ReadyState();
        });
        matchID = roomName;

    }

    public void JoinRoom(string roomName)
    {
        GameObject roomGO = Instantiate(roomPrefab, parentRoom);
        Room roomSc = roomGO.GetComponent<Room>();
        Player.instance.roomSc = roomSc;
        roomSc.lobbyText.text = roomName;
        roomSc.readyBtn.gameObject.SetActive(true);
        roomSc.readyBtn.onClick.AddListener(() =>
        {
            Player.instance.ReadyState();
        });
        matchID = roomName;
    }
    private void SpawnPlayerUI()
    {
        foreach (GameObject position in playerPositions)
        {
            if(position.transform.childCount == 1)
            {
                GameObject UiPlayer = Instantiate(UIPlayerCardPrefab, position.transform);
                UIPlayerCard uiPlayerCard = UiPlayer.GetComponent<UIPlayerCard>();
                uiPlayerCard.playerName.text = Player.instance.playerNameText;
                break;
            }
        }
    }
    
    private string GenerateRandomName()
    {
        string _id = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26)
            {
                _id += (char)(random + 65);
            }
            else
            {
                _id += (random - 26).ToString();
            }
        }
        Debug.Log($"Random Match ID: {_id}");
        return "Lobby " + _id;
    }

    public void State()
    {
        StartCoroutine(StateDecided());
    }

    IEnumerator StateDecided()
    {
        yield return new WaitForSeconds(1f);
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("SERVERR");
            GameObject serverOwnedObject = Instantiate(serverOwnedObjectPrefab);
            serverOwnedObject.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.Log("BUKANN");
        }
    }

    public void GetListPlayers()
    {
        ClearPlayerForInvList();
        Player.instance.GetPlayerOnMainLobby();
        
    }
    public void ClearPlayerForInvList()
    {
        foreach (Transform item in parentPlayerInv)
        {
            Destroy(item.gameObject);
        }
    }
    public void GetPlayerForInvList(string playerName, ulong playerID)
    {
        Debug.Log("Player Name " + playerName + " Player ID " + playerID);
        GameObject playerInv = Instantiate(playerInvPrefab, parentPlayerInv);
        UIPlayerInvList uIPlayerInvList = playerInv.GetComponent<UIPlayerInvList>();
        uIPlayerInvList.playerName = playerName;
        uIPlayerInvList.playerText.text = playerName;
        uIPlayerInvList.clientID = playerID;
        uIPlayerInvList.button.onClick.AddListener(() =>
        {
            InvitePlayer(playerID, matchID);
        });
    }

    private void InvitePlayer(ulong playerID, string matchID)
    {
        Debug.Log("INVITE PLAYER " + playerID);
       
        Player.instance.InvitePlayerServerRpc(playerID, matchID);
    }
    
}
