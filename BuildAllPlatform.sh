#!/bin/bash

CUR_DIR=$(pwd)

echo "Building for platform: Editor"
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:DefineConstants="TRACE DEBUG UNITY_EDITOR"
mkdir -p ${CUR_DIR}/Output/vFrame.Core/Runtime
cp -f ${CUR_DIR}/Build/vFrame.Core/Debug/vFrame.Core.* ${CUR_DIR}/Output/vFrame.Core/Runtime/
mkdir -p ${CUR_DIR}/Output/vFrame.Core/Editor
cp -f ${CUR_DIR}/Build/vFrame.Core.Editor/Debug/vFrame.Core.Editor.* ${CUR_DIR}/Output/vFrame.Core/Editor/

echo "Building for platform: Standalone"
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_STANDALONE"
mkdir -p ${CUR_DIR}/Output/vFrame.Core/Runtime/Standalone
cp -f ${CUR_DIR}/Build/vFrame.Core/Release/vFrame.Core.* ${CUR_DIR}/Output/vFrame.Core/Runtime/Standalone/

echo "Building for platform: Android"
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_ANDROID"
mkdir -p ${CUR_DIR}/Output/vFrame.Core/Runtime/Android
cp -f ${CUR_DIR}/Build/vFrame.Core/Release/vFrame.Core.* ${CUR_DIR}/Output/vFrame.Core/Runtime/Android/

echo "Building for platform: iOS"
msbuild vFrame.Core.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_IOS"
mkdir -p ${CUR_DIR}/Output/vFrame.Core/Runtime/iOS
cp -f ${CUR_DIR}/Build/vFrame.Core/Release/vFrame.Core.* ${CUR_DIR}/Output/vFrame.Core/Runtime/iOS/

echo "Build finished!"
echo

read -p "Press any key to continue.."
