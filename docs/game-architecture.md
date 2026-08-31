# Game Architecture

How to add a game and when to extract a GameKit. Executable kits/tests are authoritative; this doc is a signpost.

## Add a game
1. Create `Assets/App/Games/<GameName>/` (scene, art, sounds stay here).
2. Add `GameDefinition` SO in `Catalog/Data`, `GameCategory` if new, set `Category`, `SceneName`, `SupportedDifficulties` (optional) via installer.
3. Implement `MonoBehaviour, IAppScene` under `Games/<GameName>`; `Configure(AppServices)` receives injected `GameLauncher`, `GameSession`, `IAppAudioService`.
4. Build UI procedurally via `UiFactory` + `LevelChromeFactory` (approved coordinates only via `LevelChromeLayout`).
5. Keep `ScriptableObject`s immutable; own mutable `State` in a plain C# class.
6. Wire `GameLauncher` completion via `MiniGameResult` (include `DifficultyId` if needed).

## Extract a GameKit
Extract when **second** unrelated game needs the same mechanic family.

- `Assets/App/GameKits/<Mechanic>/Core/` — pure `IDragDropRule`, `DragDropLevelState`, `DragDropLevelDefinition`.
- `Assets/App/GameKits/<Mechanic>/Runtime/` — reusable `MonoBehaviour` components (`DragDropCard`, `ProximityHighlighter`, `CardAnimator`).
- Games keep per-game layout, rule derivation, instruction/voice, celebration. Do not make a giant `BaseGame`.

Current kit: `DragDrop` (used by `ShapeAnalogy`; `Classification` stays on `Games/Common` shim). No empty speculative kit folders.

## Reuse rules
| Need | Use |
|------|-----|
| App-wide music/voice/SFX, level chrome, navigation | `Assets/App/Shared/*` |
| Reusable mechanic family | `Assets/App/GameKits/<Mechanic>` |
| Concrete game | `Assets/App/Games/<GameName>` |

No singletons, no `GameManager`, no cross-game imports. Composition at `ApplicationBootstrap`; scenes configure via `IAppScene`.

## Verification
- `Window → Analysis → Code Coverage` or `EditMode` suite for pure rules.
- Batch compile: `Unity -batchmode -quit -projectPath . -executeMethod UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFiles` (or ` -runTests -testPlatform EditMode`).
