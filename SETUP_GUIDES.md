# Hướng dẫn tích hợp Death Menu, Victory Menu, và Player Kill System

Dự án của bạn đã được cập nhật với 3 tính năng chính:

## 1. ☠️ Death Menu (Menu Chết)
**File:** `Assets/Assets/Character/Scripts/DeathMenuUI.cs`

### Chức năng:
- Khi player hết máu, chờ 2 giây rồi hiển thị Death Menu
- Nút "Play" để tải lại scene Gameplay từ đầu
- Nút "Quit" để thoát game

### Cách setup trong Unity:
1. Tở scene **Gameplay.unity**
2. Tạo 1 Canvas mới (Right-click > UI > Canvas)
3. Thêm component **DeathMenuUI** vào Canvas
3. Tạo 2 Button trong Canvas: "Replay" và "Menu"
5. Gán các Button vào fields:
   - Drag "Replay" button → Replay Button field
   - Drag "Menu" button → Menu Button field
6. CanvasGroup sẽ auto-detect hoặc drag Canvas vào field
7. Thiết lập CanvasGroup sao cho:
   - Blocks Raycasts = ticked
   - Initial Alpha = 0 (sẽ fade in)
> ⚠️ Nếu bạn có các UI khác (máu, boss health) dùng CanvasGroup, đảm bảo chúng KHÔNG chặn raycast khi Death Menu xuất hiện. Script sẽ tự tắt `blocksRaycasts` của các CanvasGroup khác khi menu hiển thị, nhưng tốt nhất nên cân nhắc thứ tự các Canvas (Death Menu nên ở trên cùng).
---

## 2. 🏆 Victory Menu (Menu Chiến Thắng)

### Boss reward
- Gần như bạn có thể tự chọn những boss nào sẽ ban thưởng: kéo các đối tượng `BossHealth` cần cho quà vào danh sách **Reward Bosses** trên `GameManager`.
- Khi một boss trong danh sách đó chết, player nhận **tăng tối đa 100 máu** và hồi đầy. (Số lượng 100 có thể chỉnh trong `bossMaxHealthBonus`.)
- Nếu boss không nằm trong danh sách, không có quà.

**File:** `Assets/Assets/Character/Scripts/VictoryMenuUI.cs`

### Chức năng:
- Khi player tiêu diệt **tất cả** các boss được gán trong danh sách, đợi vài giây để animation chết chạy xong rồi hiển thị Victory Menu
- **Quan trọng:** gán tất cả boss nói trên (ví dụ boss 1, boss 2) vào mảng "Bosses" của VictoryMenuUI. Nếu chỉ có một boss trong list, menu sẽ xuất hiện ngay khi boss đó chết, bất kể còn boss khác trong scene.
- Menu chỉ xuất hiện sau khi tất cả boss được liệt kê chết (hỗ trợ nhiều boss)
- Nút "Continue" để quay lại scene Menu
- Nút "Quit" để thoát game

### Cách setup trong Unity:
1. Tở scene **Gameplay.unity**
2. Tạo 1 Canvas khác cho Victory Menu (hoặc dùng Canvas cũ)
3. Thêm component **VictoryMenuUI** vào Canvas
4. Tạo 2 Button: "Continue" và "Quit"
5. Gán các Button vào fields:
   - Drag button quay lại menu → Continue Button field
   - Drag button thoát → Quit Button field
6. Script sẽ tự động subscribe vào BossHealth.OnDied event

---

## 3. ⚠️ Player Killer (Kill Player on Collision)
**File:** `Assets/Assets/Character/Scripts/PlayerKiller.cs`

### Chức năng:
- Gắn script lên vật thể để kill player khi va chạm
- Hỗ trợ cả Trigger và Collision
- Cấu hình Instant Kill hoặc Damage-based

### Cách setup trong Unity:
1. Chọn GameObject cần kill player (lava, spike, cliff, etc.)
2. Thêm component **PlayerKiller** vào đó
3. Thiết lập:
   - **Use Instant Kill**: TRUE (để kill ngay lập tức)
   - **Requires Collider**: Đảm bảo object có Collider
4. Nếu dùng Trigger:
   - Collider → Is Trigger: ON
   - Script sẽ dùng OnTriggerEnter
5. Nếu dùng Collision thường:
   - Collider → Is Trigger: OFF
   - Script sẽ dùng OnCollisionEnter

### Ví dụ setup Lava:
- GameObject: "Lava"
- Add Collider (set Is Trigger = ON)
- Add PlayerKiller component
- Set Use Instant Kill = TRUE
- Player sẽ bị kill ngay khi chạm vào

---

## 4. 🎮 Game Manager (Tùy chọn)
**File:** `Assets/Assets/Character/Scripts/GameManager.cs`

### Chức năng:
- Quản lý trạng thái chung của game
- Singleton pattern

### Cách setup (tùy chọn):
1. Tạo 1 empty GameObject: "GameManager"
2. Thêm component **GameManager**
3. Nó sẽ tự tìm PlayerHealth, DeathMenuUI, VictoryMenuUI

---

## 📋 Tóm tắt Script được tạo:

| Script | Vị trí | Mục đích |
|--------|--------|---------|
| **DeathMenuUI.cs** | Assets/Assets/Character/Scripts/ | Quản lý Death Menu UI |
| **VictoryMenuUI.cs** | Assets/Assets/Character/Scripts/ | Quản lý Victory UI |
| **PlayerKiller.cs** | Assets/Assets/Character/Scripts/ | Kill player on collision |
| **GameManager.cs** | Assets/Assets/Character/Scripts/ | Quản lý game state |

---

## 🔧 Các file đã sửa:

- **menu.cs**: Cập nhật để tên scene là "Gameplay" và reset Time.timeScale

---

## ✅ Checklist setup:
- [ ] Create DeathMenuUI Canvas với buttons Play/Quit
- [ ] Create VictoryMenuUI Canvas với buttons Continue/Quit
- [ ] Add PlayerKiller component vào hazard objects
- [ ] Test Death Menu (kill player để see)
- [ ] Test Victory Menu (defeat boss để see)
- [ ] Test Play button reload scene
- [ ] Test Continue button go to Menu
- [ ] Test Quit buttons

---

## 🐛 Debug Tips:

Nhấn Play và:
1. Để trigger death: gọi PlayerHealth.TakeDamage() hoặc va chạm vất PlayerKiller
2. Để trigger victory: gọi BossHealth.TakeDamage() hoặc tấn công boss

Console sẽ hiển thị debug logs từ các script.

---

## 📝 Ghi chú:
- Cả Death Menu và Victory Menu đều pause game (Time.timeScale = 0)
- Menu sẽ fade in suavely (0.5 giây)
- Player chỉ kill 1 lần (hasKilled flag)
- Menu auto-detect PlayerHealth, BossHealth components

