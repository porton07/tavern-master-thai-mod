# ม็อดภาษาไทย Tavern Master (Thai Language Mod)

ม็อดภาษาไทยสำหรับเกม **Tavern Master** โดยเพิ่มตัวเลือก **`ภาษาไทย (Thai)`** แยกอิสระในเมนู Settings ไม่ทับภาษาอังกฤษหรือภาษาอื่น และฝังฟอนต์ **LINE Seed Sans TH Game Bold (v1.003)** โดยไม่ต้องติดตั้งฟอนต์ลง Windows

## จุดเด่น
- **เลือกภาษาได้อิสระ:** เพิ่มตัวเลือก `ภาษาไทย (Thai)` ใน Settings -> Language สามารถสลับไป-กลับระหว่างภาษาไทยกับภาษาอื่นได้ตลอด
- **ไม่แก้ไฟล์ DLL/Asset หลักของเกม:** ตัวม็อดอยู่ใน `mods/ThaiLang/` และโหลดผ่าน `ScriptingAssemblies.json` + `RuntimeInitializeOnLoads.json`
- **แปลครบ 1,213 คีย์:** แยกไฟล์ภาษาไว้ทั้ง `translations.json` (ที่เกมอ่าน) และ `translations.csv` (สำหรับเปิดแก้ใน Excel/Google Sheets)

## วิธีติดตั้ง (สำหรับผู้เล่นทั่วไป)

### Windows
1. ปิดเกม **Tavern Master** ให้สนิท
2. นำโฟลเดอร์ `mods` ไปวางในโฟลเดอร์เกม (เช่น `.../steamapps/common/Tavern Master/mods/ThaiLang/`)
3. ดับเบิลคลิก **`Install.cmd`**
4. เปิดเกมตามปกติ -> เข้า **Settings** -> **Language** -> เลือก **`ภาษาไทย (Thai)`**

### macOS (Intel / Apple Silicon)
1. ปิดเกม **Tavern Master** ให้สนิท
2. นำโฟลเดอร์ `mods` ไปวางในโฟลเดอร์เดียวกับ `Tavern Master.app`
3. เปิด Terminal แล้วรัน `bash mods/ThaiLang/install_macos.sh`
4. เปิดเกม -> เข้า **Settings** -> **Language** -> เลือก **`ภาษาไทย (Thai)`**

## วิธีถอนการติดตั้ง
- **Windows:** ปิดเกมแล้วดับเบิลคลิก **`Uninstall.cmd`**
- **macOS:** ปิดเกมแล้วรัน `bash mods/ThaiLang/uninstall_macos.sh`

*(ระบบจะคืนค่า `ScriptingAssemblies.json`, `RuntimeInitializeOnLoads.json` และ `resources.assets` จาก `backup/` และลบ `ThaiMod.dll` ออกจาก `Managed/`)*

## ไฟล์สำคัญใน `mods/ThaiLang/`
- `ThaiMod.dll`: ตัวโหลดคำแปลและสร้างฟอนต์ SDF ภาษาไทยตอนเปิดเกม
- `LINESeedSansTH_G_Bd.ttf`: ฟอนต์ LINE Seed Sans TH Game Bold v1.003
- `translations.json`: ไฟล์คำแปลภาษาไทยที่เกมใช้งาน (1,213 คีย์)
- `translations.csv`: ไฟล์ตาราง `key, en, th` สำหรับตรวจสอบหรือแก้ไขคำแปลเพิ่มเติม
- `en_reference.json`: ข้อความต้นฉบับภาษาอังกฤษอ้างอิง
- `src/ThaiMod.cs`: ซอร์สโค้ด C# ของม็อด

## เครดิต (Credits)
- **Mod & Thai Translation:** `porton007`
- **Font:** LINE Seed Sans TH Game Bold (v1.003) by LY Corporation (SIL Open Font License 1.1)
