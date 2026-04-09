# Player Movement & Network Synchronization Analysis
## Blade-Pursuit Project Structure

Generated: April 9, 2026

---

## 📋 Overview

This document maps all files responsible for player movement and network synchronization in the Blade-Pursuit project using **Photon Fusion** for multiplayer networking.

### Key Components:
- **Local Player Movement**: Character controller and input handling
- **Network Synchronization**: Photon Fusion-based state replication
- **Health Management**: Network-synchronized health system
- **Input Distribution**: Server-side input processing

---

## 🎮 Player Movement Files

### 1. **ThirdPersonController.cs**
**Location**: `Assets/Assets/Character/Scripts/ThirdPersonController.cs`

**Purpose**: Main character movement controller with local input handling

**Key Features:**
- Movement speed and sprint speed settings
- Ground check and gravity
- Input System integration (`PlayerInputActions`)
- Attack combo and roll controller integration
- Support for network-controlled mode (`isNetworkControlled` flag)

**Key Methods:**
- `EnableInput()` - Enables input for local player
- `DisableInput()` - Disables input for remote players
- `void OnEnable()` - Conditionally enables input based on `isNetworkControlled`
- `void OnDisable()` - Conditionally disables input

**Network Integration Points:**
- `isNetworkControlled` property - Set to `true` when spawned via NetworkPlayerSync
- Called by: `NetworkPlayerSync.Spawned()`

**Issue Areas:**
- Input is primarily handled through Input System
- Actual movement happens in NetworkPlayerSync for network players
- The `ThirdPersonController` acts as a config holder for network players

---

### 2. **PlayerMovement.cs**
**Location**: `Assets/Assets/Scenes/PlayerMovement.cs`

**Purpose**: Simple example network-based movement script

**Current Implementation:**
```csharp
public override void FixedUpdateNetwork()
{
    if (!HasStateAuthority) return;
    if (GetInput(out NetworkInputData input))
    {
        var move = new Vector3(input.move.x, 0, input.move.y);
        if (move.sqrMagnitude > 0)
            controller.Move(move * speed * Runner.DeltaTime);
    }
}
```

**Status**: ⚠️ **SIMPLE IMPLEMENTATION** - This script is basic and may not be the primary movement handler

**Note**: This might be a legacy script. The main movement logic appears to be in `NetworkPlayerSync.FixedUpdateNetwork()`.

---

### 3. **NetworkCharacterController.cs** (Fusion Runtime)
**Location**: `Assets/Photon/Fusion/Runtime/NetworkCharacterController.cs`

**Purpose**: Low-level Photon Fusion network character controller for physics synchronization

**Network Synchronization Data Structure:**
```csharp
public unsafe struct NetworkCCData : INetworkStruct {
    public NetworkTRSPData TRSPData;      // Transform (Position & Rotation)
    int _grounded;                        // Is grounded flag
    Vector3Compressed _velocityData;      // Velocity
}
```

**Key Features:**
- Inherits from `NetworkTRSP` (Transform, Rotation, Scale, Proxy)
- Automatic position/rotation synchronization
- Velocity tracking (compressed)
- Ground state tracking
- Jump and Move methods

**Key Methods:**
- `Move(Vector3 direction)` - Networked movement with gravity
- `Jump(bool ignoreGrounded, float? overrideImpulse)` - Networked jump
- `Teleport(Vector3?, Quaternion?)` - Networked teleportation
- `CopyToBuffer()` - Copies transform to network data
- `CopyToEngine()` - Copies network data to transform
- `BeforeAllTicks()` & `AfterAllTicks()` - Tick lifecycle callbacks

**Synchronization Mechanism:**
- **Before Tick**: `CopyToEngine()` - Applies network state to local transform
- **After Tick**: `CopyToBuffer()` - Copies local transform to network state
- **Spawned()**: Resets CharacterController internal state

**Graph of Sync**:
```
Network Buffer (NetworkCCData)
    ↓
CopyToEngine() [BeforeAllTicks]
    ↓
Transform.Position & CharacterController
    ↓
User Simulation (gameplay)
    ↓
CopyToBuffer() [AfterAllTicks]
    ↓
Network Buffer (NetworkCCData)
```

---

## 🌐 Network Synchronization Files

### 4. **NetworkPlayerSync.cs** ⭐ MAIN NETWORK CONTROLLER
**Location**: `Assets/Assets/Scenes/NetworkPlayerSync.cs`

**Purpose**: Primary network synchronization controller for player movement, state management, and network callbacks

**Component Hierarchy:**
- Inherits from `NetworkBehaviour`
- Manages: ThirdPersonController, CharacterController, PlayerHealth, AttackComboController, RollController

**Spawned() Method:**
- Called when player is spawned over network
- Sets up Input Authority vs State Authority logic:
  - **Local Player (HasInputAuthority)**: 
    - Enables input handling
    - Enables camera setup
    - Shows main health bar HUD
  - **Remote Player**:
    - Disables input
    - Disables attack/roll systems
    - Shows only overhead health bar

**FixedUpdateNetwork() Method** - MAIN MOVEMENT LOGIC:
```csharp
public override void FixedUpdateNetwork()
{
    if (_cc == null) return;
    
    if (GetInput(out NetworkInputData input))
    {
        // Check state (attacking, rolling, dead, etc.)
        // Calculate movement direction based on camera yaw
        
        // Movement:
        _cc.Move(moveDir * speed * Runner.DeltaTime);
        
        // Rotation:
        transform.rotation = Quaternion.Slerp(...);
        
        // Animation:
        UpdateAnimation(speed, true);
        
        // Gravity:
        _cc.Move(Vector3.down * 9.81f * Runner.DeltaTime);
    }
}
```

**Key Features:**
- **Input Consumption**: `GetInput(out NetworkInputData input)` receives input from server
- **Camera-Relative Movement**: Uses `_camera.GetCameraYaw()` to calculate move direction
- **Rotation Smoothing**: Smooth slerp rotation toward movement direction
- **Animation Updates**: Sets animator `Speed` and `IsMoving` parameters
- **RPC for Lobby**: `RpcHideLobbyOnClients()` - StateAuthority RPC to all clients

**Synchronization Mechanism:**
- Movement happens in `FixedUpdateNetwork()` (runs on both server and clients receiving state)
- Position/rotation automatically replicated by Fusion
- Animation state updated locally on each client

**Issue Areas to Investigate:**
1. **Camera Reference**: `_camera` will be `null` on server - fallback to `transform.eulerAngles.y`
2. **Animation Sync**: Animation parameters set locally - may not perfectly sync with network state
3. **Gravity Application**: Applied every frame even when not moving (double application risk)
4. **Speed Calculation**: Switching between `moveSpeed` and `sprintSpeed` - ensure clean transitions

---

### 5. **NetworkInputData.cs**
**Location**: `Assets/Assets/Scenes/NetworkInputData.cs`

**Purpose**: Network input data structure for Photon Fusion

**Data Structure:**
```csharp
public struct NetworkInputData : INetworkInput
{
    public Vector2 move;            // Movement input (-1 to 1 on X,Y axes)
    public NetworkBool sprint;      // Sprint state
}
```

**Information Carried:**
- **move**: Player input direction (W/A/S/D or arrow keys)
- **sprint**: Whether left shift is held

**Note**: Simple structure - other actions (attack, roll, etc.) handled separately

---

### 6. **NetworkManager.cs** - Input Provider
**Location**: `Assets/Assets/Scenes/NetworkManager.cs`

**Purpose**: Main Photon Fusion runner manager and input provider

**Implements**: `INetworkRunnerCallbacks`

**Key Components:**
- Stores `NetworkRunner` instance
- Manages player spawning (via `OnPlayerJoined`)
- Manages player despawning (via `OnPlayerLeft`)
- Provides input via `OnInput()` callback

**OnInput() Method** - INPUT COLLECTION:
```csharp
public void OnInput(NetworkRunner runner, NetworkInput input)
{
    var data = new NetworkInputData();
    var move = Vector2.zero;
    
    // Keyboard input collection
    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    move.y += 1;
    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  move.y -= 1;
    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  move.x -= 1;
    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;
    
    data.move = move.normalized;
    data.sprint = Input.GetKey(KeyCode.LeftShift);
    
    input.Set(data);  // Send to Fusion
}
```

**Player Management:**
- `OnPlayerJoined()` - Spawns player at SpawnPoint or random position
- `OnPlayerLeft()` - Despawns player and removes from tracking
- Maintains `Dictionary<PlayerRef, NetworkObject> _spawnedPlayers`

**Network Flow**:
```
Input Handling
(OnInput in NetworkManager)
    ↓
NetworkInputData Creation
    ↓
Fusion Input Buffer
    ↓
Sent to Server
    ↓
Server Distributes to GetInput() in NetworkPlayerSync
    ↓
Movement Application
```

---

### 7. **NetworkHealthSync.cs** - Health Synchronization
**Location**: `Assets/Assets/Scenes/NetworkHealthSync.cs`

**Purpose**: Network-synchronized health system with UI updates

**Networked Property:**
```csharp
[Networked, OnChangedRender(nameof(OnHealthChanged))]
public float NetworkedHealth { get; set; }
```

**Key Features:**
- Health only modifiable by State Authority (server)
- Automatic UI updates when health changes via `OnChangedRender` callback
- Dual UI bars: Overhead bar (all players) + Main HUD (local player only)

**TakeDamage() Method:**
```csharp
public void TakeDamage(float damage)
{
    if (HasStateAuthority)
    {
        NetworkedHealth -= damage;
        if (NetworkedHealth < 0) NetworkedHealth = 0;
    }
}
```

**Synchronization Pattern:**
- Health changes only on server (StateAuthority)
- Changes automatically replicate to all clients
- `OnHealthChanged()` callback triggers UI updates
- Remote players see health bars update automatically

---

## 🔄 Network Synchronization Flow

### Player Spawning & Initialization:
```
1. Client connects to room
2. NetworkManager.OnPlayerJoined(runner, player) [Server]
3. Server spawns player prefab at NetworkPlayerSync
4. NetworkPlayerSync.Spawned() determines Input/State Authority
5. Local player: EnableInput() + Camera setup
6. Remote player: DisableInput()
7. Both: Movement synchronized via FixedUpdateNetwork()
```

### Movement Synchronization:
```
Client Side:
  Input.GetKey() → NetworkManager.OnInput()
  ↓
  Create NetworkInputData
  ↓
  Fusion Input Queue
  ↓
  Send to Server

Server Side:
  Receives NetworkInputData
  ↓
  NetworkPlayerSync.FixedUpdateNetwork()
  ↓
  GetInput(out NetworkInputData) returns true
  ↓
  Calculate moveDir (camera-relative)
  ↓
  _cc.Move(moveDir * speed * dt)
  ↓
  Update rotation & animation
  ↓
  Fusion replicates transform
  ↓
  All clients receive updated position/rotation

Client Side (Remote Players):
  Receive position/rotation updates
  ↓
  NetworkCharacterController/Fusion updates transform
  ↓
  Animation plays locally (desynchronized!)
```

### Critical Observation ⚠️:
**Animation Sync Issue**: Animation parameters are set in `NetworkPlayerSync.FixedUpdateNetwork()` but applied locally on each client. This can cause animation desynchronization between clients.

---

## 📊 File Structure Summary

```
Assets/
├── Assets/Character/Scripts/
│   ├── ThirdPersonController.cs          [Movement config & input mgmt]
│   ├── PlayerHealth.cs                   [Local health system]
│   ├── AttackComboController.cs          [Attack states]
│   ├── RollController.cs                 [Roll/dodge states]
│   └── ...
├── Assets/Scenes/
│   ├── NetworkPlayerSync.cs              ⭐ [Main network controller]
│   ├── NetworkManager.cs                 [Input provider & player mgmt]
│   ├── NetworkInputData.cs               [Input data struct]
│   ├── NetworkHealthSync.cs              [Network health sync]
│   ├── PlayerMovement.cs                 [Legacy/simple movement]
│   └── Gameplay.unity                    [Main game scene]
└── Photon/Fusion/Runtime/
    ├── NetworkCharacterController.cs     [Low-level physics sync]
    ├── NetworkTRSP.cs                    [Transform sync base]
    └── ...
```

---

## 🐛 Potential Issues & Bug Locations

### 1. **Animation Desynchronization**
- **Location**: `NetworkPlayerSync.UpdateAnimation()`
- **Issue**: Animation parameters set locally, may not match actual network state
- **Impact**: Remote players' animations might not match their actual position
- **Fix**: Consider using networked animation parameters or state machine sync

### 2. **Camera Yaw on Server**
- **Location**: `NetworkPlayerSync.FixedUpdateNetwork()` line ~85
- **Issue**: `_camera` is null on server, fallback to `transform.eulerAngles.y`
- **Impact**: Server-side movement direction might not match client expectations
- **Fix**: Ensure fallback logic is correct, or pass camera yaw in input data

### 3. **Gravity Double Application**
- **Location**: `NetworkPlayerSync.FixedUpdateNetwork()` last line
- **Issue**: Gravity applied even when moving, might be applied twice
- **Impact**: Incorrect fall speed or float physics
- **Fix**: Ensure gravity is only applied when necessary

### 4. **Input Missing on Remote Players**
- **Location**: `NetworkPlayerSync.FixedUpdateNetwork()`
- **Issue**: `GetInput()` returns false for remote players, no movement
- **Impact**: Remote players appear frozen until their local input is processed
- **Fix**: This might be intentional (server authority) but verify with designer

### 5. **PlayerMovement.cs Legacy Code**
- **Location**: `Assets/Assets/Scenes/PlayerMovement.cs`
- **Issue**: Simple movement script might conflict with NetworkPlayerSync
- **Impact**: Dual movement logic could cause jitter or unexpected behavior
- **Fix**: Verify if this script is still in use or should be removed

### 6. **Character Controller Reset**
- **Location**: `NetworkCharacterController.Spawned()`
- **Issue**: Disables and re-enables CharacterController to clear internal state
- **Impact**: Could cause frame hitches or position resets
- **Fix**: Monitor for position resets after spawning

---

## 🎯 Debugging Recommendations

To identify the movement synchronization bug:

1. **Check Animation Parameters**:
   - Add debug logs in `UpdateAnimation()` to verify Speed and IsMoving values
   - Compare remote player animation state with actual movement

2. **Verify Input Distribution**:
   - Log `OnInput()` calls in NetworkManager
   - Verify input reaches `GetInput()` in NetworkPlayerSync

3. **Camera Yaw Handling**:
   - Add debug to show `_camera.GetCameraYaw()` value on client and server
   - Verify movement direction calculation

4. **Physics State**:
   - Monitor CharacterController.isGrounded state
   - Check velocity calculations, especially gravity

5. **Network Tick Timing**:
   - Verify `FixedUpdateNetwork()` timing between server and clients
   - Check for missed ticks or resimulation

6. **Position Replication**:
   - Add gizmos to visualize expected vs actual position
   - Check network packet drops or delays

---

## 📝 Related Documentation Files

- `ARCHITECTURE_DIAGRAMS.md` - System architecture overview
- `COMPLETE_IMPLEMENTATION_SUMMARY.md` - Implementation details
- `IMPLEMENTATION_SUMMARY.md` - Summary of implementations

---

## 🔗 Key Classes & Interfaces

| Class | Purpose |
|-------|---------|
| `NetworkBehaviour` | Base class for network-synced objects (Fusion) |
| `INetworkRunnerCallbacks` | Interface for network callbacks |
| `INetworkInput` | Interface for input data structures |
| `CharacterController` | Unity's built-in character movement component |
| `NetworkTRSP` | Fusion's transform sync base class |
| `NetworkInputData` | Custom input data structure |

---

**End of Analysis**
