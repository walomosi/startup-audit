@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist bin mkdir bin
"%CSC%" /nologo /target:winexe /out:bin\StartupAudit.exe *.cs Core\*.cs /reference:System.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll
if errorlevel 1 (echo FAIL) else (echo OK: bin\StartupAudit.exe)
