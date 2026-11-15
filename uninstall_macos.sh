#!/usr/bin/env bash
set -euo pipefail

MOD_DIR="$(cd "$(dirname "$0")" && pwd)"
APP_DIR=""

for c in \
  "$(cd "$MOD_DIR/../.." && pwd)/Tavern Master.app" \
  "$(cd "$MOD_DIR/.." && pwd)/Tavern Master.app" \
  "$PWD/Tavern Master.app" \
  "$HOME/Library/Application Support/Steam/steamapps/common/Tavern Master/Tavern Master.app"; do
  if [ -d "$c/Contents/Resources/Data" ]; then
    APP_DIR="$c"
    break
  fi
done

if [ -z "$APP_DIR" ]; then
  echo "[ERROR] ไม่พบ Tavern Master.app"
  exit 1
fi

if pgrep -x "Tavern Master" >/dev/null 2>&1; then
  echo "[ERROR] กรุณาปิดเกม Tavern Master ก่อนถอนการติดตั้งม็อด"
  exit 1
fi

DATA_DIR="$APP_DIR/Contents/Resources/Data"
BACKUP_DIR="$MOD_DIR/backup"

for f in ScriptingAssemblies.json RuntimeInitializeOnLoads.json resources.assets; do
  if [ -f "$BACKUP_DIR/$f" ]; then
    cp "$BACKUP_DIR/$f" "$DATA_DIR/$f"
  fi
done

rm -f "$DATA_DIR/Managed/ThaiMod.dll"
echo "[OK] ถอนการติดตั้งม็อดภาษาไทย Tavern Master (macOS) สำเร็จ!"
