# Infraestructura 2D/Action Unity

Base de arquitectura para juegos 2D de acción y plataformas en Unity: comunicación entre sistemas, guardado, flujo de escenas, jugador, combate, IA, diálogos, misiones, tiendas, cinemáticas y mapa, todo conectado por eventos y configurado con datos.

> **Estado: Todavia en una version beta y desarollo. 

## Índice

1. [Introducción](#1-introducción)
2. [Los sistemas](#2-los-sistemas)
3. [Cómo se monta la arquitectura](#3-cómo-se-monta-la-arquitectura)
4. [Paso a paso en el Editor de Unity](#4-paso-a-paso-en-el-editor-de-unity)
5. [Tu primera escena y cómo añadir contenido](#5-tu-primera-escena-y-cómo-añadir-contenido)
6. [Requisitos, instalación y licencia](#6-requisitos-instalación-y-licencia)

---

## 1. Introducción

### Qué es

Infraestructura 2D/Action Unity es el esqueleto técnico de un juego 2D de acción y plataformas, estilo metroidvania o action-platformer. No es un juego terminado ni una plantilla con arte, sonido o niveles: es la parte que casi todos estos proyectos necesitan y que casi nadie tiene ganas de reescribir cada vez.

Piénsalo como un núcleo técnico: un conjunto de sistemas ya conectados entre sí que, una vez montados, te dejan una base sobre la que construir contenido a base de datos (Data-Driven), sin tener que preocuparte de cómo se enteran unos sistemas de otros. Esa parte, la comunicación, ya la resuelve la arquitectura por eventos.

En concreto trae: jugador con movimiento, combate, salud y recurso; enemigos y jefes con IA; diálogos, NPCs, misiones y tiendas; cinemáticas y minijuegos; mapa; guardado en slots; menús; cámara y audio. Todo eso habla entre sí por eventos, y el contenido se define con datos, no escribiendo código nuevo cada vez.

### Por qué existe

Monté esta arquitectura como un núcleo formado por capas de sistemas conectadas por eventos, de forma que tú, como programador, solo tengas que crear contenido a base de datos reutilizables y conexiones desde el Editor. (La excepción es si quieres montar un sistema o mecánica nueva que no existe todavía en la arquitectura: ahí sí toca escribir código de verdad.)

La razón de fondo es que creo que el problema principal de muchos proyectos no es una mecánica suelta que falla, sino el funcionamiento técnico aislado en el que es fácil perder el hilo: cosas que se buscan entre sí con `Find`, singletons a los que accede todo el mundo, referencias cruzadas entre escenas, un orden de inicialización que nadie recuerda ya, o tener que enganchar scripts antiguos solo porque quieres añadir un elemento nuevo. Por eso, aunque esta arquitectura no es perfecta, al menos ofrece un esquema más visual y más fácil de seguir para no perderte en esa parte técnica.

Como se puede intuir, está pensada sobre todo para gente que empieza en la programación de videojuegos en Unity, y tampoco pretende expandirse a otros géneros fuera del que ya tiene: 2D de acción y plataformas.

### Principios de diseño

Para que todo esto no acabe siendo espagueti otra vez, hay unas pocas reglas que se repiten en todo el código:

1. **Ningún sistema conoce a otro en tiempo de ejecución.** Se comunican por un bus de eventos, el `EventBus`. Un sistema anuncia lo que ha pasado; quien tenga interés, lo escucha.
2. **Las dependencias estructurales se inyectan una sola vez.** Lo que un sistema necesita de otro (por ejemplo, el sistema de guardado) se le entrega en su método `Initialize()`, desde un único punto de arranque: el `BootstrapSystem`. A partir de ahí, todo lo demás va por eventos.
3. **El contenido son datos.** Enemigos, diálogos, misiones, objetos, tiendas, cinemáticas y rutas son `ScriptableObject`. Añadir contenido no exige tocar código.
4. **Una escena persistente y escenas de nivel que se van sumando encima.** La escena Core tiene los sistemas y vive siempre. Cada nivel se carga sobre ella. Lo que aparece en un nivel (jugador, puntos de aparición, cámara) se anuncia por eventos en vez de buscarse.
5. **Guardado en dos fases.** El sistema de guardado no conoce el estado de nadie: le pide a cada sistema que vuelque el suyo, y solo entonces escribe a disco.
6. **Sin singletons ni búsquedas en tiempo de ejecución.** No hay `FindObjectOfType` ni acceso estático a sistemas en el código de juego. (Las ventanas de Editor sí usan búsquedas, pero esas no forman parte del juego en sí.)

### Qué no es

- No trae arte, animaciones, sonidos ni niveles.
- No es un paquete de instalar y jugar: hay que montar la escena Core, asignar referencias y crear el contenido.
- No pretende cubrir todos los géneros: está pensada para 2D de acción y plataformas.
- [COMPLETAR: si hay alguna otra limitación que quieras dejar clara, va aquí.]

### Para quién es

Está pensada sobre todo para quien empieza en programación de videojuegos con Unity y quiere una base ya ordenada sobre la que construir su propio juego 2D de acción, o simplemente aprender cómo se puede organizar un proyecto así. No busca cubrir otros géneros ni escalar más allá de eso.

---

## 2. Los sistemas

El código tiene dos capas: **Core** (infraestructura sin jugabilidad concreta) y **Systems** (jugador, combate, IA y contenido). Cada sistema se resume igual:

`[Core]` vive en la escena persistente · `[Nivel]` vive en cada escena de nivel o en el prefab del jugador · **Necesita:** lo que `BootstrapSystem` le entrega en `Initialize()` · **Escucha / Lanza:** sus eventos principales.

Los objetos de nivel se anuncian por evento al activarse (`PlayerRegistered`, `SpawnPointRegistered`, `CameraFollowTargetRegistered`, `CutsceneCharacterRegistered`), y los sistemas se piden cosas entre sí lanzando eventos de petición o consulta (`FlagQuery`, `ConditionQuery`, `PoolSpawnRequest`, `ResourceConsumeRequest`, `CurrencyAddRequest`...). Los sistemas de contenido del Bootstrap son opcionales: si un campo queda vacío, no se usa.

```
  Input ──► EventBus ◄──► Jugador · IA · Diálogo · Misiones · Tienda · ...
                ▲
  Bootstrap ────┘   (inyecta las dependencias una sola vez, en Initialize)
```

### 2.1 Núcleo

- **EventBus / BaseEvent** — Bus estático (`Subscribe`, `Unsubscribe`, `Raise`). Recorre una copia de los oyentes y un fallo en uno no afecta al resto. Todo evento hereda de `BaseEvent`, que lleva un `SourceID`.
- **BootstrapSystem** `[Core]` — Único punto de arranque: en `Start()` inicializa todo en orden fijo (cada paso aislado, un fallo no aborta el arranque) y lanza `CoreInitialized`. Inicializa al jugador al oír `PlayerRegistered`.
- **DebugSystem / EventDebugger / ConsoleOverlay** `[Core]` — Logs por categorías, historial de eventos y consola en pantalla. **Necesita:** nada.
- **GameStateSystem** `[Core]` — Máquina de estados global (`Gameplay`, `Pause`, `Dialogue`, `Cutscene`, `Death`, `Dream`, `Minigame`, `MainMenu`, `Menu`). **Escucha:** pausa, muerte, diálogo, `SceneLoaded`, `GameStateChangeRequest`. **Lanza:** `GameStateChanged`, con el que input, UI, audio y movimiento se adaptan.
- **InputManager** `[Core]` — Único lector de hardware: traduce las acciones del Input System en eventos y activa los mapas `Gameplay`, `UI` y `Global` según el estado. **Necesita:** un `InputActionAsset` asignado en el Inspector.
- **SceneFlowSystem + TransitionController + SpawnPoint** `[Core]`/`[Nivel]` — Carga y descarga escenas (aditivo) con fundido y coloca al jugador en el `SpawnPoint` pedido. **Necesita:** `SaveSystem`. **Escucha:** `CoreInitialized`, `SceneTransitionRequest`, `StartGameRequest`. **Lanza:** `SceneTransitionStarted`, `SceneLoaded`.
- **SaveSystem** `[Core]` — Tres slots JSON. Guarda en dos fases: lanza `CollectSaveData` para que cada sistema escriba su estado y luego escribe a disco. **Necesita:** `SettingsSystem`. **Escucha:** `SaveRequested`, `CheckpointActivated`. **Lanza:** `SaveLoaded`.
- **SettingsSystem** `[Core]` — Ajustes de audio, controles y accesibilidad. **Escucha:** `SettingsChanged`, `SettingsRequested`. **Lanza:** `AudioSettingsChanged`, `ControlsUpdated`, `AccessibilitySettingsChanged`, `SaveRequested`.
- **GlobalVariablesSystem** `[Core]` — Flags y variables del mundo (`FlagDatabase`, `VariableDatabase`) y evaluación de `FlagCondition`; se guarda y restaura con la partida. **Necesita:** `SaveSystem`. **Atiende:** `FlagQuery`, `FlagSetRequest`, `VariableQuery`, `ConditionQuery`. **Lanza:** `GlobalFlagChanged`, `GlobalVariableChanged`.
- **ResourceManager + PoolManager** `[Core]` — `ResourceManager` devuelve el prefab de un ID (`AssetReferenceDatabase`); `PoolManager` hace el pooling y entrega objetos con `PoolSpawnRequest`. **Necesita:** `PoolManager` recibe `ResourceManager`.
- **Audio** `[Core]` — `AudioCore` inicializa `MusicManager` (`MusicPlayRequest`), `SFXManager` (`SFXPlayRequest`, usa el pool) y `SnapshotController` (snapshots del mixer según el estado). `SoundDatabase` asocia IDs con clips; `AudioEventListener` ata un sonido a un evento. **Necesita:** `AudioCore` recibe `PoolManager`.
- **UICore / UIPanel / UIStateController** `[Core]` — `UICore` registra los paneles por `PanelID` y gestiona una pila de navegación; `UIPanel` es la base de todo panel; `UIStateController` abre paneles según el estado del juego (mapeo estado → `PanelID`). **Escucha:** `UIOpenPanelRequest`, `UIClosePanelRequest`, `GameStateChanged`.
- **HUDSystem** `[Core]` — Muestra vida, recurso y moneda. **Escucha:** `PlayerHealthChanged`, `ResourceChanged`, `CurrencyChanged`.
- **Menús** `[Core]` — `PausePanel`, `SettingsPanel`, `SaveSlotPanel` (lanza `StartGameRequest`; necesita `SaveSystem`), `MainMenuController`, `MenuHotkeyController` (atajos de inventario y mapa, pide el estado `Menu`) y `ApplicationLifecycleSystem` (`QuitGameRequest`).
- **CameraSystem + CameraFollowTarget + CameraTrigger** `[Core]`/`[Nivel]` — Sobre Cinemachine: sigue al `CameraFollowTarget` que se registra y aplica la zona de mayor prioridad de los `CameraTrigger` (límites y `CameraData`). **Escucha:** `CameraFollowTargetRegistered`, `CameraZoneEntered`, `CameraZoneExited`.

### 2.2 Jugador

- **PlayerSystem** `[Nivel]` — Coordinador del prefab del jugador: se anuncia con `PlayerRegistered` y reparte a sus subsistemas lo que necesitan. **Necesita:** `GlobalVariablesSystem`, `SaveSystem`, `ItemDatabase`.
- **PlayerMovementSystem / PlayerAbilityUnlocks / PlayerLockCoordinator** `[Nivel]` — Caminar, salto (con coyote time, buffer y doble salto), dash, pared y superficies. Las habilidades se desbloquean con flags y los bloqueos simultáneos (escudo, curación, diálogo) se cuentan. **Escucha:** `MoveInput`, `JumpPressed`, `DashPressed`, `GameStateChanged`. **Lanza:** `PlayerMoved`, `PlayerJumped`, `PlayerLanded`.
- **PlayerStateSystem / SurfacePhysicsSystem** `[Nivel]` — Máquina de estados del jugador y física según el suelo o zona (`SurfaceData`: estándar, agua, direccional, rebote). **Lanzan:** `PlayerStateChanged`, `SurfaceChanged`.
- **PlayerCombatSystem / ShieldSystem / DamageReceiver** `[Nivel]` — Ataques definidos con `AttackData`, escudo direccional y daño recibido de cualquier `IDamageSource`. **Escucha:** `AttackPressed`, `ShieldPressed`, `PacifyPressed`. **Lanza:** `PlayerAttack`, `ShieldBroken`, `EnemyHit`.
- **PlayerHealthSystem** `[Nivel]` — Vida entera, invulnerabilidad, curación (consume recurso) y muerte. **Necesita:** `SaveSystem`. **Lanza:** `PlayerHealthChanged`, `PlayerDamaged`, `PlayerDeath`.
- **PlayerResourceSystem** `[Nivel]` — Recurso genérico del jugador (maná, energía...): máximo, regeneración y consumo por clave (`ResourceConsumptionTable`). **Necesita:** `SaveSystem`. **Atiende:** `ResourceConsumeRequest`, `ResourceCollectRequest`. **Lanza:** `ResourceChanged`.
- **PlayerInventorySystem / PlayerCurrencySystem** `[Nivel]` — Objetos, equipamiento con efectos numéricos reales y moneda. **Necesitan:** `SaveSystem` (el inventario también `ItemDatabase`). **Atienden:** `ItemCollected`, `EquipItemRequest`, `CurrencyAddRequest`. **Lanzan:** `InventoryUpdated`, `CurrencyChanged`.
- **InteractionSystem + interactuables** `[Nivel]` — Detecta el `IInteractable` más cercano y lo activa con `InteractPressed`. Interactuables: `NPCInteractable`, `ShopInteractable`, `SavePoint` (necesita un `SpawnPoint`), `SceneDoor` (con condición y coste opcionales) y los pickups de objeto, recurso, moneda y habilidad.
- **PlayerAnimationController** `[Nivel]` — Traduce los eventos de juego en parámetros del `Animator` y respeta los bloqueos de las cinemáticas. `CharacterAnimationID` da a cada personaje un ID estable.

### 2.3 Enemigos

- **EnemySystem / AIController / AttackHandler** `[Nivel]` — Cuerpo (vida, pacificación, muerte, botín), cerebro (estados `Idle`, `Patrol`, `Chase`, `Attack`, `Pacifiable`, `Dead` y fases de jefe) y ejecución de ataques (cuerpo a cuerpo, proyectil, carga). Implementan `IHittable`, `IPacifiable` e `IDamageSource`, que es lo que los desacopla del jugador. **Lanzan:** `EnemyKilled`, `EnemyPacified`, `BossDefeated`. **Datos:** `EnemyData`, `BossData`, `HitboxData`, `ProjectileData`.

### 2.4 Contenido

- **DialogueSystem + DialoguePanel** `[Core]` — Reproduce un `DialogueData` (líneas, condiciones, elecciones con decisiones como dar o quitar objeto, dinero o efecto de ruta). **Necesita:** `GlobalVariablesSystem`. **Escucha:** `DialogueStartRequest`, `DialogueAdvance`. **Lanza:** `DialogueStarted`, `DialogueEnded` (con el primero, `GameStateSystem` entra en `Dialogue`).
- **NPCSystem** `[Core]` — Elige el diálogo de un `NPCData` según condiciones y prioridad, y puede iniciar una misión. **Necesita:** `GlobalVariablesSystem`. **Escucha:** `NPCInteractionRequest`. **Lanza:** `DialogueStartRequest`, `QuestStartRequest`.
- **QuestSystem + QuestLogPanel** `[Core]` — Misiones con objetivos (matar, recoger, hablar, alcanzar una flag), requisitos y recompensas; guarda su progreso. **Necesita:** `GlobalVariablesSystem`, `SaveSystem`, `RouteSystem` y una `QuestDatabase` en el Inspector. **Lanza:** `QuestStarted`, `QuestCompleted`.
- **RouteSystem / EventReactionSystem** `[Core]` — `RouteSystem` calcula una ruta dominante a partir de acciones con pesos (`RouteActionData`, `RouteData`, `RouteRules`). `EventReactionSystem` conecta eventos con variables y flags sin código (`EventReactionRules`). **Necesitan:** `GlobalVariablesSystem`. **Lanzan:** `RouteChanged`, `RouteActionTriggered`.
- **ShopManager + ShopPanel** `[Core]` — Compra en tiendas definidas con `ShopData`, con precio final según la ruta dominante; pide el estado `Menu` mientras está abierta. **Necesita:** `RouteSystem`. **Escucha:** `ShopOpenRequest`, `ShopPurchaseRequest`. **Lanza:** `ShopPurchaseSuccess`, `ShopPurchaseFailed`.
- **CutsceneSystem** `[Core]` — Ejecuta un `CutsceneData` (lista de comandos: mover, animar, diálogo, cámara, esperar, fundido, música, sonido, efecto), en serie o en paralelo. `CutsceneTrigger` `[Nivel]` la lanza con condición opcional; `CutsceneCharacterReference` `[Nivel]` registra personajes. **Necesita:** `TransitionController`.
- **MinigameSystem** `[Core]` — Instancia un prefab con un componente `IMinigame` (`StartGame`, `EndGame`), pide el estado `Minigame` y aplica la recompensa o la penalización de su `MinigameData`. **Necesita:** nada. **Escucha:** `MinigameStartRequest`, `MinigameEnded`.
- **MapSystem + MapPanel** `[Core]` — Salas visitadas, posición del jugador y pines (se guardan con la partida), dibujados desde `RoomMapData`, `MapDatabase` y `MapPinIconDatabase`. **Necesita:** `SaveSystem`. **Lanza:** `MapPinsChanged`, `SaveRequested`.

### 2.5 Datos (ScriptableObject)

Todos se crean desde **Assets → Create → Infraestructura2DAction**.

| Categoría | Datos |
|---|---|
| Núcleo | `SceneReference`, `AssetReferenceDatabase`, `SoundDatabase`, `FlagDatabase`, `VariableDatabase`, `FlagCondition` |
| Jugador | `AttackData`, `SurfaceData`, `ResourceConsumptionTable` |
| Cámara | `CameraData` |
| Enemigos | `EnemyData`, `BossData`, `HitboxData`, `ProjectileData` |
| Contenido | `DialogueData`, `NPCData`, `QuestData`, `QuestDatabase`, `ItemData`, `ItemDatabase`, `ShopData`, `CutsceneData`, `MinigameData` |
| Mapa | `RoomMapData`, `MapDatabase`, `MapPinIconDatabase` |
| Rutas y reacciones | `RouteData`, `RouteActionData`, `RouteRules`, `EventReactionRules` |

### 2.6 Herramientas de Editor

Todas están en el menú **Infraestructura2DAction**.

| Herramienta | Para qué sirve |
|---|---|
| Flag Generator | Genera los nombres de flags de una entidad (NPC, jefe, misión, objeto...) siguiendo una convención. |
| Flag Validator | Escanea el proyecto y detecta flags sin registrar, duplicadas, con posibles erratas, o IDs sin flag. |
| Content Creator | Crea los `ScriptableObject` de un elemento de contenido y los ordena por capítulo y categoría. |
| Scene Setup | Crea una escena de nivel con cámara, tilemaps, punto de aparición, entrada en Build Settings y `SceneReference`. |
| World State Inspector | En Play Mode, muestra y edita flags y variables. |
| Quest Debugger | En Play Mode, activa, completa y reinicia misiones. |
| Checkpoint Debugger | En Play Mode, teletransporta al jugador a cualquier `SpawnPoint`. |
| Dialogue Preview | Previsualiza un `DialogueData` sin entrar en Play Mode. |
| NPC Dialogue Map | Muestra el árbol de diálogos condicionados de un NPC. |
| Audio Preview | Reproduce clips de audio en el Editor. |
| Transform Sanity Checker | Detecta posiciones y escalas absurdas en las escenas abiertas. |

Además, el componente `ScaleReferenceGrid` dibuja una cuadrícula de referencia de escala en la vista de Escena (solo gizmo, no afecta al juego).

---

## 3. Cómo se monta la arquitectura

### 3.1 Dos tipos de escena

- **Escena Core (persistente):** tiene todos los sistemas, la interfaz, la cámara y el fundido. Se carga una vez y nunca se descarga.
- **Escenas de nivel (aditivas):** se cargan encima de la Core y se descargan al cambiar de sala. Ahí van el jugador, los puntos de aparición, el suelo, los enemigos, las puertas, los NPCs y los objetos.
- El menú principal es, técnicamente, una escena de nivel más, con su propio `MainMenuController`.
- En Build Settings, la escena Core tiene que ir la primera (índice 0). Después van el menú y los niveles.

### 3.2 Jerarquía de la escena Core

La puedes organizar como quieras, pero esta es la recomendada:

- **Bootstrap:** el `BootstrapSystem`.
- **Systems** (un objeto vacío que agrupa el resto):
  - Debug: `DebugSystem` (obligatorio para el Bootstrap). Dentro de él, `EventDebugger` y `ConsoleOverlay` son opcionales, puedes dejarlos vacíos si no te hace falta la consola en pantalla.
  - Input: `InputManager`.
  - GameState: `GameStateSystem`.
  - SceneFlow: `SceneFlowSystem`, con `TransitionController` en el mismo objeto (es obligatorio que estén juntos).
  - Settings: `SettingsSystem`.
  - Save: `SaveSystem`.
  - Resources: `ResourceManager`.
  - Pool: `PoolManager`.
  - GlobalVariables: `GlobalVariablesSystem`.
  - Audio: `AudioCore`, con `MusicManager` (y su `AudioSource`), `SFXManager` y `SnapshotController`.
  - Cámara: la Main Camera con un `CinemachineBrain`, una `CinemachineCamera` con `CinemachineConfiner2D`, y el `CameraSystem`.
  - Contenido (opcionales): `DialogueSystem`, `RouteSystem`, `EventReactionSystem`, `NPCSystem`, `QuestSystem`, `ShopManager`, `CutsceneSystem`, `MapSystem` y `MinigameSystem`.
  - Extras: `ApplicationLifecycleSystem` y `MenuHotkeyController`.
- **UI:**
  - Un Canvas con `UICore`, `UIStateController` y `HUDSystem`.
  - Los paneles: Pausa, Ajustes, Slots de guardado, Diálogo, Inventario (con diario de misiones y mapa metidos como pestañas) y Tienda.
  - Un Canvas aparte, por encima de todos los demás, con el `CanvasGroup` y la `Image` negra del fundido.
  - Un `EventSystem` con el Input System UI Input Module.

### 3.3 Referencias del BootstrapSystem

Para que un sistema funcione de verdad en el juego, tiene que estar conectado a mano en el Bootstrap. Si dejas su campo vacío, ese sistema no se inicializa, sin más; y si es de los obligatorios, el Bootstrap te lo dirá por consola.

**Obligatorias** (el Bootstrap avisa si falta alguna):
- Debug System, Input Manager, Game State System, Scene Flow System, Settings System, Save System, Resource Manager, Pool Manager, Global Variables System, Audio Core, Camera System.
- UI Core, UI State Controller y HUD System.

**Opcionales** (vacías, ese sistema simplemente no se usa):
- Dialogue System, Route System, Event Reaction System, NPC System, Quest System, Shop Manager, Cutscene System, Map System.
- Save Slot Panel, Inventory Panel, Quest Log Panel.
- Item Database (hace falta si vas a tener inventario).
- Target Frame Rate: 60 por defecto; con 0, no se toca.

### 3.4 Otras referencias de Inspector, sistema por sistema

- **SceneFlowSystem:** Initial Scene, un `SceneReference` con la primera escena que se carga al arrancar. Puede ser tu menú principal, o directamente el primer nivel si no piensas tener menú. También Resume Saved Game On Boot.
- **TransitionController:** el `CanvasGroup`, la `Image` y la duración del fundido.
- **InputManager:** el `InputActionAsset` con las acciones del jugador.
- **CameraSystem:** la `CinemachineCamera`, el `CinemachineConfiner2D` y un `CameraData` por defecto.
- **AudioCore:** `SoundDatabase`, `MusicManager`, `SFXManager` y `SnapshotController`.
- **MusicManager:** un `AudioSource`. **SnapshotController:** cuatro snapshots del mixer (por defecto, pausa, diálogo y combate).
- **SFXManager:** el ID de pool que usa para los efectos. Por defecto es `AUDIO_SOURCE_SFX`, y debe existir en tu `AssetReferenceDatabase` con un prefab que lleve un `AudioSource`.
- **ResourceManager:** el `AssetReferenceDatabase`.
- **GlobalVariablesSystem:** las listas de `FlagDatabase` globales, los grupos por capítulo y categoría (cada grupo lleva solo un nombre informativo) y el `VariableDatabase`.
- **UICore:** la lista de paneles registrados. **UIStateController:** el `UICore` y el mapeo estado → `PanelID`.
- **HUDSystem:** la imagen de vida (de tipo Filled), un animator opcional, la imagen y el texto del recurso, y el texto de la moneda.
- **QuestSystem:** una `QuestDatabase`. **RouteSystem:** la lista de `RouteData` y un `RouteRules`. **EventReactionSystem:** un `EventReactionRules`. **MapSystem:** un `MapDatabase`.
- **DialogueSystem:** el `PanelID` del panel de diálogo por defecto. **ShopManager:** el `PanelID` de la tienda.
- **Paneles:**
  - `SaveSlotPanel`: los tres slots (botón y texto de cada uno) y la escena de nueva partida.
  - `PausePanel`: sus tres botones y la escena del menú.
  - `SettingsPanel`: los botones de volumen, daltonismo y vibración.
  - `DialoguePanel`: texto, retrato, prefab del botón de elección y su contenedor.
  - `ShopPanel`: texto del vendedor, retrato, contenedor del catálogo, prefab de entrada y botón de cerrar.
  - `InventoryPanel`: la lista de pestañas (botón y contenido de cada una), el contenedor de la cuadrícula y el prefab de hueco.

### 3.5 Prefab del jugador

Todos estos componentes van en el objeto raíz, porque se buscan entre sí en el mismo objeto (con `GetComponent`):

- `Rigidbody2D` y un `Collider2D`, con el tag `Player`.
- `PlayerSystem`, `PlayerMovementSystem`, `PlayerAbilityUnlocks`, `PlayerLockCoordinator`, `PlayerStateSystem` y `SurfacePhysicsSystem`.
- `PlayerCombatSystem`, `ShieldSystem` y `DamageReceiver`.
- `PlayerHealthSystem`, `PlayerResourceSystem`, `PlayerInventorySystem` y `PlayerCurrencySystem`.
- `InteractionSystem`.
- `PlayerAnimationController`, con su `Animator` y un `CharacterAnimationID`.

Objetos hijos:
- `GroundCheck` y `WallCheckFront`, para el movimiento.
- `HitboxOrigin`, para el ataque, y `ShieldOrigin` con su sprite, para el escudo.
- `PromptRoot`, opcional, el aviso de interacción.
- `CameraFollowTarget`, con su campo Player apuntando a la raíz del propio prefab.

Referencias que hay que asignar:
- `PlayerSystem`: sus cinco subsistemas (habilidades, salud, recurso, inventario y moneda).
- `PlayerInventorySystem`: combate, salud y recurso, para poder aplicar los efectos del equipamiento.
- `PlayerMovementSystem`: `GroundCheck`, `WallCheckFront` y la capa de suelo.
- `PlayerCombatSystem`: cuatro `AttackData` (normal, arriba, abajo y aéreo), el `HitboxOrigin`, y las capas de enemigo y de pacificación.
- `ShieldSystem`: el sprite, el origen, el tamaño y la capa de ataques enemigos.
- `DamageReceiver`: la capa de peligros. `InteractionSystem`: la capa de interactuables.
- `PlayerResourceSystem`: un `ResourceConsumptionTable`.

### 3.6 Escena de nivel

- Una instancia del prefab del jugador.
- Al menos un `SpawnPoint` con ID `Default`.
- Suelo (Tilemap) en la capa de suelo.
- Una zona de cámara: un `CameraTrigger` con un collider de límites.
- Opcionales: puertas (`SceneDoor`), puntos de guardado (`SavePoint`), NPCs, tiendas, enemigos, pickups y disparadores de cinemática.

### 3.7 Convenciones que sí o sí hay que respetar

- El jugador lleva el tag `Player`. Lo usan la cámara, las puertas, los pickups y las superficies.
- Los IDs tienen que ser únicos. Cada `UIPanel` tiene su `PanelID`, y el nombre de escena de cada `SceneReference` debe coincidir con el de Build Settings.
- Los paneles registrados en `UICore` deben empezar activos en la escena. `UICore` los desactiva al arrancar, y algunos se suscriben a eventos en su `Awake`, que solo se ejecuta si empiezan activos.
- El panel de diálogo lo abre el propio `DialogueSystem`. No lo asocies también al estado `Dialogue` en `UIStateController`, o intentarás abrirlo dos veces.
- La pantalla empieza en negro hasta que carga la primera escena. Es normal, no es un fallo.

---

## 4. Paso a paso en el Editor de Unity

### Paso 1. Preparar el proyecto

- Usa Unity 6 (desarrollado con la 6000.3).
- En el Package Manager instala Input System (acepta el reinicio que te pide), Cinemachine 3 y TextMeshPro (importando sus recursos esenciales). Comprueba que el módulo 2D Tilemap está activo.
- En Project Settings → Player, deja Active Input Handling en Input System Package, o en Both.
- Copia la carpeta `Infraestructura2DAction` dentro de tu `Assets`.
- Espera a que compile. La consola no debe mostrar errores en rojo antes de seguir.

### Paso 2. Tag y capas

- Crea el tag `Player`.
- Crea capas para `Ground`, `Player`, `Enemy`, `Interactable` y `Hazard`. Los nombres son libres, estos son solo una sugerencia.

### Paso 3. Input Actions

- Crea un asset de Input Actions con tres action maps: `Gameplay`, `UI` y `Global`.
- Gameplay: `Move` (Vector2, con teclado y mando), `Jump`, `Dash`, `CrouchSlide`, `LookUp`, `LookDown`, `Attack`, `Shield`, `Heal`, `Pacify` e `Interact`. Los nombres tienen que ser exactamente estos, o el `InputManager` no las va a encontrar.
- UI: `DialogueAdvance` y `Cancel`.
- Global: `Pause`, `OpenInventory` y `OpenMap`.
- Guarda el asset. Lo vas a asignar en el `InputManager`.
- Nota: `CrouchSlide` se lee como acción, pero de fábrica ningún sistema del paquete reacciona a ella. Está ahí preparada para que la enganches a tu propia mecánica de agacharse o deslizarse cuando la necesites; si no la usas, no pasa nada por dejarla sin bindear.

### Paso 4. Datos base

Con Assets → Create → Infraestructura2DAction, crea:

- Un `AssetReferenceDatabase`, con una entrada `AUDIO_SOURCE_SFX` cuyo prefab sea un objeto con un `AudioSource`, y un tamaño de pool de unos 10.
- Un `SoundDatabase`.
- Un `FlagDatabase` y un `VariableDatabase`.
- Un `CameraData`.
- Un `ResourceConsumptionTable`, con las claves `Heal` y `ShieldPerSecond`.
- Un `ItemDatabase` y un `QuestDatabase`.
- Cuatro `AttackData` para el jugador.

### Paso 5. Sistemas de la escena Core

- Crea una escena nueva llamada Core.
- Crea los objetos de Systems descritos en 3.2 y añade a cada uno su componente.
- En SceneFlow, añade el `SceneFlowSystem` y comprueba que el `TransitionController` aparece automáticamente en el mismo objeto.
- En Audio, añade los tres subsistemas como hijos o en el mismo objeto, y asigna el `AudioSource` al `MusicManager`.
- Asigna en cada componente las referencias de 3.4: el `InputActionAsset` al `InputManager`, el `AssetReferenceDatabase` al `ResourceManager`, el `SoundDatabase` al `AudioCore`, y así con el resto.

### Paso 6. Cámara

- La Main Camera lleva un componente `CinemachineBrain`.
- Crea una `CinemachineCamera` y añádele un `CinemachineConfiner2D`.
- En el `CameraSystem`, asigna la `CinemachineCamera`, el Confiner y el `CameraData` por defecto.
- Esto configura la cámara a nivel de Core, que es genérica para todo el juego. Los límites reales de cada sala se añaden por separado, en cada escena de nivel: un collider con un `CameraTrigger` y su propio `CameraData` (con su zoom y su comportamiento). Ese paso lo verás con más detalle en la Fase 5.1, cuando montes tu primera escena.

### Paso 7. Interfaz mínima

- Crea un Canvas con `UICore`, `UIStateController` y `HUDSystem`.
- Crea los paneles con un componente que herede de `UIPanel` (`PausePanel`, `SettingsPanel`, `SaveSlotPanel`, `DialoguePanel`, `InventoryPanel`, `ShopPanel`) y dale a cada uno un `PanelID` único: por ejemplo `Pause`, `Settings`, `SaveSlot`, `Dialogue`, `Inventory` y `Shop`.
- Deja todos los paneles activos en la jerarquía. `UICore` los desactivará él solo al arrancar.
- Añade todos esos paneles a la lista de `UICore`.
- En `UIStateController`, asocia el estado `Pause` al `PanelID` del panel de pausa. No asocies `Dialogue` (ver 3.7).
- Crea el Canvas del fundido, con un `CanvasGroup` y una `Image` negra a pantalla completa. Ponlo por encima de todo (Sort Order alto) y asígnalos al `TransitionController`.
- Añade un `EventSystem` con el Input System UI Input Module.
- Asigna en el `HUDSystem` las imágenes y textos de vida, recurso y moneda.

### Paso 8. Referencias del Bootstrap

- Selecciona el objeto Bootstrap y arrastra cada sistema a su campo correspondiente, según 3.3.
- En `SceneFlowSystem`, por ahora deja Initial Scene vacío. Lo rellenarás cuando crees la primera escena de verdad.

### Paso 9. Build Settings y primera prueba

- Abre File → Build Profiles (o Build Settings) y añade la escena Core la primera.
- Dale a Play. Deberías ver en la consola, entre otros mensajes, "Core initialized. All systems ready."
- Si falta el Initial Scene, verás un error de SceneFlow avisándolo. Es justo lo esperado en este punto, no te preocupes.
- Si algún paso de inicialización falla, el Bootstrap lo marca en rojo con el nombre del paso que falló. Revisa las referencias de ese sistema en concreto.

### Paso 10. Prefab del jugador

- Crea un objeto vacío con `Rigidbody2D` y un `Collider2D`. Ponle el tag `Player` y la capa `Player`.
- Añade los componentes de 3.5 en el objeto raíz.
- Crea los hijos `GroundCheck`, `WallCheckFront`, `HitboxOrigin`, `ShieldOrigin` (con un `SpriteRenderer`) y `CameraFollowTarget`.
- Asigna las referencias: los cinco subsistemas en `PlayerSystem`, los tres del `PlayerInventorySystem`, los checks y la capa de suelo en el movimiento, los cuatro `AttackData` en el combate, el `ResourceConsumptionTable`, y el Player del `CameraFollowTarget`.
- Convierte el objeto en prefab.
- El jugador se inicializa solo cuando aparece de verdad en una escena de nivel. Eso ya toca en el siguiente paso: montar tu primera escena.

---

## 5. Tu primera escena y cómo añadir contenido

### 5.1 Montar una escena de nivel sencilla

Con la herramienta Scene Setup:

- Abre Infraestructura2DAction → Scene Setup.
- Escribe el nombre de la escena y el Chapter (por ejemplo `Chapter_01`). La escena se guarda en `Assets/Scenes/Chapter/`, y su `SceneReference` en `Assets/Content/Chapter/SceneReferences/`.
- Marca: zona de cámara, Tilemap, SpawnPoint, Build Settings y SceneReference.
- Desmarca "Añadir CameraFollowTarget", porque ya lo metiste dentro del prefab del jugador en la Fase 4.
- Desmarca "Main Camera", porque la cámara vive en la escena Core.
- Pulsa Crear.

Después, en el Editor:

- Abre la escena creada y pinta el suelo en `Tilemap_Ground`. Tiene que estar en la capa Ground.
- Arrastra el prefab del jugador a la escena. En cuanto cargue, SceneFlow lo colocará en el SpawnPoint.
- Ajusta el polígono de CameraZone al tamaño real de la sala. El jugador debe quedar dentro de él.
- Asigna un `CameraData` a esa CameraZone (aquí es donde defines el zoom y el comportamiento de esta sala en concreto).
- Comprueba que existe un `SpawnPoint` con ID `Default`.

Para probarla:

- Abre la escena Core, selecciona el `SceneFlowSystem` y arrastra el `SceneReference` de tu escena al campo Initial Scene.
- Dale a Play desde Core. Deberías ver un fundido desde negro, el jugador en el SpawnPoint, la cámara siguiéndolo, y el movimiento funcionando.

Problemas habituales:

- El jugador cae sin parar: falta asignar la capa de suelo en `PlayerMovementSystem`.
- La cámara no sigue al jugador: revisa el campo Player del `CameraFollowTarget` y las referencias del `CameraSystem`.
- El jugador no se mueve: falta el `InputActionAsset` en el `InputManager`, o los nombres de acción no coinciden exactamente.
- No carga nada: la escena no está en Build Settings, o su nombre no coincide con el del `SceneReference`.

### 5.2 Conectar dos salas

- En la primera escena, crea un objeto con un collider (Is Trigger) y el componente `SceneDoor`.
- En Target Scene, asigna el `SceneReference` de la sala de destino.
- En Target Spawn Point ID, escribe el ID de un `SpawnPoint` que exista de verdad en la sala de destino.
- Opcionales: el color del fundido, una condición (`FlagCondition`) para poder cruzarla, y un coste de recurso.
- Con Trigger On Touch activado, basta con tocar la puerta. Si lo desactivas, hay que interactuar, y el objeto debe estar en la capa Interactable.
- Repite en la otra sala para poder volver. Las dos escenas deben estar en Build Settings.

### 5.3 Puntos de guardado

- Crea un objeto con un `SpawnPoint` (con un ID único) y un `SavePoint`, más un collider en la capa Interactable.
- Al interactuar con él, el jugador reaparecerá ahí la próxima vez, y se guardará la partida en ese momento.

### 5.4 Menú principal y nueva partida

- Crea una escena de menú con un Canvas, sus botones y un `MainMenuController` con esos botones asignados. Añádela a Build Settings.
- En `SceneFlowSystem`, pon esa escena de menú como Initial Scene y desactiva Resume Saved Game On Boot. Si lo dejas activo y el slot 1 ya tiene partida, el juego arrancará directamente ahí, saltándose el menú.
- En `SaveSlotPanel`, asigna la escena de nueva partida (tu primer nivel).
- El flujo queda así: Continuar o Nueva partida abre el panel de slots; al elegir uno, se reanuda la partida guardada de ese slot, o se carga la escena de nueva partida si estaba vacío.

### 5.5 Flags y variables

Son la base de las condiciones, las misiones, las puertas y los diálogos.

- Crea un `FlagDatabase` con Assets → Create → Infraestructura2DAction → Flags. Cada entrada lleva ID, descripción y valor por defecto. Puedes usar un prefijo y un sufijo comunes para todo el database.
- Crea un `VariableDatabase` igual, pero con valores numéricos.
- Asígnalos al `GlobalVariablesSystem`: flags globales, grupos por capítulo y variables.
- Cualquier flag o variable que uses en una condición debe estar registrada, o te avisará por consola.
- Una condición se crea con Create → Flags → Flag Condition. Sus tipos son: flag verdadera, flag falsa, variable mayor que, menor que, o igual a un valor.
- Convención de nombres: Flag Generator te da los nombres ya formados (por ejemplo `NPC_ID_MET` o `BOSS_ID_DEFEATED`). Flag Validator, después, revisa que todo lo que usas esté registrado.

### 5.6 Diálogos

- Crea un `DialogueData` con Create → Content → Dialogue Data.
- Campos: `DialogueID`, `Dialogue Panel ID` (vacío usa el panel por defecto) y la lista de líneas.
- Cada línea tiene personaje, texto, retrato, una condición opcional, una línea alternativa y sus elecciones.
- Una condición no se salta la línea sin más: si no se cumple y hay línea alternativa, se muestra esa. Si no hay alternativa, se muestra la línea normal igualmente.
- Cada elección tiene texto, una decisión y a qué línea salta después (con -1 sigue en orden). Las decisiones posibles son: ninguna, dar objeto, quitar objeto, dar o cobrar dinero, efecto de ruta, y marcar al NPC como ayudado.
- Se lanza desde un NPC, desde una cinemática (comando Show Dialogue), o por código con un `DialogueStartRequestEvent`.
- Para comprobarlo sin jugar: Dialogue Preview te enseña el diálogo entero sin entrar en Play Mode.

### 5.7 NPCs

- Con Content Creator (tipo NPC) se crean el `NPCData` y un diálogo inicial, en `Assets/Content/Chapter/NPCs` y `Dialogues`.
- `NPCData` lleva: ID, efecto sobre el recurso del jugador, misión vinculada opcional, estados de diálogo (condición, diálogo y prioridad; gana el válido con más prioridad), y un diálogo de reserva.
- En la escena, crea el objeto del NPC con un collider en la capa Interactable y el componente `NPCInteractable` con su `NPCData`.
- Al interactuar, el `NPCSystem` elige el diálogo que toca, marca al NPC como conocido, y lanza la misión vinculada si la tiene.
- Para comprobarlo: NPC Dialogue Map te muestra el árbol entero de un NPC.

### 5.8 Objetos, equipamiento y moneda

- `ItemData` lleva: ID, nombre, descripción, icono, tipo (`Consumable`, `Equipable`, `QuestKey` o `Material`) y precio base. Si es `Equipable`, indica qué modifica (`AttackDamage`, `MaxHP`, `HealRecoveryTime` o `MaxResource`) y cuánto.
- Añádelo al `ItemDatabase` que asignas en el Bootstrap.
- En la escena, un `ItemPickup` con un collider Is Trigger y el ID del objeto lo entrega al tocarlo. `CurrencyPickup` hace lo mismo con moneda, y `ResourcePickup` con recurso (este último es interactuable, no automático).
- `AbilityPickup` activa una flag de habilidad (por ejemplo `ABILITY_DASH_UNLOCKED`). Esa flag tiene que estar registrada y coincidir con la que usa `PlayerAbilityUnlocks`.

### 5.9 Misiones

- `QuestData` lleva: ID, texto, requisitos (misión previa, ruta dominante, condición de inicio), objetivos y recompensas (objeto y moneda).
- Cada objetivo tiene un tipo, un ID objetivo y una cantidad. Ese ID objetivo puede ser un `EnemyID` (matar), un `ItemID` (recoger), un `NPCID` (hablar) o una flag (alcanzar).
- Añade la misión al `QuestDatabase` que usa el `QuestSystem`. Sin eso, no se restaura al cargar la partida.
- Se inicia desde el NPC vinculado, o por código con un `QuestStartRequestEvent`.
- Para comprobarlo: Quest Debugger te deja activar, completar y reiniciar misiones en Play Mode.

### 5.10 Tiendas

- `ShopData` lleva: ID, nombre del vendedor, retrato, catálogo (objeto y, si quieres, un precio propio), frases del vendedor y multiplicadores de precio por ruta.
- En la escena, un `ShopInteractable` con su `ShopData` y un collider en la capa Interactable.
- El `PanelID` del `ShopPanel` tiene que coincidir con el que tenga asignado el `ShopManager`.
- Al abrirse, el juego pasa al estado `Menu`; se cierra con su propio botón.

### 5.11 Enemigos y jefes

- Datos: un `EnemyData` (ID, vida, velocidad, radio de detección, patrón de movimiento, puntos de ruta, alcance, cooldown, umbral de pacificación y botín) con su lista de ataques. Cada ataque enlaza un `HitboxData` (cuerpo a cuerpo) o un `ProjectileData` (proyectil), y su rango de uso.
- Para un jefe: un `BossData` con una lista de fases (porcentaje de vida en el que empieza cada una, su movimiento y sus ataques).
- Prefab del enemigo:
  - Un `Collider2D`, en una capa que esté dentro de la máscara de enemigos del `PlayerCombatSystem`.
  - Los componentes `EnemySystem`, `AIController` y `AttackHandler`, todos con el mismo `EnemyData` (o `BossData`) asignado.
  - Un objeto hijo `DetectionOrigin` y otro `HitboxOrigin`, con la capa del jugador metida en las máscaras de `AIController` y `AttackHandler`.
  - Un `Rigidbody2D` si va a usar cargas, y un `Animator` si quieres.
- Proyectiles: el prefab lleva `Rigidbody2D`, un collider Is Trigger y `EnemyProjectile`. Regístralo en el `AssetReferenceDatabase` con el mismo ID que pusiste en `ProjectileData` (`PoolAssetID`) y un tamaño de pool. La capa del proyectil tiene que estar en la máscara de peligros del `DamageReceiver`.
- Botín: activa Drops Loot y pon en Drop Pool ID el ID de un prefab que ya esté registrado en el `AssetReferenceDatabase`.
- Jefes: su flag `BOSS_ID_DEFEATED` tiene que estar registrada. Un jefe ya derrotado no vuelve a aparecer.
- Pacificar: con un umbral mayor que cero, al bajar de esa vida el enemigo queda debilitado y el jugador puede pacificarlo con la acción Pacify.

### 5.12 Rutas y reacciones

- `RouteData`: ID, nombre y puntuación inicial. Añádelos a la lista del `RouteSystem`, junto con un `RouteRules` opcional que limite qué transiciones están permitidas.
- `RouteActionData`: un ID y sus pesos por ruta. Se dispara desde una elección de diálogo, o desde una reacción.
- `EventReactionRules`: una lista de reacciones, cada una con su disparador (enemigo muerto, enemigo pacificado, jefe derrotado, escena cargada o NPC ayudado), un ID opcional, y un efecto (incrementar o fijar una variable, fijar una flag, o disparar una acción de ruta). En la clave puedes usar `{ID}` para que se sustituya por el ID real del evento. Asígnalo al `EventReactionSystem`.

### 5.13 Cinemáticas

- `CutsceneData` lleva: un ID y una lista de comandos ordenados. Cada comando tiene un tipo (mover, animar, diálogo, cámara, esperar, fundido, música, sonido, efecto) y su propio dato: personaje, posición, duración, animación, diálogo, color, ID de audio o efecto.
- Cada comando puede esperar a terminar antes del siguiente, o ejecutarse en paralelo con él.
- Los personajes que aparezcan en una cinemática llevan un `CutsceneCharacterReference`, y un `CharacterAnimationID` cuyo ID coincida con el Target Character ID del comando. El jugador también necesita el suyo.
- Para lanzarla, coloca un `CutsceneTrigger` (collider Is Trigger) con la cinemática, una condición opcional, y la opción de que se reproduzca una sola vez. Si usas una flag para recordar que ya se reprodujo, esa flag debe estar registrada.
- Limitación que conviene saber: el comando de enfoque de cámara necesita un collider como límite, pero un `ScriptableObject` no puede guardar una referencia a un objeto de escena. De momento solo funciona bien con colliders que vivan dentro de un prefab.

### 5.14 Minijuegos

- `MinigameData` lleva: ID, prefab, recompensa (curar, dar moneda o dañar a un jefe), cantidad, daño si fallas, y parámetros de dificultad libres.
- El prefab debe llevar un componente que implemente `IMinigame` (`StartGame` y `EndGame`), y lanzar un `MinigameEnded` con el resultado al terminar.
- No viene ningún componente de disparo hecho: lo inicias tú mismo con un `MinigameStartRequestEvent`, por ejemplo desde un interactuable propio.

### 5.15 Mapa

- Un `RoomMapData` por cada sala: nombre de escena (idéntico al de la escena real), posición en la cuadrícula del mapa, silueta, y límites en coordenadas de mundo.
- Añádelos al `MapDatabase`, que asignas al `MapSystem`.
- El `MapPanel` se coloca como contenido de una pestaña del `InventoryPanel`, con sus referencias asignadas. Los atajos de inventario y mapa se configuran en `MenuHotkeyController`, asociando cada uno al `PanelID` del panel que los contiene.

### 5.16 Audio

- `SoundDatabase`: cada entrada lleva ID, clip, volumen y si es música o no.
- Para que los sonidos por defecto se oigan, crea entradas con estos IDs exactos: `SFX_UI_OPEN`, `SFX_UI_CLOSE`, `SFX_SAVE_POINT`, `SFX_ITEM_PICKUP`, `SFX_CURRENCY_COLLECT`, `SFX_RESOURCE_COLLECT`, `SFX_DOOR_LOCKED`, `SFX_SHIELD_BLOCK`, `SFX_DIALOGUE_BLIP` y `SFX_ABILITY_UNLOCKED`.
- La música se pide con un `MusicPlayRequestEvent`, o con el propio comando de música de una cinemática.
- `AudioEventListener` te deja atar un sonido a una interacción, a la muerte de un enemigo, o al cambio de una flag concreta.

### 5.17 Superficies

- `SurfaceData`: ID, comportamiento (estándar, agua, direccional o rebote), y sus valores propios.
- Coloca un `SurfaceDetector` con ese dato en el suelo, o en una zona con collider Is Trigger. El jugador tiene que llevar el tag `Player` para que lo detecte.

### 5.18 Comprobar que todo está bien

- Flag Validator: te dice si tienes flags sin registrar o duplicadas.
- World State Inspector: muestra y te deja editar flags y variables en Play Mode.
- Quest Debugger y Checkpoint Debugger: para probar misiones y teletransportarte por los puntos de guardado.
- Transform Sanity Checker: encuentra posiciones y escalas absurdas que se te hayan colado en las escenas.

---

## 6. Requisitos, instalación y licencia

### 6.1 Requisitos

- Unity 6, probado en la 6000.3.
- Paquetes de Unity: Input System (`com.unity.inputsystem`, 1.18.0 o superior), Cinemachine (`com.unity.cinemachine`, 3.1.7 o superior) y UI/TextMeshPro (`com.unity.ugui`, 2.0.0 o superior — en Unity 6 este paquete ya trae TextMeshPro integrado, no hace falta instalarlo aparte).
- Módulos integrados del motor: Tilemap, Physics2D, Audio y Animation. Vienen con el editor; solo hace falta que no estén desactivados en el Package Manager.
- Recomendado, aunque no es una dependencia directa del código: `com.unity.2d.tilemap` (te da el Tile Palette para pintar el suelo a mano) y `com.unity.2d.sprite`.
- La primera vez que uses un campo de texto TextMeshPro en un proyecto nuevo, Unity te pedirá importar sus "TMP Essential Resources". Acéptalo.

### 6.2 Instalación

Hay dos formas de meter el paquete en tu proyecto:

- **Copiar la carpeta a mano.** Copia `Infraestructura2DAction` dentro de tu `Assets`. Es la forma más simple, y es la que sigue la guía paso a paso de la Fase 4.
- **Por Git URL, desde el Package Manager** (recomendada si el repositorio ya está en GitHub). Ventana → Package Manager → botón + → Add package from git URL, y pega la URL del repositorio. Unity lo instala como paquete de verdad (gracias al `package.json` y a los dos `.asmdef` que trae), fuera de tu carpeta `Assets`, sin mezclarse con tu propio código. Cuando el repositorio tenga versiones etiquetadas, puedes fijar una en concreto añadiendo `#nombre-del-tag` al final de la URL.

### 6.3 Licencia

Todo el paquete se publica bajo licencia MIT: puedes usarlo, modificarlo y meterlo en proyectos comerciales sin pedir permiso, con la única condición de conservar el aviso de copyright. El texto completo está en el archivo `LICENSE`, en la raíz del paquete.
