#!/bin/bash

CUR_DIR=$(pwd)

echo "Building for platform: Standalone"
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_STANDALONE"
cp -f ${CUR_DIR}/Build/Kernel/Release/Kernel.* ../Assets/Plugins/Kernel/Standalone/

echo "Building for platform: Android"
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_ANDROID"
cp -f ${CUR_DIR}/Build/Kernel/Release/Kernel.* ../Assets/Plugins/Kernel/Android/

echo "Building for platform: iOS"
msbuild Kernel.sln /t:Clean,Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="TRACE UNITY_IOS"
cp -f ${CUR_DIR}/Build/Kernel/Release/Kernel.* ../Assets/Plugins/Kernel/iOS/

echo "Build finished!"
echo

read -p "Press any key to continue.."
