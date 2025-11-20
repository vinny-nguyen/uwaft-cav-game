# Unity Setup Guide - Quiz System

All the code is complete. Here's what you need to do in Unity Editor.

---

## Step 1: Create Quiz Option Button Prefab (5 min)

1. In Project window, create folder: `Assets/Prefabs/Quiz`
2. In Hierarchy: Right-click → UI → Button - TextMeshPro
3. Rename to `QuizOptionButton`
4. Configure:
   - Width: 700, Height: 80
   - Style the button colors (Normal, Highlighted, Pressed)
   - Child Text (TMP): Font size 24-28, Center alignment
5. Drag to `Assets/Prefabs/Quiz/` to save as prefab
6. Delete from Hierarchy

---

## Step 2: Create Quiz Slide Prefab (15 min)

1. In Hierarchy: Right-click → Create Empty → Name it `QuizSlide`
2. Add component: `QuizController`
3. Add these children to QuizSlide:

```
QuizSlide
├─ QuestionText (TextMeshPro - Text)
├─ ProgressText (TextMeshPro - Text)  
├─ OptionsContainer (Empty GameObject + Vertical Layout Group)
├─ FeedbackText (TextMeshPro - Text)
├─ NextQuestionButton (Button - TMP)
├─ ReviewSlideButton (Button - TMP)
└─ CompletionPanel (Panel/Image)
    └─ CompletionText (TextMeshPro - Text)
```

**Configure each:**

**QuestionText:**
- Anchor: Top-Center
- Font Size: 36, Center alignment
- Width: 800, Height: 150

**ProgressText:**
- Anchor: Top-Right
- Font Size: 24, Right alignment
- Text: "Question 1/4"

**OptionsContainer:**
- Add `Vertical Layout Group`:
  - Spacing: 15
  - Child Alignment: Middle Center
  - Control Child Size: Width ✓
  - Child Force Expand: Width ✓

**FeedbackText:**
- Anchor: Bottom-Center (above buttons)
- Font Size: 28, Center alignment

**NextQuestionButton:**
- Anchor: Bottom-Right
- Text: "Next Question"
- Set Active: **FALSE** (uncheck in Inspector)

**ReviewSlideButton:**
- Anchor: Bottom-Center
- Text: "Review Slide"
- Set Active: **FALSE** (uncheck in Inspector)

**CompletionPanel:**
- Stretch to full screen
- Semi-transparent background
- Set Active: **FALSE** (uncheck in Inspector)
- CompletionText: Large font, center, "Congratulations!"

4. **Wire QuizController references:**
   - Question Text → QuestionText
   - Progress Text → ProgressText
   - Feedback Text → FeedbackText
   - Options Container → OptionsContainer
   - Option Button Prefab → QuizOptionButton.prefab
   - Next Question Button → NextQuestionButton
   - Review Slide Button → ReviewSlideButton
   - Completion Panel → CompletionPanel
   - Completion Text → CompletionText

5. Drag `QuizSlide` to `Assets/Prefabs/Quiz/` to save as prefab
6. Delete from Hierarchy

---

## Step 3: Create Quiz Transition Slide Prefab (5 min)

1. Duplicate `Assets/Prefabs/Slides/SlideTemplate.prefab` 
2. Rename to `QuizTransitionSlide`
3. Open for editing
4. Add component: `QuizTransitionSlide` (inherits from SlideBase)
5. Set SlideBase Key: "QuizTransition"
6. Design the slide:
   - Add heading text: "Congratulations! You've completed the learning module."
   - Add subtitle: "Ready to test your knowledge?"
   - Add large button: "Take Quiz"
7. Wire `QuizTransitionSlide` component:
   - Take Quiz Button → drag the button
8. Save prefab

---

## Step 4: Configure PopupController (2 min)

1. Open scene: `UpdatedNodemap`
2. Find in Hierarchy: `UI/PopupCanvas/PopupPanel`
3. Select it, find `PopupController` component
4. Assign:
   - Quiz Prefab → `QuizSlide.prefab`
   - Quiz Container → leave empty (uses SlidesContainer)

---

## Step 5: Add QuizCompletionHandler (1 min)

1. In Hierarchy: Select `Controllers/MapController`
2. Add Component → `QuizCompletionHandler`
3. Done (it auto-wires at runtime)

---

## Step 6: Update SlideDeck Assets (5 min)

For **each** of the 6 SlideDeck assets in `Assets/Data/Slides/`:

1. Select the asset (e.g., `TiresSlides.asset`)
2. In Inspector, find Slides list
3. Click `+` to add new entry at the end
4. Set:
   - Key: "QuizTransition"
   - Slide Prefab: `QuizTransitionSlide.prefab`
5. Repeat for all 6 topics:
   - TiresSlides.asset
   - BatterySlides.asset
   - ElectricMotorsSlides.asset
   - SuspensionSlides.asset
   - RegenBrakingSlides.asset
   - AerodynamicsSlides.asset

---

## Step 7: Assign Quiz JSON to NodeData (3 min)

For **each** NodeData asset (find them in your project):

1. Select the asset (e.g., Tires NodeData)
2. Find `Quiz Json` field
3. Assign the matching JSON:
   - Tires → `TiresQuiz.json`
   - Battery → `BatteryQuiz.json`
   - ElectricMotors → `ElectricMotorsQuiz.json`
   - Suspension → `SuspensionQuiz.json`
   - RegenBraking → `RegenBrakingQuiz.json`
   - Aerodynamics → `AerodynamicsQuiz.json`

---

## Step 8: Set Slide Keys (IMPORTANT!)

The quiz JSON files use these keys to link to slides:
- `TiresSlide_1`, `TiresSlide_2`, etc.
- `BatterySlide_1`, `BatterySlide_2`, etc.

**For each slide prefab:**
1. Open the prefab
2. Find `SlideBase` component
3. Set the `Key` field to match the convention above

**OR** update the quiz JSON files to match your existing slide keys.

Example: If a slide's key is `TiresSlide_1`, the quiz JSON should reference `"relatedSlideKey": "TiresSlide_1"`

---

## Testing

1. Play the scene
2. Click first node
3. Navigate through slides
4. Last slide should show "Take Quiz" button
5. Click it:
   - Slide navigation hides
   - Quiz UI appears
6. Answer questions:
   - Correct → "Next Question" appears
   - Incorrect → "Review Slide" appears
7. Click "Review Slide" → jumps to related content
8. Navigate back to quiz transition slide
9. Click "Take Quiz" again (resets)
10. Get all answers correct
11. Node completes, next unlocks, car moves

---

## Troubleshooting

**Quiz doesn't appear:**
- Check Quiz Prefab assigned in PopupController
- Check console for errors

**Review Slide doesn't work:**
- Verify slide keys match quiz JSON exactly
- Check SlideBase component has Key field set

**Node doesn't complete:**
- Verify QuizCompletionHandler is on MapController
- Check console for warnings

**No options show:**
- Check Option Button Prefab assigned in QuizController
- Verify quiz JSON is valid

---

## Total Time: ~35 minutes

All code is ready - just follow these steps and test!
