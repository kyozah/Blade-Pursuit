# 🐛 Debugger's Checklist for Movement Sync Bug

## Step-by-Step Guide to Finding the Movement Synchronization Bug

---

## ⏱️ Phase 1: Setup & Observation (5 minutes)

### 1.1 Open Required Files
- [ ] Open **NetworkPlayerSync.cs** (`Assets/Assets/Scenes/NetworkPlayerSync.cs`)
  - This is where movement is actually processed
  
- [ ] Open **NetworkManager.cs** (`Assets/Assets/Scenes/NetworkManager.cs`)
  - This is where input is collected
  
- [ ] Open **ThirdPersonController.cs** (`Assets/Assets/Character/Scripts/ThirdPersonController.cs`)
  - This is where movement settings are defined

### 1.2 Test Scenario Setup
- [ ] Launch game in **play-together mode** (2+ instances)
- [ ] One player as Host, one as Client
- [ ] Position players so you can see both on screen or use two monitors
- [ ] Have one player move and observe the other

### 1.3 Document Observable Symptoms
- [ ] **Issue**: Remote player appears to move differently than expected?
  - [ ] Animation plays but position doesn't update?
  - [ ] Position updates but animation doesn't play?
  - [ ] Movement is jittery/stuttering?
  - [ ] Movement is delayed?
  - [ ] Movement direction is wrong?

- [ ] Take screenshot or video of the bug for reference

---

## 🔍 Phase 2: Input Verification (10 minutes)

### 2.1 Verify Input Collection
Open **NetworkManager.cs** at the `OnInput()` method and add debug logs:

```csharp
public void OnInput(NetworkRunner runner, NetworkInput input)
{
    var data = new NetworkInputData();
    var move = Vector2.zero;
    
    // ADD THIS DEBUG LOG
    bool moving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || 
                  Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    Debug.Log($"[INPUT] Player moving: {moving}, moveInput: {move}");
    
    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    move.y += 1;
    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  move.y -= 1;
    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  move.x -= 1;
    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;
    
    data.move = move.normalized;
    data.sprint = Input.GetKey(KeyCode.LeftShift);
    
    Debug.Log($"[INPUT] Sending: move={data.move}, sprint={data.sprint}");
    input.Set(data);
}
```

**Expected Output**: Should see `[INPUT]` logs multiple times per second

- [ ] **Check**: Do you see input logs?
  - YES → Continue to Phase 3
  - NO → Input collection is broken, debug here first

### 2.2 Verify Input Reception
Open **NetworkPlayerSync.cs** at `FixedUpdateNetwork()` and add debug:

```csharp
public override void FixedUpdateNetwork()
{
    if (_cc == null) return;
    
    // ADD THIS DEBUG
    bool hasInput = GetInput(out NetworkInputData input);
    Debug.Log($"[SYNC] GetInput returned: {hasInput}, HasStateAuth: {HasStateAuthority}, HasInputAuth: {HasInputAuthority}");
    
    if (GetInput(out NetworkInputData input))
    {
        // ... existing code ...
    }
}
```

**Expected Output**: 
- Local player: `GetInput returned: true, HasStateAuth: false, HasInputAuth: true`
- Remote player: `GetInput returned: false, HasStateAuth: false, HasInputAuth: false`
- Server/Host: `GetInput returned: true, HasStateAuth: true, HasInputAuth: false`

- [ ] **Check**: Are the values what you expected?
  - YES → Continue to Phase 3
  - NO → Authority model is misconfigured

---

## 🎯 Phase 3: Movement Direction Calculation (10 minutes)

### 3.1 Verify Camera Yaw (Camera-Relative Movement)
Still in **NetworkPlayerSync.cs** `FixedUpdateNetwork()`:

```csharp
if (HasStateAuthority || HasInputAuthority)  // Add this condition
{
    if (GetInput(out NetworkInputData input))
    {
        Vector3 inputDir = new Vector3(input.move.x, 0, input.move.y).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            // ADD DEBUG LOGGING HERE
            float cameraYaw = (_camera != null) ? _camera.GetCameraYaw() : transform.eulerAngles.y;
            Debug.Log($"[CAMERA] Camera: {(_camera != null ? "FOUND" : "NULL (using fallback)")}, " +
                      $"CameraYaw: {cameraYaw}, transform.eulerAngles.y: {transform.eulerAngles.y}");
            
            Vector3 camForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
            Vector3 camRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
            Vector3 moveDir = (camForward * input.move.y + camRight * input.move.x).normalized;
            
            Debug.Log($"[MOVEMENT] InputDir: {inputDir}, MoveDir: {moveDir}, CameraYaw: {cameraYaw}");
            
            // ... rest of movement code ...
        }
    }
}
```

**Expected Output**: 
- Local client: `Camera: FOUND, CameraYaw: [value]`
- Server: `Camera: NULL (using fallback), CameraYaw: [value from transform]`

**⚠️ CRITICAL CHECK**:
- [ ] Does server's fallback camera yaw match what clients see?
  - YES → Movement direction should be correct
  - NO → **BUG FOUND**: Camera yaw mismatch between clients and server

---

## 🎬 Phase 4: Animation Verification (10 minutes)

### 4.1 Check Animation Parameters
In **NetworkPlayerSync.cs**, find the `UpdateAnimation()` method:

```csharp
private void UpdateAnimation(float speed, bool isMoving) {
    if (_controller.animator != null) {
        Debug.Log($"[ANIM] Setting Speed: {speed}, IsMoving: {isMoving}");
        Debug.Log($"[ANIM] Current State: {_controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion")}");
        
        _controller.animator.SetFloat("Speed", speed);
        _controller.animator.SetBool("IsMoving", isMoving);
        
        // VERIFY WHAT WAS SET
        Debug.Log($"[ANIM] VERIFIED - Speed param: {_controller.animator.GetFloat("Speed")}, " +
                  $"IsMoving param: {_controller.animator.GetBool("IsMoving")}");
    }
}
```

**Expected Output**: Should show matching values

- [ ] Check: Do animation parameters match what you set?
  - YES → Animation system is working
  - NO → **BUG FOUND**: Animator parameter issue

### 4.2 Compare Local vs Remote Animation
In Play Mode:
- [ ] Watch your local character animation
- [ ] Watch remote character animation
- [ ] Do they play the same animations?
  - YES → Animation sync is working
  - NO → **BUG FOUND**: Animation desynchronization

---

## ⚙️ Phase 5: Physics & Gravity Check (10 minutes)

### 5.1 Verify CharacterController State
In **NetworkPlayerSync.cs** `FixedUpdateNetwork()`:

```csharp
if (GetInput(out NetworkInputData input))
{
    // ADD AT START
    Debug.Log($"[PHYSICS] CC.isGrounded: {_cc.isGrounded}, " +
              $"Position: {transform.position}, " +
              $"Velocity: {_cc.velocity}");
    
    // ... existing movement code ...
    
    // ADD BEFORE GRAVITY
    Debug.Log($"[MOVEMENT] About to apply gravity. Current Y velocity: {_cc.velocity.y}");
    
    // Gravity
    _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime);
    
    // ADD AFTER GRAVITY
    Debug.Log($"[MOVEMENT] After gravity. Y velocity: {_cc.velocity.y}");
}
```

**Expected Output**:
- `isGrounded` should be `true` when standing still
- `isGrounded` should be `false` when jumping
- Y velocity should become increasingly negative when falling

- [ ] Check: Do velocity values look correct?
  - YES → Physics is working
  - NO → **BUG FOUND**: Physics calculation issue

---

## 📊 Phase 6: Network Data Replication (15 minutes)

### 6.1 Monitor Position Replication
In **NetworkCharacterController.cs**:

```csharp
void CopyToBuffer() {
    Debug.Log($"[NET-BUFFER] Position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
    Data.TRSPData.Position = transform.position;
    Data.TRSPData.Rotation = transform.rotation;
}

void CopyToEngine() {
    Debug.Log($"[NET-ENGINE] Applying - Position: {Data.TRSPData.Position}, Rotation: {Data.TRSPData.Rotation}");
    // ... existing code ...
}
```

**Expected Output**: Position and rotation values should be continuously updated

- [ ] Check: Are positions being replicated?
  - YES → Network replication is working
  - NO → **BUG FOUND**: Network synchronization issue

---

## 🎪 Phase 7: Authority & State Check (10 minutes)

### 7.1 Verify Authority Assignment
In **NetworkPlayerSync.cs** `Spawned()`:

```csharp
public override void Spawned()
{
    // ADD THIS AT START
    Debug.Log($"[AUTHORITY] GameObject: {gameObject.name}, " +
              $"HasInputAuth: {HasInputAuthority}, " +
              $"HasStateAuth: {HasStateAuthority}, " +
              $"IsValid: {IsValid}, " +
              $"IsMoving: {IsMoving}");
    
    // ... existing code ...
}
```

**Expected Output**:
- Local player: `HasInputAuth: true, HasStateAuth: false`
- Remote player: `HasInputAuth: false, HasStateAuth: false`
- Server/Host: `HasInputAuth: false, HasStateAuth: true`

- [ ] Check: Are authorities assigned correctly?
  - YES → Continue to Phase 8
  - NO → **BUG FOUND**: Authority misconfiguration

---

## 💡 Phase 8: Identify the Bug Type

Based on your observations, identify which category the bug falls into:

### **Type A: Input Not Working**
**Symptom**: Players don't move at all or movement is ignored
**Location**: NetworkManager.OnInput() or GetInput() returns false
**Fix**: Check NetworkRunner configuration and input authority

### **Type B: Animation Mismatch** 
**Symptom**: Players move but animations don't play or show wrong animation
**Location**: NetworkPlayerSync.UpdateAnimation() running with wrong values
**Fix**: Ensure animation parameters sync with network state

### **Type C: Camera Yaw Mismatch**
**Symptom**: Remote players move in wrong direction or sideway movement is inverted
**Location**: Camera fallback logic when _camera is null
**Fix**: Pass camera yaw in input data or sync camera state

### **Type D: Physics Desync**
**Symptom**: Players fall through ground or float
**Location**: Gravity calculation or CharacterController state
**Fix**: Verify gravity is applied consistently

### **Type E: Network Replication Lag**
**Symptom**: Movement is very delayed on remote players
**Location**: Fusion tick rate or network bandwidth
**Fix**: Check Fusion configuration and network settings

### **Type F: Position Jitter**
**Symptom**: Players jitter or stutter during movement
**Location**: Conflicting movement sources (PlayerMovement.cs vs NetworkPlayerSync.cs)
**Fix**: Remove duplicate movement systems

---

## 🔧 Quick Fix Checklist

Once you've identified the bug type, try these fixes:

### **For Animation Mismatch**:
```csharp
// Ensure animation matches network movement state
UpdateAnimation(speed, moveDir.magnitude >= 0.1f);
```

### **For Camera Yaw Mismatch**:
```csharp
// Send camera yaw through input data
// Then use it on server for consistent direction calculation
```

### **For Physics Issues**:
```csharp
// Ensure gravity is only applied when needed
if (Data.Grounded && moveVelocity.y < 0) {
    moveVelocity.y = 0f;
}
```

### **For Replication Lag**:
```csharp
// Increase Fusion tick rate in NetworkRunner settings
// Or check network bandwidth
```

---

## 📝 Bug Report Template

Once you've found the bug, document it:

```
BUG: [Brief Description]
Type: [A/B/C/D/E/F]
Severity: [Critical/High/Medium/Low]
Reproducibility: [Always/Often/Sometimes/Rare]

SYMPTOMS:
- [What you observe]

LOCATION:
- File: [path/to/file.cs]
- Method: [MethodName]
- Line: [line number]

ROOT CAUSE:
[What causes the bug]

REPRODUCTION STEPS:
1. [Step 1]
2. [Step 2]
3. [Observed bug]

PROPOSED FIX:
[How to fix it]
```

---

## 🚀 Tips & Tricks

- **Use Scene Camera**: Switch between Scene view and Game view to see actual vs rendered positions
- **Enable Gizmos**: Add gizmos to visualize movement vectors and positions
- **Frame Debugger**: Use Frame Debugger to see exact frame where position diverges
- **Profiler**: Use Profiler to check if physics calculations are expensive
- **Network Simulator**: Use Fusion's network simulator to test with latency/packet loss
- **Breakpoints**: Set breakpoints in key methods to pause and inspect state

---

**Version**: 1.0  
**Last Updated**: April 9, 2026  
**Project**: Blade-Pursuit
