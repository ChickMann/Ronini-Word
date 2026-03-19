# ⚔️ RONIN WORD — Learn Japanese Through Combat

<div align="center">

[![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)](https://play.google.com/store/apps/details?id=com.chickman.roninword)
[![Version](https://img.shields.io/badge/Version-1.9.2-blue?style=for-the-badge)](https://play.google.com/store/apps/details?id=com.chickman.roninword)
[![Rating](https://img.shields.io/badge/Rating-⭐%205.0%2F5-yellow?style=for-the-badge)](https://play.google.com/store/apps/details?id=com.chickman.roninword)
[![Engine](https://img.shields.io/badge/Engine-Unity-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com)
[![Genre](https://img.shields.io/badge/Genre-Rhythm%20Combat%20%7C%20Educational-purple?style=for-the-badge)](#)

<br/>

> **Become a Ronin. Learn Japanese. Fight to survive.**
>
> *No boring flashcards. No dry textbook lessons.*
> *Every vocabulary word is a sword strike.*

<br/>

[![Google Play](https://img.shields.io/badge/Google_Play-Download_Free-3DDC84?style=for-the-badge&logo=google-play&logoColor=white)](https://play.google.com/store/apps/details?id=com.chickman.roninword)
[![Facebook Fanpage](https://img.shields.io/badge/Facebook-RONIN_WORD-1877F2?style=for-the-badge&logo=facebook&logoColor=white)](https://www.facebook.com/RONINWORD)

</div>

---

## 🎮 About

**Ronin Word** is a unique 2D action-RPG for Android where you play as a lone Ronin swordsman conquering waves of enemies using the power of **Japanese vocabulary**.

Instead of tapping a regular attack button, every sword strike is powered by **correctly selecting Hiragana/Katakana characters** to spell out a vocabulary word. Right answer — your blade rings, the enemy gets knocked back. Wrong answer — you take the hit.

**Learning Japanese has never been this exciting or nerve-wracking.**

---

## ✨ Key Features

| Feature | Description |
|---------|-------------|
| ⚔️ **Combat-based Learning** | Spell words = deal damage. Forget vocabulary = lose HP. |
| ⚡ **Enemy Tension Gauge** | Enemies charge up in real-time. React before they unleash their attack. |
| 💡 **Finisher System** | Complete a word perfectly → enter **FOCUS** state → unleash a cinematic killing blow. |
| 🎬 **Cinematic Action** | Camera shake, parallax backgrounds, slash VFX, and Timeline-driven cutscenes. |
| 📊 **Global Leaderboard** | Compete for high scores with players worldwide via Firebase. |
| 🔐 **Google Sign-In** | Save progress and your player profile securely. |
| 📈 **Enemy Variety** | Regular soldiers → elite fighters → Bosses, each with distinct mechanics. |

---

## 🕹️ Core Gameplay Loop

```
[Encounter Enemy] → [Vocab question appears] → [Select correct characters in order]
        ↓                                                   ↓
[Tension Gauge drains in real-time]             [Correct] → Parry + Enemy Knockback + Gauge reset
[Gauge hits 0 = enemy attacks!]                 [Wrong]  → Player takes damage + Screen flash
                                                            ↓
                                        [All words spelled] → Enemy BrokenStand (stunned)
                                                            ↓
                                              [FOCUS MODE — Final Vocab]
                                                            ↓
                                              [Spell it out to finish the fight]
                                                            ↓
                                               ⚔️ Cinematic Killing Blow → Enemy is DEAD
                                                            ↓
                                                    [Next enemy spawns]
```

> ❌ **Enemies never recover.** Every enemy is destroyed the moment the finisher lands.
> If the player runs out of HP before finishing — it's **Game Over**.

### 🗡️ Player Combat States

- **🟢 Healthy (HP ≥ 2)** → Normal stance — parries and fights at full strength
- **🟡 Critical (HP = 1)** → BrokenStand — wounded posture, still fighting but one hit away from death
- **🔴 Dead (HP = 0)** → Game Over — the enemy lands the finishing blow

---

## 🏗️ Technical Architecture

Built with **Unity (URP — Universal Render Pipeline)** and a clean, modular code structure:

```
Assets/
├── Scripts/
│   ├── ControlManager/           # Core gameplay systems
│   │   ├── CombatManager.cs      # Combat state machine
│   │   ├── PlayerController.cs   # Player character control
│   │   ├── EnemyController.cs    # Enemy AI & behavior
│   │   ├── UIManager.cs          # In-game UI management
│   │   └── InputDisplayManager.cs # Hexagonal Hiragana keyboard
│   ├── DatabaseManager/          # Firebase & cloud backend
│   │   ├── GameDataManager.cs    # Game data management
│   │   ├── LeaderboardController.cs  # Global leaderboard
│   │   ├── LoginWithGoogle.cs    # Google Sign-In integration
│   │   └── VocabFirebaseManager.cs   # Cloud vocabulary sync
│   ├── ScriptableObject/         # Pure data assets
│   │   ├── VocabData.cs          # Vocabulary data (Smart Auto-ID)
│   │   ├── EnemyProfile.cs       # Enemy configuration
│   │   └── LevelData.cs          # Stage wave configuration
│   ├── Actor/                    # Base actor classes
│   └── SoundManager/             # Audio system
└── ...
```

### 🔧 Tech Stack

- **Unity** (URP, Cinemachine, Timeline, TextMesh Pro)
- **Firebase** (Realtime Database, Authentication)
- **Google Sign-In**
- **Google Play In-App Update**
- **2D Animation + Sprite Shapes**
- **Unity Input System**

---

## 📱 Release Information

| Info | Details |
|------|---------|
| 🏷️ **Game Title** | Ronin Word |
| 🏢 **Developer** | ChickMan |
| 📅 **Release Date** | February 21, 2026 |
| 🔄 **Latest Version** | 1.9.2 |
| 📱 **Requirements** | Android 7.1+ |
| 🎯 **Category** | Educational / Rhythm Combat |
| ⭐ **Rating** | 5.0 / 5 |
| 💰 **Price** | Free |

---

## 🌟 Gameplay Highlights

### ⚡ Enemy Tension Gauge
While you're thinking, the enemy is charging. You must tap the correct character **before the gauge fills** — otherwise you take a hit instantly. This mechanic turns vocabulary recall into pure reflex.

### 🎯 Hexagonal Input Keyboard
Hiragana/Katakana characters are laid out in a honeycomb grid. The unique layout helps build character recognition faster, and makes input feel intuitive and tactile.

### 🎬 Cinematic Combat
Every fight ends with a cinematically-driven finishing blow — camera shake, high-speed dash, and a Timeline-animated killing sequence.

---

## 🤝 Community & Contact

<div align="center">

[![Google Play](https://img.shields.io/badge/📲_Google_Play-Download_Now-3DDC84?style=for-the-badge)](https://play.google.com/store/apps/details?id=com.chickman.roninword)
[![Facebook](https://img.shields.io/badge/📣_Facebook-RONIN_WORD_Fanpage-1877F2?style=for-the-badge)](https://www.facebook.com/RONINWORD)

Follow the fanpage for the latest updates, community events, and behind-the-scenes development content!

</div>

---

<div align="center">

**Made with ❤️ by ChickMan**

*"Every vocabulary word is a sword strike. Learn to conquer."*

</div>
