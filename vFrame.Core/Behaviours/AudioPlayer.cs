//------------------------------------------------------------
//        File:  AudioPlayer.cs
//       Brief:  音乐播放器简单封装
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-11-07 20:19
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================
using System;
using UnityEngine;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Behaviours
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;
        private bool _isPause;
        private Action _onPlayFinished;
        private bool _isDirty;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Pause() {
            _isPause = true;
            _audioSource.Pause();
        }

        public void UnPause() {
            _isPause = false;
            _audioSource.UnPause();
        }

        public bool IsPause {
            get { return _isPause; }
        }

        public void Mute() {
            _audioSource.mute = true;
        }

        public void UnMute() {
            _audioSource.mute = false;
        }

        public float Volume {
            get { return _audioSource.volume; }
            set { _audioSource.volume = value; }
        }

        public bool IsPlaying {
            get { return _audioSource.isPlaying; }
        }

        public AudioSource Source {
            get { return _audioSource; }
        }

        public void Stop() {
            _audioSource.Stop();
            _isDirty = false;
            _isPause = false;
            _onPlayFinished = null;
        }

        public void Play() {
            if (null == _audioSource.clip) {
                Logger.Error("AudioClip not set.");
                return;
            }

            if (IsPlaying) {
                Stop();
            }
            _audioSource.Play();
        }

        public void Play(bool loop, float volume, Action onPlayFinished = null) {
            if (null == _audioSource.clip) {
                Logger.Error("AudioClip not set.");
                return;
            }

            if (IsPlaying) {
                Stop();
            }

            _onPlayFinished = onPlayFinished;
            _audioSource.loop = loop;
            _audioSource.volume = volume;
            _audioSource.Play();
            _isDirty = true;
        }

        public void Play(AudioClip clip, bool loop, float volume, Action onPlayFinished = null) {
            if (IsPlaying) {
                Stop();
            }

            _onPlayFinished = onPlayFinished;
            _audioSource.clip = clip;
            _audioSource.loop = loop;
            _audioSource.volume = volume;
            _audioSource.Play();
            _isDirty = true;
        }

        private void Update() {
            if (!_isDirty) {
                return;
            }

            if (_isPause || _audioSource.isPlaying)
                return;

            if (null != _onPlayFinished) {
                _onPlayFinished.Invoke();
                _onPlayFinished = null;
            }
            _isDirty = false;
        }
    }
}