using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class Room : NetworkBehaviour
{
    public List<Player> playersInRoom = new List<Player>();
    public string roomName;
    public TMP_Text lobbyText;
    public Transform[] playerPositions;
    [SerializeField]
    private GameObject UIPlayerCardPrefab;
    [SerializeField]
    private Transform parentPlayerInvitation;

    public Button readyBtn, beginGameBtn;
    // Start is called before the first frame update
    void Start()
    {
        UILobbyCanvasManager.instance.parentPlayerInv = parentPlayerInvitation;
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


    public void GetListPlayers()
    {
        Player.instance.GetPlayerOnMainLobby();
    }

    private void OnEnable()
    {
        StartCoroutine(Player.instance.GetPlayerInRoom());
    }

    public void SpawnPlayerPrefabUI(string playerName, Player player)
    {
        foreach (Transform position in playerPositions)
        {
            if (position.childCount == 1)
            {
                GameObject UiPlayer = Instantiate(UIPlayerCardPrefab, position.transform);
                if(player.playerNameText == playerName)
                {
                    player.playerCard = UiPlayer;
                }
                else
                {
                    player.AssginPrefabCardServerRpc(player.clientId, lobbyText.text);
                }
                UIPlayerCard uiPlayerCard = UiPlayer.GetComponent<UIPlayerCard>();
                uiPlayerCard.playerName.text = playerName;
                break;
            }
        }
    }

    public void AssignAnotherPLayerPrefabUI(Player player)
    {
        if (player.playerCard != null) return;
        Debug.Log("ASSIGN PLAYER PREFAB " + player.playerNameText);
        foreach (Transform position in playerPositions)
        {
            if (position.childCount > 1)
            {
                if (player.playerNameText == position.GetChild(1).GetComponent<UIPlayerCard>().playerName.text)
                {
                    player.playerCard = position.GetChild(1).gameObject;
                }
                else
                {
                    Debug.Log("GA COCOK");
                }
            }
        }
    }
    public void ClearPlayerInvList()
    {
        foreach (Transform item in parentPlayerInvitation.transform)
        {
            Debug.Log("CLEAR " + item.gameObject.name);
            Destroy(item.gameObject);
        }
    }
    public void ClearPlayer()
    {
        foreach (Transform position in playerPositions)
        {
            if (position.childCount > 1)
            {
                Destroy(position.GetChild(1).gameObject);
            }
        }
    }
}