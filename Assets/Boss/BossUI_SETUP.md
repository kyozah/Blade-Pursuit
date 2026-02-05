# Boss UI Setup (Intro name + Screen-space Health Bar) 🔧
Mục tiêu: Hiển thị tên boss lớn khi boss phát hiện người chơi (3s) rồi fade sang thanh máu trên màn hình (kiểu Hollow Knight).
---
## Thành phần liên quan (files)
- `Assets/Boss/Scripts/BossHealth.cs` ✅ (đã thêm `BossName`, `OnHealthChanged`, `OnDied`)
- `Assets/Boss/Scripts/BossHealthScreenUI.cs` ✅ (Screen-space UI, intro + main HUD, smoothing)
- `Assets/Boss/Scripts/BossUIManager.cs` ✅ (singleton để bind và show UI)
- `Assets/Boss/Scripts/BossBrain.cs` ✅ (gọi `BossUIManager.Instance.ShowBoss(this)` khi phát hiện và khi phase2 roars)
---
## 1) Tạo Canvas UI (Screen Space - Overlay) 🖥️
1. Trong Scene tạo `Canvas` → **Render Mode = Screen Space - Overlay**.
2. Tạo container UI (Empty GameObject) con Canvas, attach `CanvasGroup` component (dùng để show/hide/fade) — gọi ví dụ `BossUI_Root`.
3. Trong `BossUI_Root` tạo hai group con:
   - `IntroGroup` (contains a large `Text` or TMP Text *or* an `Image` for boss icon)
     - Add `CanvasGroup` (assign to `introCanvasGroup`)
     - Add `Text` → set style/size → assign to `introNameText` (optional if using image)
     - Add `Image` → assign to `introImage` (optional: used to show boss portrait/logo instead of text). *Intro thường là image-only nếu `introImage` có sprite.*
   - `MainGroup` (contains small `Text` for name + `Slider` for HP, or a small icon)
     - Add `CanvasGroup` (assign to `mainCanvasGroup`)
     - Add `Slider` (non-interactable) → assign to `healthSlider`
     - Add `Text` (small) → assign to `bossNameText` (optional if using icon)
     - Add `Image` (small) → assign to `bossIconImage` (optional: small portrait/icon for main HUD)
     - Add `Image` (frame/background for health bar) → assign to `healthBarImage` (optional: decorative image behind the slider)
4. (Optional) Add a top-level `CanvasGroup` on `BossUI_Root` for overall visibility → assign to `canvasGroup` on script.
---
## 2) Thêm component `BossHealthScreenUI` 🧩
- Attach `BossHealthScreenUI` to `BossUI_Root` (hoặc một GameObject quản lý panels).
- Gán references trong Inspector:
  - `healthSlider` → (Slider)
  - `bossNameText` → (Text)
  - `introNameText` → (Text in `IntroGroup`)
  - `introImage` → (Image in `IntroGroup`) **optional: intro image shown for 3s**
  - `introCanvasGroup` → (IntroGroup CanvasGroup)
  - `bossIconImage` → (Image in `MainGroup`) **optional: small icon next to name**
  - `healthBarImage` → (Image in `MainGroup`) **optional: frame/background for slider**
  - `mainCanvasGroup` → (MainGroup CanvasGroup)
  - `canvasGroup` → (BossUI_Root CanvasGroup) (optional)
- Tùy chỉnh các parameter: `smoothSpeed`, `introDuration` (mặc định 3s), `showDuration`. Note: leave `bossHealth` field empty in inspector (do not pre-bind) to avoid showing UI on game start.
---
## 3) Thiết lập `BossUIManager` singleton 🗂️
1. Tạo GameObject (ví dụ `BossUIManager`) trong Scene.
2. Attach `BossUIManager` script.
3. Assign `bossUI` = `BossUI_Root` (GameObject có `BossHealthScreenUI`).
> Lưu ý: `BossBrain` sẽ tự gọi `BossUIManager.Instance.ShowBoss(this)` khi detect player / chuyển phase.
---
## 4) Chuẩn bị Boss prefab / object
- Trên prefab boss, đảm bảo có `BossBrain` và một child chứa `BossHealth`.
- Trong `BossHealth` set `BossName` (ví dụ: "Colosseum Warden") và (tuỳ chọn) gán `BossIcon` (Sprite) để hiển thị hình ảnh thay vì text.
---
## 5) Test nhanh ✅
1. Play Scene.
2. Di chuyển nhân vật vào trong `detectRange` của boss.
3. Kỳ vọng: **Intro** hiển thị **một ảnh lớn** (3s) — nếu `BossIcon` hoặc override sprite được gán thì sẽ hiển thị **hình ảnh** thay vì text. Sau 3s, intro fade sang **Main HUD** hiển thị:
   - `bossNameText` (tên nhỏ) hoặc `bossIconImage` (nếu có sprite)
   - `healthBarImage` (frame/background, nếu gán)
   - `healthSlider` (slider hiển thị HP) 

Khi boss bị thương, slider sẽ cập nhật và UI sẽ hiện lại; khi boss chết UI ẩn. Nếu muốn image-only intro mà không show text, chỉ cần gán `introImage` cho boss hoặc truyền sprite override.

**Ví dụ gọi API**:

- Tự động từ `BossBrain` (mặc định):

```csharp
BossUIManager.Instance.ShowBoss(this);
```

- Gọi với tên override, sprite override cho icon, và sprite cho healthbar:

```csharp
// show with custom name, intro icon and custom healthbar image
BossUIManager.Instance.ShowBoss(this, "Overlord", introSprite, healthbarSprite);
```

- Thay đổi sprite khi UI đang hiển thị (icon):

```csharp
BossUIManager.Instance.SetCurrentBossDisplaySprite(newSprite, writeBackToBoss: false);
```

- Thay đổi healthbar image khi UI đang hiển thị:

```csharp
BossUIManager.Instance.SetCurrentBossHealthBarSprite(healthbarSprite);
```

- Xóa override hoặc quay lại sử dụng icon/name gốc của boss:

```csharp
BossUIManager.Instance.ClearCurrentBossDisplaySprite();
BossUIManager.Instance.ClearCurrentBossHealthBarSprite();
```



---

## 6) Troubleshooting ⚠️
- Nếu không thấy UI:
  - Kiểm tra `BossUIManager.bossUI` đã gán chưa.
  - Kiểm tra `BossHealthScreenUI` có `healthSlider`/`introNameText`/`introCanvasGroup`/`introImage`/`bossIconImage`/`healthBarImage` gán đúng không.
  - Đảm bảo bạn **không** đặt `bossHealth` trực tiếp trên `BossHealthScreenUI` trong Inspector (để tránh auto-bind hiển thị khi game bắt đầu). Thay vào đó để `bossHealth` trống và để `BossUIManager.ShowBoss(...)` bind khi cần.
  - Kiểm tra `BossBrain` có gọi `BossUIManager.Instance?.ShowBoss(this)` khi detect (mặc định đã thêm trong `HandleIdle` và khi phase2 bắt đầu).
- Nếu tên/ảnh không đúng: đảm bảo `BossHealth.BossName` và `BossHealth.BossIcon` đã set cho instance boss, hoặc dùng `BossUIManager.SetCurrentBossDisplaySprite` / `SetCurrentBossDisplayName` để override runtime.

---

## 7) Tùy chọn mở rộng (gợi ý) ✨
- Tạo prefab `Assets/Boss/Prefabs/BossUI.prefab` chứa layout sẵn và art.
- Thêm animation hoặc particle khi intro bắt đầu.
- Thay slider bằng segmented hearts/shields (như Hollow Knight) bằng custom control.
- Tự động bind/unbind khi player enter/leave boss arena bằng trigger collider (tôi có thể thêm script ví dụ nếu muốn).

---

Nếu bạn muốn, tôi có thể tiếp tục: tạo **prefab Boss UI mẫu** + **script auto-bind** (trigger/arena) để thao tác dễ hơn. Bạn muốn tôi tạo luôn prefab không? ✅