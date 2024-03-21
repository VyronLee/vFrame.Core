//------------------------------------------------------------
//        File:  AnimationExtension.cs
//       Brief:  动画组件扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-11 09:57
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections;
using UnityEngine;

namespace vFrame.Core.Unity.Extensions
{
    public static class AnimationExtension
    {
        /// <summary>
        ///     重置到动画开头
        /// </summary>
        /// <param name="animation"></param>
        public static void Reset(this Animation animation) {
            animation.Rewind();
            animation.Play();
            animation.Sample();
            animation.Stop();
        }

        /// <summary>
        ///     播放动画到最后一帧
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
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
        ///     播放动画，并一直等待到动画播放完成
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="name"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public static IEnumerator PlayUntilFinished(this Animation animation, string name, bool reset = true) {
            if (reset) {
                animation.Reset();
            }
            var clip = animation.GetClip(name);
            animation.clip = clip;
            animation.Play(name);
            yield return new WaitWhile(() => animation.isPlaying);
            //yield return WaitUntilFinished(animation); // Not working, why?
        }

        /// <summary>
        ///     播放动画，并一直等待到动画播放完成
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerator CrossFadeUntilFinished(this Animation animation, string name) {
            var clip = animation.GetClip(name);
            animation.clip = clip;
            animation.CrossFade(name);
            yield return new WaitWhile(() => animation.isPlaying);
            //yield return WaitUntilFinished(animation); // Not working, why?
        }

        /// <summary>
        ///     等待动画播放完成
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
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