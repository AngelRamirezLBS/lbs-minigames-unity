# LBS+ Games visual identity

**Status:** Approved direction for team use.  
**Working product name:** `LBS+ Games` — provisional; it is not final product naming.

This document records the agreed visual direction for the standalone LBS+ Games minigame app. It guides future Hub/Menu and game UI work; it does not authorize UI implementation.

## Decision summary

| Topic | Approved decision |
| --- | --- |
| Product boundary | LBS+ Games is a standalone landscape minigame app. It is visually related to LBS+, but is not embedded in the portrait LBS+ app navigation. |
| Primary devices | Android tablets and 98-inch Android touch TVs. |
| Interaction | Every game requires simultaneous multitouch, large touch targets, and legibility from a long viewing distance. |
| First design priority | Design the Hub/Menu before individual games. The Hub is a direct game-selection gallery and establishes shared UI patterns. |
| Theme scope | Initial work covers the primary-school theme only. The secondary/prep yellow/cyan theme is deferred. |

## Primary-school color system

| Role | Value | Status | Use |
| --- | --- | --- | --- |
| Primary | `#9448F4` | Approved | Primary-school identity, emphasis, and primary actions. |
| Secondary | `#FFB740` | Approved | Supporting emphasis, rewards, and functional feedback. |
| Dark ink | `#241A35` | Proposed | High-contrast text and icons, including readable content on orange controls. |
| Neutral canvas | `#F7F5FA` | Proposed | Calm application background. |
| Neutral surface | `#FFFFFF` | Proposed | Raised cards and rounded surfaces. |
| Success | `#167A4A` | Proposed | Positive completion and correct-answer feedback. |
| Error | `#B3261E` | Proposed | Error and incorrect-answer feedback. |

Exact neutral, success, and error colors were not supplied; the proposed values above are starting semantic roles, not approved brand colors. Do not use small white text on orange controls. Use dark ink on orange when text or icons must remain readable.

## Typography and supplied materials

Use **Volte** as the interface typeface. The supplied identity font reference is [`fonts/Volte-Regular.otf`](../fonts/Volte-Regular.otf); do not assume that other weights are available or use them as a dependency.

Use the provided logo as a source reference, not as permission to redefine the provisional product name or create a final lockup.

## Visual language

- Use rounded surfaces, clear and simple line icons, and a hierarchy that remains readable at a distance.
- Make feedback vivid and functional: it must communicate state and outcome without overwhelming the task.
- Preserve a clear separation between primary actions, supporting emphasis, status, and feedback.
- Design for shared-screen play first: controls and feedback must work for multiple simultaneous touches.

## Game worlds and LBS+ connection

Individual game worlds may vary by subject. Space motifs and LBS+ characters may provide optional decorative connective tissue, but they must not become playable characters or a dependency for a game to work.

## Screen direction

### Hub/Menu — first

The first design focus is the Hub/Menu: a direct game-selection gallery. It should establish the reusable navigation, card, action, feedback, and readability patterns that individual games adopt later.

### Competitive screens — later

Future competitive screens use a symmetric **left / center / right** layout. The center may use replaceable, lightweight provisional art. Prefer a direct tug-of-war metaphor: opposing figures, a rope, and a central progress marker. Do not copy MathBug branding, art, or UI.

## Source materials

Use these local materials as visual references:

| Source | Purpose |
| --- | --- |
| [`logo.png`](../logo.png) | Existing LBS+ logo reference. |
| [`fonts/Volte-Regular.otf`](../fonts/Volte-Regular.otf) | Supplied Volte typeface reference. |
| [`app-lbs-screenshots/`](../app-lbs-screenshots/) | Existing LBS+ visual reference screens. |

## Deferred and out of scope

| Item | Status |
| --- | --- |
| UI implementation | Out of scope for this document. |
| Final product naming | Deferred; `LBS+ Games` remains provisional. |
| Secondary/prep yellow/cyan theme | Deferred. |
| Final character assets | Deferred. |
| Detailed player-profile and setup flows | Deferred. |
