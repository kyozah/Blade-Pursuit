# 🔗 ARCHITECTURE & FLOW DIAGRAMS

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      BLADE PURSUIT GAME                          │
└─────────────────────────────────────────────────────────────────┘
                             │
                ┌────────────┼────────────┐
                │            │            │
         ┌──────▼────────┐   │    ┌──────▼────────┐
         │  PlayerHealth │   │    │  BossHealth   │
         └──────┬────────┘   │    └──────┬────────┘
                │            │           │
           Die()│            │     OnDied│
                │            │           │
        ┌───────▼─────────────┴──────────┴────────────┐
        │                                              │
        │      DEATH MENU FLOW    VICTORY MENU FLOW    │
        │                                              │
        └───────┬──────────────────────────────────────┘
                │
      ┌─────────┴──────────┐
      │                    │
      │ Time.timeScale = 0 │
      │ (PAUSE GAME)       │
      │                    │
      └────────────────────┘
```

---

## Death Flow (Players lose)

```
Player takes damage
      │
      ▼
PlayerHealth.TakeDamage()
      │
      ├─ currentHealth <= 0?
      │
      ▼ YES
   Die()
      │
      ├─ isDead = true
      ├─ Stop all controls
      ├─ Play death animation
      ├─ Call HandleDeath()
      │
      ▼ (after deathDelay 3s)
      │
      ├─ Check if DeathMenuUI exists
      │
      └─▶ DeathMenuUI.ShowDeathMenu()
            │
            ├─ Time.timeScale = 0 (PAUSE)
            ├─ Fade in canvas (0.5s)
            ├─ Show buttons
            │
            ├──────────────────────┐
            │                      │
            ▼ Play clicked         ▼ Quit clicked
            │                      │
      Time.timeScale=1    Time.timeScale=1
      LoadScene("Gameplay")  Application.Quit()
            │                      │
            ▼                      ▼
      Fresh Gameplay          Game closes
      (all reset)
```

---

## Victory Flow (Players win)

```
Boss takes final damage
      │
      ▼
BossHealth.TakeDamage()
      │
      ├─ CurrentHP <= 0?
      │
      ▼ YES
      │
      ├─ OnDied?.Invoke()  ◄─── EVENT FIRED!
      ├─ Boss death animation
      ├─ Show chest reward
      │
      └─▶ VictoryMenuUI subscribes to OnDied
            │
            ├─ ShowVictoryMenu()
            │
            ├─ Time.timeScale = 0 (PAUSE)
            ├─ Fade in canvas (0.5s)
            ├─ Show "VICTORY!" + buttons
            │
            ├──────────────────────┐
            │                      │
            ▼ Continue             ▼ Quit
            │                      │
      Time.timeScale=1    Time.timeScale=1
      LoadScene("Menu")      Application.Quit()
            │                      │
            ▼                      ▼
      Back to main menu      Game closes
```

---

## Collision Kill Flow (Hazard)

```
Player collision with hazard object
      │
      ▼
OnTriggerEnter / OnCollisionEnter
      │
      ├─ Find PlayerHealth component
      │
      ▼ Found
      │
  PlayerKiller.KillPlayer()
      │
      ├─ TakeDamage(9999)
      │
      └─▶ PlayerHealth.TakeDamage()
            │
            └─▶ DEATH FLOW (see above)
                      │
                      ▼
                Death Menu appears
```

---

## Scene Transition Flow

```
┌──────────┐
│  MENU    │ Scene 0
│ Scene    │
└────┬─────┘
     │
     │ PlayGame()
     │ SceneManager.LoadScene("Gameplay")
     │
     ▼
┌──────────┐
│          │
│ GAMEPLAY │ Scene 1
│  Scene   │
│          │
└──────────┘
     │
     ├────────────────────┐
     │                    │
     │ Player Death        │ Boss Defeat
     │ (after 2s)         │ (immediate)
     │                    │
     ▼                    ▼
  ┌──────────────┐   ┌──────────────┐
  │ Death Menu   │   │ Victory Menu │
  │ [Play]       │   │ [Continue]   │
  │ [Quit]       │   │ [Quit]       │
  └──────┬───────┘   └──────┬───────┘
         │ Play clicked     │ Continue clicked
         │                  │
         └──────────┬───────┘
                    │
                    ▼
           ┌─────────────────┐
           │    GAMEPLAY     │ OR  MENU
           │  (reloaded)     │
           └─────────────────┘
```

---

## Component Hierarchy (Gameplay Scene)

```
Canvas (DeathMenuCanvas)
├── CanvasGroup (for fading)
├── DeathMenuUI (script)
├── Panel (dark background)
├── Text: "YOU DIED"
├── PlayButton
│   ├── Button component
│   └── TextMeshPro: "Play"
└── QuitButton
    ├── Button component
    └── TextMeshPro: "Quit"

Canvas (VictoryMenuCanvas)
├── CanvasGroup (for fading)
├── VictoryMenuUI (script)
├── Panel (victory background)
├── Text: "VICTORY!"
├── ContinueButton
│   ├── Button component
│   └── TextMeshPro: "Continue"
└── QuitButton
    ├── Button component
    └── TextMeshPro: "Quit"

Player
├── Collider
├── CharacterController
├── PlayerHealth (script)
├── ThirdPersonController
├── AttackComboController
└── ... other components

Boss
├── Collider
├── Animator
├── BossHealth (script)
├── BossBrain
└── ... other components

Hazard Objects (Lava, Spike, Pit)
├── Collider (set Is Trigger if needed)
└── PlayerKiller (script)
```

---

## Event/Callback Flow

```
PlayerHealth Class:
  ├── isDead boolean
  ├── Die() method
  └── IsDead() getter
       │
       └─ Used by DeathMenuUI.Update()
          to check player death state
          then show menu after 2s delay

BossHealth Class:
  ├── OnDied action event
  └── Invoked in TakeDamage()
      when CurrentHP <= 0
       │
       └─ VictoryMenuUI.Start()
          └─ subscribes: bossHealth.OnDied += ShowVictoryMenu
             └─ Callback called automatically
```

---

## Time Management (Pause/Resume)

```
Normal Gameplay:
  Time.timeScale = 1.0 (normal speed)
  All physics/animations run normally

Death Menu Shows:
  Time.timeScale = 0 (PAUSED)
  ├─ Physics frozen
  ├─ Animations frozen
  ├─ Coroutines still run (using unscaledDeltaTime)
  └─ UI fadeIn uses unscaledDeltaTime

Button Clicked:
  Time.timeScale = 1.0 (RESUME)
  └─ Before LoadScene or Application.Quit()

Victory Menu Shows:
  Time.timeScale = 0 (PAUSED)
  └─ Same as Death Menu
```

---

## File Organization

```
Assets/
└── Assets/
    ├── Character/
    │   └── Scripts/
    │       ├── PlayerHealth.cs (modified: already exists)
    │       ├── DeathMenuUI.cs (NEW)
    │       ├── VictoryMenuUI.cs (NEW)
    │       ├── PlayerKiller.cs (NEW)
    │       └── GameManager.cs (NEW - optional)
    │
    ├── Boss/
    │   └── Scripts/
    │       └── BossHealth.cs (already exists)
    │
    └── Scenes/
        ├── Menu.unity
        ├── Gameplay.unity
        └── menu.cs (modified: updated gameSceneName)
```

---

## Dependencies & Links

```
DeathMenuUI.cs
  ├─ Requires: PlayerHealth component in scene
  ├─ Requires: CanvasGroup on same GameObject
  ├─ Uses: UnityEngine.SceneManagement
  └─ Listens to: PlayerHealth.IsDead()

VictoryMenuUI.cs
  ├─ Requires: BossHealth component in scene
  ├─ Requires: CanvasGroup on same GameObject
  ├─ Uses: UnityEngine.SceneManagement
  └─ Listens to: BossHealth.OnDied event

PlayerKiller.cs
  ├─ Requires: Collider component
  ├─ Finds: PlayerHealth component (auto)
  ├─ Uses: OnTriggerEnter or OnCollisionEnter
  └─ Calls: PlayerHealth.TakeDamage()

GameManager.cs (optional)
  ├─ Singleton pattern
  ├─ Finds: PlayerHealth, DeathMenuUI, VictoryMenuUI
  ├─ Purpose: Centralized game state management
  └─ Not required for functionality
```

---

## Data Flow Summary

```
INPUT (Player)
  │
  ├─ Movement → ThirdPersonController
  ├─ Attack → AttackComboController
  └─ Collision → Physics engine
       │
       ▼
GAME LOGIC
  │
  ├─ Take Damage → PlayerHealth.TakeDamage()
  ├─ Boss Take Damage → BossHealth.TakeDamage()
  └─ Collision Hazard → PlayerKiller
       │
       ▼
STATE CHANGES
  │
  ├─ isDead = true
  ├─ OnDied?.Invoke()
  │
       │
       ▼
UI RESPONSE
  │
  ├─ DeathMenuUI.ShowDeathMenu()
  └─ VictoryMenuUI.ShowVictoryMenu()
       │
       ▼
OUTPUT (User sees)
  │
  ├─ Fade in menu
  ├─ Game paused
  └─ Buttons clickable
```

---

**Diagram-based documentation to understand the complete system architecture!** 📊

