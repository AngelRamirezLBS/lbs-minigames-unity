---
name: github-pr
description: Crear Pull Requests de este proyecto (lbs-minigames) en español con título conventional commit, resumen, archivos cambiados y pasos de prueba. Use cuando se prepare un PR, se abra un PR, o se genere la descripción del PR.
metadata:
  platforms: 'cross-platform'
  languages: 'spanish'
  category: 'git'
---

# Creación de Pull Requests — lbs-minigames

Skill específica de este proyecto Unity. Genera PRs **en español** (título y descripción) hacia la rama base `main`, siguiendo el flujo `feature → integration → main`. No sube workflows ni validaciones automáticas por ahora: la skill solo redacta y abre el PR con `gh`.

## 1. Preparación

- **gh CLI**: Verifica que `gh` esté instalado y autenticado (`gh auth status`). Si no está, presenta título/descripción al usuario y pregunta cómo proceder (no uses un comando sustituto).
- **Rama base**: Por defecto `main`. Si el trabajo se está integrando, se apunta a la rama de integración (`integration/games`) según el flujo activo.
- **Commits**: Confirma que los cambios están commiteados en la rama de feature (`git status` limpio salvo `.atl/` y artefactos fuera de `Assets/`).
- **Estado git**: Revisa `git log origin/main..HEAD --oneline` para derivar el título y el alcance del PR.

## 2. Flujo de integración

- Este proyecto desarrolla en ramas `feature/<desc>` y acumula juego en la rama de integración (`integration/games`).
- El PR debe describir si el cambio es: una feature de juego nueva, un ajuste del hub, o una integración desde `integration/games`.
- La base del PR es `main` salvo que estés consolidando la rama de integración (entonces la base es `integration/games`).

## 3. Título del PR (conventional commit, en español)

- Formato: `<type>(<scope>): <descripción>`.
- El `type` sigue conventional commits: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`.
- `scope` habitual en este repo: `hub`, `games`, `catalog`, `integration`, `audio`, `ui`, `input`.
- Ejemplos:
  - `feat(hub): integrar secuencia de lógica extendida`
  - `fix(games): corregir tap fantasma al volver al hub`
  - `feat(games): añadir Cube Platform a la secuencia`
- Deriva el título de los commit messages de la rama (`git log origin/main..HEAD --oneline`).

## 4. Cuerpo del PR (en español)

Estructura obligatoria:

### Resumen
- 1–3 bullets explicando **qué** y **por qué** en lenguaje simple. En español.

### Archivos cambiados
- Tabla de archivos y qué cambiaron:
  ```markdown
  | Archivo | Cambio |
  |---------|--------|
  | `Assets/App/Lobby/LobbyController.cs` | Alineó margen izquierdo al logo del header |
  ```

### Pasos de prueba (cómo verificarlo)
- Pasos manuales concretos para probar el cambio en Unity, por ejemplo:
  - Abrir el proyecto en Unity.
  - Correr `Tools → LBS Mini Games → Install First Vertical Slice` si cambió el catálogo/escenas.
  - Reproducir el flujo específico que se toca (entrar a un juego, completar, volver al hub).

### Notas (opcional)
- Notas de assets/LFS: si se agregaron binarios (audio/PNG), indicar que están en Git LFS.
- Referencias a issues si existieran (`Closes #N`).

## 5. Ejecución

- Si `gh` está disponible:
  ```bash
  gh pr create --title "<título>" --body-file <archivo_body>
  ```
  o directamente con `--body "<descripción>"`.
- Usar `--base main` a menos que la base sea otra (integración).
- Si `gh` NO está disponible, presenta el título y la descripción generados al usuario y detente.

## 6. Reglas de este proyecto

- **Idioma**: título y descripción SIEMPRE en español (los commits siguen conventional commits; el PR es human-facing en español).
- **No inventar workflows**: este repo no sube CI/labels automáticos todavía; no agregar secciones de "checks".
- **No commits de metadatos**: no incluir `.atl/` ni cargas fuera de `Assets/` en el alcance del PR. Mencionarlo solo como nota si es relevante.
- **Convención de commits**: el mensaje de commit usa `type(scope): descripción` (inglés/estilo del repo). El PR deriva de esos commits pero el cuerpo va en español.
