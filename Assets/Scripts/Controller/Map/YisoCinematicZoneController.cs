using System.Collections;
using System.Collections.Generic;
using Camera;
using Character.Core;
using Cinemachine;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Manager;
using Sirenix.OdinInspector;
using Tools.Feedback.Camera;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Utils.Beagle;

namespace Controller.Map {
    /// <summary>
    /// Camera Control을 위해 정의된 Cinematic Zone의 Controller
    /// Orthographic Size 변경
    /// Camera Switching (with setting confiner & path finding)
    /// </summary>
    [AddComponentMenu("Yiso/Controller/Map/CinematicZoneController")]
    public class YisoCinematicZoneController : RunIBehaviour {
        [Title("Settings")] public bool cameraStartActive = false; // Play할때 Active상태인 Camera인지
        public CinemachineVirtualCamera virtualCamera;

        [Title("Confiner")]
        // Map Controller Confiner : Map 전체에서 작동하는 Player Camera (Default)의 Confiner로 등록됨
        // Cinematic Zone Confiner : Cinematic Zone마다 존재하는 Virtual Camera의 Confiner로 등록됨
        public bool useConfiner = true;

        [Title("Transition")] public float transitionTime = 1f;
        public float switchCooldown = 1f;

        [Title("Collisions")] public LayerMask targetLayerMask = LayerManager.PlayerLayerMask;

        [Title("Activation")] public List<GameObject> activationList; // 해당 Zone에 들어갈때 Enemy나 Item Box를 스폰하고 싶으면..

        [Title("Zoom")] public bool useCinematicZoom = false;
        [ShowIf("useCinematicZoom")] public float orthographicSize = 5f;

        [Title("Shaker")] public bool useCameraShaker = true; // TODO: 임시

        [Title("Events")]
        /// a UnityEvent to trigger when entering the zone for the first time
        [Tooltip("a UnityEvent to trigger when entering the zone for the first time")]
        public UnityEvent onEnterZoneForTheFirstTimeEvent;

        /// a UnityEvent to trigger when entering the zone
        [Tooltip("a UnityEvent to trigger when entering the zone")]
        public UnityEvent onEnterZoneEvent;

        /// a UnityEvent to trigger when exiting the zone
        [Tooltip("a UnityEvent to trigger when exiting the zone")]
        public UnityEvent onExitZoneEvent;

        public bool CurrentZone { get; protected set; } = false;
        public bool IsVisited { get; protected set; } = false;
        public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoCinematicZoneController>();

        protected UnityEngine.Camera mainCamera;
        protected CinemachineBrain cinemachineBrain;
        protected YisoCinematicCameraController cinematicCameraController;
        protected CinemachineConfiner cinemachineConfiner;

        protected GameObject confinerGameObject;
        protected Rigidbody2D confinerRigidbody2D;
        protected CompositeCollider2D confinerCompositeCollider2D;

        protected BoxCollider2D boxCollider2D;
        protected PolygonCollider2D polygonCollider2D;

        protected Coroutine activateZoneObjectsCoroutine;
        protected Coroutine inactivateZoneObjectsCoroutine;

        protected YisoCameraShaker cameraShaker; // TODO: 임시

        private float switchTimer = 0f; // Hysteresis 적용
        private bool isVisiting = false;

        #region Initialization

        protected override void Awake() {
            InitializeCollider();
            if (!Application.isPlaying) return;
            Initialization();
        }

        protected virtual void InitializeCollider() {
            var collider2D = GetComponent<Collider2D>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            polygonCollider2D = GetComponent<PolygonCollider2D>();
            collider2D.isTrigger = true;
        }

        protected virtual void Initialization() {
            // Find Main Camera
            if (mainCamera == null) {
                mainCamera = UnityEngine.Camera.main;
            }

            if (mainCamera == null) {
                LogService.Warn($"[CinematicZoneController] {name} : Main Camera is not found.");
                return;
            }

            // Find Cinemachine Brain
            cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null) {
                LogService.Warn(
                    $"[CinematicZoneController] {name} : CinemachineBrain component is not found on the main camera.");
                return;
            }

            // Find Zone Virtual Camera
            if (virtualCamera == null) {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            }

            if (virtualCamera == null) {
                LogService.Warn(
                    $"[CinematicZoneController] {name} : no virtual camera is attached to this zone. Set one in its inspector.");
            }

            // Set up Confiner
            if (useConfiner) {
                SetupConfinerGameObject();
            }

            // Set Cinematic Camera Controller
            cinematicCameraController = virtualCamera.gameObject.GetOrAddComponent<YisoCinematicCameraController>();
            cinematicCameraController.confineCameraToLevelBounds =
                !useConfiner; // Zone마다 있는 Confiner를 사용할 것이기 때문 (맵 전체 Confiner를 사용하지 않는다는 뜻)
            cinematicCameraController.listenToSetConfinerEvents =
                !useConfiner; // Zone에 있는 Confiner 사용할 것이기 때문 (Start 이후 Set Confiner camera event Listening 할건지)  

            // Set Orthographic Size
            if (useCinematicZoom) virtualCamera.m_Lens.OrthographicSize = orthographicSize;
            
            // TODO: 임시 나중에 Feedback으로 통합
            if (useCameraShaker) InitializeCameraShaker();
        }

        /// <summary>
        /// TODO: 임시
        /// </summary>
        protected virtual void InitializeCameraShaker() {
            cameraShaker = virtualCamera.gameObject.GetComponent<YisoCameraShaker>(); 
            if (cameraShaker == null) {
                cameraShaker = virtualCamera.gameObject.AddComponent<YisoCameraShaker>();
            }
            
            var perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlinNoise == null) {
                perlinNoise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            // 6D Shake Noise Profile 설정 (Assets에 6D Shake Noise Profile이 있다고 가정)
            var noiseProfile = Resources.Load<NoiseSettings>("Camera/6DShake"); // 해당 Noise Profile의 경로에 맞게 수정
            perlinNoise.m_NoiseProfile = noiseProfile;
            
            // Amplitude Gain과 Frequency Gain 설정
            perlinNoise.m_AmplitudeGain = 0f;
            perlinNoise.m_FrequencyGain = 5f;
            
            cameraShaker.Initialization();
        }

        protected override void Start() {
            if (!Application.isPlaying) return;
            if (useConfiner) SetupConfiner();
            StartCoroutine(EnableCamera(cameraStartActive, 1));
        }

        protected virtual void SetupConfinerGameObject() {
            // we remove the object if needed
            var child = transform.Find("Confiner"); // Confiner 오브젝트가 하위에 있는지 체크
            if (child != null) {
                DestroyImmediate(child.gameObject);
            }

            // we create an empty child object
            confinerGameObject = new GameObject("Confiner");
            confinerGameObject.transform.localPosition = Vector3.zero;
            confinerGameObject.transform.SetParent(this.transform);
        }

        protected virtual void SetupConfiner() {
            // we add a rigidbody2D to it and set it up
            confinerRigidbody2D = confinerGameObject.AddComponent<Rigidbody2D>();
            confinerRigidbody2D.bodyType = RigidbodyType2D.Static;
            confinerRigidbody2D.simulated = false;
            confinerRigidbody2D.useAutoMass = true;
            confinerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;

            // we copy the collider and set it up
            CopyCollider();
            confinerGameObject.transform.localPosition = Vector3.zero;

            // we reset these settings, set differently initially to avoid a weird Unity warning
            confinerRigidbody2D.bodyType = RigidbodyType2D.Static;
            confinerRigidbody2D.useAutoMass = false;

            // we add a composite collider 2D and set it up
            confinerCompositeCollider2D = confinerGameObject.AddComponent<CompositeCollider2D>();
            confinerCompositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
            confinerCompositeCollider2D.isTrigger = true;

            // we set the composite collider as the virtual camera's confiner
            cinemachineConfiner = virtualCamera.gameObject.GetOrAddComponent<CinemachineConfiner>();
            cinemachineConfiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
            cinemachineConfiner.m_ConfineScreenEdges = true;
            cinemachineConfiner.m_BoundingShape2D = confinerCompositeCollider2D;
        }

        protected virtual void CopyCollider() {
            if (boxCollider2D != null) {
                var confinerBoxCollider = confinerGameObject.AddComponent<BoxCollider2D>();
                confinerBoxCollider.size = boxCollider2D.size;
                confinerBoxCollider.offset = boxCollider2D.offset;
                confinerBoxCollider.usedByComposite = true;
                confinerBoxCollider.isTrigger = true;
            }

            if (polygonCollider2D != null) {
                var confinerPolygonCollider2D = confinerGameObject.AddComponent<PolygonCollider2D>();
                confinerPolygonCollider2D.isTrigger = true;
                confinerPolygonCollider2D.usedByComposite = true;
                confinerPolygonCollider2D.offset = polygonCollider2D.offset;
                confinerPolygonCollider2D.points = polygonCollider2D.points;
            }
        }

        #endregion

        #region Core

        protected virtual IEnumerator EnableCamera(bool state, int frames) {
            if (virtualCamera == null || cinemachineBrain == null) yield break;

            cinemachineBrain.m_DefaultBlend.m_Time = transitionTime;

            if (frames > 0) {
                yield return YisoCoroutineUtils.WaitForFrames(frames);
            }

            virtualCamera.enabled = state;

            if (state) {
                cinematicCameraController.followPlayer = true;
                cinematicCameraController.StartFollowing();
            }
            else {
                cinematicCameraController.StopFollowing();
                cinematicCameraController.followPlayer = false;
            }
        }

        protected virtual void EnterZone() {
            if (!IsVisited) onEnterZoneForTheFirstTimeEvent?.Invoke();

            CurrentZone = true;
            IsVisited = true;

            onEnterZoneEvent?.Invoke();

            StartCoroutine(EnableCamera(true, 0));

            if (inactivateZoneObjectsCoroutine != null) StopCoroutine(inactivateZoneObjectsCoroutine);
            activateZoneObjectsCoroutine = StartCoroutine(ActivateZoneObjects());
        }

        protected virtual void ExitZone() {
            CurrentZone = false;

            onExitZoneEvent?.Invoke();

            StartCoroutine(EnableCamera(false, 0));

            if (activateZoneObjectsCoroutine != null) StopCoroutine(activateZoneObjectsCoroutine);
            inactivateZoneObjectsCoroutine = StartCoroutine(InactivateZoneObjects());
        }

        #endregion

        #region Activator

        protected virtual IEnumerator ActivateZoneObjects() {
            if (activationList == null || activationList.Count == 0) yield break;

            var characters = new List<YisoCharacter>();
            foreach (var go in activationList) {
                if (go.transform.parent != null && !go.transform.parent.gameObject.activeInHierarchy) continue;
                if (go.TryGetComponent<YisoCharacter>(out var character)) {
                    if (character.conditionState != null && character.conditionState.CurrentState ==
                        YisoCharacterStates.CharacterConditions.Dead) {
                        continue;
                    }

                    characters.Add(character);
                    go.SetActive(true);
                    if (go.activeInHierarchy) character.Freeze(YisoCharacterStates.FreezePriority.Default);
                }
                else {
                    go.SetActive(true);
                }
            }

            yield return new WaitForSeconds(transitionTime);

            foreach (var character in characters) {
                if (character.gameObject.activeInHierarchy) character.UnFreeze(YisoCharacterStates.FreezePriority.Default);
            }
        }

        protected virtual IEnumerator InactivateZoneObjects() {
            yield return new WaitForSeconds(transitionTime);
            foreach (var go in activationList) {
                go.SetActive(false);
            }
        }

        #endregion

        public override void OnUpdate() {
            base.OnUpdate();
            if (switchTimer > 0) switchTimer -= Time.deltaTime;
            if (switchTimer <= 0) {
                if (isVisiting && !CurrentZone) {
                    EnterZone();
                    switchTimer = switchCooldown;
                }
                if (!isVisiting && CurrentZone) {
                    ExitZone();
                    switchTimer = switchCooldown;
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (!gameObject.activeInHierarchy) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isVisiting = true;
        }

        protected virtual void OnTriggerStay2D(Collider2D other) {
            if (!gameObject.activeInHierarchy) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isVisiting = true;
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (!gameObject.activeInHierarchy) return;
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isVisiting = false;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (activateZoneObjectsCoroutine != null) StopCoroutine(activateZoneObjectsCoroutine);
            if (inactivateZoneObjectsCoroutine != null) StopCoroutine(inactivateZoneObjectsCoroutine);
        }
    }
}