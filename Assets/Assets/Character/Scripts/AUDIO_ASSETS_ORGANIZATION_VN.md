# Tổ Chức Tài Nguyên Âm Thanh

## Cấu Trúc Thư Mục Được Khuyến Nghị

```
Assets/
├── Audio/
│   ├── Music/
│   │   ├── MainTheme.mp3
│   │   ├── BattleTheme.mp3
│   │   └── MenuTheme.mp3
│   │
│   ├── SFX/ (Sound Effects)
│   │   ├── Player/
│   │   │   ├── Attacks/
│   │   │   │   ├── slash1.wav
│   │   │   │   ├── slash2.wav
│   │   │   │   └── punch1.wav
│   │   │   ├── Movement/
│   │   │   │   ├── footstep1.wav
│   │   │   │   ├── footstep2.wav
│   │   │   │   └── run.wav
│   │   │   ├── Damage/
│   │   │   │   ├── ouch1.wav
│   │   │   │   └── impact.wav
│   │   │   └── Death/
│   │   │       └── death.wav
│   │   │
│   │   ├── Enemies/
│   │   │   ├── Skeleton/
│   │   │   │   ├── Attacks/
│   │   │   │   │   ├── bone_throw1.wav
│   │   │   │   │   └── bone_throw2.wav
│   │   │   │   ├── Movement/
│   │   │   │   │   ├── bone_rattle1.wav
│   │   │   │   │   └── bone_rattle2.wav
│   │   │   │   ├── Damage/
│   │   │   │   │   └── crack.wav
│   │   │   │   └── Death/
│   │   │   │       └── skeleton_death.wav
│   │   │   │
│   │   │   ├── Fly/
│   │   │   │   ├── Attacks/
│   │   │   │   │   ├── sting1.wav
│   │   │   │   │   └── sting2.wav
│   │   │   │   ├── Movement/
│   │   │   │   │   ├── buzz1.wav
│   │   │   │   │   └── wings_flap.wav
│   │   │   │   ├── Damage/
│   │   │   │   │   └── hurt.wav
│   │   │   │   └── Death/
│   │   │   │       └── fly_death.wav
│   │   │   │
│   │   │   └── Tank/
│   │   │       ├── Attacks/
│   │   │       │   ├── bash1.wav
│   │   │       │   └── slam.wav
│   │   │       ├── Movement/
│   │   │       │   ├── heavy_step1.wav
│   │   │       │   └── metal_clang.wav
│   │   │       ├── Damage/
│   │   │       │   └── metal_dent.wav
│   │   │       └── Death/
│   │   │           └── tank_death.wav
│   │   │
│   │   ├── UI/
│   │   │   ├── button_click.wav
│   │   │   ├── select.wav
│   │   │   └── error.wav
│   │   │
│   │   └── Environment/
│   │       ├── ambient_forest.wav
│   │       └── wind.wav
│   │
│   └── Scripts/ (Audio System Scripts)
│       ├── AudioManager.cs
│       ├── PlayerAudioSystem.cs
│       └── EnemyAudioSystem.cs
```

## Quy Ước Đặt Tên Tệp

### Quy Ước Chung

```
[Loại]_[Nguồn]_[Hành Động]_[Biến Thể].wav

Ví dụ:
- SFX_Player_Attack_Slash1.wav
- SFX_Skeleton_Movement_BoneRattle1.wav
- SFX_Fly_Death_Screech.wav
```

### Chi Tiết Từng Loại

**Âm Thanh Tấn Công:**
```
Player_Attack_Slash1.wav
Player_Attack_Slash2.wav
Player_Attack_Punch1.wav
Skeleton_Attack_Throw1.wav
Fly_Attack_Sting1.wav
Tank_Attack_Bash1.wav
```

**Âm Thanh Chuyển Động:**
```
Player_Movement_Footstep1.wav
Player_Movement_Footstep2.wav
Player_Movement_Run.wav
Skeleton_Movement_Rattle1.wav
Fly_Movement_Buzz1.wav
Fly_Movement_Wings.wav
Tank_Movement_Step1.wav
Tank_Movement_MetalClang.wav
```

**Âm Thanh Sát Thương:**
```
Player_Damage_Ouch1.wav
Player_Damage_Impact.wav
Skeleton_Damage_Crack.wav
Fly_Damage_Hurt.wav
Tank_Damage_Dent.wav
```

**Âm Thanh Chết:**
```
Player_Death.wav
Skeleton_Death.wav
Fly_Death.wav
Tank_Death.wav
```

## Định Dạng Tệp Được Khuyến Nghị

### Chất Lượng Âm Thanh

| Loại | Định Dạng | Mẫu | Bit Depth | Ghi Chú |
|------|-----------|-----|-----------|---------|
| **Âm Thanh Tấn Công** | WAV/MP3 | 44.1 kHz | 16-bit | Chất lượng cao, độc lập |
| **Âm Thanh Chuyển Động** | WAV/MP3 | 44.1 kHz | 16-bit | Có thể lặp lại ngắn |
| **Âm Thanh Sát Thương** | WAV/MP3 | 44.1 kHz | 16-bit | Độc lập, ngắn gọn |
| **Âm Thanh Chết** | WAV | 44.1 kHz | 16-bit | Có thể dài 1-3 giây |

### Cài Đặt Nhập Khẩu Unity

Cho mỗi clip âm thanh, cấu hình cài đặt:

1. Chọn clip âm thanh trong Project
2. Kiểm tra Inspector:

```
Audio Importer Settings:
- Load Type: Decompress On Load (cho SFX ngắn)
                 hoặc Streaming (cho âm nhạc dài)
- Compression Format: Vorbis (cho in-game)
                      AAC (cho mobile)
- Sample Rate Setting: Optimize
- Preload Audio Data: ON (cho SFX ngắn)
```

## Quản Lý Kho Âm Thanh

### Bộ Âm Thanh Tối Thiểu

**Thiết Lập Cơ Bản (16 clips):**

```
Player:
✓ 1x Attack (slash)
✓ 1x Movement (footstep)
✓ 1x Damage (ouch)
✓ 1x Death

Skeleton:
✓ 1x Attack
✓ 1x Movement
✓ 1x Damage
✓ 1x Death

Fly:
✓ 1x Attack
✓ 1x Movement
✓ 1x Damage
✓ 1x Death

Tank:
✓ 1x Attack
✓ 1x Movement
✓ 1x Damage
✓ 1x Death
```

### Bộ Âm Thanh Tối Ưu

**Thiết Lập Tốt Hơn (40-50 clips):**

```
Player (12-15 clips):
✓ 3x Attack (slash, punch, etc.)
✓ 3x Movement (footstep variations)
✓ 2x Damage
✓ 1x Death

Skeleton (10-12 clips):
✓ 2x Attack
✓ 3x Movement (bone rattle, motion, etc.)
✓ 1x Damage
✓ 1x Death

Fly (10-12 clips):
✓ 2x Attack
✓ 3x Movement (buzz, wings, etc.)
✓ 1x Damage
✓ 1x Death

Tank (8-10 clips):
✓ 2x Attack
✓ 2x Movement (heavy, metal, etc.)
✓ 1x Damage
✓ 1x Death

UI (3-5 clips):
✓ Click
✓ Select
✓ Error
```

## Cài Đặt AudioManager Trong Inspector

### Ví Dụ Cấu Hình Lengkap

#### Player Attack Sounds

```
Size: 2

Element 0:
  Clip: Player_Attack_Slash1.wav
  Volume: 0.8
  Pitch: 1.0
  Loop: OFF
  Random Pitch Min: 0.95
  Random Pitch Max: 1.05

Element 1:
  Clip: Player_Attack_Slash2.wav
  Volume: 0.8
  Pitch: 1.0
  Loop: OFF
  Random Pitch Min: 0.95
  Random Pitch Max: 1.05
```

#### Skeleton Movement Sounds

```
Size: 2

Element 0:
  Clip: Skeleton_Movement_BoneRattle1.wav
  Volume: 0.5
  Pitch: 1.0
  Loop: OFF
  Random Pitch Min: 0.98
  Random Pitch Max: 1.02

Element 1:
  Clip: Skeleton_Movement_BoneRattle2.wav
  Volume: 0.5
  Pitch: 1.0
  Loop: OFF
  Random Pitch Min: 0.98
  Random Pitch Max: 1.02
```

#### Fly Attack Sounds

```
Size: 2

Element 0:
  Clip: Fly_Attack_Sting1.wav
  Volume: 0.7
  Pitch: 1.1
  Loop: OFF
  Random Pitch Min: 1.0
  Random Pitch Max: 1.2

Element 1:
  Clip: Fly_Attack_Sting2.wav
  Volume: 0.7
  Pitch: 1.1
  Loop: OFF
  Random Pitch Min: 1.0
  Random Pitch Max: 1.2
```

## Khắc Phục Sự Cố Tài Nguyên

### Tệp Clip Không Được Nhìn Thấy

**Vấn đề:** Clip không xuất hiện trong AudioManager Inspector

**Giải pháp:**
1. Kiểm tra xem tệp WAV/MP3 có trong thư mục Assets không
2. Đợi Unity tải lại (F5)
3. Nhấp chuột phải trên tệp → Reimport
4. Kiểm tra xem tệp có phần mở rộng đúng (.wav, .mp3)

### Âm Thanh Không Phát

**Vấn đề:** Clip được chọn nhưng không phát âm thanh

**Giải pháp:**
1. Kiểm tra xem AudioSource có được tạo không (trong Player/Enemy GameObject)
2. Kiểm tra xem clip có trống không
3. Kiểm tra thể tích hệ thống
4. Đảm bảo AudioListener được gắn vào Main Camera

### Âm Thanh Méo Tiếng

**Vấn đề:** Âm thanh phát ra có vẻ bị nén hoặc méo

**Giải pháp:**
1. Kiểm tra volume của SoundEffect (< 1.0)
2. Kiểm tra tệp clip gốc có tốt không (mở trong trình phát âm thanh)
3. Giảm volume trong cài đặt nhập khẩu
4. Sử dụng định dạng WAV thay vì MP3 nếu cần chất lượng cao

## Tối Ưu Hóa Hiệu Suất

### Kích Thước Tệp

**Giảm Kích Thước:**

| Phương Pháp | Tiết Kiệm | Mất Mát |
|-----------|-----------|--------|
| MP3 @ 128 kbps | 75% | Nhẹ |
| Vorbis @ 80 kbps | 70% | Nhẹ |
| Giảm 44.1 kHz → 22 kHz | 50% | Trung Bình |
| WAV (không nén) | 0% | Không |

**Khuyến Nghị:**
- Âm thanh SFX: MP3 @ 128 kbps hoặc Vorbis
- Âm nhạc: MP3 @ 192 kbps hoặc Vorbis
- Âm thanh quan trọng: WAV hoặc FLAC

### Bộ Nhớ

**Ước Tính Bộ Nhớ:**
- 1 phút âm thanh 16-bit 44.1 kHz Stereo ≈ 10 MB
- 30 giây âm thanh mono ≈ 1.3 MB
- 5 giây âm thanh tấn công ≈ 0.4 MB

**Khuyến Nghị Hệ Thống:**
- Trò chơi Desktop: 50-100 MB cho âm thanh
- Trò chơi Mobile: 20-30 MB cho âm thanh
- Bộ Cơ Bản: 5-10 MB

## Danh Sách Kiểm Tra Tài Nguyên

- [ ] Thư mục Audio được tạo với cấu trúc đúng
- [ ] Tất cả tệp clip được đặt tên theo quy ước
- [ ] Tất cả clip được nhập vào Unity
- [ ] AudioManager có tất cả danh sách được cấu hình
- [ ] Mỗi danh sách có ít nhất 1 clip
- [ ] Mỗi clip có volume và pitch đúng
- [ ] Cài đặt nhập khẩu tối ưu hóa cho hiệu suất
- [ ] Tổng kích thước tệp phù hợp với bộ nhớ
- [ ] Tất cả clip được thử nghiệm trong Play mode

## Tài Nguyên Bổ Sung

### Nguồn Âm Thanh Miễn Phí

- Freesound.org - Âm thanh do cộng đồng tải lên
- Pixabay - Âm thanh miễn phí không yêu cầu quy dụng
- OpenGameArt.org - Tài nguyên trò chơi mở
- Zapsplat - Effectz âm thanh chất lượng cao
- BBC Sound Effects Library - Thư viện bộ sưu tập BBC

### Công Cụ Chỉnh Sửa Âm Thanh

- Audacity (Miễn phí) - Chỉnh sửa âm thanh cơ bản
- GarageBand (Mac) - Tạo âm thanh
- Adobe Audition (Trả phí) - Chỉnh sửa chuyên nghiệp
- FL Studio (Trả phí) - Tạo âm thanh chất lượng cao

## Tóm Tắt

1. **Tổ Chức** thư mục theo loại (Player, Enemies, UI)
2. **Đặt Tên** theo quy ước (Player_Attack_Slash1.wav)
3. **Nhập** với cài đặt tối ưu hóa
4. **Cấu Hình** trong AudioManager Inspector
5. **Kiểm Tra** trong Play mode
6. **Tối Ưu** kích thước tệp khi cần
