# Boss UI Setup Guide

This document explains how the boss UI system works and shows you how to configure it for two separate bosses (or more) in the scene.  It also includes tips for splitting the UI into distinct instances and improving the code.

---

## Overview

The game uses a single `BossUIManager` singleton (`Assets/Boss/Scripts/BossUIManager.cs`) to manage one or more `BossHealthScreenUI` objects.  Each UI instance is a screen‑space prefab or canvas containing the name, icon and health bar for a boss.  When a boss is spawned or detected, the manager binds the correct `BossHealth` component to one of the UI slots and plays the intro animation.

The architecture already supports multiple UI slots.  To show two different bosses with unique HUDs, simply assign two (or more) UI objects in the inspector and the manager will pick an unused one automatically.

---

## Step‑by‑Step Setup

1. **Create / Duplicate UI Prefab**
   * Design a `BossHealthScreenUI` prefab for your first boss (`BossA_UI.prefab`).
   * Duplicate it for the second boss (`BossB_UI.prefab`).  You can customise colours, art, layout, etc.

2. **Place UI Instances in Scene**
   * Add both prefabs to your UI canvas or scene hierarchy.  Make sure each one has a `BossHealthScreenUI` component attached.
   * Optionally disable them by default; the manager will enable them when a boss appears.

3. **Assign to Manager**
   * Select the object that contains the `BossUIManager` component (typically an empty GameObject in a UI folder).
   * In the inspector, expand the `bossUIs` array and set its size to `2`.
   * Drag the first UI instance into element 0 and the second into element 1.  The order doesn’t matter unless you use explicit indexing (see below).

4. **Configure BossBrain (optional)**
   * If you want a specific boss to always use a given UI slot, add an integer field to `BossBrain` (e.g. `public int bossUIIndex`) and set it per‑boss in the inspector.
   * When `ShowBoss` is called, `BossUIManager` will prefer that index if it’s valid.

5. **Triggering the UI**
   * Call `BossUIManager.Instance.ShowBoss(myBossBrain);` from your boss logic when the player is detected.
   * You can pass optional overrides for name, icon sprite, or health bar sprite.

6. **Customising at Runtime**
   * Use `SetBossDisplayName`, `SetBossDisplaySprite`, and `SetBossHealthBarSprite` helper methods to update a boss’s UI stay in sync with dynamic changes.
   * Call the corresponding `Clear` methods to remove overrides.

7. **Cleaning Up**
   * The manager automatically unbinds a UI slot when the bound boss dies (`OnDied` event).  No manual cleanup is required.

---

## Example: Two Bosses Setup

```csharp
// BossABrain.cs
public class BossA : BossBrain
{
    void Awake()
    {
        bossUIIndex = 0;           // always use the first slot
        BossUIManager.Instance.ShowBoss(this);
    }
}

// BossBBrain.cs
public class BossB : BossBrain
{
    void Awake()
    {
        bossUIIndex = 1;           // always use the second slot
        BossUIManager.Instance.ShowBoss(this, "Vanguard", someSprite);
    }
}
```

With the above code each boss will display in its own UI object.  You can still rely on auto‑assignment by omitting `bossUIIndex` or setting it to -1.

---

## Code Improvement Suggestions

* **Separate manager for each boss type** – if the two bosses have drastically different UI layouts, consider creating two subclasses of `BossHealthScreenUI` and two manager components.  The current `BossUIManager` already supports multiple slots but all slots share common behaviour.
* **Caching and reuse** – the `GetOrCreateUIForBoss` method currently chooses the first unused slot; you may want a more deterministic mapping (e.g. using a dictionary keyed by boss type or prefab).
* **Editor tools** – write a custom inspector for `BossUIManager` that validates the array size and warns if slots look identical.  You could also add a button to auto‑populate using children.
* **Event unsubscription** – the lambda assigned to `h.OnDied` does not remove itself; consider using a named method and unsubscribing on disable to avoid potential memory leaks in long scenarios.

---

## Troubleshooting

- **UI not showing**: verify that the `bossUIs` array is populated and that each element contains a valid `BossHealthScreenUI` component.  Check for console errors from `ShowBoss`.
- **Wrong UI reused**: if you see one slot repeatedly showing multiple bosses, ensure the array size matches the number of active bosses and clear `boundUIs` when restarting scenes.
- **Null `BossHealth`**: `ShowBoss` expects the boss brain to have a `BossHealth` child.  Add one or adjust your prefab hierarchy accordingly.

---

This document should give you the foundation to extend the boss UI system with two separate UIs or more.  Feel free to update it as the code evolves.