# LBS Mini Games

A Unity project for a modular collection of educational mini-games. The current vertical slice is **Animal Classification**: open the lobby, choose **Animals**, launch **Animal Classification**, drag the dolphin to **Mammal**, and return to the lobby to see the last result.

## Prerequisites

| Need | Version / purpose |
| --- | --- |
| Unity Hub | Install and open the project. |
| Unity Editor | **6000.5.9f1** (the project-pinned version). |
| Git and Git LFS | Clone the repository and retrieve LFS-tracked media. |
| Android Build Support, OpenJDK, Android SDK and NDK | Required only to run on an Android tablet. Install them with Unity 6000.5.9f1 through Unity Hub. |
| iOS Build Support and Xcode | Required only for iOS work. |

The package manifest uses Unity's built-in UGUI package (`com.unity.ugui` 2.5.0), Multiplayer Center (1.0.1), and Unity modules. Unity resolves these when the project opens.

## Clone and open

```bash
git clone git@github.com:AngelRamirezLBS/lbs-minigames-unity.git
cd lbs-minigames-unity
git lfs install
git lfs pull
```

In Unity Hub, select **Add** and choose the cloned project directory. Open it with **Unity 6000.5.9f1** and allow package import and asset refresh to finish.

## Run in the Unity Editor

1. Open `Assets/App/Bootstrap/Bootstrap.unity`.
2. Enter Play mode.
3. The persistent bootstrap creates the application services and loads `Lobby`.
4. In the lobby, select **Animals**, then **Animal Classification**.
5. Drag the **DOLPHIN** token onto **Mammal**. The finish button becomes available; use it to return to the lobby and confirm the displayed last result.

`Bootstrap`, `Lobby`, and `Classification` are enabled in Build Settings, in that order. Start the application from `Bootstrap`; opening a downstream scene directly bypasses bootstrap service configuration.

## Test on an Android tablet

1. On the tablet, enable Developer options and **USB debugging**. Connect it by USB and authorize the computer if prompted.
2. In Unity, open **File > Build Profiles**. Create or select an **Android** profile and switch to it if Unity asks.
3. Confirm the tablet appears in **Run Device** and select it.
4. Choose **Build And Run**. Unity builds, installs, and launches the app on the selected tablet.
5. Follow the editor flow above and verify both landscape directions.

The application is configured for auto-rotation with **Landscape Left** and **Landscape Right** enabled; both portrait orientations are disabled. UI canvases use a 1920×1080 reference resolution.

## Project layout and modularity

| Path | Responsibility |
| --- | --- |
| `Assets/App/Bootstrap` | Persistent application bootstrap and editor-only vertical-slice installer. |
| `Assets/App/Lobby` | Catalog-driven lobby scene and controller. |
| `Assets/App/Catalog` | `GameCatalog`, categories, game definitions, and the catalog data assets. |
| `Assets/App/Navigation` | Session state, scene loading, and game launch/completion flow. |
| `Assets/App/Shared` | Cross-game contracts, result model, and shared UI helpers. |
| `Assets/App/Games/Common` | Reusable game interaction mechanics. |
| `Assets/App/Games/Classification` | The Animal Classification scene and game-specific behavior. |
| `Packages` / `ProjectSettings` | Unity package lockfiles and project-wide settings. |

Keep a mini-game's scene, rules, and presentation inside its own `Assets/App/Games/<GameName>` folder. Put reusable mechanics in `Games/Common` and app-wide contracts or helpers in `Shared`; do not make the bootstrap or lobby depend on a specific game's implementation.

## Git and Unity collaboration

- Keep **Asset Serialization** set to **Force Text** and **Version Control** set to **Visible Meta Files**. These are already configured in the project.
- Commit `.meta` files together with every added, moved, or deleted Unity asset. Do not regenerate or omit them.
- Do not commit Unity-generated working directories such as `Library/` or `Temp/`; they are ignored along with build output and editor caches.
- Unity YAML assets (`.unity`, `.prefab`, `.asset`, `.mat`, and `.meta`) are text files with UnityYAML merge configuration. Keep them in normal Git history.
- This repository's LFS policy is binary source media only: `.psd`, `.psb`, `.fbx`, `.blend`, audio (`.wav`, `.aiff`, `.mp3`, `.ogg`), and video (`.mp4`, `.mov`, `.webm`). Do not use LFS for Unity YAML files.

## Add a mini-game

1. Create the game scene and game-specific code under `Assets/App/Games/<GameName>`.
2. Implement the existing game contracts as appropriate: the scene component needs `IAppScene` so the bootstrap can configure it after loading, and a playable mini-game should expose `IMiniGame` and report its result through `GameLauncher`.
3. Add the scene to Build Settings, preserving `Bootstrap` as the entry scene.
4. Create a `GameDefinition` asset with a unique ID, display information, category, and exact scene name. Add it to `MiniGameCatalog`; create and register a `GameCategory` only when needed.
5. Run the complete Bootstrap → Lobby → game → Lobby flow in the editor and on the target device.

## Prototype scope

This repository currently contains one category (**Animals**) and one playable game (**Animal Classification**) with one dolphin classification interaction. The last result is held only for the running session. There are no committed automated test assets or test suites, persistent progression, account/service integration, or production content pipeline in the current vertical slice.
