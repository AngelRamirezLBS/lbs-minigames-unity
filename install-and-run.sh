#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# install-and-run.sh — Install an already-built APK and optionally launch it
#
# Assumes the APK already exists (from Unity's Build, or build-and-run.sh) and
# just installs it on a connected Android device, then optionally launches it.
# Picks the newest APK found in the project root or build/outputs/.
#
# Usage:
#   ./install-and-run.sh                  # install + launch (single device)
#   ./install-and-run.sh --device <id>    # target a specific device serial
#   ./install-and-run.sh --no-run         # install only, do not launch
#   ./install-and-run.sh --apk <path>     # use a specific APK file
#   ./install-and-run.sh --force-uninstall# uninstall first (fixes INCOMPATIBLE)
#   ./install-and-run.sh --dry-run        # show what would be done, do nothing
# =============================================================================

# --- Config ------------------------------------------------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${PROJECT_PATH:-$SCRIPT_DIR}"

EDITOR_VERSION="$(grep -m1 'm_EditorVersion:' "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | awk '{print $2}')"
ADB="/Applications/Unity/Hub/Editor/${EDITOR_VERSION}/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
PACKAGE_NAME="com.DefaultCompany.lbsminigames"

DO_RUN=1
FORCE_UNINSTALL=0
SPECIFIC_DEVICE=""
DRY_RUN=0
APK_PATH=""

# --- Help --------------------------------------------------------------------
usage() {
  sed -n '2,17p' "$0" | sed 's/^# \{0,1\}//'
  exit 0
}

# --- Parse args --------------------------------------------------------------
while [[ $# -gt 0 ]]; do
  case "$1" in
    --device)    SPECIFIC_DEVICE="${2:?--device requires a value}"; shift 2 ;;
    --no-run)    DO_RUN=0; shift ;;
    --apk)       APK_PATH="${2:?--apk requires a value}"; shift 2 ;;
    --force-uninstall) FORCE_UNINSTALL=1; shift ;;
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
  if [[ ! -x "$ADB" ]]; then
    red "adb not found at: $ADB"
    red "Android Player (SDK/NDK/OpenJDK) must be installed for this editor."
    exit 1
  fi
  green "adb: $ADB"
}

# --- APK resolution ----------------------------------------------------------
# Pick the newest APK among the known locations (project root + build/outputs),
# unless the user passed --apk explicitly.
resolve_apk() {
  local candidates=()
  if [[ -n "$APK_PATH" ]]; then
    candidates+=("$APK_PATH")
  else
    candidates+=("$PROJECT_PATH/lbs-minigames-android.apk")
    candidates+=("$PROJECT_PATH/build/outputs/lbs-minigames-android.apk")
  fi

  local newest="" newest_time=-1 mt
  for candidate in "${candidates[@]}"; do
    if [[ -f "$candidate" ]]; then
      mt="$(stat -f %m "$candidate" 2>/dev/null || echo 0)"
      if [[ "$mt" -gt "$newest_time" ]]; then
        newest="$candidate"
        newest_time="$mt"
      fi
    fi
  done

  if [[ -z "$newest" ]]; then
    red "No APK found. Build it first (Unity or ./build-and-run.sh), or pass --apk <path>."
    exit 1
  fi

  APK_PATH="$newest"
  green "Using APK: $APK_PATH ($(du -h "$APK_PATH" | cut -f1))"
}

# --- Device handling ---------------------------------------------------------
list_available_devices() {
  "$ADB" devices | awk 'NR>1 && $2=="device" {print $1}'
}

select_device() {
  local all_authorized unauthorized
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
  if "${ADB_CMD[@]}" install -r "$APK_PATH"; then
    green "Install OK"
  else
    yellow "Install failed. If the error is INSTALL_FAILED_UPDATE_INCOMPATIBLE,"
    yellow "the device has an old build with a different signature. Retry with:"
    yellow "  ./install-and-run.sh --force-uninstall"
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
  resolve_apk

  if [[ "$DRY_RUN" -eq 1 ]]; then
    step "Dry run — showing plan only"
    echo "  APK:           $APK_PATH"
    echo "  Package:       $PACKAGE_NAME"
    echo "  Launch:        $([ "$DO_RUN" -eq 1 ] && echo yes || echo no)"
    echo "  Force uninst : $([ "$FORCE_UNINSTALL" -eq 1 ] && echo yes || echo no)"
    echo "  Target dev:    ${SPECIFIC_DEVICE:-auto}"
    exit 0
  fi

  select_device
  install_apk

  if [[ "$DO_RUN" -eq 1 ]]; then
    launch_app
  fi

  step "Done"
}

main "$@"
