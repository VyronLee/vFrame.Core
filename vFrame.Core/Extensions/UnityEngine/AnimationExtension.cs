//------------------------------------------------------------
//        File:  AnimationExtension.cs
//       Brief:  动画组件扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-02-11 09:57
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections;
using UnityEngine;

namespace vFrame.Core.Extensions.UnityEngine
{
    public static class AnimationExtension
    {
        /// <summary>
        /// 重置到动画开头
        /// </summary>
        /// <param name="animation"></param>
        public static void Reset(this Animation animation)
        {
            animation.Rewind();
            animation.Play();
            animation.Sample();
            animation.Stop();
        }

        /// <summary>
        /// 播放动画，并一直等待到动画播放完成
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerator WaitUntilPlayFinished(this Animation animation, string name)
        {
            var clip = animation.GetClip(name);
            animation.Reset();
            animation.Play(name);
            yield return new WaitForSeconds(clip.length);
        }
    }
}