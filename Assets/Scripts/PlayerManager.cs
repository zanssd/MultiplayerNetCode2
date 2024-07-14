using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField]
    private ThirdPersonController playerMovement;
    [SerializeField]
    private PlayerInput playerInput;

    void Start()
    {
        Debug.Log("SPAWNED");
        if (!IsOwner) return;
        Debug.Log("CLIENT IN");
        playerMovement.enabled = true;
        playerInput.enabled = true;
    }

}
