using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private TMP_Text emptyText;

    private Dictionary<PlayerRef, GameObject> playerItems = new Dictionary<PlayerRef, GameObject>();
    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();
    private NetworkRunner runner;
    private NetworkChatManager chatManager;
    private bool isInRoom = false;

    private void Start()
    {
        runner = FindFirstObjectByType<NetworkRunner>();
        chatManager = FindFirstObjectByType<NetworkChatManager>();
        Debug.Log($"[PlayerListUI] Start: runner={runner != null}, chatManager={chatManager != null}");
        SetVisible(true);
        ClearList();
    }

    private void Update()
    {
        if (runner != null && runner.IsRunning && isInRoom)
        {
            RefreshPlayerList();
            UpdatePlayerNames(); // Cập nhật tên liên tục
        }
    }

    public void OnEnterRoom()
    {
        Debug.Log("[PlayerListUI] OnEnterRoom - Bắt đầu cập nhật danh sách người chơi");
        isInRoom = true;
        RefreshPlayerList();
    }

    public void OnExitRoom()
    {
        Debug.Log("[PlayerListUI] OnExitRoom - Xóa danh sách người chơi");
        isInRoom = false;
        ClearList();
    }

    public void ClearList()
    {
        foreach (var item in playerItems.Values)
            Destroy(item);
        playerItems.Clear();
        playerNames.Clear();
        if (emptyText != null) emptyText.gameObject.SetActive(true);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        Debug.Log($"[PlayerListUI] SetVisible: {visible}");
    }

    private void UpdatePlayerNames()
    {
        if (playerItems.Count == 0) return;

        // Cập nhật tên cho từng người chơi
        foreach (var player in playerItems.Keys.ToList())
        {
            string newName = GetPlayerName(player);
            if (playerNames.TryGetValue(player, out string oldName) && oldName == newName)
                continue;

            playerNames[player] = newName;

            if (playerItems.TryGetValue(player, out GameObject item))
            {
                var itemScript = item.GetComponent<PlayerListItem>();
                if (itemScript != null)
                {
                    itemScript.SetPlayerName(newName);
                    Debug.Log($"[PlayerListUI] Updated name for player {player.PlayerId}: {newName}");
                }
            }
        }
    }

    private void RefreshPlayerList()
    {
        if (runner == null || runner.ActivePlayers == null) return;

        var activeSet = runner.ActivePlayers.ToHashSet();

        // Xóa những người đã rời
        List<PlayerRef> toRemove = new List<PlayerRef>();
        foreach (var kvp in playerItems)
        {
            if (!activeSet.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }
        foreach (var p in toRemove)
        {
            Destroy(playerItems[p]);
            playerItems.Remove(p);
            playerNames.Remove(p);
        }

        // Thêm người mới
        foreach (var player in runner.ActivePlayers)
        {
            if (!playerItems.ContainsKey(player))
            {
                Debug.Log($"[PlayerListUI] Adding player: {player.PlayerId}");

                if (playerListItemPrefab == null)
                {
                    Debug.LogError("[PlayerListUI] playerListItemPrefab is null!");
                    return;
                }

                if (playerListContent == null)
                {
                    Debug.LogError("[PlayerListUI] playerListContent is null!");
                    return;
                }

                GameObject newItem = Instantiate(playerListItemPrefab, playerListContent);
                string playerName = GetPlayerName(player);
                playerNames[player] = playerName;

                var itemScript = newItem.GetComponent<PlayerListItem>();
                if (itemScript != null)
                    itemScript.SetPlayerName(playerName);
                else
                    Debug.LogError("[PlayerListUI] PlayerListItem component missing on prefab!");

                playerItems.Add(player, newItem);
            }
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(playerItems.Count == 0);
    }

    private string GetPlayerName(PlayerRef player)
    {
        // Cách 1: Lấy từ NetworkChatManager
        if (chatManager != null)
        {
            string name = chatManager.GetPlayerName(player);
            if (!string.IsNullOrEmpty(name) && name != "Unknown")
                return name;
        }

        // Cách 2: Tìm trực tiếp từ NetworkPlayerSync trong scene
        var players = FindObjectsByType<NetworkPlayerSync>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.Object != null && p.Object.InputAuthority == player)
            {
                string name = p.NetworkPlayerName.ToString();
                if (!string.IsNullOrEmpty(name) && name != "Unknown")
                    return name;
            }
        }

        // Cách 3: Tìm từ PlayerHealth (có thể lấy tên từ component khác)
        var healthPlayers = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (var p in healthPlayers)
        {
            var netObj = p.GetComponent<NetworkObject>();
            if (netObj != null && netObj.InputAuthority == player)
            {
                var nameTag = p.GetComponentInChildren<PlayerNameTag>();
                if (nameTag != null)
                {
                    string name = nameTag.GetPlayerName();
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
            }
        }

        // Fallback: dùng PlayerId
        return $"Player {player.PlayerId}";
    }
}