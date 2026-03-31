# Hướng Dẫn Chạy Test Chức Năng Game Blade-Pursuit

## 📋 Tổng Quan

Game được chia làm **3 phần kiểm thử** với tổng cộng **10 Test Cases**:

| Phần | Tên | Số Test | Mục Đích |
|------|-----|---------|---------|
| 1️⃣ | **Movement Tests** | 4 | Kiểm tra di chuyển nhân vật |
| 2️⃣ | **UI Tests** | 3 | Kiểm tra giao diện người dùng |
| 3️⃣ | **Health System Tests** | 3 | Kiểm tra hệ thống máu/sức khỏe |
| - | **TỔNG** | **10** | - |

---

## 🚀 Hướng Dẫn Chạy Test

### Bước 1: Mở Test Runner trong Unity
1. Mở **Unity Editor**
2. Vào menu: `Window` → `General` → `Test Runner`
3. Cửa sổ **Test Runner** sẽ mở bên cạnh

### Bước 2: Chọn Chế Độ Test
- **Play Mode**: Để test gameplay thực tế (mặc định cho tests này)
- **Edit Mode**: Để test editor functionality

### Bước 3: Xem Danh Sách Test
Trong cửa sổ Test Runner, bạn sẽ thấy:
```
✓ MovementTests
  ✓ Test_1_1_Player_Moves_Forward_On_W_Input
  ✓ Test_1_2_Player_Moves_Backward_On_S_Input
  ✓ Test_1_3_Player_Moves_Left_And_Right
  ✓ Test_1_4_Player_Can_Roll_Dodge_Quickly

✓ UITests
  ✓ Test_2_1_HealthBar_Displays_Correctly
  ✓ Test_2_2_Death_Menu_Appears_On_Player_Death
  ✓ Test_2_3_Victory_Menu_Appears_On_Level_Complete

✓ HealthSystemTests
  ✓ Test_3_1_Health_Decreases_On_Damage
  ✓ Test_3_2_Health_Increases_With_Healing_Item
  ✓ Test_3_3_Player_Dies_When_Health_Reaches_Zero
```

### Bước 4: Chạy Tests
- **Chạy tất cả**: Nhấn nút `Run All`
- **Chạy một loại**: Click vào tên class (vd: `MovementTests`)
- **Chạy một test**: Click vào tên method

### Bước 5: Xem Kết Quả
- ✅ **Green**: Test passed
- ❌ **Red**: Test failed
- 🟡 **Yellow**: Test skipped

---

## 📊 Chi Tiết Từng Phần Test

### PHẦN 1️⃣: MOVEMENT TESTS (4 Tests)

#### Test 1.1: Di chuyển tiến (W key)
```
📍 Vị trí ban đầu: (0, 0, 0)
👣 Hành động: Nhấn W
✅ Kết quả: Vị trí Z tăng (0 < Z < 5)
```

#### Test 1.2: Di chuyển lùi (S key)
```
📍 Vị trí ban đầu: (0, 0, 0)
👣 Hành động: Nhấn S
✅ Kết quả: Vị trí Z giảm (Z < 0)
```

#### Test 1.3: Di chuyển trái/phải (A/D keys)
```
📍 Vị trí ban đầu: (0, 0, 0)
👣 Hành động: Nhấn D rồi A
✅ Kết quả: X+ rồi X- (X thay đổi)
```

#### Test 1.4: Lăn tránh (Roll/Dodge)
```
📍 Vị trí ban đầu: (0, 0, 0)
👣 Hành động: Lăn sang phải trong 0.5s
✅ Kết quả: Khoảng cách ≥ 1.6m
```

---

### PHẦN 2️⃣: UI TESTS (3 Tests)

#### Test 2.1: Thanh máu hiển thị
```
📍 Lúc bắt đầu game
✅ Yêu cầu:
  - Health Bar visible
  - fillAmount = 1.0 (đầy)
  - Màu xanh (health good)
```

#### Test 2.2: Menu thua cuộc
```
📍 Lúc player chết (HP = 0)
✅ Yêu cầu:
  - Death Menu xuất hiện
  - Alpha = 1.0
  - Có nút "Retry" hoặc "Menu"
```

#### Test 2.3: Menu thắng cuộc
```
📍 Lúc tiêu diệt tất cả kẻ thù
✅ Yêu cầu:
  - Victory Menu xuất hiện
  - Alpha = 1.0
  - Có thông báo "YOU WIN!"
```

---

### PHẦN 3️⃣: HEALTH SYSTEM TESTS (3 Tests)

#### Test 3.1: Giảm máu
```
📍 HP ban đầu: 100
👣 Hành động: Nhận 20 damage
✅ Kết quả: HP = 80
```

#### Test 3.2: Tăng máu
```
📍 HP: 70 (sau khi nhận 30 damage)
👣 Hành động: Dùng potion hồi 25 HP
✅ Kết quả: HP = 95 (≤ 100 max)
```

#### Test 3.3: Chết
```
📍 HP: 100
👣 Hành động: Nhận 150 damage
✅ Kết quả:
  - HP = 0
  - isDead = true
  - Game Over
```

---

## 🔍 Đọc Kết Quả Test

### Console Output
Khi chạy test, Unity Console sẽ hiển thị:

```
✅ Test 1.1 PASSED: Nhân vật di chuyển tiến thành công
✅ Test 1.2 PASSED: Nhân vật di chuyển lùi thành công
❌ Test 1.3 FAILED: Nhân vật không di chuyển sang phải
```

### Cấu Trúc Log
- ✅ = Test passed
- ❌ = Test failed
- ⚠️ = Warning
- 💀 = Player dies
- 💚 = Health restored

---

## ⚙️ Cấu Hình Test

### Cài đặt Play Mode Test
Một số test cần chạy tối thiểu bao lâu:
- Movement: 0.5 - 1.0 giây
- UI: 0.5 giây
- Health: 0.1 - 0.5 giây

**Total time**: ~5-10 giây cho tất cả 10 tests

### Dependencies
Đảm bảo dự án có:
- ✅ NUnit (tích hợp sẵn)
- ✅ Unity Test Framework (UTF)
- ✅ Scenes folder có các scripts cần test

---

## 🐛 Xử Lý Lỗi Phổ Biến

### Lỗi: "Can't find TestRunner"
**Giải pháp**: 
- Window → General → Test Runner

### Lỗi: "No tests found"
**Giải pháp**:
- Đảm bảo file test ở trong thư mục `Tests` hoặc có suffix `Tests`
- Tests file phải public

### Lỗi: "Script cannot be loaded"
**Giải pháp**:
- Rebuild solution: File → Generate Project Files
- Refresh assets: Ctrl + R

### Tests chạy chậm
**Giải pháp**:
- Giảm WaitForSeconds thời gian
- Chạy ở Play Mode thay vì Edit Mode

---

## 📝 Thêm Test Mới

Để thêm test mới, tạo file `.cs` với format:

```csharp
using UnityEngine.TestTools;
using NUnit.Framework;

public class MyNewTests
{
    [UnityTest] // hoặc [Test]
    public IEnumerator Test_My_New_Feature()
    {
        // Arrange
        var obj = new GameObject("Test");
        
        // Act
        obj.transform.position += Vector3.one;
        
        yield return null;
        
        // Assert
        Assert.AreEqual(obj.transform.position, Vector3.one);
    }
}
```

---

## 📚 Tài Liệu Tham Khảo

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Assertion Reference](https://nunit.org/)

---

## 🎯 Mục Tiêu
✅ Tất cả 10 test phải PASSED trước khi release game
✅ Chạy test trước khi build game
✅ Thêm test mới khi thêm feature mới

---

**Tạo ngày**: 2026-03-31
**Phiên bản**: 1.0
