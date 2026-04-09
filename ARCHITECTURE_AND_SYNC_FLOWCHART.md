# Architecture & Synchronization Flowchart

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        BLADE-PURSUIT NETWORKING SYSTEM                      │
│                          (Photon Fusion Based)                               │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────┐                          ┌──────────────────────┐
│   CLIENT 1 (Local)   │                          │   CLIENT 2 (Remote)  │
├──────────────────────┤                          ├──────────────────────┤
│ Input System         │                          │ Input System         │
│ PlayerInputActions   │                          │ (disabled)           │
│      ↓               │                          │      ↓               │
│ Input.GetKey()       │◄───────────────────────►│ Receives Input       │
│      ↓               │                          │ from Network         │
│ NetworkManager       │                          │      ↓               │
│ .OnInput()           │                          │ (Becomes Network)    │
│      ↓               │                          │      ↓               │
│ NetworkInputData     │                          │ NetworkInputData     │
│ {move, sprint}       │                          │ {move, sprint}       │
│      ↓               │                          │      ↓               │
└──────────────────────┘                          └──────────────────────┘
           │                                               │
           │   Fusion Input Buffer                       │
           │   (Delayed & Synchronized)                 │
           └───────────────────┬──────────────────────────┘
                               │
                    ┌──────────▼──────────┐
                    │  FUSION RUNNER      │
                    │  (Simulates Tick)   │
                    │                     │
                    │ BeforeAllTicks      │
                    │ Gameplay Tick       │
                    │ AfterAllTicks       │
                    │                     │
                    └──────────┬──────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
┌───────▼────────┐   ┌─────────▼─────────┐   ┌──────▼────────┐
│ SERVER/HOST    │   │ CLIENT 1 (Local)  │   │ CLIENT 2      │
├────────────────┤   ├───────────────────┤   ├───────────────┤
│ HasInputAuth:  │   │ HasInputAuth: YES │   │ HasInputAuth: │
│    NO          │   │ HasStateAuth: NO  │   │   NO          │
│ HasStateAuth:  │   │                   │   │ HasStateAuth: │
│    YES         │   │                   │   │   NO          │
│                │   │                   │   │               │
│ Processes ALL  │   │ Provides Input    │   │ Observes      │
│ client inputs  │   │ Owns Camera       │   │ Replication   │
│ Runs movement  │   │ Local Rendering   │   │               │
└────┬───────────┘   └─────┬─────────────┘   └───────┬───────┘
     │                     │                         │
     │ NetworkPlayerSync   │ NetworkPlayerSync      │
     │ .FixedUpdateNetwork │ .FixedUpdateNetwork   │
     │                     │                       │
     │ GetInput() ✓        │ GetInput() ✗          │ Sees replicated
     │ Process Movement    │ No local input        │ transforms only
     │ Update Position     │ Applies same logic    │
     │ Apply Gravity       │ for consistent result │
     │ Replicate Data      │                       │
     │                     │                       │
     └─────────┬───────────┴───────────────────────┘
               │
      ┌────────▼─────────┐
      │ Fusion Network   │
      │ Synchronization  │
      │                  │
      │ Broadcasts:      │
      │ • Position       │
      │ • Rotation       │
      │ • Velocity       │
      │ • Grounded State │
      │ • Health         │
      │                  │
      └────────┬─────────┘
               │
      ┌────────▼─────────┐
      │ All Clients      │
      │ Receive Updates  │
      │ & Render         │
      └──────────────────┘
```

---

## Movement Data Flow (Detailed)

```
                            TICK CYCLE
                               ↓
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
        ↓                      ↓                      ↓
    BeforeAllTicks      FixedUpdateNetwork()     AfterAllTicks
        │                      │                      │
        │                      │                      │
    [Server]               [Server]              [Server]
    CopyToEngine()         GetInput()            CopyToBuffer()
        │                      │                      │
        ├─ Set position        ├─ Receive input       ├─ Read current
        ├─ Set rotation        │  {move, sprint}      │  position
        ├─ Re-enable CC        │                      ├─ Read current
        │                      ├─ Check state         │  rotation
        │                      │  (attack, roll...)   │
        │                      │                      ├─ Store to Network
        │                      ├─ Calculate moveDir   │  Buffer
        │                      │  (camera-relative)   │
        │                      │                      │
        │                      ├─ CharacterController │
        │                      │  .Move(             │
        │                      │   moveDir * speed)   │
        │                      │                      │
        │                      ├─ Apply Gravity       │
        │                      │  .Move(              │
        │                      │   Vector3.down * g)  │
        │                      │                      │
        │                      ├─ Update Rotation     │
        │                      │                      │
        │                      ├─ Update Animator     │
        │                      │  Speed parameter     │
        │                      │  IsMoving parameter  │
        │                      │                      │
        └──────────────────────┴──────────────────────┴─────────────────┐
                                                                        │
                      ┌─────────────────────────────────────────────────┤
                      │                                                 │
                      ▼                                                 ▼
                [Fusion Replicates Transform Data]        [Clients receive transform]
                      │                                                 │
                      └────────────┬──────────────────────────────────┬─┘
                                   │                                  │
                        ┌──────────▼──────────┐            ┌─────────▼──────┐
                        │ LOCAL CLIENT        │            │ REMOTE CLIENT   │
                        ├─────────────────────┤            ├─────────────────┤
                        │ Position updated    │            │ Position updated│
                        │ (from replication)  │            │ (from replication)
                        │                     │            │                 │
                        │ Plays animation     │            │ Plays animation │
                        │ (locally generated) │            │ (locally gen.)  │
                        │                     │            │                 │
                        │ [Animation Sync     │            │ ⚠️ MAY DIFFER   │
                        │  Should Match ✓]    │            │ from movement   │
                        │                     │            │ [Desync Risk!]  │
                        └─────────────────────┘            └─────────────────┘
```

---

## Component Relationship Diagram

```
┌────────────────────────────────────────────────────────────┐
│               PLAYER GAME OBJECT (Network)                 │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │ NetworkPlayerSync (NetworkBehaviour)                │ │
│  │ • Main network controller                           │ │
│  │ • Handles FixedUpdateNetwork()                      │ │
│  │ • Calls ThirdPersonController methods              │ │
│  │ • Manages state authority                          │ │
│  └────────────┬───────────────────╥────────────────────┘ │
│               │                   ║                       │
│               ├──►┌────────────────╨────────────────────┐ │
│               │   │ ThirdPersonController              │ │
│               │   │ • Movement speeds                  │ │
│               │   │ • Rotation settings               │ │
│               │   │ • Input handling (local mode)     │ │
│               │   │ • Animation controller            │ │
│               │   └────────────────────────────────────┘ │
│               │                                          │
│               ├──►┌────────────────────────────────────┐ │
│               │   │ CharacterController (Unity)       │ │
│               │   │ • Physics movement                │ │
│               │   │ • Collision detection             │ │
│               │   │ • Ground checking                 │ │
│               │   └────────────────────────────────────┘ │
│               │                                          │
│               ├──►┌────────────────────────────────────┐ │
│               │   │ NetworkHealthSync                 │ │
│               │   │ (Separate NetworkBehaviour)       │ │
│               │   │ • Health replication              │ │
│               │   │ • OnChangedRender callbacks       │ │
│               │   │ • UI updates                      │ │
│               │   └────────────────────────────────────┘ │
│               │                                          │
│               ├──►┌────────────────────────────────────┐ │
│               │   │ PlayerHealth                      │ │
│               │   │ • Local health state              │ │
│               │   │ • Invincibility frames            │ │
│               │   │ • Knockback handling              │ │
│               │   └────────────────────────────────────┘ │
│               │                                          │
│               ├──►┌────────────────────────────────────┐ │
│               │   │ AttackComboController             │ │
│               │   │ • Attack state management         │ │
│               │   │ • Combo logic                     │ │
│               │   └────────────────────────────────────┘ │
│               │                                          │
│               └──►┌────────────────────────────────────┐ │
│                   │ RollController                    │ │
│                   │ • Roll/dodge mechanics            │ │
│                   │ • Animation triggers              │ │
│                   └────────────────────────────────────┘ │
│                                                          │
└──────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│           SINGLETON MANAGERS (Not on Player)               │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌─────────────────┐  ┌──────────────────┐                │
│  │ NetworkManager  │  │ ThirdPersonCamera│                │
│  │ • Input Provider│  │ • Camera control │                │
│  │ • Player spawn/ │  │ • Yaw tracking   │                │
│  │   despawn       │  │ • Local player   │                │
│  │ • OnInput()     │  │   only           │                │
│  └─────────────────┘  └──────────────────┘                │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

---

## Synchronization Authority Model

```
┌─────────────────────────────────────────────────────┐
│          INPUT AUTHORITY vs STATE AUTHORITY          │
├─────────────────────────────────────────────────────┤
│                                                     │
│  LOCAL PLAYER                  REMOTE PLAYER       │
│  ┌─────────────────────┐       ┌─────────────────┐ │
│  │ Input Authority: ✓  │       │ Input Authority:│ │
│  │ (This Machine)      │       │      ✗ (No)     │ │
│  │                     │       │                 │ │
│  │ State Authority: ✗  │       │ State Authority:│ │
│  │ (Server has it)     │       │      ✗ (No)     │ │
│  │                     │       │                 │ │
│  │ CAN:                │       │ CAN:            │ │
│  │ • Send input        │       │ • Observe       │ │
│  │ • Control camera    │       │ • Render        │ │
│  │ • See local HUD     │       │ • Play anim.    │ │
│  │                     │       │                 │ │
│  │ CANNOT:             │       │ CANNOT:         │ │
│  │ • Modify state      │       │ • Send input    │ │
│  │ • Modify health     │       │ • Move self     │ │
│  │ • Spawn objects     │       │ • Modify health │ │
│  └─────────────────────┘       └─────────────────┘ │
│                                                     │
│  SERVER (HOST)                                      │
│  ┌───────────────────────────────────────────────┐ │
│  │ Input Authority: ✗ (Receives from players)   │ │
│  │ State Authority: ✓ (Authoritative)           │ │
│  │                                              │ │
│  │ RESPONSIBILITIES:                            │ │
│  │ • Process all input                          │ │
│  │ • Run movement physics                       │ │
│  │ • Modify health/state                        │ │
│  │ • Replicate to all clients                   │ │
│  │ • Resolve conflicts                          │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Known Issues Map

```
┌──────────────────────────────────────────────────────────┐
│              POTENTIAL BUG LOCATIONS                     │
└──────────────────────────────────────────────────────────┘

1. ANIMATION DESYNC                   [MEDIUM SEVERITY]
   Location: NetworkPlayerSync.UpdateAnimation()
   ┌────────────────────────────────────────────────┐
   │ animator.SetFloat("Speed", speed)              │
   │ animator.SetBool("IsMoving", isMoving)         │
   │                                                │
   │ ⚠️ Set locally on each client independently   │
   │ ⚠️ May not match actual network position      │
   │ ⚠️ Remote players see different animations    │
   └────────────────────────────────────────────────┘

2. CAMERA YAW FALLBACK                [MEDIUM SEVERITY]
   Location: NetworkPlayerSync.FixedUpdateNetwork(), line ~85
   ┌────────────────────────────────────────────────┐
   │ float cameraYaw = (_camera != null)            │
   │   ? _camera.GetCameraYaw()                     │
   │   : transform.eulerAngles.y;  ◄─── FALLBACK   │
   │                                                │
   │ ⚠️ _camera is NULL on server                  │
   │ ⚠️ transform.eulerAngles.y may not match       │
   │ ⚠️ Movement direction calculation error        │
   └────────────────────────────────────────────────┘

3. GRAVITY APPLICATION               [LOW SEVERITY]
   Location: NetworkPlayerSync.FixedUpdateNetwork(), last line
   ┌────────────────────────────────────────────────┐
   │ _cc.Move(Vector3.down * 9.81f * DeltaTime)   │
   │                                                │
   │ ⚠️ Applied every tick                         │
   │ ⚠️ Could be applied twice if not careful      │
   │ ⚠️ Physics inconsistency potential            │
   └────────────────────────────────────────────────┘

4. LEGACY CODE CONFLICT              [MEDIUM SEVERITY]
   Location: PlayerMovement.cs
   ┌────────────────────────────────────────────────┐
   │ Simple movement script in Scenes folder        │
   │                                                │
   │ ⚠️ May conflict with NetworkPlayerSync        │
   │ ⚠️ Verify if this is still in use             │
   │ ⚠️ Could cause jitter or unexpected behavior  │
   └────────────────────────────────────────────────┘

5. CHARACTER CONTROLLER RESET        [LOW SEVERITY]
   Location: NetworkCharacterController.Spawned()
   ┌────────────────────────────────────────────────┐
   │ _controller.enabled = false;                  │
   │ _controller.enabled = true;                   │
   │                                                │
   │ ⚠️ Could cause frame hitches                  │
   │ ⚠️ May reset internal position                │
   │ ⚠️ Could contribute to position jitter        │
   └────────────────────────────────────────────────┘
```

---

**Architecture Last Updated**: April 9, 2026
