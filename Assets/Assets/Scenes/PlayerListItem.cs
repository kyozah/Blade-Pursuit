using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    public void SetPlayerName(string name)
    {
        if (playerNameText != null)
            playerNameText.text = name;
        else
            Debug.LogError("PlayerListItem: playerNameText not assigned!");
    }
}