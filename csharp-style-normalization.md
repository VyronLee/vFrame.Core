# C# style normalization

## Rules
- Normalize file header to standard template
- Namespace should match assembly name only (no folder-derived suffixes)
- Add XML documentation comments for all methods

## Assemblies
- `vFrame.Core` (asmdef) → target namespace: `vFrame.Core`
- `vFrame.Core.Unity` (asmdef) → target namespace: `vFrame.Core.Unity`

## Exclusions
- `3rd/` directories (LitJson, ObjectsComparer, ObjectsCopy, SevenZip, Zlib, ZStd, SerializableDictionary)
- `Plugins/`, `Library/`, `Temp/`, `obj/`, `Logs/`
- Editor/Benchmarks, Tests (not requested)
- `Assets/Editor/BatchTestRunner.cs` (Assembly-CSharp-Editor, not project code)

---

## vFrame.Core Runtime

| Status | File | Note |
|---|---|---|
| done | Assets/vFrame.Core/Runtime/Base/BaseObject.cs | |
| done | Assets/vFrame.Core/Runtime/Base/BaseObjectException.cs | |
| done | Assets/vFrame.Core/Runtime/Base/CreateAbility.cs | |
| done | Assets/vFrame.Core/Runtime/Base/IBaseObject.cs | |
| done | Assets/vFrame.Core/Runtime/Base/ICreatable.cs | |
| done | Assets/vFrame.Core/Runtime/Base/IDestroyable.cs | |
| done | Assets/vFrame.Core/Runtime/Base/ILifetime.cs | |
| done | Assets/vFrame.Core/Runtime/Base/Lifetime.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/AsynchronousBlockBasedCompression.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/BlockBasedCompression.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/BlockBasedCompressionConst.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/BlockBasedCompressionException.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/BlockBasedCompressionOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/BlockBasedCompression/SynchronousBlockBasedCompression.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/Compressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/CompressorOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/CompressorPool.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/CompressorType.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/ICompressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/LZ4/LZ4Compressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/LZ4/LZ4CompressorOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/LZMA/LZMACompressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/LZMA/LZMACompressorOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/Zlib/ZlibCompressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/Zlib/ZlibCompressorOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/ZStd/ZStdCompressor.cs | |
| done | Assets/vFrame.Core/Runtime/Compression/Compressor/ZStd/ZStdCompressorOptions.cs | |
| done | Assets/vFrame.Core/Runtime/Containers/Component.cs | |
| done | Assets/vFrame.Core/Runtime/Containers/Container.cs | |
| done | Assets/vFrame.Core/Runtime/Containers/IComponent.cs | |
| done | Assets/vFrame.Core/Runtime/Containers/IContainer.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/Dispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/ICommand.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/ICommandDispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IDecision.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IDecisionDispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IDispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IEvent.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IEventDispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IRequest.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/IRequestDispatcher.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/ISubscription.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/Subscription.cs | |
| done | Assets/vFrame.Core/Runtime/Dispatchers/SubscriptionPool.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/Encryptor.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/EncryptorPool.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/EncryptorType.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/IEncryptor.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/PlainEncryptor.cs | |
| done | Assets/vFrame.Core/Runtime/Encryption/XOREncryptor.cs | |
| done | Assets/vFrame.Core/Runtime/Exceptions/ThrowHelper.cs | |
| done | Assets/vFrame.Core/Runtime/Exceptions/vFrameException.cs | |
| done | Assets/vFrame.Core/Runtime/Extensions/ArrayExtension.cs | |
| done | Assets/vFrame.Core/Runtime/Extensions/ByteExtension.cs | |
| done | Assets/vFrame.Core/Runtime/Extensions/ObjectExtension.cs | |
| done | Assets/vFrame.Core/Runtime/Extensions/StreamExtension.cs | |
| done | Assets/vFrame.Core/Runtime/Extensions/StringExtension.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Accessors/Accessor.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Box/Box.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Box/BoxAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Box/BoxPool.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/GCFreeAction/GCFreeCallback.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/GCFreeAction/Builtin/ActionCallback.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/RecycleOnDestroy/RecycleOnDestroy.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Singletons/Singleton.cs | |
| done | Assets/vFrame.Core/Runtime/Generic/Singletons/SingletonException.cs | |
| done | Assets/vFrame.Core/Runtime/Localize/ILocalization.cs | |
| done | Assets/vFrame.Core/Runtime/Localize/ILocalizationReader.cs | |
| done | Assets/vFrame.Core/Runtime/Localize/Localization.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/LogFormatType.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/Logger.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/LogLevelDef.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/LogTag.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/LogToFile.cs | |
| done | Assets/vFrame.Core/Runtime/Loggers/StackTraceUtility.cs | |
| done | Assets/vFrame.Core/Runtime/MultiThreading/IAsync.cs | |
| done | Assets/vFrame.Core/Runtime/MultiThreading/ITask.cs | |
| done | Assets/vFrame.Core/Runtime/MultiThreading/ParallelTaskRunner.cs | |
| done | Assets/vFrame.Core/Runtime/MultiThreading/Task.cs | |
| done | Assets/vFrame.Core/Runtime/MultiThreading/ThreadedTask.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/IObjectPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/IObjectPoolManager.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/IPoolObjectAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/IPoolObjectResetable.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/ObjectPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/ObjectPoolDiagnostics.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/ObjectPoolManager.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/DictionaryAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/DictionaryPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/HashSetAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/HashSetPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/ListAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/ListPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/QueueAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/QueuePool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/SortedDictionaryAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/SortedDictionaryPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/StackAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/StackPool.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/StringBuilderAllocator.cs | |
| done | Assets/vFrame.Core/Runtime/ObjectPools/Builtin/StringBuilderPool.cs | |
| done | Assets/vFrame.Core/Runtime/Profiles/PerfProfile.cs | |
| done | Assets/vFrame.Core/Runtime/Utils/EnumUtils.cs | |
| done | Assets/vFrame.Core/Runtime/Utils/MessageDigestUtils.cs | |
| done | Assets/vFrame.Core/Runtime/Utils/TimeUtils.cs | |

## vFrame.Core.Unity Runtime

| Status | File | Note |
|---|---|---|
| done | Assets/vFrame.Core.Unity/Runtime/Asynchronous/AsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Asynchronous/AsyncRequestCtrl.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Asynchronous/AsyncRequestException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Asynchronous/IAsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Asynchronous/IAsyncRequestCtrl.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutinePool.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutinePoolBehaviour.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutinePoolDebug.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutinePoolException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutineRunnerBehaviour.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutineState.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Coroutine/CoroutineTask.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/Agent/DownloadAgentBase.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/Agent/DownloadAgentUnityWebRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/Agent/DownloadAgentWebClient.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/Agent/IDownloadAgent.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/Downloader.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/DownloadEventArgs.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/DownloadSpeedCounter.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Download/DownloadTask.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Extensions/AnimationExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Extensions/ComponentExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Extensions/GameObjectExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Extensions/TransformExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Extensions/UI/RectTransformExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Generic/GCFreeAction/UnityActionCallback.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Generic/Singletons/MonoSingleton.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Loggers/LogLevelExtension.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Loggers/UnityLogger.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/AssetInfo.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/DownloadState.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/HashChecker.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/Manifest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/ManifestJson.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/PatchConst.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/Patcher.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/PatchOptions.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/UpdateEvent.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/UpdateState.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Patch/VersionManifest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Behaviours/PoolObjectIdentity.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Exceptions/AssetLoadFailedException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Exceptions/ObjectNotLoadedException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Exceptions/ObjectNotReadyException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Exceptions/SpawnPoolException.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/IPool.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/ISpawnPools.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/DefaultGameObjectLoader.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/DefaultGameObjectLoaderFactory.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/IGameObjectLoader.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/IGameObjectLoaderFactory.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/ILoadAsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/LoadAsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Loaders/LoadAsyncRequestOnLoaded.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Pool.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Preload/IPreloadAsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/Preload/PreloadAsyncRequest.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/SpawnPools.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/SpawnPoolsContext.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/SpawnPoolsDebug.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/SpawnPools/SpawnPoolsSettings.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Utils/PathUtils.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Utils/Unity/GameObjectUtils.cs | |
| done | Assets/vFrame.Core.Unity/Runtime/Utils/Unity/InputUtils.cs |
