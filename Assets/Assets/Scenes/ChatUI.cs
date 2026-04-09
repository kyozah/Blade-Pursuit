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
    
    public float messageLifetime = 10f;
    public int maxMessages = 50;
    
    private bool isChatActive = false;
    private List<GameObject> messageObjects = new List<GameObject>();
    private NetworkChatManager chatManager;
    private string localPlayerName;
    
    void Start()
    {
        Debug.Log("ChatUI Start");
        
        chatPanel.SetActive(false);
        sendButton.onClick.AddListener(OnSendMessage);
        chatInputField.onSubmit.AddListener(_ => OnSendMessage());
        
        // ✅ TÌM CHAT MANAGER VỚI DELAY
        Invoke(nameof(FindChatManager), 0.5f);
        
        var networkManager = FindFirstObjectByType<NetworkManager>();
        localPlayerName = networkManager != null ? networkManager.GetLocalPlayerName() : PlayerPrefs.GetString("PlayerName", "Player");
        Debug.Log($"Local player name: {localPlayerName}");
    }
    
    void FindChatManager()
    {
        chatManager = FindFirstObjectByType<NetworkChatManager>();
        Debug.Log($"ChatManager found: {chatManager != null}");
        
        if (chatManager != null)
        {
            chatManager.OnNewMessage += OnNewMessage;
            var history = chatManager.GetChatHistory();
            foreach (var msg in history)
            {
                AddMessageToUI(msg);
            }
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
        
        // ✅ KIỂM TRA CHAT MANAGER
        if (chatManager == null)
        {
            chatManager = FindFirstObjectByType<NetworkChatManager>();
            if (chatManager == null)
            {
                Debug.LogError("ChatManager is null! Cannot send message.");
                return;
            }
        }
        
        // ✅ KIỂM TRA RUNNER
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found!");
            return;
        }
        
        if (runner.LocalPlayer == null)
        {
            Debug.LogError("LocalPlayer is null!");
            return;
        }
        
        chatManager.SendChatMessage(message, localPlayerName, runner.LocalPlayer);
        chatInputField.text = "";
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
        
        Destroy(messageObj, messageLifetime);
    }
    
    void OnDestroy()
    {
        if (chatManager != null)
            chatManager.OnNewMessage -= OnNewMessage;
    }
}