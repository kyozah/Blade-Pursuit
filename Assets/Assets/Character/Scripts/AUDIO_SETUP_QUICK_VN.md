# Hướng Dẫn Thiết Lập Nhanh Hệ Thống Âm Thanh

## Thiết Lập Từng Bước

### Bước 1: Tạo AudioManager Trong Scene

1. Tạo GameObject mới **Trống** (chuột phải trong Hierarchy → Create Empty)
2. Đặt tên là `AudioManager`
3. Thêm component: `AudioManager.cs`

### Bước 2: Cấu Hình AudioManager

Trong Inspector cho component AudioManager:

#### Âm Thanh Tấn Công Player
1. Đặt size thành 1 hoặc nhiều hơn
2. Cho mỗi âm thanh tấn công:
   - Name: `attack1`, `attack2`, `slash1`, v.v.
   - Clip: Chọn clip âm thanh tấn công của bạn
   - Volume: 0.8
   - Pitch: 1.0

#### Âm Thanh Chuyển Động Player
1. Đặt size thành 1 hoặc nhiều hơn
2. Cho mỗi âm thanh chuyển động:
   - Name: `footstep1`, `footstep2`, `run`, v.v.
   - Clip: Chọn clip bước chân của bạn
   - Volume: 0.5
   - Pitch: 1.0

#### Âm Thanh Sát Thương Player
1. Đặt size thành 1 hoặc nhiều hơn
2. Cho mỗi âm thanh sát thương:
   - Name: `hurt1`, `ouch`, `impact`, v.v.
   - Clip: Chọn clip đau của bạn
   - Volume: 0.7
   - Pitch: 1.0

#### Âm Thanh Chết Player
1. Đặt size thành 1 hoặc nhiều hơn
2. Cho mỗi âm thanh chết:
   - Name: `death1`, `death2`, v.v.
   - Clip: Chọn clip chết của bạn
   - Volume: 1.0
   - Pitch: 1.0

### Bước 3: Cấu Hình Âm Thanh Kẻ Địch

Lặp lại quy trình tương tự cho:
- **Skeleton:** skeletonAttackSounds, skeletonMovementSounds, skeletonDamageSounds, skeletonDeathSounds
- **Fly:** flyAttackSounds, flyMovementSounds, flyDamageSounds, flyDeathSounds
- **Tank:** tankAttackSounds, tankMovementSounds, tankDamageSounds, tankDeathSounds

### Bước 4: Thêm Components Vào Player

1. Chọn GameObject Player của bạn
2. Add Component: `PlayerAudioSystem.cs`
3. Cấu Hình (tùy chọn):
   - Bật/tắt `useRandomPitch` để có sự đa dạng
   - Điều chỉnh `pitchVariation` (0.1 là mặc định)

### Bước 5: Thêm Components Vào Kẻ Địch

Cho mỗi loại kẻ địch (Skeleton, Fly, Tank):

1. Chọn GameObject của kẻ địch
2. Add Component: `EnemyAudioSystem.cs`
3. Đặt `Enemy Type` thành giá trị phù hợp (Skeleton/Fly/Tank)
4. Cấu Hình (tùy chọn):
   - Bật/tắt `useRandomPitch` để có sự đa dạng
   - Điều chỉnh `pitchVariation`
   - Điều chỉnh `movementSoundInterval`

### Bước 6: Kiểm Tra

1. Nhấn **Play** trong Unity
2. Lắng nghe:
   - Âm thanh tấn công khi đánh kẻ địch
   - Âm thanh bước chân khi di chuyển
   - Âm thanh sát thương khi bị tấn công
   - Âm thanh chết khi chết
   - Âm thanh kẻ địch khi chúng tấn công/di chuyển/chết

## Clip Âm Thanh Tối Thiểu Cần Có

**Thiết Lập Tối Thiểu (16 clip tổng cộng):**
- 1 âm thanh tấn công player
- 1 âm thanh bước chân player
- 1 âm thanh sát thương player
- 1 âm thanh chết player
- Tương tự cho Skeleton, Fly, Tank (mỗi loại 4 clip)

**Thiết Lập Tốt Hơn (40-50 clips):**
- 2-3 biến thể cho âm thanh tấn công
- 2-3 biến thể cho âm thanh bước chân
- 2 biến thể cho âm thanh sát thương
- 1 âm thanh chết
- Tương tự cho mỗi loại kẻ địch

## Cài Đặt Âm Thanh Theo Loại

### Âm Thanh Tấn Công
- Âm lượng: 0.7-1.0
- Pitch: 0.95-1.05 (biến thể nhẹ)
- Loop: OFF

### Âm Thanh Chuyển Động
- Âm lượng: 0.3-0.6
- Pitch: 0.95-1.05
- Loop: OFF
- Thời lượng: 0.3-0.8 giây

### Âm Thanh Sát Thương
- Âm lượng: 0.5-0.8
- Pitch: 0.9-1.1
- Loop: OFF

### Âm Thanh Chết
- Âm lượng: 0.8-1.0
- Pitch: 0.8-1.0
- Loop: OFF
- Thời lượng: 1-3 giây

## Tích Hợp Script - Tóm Tắt

### Trigger Âm Thanh Tự Động

**Player:**
- ✅ Tấn Công: Được kích hoạt trong sự kiện `AttackComboController.OnAttackStart`
- ✅ Chuyển Động: Được phát hiện bởi kiểm tra chuyển động trong `PlayerAudioSystem.Update()`
- ✅ Sát Thương: Được gọi trong `PlayerHealth.TakeDamage()`
- ✅ Chết: Được gọi trong `PlayerHealth.Die()`

**Kẻ Địch (Skeleton, Fly, Tank):**
- ✅ Tấn Công: Được kích hoạt trong `Enemy.StartAttack()`
- ✅ Chuyển Động: Được phát hiện định kỳ trong `EnemyAudioSystem.Update()`
- ✅ Sát Thương: Được gọi trong `Enemy.TakeDamage()`
- ✅ Chết: Được gọi trong `Enemy.Die()`

**Không cần thay đổi mã bổ sung nào!**

## Gỡ Lỗi

### Bật Debug Logs
1. Mở AudioManager trong inspector
2. Tất cả các hệ thống sẽ ghi log bằng emoji 🔊 khi âm thanh phát

### Kiểm Tra Console Để Biết Thông Báo
- "🔊 Player Attack Sound" - Tấn công được kích hoạt
- "🔊 Player Movement Sound" - Chuyển động được phát hiện
- "🔊 Player Damage Sound" - Sát thương được nhận
- "🔊 Player Death Sound" - Player chết
- "🔊 Skeleton Audio System initialized" - Thiết lập kẻ địch hoàn thành

### Audio Sources Không Được Tạo
1. Kiểm tra AudioManager có tồn tại trong scene không
2. Xác minh script AudioManager được gắn
3. Kiểm tra console để tìm lỗi tham chiếu null

## Mẹo Hiệu Suất

1. **Sử Dụng Nén Thích Hợp:**
   - Cài đặt nhập cho clip âm thanh
   - Sử dụng nén Vorbis cho hầu hết âm thanh
   - AAC cho mobile

2. **Độ Dài Âm Thanh:**
   - Giữ âm thanh tấn công 0.5-1 giây
   - Giữ bước chân 0.3-0.5 giây
   - Cho phép âm thanh chết dài hơn

3. **Mức Âm Lượng:**
   - Cân bằng đúng âm lượng trong AudioManager
   - Tránh méo tiếng (volume > 1.0)
   - Kiểm tra bằng tai nghe và loa

## Sự Cố Thường Gặp & Giải Pháp

### Không Có Âm Thanh Phát
- [ ] Kiểm tra AudioManager có tồn tại trong scene không
- [ ] Xác minh AudioManager.cs được gắn
- [ ] Kiểm tra mức âm lượng (không đặt thành 0)
- [ ] Xác minh clip âm thanh được nhập chính xác
- [ ] Kiểm tra thanh trượt Volume trong cửa sổ Game

### Âm Thanh Quá To/Yếu
- [ ] Điều chỉnh volume trong AudioManager.SoundEffect
- [ ] Kiểm tra xem clip âm thanh tự nó có quá to/yếu không
- [ ] Xác minh mức âm thanh Audio Settings
- [ ] Điều chỉnh âm lượng hệ thống

### Chỉ Một Âm Thanh Phát
- [ ] Kiểm tra xem size của danh sách âm thanh có > 1 không
- [ ] Xác minh nhiều clip được gán
- [ ] Kiểm tra cài đặt useRandomPitch

### Âm Thanh 3D Không Hoạt Động
- [ ] Xác minh scene có AudioListener (thường ở trên Main Camera)
- [ ] Kiểm tra giá trị spatialBlend trong AudioManager
- [ ] Đặt kẻ địch đủ xa để nhận thấy hiệu ứng
- [ ] Kiểm tra Min/Max Distance trên audio sources

## Bước Tiếp Theo

1. ✅ Thêm AudioManager vào scene
2. ✅ Tạo/nhập clip âm thanh
3. ✅ Cấu hình danh sách âm thanh trong AudioManager
4. ✅ Thêm PlayerAudioSystem vào Player
5. ✅ Thêm EnemyAudioSystem vào mỗi kẻ địch
6. ✅ Kiểm tra trong Play mode
7. ✅ Điều chỉnh âm lượng và thời gian
8. ✅ Thêm nhiều biến thể âm thanh khi cần

## Hỗ Trợ

Để tìm hiểu chi tiết đầy đủ, xem: `START_HERE_AUDIO_SETUP_VN.md`
