# System Overview

> **For AI Assistants:** This document provides a complete overview of the UWAFT CAV Game NodeMap system architecture.  
> Start here to understand the entire system before making changes.

**Last Updated:** November 9, 2025  
**Unity Version:** 2023.x  
**System Version:** NodeMap v2.0

---

## Table of Contents
1. [Quick Start](#quick-start)
2. [Architecture Overview](#architecture-overview)
3. [Core Principles](#core-principles)
4. [System Diagram](#system-diagram)
5. [Component Overview](#component-overview)
6. [Data Flow](#data-flow)
7. [Folder Structure](#folder-structure)
8. [Naming Conventions](#naming-conventions)

---

## Quick Start

**What is this?**  
An educational EV racing game for the University of Waterloo EcoCAR Team. Players progress through 6 learning nodes, each containing slides, mini-games, and quizzes. Completing nodes unlocks car upgrades and advances the player's journey.

**Core Gameplay:**
1. Click a node on the map
2. Learn through interactive slides (with embedded mini-games)
3. Pass a quiz to complete the node
4. Car moves to the next node
5. Receive visual upgrades
6. Repeat for 6 nodes

---

## Architecture Overview

The NodeMap system follows an **event-driven, unidirectional data flow** architecture:

```
User Input → MapController → MapState (Single Source of Truth)
                ↓
            State Changes
                ↓
            Events Fired
                ↓
   Views Update (NodeManager, CarController, PopupController)
```

**Key Architectural Patterns:**
- **Single Source of Truth** - MapState holds ALL game state
- **Event-Driven Communication** - Components communicate via C# events
- **Dependency Injection** - Components are wired together, not hardcoded
- **ConfigurableComponent** - Centralized config access pattern
- **Type Safety** - NodeId wrapper prevents index bugs

---

## Core Principles

### 1. **Separation of Concerns**
Each component has ONE job:
- `MapState` = Manage state
- `MapController` = Orchestrate components
- `NodeManager` = Render node visuals
- `CarController` = Move the car
- `PopupController` = Display learning content

### 2. **Unidirectional Data Flow**
State changes flow in ONE direction:
```
MapState.TryCompleteNode() 
  → fires OnNodeCompletedChanged event
  → MapController listens
  → Updates visuals via NodeManager
```

Never: `View → directly modify MapState`  
Always: `View → emit event → Controller → MapState`

### 3. **Designer-Friendly**
- Content lives in ScriptableObjects (NodeData, SlideDeck, QuizData)
- No code changes needed to add content
- Visual tools for creating slide decks
- Inspector-based configuration (MapConfig)

### 4. **Simplicity Over Cleverness**
- Explicit over implicit
- Readable over compact
- Boring over clever
- Comments explain "why", not "what"

---

## System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    MapControllerSimple                      │
│                   (Main Orchestrator)                       │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────────┐  │
│  │ Subscribe  │  │  Handle    │  │   Coordinate        │  │
│  │ to Events  │→ │  Actions   │→ │   Components        │  │
│  └────────────┘  └────────────┘  └─────────────────────┘  │
└───────┬──────────────────┬──────────────────┬──────────────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────────┐
│   MapState   │  │NodeManager   │  │ CarMovement      │
│              │  │              │  │ Controller       │
│ • Node       │  │ • Creates    │  │                  │
│   states     │  │   node views │  │ • Moves car      │
│ • Car pos    │  │ • Updates    │  │   on spline      │
│ • Active     │  │   visuals    │  │ • Wheel spin     │
│   node       │  │ • Spline     │  │ • Bounce         │
│              │  │   positions  │  │                  │
│ EVENTS:      │  │              │  │ EVENTS:          │
│ • OnNode     │  │ EVENTS:      │  │ • OnArrived      │
│   Completed  │  │ • OnNode     │  │   AtNode         │
│ • OnNode     │  │   Clicked    │  │ • OnStarted      │
│   Unlocked   │  │              │  │   Moving         │
│ • OnCarMoved │  │              │  │                  │
└──────────────┘  └──────────────┘  └──────────────────┘
        │                  │                  │
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────────────────────────────────────────────┐
│              LevelNodeView (6 instances)             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐          │
│  │  Node 1  │  │  Node 2  │  │  Node 3  │  ...     │
│  │ [Sprite] │  │ [Sprite] │  │ [Sprite] │          │
│  │ [Button] │  │ [Button] │  │ [Button] │          │
│  │  [Anim]  │  │  [Anim]  │  │  [Anim]  │          │
│  └──────────┘  └──────────┘  └──────────┘          │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│              PopupController (Content)                │
│  ┌──────────────────────────────────────────────┐   │
│  │  Learning Session                            │   │
│  │  • Slides (with mini-popups)                 │   │
│  │  • Mini-Games (embedded in slide flow)       │   │
│  │  • Quiz (final test)                         │   │
│  │  • Navigation (Next/Prev/Review)             │   │
│  └──────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────┘
```

---

## Component Overview

### **Core Systems**

#### **MapState** (`Core/MapState.cs`)
- **Purpose:** Single source of truth for all progression state
- **Responsibilities:** Track node unlock/completion, car position, active node
- **Key Feature:** Only way to modify game state (commands pattern)
- **Events:** OnNodeCompletedChanged, OnNodeUnlockedChanged, OnCarNodeChanged

#### **MapControllerSimple** (`MapControllerSimple.cs`)
- **Purpose:** Main orchestrator that wires all components together
- **Responsibilities:** Subscribe to events, coordinate state changes, handle user actions
- **Key Feature:** Thin controller - just wires things, no business logic
- **Dependencies:** MapState, NodeManager, CarController, PopupController

#### **NodeId** (`Content/NodeId.cs`)
- **Purpose:** Type-safe wrapper around node indices
- **Responsibilities:** Prevent mixing integers with node IDs
- **Key Feature:** Validation methods (IsValid, GetNext, GetPrevious)

---

### **Visual Components**

#### **NodeManagerSimple** (`NodeManagerSimple.cs`)
- **Purpose:** Manages all node visuals on the map
- **Responsibilities:** Create node views, position on spline, update visuals
- **Key Feature:** Converts NodeId to spline T values
- **Events:** OnNodeClicked

#### **LevelNodeView** (`Node/LevelNodeView.cs`)
- **Purpose:** Individual node button on the map
- **Responsibilities:** Display node sprite, handle clicks, trigger animations
- **Key Feature:** Loads sprites via Addressables based on state
- **States:** Inactive (gray/locked), Active (yellow/unlocked), Completed (green/done)

#### **NodeStateAnimation** (`Animations/NodeStateAnimation.cs`)
- **Purpose:** Visual feedback for node interactions
- **Responsibilities:** Pop animation (state change), Shake animation (locked click)
- **Key Feature:** Self-contained - manages own coroutines

---

### **Car System**

#### **CarMovementController** (`Car/CarMovementController.cs`)
- **Purpose:** Animates car movement between nodes
- **Responsibilities:** Follow spline path, rotate wheels, bounce effect
- **Key Feature:** Easing curves for smooth acceleration/deceleration
- **Events:** OnArrivedAtNode, OnStartedMovingToNode

#### **CarVisual** (`Car/CarVisual.cs`)
- **Purpose:** Manage car sprite upgrades
- **Responsibilities:** Swap frame/tire sprites when node completes
- **Key Feature:** Simple sprite swapping (currently not fully integrated)

---

### **Content System**

#### **PopupController** (`PopupController.cs`)
- **Purpose:** Display learning content (slides, quizzes)
- **Responsibilities:** Show slides, navigate, handle quiz flow
- **Key Feature:** Slide deck navigation with indicators
- **Note:** Being enhanced to support embedded mini-games

#### **NodeData** (`Content/NodeData.cs`)
- **Purpose:** ScriptableObject defining node content
- **Contains:** SlideDeck, QuizData, upgrade sprites, title
- **Key Feature:** Designer-friendly content creation

#### **SlideDeck** (`Content/SlideDeck.cs`)
- **Purpose:** Collection of slides for a node
- **Contains:** Array of SlideReference (slides + mini-games)
- **Key Feature:** Supports both learning slides and mini-game insertions

---

### **Mini-Games**

#### **WordUnscrambleController** (`Minigames/WordUnscrambleController.cs`)
- **Purpose:** Word puzzle mini-game
- **Implements:** Will implement IMiniGame interface
- **Key Feature:** Unlimited retries, blocks progression until correct

#### **MemoryMatchController** (`Minigames/MemoryMatch/MemoryMatchController.cs`)
- **Purpose:** Card matching mini-game
- **Implements:** Will implement IMiniGame interface
- **Key Feature:** Pair matching with flip animations

#### **DragDropController** (`Minigames/DragDropMatch/DragDropController.cs`)
- **Purpose:** Drag items to correct zones
- **Implements:** Will implement IMiniGame interface
- **Key Feature:** Visual feedback on drop zones

---

### **Configuration**

#### **MapConfig** (`Config/MapConfig.cs`)
- **Purpose:** Centralized configuration for all tunables
- **Contains:** Animation speeds, spline ranges, node count, car movement params
- **Key Feature:** Singleton instance, OnValidate for value constraints
- **Access Pattern:** Components use ConfigurableComponent base class

#### **ConfigurableComponent** (`Core/ConfigurableComponent.cs`)
- **Purpose:** Base class for accessing MapConfig
- **Provides:** GetConfig<T>() helper method with fallbacks
- **Key Feature:** Eliminates boilerplate null-checking

---

### **Services**

#### **AssetLoadingService** (`Services/AssetLoadingService.cs`)
- **Purpose:** Wrapper for Unity Addressables
- **Responsibilities:** Load sprites, prefabs, handle cleanup
- **Key Feature:** Prevents memory leaks from Addressables handles

---

### **UI Utilities**

#### **SlideBase** (`UI/SlideBase.cs`)
- **Purpose:** Base class for slide prefabs
- **Provides:** Key property for quiz references, lifecycle hooks
- **Key Feature:** OnEnter/OnExit methods for slide transitions

#### **MiniPopover** (`MiniPopup/MiniPopover.cs`)
- **Purpose:** Small tooltip popups on slides
- **Responsibilities:** Show/hide tooltip text, position over hotspots
- **Key Feature:** Fade in/out animations

#### **HotSpotDot** (`MiniPopup/HotSpotDot.cs`)
- **Purpose:** Clickable hotspot on slides
- **Responsibilities:** Trigger tooltip display
- **Key Feature:** Can be manually added to slide prefabs

---

## Data Flow

### **Node Click Flow**
```
1. User clicks node button
   ↓
2. LevelNodeView.OnClicked fires
   ↓
3. NodeManager.OnNodeClicked event
   ↓
4. MapController.HandleNodeClicked(nodeId)
   ↓
5. Check: MapState.IsNodeUnlocked(nodeId)?
   ├─ YES → PopupController.Open(nodeData)
   └─ NO  → NodeManager.ShakeNode(nodeId)
```

### **Node Completion Flow**
```
1. Player completes quiz
   ↓
2. PopupController fires completion event
   ↓
3. MapController.CompleteNode(nodeId)
   ↓
4. MapState.TryCompleteNode(nodeId)
   ├─ Marks node as completed
   ├─ Unlocks next node (if all previous done)
   └─ Fires OnNodeCompletedChanged event
   ↓
5. MapController listens to event
   ├─ Updates node visual via NodeManager
   ├─ Moves car via CarController
   └─ Saves to PlayerPrefs
```

### **Car Movement Flow**
```
1. MapState.TryMoveCarTo(nodeId)
   ↓
2. Fires OnCarNodeChanged event
   ↓
3. MapController.HandleCarNodeChanged(nodeId)
   ↓
4. CarController.MoveToNode(nodeId, nodeManager)
   ├─ Gets spline T from NodeManager
   ├─ Animates car along spline
   ├─ Rotates wheels, adds bounce
   └─ Fires OnArrivedAtNode when done
   ↓
5. MapController.HandleCarArrived(nodeId)
   └─ Saves progress to PlayerPrefs
```

---

## Folder Structure

```
Assets/Scripts/Nodemap/
│
├── Docs/                          ← You are here
│   ├── SYSTEM_OVERVIEW.md         ← This file
│   ├── NODE_AND_LEARNING_FLOW.md  ← Gameplay mechanics
│   ├── COMPONENT_API.md           ← Quick reference
│   └── CONTENT_CREATION.md        ← Designer guide
│
├── Core/                          ← Foundational classes
│   ├── MapState.cs                ← Single source of truth
│   └── ConfigurableComponent.cs   ← Base class for config access
│
├── Content/                       ← Data structures
│   ├── NodeData.cs                ← Node definition (ScriptableObject)
│   ├── NodeId.cs                  ← Type-safe node identifier
│   └── SlideDeck.cs               ← Slide collection
│
├── Config/                        ← Configuration
│   └── MapConfig.cs               ← All tunables (ScriptableObject)
│
├── Node/                          ← Node visuals
│   └── LevelNodeView.cs           ← Individual node button
│
├── Car/                           ← Car movement & visuals
│   ├── CarMovementController.cs   ← Movement along spline
│   └── CarVisual.cs               ← Sprite upgrades
│
├── Animations/                    ← Visual feedback
│   └── NodeStateAnimation.cs      ← Pop & shake animations
│
├── Minigames/                     ← Interactive activities
│   ├── WordUnscrambleController.cs
│   ├── MemoryMatch/
│   │   └── MemoryMatchController.cs
│   └── DragDropMatch/
│       └── DragDropController.cs
│
├── MiniPopup/                     ← Tooltip system
│   ├── HotSpotDot.cs              ← Clickable hotspot
│   ├── HotSpotGroup.cs            ← Group of hotspots
│   └── MiniPopover.cs             ← Tooltip popup
│
├── UI/                            ← UI utilities
│   └── SlideBase.cs               ← Base class for slides
│
├── Services/                      ← Shared services
│   └── AssetLoadingService.cs     ← Addressables wrapper
│
├── Commands/                      ← Command pattern (future)
│   └── MapCommands.cs             ← Reusable commands
│
├── Examples/                      ← Sample implementations
│
├── MapControllerSimple.cs         ← Main orchestrator
├── NodeManagerSimple.cs           ← Node visual manager
└── PopupController.cs             ← Content display controller
```

---

## Naming Conventions

### **Files**
- **Controllers:** `*Controller.cs` (e.g., `MapControllerSimple.cs`)
- **Views:** `*View.cs` (e.g., `LevelNodeView.cs`)
- **Data:** Descriptive nouns (e.g., `NodeData.cs`, `SlideDeck.cs`)
- **Services:** `*Service.cs` (e.g., `AssetLoadingService.cs`)

### **Variables**
- **Private fields:** `_camelCase` with underscore (e.g., `_currentNodeId`)
- **Public properties:** `PascalCase` (e.g., `CurrentNodeId`)
- **SerializeFields:** `camelCase` (e.g., `nodeManager`)
- **Constants:** `UPPER_SNAKE_CASE` (e.g., `MAX_NODES`)

### **Methods**
- **Public API:** `PascalCase` (e.g., `TryCompleteNode()`)
- **Private helpers:** `PascalCase` (e.g., `UpdateNodeVisual()`)
- **Event handlers:** `Handle*` prefix (e.g., `HandleNodeClicked()`)
- **Coroutines:** `*Routine` suffix (e.g., `MoveCarRoutine()`)

### **Events**
- **Pattern:** `OnVerbPastTense` (e.g., `OnNodeClicked`, `OnArrivedAtNode`)
- **Parameters:** Include context (e.g., `OnNodeCompletedChanged(NodeId id, bool completed)`)

### **Prefabs**
- **Slides:** `Slide_##.prefab` (e.g., `Slide_01.prefab`)
- **Mini-games:** Descriptive (e.g., `WordUnscramble.prefab`)
- **Nodes:** `Node_#.prefab` (e.g., `Node_1.prefab`)

### **ScriptableObjects**
- **Node Data:** `Node#_[Topic].asset` (e.g., `Node1_Battery.asset`)
- **Slide Decks:** `[Topic]_SlideDeck.asset` (e.g., `Battery_SlideDeck.asset`)
- **Config:** `MapConfig.asset` (singleton)

---

## Key Dependencies

### **Unity Packages**
- **Addressables** - Asset loading and management
- **TextMeshPro** - UI text rendering
- **Splines** - Car path following
- **Unity UI** - Button, Image, Canvas components

### **PlayerPrefs Keys**
- `NodeUnlocked_{i}` - Boolean for each node (0-5)
- `NodeCompleted_{i}` - Boolean for each node (0-5)
- `CurrentCarNode` - Integer (current car position)
- `ActiveNode` - Integer (current active node)

---

## Common Patterns

### **Event Subscription**
```csharp
private void Start()
{
    mapState.OnNodeCompletedChanged += HandleNodeCompleted;
}

private void OnDestroy()
{
    if (mapState != null)
        mapState.OnNodeCompletedChanged -= HandleNodeCompleted;
}
```

### **Config Access**
```csharp
public class MyComponent : ConfigurableComponent
{
    private void DoSomething()
    {
        float speed = GetConfig(c => c.moveSpeed, 2f);
    }
}
```

### **Node Validation**
```csharp
if (!nodeId.IsValid(nodeCount))
{
    Debug.LogError($"Invalid node ID: {nodeId}");
    return;
}
```

---

## For AI Assistants: Before Making Changes

1. **Read this file first** to understand the architecture
2. **Check `NODE_AND_LEARNING_FLOW.md`** for gameplay mechanics
3. **Reference `COMPONENT_API.md`** for specific component details
4. **Follow existing patterns** - don't introduce new ones without reason
5. **Test after changes:**
   - Play through one full node
   - Check console for errors
   - Verify save/load works
6. **Update docs** if you change architecture or add new components

---

## Questions?

If you're unsure about something:
- Check the other docs in this folder
- Look for similar code in the existing codebase
- Follow the principle: "Simple and consistent" beats "clever and unique"

---

**End of System Overview**
