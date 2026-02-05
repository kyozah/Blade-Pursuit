# Hướng Dẫn Triển Khai Hệ Thống Âm Thanh - BẮT ĐẦU TẠI ĐÂY

## 📦 Những Gì Bạn Nhận Được

Hệ thống âm thanh hoàn chỉnh, sẵn sàng phát hành cho Blade Pursuit thêm hiệu ứng âm thanh tuyệt vời:
- **Player:** Âm thanh tấn công, di chuyển, nhận sát thương, và chết
- **3 Loại Kẻ Địch:** Skeleton, Fly, Tank (mỗi loại có âm thanh riêng)

**Tất cả âm thanh phát song song với animation để phản hồi chân thực.**

---

## 🚀 Bắt Đầu Nhanh (5 phút)

### 1. Mở Scene Game Của Bạn

### 2. Tạo AudioManager
```
Nhấp chuột phải trong Hierarchy → Create Empty
Đặt tên: "AudioManager"
Add Component → AudioManager.cs
```

### 3. Kéo Audio Clips vào AudioManager
Trong Inspector, mở rộng từng danh sách âm thanh và thêm các clip âm thanh của bạn.

### 4. Thêm PlayerAudioSystem vào Player
Chọn Player → Add Component → PlayerAudioSystem.cs

### 5. Thêm EnemyAudioSystem vào Kẻ Địch
Với mỗi kẻ địch (Skeleton, Fly, Tank):
- Add Component → EnemyAudioSystem.cs
- Đặt Enemy Type trong inspector

### 6. Kiểm Tra
Nhấn Play và lắng nghe! Âm thanh sẽ phát khi:
- Player tấn công
- Player di chuyển
- Player nhận sát thương
- Player chết
- Kẻ địch tấn công, di chuyển, nhận sát thương, và chết

---

## 📁 Tham Khảo Tệp Tin

### Scripts Mới (Sẵn Sàng Dùng)
```
Assets/Assets/Character/Scripts/
├── AudioManager.cs              ← Quản lý âm thanh trung tâm
├── PlayerAudioSystem.cs         ← Xử lý âm thanh Player
└── EnemyAudioSystem.cs         ← Xử lý âm thanh Kẻ Địch
```

### Scripts Đã Sửa Đổi (Tích Hợp Sẵn)
```
Assets/Assets/Character/Scripts/
├── Enemy.cs                     ← Thêm trigger âm thanh
├── PlayerHealth.cs              ← Thêm trigger âm thanh
├── Skeleton.cs                  ← Sẵn sàng
├── Fly.cs                       ← Sẵn sàng
└── Tank.cs                      ← Sẵn sàng
```

### Tài Liệu (Tham Khảo)
```
Assets/Assets/Character/Scripts/
├── START_HERE_AUDIO_SETUP_VN.md      ← BẮT ĐẦU TẠI ĐÂY!
├── AUDIO_SYSTEM_GUIDE_VN.md          ← Hướng dẫn tham khảo
├── AUDIO_SETUP_QUICK_VN.md           ← Thiết lập nhanh
└── ... (các tệp khác)
```

---

## 🔧 Hướng Dẫn Thiết Lập (Từng Bước)

### BƯỚC 1: Thiết Lập Scene (1 phút)

1. Mở scene game của bạn trong Unity
2. Nhấp chuột phải trong panel Hierarchy
3. Chọn Create Empty
4. Đặt tên object mới: `AudioManager`
5. Với AudioManager được chọn, vào Inspector
6. Nhấp "Add Component"
7. Tìm `AudioManager` và thêm nó
8. Kiểm tra không có lỗi màu đỏ trong console

### BƯỚC 2: Cấu Hình Danh Sách Âm Thanh (5-10 phút)

Trong Inspector của AudioManager, cấu hình các danh sách này:

**ÂM THANH PLAYER:**
```
Player Attack Sounds (tối thiểu 1):
  - Clip: Tệp âm thanh tấn công của bạn
  - Volume: 0.8
  - Pitch: 1.0

Player Movement Sounds (tối thiểu 1):
  - Clip: Tệp âm thanh bước chân của bạn
  - Volume: 0.5
  - Pitch: 1.0

Player Damage Sounds (tối thiểu 1):
  - Clip: Tệp âm thanh đau/sát thương của bạn
  - Volume: 0.7
  - Pitch: 1.0

Player Death Sounds (tối thiểu 1):
  - Clip: Tệp âm thanh chết của bạn
  - Volume: 1.0
  - Pitch: 1.0
```

**ÂM THANH SKELETON** (cùng mô hình):
```
Skeleton Attack Sounds → Clip tấn công skeleton
Skeleton Movement Sounds → Clip chuyển động skeleton
Skeleton Damage Sounds → Clip đau của skeleton
Skeleton Death Sounds → Clip chết của skeleton
```

**ÂM THANH FLY** (cùng mô hình):
```
Fly Attack Sounds → Clip tấn công fly (kêu, đốt, v.v.)
Fly Movement Sounds → Clip chuyển động fly (bay, kêu)
Fly Damage Sounds → Clip đau của fly
Fly Death Sounds → Clip chết của fly
```

**ÂM THANH TANK** (cùng mô hình):
```
Tank Attack Sounds → Clip tấn công tank (dạp nặng, bash)
Tank Movement Sounds → Clip chuyển động tank (bước nặng)
Tank Damage Sounds → Clip đau của tank (kêu, kêu kim loại)
Tank Death Sounds → Clip chết của tank (va chạm, rơi nặng)
```

### BƯỚC 3: Thiết Lập Âm Thanh Player (30 giây)

1. Chọn GameObject **Player** trong Hierarchy
2. Vào Inspector
3. Nhấp "Add Component"
4. Tìm `PlayerAudioSystem` và thêm nó
5. Các cài đặt là tùy chọn (để mặc định nếu không chắc)
6. Audio sources sẽ tự động tạo

### BƯỚC 4: Thiết Lập Âm Thanh Kẻ Địch (1 phút cho mỗi kẻ)

Cho kẻ địch **Skeleton**:
1. Chọn enemy Skeleton trong Hierarchy
2. Add Component → `EnemyAudioSystem`
3. Trong Inspector, đặt **Enemy Type** = `Skeleton`
4. Audio sources sẽ tự động tạo

Lặp lại cho kẻ địch **Fly** và **Tank**, cài đặt loại kẻ địch phù hợp.

### BƯỚC 5: Kiểm Tra (2 phút)

1. Nhấn **Play** trong Unity
2. Lắng nghe âm thanh:
   - Tấn công: Nhấp để tấn công kẻ địch (sẽ nghe âm thanh tấn công)
   - Chuyển động: Di chuyển nhân vật xung quanh (sẽ nghe bước chân)
   - Sát thương: Bị đánh bởi kẻ địch (sẽ nghe âm thanh sát thương)
   - Chết: Chết (sẽ nghe âm thanh chết)
3. Kiểm tra Console để xác nhận âm thanh với log 🔊
4. Lắng nghe âm thanh kẻ địch trong chiến đấu

### BƯỚC 6: Điều Chỉnh Âm Lượng (2-5 phút)

Nếu âm thanh quá to hoặc quá yếu:
1. Vào AudioManager trong Inspector
2. Tìm âm thanh cần điều chỉnh
3. Thay đổi giá trị **Volume** (0.0 đến 1.0)
4. Nhấn Play để kiểm tra
5. Lặp lại cho đến khi hài lòng

---

## 🎵 Các Clip Âm Thanh Tối Thiểu Cần Thiết

Bạn cần tối thiểu **16 clip âm thanh**:

**Player (4 clip):**
- 1 âm thanh tấn công
- 1 âm thanh bước chân/chuyển động
- 1 âm thanh sát thương/đau
- 1 âm thanh chết

**Skeleton (4 clip):**
- 1 âm thanh tấn công
- 1 âm thanh chuyển động
- 1 âm thanh sát thương
- 1 âm thanh chết

**Fly (4 clip):**
- 1 âm thanh tấn công
- 1 âm thanh chuyển động
- 1 âm thanh sát thương
- 1 âm thanh chết

**Tank (4 clip):**
- 1 âm thanh tấn công
- 1 âm thanh chuyển động
- 1 âm thanh sát thương
- 1 âm thanh chết

**Tổng cộng: 16 clip tối thiểu**

Để có chất lượng tốt hơn, sử dụng **2-3 biến thể cho mỗi loại âm thanh** = 40-50 clip tổng cộng.

---

## 🎧 Tìm Kiếm Clip Âm Thanh

Nguồn âm thanh miễn phí:
- **Freesound.org** - Thư viện âm thanh lớn (tạo tài khoản)
- **Zapsplat.com** - Hiệu ứng âm thanh miễn phí (không cần đăng ký)
- **BBC Sound Effects Library** - Chất lượng chuyên nghiệp
- **Mixkit.co** - Âm thanh không bản quyền thương mại
- **OpenGameArt.org** - Âm thanh dành riêng cho game

**Nhập vào Unity:**
1. Tải xuống dưới dạng .mp3 hoặc .wav
2. Kéo vào thư mục `Assets/Audio/`
3. Unity tự động nhập dưới dạng AudioClip

---

## ✅ Danh Sách Kiểm Tra Xác Minh

Sau khi thiết lập, hãy xác minh:

- [ ] AudioManager tồn tại trong scene
- [ ] Tất cả danh sách âm thanh có clips gán
- [ ] Không có lỗi màu đỏ trong Console
- [ ] PlayerAudioSystem được thêm vào Player
- [ ] EnemyAudioSystem được thêm vào mỗi kẻ địch
- [ ] Enemy Type được cài đặt chính xác (Skeleton, Fly, Tank)
- [ ] Nhấn Play mà không gặp sự cố
- [ ] Âm thanh tấn công phát khi tấn công
- [ ] Âm thanh chuyển động phát khi di chuyển
- [ ] Âm thanh sát thương phát khi bị đánh
- [ ] Âm thanh chết phát khi chết
- [ ] Kẻ địch có âm thanh riêng trong chiến đấu

---

## 🔊 Cách Nó Hoạt Động

### Khi Player Tấn Công:
```
Player nhấp nút tấn công
    ↓
AttackComboController kích hoạt tấn công
    ↓
OnAttackStart event phát hành
    ↓
PlayerAudioSystem.PlayAttackSound() được gọi
    ↓
Clip tấn công ngẫu nhiên được chọn từ danh sách
    ↓
Âm thanh phát từ vị trí player (âm thanh 3D)
    ↓
Xảy ra song song với animation tấn công
```

### Khi Kẻ Địch Tấn Công:
```
Enemy.StartAttack() được gọi
    ↓
EnemyAudioSystem.PlayAttackSound() được gọi
    ↓
Âm thanh dành riêng cho kẻ địch được chọn
    ↓
Âm thanh phát từ vị trí kẻ địch (âm thanh 3D)
    ↓
Xảy ra song song với animation tấn công
```

### Khi Player Nhận Sát Thương:
```
Player.TakeDamage() được gọi
    ↓
PlayerAudioSystem.PlayDamageSound() được gọi
    ↓
Âm thanh sát thương phát ngay lập tức
    ↓
Xảy ra song song với animation va chạm
```

### Khi Player Chết:
```
Player.Die() được gọi
    ↓
PlayerAudioSystem.PlayDeathSound() được gọi
    ↓
Âm thanh chết phát ngay lập tức
    ↓
Xảy ra song parallel với animation chết
```

---

## ⚙️ Tham Chiếu Cài Đặt

### Cài Đặt PlayerAudioSystem

Trong Inspector, bạn có thể bật:
- **Use Random Pitch**: Bật để có sự đa dạng giọng nói
- **Pitch Variation**: Mức độ thay đổi pitch (0.1 = 10%)

Đề xuất: Để mặc định (useRandomPitch=true, pitchVariation=0.1)

### Cài Đặt EnemyAudioSystem

- **Enemy Type**: Chọn Skeleton, Fly, hoặc Tank
- **Use Random Pitch**: Bật để có sự đa dạng
- **Pitch Variation**: Lượng thay đổi
- **Movement Sound Interval**: Tần suất phát âm thanh chuyển động (0.5s mặc định)

Đề xuất: Để mặc định

### Hướng Dẫn Âm Lượng AudioManager

```
Âm Thanh Tấn Công:     0.7 - 1.0
Âm Thanh Chuyển Động:   0.3 - 0.6
Âm Thanh Sát Thương:    0.5 - 0.8
Âm Thanh Chết:         0.8 - 1.0
```

---

## 🎯 Hành Vi Dự Kiến

### Âm Thanh Tấn Công
- Phát khi combo bắt đầu
- Thay đổi nếu cấu hình nhiều clip
- Có thể chồng lên với âm thanh khác

### Âm Thanh Chuyển Động
- Phát định kỳ khi di chuyển (không liên tục)
- Cập nhật mỗi 0.5 giây
- Dừng khi player dừng di chuyển

### Âm Thanh Sát Thương
- Phát ngay lập tức khi nhận sát thương
- Phát song song với animation va chạm
- Hỗ trợ nhiều âm thanh đồng thời

### Âm Thanh Chết
- Phát khi nhân vật chết
- Phát song parallel với animation chết
- Thời lượng đầy đủ được phép hoàn thành

---

## 🚀 Cấu Hình Nâng Cao

### Thêm Nhiều Biến Thể Âm Thanh

Để thêm nhiều biến thể âm thanh tấn công:

1. Mở AudioManager trong Inspector
2. Tìm "Player Attack Sounds"
3. Tăng **Size** từ 1 lên 3 (hoặc nhiều hơn)
4. Kéo các clip tấn công khác nhau vào mỗi slot
5. Hệ thống sẽ chọn ngẫu nhiên một lần mỗi lần

### Điều Chỉnh Âm Thanh Dành Riêng Cho Kẻ Địch

Mỗi loại kẻ địch có thể có âm thanh riêng:

1. Cấu hình Skeleton Attack Sounds riêng biệt
2. Cấu hình Fly Attack Sounds với âm thanh giống như fly
3. Cấu hình Tank Attack Sounds với âm thanh nặng

Hệ thống tự động sử dụng danh sách chính xác dựa trên loại kẻ địch.

### Pitch Tùy Chỉnh Cho Mỗi Âm Thanh

Mỗi âm thanh có thể có pitch của riêng nó:

1. Trong AudioManager, nhấp vào entry âm thanh
2. Thay đổi giá trị **Pitch**
3. 1.0 = tốc độ bình thường
4. 0.5 = nửa tốc độ (sâu hơn)
5. 2.0 = gấp đôi tốc độ (cao hơn)

---

## 🐛 Xử Lý Sự Cố

### Không Có Âm Thanh Phát
**Nguyên nhân:** AudioManager chưa được thiết lập hoặc clip bị thiếu
**Cách Sửa:** 
1. Xác minh AudioManager tồn tại trong scene
2. Kiểm tra tất cả danh sách âm thanh có clips gán
3. Kiểm tra console để tìm thông báo lỗi

### Âm Thanh Quá Yếu
**Nguyên nhân:** Âm lượng được đặt quá thấp
**Cách Sửa:** Tăng giá trị volume trong AudioManager (thử 0.8-1.0)

### Âm Thanh Quá To
**Nguyên nhân:** Âm lượng được đặt quá cao
**Cách Sửa:** Giảm giá trị volume (thử 0.5-0.7)

### Cùng Một Âm Thanh Lặp Lại
**Nguyên nhân:** Chỉ có một clip trong danh sách âm thanh
**Cách Sửa:** Thêm nhiều clip vào danh sách để có sự đa dạng

### Không Có Định Vị 3D
**Nguyên nhân:** AudioListener bị thiếu hoặc cài đặt spatial sai
**Cách Sửa:** Đảm bảo AudioListener tồn tại (thường ở trên Main Camera)

### Lỗi Khi Chạy
**Nguyên nhân:** Component bị thiếu hoặc tham chiếu null
**Cách Sửa:** Kiểm tra console để tìm lỗi cụ thể, xác minh components được gắn

---

## 📚 Tham Chiếu Tài Liệu

Để tìm hiểu chi tiết, xem:

- **AUDIO_SYSTEM_GUIDE_VN.md** - Tài liệu kỹ thuật đầy đủ
- **AUDIO_SETUP_QUICK_VN.md** - Hướng dẫn tham khảo nhanh
- **AUDIO_ASSETS_ORGANIZATION_VN.md** - Cách tổ chức các tệp âm thanh
- **ARCHITECTURE_DIAGRAM.md** - Cách hoạt động của hệ thống
- **IMPLEMENTATION_CHECKLIST_VN.md** - Các bước xác minh

---

## ✨ Các Tính Năng Chính

✅ **Tích Hợp Tự Động** - Không cần ràng buộc sự kiện thủ công
✅ **Âm Thanh Song Song** - Âm thanh phát CÙNG với animations
✅ **Âm Thanh Không Gian 3D** - Âm thanh phát từ vị trí nhân vật
✅ **Sự Đa Dạng Âm Thanh** - Hỗ trợ nhiều clip cho mỗi loại âm thanh
✅ **Cấu Hình Trong Inspector** - Cấu hình trong Unity Inspector
✅ **Chất Lượng Chuyên Nghiệp** - Mã code chất lượng phát hành
✅ **Không Thay Đổi Ngã Tư** - Tất cả mã hiện có được bảo toàn
✅ **Xử Lý Lỗi Toàn Diện** - Xử lý graceful khi thiếu audio

---

## 🎮 Kiểm Tra Trong Game

### Danh Sách Kiểm Tra
1. [ ] Âm thanh tấn công player phát khi tấn công
2. [ ] Âm thanh bước chân player phát khi di chuyển
3. [ ] Âm thanh kẻ địch phát khi tấn công
4. [ ] Âm thanh chết khi chết
5. [ ] Âm thanh sát thương phát với animation va chạm
6. [ ] Nhiều clip phát ngẫu nhiên (không cùng âm thanh hai lần)
7. [ ] Âm thanh giảm dần theo khoảng cách (âm thanh 3D)
8. [ ] Không có sự chồng chéo/méo tiếng
9. [ ] Âm lượng phù hợp cho mỗi loại âm thanh
10. [ ] Cả ba loại kẻ địch có âm thanh riêng

---

## 📞 Hỗ Trợ

Để tìm kiếm vấn đề cụ thể hoặc câu hỏi chi tiết:
- Xem lại tệp tài liệu thích hợp
- Kiểm tra IMPLEMENTATION_CHECKLIST_VN.md
- Xem ARCHITECTURE_DIAGRAM.md để hiểu hệ thống
- Xác minh tất cả các tệp ở những vị trí chính xác

---

## 🏁 Dấu Hiệu Thành Công

Hệ thống âm thanh của bạn hoạt động chính xác khi:

✅ Không có lỗi đỏ trong Console
✅ AudioManager hiển thị trong Hierarchy
✅ Âm thanh phát khi dự kiến
✅ Âm thanh thay đổi (các clip khác nhau được chọn)
✅ Âm thanh phát song song với animations
✅ Hiệu ứng âm thanh 3D hoạt động (yên hơn khi xa)
✅ Tất cả các loại âm thanh hoạt động (tấn công, chuyển động, sát thương, chết)
✅ Tất cả các loại nhân vật có âm thanh riêng

---

## 🎵 Bước Tiếp Theo

1. **Ngay Lập Tức:** Hoàn thành các bước thiết lập ở trên
2. **Ngắn Hạn:** Kiểm tra và điều chỉnh âm lượng
3. **Trung Hạn:** Thêm nhiều biến thể âm thanh
4. **Dài Hạn:** Thêm âm nhạc, âm thanh xung quanh, diễn xuất giọng nói

---

## 📋 Tóm Tắt

Bạn có một hệ thống âm thanh hoàn chỉnh sẵn sàng sử dụng:
- 3 script mới (AudioManager, PlayerAudioSystem, EnemyAudioSystem)
- 5 script hiện có được cập nhật với trigger âm thanh
- 5 tệp tài liệu với hướng dẫn đầy đủ

**Tất cả đều được tích hợp. Bạn chỉ cần:**
1. Thêm AudioManager vào scene của bạn
2. Gán các clip âm thanh
3. Gắn các components vào nhân vật
4. Kiểm tra và điều chỉnh

**Thời gian thiết lập ước tính: 15-30 phút**

Tận hưởng trải nghiệm âm thanh sống động! 🎧
