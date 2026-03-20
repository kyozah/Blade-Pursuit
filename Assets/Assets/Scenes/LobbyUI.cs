using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Tham chiếu")]
    public NetworkManager networkManager;

    [Header("Tạo phòng")]
    public TMP_InputField createRoomInput;
    public Button createRoomButton;

    [Header("Vào phòng bằng tên")]
    public TMP_InputField joinRoomInput;
    public Button joinRoomButton;

    [Header("Danh sách phòng")]
    public Transform roomListContainer;
    public RoomListItem roomListItemPrefab;

    private List<RoomListItem> _items = new();

    void Start()
    {
        createRoomButton.onClick.AddListener(() =>
        {
            var name = createRoomInput.text.Trim();
            if (!string.IsNullOrEmpty(name)) networkManager.CreateRoom(name);
        });

        joinRoomButton.onClick.AddListener(() =>
        {
            var name = joinRoomInput.text.Trim();
            if (!string.IsNullOrEmpty(name)) networkManager.JoinRoom(name);
        });
    }

    public void BuildRoomList(List<SessionInfo> sessions)
    {
        foreach (var item in _items) Destroy(item.gameObject);
        _items.Clear();

        foreach (var s in sessions)
        {
            var item = Instantiate(roomListItemPrefab, roomListContainer);
            item.Init(s.Name, () => networkManager.JoinRoom(s.Name));
            _items.Add(item);
        }
    }
}