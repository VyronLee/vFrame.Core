namespace vFrame.Core.Unity.TestRunner
{
    /// <summary>
    /// 薄入口：供 `Unity -batchmode -executeMethod
    /// vFrame.Core.Unity.TestRunner.CoreTestRunner.Run` 在进程内执行 vFrame.Core
    /// 的全部 EditMode 程序集（vFrame.Core.Tests.EditMode 与 vFrame.Core.Tests.EditMode.Unity）。
    /// 委托给 <see cref="HeadlessTestRunner.RunAllEditMode"/>，在进程内跑测试，
    /// 因此不受 Unity Test Protocol 端口（38000-38100）在本机被阻塞的影响。
    /// 与 vFrame.GameFramework 的 GameFrameworkTestRunner.Run 保持一致的每项目入口约定。
    /// </summary>
    public static class CoreTestRunner
    {
        public static void Run() => HeadlessTestRunner.RunAllEditMode();
    }
}
