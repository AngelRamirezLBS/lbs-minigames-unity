# App Architecture Boundaries

Global rules for `Assets/App` — executable templates and tests are authoritative over prose.

## Quick path
1. Pick layer: `Shared` (app-wide), `GameKits/<Mechanic>` (reusable mechanic family), `Games/<GameName>` (concrete game).
2. Inject dependencies via `ApplicationBootstrap` → `AppServices` (no singletons/service locator).
3. Author immutable config as `ScriptableObject` (catalog, audio, level definitions) — never mutate assets at runtime.
4. Verify with EditMode tests + batch compile; open representative scenes and check domain reload.

## Details
| Topic | Decision |
|-------|----------|
| Shared vs GameKits vs Games | `Shared` = survives across scenes (audio, level chrome, navigation, UI factories). `GameKits/<Mechanic>` = reusable pure rule/state/components for a mechanic family (e.g., `DragDrop`). `Games/<GameName>` = independent concrete game (scene, art, layout, rule derivation, celebration). Never create `Games/Logic` or cross-game dependencies (e.g., ShapeAnalogy ❌ depends on DragDrop is allowed via Kit, but Classification ❌ NumberPull direct). |
| Audio ownership | `ApplicationBootstrap` owns `AppAudioService` (DontDestroyOnLoad, 3 sources: Music/Voice/SFX). Music starts once, same clip does not restart, survives scene loads. Voice interrupts + ducks music. SFX uses PlayOneShot. Pause/focus and idempotent `StopAll` required. Games inject via `AppServices.Audio`; ShapeAnalogy local music removed. |
| Launch request / difficulty | `DifficultyDefinition` (ID, name, order, icon) + `GameDefinition` (supported list, default). `GameLaunchRequest` (Game + Difficulty, difficulty may be null only for legacy fallback). `GameLauncher.Launch(Request)` and `Launch(GameDefinition)` fallback to default. `GameSession.CurrentRequest` + `SelectedGame` compat. `MiniGameResult.DifficultyId` optional. Lobby auto-launches default via `LobbyLaunchModel`. |
| Immutable SOs | All `ScriptableObject` assets are authoring data only. Clone explicitly if runtime mutation needed. Validate via `IsValid()`; installer uses `Configure(...)` without destructive asset deletion. |
| No singletons | No static `GameManager`, no `FindObjectOfType` service locator. Composition at bootstrap/scene boundary, injected callbacks. MonoBehaviours = lifecycle/adaptor only; logic in plain C#. |
| Layout constants | `LevelChromeLayout` centralizes `1920x1080` reference and approved coordinates (Exit `145,150` `170x170`, Hong `145,930` `220x220`). No duplicate literals in games. |
| Kit extraction | Create `GameKits/<Mechanic>` when second game needs same mechanic. Extract pure `IDragDropRule`, `DragDropLevelState`, `ProximityHighlighter`, `CardAnimator`, `DragDropLevelDefinition` before game-specific coupling. |

## Checklist
- [ ] New code respects layer ownership (no cross-game import)
- [ ] `ScriptableObject` not mutated at runtime (check `Configure` vs `SetDirty`)
- [ ] `ApplicationBootstrap` owns persistent services, `IAppScene.Configure` injects without singleton
- [ ] Audio: music not restarted for same clip, voice ducks, SFX one-shot
- [ ] `GameDefinition` old assets still launch via fallback (empty difficulty list → valid)
- [ ] Tests added for new contracts

## Next step
See `docs/game-architecture.md` for “add a game / extract a kit” steps and `GameKits/DragDrop/AGENTS.md` for kit rules.
