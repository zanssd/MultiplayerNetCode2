using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIPlayerCard : MonoBehaviour
{
    public TMP_Text playerName;
    public TMP_Text state;
    public static TMP_Text staticState;

    private void Start()
    {
        staticState = state;
    }
}
