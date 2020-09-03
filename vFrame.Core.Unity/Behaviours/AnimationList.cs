//------------------------------------------------------------
//        File:  AnimationList.cs
//       Brief:  动画列表，用于设定一系列动画名称以及其对应的动画片段
//               方便代码中根据名称直接播放对应动画
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-03-22 14:44
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Extensions.UnityEngine;

namespace vFrame.Core.Behaviours
{
    [RequireComponent(typeof(Animation))]
    public class AnimationList : MonoBehaviour
    {
        [Serializable]
        public class AnimationSet
        {
            public string name;
            public AnimationClip clip;
        }

        [SerializeField] private List<AnimationSet> _animations = null;

        private Animation _animation;

        private void Awake() {
            _animation = GetComponent<Animation>();
        }

        /// <summary>
        /// 根据指定名称获取动画片段
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public AnimationClip GetAnimation(string animationName) {
            AnimationClip clip = null;
            foreach (var animationSet in _animations) {
                if (animationSet.name != animationName)
                    continue;
                clip = animationSet.clip;
                break;
            }

            return clip;
        }

        /// <summary>
        /// 根据指定名称播放动画片段
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public bool Play(string animationName) {
            var clip = GetAnimation(animationName);
            if(!clip)
                return false;
            _animation.Reset();
            return _animation.Play(clip.name);
        }

        /// <summary>
        /// 根据指定名称播放动画片段，直到动画播放完成
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public IEnumerator PlayUntilFinished(string animationName) {
            var clip = GetAnimation(animationName);
            if (clip)
                yield return _animation.PlayUntilFinished(clip.name);
        }

        /// <summary>
        /// 根据指定名称播放动画片段
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public void CrossFade(string animationName) {
            var clip = GetAnimation(animationName);
            if(!clip)
                return;
            _animation.CrossFade(clip.name);
        }

        /// <summary>
        /// 根据指定名称播放动画片段，直到动画播放完成
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public IEnumerator CrossFadeUntilFinished(string animationName) {
            var clip = GetAnimation(animationName);
            if (clip)
                yield return _animation.CrossFadeUntilFinished(clip.name);
        }
    }
}