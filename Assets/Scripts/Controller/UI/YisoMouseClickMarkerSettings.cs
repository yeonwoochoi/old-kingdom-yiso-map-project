using System;
using System.Threading.Tasks;
using Core.Service;
using Core.Service.ObjectPool;
using UnityEngine;
using Utils.Beagle;

namespace Controller.UI {
    [Serializable]
    public class YisoMouseClickMarkerSettings {
        public GameObject mouseClickCursorPrefab;
        public GameObject mouseClickCursorRoot;
        
        protected IYisoObjectPoolService poolService;
        protected float lastClickTime = 0f;
        protected readonly float clickInterval = 0.1f;

        protected const string ClickAnimationParameterName = "Click";
        protected int clickAnimationParameter;

        public void Initialization() {
            poolService = YisoServiceProvider.Instance.Get<IYisoObjectPoolService>();
            poolService.WarmPool(mouseClickCursorPrefab, 30, mouseClickCursorRoot);
            lastClickTime = 0f;
            clickAnimationParameter = Animator.StringToHash(ClickAnimationParameterName);
        }
        
        public void ShowMouseClickCursor(Vector2 mousePosition) {
            if (Time.time - lastClickTime <= clickInterval) return;
            lastClickTime = Time.time;
            var cursor = poolService.SpawnObject<Transform>(mouseClickCursorPrefab, mouseClickCursorRoot);
            var animator = cursor.gameObject.GetComponentInChildren<Animator>();
            
            cursor.gameObject.transform.position = mousePosition;

            if (animator != null) {
                YisoAnimatorUtils.UpdateAnimatorTrigger(animator, clickAnimationParameter);
            }

            ExecuteWithDelay(0.5f, () => poolService.ReleaseObject(cursor.gameObject));

            async void ExecuteWithDelay(float delay, Action action) {
                await Task.Delay((int) (delay * 1000));
                action?.Invoke();
            }
        }
    }
}