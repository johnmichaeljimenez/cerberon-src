@echo off
setlocal enabledelayedexpansion

set "SOURCE=%~dp0"
set "TARGET=%~dp0../Assets"

echo Starting PSD to PNG conversion (recursive + mirror folders)
echo Source: %SOURCE%
echo Target: %TARGET%
echo.

for /r "%SOURCE%" %%F in (*.psd) do (
    set "filedir=%%~dpF"
    set "reldir=!filedir:%SOURCE%=!"
    set "outdir=%TARGET%!reldir!"

    if not exist "!outdir!" (
        mkdir "!outdir!"
        echo   Created folder: !outdir!
    )

    set "outfile=!outdir!%%~nF.png"

    echo   Converting: %%~nxF  into  %%~nF.png
    magick "%%F[0]" "!outfile!"
)

echo.
echo Done
pause