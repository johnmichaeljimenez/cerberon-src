@echo off
setlocal enabledelayedexpansion

set "SOURCE=%~dp0"
for %%A in ("%~dp0..\Assets") do set "TARGET=%%~fA"

echo Starting PSD to PNG conversion (recursive + mirror folders)...
echo Source: %SOURCE%
echo Target: %TARGET%
echo.

for /r "%SOURCE%" %%F in (*.psd) do (
    set "fullpath=%%F"
    
    set "relpath=!fullpath:%SOURCE%=!"
    
    set "outfile=%TARGET%\!relpath!"
    set "outfile=!outfile:.psd=.png!"
    
    for %%D in ("!outfile!") do set "outdir=%%~dpD"
    if not exist "!outdir!" (
        mkdir "!outdir!"
        echo   Created folder: !outdir!
    )

    echo   Converting: %%~nxF  into  !relpath:.psd=.png!
    magick "%%F[0]" "!outfile!"
)

echo.
echo Done
pause