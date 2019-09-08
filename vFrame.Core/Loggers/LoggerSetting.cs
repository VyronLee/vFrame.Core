//------------------------------------------------------------
//        File:  LoggerSetting.cs
//       Brief:  日志设置
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2018-10-20 18:10
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================
namespace Kernel.Log
{
    public static class LoggerSetting
    {
        public const int DefaultCapacity = 1000;
        public const string DefaultTagFormatter = "#{0}#";
        public const float NotifierDisplayDuration = 3.0f;
        
#if UNITY_EDITOR || UNITY_STANDALONE
        public const float TouchTimeTotal = 20f;
        public static readonly int[] TouchQudrantRequired = {1, 2, 3, 4};
#else
        public const float TouchTimeTotal = 2f;
        public static readonly int[] TouchQudrantRequired = {1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4};
#endif
        
    }
}