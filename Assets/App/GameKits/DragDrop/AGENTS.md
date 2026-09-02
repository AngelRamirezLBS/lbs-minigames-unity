# DragDrop Kit Usage

Reusable mechanic kit for drag-drop families. Games remain independent under `Games/<GameName>`; kit provides pure contracts and reusable components.

## Quick path
1. Author `DragDropLevelDefinition` SO (immutable per-level content, optional difficulty tuning).
2. Implement `IDragDropRule` (pure `Evaluate(tokenId, overTarget)`) or use `DragDropLevelState(correctAnswerId)`.
3. Place `DragDropCard` on each token (CanvasGroup required) and `ProximityHighlighter` on target frame.
4. Drive animations via `CardAnimator.PunchPlace` / `ShakeBoard`.

## Details
| Topic | Decision |
|-------|----------|
| State | `DragDropLevelState` is pure (Idle→Dragging→Resolving/Celebrating→Final). No `ScriptableObject` mutation; owned by game MonoBehaviour. Use `IDragDropRule` for outcome, not hardcoded strings in component. |
| Pointer policy | `DragDropCard` = one-pointer default (`activePointer == int.MinValue` guard, fair same-frame resolution via `pointerId`). Symmetric cleanup in `OnDisable`/`OnDestroy` and `Restore()`. Global single-drag enforced by game (`activePointer` in `ShapeAnalogyGame`) — kit allows per-card ownership, game decides global. |
| Cleanup | Always `HideImmediate()` highlighter on `End`, `Restore()` cards on incorrect/Outside, `Accept()` on correct. Card `ResetCard()` on `Cleanup`. Unsubscribe events in `OnDestroy`. |
| No duplication | Do not duplicate orange outline interpolation — use `ProximityHighlighter.ShowForDistance` (maxDist 350, α 0.35→1, outline 8→3, scale 1→1.02, threshold 0.02). Do not duplicate `PunchPlace` (0.22s, scale 1→1.08 parabola, y+10→-4→0 easeOutBack) or `ShakeBoard` (0.48s, 18px). |
| Animation | Keep stars/serpentinas behavior exactly: 5 large serpentinas burst, rotation ±35° subtle / ±25° confetti, stars rise+0.2s pause+slow fall. Kit does not own celebration — that stays per-game. |
| Definition | `DragDropLevelDefinition` is immutable SO; runtime clones if needed. Validate `IsValid()`; list `Cards` with `tokenId/sprite/center/size/isCorrect`. |
| Compatibility | `Games/Common/DragDropToken`/`DropTarget` remain for `Classification` — do not break. New games use `GameKits/DragDrop/Runtime/DragDropCard`. Shim via namespace alias acceptable; avoid two diverging full implementations. |

## Extension points
- New rule: implement `IDragDropRule` and inject into `DragDropLevelState`.
- New layout: add `DragDropLevelDefinition` variant and feed `CreateDraggable` positions from definition (future difficulty tuning).

## Checklist
- [ ] Outcome via `IDragDropRule`, not inline string compare in component
- [ ] One-pointer guard + symmetric `OnDisable`/`OnDestroy` cleanup
- [ ] `ProximityHighlighter` + `CardAnimator` reused, not duplicated
- [ ] No `ScriptableObject` mutated as session state
- [ ] EditMode test for rule/state and proximity mapping
