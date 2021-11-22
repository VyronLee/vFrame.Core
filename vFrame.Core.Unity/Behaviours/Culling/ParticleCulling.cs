//------------------------------------------------------------
//        File:  ParticleCulling.cs
//       Brief:  粒子剔除
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-10 17:27
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vFrame.Core.Behaviours.Culling
{
    public class ParticleCulling : CullingBehaviour
    {
        private List<ParticleSystem> _particleSystems;
        private List<Renderer> _renderer;

        protected override void Awake() {
            base.Awake();

            _particleSystems = GetComponentsInChildren<ParticleSystem>().ToList();
            var ps = GetComponent<ParticleSystem>();
            if (ps)
                _particleSystems.Add(ps);

            _renderer = GetComponentsInChildren<Renderer>().ToList();
            var render = GetComponent<Renderer>();
            if (render)
                _renderer.Add(render);
        }

        protected override void OnBecameVisible() {
            _particleSystems.ForEach(ps => ps.Play());
            _renderer.ForEach(rd => rd.enabled = true);
        }

        protected override void OnBecameInvisible() {
            _particleSystems.ForEach(ps => ps.Stop());
            _renderer.ForEach(rd => rd.enabled = false);
        }
    }
}