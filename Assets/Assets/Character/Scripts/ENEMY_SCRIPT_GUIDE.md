# Enemy Scripts — Hướng Dẫn Sử Dụng & Tùy Biến 🔧

## Tổng quan ✨
Tài liệu này mô tả các script liên quan tới hệ thống enemy trong dự án:
- `Enemy.cs` — Base class cho hành vi AI (Idle, Chase, Attack, Retreat), knockback, và health.
- `EnemyManager.cs` — Quản lý spawn, giới hạn số lượng, và điều phối attack (chỉ 1 enemy attack tại 1 thời điểm với cooldown).
- `Skeleton.cs`, `Goblin.cs`, `Tank.cs` — Các class kế thừa `Enemy` để tùy biến nhanh các loại quái.

---

## `Enemy.cs` 🔥
**Mục đích:** Định nghĩa hành vi AI cơ bản, xử lý damage, knockback, animation triggers.

### Các trường quan trọng (Inspector)
- `maxHealth` (float): Máu tối đa.
- `attackDamage` (float): Damage gây ra khi attack.
- `detectionRange`, `attackRange` (float): Khoảng phát hiện và tầm tấn công.
- `retreatDistance` (float): Khoảng lùi sau khi đánh.
- `moveSpeed` (float): Tốc độ di chuyển.
- `attackDelay` (float): Thời gian chuẩn bị trước khi gây damage.
- Knockback settings: `knockbackForce`, `knockbackUpwardForce`, `knockbackDuration`, `knockbackDrag`.
- `usePlayerForwardDirection` (bool): Dùng hướng player để tính knockback.

### Hành vi chính
- AI state: Idle → Chase → Attack → Retreat.
- `StartAttack()` kick off animation trigger `Attack`, gọi coroutine `PerformAttack()`.
- `PerformAttack()` đợi `attackDelay`, áp damage bằng `attackDamage`, tính retreat.
- `TakeDamage()` có 2 overload: với attacker position và với attacker forward direction; sẽ gọi `ApplyVelocityKnockback()`.
- Trước đây có delay cố định khi spawn — đã được loại bỏ: mới spawn sẽ action ngay nếu player trong zone và ở trong detection range.

### Yêu cầu component trên prefab
- `Rigidbody` (non-kinematic, gravity bật), không khóa linear velocity trừ khi cần.
- `Animator` với params:
  - Trigger: `Attack`, `Hit`, `Death`
  - Bool: `IsMoving`
- Collider phù hợp (Capsule/Box) để va chạm.

> Lưu ý về scripts trên Prefab:
- **Chỉ cần gắn script của loại quái cụ thể** (ví dụ `Fly`, `Skeleton`, `Tank`) vì các class này kế thừa từ `Enemy` và đã chứa toàn bộ hành vi cần thiết.
- **Không cần gắn cả `Enemy` và `Fly` đồng thời**; chỉ giữ một script (ở thực tế chỉ attach `Fly`).

---

## `EnemyManager.cs` 🗺️
**Mục đích:** Spawn các prefab theo vùng trigger, quản lý số lượng và điều phối attack cooldown.

### Các trường (Inspector)
- `skeletonPrefab`, `flyPrefab`, `tankPrefab` (GameObject).
- `skeletonWeight`, `flyWeight`, `tankWeight` (float, sum không cần đúng 1 — code tự chuẩn hóa): xác suất spawn.
- `maxEnemies`, `spawnRadius`, `spawnHeight`.
- `attackCooldown` (float): Thời gian giữa các lượt attack trên toàn vùng.

### Thay đổi chính
- Spawn đa loại theo tỷ lệ (weights) bằng `GetRandomPrefab()`.
- `Enemy` khi `SetManager()` sẽ được khởi động ngay lập tức (nếu player trong zone) — quái không còn idle ngay sau spawn.
- Vẫn giữ cơ chế: chỉ 1 enemy có thể `StartAttack()` khi `attackCooldown` chưa hết.

### Gợi ý setup
- Gán prefab phù hợp cho mỗi trường prefab.
- Cân bằng weights để đạt tỉ lệ spawn mong muốn.

## Hướng dẫn setup (Bước từng bước) ✅
Dưới đây là hướng dẫn ngắn gọn, làm theo từng bước trong Unity Editor.

1. **Chuẩn bị Player**
   - Đảm bảo GameObject Player có **Tag** là `Player`.
   - Player cần có **Collider** (để kích hoạt trigger zone) và script `PlayerHealth` (để nhận damage).

2. **Tạo prefab cơ bản cho 1 enemy**
   - Tạo một GameObject mới, đặt tên (ví dụ `Skeleton_Base`).
   - Thêm component: `Rigidbody` (gravity = true, isKinematic = false), **Collider** (Capsule/Box), `Animator`.
   - Gắn script `Skeleton` (hoặc `Goblin` / `Tank` nếu muốn tạo loại chính).
   - Trong Inspector của script: điều chỉnh **Stats** (ví dụ `maxHealth`, `attackDamage`, `moveSpeed`, `attackDelay`).
   - Tạo `AnimatorController` và đảm bảo các parameter: Trigger `Attack`, `Hit`, `Death`; Bool `IsMoving`.
    
    **Death animation setup (Thiết lập animation chết)**
    - Khuyến nghị: thêm Bool parameter `IsDead` (recommended). Khi enemy chết, script sẽ set `IsDead = true` nếu parameter có, hoặc sẽ fallback sang Trigger `Death`.
    - Để đảm bảo GameObject bị destroyed đúng sau khi clip death chạy xong, thêm **Animation Event** ở frame cuối của clip death, gọi method `OnDeathAnimationComplete()` (public trong `Enemy`). Hoặc bật `Use Death Animation Event` trong Inspector của `Enemy` (thuộc tính `useDeathAnimationEvent`) và thêm event trong Animation clip.
    - Nếu không dùng animation event, chỉnh `deathAnimationDuration` trong Inspector (mặc định = 2s) để thời gian destroy phù hợp với clip.
    - Gợi ý: đảm bảo state `Death` không trả về state khác (exit time off) và chuyển sang một state rời khỏi bàn điều khiển khi hoàn tất (hoặc sử dụng animation event để destroy).

**Troubleshooting: nếu death animation lặp liên tục**
- Kiểm tra clip animation (select clip in Project): trong Import Settings, **uncheck 'Loop Time'** cho clip Death.
- Mở `Animator` và chọn state `Death`: đảm bảo không có transition quay về chính state đó, và nếu có transition out thì **bỏ 'Has Exit Time'** nếu không muốn re-enter.
- Sử dụng `IsDead` boolean (recommended): script bây giờ chỉ set `IsDead = true` 1 lần. Nếu bạn vẫn thấy lặp, kiểm tra animation transitions hoặc animation events có thể gọi lại trigger.
- Nếu dùng Trigger `Death` thay vì `IsDead`, đảm bảo trigger chỉ được gọi 1 lần và không có animation event/transition gọi trigger lại.
- Thử bật `Use Death Animation Event` và đặt Animation Event `OnDeathAnimationComplete()` ở frame cuối để chắc chắn object bị destroy ngay sau kết thúc clip.
   - Kéo GameObject vào thư mục `Assets/.../Prefabs` để lưu thành Prefab.

3. **Thiết lập animation**
   - Trong `AnimatorController`: tạo states `Idle`, `Run`, `Attack`, `Hit`, `Death`.
   - Thêm transitions phù hợp và dùng các parameter đã nêu để trigger animation.
   - Kiểm tra trên prefab rằng `Animator` có controller đúng và animations hoạt động.

4. **Tạo/Thiết lập `EnemyManager`**
   - Tạo một GameObject trống, tên `EnemyManager_ZoneX`.
   - Add component `EnemyManager` và một Collider (Box/Sphere) với **Is Trigger = true**.
   - Trong Inspector của EnemyManager: kéo các prefab vào `skeletonPrefab`, `flyPrefab`, `tankPrefab`.
   - Điều chỉnh `skeletonWeight`, `flyWeight`, `tankWeight` (ví dụ mặc định `0.5`, `0.3`, `0.2`).
   - **Bấm nút `Edit Allowed Spawns` để bật/tắt loại quái được phép spawn** và tích chọn `Allow Skeleton` / `Allow Fly` / `Allow Tank`.
   - **Thiết lập `Max Fly Per Zone`** để giới hạn số Fly tồn tại đồng thời trong khu vực (mặc định = 1).
   - Set `maxEnemies`, `spawnRadius`, `spawnHeight` phù hợp với khu vực.

5. **Test nhanh**
   - Chạy scene, di chuyển Player vào vùng trigger.
   - Xác nhận enemy spawn ngay và **bắt đầu chase nếu ở trong detectionRange** (không còn chờ 3s).
   - Khi gần, kiểm tra enemy thực hiện attack (animation, damage) và manager đảm bảo chỉ một enemy attack tại một thời điểm với `attackCooldown`.

6. **Checklist Troubleshooting (Spawn cụ thể)**
   - **Collider / Trigger**: Kiểm tra `EnemyManager` có **Collider** (Box/Sphere) và **Is Trigger = true**. Nếu không, `OnTriggerEnter` sẽ không được gọi.
   - **Player tag & Physics**: Player phải có **Tag = Player**, có **Collider** và **Rigidbody** (thường non-kinematic). Lưu ý: để trigger xảy ra, ít nhất một trong hai collider (player hoặc manager) phải có Rigidbody.
   - **Player bắt đầu trong zone**: Nếu Player đã nằm trong vùng lúc bắt đầu scene, `OnTriggerEnter` sẽ không được gọi. (Code đã thêm kiểm tra lúc `Start()` — nếu Player đang ở trong zone thì hệ thống sẽ spawn ngay.)
   - **Kiểm tra prefab**: Đảm bảo `skeletonPrefab`, `goblinPrefab`, hoặc `tankPrefab` đã được gán trong `EnemyManager` Inspector (ít nhất 1 prefab phải có mặt).
   - **Component trên prefab**: Mỗi prefab phải có script kế thừa `Enemy` (ví dụ `Skeleton`, `Goblin`, `Tank`), `Rigidbody`, `Animator`, và Collider.
   - **Console logs**: Mở Console để xem cảnh báo/lỗi từ `EnemyManager` (ví dụ: "no prefabs assigned", "Collider IsTrigger = false", hoặc "Prefab does not contain an 'Enemy' component"). Những log này sẽ giúp xác định nguyên nhân nhanh.
   - **Spawn position**: Nếu quái spawn dưới đất hoặc trong wall, điều chỉnh `spawnHeight` hoặc `spawnRadius` để đưa vị trí spawn an toàn.
   - **Test nhanh**: Set `maxEnemies = 1`, thiết lập weights để chỉ spawn `Goblin` (ví dụ goblinWeight=1, others=0), sau đó vào ra vùng để kiểm tra behaviour cụ thể.

---

## Các loại enemy cụ thể 🧩
- `Skeleton` — hành vi mặc định (giữ các giá trị cơ bản).
- `Fly` — **chết 1 phát (maxHealth = 1)**, tốc độ cao có thể đuổi kịp người chơi khi chạy (`moveSpeed = 18`), tấn công nhanh với cooldown 1s (`attackCooldownOverride = 1`). Ví dụ mặc định trong script: `maxHealth = 1`, `attackDelay = 0.5`, `moveSpeed = 18`, `attackDamage = 8`, `attackCooldownOverride = 1`.
- `Tank` — máu và damage cao, di chuyển & đánh chậm. Ví dụ mặc định: `maxHealth = 300`, `attackDelay = 2.5`, `moveSpeed = 2`, `attackDamage = 25`.

Muốn thêm loại mới: Tạo class kế thừa `Enemy` và override `Start()` để đặt giá trị mặc định trước khi gọi `base.Start()`.

---

## Cách tạo Prefab đúng chuẩn ✅
1. Tạo GameObject, attach `Enemy` (hoặc `Skeleton`/`Goblin`/`Tank`) script.
2. Thêm `Rigidbody` (gravity true, isKinematic false), `Animator`, và collider.
3. Tạo `AnimatorController` với các state và parameters đã nêu ở trên.
4. Set Tag, nếu cần add layer hoặc collision settings.
5. Lưu Prefab vào `Assets/.../Prefabs` và gán vào `EnemyManager`.

---

## Troubleshooting nhanh ⚠️
- Enemy không di chuyển: kiểm tra `Rigidbody` có bị `isKinematic` hoặc constraints block không.
- Enemy spawn nhưng không hành động: kiểm tra `EnemyManager` có `Is Trigger` và `Player` có tag `Player`.
- Enemy không attack sau khi spawn: confirm `EnemyManager.IsPlayerInZone()` trả về true và khoảng cách ≤ `detectionRange`.
- Animator không chạy: kiểm tra parameter names khớp (case-sensitive).

---

## Tips & Tương lai 💡
- Nếu số lượng enemy cao, dùng Object Pooling để giảm GC và Instantiate overhead.
- Muốn wave hoặc spawn theo pattern, mở rộng `EnemyManager` để hỗ trợ waves và spawn schedule.
- Có thể thêm `IEnemy` interface nếu muốn có nhiều kiểu enemy không dùng rigidbody hoặc AI khác nhau.

---

Nếu bạn muốn, tôi có thể:
- Thêm một ví dụ prefab và Animator Controller mẫu vào repo ✅
- Thêm phần hướng dẫn cân bằng (balancing table) với các giá trị đề xuất ✍️

---

_File được tạo tự động. Nếu cần chỉnh nội dung hay format theo team style, báo tôi để cập nhật._
