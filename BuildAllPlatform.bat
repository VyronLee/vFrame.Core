@echo off

SET CURDIR=%~dp0

echo Building for platform: Standalone
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_STANDALONE"
copy /Y %CURDIR%\Build\Kernel\Release\Kernel.* ..\Assets\Plugins\Kernel\Standalone\

echo Building for platform: Android
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_ANDROID"
copy /Y %CURDIR%\Build\Kernel\Release\Kernel.* ..\Assets\Plugins\Kernel\Android\

echo Building for platform: iOS
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_IOS"
copy /Y %CURDIR%\Build\Kernel\Release\Kernel.* ..\Assets\Plugins\Kernel\iOS\

Pause