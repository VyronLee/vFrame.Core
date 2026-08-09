// ------------------------------------------------------------
//         File: UniTaskAsyncExtensions.cs
//        Brief: Bridges Core's coroutine-driven IAsync to UniTask,
//               delivering awaitability + cancellation at the Unity
//               layer. Core itself stays Unity-free (server-safe).
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-08-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    ///     Bridges Core's coroutine-driven <see cref="IAsync"/> to
    ///     <see cref="UniTask"/>, so callers can <c>await</c> a Core async
    ///     operation. Core itself remains Unity-free (and server-side usable);
    ///     awaitability is delivered here, at the Unity layer.
    /// </summary>
    public static class UniTaskAsyncExtensions
    {
        /// <summary>
        ///     Pumps the coroutine <paramref name="op"/> via
        ///     <see cref="UniTask.Yield(CancellationToken)"/> until
        ///     <see cref="IAsync.IsDone"/>. A cancelled
        ///     <paramref name="cancellationToken"/> throws
        ///     <see cref="System.OperationCanceledException"/> without
        ///     requiring further frame pumps.
        /// </summary>
        /// <remarks>
        ///     A multi-frame pump requires the Unity PlayerLoop (PlayMode).
        ///     Cancelling an already-running op only stops the <em>await</em>;
        ///     the underlying coroutine is not forcibly aborted.
        /// </remarks>
        public static async UniTask ToUniTask(this IAsync op, CancellationToken cancellationToken = default)
        {
            while (!op.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!op.MoveNext())
                {
                    return;
                }

                await UniTask.Yield(cancellationToken);
            }
        }
    }
}
