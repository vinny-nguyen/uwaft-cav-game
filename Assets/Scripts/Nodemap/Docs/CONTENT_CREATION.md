# Content Creation Workflow

> **For Designers and Content Creators:** Step-by-step guide to creating node content without touching code.  
> **For AI Assistants:** Reference for content pipeline and tooling.

**Last Updated:** November 9, 2025  
**Audience:** Non-technical content creators + AI assistants building tools

---

## Table of Contents
1. [Overview](#overview)
2. [PowerPoint to Unity Workflow](#powerpoint-to-unity-workflow)
3. [Slide Deck Builder Tool](#slide-deck-builder-tool)
4. [Adding Hotspots Manually](#adding-hotspots-manually)
5. [Creating Quizzes](#creating-quizzes)
6. [Mini-Game Integration](#mini-game-integration)
7. [Testing Your Content](#testing-your-content)
8. [Troubleshooting](#troubleshooting)

---

## Overview

**Goal:** Enable designers to create educational content for nodes **without writing code**.

**Pipeline:**
```
PowerPoint Slides → Export as Images → Slide Deck Builder Tool → Unity Prefabs → SlideDeck Asset
```

**What You Create:**
1. **Slides** - Full-screen educational images (exported from PowerPoint)
2. **Hotspots** - Optional clickable info popups (added manually in Unity)
3. **Quizzes** - JSON file with questions and slide references
4. **Mini-Games** - Prefabs inserted between slides (created by developers)

**What You Don't Touch:**
- C# scripts (handled by developers)
- Complex animations (auto-generated)
- State management (handled by MapState)

---

## PowerPoint to Unity Workflow

### **Step 1: Create PowerPoint Slides**

**Requirements:**
- Slide size: **16:9 aspect ratio** (1920x1080 recommended)
- Font size: **36pt minimum** for readability
- High contrast: Dark text on light background (or vice versa)
- No animations: They won't translate to Unity

**Content Guidelines:**
- **1 concept per slide** - Don't overcrowd
- **Visual hierarchy** - Title → Diagram → Key Points
- **Consistent style** - Use same color scheme across all nodes

**Example Slide Structure:**
```
Slide 1: Title + Overview
Slide 2: Component Diagram (Battery)
Slide 3: How It Works (Energy Flow)
Slide 4: Real-World Example (EV Battery Pack)
Slide 5: Key Takeaways
```

---

### **Step 2: Export Slides as Images**

**PowerPoint Export Process:**
1. File → Save As
2. Choose location: `Assets/Resources/Slides/Node_01/`
3. Save as type: **PNG**
4. PowerPoint will ask: "Every Slide" → Click **Every Slide**

**Naming Convention:**
PowerPoint auto-generates:
```
slide__01.png
slide__02.png
slide__03.png
slide__04.png
slide__05.png
...
```

**Important:** Keep the `slide__##.png` format (double underscore).

**File Structure After Export:**
```
Assets/Resources/Slides/
  ├─ Node_01/
  │   ├─ slide__01.png
  │   ├─ slide__02.png
  │   ├─ slide__03.png
  │   └─ ...
  ├─ Node_02/
  │   ├─ slide__01.png
  │   └─ ...
  └─ ...
```

---

### **Step 3: Import to Unity**

**Unity Import Settings (Automatic):**
- Texture Type: **Sprite (2D and UI)**
- Pixels Per Unit: 100
- Filter Mode: Bilinear
- Max Size: 2048

**Manual Check:**
1. Select all slide images in Unity Project window
2. Inspector → Texture Type should be "Sprite (2D and UI)"
3. If not, change it and click Apply

---

## Slide Deck Builder Tool

### **What It Does**

The Slide Deck Builder is a **Unity Editor tool** that:
- Scans a folder for `slide__##.png` images
- Auto-generates slide prefabs (one per image)
- Inserts mini-game prefabs at specified positions
- Creates a `SlideDeck` ScriptableObject asset
- Links everything together

**Result:** Fully configured SlideDeck ready to use in NodeData.

---

### **Using the Tool**

#### **1. Open the Tool**
```
Unity Menu Bar → Tools → UWAFT → Slide Deck Builder
```

A window will appear with these fields:
- **Node ID** - Which node (0-5)
- **Slides Folder** - Where your images are
- **Mini-Game Insertions** - Where to place interactive activities
- **Output Path** - Where to save the SlideDeck asset

---

#### **2. Configure Settings**

**Example Configuration:**
```
Node ID: 1
Slides Folder: Assets/Resources/Slides/Node_01
Mini-Game Insertions:
  - Position: After slide 3
    Prefab: Prefabs/MiniGames/WordUnscramble_Battery
  - Position: After slide 7
    Prefab: Prefabs/MiniGames/DragDrop_Components
Output Path: Assets/Data/SlideDecks/Node_01_SlideDeck.asset
```

**What This Means:**
- Build deck for Node 1
- Use images from `Slides/Node_01/`
- Insert Word Unscramble after slide 3
- Insert Drag & Drop after slide 7
- Save result to `Data/SlideDecks/`

---

#### **3. Generate Slide Deck**

Click **"Generate Slide Deck"** button.

**What Happens:**
1. Tool scans folder for `slide__##.png` files
2. Creates prefab for each slide (image fills screen)
3. Inserts mini-game prefabs at specified positions
4. Creates `SlideDeck` asset with `SlideReference[]` array
5. Logs result to Unity Console

**Console Output:**
```
[Slide Deck Builder] Found 10 slide images
[Slide Deck Builder] Created 10 slide prefabs
[Slide Deck Builder] Inserted 2 mini-games
[Slide Deck Builder] SlideDeck created at: Assets/Data/SlideDecks/Node_01_SlideDeck.asset
[Slide Deck Builder] Total slides: 12 (10 learning + 2 mini-games)
```

---

#### **4. Verify Output**

**Check SlideDeck Asset:**
1. Navigate to `Assets/Data/SlideDecks/Node_01_SlideDeck.asset`
2. Select it in Project window
3. Inspector shows `slides` array (size 12)
4. Each element has:
   - **Slide Prefab** - Reference to prefab
   - **Type** - Learning or MiniGame
   - **Key** - Identifier (e.g., "slide_03")

**Expected Structure:**
```
slides[0]:  Slide_01 (Learning, key: "slide_01")
slides[1]:  Slide_02 (Learning, key: "slide_02")
slides[2]:  Slide_03 (Learning, key: "slide_03")
slides[3]:  WordUnscramble (MiniGame, key: "minigame_01")
slides[4]:  Slide_04 (Learning, key: "slide_04")
slides[5]:  Slide_05 (Learning, key: "slide_05")
slides[6]:  Slide_06 (Learning, key: "slide_06")
slides[7]:  Slide_07 (Learning, key: "slide_07")
slides[8]:  DragDrop (MiniGame, key: "minigame_02")
slides[9]:  Slide_08 (Learning, key: "slide_08")
slides[10]: Slide_09 (Learning, key: "slide_09")
slides[11]: Slide_10 (Learning, key: "slide_10")
```

---

### **5. Link to NodeData**

**Assign SlideDeck to Node:**
1. Open `Assets/Data/Nodes/Node_01.asset`
2. Find **Slide Deck** field
3. Drag `Node_01_SlideDeck.asset` into the field
4. Save (Ctrl+S)

**Done!** Node 1 now has slides + mini-games.

---

## Adding Hotspots Manually

**What are Hotspots?**
Clickable info buttons on slides that show extra details in a popup.

**Example Use Case:**
- Main slide shows Battery Diagram
- Hotspot on "Anode" → Popup explains anode function
- Hotspot on "Cathode" → Popup explains cathode function

---

### **Step 1: Open Slide Prefab**

1. Navigate to `Assets/Prefabs/Slides/Node_01/Slide_03.prefab`
2. Double-click to open Prefab Mode

---

### **Step 2: Add Hotspot Button**

1. Right-click `Slide_03` in Hierarchy
2. UI → Button - TextMeshPro
3. Rename to "Hotspot_Anode"
4. Position over the part of the image you want to highlight

**Recommended Hotspot Style:**
- **Size:** 40x40 pixels (small icon)
- **Icon:** Info icon (ⓘ) or question mark (?)
- **Color:** Semi-transparent (alpha 0.7)
- **Position:** Directly on diagram element

---

### **Step 3: Configure Hotspot**

**Add HotspotBehavior Script:**
1. Select `Hotspot_Anode` button
2. Inspector → Add Component → `HotspotBehavior`
3. Fill in fields:
   - **Popup Text:** "The anode is the negative terminal where electrons exit during discharge..."
   - **Popup Title:** "Anode"

**Button Click Setup (Already Handled):**
`HotspotBehavior` auto-wires the button's `onClick` event.

---

### **Step 4: Style the Hotspot**

**Visual Feedback:**
- Idle: Semi-transparent white
- Hover: Full opacity + slight scale (1.1x)
- Clicked: Show tooltip popup

**Prefab Variant (Optional):**
Create a "Hotspot" prefab variant for consistency:
1. Create first hotspot
2. Right-click → Create → Prefab Variant
3. Save as `Assets/Prefabs/UI/Hotspot.prefab`
4. Reuse for all future hotspots (just change text)

---

### **Step 5: Test Hotspot**

1. Save prefab (Ctrl+S)
2. Enter Play Mode
3. Click the node containing this slide
4. Navigate to the slide
5. Click the hotspot icon
6. Verify popup appears with correct text

---

## Creating Quizzes

### **Quiz File Structure**

Quizzes are stored as **JSON files** in `Assets/Resources/QuizData/`.

**File Naming:** `Quiz_Node_##.json` (e.g., `Quiz_Node_01.json`)

---

### **JSON Format**

```json
{
  "nodeId": 1,
  "questions": [
    {
      "question": "What component stores electrical energy in an EV?",
      "options": [
        "Battery Pack",
        "Electric Motor",
        "Inverter",
        "Onboard Charger"
      ],
      "correctIndex": 0,
      "relatedSlideKeys": ["slide_02", "slide_03"]
    },
    {
      "question": "What converts DC power to AC power for the motor?",
      "options": [
        "Battery Management System",
        "DC-DC Converter",
        "Inverter",
        "Charger"
      ],
      "correctIndex": 2,
      "relatedSlideKeys": ["slide_05"]
    },
    {
      "question": "Which system monitors battery cell voltages?",
      "options": [
        "Inverter",
        "Battery Management System (BMS)",
        "Onboard Charger",
        "Motor Controller"
      ],
      "correctIndex": 1,
      "relatedSlideKeys": ["slide_04", "slide_06"]
    }
  ]
}
```

---

### **Field Descriptions**

| Field | Type | Description |
|-------|------|-------------|
| `nodeId` | `int` | Which node (0-5) |
| `question` | `string` | Question text |
| `options` | `string[]` | Array of 2-4 answer choices |
| `correctIndex` | `int` | Index of correct answer (0-based) |
| `relatedSlideKeys` | `string[]` | Slides to review if wrong (e.g., `["slide_03"]`) |

---

### **Best Practices**

**Question Quality:**
- Clear and unambiguous
- Test understanding, not memorization
- Avoid "trick" questions

**Answer Options:**
- 3-4 options (fewer = guessing, more = overwhelming)
- Similar plausibility (avoid obvious wrong answers)
- Consistent length (don't make correct answer longer)

**Related Slides:**
- Link to slides that explain the concept
- Usually 1-2 slides per question
- Use the slide's `key` from SlideDeck

---

### **Creating QuizData Asset**

**Option A: Import JSON to ScriptableObject (Automatic)**
1. Place JSON file in `Assets/Resources/QuizData/`
2. Unity auto-detects and creates `QuizData` asset
3. Asset appears in same folder

**Option B: Create Manually**
1. Right-click in Project → Create → UWAFT → Quiz Data
2. Name it `Quiz_Node_01`
3. Fill in fields in Inspector

**Link to NodeData:**
1. Open `Assets/Data/Nodes/Node_01.asset`
2. Assign `Quiz_Node_01` to **Quiz Data** field

---

## Mini-Game Integration

### **What are Mini-Games?**

Interactive checkpoints **embedded between slides**. Players must complete them to continue.

**Types Available:**
- **Word Unscramble** - Rearrange letters to form correct term
- **Drag & Drop** - Match components to descriptions
- **Memory Match** - Flip cards to find pairs

---

### **Mini-Game Prefabs**

Located in `Assets/Prefabs/MiniGames/`

Each prefab is **pre-configured** by developers and ready to use.

**Example Prefabs:**
```
WordUnscramble_Battery.prefab
DragDrop_Components.prefab
MemoryMatch_EVParts.prefab
```

---

### **Inserting Mini-Games**

#### **Option 1: Use Slide Deck Builder Tool**

When configuring the tool, specify insertions:
```
Mini-Game Insertions:
  - After slide 3: Prefabs/MiniGames/WordUnscramble_Battery
  - After slide 7: Prefabs/MiniGames/DragDrop_Components
```

Tool auto-inserts them in the correct positions.

---

#### **Option 2: Manual Insertion (Advanced)**

1. Open `SlideDeck` asset in Inspector
2. Find `slides` array
3. Insert new element at desired index
4. Set fields:
   - **Slide Prefab:** Drag mini-game prefab
   - **Type:** MiniGame
   - **Key:** `minigame_##`

---

### **Testing Mini-Games**

1. Enter Play Mode
2. Navigate to the slide before the mini-game
3. Click Next
4. Mini-game loads
5. Verify:
   - Next button is disabled
   - Instructions are clear
   - Incorrect attempts give feedback
   - Correct answer enables Next button

---

## Testing Your Content

### **Test Checklist**

**Slide Flow:**
- [ ] All slides display correctly
- [ ] Images are high-resolution (not blurry)
- [ ] Text is readable (not too small)
- [ ] Next/Previous buttons work
- [ ] Slide count is correct

**Hotspots:**
- [ ] Hotspots are visible
- [ ] Hotspots are positioned correctly
- [ ] Clicking shows tooltip with correct text
- [ ] Tooltip closes when clicking outside

**Mini-Games:**
- [ ] Mini-games appear at correct positions
- [ ] Next button is disabled until completion
- [ ] Wrong answers give feedback
- [ ] Correct answers enable Next button
- [ ] Mini-games can be retried

**Quiz:**
- [ ] All questions display
- [ ] Answer options are correct
- [ ] Correct answer is marked correctly in JSON
- [ ] Wrong answer shows "Review" button
- [ ] Review jumps to correct slide
- [ ] Quiz restarts after review
- [ ] Passing quiz completes node

**Node Completion:**
- [ ] Node changes to green (completed) after quiz
- [ ] Next node unlocks
- [ ] Car moves to next node
- [ ] Progress saves (reload scene to verify)

---

### **Play Mode Testing**

**Full Playthrough:**
1. Enter Play Mode
2. Click Node (should open content)
3. Navigate all slides
4. Complete all mini-games
5. Take quiz
6. Intentionally get one question wrong
7. Click Review
8. Navigate slides again
9. Retake quiz
10. Pass all questions
11. Verify node completes

**Edge Cases:**
- Spam clicking Next (should not skip slides)
- Closing popup mid-session (should remember position)
- Wrong answer on last quiz question (review still works?)

---

## Troubleshooting

### **Problem: Slides Not Showing**

**Symptom:** Popup opens but slides are blank.

**Causes:**
1. Images not in `Resources/` folder
2. SlideDeck not assigned to NodeData
3. Slide prefabs missing references

**Fix:**
1. Verify images are in `Assets/Resources/Slides/Node_##/`
2. Check NodeData has SlideDeck assigned
3. Open slide prefab → verify Image component has sprite assigned

---

### **Problem: Mini-Games Not Blocking Next Button**

**Symptom:** Can skip mini-games without completing them.

**Causes:**
1. Mini-game doesn't implement `IMiniGame` interface
2. PopupController not listening to `OnCompleted` event

**Fix:**
1. Check mini-game script has `public event Action<bool> OnCompleted;`
2. Verify PopupController subscribes to event

---

### **Problem: Quiz Review Not Working**

**Symptom:** Clicking Review button does nothing.

**Causes:**
1. `relatedSlideKeys` array is empty
2. Slide keys don't match SlideDeck keys
3. PopupController doesn't have `JumpToSlideByKey` method

**Fix:**
1. Check JSON quiz file has `relatedSlideKeys: ["slide_03"]`
2. Verify keys match slide prefab names
3. Test with developer

---

### **Problem: Hotspots Not Clickable**

**Symptom:** Clicking hotspot does nothing.

**Causes:**
1. Button `onClick` event not wired
2. `HotspotBehavior` script missing
3. Hotspot is behind another UI element

**Fix:**
1. Select hotspot → Inspector → check `onClick` has listener
2. Add `HotspotBehavior` component if missing
3. Adjust Canvas sorting order or Z-position

---

### **Problem: Images are Blurry**

**Symptom:** Slides look pixelated or low-resolution.

**Causes:**
1. Source PowerPoint slides too small
2. Unity Max Size set too low
3. Filter Mode set to Point instead of Bilinear

**Fix:**
1. Re-export PowerPoint at 1920x1080 minimum
2. Select image → Inspector → Max Size: 2048
3. Filter Mode: Bilinear

---

### **Problem: Node Won't Complete**

**Symptom:** Passed quiz but node stays yellow (not green).

**Causes:**
1. `QuizController.OnQuizPassed` event not firing
2. MapState not receiving completion call
3. PlayerPrefs not saving

**Fix:**
1. Check Console for errors
2. Verify quiz `correctIndex` values are accurate
3. Test with developer

---

## Quick Reference: Complete Workflow

```
1. CREATE POWERPOINT
   ├─ 16:9 aspect ratio
   ├─ High contrast
   └─ 1 concept per slide

2. EXPORT AS IMAGES
   ├─ Save as PNG
   ├─ "Every Slide"
   └─ Auto-named: slide__01.png, slide__02.png, ...

3. IMPORT TO UNITY
   ├─ Place in Assets/Resources/Slides/Node_##/
   └─ Verify Texture Type: Sprite (2D and UI)

4. RUN SLIDE DECK BUILDER
   ├─ Tools → UWAFT → Slide Deck Builder
   ├─ Set Node ID, Slides Folder
   ├─ Add Mini-Game Insertions
   └─ Generate Slide Deck

5. ADD HOTSPOTS (Optional)
   ├─ Open slide prefab
   ├─ Add Button (UI → Button - TextMeshPro)
   ├─ Add HotspotBehavior script
   ├─ Configure popup text
   └─ Position over diagram

6. CREATE QUIZ
   ├─ Write JSON file (Quiz_Node_##.json)
   ├─ Place in Assets/Resources/QuizData/
   ├─ Create QuizData asset
   └─ Assign to NodeData

7. TEST CONTENT
   ├─ Enter Play Mode
   ├─ Complete full playthrough
   ├─ Test review flow
   └─ Verify node completion

8. PUBLISH
   ├─ Commit to Git
   └─ Notify team
```

---

**End of Content Creation Workflow**
