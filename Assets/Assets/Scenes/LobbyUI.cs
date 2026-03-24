using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Tham chiếu")]
    public NetworkManager networkManager;

    [Header("Panel Lobby (ẩn khi vào game)")]
    public GameObject lobbyPanel;   // Gán toàn bộ Canvas/Panel lobby vào đây

    [Header("Tạo phòng")]
    public TMP_InputField createRoomInput;
    public Button createRoomButton;

    [Header("Vào phòng bằng tên")]
    public TMP_InputField joinRoomInput;
    public Button joinRoomButton;

    [Header("Bắt đầu game (chỉ Host thấy)")]
    public Button startGameButton;

    [Header("Danh sách phòng")]
    public Transform roomListContainer;
    public RoomListItem roomListItemPrefab;

    private List<RoomListItem> _items = new();

    void Start()
    {
        networkManager.JoinLobby();

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        createRoomButton.onClick.AddListener(() =>
        {
            var name = createRoomInput.text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                networkManager.CreateRoom(name);
                if (startGameButton != null)
                    startGameButton.gameObject.SetActive(true);
            }
        });

        joinRoomButton.onClick.AddListener(() =>
        {
            var name = joinRoomInput.text.Trim();
            if (!string.IsNullOrEmpty(name)) networkManager.JoinRoom(name);
        });

        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => networkManager.StartGame());
    }

    // Gọi khi bắt đầu game — ẩn lobby UI đi
    public void HideLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    // Gọi khi muốn quay về lobby
    public void ShowLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
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