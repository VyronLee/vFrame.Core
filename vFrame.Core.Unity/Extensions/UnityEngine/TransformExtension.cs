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
using vFrame.Core.Behaviours;

namespace vFrame.Core.Extensions.UnityEngine
{
    public static class TransformExtension
    {
        public static void TravelSelfAndChildren<T>(this Transform transform, Action<T> traveller) where T : Component {
            var component = transform.GetComponent<T>();
            if (component)
                traveller.Invoke(component);

            var components = transform.GetComponentsInChildren<T>(true);
            foreach (var comp in components)
                traveller.Invoke(comp);
        }

        public static void EnableComponents<T>(this Transform transform) where T : Behaviour {
            TravelSelfAndChildren<T>(transform, v => v.enabled = true);
        }

        public static void DisableComponents<T>(this Transform transform) where T : Behaviour {
            TravelSelfAndChildren<T>(transform, v => v.enabled = false);
        }

        public static void EnableAllRenderer(this Transform transform) {
            TravelSelfAndChildren<Renderer>(transform, v => v.enabled = true);
        }

        public static void EnableAllRenderer(this Transform transform, Type exceptType) {
            TravelSelfAndChildren<Renderer>(transform, v => {
                if (exceptType == v.GetType()) {
                    return;
                }
                v.enabled = true;
            });
        }

        public static void DisableAllRenderer(this Transform transform) {
            TravelSelfAndChildren<Renderer>(transform, v => v.enabled = false);
        }

        public static void EnableAllGraphic(this Transform transform) {
            TravelSelfAndChildren<Graphic>(transform, v => v.enabled = true);
        }

        public static void DisableAllGraphic(this Transform transform) {
            TravelSelfAndChildren<Graphic>(transform, v => v.enabled = false);
        }

        public static void StartAllParticleSystems(this Transform transform) {
            TravelSelfAndChildren<ParticleSystem>(transform, v => v.Play());
        }

        public static void StopAllParticleSystems(this Transform transform) {
            TravelSelfAndChildren<ParticleSystem>(transform,
                v => v.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
        }

        public static void ClearAllParticleSystems(this Transform transform) {
            TravelSelfAndChildren<ParticleSystem>(transform, v => {
                v.Clear(true);
            });
        }

        public static void EnableAllAnimations(this Transform transform) {
            EnableComponents<Animation>(transform);
        }

        public static void DisableAllAnimations(this Transform transform) {
            DisableComponents<Animation>(transform);
        }

        public static void EnableAllAnimators(this Transform transform) {
            EnableComponents<Animator>(transform);
        }

        public static void DisableAllAnimators(this Transform transform) {
            DisableComponents<Animator>(transform);
        }

        public static void StopAllAnimations(this Transform transform) {
            TravelSelfAndChildren<Animation>(transform, v => v.Stop());
        }

        public static void StopAllAnimators(this Transform transform) {
            TravelSelfAndChildren<Animator>(transform, v => v.enabled = false);
        }

        public static void EnableAllTrailRenderers(this Transform transform) {
            TravelSelfAndChildren<TrailRenderer>(transform, v => v.enabled = true);
        }

        public static void DisableAllTrailRenderers(this Transform transform) {
            TravelSelfAndChildren<TrailRenderer>(transform, v => v.enabled = false);
        }

        public static void EnableAndClearAllTrailRenderers(this Transform transform) {
            TravelSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = true;
                v.Clear();
            });
        }

        public static void DisableAndClearAllTrailRenderers(this Transform transform) {
            TravelSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = false;
                v.Clear();
            });
        }

        public static void ClearAllTrailRenderers(this Transform transform) {
            TravelSelfAndChildren<TrailRenderer>(transform, v => v.Clear());
        }

        public static void ClearAllTrailRendererEx(this Transform transform) {
            TravelSelfAndChildren<TrailRendererEx>(transform, v => v.Clear());
        }

        public static Bounds CalculateBounds(this Transform transform) {
            var bounds = new Bounds();
            transform.TravelSelfAndChildren<Transform>(v => {
                var renderer = v.GetComponent<Renderer>();
                if (null == renderer) {
                    return;
                }
                var scale = v.transform.lossyScale;
                var b = renderer.bounds;
                b.center = new Vector3(b.center.x * scale.x, b.center.y * scale.y, b.center.z * scale.z);
                b.extents = new Vector3(b.extents.x * scale.x, b.extents.y * scale.y, b.extents.z * scale.z);
                bounds.Encapsulate(b);
            });
            return bounds;
        }
    }
}