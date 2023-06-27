SolutionFile = vFrame.Core.sln
UnityVersion = 2018

Compiler = MSBuild /t:Build /p:Platform=Any\ CPU
PlatformConstantsDefined_Editor = /p:DefineConstants=TRACE\ UNITY_EDITOR
PlatformConstantsDefined_Android = /p:DefineConstants=TRACE\ UNITY_ANDROID
PlatformConstantsDefined_IOS = /p:DefineConstants=TRACE\ UNITY_IOS
PlatformConstantsDefined_Standalone = /p:DefineConstants=TRACE\ UNITY_STANDALONE

ConfigRelease = /p:Configuration=Release_$(UnityVersion)

BuildDir = Build
BuildReleaseDir = $(BuildDir)/Release
ReleaseEditorDir = $(BuildReleaseDir)/Editor
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

all: editor output_editor android output_android ios output_ios standalone output_standalone

clean:
	rm -rf $(OutputDir)/Editor/*.dll
	rm -rf $(OutputDir)/Runtime/*.dll
	rm -rf $(OutputDir)/Runtime/**/*.dll
	rm -rf $(BuildDir)

editor:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformConstantsDefined_Editor)

android:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformConstantsDefined_Android)

ios:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformConstantsDefined_iOS)

standalone:
	$(Compiler) $(ConfigRelease) $(SolutionFile) $(PlatformConstantsDefined_Standalone)

output_editor:
	mkdir -p $(EditorOutputDir)
	mkdir -p $(RuntimeOutputDir)
	cp -rf $(ReleaseEditorDir)/$(EditorAssembly) $(EditorOutputDir)/$(EditorAssembly)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeLiteAssembly) $(RuntimeOutputDir)/$(RuntimeLiteAssembly)
	cp -rf $(ReleaseRuntimeDir)/$(RuntimeUnityAssembly) $(RuntimeOutputDir)/$(RuntimeUnityAssembly)

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

.PHONY:
	clean editor android ios standalone output_editor output_android output_ios output_standalone