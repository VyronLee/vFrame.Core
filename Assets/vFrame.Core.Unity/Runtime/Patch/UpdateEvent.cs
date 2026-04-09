// ------------------------------------------------------------
//         File: UpdateEvent.cs
//        Brief: Event payload dispatched by the Patcher during the update lifecycle.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class UpdateEvent
    {
        public enum EventCode
        {
            /// <summary>
            ///     Local manifest file could not be found.
            /// </summary>
            ErrorNoLocalManifest,

            /// <summary>
            ///     Failed to download the version file.
            /// </summary>
            ErrorDownloadVersion,

            /// <summary>
            ///     Failed to parse the version file.
            /// </summary>
            ErrorParseVersion,

            /// <summary>
            ///     Failed to download the manifest file.
            /// </summary>
            ErrorDownloadManifest,

            /// <summary>
            ///     Failed to parse the manifest file.
            /// </summary>
            ErrorParseManifest,

            /// <summary>
            ///     A new asset version has been found on the server.
            /// </summary>
            NewAssetsVersionFound,

            /// <summary>
            ///     A new game (engine) version has been found; force update required.
            /// </summary>
            NewGameVersionFound,

            /// <summary>
            ///     Local assets are already up to date.
            /// </summary>
            AlreadyUpToDate,

            /// <summary>
            ///     Overall download progress has changed.
            /// </summary>
            UpdateProgression,

            /// <summary>
            ///     A single asset has been downloaded successfully.
            /// </summary>
            AssetUpdated,

            /// <summary>
            ///     One or more assets failed to download.
            /// </summary>
            ErrorDownloadFailed,

            /// <summary>
            ///     The update process has finished successfully.
            /// </summary>
            UpdateFinished,

            /// <summary>
            ///     The update process has failed.
            /// </summary>
            UpdateFailed,

            /// <summary>
            ///     Hash validation has started.
            /// </summary>
            HashStart,

            /// <summary>
            ///     Hash validation progress update.
            /// </summary>
            HashProgression,

            /// <summary>
            ///     Hash validation has detected mismatched files.
            /// </summary>
            HashValidationFailed
        }

        /// <summary>
        ///     Name of the asset related to this event.
        /// </summary>
        public string AssetName;

        /// <summary>
        ///     The event code describing what happened.
        /// </summary>
        public EventCode Code;

        /// <summary>
        ///     Total bytes downloaded so far.
        /// </summary>
        public ulong DownloadedSize;

        /// <summary>
        ///     Download progress as a ratio of downloaded bytes to total size.
        /// </summary>
        public float Percent;

        /// <summary>
        ///     Download progress as a ratio of completed files to total files.
        /// </summary>
        public float PercentByFile;
    }
}
