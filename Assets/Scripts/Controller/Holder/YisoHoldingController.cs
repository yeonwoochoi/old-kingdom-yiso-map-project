using System.Collections;
using Controller.Emoticon;
using Core.Behaviour;
using Core.Domain.Locale;
using Core.Service;
using Core.Service.Game;
using Core.Service.Temp;
using Core.Service.UI;
using Core.Service.UI.HUD;
using Core.Service.UI.Popup;
using Manager_Temp_;
using Sirenix.OdinInspector;
using Tools.Inputs;
using UI.HUD.Interact;
using UnityEngine;

namespace Controller.Holder {
    /// <summary>
    /// Quest, Cabinet, Item, Quest Reward Box 등등 object를 들고 있는 gameObject controller임
    /// target object가 근처로 오면 Emoticon, Interact Button 나타남
    /// Interact button 누르면 target object이랑 상호작용 (ex. item을 준다던지, Quest를 시작/완료시키던지)
    /// </summary>
    public abstract class YisoHoldingController : RunIBehaviour {
        [Title("Target")] 
        public bool detectTarget = true;
        public LayerMask targetLayerMask = LayerManager.PlayerLayerMask;

        [Title("Emoticon")] 
        public bool showEmoticon = true;
        [ShowIf("showEmoticon")] public bool alwaysShowEmoticons = false;
        [ShowIf("showEmoticon")] public bool useCustomEmoticon = false;
        [ShowIf("useCustomEmoticon")] public YisoEmotionController.EmotionType customEmoticonType;

        [Title("Interact")]
        public bool interactWithPlayer = true;
        [ShowIf("interactWithPlayer")] public bool useMouseClickInteractionOnDesktop = true; // desktop모드에선 캐릭터를 클릭하는 것으로 상호작용할지 여부
        [ShowIf("useMouseClickInteractionOnDesktop")] public bool useCustomMouseDetector = false;
        [ShowIf("useCustomMouseDetector")] public YisoMouseClickEventDispatcher customMouseDetector;
        [ShowIf("interactWithPlayer")] public YisoHudUIInteractTypes interactHUDType = YisoHudUIInteractTypes.SPEECH;

        public bool showInteractionConfirmationPopup  = false; // 상호 작용을 할지 말지를 묻는 팝업이 먼저 뜨게끔 할지 여부
        [ShowIf("showInteractionConfirmationPopup")] public YisoLocale confirmationPopupTitle;
        [ShowIf("showInteractionConfirmationPopup")] public YisoLocale confirmationPopupContent;
        public IYisoHUDUIService HUDUIService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();
        public IYisoPopupUIService PopupUIService => YisoServiceProvider.Instance.Get<IYisoPopupUIService>();
        public IYisoUIService UIService => YisoServiceProvider.Instance.Get<IYisoUIService>();
        protected IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();
        public YisoLocale.Locale CurrentLocale => YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
        public abstract YisoEmotionController.EmotionType EmotionType { get; }

        protected YisoEmotionController emotionController;
        protected YisoMouseClickEventDispatcher mouseClickEventDispatcher;
        protected bool initialized = false;
        protected bool isShowingEmoticon = false;
        protected bool isShowingInteractButton = false;
        protected bool isDetected = false;
        protected bool isMobile = false;
        protected string key;
        protected IEnumerator interactButtonCoroutine = null;

        protected override void Start() {
            base.Start();
            Initialization();
        }

        protected virtual void Initialization() {
            if (initialized) return;
            isMobile = YisoServiceProvider.Instance.Get<IYisoGameService>().IsMobile();
            // defaultCursor = Cursor.texture; // 현재 커서를 기본 커서로 저장
            InitializeEmoticon();
            InitializeMouseClickDispatcher();
            initialized = true;
        }
        
        private void InitializeEmoticon() {
            if (!showEmoticon) return;

            var emotionControllers = gameObject.GetComponentsInChildren<YisoEmotionController>();
            emotionController = emotionControllers.Length > 0 ? emotionControllers[0] :
                Instantiate(TempService.GetGameManager().GameModules.AssetModule.EmoticonPrefab, transform)
                    .GetComponent<YisoEmotionController>();

            emotionController.Initialization();
        }

        private void InitializeMouseClickDispatcher() {
            if (!useMouseClickInteractionOnDesktop) return;
            if (!GameManager.HasInstance) return;
            if (useCustomMouseDetector) {
                if (customMouseDetector != null) {
                    mouseClickEventDispatcher = customMouseDetector;
                    var position = mouseClickEventDispatcher.transform.localPosition;
                    mouseClickEventDispatcher.transform.localPosition = new Vector3(position.x, position.y, -5f);
                    mouseClickEventDispatcher.RegisterCallback(OnMouseClickInteract);
                    return;
                }
            }
            var mouseDetectorPrefab = GameManager.Instance.GameModules.AssetModule.DefaultMouseClickDetectorPrefab;
            mouseClickEventDispatcher = Instantiate(mouseDetectorPrefab, transform).GetComponent<YisoMouseClickEventDispatcher>();
            mouseClickEventDispatcher.transform.localPosition = new Vector3(0f, 0f, -5f);
            mouseClickEventDispatcher.RegisterCallback(OnMouseClickInteract);
        }

        private void Reset() {
            ResetDetection();
            HideInteractButtonIfVisible();
            CancelEmoticonIfVisible();
        }

        private void ResetDetection() {
            isDetected = false;
        }

        private void HideInteractButtonIfVisible() {
            if (!isShowingInteractButton) return;

            HideInteractButton();
            if (interactButtonCoroutine != null) {
                StopCoroutine(interactButtonCoroutine);
            }
            isShowingInteractButton = false;
        }

        private void CancelEmoticonIfVisible() {
            if (!isShowingEmoticon) return;

            emotionController.CancelEmotion();
            isShowingEmoticon = false;
        }

        public override void OnUpdate() {
            if (!initialized) return;
            if (showEmoticon) ShowEmoticonIfConditionsMet();
        }

        protected override void OnEnable() {
            base.OnEnable();
            Initialization();
        }

        protected override void OnDisable() {
            base.OnDisable();
            Reset();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Reset();
        }

        #region Emoticon

        protected virtual bool CanDisplayEmoticon() {
            if (alwaysShowEmoticons) return true;
            return !detectTarget || isDetected;
        }

        protected virtual void ShowEmoticonIfConditionsMet() {
            if (isShowingEmoticon || !CanDisplayEmoticon()) return;
            isShowingEmoticon = true;
            emotionController.ShowEmoticon(EmotionType, () => !CanDisplayEmoticon(),
                () => { isShowingEmoticon = false; });
        }

        #endregion

        #region Interaction (Core)

        protected virtual bool CanInteract() {
            return interactWithPlayer;
        }

        protected virtual void PerformInteraction() { }

        #endregion

        #region Interact HUD Button

        protected virtual bool CanShowInteractHUD() {
            return (!useMouseClickInteractionOnDesktop || isMobile) && (!detectTarget || isDetected);
        }

        protected virtual void ShowInteractButton() {
            HUDUIService.ShowInteractButton(interactHUDType, showInteractionConfirmationPopup ? ShowInteractionConfirmationPopup : PerformInteraction);
            isShowingInteractButton = true;
        }

        protected virtual void HideInteractButton() {
            HUDUIService.HideInteractButton(interactHUDType);
            isShowingInteractButton = false;
        }

        protected virtual IEnumerator ShowInteractButtonCo() {
            if (isShowingInteractButton) yield break;
            ShowInteractButton();
            while (CanInteract() && CanShowInteractHUD()) {
                yield return null;
            }
            HideInteractButton();
        }

        protected virtual void ShowInteractionConfirmationPopup() {
            PopupUIService.AlertS(confirmationPopupTitle[CurrentLocale], confirmationPopupContent[CurrentLocale], PerformInteraction, () => { });
        }

        #endregion

        #region Interact Mouse

        protected virtual bool CanInteractByMouseClick() {
            return useMouseClickInteractionOnDesktop && !isMobile && isDetected;
        }

        protected virtual void OnMouseClickInteract() {
            if (CanInteract() && CanInteractByMouseClick()) {
                if (showInteractionConfirmationPopup) {
                    ShowInteractionConfirmationPopup();
                }
                else {
                    PerformInteraction();
                }
            }
        }

        #endregion

        #region Trigger

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (!initialized || !LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isDetected = true;
            if (CanInteract() && CanShowInteractHUD()) StartInteractButtonCoroutine();
        }

        protected virtual void OnTriggerStay2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isDetected = true;
            if (CanInteract() && CanShowInteractHUD() && !isShowingInteractButton) StartInteractButtonCoroutine();
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (!initialized || !LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            isDetected = false;
        }
        
        private void StartInteractButtonCoroutine() {
            interactButtonCoroutine = ShowInteractButtonCo();
            StartCoroutine(interactButtonCoroutine);
        }

        #endregion
    }
}