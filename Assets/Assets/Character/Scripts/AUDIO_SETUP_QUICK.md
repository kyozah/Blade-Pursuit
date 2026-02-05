# Audio System Quick Setup Guide

## Step-by-Step Setup

### Step 1: Create AudioManager in Your Scene

1. Create a new **Empty GameObject** in your scene (right-click in Hierarchy → Create Empty)
2. Name it `AudioManager`
3. Add component: `AudioManager.cs`

### Step 2: Configure AudioManager

In the Inspector for AudioManager component:

#### Player Attack Sounds
1. Set size to 1 or more
2. For each attack sound:
   - Name: `attack1`, `attack2`, `slash1`, etc.
   - Clip: Select your attack audio clip
   - Volume: 0.8
   - Pitch: 1.0

#### Player Movement Sounds
1. Set size to 1 or more
2. For each movement sound:
   - Name: `footstep1`, `footstep2`, `run`, etc.
   - Clip: Select your footstep audio clip
   - Volume: 0.5
   - Pitch: 1.0

#### Player Damage Sounds
1. Set size to 1 or more
2. For each damage sound:
   - Name: `hurt1`, `ouch`, `impact`, etc.
   - Clip: Select your pain audio clip
   - Volume: 0.7
   - Pitch: 1.0

#### Player Death Sounds
1. Set size to 1 or more
2. For each death sound:
   - Name: `death1`, `death2`, etc.
   - Clip: Select your death audio clip
   - Volume: 1.0
   - Pitch: 1.0

### Step 3: Configure Enemy Sounds

Repeat the same process for:
- **Skeleton:** skeletonAttackSounds, skeletonMovementSounds, skeletonDamageSounds, skeletonDeathSounds
- **Fly:** flyAttackSounds, flyMovementSounds, flyDamageSounds, flyDeathSounds
- **Tank:** tankAttackSounds, tankMovementSounds, tankDamageSounds, tankDeathSounds

### Step 4: Add Components to Player

1. Select your Player GameObject
2. Add Component: `PlayerAudioSystem.cs`
3. Configure (optional):
   - Toggle `useRandomPitch` for variety
   - Adjust `pitchVariation` (0.1 is default)

### Step 5: Add Components to Enemies

For each enemy type (Skeleton, Fly, Tank):

1. Select the enemy GameObject
2. Add Component: `EnemyAudioSystem.cs`
3. Set `Enemy Type` to appropriate value (Skeleton/Fly/Tank)
4. Configure (optional):
   - Toggle `useRandomPitch` for variety
   - Adjust `pitchVariation`
   - Adjust `movementSoundInterval`

### Step 6: Test

1. Press **Play** in Unity
2. Listen for:
   - Player attack sounds when hitting enemies
   - Player footsteps when moving
   - Enemy sounds when they move and attack
   - Damage and death sounds for all characters

## Minimal Audio Clips Needed

**Minimum setup (1 clip per sound type):**
- 1 player attack sound
- 1 player footstep
- 1 player damage sound
- 1 player death sound
- 1 skeleton attack sound
- 1 skeleton movement sound
- 1 skeleton damage sound
- 1 skeleton death sound
- 1 fly attack sound
- 1 fly movement sound
- 1 fly damage sound
- 1 fly death sound
- 1 tank attack sound
- 1 tank movement sound
- 1 tank damage sound
- 1 tank death sound

**Total: 16 minimum audio clips**

## Recommended Audio Clips

**Better setup (multiple variations):**
- 2-3 player attack sounds (for combo variety)
- 2-3 player footsteps (for randomness)
- 2 player damage sounds
- 1 player death sound
- Similar for each enemy type

**Total: 40-50 audio clips for variety**

## Audio Settings by Type

### Attack Sounds
- Volume: 0.7-1.0
- Pitch: 0.95-1.05 (slight variation)
- Loop: OFF

### Movement Sounds
- Volume: 0.3-0.6
- Pitch: 0.95-1.05
- Loop: OFF
- Duration: 0.3-0.8 seconds

### Damage Sounds
- Volume: 0.5-0.8
- Pitch: 0.9-1.1
- Loop: OFF

### Death Sounds
- Volume: 0.8-1.0
- Pitch: 0.8-1.0
- Loop: OFF
- Duration: 1-3 seconds

## Script Integration Summary

### Automatic Audio Triggers

**Player:**
- ✅ Attack: Triggered in `AttackComboController.OnAttackStart` event
- ✅ Movement: Detected by motion check in `PlayerAudioSystem.Update()`
- ✅ Damage: Called in `PlayerHealth.TakeDamage()`
- ✅ Death: Called in `PlayerHealth.Die()`

**Enemies (Skeleton, Fly, Tank):**
- ✅ Attack: Triggered in `Enemy.StartAttack()`
- ✅ Movement: Detected periodically in `EnemyAudioSystem.Update()`
- ✅ Damage: Called in `Enemy.TakeDamage()`
- ✅ Death: Called in `Enemy.Die()`

**No additional code changes needed!**

## Debugging

### Enable Debug Logs
1. Open AudioManager in inspector
2. All systems log with 🔊 emoji when sounds play

### Check Console for Messages
- "🔊 Player Attack Sound" - Attack triggered
- "🔊 Player Movement Sound" - Movement detected
- "🔊 Player Damage Sound" - Damage taken
- "🔊 Player Death Sound" - Player died
- "🔊 Skeleton Audio System initialized" - Enemy setup complete

### Audio Sources Not Creating
1. Check that AudioManager exists in scene
2. Verify AudioManager script is attached
3. Check console for any null reference errors

## Performance Tips

1. **Use Appropriate Compression:**
   - Import settings for audio clips
   - Use Vorbis compression for most sounds
   - AAC for mobile

2. **Audio Length:**
   - Keep attack sounds 0.5-1 second
   - Keep footsteps 0.3-0.5 seconds
   - Allow death sounds to be longer

3. **Volume Levels:**
   - Properly balance volumes in AudioManager
   - Avoid clipping (volume > 1.0)
   - Test with headphones and speakers

## Common Issues & Solutions

### No Sound Playing
- [ ] Check AudioManager exists in scene
- [ ] Verify AudioManager.cs is attached
- [ ] Check volume levels (not set to 0)
- [ ] Verify audio clips are imported correctly
- [ ] Check Game window Volume slider

### Sound is Too Loud/Quiet
- [ ] Adjust volume in AudioManager.SoundEffect
- [ ] Check if pitch is affecting playback
- [ ] Verify audio clip itself isn't too quiet/loud

### Only One Sound Plays
- [ ] Check if size of sound list is > 1
- [ ] Verify multiple clips are assigned
- [ ] Check useRandomPitch setting

### Spatial Audio Not Working
- [ ] Verify scene has AudioListener (usually on Main Camera)
- [ ] Check spatialBlend values in AudioManager
- [ ] Position enemies far enough from player

## Next Steps

1. ✅ Add AudioManager to scene
2. ✅ Create/import audio clips
3. ✅ Configure sound lists in AudioManager
4. ✅ Add PlayerAudioSystem to Player
5. ✅ Add EnemyAudioSystem to each enemy
6. ✅ Test in Play mode
7. ✅ Adjust volumes and timings
8. ✅ Add more audio variations as desired

## Support

For detailed documentation, see: `AUDIO_SYSTEM_GUIDE.md`
