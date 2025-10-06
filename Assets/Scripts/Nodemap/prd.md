# EV Learning Game – MVP PRD

## 1. Goal

Create a learning game where students unlock and learn about six EV components by progressing through nodes on a map.  
At each node:

- Students view a learning popup (slides, diagrams, mini-popups).
- They complete a short quiz (4–5 questions).
- They play one or more simple mini-games (crossword, word search, mix & match).
- After success, the car is upgraded visually and the next node unlocks.

Driving gameplay is handled by another team.

---

## 2. Core Features

- **Map with 6 Nodes**

  - States: inactive (gray), active (yellow), completed (green).
  - Sprites come from `Inactive/`, `Active/`, `Completed/` folders.

- **Car Movement**

  - Car starts off-screen and moves along a predefined spline to the current node.
  - Car moves to new node when unlocked.
  - Car appearance = **frame + tires assets combined**. Upgrades replace these parts.

- **Learning Popups**

  - Slides authored directly in Unity (designer-controlled).
  - Supports dropdowns and clickable diagram hotspots (mini-popups).
  - Quiz at the end, loaded from a simple JSON-like file (questions, options, correct answer, reference slide).
  - Wrong answers jump back to the relevant slide.

- **Mini-Games**

  - Each node includes at least one mini-game (crossword, word search, or match).

- **Car Upgrade Screen**

  - Show short animation when parts upgrade (using frame/tire assets).
  - Show 1–2 lines of text explaining benefit.

- **Tutorial**

  - Short intro on first launch explaining map, nodes, and upgrades.

- **Progression**
  - Unlock nodes in order.
  - Save state with PlayerPrefs.

---

## 3. Requirements for Implementation

- Keep code simple, clear, and modular.
- Avoid extra abstractions or frameworks.
- Use built-in Unity features (UI, Animator, simple lerps).
- Quizzes and node data should be **easy to edit** without touching code.
- Ensure smooth transitions and micro-animations (popups, dropdowns, car movement).

---

## 4. Acceptance Criteria

- [ ] Map shows 6 nodes with correct state colors.
- [ ] Car spawns off-screen and moves along spline to active node.
- [ ] Clicking active node opens popup with slides, dropdowns, and hotspots.
- [ ] Quiz loads from file (4–5 questions, 4 options). Wrong answers redirect to relevant slide.
- [ ] Completing quiz + mini-game unlocks upgrade screen and next node.
- [ ] Car upgrades visually by swapping in new frame/tire assets.
- [ ] Tutorial shows once on first launch.
- [ ] Progression persists with PlayerPrefs.

---

## 5. Unity Standards (Addendum)

To follow Unity best practices and keep consistency with Unity MCP:

- **UI vs World**

  - All menus, map, nodes, popups, quizzes, upgrade screens, and tutorial overlays belong in **UI Canvas**.
  - The car and spline remain in the **world space**.
  - When aligning the car under UI nodes, compute positions with `WorldToScreenPoint` and `ScreenToCanvas`.

- **Scene Layout**

  - Use a single main Canvas (Screen Space – Overlay or Screen Space – Camera).
  - Keep the spline and car as separate world objects.
  - Use prefabs for reusable UI (NodeView, Popups, UpgradeScreen).

- **UI Standards**

  - Use TextMeshPro for all text.
  - Anchor elements properly for scaling across resolutions.
  - Keep animations lightweight: fade, scale, slide (200–300ms).
  - Ensure keyboard/controller navigation works for accessibility.

- **Car Assembly**

  - Car visuals = frame + two tire sprites combined.
  - Upgrades swap these parts directly (via `SpriteRenderer`).
  - Play a short animation (fade/scale) when applying an upgrade.

- **Coding Guidelines**
  - Scripts should be small and focused (≤200 lines).
  - Prefer Unity events or delegates over polling.
  - Store data externally (JSON/TextAsset for quizzes) or ScriptableObjects for static content.
  - Save/load state via a single `ProgressionController`.

---

## 6. Progression System

- Centralized **Progression Service** manages node states (inactive, active, completed).
- Exposes events (`OnNodeStateChanged`, `OnActiveNodeChanged`) so UI, popups, and car movement stay in sync.
- For MVP, saves via **PlayerPrefs**; later can be swapped with database/cloud sync.
- Each node tracks: quiz passed, minigames done, completion state.

---

## 7. Node Interaction

- Locked nodes can be clicked, but should show that they are locked and need to be unlocked.
- Completed nodes remain visible and when clicked on, should show the popup for that node.
- Node click → triggers popup/quiz/mini-games flow.

---

## 8. Data Format

- **Quizzes**: stored in a JSON/TextAsset structure (question, options, correct answer, reference slide).
- **Node info** (text for upgrades, car parts, etc.) can also be stored as simple JSON or ScriptableObjects.
- Goal: designers can add/edit content without touching scripts.

---

## 9. Car Visuals

- Car is built from **frame sprite + two tire sprites**.
- Upgrades swap out sprites and optionally play a short animation (scale/fade).
- Tire sprite is reused for both front & rear tires.

---

## 10. Tutorial

- Plays only once on first launch (flag in PlayerPrefs).
- Shows map basics, node states, and how to progress.
- Skippable after first view.
