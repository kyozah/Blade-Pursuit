# Kế Hoạch Kiểm Thử Blade-Pursuit

## Tổng Quan
Tổng cộng: **30 Test Cases** chia thành **3 phần chính**, **mỗi phần 10 tests**

---

## **PHẦN 1: KIỂM THỬ DI CHUYỂN (Movement Tests)** - 10 Tests

### Test 1.1: Di chuyển tiến về phía trước (W key)
- **Mô tả**: Kiểm tra nhân vật di chuyển về phía trước khi nhấn phím W
- **Yêu cầu**: PlayerMovement script phải hoạt động
- **Kết quả mong đợi**: Vị trí Z của player tăng lên

### Test 1.2: Di chuyển lùi (S key)
- **Mô tả**: Kiểm tra nhân vật di chuyển lùi khi nhấn phím S
- **Yêu cầu**: PlayerMovement script phải hoạt động
- **Kết quả mong đợi**: Vị trí Z của player giảm xuống

### Test 1.3: Di chuyển sang trái/phải (A/D keys)
- **Mô tả**: Kiểm tra nhân vật di chuyển sang trái (A) và phải (D)
- **Yêu cầu**: PlayerMovement script phải hoạt động
- **Kết quả mong đợi**: Vị trí X của player thay đổi tương ứng

### Test 1.4: Roll/Lăn tránh (Dodge Roll)
- **Mô tả**: Kiểm tra khả năng lăn tránh và tốc độ
- **Yêu cầu**: RollController script phải hoạt động
- **Kết quả mong đợi**: Nhân vật có thể lăn nhanh để tránh tấn công

### Test 1.5: Kiểm tra Velocity/Speed
- **Mô tả**: Kiểm tra vận tốc nhân vật hợp lệ (≥ 0, < max speed)
- **Yêu cầu**: Speed configuration đúng
- **Kết quả mong đợi**: Vận tốc trong khoảng hợp lệ

### Test 1.6: Không di chuyển khi không có input
- **Mô tả**: Kiểm tra nhân vật đứng yên khi không có input
- **Yêu cầu**: Input system hoạt động đúng
- **Kết quả mong đợi**: Vị trí không thay đổi

### Test 1.7: Collision - không đi qua tường
- **Mô tả**: Kiểm tra nhân vật không đi qua các vật cản
- **Yêu cầu**: CharacterController và collider hoạt động
- **Kết quả mong đợi**: Nhân vật bị chặn bởi tường

### Test 1.8: Sprint (tốc độ cao)
- **Mô tả**: Kiểm tra sprint tăng tốc độ gấp đôi
- **Yêu cầu**: Sprint mechanic phải hoạt động
- **Kết quả mong đợi**: Sprint distance > normal distance

### Test 1.9: Jump
- **Mô tả**: Kiểm tra nhân vật có thể nhảy lên
- **Yêu cầu**: Jump mechanic phải hoạt động
- **Kết quả mong đợi**: Y position tăng lên

### Test 1.10: Knockback/Stun effect
- **Mô tả**: Kiểm tra nhân vật bị empush khi knockback
- **Yêu cầu**: Knockback system phải hoạt động
- **Kết quả mong đợi**: Nhân vật di chuyển xa theo hướng knockback

---

## **PHẦN 2: KIỂM THỬ GIAO DIỆN (UI Tests)** - 10 Tests

### Test 2.1: Hiển thị thanh máu
- **Mô tả**: Kiểm tra thanh máu (Health Bar) hiển thị đúng
- **Yêu cầu**: PlayerHealthBar script phải hoạt động
- **Kết quả mong đợi**: Thanh máu hiển thị đầy đủ khi game bắt đầu

### Test 2.2: Menu thua cuộc (Death Menu)
- **Mô tả**: Kiểm tra menu thua cuộc hiển thị khi player chết
- **Yêu cầu**: DeathMenuUI script phải hoạt động
- **Kết quả mong đợi**: Màn hình thua xuất hiện, có nút "Retry" hoặc "Menu"

### Test 2.3: Menu thắng cuộc (Victory Menu)
- **Mô tả**: Kiểm tra menu thắng hiển thị khi player tiêu diệt tất cả kẻ thù
- **Yêu cầu**: VictoryMenuUI script phải hoạt động
- **Kết quả mong đợi**: Màn hình thắng xuất hiện với animation/thông báo

### Test 2.4: Health Bar cập nhật khi máu thay đổi
- **Mô tả**: Kiểm tra thanh máu update real-time theo HP
- **Yêu cầu**: Health bar binding phải hoạt động
- **Kết quả mong đợi**: fillAmount thay đổi theo HP

### Test 2.5: Damage Popup/Text
- **Mô tả**: Kiểm tra damage text hiển thị khi nhận damage
- **Yêu cầu**: Damage popup system phải hoạt động
- **Kết quả mong đợi**: Damage number xuất hiện trên screen

### Test 2.6: Score/Points display
- **Mô tả**: Kiểm tra độ điểm hiển thị và cập nhật
- **Yêu cầu**: Score system phải hoạt động
- **Kết quả mong đợi**: Score text update khi có điểm mới

### Test 2.7: Pause Menu
- **Mô tả**: Kiểm tra menu pause có thể bật/tắt
- **Yêu cầu**: Pause menu system phải hoạt động
- **Kết quả mong đợi**: Pause menu toggle đúng

### Test 2.8: Button Interactions
- **Mô tả**: Kiểm tra button có thể được click
- **Yêu cầu**: Button component phải hoạt động
- **Kết quả mong đợi**: Button response đúng

### Test 2.9: Text Font/Size
- **Mô tả**: Kiểm tra font, kích thước text đúng
- **Yêu cầu**: UI assets cấu hình đúng
- **Kết quả mong đợi**: Font và size hợp lệ

### Test 2.10: Game Over screen delay
- **Mô tả**: Kiểm tra game over screen xuất hiện sau delay
- **Yêu cầu**: Death delay config đúng
- **Kết quả mong đợi**: Screen xuất hiện sau delay nhất định

---

## **PHẦN 3: KIỂM THỬ MÁU/SỨC KHỎE (HP Tests)** - 10 Tests

### Test 3.1: Giảm máu khi bị tấn công
- **Mô tả**: Kiểm tra máu giảm khi nhân vật bị kẻ thù tấn công
- **Yêu cầu**: PlayerHealth script và DamageTrigger phải hoạt động
- **Kết quả mong đợi**: HP giảm đúng số lượng sát thương

### Test 3.2: Tăng máu khi dùng thuốc/item phục hồi
- **Mô tả**: Kiểm tra máu tăng lên khi sử dụng item hồi máu
- **Yêu cầu**: PlayerHealth script phải xử lý healing
- **Kết quả mong đợi**: HP tăng lên cho đến mức tối đa

### Test 3.3: Chết khi máu bằng 0
- **Mô tả**: Kiểm tra nhân vật chết khi HP = 0
- **Yêu cầu**: PlayerHealth và PlayerKiller script phải hoạt động
- **Kết quả mong đợi**: Game over, hiển thị Death Menu

### Test 3.4: Invincibility frames (bất tử tạm thời)
- **Mô tả**: Kiểm tra không nhận damage trong thời gian bất tử
- **Yêu cầu**: I-frames system phải hoạt động
- **Kết quả mong đợi**: HP không thay đổi lúc bất tử

### Test 3.5: Knockback damage
- **Mô tả**: Kiểm tra damage + knockback phù hợp
- **Yêu cầu**: Knockback system phải hoạt động
- **Kết quả mong đợi**: HP giảm và bị empush

### Test 3.6: Max Health cap
- **Mô tả**: Kiểm tra máu không vượt quá giới hạn tối đa
- **Yêu cầu**: Health capping logic phải hoạt động
- **Kết quả mong đợi**: HP ≤ maxHealth

### Test 3.7: Damage từ các nguồn khác nhau
- **Mô tả**: Kiểm tra damage từ enemy, spike, poison tính đúng
- **Yêu cầu**: Multiple damage source handling
- **Kết quả mong đợi**: Tất cả damage được áp dụng

### Test 3.8: Poison/Damage over time (DoT)
- **Mô tả**: Kiểm tra damage liên tục trong thời gian nhất định
- **Yêu cầu**: DoT system phải hoạt động
- **Kết quả mong đợi**: Tích lũy damage theo thời gian

### Test 3.9: Shield/Armor reduction
- **Mô tả**: Kiểm tra armor giảm damage nhận được
- **Yêu cầu**: Armor mitigation system phải hoạt động
- **Kết quả mong đợi**: Damage thực tế < base damage

### Test 3.10: Respawn logic
- **Mô tả**: Kiểm tra player respawn tại điểm spawn sau khi chết
- **Yêu cầu**: Respawn system phải hoạt động
- **Kết quả mong đợi**: HP full, respawn tại spawn point

---

## Cấu Trúc File Test

```
Assets/Assets/Scenes/New Folder/Tests/
├── MovementTests.cs          (10 Tests)
├── UITests.cs                (10 Tests)
└── HealthSystemTests.cs      (10 Tests)
```

## Công Cụ Kiểm Thử
- **Framework**: NUnit (tích hợp sẵn Unity)
- **Unity Test Framework (UTF)**: Cho async tests
- **Assertion**: Assert.AreEqual, Assert.Greater, Assert.Less, v.v.

## Hướng Dẫn Chạy Test
1. Mở Unity Editor
2. Vào **Window > General > Test Runner**
3. Chọn **Play Mode** hoặc **Edit Mode**
4. Nhấn **Run All Tests**

