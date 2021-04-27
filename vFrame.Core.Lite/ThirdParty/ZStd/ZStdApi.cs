using System;
using System.Runtime.InteropServices;

namespace vFrame.Core.ThirdParty.ZStd
{
    public static class ZStdApi
    {
#if (UNITY_IOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string ZSTDDLL = "__Internal";
#else
        private const string ZSTDDLL = "zstd";
#endif

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int decompress([Out] byte[] dst, int dstSize, [In] byte[] src, int srcSize);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int decompressDictionary([In] [Out] DecompressContext context, [Out] byte[] dst,
            int dstSize, [In] byte[] src, int srcSize);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int decompressStream([In] [Out] DecompressContext context, [Out] byte[] dst, int dstSize,
            [In] byte[] src, int srcSize);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int decompressStreamDictionary([In] [Out] DecompressContext context, [Out] byte[] dst,
            int dstSize, [In] byte[] src, int srcSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DecompressContext : IDisposable
    {
#if (UNITY_IOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string ZSTDDLL = "__Internal";
#else
        private const string ZSTDDLL = "zstd";
#endif

        public static DecompressContext CreateWithStream() {
            var context = new DecompressContext();
            initializeDecompressContext(context);
            return context;
        }

        public static DecompressContext CreateWithDictionary(byte[] dict) {
            var context = new DecompressContext();
            initializeDecompressContextDictionary(context, dict, dict.Length);
            return context;
        }

        public static DecompressContext CreateWithStreamDictionary(byte[] dict) {
            var context = new DecompressContext();
            initializeDecompressContextStreamDictionary(context, dict, dict.Length);
            return context;
        }

        public IntPtr dctx;
        public IntPtr ddict;
        public IntPtr dstream;
        public int totalReadSize;

        public void Dispose() {
            finalizeDecompressContext(this);
        }

        private DecompressContext() {
            dctx = default;
            ddict = default;
            dstream = default;
            totalReadSize = 0;
        }

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void initializeDecompressContext([In] [Out] DecompressContext context);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void initializeDecompressContextDictionary([In] [Out] DecompressContext context,
            [In] byte[] dict, int dictSize);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void initializeDecompressContextStreamDictionary([In] [Out] DecompressContext context,
            [In] byte[] dict, int dictSize);

        [DllImport(ZSTDDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void finalizeDecompressContext([In] [Out] DecompressContext context);
    }
}