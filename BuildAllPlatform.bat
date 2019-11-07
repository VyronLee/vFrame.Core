@echo off

SET CURDIR=%~dp0

echo Building for platform: Editor
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:DefineConstants="TRACE DEBUG UNITY_EDITOR"
if not exist "%CURDIR%\Output\vFrame.Core\Runtime" mkdir %CURDIR%\Output\vFrame.Core\Runtime
copy /Y %CURDIR%\Build\vFrame.Core\Debug\vFrame.Core.* %CURDIR%\Output\vFrame.Core\Runtime\
if not exist "%CURDIR%\Output\vFrame.Core\Editor" mkdir %CURDIR%\Output\vFrame.Core\Editor
copy /Y %CURDIR%\Build\vFrame.Core.Editor\Debug\vFrame.Core.Editor.* %CURDIR%\Output\vFrame.Core\Editor\

echo Building for platform: Standalone
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_STANDALONE"
if not exist "%CURDIR%\Output\vFrame.Core\Runtime\Standalone" mkdir %CURDIR%\Output\vFrame.Core\Runtime\Standalone
copy /Y %CURDIR%\Build\vFrame.Core\Release\vFrame.Core.* %CURDIR%\Output\vFrame.Core\Runtime\Standalone\

echo Building for platform: Android
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_ANDROID"
if not exist "%CURDIR%\Output\vFrame.Core\Runtime\Android" mkdir %CURDIR%\Output\vFrame.Core\Runtime\Android
copy /Y %CURDIR%\Build\vFrame.Core\Release\vFrame.Core.* %CURDIR%\Output\vFrame.Core\Runtime\Android\

echo Building for platform: iOS
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_IOS"
if not exist "%CURDIR%\Output\vFrame.Core\Runtime\iOS" mkdir %CURDIR%\Output\vFrame.Core\Runtime\iOS
copy /Y %CURDIR%\Build\vFrame.Core\Release\vFrame.Core.* %CURDIR%\Output\vFrame.Core\Runtime\iOS\

Pause
