using System;
using Core.Behaviour;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Game.Dialogue {
    public class YisoGameDialogueNextIndicatorUI : RunIBehaviour {
        [SerializeField] private float moveValue = 1.0f;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Sequence sequence;
        private Vector3 initialPosition;

        protected override void Awake() {
            base.Awake();
            rectTransform = (RectTransform) transform;
            canvasGroup = GetComponent<CanvasGroup>();
            initialPosition = rectTransform.localPosition;
        }

        public void ActiveIndicator(bool flag) {
            canvasGroup.Visible(flag);
            if (!flag) {
                sequence.Pause();
                rectTransform.localPosition = initialPosition;
                return;
            }
            
            sequence ??= DOTween.Sequence()
                .SetAutoKill(false)
                .Append(rectTransform.DOLocalMoveY(initialPosition.y - moveValue, .25f))
                .Append(rectTransform.DOLocalMoveY(initialPosition.y, .25f))
                .SetLoops(-1);
            
            sequence.Restart();
        }
    }
}