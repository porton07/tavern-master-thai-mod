# Tavern Master - Thai Language Mod (ม็อดภาษาไทย)

![Tavern Master Thai Language Mod](screenshots/nexus-cover-1920x1080.png)

ม็อดภาษาไทยสำหรับเกม **Tavern Master** แปลครบ **1,213 คีย์** พร้อมฟอนต์ **LINE Seed Sans TH Game Bold (v1.003)** ในตัว ไม่ต้องติดตั้งฟอนต์ลง Windows และสามารถเลือกภาษาไทยแยกอิสระได้ในหน้า Settings

Thai Language Mod for **Tavern Master** (1,213 localization keys) with embedded **LINE Seed Sans TH Game Bold (v1.003)** font and an independent language option in Settings.

---

## จุดเด่น (Features)

- **เลือกภาษาไทยได้อิสระ (Independent Language Selector):** เพิ่มตัวเลือก **`ภาษาไทย (Thai)`** เข้าไปใน **Settings > Language** ไม่ทับภาษาอังกฤษหรือภาษาอื่น สลับภาษาไป-กลับได้ตลอดเวลา
- **ฟอนต์ภาษาไทยคมชัดในตัว (Embedded Thai Font):** ใช้ฟอนต์ LINE Seed Sans TH Game Bold (SIL OFL 1.1) รองรับทั้ง **TextMeshPro (SDF)** และ **Unity UI Text** โดยไม่ต้องติดตั้งฟอนต์ลงระบบ
- **แปลครบทุกหมวด (Full UI & Gameplay Localization):** แปลครบ 1,213 คีย์ พร้อมไฟล์ [translations.json](translations.json) และ [translations.csv](translations.csv) สำหรับตรวจสอบหรือปรับแก้คำแปลเพิ่มเติม
- **ติดตั้งและถอนการติดตั้งง่าย (Easy Install / Uninstall):** มีตัวติดตั้งอัตโนมัติพร้อมสำรองไฟล์ต้นฉบับไว้ใน `backup/` และคืนค่าเดิมได้ทุกเมื่อ
- **รองรับ Cross-Platform:** ใช้งานได้บน **Windows (x64)** และมีสคริปต์รองรับ **macOS (Intel x64 / Apple Silicon ARM64)**

---

## วิธีติดตั้ง (Installation)

> แนะนำให้ดาวน์โหลดแพ็กเกจสำเร็จรูป `TavernMaster-ThaiLang-v1.0.0.zip` จากหน้า **Releases** (หรือ **Nexus Mods**)

### Windows
1. ปิดเกม **Tavern Master** ให้สนิท
2. แตกไฟล์ ZIP ลงในโฟลเดอร์เกม (เช่น `.../steamapps/common/Tavern Master/`) จะได้โฟลเดอร์ `mods/ThaiLang/`
3. ดับเบิลคลิก **`mods/ThaiLang/Install.cmd`**
4. เปิดเกมตามปกติ -> เข้า **Settings** -> **Language** -> เลือก **`ภาษาไทย (Thai)`**

### macOS (Intel / Apple Silicon)
1. ปิดเกม **Tavern Master** ให้สนิท
2. แตกไฟล์ ZIP ลงในโฟลเดอร์เดียวกับ `Tavern Master.app`
3. เปิด Terminal แล้วรัน:
   ```bash
   bash mods/ThaiLang/install_macos.sh
   ```
4. เปิดเกม -> เข้า **Settings** -> **Language** -> เลือก **`ภาษาไทย (Thai)`**

---

## วิธีถอนการติดตั้ง (Uninstallation)

- **Windows:** ปิดเกม แล้วดับเบิลคลิก **`mods/ThaiLang/Uninstall.cmd`**
- **macOS:** ปิดเกม แล้วรัน:
  ```bash
  bash mods/ThaiLang/uninstall_macos.sh
  ```

ระบบจะคืนค่า `ScriptingAssemblies.json`, `RuntimeInitializeOnLoads.json` และ `resources.assets` จาก `backup/` พร้อมลบ `ThaiMod.dll` ออกจาก `Managed/` ให้โดยอัตโนมัติ

---

## โครงสร้างไฟล์ (Repository Structure)

| ไฟล์ / โฟลเดอร์ | หน้าที่ |
| --- | --- |
| `src/ThaiMod.cs` | ซอร์สโค้ด C# ของม็อด (Runtime hook, Font loader, Language dropdown) |
| `ThaiMod.dll` | ไฟล์ DLL ที่คอมไพล์แล้ว |
| `build.cmd` | สคริปต์คอมไพล์ `ThaiMod.dll` บน Windows ด้วย `csc.exe` |
| `translations.json` | ไฟล์คำแปลภาษาไทยที่ม็อดโหลดใช้งานจริง (1,213 คีย์) |
| `translations.csv` | ไฟล์ตาราง `key, en, th` (UTF-8 BOM) สำหรับเปิดแก้ใน Excel / Google Sheets |
| `en_reference.json` | ข้อความต้นฉบับภาษาอังกฤษอ้างอิง |
| `LINESeedSansTH_G_Bd.ttf` | ฟอนต์ LINE Seed Sans TH Game Bold v1.003 |
| `Install.cmd` / `Uninstall.cmd` / `Manage-Mod.ps1` | ตัวติดตั้งและถอนการติดตั้งสำหรับ Windows |
| `install_macos.sh` / `uninstall_macos.sh` | ตัวติดตั้งและถอนการติดตั้งสำหรับ macOS |
| `licenses/` | ข้อมูลลิขสิทธิ์ฟอนต์ (SIL OFL 1.1) |

---

## การ Build จาก Source (Windows)

วางโฟลเดอร์นี้ไว้ที่ `<Tavern Master>/mods/ThaiLang/` แล้วรัน:

```cmd
build.cmd
```

---

## เครดิต (Credits)

- **Mod & Thai Translation:** [`porton007`](https://github.com/porton07)
- **Font:** [LINE Seed Sans TH](https://seed.line.me/index_th.html) by LY Corporation (SIL Open Font License 1.1)
