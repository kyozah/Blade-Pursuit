using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;

public class ChatUI : MonoBehaviour
{
    public GameObject chatPanel;
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContentParent;
    public GameObject chatMessagePrefab;
    public ScrollRect scrollRect;
    
    public float messageLifetime = 0f; // Set to 0 to keep messages indefinitely
    public int maxMessages = 50;
    
    private bool isChatActive = false;
    private List<GameObject> messageObjects = new List<GameObject>();
    private NetworkChatManager chatManager;
    private string localPlayerName;
    private bool isSubscribed;
    private readonly Queue<string> pendingMessages = new Queue<string>();
    private bool flushLoopRunning;
    
    void Start()
    {
        Debug.Log("ChatUI Start");
        
        chatPanel.SetActive(false);
        sendButton.onClick.AddListener(OnSendMessage);
        chatInputField.onSubmit.AddListener(_ => OnSendMessage());
        
        // Chat manager có thể spawn trễ hơn UI, nên retry để không mất chat.
        InvokeRepeating(nameof(FindChatManager), 0.25f, 0.5f);
        
        var networkManager = FindFirstObjectByType<NetworkManager>();
        localPlayerName = networkManager != null ? networkManager.GetLocalPlayerName() : PlayerPrefs.GetString("PlayerName", "Player");
        Debug.Log($"Local player name: {localPlayerName}");
    }
    
    void FindChatManager()
    {
        if (isSubscribed && chatManager != null)
        {
            CancelInvoke(nameof(FindChatManager));
            return;
        }

        chatManager = NetworkChatManager.Instance;
        Debug.Log($"ChatManager found: {chatManager != null}");
        
        if (chatManager != null && chatManager.Object != null)
        {
            if (!isSubscribed)
            {
                chatManager.OnNewMessage += OnNewMessage;
                isSubscribed = true;
            }

            var history = chatManager.GetChatHistory();
            foreach (var msg in history)
            {
                AddMessageToUI(msg);
            }

            CancelInvoke(nameof(FindChatManager));

            // Nếu lúc trước có message bị queue vì chưa ready thì flush ngay.
            EnsureFlushLoop();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isChatActive)
                SendAndClose();
            else
                OpenChat();
        }
        
        if (isChatActive && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }
    }
    
    void OpenChat()
    {
        Debug.Log("OpenChat");
        isChatActive = true;
        chatPanel.SetActive(true);
        
        // Clear existing message objects to avoid duplicates
        foreach (var obj in messageObjects)
        {
            Destroy(obj);
        }
        messageObjects.Clear();
        
        // Load chat history
        if (chatManager != null)
        {
            var history = chatManager.GetChatHistory();
            foreach (var msg in history)
            {
                AddMessageToUI(msg);
            }
        }
        
        chatInputField.Select();
        chatInputField.ActivateInputField();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void CloseChat()
    {
        Debug.Log("CloseChat");
        isChatActive = false;
        chatPanel.SetActive(false);
        chatInputField.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void SendAndClose()
    {
        OnSendMessage();
        CloseChat();
    }
    
    void OnSendMessage()
    {
        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            Debug.Log("Message empty, ignoring");
            return;
        }
        
        Debug.Log($"Sending message: '{message}'");

        // Luôn clear input để UX mượt, dù message có thể được gửi trễ.
        chatInputField.text = "";

        EnqueueMessage(message);
    }

    private void EnqueueMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        pendingMessages.Enqueue(message);
        EnsureFlushLoop();
    }

    private void EnsureFlushLoop()
    {
        if (flushLoopRunning)
            return;

        flushLoopRunning = true;
        InvokeRepeating(nameof(FlushPendingMessages), 0.1f, 0.25f);
    }

    private void StopFlushLoopIfIdle()
    {
        if (pendingMessages.Count > 0)
            return;

        flushLoopRunning = false;
        CancelInvoke(nameof(FlushPendingMessages));
    }

    private void FlushPendingMessages()
    {
        // Lấy lại tên local phòng trường hợp vào game trước khi NameInputUI set xong.
        var networkManager = FindFirstObjectByType<NetworkManager>();
        localPlayerName = networkManager != null ? networkManager.GetLocalPlayerName() : PlayerPrefs.GetString("PlayerName", "Player");

        // 1) Chat manager phải tồn tại và đã được Fusion init (Object != null)
        chatManager = NetworkChatManager.Instance;
        if (chatManager == null || chatManager.Object == null)
            return;

        // 2) Runner phải tồn tại và có LocalPlayer hợp lệ
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null)
            return;

        PlayerRef sender = runner.LocalPlayer;
        // Một số version Fusion không có PlayerRef.IsValid → dùng so sánh với None/default.
        if (sender == PlayerRef.None || sender == default)
            return;

        // 3) Gửi hết queue (giới hạn an toàn mỗi tick để tránh spike)
        int maxPerTick = 5;
        for (int i = 0; i < maxPerTick && pendingMessages.Count > 0; i++)
        {
            string msg = pendingMessages.Dequeue();
            chatManager.SendChatMessage(msg, localPlayerName, sender);
        }

        StopFlushLoopIfIdle();
    }
    
    void OnNewMessage(NetworkChatMessage msg)
    {
        Debug.Log($"New message: {msg.senderName}: {msg.message}");
        AddMessageToUI(msg);
    }
    
    void AddMessageToUI(NetworkChatMessage msg)
    {
        if (chatMessagePrefab == null)
        {
            Debug.LogError("ChatMessagePrefab is NULL!");
            return;
        }
        
        GameObject messageObj = Instantiate(chatMessagePrefab, chatContentParent);
        TextMeshProUGUI textComponent = messageObj.GetComponent<TextMeshProUGUI>();
        
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on prefab!");
            Destroy(messageObj);
            return;
        }
        
        string senderName = msg.senderName.ToString();
        string messageText = msg.message.ToString();
        textComponent.text = $"<color=#FFD700><b>{senderName}</b></color>: {messageText}";
        
        messageObjects.Add(messageObj);
        
        if (messageObjects.Count > maxMessages)
        {
            Destroy(messageObjects[0]);
            messageObjects.RemoveAt(0);
        }
        
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        
        // Removed Destroy(messageObj, messageLifetime); to keep messages indefinitely
    }
    
    void OnDestroy()
    {
        if (chatManager != null)
            chatManager.OnNewMessage -= OnNewMessage;
        isSubscribed = false;

        CancelInvoke(nameof(FindChatManager));
        CancelInvoke(nameof(FlushPendingMessages));
    }
}