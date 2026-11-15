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
  echo "กรุณาวางโฟลเดอร์ mods/ThaiLang ไว้ในโฟลเดอร์เดียวกับ Tavern Master.app แล้วลองใหม่อีกครั้ง"
  exit 1
fi

if pgrep -x "Tavern Master" >/dev/null 2>&1; then
  echo "[ERROR] กรุณาปิดเกม Tavern Master ก่อนติดตั้งม็อด"
  exit 1
fi

GAME_DIR="$(dirname "$APP_DIR")"
TARGET_MOD_DIR="$GAME_DIR/mods/ThaiLang"
if [ "$MOD_DIR" != "$TARGET_MOD_DIR" ]; then
  mkdir -p "$TARGET_MOD_DIR"
  cp -R "$MOD_DIR"/* "$TARGET_MOD_DIR"/
  MOD_DIR="$TARGET_MOD_DIR"
fi

DATA_DIR="$APP_DIR/Contents/Resources/Data"
BACKUP_DIR="$MOD_DIR/backup"
mkdir -p "$BACKUP_DIR"

for f in ScriptingAssemblies.json RuntimeInitializeOnLoads.json resources.assets; do
  if [ ! -f "$BACKUP_DIR/$f" ] && [ -f "$DATA_DIR/$f" ]; then
    cp "$DATA_DIR/$f" "$BACKUP_DIR/$f"
  fi
done

cp "$MOD_DIR/ThaiMod.dll" "$DATA_DIR/Managed/ThaiMod.dll"

if [ -f "$MOD_DIR/resources.assets.gz" ]; then
  gzip -dc "$MOD_DIR/resources.assets.gz" > "$DATA_DIR/resources.assets"
fi

python3 - "$BACKUP_DIR" "$DATA_DIR" <<'PY'
import json, os, sys
bak_dir, data_dir = sys.argv[1], sys.argv[2]

sa_path = os.path.join(data_dir, "ScriptingAssemblies.json")
with open(os.path.join(bak_dir, "ScriptingAssemblies.json"), "r", encoding="utf-8") as f:
    sa = json.load(f)
if "ThaiMod.dll" not in sa["names"]:
    sa["names"].append("ThaiMod.dll")
    sa["types"].append(16)
with open(sa_path, "w", encoding="utf-8") as f:
    json.dump(sa, f, separators=(",", ":"))

ri_path = os.path.join(data_dir, "RuntimeInitializeOnLoads.json")
with open(os.path.join(bak_dir, "RuntimeInitializeOnLoads.json"), "r", encoding="utf-8") as f:
    ri = json.load(f)
ri["root"] = [r for r in ri["root"] if r.get("assemblyName") != "ThaiMod"]
ri["root"].append({
    "assemblyName": "ThaiMod",
    "nameSpace": "ThaiMod",
    "className": "Bootstrap",
    "methodName": "Init",
    "loadTypes": 4,
    "isUnityClass": False,
})
with open(ri_path, "w", encoding="utf-8") as f:
    json.dump(ri, f, separators=(",", ":"))
PY

echo "[OK] ติดตั้งม็อดภาษาไทย Tavern Master (macOS) สำเร็จ!"
echo "เปิดเกมแล้วไปที่ Settings > Language > เลือก 'ภาษาไทย (Thai)' ได้เลยครับ"
