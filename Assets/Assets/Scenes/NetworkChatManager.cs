using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class NetworkChatManager : NetworkBehaviour
{
    public static NetworkChatManager Instance { get; private set; }
    
    [Networked, Capacity(50)] 
    private NetworkArray<NetworkChatMessage> ChatHistory { get; }
    
    private List<NetworkChatMessage> localChatHistory = new List<NetworkChatMessage>();
    private Dictionary<PlayerRef, string> playerNames = new Dictionary<PlayerRef, string>();
    
    public System.Action<NetworkChatMessage> OnNewMessage;
    
    public override void Spawned()
    {
        Debug.Log($"[ChatManager] Spawned - HasStateAuthority: {HasStateAuthority}, Object: {Object != null}");
        
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ChatManager] Instance set");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // ✅ KIỂM TRA OBJECT TRƯỚC KHI GỌI RPC
    private bool IsReady()
    {
        if (Object == null)
        {
            Debug.LogError("[ChatManager] Object is null! NetworkBehaviour not initialized.");
            return false;
        }
        return true;
    }
    
    public void SendChatMessage(string message, string playerName, PlayerRef sender)
    {
        Debug.Log($"[ChatManager] SendChatMessage - message: '{message}', HasStateAuthority: {HasStateAuthority}, Object: {Object != null}");
        
        if (string.IsNullOrEmpty(message)) return;
        
        // ✅ KIỂM TRA OBJECT TRƯỚC
        if (!IsReady()) return;
        
        if (HasStateAuthority)
        {
            Debug.Log("[ChatManager] Adding message directly (I'm host)");
            AddMessageToHistory(new NetworkChatMessage
            {
                message = message,
                senderName = playerName,
                senderRef = sender,
                timestamp = System.DateTime.UtcNow.Ticks
            });
        }
        else
        {
            Debug.Log("[ChatManager] Sending RPC to host");
            RpcSendChatMessage(message, playerName, sender);
        }
    }
    
    // ✅ THÊM Rpc = RpcSources.All để đảm bảo
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcSendChatMessage(string message, string playerName, PlayerRef sender)
    {
        Debug.Log($"[ChatManager] RpcSendChatMessage received - message: '{message}', playerName: '{playerName}'");
        
        // ✅ KIỂM TRA LẠI TRONG RPC
        if (!HasStateAuthority)
        {
            Debug.LogWarning("[ChatManager] RpcSendChatMessage called but not state authority!");
            return;
        }
        
        AddMessageToHistory(new NetworkChatMessage
        {
            message = message,
            senderName = playerName,
            senderRef = sender,
            timestamp = System.DateTime.UtcNow.Ticks
        });
    }
    
    private void AddMessageToHistory(NetworkChatMessage msg)
    {
        Debug.Log($"[ChatManager] AddMessageToHistory - {msg.senderName}: {msg.message}");
        
        for (int i = 0; i < ChatHistory.Length - 1; i++)
        {
            ChatHistory.Set(i, ChatHistory.Get(i + 1));
        }
        ChatHistory.Set(ChatHistory.Length - 1, msg);
        
        RpcReceiveNewMessage(msg);
        OnNewMessage?.Invoke(msg);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcReceiveNewMessage(NetworkChatMessage msg)
    {
        Debug.Log($"[ChatManager] RpcReceiveNewMessage - {msg.senderName}: {msg.message}");
        localChatHistory.Add(msg);
        OnNewMessage?.Invoke(msg);
    }
    
    public List<NetworkChatMessage> GetChatHistory()
    {
        localChatHistory.Clear();
        for (int i = 0; i < ChatHistory.Length; i++)
        {
            var msg = ChatHistory.Get(i);
            string msgValue = msg.message.ToString();
            if (!string.IsNullOrEmpty(msgValue))
                localChatHistory.Add(msg);
        }
        return localChatHistory;
    }
    
    public void RegisterPlayerName(PlayerRef player, string name)
    {
        Debug.Log($"[ChatManager] RegisterPlayerName - player: {player}, name: '{name}'");
        
        if (!IsReady()) return;
        
        if (HasStateAuthority)
        {
            playerNames[player] = name;
            RpcSyncPlayerName(player, name);
        }
        else
        {
            RpcRegisterPlayerName(player, name);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRegisterPlayerName(PlayerRef player, string name)
    {
        Debug.Log($"[ChatManager] RpcRegisterPlayerName - player: {player}, name: '{name}'");
        playerNames[player] = name;
        RpcSyncPlayerName(player, name);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSyncPlayerName(PlayerRef player, string name)
    {
        Debug.Log($"[ChatManager] RpcSyncPlayerName - player: {player}, name: '{name}'");
        playerNames[player] = name;
        
        var players = FindObjectsByType<NetworkPlayerSync>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.Object.InputAuthority == player)
            {
                var nameTag = p.GetComponentInChildren<PlayerNameTag>();
                if (nameTag != null)
                {
                    nameTag.SetPlayerName(name);
                }
            }
        }
    }
    
    public string GetPlayerName(PlayerRef player)
    {
        return playerNames.TryGetValue(player, out string name) ? name : "Unknown";
    }
}

public struct NetworkChatMessage : INetworkStruct
{
    public NetworkString<_128> message;
    public NetworkString<_16> senderName;
    public PlayerRef senderRef;
    public long timestamp;
}