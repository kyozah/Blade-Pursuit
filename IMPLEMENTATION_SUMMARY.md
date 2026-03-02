# 🎮 Blade Pursuit - Công Năng Mới

## 📌 Tóm tắt 3 tính năng chính

### 1️⃣ Khi player hết máu
```
Player Health = 0
        ↓
   Wait 2 seconds
        ↓
 Show Death Menu (Fade in)
        ↓
   ┌──────────┬──────────┐
   │          │          │
 [Play]    [Quit]
   │          │
   ↓          ↓
Reload Gameplay  Exit Game
```

### 2️⃣ Khi player va chạm vật có PlayerKiller
```
Player Touch Object with PlayerKiller
        ↓
TriggerEnter/CollisionEnter
        ↓
TakeDamage(9999)
        ↓
Player Dies (and shows Death Menu)
```

### 3️⃣ Khi player tiêu diệt boss
```
Boss Health = 0
        ↓
BossHealth.OnDied event fires
        ↓
VictoryMenuUI.ShowVictoryMenu()
        ↓
   ┌──────────────┬──────────┐
   │              │          │
[Continue]    [Quit]
   │              │
   ↓              ↓
Load Menu    Exit Game
```

---

## 📦 Scripts được tạo:

### DeathMenuUI.cs
```csharp
public class DeathMenuUI : MonoBehaviour
{
    public void ShowDeathMenu()
    public void OnPlayClicked()
    public void OnQuitClicked()
}
```

### VictoryMenuUI.cs
```csharp
public class VictoryMenuUI : MonoBehaviour
{
    public void ShowVictoryMenu()
    public void OnContinueClicked()
    public void OnQuitClicked()
}
```

### PlayerKiller.cs
```csharp
public class PlayerKiller : MonoBehaviour
{
    [SerializeField] bool useInstantKill = true;
    public void KillPlayer(PlayerHealth player)
}
```

### GameManager.cs
```csharp
public class GameManager : MonoBehaviour (Singleton)
{
    public void OnPlayerDeath()
    public void OnBossDefeated()
    public void GoToMainMenu()
    public void ReloadCurrentScene()
    public void QuitGame()
}
```

---

## 🔌 Cách kết nối trong Unity Editor

### Step 1: Tạo Canvas cho Death Menu
1. Tở scene Gameplay
2. Right-click in Hierarchy → UI → Canvas
3. Đặt tên: "DeathMenuCanvas"
4. Add component: **DeathMenuUI**
5. Tạo 2 buttons: "PlayButton" và "QuitButton"
6. Drag buttons vào DeathMenuUI fields
7. Set CanvasGroup alpha = 0

### Step 2: Tạo Canvas cho Victory Menu
1. Right-click in Hierarchy → UI → Canvas
2. Đặt tên: "VictoryMenuCanvas"
3. Add component: **VictoryMenuUI**
4. Tạo 2 buttons: "ContinueButton" và "QuitButton"
5. Drag buttons vào VictoryMenuUI fields
6. Set CanvasGroup alpha = 0

### Step 3: Setup PlayerKiller trên hazard objects
1. Chọn object gây chết (lava, spike, pit, etc.)
2. Add component: **PlayerKiller**
3. Ensure có Collider (trigger hoặc regular)
4. Set useInstantKill = true

---

## 🎯 Flow hoàn chỉnh:

```
Gameplay Scene
    ├── PlayerHealth (checks death)
    │   └── On Death → DeathMenuUI.ShowDeathMenu() after 2s
    │       ├── [Play] → SceneManager.LoadScene("Gameplay")
    │       └── [Quit] → Application.Quit()
    │
    ├── BossHealth (checks defeat)
    │   └── OnDied event → VictoryMenuUI.ShowVictoryMenu()
    │       ├── [Continue] → SceneManager.LoadScene("Menu")
    │       └── [Quit] → Application.Quit()
    │
    └── Hazard Objects
        └── PlayerKiller component
            └── On Collision → TakeDamage() from PlayerHealth
                └── Triggers Death flow above
```

---

## 💡 Mẹo setup

- **Dùng Trigger cho lava/gaps**: Is Trigger = ON, sẽ dùng OnTriggerEnter
- **Dùng Collision cho obstacles**: Is Trigger = OFF, sẽ dùng OnCollisionEnter
- **Death Menu chờ 2s**: Để player animation kịp chạy
- **Victory Menu tự active**: Khi BossHealth.OnDied được gọi
- **Time.timeScale = 0**: Cả 2 menu pause game

---

## 🧪 Test checklist:

```
□ Scene Gameplay can load successfully
□ DeathMenuUI button clicks work
□ VictoryMenuUI button clicks work
□ PlayerKiller kills player on collision
□ Death Menu appears 2s after death
□ Victory Menu appears after boss death
□ Play button reloads Gameplay
□ Continue button loads Menu
□ Quit buttons work
□ All UI fade in smoothly
```

---

**Tất cả yêu cầu của bạn đã được implement! 🎉**

