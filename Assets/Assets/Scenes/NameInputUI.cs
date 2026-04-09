using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class NameInputUI : MonoBehaviour
{
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public TextMeshProUGUI errorText;
    
    public int minNameLength = 2;
    public int maxNameLength = 16;
    
    void Start()
    {
        Debug.Log("NameInputUI Start");
        
        confirmButton.onClick.AddListener(OnConfirmName);
        nameInputField.onSubmit.AddListener(_ => OnConfirmName());
        
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(savedName) && savedName.Length >= minNameLength)
        {
            nameInputField.text = savedName;
        }
        
        nameInputPanel.SetActive(true);
        errorText.gameObject.SetActive(false);
    }
    
    void OnConfirmName()
    {
        string inputName = nameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(inputName))
        {
            ShowError("Please enter a name!");
            return;
        }
        
        if (inputName.Length < minNameLength)
        {
            ShowError($"Name must be at least {minNameLength} characters!");
            return;
        }
        
        if (inputName.Length > maxNameLength)
        {
            ShowError($"Name cannot exceed {maxNameLength} characters!");
            return;
        }
        
        PlayerPrefs.SetString("PlayerName", inputName);
        PlayerPrefs.Save();
        
        var networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
        {
            networkManager.SetLocalPlayerName(inputName);
        }
        
        nameInputPanel.SetActive(false);
        Debug.Log($"Name set to: {inputName}");
    }
    
    void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke(nameof(HideError), 3f);
    }
    
    void HideError()
    {
        errorText.gameObject.SetActive(false);
    }
}