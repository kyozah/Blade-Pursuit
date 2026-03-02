# Hướng Dẫn Hệ Thống Âm Thanh - Tài Liệu Đầy Đủ

## Mục Lục

1. [Tổng Quan Kiến Trúc](#tổng-quan-kiến-trúc)
2. [Các Component](#các-component)
3. [Cài Đặt Chi Tiết](#cài-đặt-chi-tiết)
4. [Tích Hợp Script](#tích-hợp-script)
5. [Quản Lý Âm Thanh](#quản-lý-âm-thanh)
6. [Gỡ Lỗi](#gỡ-lỗi)
7. [Ví Dụ Mã](#ví-dụ-mã)

---

## Tổng Quan Kiến Trúc

### Kiến Trúc Hệ Thống

```
AudioManager (Singleton)
    ├── Danh sách âm thanh tấn công Player
    ├── Danh sách âm thanh chuyển động Player
    ├── Danh sách âm thanh sát thương Player
    ├── Danh sách âm thanh chết Player
    ├── Danh sách âm thanh tấn công Skeleton
    ├── Danh sách âm thanh chuyển động Skeleton
    ├── Danh sách âm thanh sát thương Skeleton
    ├── Danh sách âm thanh chết Skeleton
    ├── (Tương tự cho Fly và Tank)
    └── Các Audio Sources

PlayerAudioSystem
    ├── Kích hoạt âm thanh tấn công từ sự kiện
    ├── Phát hiện chuyển động và phát âm thanh
    ├── Xử lý sát thương từ PlayerHealth
    └── Xử lý chết từ PlayerHealth

EnemyAudioSystem
    ├── Kích hoạt âm thanh dựa trên loại kẻ địch
    ├── Phát hiện chuyển động định kỳ
    ├── Xử lý sát thương từ Enemy
    └── Xử lý chết từ Enemy
```

### Luồng Dữ Liệu Âm Thanh

```
[Sự kiện Trò chơi] → [Audio System] → [AudioManager] → [Audio Source] → [Loa]

Ví dụ:
Player tấn công → PlayerAudioSystem.OnAttackStarted() → AudioManager.PlayRandomSound()
                 → Audio Source phát clip tấn công → Âm thanh phát ra
```

---

## Các Component

### 1. AudioManager.cs

**Mục Đích:** Quản lý tập trung tất cả âm thanh trong trò chơi.

**Loại:** Singleton tĩnh

**Giao Diện Công Khai:**

```csharp
// Phát một âm thanh ngẫu nhiên từ danh sách trên Audio Source
public void PlayRandomSoundOnSource(AudioSource source, List<SoundEffect> soundList)

// Phát âm thanh cụ thể
public void PlaySpecificSound(AudioSource source, SoundEffect sound)

// Dừng âm thanh
public void StopSound(AudioSource source)
```

**Tính Chất Cấu Hình:**

```csharp
public List<SoundEffect> playerAttackSounds;      // Âm thanh tấn công Player
public List<SoundEffect> playerMovementSounds;    // Âm thanh chuyển động Player
public List<SoundEffect> playerDamageSounds;      // Âm thanh sát thương Player
public List<SoundEffect> playerDeathSounds;       // Âm thanh chết Player

public List<SoundEffect> skeletonAttackSounds;    // Âm thanh tấn công Skeleton
public List<SoundEffect> skeletonMovementSounds;  // v.v.
// ... tương tự cho Fly và Tank
```

**Cách Dùng:**

```csharp
// Trong PlayerAudioSystem hoặc EnemyAudioSystem
AudioManager.Instance.PlayRandomSoundOnSource(attackSource, AudioManager.Instance.playerAttackSounds);
```

### 2. PlayerAudioSystem.cs

**Mục Đích:** Quản lý tất cả âm thanh của Player, bao gồm tấn công, chuyển động, sát thương và chết.

**Tính Chất:**

```csharp
public AudioSource attackSource;        // Audio Source cho âm thanh tấn công
public AudioSource movementSource;      // Audio Source cho âm thanh chuyển động
public AudioSource damageSource;        // Audio Source cho âm thanh sát thương
public AudioSource deathSource;         // Audio Source cho âm thanh chết

public bool useRandomPitch = true;      // Thêm biến thể pitch ngẫu nhiên
public float pitchVariation = 0.1f;     // Mức độ biến thể pitch
```

**Các Sự Kiện:**

- **OnAttackStarted()**: Được gọi từ `AttackComboController.OnAttackStart`
- **PlayMovementSound()**: Được gọi định kỳ trong `Update()` khi nhân vật di chuyển
- **PlayDamageSound()**: Được gọi từ `PlayerHealth.TakeDamage()`
- **PlayDeathSound()**: Được gọi từ `PlayerHealth.Die()`

**Cách Dùng:**

```csharp
// Từ AttackComboController
OnAttackStart?.Invoke(); // Kích hoạt sự kiện
// → PlayerAudioSystem.OnAttackStarted() tự động được gọi

// Từ PlayerHealth
audioSystem.PlayDamageSound();
audioSystem.PlayDeathSound();
```

### 3. EnemyAudioSystem.cs

**Mục Đích:** Quản lý âm thanh cho kẻ địch, hỗ trợ 3 loại: Skeleton, Fly, Tank.

**Tính Chất:**

```csharp
public enum EnemyType
{
    Skeleton,
    Fly,
    Tank
}

public EnemyType enemyType = EnemyType.Skeleton;

public AudioSource attackSource;        // Audio Source cho âm thanh tấn công
public AudioSource movementSource;      // Audio Source cho âm thanh chuyển động
public AudioSource damageSource;        // Audio Source cho âm thanh sát thương
public AudioSource deathSource;         // Audio Source cho âm thanh chết

public float movementSoundInterval = 0.5f;  // Thời gian giữa các âm thanh chuyển động
```

**Các Phương Thức Chính:**

```csharp
// Lấy danh sách âm thanh cho loại kẻ địch
private List<SoundEffect> GetAttackSoundList()
private List<SoundEffect> GetMovementSoundList()
private List<SoundEffect> GetDamageSoundList()
private List<SoundEffect> GetDeathSoundList()

// Phát âm thanh
public void PlayAttackSound()
public void PlayDamageSound()
public void PlayDeathSound()
private void PlayMovementSound()
```

**Cách Dùng:**

```csharp
// Trong Enemy.StartAttack()
audioSystem.PlayAttackSound();

// Trong Enemy.TakeDamage()
audioSystem.PlayDamageSound();

// Trong Enemy.Die()
audioSystem.PlayDeathSound();
```

---

## Cài Đặt Chi Tiết

### Bước 1: Tạo AudioManager

1. Tạo GameObject trống: `Hierarchy` → `Create Empty`
2. Đặt tên là `AudioManager`
3. Add Component → `AudioManager.cs`

### Bước 2: Cấu Hình Danh Sách Âm Thanh

Trong Inspector cho AudioManager component:

#### Cấu Hình Âm Thanh Player

**Player Attack Sounds:**
```
Size: 2 (hoặc nhiều hơn)

Element 0:
- Clip: [Chọn clip tấn công 1]
- Volume: 0.8
- Pitch: 1.0
- Loop: OFF
- Random Pitch Min: 0.95
- Random Pitch Max: 1.05

Element 1:
- Clip: [Chọn clip tấn công 2]
- Volume: 0.8
- Pitch: 1.0
- Loop: OFF
- Random Pitch Min: 0.95
- Random Pitch Max: 1.05
```

**Player Movement Sounds:**
```
Size: 2

Element 0:
- Clip: [Clip bước chân 1 - 0.3-0.5 giây]
- Volume: 0.4
- Pitch: 1.0
- Loop: OFF

Element 1:
- Clip: [Clip bước chân 2 - 0.3-0.5 giây]
- Volume: 0.4
- Pitch: 1.0
- Loop: OFF
```

**Player Damage Sounds:**
```
Size: 1-2

Element 0:
- Clip: [Clip đau/tác động]
- Volume: 0.7
- Pitch: 1.0
- Loop: OFF
```

**Player Death Sounds:**
```
Size: 1

Element 0:
- Clip: [Clip chết - có thể dài 1-3 giây]
- Volume: 1.0
- Pitch: 1.0
- Loop: OFF
```

#### Cấu Hình Âm Thanh Kẻ Địch

Lặp lại quy trình tương tự cho:
- `Skeleton Attack Sounds`, `Skeleton Movement Sounds`, `Skeleton Damage Sounds`, `Skeleton Death Sounds`
- `Fly Attack Sounds`, `Fly Movement Sounds`, `Fly Damage Sounds`, `Fly Death Sounds`
- `Tank Attack Sounds`, `Tank Movement Sounds`, `Tank Damage Sounds`, `Tank Death Sounds`

**Mẹo:** Skeleton có thể có âm thanh xương lạch cạch, Fly có thể có tiếng kêu và cánh vỗ, Tank có thể có tiếng bước nặng và tiếng kim loại.

### Bước 3: Thêm Components Vào Player

1. Chọn GameObject Player trong Hierarchy
2. Add Component → `PlayerAudioSystem.cs`
3. Cấu Hình (tùy chọn):
   - `Use Random Pitch`: ON (để có sự đa dạng)
   - `Pitch Variation`: 0.1 (biến đổi pitch nhẹ nhàng)

**Lưu Ý:** AudioSources sẽ được tạo tự động.

### Bước 4: Thêm Components Vào Kẻ Địch

Cho **mỗi GameObject kẻ địch:**

1. Chọn GameObject Skeleton/Fly/Tank
2. Add Component → `EnemyAudioSystem.cs`
3. Cấu Hình:
   - `Enemy Type`: Chọn loại thích hợp (Skeleton/Fly/Tank)
   - `Movement Sound Interval`: 0.5 (mặc định tốt)
   - `Use Random Pitch`: ON

---

## Tích Hợp Script

### Sự Kiện Tấn Công Player

**Vị Trí Kích Hoạt:** `AttackComboController.cs`, phương thức `PerformAttack()`

```csharp
// Trong AttackComboController.cs - Dòng ~250
private void PerformAttack()
{
    // ... logic tấn công khác ...
    
    // Kích hoạt sự kiện
    OnAttackStart?.Invoke();
    
    // PlayerAudioSystem lắng nghe sự kiện này tự động
}
```

**Cách PlayerAudioSystem Xử Lý:**

```csharp
// Trong PlayerAudioSystem.cs - Phương thức OnAttackStarted()
private void OnAttackStarted()
{
    Debug.Log("🔊 Player Attack Sound");
    AudioManager.Instance.PlayRandomSoundOnSource(attackSource, AudioManager.Instance.playerAttackSounds);
}
```

### Sự Kiện Sát Thương Player

**Vị Trí Kích Hoạt:** `PlayerHealth.cs`, phương thức `TakeDamage()`

```csharp
// Trong PlayerHealth.cs - Dòng ~115
public void TakeDamage(float damage)
{
    // ... logic sát thương ...
    
    // Gọi âm thanh sát thương
    if (audioSystem != null)
        audioSystem.PlayDamageSound();
}
```

### Sự Kiện Chết Player

**Vị Trí Kích Hoạt:** `PlayerHealth.cs`, phương thức `Die()`

```csharp
// Trong PlayerHealth.cs - Dòng ~245
private void Die()
{
    // ... logic chết ...
    
    // Gọi âm thanh chết
    if (audioSystem != null)
        audioSystem.PlayDeathSound();
}
```

### Sự Kiện Tấn Công Kẻ Địch

**Vị Trí Kích Hoạt:** `Enemy.cs`, phương thức `StartAttack()`

```csharp
// Trong Enemy.cs - Dòng ~300
protected virtual void StartAttack()
{
    // ... logic tấn công ...
    
    // Gọi âm thanh tấn công
    if (audioSystem != null)
        audioSystem.PlayAttackSound();
}
```

### Sự Kiện Sát Thương Kẻ Địch

**Vị Trí Kích Hoạt:** `Enemy.cs`, phương thức `TakeDamage()`

```csharp
// Trong Enemy.cs - Dòng ~460
public virtual void TakeDamage(float damage)
{
    // ... logic sát thương ...
    
    // Gọi âm thanh sát thương
    if (audioSystem != null)
        audioSystem.PlayDamageSound();
}
```

### Sự Kiện Chết Kẻ Địch

**Vị Trí Kích Hoạt:** `Enemy.cs`, phương thức `Die()`

```csharp
// Trong Enemy.cs - Dòng ~590
protected virtual void Die()
{
    // ... logic chết ...
    
    // Gọi âm thanh chết
    if (audioSystem != null)
        audioSystem.PlayDeathSound();
}
```

---

## Quản Lý Âm Thanh

### Cấu Trúc SoundEffect

```csharp
[System.Serializable]
public class SoundEffect
{
    public string name;                    // Tên để nhận dạng (không bắt buộc)
    public AudioClip clip;                 // Clip âm thanh
    [Range(0f, 1f)] public float volume = 1f;    // Âm lượng (0-1)
    [Range(0.5f, 2f)] public float pitch = 1f;   // Pitch (0.5-2.0)
    public bool loop = false;              // Loop hay không
    [Range(0.5f, 1.5f)] public float randomPitchMin = 0.95f;  // Min pitch ngẫu nhiên
    [Range(0.5f, 1.5f)] public float randomPitchMax = 1.05f;  // Max pitch ngẫu nhiên
}
```

### Âm Lượng & Pitch

**Khuyến Nghị Âm Lượng:**
- Tấn công: 0.7 - 1.0
- Chuyển động: 0.3 - 0.6
- Sát thương: 0.5 - 0.8
- Chết: 0.8 - 1.0

**Khuyến Nghị Pitch:**
- Mặc định: 1.0
- Biến thể: ±5% (0.95 - 1.05) để có sự đa dạng
- Cho âm thanh chết: 0.8 - 1.0 để tạo cảm giác "khớp"

### Khoảng Cách Phát Âm Thanh Chuyển Động

**Cài Đặt `movementSoundInterval`:**
- Nhanh/Chạy: 0.3 - 0.4 giây
- Đi bộ bình thường: 0.5 - 0.7 giây
- Thảo lược: 1.0 giây

**Cách Tính:**
```
Interval = 60 / (BPM Bước Chân Mong Muốn)
```

---

## Gỡ Lỗi

### Bật Debug Logging

Tất cả các system đều ghi log với emoji 🔊:

```csharp
Debug.Log("🔊 Player Attack Sound");
Debug.Log("🔊 Player Movement Sound");
Debug.Log("🔊 Enemy Skeleton Audio System initialized");
```

### Kiểm Tra Console

Mở Console Window: `Window` → `General` → `Console`

**Thông Báo Bình Thường:**
```
🔊 AudioManager Instance Created
🔊 Player Attack Sound
🔊 Player Movement Sound (khi di chuyển)
🔊 Player Damage Sound (khi bị tấn công)
🔊 Player Death Sound (khi chết)
🔊 Skeleton Audio System initialized (cho mỗi Skeleton)
```

**Thông Báo Lỗi:**
```
NullReferenceException: audioSystem is null
→ Kiểm tra xem EnemyAudioSystem component được thêm chưa

MissingComponentException: AudioManager component not found
→ Kiểm tra xem AudioManager GameObject có tồn tại trong scene không
```

### Kiểm Tra Audio Sources

**Trong Game View:**
1. Phát trò chơi
2. Chọn Player GameObject
3. Kiểm tra Inspector, xem các Audio Source components có tồn tại không
4. Kiểm tra "Playing" trạng thái của Audio Source

### Vấn Đề Không Có Âm Thanh

**Kiểm Tra Danh Sách:**

```csharp
// Thêm code này tạm thời vào Awake() của AudioManager
void Awake()
{
    // ...
    
    // Debug logs
    Debug.Log($"Player Attack Sounds: {playerAttackSounds.Count}");
    Debug.Log($"Player Movement Sounds: {playerMovementSounds.Count}");
    // ... kiểm tra tất cả danh sách
}
```

**Kiểm Tra Volume:**
- Kiểm tra thanh trượt Master Volume trong Game View (góc trên trái)
- Kiểm tra volume trong Preferences → Audio
- Kiểm tra cài đặt âm thanh của hệ thống

### Vấn Đề Âm Thanh Quá To/Yếu

1. Kiểm tra volume của từng SoundEffect trong AudioManager
2. Kiểm tra xem clip âm thanh tự nó có quá to/yếu không
3. Điều chỉnh `randomPitchMin` và `randomPitchMax` để kiểm soát biến thể

---

## Ví Dụ Mã

### Ví Dụ 1: Phát Âm Thanh Tấn Công Tùy Chỉnh

```csharp
// Nếu bạn muốn phát âm thanh riêng lẻ thay vì ngẫu nhiên:
public void PlayAttackSoundByIndex(int index)
{
    if (index >= 0 && index < AudioManager.Instance.playerAttackSounds.Count)
    {
        var sound = AudioManager.Instance.playerAttackSounds[index];
        AudioManager.Instance.PlaySpecificSound(attackSource, sound);
    }
}
```

### Ví Dụ 2: Điều Chỉnh Volume Theo Thời Gian

```csharp
// Fade out âm thanh chết từ từ
public void PlayDeathSoundWithFade()
{
    audioSystem.PlayDeathSound();
    
    // Fade out volume sau 2 giây
    StartCoroutine(FadeOutAudio(audioSystem.deathSource, 2f));
}

IEnumerator FadeOutAudio(AudioSource source, float duration)
{
    float elapsedTime = 0f;
    float startVolume = source.volume;
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
        yield return null;
    }
    
    source.Stop();
    source.volume = startVolume;
}
```

### Ví Dụ 3: Phát Âm Thanh Dựa Trên Tốc Độ

```csharp
// Trong PlayerAudioSystem, có thể tùy chỉnh:
void Update()
{
    // ... logic chuyển động hiện tại ...
    
    // Có thể thêm: thay đổi pitch dựa trên tốc độ
    float speedFactor = characterController.velocity.magnitude / maxSpeed;
    movementSource.pitch = 0.8f + (speedFactor * 0.4f); // Pitch từ 0.8 đến 1.2
}
```

### Ví Dụ 4: Phát Âm Thanh Vị Trí 3D

```csharp
// Nếu bạn muốn âm thanh kẻ địch phát ra từ vị trí của nó:
public void Setup3DAudio(Vector3 position)
{
    attackSource.spatialBlend = 1f; // 1 = 3D, 0 = 2D
    attackSource.transform.position = position;
    attackSource.minDistance = 5f;
    attackSource.maxDistance = 50f;
}
```

---

## Tóm Tắt Nhanh

| Thành Phần | Mục Đích | Cài Đặt Chính |
|-----------|---------|------------|
| **AudioManager** | Quản lý tập trung tất cả âm thanh | Cấu hình danh sách âm thanh |
| **PlayerAudioSystem** | Âm thanh Player | Gắn vào Player GameObject |
| **EnemyAudioSystem** | Âm thanh Kẻ Địch | Gắn vào mỗi kẻ địch, chọn loại |
| **Audio Sources** | Phát âm thanh | Được tạo tự động |

**Quy Trình Cài Đặt Tóm Tắt:**
1. Tạo AudioManager trong scene
2. Cấu hình danh sách âm thanh
3. Thêm PlayerAudioSystem vào Player
4. Thêm EnemyAudioSystem vào kẻ địch
5. Kiểm tra trong Play mode

**Không cần thay đổi bất kỳ mã trò chơi khác!**
