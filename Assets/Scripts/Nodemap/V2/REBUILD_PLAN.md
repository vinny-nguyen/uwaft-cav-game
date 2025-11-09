# 🎯 NodeMap V2 - Clean Slate Rebuild Plan

> **Last Updated:** November 9, 2025  
> **Status:** Planning Phase  
> **Goal:** Build a professional, maintainable, scalable NodeMap system from scratch

---

## 📋 Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Folder Structure](#folder-structure)
4. [Implementation Roadmap](#implementation-roadmap)
5. [What Gets Kept vs Rebuilt](#what-gets-kept-vs-rebuilt)
6. [Success Criteria](#success-criteria)
7. [Bonus Features](#bonus-features)

---

## Overview

### Why Rebuild?

**Current Issues with V1:**
- MapControllerSimple does too much (god object)
- PopupController mixes concerns (slides + quiz + mini-games)
- No clear separation between presentation and business logic
- Event subscriptions scattered (memory leak risks)
- Hard to test, hard to extend

**V2 Goals:**
- ✅ Clean architecture (State → Services → Domain → Controllers → Views)
- ✅ Dependency injection from the start
- ✅ Testable components (unit tests for business logic)
- ✅ Professional patterns (Command, Observer, State Machine)
- ✅ Keep all existing prefabs, assets, and data
- ✅ Modern C# patterns (async/await, Result<T>, immutability)

### Timeline

**Total Duration:** 4-5 weeks  
**Approach:** Bottom-up (Foundation → Business Logic → UI → Polish)

---

## Architecture

### Layer Diagram

```
┌─────────────────────────────────────────────────────┐
│                   Game Bootstrap                     │
│         (Initialize services, load config)           │
└────────────────────┬────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌──────────────┐          ┌──────────────┐
│   Services   │          │     State    │
│              │          │              │
│ • Asset      │◄─────────│ • Game       │
│ • Save/Load  │          │ • Progression│
│ • Audio      │          │ • Session    │
│ • Config     │          │              │
└──────┬───────┘          └──────┬───────┘
       │                         │
       │                         │ Events
       │                         ▼
       │              ┌────────────────────┐
       │              │      Domain        │
       │              │  (Business Logic)  │
       │              │                    │
       │              │ • Progression Mgr  │
       └─────────────►│ • Session Mgr     │
                      │ • Quiz Evaluator   │
                      └──────┬─────────────┘
                             │
                             │ Commands
                             ▼
                  ┌──────────────────────┐
                  │    Controllers       │
                  │  (Orchestration)     │
                  │                      │
                  │ • NodeMap            │
                  │ • LearningSession    │
                  │ • Quiz               │
                  │ • Car                │
                  └──────┬───────────────┘
                         │
                         │ Updates
                         ▼
              ┌──────────────────────┐
              │        Views         │
              │   (Presentation)     │
              │                      │
              │ • NodeView           │
              │ • LearningView       │
              │ • QuizView           │
              │ • CarView            │
              └──────────────────────┘
```

### Design Principles

1. **Separation of Concerns**
   - Each layer has ONE responsibility
   - Business logic is independent of Unity (testable)
   - Views are dumb (just render + emit events)

2. **Dependency Inversion**
   - High-level modules don't depend on low-level modules
   - Both depend on abstractions (interfaces)
   - ServiceLocator for dependency injection

3. **Single Source of Truth**
   - `GameState` holds all game data
   - Only mutated through explicit methods
   - Emits events on changes

4. **Event-Driven Communication**
   - Components communicate via events (not direct calls)
   - Decoupled, testable, maintainable
   - Centralized `GameEvents` for global events

5. **Fail-Safe with Result<T>**
   - No exceptions for expected failures
   - Explicit error handling
   - Type-safe success/failure

---

## Folder Structure

```
Assets/Scripts/Nodemap/V2/
│
├─ Core/                          [Foundation types, no dependencies]
│   ├─ NodeId.cs                  [Type-safe node identifier]
│   ├─ Result.cs                  [Error handling type]
│   ├─ GameState.cs               [Single source of truth]
│   └─ GameEvents.cs              [Global event definitions]
│
├─ Services/                      [Infrastructure, singletons]
│   ├─ IService.cs                [Service contract]
│   ├─ ServiceLocator.cs          [Dependency injection]
│   ├─ AssetService.cs            [Addressables management]
│   ├─ ProgressionService.cs      [Save/load via PlayerPrefs]
│   ├─ AudioService.cs            [Sound effects/music]
│   └─ ConfigService.cs           [MapConfig wrapper]
│
├─ Domain/                        [Business logic, no Unity deps]
│   ├─ Models/
│   │   ├─ NodeModel.cs           [Node data representation]
│   │   ├─ LearningSession.cs     [Session state model]
│   │   └─ QuizSession.cs         [Quiz state model]
│   ├─ ProgressionManager.cs      [Unlock rules, progression logic]
│   ├─ SessionManager.cs          [Learning flow state machine]
│   └─ QuizEvaluator.cs           [Quiz scoring, validation]
│
├─ Controllers/                   [Orchestration, wire layers together]
│   ├─ NodeMapController.cs       [Map management, node interactions]
│   ├─ LearningController.cs      [Session flow, slide navigation]
│   ├─ QuizController.cs          [Quiz flow, answer validation]
│   └─ CarController.cs           [Movement, upgrades]
│
├─ Views/                         [UI layer, no logic]
│   ├─ NodeView.cs                [Individual node button]
│   ├─ NodeMapView.cs             [Entire map container]
│   ├─ LearningView.cs            [Slide display UI]
│   ├─ QuizView.cs                [Quiz display UI]
│   └─ CarView.cs                 [Car visual representation]
│
├─ Components/                    [Reusable, composable utilities]
│   ├─ Animations/
│   │   ├─ NodeAnimation.cs       [Node state animations]
│   │   ├─ TransitionEffect.cs    [Slide transitions]
│   │   └─ ParticleController.cs  [Particle effects]
│   ├─ MiniGames/
│   │   ├─ IMiniGame.cs           [Mini-game interface]
│   │   ├─ MiniGameBase.cs        [Base implementation]
│   │   └─ Adapters/              [V1 mini-game wrappers]
│   │       ├─ WordUnscrambleAdapter.cs
│   │       ├─ MemoryMatchAdapter.cs
│   │       └─ DragDropAdapter.cs
│   └─ UI/
│       ├─ ProgressBar.cs         [Reusable progress bar]
│       ├─ LoadingSpinner.cs      [Loading indicator]
│       ├─ ConfirmationDialog.cs  [Yes/No prompts]
│       └─ TooltipManager.cs      [Hover tooltips]
│
├─ Bootstrap/                     [Game initialization]
│   └─ GameBootstrap.cs           [Entry point, DI setup]
│
└─ Docs/                          [V2-specific documentation]
    ├─ ARCHITECTURE.md            [Detailed architecture guide]
    ├─ API_REFERENCE.md           [Component API docs]
    └─ MIGRATION_GUIDE.md         [V1 → V2 migration]
```

---

## Implementation Roadmap

### **Week 1: Foundation Layer** 🏗️

#### Day 1: Core Types
**Goal:** Establish type-safe primitives

**Tasks:**
- [ ] Create V2 folder structure
- [ ] Create `NodeId.cs` (enhanced from V1)
  - Value object pattern
  - Operators: `==`, `!=`, `<`, `>`, `<=`, `>=`
  - Validation: `IsValid()`, `IsBetween()`
  - Navigation: `Next()`, `Previous()`
  - Range iteration support
- [ ] Create `Result.cs`
  - Success/Failure factory methods
  - Implicit conversions
  - Match pattern support
  - Chaining methods (`Then()`, `OnSuccess()`, `OnFailure()`)
- [ ] Create `GameEvents.cs`
  - Node events (unlocked, completed, clicked)
  - Session events (started, ended, phase changed)
  - Car events (moved, upgraded)
  - Quiz events (passed, failed, reviewed)

**Success Criteria:** Core types compile, no external dependencies

---

#### Day 2: Service Infrastructure
**Goal:** Build dependency injection system

**Tasks:**
- [ ] Create `IService.cs` interface
  ```csharp
  public interface IService
  {
      void Initialize();
      void Shutdown();
  }
  ```
- [ ] Create `ServiceLocator.cs`
  - Generic registration/retrieval
  - Lazy initialization
  - Lifecycle management
  - Error handling (missing services)
- [ ] Create `ConfigService.cs`
  - Wraps MapConfig ScriptableObject
  - Type-safe config access
  - Fallback values
  - Validation on load

**Success Criteria:** Can register/retrieve services, config loads correctly

---

#### Day 3: Game State
**Goal:** Single source of truth for game data

**Tasks:**
- [ ] Create `GameState.cs`
  - Properties: ActiveNode, CarNode, UnlockedNodes, CompletedNodes
  - Methods: `TryUnlockNode()`, `TryCompleteNode()`, `TryMoveCarTo()`
  - Event emissions on state changes
  - Immutable where possible
  - Validation on all mutations
- [ ] Create `ProgressionService.cs`
  - Save to PlayerPrefs (JSON serialization)
  - Load from PlayerPrefs (with defaults)
  - Reset progression
  - Backup/restore functionality
- [ ] Unit tests for GameState
  - Test unlock logic
  - Test completion logic
  - Test edge cases (invalid nodes, double completion)

**Success Criteria:** State mutates correctly, saves/loads reliably

---

#### Day 4: Asset Management
**Goal:** Efficient asset loading via Addressables

**Tasks:**
- [ ] Migrate `AssetLoadingService.cs` → `AssetService.cs`
  - Convert callbacks to async/await
  - Implement asset caching
  - Preloading strategy
  - Graceful error handling (missing assets)
  - Memory management (release unused assets)
- [ ] Create `AssetReference` wrapper
  - Type-safe asset references
  - Automatic handle cleanup
  - Loading state tracking

**Success Criteria:** Assets load smoothly, no memory leaks

---

#### Day 5: Domain Models
**Goal:** Business entity representations

**Tasks:**
- [ ] Create `NodeModel.cs`
  - Id, Title, State, SlideDeck, QuizData, Upgrades
  - Factory method from NodeData ScriptableObject
  - Immutable properties
- [ ] Create `LearningSession.cs`
  - NodeId, Phase (Slides/MiniGame/Quiz), CurrentSlideIndex
  - Navigation methods: `NextSlide()`, `PreviousSlide()`, `CanAdvance()`
  - Completion tracking
  - State machine for phases
- [ ] Create `QuizSession.cs`
  - QuizData, CurrentQuestionIndex, Answers, Score
  - Methods: `SubmitAnswer()`, `GetResults()`
  - Retry tracking

**Success Criteria:** Models are immutable, testable, well-defined

---

### **Week 2: Business Logic Layer** 🧠

#### Day 1: Progression Manager
**Goal:** Unlock/completion rules

**Tasks:**
- [ ] Create `ProgressionManager.cs`
  - `CanUnlockNode(NodeId, GameState)` → checks prerequisites
  - `GetNextUnlockable(GameState)` → returns next valid node
  - `AreAllPreviousNodesComplete(NodeId, GameState)` → sequential check
  - `CalculateProgress(GameState)` → percentage complete
- [ ] Unit tests
  - Test sequential unlocking
  - Test skip prevention
  - Test edge cases (all complete, none complete)

**Success Criteria:** Unlock logic is bulletproof, fully tested

---

#### Day 2: Session Manager
**Goal:** Learning flow state machine

**Tasks:**
- [ ] Create `SessionManager.cs`
  - State machine: Idle → Loading → Slides → MiniGame → Quiz → Complete
  - `StartSession(NodeId)` → initialize session
  - `AdvancePhase(LearningSession)` → move to next phase
  - `CanAdvance(LearningSession)` → check if mini-game complete
  - `CompleteSession(LearningSession)` → mark node done
- [ ] State transition validation
- [ ] Unit tests for all transitions

**Success Criteria:** Session flow is deterministic, testable

---

#### Day 3: Quiz Evaluator
**Goal:** Quiz scoring and validation

**Tasks:**
- [ ] Create `QuizEvaluator.cs`
  - `IsCorrect(QuizQuestion, int answerIndex)` → validate answer
  - `EvaluateQuiz(QuizData, int[] answers)` → score quiz
  - `GetRelatedSlides(QuizQuestion)` → return slide keys
  - `CalculateStarRating(int score, int total)` → 1-3 stars
- [ ] Unit tests with sample quiz data

**Success Criteria:** Quiz evaluation is accurate, handles edge cases

---

#### Day 4: Mini-Game Integration
**Goal:** Standardize mini-game interface

**Tasks:**
- [ ] Create `IMiniGame.cs` interface
  ```csharp
  public interface IMiniGame
  {
      event Action<bool> OnCompleted;
      void Initialize();
      void ResetGame();
      string GetInstructions();
      bool IsComplete { get; }
  }
  ```
- [ ] Create `MiniGameBase.cs` abstract class
  - Implements common functionality
  - Helper method: `NotifyCompleted(bool success)`
  - Tracks completion state
- [ ] Create `WordUnscrambleAdapter.cs`
  - Wraps V1 `WordUnscrambleController`
  - Implements `IMiniGame`
  - Converts UnityEvent to Action<bool>

**Success Criteria:** At least one V1 mini-game works via adapter

---

#### Day 5: Bootstrap System
**Goal:** Initialize the game properly

**Tasks:**
- [ ] Create `GameBootstrap.cs`
  - Awake: Register all services
  - Start: Initialize services in order
  - Load game state from ProgressionService
  - Initialize controllers
  - Handle initialization errors gracefully
- [ ] Create initialization order diagram
- [ ] Test in Unity Editor

**Success Criteria:** Game initializes without errors, services are ready

---

### **Week 3: Controllers & Views Layer** 🎮

#### Day 1: NodeMap System
**Goal:** Interactive node map

**Tasks:**
- [ ] Create `NodeMapController.cs`
  - Subscribe to GameState events
  - Handle node clicks
  - Trigger animations on state changes
  - Start learning sessions
- [ ] Create `NodeMapView.cs`
  - Container for all NodeViews
  - Emit OnNodeClicked event
  - Initialize node positions on spline
- [ ] Create `NodeView.cs`
  - Render node sprite based on state
  - Emit OnClicked event
  - Delegate animations to NodeAnimation component
  - Update visual on state change

**Success Criteria:** Can click nodes, see state changes, animations play

---

#### Day 2: Car System
**Goal:** Smooth car movement and upgrades

**Tasks:**
- [ ] Create `CarController.cs`
  - Subscribe to GameState.OnCarMoved
  - Coroutine for spline movement
  - Easing curves (ease-in-out)
  - Event emission (OnStartedMoving, OnArrived)
- [ ] Create `CarView.cs`
  - Apply frame/tire sprites
  - Wheel spin animation
  - Bounce effect during movement
  - Particle trail based on tier
- [ ] Migrate movement logic from V1 CarMovementController

**Success Criteria:** Car moves smoothly, upgrades apply visually

---

#### Day 3: Learning Session
**Goal:** Slide navigation and display

**Tasks:**
- [ ] Create `LearningController.cs`
  - Start session via SessionManager
  - Handle Next/Previous navigation
  - Block Next button during mini-games
  - Handle session completion
- [ ] Create `LearningView.cs`
  - Display slides from SlideDeck
  - Transition effects (fade/slide)
  - Progress indicator (3/10)
  - Next/Previous buttons with proper states
  - Exit confirmation dialog

**Success Criteria:** Can navigate slides, mini-games block progression

---

#### Day 4: Quiz System
**Goal:** Quiz display and review flow

**Tasks:**
- [ ] Create `QuizController.cs`
  - Load quiz via QuizEvaluator
  - Handle answer selection
  - Show correct/incorrect feedback
  - Trigger review flow on wrong answer
  - Restart quiz after review
- [ ] Create `QuizView.cs`
  - Display questions and options
  - Visual feedback (green checkmark, red X)
  - Review button on wrong answer
  - Results screen (score, stars)
  - Retry/Continue buttons

**Success Criteria:** Quiz works, review flow navigates to slides

---

#### Day 5: Integration Testing
**Goal:** Full end-to-end flow works

**Tasks:**
- [ ] Test full flow: Click node → Slides → Mini-game → Quiz → Completion
- [ ] Test edge cases:
  - Click locked node (should shake)
  - Close popup mid-session (should resume)
  - Wrong quiz answer (should show review)
  - Mini-game failure (should allow retry)
- [ ] Fix any bugs discovered
- [ ] Performance profiling (60 FPS target)

**Success Criteria:** Complete one full node without errors

---

### **Week 4: Polish & Finalization** ✨

#### Day 1-2: Animations & Transitions
**Goal:** Professional visual feedback

**Tasks:**
- [ ] Migrate `NodeStateAnimation.cs` → `NodeAnimation.cs`
  - Pop animation (state change)
  - Shake animation (locked click)
  - Unlock animation (pulse + particles)
  - Complete animation (sparkles)
- [ ] Create `TransitionEffect.cs`
  - Fade in/out
  - Slide left/right
  - Scale up/down
  - Custom easing curves
- [ ] Create `ParticleController.cs`
  - Unlock burst
  - Completion sparkles
  - Quiz correct celebration
  - Car upgrade reveal

**Success Criteria:** Every interaction has visual feedback

---

#### Day 3-4: UI Components
**Goal:** Reusable UI elements

**Tasks:**
- [ ] Create `ProgressBar.cs`
  - Animated fill (smooth lerp)
  - Color gradient based on progress
  - Optional label (e.g., "50%")
- [ ] Create `LoadingSpinner.cs`
  - Rotation animation
  - Show during asset loading
  - Fade in/out
- [ ] Create `ConfirmationDialog.cs`
  - Generic Yes/No popup
  - Customizable message
  - Blur background effect
- [ ] Create `TooltipManager.cs`
  - Show on hover
  - Auto-position (avoid screen edges)
  - Fade in/out

**Success Criteria:** UI feels polished, professional

---

#### Day 5: Final Polish & Documentation
**Goal:** Ship-ready quality

**Tasks:**
- [ ] Update all documentation:
  - ARCHITECTURE.md (detailed layer explanations)
  - API_REFERENCE.md (all public methods/events)
  - MIGRATION_GUIDE.md (V1 → V2 transition)
- [ ] Performance optimization:
  - Profile with Unity Profiler
  - Fix any frame drops
  - Optimize memory usage
- [ ] Code cleanup:
  - Remove debug logs
  - Fix compiler warnings
  - Consistent formatting
- [ ] Final playthrough test

**Success Criteria:** Ready to demo, documentation complete

---

## What Gets Kept vs Rebuilt

### ✅ KEEP (From V1)

**Assets & Data:**
- ✅ All slide prefabs (`Assets/Prefabs/Slides/`)
- ✅ All mini-game prefabs (WordUnscramble, MemoryMatch, DragDrop)
- ✅ Car prefab and sprites
- ✅ Node sprites (Addressables)
- ✅ NodeData ScriptableObjects
- ✅ SlideDeck ScriptableObjects
- ✅ QuizData ScriptableObjects
- ✅ MapConfig ScriptableObject

**Code (with refactoring):**
- ✅ NodeId (enhance with operators)
- ✅ MapConfig (wrap in ConfigService)
- ✅ AssetLoadingService (convert to async)
- ✅ NodeStateAnimation (migrate to NodeAnimation)
- ✅ Mini-game logic (wrap in adapters)

---

### 🔄 MIGRATE & ENHANCE

| V1 File | V2 File | Changes |
|---------|---------|---------|
| `NodeId.cs` | `Core/NodeId.cs` | Add operators, validation, iteration |
| `AssetLoadingService.cs` | `Services/AssetService.cs` | Async/await, caching, preloading |
| `MapConfig.cs` | `Services/ConfigService.cs` | Wrap with type-safe access |
| `NodeStateAnimation.cs` | `Components/Animations/NodeAnimation.cs` | Add more animations, particle integration |
| `CarMovementController.cs` | `Controllers/CarController.cs` | Cleaner separation, better events |

---

### ❌ REBUILD FROM SCRATCH

**Reason:** Poor separation of concerns, hard to maintain

| V1 File | V2 Replacement | Why Rebuild? |
|---------|----------------|--------------|
| `MapState.cs` | `Core/GameState.cs` | Better API, immutability, validation |
| `MapControllerSimple.cs` | `Controllers/NodeMapController.cs` | Too much responsibility, hard to test |
| `PopupController.cs` | `Controllers/LearningController.cs` + `Views/LearningView.cs` | Mixes concerns, not MVC |
| `NodeManagerSimple.cs` | `Views/NodeMapView.cs` | Simpler, just manages views |
| `LevelNodeView.cs` | `Views/NodeView.cs` | Remove logic, emit events only |

---

## Success Criteria

### After 4 Weeks, You Should Have:

#### ✅ Architecture Quality
- [ ] Clean separation of concerns (6 distinct layers)
- [ ] Dependency injection via ServiceLocator
- [ ] Testable business logic (no Unity deps in Domain)
- [ ] Event-driven communication (no tight coupling)
- [ ] Error handling with Result<T> (no exceptions)

#### ✅ Functionality
- [ ] All 6 nodes work (Battery, Motor, Inverter, BMS, Charger, Thermal)
- [ ] Full flow: Unlock → Slides → Mini-games → Quiz → Completion
- [ ] Car movement with smooth animations
- [ ] Car upgrades apply visually
- [ ] Save/load progression (PlayerPrefs)
- [ ] Quiz review flow (wrong answer → review slides → restart)

#### ✅ Visual Quality
- [ ] Smooth animations (60 FPS)
- [ ] Professional transitions (fade, slide, scale)
- [ ] Particle effects (unlock, complete, quiz pass)
- [ ] Clear visual feedback (hover, press, disabled states)
- [ ] Loading states (spinners during asset loading)
- [ ] Error states (friendly messages, retry buttons)

#### ✅ Code Quality
- [ ] No compiler warnings
- [ ] No console errors during normal play
- [ ] Comprehensive documentation (3 markdown files)
- [ ] Unit tests for business logic (>80% coverage)
- [ ] Consistent naming conventions
- [ ] Code comments where necessary

#### ✅ User Experience
- [ ] No dead states (always clear what to do next)
- [ ] Accessible (keyboard navigation, focus states)
- [ ] Responsive (animations under 300ms)
- [ ] Forgiving (confirmation dialogs, auto-save)
- [ ] Satisfying (celebrations, upgrades, progress visible)

---

## Bonus Features

### 🌟 During Development (If Time Permits)

#### 1. **Undo/Redo System** 🔄
**Complexity:** Medium  
**Value:** High (great for testing, debugging)

**Implementation:**
- Command pattern for all state changes
- Maintain command history stack
- Undo reverts to previous state
- Redo reapplies command

**Use Cases:**
- Developer: Test different paths without restarting
- Player: Redo quiz if accidentally clicked wrong answer

**Files:**
- `Commands/ICommand.cs`
- `Commands/CommandHistory.cs`
- `Commands/UnlockNodeCommand.cs`
- `Commands/CompleteNodeCommand.cs`

---

#### 2. **Analytics & Heatmaps** 📊
**Complexity:** Low  
**Value:** High (understand player behavior)

**Metrics to Track:**
- Time spent on each slide
- Quiz question difficulty (% getting it wrong)
- Mini-game completion rates
- Most reviewed slides
- Drop-off points (where players quit)

**Implementation:**
- `Services/AnalyticsService.cs`
- Log events to JSON file
- Visualize in Unity Editor window
- Export to CSV for analysis

**Insights:**
- Which slides are too long/boring?
- Which quiz questions are too hard?
- Which mini-games are frustrating?

---

#### 3. **Slide Annotations & Highlights** ✏️
**Complexity:** Medium  
**Value:** Medium (better learning retention)

**Features:**
- Click to highlight important text
- Add personal notes to slides
- Bookmark slides for quick access
- Review all notes before quiz

**Implementation:**
- `Domain/Models/Annotation.cs`
- Save annotations per-user
- UI overlay for adding/editing
- "My Notes" summary screen

---

#### 4. **Adaptive Difficulty** 🎚️
**Complexity:** High  
**Value:** High (personalized learning)

**Features:**
- Track quiz performance across nodes
- If player struggles → Show extra slides
- If player excels → Skip review slides
- Adjust mini-game difficulty dynamically

**Implementation:**
- `Domain/DifficultyManager.cs`
- Machine learning model (optional)
- Simple heuristic: 3 wrong answers = extra help

**Benefit:** Keeps players engaged (not too easy, not too hard)

---

#### 5. **Leaderboard Integration** 🏆
**Complexity:** Medium  
**Value:** Medium (motivation through competition)

**Features:**
- Time-to-complete leaderboard per node
- Global leaderboard (all nodes)
- Friend leaderboards
- Achievements (speedrun, perfect score)

**Implementation:**
- Unity Gaming Services (already in project)
- `Services/LeaderboardService.cs`
- Display in main menu
- Share score on social media

---

#### 6. **Voice Narration** 🎙️
**Complexity:** Low-Medium  
**Value:** High (accessibility + immersion)

**Features:**
- AI-generated voice reads slide text
- Narration for mini-game instructions
- Quiz question readout
- Adjustable speed, volume
- Skip/pause controls

**Implementation:**
- Use Unity TTS or external API (ElevenLabs, Google TTS)
- `Services/NarrationService.cs`
- Preload audio files (or stream)
- Sync with slide transitions

**Accessibility Win:** Helps dyslexic players, non-native English speakers

---

#### 7. **Dark Mode / Color Themes** 🎨
**Complexity:** Low  
**Value:** Medium (aesthetic preference)

**Features:**
- Toggle dark/light theme
- Multiple color schemes (EcoCAR branding)
- High contrast mode (accessibility)
- Per-user preference saved

**Implementation:**
- `UI/ThemeManager.cs`
- Define color palettes in ScriptableObject
- Apply via SpriteRenderer, Image, TextMeshPro
- Smooth transition animation

---

#### 8. **Export Learning Progress PDF** 📄
**Complexity:** Medium  
**Value:** High (study aid outside game)

**Features:**
- Generate PDF with all slide images
- Include quiz questions + answers
- Add user's notes and highlights
- Print or save to device

**Implementation:**
- `Services/PDFExporter.cs`
- Use iTextSharp or similar library
- Trigger from pause menu
- Email or save to Documents folder

**Use Case:** Study before exam, share with teammates

---

#### 9. **Mini-Game High Scores** 🎮
**Complexity:** Low  
**Value:** Medium (replayability)

**Features:**
- Track best time for each mini-game
- Show personal best vs global best
- Retry mini-game after completion
- Unlock bonus mini-games (harder variants)

**Implementation:**
- Save scores in ProgressionService
- Display in mini-game UI
- Leaderboard per mini-game type

**Engagement:** Players want to beat their own score

---

#### 10. **Easter Eggs & Secrets** 🥚
**Complexity:** Low  
**Value:** High (delight factor)

**Ideas:**
- Hidden node (click 7 times on logo → bonus trivia)
- Konami code → Unlock rainbow car skin
- Click car 10 times → Do a flip animation
- Hidden quiz question referencing EcoCAR memes
- Secret ending for 100% completion

**Implementation:**
- Simple input detection
- Unlock flags in GameState
- Achievement tracking

**Why:** Makes the game memorable, shareable

---

### 🚀 Post-Launch (Future Enhancements)

#### 11. **Multiplayer Quiz Mode** 👥
- Race against friend to complete quiz
- Real-time vs turn-based
- Leaderboard for fastest correct answers

#### 12. **AR Mode for Car Upgrades** 📱
- View upgraded car in real world
- Take photos with AR car
- Share on social media

#### 13. **Integration with Canvas LMS** 🎓
- Auto-submit quiz scores to Canvas
- Sync progress with university system
- Award course credit for completion

#### 14. **Procedural Slide Generation** 🤖
- AI generates slides from PDF textbook
- Automatically creates quizzes from content
- Reduces manual content creation

#### 15. **VR Mode** 🥽
- Walk around 3D node map
- Immersive slide viewing
- VR mini-games

---

## Priority Ranking for Bonus Features

### **HIGH PRIORITY** (Add During Development)
1. ✅ **Analytics & Heatmaps** - Understand player behavior
2. ✅ **Voice Narration** - Accessibility win
3. ✅ **Adaptive Difficulty** - Personalized learning

### **MEDIUM PRIORITY** (Add in Week 4 if Time)
4. ✅ **Slide Annotations** - Better retention
5. ✅ **Export PDF** - Study aid
6. ✅ **Easter Eggs** - Delight factor

### **LOW PRIORITY** (Post-Launch)
7. ⏳ **Dark Mode** - Nice to have
8. ⏳ **Mini-Game High Scores** - Replayability
9. ⏳ **Leaderboard** - Motivation
10. ⏳ **Undo/Redo** - QOL for testing

---

## Next Steps

**Ready to start?**

1. ✅ Create V2 folder structure
2. ✅ Begin Week 1, Day 1 (Core Types)
3. ✅ Reference this document throughout development
4. ✅ Update checklist as you complete tasks

**This document is your blueprint. Let's build something amazing!** 🚀

---

**Last Updated:** November 9, 2025  
**Document Version:** 1.0  
**Status:** Ready to Execute
