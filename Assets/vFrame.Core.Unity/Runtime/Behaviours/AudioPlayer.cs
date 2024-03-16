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

        public void ResetAudioSource() {
            if (!_audioSource) {
                return;
            }
            _audioSource.time = 0f;
            _audioSource.pitch = 1f;
        }

        public void Pause() {
            _isPause = true;
            if (_audioSource) {
                _audioSource.Pause();
            }
        }

        public void UnPause() {
            _isPause = false;
            if (_audioSource) {
                _audioSource.UnPause();
            }
        }

        public bool IsPause {
            get { return _isPause; }
        }

        public void Mute() {
            if (_audioSource) {
                _audioSource.mute = true;
            }
        }

        public void UnMute() {
            if (_audioSource) {
                _audioSource.mute = false;
            }
        }

        public float Volume {
            get {
                if (_audioSource) {
                    return _audioSource.volume;
                }
                return 0f;
            }
            set {
                if (_audioSource) {
                    _audioSource.volume = value;
                }
            }
        }

        public void SetClip(AudioClip clip) {
            if (_audioSource) {
                _audioSource.clip = clip;
            }
        }

        public bool IsPlaying {
            get {
                if (_audioSource) {
                    return _audioSource.isPlaying;
                }
                return false;
            }
        }

        public AudioSource Source {
            get { return _audioSource; }
        }

        public void Stop() {
            if (_audioSource) {
                _audioSource.Stop();
            }
            _isDirty = false;
            _isPause = false;
            _onPlayFinished = null;
        }

        public void Play() {
            if (!_audioSource || !_audioSource.clip) {
                Logger.Error("AudioClip not set.");
                return;
            }

            if (IsPlaying) {
                Stop();
            }
            _audioSource.Play();
        }

        public void Play(bool loop, float volume, Action onPlayFinished = null) {
            if (!_audioSource || !_audioSource.clip) {
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

        public void Play(bool loop, float volume, bool destroyWhenFinished) {
            if (!_audioSource || !_audioSource.clip) {
                Logger.Error("AudioClip not set.");
                return;
            }

            if (IsPlaying) {
                Stop();
            }

            if (destroyWhenFinished) {
                _onPlayFinished = () => { Destroy(gameObject); };
            }
            _audioSource.loop = loop;
            _audioSource.volume = volume;
            _audioSource.Play();
            _isDirty = true;
        }

        public void Play(AudioClip clip, bool loop, float volume, Action onPlayFinished = null) {
            if (!_audioSource) {
                return;
            }

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

            if (_isPause || !_audioSource || _audioSource.isPlaying)
                return;

            if (null != _onPlayFinished) {
                _onPlayFinished.Invoke();
                _onPlayFinished = null;
            }
            _isDirty = false;
        }
    }
}