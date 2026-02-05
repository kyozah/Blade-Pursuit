# Audio Assets Organization & Examples

## Recommended Folder Structure

```
Assets/
├── Audio/
│   ├── Master.mixer (Optional - for audio mixing)
│   ├── Player/
│   │   ├── Attacks/
│   │   │   ├── Slash1.mp3
│   │   │   ├── Slash2.mp3
│   │   │   ├── Slash3.mp3
│   │   │   └── SwingMetal.mp3
│   │   ├── Movement/
│   │   │   ├── Footstep_Wood1.mp3
│   │   │   ├── Footstep_Wood2.mp3
│   │   │   ├── Footstep_Grass.mp3
│   │   │   └── Run.mp3
│   │   ├── Damage/
│   │   │   ├── Pain1.mp3
│   │   │   ├── Pain2.mp3
│   │   │   └── GaspForAir.mp3
│   │   └── Death/
│   │       ├── Death_Scream.mp3
│   │       └── Fall.mp3
│   │
│   ├── Enemies/
│   │   ├── Skeleton/
│   │   │   ├── Attack/
│   │   │   │   ├── BoneCrack.mp3
│   │   │   │   ├── Slash.mp3
│   │   │   │   └── Rattle.mp3
│   │   │   ├── Movement/
│   │   │   │   ├── BoneFootstep1.mp3
│   │   │   │   ├── BoneFootstep2.mp3
│   │   │   │   └── BoneRattle.mp3
│   │   │   ├── Damage/
│   │   │   │   ├── BoneCrunch.mp3
│   │   │   │   └── BoneBreak.mp3
│   │   │   └── Death/
│   │   │       ├── BoneCollapse.mp3
│   │   │       └── Shatter.mp3
│   │   │
│   │   ├── Fly/
│   │   │   ├── Attack/
│   │   │   │   ├── Buzz_Attack.mp3
│   │   │   │   ├── Sting.mp3
│   │   │   │   └── Screech.mp3
│   │   │   ├── Movement/
│   │   │   │   ├── BuzzFly1.mp3
│   │   │   │   ├── BuzzFly2.mp3
│   │   │   │   └── WingFlutter.mp3
│   │   │   ├── Damage/
│   │   │   │   ├── FlyHurt.mp3
│   │   │   │   └── BuzzPain.mp3
│   │   │   └── Death/
│   │   │       ├── FlyDeath.mp3
│   │   │       └── BuzzDie.mp3
│   │   │
│   │   └── Tank/
│   │       ├── Attack/
│   │       │   ├── HeavySlam.mp3
│   │       │   ├── ShieldBash.mp3
│   │       │   └── GruntAttack.mp3
│   │       ├── Movement/
│   │       │   ├── HeavyFootstep1.mp3
│   │       │   ├── HeavyFootstep2.mp3
│   │       │   ├── Armor_Clank.mp3
│   │       │   └── Breathing.mp3
│   │       ├── Damage/
│   │       │   ├── TankGrunt.mp3
│   │       │   ├── MetalDing.mp3
│   │       │   └── Pain.mp3
│   │       └── Death/
│   │           ├── TankCrash.mp3
│   │           ├── FallHeavy.mp3
│   │           └── DeathGroan.mp3
│   │
│   └── UI/ (Optional)
│       ├── Button_Click.mp3
│       ├── Menu_Open.mp3
│       └── Menu_Close.mp3
```

## Audio Clip Naming Convention

**Format:** `[Type]_[Action]_[Variant].mp3`

**Examples:**
```
Player_Attack_Slash1.mp3
Player_Attack_Slash2.mp3
Player_Movement_Footstep_Wood.mp3
Player_Damage_Pain.mp3
Player_Death_Scream.mp3

Skeleton_Attack_Bone_Crack.mp3
Skeleton_Movement_Footstep_Bone.mp3
Skeleton_Damage_Impact.mp3
Skeleton_Death_Collapse.mp3

Fly_Attack_Sting.mp3
Fly_Movement_Buzz.mp3
Fly_Damage_Hurt.mp3
Fly_Death_Die.mp3

Tank_Attack_Slam.mp3
Tank_Movement_Footstep_Heavy.mp3
Tank_Damage_Grunt.mp3
Tank_Death_Crash.mp3
```

## AudioManager Configuration Examples

### Minimal Setup (16 clips total)

**Player Attack Sounds (1 clip):**
```
- Name: attack1
- Clip: Player_Attack_Slash1.mp3
- Volume: 0.8
- Pitch: 1.0
```

**Player Movement Sounds (1 clip):**
```
- Name: footstep
- Clip: Player_Movement_Footstep.mp3
- Volume: 0.5
- Pitch: 1.0
```

**Player Damage Sounds (1 clip):**
```
- Name: damage
- Clip: Player_Damage_Pain.mp3
- Volume: 0.7
- Pitch: 1.0
```

**Player Death Sounds (1 clip):**
```
- Name: death
- Clip: Player_Death_Scream.mp3
- Volume: 1.0
- Pitch: 1.0
```

*(Repeat for Skeleton, Fly, Tank)*

### Recommended Setup (40+ clips total)

**Player Attack Sounds (3 clips):**
```
- Name: slash1
  Clip: Player_Attack_Slash1.mp3
  Volume: 0.8
  Pitch: 1.0

- Name: slash2
  Clip: Player_Attack_Slash2.mp3
  Volume: 0.8
  Pitch: 0.95

- Name: slash3
  Clip: Player_Attack_Slash3.mp3
  Volume: 0.8
  Pitch: 1.05
```

**Player Movement Sounds (3 clips):**
```
- Name: step1
  Clip: Player_Movement_Footstep_Wood1.mp3
  Volume: 0.5
  Pitch: 1.0

- Name: step2
  Clip: Player_Movement_Footstep_Wood2.mp3
  Volume: 0.5
  Pitch: 0.98

- Name: run
  Clip: Player_Movement_Run.mp3
  Volume: 0.6
  Pitch: 1.0
```

**Player Damage Sounds (2 clips):**
```
- Name: pain1
  Clip: Player_Damage_Pain1.mp3
  Volume: 0.7
  Pitch: 1.0

- Name: pain2
  Clip: Player_Damage_Pain2.mp3
  Volume: 0.7
  Pitch: 0.95
```

**Player Death Sounds (1 clip):**
```
- Name: death
  Clip: Player_Death_Scream.mp3
  Volume: 1.0
  Pitch: 1.0
```

### Skeleton Setup (Recommended)

**Attack Sounds (3 clips):**
```
- Name: boneCrack
  Clip: Skeleton_Attack_Bone_Crack.mp3
  Volume: 0.75
  Pitch: 1.0

- Name: slash
  Clip: Skeleton_Attack_Slash.mp3
  Volume: 0.7
  Pitch: 1.0

- Name: rattle
  Clip: Skeleton_Attack_Rattle.mp3
  Volume: 0.6
  Pitch: 1.0
```

**Movement Sounds (3 clips):**
```
- Name: step1
  Clip: Skeleton_Movement_BoneFootstep1.mp3
  Volume: 0.4
  Pitch: 1.0

- Name: step2
  Clip: Skeleton_Movement_BoneFootstep2.mp3
  Volume: 0.4
  Pitch: 0.95

- Name: rattle
  Clip: Skeleton_Movement_BoneRattle.mp3
  Volume: 0.3
  Pitch: 1.0
```

**Damage Sounds (2 clips):**
```
- Name: crunch1
  Clip: Skeleton_Damage_Crunch.mp3
  Volume: 0.65
  Pitch: 1.0

- Name: crunch2
  Clip: Skeleton_Damage_Break.mp3
  Volume: 0.65
  Pitch: 0.9
```

**Death Sounds (1 clip):**
```
- Name: collapse
  Clip: Skeleton_Death_Collapse.mp3
  Volume: 0.8
  Pitch: 1.0
```

### Fly Setup (Recommended)

**Attack Sounds (3 clips):**
```
- Name: buzzAttack
  Clip: Fly_Attack_Buzz_Attack.mp3
  Volume: 0.7
  Pitch: 1.0

- Name: sting
  Clip: Fly_Attack_Sting.mp3
  Volume: 0.75
  Pitch: 1.0

- Name: screech
  Clip: Fly_Attack_Screech.mp3
  Volume: 0.65
  Pitch: 1.0
```

**Movement Sounds (3 clips):**
```
- Name: buzz1
  Clip: Fly_Movement_Buzz1.mp3
  Volume: 0.4
  Pitch: 1.0

- Name: buzz2
  Clip: Fly_Movement_Buzz2.mp3
  Volume: 0.4
  Pitch: 1.05

- Name: flutter
  Clip: Fly_Movement_Flutter.mp3
  Volume: 0.35
  Pitch: 1.0
```

**Damage Sounds (2 clips):**
```
- Name: hurt1
  Clip: Fly_Damage_Hurt.mp3
  Volume: 0.6
  Pitch: 1.0

- Name: hurt2
  Clip: Fly_Damage_BuzzPain.mp3
  Volume: 0.55
  Pitch: 0.95
```

**Death Sounds (1 clip):**
```
- Name: death
  Clip: Fly_Death_Die.mp3
  Volume: 0.8
  Pitch: 1.0
```

### Tank Setup (Recommended)

**Attack Sounds (3 clips):**
```
- Name: slam
  Clip: Tank_Attack_Slam.mp3
  Volume: 0.9
  Pitch: 1.0

- Name: bash
  Clip: Tank_Attack_ShieldBash.mp3
  Volume: 0.85
  Pitch: 0.95

- Name: grunt
  Clip: Tank_Attack_Grunt.mp3
  Volume: 0.7
  Pitch: 1.0
```

**Movement Sounds (4 clips):**
```
- Name: step1
  Clip: Tank_Movement_HeavyFootstep1.mp3
  Volume: 0.6
  Pitch: 1.0

- Name: step2
  Clip: Tank_Movement_HeavyFootstep2.mp3
  Volume: 0.6
  Pitch: 0.95

- Name: clank
  Clip: Tank_Movement_Armor_Clank.mp3
  Volume: 0.4
  Pitch: 1.0

- Name: breath
  Clip: Tank_Movement_Breathing.mp3
  Volume: 0.3
  Pitch: 1.0
```

**Damage Sounds (3 clips):**
```
- Name: grunt1
  Clip: Tank_Damage_Grunt.mp3
  Volume: 0.75
  Pitch: 1.0

- Name: ding
  Clip: Tank_Damage_MetalDing.mp3
  Volume: 0.7
  Pitch: 1.0

- Name: pain
  Clip: Tank_Damage_Pain.mp3
  Volume: 0.65
  Pitch: 0.9
```

**Death Sounds (2 clips):**
```
- Name: crash
  Clip: Tank_Death_Crash.mp3
  Volume: 1.0
  Pitch: 0.9

- Name: groan
  Clip: Tank_Death_Groan.mp3
  Volume: 0.85
  Pitch: 1.0
```

## Audio Clip Format Specifications

**Recommended Format:**
- Format: MP3 or OGG Vorbis
- Bitrate: 128-192 kbps
- Sample Rate: 44100 Hz or 48000 Hz
- Channels: Mono (for positional sounds) or Stereo (for ambient)

**Unity Import Settings:**
- Compression Format: Vorbis (for size) or PCM (for quality)
- Load Type: Compressed in Memory (default)
- Preload Audio Data: OFF (for large files)

## Pitch Variation Table

Use these pitch values for natural variation:

```
Standard Pitch: 1.0

Low Variation (±0.05):
- 0.95, 1.0, 1.05

Medium Variation (±0.1):
- 0.90, 0.95, 1.0, 1.05, 1.10

High Variation (±0.15):
- 0.85, 0.90, 0.95, 1.0, 1.05, 1.10, 1.15
```

## Integration with AudioManager

All clips should be referenced in AudioManager.cs through the Inspector:

```
AudioManager Component:
├── Player Attack Sounds (Size: 3)
├── Player Movement Sounds (Size: 3)
├── Player Damage Sounds (Size: 2)
├── Player Death Sounds (Size: 1)
├── Skeleton Attack Sounds (Size: 3)
├── Skeleton Movement Sounds (Size: 3)
├── Skeleton Damage Sounds (Size: 2)
├── Skeleton Death Sounds (Size: 1)
├── Fly Attack Sounds (Size: 3)
├── Fly Movement Sounds (Size: 3)
├── Fly Damage Sounds (Size: 2)
├── Fly Death Sounds (Size: 1)
├── Tank Attack Sounds (Size: 3)
├── Tank Movement Sounds (Size: 4)
├── Tank Damage Sounds (Size: 3)
└── Tank Death Sounds (Size: 2)
```

## Total Audio Counts

- **Minimal:** 16 clips (1 per sound type)
- **Basic:** 24 clips (2-3 per sound type)
- **Recommended:** 40+ clips (3-4 per sound type with variations)
- **Full:** 50+ clips (comprehensive variety)

## Free Audio Resources

Consider finding sounds from:
- Freesound.org
- Zapsplat.com
- BBC Sound Effects Library
- Mixkit.co
- OpenGameArt.org

Remember to check licensing before using!
