//------------------------------------------------------------
//        File:  IFileReader.cs
//       Brief:  IFileReader
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-14 10:25
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

namespace vFrame.Core.FileReaders
{
    public interface IFileReader
    {
        byte[] ReadAllBytes(string path);

        string ReadAllText(string path);

        bool FileExist(string path);
    }
}