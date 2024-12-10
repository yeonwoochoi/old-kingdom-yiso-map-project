using System.Collections;
using Character.Core;
using Cinemachine;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;

namespace Camera {
    [AddComponentMenu("Yiso/Camera/CinematicCameraController")]
    public class YisoCinematicCameraController : RunIBehaviour, IYisoEventListener<YisoCameraEvent> {
        public bool followPlayer = true;
        public bool confineCameraToLevelBounds = true;
        public bool listenToSetConfinerEvents = true;
        [ReadOnly] public YisoCharacter targetCharacter;

        protected CinemachineVirtualCamera virtualCamera;
        protected CinemachineConfiner confiner2D;
        protected float prevOrthoSize = 5f;
        protected bool isFollowing = false;
        
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCinematicCameraController>();

        #region Initialization

        protected override void Awake() {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            confiner2D = GetOrAddComponent<CinemachineConfiner>();
            confiner2D.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
            confiner2D.m_ConfineScreenEdges = true;
        }

        protected override void Start() {
            if (confiner2D == null || !confineCameraToLevelBounds) return;
            if (GameManager.HasInstance) {
                confiner2D.m_BoundingShape2D = GameManager.Instance.CurrentMapController?.CurrentCameraBoundary;
            }
        }

        #endregion

        #region Event

        public virtual void SetTarget(YisoCharacter character) {
            targetCharacter = character;
        }

        public virtual void StartFollowing() {
            if (!followPlayer || targetCharacter == null) return;
            isFollowing = true;
            virtualCamera.Follow = targetCharacter.CameraTarget.transform;
            virtualCamera.enabled = true;
        }

        public virtual void StopFollowing() {
            if (!followPlayer) return;
            isFollowing = false;
            virtualCamera.Follow = null;
            virtualCamera.enabled = false;
        }

        protected virtual IEnumerator RefreshPosition() {
            virtualCamera.enabled = false;
            yield return null;
            StartFollowing();
        }

        #endregion


        public void OnEvent(YisoCameraEvent cameraEvent) {
            switch (cameraEvent.eventType) {
                case YisoCameraEventTypes.SetTargetCharacter:
                    if (cameraEvent.targetCharacter != null) {
                        SetTarget(cameraEvent.targetCharacter);
                    }

                    break;
                case YisoCameraEventTypes.SetConfiner:
                    if (confiner2D != null && listenToSetConfinerEvents) {
                        confiner2D.m_BoundingShape2D = cameraEvent.bounds;
                    }

                    break;
                case YisoCameraEventTypes.StartFollowing:
                    if (targetCharacter == null) {
                        LogService.Warn("[YisoCinematicCameraController] Target character for the camera to follow is null");
                        return;
                    }

                    StartFollowing();
                    break;
                case YisoCameraEventTypes.StopFollowing:
                    StopFollowing();
                    break;
                case YisoCameraEventTypes.RefreshPosition:
                    StartCoroutine(RefreshPosition());
                    break;
                case YisoCameraEventTypes.ResetPriorities:
                    virtualCamera.Priority = 0;
                    break;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            this.YisoEventStartListening();
        }

        protected override void OnDisable() {
            base.OnDisable();
            this.YisoEventStopListening();
        }
    }
}