//------------------------------------------------------------
//        File:  TransformExtension.cs
//       Brief:  Transform扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-09 15:47
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace vFrame.Core.Extensions.UnityEngine
{
    public static class TransformExtension
    {
        public static void TravelSelfAndChildren<T>(this Transform transform, Action<T> traveller) where T : Component
        {
            var component = transform.GetComponent<T>();
            if (component)
                traveller.Invoke(component);

            var components = transform.GetComponentsInChildren<T>(true);
            foreach (var comp in components)
                traveller.Invoke(comp);
        }

        public static void EnableComponents<T>(this Transform transform) where T: Behaviour
        {
            TravelSelfAndChildren<T>(transform, v => v.enabled = true);
        }

        public static void DisableComponents<T>(this Transform transform) where T: Behaviour
        {
            TravelSelfAndChildren<T>(transform, v => v.enabled = false);
        }

        public static void EnableAllRenderer(this Transform transform)
        {
            TravelSelfAndChildren<Renderer>(transform, v => v.enabled = true);
        }

        public static void DisableAllRenderer(this Transform transform)
        {
            TravelSelfAndChildren<Renderer>(transform, v => v.enabled = false);
        }

        public static void EnableAllGraphic(this Transform transform)
        {
            TravelSelfAndChildren<Graphic>(transform, v => v.enabled = true);
        }

        public static void DisableAllGraphic(this Transform transform)
        {
            TravelSelfAndChildren<Graphic>(transform, v => v.enabled = false);
        }

        public static void StartAllParticleSystems(this Transform transform)
        {
            TravelSelfAndChildren<ParticleSystem>(transform, v => v.Play());
        }

        public static void StopAllParticleSystems(this Transform transform)
        {
            TravelSelfAndChildren<ParticleSystem>(transform,
                v => v.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
        }

        public static void EnableAllParticleSystems(this Transform transform)
        {
            TravelSelfAndChildren<ParticleSystem>(transform, v => v.gameObject.SetActive(true));
        }

        public static void DisableAllParticleSystems(this Transform transform)
        {
            TravelSelfAndChildren<ParticleSystem>(transform, v => v.gameObject.SetActive(false));
        }

        public static void EnableAllAnimations(this Transform transform)
        {
            EnableComponents<Animation>(transform);
        }

        public static void DisableAllAnimations(this Transform transform)
        {
            DisableComponents<Animation>(transform);
        }

        public static void EnableAllAnimators(this Transform transform)
        {
            EnableComponents<Animator>(transform);
        }

        public static void DisableAllAnimators(this Transform transform)
        {
            DisableComponents<Animator>(transform);
        }

        public static void StopAllAnimations(this Transform transform)
        {
            TravelSelfAndChildren<Animation>(transform, v => v.Stop());
        }

        public static void StopAllAnimators(this Transform transform)
        {
            TravelSelfAndChildren<Animator>(transform, v => v.enabled = false);
        }

    }
}

