# Quick Reference: Movement & Network Sync Files

## 🎯 File Locations & Purposes

### LOCAL MOVEMENT
| File | Path | Purpose |
|------|------|---------|
| **ThirdPersonController.cs** | `Assets/Assets/Character/Scripts/` | Character movement config, input handling, speed/rotation settings |
| **PlayerHealth.cs** | `Assets/Assets/Character/Scripts/` | Local health tracking, damage, knockback |
| **AttackComboController.cs** | `Assets/Assets/Character/Scripts/` | Attack state management |
| **RollController.cs** | `Assets/Assets/Character/Scripts/` | Roll/dodge mechanics |
| **ThirdPersonCamera.cs** | `Assets/Assets/Character/Scripts/` | Camera control, yaw tracking |

### NETWORK SYNCHRONIZATION ⭐
| File | Path | Purpose | Priority |
|------|------|---------|----------|
| **NetworkPlayerSync.cs** | `Assets/Assets/Scenes/` | ⭐ Main network controller - handles spawning, movement sync, state authority | HIGH |
| **NetworkManager.cs** | `Assets/Assets/Scenes/` | Input provider, player spawn/despawn management | HIGH |
| **NetworkInputData.cs** | `Assets/Assets/Scenes/` | Input data structure (move vector + sprint) | HIGH |
| **NetworkHealthSync.cs** | `Assets/Assets/Scenes/` | Network health synchronization with UI updates | MEDIUM |
| **PlayerMovement.cs** | `Assets/Assets/Scenes/` | Simple/legacy movement script (verify if in use) | LOW |

### PHOTON FUSION RUNTIME
| File | Path | Purpose |
|------|------|---------|
| **NetworkCharacterController.cs** | `Assets/Photon/Fusion/Runtime/` | Low-level physics sync, position/rotation replication |
| **NetworkCharacterController.cs** | Contains `NetworkCCData` struct | Network data: position, rotation, velocity, grounded state |

---

## 🔄 Data Flow Diagram

```
INPUT COLLECTION (Per Frame)
    ↓
NetworkManager.OnInput()
    ↓
NetworkInputData: {move, sprint}
    ↓
Fusion Input Buffer
    ↓
────────────────────────────────────────────
    ↓                                    ↓
[Server Processing]              [Client Processing]
    ↓                                    ↓
NetworkPlayerSync.                NetworkPlayerSync.
FixedUpdateNetwork()              Receives replication
    ↓                                    ↓
GetInput() → receives              Camera observes
input data                         remote player
    ↓                                    ↓
Calculate moveDir                  Local animation
(camera-relative)                  playback
    ↓                                    ↓
CharacterController.Move()         Position/Rotation
Apply gravity                       from server
    ↓                                    ↓
Update animation                   Animation may
    ↓                                    desync ⚠️
Fusion network                      ↓
replication                        Visual update
    ↓
All clients receive
updated position/rotation
```

---

## 🎮 Key Methods & Entry Points

### NetworkManager (Input Source)
```csharp
public void OnInput(NetworkRunner runner, NetworkInput input)
  → Collects keyboard input (W/A/S/D, Shift)
  → Creates NetworkInputData
  → Sends to Fusion

public void OnPlayerJoined(...)
  → Spawns player at spawn point
  → Creates NetworkPlayerSync instance

public void OnPlayerLeft(...)
  → Removes player from game
```

### NetworkPlayerSync (Movement Controller)
```csharp
public override void Spawned()
  → Initializes Input Authority vs State Authority
  → Enables/disables input
  → Sets up camera for local player

public override void FixedUpdateNetwork()
  → Main movement logic (runs on server, clients replicate)
  → GetInput() receives keyboard input
  → Moves CharacterController
  → Updates animator
  → Applies gravity
```

### NetworkHealthSync (Health System)
```csharp
public void TakeDamage(float damage)
  → Only called by StateAuthority (server)
  → Updates NetworkedHealth
  → Automatically replicates to clients

[OnChangedRender(nameof(OnHealthChanged))]
  → Callback when NetworkedHealth changes
  → Updates UI bars for all viewers
```

---

## 🐛 Suspected Bug Areas

### Issue #1: Animation Desynchronization
**Where**: `NetworkPlayerSync.UpdateAnimation()`  
**Problem**: Animation parameters set locally on every client independently  
**Result**: Remote player animations might not match network movement state  
**Severity**: Medium

### Issue #2: Camera Yaw Fallback
**Where**: `NetworkPlayerSync.FixedUpdateNetwork()` line ~85  
**Problem**: `_camera` is null on server, uses `transform.eulerAngles.y`  
**Result**: Player movement direction might not match client input on server  
**Severity**: Medium

### Issue #3: Gravity Application
**Where**: `NetworkPlayerSync.FixedUpdateNetwork()` final line  
**Problem**: Gravity applied every tick even when character is moving  
**Result**: Potential physics inconsistency  
**Severity**: Low

### Issue #4: Legacy Code
**Where**: `PlayerMovement.cs`  
**Problem**: Simple movement script might conflict with NetworkPlayerSync  
**Result**: Dual movement logic, jitter, or unpredictable behavior  
**Severity**: Medium (if file is in use)

---

## 📊 Authority Model

| Component | Input Authority | State Authority | Result |
|-----------|-----------------|-----------------|--------|
| **Local Player** | This Client | Server | Client sends input, server applies and broadcasts |
| **Remote Player** | Other Client | Server | Server replicates state, this client observes |
| **Health** | N/A | Server | Only server can modify NetworkedHealth |
| **Position/Rotation** | N/A | Server | Synchronized via Fusion replication |

---

## 🔍 To Find the Bug:

1. **Enable Debug Logs** in NetworkPlayerSync.FixedUpdateNetwork():
   ```csharp
   Debug.Log($"[MOVEMENT] moveDir: {moveDir}, speed: {speed}, position: {transform.position}");
   Debug.Log($"[ANIMATOR] Speed param set to: {speed}");
   ```

2. **Compare Local vs Remote** player movement in the scene

3. **Check Network Tick Rate**:
   - Verify `Runner.DeltaTime` is consistent
   - Check for missed ticks or resimulation

4. **Monitor Physics State**:
   - Is `CharacterController.isGrounded` correct?
   - Are velocity values expected?

5. **Verify Input Distribution**:
   - Add logs in `NetworkManager.OnInput()`
   - Check if input reaches players correctly

---

**Last Updated**: April 9, 2026  
**Project**: Blade-Pursuit  
**Network Framework**: Photon Fusion
