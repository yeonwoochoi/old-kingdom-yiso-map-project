using System;
using Core.Behaviour;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects {
    [RequireComponent(typeof(Selectable))]
    public class YisoUIHoldEffect : RunIBehaviour {
        private Selectable selectable;
        private RectTransform rectTransform;

        protected override void Start() {
            rectTransform = (RectTransform) transform;
            selectable = GetComponent<Selectable>();
            
            selectable.OnPointerDownAsObservable()
                .Where(_ => selectable.interactable)
                .Subscribe(_ => rectTransform.localScale = new Vector3(0.95f, 0.95f))
                .AddTo(this);

            selectable.OnPointerUpAsObservable()
                .Where(_ => selectable.interactable)
                .Subscribe(_ => rectTransform.localScale = Vector3.one)
                .AddTo(this);
        }
    }
}