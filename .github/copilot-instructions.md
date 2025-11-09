# UWAFT CAV Game – AI Agent Instructions

## Project Overview

Educational EV racing game for the University of Waterloo EcoCAR Team. Players progress through six learning nodes (EV components), completing slides, quizzes, and mini‑games to unlock car upgrades. Built in **Unity 2023+** using **2D physics, URP, Addressables, and Unity Gaming Services** for leaderboards.

**Architecture:** Two main gameplay loops:

1. **NodeMap System** – Learning progression (slides, quizzes, minigames, upgrades)
2. **Driving Scene** – Physics‑based 2D car racing with torque‑driven movement

---

## 🔧 Available MCP Tools (Use Proactively)

You have access to Model Context Protocol (MCP) servers. **Use these tools proactively** whenever they would be helpful—you don't need explicit user permission.

### **Exa MCP** (Research & Web Search)
**When to use:**
- Looking up Unity best practices, design patterns, or C# features
- Researching game development techniques (animations, state machines, etc.)
- Finding solutions to technical problems or errors
- Checking latest library/package documentation
- General web research for implementation ideas

**How to use:**
```
Use mcp_exa_web_search_exa for general web searches
Use mcp_exa_get_code_context_exa for code-specific searches (APIs, libraries, SDKs)
```

**Examples:**
- "Unity dependency injection patterns" → Use Exa for articles/examples
- "C# Result<T> monad implementation" → Use Exa for code samples
- "Best practices for Unity Addressables async loading" → Use Exa for documentation

---

### **Context7 MCP** (Library Documentation)
**When to use:**
- Need official documentation for Unity APIs
- Looking up Addressables, TextMeshPro, or URP documentation
- Understanding C# language features or .NET APIs
- Checking Unity Gaming Services APIs
- Any time you need authoritative, up-to-date library docs

**How to use:**
```
1. Call mcp_upstash_conte_resolve-library-id to get the library ID
2. Call mcp_upstash_conte_get-library-docs with the resolved ID
```

**Examples:**
- "How does Unity Addressables async/await work?" → Resolve "unity/addressables" → Get docs
- "TextMeshPro text formatting tags" → Resolve "unity/textmeshpro" → Get docs
- "C# async/await best practices" → Resolve "dotnet/csharp" → Get docs

---

### **GitHub MCP** (Repository Code Search)
**When to use:**
- Searching for patterns in the uwaft-cav-game repository
- Finding references to specific components or methods
- Understanding how existing V1 code works
- Checking for similar implementations in the codebase

**How to use:**
```
Use github_repo with repo="vinny-nguyen/uwaft-cav-game"
```

**Examples:**
- "How is AssetLoadingService currently used?" → Search repo
- "Find all references to MapState.TryCompleteNode" → Search repo
- "What mini-games exist in the project?" → Search repo

---

### **Prisma MCP** (Database Management)
**When to use:**
- If the project ever needs database integration
- Managing player progression in a backend (future feature)
- Leaderboard data persistence (when implementing Unity Gaming Services alternative)

**Currently:** Not needed for V2 (using PlayerPrefs), but available for future features.

---

## 🎯 MCP Usage Guidelines

### **Be Proactive**
- ❌ DON'T wait for user to say "search for X"
- ✅ DO automatically search when you need information
- ✅ DO use Context7 when referencing Unity/C# APIs
- ✅ DO use Exa when researching best practices

### **When to Use Which MCP**
| Situation | MCP to Use |
|-----------|------------|
| "What's the Unity Addressables API for async loading?" | Context7 → unity/addressables |
| "Best practices for Unity dependency injection" | Exa → Code context search |
| "How does V1 PopupController work?" | GitHub repo search |
| "Unity coroutine vs async/await performance" | Exa → Web search |
| "TextMeshPro rich text tags" | Context7 → unity/textmeshpro |
| "C# Result<T> monad examples" | Exa → Code context search |

### **Transparent Usage**
When you use an MCP, briefly mention it:
- ✅ "Let me check the Unity Addressables docs..." (then use Context7)
- ✅ "Searching for best practices..." (then use Exa)
- ✅ "Looking at how V1 handles this..." (then use GitHub search)

This keeps the user informed without asking permission.

---

## Documentation Map

This file provides a **quick primer** for AI agents. For deeper details, see:

| When you need to... | Read this file |
|---------------------|----------------|
| Understand overall architecture | [SYSTEM_OVERVIEW.md](../Assets/Scripts/Nodemap/Docs/SYSTEM_OVERVIEW.md) |
| Learn gameplay progression flow | [NODE_AND_LEARNING_FLOW.md](../Assets/Scripts/Nodemap/Docs/NODE_AND_LEARNING_FLOW.md) |
| Look up component methods/events | [COMPONENT_API.md](../Assets/Scripts/Nodemap/Docs/COMPONENT_API.md) |
| Build content creation tools | [CONTENT_CREATION.md](../Assets/Scripts/Nodemap/Docs/CONTENT_CREATION.md) |

**AI Agent Workflow:**
1. Read this file first (5 min) ← You are here
2. Read SYSTEM_OVERVIEW.md (10 min) ← Architecture deep dive
3. Reference COMPONENT_API.md as needed ← Quick lookup while coding
4. Read role-specific docs based on your task

---

## AI Agent Quick Start

### 🤖 I'm helping with **Coding/Bug Fixes**
1. Read: SYSTEM_OVERVIEW.md → COMPONENT_API.md
2. Key Principles:
   - Never modify MapState directly (use TryCompleteNode, TryMoveCarTo)
   - All config via MapConfig (no hardcoded values)
   - Events for communication (not direct calls)
   - Check COMPONENT_API.md for method signatures before editing
3. Before editing, check: "Does this maintain unidirectional data flow?"

### 🎨 I'm helping with **Content Tools/Workflow**
1. Read: CONTENT_CREATION.md → NODE_AND_LEARNING_FLOW.md
2. Key Constraints:
   - Designers don't write code
   - All assets through Resources/ folder
   - PowerPoint → PNG export → Slide Deck Builder tool
3. Goal: Make content creation **zero-code** for designers

### 🏗️ I'm helping with **Architecture/Refactoring**
1. Read: SYSTEM_OVERVIEW.md → All other docs
2. Key Patterns:
   - Event-driven (State → Events → Views)
   - ConfigurableComponent base class
   - Type-safe NodeId wrapper
3. Before proposing changes: Check if it fits existing patterns

### 🧪 I'm helping with **Testing/QA**
1. Read: NODE_AND_LEARNING_FLOW.md → COMPONENT_API.md
2. Critical Flows:
   - Sequential node unlocking (no skipping)
   - Quiz review flow (wrong → review slides → restart)
   - Mini-game blocking (Next disabled until complete)
3. Test: Full playthrough from Node 0 → Node 5

---

## Core NodeMap Architecture

### 1. Event‑Driven State Management

The NodeMap follows a **unidirectional data flow** pattern for clean separation of state, view, and logic.

```
MapState (single source of truth)
  ↓ events
MapControllerSimple (orchestrator)
  ↓ commands
NodeManagerSimple, CarMovementController, PopupController (view/controllers)
```

* `MapState` holds all persistent data: node statuses, active node index, completion states, and car position.
* All changes to gameplay state must go through **MapState methods**:

  ```csharp
  bool TryCompleteNode(NodeId id);
  bool TryMoveCarTo(NodeId id);
  ```
* **Never modify state directly in views.** They dispatch actions; only the state layer mutates data.

**Files:** `Assets/Scripts/Nodemap/Core/MapState.cs`, `MapControllerSimple.cs`, `NodeManagerSimple.cs`

---

### 2. ConfigurableComponent Pattern

Every functional MonoBehaviour under the NodeMap inherits from `ConfigurableComponent`, giving safe, centralized access to configuration values in `MapConfig`.

```csharp
public class PopupController : ConfigurableComponent {
    void Start() {
        float fadeDur = GetConfig(c => c.popupFadeDuration, 0.25f);
    }
}
```

* Configuration lives in `Resources/Config/MapConfig.asset`
* All tunables (move speeds, animation durations, car offsets, popup timings) are stored there.
* **Never hardcode** these values; use `GetConfig()` calls with fallbacks instead.

**Files:** `Assets/Scripts/Nodemap/Config/MapConfig.cs`

---

### 3. NodeId Value Object

Type‑safe wrapper around node indices to prevent misuse of raw ints.

```csharp
NodeId current = new NodeId(0);
if (current.IsValid(nodeCount)) {
    NodeId next = current.GetNext(nodeCount);
}
```

Adds clarity and avoids index‑off‑by‑one issues.

**Files:** `Assets/Scripts/Nodemap/Core/NodeId.cs`

---

### 4. Asset Loading (Addressables)

All assets (sprites, prefabs, slide decks) load via `AssetLoadingService` to ensure clean handle management.

```csharp
AssetLoader.LoadSprite("Sprites/Nodes/Active/node1", sprite => icon.sprite = sprite);
```

Never handle Addressable `AsyncOperationHandle` directly. The loader handles caching and cleanup internally.

**File:** `Assets/Scripts/Nodemap/Services/AssetLoadingService.cs`

---

## NodeMap Component Overview

### 1. Node Data & Content Flow

Each node uses a `NodeData` ScriptableObject with:

* **Slide Deck** (`SlideDeck` asset) – prefab list for learning slides
* **Quiz Data** – JSON file under `Resources/quiz_data.json`
* **MiniGames** – prefab references (`DragDrop`, `MemoryMatch`, etc.)
* **Upgrade Assets** – frame and tire sprites applied post‑completion

**Files:** `Assets/Scripts/Nodemap/Content/NodeData.cs`
**Assets:** `Assets/Data/Nodes/*.asset`

#### Data Pipeline

```
NodeData → PopupController → QuizController → MiniGameController → MapState
```

Each step emits an event to the next controller to maintain decoupling.

---

### 2. Popup System

`PopupController` orchestrates all popups (slides, quizzes, upgrades) for each node.

#### Responsibilities

* Instantiate slide prefabs from `NodeData.slideDeck`.
* Animate popup in/out with fade duration from `MapConfig`.
* Handle quiz transitions and mini‑game launching.
* Handle slide navigation: `Next`, `Prev`, and direct jump via quiz references.

```csharp
PopupController.ShowLearning(NodeId id);
PopupController.ShowQuiz(NodeId id);
PopupController.ShowMiniGame(NodeId id);
PopupController.ShowUpgrade(NodeId id);
```

#### Quiz Integration

When the user answers incorrectly:

* `QuizController` emits `OnWrongAnswer(slideKey)`.
* `PopupController` calls `JumpToSlideByKey(slideKey)` in the learning popup.

On correct completion:

* Emits `OnQuizPassed()` → triggers mini‑game or upgrade.

**Files:** `Assets/Scripts/Nodemap/PopupController.cs`, `QuizController.cs`, `LearningPopup.cs`

---

### 3. Learning Slides

Slides are prefab‑based modules with optional hotspots and dropdowns.

Each slide prefab supports:

* **Hotspots:** Clickable UI buttons that trigger `TooltipPopup.Show(text)`.
* **Dropdowns:** Expandable sections for deeper explanations.
* **Media Panels:** Optional diagrams, short text, or animated GIFs.

Slide prefabs are created by designers—no code editing required.

**Files:** `Assets/Prefabs/Slides/*.prefab`
**Scripts:** `SlideBehaviour.cs`, `SlideDeckController.cs`

---

### 4. Quiz System

**Quiz Data Structure (JSON)**

```json
{
  "nodeId": 2,
  "questions": [
    {"prompt": "What component recovers kinetic energy?", "options": ["Motor", "BMS", "Inverter"], "correctIndex": 0, "referenceSlide": "s3"}
  ]
}
```

**QuizController.cs:**

* Loads data from `quiz_data.json`
* Renders questions dynamically (TextMeshPro + Buttons)
* Emits `OnWrongAnswer(slideKey)` and `OnQuizPassed()` events
* Integrates with `PopupController`

Each quiz is modular and can be attached to any node’s content flow.

---

### 5. Mini‑Game Integration

All mini‑games share a base interface:

```csharp
public interface IMiniGame {
    event Action<bool> OnCompleted; // success flag
    void Initialize(NodeId id);
    void StartGame();
}
```

Mini‑games notify `PopupController` via `OnCompleted` when finished, which updates `MapState.TryCompleteNode(id)`.

**Examples:**

* `WordUnscrambleController`
* `MemoryMatchController`
* `DragDropController`

---

### 6. Car Movement

`CarMovementController` animates the car along splines when nodes unlock or are clicked.

```csharp
public void MoveTo(NodeId id) {
    StartCoroutine(MoveCarRoutine(id));
}
```

* Movement speed = `MapConfig.moveSpeed`
* Events: `OnStartedMovingToNode`, `OnArrivedAtNode`
* Handles tire spin and simple suspension bounce

Car snaps to the active node on load and animates for transitions only.

**File:** `Assets/Scripts/Nodemap/Car/CarMovementController.cs`

---

## NodeMap Workflow (End‑to‑End)

```
User clicks node
  → NodeManagerSimple verifies active
  → PopupController.ShowLearning(node)
  → Learning slides navigate → Quiz starts
  → QuizController evaluates answers
     → wrong → jump to slide
     → pass → PopupController.ShowMiniGame()
  → MiniGame completes → MapState.TryCompleteNode()
  → CarMovementController.MoveToNextNode()
  → PopupController.ShowUpgrade(node)
  → Apply visual upgrades
```

**PlayerPrefs keys:** `NodeUnlocked_{i}`, `NodeCompleted_{i}`, `ActiveNode`, `CarNode`

---

## Testing & Development Utilities

### Node Progression Testing

* Scene: `NodeMapFullHD.unity`
* Debug methods:

  ```csharp
  MapControllerSimple.ResetProgression();
  MapState.TryMoveCarTo(new NodeId(2));
  ```
* Debug menu buttons in play mode allow unlocking or completing nodes manually.

### Adding New Nodes

1. Create `NodeData` asset.
2. Assign slide deck, quiz, and mini‑games.
3. Append to `NodeManagerSimple.nodeData` list.
4. Increment `MapConfig.nodeCount` if necessary.

---

## UI & Animation Standards

* Popups fade/scale in 200–300 ms (configurable).
* Car moves with ease‑in/out curve.
* Quiz transitions use fade and scale effects for clarity.
* Accessibility: All popups keyboard navigable.

**Use:** TextMeshPro for all text, `CanvasGroup` for popup transitions.

---

## Rules for AI Agents

### ✅ Always Do This
* **Read SYSTEM_OVERVIEW.md before making changes** - Understand patterns first
* **Use MapState methods** - `TryCompleteNode()`, `TryMoveCarTo()` (never direct field access)
* **Access config via GetConfig()** - `GetConfig(c => c.moveSpeed, 2.0f)` with fallback
* **Subscribe/unsubscribe in OnEnable/OnDisable** - Prevent memory leaks
* **Check COMPONENT_API.md** - Verify method signatures and events before editing
* **Follow existing patterns** - If 5 components do it one way, do it that way
* **One responsibility per script** - Keep components ≤200 lines

### ❌ Never Do This
* **Never modify MapState fields directly** - Always use methods
* **Never hardcode values** - Use MapConfig (speeds, durations, offsets)
* **Never call PlayerPrefs from views** - Only MapState handles persistence
* **Never use GameObject.Find() in Update()** - Cache references in Awake/Start
* **Never handle Addressables directly** - Use AssetLoadingService
* **Never skip state validation** - Always check `IsNodeUnlocked()` before operations
* **Never use UnityEvent for core logic** - Use C# events (`Action<T>`)

### 🎨 Event Naming Convention
* **Pattern:** `OnVerbPastTense`
* **Examples:** `OnNodeClicked`, `OnArrivedAtNode`, `OnQuizPassed`
* **Why:** Makes it clear the event fires AFTER the action completes

### 📁 File Naming Convention
* **Components:** `ComponentNameBehavior.cs` (e.g., `CarMovementController.cs`)
* **ScriptableObjects:** `TypeName.cs` (e.g., `NodeData.cs`, `MapConfig.cs`)
* **Interfaces:** `IInterfaceName.cs` (e.g., `IMiniGame.cs`)
* **Prefabs:** `PascalCase.prefab` (e.g., `WordUnscramble_Battery.prefab`)

### 🧩 When to Check Which Doc
| Scenario | Check This Doc |
|----------|----------------|
| "How do I complete a node?" | COMPONENT_API.md → MapState.TryCompleteNode |
| "What events does CarMovementController fire?" | COMPONENT_API.md → CarMovementController section |
| "How does the quiz review flow work?" | NODE_AND_LEARNING_FLOW.md → Quiz System section |
| "What's the slide navigation flow?" | NODE_AND_LEARNING_FLOW.md → Learning Session Flow |
| "Where should I put this new script?" | SYSTEM_OVERVIEW.md → Folder Structure section |
| "How do I add a new node?" | CONTENT_CREATION.md → Complete Workflow |

---

## Summary

The NodeMap subsystem is an **event‑driven, configuration‑driven flow** connecting learning content (slides, quizzes, mini‑games) through clean architecture patterns.

### For AI Agents: Next Steps

**Just starting?**
1. ✅ You've read this file (copilot-instructions.md)
2. → Read [SYSTEM_OVERVIEW.md](../Assets/Scripts/Nodemap/Docs/SYSTEM_OVERVIEW.md) next
3. → Bookmark [COMPONENT_API.md](../Assets/Scripts/Nodemap/Docs/COMPONENT_API.md) for reference

**Ready to code?**
- Check existing components for patterns
- Verify method signatures in COMPONENT_API.md
- Test in Play Mode before committing
- Update docs if you change behavior

**Building tools for designers?**
- Read CONTENT_CREATION.md first
- Goal: Zero-code workflow for content creation
- Test with non-technical users

**Questions?**
- Search the 4 docs (Ctrl+F works great)
- Check COMPONENT_API.md usage examples
- Look at existing code for patterns
- Ask the user if genuinely unclear

---

**Remember:** The goal is scalable, designer‑friendly extensibility with strong code boundaries, while keeping gameplay smooth, interactive, and educational.
