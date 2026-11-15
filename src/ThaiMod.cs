using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace ThaiMod
{
    public static class Bootstrap
    {
        public static Font ThaiUnityFont;
        public static TMP_FontAsset ThaiTmpFont;
        public static Dictionary<string, string> ThaiDict = new Dictionary<string, string>();
        private static bool _initialized;
        private static string _modDir;
        private static string _logPath;

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int AddFontResourceExW(string lpszFilename, uint fl, IntPtr pdv);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("libc", SetLastError = true)]
        private static extern int mprotect(IntPtr addr, UIntPtr len, int prot);

        [DllImport("libc")]
        private static extern int getpagesize();

        [DllImport("libSystem.B.dylib")]
        private static extern void sys_icache_invalidate(IntPtr start, UIntPtr len);

        private const uint FR_PRIVATE = 0x10;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        public const LocalizationModel.LanguageType ThaiLang = (LocalizationModel.LanguageType)13;

        public static void Log(string msg)
        {
            try
            {
                if (!string.IsNullOrEmpty(_logPath))
                {
                    File.AppendAllText(_logPath, DateTime.Now.ToString("HH:mm:ss.fff ") + msg + Environment.NewLine);
                }
            }
            catch { }
        }

        private static string FindModDir()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
                for (int i = 0; i < 5 && dir != null; i++)
                {
                    string candidate = Path.Combine(dir.FullName, "mods", "ThaiLang");
                    if (File.Exists(Path.Combine(candidate, "translations.json")))
                        return candidate;
                    dir = dir.Parent;
                }
            }
            catch { }
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "mods", "ThaiLang");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                _modDir = FindModDir();
                _logPath = Path.Combine(_modDir, "mod.log");
                File.WriteAllText(_logPath, "ThaiMod Init starting (" + Application.platform + ", " + RuntimeInformation.ProcessArchitecture + ")..." + Environment.NewLine);

                LoadTranslations();
                LoadThaiFont();
                InstallHooks();

                SceneManager.sceneLoaded += OnSceneLoaded;
                var go = new GameObject("ThaiModRunner");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<ThaiModRunner>();
                Log("ThaiMod Init completed successfully.");
            }
            catch (Exception ex)
            {
                Log("ThaiMod Init ERROR: " + ex);
            }
        }

        public static void LoadTranslations()
        {
            string jsonPath = Path.Combine(_modDir, "translations.json");
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (dict != null)
                {
                    ThaiDict = dict;
                    Log("Loaded translations: " + ThaiDict.Count + " keys.");
                }
            }
            else
            {
                Log("translations.json not found at: " + jsonPath);
            }
        }

        private static bool _embeddedFontLoaded;

        public static void EnsureEmbeddedThaiUnityFont()
        {
            if (_embeddedFontLoaded) return;
            try
            {
                var ehs = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
                if (ehs != null && ehs.sourceFontFile != null)
                {
                    ThaiUnityFont = ehs.sourceFontFile;
                    UnityEngine.Object.DontDestroyOnLoad(ThaiUnityFont);
                    _embeddedFontLoaded = true;
                    Log("Loaded embedded ThaiUnityFont via Electronic Highway Sign SDF: " + ThaiUnityFont.name);
                    return;
                }
                foreach (var rf in Resources.FindObjectsOfTypeAll<Font>())
                {
                    if (rf != null && rf.name == "Electronic Highway Sign")
                    {
                        ThaiUnityFont = rf;
                        UnityEngine.Object.DontDestroyOnLoad(ThaiUnityFont);
                        _embeddedFontLoaded = true;
                        Log("Loaded embedded ThaiUnityFont via FindObjectsOfTypeAll: " + rf.name);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("EnsureEmbeddedThaiUnityFont failed: " + ex.Message);
            }
        }

        private static void LoadThaiFont()
        {
            EnsureEmbeddedThaiUnityFont();
            if (_embeddedFontLoaded) return;

            if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
                return;

            string fontPath = Path.Combine(_modDir, "LINESeedSansTH_G_Bd.ttf");
            if (!File.Exists(fontPath))
            {
                Log("Font file not found: " + fontPath);
                return;
            }

            int added = AddFontResourceExW(fontPath, FR_PRIVATE, IntPtr.Zero);
            Log("AddFontResourceExW result: " + added + " (err=" + Marshal.GetLastWin32Error() + ")");

            string[] candidates = new string[]
            {
                "LINE Seed Sans TH Game",
                "LINE Seed Sans TH Game Bold",
                "LINE Seed Sans TH",
                "LINESeedSansTH_G_Bd"
            };

            foreach (string name in candidates)
            {
                try
                {
                    Font f = Font.CreateDynamicFontFromOSFont(name, 32);
                    if (f != null)
                    {
                        // Verify it actually resolved to something with Thai glyph 'ก'
                        f.RequestCharactersInTexture("กขค", 32);
                        CharacterInfo ci;
                        if (f.GetCharacterInfo('ก', out ci, 32))
                        {
                            ThaiUnityFont = f;
                            UnityEngine.Object.DontDestroyOnLoad(ThaiUnityFont);
                            Log("Created ThaiUnityFont from OS name: " + name);
                            break;
                        }
                        else
                        {
                            Log("Font '" + name + "' created but missing Thai glyph 'ก'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Font candidate '" + name + "' failed: " + ex.Message);
                }
            }

            if (ThaiUnityFont != null)
            {
                Log("ThaiUnityFont ready; will create ThaiTmpFont on first scene load.");
            }
        }

        public static void EnsureThaiTmpFont()
        {
            if (ThaiTmpFont != null) return;
            try
            {
                string fontPath = Path.Combine(_modDir, "LINESeedSansTH_G_Bd.ttf");
                if (!File.Exists(fontPath)) return;

                FontEngine.InitializeFontEngine();
                var err = FontEngine.LoadFontFace(fontPath, 64);
                if (err != FontEngineError.Success)
                {
                    Log("FontEngine.LoadFontFace failed: " + err);
                    return;
                }

                FaceInfo faceInfo = FontEngine.GetFaceInfo();
                var unicodeToGlyph = new Dictionary<uint, uint>();
                var glyphIndexSet = new HashSet<uint>();
                var unicodes = new List<uint>();
                for (uint c = 0x0020; c <= 0x007E; c++) unicodes.Add(c);
                for (uint c = 0x0E01; c <= 0x0E5B; c++) unicodes.Add(c);
                unicodes.Add(0x2013); unicodes.Add(0x2014); unicodes.Add(0x2018); unicodes.Add(0x2019);
                unicodes.Add(0x201C); unicodes.Add(0x201D); unicodes.Add(0x2022); unicodes.Add(0x2026);

                var miGetGlyphIndex = typeof(FontEngine).GetMethod("GetGlyphIndex", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(uint) }, null);
                var miTryAddGlyphs = typeof(FontEngine).GetMethod("TryAddGlyphsToTexture", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new Type[] { typeof(List<uint>), typeof(int), typeof(GlyphPackingMode), typeof(List<GlyphRect>), typeof(List<GlyphRect>), typeof(GlyphRenderMode), typeof(Texture2D), typeof(Glyph[]).MakeByRefType() }, null);
                if (miGetGlyphIndex == null || miTryAddGlyphs == null)
                {
                    Log("FontEngine methods missing: GetGlyphIndex=" + (miGetGlyphIndex != null) + " TryAddGlyphs=" + (miTryAddGlyphs != null));
                    return;
                }

                foreach (uint u in unicodes)
                {
                    uint gi = (uint)miGetGlyphIndex.Invoke(null, new object[] { u });
                    if (gi != 0)
                    {
                        unicodeToGlyph[u] = gi;
                        glyphIndexSet.Add(gi);
                    }
                }

                var glyphIndexes = new List<uint>(glyphIndexSet);

                int atlasW = 1024, atlasH = 1024, padding = 9;
                Texture2D tex = new Texture2D(atlasW, atlasH, TextureFormat.Alpha8, false);
                tex.name = "LINE Seed Sans TH Game Atlas";
                UnityEngine.Object.DontDestroyOnLoad(tex);

                var freeRects = new List<GlyphRect> { new GlyphRect(0, 0, atlasW - 1, atlasH - 1) };
                var usedRects = new List<GlyphRect>();
                object[] tryArgs = new object[] { glyphIndexes, padding, GlyphPackingMode.BestShortSideFit, freeRects, usedRects, GlyphRenderMode.SDFAA, tex, null };
                bool ok = (bool)miTryAddGlyphs.Invoke(null, tryArgs);
                Glyph[] packedGlyphs = tryArgs[7] as Glyph[];
                tex.Apply(false, false);

                TMP_FontAsset fa = ScriptableObject.CreateInstance<TMP_FontAsset>();
                fa.name = "LINE Seed Sans TH Game SDF";
                var fVer = typeof(TMP_FontAsset).GetField("m_Version", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fVer != null) fVer.SetValue(fa, "1.1.0");
                fa.atlasPopulationMode = AtlasPopulationMode.Static;
                fa.faceInfo = faceInfo;

                var fWidth = typeof(TMP_FontAsset).GetField("m_AtlasWidth", BindingFlags.Instance | BindingFlags.NonPublic);
                var fHeight = typeof(TMP_FontAsset).GetField("m_AtlasHeight", BindingFlags.Instance | BindingFlags.NonPublic);
                var fPad = typeof(TMP_FontAsset).GetField("m_AtlasPadding", BindingFlags.Instance | BindingFlags.NonPublic);
                var fMode = typeof(TMP_FontAsset).GetField("m_AtlasRenderMode", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fWidth != null) fWidth.SetValue(fa, atlasW);
                if (fHeight != null) fHeight.SetValue(fa, atlasH);
                if (fPad != null) fPad.SetValue(fa, padding);
                if (fMode != null) fMode.SetValue(fa, GlyphRenderMode.SDFAA);

                fa.atlasTextures = new Texture2D[] { tex };

                Shader sdfShader = Shader.Find("TextMeshPro/Mobile/Distance Field");
                if (sdfShader == null && TMP_Settings.defaultFontAsset != null && TMP_Settings.defaultFontAsset.material != null)
                    sdfShader = TMP_Settings.defaultFontAsset.material.shader;
                if (sdfShader == null)
                {
                    foreach (var existingFa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                    {
                        if (existingFa != null && existingFa.material != null && existingFa.material.shader != null)
                        {
                            sdfShader = existingFa.material.shader;
                            break;
                        }
                    }
                }
                if (sdfShader == null)
                {
                    Log("Could not find TMP Distance Field shader!");
                    return;
                }
                Material mat = new Material(sdfShader);
                mat.name = "LINE Seed Sans TH Game Material";
                mat.mainTexture = tex;
                mat.SetFloat(ShaderUtilities.ID_TextureWidth, atlasW);
                mat.SetFloat(ShaderUtilities.ID_TextureHeight, atlasH);
                mat.SetFloat(ShaderUtilities.ID_GradientScale, padding + 1);
                mat.SetFloat(ShaderUtilities.ID_WeightNormal, 0f);
                mat.SetFloat(ShaderUtilities.ID_WeightBold, 0.75f);
                UnityEngine.Object.DontDestroyOnLoad(mat);
                fa.material = mat;

                var fGlyphTbl = typeof(TMP_FontAsset).GetField("m_GlyphTable", BindingFlags.Instance | BindingFlags.NonPublic);
                var fCharTbl = typeof(TMP_FontAsset).GetField("m_CharacterTable", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fa.glyphTable == null && fGlyphTbl != null) fGlyphTbl.SetValue(fa, new List<Glyph>());
                if (fa.characterTable == null && fCharTbl != null) fCharTbl.SetValue(fa, new List<TMP_Character>());
                if (fa.fallbackFontAssetTable == null) fa.fallbackFontAssetTable = new List<TMP_FontAsset>();

                var glyphMap = new Dictionary<uint, Glyph>();
                if (packedGlyphs != null)
                {
                    foreach (var g in packedGlyphs)
                    {
                        if (g != null)
                        {
                            fa.glyphTable.Add(g);
                            glyphMap[g.index] = g;
                        }
                    }
                }
                foreach (var kv in unicodeToGlyph)
                {
                    Glyph g;
                    if (glyphMap.TryGetValue(kv.Value, out g))
                    {
                        fa.characterTable.Add(new TMP_Character(kv.Key, g));
                    }
                }

                fa.ReadFontAssetDefinition();
                UnityEngine.Object.DontDestroyOnLoad(fa);
                ThaiTmpFont = fa;

                if (TMP_Settings.fallbackFontAssets != null && !TMP_Settings.fallbackFontAssets.Contains(ThaiTmpFont))
                {
                    TMP_Settings.fallbackFontAssets.Insert(0, ThaiTmpFont);
                }
                var allFontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (var existing in allFontAssets)
                {
                    if (existing != null && existing != ThaiTmpFont)
                    {
                        if (existing.fallbackFontAssetTable == null)
                            existing.fallbackFontAssetTable = new List<TMP_FontAsset>();
                        if (!existing.fallbackFontAssetTable.Contains(ThaiTmpFont))
                            existing.fallbackFontAssetTable.Insert(0, ThaiTmpFont);
                    }
                }
                Log("Created ThaiTmpFont (ok=" + ok + ", glyphs=" + fa.glyphTable.Count + ", chars=" + fa.characterTable.Count + ") and attached to " + allFontAssets.Length + " TMP_FontAssets.");
                var tmpTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
                foreach (var tt in tmpTexts)
                {
                    if (tt != null && tt.gameObject.scene.IsValid())
                    {
                        tt.SetAllDirty();
                        tt.ForceMeshUpdate(true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("EnsureThaiTmpFont ERROR: " + ex);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                Log("Scene loaded: " + scene.name + " (Language=" + (LocalizationModel.I != null ? LocalizationModel.I.Language.ToString() : "null") + ")");
                EnsureEmbeddedThaiUnityFont();
                EnsureThaiTmpFont();
                if (ThaiTmpFont != null && TMP_Settings.fallbackFontAssets != null && !TMP_Settings.fallbackFontAssets.Contains(ThaiTmpFont))
                {
                    TMP_Settings.fallbackFontAssets.Insert(0, ThaiTmpFont);
                }

                // Also attach Thai fallback to all loaded TMP_FontAssets
                if (ThaiTmpFont != null)
                {
                    var allFontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                    foreach (var fa in allFontAssets)
                    {
                        if (fa != null && fa != ThaiTmpFont)
                        {
                            if (fa.fallbackFontAssetTable == null)
                                fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
                            if (!fa.fallbackFontAssetTable.Contains(ThaiTmpFont))
                                fa.fallbackFontAssetTable.Insert(0, ThaiTmpFont);
                        }
                    }
                }

                // Ensure SettingsPopup gets Thai option whenever present in scene
                var popups = Resources.FindObjectsOfTypeAll<SettingsPopup>();
                foreach (var p in popups)
                {
                    EnsureThaiOption(p);
                }

                // If Thai is active, refresh Unity UI Text fonts
                if (LocalizationModel.I != null && LocalizationModel.I.Language == ThaiLang && ThaiUnityFont != null)
                {
                    var texts = Resources.FindObjectsOfTypeAll<Text>();
                    foreach (var t in texts)
                    {
                        if (t != null && t.font != ThaiUnityFont) t.font = ThaiUnityFont;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("OnSceneLoaded ERROR: " + ex);
            }
        }

        public static void EnsureThaiOption(SettingsPopup popup)
        {
            if (popup == null) return;
            var ddField = typeof(SettingsPopup).GetField("languageDropdown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (ddField == null) return;
            var dd = ddField.GetValue(popup) as TMP_Dropdown;
            if (dd == null || dd.options == null) return;

            if (dd.options.Count == 13)
            {
                dd.options.Add(new TMP_Dropdown.OptionData("ภาษาไทย (Thai)"));
                if ((int)SettingsModel.I.ChosenLanguage == 13)
                {
                    dd.SetValueWithoutNotify(13);
                }
                dd.RefreshShownValue();
                Log("Added Thai option to SettingsPopup.languageDropdown.");
            }
        }

        private static void InstallHooks()
        {
            Detour(
                typeof(LocalizationModel).GetMethod("GetText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                typeof(Bootstrap).GetMethod("Hook_GetText", BindingFlags.Static | BindingFlags.Public)
            );
            Detour(
                typeof(LocalizedLabel).GetMethod("SetFontForUnityText", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
                typeof(Bootstrap).GetMethod("Hook_SetFontForUnityText", BindingFlags.Static | BindingFlags.Public)
            );
            Detour(
                typeof(LabelFontSwitcher).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                typeof(Bootstrap).GetMethod("Hook_LabelFontSwitcherAwake", BindingFlags.Static | BindingFlags.Public)
            );
            Detour(
                typeof(TmpFontFixer).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                typeof(Bootstrap).GetMethod("Hook_TmpFontFixerAwake", BindingFlags.Static | BindingFlags.Public)
            );
        }

        private static unsafe void Detour(MethodInfo target, MethodInfo replacement)
        {
            if (target == null || replacement == null)
            {
                Log("Detour skipped: null method (" + (target != null ? target.Name : "null") + ")");
                return;
            }
            RuntimeHelpers.PrepareMethod(target.MethodHandle);
            RuntimeHelpers.PrepareMethod(replacement.MethodHandle);
            IntPtr src = target.MethodHandle.GetFunctionPointer();
            IntPtr dst = replacement.MethodHandle.GetFunctionPointer();

            bool isMac = Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
            bool isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

            if (!isMac)
            {
                uint oldProtect;
                VirtualProtect(src, (UIntPtr)16, PAGE_EXECUTE_READWRITE, out oldProtect);
                byte* p = (byte*)src.ToPointer();
                // x64: mov rax, dst; jmp rax (12 bytes)
                p[0] = 0x48;
                p[1] = 0xB8;
                *(long*)(p + 2) = dst.ToInt64();
                p[10] = 0xFF;
                p[11] = 0xE0;
                VirtualProtect(src, (UIntPtr)16, oldProtect, out oldProtect);
            }
            else
            {
                long pageSize = 4096;
                try { pageSize = getpagesize(); } catch { }
                long addrVal = src.ToInt64();
                long pageStart = addrVal & ~(pageSize - 1);
                long span = (addrVal - pageStart) + 32;
                int mres = mprotect(new IntPtr(pageStart), (UIntPtr)span, 7); // PROT_READ | PROT_WRITE | PROT_EXEC
                if (mres != 0)
                    Log("mprotect warning: res=" + mres + " err=" + Marshal.GetLastWin32Error());

                byte* p = (byte*)src.ToPointer();
                if (isArm64)
                {
                    // ARM64: LDR X16, #8; BR X16; .quad dst (16 bytes)
                    *(uint*)(p + 0) = 0x58000050u;
                    *(uint*)(p + 4) = 0xD61F0200u;
                    *(long*)(p + 8) = dst.ToInt64();
                }
                else
                {
                    // x64: mov rax, dst; jmp rax (12 bytes)
                    p[0] = 0x48;
                    p[1] = 0xB8;
                    *(long*)(p + 2) = dst.ToInt64();
                    p[10] = 0xFF;
                    p[11] = 0xE0;
                }
                try { sys_icache_invalidate(src, (UIntPtr)16); } catch { }
            }
            Log("Detoured " + target.DeclaringType.Name + "." + target.Name + " -> " + replacement.Name);
        }

        public static string Hook_GetText(LocalizationModel self, string key)
        {
            if (key == null) return "";
            if (self.Language == ThaiLang)
            {
                string val;
                if (ThaiDict.TryGetValue(key, out val))
                    return val;
                if (KeysEnglish.Dict.TryGetValue(key, out val))
                    return val;
                return key;
            }

            Dictionary<string, string> dict = null;
            switch (self.Language)
            {
                case LocalizationModel.LanguageType.English: dict = KeysEnglish.Dict; break;
                case LocalizationModel.LanguageType.Russian: dict = KeysRussian.Dict; break;
                case LocalizationModel.LanguageType.Turkish: dict = KeysTurkish.Dict; break;
                case LocalizationModel.LanguageType.French: dict = KeysFrench.Dict; break;
                case LocalizationModel.LanguageType.Spanish: dict = KeysSpanish.Dict; break;
                case LocalizationModel.LanguageType.German: dict = KeysGerman.Dict; break;
                case LocalizationModel.LanguageType.Italian: dict = KeysItalian.Dict; break;
                case LocalizationModel.LanguageType.Portuguese: dict = KeysPortuguese.Dict; break;
                case LocalizationModel.LanguageType.ChineseSimplified: dict = KeysChineseSimplified.Dict; break;
                case LocalizationModel.LanguageType.ChineseTraditional: dict = KeysChineseTraditional.Dict; break;
                case LocalizationModel.LanguageType.Japanese: dict = KeysJapanese.Dict; break;
                case LocalizationModel.LanguageType.Korean: dict = KeysKorean.Dict; break;
                case LocalizationModel.LanguageType.Polish: dict = KeysPolish.Dict; break;
                default: dict = KeysEnglish.Dict; break;
            }
            string res;
            if (dict != null && dict.TryGetValue(key, out res))
                return res;
            if (KeysEnglish.Dict.TryGetValue(key, out res))
                return res;
            Debug.LogError("Key could not be found: " + key);
            return key;
        }

        public static void Hook_SetFontForUnityText(Text text)
        {
            if (text == null) return;
            if (LocalizationModel.I.Language == ThaiLang && ThaiUnityFont != null)
            {
                text.font = ThaiUnityFont;
                return;
            }
            switch (LocalizationModel.I.Language)
            {
                case LocalizationModel.LanguageType.ChineseSimplified: text.font = VisualSettings.I.ChineseSimplifiedFont; break;
                case LocalizationModel.LanguageType.ChineseTraditional: text.font = VisualSettings.I.JapaneseTraditionalFont; break;
                case LocalizationModel.LanguageType.Japanese: text.font = VisualSettings.I.JapaneseFont; break;
                case LocalizationModel.LanguageType.Korean: text.font = VisualSettings.I.KoreanFont; break;
                default: text.font = VisualSettings.I.RegularFont; break;
            }
        }

        public static void Hook_LabelFontSwitcherAwake(LabelFontSwitcher self)
        {
            Text t = self.GetComponent<Text>();
            if (t == null) return;
            if (LocalizationModel.I.Language == ThaiLang && ThaiUnityFont != null)
            {
                t.font = ThaiUnityFont;
                return;
            }
            switch (LocalizationModel.I.Language)
            {
                case LocalizationModel.LanguageType.ChineseSimplified: t.font = VisualSettings.I.ChineseSimplifiedFont; break;
                case LocalizationModel.LanguageType.ChineseTraditional: t.font = VisualSettings.I.JapaneseTraditionalFont; break;
                case LocalizationModel.LanguageType.Japanese: t.font = VisualSettings.I.JapaneseFont; break;
                default: t.font = VisualSettings.I.RegularFont; break;
            }
        }

        public static void Hook_TmpFontFixerAwake(TmpFontFixer self)
        {
            var faField = typeof(TmpFontFixer).GetField("fontAsset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (faField == null) return;
            var fa = faField.GetValue(self) as TMP_FontAsset;
            if (fa == null || fa.fallbackFontAssetTable == null) return;

            if (LocalizationModel.I.Language == ThaiLang && ThaiTmpFont != null)
            {
                if (!fa.fallbackFontAssetTable.Contains(ThaiTmpFont))
                    fa.fallbackFontAssetTable.Insert(0, ThaiTmpFont);
                return;
            }

            string fieldName = null;
            switch (LocalizationModel.I.Language)
            {
                case LocalizationModel.LanguageType.ChineseSimplified: fieldName = "chineseSimplifiedFontAsset"; break;
                case LocalizationModel.LanguageType.ChineseTraditional: fieldName = "chineseTraditionalFontAsset"; break;
                case LocalizationModel.LanguageType.Japanese: fieldName = "japaneseFontAsset"; break;
                case LocalizationModel.LanguageType.Korean: fieldName = "koreanFontAsset"; break;
            }
            if (fieldName != null)
            {
                var f = typeof(TmpFontFixer).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var target = f != null ? f.GetValue(self) as TMP_FontAsset : null;
                if (target != null && fa.fallbackFontAssetTable.Count > 1)
                {
                    fa.fallbackFontAssetTable[1] = target;
                }
            }
        }

    }

    public class ThaiModRunner : MonoBehaviour
    {
        private void Start()
        {
            Bootstrap.Log("ThaiModRunner.Start() activeScene=" + SceneManager.GetActiveScene().name);
            Bootstrap.EnsureThaiTmpFont();
        }

        private static bool ContainsThai(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '\u0E00' && c <= '\u0E7F') return true;
            }
            return false;
        }

        public static void ApplyThaiFontsInScene()
        {
            if (LocalizationModel.I == null || LocalizationModel.I.Language != Bootstrap.ThaiLang) return;
            if (Bootstrap.ThaiUnityFont != null)
            {
                foreach (var t in Resources.FindObjectsOfTypeAll<Text>())
                {
                    if (t != null && t.gameObject.scene.IsValid() && t.font != Bootstrap.ThaiUnityFont)
                        t.font = Bootstrap.ThaiUnityFont;
                }
            }
            if (Bootstrap.ThaiTmpFont != null)
            {
                foreach (var tt in Resources.FindObjectsOfTypeAll<TMP_Text>())
                {
                    if (tt != null && tt.gameObject.scene.IsValid() && tt.font != Bootstrap.ThaiTmpFont && ContainsThai(tt.text))
                    {
                        tt.font = Bootstrap.ThaiTmpFont;
                        tt.SetAllDirty();
                    }
                }
            }
        }

        private void Update()
        {
            if (Time.frameCount % 15 == 0)
            {
                Bootstrap.EnsureEmbeddedThaiUnityFont();
                Bootstrap.EnsureThaiTmpFont();
                var p = UnityEngine.Object.FindObjectOfType<SettingsPopup>();
                if (p != null)
                {
                    Bootstrap.EnsureThaiOption(p);
                }
                ApplyThaiFontsInScene();
            }
        }
    }
}
