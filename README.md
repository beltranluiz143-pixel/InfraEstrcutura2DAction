# Infraestructura 2D/Action Unity

A 2D action-platformer architecture base for Unity: system communication, saving, scene flow, player, combat, AI, dialogue, quests, shops, cutscenes and map, all wired through events and configured with data.

> **Status:** [FILL IN: the project's real version/status, e.g. "first release, still in development".]

## Table of contents

1. [Introduction](#1-introduction)
2. [The systems](#2-the-systems)
3. [How the architecture is put together](#3-how-the-architecture-is-put-together)
4. [Step by step in the Unity Editor](#4-step-by-step-in-the-unity-editor)
5. [Your first scene and how to add content](#5-your-first-scene-and-how-to-add-content)
6. [Requirements, installation and license](#6-requirements-installation-and-license)

---

## 1. Introduction

### What it is

Infraestructura 2D/Action Unity is the technical skeleton of a 2D action-platformer game — think metroidvania or action-platformer in general. It's not a finished game, and it's not a template with art, sound or levels: it's the part almost every project like this needs, and that almost nobody enjoys rewriting from scratch every time.

Think of it as a technical core: a set of systems that are already wired together and that, once you've set them up, give you a base to build content on top of, data-driven, without worrying about how one system finds out about another. That part — the communication between systems — is already handled by the event-based architecture.

Concretely, it includes: a player with movement, combat, health and a resource; enemies and bosses with AI; dialogue, NPCs, quests and shops; cutscenes and minigames; a map; slot-based saving; menus; camera and audio. All of it talks to each other through events, and content is defined with data instead of writing new code every time.

### Why it exists

I built this architecture as a core made of layers of systems connected by events, so that you, as the programmer, only need to create content out of reusable data and connections made from the Editor. (The exception is when you want to build a brand-new system or mechanic that doesn't exist yet in the architecture — that's when you actually need to write code.)

The underlying reason is that I think the main problem in a lot of projects isn't one broken mechanic here or there, but the isolated, tangled way the technical side ends up working, where it's easy to lose track of things: stuff that looks for each other with `Find`, singletons that everyone reaches into, cross-scene references, an initialization order nobody remembers anymore, or having to hook into old scripts just because you want to add one new element. That's why, even though this architecture isn't perfect, it at least gives you a more visual, easier-to-follow scheme so you don't get lost in that technical layer.

As you can probably tell, it's mainly aimed at people who are just getting started with game programming in Unity, and it isn't trying to expand beyond the genre it already targets: 2D action and platforming.

### Design principles

So this doesn't turn into spaghetti all over again, a handful of rules repeat throughout the whole codebase:

1. **No system knows about another one at runtime.** They talk through an event bus, the `EventBus`. A system announces what just happened; whoever cares, listens.
2. **Structural dependencies are injected exactly once.** Whatever a system needs from another one (the save system, say) is handed to it in its `Initialize()` method, from a single startup point: `BootstrapSystem`. After that, it's events only.
3. **Content is data.** Enemies, dialogue, quests, items, shops, cutscenes and routes are all `ScriptableObject`s. Adding content doesn't require touching code.
4. **One persistent scene, and additive level scenes.** The Core scene holds the systems and is always loaded. Every level loads on top of it. Whatever shows up in a level (player, spawn points, camera) announces itself through events instead of being searched for.
5. **Saving happens in two phases.** The save system doesn't know anyone's state: it asks every system to dump its own, and only then writes to disk.
6. **No singletons, no runtime lookups.** There's no `FindObjectOfType` and no static access to systems anywhere in the gameplay code. (Editor windows do use lookups — but those aren't part of the game itself.)

### What it isn't

- It doesn't ship with art, animations, sound or levels.
- It isn't an install-and-play package: you need to set up the Core scene, assign references and build your content.
- It doesn't try to cover every genre: it's built for 2D action and platforming.
- [FILL IN: any other limitation you want to spell out.]

### Who it's for

Mainly for people who are starting out with game programming in Unity and want an already-organized base to build their own 2D action game on top of, or simply to learn how a project like this can be structured. It's not trying to cover other genres or scale beyond that.

---

## 2. The systems

The code has two layers: **Core** (infrastructure with no concrete gameplay) and **Systems** (player, combat, AI and content). Every system is summarized the same way:

`[Core]` lives in the persistent scene · `[Level]` lives in each level scene or in the player prefab · **Needs:** whatever `BootstrapSystem` hands it in `Initialize()` · **Listens / Raises:** its main events.

Level objects announce themselves through an event when they get enabled (`PlayerRegistered`, `SpawnPointRegistered`, `CameraFollowTargetRegistered`, `CutsceneCharacterRegistered`), and systems ask each other for things by raising request or query events (`FlagQuery`, `ConditionQuery`, `PoolSpawnRequest`, `ResourceConsumeRequest`, `CurrencyAddRequest`...). The Bootstrap's content systems are optional: leave a field empty and that system just isn't used.

```
  Input ──► EventBus ◄──► Player · AI · Dialogue · Quests · Shop · ...
                ▲
  Bootstrap ────┘   (injects dependencies once, in Initialize)
```

### 2.1 Core

- **EventBus / BaseEvent** — Static bus (`Subscribe`, `Unsubscribe`, `Raise`). Iterates over a copy of the listeners, so a failure in one doesn't affect the rest. Every event inherits from `BaseEvent`, which carries a `SourceID`.
- **BootstrapSystem** `[Core]` — The single startup point: in `Start()` it initializes everything in a fixed order (each step isolated, one failure doesn't abort the boot) and raises `CoreInitialized`. It initializes the player once it hears `PlayerRegistered`.
- **DebugSystem / EventDebugger / ConsoleOverlay** `[Core]` — Categorized logs, an event history and an on-screen console. **Needs:** nothing.
- **GameStateSystem** `[Core]` — Global state machine (`Gameplay`, `Pause`, `Dialogue`, `Cutscene`, `Death`, `Dream`, `Minigame`, `MainMenu`, `Menu`). **Listens:** pause, death, dialogue, `SceneLoaded`, `GameStateChangeRequest`. **Raises:** `GameStateChanged`, which is what lets input, UI, audio and movement adapt to the current state.
- **InputManager** `[Core]` — The only script that reads hardware: it translates Input System actions into events and enables the `Gameplay`, `UI` and `Global` maps depending on the state. **Needs:** an `InputActionAsset` assigned in the Inspector.
- **SceneFlowSystem + TransitionController + SpawnPoint** `[Core]`/`[Level]` — Loads and unloads scenes (additive) with a fade, and places the player at the requested `SpawnPoint`. **Needs:** `SaveSystem`. **Listens:** `CoreInitialized`, `SceneTransitionRequest`, `StartGameRequest`. **Raises:** `SceneTransitionStarted`, `SceneLoaded`.
- **SaveSystem** `[Core]` — Three JSON slots. Saves in two phases: raises `CollectSaveData` so every system writes its own state, then writes to disk. **Needs:** `SettingsSystem`. **Listens:** `SaveRequested`, `CheckpointActivated`. **Raises:** `SaveLoaded`.
- **SettingsSystem** `[Core]` — Audio, controls and accessibility settings. **Listens:** `SettingsChanged`, `SettingsRequested`. **Raises:** `AudioSettingsChanged`, `ControlsUpdated`, `AccessibilitySettingsChanged`, `SaveRequested`.
- **GlobalVariablesSystem** `[Core]` — World flags and variables (`FlagDatabase`, `VariableDatabase`) and `FlagCondition` evaluation; saved and restored with the save file. **Needs:** `SaveSystem`. **Handles:** `FlagQuery`, `FlagSetRequest`, `VariableQuery`, `ConditionQuery`. **Raises:** `GlobalFlagChanged`, `GlobalVariableChanged`.
- **ResourceManager + PoolManager** `[Core]` — `ResourceManager` returns the prefab for an ID (`AssetReferenceDatabase`); `PoolManager` does the actual pooling and hands out objects via `PoolSpawnRequest`. **Needs:** `PoolManager` receives `ResourceManager`.
- **Audio** `[Core]` — `AudioCore` initializes `MusicManager` (`MusicPlayRequest`), `SFXManager` (`SFXPlayRequest`, uses the pool) and `SnapshotController` (mixer snapshots per game state). `SoundDatabase` maps IDs to clips; `AudioEventListener` ties a sound to an event. **Needs:** `AudioCore` receives `PoolManager`.
- **UICore / UIPanel / UIStateController** `[Core]` — `UICore` registers panels by `PanelID` and keeps a navigation stack; `UIPanel` is the base class every panel inherits from; `UIStateController` opens panels depending on the game state (a state → `PanelID` mapping). **Listens:** `UIOpenPanelRequest`, `UIClosePanelRequest`, `GameStateChanged`.
- **HUDSystem** `[Core]` — Shows health, resource and currency. **Listens:** `PlayerHealthChanged`, `ResourceChanged`, `CurrencyChanged`.
- **Menus** `[Core]` — `PausePanel`, `SettingsPanel`, `SaveSlotPanel` (raises `StartGameRequest`; needs `SaveSystem`), `MainMenuController`, `MenuHotkeyController` (inventory and map shortcuts, requests the `Menu` state) and `ApplicationLifecycleSystem` (`QuitGameRequest`).
- **CameraSystem + CameraFollowTarget + CameraTrigger** `[Core]`/`[Level]` — Built on Cinemachine: follows whichever `CameraFollowTarget` registers itself, and applies the highest-priority `CameraTrigger` zone (its bounds and its `CameraData`). **Listens:** `CameraFollowTargetRegistered`, `CameraZoneEntered`, `CameraZoneExited`.

### 2.2 Player

- **PlayerSystem** `[Level]` — Coordinator for the player prefab: announces itself with `PlayerRegistered` and hands its subsystems whatever they need. **Needs:** `GlobalVariablesSystem`, `SaveSystem`, `ItemDatabase`.
- **PlayerMovementSystem / PlayerAbilityUnlocks / PlayerLockCoordinator** `[Level]` — Walking, jumping (with coyote time, jump buffer and double jump), dashing, wall sliding and surfaces. Abilities unlock via flags, and simultaneous movement locks (shield, healing, dialogue) are counted so they don't release too early. **Listens:** `MoveInput`, `JumpPressed`, `DashPressed`, `GameStateChanged`. **Raises:** `PlayerMoved`, `PlayerJumped`, `PlayerLanded`.
- **PlayerStateSystem / SurfacePhysicsSystem** `[Level]` — The player's state machine, and physics based on the current ground or zone (`SurfaceData`: standard, water, directional, bouncy). **Raise:** `PlayerStateChanged`, `SurfaceChanged`.
- **PlayerCombatSystem / ShieldSystem / DamageReceiver** `[Level]` — Attacks defined with `AttackData`, a directional shield, and damage taken from anything implementing `IDamageSource`. **Listens:** `AttackPressed`, `ShieldPressed`, `PacifyPressed`. **Raises:** `PlayerAttack`, `ShieldBroken`, `EnemyHit`.
- **PlayerHealthSystem** `[Level]` — Whole-number health, temporary invulnerability, healing (which consumes the resource) and death. **Needs:** `SaveSystem`. **Raises:** `PlayerHealthChanged`, `PlayerDamaged`, `PlayerDeath`.
- **PlayerResourceSystem** `[Level]` — The player's generic resource (mana, stamina, whatever your game needs): a maximum, regeneration, and per-key consumption via `ResourceConsumptionTable`. **Needs:** `SaveSystem`. **Handles:** `ResourceConsumeRequest`, `ResourceCollectRequest`. **Raises:** `ResourceChanged`.
- **PlayerInventorySystem / PlayerCurrencySystem** `[Level]` — Items, equipment with real numeric effects, and currency. **Need:** `SaveSystem` (the inventory also needs `ItemDatabase`). **Handle:** `ItemCollected`, `EquipItemRequest`, `CurrencyAddRequest`. **Raise:** `InventoryUpdated`, `CurrencyChanged`.
- **InteractionSystem + interactables** `[Level]` — Detects the closest `IInteractable` and triggers it with `InteractPressed`. Available interactables: `NPCInteractable`, `ShopInteractable`, `SavePoint` (needs a `SpawnPoint` on the same object), `SceneDoor` (with an optional condition and cost), and the item, resource, currency and ability pickups.
- **PlayerAnimationController** `[Level]` — Translates gameplay events into `Animator` parameters, with no logic of its own, and respects whatever locks cutscenes impose. `CharacterAnimationID` gives every character a stable ID so cutscenes can direct them.

### 2.3 Enemies

- **EnemySystem / AIController / AttackHandler** `[Level]` — `EnemySystem` is the body (health, pacification, death, loot). `AIController` is the brain (`Idle`, `Patrol`, `Chase`, `Attack`, `Pacifiable` and `Dead` states, plus boss phases). `AttackHandler` executes the chosen attacks (melee, projectile, charge). They implement `IHittable`, `IPacifiable` and `IDamageSource`, which is exactly what decouples them from the player. **Raise:** `EnemyKilled`, `EnemyPacified`, `BossDefeated`. **Data:** `EnemyData`, `BossData`, `HitboxData`, `ProjectileData`.

### 2.4 Content

- **DialogueSystem + DialoguePanel** `[Core]` — Plays back a `DialogueData` (lines, conditions, choices with decisions like giving or removing an item, giving or charging money, or a route effect). **Needs:** `GlobalVariablesSystem`. **Listens:** `DialogueStartRequest`, `DialogueAdvance`. **Raises:** `DialogueStarted`, `DialogueEnded` (on the first one, `GameStateSystem` enters `Dialogue`).
- **NPCSystem** `[Core]` — Picks the right dialogue out of an `NPCData` based on conditions and priority, and can kick off a quest. **Needs:** `GlobalVariablesSystem`. **Listens:** `NPCInteractionRequest`. **Raises:** `DialogueStartRequest`, `QuestStartRequest`.
- **QuestSystem + QuestLogPanel** `[Core]` — Quests with objectives (kill, collect, talk, reach a flag), requirements and rewards; saves its progress. **Needs:** `GlobalVariablesSystem`, `SaveSystem`, `RouteSystem`, and a `QuestDatabase` assigned in the Inspector. **Raises:** `QuestStarted`, `QuestCompleted`.
- **RouteSystem / EventReactionSystem** `[Core]` — `RouteSystem` computes a dominant route out of weighted actions (`RouteActionData`, `RouteData`, `RouteRules`). `EventReactionSystem` wires events to variables and flags without writing code (`EventReactionRules`). **Need:** `GlobalVariablesSystem`. **Raise:** `RouteChanged`, `RouteActionTriggered`.
- **ShopManager + ShopPanel** `[Core]` — Handles purchases in shops defined with `ShopData`, with the final price depending on the dominant route; requests the `Menu` state while the shop is open. **Needs:** `RouteSystem`. **Listens:** `ShopOpenRequest`, `ShopPurchaseRequest`. **Raises:** `ShopPurchaseSuccess`, `ShopPurchaseFailed`.
- **CutsceneSystem** `[Core]` — Runs a `CutsceneData` (an ordered list of commands: move, animate, dialogue, camera, wait, fade, music, sound, effect), either in sequence or in parallel. `CutsceneTrigger` `[Level]` fires it with an optional condition; `CutsceneCharacterReference` `[Level]` registers characters. **Needs:** `TransitionController`.
- **MinigameSystem** `[Core]` — Instantiates a prefab with an `IMinigame` component (`StartGame`, `EndGame`), requests the `Minigame` state, and applies the reward or penalty from its `MinigameData`. **Needs:** nothing. **Listens:** `MinigameStartRequest`, `MinigameEnded`.
- **MapSystem + MapPanel** `[Core]` — Visited rooms, player position and pins (saved with the game), drawn from `RoomMapData`, `MapDatabase` and `MapPinIconDatabase`. **Needs:** `SaveSystem`. **Raises:** `MapPinsChanged`, `SaveRequested`.

### 2.5 Data (ScriptableObject)

All created from **Assets → Create → Infraestructura2DAction**.

| Category | Data |
|---|---|
| Core | `SceneReference`, `AssetReferenceDatabase`, `SoundDatabase`, `FlagDatabase`, `VariableDatabase`, `FlagCondition` |
| Player | `AttackData`, `SurfaceData`, `ResourceConsumptionTable` |
| Camera | `CameraData` |
| Enemies | `EnemyData`, `BossData`, `HitboxData`, `ProjectileData` |
| Content | `DialogueData`, `NPCData`, `QuestData`, `QuestDatabase`, `ItemData`, `ItemDatabase`, `ShopData`, `CutsceneData`, `MinigameData` |
| Map | `RoomMapData`, `MapDatabase`, `MapPinIconDatabase` |
| Routes and reactions | `RouteData`, `RouteActionData`, `RouteRules`, `EventReactionRules` |

### 2.6 Editor tools

All of them live under the **Infraestructura2DAction** menu.

| Tool | What it's for |
|---|---|
| Flag Generator | Generates the flag names for an entity (NPC, boss, quest, item...) following a fixed convention. |
| Flag Validator | Scans the project and flags unregistered or duplicate flags, likely typos, or IDs missing a flag. |
| Content Creator | Creates the `ScriptableObject`s for a piece of content and sorts them by chapter and category. |
| Scene Setup | Creates a level scene with camera, tilemaps, a spawn point, a Build Settings entry and a `SceneReference`. |
| World State Inspector | In Play Mode, shows and edits flags and variables. |
| Quest Debugger | In Play Mode, activates, completes and resets quests. |
| Checkpoint Debugger | In Play Mode, teleports the player to any `SpawnPoint`. |
| Dialogue Preview | Previews a `DialogueData` without entering Play Mode. |
| NPC Dialogue Map | Shows an NPC's conditional dialogue tree. |
| Audio Preview | Plays audio clips right in the Editor. |
| Transform Sanity Checker | Flags absurd positions and scales in the open scenes. |

There's also a `ScaleReferenceGrid` component that draws a scale-reference grid in the Scene view (a gizmo only — it has no effect on the actual game).

---

## 3. How the architecture is put together

### 3.1 Two kinds of scene

- **Core scene (persistent):** holds every system, the UI, the camera and the fade. It loads once and is never unloaded.
- **Level scenes (additive):** load on top of Core and unload when you leave the room. This is where the player, spawn points, ground, enemies, doors, NPCs and items live.
- The main menu is, technically, just another level scene, with its own `MainMenuController`.
- Build Settings order: the Core scene has to be first (index 0). The menu and the levels come after.

### 3.2 Core scene hierarchy

You can organize it however you like, but here's the recommended layout:

- **Bootstrap:** the `BootstrapSystem`.
- **Systems** (an empty object grouping everything else):
  - Debug: `DebugSystem` (required by the Bootstrap). Inside it, `EventDebugger` and `ConsoleOverlay` are optional — you can leave them empty if you don't need an on-screen console.
  - Input: `InputManager`.
  - GameState: `GameStateSystem`.
  - SceneFlow: `SceneFlowSystem`, with `TransitionController` on the same object (they're required to sit together).
  - Settings: `SettingsSystem`.
  - Save: `SaveSystem`.
  - Resources: `ResourceManager`.
  - Pool: `PoolManager`.
  - GlobalVariables: `GlobalVariablesSystem`.
  - Audio: `AudioCore`, with `MusicManager` (and its `AudioSource`), `SFXManager` and `SnapshotController`.
  - Camera: the Main Camera with a `CinemachineBrain`, a `CinemachineCamera` with a `CinemachineConfiner2D`, and the `CameraSystem`.
  - Content (optional): `DialogueSystem`, `RouteSystem`, `EventReactionSystem`, `NPCSystem`, `QuestSystem`, `ShopManager`, `CutsceneSystem`, `MapSystem` and `MinigameSystem`.
  - Extras: `ApplicationLifecycleSystem` and `MenuHotkeyController`.
- **UI:**
  - A Canvas with `UICore`, `UIStateController` and `HUDSystem`.
  - The panels: Pause, Settings, Save Slots, Dialogue, Inventory (with a quest log and the map as tabs) and Shop.
  - A separate Canvas, on top of everything else, with the `CanvasGroup` and the black `Image` used for the fade.
  - An `EventSystem` with the Input System UI Input Module.

### 3.3 BootstrapSystem references

For a system to actually work in the game, it has to be wired by hand into the Bootstrap. Leave its field empty and that system just doesn't initialize — and if it's one of the required ones, the Bootstrap will tell you so in the console.

**Required** (the Bootstrap warns you if any of these is missing):
- Debug System, Input Manager, Game State System, Scene Flow System, Settings System, Save System, Resource Manager, Pool Manager, Global Variables System, Audio Core, Camera System.
- UI Core, UI State Controller and HUD System.

**Optional** (empty means that system simply isn't used):
- Dialogue System, Route System, Event Reaction System, NPC System, Quest System, Shop Manager, Cutscene System, Map System.
- Save Slot Panel, Inventory Panel, Quest Log Panel.
- Item Database (needed if you're going to have an inventory).
- Target Frame Rate: 60 by default; 0 leaves it untouched.

### 3.4 Other Inspector references, system by system

- **SceneFlowSystem:** Initial Scene, a `SceneReference` for the first scene loaded on boot. It can be your main menu, or the first level directly if you're not building a menu. Also Resume Saved Game On Boot.
- **TransitionController:** the `CanvasGroup`, the `Image` and the fade duration.
- **InputManager:** the `InputActionAsset` with the player's actions.
- **CameraSystem:** the `CinemachineCamera`, the `CinemachineConfiner2D` and a default `CameraData`.
- **AudioCore:** `SoundDatabase`, `MusicManager`, `SFXManager` and `SnapshotController`.
- **MusicManager:** an `AudioSource`. **SnapshotController:** four mixer snapshots (default, pause, dialogue, combat).
- **SFXManager:** the pool ID it uses for sound effects. It defaults to `AUDIO_SOURCE_SFX`, and that ID must exist in your `AssetReferenceDatabase`, pointing to a prefab with an `AudioSource`.
- **ResourceManager:** the `AssetReferenceDatabase`.
- **GlobalVariablesSystem:** the lists of global `FlagDatabase`s, the chapter-and-category groups (each group only carries an informational name), and the `VariableDatabase`.
- **UICore:** the list of registered panels. **UIStateController:** the `UICore` and the state → `PanelID` mapping.
- **HUDSystem:** the health image (Filled type), an optional animator, the resource image and text, and the currency text.
- **QuestSystem:** a `QuestDatabase`. **RouteSystem:** the `RouteData` list and a `RouteRules`. **EventReactionSystem:** an `EventReactionRules`. **MapSystem:** a `MapDatabase`.
- **DialogueSystem:** the default dialogue `PanelID`. **ShopManager:** the shop's `PanelID`.
- **Panels:**
  - `SaveSlotPanel`: the three slots (a button and a text for each) and the new-game scene.
  - `PausePanel`: its three buttons and the menu scene.
  - `SettingsPanel`: the volume, colorblind and vibration buttons.
  - `DialoguePanel`: text, portrait, the choice-button prefab and its container.
  - `ShopPanel`: vendor text, portrait, the catalog container, the entry prefab and the close button.
  - `InventoryPanel`: the list of tabs (a button and content for each), the grid container and the slot prefab.

### 3.5 Player prefab

All of these go on the root object, since they look each other up on the same object via `GetComponent`:

- `Rigidbody2D` and a `Collider2D`, tagged `Player`.
- `PlayerSystem`, `PlayerMovementSystem`, `PlayerAbilityUnlocks`, `PlayerLockCoordinator`, `PlayerStateSystem` and `SurfacePhysicsSystem`.
- `PlayerCombatSystem`, `ShieldSystem` and `DamageReceiver`.
- `PlayerHealthSystem`, `PlayerResourceSystem`, `PlayerInventorySystem` and `PlayerCurrencySystem`.
- `InteractionSystem`.
- `PlayerAnimationController`, with its `Animator` and a `CharacterAnimationID`.

Child objects:
- `GroundCheck` and `WallCheckFront`, for movement.
- `HitboxOrigin`, for attacks, and `ShieldOrigin` with its sprite, for the shield.
- `PromptRoot`, optional, the interaction prompt.
- `CameraFollowTarget`, with its Player field pointing back at the prefab's own root.

References you need to assign:
- `PlayerSystem`: its five subsystems (abilities, health, resource, inventory and currency).
- `PlayerInventorySystem`: combat, health and resource, so it can apply equipment effects.
- `PlayerMovementSystem`: `GroundCheck`, `WallCheckFront` and the ground layer.
- `PlayerCombatSystem`: four `AttackData` (normal, up, down and air), the `HitboxOrigin`, and the enemy and pacify layers.
- `ShieldSystem`: the sprite, the origin, the size and the enemy-attack layer.
- `DamageReceiver`: the hazard layer. `InteractionSystem`: the interactable layer.
- `PlayerResourceSystem`: a `ResourceConsumptionTable`.

### 3.6 Level scene

- One instance of the player prefab.
- At least one `SpawnPoint` with ID `Default`.
- Ground (Tilemap) on the ground layer.
- A camera zone: a `CameraTrigger` with a bounds collider.
- Optional: doors (`SceneDoor`), save points (`SavePoint`), NPCs, shops, enemies, pickups and cutscene triggers.

### 3.7 Non-negotiable conventions

- The player carries the `Player` tag. The camera, doors, pickups and surfaces all rely on it.
- IDs must be unique. Every `UIPanel` has its `PanelID`, and every `SceneReference`'s scene name must match the one in Build Settings.
- Panels registered in `UICore` must start active in the scene. `UICore` deactivates them on boot, and some of them subscribe to events in their `Awake`, which only runs if they start active.
- The dialogue panel is opened by `DialogueSystem` itself. Don't also map `Dialogue` to it in `UIStateController`, or you'll try to open it twice.
- The screen starts black until the first scene loads. That's expected, not a bug.

---

## 4. Step by step in the Unity Editor

### Step 1. Set up the project

- Use Unity 6 (built against 6000.3).
- In the Package Manager, install Input System (accept the restart it asks for), Cinemachine 3 and TextMeshPro (import its essential resources). Make sure the 2D Tilemap module is enabled.
- Under Project Settings → Player, set Active Input Handling to Input System Package, or Both.
- Copy the `Infraestructura2DAction` folder into your `Assets`.
- Wait for it to compile. The console shouldn't show any red errors before you move on.

### Step 2. Tag and layers

- Create the `Player` tag.
- Create layers for `Ground`, `Player`, `Enemy`, `Interactable` and `Hazard`. The names are up to you — these are just suggestions.

### Step 3. Input Actions

- Create an Input Actions asset with three action maps: `Gameplay`, `UI` and `Global`.
- Gameplay: `Move` (Vector2, keyboard and gamepad), `Jump`, `Dash`, `CrouchSlide`, `LookUp`, `LookDown`, `Attack`, `Shield`, `Heal`, `Pacify` and `Interact`. The names have to match exactly, or `InputManager` won't find them.
- UI: `DialogueAdvance` and `Cancel`.
- Global: `Pause`, `OpenInventory` and `OpenMap`.
- Save the asset. You'll assign it on the `InputManager`.
- Note: `CrouchSlide` is read as an action, but out of the box no system in the package reacts to it. It's there ready for you to hook up your own crouch/slide mechanic whenever you need one; leaving it unbound is perfectly fine.

### Step 4. Base data

From Assets → Create → Infraestructura2DAction, create:

- An `AssetReferenceDatabase`, with an `AUDIO_SOURCE_SFX` entry whose prefab is an object with an `AudioSource`, and a pool size of around 10.
- A `SoundDatabase`.
- A `FlagDatabase` and a `VariableDatabase`.
- A `CameraData`.
- A `ResourceConsumptionTable`, with the `Heal` and `ShieldPerSecond` keys.
- An `ItemDatabase` and a `QuestDatabase`.
- Four `AttackData` for the player.

### Step 5. Core scene systems

- Create a new scene called Core.
- Create the Systems objects described in 3.2 and add each one's component.
- Under SceneFlow, add `SceneFlowSystem` and check that `TransitionController` shows up automatically on the same object.
- Under Audio, add the three subsystems as children or on the same object, and assign the `AudioSource` to `MusicManager`.
- Assign the references from 3.4 on each component: the `InputActionAsset` on `InputManager`, the `AssetReferenceDatabase` on `ResourceManager`, the `SoundDatabase` on `AudioCore`, and so on for the rest.

### Step 6. Camera

- The Main Camera carries a `CinemachineBrain` component.
- Create a `CinemachineCamera` and add a `CinemachineConfiner2D` to it.
- On `CameraSystem`, assign the `CinemachineCamera`, the Confiner and the default `CameraData`.
- This sets up the camera at the Core level, which is generic for the whole game. Each room's actual bounds get added separately, in each level scene: a collider with a `CameraTrigger` and its own `CameraData` (with its own zoom and behavior). You'll see that step in more detail in Phase 5.1, when you build your first scene.

### Step 7. Minimal UI

- Create a Canvas with `UICore`, `UIStateController` and `HUDSystem`.
- Create the panels with a component that inherits from `UIPanel` (`PausePanel`, `SettingsPanel`, `SaveSlotPanel`, `DialoguePanel`, `InventoryPanel`, `ShopPanel`), and give each one a unique `PanelID` — for example `Pause`, `Settings`, `SaveSlot`, `Dialogue`, `Inventory` and `Shop`.
- Leave all the panels active in the hierarchy. `UICore` will deactivate them on its own at boot.
- Add all of those panels to `UICore`'s list.
- On `UIStateController`, map the `Pause` state to the pause panel's `PanelID`. Don't map `Dialogue` (see 3.7).
- Create the fade Canvas, with a `CanvasGroup` and a full-screen black `Image`. Put it above everything else (high Sort Order) and assign both to `TransitionController`.
- Add an `EventSystem` with the Input System UI Input Module.
- On `HUDSystem`, assign the health, resource and currency images and texts.

### Step 8. Bootstrap references

- Select the Bootstrap object and drag every system into its field, per 3.3.
- On `SceneFlowSystem`, leave Initial Scene empty for now. You'll fill it in once you've created your first scene.

### Step 9. Build Settings and first run

- Open File → Build Profiles (or Build Settings) and add the Core scene first.
- Hit Play. Among other things, you should see "Core initialized. All systems ready." in the console.
- If Initial Scene is missing, you'll see a SceneFlow error pointing it out. That's expected at this point, don't worry about it.
- If any initialization step fails, the Bootstrap logs it in red with the name of the step that failed. Check that system's references.

### Step 10. Player prefab

- Create an empty object with a `Rigidbody2D` and a `Collider2D`. Tag it `Player` and set its layer to `Player`.
- Add the components from 3.5 to the root object.
- Create the children `GroundCheck`, `WallCheckFront`, `HitboxOrigin`, `ShieldOrigin` (with a `SpriteRenderer`) and `CameraFollowTarget`.
- Assign the references: the five subsystems on `PlayerSystem`, the three on `PlayerInventorySystem`, the checks and the ground layer on movement, the four `AttackData` on combat, the `ResourceConsumptionTable`, and the Player field on `CameraFollowTarget`.
- Turn the object into a prefab.
- The player only initializes once it actually shows up in a level scene. That's exactly what the next step covers: building your first scene.

---

## 5. Your first scene and how to add content

### 5.1 Building a simple level scene

Using the Scene Setup tool:

- Open Infraestructura2DAction → Scene Setup.
- Type the scene's name and its Chapter (for example `Chapter_01`). The scene is saved under `Assets/Scenes/Chapter/`, and its `SceneReference` under `Assets/Content/Chapter/SceneReferences/`.
- Check: camera zone, Tilemap, SpawnPoint, Build Settings and SceneReference.
- Uncheck "Add CameraFollowTarget" — you already put one inside the player prefab back in Phase 4.
- Uncheck "Main Camera" — the camera lives in the Core scene.
- Click Create.

Then, in the Editor:

- Open the new scene and paint the ground onto `Tilemap_Ground`. It needs to be on the Ground layer.
- Drag the player prefab into the scene. As soon as it loads, SceneFlow will place it at the SpawnPoint.
- Resize the CameraZone polygon to fit the room. The player must end up inside it.
- Assign a `CameraData` to that CameraZone (this is where you define that room's own zoom and behavior).
- Check that a `SpawnPoint` with ID `Default` exists.

To test it:

- Open the Core scene, select `SceneFlowSystem` and drag your scene's `SceneReference` into the Initial Scene field.
- Hit Play from Core. You should see a fade in from black, the player at the SpawnPoint, the camera following it, and movement working.

Common issues:

- The player keeps falling: you forgot to assign the ground layer on `PlayerMovementSystem`.
- The camera doesn't follow: check the Player field on `CameraFollowTarget` and `CameraSystem`'s references.
- The player doesn't move: the `InputActionAsset` is missing on `InputManager`, or the action names don't match exactly.
- Nothing loads: the scene isn't in Build Settings, or its name doesn't match the `SceneReference`.

### 5.2 Connecting two rooms

- In the first scene, create an object with a collider (Is Trigger) and the `SceneDoor` component.
- Under Target Scene, assign the destination room's `SceneReference`.
- Under Target Spawn Point ID, type the ID of a `SpawnPoint` that actually exists in the destination room.
- Optional: the fade color, a condition (`FlagCondition`) required to cross it, and a resource cost.
- With Trigger On Touch enabled, just touching the door is enough. Disable it and you'll need to interact instead, with the object placed on the Interactable layer.
- Repeat it in the other room so you can come back. Both scenes need to be in Build Settings.

### 5.3 Save points

- Create an object with a `SpawnPoint` (with a unique ID) and a `SavePoint`, plus a collider on the Interactable layer.
- Interacting with it makes the player respawn there next time, and saves the game right then.

### 5.4 Main menu and new game

- Create a menu scene with a Canvas, its buttons, and a `MainMenuController` with those buttons assigned. Add it to Build Settings.
- On `SceneFlowSystem`, set that menu scene as Initial Scene and turn off Resume Saved Game On Boot. Leave it on and, if slot 1 already has a save, the game will boot straight into it, skipping the menu.
- On `SaveSlotPanel`, assign the new-game scene (your first level).
- The flow ends up being: Continue or New Game opens the slot panel; picking a slot either resumes that slot's saved game, or loads the new-game scene if it was empty.

### 5.5 Flags and variables

These are the foundation for conditions, quests, doors and dialogue.

- Create a `FlagDatabase` via Assets → Create → Infraestructura2DAction → Flags. Each entry has an ID, a description and a default value. You can use a shared prefix and suffix for the whole database.
- Create a `VariableDatabase` the same way, with numeric values instead.
- Assign both to `GlobalVariablesSystem`: global flags, chapter groups and variables.
- Any flag or variable you use in a condition needs to be registered, or you'll get a console warning.
- A condition is created via Create → Flags → Flag Condition. Its types are: flag is true, flag is false, variable greater than, less than, or equal to a value.
- Naming convention: Flag Generator gives you names already formatted (say, `NPC_ID_MET` or `BOSS_ID_DEFEATED`). Flag Validator then checks that everything you use is actually registered.

### 5.6 Dialogue

- Create a `DialogueData` via Create → Content → Dialogue Data.
- Fields: `DialogueID`, `Dialogue Panel ID` (empty uses the default panel) and the list of lines.
- Every line has a character, text, a portrait, an optional condition, an alternative line and its choices.
- A condition doesn't skip the line outright: if it isn't met and there's an alternative line, that one shows instead. If there's no alternative, the normal line shows anyway.
- Every choice has text, a decision, and which line to jump to afterward (-1 continues in order). The possible decisions are: none, give item, remove item, give or charge money, route effect, and mark the NPC as helped.
- It's triggered from an NPC, from a cutscene (a Show Dialogue command), or from code with a `DialogueStartRequestEvent`.
- To check it without playing: Dialogue Preview shows you the whole dialogue without entering Play Mode.

### 5.7 NPCs

- Content Creator (NPC type) creates the `NPCData` and an intro dialogue for you, under `Assets/Content/Chapter/NPCs` and `Dialogues`.
- `NPCData` carries: ID, an effect on the player's resource, an optional linked quest, dialogue states (condition, dialogue and priority — the highest-priority valid one wins), and a fallback dialogue.
- In the scene, create the NPC's object with a collider on the Interactable layer and an `NPCInteractable` component pointing at its `NPCData`.
- On interaction, `NPCSystem` picks the right dialogue, marks the NPC as met, and starts its linked quest if it has one.
- To check it: NPC Dialogue Map shows you the whole tree for an NPC.

### 5.8 Items, equipment and currency

- `ItemData` carries: ID, display name, description, icon, type (`Consumable`, `Equipable`, `QuestKey` or `Material`) and a base price. If it's `Equipable`, it also states what it modifies (`AttackDamage`, `MaxHP`, `HealRecoveryTime` or `MaxResource`) and by how much.
- Add it to the `ItemDatabase` you assign in the Bootstrap.
- In the scene, an `ItemPickup` with a trigger collider and the item's ID hands it over on touch. `CurrencyPickup` does the same for currency, and `ResourcePickup` for the resource (this last one is an interactable, not automatic).
- `AbilityPickup` flips an ability flag (say, `ABILITY_DASH_UNLOCKED`). That flag needs to be registered and match the one `PlayerAbilityUnlocks` reads.

### 5.9 Quests

- `QuestData` carries: ID, text, requirements (a prior quest, a dominant route, a start condition), objectives and rewards (an item and currency).
- Every objective has a type, a target ID and an amount. That target ID can be an `EnemyID` (kill), an `ItemID` (collect), an `NPCID` (talk to), or a flag (reach).
- Add the quest to the `QuestDatabase` that `QuestSystem` uses. Without that, it won't be restored when a save is loaded.
- It's started from its linked NPC, or from code with a `QuestStartRequestEvent`.
- To check it: Quest Debugger lets you activate, complete and reset quests in Play Mode.

### 5.10 Shops

- `ShopData` carries: ID, vendor name, portrait, a catalog (item, and optionally its own price), the vendor's lines, and price multipliers per route.
- In the scene, a `ShopInteractable` with its `ShopData` and a collider on the Interactable layer.
- The `ShopPanel`'s `PanelID` has to match the one assigned on `ShopManager`.
- Opening it moves the game to the `Menu` state; its own close button leaves it.

### 5.11 Enemies and bosses

- Data: an `EnemyData` (ID, health, speed, detection radius, movement pattern, path points, range, cooldown, pacify threshold and loot) with its list of attacks. Each attack links a `HitboxData` (melee) or a `ProjectileData` (ranged), plus the range where it's used.
- For a boss: a `BossData` with a list of phases (the health percentage each one kicks in at, its movement, and its attacks).
- Enemy prefab:
  - A `Collider2D`, on a layer included in `PlayerCombatSystem`'s enemy mask.
  - The `EnemySystem`, `AIController` and `AttackHandler` components, all pointing at the same `EnemyData` (or `BossData`).
  - A `DetectionOrigin` child object and another for `HitboxOrigin`, with the player's layer included in `AIController`'s and `AttackHandler`'s masks.
  - A `Rigidbody2D` if it uses charges, and an `Animator` if you want one.
- Projectiles: the prefab carries a `Rigidbody2D`, a trigger collider and `EnemyProjectile`. Register it in the `AssetReferenceDatabase` under the same ID you put in `ProjectileData` (`PoolAssetID`), with a pool size. The projectile's layer needs to be in `DamageReceiver`'s hazard mask.
- Loot: enable Drops Loot and set Drop Pool ID to the ID of a prefab already registered in the `AssetReferenceDatabase`.
- Bosses: their `BOSS_ID_DEFEATED` flag needs to be registered. A defeated boss doesn't come back.
- Pacifying: with a threshold above zero, dropping below that health leaves the enemy weakened, and the player can pacify it with the Pacify action.

### 5.12 Routes and reactions

- `RouteData`: ID, name and starting score. Add these to `RouteSystem`'s list, along with an optional `RouteRules` limiting which transitions are allowed.
- `RouteActionData`: an ID and its per-route weights. It's triggered from a dialogue choice, or from a reaction.
- `EventReactionRules`: a list of reactions, each with a trigger (enemy killed, enemy pacified, boss defeated, scene loaded, or NPC helped), an optional ID, and an effect (increment or set a variable, set a flag, or fire a route action). You can use `{ID}` in the key to have it swapped for the event's real ID. Assign it to `EventReactionSystem`.

### 5.13 Cutscenes

- `CutsceneData` carries an ID and an ordered list of commands. Every command has a type (move, animate, dialogue, camera, wait, fade, music, sound, effect) and its own data: character, position, duration, animation, dialogue, color, audio or effect ID.
- Each command can either wait to finish before the next one, or run in parallel with it.
- Characters that appear in a cutscene need a `CutsceneCharacterReference`, and a `CharacterAnimationID` whose ID matches the command's Target Character ID. The player needs one too.
- To trigger it, place a `CutsceneTrigger` (trigger collider) with the cutscene, an optional condition, and the option to play it only once. If you use a flag to remember it already played, that flag needs to be registered.
- A known limitation: the camera-focus command needs a collider as its bounds, but a `ScriptableObject` can't hold a reference to a scene object. For now it only works reliably with colliders that live inside a prefab.

### 5.14 Minigames

- `MinigameData` carries: ID, prefab, reward (heal, give currency, or damage a boss), amount, damage on failure, and free-form difficulty parameters.
- The prefab needs a component implementing `IMinigame` (`StartGame` and `EndGame`), and must raise `MinigameEnded` with the result once it's done.
- There's no ready-made trigger component: you start it yourself with a `MinigameStartRequestEvent`, for instance from your own interactable.

### 5.15 Map

- One `RoomMapData` per room: scene name (identical to the actual scene), position on the map grid, silhouette, and world-space bounds.
- Add them to the `MapDatabase`, which you assign to `MapSystem`.
- `MapPanel` is placed as the content of one of `InventoryPanel`'s tabs, with its references assigned. The inventory and map shortcuts are set up on `MenuHotkeyController`, each pointing at the `PanelID` of the panel that holds it.

### 5.16 Audio

- `SoundDatabase`: every entry carries an ID, a clip, a volume, and whether it's music.
- For the default sounds to actually play, create entries with these exact IDs: `SFX_UI_OPEN`, `SFX_UI_CLOSE`, `SFX_SAVE_POINT`, `SFX_ITEM_PICKUP`, `SFX_CURRENCY_COLLECT`, `SFX_RESOURCE_COLLECT`, `SFX_DOOR_LOCKED`, `SFX_SHIELD_BLOCK`, `SFX_DIALOGUE_BLIP` and `SFX_ABILITY_UNLOCKED`.
- Music is requested with a `MusicPlayRequestEvent`, or with a cutscene's own music command.
- `AudioEventListener` lets you tie a sound to an interaction, to an enemy's death, or to a specific flag changing.

### 5.17 Surfaces

- `SurfaceData`: ID, behavior (standard, water, directional or bouncy), and its own values.
- Place a `SurfaceDetector` with that data on the ground, or in a zone with a trigger collider. The player needs the `Player` tag for it to be detected.

### 5.18 Double-checking everything

- Flag Validator: tells you about unregistered or duplicate flags.
- World State Inspector: shows and lets you edit flags and variables in Play Mode.
- Quest Debugger and Checkpoint Debugger: for testing quests and teleporting between save points.
- Transform Sanity Checker: finds absurd positions and scales that snuck into your scenes.

---

## 6. Requirements, installation and license

### 6.1 Requirements

- Unity 6, tested on 6000.3.
- Unity packages: Input System (`com.unity.inputsystem`, 1.18.0 or later), Cinemachine (`com.unity.cinemachine`, 3.1.7 or later) and UI/TextMeshPro (`com.unity.ugui`, 2.0.0 or later — in Unity 6 this package already ships with TextMeshPro built in, no need to install it separately).
- Built-in engine modules: Tilemap, Physics2D, Audio and Animation. These ship with the Editor; just make sure they aren't disabled in the Package Manager.
- Recommended, though not a direct code dependency: `com.unity.2d.tilemap` (gives you the Tile Palette to paint ground by hand) and `com.unity.2d.sprite`.
- The first time you use a TextMeshPro text field in a new project, Unity will ask to import its "TMP Essential Resources". Accept it.

### 6.2 Installation

There are two ways to get the package into your project:

- **Copy the folder by hand.** Copy `Infraestructura2DAction` into your `Assets`. This is the simplest option, and the one the step-by-step guide in Phase 4 follows.
- **Via Git URL, from the Package Manager** (recommended once the repository is on GitHub). Window → Package Manager → the + button → Add package from git URL, and paste the repository's URL. Unity installs it as a proper package (thanks to the `package.json` and the two `.asmdef` files it ships with), outside your `Assets` folder, without mixing with your own code. Once the repository has tagged releases, you can pin a specific one by appending `#tag-name` to the URL.

### 6.3 License

The whole package is released under the MIT license: you can use it, modify it and ship it in commercial projects without asking permission, on the one condition that you keep the copyright notice. The full text lives in the `LICENSE` file, at the root of the package.
