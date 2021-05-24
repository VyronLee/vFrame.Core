//------------------------------------------------------------
//        File:  PackageStruct.cs
//       Brief:  Package structures.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2020-03-11 16:42
//   Copyright:  Copyright (c) 2020, VyronLee
//============================================================

namespace vFrame.Core.FileSystems.Package
{
    public struct PackageHeader
    {
        public long Id;
        public long Version;
        public long Size;
        public long FileListOffset;
        public long FileListSize;
        public long BlockTableOffset;
        public long BlockTableSize;
        public long BlockOffset;
        public long Reserved;

        // 72 bytes
        public static int GetMarshalSize() {
            return sizeof(long) * 9;
        }
    }

    public struct PackageBlockInfo
    {
        public long Flags;
        public long Offset;
        public long OriginalSize;
        public long CompressedSize;
        public long EncryptKey;

        // 40 bytes
        public static int GetMarshalSize() {
            return sizeof(long) * 5;
        }

        // ==================================
        // Not save
        public long OpFlags;
    }
}