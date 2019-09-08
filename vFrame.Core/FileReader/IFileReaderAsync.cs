//------------------------------------------------------------
//        File:  IFileReaderAsync.cs
//       Brief:  IFileReaderAsync
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-14 10:26
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using vFrame.Core.Base;

namespace vFrame.Core.FileReader
{
    public interface IFileReaderRequest : IAsync
    {
        byte[] GetBytes();
    }

    public interface IFileReaderAsync
    {
        IFileReaderRequest ReadAllBytesAsync(string path);
    }
}