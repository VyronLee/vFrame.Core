// ------------------------------------------------------------
//         File: TransformExtension.cs
//        Brief: Extension methods for Transform providing bulk
//               enable/disable, particle system control,
//               bounds calculation, and hierarchy queries.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-05-09 15:47:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vFrame.Core.Unity
{
    public static class TransformExtension
    {
        /// <summary>
        ///     Traverses the transform itself and all its children, invoking the
        ///     specified action on every component of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="transform">The root transform to traverse.</param>
        /// <param name="traveller">The action applied to each found component.</param>
        /// <typeparam name="T">The component type to search for.</typeparam>
        public static void TraverseSelfAndChildren<T>(this Transform transform, Action<T> traveller)
            where T : UnityEngine.Component {
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

        /// <summary>
        ///     Enables all components of type <typeparamref name="T"/> on the
        ///     transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        /// <typeparam name="T">The behaviour type to enable.</typeparam>
        public static void EnableComponents<T>(this Transform transform) where T : Behaviour {
            TraverseSelfAndChildren<T>(transform, v => v.enabled = true);
        }

        /// <summary>
        ///     Disables all components of type <typeparamref name="T"/> on the
        ///     transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        /// <typeparam name="T">The behaviour type to disable.</typeparam>
        public static void DisableComponents<T>(this Transform transform) where T : Behaviour {
            TraverseSelfAndChildren<T>(transform, v => v.enabled = false);
        }

        /// <summary>
        ///     Enables all <see cref="Renderer"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAllRenderer(this Transform transform) {
            TraverseSelfAndChildren<Renderer>(transform, v => v.enabled = true);
        }

        /// <summary>
        ///     Enables all <see cref="Renderer"/> components except those
        ///     matching the specified type.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        /// <param name="exceptType">Renderer subclass type to skip.</param>
        public static void EnableAllRenderer(this Transform transform, Type exceptType) {
            TraverseSelfAndChildren<Renderer>(transform, v => {
                if (exceptType == v.GetType()) {
                    return;
                }
                v.enabled = true;
            });
        }

        /// <summary>
        ///     Disables all <see cref="Renderer"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAllRenderer(this Transform transform) {
            TraverseSelfAndChildren<Renderer>(transform, v => v.enabled = false);
        }

        /// <summary>
        ///     Enables all <see cref="Graphic"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAllGraphic(this Transform transform) {
            TraverseSelfAndChildren<Graphic>(transform, v => v.enabled = true);
        }

        /// <summary>
        ///     Disables all <see cref="Graphic"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAllGraphic(this Transform transform) {
            TraverseSelfAndChildren<Graphic>(transform, v => v.enabled = false);
        }

        /// <summary>
        ///     Starts playing all <see cref="ParticleSystem"/> components on
        ///     the transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void StartAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform, v => v.Play());
        }

        /// <summary>
        ///     Stops all <see cref="ParticleSystem"/> components on the
        ///     transform and its children, clearing emitted particles.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void StopAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform,
                v => v.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear));
        }

        /// <summary>
        ///     Clears all particles from every <see cref="ParticleSystem"/>
        ///     on the transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void ClearAllParticleSystems(this Transform transform) {
            TraverseSelfAndChildren<ParticleSystem>(transform, v => { v.Clear(true); });
        }

        /// <summary>
        ///     Enables all <see cref="Animation"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAllAnimations(this Transform transform) {
            EnableComponents<Animation>(transform);
        }

        /// <summary>
        ///     Disables all <see cref="Animation"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAllAnimations(this Transform transform) {
            DisableComponents<Animation>(transform);
        }

        /// <summary>
        ///     Enables all <see cref="Animator"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAllAnimators(this Transform transform) {
            EnableComponents<Animator>(transform);
        }

        /// <summary>
        ///     Disables all <see cref="Animator"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAllAnimators(this Transform transform) {
            DisableComponents<Animator>(transform);
        }

        /// <summary>
        ///     Stops all <see cref="Animation"/> components on the transform
        ///     and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void StopAllAnimations(this Transform transform) {
            TraverseSelfAndChildren<Animation>(transform, v => v.Stop());
        }

        /// <summary>
        ///     Stops all <see cref="Animator"/> components by disabling them.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void StopAllAnimators(this Transform transform) {
            TraverseSelfAndChildren<Animator>(transform, v => v.enabled = false);
        }

        /// <summary>
        ///     Enables all <see cref="TrailRenderer"/> components on the
        ///     transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.enabled = true);
        }

        /// <summary>
        ///     Disables all <see cref="TrailRenderer"/> components on the
        ///     transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.enabled = false);
        }

        /// <summary>
        ///     Enables and clears all <see cref="TrailRenderer"/> components
        ///     on the transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void EnableAndClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = true;
                v.Clear();
            });
        }

        /// <summary>
        ///     Disables and clears all <see cref="TrailRenderer"/> components
        ///     on the transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void DisableAndClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => {
                v.enabled = false;
                v.Clear();
            });
        }

        /// <summary>
        ///     Clears all <see cref="TrailRenderer"/> components on the
        ///     transform and its children without changing their enabled state.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        public static void ClearAllTrailRenderers(this Transform transform) {
            TraverseSelfAndChildren<TrailRenderer>(transform, v => v.Clear());
        }

        /// <summary>
        ///     Calculates the bounding box that encloses all <see cref="Renderer"/>
        ///     components on the transform and its children.
        /// </summary>
        /// <param name="transform">The root transform.</param>
        /// <returns>A <see cref="Bounds"/> struct enclosing all renderers.</returns>
        public static Bounds CalculateBounds(this Transform transform) {
            var hasBounds = false;
            var bounds = new Bounds(transform.position, Vector3.zero);
            transform.TraverseSelfAndChildren<Renderer>(v => {
                var b = v.bounds;
                bounds.Encapsulate(b);
                hasBounds = true;
            });
            return hasBounds ? bounds : new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        ///     Returns the full hierarchy path of the transform, from root to
        ///     itself, separated by '/'.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <returns>A slash-separated path string.</returns>
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

        /// <summary>
        ///     Recursively counts all child transforms, including indirect
        ///     descendants.
        /// </summary>
        /// <param name="transform">The parent transform.</param>
        /// <returns>The total number of child transforms.</returns>
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