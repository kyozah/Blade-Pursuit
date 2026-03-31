# 📋 Tóm Tắt Kế Hoạch Test - Blade Pursuit v2.0

## 🎮 Cấu Trúc Test Chức Năng Game

Đã tổ chức game test thành **3 phần chính** với **30 test cases hoàn chỉnh** (mỗi phần 10 tests).

---

## 📂 Cấu Trúc File

```
Assets/Assets/Scenes/New Folder/Tests/
├── MovementTests.cs              ✅ 10 Tests
├── UITests.cs                    ✅ 10 Tests
└── HealthSystemTests.cs          ✅ 10 Tests

Blade-Pursuit/ (root)
├── TESTING_PLAN.md               📋 Kế hoạch chi tiết
├── TEST_EXECUTION_GUIDE.md       📖 Hướng dẫn chạy test
└── TEST_SUMMARY_V2.md            ← File này
```

---

## 🔍 Chi Tiết 30 Tests

### **PHẦN 1: DI CHUYỂN (Movement)** 🎯 10 Tests

| # | Tên Test | Mô Tả | Kết Quả Mong Đợi |
|---|----------|-------|------------------|
| 1.1 | Forward Move (W) | Nhân vật di chuyển tiến | Z position tăng |
| 1.2 | Backward Move (S) | Nhân vật di chuyển lùi | Z position giảm |
| 1.3 | Side Move (A/D) | Nhân vật di chuyển trái/phải | X position thay đổi |
| 1.4 | Roll/Dodge | Nhân vật lăn tránh nhanh | Khoảng cách ≥ 2m |
| 1.5 | Velocity/Speed | Kiểm tra vận tốc hợp lệ | 0 < velocity < max |
| 1.6 | No Input | Đứng yên khi không input | Vị trí không thay đổi |
| 1.7 | Collision | Không đi qua tường | Bị chặn bởi collider |
| 1.8 | Sprint | Tốc độ gấp đôi | Sprint distance > normal |
| 1.9 | Jump | Nhảy lên | Y position tăng |
| 1.10 | Knockback | Bị đẩy về phía sau | Di chuyển xa theo hướng |

**File**: `MovementTests.cs`  
**Mục đích**: Đảm bảo nhân vật di chuyển mượt mà và chính xác

---

### **PHẦN 2: GIAO DIỆN (UI)** 🎨 10 Tests

| # | Tên Test | Mô Tả | Kết Quả Mong Đợi |
|---|----------|-------|------------------|
| 2.1 | Health Bar | Thanh máu hiển thị | Visible, fillAmount = 100% |
| 2.2 | Death Menu | Menu thua xuất hiện | Visible khi HP = 0 |
| 2.3 | Victory Menu | Menu thắng xuất hiện | Visible khi thắng |
| 2.4 | Health Bar Update | Bar update real-time | fillAmount theo HP |
| 2.5 | Damage Popup | Damage text hiển thị | Number xuất hiện |
| 2.6 | Score Display | Điểm hiển thị | Score text update |
| 2.7 | Pause Menu | Menu pause toggle | Bật/tắt đúng |
| 2.8 | Button Click | Button phản hồi | onClick invoke |
| 2.9 | Text Font/Size | Font cấu hình đúng | Size 30, Bold |
| 2.10 | Game Over Delay | Delay trước Game Over | 2 giây |

**File**: `UITests.cs`  
**Mục đích**: Đảm bảo UI hoạt động đúng trong các tình huống quan trọng

---

### **PHẦN 3: SỨC KHỎE/MÁU (Health)** ❤️ 10 Tests

| # | Tên Test | Mô Tả | Kết Quả Mong Đợi |
|---|----------|-------|------------------|
| 3.1 | Take Damage | Máu giảm khi bị đánh | HP = 100 - 20 = 80 |
| 3.2 | Heal | Máu tăng từ potion | HP ≤ 100 |
| 3.3 | Death | Chết khi HP = 0 | isDead = true |
| 3.4 | I-frames | Không nhận damage lúc bất tử | HP không thay đổi |
| 3.5 | Knockback DMG | Damage + knockback | HP giảm + empush |
| 3.6 | Max Health Cap | Máu không vượt max | HP ≤ 100 |
| 3.7 | Multi Damage | Damage từ nhiều nguồn | Tất cả tính đúng |
| 3.8 | Damage Over Time | DoT liên tục | Tích lũy 10 damage/2s |
| 3.9 | Armor Reduction | Armor giảm damage | DMG < base damage |
| 3.10 | Respawn | Respawn tại spawn point | HP full, alive |

**File**: `HealthSystemTests.cs`  
**Mục đích**: Đảm bảo hệ thống máu hoạt động chính xác

---

## 🚀 Cách Chạy Test

### Quick Start (5 bước)

1. **Mở Test Runner**
   - `Window` → `General` → `Test Runner`

2. **Chọn Play Mode**
   - Mặc định là Play Mode

3. **Nhấn "Run All"**
   - Tất cả 30 tests sẽ chạy

4. **Xem Kết Quả trong Console**
   - ✅ = Pass
   - ❌ = Fail

5. **Xem Chi Tiết trong Test Runner Window**
   - Green = Pass
   - Red = Fail

---

## 📊 Mục Tiêu Kiểm Thử

| Mục Tiêu | Tests | Trạng Thái |
|----------|-------|-----------|
| Movement | 10/10 | ⏳ |
| UI | 10/10 | ⏳ |
| Health | 10/10 | ⏳ |
| **TOTAL** | **30/30** | ⏳ |

---

## ✨ Tính Năng Chính

✅ **Tự động kiểm tra** - Không cần chạy thủ công  
✅ **Output rõ ràng** - PASSED hoặc FAILED  
✅ **Có thể tái sử dụng** - Chạy lại bất kỳ lúc nào  
✅ **Dễ mở rộng** - Thêm tests mới dễ dàng  
✅ **Không ảnh hưởng game** - Xóa objects tạm sau khi test  

---

## 💡 Ví Dụ Console Output

```
========== Test Runner Started ==========

[MOVEMENT - 10 Tests]
✅ Test 1.1 PASSED: Forward move
✅ Test 1.2 PASSED: Backward move
✅ Test 1.3 PASSED: Side move
✅ Test 1.4 PASSED: Roll dodge
✅ Test 1.5 PASSED: Velocity valid
✅ Test 1.6 PASSED: No input
✅ Test 1.7 PASSED: Collision blocks
✅ Test 1.8 PASSED: Sprint speed
✅ Test 1.9 PASSED: Jump
✅ Test 1.10 PASSED: Knockback

[UI - 10 Tests]
✅ Test 2.1 PASSED: Health bar display
✅ Test 2.2 PASSED: Death menu
✅ Test 2.3 PASSED: Victory menu
✅ Test 2.4 PASSED: Health bar update
✅ Test 2.5 PASSED: Damage popup
✅ Test 2.6 PASSED: Score display
✅ Test 2.7 PASSED: Pause menu
✅ Test 2.8 PASSED: Button click
✅ Test 2.9 PASSED: Text font/size
✅ Test 2.10 PASSED: Game over delay

[HEALTH - 10 Tests]
✅ Test 3.1 PASSED: Take damage
✅ Test 3.2 PASSED: Heal
✅ Test 3.3 PASSED: Death
✅ Test 3.4 PASSED: I-frames
✅ Test 3.5 PASSED: Knockback damage
✅ Test 3.6 PASSED: Max health cap
✅ Test 3.7 PASSED: Multi damage
✅ Test 3.8 PASSED: Damage over time
✅ Test 3.9 PASSED: Armor reduction
✅ Test 3.10 PASSED: Respawn

========== RESULT ==========
Total: 30 Tests
Passed: 30 ✅
Failed: 0 ❌
Time: 25.5s

Status: ALL TESTS PASSED 🎉
```

---

## 📞 Hỗ Trợ

| Vấn đề | Giải pháp |
|--------|----------|
| Không tìm tests | Kiểm tra: `Assets/Assets/Scenes/New Folder/Tests/` |
| Tests fail | Xem console log, kiểm tra assertions |
| Chạy chậm | Dùng Play Mode, giảm WaitForSeconds |

---

## ✍️ Thông Tin

- **Phiên bản**: 2.0 (30 tests)
- **Ngày cập nhật**: 2026-03-31
- **Framework**: NUnit + Unity Test Framework
- **Trạng thái**: ✅ Sẵn sàng chạy

---

**🎉 Hệ Thống Test Blade-Pursuit hoàn chỉnh với 30 tests!**
