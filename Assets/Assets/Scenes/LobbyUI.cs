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
    public GameObject lobbyPanel;

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

    [Header("Player List (trong phòng)")]
    public PlayerListUI playerListUI;

    [Header("Disconnected Message")]
    public TMP_Text disconnectedText;

    private List<RoomListItem> _items = new List<RoomListItem>();

    void Start()
    {
        if (networkManager == null)
        {
            Debug.LogError("[LobbyUI] networkManager chưa được gán trong Inspector!");
            networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("[LobbyUI] Không tìm thấy NetworkManager trong scene!");
                return;
            }
            Debug.Log("[LobbyUI] Tìm thấy NetworkManager tự động.");
        }

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
            if (!string.IsNullOrEmpty(name))
            {
                networkManager.JoinRoom(name);
            }
        });

        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => networkManager.StartGame());

        // Đảm bảo PlayerListUI hiển thị (khung) nhưng chưa có dữ liệu
        if (playerListUI != null)
        {
            playerListUI.SetVisible(true);
            playerListUI.ClearList();
        }

        Debug.Log("[LobbyUI] Start() xong, gọi JoinLobby...");
        networkManager.JoinLobby();
    }

    // Ẩn toàn bộ panel lobby (khi bắt đầu game)
    public void HideLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    // Hiện lại panel lobby (khi rời phòng)
    public void ShowLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    // Gọi khi đã vào phòng thành công (host hoặc client)
    public void NotifyEnteredRoom()
    {
        Debug.Log("[LobbyUI] NotifyEnteredRoom - cập nhật danh sách người chơi");
        if (playerListUI != null)
            playerListUI.OnEnterRoom();
        // Không ẩn lobby panel để vẫn thấy danh sách phòng và nút tạo/join
    }

    // Gọi khi rời phòng (disconnect)
    public void NotifyLeftRoom()
    {
        Debug.Log("[LobbyUI] NotifyLeftRoom - xóa danh sách người chơi");
        if (playerListUI != null)
            playerListUI.OnExitRoom();
        ShowLobby(); // Hiện lại panel lobby (nếu đã bị ẩn)
    }

    public void BuildRoomList(List<SessionInfo> sessions)
    {
        foreach (var item in _items) Destroy(item.gameObject);
        _items.Clear();

        if (roomListContainer == null)
        {
            Debug.LogError("[LobbyUI] roomListContainer chưa được gán trong Inspector.");
            return;
        }

        if (roomListItemPrefab == null)
        {
            Debug.LogError("[LobbyUI] roomListItemPrefab đang NULL (chưa gán prefab item phòng).");
            return;
        }

        foreach (var s in sessions)
        {
            var item = Instantiate(roomListItemPrefab, roomListContainer);
            item.Init(s.Name, () =>
            {
                networkManager.JoinRoom(s.Name);
            });
            _items.Add(item);
        }
    }

    public void ShowDisconnectedMessage(string reason)
    {
        if (disconnectedText != null)
        {
            disconnectedText.text = $"Disconnected: {reason}";
            disconnectedText.gameObject.SetActive(true);
        }
        Debug.Log($"[UI] Disconnected: {reason}");
        NotifyLeftRoom();
    }
}