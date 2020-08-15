using System.IO;
using vFrame.Core.FileSystems;
using vFrame.Core.Loggers;
using vFrame.Core.Profiles;

namespace ConsoleApplication1
{
    internal static class Program
    {
        private static readonly VFSPath FilePath = "Data/Lua";
        private static readonly VFSPath VPKPath = "Data/Data-encrypt.vpk";

        private static void TestLoadFromFiles() {
            var fileManager = new FileSystemManager();
            fileManager.Create();

            var fs = fileManager.AddFileSystem(FilePath);
            var files = fs.List();

            foreach (var file in files) {
                PerfProfile.Start(out var id);
                PerfProfile.Pin(file, id);
                fileManager.ReadAllText(file);
                PerfProfile.Unpin(id);
            }
        }

        private static void TestLoadFromVPK() {
            var fileManager = new FileSystemManager();
            fileManager.Create();

            var fs = fileManager.AddFileSystem(VPKPath);
            var files = fs.List();

            foreach (var file in files) {
                PerfProfile.Start(out var id);
                PerfProfile.Pin(file, id);
                fileManager.ReadAllText(file);
                PerfProfile.Unpin(id);
            }
        }

        public static void Main(string[] args) {
            Logger.LogLevel = LogLevelDef.Info;
            Logger.LogFormatMask = LogFormatType.Class | LogFormatType.Function;

            var log1 = new StreamWriter(File.OpenWrite("log1.txt"));
            void Log1Proc(Logger.LogContext context) {
                if (context.Tag.ToString() != "PerfProfile") {
                    return;
                }
                log1.WriteLine(context.Content);
            }

            var log2 = new StreamWriter(File.OpenWrite("log2.txt"));
            void Log2Proc(Logger.LogContext context) {
                if (context.Tag.ToString() != "PerfProfile") {
                    return;
                }
                log2.WriteLine(context.Content);
            }

            Logger.OnLogReceived += Log1Proc;

            PerfProfile.Start(out var id);
            PerfProfile.Pin("============= Load from files =============", id);
            TestLoadFromFiles();
            PerfProfile.Unpin(id);

            Logger.OnLogReceived -= Log1Proc;
            Logger.OnLogReceived += Log2Proc;

            PerfProfile.Start(out id);
            PerfProfile.Pin("============= Load from VPK ============", id);
            TestLoadFromVPK();
            PerfProfile.Unpin(id);

            log1.Dispose();
            log2.Dispose();
        }
    }
}