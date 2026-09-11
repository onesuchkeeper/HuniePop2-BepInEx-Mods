@echo off
setlocal EnableDelayedExpansion

REM ============================================================
REM Pyragify Batch Configuration
REM ============================================================

REM This BAT lives in:
REM
REM     Parent\Pyragify\run-pyragify.bat
REM
REM Target directories live in the parent directory:
REM
REM     Parent\Hp2BaseMod
REM     Parent\ExpandedWardrobe
REM     etc.

set "PYRAGIFY_DIR=%~dp0"
set "ROOT=%~dp0.."
set "RESULT_PATH=%PYRAGIFY_DIR%result"


REM ============================================================
REM OPTIONS
REM ============================================================

REM Set to true to completely delete each target's
REM Workspace_Chunks directory before running Pyragify.
REM
REM This forces Pyragify to regenerate everything.
REM
REM Set to false to leave existing output alone.

set "CLEAN=true"


REM ============================================================
REM TARGETS
REM ============================================================
REM
REM Directories are relative to the parent directory.
REM Separate directories using |
REM
REM The | character is safe because Windows does not allow it
REM in directory names.
REM
REM Example:
REM
REM set "TARGETS=Hp2BaseMod|ExpandedWardrobe|Some Mod With Spaces"
REM

set "TARGETS=Hp2BaseMod|AnniversaryTitle|Cheat|ExpandedWardrobe|ExtraLocations|HiraganaLogo|Hp2BaseMod.Analyzer|Hp2BaseModTweaks|HuniePopUltimate|MidSingleDatePhotos|Randomizer|RepeatThreesome|SingleDate"


REM ============================================================
REM Pyragify Configuration
REM ============================================================

set "MAX_WORDS=200000"
set "MAX_FILE_SIZE=10485760"


REM ============================================================
REM Prepare Result Directory
REM ============================================================

if not exist "%RESULT_PATH%\" (
    mkdir "%RESULT_PATH%"
)


REM ============================================================
REM Clean Previous Results
REM ============================================================

if /I "%CLEAN%"=="true" (

    echo.
    echo ========================================
    echo       Cleaning Previous Results
    echo ========================================
    echo.

    REM Remove all previously generated result files.
    del /Q "%RESULT_PATH%\*.txt" >nul 2>&1

    echo Result directory cleaned.
    echo.
)


REM ============================================================
REM Process Each Target
REM ============================================================

echo.
echo ========================================
echo       Pyragify Batch Processing
echo ========================================
echo.
echo Parent directory:
echo %ROOT%
echo.
echo Result directory:
echo %RESULT_PATH%
echo.
echo Full regeneration:
echo %CLEAN%
echo.

for %%T in ("%TARGETS:|=" "%") do (

    set "TARGET=%%~T"
    set "TARGET_PATH=%ROOT%\!TARGET!"
    set "OUTPUT_PATH=!TARGET_PATH!\Workspace_Chunks"
    set "REMAINING_PATH=!OUTPUT_PATH!\remaining"

    echo ----------------------------------------
    echo Processing: !TARGET!
    echo ----------------------------------------
    echo Target: !TARGET_PATH!
    echo.

    if not exist "!TARGET_PATH!\" (

        echo ERROR: Directory does not exist:
        echo        !TARGET_PATH!
        echo.

    ) else (

        REM ----------------------------------------------------
        REM Clean existing Pyragify output
        REM ----------------------------------------------------

        if /I "%CLEAN%"=="true" (

            if exist "!OUTPUT_PATH!\" (
                echo Cleaning existing Pyragify output...
                rmdir /S /Q "!OUTPUT_PATH!"
                echo Cleaned.
                echo.
            )
        )


        REM ----------------------------------------------------
        REM Create temporary Pyragify configuration
        REM ----------------------------------------------------

        set "TEMP_CONFIG=%TEMP%\pyragify_config_!RANDOM!.yaml"

        (
            REM Single quotes are important here.
            REM YAML double quotes interpret Windows backslashes
            REM as escape characters.
            echo repo_path: '!TARGET_PATH!'
            echo output_dir: '!OUTPUT_PATH!'
            echo.
            echo max_words: %MAX_WORDS%
            echo max_file_size: %MAX_FILE_SIZE%
            echo.
            echo skip_dirs:
            echo   - ".git"
            echo   - ".vs"
            echo   - "bin"
            echo   - "obj"
            echo   - "Workspace_Chunks"
            echo.
            echo skip_patterns:
            echo   - "**/obj/**"
            echo   - "**/bin/**"
            echo   - "*.dll"
            echo   - "*.exe"
            echo   - "*.csproj"
            echo   - "*.sln"
            echo   - "*.meta"
            echo   - "*.pdb"
            echo   - "*.cache"
            echo   - "*.json"
            echo   - "*.editorconfig"
            echo   - "*.txt"
            echo   - "*.xml"
            echo   - "*.md"
            echo   - "*.yaml"
            echo   - "*.yml"
            echo   - "*.config"
            echo   - "*.props"
            echo   - "*.targets"
            echo   - "*.csproj.*"
            echo.
            echo verbose: true
        ) > "!TEMP_CONFIG!"


        REM ----------------------------------------------------
        REM Run Pyragify
        REM ----------------------------------------------------

        pyragify --config-file "!TEMP_CONFIG!"

        if errorlevel 1 (

            echo.
            echo ERROR: Pyragify failed for:
            echo        !TARGET!
            echo.

        ) else (

            REM ------------------------------------------------
            REM Move generated chunks into:
            REM
            REM Pyragify\result\
            REM
            REM chunk_0.txt  -> Target_0.txt
            REM chunk_1.txt  -> Target_1.txt
            REM etc.
            REM ------------------------------------------------

            if exist "!REMAINING_PATH!\" (

                echo.
                echo Moving chunks to result directory...

                set /a COUNT=0

                for %%F in ("!REMAINING_PATH!\chunk_*.txt") do (

                    REM Extract the original Pyragify chunk number.
                    REM
                    REM chunk_12.txt
                    REM       ^^^^^
                    REM       becomes 12
                    set "FILE_NAME=%%~nF"
                    set "INDEX=!FILE_NAME:chunk_=!"

                    set "DEST_FILE=!RESULT_PATH!\!TARGET!_!INDEX!.txt"

                    echo %%~nxF ^> !TARGET!_!INDEX!.txt

                    move /Y "%%~fF" "!DEST_FILE!" >nul

                    if not errorlevel 1 (
                        set /a COUNT+=1
                    ) else (
                        echo ERROR: Failed to move %%~nxF
                    )
                )

                echo.
                echo SUCCESS: !TARGET!
                echo Chunks moved: !COUNT!
                echo.

            ) else (

                echo.
                echo WARNING: Pyragify succeeded but no
                echo          remaining directory was found.
                echo.
                echo Expected:
                echo !REMAINING_PATH!
                echo.
            )
        )


        REM ----------------------------------------------------
        REM Remove temporary configuration
        REM ----------------------------------------------------

        del "!TEMP_CONFIG!" >nul 2>&1
    )
)


echo.
echo ========================================
echo        All Processing Complete
echo ========================================
echo.
echo Results:
echo %RESULT_PATH%
echo.

pause
