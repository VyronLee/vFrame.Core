SolutionFile = vFrame.Core.sln

Compiler = msbuild /t:Rebuild  /p:Platform="Any CPU"
PlatformContantsDefined_Editor = /p:DefineConstants="TRACE UNITY_EDITOR"
PlatformContantsDefined_Android = /p:DefineConstants="TRACE UNITY_ANDROID"
PlatformContantsDefined_IOS = /p:DefineConstants="TRACE UNITY_IOS"
PlatformContantsDefined_Standalone = /p:DefineConstants="TRACE UNITY_STANDALONE"

ConfigDebug = /p:Configuration=Debug
ConfigRelease = /p:Configuration=Release

BuildDir = Build
BuildReleaseDir = $(BuildDir)/Release
BuildDebugDir = $(BuildDir)/Debug

DebugEditorDir = $(BuildDebugDir)/Editor
DebugRuntimeDir = $(BuildDebugDir)/Runtime
ReleaseRuntimeDir = $(BuildReleaseDir)/Runtime

OutputDir = Output
EditorOutputDir = $(OutputDir)/Editor
RuntimeOutputDir = $(OutputDir)/Runtime
AndroidOutputDir = $(RuntimeOutputDir)/Android
IOSOutputDir = $(RuntimeOutputDir)/iOS
StandaloneOutputDir = $(RuntimeOutputDir)/Standalone

EditorAssembly = vFrame.Core.Editor.dll
RuntimeLiteAssembly = vFrame.Core.Lite.dll
RuntimeUnityAssembly = vFrame.Core.Unity.dll

DeployDir = ../../Project/Assets/Core/vFrame.Core/

all: clean editor output_editor android output_android ios output_ios standalone output_standalone

debug: clean editor output_editor

clean:
	rm -rf $(OutputDir)
	rm -rf $(BuildDir)

editor:
	$(Compiler) $(ConfigDebug) $(SolutionFile) $(PlatformContantsDefined_Editor)

android:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformContantsDefined_Android)

ios:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformContantsDefined_iOS)

standalone:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformContantsDefined_Standalone)

output_editor:
	mkdir -p $(EditorOutputDir)
	mkdir -p $(RuntimeOutputDir)
	cp -rf $(DebugEditorDir)/$(EditorAssembly) $(EditorOutputDir)/$(EditorAssembly)
	cp -rf $(DebugRuntimeDir)/$(RuntimeLiteAssembly) $(RuntimeOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(DebugRuntimeDir)/$(RuntimeUnityAssembly) $(RuntimeOutputDir)/$(RuntimeUnityAssembly)

output_android:
	mkdir -p $(AndroidOutputDir)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeLiteAssembly) $(AndroidOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeUnityAssembly) $(AndroidOutputDir)/$(RuntimeUnityAssembly)

output_ios:
	mkdir -p $(IOSOutputDir)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeLiteAssembly) $(IOSOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeUnityAssembly) $(IOSOutputDir)/$(RuntimeUnityAssembly)

output_standalone:
	mkdir -p $(StandaloneOutputDir)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeLiteAssembly) $(StandaloneOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeUnityAssembly) $(StandaloneOutputDir)/$(RuntimeUnityAssembly)

deploy:
	cp -rfv $(EditorOutputDir) $(DeployDir)
	cp -rfv $(RuntimeOutputDir) $(DeployDir)

.PHONY:
	clean editor android ios standalone output_editor output_android output_ios output_standalone

