SolutionFile = vFrame.Core.sln

Compiler = msbuild /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
PlatformContantsDefined_Editor = /p:DefineConstants="TRACE UNITY_EDITOR"
PlatformContantsDefined_Android = /p:DefineConstants="TRACE UNITY_ANDROID"
PlatformContantsDefined_IOS = /p:DefineConstants="TRACE UNITY_IOS"
PlatformContantsDefined_Standalone = /p:DefineConstants="TRACE UNITY_STANDALONE"

BuildDir = Build
BuildReleaseDir = $(BuildDir)/Release
BuildEditorDir = $(BuildReleaseDir)/Editor
BuildRuntimeDir = $(BuildReleaseDir)/Runtime

OutputDir = Output
EditorOutputDir = $(OutputDir)/Editor
RuntimeOutputDir = $(OutputDir)/Runtime
AndroidOutputDir = $(RuntimeOutputDir)/Android
IOSOutputDir = $(RuntimeOutputDir)/iOS
StandaloneOutputDir = $(RuntimeOutputDir)/Standalone

EditorAssembly = vFrame.Core.Editor.dll
RuntimeLiteAssembly = vFrame.Core.Lite.dll
RuntimeUnityAssembly = vFrame.Core.Unity.dll

all: clean editor output_editor android output_android ios output_ios standalone output_standalone

clean:
	rm -rf $(OutputDir)
	rm -rf $(BuildDir)

editor:
	$(Compiler) $(SolutionFile) $(PlatformContantsDefined_Editor)

android:
	$(Compiler) $(SolutionFile) $(PlatformContantsDefined_Android)

ios:
	$(Compiler) $(SolutionFile) $(PlatformContantsDefined_iOS)

standalone:
	$(Compiler) $(SolutionFile) $(PlatformContantsDefined_Standalone)

output_editor:
	mkdir -p $(EditorOutputDir)
	mkdir -p $(RuntimeOutputDir)
	cp -rf $(BuildEditorDir)/$(EditorAssembly) $(EditorOutputDir)/$(EditorAssembly)
	cp -rf $(BuildRuntimeDir)/$(RuntimeLiteAssembly) $(RuntimeOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(BuildRuntimeDir)/$(RuntimeUnityAssembly) $(RuntimeOutputDir)/$(RuntimeUnityAssembly)

output_android:
	mkdir -p $(AndroidOutputDir)
	cp -rf $(BuildRuntimeDir)/$(RuntimeLiteAssembly) $(AndroidOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(BuildRuntimeDir)/$(RuntimeUnityAssembly) $(AndroidOutputDir)/$(RuntimeUnityAssembly)

output_ios:
	mkdir -p $(IOSOutputDir)
	cp -rf $(BuildRuntimeDir)/$(RuntimeLiteAssembly) $(IOSOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(BuildRuntimeDir)/$(RuntimeUnityAssembly) $(IOSOutputDir)/$(RuntimeUnityAssembly)

output_standalone:
	mkdir -p $(StandaloneOutputDir)
	cp -rf $(BuildRuntimeDir)/$(RuntimeLiteAssembly) $(StandaloneOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(BuildRuntimeDir)/$(RuntimeUnityAssembly) $(StandaloneOutputDir)/$(RuntimeUnityAssembly)

.PHONY:
	clean editor android ios standalone output_editor output_android output_ios output_standalone

