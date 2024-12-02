using System.Collections.Generic;
using Cinemachine;
using Core.Behaviour;
using MEC;
using Sirenix.OdinInspector;
using Tools.Feedback.Core;
using UnityEngine;
using Utils.ObjectId;

namespace Tools.Feedback.Camera {
    public struct YisoCameraShakeEvent {
        private static event Delegate OnEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization() {
            OnEvent = null;
        }

        public static void Register(Delegate callback) {
            OnEvent += callback;
        }

        public static void Unregister(Delegate callback) {
            OnEvent -= callback;
        }

        public delegate void Delegate(ShakeItem shakeItem);

        public static void Trigger(ShakeItem shakeItem) {
            OnEvent?.Invoke(shakeItem);
        }
    }

    public struct ShakeItem {
        public float duration;
        public float amplitude;
        public float frequency;
        public bool infinite;
        public bool useUnscaledTime;

        public ShakeItem(float duration, float amplitude, float frequency, bool infinite = false, bool useUnscaledTime = false) {
            this.duration = duration;
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.infinite = infinite;
            this.useUnscaledTime = useUnscaledTime;
        }
    }
    
    // TODO: Yiso Feedback으로 통합하기
    [AddComponentMenu("Yiso/Feedbacks/Camera/CameraShaker")]
    public class YisoCameraShaker : RunIBehaviour {
        [SerializeField] private int channel = 0;
        [SerializeField] private float defaultShakeAmplitude = .5f;
        [SerializeField] private float defaultShakeFrequency = 10f;
        [SerializeField] [ReadOnly] private float idleAmplitude;
        [SerializeField] [ReadOnly] private float idleFrequency = 1f;
        [SerializeField] private float lerpSpeed = 5f;

        [BoxGroup("Test"), SerializeField] private float testDuration = 0.3f;
        [BoxGroup("Test"), SerializeField] private float testAmplitude = 2f;
        [BoxGroup("Test"), SerializeField] private float testFrequency = 20f;

        [BoxGroup("Test"), Button]
        public void TestShake() {
            ShakeCamera(testDuration, testAmplitude, testFrequency, false);
        }

        public float GetTime() => timeScaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;

        public float GetDeltaTime() => timeScaleMode == TimescaleModes.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;

        private TimescaleModes timeScaleMode;
        private CinemachineBasicMultiChannelPerlin perlin;
        private CinemachineVirtualCamera virtualCamera;
        private float targetAmplitude;
        private float targetFrequency;
        private string tag;
        private bool isShaking = false;
        private bool initialized = false;

        protected override void Start() {
            Initialization();
        }

        public virtual void Initialization() {
            if (initialized) return;
            
            virtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            tag = YisoObjectID.GenerateString();

            if (perlin != null) {
                idleAmplitude = perlin.m_AmplitudeGain;
                idleFrequency = perlin.m_FrequencyGain;

                targetAmplitude = idleAmplitude;
                targetFrequency = idleFrequency;
            
                initialized = true;
            }
            else {
                initialized = false;
            }
        }

        public override void OnUpdate() {
            if (perlin == null) return;
            perlin.m_AmplitudeGain = targetAmplitude;
            perlin.m_FrequencyGain =
                Mathf.Lerp(perlin.m_FrequencyGain, targetFrequency, GetDeltaTime() * lerpSpeed);
        }

        private void ShakeCamera(float duration, bool infinite, bool useUnscaledTime = false) {
            ShakeCamera(duration, defaultShakeAmplitude, defaultShakeFrequency, infinite, useUnscaledTime);
        }

        private void ShakeCamera(float duration, float amplitude, float frequency, bool infinite,
            bool useUnscaledTime = false) {
            if (isShaking) {
                StopShake();
            }

            Timing.RunCoroutine(
                DOShakeCamera(duration, amplitude, frequency, infinite, useUnscaledTime).CancelWith(gameObject), tag);
        }

        private void StopShake() {
            if (!isShaking) return;
            Timing.KillCoroutines(tag);
        }

        private IEnumerator<float> DOShakeCamera(float duration, float amplitude, float frequency, bool infinite,
            bool useUnscaledTime) {
            isShaking = true;
            targetAmplitude = amplitude;
            targetFrequency = frequency;
            timeScaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
            if (!infinite) {
                yield return Timing.WaitForSeconds(duration);
                CameraReset();
                isShaking = false;
            }
        }

        private void CameraReset() {
            targetAmplitude = idleAmplitude;
            targetFrequency = idleFrequency;
        }

        private void HandleShakeItem(ShakeItem item) {
            var amplitude = item.amplitude <= 0 ? defaultShakeAmplitude : item.amplitude;
            var frequency = item.frequency <= 0 ? defaultShakeFrequency : item.frequency;
            ShakeCamera(item.duration, amplitude, frequency, item.infinite, item.useUnscaledTime);
        }

        protected override void OnEnable() {
            base.OnEnable();
            YisoCameraShakeEvent.Register(HandleShakeItem);
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoCameraShakeEvent.Unregister(HandleShakeItem);
        }
    }
}