// ------------------------------------------------------------
//         File: AnimationExtension.cs
//        Brief: Extension methods for Unity Animation component playback control.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-11 09:57:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections;
using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class AnimationExtension
    {
        /// <summary>
        ///     Rewinds the animation to the first frame, samples it, and stops playback.
        /// </summary>
        /// <param name="animation">The Animation component to reset.</param>
        public static void Reset(this Animation animation) {
            animation.Rewind();
            animation.Play();
            animation.Sample();
            animation.Stop();
        }

        /// <summary>
        ///     Advances the current clip to its last frame by setting normalized time to 1,
        ///     samples, and stops playback.
        /// </summary>
        /// <param name="animation">The Animation component to forward.</param>
        /// <returns><c>true</c> if the clip was successfully forwarded; <c>false</c> if no clip or state is available.</returns>
        public static bool ForwardToEnd(this Animation animation) {
            if (!animation.clip) {
                return false;
            }

            var state = animation[animation.clip.name];
            if (!state) {
                return false;
            }

            animation.Play();
            state.normalizedTime = 1f;
            animation.Sample();
            animation.Stop();

            return true;
        }

        /// <summary>
        ///     Plays the named animation clip and yields until playback finishes.
        /// </summary>
        /// <param name="animation">The Animation component to play.</param>
        /// <param name="name">The name of the clip to play.</param>
        /// <param name="reset">If <c>true</c>, resets the animation before playing.</param>
        /// <returns>An enumerator that completes when the animation stops playing.</returns>
        public static IEnumerator PlayUntilFinished(this Animation animation, string name, bool reset = true) {
            if (reset) {
                animation.Reset();
            }
            var clip = animation.GetClip(name);
            animation.clip = clip;
            animation.Play(name);
            yield return new WaitWhile(() => animation.isPlaying);
        }

        /// <summary>
        ///     Cross-fades to the named animation clip and yields until playback finishes.
        /// </summary>
        /// <param name="animation">The Animation component to cross-fade.</param>
        /// <param name="name">The name of the clip to cross-fade into.</param>
        /// <returns>An enumerator that completes when the animation stops playing.</returns>
        public static IEnumerator CrossFadeUntilFinished(this Animation animation, string name) {
            var clip = animation.GetClip(name);
            animation.clip = clip;
            animation.CrossFade(name);
            yield return new WaitWhile(() => animation.isPlaying);
        }

        /// <summary>
        ///     Waits until the current clip has finished playing by polling normalized time each frame.
        /// </summary>
        /// <param name="animation">The Animation component to monitor.</param>
        /// <returns>An enumerator that completes when the clip reaches normalized time >= 1.</returns>
        private static IEnumerator WaitUntilFinished(Animation animation) {
            while (true) {
                if (!animation.clip) {
                    yield break;
                }
                var state = animation[animation.clip.name];
                if (!state) {
                    yield break;
                }
                if (state.normalizedTime >= 1) {
                    yield break;
                }
                yield return null;
            }
        }
    }
}
