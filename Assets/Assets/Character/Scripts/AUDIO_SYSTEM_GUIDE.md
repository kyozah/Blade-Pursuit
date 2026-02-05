# Audio System Documentation

## Overview

This audio system provides comprehensive sound effects for both the player and three types of enemies (Skeleton, Fly, Tank). The system is designed to play sounds in parallel with animations, creating immersive audio feedback.

## Components

### 1. **AudioManager.cs**
Central audio management system that handles all sound effects.

**Features:**
- Singleton pattern for global access
- Support for multiple sound lists (player and each enemy type)
- Spatial audio (3D sound positioning)
- Random sound variation from sound lists
- Configurable volume and pitch

**Sound Lists:**
- **Player:**
  - `playerAttackSounds` - Attack/slash sounds
  - `playerMovementSounds` - Footstep/running sounds
  - `playerDamageSounds` - Damage/pain sounds
  - `playerDeathSounds` - Death sounds

- **Skeleton:**
  - `skeletonAttackSounds`
  - `skeletonMovementSounds`
  - `skeletonDamageSounds`
  - `skeletonDeathSounds`

- **Fly:**
  - `flyAttackSounds`
  - `flyMovementSounds`
  - `flyDamageSounds`
  - `flyDeathSounds`

- **Tank:**
  - `tankAttackSounds`
  - `tankMovementSounds`
  - `tankDamageSounds`
  - `tankDeathSounds`

### 2. **PlayerAudioSystem.cs**
Manages all player audio effects.

**Features:**
- Automatic creation of audio sources if not assigned
- Attack sound triggered by `AttackComboController.OnAttackStart` event
- Movement sound detection and playback
- Damage sound playback when taking damage
- Death sound playback when dying
- Optional pitch variation for variety

**Audio Sources Used:**
- `attackAudioSource` - For slash/attack sounds
- `movementAudioSource` - For footsteps
- `damageAudioSource` - For damage feedback
- `deathAudioSource` - For death sound

**Settings:**
- `useRandomPitch` - Enable pitch variation
- `pitchVariation` - Range of pitch variation (default: 0.1)

### 3. **EnemyAudioSystem.cs**
Manages all enemy audio effects.

**Features:**
- Support for three enemy types: Skeleton, Fly, Tank
- Automatic enemy type detection and sound assignment
- Attack sound triggered when enemy attacks
- Movement sound played periodically while moving
- Damage sound played when taking damage
- Death sound played when dying
- Optional pitch variation

**Setup:**
```
Inspector Setup:
1. Add "EnemyAudioSystem" component to enemy GameObject
2. Set "Enemy Type" to appropriate value (Skeleton, Fly, or Tank)
3. Audio sources are auto-created if not assigned
```

**Settings:**
- `enemyType` - Type of enemy (Skeleton, Fly, Tank)
- `useRandomPitch` - Enable pitch variation
- `pitchVariation` - Range of pitch variation (default: 0.15)
- `movementSoundInterval` - Time between movement sound plays (default: 0.5s)

## Integration Points

### Player Integration

#### In AttackComboController.cs
- Audio plays automatically via `OnAttackStart` event
- Attack sound plays as combo begins

#### In PlayerHealth.cs
- **Damage Sound:** Plays in `TakeDamage()` method
- **Death Sound:** Plays in `Die()` method
- Both sounds play parallel with animations

#### In ThirdPersonController.cs
- **Movement Sound:** Detected by `PlayerAudioSystem.Update()`
- Plays when player moves, stops when idle

### Enemy Integration

#### In Enemy.cs (Base Class)
- **Attack Sound:** Plays in `StartAttack()` method
- **Damage Sound:** Plays in `TakeDamage()` method when hit
- **Death Sound:** Plays in `Die()` method
- All sounds play parallel with animations

#### In Skeleton, Fly, Tank Classes
- Inherit audio from Enemy base class
- Each type can have unique sounds via AudioManager

## Usage

### Setting Up Audio Manager

1. **Create Empty GameObject** in your scene
2. **Add AudioManager.cs** component
3. **Configure Sound Lists** in Inspector:
   - Drag audio clips into appropriate lists
   - Set volume levels (0-1)
   - Set pitch values
   - Enable loop if needed

### Example Setup

```
Assets/
├── Audio/
│   ├── Player/
│   │   ├── Attack1.mp3
│   │   ├── Attack2.mp3
│   │   ├── Footstep.mp3
│   │   ├── Damage.mp3
│   │   └── Death.mp3
│   ├── Skeleton/
│   │   ├── Attack.mp3
│   │   ├── Move.mp3
│   │   ├── Hit.mp3
│   │   └── Die.mp3
│   ├── Fly/
│   │   ├── Buzz.mp3
│   │   ├── Flutter.mp3
│   │   ├── Screech.mp3
│   │   └── Death.mp3
│   └── Tank/
│       ├── Stomp.mp3
│       ├── Walk.mp3
│       ├── Impact.mp3
│       └── Crash.mp3
```

### Code Example: Playing Sounds

**From PlayerAudioSystem:**
```csharp
public void PlayAttackSound()
{
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayRandomSoundOnSource(
            attackAudioSource, 
            AudioManager.Instance.playerAttackSounds
        );
    }
}
```

**From EnemyAudioSystem:**
```csharp
public void PlayAttackSound()
{
    if (AudioManager.Instance == null) return;
    
    var soundList = GetAttackSoundList(); // Returns appropriate list based on enemy type
    if (soundList != null && soundList.Count > 0)
    {
        AudioManager.Instance.PlayRandomSoundOnSource(attackAudioSource, soundList);
    }
}
```

## Audio Synchronization

All audio effects play **parallel with animations**:

### Player
- Attack sound triggers with combo start (animation begins)
- Movement sound detects motion and plays
- Damage sound plays immediately when impact animation triggers
- Death sound plays when death animation starts

### Enemies
- Attack sound plays when `StartAttack()` is called (animation begins)
- Movement sound plays periodically while moving
- Damage sound plays when `Hit` trigger is set (animation plays)
- Death sound plays when `IsDead` bool is set (animation plays)

## Sound List Configuration

### Creating a Sound List

In the Inspector:
1. Select the element count in the list
2. For each element:
   - **Name:** Identifier for the sound (e.g., "slash1", "footstep")
   - **Clip:** AudioClip asset
   - **Volume:** 0-1 range
   - **Pitch:** Playback speed (1 = normal, 0.5 = half speed, 2 = double speed)
   - **Loop:** Whether to loop the sound

### Recommended Volumes

- **Attacks:** 0.7-1.0
- **Movement:** 0.3-0.6
- **Damage:** 0.5-0.8
- **Death:** 0.8-1.0

## Troubleshooting

### Sounds Not Playing

1. **Check AudioManager:**
   - Is it in the scene?
   - Are sound clips properly assigned?
   - Check console for warnings

2. **Check Audio Sources:**
   - Are they created automatically? Check in Play mode
   - Verify audio source settings

3. **Enable Debug:**
   - Check console logs (e.g., "🔊 Player Attack Sound")

### Volume Issues

- Adjust volume in AudioManager.SoundEffect list
- Check Game volume in Audio Settings
- Verify spatialBlend settings (0 = 2D, 1 = 3D)

### Spatial Audio Not Working

- Ensure AudioListener is in scene (attached to camera)
- Check `spatialBlend` value (should be > 0 for 3D audio)
- Verify audio sources have Min/Max Distance set properly

## Performance Considerations

- Sounds are destroyed after playback (no memory leak)
- Spatial audio creates temporary GameObjects
- Movement sounds are rate-limited via `movementSoundInterval`
- Consider pooling audio sources if many sounds play simultaneously

## Future Enhancements

- Audio pooling system for better performance
- Dynamic volume mixing
- Sound effect filtering based on game state
- Music system integration
- Voiceover support

## Credits

Audio system created for Blade Pursuit game.
Uses Unity's AudioSource and AudioClip system with spatial audio capabilities.
