using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // Namespace chuẩn cho Unity 6
using UnityEngine.SceneManagement;
using TMPro;

public class IntroMasterDirector : MonoBehaviour
{
    [Header("--- Hệ Thống Camera ---")]
    public CinemachineCamera camWide;
    public CinemachineCamera camCloseUp;
    public CinemachineCamera camPOV;

    [Header("--- Hiệu Ứng & Giao Diện ---")]
    public CanvasGroup fadeCanvasGroup;   // Kéo Panel đen vào đây
    public TextMeshProUGUI subtitleText;  // Kéo TextMeshPro vào đây
    public GameObject vfxVoid;            // Hiệu ứng Hư Không (Portal/Khói tím)
    public CinemachineImpulseSource impulseSource; // Gắn vào Vladislav để rung màn hình

    [Header("--- Cài Đặt Thời Gian (Giây) ---")]
    public float timeWide = 5f;
    public float timeCloseUp = 5f;
    public float timePOV = 6f;

    [Header("--- Cấu Hình Chữ ---")]
    public float typingSpeed = 0.05f;

    private void Awake()
    {
        // Đảm bảo lúc bắt đầu màn hình tối đen và các cam ở mức ưu tiên thấp
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;
        
        // Reset Priority để tránh xung đột ban đầu
        if(camWide) camWide.Priority = 0;
        if(camCloseUp) camCloseUp.Priority = 0;
        if(camPOV) camPOV.Priority = 0;
    }

    void Start()
    {
        // Chỉ chạy DUY NHẤT 1 lần khi nhấn Play
        StopAllCoroutines();
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // --- BƯỚC 0: KHỞI TẠO ---
        vfxVoid.SetActive(false);
        subtitleText.text = "";

        // --- BƯỚC 1: FADE IN (MỞ MÀN) ---
        yield return StartCoroutine(FadeEffect(0f, 2f)); // Mất 2 giây để hiện hình ảnh

        // --- BƯỚC 2: CẢNH RỘNG (HÒA BÌNH) ---
        SwitchCamera(camWide);
        yield return StartCoroutine(TypeText("Dragomirov... từng là nhịp đập của lục địa. Nơi trật tự được viết bằng niềm tin, và hòa bình được gìn giữ bởi dòng máu cổ xưa."));
        
        Debug.Log("Đang chờ cảnh Wide: " + timeWide + " giây");
        yield return new WaitForSeconds(timeWide);

        // --- BƯỚC 3: BIẾN CỐ (SA NGÃ) ---
        subtitleText.text = ""; // Xóa chữ cũ trước khi chuyển cảnh
        SwitchCamera(camCloseUp);
        
        // Đợi 1 giây cho camera lướt tới rồi mới nổ hiệu ứng
        yield return new WaitForSeconds(1f);
        vfxVoid.SetActive(true);
        
        // Rung màn hình khi Hư Không mở ra
        if (impulseSource != null) impulseSource.GenerateImpulse();

        yield return StartCoroutine(TypeText("Nhưng đặc ân của dòng máu cũng chính là mầm mống tai họa. Đêm không trăng đó... Hư Không đã tìm đến. Nó không xâm lược, nó chỉ ăn mòn."));
        yield return StartCoroutine(TypeText("Vị lãnh chúa tận tụy đã chọn sự sa ngã để bảo vệ thần dân theo cách méo mó nhất. Nhân tính bị vứt bỏ, chỉ còn lại cơn khát thanh trừng."));
        subtitleText.text = "";
        yield return new WaitForSeconds(1f);
        Debug.Log("Đang chờ cảnh CloseUp: " + timeCloseUp + " giây");
        yield return new WaitForSeconds(timeCloseUp);

        // --- BƯỚC 4: CHẠY TRỐN (POV) ---
        subtitleText.text = "";
        SwitchCamera(camPOV);
        

        yield return StartCoroutine(TypeText("Chạy đi, Aleksandr! Đừng nhìn lại!"));
        Debug.Log("Đang chờ cảnh POV: " + timePOV + " giây");
        yield return new WaitForSeconds(timePOV);

        // --- BƯỚC 5: KẾT THÚC (FADE OUT) ---
        subtitleText.text = "";
        yield return StartCoroutine(FadeEffect(1f, 2f)); // Màn hình tối đen lại trong 2 giây
        
        // --- BƯỚC 6: CHUYỂN SCENE ---
        Debug.Log("Chuyển sang Gameplay...");
        SceneManager.LoadScene("Gameplay");
    }

    // --- HÀM HỖ TRỢ CHUYỂN CAMERA ---
    void SwitchCamera(CinemachineCamera targetCam)
    {
        if (targetCam == null) return;

        // Ép tất cả về Priority thấp
        camWide.Priority = 5;
        camCloseUp.Priority = 5;
        camPOV.Priority = 5;

        // Đẩy cam mục tiêu lên cao để Cinemachine Brain tự lướt tới
        targetCam.Priority = 20;
        Debug.Log("<color=yellow>Đạo diễn:</color> Đã chuyển sang " + targetCam.name);
    }

    // --- HÀM HỖ TRỢ LÀM MỜ MÀN HÌNH ---
    IEnumerator FadeEffect(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;
    }

    // --- HÀM HỖ TRỢ HIỆN CHỮ TỪNG TỪ ---
    IEnumerator TypeText(string message)
    {
        subtitleText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}