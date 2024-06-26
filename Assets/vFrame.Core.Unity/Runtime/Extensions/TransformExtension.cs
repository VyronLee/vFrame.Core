//------------------------------------------------------------
//        File:  TransformExtension.cs
//       Brief:  Transform扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-05-09 15:47
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using vFrame.Core.ObjectPools.Builtin;

namespace vFrame.Core.Unity.Extensions
{
    public static class TransformExtension
    {
        public static void TraverseSelfAndChildren<T>(this Transform transform, Action<T> traveller)
            where T : Component {
            var buffer = ListPool<T>.Shared.Get();
            transform.GetComponentsInChildren(true, buffer);
            try {
                foreach (var comp in buffer) {
                    traveller.Invoke(comp);
                }
            }
            finally {
                ListPool<T>.Shared.Return(buffer);
            }
        }

        public static void EnableComponents<T>(this Transform transform) where T : Behaviour {
            TraverseSelfAndChildren<T>(transform, v => v.enabled = true);
        }

        public static void DisableComponents<T>(this Transform transform) where T : Behaviour {
            TraverseSelfAndChildren<T>(transform, v => v.enabled = false);
        }

        public static void EnableAllRenderer(this Transform transform) {
            TraverseSelfAndChildren<Renderer>(transform, v => v.enabled = true);
        }

        public static void EnableAllRenderer(this Transform transform, Type exceptType) {
            TraverseSelfAndChildren<Renderer>(transform, v => {
                if (exceptType == v.GetType()) {
                    return;
                }
                v.enabled = true;
            });
        }

        public static void DisableAllRenderer(this Transform transform) {
            TraverseSelfAndChildren<Renderer>(transform, v => v.enabled = false);
        }

        public static void EnableAllGraphic(this Transform transform) {
            TraverseSelfAndChildren<Graphic>(transform, v => v.enabled = true);
        }

        public static void DisableAllGraphic(this Transform transform) {
            TraverseSelfAndChildren<Graphic>(transform, v => v.enabled = false);
        }

        public static void StartAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform, v => v.Play());
        }

        public static void StopAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform,
                v => v.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
        }

        public static void ClearAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform, v => { v.Clear(true); });
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
            TraverseSelfAndChildren<Animation>(transform, v => v.Stop());
        }

        public static void StopAllAnimators(this Transform transform) {
            TraverseSelfAndChildren<Animator>(transform, v => v.enabled = false);
        }

        public static void EnableAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.enabled = true);
        }

        public static void DisableAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.enabled = false);
        }

        public static void EnableAndClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = true;
                v.Clear();
            });
        }

        public static void DisableAndClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = false;
                v.Clear();
            });
        }

        public static void ClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.Clear());
        }

        public static Bounds CalculateBounds(this Transform transform) {
            var bounds = new Bounds();
            transform.TraverseSelfAndChildren<Transform>(v => {
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

        public static string GetHierarchyPath(this Transform transform) {
            var names = new List<string>();

            do {
                names.Add(transform.name);
                transform = transform.parent;
            } while (transform);

            names.Reverse();
            var path = string.Join("/", names);
            return path;
        }

        public static int GetAllChildrenCount(this Transform transform) {
            var ret = 0;
            ret += transform.childCount;
            for (var i = 0; i < transform.childCount; i++) {
                ret += GetAllChildrenCount(transform.GetChild(i));
            }
            return ret;
        }
    }
}