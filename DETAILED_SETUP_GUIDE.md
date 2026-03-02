# 🎮 HƯỚNG DẪN SETUP CHI TIẾT - Blade Pursuit

## ⚙️ SETUP DEATH MENU (Menu Chết)

### Bước 1: Tạo Canvas cho Death Menu
```
1. Trong Gameplay scene
2. Hierarchy → Right-click → UI → Canvas
3. Đặt tên: "DeathMenuCanvas"
4. Inspector → Add Component → DeathMenuUI
```

### Bước 2: Tạo các Button
```
1. Select DeathMenuCanvas
2. Right-click → UI → Button (TextMeshPro)
3. Tạo 2 button: "PlayButton" và "QuitButton"
4. Đặt text: "Play" và "Quit"
```

### Bước 3: Assign buttons vào DeathMenuUI
```
1. Select DeathMenuCanvas
2. Inspector → DeathMenuUI component
3. Play Button: Drag "PlayButton" vào field
4. Quit Button: Drag "QuitButton" vào field
```

### Bước 4: Setup CanvasGroup
```
1. Select DeathMenuCanvas
2. Inspector → CanvasGroup
3. Alpha: 0
4. Blocks Raycasts: ticked
```

### Bước 5: Customize UI (tùy chọn)
```
- Make buttons bigger: Select PlayButton → size (400x100)
- Change text size: Select text child → size 40
- Add background: Select DeathMenuCanvas → Add Image component
- Position buttons nicely in center
```

---

## 🏆 SETUP VICTORY MENU (Menu Chiến Thắng)

> **Lưu ý:** Menu chiến thắng chỉ hiện khi boss cuối cùng bị tiêu diệt. Hệ thống tự đếm số boss đang tồn tại trong scene; nếu bạn có 2 boss, phải giết cả hai mới nhận Victory.

### Bước 1: Tạo Canvas cho Victory Menu
```
1. Hierarchy → Right-click → UI → Canvas
2. Đặt tên: "VictoryMenuCanvas"
3. Inspector → Add Component → VictoryMenuUI
```

### Bước 2: Tạo các Button
```
1. Select VictoryMenuCanvas
2. Right-click → UI → Button (TextMeshPro)
3. Tạo 2 button: "ContinueButton" và "QuitButton"
4. Đặt text: "Continue to Menu" và "Quit"
```

### Bước 3: Assign buttons vào VictoryMenuUI
```
1. Select VictoryMenuCanvas
2. Inspector → VictoryMenuUI component
3. Continue Button: Drag button quay lại menu
4. Quit Button: Drag button thoát
```

### Bước 4: Setup CanvasGroup
```
1. Select VictoryMenuCanvas
2. Inspector → CanvasGroup
3. Alpha: 0
4. Blocks Raycasts: ticked
```

---

## ☠️ SETUP PLAYER KILLER (Kill Player on Collision)

### Cách 1: Dùng Trigger (cho Lava, Pit)
```
1. Chọn hazard object (lava, gap, water, etc.)
2. Inspector → Add Component → Collider
   - Example: BoxCollider, CapsuleCollider
3. Tick "Is Trigger"
4. Inspector → Add Component → PlayerKiller
5. Thế là xong! Player sẽ die khi chạm vào
```

### Cách 2: Dùng Collision thường (cho Spike trap)
```
1. Chọn hazard object (spike, blade, etc.)
2. Inspector → Collider → untick "Is Trigger"
3. Inspector → Add Component → PlayerKiller
4. Hoặc thêm vào spike object sẵn có
```

### Ví dụ cấu hình:
```
Lava GameObject:
├── Transform
├── BoxCollider
│   └── Is Trigger: ON
└── PlayerKiller
    ├── Use Instant Kill: true
    ├── Kill Damage: 9999
    └── Debug Logs: true
```

---

## 🎮 GAME MANAGER (Tùy chọn - Singleton)

### Cách setup:
```
1. Hierarchy → Right-click → Create Empty
2. Đặt tên: "GameManager"
3. Inspector → Add Component → GameManager
4. Nó sẽ tự tìm PlayerHealth, DeathMenuUI, VictoryMenuUI
```

### Khi nào cần dùng:
- Nếu muốn gọi game events từ nơi khác
- Nếu muốn manage pause/resume game
- Nếu muốn control scene transitions từ một nơi

Không có GameManager vẫn chạy được, nhưng nó hữu ích để quản lý.

---

## 📋 QUICK CHECKLIST

### Canvas Setup:
- [ ] DeathMenuCanvas created with 2 buttons
- [ ] VictoryMenuCanvas created with 2 buttons
- [ ] DeathMenuUI component assigned to DeathMenuCanvas
- [ ] VictoryMenuUI component assigned to VictoryMenuCanvas
- [ ] Buttons linked to DeathMenuUI and VictoryMenuUI
- [ ] CanvasGroup Alpha set to 0
- [ ] CanvasGroup Blocks Raycasts enabled

### PlayerKiller Setup:
- [ ] Identified all hazard objects (lava, spikes, pits)
- [ ] Added PlayerKiller component to each
- [ ] Collider set to Trigger for lava/gaps
- [ ] Collider NOT Trigger for spike/blades

### Scene Settings:
- [ ] Gameplay scene in Build Settings
- [ ] Menu scene in Build Settings
- [ ] Scene names match: "Gameplay", "Menu"

---

## 🧪 TESTING

### Test Death Menu:
```
1. Play game and get killed (by boss or hazard)
2. Wait 2 seconds
3. Death Menu should fade in
4. Click Play → should reload Gameplay
5. Click Quit → should close game
```

### Test Victory Menu:
```
1. Defeat boss
2. Victory Menu should appear
3. Click Continue → should load Menu scene
4. Click Quit → should close game
```

### Test PlayerKiller:
```
1. Run into hazard with PlayerKiller
2. Player should die immediately
3. Death Menu appears after 2s
```

---

## ⚠️ COMMON ISSUES

### Death Menu không hiển thị?
- Kiểm tra DeathMenuUI component có gắn trên Canvas không
- Kiểm tra CanvasGroup.alpha = 0
- Kiểm tra Blocks Raycasts = ON

### Victory Menu không hiển thị?
- Kiểm tra BossHealth.OnDied event được gọi không
- Console sẽ log "[VictoryMenuUI] Victory menu shown"
- Kiểm tra VictoryMenuUI component gắn trên Canvas

### PlayerKiller không work?
- Kiểm tra object có Collider không
- Console sẽ log "[PlayerKiller] ⚠️ INSTANT KILLING PLAYER!"
- Kiểm tra player tag/layer nếu cần

### Buttons không click được?
- Kiểm tra CanvasGroup.blocksRaycasts = ON
- Kiểm tra Button component có OnClick listener không
- Thường script tự thêm listener, nên chỉ cần gán button vào field

---

## 📝 CHI TIẾT TỪng SCRIPT

### DeathMenuUI.cs
```csharp
- Chờ 2 giây sau khi PlayerHealth.IsDead() = true
- Pause game (Time.timeScale = 0)
- Show menu với fade in animation (0.5s)
- Khi hiển thị, script tự tắt `blocksRaycasts` trên mọi lớp CanvasGroup khác (ví dụ health bar) để các nút Death Menu có thể nhận click
- OnReplayClicked: reload current active scene (`SceneManager.GetActiveScene().name`)
- OnMenuClicked: load main Menu scene (`SceneManager.LoadScene("Menu")`)
```

### VictoryMenuUI.cs
```csharp
- Subscribe vào BossHealth.OnDied event của mỗi boss đã gán
- Khi tất cả boss đã chết (theo danh sách được gán), chờ `victoryDelay` (mặc định 4s) để death animation hoàn tất
- Pause game (Time.timeScale = 0)
- Show menu với fade in animation (0.5s)
- OnContinueClicked: SceneManager.LoadScene("Menu")
- OnQuitClicked: Application.Quit()
```

### Boss kill reward
```
// Trong GameManager:
[SerializeField] private float bossMaxHealthBonus = 100f;
[SerializeField] private BossHealth[] rewardBosses;

private void Start()
{
    foreach (var bh in rewardBosses)
        if (bh != null) bh.OnDied += () => GiveBossReward(bh);
}

private void GiveBossReward(BossHealth bh)
{
    if (playerHealth != null)
        playerHealth.IncreaseMaxHealth(bossMaxHealthBonus, true);
}
```
- Gán boss cụ thể vào danh sách **Reward Bosses** trong inspector.
- Chỉ những boss này sẽ cho quà khi chết.

### PlayerKiller.cs
```csharp
- OnTriggerEnter / OnCollisionEnter → TryKillPlayer()
- Tìm PlayerHealth component
- Gọi TakeDamage(9999)
- Kill một lần thôi (hasKilled flag)
```

> Note: all enemies now include a `damageStunDuration` parameter (default 2s). After taking damage they will chase but cannot start a new attack until the timer expires.

---

## 🎨 UI CUSTOMIZATION

Nếu muốn make UI đẹp hơn:

### Death Menu:
```
- Add Panel với dark background (0,0,0,150) transparent
- Add "YOU DIED" text ở trên
- Buttons dưới
- Font size: 36-48
```

### Victory Menu:
```
- Add Panel với gold/victory color background
- Add "VICTORY!" text ở trên
- Buttons dưới
- Add champion crest/medal image
```

---

**Đọc xong các bước này, bạn có thể setup đầy đủ hệ thống Death/Victory menu!** ✅

Nếu gặp vấn đề, check console logs - tất cả scripts đều có debug.log()

