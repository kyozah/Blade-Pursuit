using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public Button joinButton;

    public void Init(string roomName, Action onJoin)
    {
        roomNameText.text = roomName;
        joinButton.onClick.AddListener(() => onJoin());
    }
}