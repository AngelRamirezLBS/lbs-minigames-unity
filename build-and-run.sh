#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# build-and-run.sh — Headless Android build + install + launch for lbs-minigames
#
# Builds an APK with the Unity Editor CLI, installs it on a connected Android
# device, and launches the app. No editor UI required.
#
# Usage:
#   ./build-and-run.sh                  # build + install + launch (single device)
#   ./build-and-run.sh --device <id>    # target a specific device serial
#   ./build-and-run.sh --no-run         # build + install only, do not launch
#   ./build-and-run.sh --no-build       # install + launch only (reuse existing APK)
#   ./build-and-run.sh --force-uninstall# uninstall first (fixes INCOMPATIBLE)
#   ./build-and-run.sh --rebuild        # force a rebuild even if APK is fresh
#   ./build-and-run.sh --dry-run        # show what would be done, do nothing
# =============================================================================

# --- Config ------------------------------------------------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${PROJECT_PATH:-$SCRIPT_DIR}"

# Resolve the Unity editor + AndroidPlayer from the project's version.
EDITOR_VERSION="$(grep -m1 'm_EditorVersion:' "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | awk '{print $2}')"
UNITY_EDITOR="/Applications/Unity/Hub/Editor/${EDITOR_VERSION}/Unity.app/Contents/MacOS/Unity"
ADB="/Applications/Unity/Hub/Editor/${EDITOR_VERSION}/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"

BUILD_METHOD="Lbs.MiniGames.Bootstrap.Editor.AndroidBuild.BuildAndroidApk"
OUTPUT_APK="$PROJECT_PATH/build/outputs/lbs-minigames-android.apk"
PACKAGE_NAME="com.DefaultCompany.lbsminigames"

DO_BUILD=1
DO_RUN=1
FORCE_UNINSTALL=0
SPECIFIC_DEVICE=""
DRY_RUN=0
FORCE_REBUILD=0

# --- Help --------------------------------------------------------------------
usage() {
  sed -n '2,16p' "$0" | sed 's/^# \{0,1\}//'
  exit 0
}

# --- Parse args --------------------------------------------------------------
while [[ $# -gt 0 ]]; do
  case "$1" in
    --device)    SPECIFIC_DEVICE="${2:?--device requires a value}"; shift 2 ;;
    --no-run)    DO_RUN=0; shift ;;
    --no-build)  DO_BUILD=0; shift ;;
    --force-uninstall) FORCE_UNINSTALL=1; shift ;;
    --rebuild)       FORCE_REBUILD=1; shift ;;
    --dry-run)   DRY_RUN=1; shift ;;
    -h|--help)   usage ;;
    *) echo "Unknown option: $1"; usage ;;
  esac
done

# --- Color helpers -----------------------------------------------------------
green() { printf '\033[0;32m%s\033[0m\n' "$*"; }
yellow() { printf '\033[0;33m%s\033[0m\n' "$*"; }
red() { printf '\033[0;31m%s\033[0m\n' "$*"; }
step() { printf '\n\033[1;36m==> %s\033[0m\n' "$*"; }

# --- Preflight ---------------------------------------------------------------
check_prereqs() {
  if [[ ! -x "$UNITY_EDITOR" ]]; then
    red "Unity editor not found at: $UNITY_EDITOR"
    red "Expected version: $EDITOR_VERSION"
    exit 1
  fi
  if [[ ! -x "$ADB" ]]; then
    red "adb not found at: $ADB"
    red "Android Player (SDK/NDK/OpenJDK) must be installed for this editor."
    exit 1
  fi
  green "Unity editor:  $UNITY_EDITOR"
  green "adb:           $ADB"
}

# --- Build -------------------------------------------------------------------
# Returns 0 (fresh) if the APK is newer than every source path we know matters.
apk_is_fresh() {
  [[ -f "$OUTPUT_APK" ]] || return 1
  local apk_mtime newest_mtime src
  apk_mtime="$(stat -f %m "$OUTPUT_APK" 2>/dev/null)"
  # Find the newest modified file under the folders that drive a build.
  newest_mtime="$(find "$PROJECT_PATH/Assets" "$PROJECT_PATH/ProjectSettings" \
    -type f -not -path '*/.meta' \
    -not -path '*/Library/*' \
    -exec stat -f %m {} \; 2>/dev/null | sort -rn | head -1)"
  [[ -n "$newest_mtime" && "$apk_mtime" -ge "$newest_mtime" ]]
}

build_apk() {
  if [[ "$FORCE_REBUILD" -ne 1 ]] && apk_is_fresh; then
    green "APK is up to date — skipping build (use --rebuild to force)."
    return 0
  fi

  step "Building APK (headless)"
  log_file="$PROJECT_PATH/build/outputs/build-and-run.log"
  rm -f "$log_file"

  "$UNITY_EDITOR" \
    -batchmode -nographics \
    -projectPath "$PROJECT_PATH" \
    -executeMethod "$BUILD_METHOD" \
    -quit -logFile "$log_file"

  if [[ ! -f "$OUTPUT_APK" ]]; then
    red "Build failed — no APK produced. See log:"
    red "  $log_file"
    exit 1
  fi
  green "APK ready: $OUTPUT_APK ($(du -h "$OUTPUT_APK" | cut -f1))"
}

# --- Device handling ---------------------------------------------------------
list_available_devices() {
  "$ADB" devices | awk 'NR>1 && $2=="device" {print $1}'
}

select_device() {
  local all_authorized unauthorized
  # Show unauthorized too so the user knows to authorize them.
  # Filter out the adb daemon startup noise (==> lines and blank lines).
  echo "Connected devices:"
  "$ADB" devices | tail -n +2 | grep -E '^[a-zA-Z0-9]' | sed 's/[[:space:]]\+/  /g'

  all_authorized="$(list_available_devices)"
  unauthorized="$("$ADB" devices | awk 'NR>1 && $2=="unauthorized" {print $1}')"

  if [[ -n "$unauthorized" ]]; then
    yellow "WARNING: device(s) are 'unauthorized': $unauthorized"
    yellow "  Accept the USB debugging dialog on the device, then re-run."
    exit 1
  fi

  if [[ -n "$SPECIFIC_DEVICE" ]]; then
    if [[ "$all_authorized" == *"$SPECIFIC_DEVICE"* ]]; then
      DEVICE_SERIAL="$SPECIFIC_DEVICE"
    else
      red "Requested device $SPECIFIC_DEVICE is not authorized/connected."
      exit 1
    fi
  elif [[ -n "$all_authorized" ]]; then
    local num
    num="$(echo "$all_authorized" | wc -l | tr -d ' ')"
    if [[ "$num" -eq 1 ]]; then
      DEVICE_SERIAL="$(echo "$all_authorized" | head -1)"
      green "Single device detected: $DEVICE_SERIAL"
    else
      echo "Multiple authorized devices found. Pick one:"
      echo "$all_authorized" | nl -w2 -s') '
      printf '%s' "Enter number: "
      read -r choice
      DEVICE_SERIAL="$(echo "$all_authorized" | sed -n "${choice}p")"
      if [[ -z "$DEVICE_SERIAL" ]]; then
        red "Invalid selection."
        exit 1
      fi
    fi
  else
    red "No authorized devices connected. Connect one and accept USB debugging."
    exit 1
  fi

  ADB_CMD=("$ADB" -s "$DEVICE_SERIAL")
  green "Using device: $DEVICE_SERIAL"
}

# --- Install -----------------------------------------------------------------
install_apk() {
  step "Installing APK on $DEVICE_SERIAL"
  if [[ "$FORCE_UNINSTALL" -eq 1 ]]; then
    echo "Force-uninstalling existing package (wipes app data)..."
    "${ADB_CMD[@]}" uninstall "$PACKAGE_NAME" >/dev/null 2>&1 || true
  fi
  if "${ADB_CMD[@]}" install -r "$OUTPUT_APK"; then
    green "Install OK"
  else
    yellow "Install failed. If the error is INSTALL_FAILED_UPDATE_INCOMPATIBLE,"
    yellow "the device has an old build with a different signature. Retry with:"
    yellow "  ./build-and-run.sh --force-uninstall"
    exit 1
  fi
}

# --- Launch ------------------------------------------------------------------
launch_app() {
  step "Launching $PACKAGE_NAME"
  "${ADB_CMD[@]}" shell monkey -p "$PACKAGE_NAME" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
  sleep 3
  local pid
  pid="$("${ADB_CMD[@]}" shell pidof "$PACKAGE_NAME" 2>/dev/null | tr -d '\r')"
  if [[ -n "$pid" ]]; then
    green "App running (pid: $pid)"
  else
    yellow "App launched but PID not detected — it may still be starting or crashed."
    "${ADB_CMD[@]}" logcat -d -t 20 | grep -iE "FATAL|AndroidRuntime|DefaultCompany" | tail -20 || true
  fi
}

# --- Main --------------------------------------------------------------------
main() {
  check_prereqs
  if [[ "$DRY_RUN" -eq 1 ]]; then
    step "Dry run — showing plan only"
    echo "  Unity editor: $UNITY_EDITOR"
    echo "  Build method: $BUILD_METHOD"
    echo "  Output APK:   $OUTPUT_APK"
    echo "  Package:      $PACKAGE_NAME"
    echo "  Build:        $([ "$DO_BUILD" -eq 1 ] && echo yes || echo no)"
    echo "  Launch:       $([ "$DO_RUN" -eq 1 ] && echo yes || echo no)"
    echo "  Force uninst: $([ "$FORCE_UNINSTALL" -eq 1 ] && echo yes || echo no)"
    echo "  Target dev:   ${SPECIFIC_DEVICE:-auto}"
    exit 0
  fi

  if [[ "$DO_BUILD" -eq 1 ]]; then
    build_apk
  fi

  select_device
  install_apk

  if [[ "$DO_RUN" -eq 1 ]]; then
    launch_app
  fi

  step "Done"
}

main "$@"
