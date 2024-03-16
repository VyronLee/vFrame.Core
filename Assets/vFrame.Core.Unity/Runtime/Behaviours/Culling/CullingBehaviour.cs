//------------------------------------------------------------
//        File:  CullingBehaviour.cs
//       Brief:  剔除
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-04-29 14:58
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace vFrame.Core.Behaviours.Culling
{
    [ExecuteInEditMode]
    public abstract class CullingBehaviour : MonoBehaviour
    {
        private CullingGroup _cullingGroup;

        [SerializeField]
        private Camera _targetCamera;

        [SerializeField]
        private float _cullingRadius = 1;
        [SerializeField]
        private Vector3 _offset;

        private readonly BoundingSphere[] _boundingSpheres = { new BoundingSphere(Vector3.zero, 1) };

        public event Action<bool> onCullingStateChanged;

        /// <summary>
        /// 启动
        /// </summary>
        protected virtual void Awake() {

        }

        /// <summary>
        /// 启用
        /// </summary>
        private void OnEnable() {
            _cullingGroup = new CullingGroup();
            _cullingGroup.SetBoundingSpheres(_boundingSpheres);
            _cullingGroup.SetBoundingSphereCount(1);
            _cullingGroup.onStateChanged += OnStateChanged;
        }

        /// <summary>
        /// 禁用
        /// </summary>
        private void OnDisable() {
            Reset();
        }

        /// <summary>
        /// 开始
        /// </summary>
        private void Start() {
#if !UNITY_EDITOR
            // 默认隐藏，由CullingGroup触发更新
            OnBecameInvisible();
#endif
            // 更新剔除组信息
            UpdateCullingGroup();
        }

        /// <summary>
        /// 设置剔除使用的相机
        /// </summary>
        public Camera TargetCamera {
            set { _targetCamera = value; }
            get { return _targetCamera ? _targetCamera : Camera.main; }
        }

        /// <summary>
        /// 自动更新包围盒位置
        /// </summary>
        public bool AutoUpdate { set; get; } = true;

        /// <summary>
        /// 是否可见
        /// </summary>
        /// <returns></returns>
        public bool IsVisible() {
            return _cullingGroup.IsVisible(0);
        }

        /// <summary>
        /// 更新剔除区域
        /// </summary>
        public void UpdateCullingGroup() {
            _boundingSpheres[0] = new BoundingSphere(transform.TransformPoint(_offset), _cullingRadius);

            if (_cullingGroup == null)
                return;

            _cullingGroup.targetCamera = TargetCamera;
            _cullingGroup.SetBoundingSpheres(_boundingSpheres);
        }

        private void Update() {
#if !UNITY_EDITOR
            if (!AutoUpdate)
                return;
#endif
            if (!Application.isPlaying) {
                return;
            }
            UpdateCullingGroup();
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public void UpdateCullingState() {
            if (IsVisible())
                OnBecameVisible();
            else
                OnBecameInvisible();
        }

        /// <summary>
        /// 可见性变更处理
        /// </summary>
        /// <param name="sphere"></param>
        private void OnStateChanged(CullingGroupEvent sphere) {
            if (sphere.isVisible)
                OnBecameVisible();
            else
                OnBecameInvisible();

            if (null != onCullingStateChanged)
                onCullingStateChanged.Invoke(!sphere.isVisible);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        private void OnDestroy() {
            Reset();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset() {
            if (_cullingGroup != null)
                _cullingGroup.Dispose();
            _cullingGroup = null;
        }

        protected virtual void OnBecameVisible() {
        }

        protected virtual void OnBecameInvisible() {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(_boundingSpheres[0].position, _cullingRadius);
        }

        private void OnValidate() {
            UpdateCullingGroup();
        }
#endif
    }
}