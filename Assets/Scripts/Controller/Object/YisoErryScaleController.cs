using System;
using Core.Behaviour;
using Core.Service;
using Core.Service.Stage;
using UnityEngine;

namespace Controller.Object {
    [AddComponentMenu("Yiso/Controller/Object/ErryScaleController")]
    public class YisoErryScaleController : RunIBehaviour {
        protected override void Start() {
            SetScaleByStage();
        }

        protected virtual void SetScaleByStage() {
            gameObject.transform.localScale =
                GetScaleByStage(YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageId());
        }

        private Vector3 GetScaleByStage(int stage) {
            return Vector3.one * (float) (0.9f + 0.01f * Math.Truncate(stage / 10f));
        }
    }
}