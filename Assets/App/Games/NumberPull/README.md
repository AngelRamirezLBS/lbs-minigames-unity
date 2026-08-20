# Number Pull MVP

`Number Pull` is a local two-player arithmetic game registered as `math.number-pull`.

- Runtime state is owned by `NumberPullGame`; the rules and seeded problem generator live in the Unity-independent `Domain` assembly.
- Gameplay controls use feature-local `Input.touches` polling. Contacts are owned by `fingerId` from `Began` through `Ended`/`Canceled`; gameplay does not also register uGUI button callbacks.
- Audio cues are intentionally limited runtime synthesis placeholders. Replace them with approved original production clips through the feature-owned audio adapter when final audio is available.
- Mouse input is an editor/standalone convenience only and is not multitouch evidence.
- Physical multitouch, acoustic quality, device performance, and safe-area behavior require target-device validation.
