@echo off
setlocal
set "GAME_DIR=%~dp0..\.."
set "MANAGED=%GAME_DIR%\Tavern Master_Data\Managed"
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist "%MANAGED%\Assembly-CSharp.dll" (
    echo [ERROR] Could not find Tavern Master_Data\Managed\Assembly-CSharp.dll
    exit /b 1
)

"%CSC%" /nologo /target:library /unsafe /nostdlib /noconfig ^
  /out:"%~dp0ThaiMod.dll" ^
  /r:"%MANAGED%\mscorlib.dll" ^
  /r:"%MANAGED%\System.dll" ^
  /r:"%MANAGED%\System.Core.dll" ^
  /r:"%MANAGED%\netstandard.dll" ^
  /r:"%MANAGED%\Assembly-CSharp.dll" ^
  /r:"%MANAGED%\UnityEngine.CoreModule.dll" ^
  /r:"%MANAGED%\UnityEngine.TextRenderingModule.dll" ^
  /r:"%MANAGED%\UnityEngine.TextCoreFontEngineModule.dll" ^
  /r:"%MANAGED%\UnityEngine.TextCoreTextEngineModule.dll" ^
  /r:"%MANAGED%\UnityEngine.UI.dll" ^
  /r:"%MANAGED%\Unity.TextMeshPro.dll" ^
  /r:"%MANAGED%\Newtonsoft.Json.dll" ^
  "%~dp0src\ThaiMod.cs"
