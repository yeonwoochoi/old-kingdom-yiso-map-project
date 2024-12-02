using System.Collections;
using Core.Behaviour;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Init.Loading {
    public class YisoInitLoadingTabToStartUI : RunIBehaviour {
        [SerializeField] private TextMeshProUGUI tabToStartText;
        
        private Image tabImage;
        private CanvasGroup tabToStartCanvas;
        public CanvasGroup PanelCanvas { get; private set; }

        private bool isTabbed = false;

        public IEnumerator DOBlink() {
            while (!isTabbed) {
                yield return tabToStartCanvas.DOVisible(1f, 0.5f).WaitForCompletion();
                yield return tabToStartCanvas.DOVisible(0f, 0.5f).WaitForCompletion();
            }
        }

        protected override void Start() {
            tabImage = GetComponent<Image>();
            PanelCanvas = GetComponent<CanvasGroup>();
            tabToStartCanvas = tabToStartText.GetComponent<CanvasGroup>();
            
            PanelCanvas.Visible(false);
            tabToStartCanvas.Visible(false);
            
            tabImage.OnPointerClickAsObservable().Subscribe(OnClickImage).AddTo(this);
        }

        private void OnClickImage(PointerEventData eventData) {
            isTabbed = true;
        }
    }
}