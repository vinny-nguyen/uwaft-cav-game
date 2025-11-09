# Node and Learning Flow

> **For AI Assistants:** This document explains the two core gameplay loops: node progression and learning sessions.  
> Read `SYSTEM_OVERVIEW.md` first for context.

**Last Updated:** November 9, 2025  
**Covers:** Node progression mechanics + Learning content flow

---

## Table of Contents
1. [Overview](#overview)
2. [Node Progression System](#node-progression-system)
3. [Learning Session Flow](#learning-session-flow)
4. [Complete End-to-End Flow](#complete-end-to-end-flow)
5. [State Persistence](#state-persistence)

---

## Overview

The game has **two interconnected systems**:

1. **Node System** - Meta-progression (unlocking nodes, moving car, upgrading)
2. **Learning System** - Content delivery (slides, mini-games, quizzes)

**Relationship:**
```
Player → Clicks Node (Node System)
       → Learns Content (Learning System)
       → Completes Quiz (Learning System)
       → Node Completes (Node System)
       → Next Node Unlocks (Node System)
       → Car Moves (Node System)
```

---

## Node Progression System

### **What is a Node?**

A node represents a **learning checkpoint** on the map. Think of it like a level in a traditional game.

**Properties:**
- **Visual position** - Placed along a spline path
- **Content** - Slides, mini-games, quiz
- **State** - Inactive, Active, or Completed
- **Reward** - Car upgrade (frame sprite, tire sprite)

---

### **Node States**

```csharp
public enum NodeState 
{ 
    Inactive,   // Locked - can't access yet
    Active,     // Unlocked - ready to play
    Completed   // Done - can replay anytime
}
```

**Visual Representation:**
- **Inactive:** Gray sprite, shake on click
- **Active:** Yellow sprite, opens content on click
- **Completed:** Green sprite, opens review on click

---

### **NodeId - Type-Safe Wrapper**

Instead of using raw integers, we use `NodeId`:

```csharp
NodeId current = new NodeId(0);  // Node 0
NodeId next = current.GetNext(6); // Node 1 (if valid)

if (next.HasValue && next.Value.IsValid(6))
{
    // Safe to use next
}
```

**Benefits:**
- Can't accidentally mix node indices with other numbers
- Built-in validation
- Clear intent in code

**Common Methods:**
```csharp
nodeId.IsValid(nodeCount)      // Check if in range [0, nodeCount)
nodeId.GetNext(nodeCount)      // Get next node (or null if last)
nodeId.GetPrevious()           // Get previous node (or null if first)
NodeId.First                   // Node 0
NodeId.Last(nodeCount)         // Last node in collection
```

---

### **MapState - Single Source of Truth**

All node progression state lives in **one place**:

```csharp
public class MapState
{
    private NodeId _currentCarNodeId;      // Where the car is
    private NodeId _activeNodeId;          // Which node is currently playable
    private bool[] _unlockedNodes;         // Which nodes are unlocked
    private bool[] _completedNodes;        // Which nodes are completed
    
    // Events
    public event Action<NodeId> OnCarNodeChanged;
    public event Action<NodeId> OnActiveNodeChanged;
    public event Action<NodeId, bool> OnNodeUnlockedChanged;
    public event Action<NodeId, bool> OnNodeCompletedChanged;
}
```

**Key Principle:** Only MapState can modify progression data. All changes go through its methods.

---

### **State Mutations (Commands)**

#### **Complete a Node**
```csharp
public bool TryCompleteNode(NodeId nodeId)
{
    // Validate
    if (!IsValidNodeId(nodeId)) return false;
    if (!IsNodeUnlocked(nodeId)) return false;
    if (IsNodeCompleted(nodeId)) return true; // Already done
    
    // Update state
    _completedNodes[nodeId.Value] = true;
    OnNodeCompletedChanged?.Invoke(nodeId, true);
    
    // Auto-unlock next node
    TryUnlockNextNode(nodeId);
    
    return true;
}
```

**Triggers:**
- Marks node as completed
- Fires `OnNodeCompletedChanged` event
- Automatically unlocks next node if all previous nodes are complete

---

#### **Unlock Next Node**
```csharp
private void TryUnlockNextNode(NodeId completedNodeId)
{
    var nextNodeId = new NodeId(completedNodeId.Value + 1);
    
    if (!IsValidNodeId(nextNodeId)) return; // No next node
    
    // Check if ALL previous nodes are completed
    for (int i = 0; i <= completedNodeId.Value; i++)
    {
        if (!_completedNodes[i]) return; // Gap in progression
    }
    
    // Unlock next node
    if (!_unlockedNodes[nextNodeId.Value])
    {
        _unlockedNodes[nextNodeId.Value] = true;
        OnNodeUnlockedChanged?.Invoke(nextNodeId, true);
        
        _activeNodeId = nextNodeId;
        OnActiveNodeChanged?.Invoke(nextNodeId);
    }
}
```

**Important:** Nodes unlock **sequentially**. You can't skip nodes.

---

#### **Move Car**
```csharp
public bool TryMoveCarTo(NodeId nodeId)
{
    if (!IsValidNodeId(nodeId)) return false;
    if (!IsNodeUnlocked(nodeId)) return false;
    if (_currentCarNodeId.Equals(nodeId)) return true; // Already there
    
    _currentCarNodeId = nodeId;
    OnCarNodeChanged?.Invoke(nodeId);
    return true;
}
```

**Triggers:**
- Updates car position
- Fires `OnCarNodeChanged` event
- MapController listens and animates the car

---

### **Node Progression Flow Diagram**

```
Game Start
  ↓
Load MapState from PlayerPrefs
  ├─ Node 0: Unlocked, Not Completed
  ├─ Node 1-5: Locked, Not Completed
  └─ Car at Node 0
  ↓
Render Map
  ├─ Node 0: Yellow (Active)
  ├─ Node 1-5: Gray (Inactive)
  └─ Car positioned at Node 0
  ↓
User clicks Node 0
  ↓
MapState.IsNodeUnlocked(0)? → YES
  ↓
PopupController.Open(Node 0 data)
  ↓
[Learning Session - see next section]
  ↓
Quiz Passed
  ↓
MapState.TryCompleteNode(0)
  ├─ Mark node 0 as completed
  ├─ Fire OnNodeCompletedChanged(0, true)
  └─ Check: All previous nodes done? → YES
      ↓
      Unlock Node 1
      ├─ Set _unlockedNodes[1] = true
      ├─ Fire OnNodeUnlockedChanged(1, true)
      └─ Set _activeNodeId = Node 1
  ↓
MapState.TryMoveCarTo(1)
  ├─ Set _currentCarNodeId = Node 1
  └─ Fire OnCarNodeChanged(1)
  ↓
CarMovementController.MoveToNode(1)
  ├─ Animate along spline
  ├─ Rotate wheels
  ├─ Add bounce effect
  └─ Fire OnArrivedAtNode(1) when done
  ↓
MapController saves to PlayerPrefs
  ↓
Visual Updates
  ├─ Node 0: Green (Completed)
  ├─ Node 1: Yellow (Active)
  ├─ Node 2-5: Gray (Inactive)
  └─ Car at Node 1
```

---

## Learning Session Flow

### **What is a Learning Session?**

When a player clicks an active node, they enter a **learning session**:
1. View slides (learning content)
2. Complete mini-games (embedded checkpoints)
3. Pass a quiz (final test)

**Goal:** Complete all content to mark the node as done.

---

### **Content Structure**

Each node contains:

```csharp
public class NodeData : ScriptableObject
{
    public int id;                      // Node index (0-5)
    public string title;                // "Battery Basics"
    public SlideDeck slideDeck;         // Slides + mini-games
    public QuizData quizData;           // Final quiz
    public Sprite upgradeFrame;         // Car upgrade reward
    public Sprite upgradeTire;          // Tire upgrade reward
}
```

---

### **SlideDeck Structure**

A SlideDeck contains **mixed content** - both learning slides and mini-games:

```csharp
public class SlideDeck : ScriptableObject
{
    public SlideReference[] slides;  // Can be slides OR mini-games
}

[Serializable]
public class SlideReference
{
    public GameObject slidePrefab;   // The prefab to instantiate
    public SlideType type;           // Learning or MiniGame
    public string key;               // For quiz references
}

public enum SlideType
{
    Learning,   // Regular educational slide
    MiniGame    // Interactive activity
}
```

**Example SlideDeck:**
```
slides[0] = Slide_01 (Learning)
slides[1] = Slide_02 (Learning)
slides[2] = Slide_03 (Learning)
slides[3] = WordUnscramble (MiniGame) ← Embedded checkpoint
slides[4] = Slide_04 (Learning)
slides[5] = Slide_05 (Learning)
slides[6] = DragDrop (MiniGame) ← Another checkpoint
slides[7] = Slide_06 (Learning)
```

---

### **Slide Navigation**

**Learning Slides:**
- Image fills the screen
- "Next" button enabled immediately
- Can go back with "Previous" button
- Can have mini-popups (hotspots) for extra info

**Mini-Game Slides:**
- Interactive activity
- "Next" button **disabled** until completed
- Unlimited retries
- Must get correct answer to proceed

---

### **Mini-Games**

All mini-games implement the `IMiniGame` interface:

```csharp
public interface IMiniGame
{
    event Action<bool> OnCompleted;  // true = success, false = try again
    void ResetGame();                // Reset to initial state
}
```

**Flow:**
```
Mini-game loads
  ↓
Next button is disabled
  ↓
Player attempts mini-game
  ↓
Gets it wrong
  ↓
OnCompleted(false) fires
  ↓
Next button stays disabled
  ↓
Player tries again
  ↓
Gets it correct!
  ↓
OnCompleted(true) fires
  ↓
Next button enables
  ↓
Player clicks Next → Continue to next slide
```

**Example (Word Unscramble):**
```csharp
public class WordUnscrambleSlide : MonoBehaviour, IMiniGame
{
    public event Action<bool> OnCompleted;
    
    private void CheckAnswer()
    {
        if (userAnswer == correctAnswer)
        {
            OnCompleted?.Invoke(true);  // ✓ Unlock Next button
        }
        else
        {
            OnCompleted?.Invoke(false); // ✗ Try again
        }
    }
}
```

---

### **Quiz System**

After all slides (including embedded mini-games), the quiz starts.

**Quiz Structure:**
```csharp
public class QuizData : ScriptableObject
{
    public QuizQuestion[] questions;
}

[Serializable]
public class QuizQuestion
{
    public string question;             // "What component stores energy?"
    public string[] options;            // ["Battery", "Motor", "Inverter"]
    public int correctIndex;            // 0
    public string[] relatedSlideKeys;   // ["slide_03", "slide_05"]
}
```

---

### **Quiz Flow with Review**

```
Show Question 1
  ↓
User selects answer
  ↓
Correct?
  ├─ YES → Show "Correct! ✓"
  │         ↓
  │         Next Question
  │
  └─ NO → Show "Incorrect. Review?"
            ↓
            Display "Review" button
            ↓
            User clicks Review
            ↓
            QuizController fires: OnWrongAnswer(relatedSlideKeys)
            ↓
            LearningSessionController.JumpToSlide(slideKey)
            ↓
            User reviews slide(s)
            ↓
            User clicks through slides
            ↓
            Reaches quiz again
            ↓
            Quiz RESTARTS from Question 1
            ↓
            User retakes quiz
            ↓
            Answers all correctly
            ↓
            QuizController fires: OnQuizPassed()
            ↓
            LearningSessionController fires: OnSessionCompleted()
            ↓
            MapController.CompleteNode()
```

**Key Points:**
- Wrong answer → Jump to related slide
- Can review multiple slides
- Quiz restarts from beginning after review
- Must pass all questions to complete node

---

### **Learning Session Components**

#### **LearningSessionController** (Future)
- Manages slide progression
- Handles mini-game blocking
- Coordinates quiz flow
- Manages review navigation

#### **PopupController** (Current)
- Shows slides
- Basic navigation (Next/Prev)
- Quiz integration (partial)

#### **QuizController**
- Displays questions
- Checks answers
- Fires events (OnWrongAnswer, OnQuizPassed)
- Provides review button

---

### **Learning Session Diagram**

```
Node Clicked
  ↓
LearningSessionController.StartSession(nodeData)
  ↓
Load slides from SlideDeck
  ├─ 10 learning slides
  └─ 3 mini-game slides (positions 3, 7, 10)
  ↓
┌─────────────────────────────────────┐
│  Slide Navigation Loop              │
├─────────────────────────────────────┤
│  Show Slide 1 (Learning)            │
│    → Next enabled → User clicks     │
│  Show Slide 2 (Learning)            │
│    → Next enabled → User clicks     │
│  Show Slide 3 (Mini-Game)           │
│    → Next DISABLED                  │
│    → User plays mini-game           │
│    → Gets wrong → Try again         │
│    → Gets correct → Next ENABLED    │
│    → User clicks Next               │
│  Show Slide 4 (Learning)            │
│    → ... continues ...              │
└─────────────────────────────────────┘
  ↓
All slides completed
  ↓
Start Quiz
  ↓
┌─────────────────────────────────────┐
│  Quiz Loop                          │
├─────────────────────────────────────┤
│  Question 1                         │
│    → Wrong → Review slide_03        │
│    → User reviews                   │
│    → Quiz restarts                  │
│    → Correct → Next question        │
│  Question 2                         │
│    → Correct → Next question        │
│  Question 3                         │
│    → Correct → Quiz passed!         │
└─────────────────────────────────────┘
  ↓
OnSessionCompleted() fires
  ↓
MapController.CompleteNode(nodeId)
  ↓
Node marked as completed
```

---

## Complete End-to-End Flow

### **Player Journey: Node 0 → Node 1**

```
1. GAME START
   ├─ MapState loads from PlayerPrefs
   ├─ Node 0 is unlocked
   └─ Car positioned at Node 0

2. USER CLICKS NODE 0
   ├─ NodeManagerSimple fires OnNodeClicked(0)
   ├─ MapController.HandleNodeClicked(0)
   ├─ MapState.IsNodeUnlocked(0) → true
   └─ PopupController.Open(Node0Data, isCompleted: false)

3. LEARNING SESSION STARTS
   ├─ Load SlideDeck (15 slides total)
   │   ├─ slides[0-2]: Learning
   │   ├─ slides[3]: Mini-game (Word Unscramble)
   │   ├─ slides[4-6]: Learning
   │   ├─ slides[7]: Mini-game (Drag Drop)
   │   └─ slides[8-14]: Learning
   └─ Show first slide

4. SLIDE PROGRESSION
   ├─ User views slide 1 → clicks Next
   ├─ User views slide 2 → clicks Next
   ├─ User views slide 3 → clicks Next
   └─ Mini-game loads (slide 4)

5. MINI-GAME CHECKPOINT
   ├─ Next button disables
   ├─ User attempts word unscramble
   ├─ Gets it wrong → feedback shown
   ├─ User tries again
   ├─ Gets it correct → OnCompleted(true)
   ├─ Next button enables
   └─ User clicks Next → continue

6. MORE SLIDES
   ├─ User continues through slides
   ├─ Encounters another mini-game at slide 8
   ├─ Completes it
   └─ Finishes all 15 slides

7. QUIZ PHASE
   ├─ Quiz starts with Question 1
   ├─ User answers correctly → Question 2
   ├─ User answers Question 2 incorrectly
   ├─ "Review" button appears
   ├─ User clicks Review
   ├─ Jumps to slide_05 (related content)
   ├─ User reviews slides 5-7
   ├─ Quiz restarts from Question 1
   ├─ User answers all 3 questions correctly
   └─ QuizController fires OnQuizPassed()

8. NODE COMPLETION
   ├─ OnSessionCompleted() fires
   ├─ MapController.CompleteNode(0)
   ├─ MapState.TryCompleteNode(0)
   │   ├─ _completedNodes[0] = true
   │   ├─ OnNodeCompletedChanged(0, true)
   │   └─ TryUnlockNextNode(0)
   │       ├─ _unlockedNodes[1] = true
   │       ├─ OnNodeUnlockedChanged(1, true)
   │       └─ _activeNodeId = Node 1
   └─ MapState.TryMoveCarTo(1)

9. CAR MOVEMENT
   ├─ OnCarNodeChanged(1) fires
   ├─ MapController.HandleCarNodeChanged(1)
   ├─ CarMovementController.MoveToNode(1)
   │   ├─ Get spline position for node 1
   │   ├─ Animate car along curve
   │   ├─ Rotate wheels (360°/sec)
   │   ├─ Add bounce (sine wave, 5Hz)
   │   └─ OnArrivedAtNode(1) fires
   └─ MapController.HandleCarArrived(1)

10. VISUAL UPDATES
    ├─ Node 0: Changes to green sprite (Completed)
    ├─ Node 1: Changes to yellow sprite (Active)
    ├─ Node 2-5: Stay gray (Inactive)
    └─ Car at new position

11. PERSISTENCE
    ├─ MapState.SaveToPlayerPrefs()
    │   ├─ NodeUnlocked_0 = 1
    │   ├─ NodeUnlocked_1 = 1
    │   ├─ NodeCompleted_0 = 1
    │   ├─ CurrentCarNode = 1
    │   └─ ActiveNode = 1
    └─ Progress saved

12. READY FOR NODE 1
    └─ User can now click Node 1 to continue
```

---

## State Persistence

### **PlayerPrefs Keys**

```csharp
// Per-node keys (i = 0 to 5)
"NodeUnlocked_{i}"   // int: 1 = unlocked, 0 = locked
"NodeCompleted_{i}"  // int: 1 = completed, 0 = incomplete

// Global keys
"CurrentCarNode"     // int: Node index where car is (0-5)
"ActiveNode"         // int: Node index that's currently active (0-5)
```

**Example saved state after completing Node 0:**
```
NodeUnlocked_0 = 1
NodeUnlocked_1 = 1
NodeUnlocked_2 = 0
NodeUnlocked_3 = 0
NodeUnlocked_4 = 0
NodeUnlocked_5 = 0

NodeCompleted_0 = 1
NodeCompleted_1 = 0
NodeCompleted_2 = 0
NodeCompleted_3 = 0
NodeCompleted_4 = 0
NodeCompleted_5 = 0

CurrentCarNode = 1
ActiveNode = 1
```

---

### **Save Timing**

State is saved at these moments:
- Node completes → `MapState.SaveToPlayerPrefs()`
- Car arrives at node → `MapState.SaveToPlayerPrefs()`
- Node unlocks → `MapState.SaveToPlayerPrefs()`

**Why multiple saves?** Ensures progress is never lost, even if game closes unexpectedly.

---

### **Load on Start**

```csharp
// MapControllerSimple.Awake()
private void InitializeState()
{
    int nodeCount = GetConfig(c => c.nodeCount, 6);
    mapState = new MapState(nodeCount);
    mapState.LoadFromPlayerPrefs();  // ← Restore saved progress
}
```

First time playing:
- All nodes locked except Node 0
- Car at Node 0
- Node 0 active

Returning player:
- Loads exact state from last play
- Car positioned correctly
- Correct nodes unlocked/completed

---

## Key Takeaways

### **Node System**
1. Nodes unlock **sequentially** - no skipping
2. MapState is the **single source of truth**
3. All changes go through MapState methods
4. Events drive visual updates
5. Progress auto-saves

### **Learning System**
1. Slides can be **learning or mini-games**
2. Mini-games **block progression** until correct
3. Quiz wrong answers → **review flow**
4. Quiz restarts after review
5. Passing quiz = node completion

### **Integration**
1. Learning completion triggers node completion
2. Node completion unlocks next node
3. Node unlock moves car
4. Car arrival saves progress
5. Loop continues for 6 nodes

---

**End of Node and Learning Flow**
