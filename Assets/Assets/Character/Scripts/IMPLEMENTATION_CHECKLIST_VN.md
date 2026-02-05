# Danh Sách Kiểm Tra Triển Khai Hệ Thống Âm Thanh

## Danh Sách Kiểm Tra Toàn Bộ Quá Trình

Sử dụng danh sách này để theo dõi tiến độ cài đặt hệ thống âm thanh.

---

## Giai Đoạn 1: Chuẩn Bị

### Tệp Script

- [ ] **AudioManager.cs** được tạo tại `Assets/Assets/Character/Scripts/`
- [ ] **PlayerAudioSystem.cs** được tạo tại `Assets/Assets/Character/Scripts/`
- [ ] **EnemyAudioSystem.cs** được tạo tại `Assets/Assets/Character/Scripts/`
- [ ] Tất cả 3 script biên dịch không có lỗi
- [ ] Không có cảnh báo biên dịch

### Tổ Chức Tài Nguyên

- [ ] Thư mục `Assets/Audio/` được tạo
- [ ] Thư mục con `SFX/`, `Music/` được tạo
- [ ] Thư mục `SFX/Player/` được tạo với các thư mục con
- [ ] Thư mục `SFX/Enemies/` được tạo với Skeleton/, Fly/, Tank/
- [ ] Tất cả tệp clip được tổ chức đúng

### Tệp Clip Âm Thanh

- [ ] Ít nhất 16 clip âm thanh được thu thập (1 cho mỗi loại/hành động)
- [ ] Tất cả clip ở định dạng WAV hoặc MP3
- [ ] Tất cả clip được đặt tên theo quy ước
- [ ] Không có clip bị hỏng hoặc không có tiếng

---

## Giai Đoạn 2: Thiết Lập AudioManager

### Tạo GameObject

- [ ] GameObject trống được tạo và đặt tên `AudioManager`
- [ ] AudioManager được đặt tại vị trí gốc (0, 0, 0) - không bắt buộc
- [ ] AudioManager không có vật lý hoặc va chạm

### Thêm Component

- [ ] **AudioManager.cs** component được thêm vào AudioManager GameObject
- [ ] Script biên dịch không có lỗi
- [ ] Không có cảnh báo trong Inspector

### Cấu Hình Âm Thanh Player

#### Player Attack Sounds
- [ ] Size được đặt thành 1 hoặc nhiều hơn
- [ ] Mỗi Element có:
  - [ ] **Clip** được gán (clip tấn công của player)
  - [ ] **Volume** được đặt (khuyến nghị: 0.8)
  - [ ] **Pitch** được đặt thành 1.0
  - [ ] **Loop** được tắt (OFF)
  - [ ] **Random Pitch Min** được đặt (khuyến nghị: 0.95)
  - [ ] **Random Pitch Max** được đặt (khuyến nghị: 1.05)

#### Player Movement Sounds
- [ ] Size được đặt thành 1 hoặc nhiều hơn
- [ ] Mỗi Element có:
  - [ ] **Clip** được gán (clip bước chân)
  - [ ] **Volume** được đặt (khuyến nghị: 0.4-0.5)
  - [ ] **Pitch** được đặt thành 1.0
  - [ ] **Loop** được tắt
  - [ ] **Random Pitch** được đặt (khuyến nghị: 0.98-1.02)

#### Player Damage Sounds
- [ ] Size được đặt thành 1 hoặc nhiều hơn
- [ ] Mỗi Element có:
  - [ ] **Clip** được gán
  - [ ] **Volume** được đặt (khuyến nghị: 0.7)
  - [ ] **Pitch** được đặt thành 1.0
  - [ ] **Loop** được tắt

#### Player Death Sounds
- [ ] Size được đặt thành 1 hoặc nhiều hơn
- [ ] Mỗi Element có:
  - [ ] **Clip** được gán (clip chết dài 1-3 giây)
  - [ ] **Volume** được đặt (khuyến nghị: 1.0)
  - [ ] **Pitch** được đặt (khuyến nghị: 0.8-1.0)
  - [ ] **Loop** được tắt

### Cấu Hình Âm Thanh Skeleton

- [ ] **Skeleton Attack Sounds** được cấu hình
  - [ ] Size ≥ 1
  - [ ] Tất cả Element có clip, volume, pitch
  
- [ ] **Skeleton Movement Sounds** được cấu hình
  - [ ] Size ≥ 1
  - [ ] Tất cả Element được cấu hình
  
- [ ] **Skeleton Damage Sounds** được cấu hình
  - [ ] Size ≥ 1
  - [ ] Element được cấu hình
  
- [ ] **Skeleton Death Sounds** được cấu hình
  - [ ] Size ≥ 1
  - [ ] Element được cấu hình

### Cấu Hình Âm Thanh Fly

- [ ] **Fly Attack Sounds** được cấu hình (Size ≥ 1)
- [ ] **Fly Movement Sounds** được cấu hình (Size ≥ 1)
- [ ] **Fly Damage Sounds** được cấu hình (Size ≥ 1)
- [ ] **Fly Death Sounds** được cấu hình (Size ≥ 1)
- [ ] Tất cả các danh sách có clip và cài đặt đúng

### Cấu Hình Âm Thanh Tank

- [ ] **Tank Attack Sounds** được cấu hình (Size ≥ 1)
- [ ] **Tank Movement Sounds** được cấu hình (Size ≥ 1)
- [ ] **Tank Damage Sounds** được cấu hình (Size ≥ 1)
- [ ] **Tank Death Sounds** được cấu hình (Size ≥ 1)
- [ ] Tất cả các danh sách có clip và cài đặt đúng

---

## Giai Đoạn 3: Thiết Lập Player Audio

### Thêm Component

- [ ] Chọn GameObject **Player** trong Hierarchy
- [ ] **PlayerAudioSystem.cs** component được thêm
- [ ] Script biên dịch không có lỗi
- [ ] Không có lỗi tham chiếu trong Inspector

### Cấu Hình (Tùy Chọn)

- [ ] **Use Random Pitch** được bật (ON) để có sự đa dạng
- [ ] **Pitch Variation** được đặt thành 0.1 hoặc giá trị mong muốn
- [ ] Audio Sources sẽ được tạo tự động khi Play

### Kiểm Tra Audio Sources

- [ ] Khi Play, kiểm tra xem các Audio Source đã được tạo:
  - [ ] attackSource
  - [ ] movementSource
  - [ ] damageSource
  - [ ] deathSource

---

## Giai Đoạn 4: Thiết Lập Kẻ Địch Audio

### Cho Mỗi Skeleton GameObject

- [ ] **EnemyAudioSystem.cs** component được thêm
- [ ] Script biên dịch không có lỗi
- [ ] **Enemy Type** được đặt thành `Skeleton`
- [ ] Các cài đặt tùy chọn:
  - [ ] **Use Random Pitch** được bật (tùy chọn)
  - [ ] **Pitch Variation** được đặt (tùy chọn)
  - [ ] **Movement Sound Interval** được đặt (khuyến nghị: 0.5)

### Cho Mỗi Fly GameObject

- [ ] **EnemyAudioSystem.cs** component được thêm
- [ ] Script biên dịch không có lỗi
- [ ] **Enemy Type** được đặt thành `Fly`
- [ ] Các cài đặt tùy chọn được cấu hình nếu cần

### Cho Mỗi Tank GameObject

- [ ] **EnemyAudioSystem.cs** component được thêm
- [ ] Script biên dịch không có lỗi
- [ ] **Enemy Type** được đặt thành `Tank`
- [ ] Các cài đặt tùy chọn được cấu hình nếu cần

### Kiểm Tra

- [ ] Tất cả kẻ địch trong scene có EnemyAudioSystem
- [ ] Tất cả EnemyAudioSystem có Enemy Type được đặt đúng
- [ ] Không có lỗi hoặc cảnh báo

---

## Giai Đoạn 5: Kiểm Tra Trong Play Mode

### Âm Thanh Tấn Công

- [ ] Nhấn Play
- [ ] Tấn công kẻ địch (nhấn tấn công)
- [ ] **Nghe thấy** âm thanh tấn công của player
- [ ] **Nghe thấy** kẻ địch phát âm thanh khi bị tấn công
- [ ] Âm thanh phát ra từ vị trí đúng
- [ ] Âm thanh có độ to phù hợp (không quá to/yếu)

### Âm Thanh Chuyển Động

- [ ] Nhấn Play
- [ ] Di chuyển player (W, A, S, D)
- [ ] **Nghe thấy** âm thanh bước chân
- [ ] Khi dừng lại, âm thanh bước chân dừng
- [ ] Khi chạy nhanh, âm thanh tần suất thích hợp

### Âm Thanh Sát Thương

- [ ] Nhấn Play
- [ ] Để kẻ địch tấn công player
- [ ] **Nghe thấy** âm thanh sát thương của player
- [ ] Âm thanh có độ to phù hợp
- [ ] Âm thanh không bị cắt xén

### Âm Thanh Chết

- [ ] Nhấn Play
- [ ] Để player chết (hoặc gọi chết bằng console)
- [ ] **Nghe thấy** âm thanh chết của player
- [ ] Âm thanh phát hoàn toàn (không bị cắt)

### Âm Thanh Kẻ Địch

- [ ] Nhấn Play
- [ ] Nếu có Skeleton: **Nghe thấy** âm thanh Skeleton
  - [ ] Tấn công: ✓
  - [ ] Chuyển động: ✓
  - [ ] Sát thương: ✓
  - [ ] Chết: ✓

- [ ] Nếu có Fly: **Nghe thấy** âm thanh Fly
  - [ ] Tấn công: ✓
  - [ ] Chuyển động: ✓
  - [ ] Sát thương: ✓
  - [ ] Chết: ✓

- [ ] Nếu có Tank: **Nghe thấy** âm thanh Tank
  - [ ] Tấn công: ✓
  - [ ] Chuyển động: ✓
  - [ ] Sát thương: ✓
  - [ ] Chết: ✓

---

## Giai Đoạn 6: Xác Minh Console Logs

- [ ] Mở Console Window: `Window` → `General` → `Console`
- [ ] Chạy trò chơi (Play)
- [ ] Kiểm tra Console để tìm:
  - [ ] "🔊 AudioManager Instance Created" hoặc tương tự
  - [ ] "🔊 PlayerAudioSystem initialized" (tùy chọn)
  - [ ] "🔊 Skeleton Audio System initialized" (cho mỗi Skeleton)
  - [ ] "🔊 Fly Audio System initialized" (cho mỗi Fly)
  - [ ] "🔊 Tank Audio System initialized" (cho mỗi Tank)
  - [ ] "🔊 Player Attack Sound" (khi tấn công)
  - [ ] "🔊 Player Movement Sound" (khi di chuyển)
- [ ] Không có NullReferenceException hoặc lỗi khác

---

## Giai Đoạn 7: Điều Chỉnh & Tối Ưu Hóa

### Cân Bằng Âm Lượng

- [ ] Âm thanh tấn công không quá to
- [ ] Âm thanh bước chân không gây bão và có thể nghe được
- [ ] Âm thanh sát thương rõ ràng
- [ ] Âm thanh chết tạo bầu không khí đầy đủ

### Tùy Chỉnh Pitch

- [ ] Âm thanh tấn công có sự đa dạng ngẫu nhiên
- [ ] Âm thanh bước chân không lặp lại đơn điệu
- [ ] Âm thanh kẻ địch phù hợp với loại (Fly cao, Tank thấp)

### Tối Ưu Hóa Hiệu Suất

- [ ] Kiểm tra CPU usage trong Profiler
- [ ] Nếu cần, giảm số lượng Audio Sources
- [ ] Nén clip âm thanh với Vorbis hoặc MP3 nếu cần

---

## Giai Đoạn 8: Tài Liệu & Bảo Trì

### Tài Liệu

- [ ] **START_HERE_AUDIO_SETUP_VN.md** được tạo
- [ ] **AUDIO_SYSTEM_GUIDE_VN.md** được tạo
- [ ] **AUDIO_SETUP_QUICK_VN.md** được tạo
- [ ] **AUDIO_ASSETS_ORGANIZATION_VN.md** được tạo
- [ ] Tất cả tài liệu ở định dạng Markdown

### Bảo Trì

- [ ] Tất cả Script được backup
- [ ] Tất cả cài đặt được ghi chép
- [ ] Danh sách clip âm thanh được lưu giữ
- [ ] Scene được lưu sau khi hoàn thành

---

## Sự Cố Thường Gặp - Kiểm Tra

### Nếu Không Có Âm Thanh

- [ ] AudioManager GameObject có tồn tại trong scene không?
- [ ] AudioManager.cs component có được thêm không?
- [ ] Tất cả danh sách âm thanh có clip không?
- [ ] Volume không được đặt thành 0?
- [ ] AudioListener có trên Main Camera không?

### Nếu Có Lỗi Biên Dịch

- [ ] Tất cả 3 script có trong thư mục Scripts không?
- [ ] Có bất kỳ lỗi cú pháp nào không?
- [ ] Có tham chiếu vòng không?
- [ ] Kiểm tra Console để biết thêm chi tiết

### Nếu Âm Thanh Quá To/Yếu

- [ ] Kiểm tra volume trong SoundEffect
- [ ] Kiểm tra cài đặt âm thanh hệ thống
- [ ] Kiểm tra Master Volume slider trong Game View
- [ ] Kiểm tra xem clip gốc có vấn đề không

### Nếu Âm Thanh Bị Cắt Xén

- [ ] Kiểm tra xem clip đủ dài không
- [ ] Kiểm tra xem có Audio Source khác đang phát không
- [ ] Tăng `movementSoundInterval` để tránh xung đột

---

## Checklist Hoàn Thành

**Khi Tất Cả Mục Được Đánh Dấu:**

✅ Hệ thống âm thanh đã được triển khai thành công!

**Bước Tiếp Theo (Tùy Chọn):**
- [ ] Thêm âm nhạc nền cho scene
- [ ] Tích hợp Audio Mixer để kiểm soát volume
- [ ] Tạo thanh điều chỉnh âm lượng trong menu
- [ ] Thêm âm thanh UI (click, select, error)
- [ ] Tối ưu hóa cho nền tảng mobile

---

## Ghi Chú

Sử dụng không gian này để ghi chú về quá trình cài đặt:

```
Ngày bắt đầu: _______________
Ngày hoàn thành: _______________
Số lượng clip sử dụng: _______________
Ghi chú: _________________________________
```

---

## Tham Chiếu Nhanh

| Bước | Hành Động | Tệp |
|------|---------|-----|
| 1 | Tạo AudioManager | AudioManager.cs |
| 2 | Cấu hình Âm Thanh | Inspector |
| 3 | Thêm PlayerAudioSystem | Player GameObject |
| 4 | Thêm EnemyAudioSystem | Mỗi Enemy |
| 5 | Kiểm Tra Play Mode | Scene |
| 6 | Xem Console Logs | Console |
| 7 | Điều Chỉnh | Inspector |
| 8 | Hoàn Thành | ✅ |
