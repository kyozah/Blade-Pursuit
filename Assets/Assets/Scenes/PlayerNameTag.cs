using UnityEngine;
using TMPro;

public class PlayerNameTag : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI nameText;
    public Transform followTarget;
    
    [Header("Settings")]
    public float heightOffset = 1.5f;
    public bool faceCamera = true;
    
    private Camera mainCamera;
    private string currentName = "";
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (followTarget == null)
            followTarget = transform.parent;
        
        // ✅ CHỈ LẤY REFERENCE, KHÔNG GHI ĐÈ CÀI ĐẶT
        if (nameText == null)
        {
            nameText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (nameText != null && !string.IsNullOrEmpty(currentName))
        {
            nameText.text = currentName;
        }
        
        // ✅ KHÔNG set fontSize, alignment, color ở đây
        // ✅ KHÔNG set localScale ở đây
        // Để Inspector kiểm soát hoàn toàn
        
        Debug.Log($"[PlayerNameTag] Initialized on {gameObject.name}, followTarget: {(followTarget != null ? followTarget.name : "NULL")}");
    }
    
    void LateUpdate()
    {
        if (followTarget == null) return;
        
        // Cập nhật vị trí
        transform.position = followTarget.position + Vector3.up * heightOffset;
        
        // Xoay về phía camera (Billboard)
        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        
        currentName = name;
        
        if (nameText == null)
        {
            nameText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
        
        if (nameText != null)
        {
            nameText.text = name;
            Debug.Log($"[PlayerNameTag] Set name to: '{name}'");
        }
    }
    
    public string GetPlayerName()
    {
        return currentName;
    }
}