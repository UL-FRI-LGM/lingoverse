# LingoVerse

LingoVerse is an open-world VR role-playing game for immersive foreign language learning, built in Unity for Meta Quest devices. Players explore virtual environments, interact with NPCs in the target language, pick up labeled objects, and play vocabulary mini-games — all within a fully immersive VR experience.

The project also includes **PINA** (Powerful Intuitive Node/Narrative Assistant), a visual node-based dialogue editor that allows teachers and content creators to build branching conversations without writing code.

## Features

### Immersive VR Environments

- **City** — urban setting for everyday vocabulary and conversations
- **Countryside** — rural environment with nature and animal vocabulary
- **Kitchen** — indoor space for learning kitchen utensils and food items
- **Classroom** — school setting for formal language and grammar structures

All environments use a low-poly art style optimized for mobile VR, with professional 3D models from Synty Studios.

### NPC Dialogues

NPCs are placed throughout environments with predefined dialogue trees that teach specific words and phrases. Dialogues include both text and audio, followed by interactive response tasks:

- **Multiple choice** — select an answer using the controller ray
- **Speech recognition** — speak your answer into the microphone (powered by Whisper and VOSK)
- **Continuation** — press a button to advance the conversation

Correct answers earn experience points (XP). Incorrect answers trigger dialogue repetition for reinforcement.

### Interactive Objects

When a player picks up an object for the first time, a popup displays the item's name in the target language along with a native-speaker audio pronunciation. Over 64 objects are available across categories including animals, foods, fruits, and classroom items.

### Mini-Games

**Match Pairs** — players grab objects from one side and place them on sockets labeled with foreign-language words on the other side. Scored on accuracy and completion time. Available themed sets include fruits, kitchen items, park objects, and a tutorial game.

### Quest System

Multi-step quests guide learners through structured sequences of dialogues, item collection, and mini-games. Quests support prerequisites, so completing earlier quests unlocks later ones. Each quest awards XP on completion.

### Scoring and Progression

An XP-based scoring system tracks player progress across dialogues, quests, and mini-games. Scores persist between sessions and are displayed on an in-game leaderboard.

## PINA — Dialogue Editor

PINA is the built-in visual dialogue editor accessible from **Graph → Dialogue Graph** in the Unity Editor. It enables content creation without programming:

- **Dialogue Nodes** (green) — text with multiple-choice answer buttons
- **Whisper Nodes** (blue) — text with speech recognition for spoken answers
- **Text-to-Speech** — generate audio directly from node text in Slovenian, English, German, and French (male and female voices)
- **Save/Load** — dialogues are stored as ScriptableObjects in `Resources/Dialogues/`

To assign a dialogue to a character, select the NPC in the scene hierarchy and set the `StartNode` field in its `Dialogue Point` component.

## Speech Technologies

| Technology  | Type           | Description                                                         |
| ----------- | -------------- | ------------------------------------------------------------------- |
| **Whisper** | Speech-to-Text | OpenAI model for high-accuracy speech recognition (local or cloud)  |
| **VOSK**    | Speech-to-Text | Offline speech recognition with models for German and English       |
| **Piper**   | Text-to-Speech | Neural TTS running locally via Unity Sentis, supporting 4 languages |

## Architecture

The application uses an **event-driven architecture** with a central `GameEventsManager` singleton that decouples all subsystems:

```
GameEventsManager
├── MovementEvents      ├── DialogueEvents     ├── ScoreEvents
├── QuestEvents         ├── ItemEvents         ├── WhisperEvents
├── InputEvents         ├── AudioEvents        ├── SceneEvents
```

Key managers: `DialogueManager`, `QuestManager`, `AudioManager`, `ScoreManager`, `DataManager`, `SceneTransitionManager`, `PlayerManager`, `ItemManager`.

### Data Collection

`DataManager` logs gameplay telemetry (dialogue scores, quest completions, mini-game results, item pickups, play time) to local CSV files for learning analytics.

## VR Optimization

- **Occlusion culling** — renders only visible geometry
- **Baked lighting** — pre-computed lighting stored in textures
- **Tunneling vignette** — reduces motion sickness during movement
- **Scene management** — smaller focused scenes for faster loading

## Getting Started

### Requirements

- Unity Hub with Unity **6000.0.43f1**
- Meta Quest Link application (Windows)
- Meta Quest headset
- Workshop Unity project with PINA components

### Quick Start

1. Open the project in Unity Hub
2. Connect Meta Quest via Quest Link (USB-C or Air Link)
3. Open **Graph → Dialogue Graph** to create dialogues with PINA
4. Assign dialogues to NPCs via the `Dialogue Point` component
5. Press **Play** to test with the connected headset

> **Note:** Changes made during Play mode are not saved. Save your project regularly.

### Controls

| Button             | Action                                                                    |
| ------------------ | ------------------------------------------------------------------------- |
| **A**              | Confirm selection, start dialogue, show object description, record speech |
| **Grip**           | Pick up objects                                                           |
| **Controller ray** | Point at buttons and navigate menus                                       |
| **Right joystick** | Teleportation movement                                                    |

## Supported Languages

| Language  | Speech-to-Text | Text-to-Speech     |
| --------- | -------------- | ------------------ |
| German    | VOSK, Whisper  | Eva K, Thorsten    |
| English   | VOSK, Whisper  | LJSpeech, HFC Male |
| French    | —              | Tom                |
| Slovenian | —              | Artur              |

## Research

LingoVerse was evaluated in a pilot study with 14 high school students. Results showed statistically significant improvement in German language proficiency after using the VR game, with 85.7% of participants recommending it to peers.

> Žnideršič K., Špruk N.J., Marolt M., Pesek M. (2025). _Language Learning with VR: The Effects of Immersive Gamification on Student Motivation and Knowledge._ CSEDU 2025.

## Acknowledgements

Developed at the Laboratory for Computer Graphics and Multimedia, Faculty of Computer and Information Science, University of Ljubljana. Research partially co-funded by the TeachXR project (contract C4350-24-927003).

## License

This project is part of the TeachXR initiative. Contact the authors for licensing information.
