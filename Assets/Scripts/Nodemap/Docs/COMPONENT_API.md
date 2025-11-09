# Component API Reference

> **For AI Assistants:** Quick reference for all major NodeMap components.  
> Includes methods, events, properties, and usage examples.

**Last Updated:** November 9, 2025  
**Purpose:** Fast lookup for component interactions

---

## Table of Contents
1. [Core State](#core-state)
2. [Controllers](#controllers)
3. [Node Components](#node-components)
4. [Car Components](#car-components)
5. [Content Components](#content-components)
6. [Animation Components](#animation-components)
7. [Common Usage Patterns](#common-usage-patterns)

---

## Core State

### **MapState**
**Location:** `Assets/Scripts/Nodemap/Core/MapState.cs`  
**Purpose:** Single source of truth for all node progression state

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `IsNodeUnlocked` | `NodeId id` | `bool` | Check if node can be accessed |
| `IsNodeCompleted` | `NodeId id` | `bool` | Check if node is finished |
| `TryCompleteNode` | `NodeId id` | `bool` | Mark node done, unlock next |
| `TryMoveCarTo` | `NodeId id` | `bool` | Move car to node (if unlocked) |
| `GetActiveNodeId` | - | `NodeId` | Get currently active node |
| `GetCarNodeId` | - | `NodeId` | Get node where car is |
| `GetNodeState` | `NodeId id` | `NodeState` | Get state (Inactive/Active/Completed) |
| `SaveToPlayerPrefs` | - | `void` | Persist state |
| `LoadFromPlayerPrefs` | - | `void` | Restore state |
| `ResetProgression` | - | `void` | Clear all progress (debug only) |

#### Events
| Event | Signature | Fired When |
|-------|-----------|------------|
| `OnNodeUnlockedChanged` | `Action<NodeId, bool>` | Node unlocks/locks |
| `OnNodeCompletedChanged` | `Action<NodeId, bool>` | Node completes |
| `OnActiveNodeChanged` | `Action<NodeId>` | Active node changes |
| `OnCarNodeChanged` | `Action<NodeId>` | Car moves to new node |

#### Usage Example
```csharp
// Subscribe to state changes
mapState.OnNodeCompletedChanged += HandleNodeCompleted;
mapState.OnCarNodeChanged += HandleCarMoved;

// Check state
if (mapState.IsNodeUnlocked(new NodeId(2)))
{
    Debug.Log("Node 2 is accessible");
}

// Mutate state
bool success = mapState.TryCompleteNode(new NodeId(0));
if (success)
{
    // Node 0 completed, Node 1 unlocked, car moved
}

// Cleanup
mapState.OnNodeCompletedChanged -= HandleNodeCompleted;
```

---

### **NodeId**
**Location:** `Assets/Scripts/Nodemap/Core/NodeId.cs`  
**Purpose:** Type-safe wrapper around node indices

#### Static Members
| Member | Type | Description |
|--------|------|-------------|
| `First` | `NodeId` | Node 0 |
| `Last(int nodeCount)` | `NodeId` | Final node in collection |

#### Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `IsValid` | `int nodeCount` | `bool` | Check if index in range |
| `GetNext` | `int nodeCount` | `NodeId?` | Next node (null if last) |
| `GetPrevious` | - | `NodeId?` | Previous node (null if first) |
| `Equals` | `NodeId other` | `bool` | Compare equality |

#### Usage Example
```csharp
NodeId current = new NodeId(2);
int nodeCount = 6;

// Validation
if (current.IsValid(nodeCount))
{
    Debug.Log($"Node {current.Value} is valid");
}

// Navigation
NodeId? next = current.GetNext(nodeCount);
if (next.HasValue)
{
    Debug.Log($"Next node: {next.Value.Value}");
}

// Static helpers
NodeId first = NodeId.First;           // Node 0
NodeId last = NodeId.Last(nodeCount);  // Node 5
```

---

## Controllers

### **MapControllerSimple**
**Location:** `Assets/Scripts/Nodemap/MapControllerSimple.cs`  
**Purpose:** Orchestrate all NodeMap systems (state, nodes, car, popups)

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ResetProgression` | - | `void` | Clear all progress (debug) |

#### Private Event Handlers
| Handler | Event Source | Purpose |
|---------|--------------|---------|
| `HandleNodeClicked` | `NodeManagerSimple.OnNodeClicked` | Open popup or shake inactive |
| `HandleCarNodeChanged` | `MapState.OnCarNodeChanged` | Animate car movement |
| `HandleNodeCompleted` | `MapState.OnNodeCompletedChanged` | Show upgrade popup |
| `HandleCarArrived` | `CarMovementController.OnArrivedAtNode` | Save state, apply upgrades |

#### Initialization Flow
```csharp
void Awake()
{
    // 1. Load config
    // 2. Create MapState
    // 3. Load progress
    // 4. Subscribe to events
}

void Start()
{
    // 1. Initialize nodes (NodeManagerSimple)
    // 2. Position car at current node
}
```

---

### **NodeManagerSimple**
**Location:** `Assets/Scripts/Nodemap/NodeManagerSimple.cs`  
**Purpose:** Manage all node visuals on the map

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Initialize` | `MapState state` | `void` | Setup all nodes |
| `GetTargetT` | `NodeId id` | `float` | Get spline position (0-1) |

#### Events
| Event | Signature | Fired When |
|-------|-----------|------------|
| `OnNodeClicked` | `Action<NodeId>` | User clicks any node |

#### Usage Example
```csharp
// Initialization
nodeManager.Initialize(mapState);

// Get spline position
NodeId target = new NodeId(3);
float t = nodeManager.GetTargetT(target); // e.g., 0.5 (midpoint)

// Listen for clicks
nodeManager.OnNodeClicked += HandleNodeClicked;
```

---

### **PopupController**
**Location:** `Assets/Scripts/Nodemap/PopupController.cs`  
**Purpose:** Display slides and quizzes for each node

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Open` | `NodeData data, bool isCompleted` | `void` | Show node content |
| `Close` | - | `void` | Hide popup |
| `NextSlide` | - | `void` | Navigate forward |
| `PrevSlide` | - | `void` | Navigate backward |
| `JumpToSlideByKey` | `string key` | `void` | Jump to specific slide (quiz review) |

#### Events
| Event | Signature | Fired When |
|-------|-----------|------------|
| `OnPopupClosed` | `Action` | Popup closes |
| `OnLearningCompleted` | `Action<NodeId>` | All slides done |

#### Usage Example
```csharp
// Open node content
NodeData nodeData = GetNodeData(0);
popupController.Open(nodeData, isCompleted: false);

// Handle quiz review
quizController.OnWrongAnswer += (slideKeys) =>
{
    popupController.JumpToSlideByKey(slideKeys[0]);
};

// Navigation
popupController.NextSlide();  // Manual navigation
```

---

### **QuizController**
**Location:** `Assets/Scripts/Nodemap/QuizController.cs`  
**Purpose:** Display and evaluate quiz questions

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `LoadQuiz` | `QuizData data` | `void` | Initialize quiz |
| `ShowQuestion` | `int index` | `void` | Display question |
| `CheckAnswer` | `int selectedIndex` | `void` | Evaluate answer |
| `RestartQuiz` | - | `void` | Reset to question 1 |

#### Events
| Event | Signature | Fired When |
|-------|-----------|------------|
| `OnQuizPassed` | `Action` | All questions correct |
| `OnWrongAnswer` | `Action<string[]>` | Incorrect answer (passes related slide keys) |

#### Usage Example
```csharp
// Load quiz
QuizData quizData = nodeData.quizData;
quizController.LoadQuiz(quizData);

// Handle events
quizController.OnWrongAnswer += (slideKeys) =>
{
    // Show review button
    reviewButton.gameObject.SetActive(true);
    reviewButton.onClick.AddListener(() =>
    {
        popupController.JumpToSlideByKey(slideKeys[0]);
        quizController.RestartQuiz();
    });
};

quizController.OnQuizPassed += () =>
{
    mapState.TryCompleteNode(currentNodeId);
};
```

---

## Node Components

### **LevelNodeView**
**Location:** `Assets/Scripts/Nodemap/Node/LevelNodeView.cs`  
**Purpose:** Individual node button on the map

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Initialize` | `NodeId id, MapState state` | `void` | Setup node |
| `SetState` | `NodeState state, bool animate` | `void` | Update visual state |
| `PlayShake` | - | `void` | Shake animation (locked) |

#### Usage Example
```csharp
// Setup
LevelNodeView nodeView = GetComponent<LevelNodeView>();
nodeView.Initialize(new NodeId(2), mapState);

// Update state
NodeState newState = NodeState.Completed;
nodeView.SetState(newState, animate: true); // Plays pop animation

// Locked feedback
nodeView.PlayShake(); // Called when clicking inactive node
```

---

### **NodeData**
**Location:** `Assets/Scripts/Nodemap/Content/NodeData.cs`  
**Purpose:** ScriptableObject holding all node content

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| `id` | `int` | Node index (0-5) |
| `title` | `string` | Display name |
| `slideDeck` | `SlideDeck` | Learning slides + mini-games |
| `quizData` | `QuizData` | Final quiz |
| `upgradeFrame` | `Sprite` | Car frame upgrade |
| `upgradeTire` | `Sprite` | Tire upgrade |

#### Usage Example
```csharp
// Load from Resources
NodeData nodeData = Resources.Load<NodeData>("Data/Nodes/Node_01");

// Access content
string title = nodeData.title; // "Battery Basics"
SlideDeck deck = nodeData.slideDeck;
QuizData quiz = nodeData.quizData;

// Apply upgrades
carVisual.ApplyUpgrade(nodeData.upgradeFrame, nodeData.upgradeTire);
```

---

## Car Components

### **CarMovementController**
**Location:** `Assets/Scripts/Nodemap/Car/CarMovementController.cs`  
**Purpose:** Animate car along spline between nodes

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `MoveToNode` | `NodeId target, float targetT` | `void` | Animate to new position |
| `SnapToNode` | `NodeId target, float targetT` | `void` | Instant teleport (no animation) |

#### Events
| Event | Signature | Fired When |
|-------|-----------|------------|
| `OnStartedMovingToNode` | `Action<NodeId>` | Movement begins |
| `OnArrivedAtNode` | `Action<NodeId>` | Movement completes |

#### Configuration (via MapConfig)
| Config Key | Type | Default | Description |
|------------|------|---------|-------------|
| `moveSpeed` | `float` | `2.0` | Units per second |
| `bouncePeriod` | `float` | `5.0` | Bounce frequency (Hz) |
| `bounceAmplitude` | `float` | `0.1` | Bounce height |
| `wheelSpinSpeed` | `float` | `360.0` | Degrees per second |

#### Usage Example
```csharp
// Subscribe to events
carController.OnStartedMovingToNode += (id) =>
{
    Debug.Log($"Moving to node {id.Value}");
};

carController.OnArrivedAtNode += (id) =>
{
    mapState.SaveToPlayerPrefs();
};

// Move car
NodeId target = new NodeId(3);
float targetT = nodeManager.GetTargetT(target);
carController.MoveToNode(target, targetT);

// Snap instantly (on load)
carController.SnapToNode(initialNode, initialT);
```

---

### **CarVisual**
**Location:** `Assets/Scripts/Nodemap/Car/CarVisual.cs`  
**Purpose:** Apply visual upgrades to car (frame, tires)

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ApplyUpgrade` | `Sprite frame, Sprite tire` | `void` | Change car sprites |

#### Usage Example
```csharp
// After node completion
CarVisual carVisual = car.GetComponent<CarVisual>();
NodeData nodeData = GetCompletedNodeData();

carVisual.ApplyUpgrade(
    nodeData.upgradeFrame,
    nodeData.upgradeTire
);
```

---

## Content Components

### **SlideDeck**
**Location:** `Assets/Scripts/Nodemap/Content/SlideDeck.cs`  
**Purpose:** Collection of slides and mini-games for a node

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| `slides` | `SlideReference[]` | Mixed learning + mini-game content |

#### SlideReference Structure
```csharp
[Serializable]
public class SlideReference
{
    public GameObject slidePrefab;  // Prefab to instantiate
    public SlideType type;          // Learning or MiniGame
    public string key;              // For quiz references (e.g., "slide_03")
}

public enum SlideType
{
    Learning,   // Regular slide
    MiniGame    // Interactive checkpoint
}
```

#### Usage Example
```csharp
SlideDeck deck = nodeData.slideDeck;

for (int i = 0; i < deck.slides.Length; i++)
{
    SlideReference slideRef = deck.slides[i];
    
    if (slideRef.type == SlideType.Learning)
    {
        // Show learning slide
        GameObject slide = Instantiate(slideRef.slidePrefab, slideContainer);
    }
    else if (slideRef.type == SlideType.MiniGame)
    {
        // Show mini-game (blocks Next until complete)
        GameObject miniGame = Instantiate(slideRef.slidePrefab, slideContainer);
        IMiniGame game = miniGame.GetComponent<IMiniGame>();
        game.OnCompleted += (success) =>
        {
            if (success) EnableNextButton();
        };
    }
}
```

---

### **IMiniGame** (Interface)
**Location:** `Assets/Scripts/Nodemap/Content/Interfaces/IMiniGame.cs`  
**Purpose:** Standard contract for all mini-games

#### Interface Definition
```csharp
public interface IMiniGame
{
    event Action<bool> OnCompleted;  // true = success, false = retry
    void ResetGame();                // Reset to initial state
}
```

#### Implementation Example
```csharp
public class WordUnscrambleSlide : MonoBehaviour, IMiniGame
{
    public event Action<bool> OnCompleted;
    
    private string correctAnswer = "BATTERY";
    
    public void CheckAnswer(string userAnswer)
    {
        if (userAnswer == correctAnswer)
        {
            OnCompleted?.Invoke(true);  // Success!
        }
        else
        {
            OnCompleted?.Invoke(false); // Try again
        }
    }
    
    public void ResetGame()
    {
        // Clear input field, re-scramble letters
    }
}
```

---

### **QuizData**
**Location:** `Assets/Scripts/Nodemap/Content/QuizData.cs`  
**Purpose:** ScriptableObject holding quiz questions

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| `nodeId` | `int` | Which node this quiz belongs to |
| `questions` | `QuizQuestion[]` | Array of questions |

#### QuizQuestion Structure
```csharp
[Serializable]
public class QuizQuestion
{
    public string question;              // "What stores energy?"
    public string[] options;             // ["Battery", "Motor", "Inverter"]
    public int correctIndex;             // 0
    public string[] relatedSlideKeys;    // ["slide_03", "slide_05"]
}
```

#### Usage Example
```csharp
QuizData quizData = Resources.Load<QuizData>("Data/Quiz/Quiz_Node01");

foreach (QuizQuestion q in quizData.questions)
{
    Debug.Log(q.question);
    for (int i = 0; i < q.options.Length; i++)
    {
        Debug.Log($"{i}: {q.options[i]}");
    }
}
```

---

## Animation Components

### **NodeStateAnimation**
**Location:** `Assets/Scripts/Nodemap/Animations/NodeStateAnimation.cs`  
**Purpose:** Visual feedback for node state changes (pop/shake)

#### Public Methods
| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `PlayPop` | - | `void` | Scale up/down animation |
| `PlayShake` | - | `void` | Horizontal shake (locked nodes) |

#### Configuration (via MapConfig)
| Config Key | Type | Default | Description |
|------------|------|---------|-------------|
| `popDuration` | `float` | `0.2` | Pop animation length |
| `popScale` | `float` | `1.2` | Scale multiplier |
| `shakeDuration` | `float` | `0.3` | Shake animation length |
| `shakeIntensity` | `float` | `10.0` | Shake distance |

#### Usage Example
```csharp
// Attached to node GameObject
NodeStateAnimation anim = node.GetComponent<NodeStateAnimation>();

// Trigger animations
anim.PlayPop();   // Called when state changes
anim.PlayShake(); // Called when clicking locked node
```

---

## Common Usage Patterns

### **1. Check if Node is Accessible**
```csharp
NodeId nodeId = new NodeId(3);

if (mapState.IsNodeUnlocked(nodeId))
{
    // User can interact with this node
    popupController.Open(nodeData, isCompleted: false);
}
else
{
    // Show locked feedback
    nodeView.PlayShake();
}
```

---

### **2. Complete a Node**
```csharp
// Called when quiz is passed
public void OnQuizPassed(NodeId nodeId)
{
    bool success = mapState.TryCompleteNode(nodeId);
    
    if (success)
    {
        // Triggers:
        // - OnNodeCompletedChanged event
        // - Auto-unlocks next node
        // - Moves car (via OnCarNodeChanged)
    }
}
```

---

### **3. Subscribe to State Events**
```csharp
void OnEnable()
{
    mapState.OnNodeCompletedChanged += HandleNodeCompleted;
    mapState.OnCarNodeChanged += HandleCarMoved;
}

void OnDisable()
{
    mapState.OnNodeCompletedChanged -= HandleNodeCompleted;
    mapState.OnCarNodeChanged -= HandleCarMoved;
}

private void HandleNodeCompleted(NodeId id, bool completed)
{
    Debug.Log($"Node {id.Value} completed: {completed}");
}

private void HandleCarMoved(NodeId id)
{
    Debug.Log($"Car moved to node {id.Value}");
}
```

---

### **4. Load and Display Slides**
```csharp
public void ShowLearningContent(NodeData nodeData)
{
    SlideDeck deck = nodeData.slideDeck;
    
    foreach (SlideReference slideRef in deck.slides)
    {
        GameObject slideObj = Instantiate(slideRef.slidePrefab, slideContainer);
        
        if (slideRef.type == SlideType.MiniGame)
        {
            IMiniGame miniGame = slideObj.GetComponent<IMiniGame>();
            miniGame.OnCompleted += (success) =>
            {
                if (success)
                {
                    nextButton.interactable = true;
                }
                else
                {
                    ShowFeedback("Try again!");
                }
            };
            
            nextButton.interactable = false; // Block until complete
        }
    }
}
```

---

### **5. Handle Quiz Review Flow**
```csharp
quizController.OnWrongAnswer += (relatedSlideKeys) =>
{
    // Show review UI
    reviewButton.gameObject.SetActive(true);
    
    reviewButton.onClick.AddListener(() =>
    {
        // Jump back to slides
        popupController.JumpToSlideByKey(relatedSlideKeys[0]);
        
        // Restart quiz when returning
        quizController.RestartQuiz();
    });
};
```

---

### **6. Animate Car Movement**
```csharp
private void MoveCarToNode(NodeId target)
{
    float targetT = nodeManager.GetTargetT(target);
    
    carController.OnArrivedAtNode += HandleArrived;
    carController.MoveToNode(target, targetT);
}

private void HandleArrived(NodeId id)
{
    carController.OnArrivedAtNode -= HandleArrived;
    
    // Save progress
    mapState.SaveToPlayerPrefs();
    
    // Show upgrade popup
    popupController.ShowUpgrade(nodeData);
}
```

---

### **7. Access Configuration Values**
```csharp
public class MyComponent : ConfigurableComponent
{
    void Start()
    {
        // Safe config access with fallback
        float speed = GetConfig(c => c.moveSpeed, 2.0f);
        float popDur = GetConfig(c => c.popDuration, 0.2f);
        
        // Use values
        StartCoroutine(AnimateRoutine(popDur));
    }
}
```

---

### **8. Reset Progress (Debug)**
```csharp
// MapControllerSimple
public void ResetProgression()
{
    mapState.ResetProgression();    // Clear state
    mapState.SaveToPlayerPrefs();   // Persist cleared state
    
    // Reload scene to refresh visuals
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
```

---

### **9. Load Node Data**
```csharp
// From Resources folder
NodeData nodeData = Resources.Load<NodeData>($"Data/Nodes/Node_{nodeId.Value:00}");

if (nodeData != null)
{
    popupController.Open(nodeData, mapState.IsNodeCompleted(nodeId));
}
else
{
    Debug.LogError($"Missing NodeData for node {nodeId.Value}");
}
```

---

### **10. Apply Car Upgrades**
```csharp
private void ShowUpgradeReward(NodeData nodeData)
{
    // Show upgrade popup with sprites
    upgradePopup.Show(nodeData.upgradeFrame, nodeData.upgradeTire);
    
    // Wait for user to close popup
    upgradePopup.OnClosed += () =>
    {
        // Apply to car
        carVisual.ApplyUpgrade(nodeData.upgradeFrame, nodeData.upgradeTire);
    };
}
```

---

## Quick Reference: Event Flow

```
User Action → Controller → MapState → Events → Views Update
```

**Example: Completing Node 0**
```
QuizController.OnQuizPassed fires
  ↓
MapController.CompleteNode(0) called
  ↓
MapState.TryCompleteNode(0) executes
  ├─ Updates internal state
  ├─ Fires OnNodeCompletedChanged(0, true)
  └─ Calls TryUnlockNextNode(0)
      ├─ Unlocks Node 1
      ├─ Fires OnNodeUnlockedChanged(1, true)
      └─ Fires OnCarNodeChanged(1)
  ↓
Views respond to events:
  ├─ LevelNodeView (Node 0) changes to green
  ├─ LevelNodeView (Node 1) changes to yellow
  └─ CarMovementController animates to Node 1
```

---

**End of Component API Reference**
