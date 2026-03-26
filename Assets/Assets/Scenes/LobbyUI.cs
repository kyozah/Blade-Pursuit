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

    private List<RoomListItem> _items = new();

    void Start()
    {
        // ── Kiểm tra references ───────────────────────────
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
                HideLobby();
                networkManager.JoinRoom(name);
            }
        });

        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => networkManager.StartGame());

        Debug.Log("[LobbyUI] Start() xong, gọi JoinLobby...");
        networkManager.JoinLobby();
    }

    public void HideLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    public void ShowLobby()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    [Header("Disconnected Message")]
    public TMP_Text disconnectedText; // Assign a TextMeshPro text in the UI

    public void BuildRoomList(List<SessionInfo> sessions)
    {
        foreach (var item in _items) Destroy(item.gameObject);
        _items.Clear();

        foreach (var s in sessions)
        {
            var item = Instantiate(roomListItemPrefab, roomListContainer);
            item.Init(s.Name, () =>
            {
                HideLobby();
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
    }
}