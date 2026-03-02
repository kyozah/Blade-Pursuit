# Hệ Thống Âm Thanh Blade Pursuit - Hướng Dẫn Đầy Đủ

## 📋 Mục Lục

1. [Giới Thiệu](#giới-thiệu)
2. [Cài Đặt Nhanh](#cài-đặt-nhanh)
3. [Các Tệp Được Bao Gồm](#các-tệp-được-bao-gồm)
4. [Kiến Trúc Hệ Thống](#kiến-trúc-hệ-thống)
5. [Hướng Dẫn Chi Tiết](#hướng-dẫn-chi-tiết)
6. [Gỡ Lỗi](#gỡ-lỗi)
7. [FAQ](#faq)

---

## Giới Thiệu

Đây là hệ thống âm thanh hoàn toàn tích hợp cho trò chơi Blade Pursuit, cung cấp:

- ✅ **Âm thanh Player**: Tấn công, chuyển động, sát thương, chết
- ✅ **Âm thanh Kẻ Địch**: Hỗ trợ 3 loại (Skeleton, Fly, Tank)
- ✅ **Hệ Thống Quản Lý Tập Trung**: AudioManager Singleton
- ✅ **Tích Hợp Tự Động**: Không cần thay đổi mã trò chơi
- ✅ **Ghi Âm Debug**: Theo dõi tất cả các sự kiện âm thanh

**Không cần sửa đổi bất kỳ script trò chơi nào!**

---

## Cài Đặt Nhanh

### Bước 1: Tạo AudioManager
```
Nhấp chuột phải trong Hierarchy → Create Empty
Đặt tên: AudioManager
Add Component: AudioManager.cs
```

### Bước 2: Cấu Hình Âm Thanh
```
Trong Inspector AudioManager:
- Gán clip vào từng danh sách âm thanh
- Đặt volume phù hợp (0.8 cho tấn công, 0.4 cho bước chân)
- Thiết lập pitch (khuyến nghị: 1.0)
```

### Bước 3: Thêm Components
```
Player: Add PlayerAudioSystem.cs
Mỗi Enemy: Add EnemyAudioSystem.cs (chọn loại)
```

### Bước 4: Kiểm Tra
```
Nhấn Play → Nghe âm thanh phát ra
Mở Console để xem debug logs
```

**Hoàn tất!** Hệ thống âm thanh đã sẵn sàng.

---

## Các Tệp Được Bao Gồm

### Script Audio (Tạo Mới)

| Tệp | Mục Đích | Vị Trí |
|-----|---------|--------|
| **AudioManager.cs** | Quản lý tập trung âm thanh | Assets/Assets/Character/Scripts/ |
| **PlayerAudioSystem.cs** | Xử lý âm thanh Player | Assets/Assets/Character/Scripts/ |
| **EnemyAudioSystem.cs** | Xử lý âm thanh Kẻ Địch | Assets/Assets/Character/Scripts/ |

### Tài Liệu (Hướng Dẫn)

| Tệp | Nội Dung |
|-----|---------|
| **START_HERE_AUDIO_SETUP_VN.md** | Hướng dẫn bắt đầu chi tiết |
| **AUDIO_SYSTEM_GUIDE_VN.md** | Hướng dẫn đầy đủ và kiến trúc |
| **AUDIO_SETUP_QUICK_VN.md** | Hướng dẫn thiết lập nhanh |
| **AUDIO_ASSETS_ORGANIZATION_VN.md** | Tổ chức tài nguyên âm thanh |
| **IMPLEMENTATION_CHECKLIST_VN.md** | Danh sách kiểm tra triển khai |
| **README_AUDIO_SYSTEM_VN.md** | Tệp này |

### Script Được Sửa Đổi

| Tệp | Thay Đổi | Vị Trí |
|-----|---------|--------|
| **Enemy.cs** | Thêm audio system reference | Dòng 61 |
| | Gọi PlayAttackSound() | Dòng 300 |
| | Gọi PlayDamageSound() | Dòng 460 |
| | Gọi PlayDeathSound() | Dòng 590 |
| **PlayerHealth.cs** | Thêm audio system reference | Dòng 55 |
| | Gọi PlayDamageSound() | Dòng 115 |
| | Gọi PlayDeathSound() | Dòng 245 |
| **Skeleton.cs** | Không thay đổi (EnemyAudioSystem tự động) | - |
| **Fly.cs** | Không thay đổi (EnemyAudioSystem tự động) | - |
| **Tank.cs** | Không thay đổi (EnemyAudioSystem tự động) | - |

---

## Kiến Trúc Hệ Thống

### Sơ Đồ Kiến Trúc

```
┌─────────────────────────────────────────────────────┐
│                  AudioManager (Singleton)           │
│  Quản lý tập trung tất cả danh sách âm thanh       │
└──────────────┬──────────────┬──────────────┬────────┘
               │              │              │
        ┌──────▼────────┐ ┌──▼──────────┐ ┌─▼────────────┐
        │PlayerAudioSys │ │EnemyAudioSys│ │ Audio Sources│
        │   - Attack    │ │ - Skeleton  │ │ - Player     │
        │   - Movement  │ │ - Fly       │ │ - Enemies    │
        │   - Damage    │ │ - Tank      │ └──────────────┘
        │   - Death     │ │ - Mix dùng  │
        └──────────────┘ │   Enemy Type │
                        │   field      │
                        └──────────────┘
```

### Luồng Dữ Liệu

```
[Sự kiện trò chơi]
    ↓
[Audio System]
    ↓
[AudioManager.PlayRandomSound()]
    ↓
[Audio Source]
    ↓
[Loa]
```

### Ví Dụ Cụ Thể

```
Player Tấn Công:
  AttackComboController.PerformAttack()
    → OnAttackStart?.Invoke()
    → PlayerAudioSystem.OnAttackStarted()
    → AudioManager.PlayRandomSoundOnSource()
    → Audio Source phát clip tấn công
    → Âm thanh phát ra loa

Skeleton Chuyển Động:
  Enemy.Update() - Xác phát hiện chuyển động
    → EnemyAudioSystem.PlayMovementSound()
    → AudioManager.PlayRandomSoundOnSource()
    → Phát âm thanh Skeleton chuyển động
```

---

## Hướng Dẫn Chi Tiết

### 1. Chuẩn Bị Clip Âm Thanh

**Tối Thiểu (16 clips):**
- 4 cho Player (tấn công, chuyển động, sát thương, chết)
- 4 cho Skeleton
- 4 cho Fly
- 4 cho Tank

**Tối Ưu (40-50 clips):**
- 3 biến thể cho mỗi loại âm thanh
- Sự đa dạng lớn hơn và trò chơi sinh động hơn

### 2. Tổ Chức Tệp

```
Assets/
  Audio/
    SFX/
      Player/
        Attacks/
        Movement/
        Damage/
        Death/
      Enemies/
        Skeleton/
        Fly/
        Tank/
```

### 3. Cài Đặt AudioManager

**Mở AudioManager trong Inspector và:**
1. Mở rộng mỗi danh sách âm thanh
2. Đặt Size thành số lượng clip
3. Kéo clip vào mỗi Element
4. Đặt Volume và Pitch

### 4. Gắn Components

```csharp
// Player
GameObject player = GameObject.Find("Player");
player.AddComponent<PlayerAudioSystem>();

// Hoặc sử dụng Inspector: Add Component → PlayerAudioSystem

// Enemies
foreach (GameObject enemy in skeletons)
{
    EnemyAudioSystem system = enemy.AddComponent<EnemyAudioSystem>();
    system.enemyType = EnemyAudioSystem.EnemyType.Skeleton;
}
```

### 5. Kiểm Tra

```
1. Nhấn Play
2. Lắng nghe âm thanh:
   - Tấn công: Tấn công kẻ địch
   - Chuyển động: Di chuyển
   - Sát thương: Để bị tấn công
   - Chết: Để chết
3. Mở Console (Ctrl + Shift + C) để xem logs
```

---

## Gỡ Lỗi

### Không Có Âm Thanh

**Kiểm Tra:**
1. AudioManager GameObject có tồn tại? ✓
2. AudioManager.cs component có được thêm? ✓
3. Tất cả danh sách có clip? ✓
4. Volume không là 0? ✓
5. Main Camera có AudioListener? ✓

**Giải Pháp:**
```csharp
// Thêm vào AudioManager.Awake() để debug:
void Awake()
{
    // Kiểm tra danh sách
    Debug.Log($"Player Attack Sounds: {playerAttackSounds.Count} clips");
    
    // Kiểm tra từng clip
    foreach (var sound in playerAttackSounds)
    {
        Debug.Log($"  - {sound.clip.name}: volume={sound.volume}");
    }
}
```

### Lỗi Biên Dịch

**"The type or namespace name 'AudioManager' could not be found"**
- Kiểm tra AudioManager.cs ở đúng thư mục
- Kiểm tra tên class (phải là `AudioManager`)

**"NullReferenceException: audioSystem is null"**
- Kiểm tra EnemyAudioSystem component được thêm
- Kiểm tra Enemy script tìm được component

### Âm Thanh Quá To/Yếu

**Giải Pháp:**
1. Điều chỉnh `volume` trong SoundEffect (0.0-1.0)
2. Kiểm tra clip gốc có vấn đề
3. Kiểm tra Master Volume trong Game View
4. Kiểm tra cài đặt âm thanh hệ thống

### Âm Thanh Bị Cắt Xén

**Giải Pháp:**
1. Kiểm tra xem clip đủ dài
2. Tăng `movementSoundInterval` để tránh xung đột
3. Kiểm tra xem Audio Source khác đang phát không

---

## FAQ

### Q: Tôi có cần thay đổi script trò chơi không?
**A:** Không! Hệ thống âm thanh tích hợp tự động. Chỉ cần thêm components.

### Q: Âm thanh sẽ chạy song song với animation không?
**A:** Có! Âm thanh được kích hoạt từ sự kiện, không bị chặn animation.

### Q: Tôi có thể thêm âm thanh riêng cho mỗi kẻ địch không?
**A:** Có! EnemyAudioSystem hỗ trợ Skeleton, Fly, Tank. Bạn có thể thêm loại khác.

### Q: Làm cách nào để tắt âm thanh cụ thể?
**A:** Đặt danh sách âm thanh thành Size = 0 hoặc bỏ clip.

### Q: Hệ thống âm thanh có ảnh hưởng đến hiệu suất không?
**A:** Không đáng kể. Sử dụng Audio Source tối thiểu để tối ưu hóa.

### Q: Tôi có thể sử dụng âm thanh 3D không?
**A:** Có! Đặt `spatialBlend = 1f` trên Audio Source.

### Q: Làm cách nào để điều chỉnh pitch ngẫu nhiên?
**A:** Sử dụng `randomPitchMin` và `randomPitchMax` trong SoundEffect.

### Q: Có thể phát 2 âm thanh cùng lúc không?
**A:** Có! Sử dụng các Audio Source khác nhau (tấn công vs chuyển động).

---

## Mẹo & Thủ Thuật

### Mẹo 1: Sử Dụng Pitch Ngẫu nhiên
```csharp
public bool useRandomPitch = true;
public float pitchVariation = 0.1f; // ±10% pitch
```
Điều này làm cho âm thanh lặp lại ít nhẩn đơn điệu hơn.

### Mẹo 2: Điều Chỉnh Volume Động
```csharp
// Fade out âm thanh từ từ
StartCoroutine(FadeOutAudio(audioSource, 2f));
```

### Mẹo 3: Phát Âm Thanh Cụ Thể
```csharp
// Thay vì random, phát clip cụ thể
var specificSound = AudioManager.Instance.playerAttackSounds[0];
AudioManager.Instance.PlaySpecificSound(source, specificSound);
```

### Mẹo 4: Kiểm Tra Hiệu Suất
```
Window → Analysis → Profiler → Audio
```
Kiểm tra số lượng Audio Sources đang sử dụng.

---

## Bước Tiếp Theo (Tùy Chọn)

### Nâng Cao

1. **Âm Nhạc Nền**
   - Tạo AudioSource riêng cho nhạc
   - Chuyển đổi track dựa trên trạng thái trò chơi

2. **Audio Mixer**
   - Kiểm soát volume theo nhóm (Master, SFX, Music)
   - Áp dụng effects (reverb, echo)

3. **Menu Âm Lượng**
   - Tạo thanh trượt điều chỉnh volume
   - Lưu cài đặt âm lượng

4. **Voice Acting** (Nâng Cao)
   - Thêm tiếng nói cho câu thoại
   - Đồng bộ hóa với animation

---

## Liên Hệ & Hỗ Trợ

### Tài Liệu Khác

- `START_HERE_AUDIO_SETUP_VN.md` - Hướng dẫn bắt đầu
- `AUDIO_SYSTEM_GUIDE_VN.md` - Hướng dẫn chi tiết
- `AUDIO_SETUP_QUICK_VN.md` - Hướng dẫn nhanh
- `IMPLEMENTATION_CHECKLIST_VN.md` - Danh sách kiểm tra

### Tài Nguyên

- **Clip Âm Thanh Miễn Phí:** Freesound.org, Pixabay, OpenGameArt
- **Công Cụ Chỉnh Sửa:** Audacity (miễn phí), Adobe Audition (trả phí)
- **Tham Khảo Unity:** docs.unity3d.com/ScriptReference/AudioSource.html

---

## Tóm Tắt

| Yếu Tố | Chi Tiết |
|--------|---------|
| **Số Script Tạo** | 3 (AudioManager, PlayerAudioSystem, EnemyAudioSystem) |
| **Số Script Sửa** | 2 (Enemy, PlayerHealth) |
| **Thời Gian Cài Đặt** | 30-60 phút (tùy vào số clip) |
| **Clip Tối Thiểu** | 16 |
| **Clip Tối Ưu** | 40-50 |
| **Không Cần Thay Đổi** | Skeleton, Fly, Tank scripts |

---

## Versioning

- **Phiên Bản 1.0** - Hệ thống âm thanh hoàn chỉnh
  - AudioManager Singleton
  - PlayerAudioSystem
  - EnemyAudioSystem với hỗ trợ 3 loại
  - Tài liệu đầy đủ

---

**Hệ thống âm thanh đã sẵn sàng! Bắt đầu với START_HERE_AUDIO_SETUP_VN.md.**
