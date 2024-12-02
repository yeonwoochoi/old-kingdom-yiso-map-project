using Core.Behaviour;
using Core.Domain.Item;
using Core.Domain.Locale;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Interact.Store.Holder {
    public class YisoInteractStoreInventoryItemHolderUI : RunIBehaviour {
        [SerializeField] private GameObject content;
        
        [SerializeField] private Image blockerImage;
        [SerializeField] private YisoInteractStoreItemHolderUI holder;
        [SerializeField] private GameObject toggleObject;
        [SerializeField] private float toggleRate = 0.1f;

        public event UnityAction<bool> OnSelectEvent; 

        public YisoItem Item => holder.Item;
        public double Price => holder.Price;

        public Toggle HolderToggle => holder.ItemToggle;

        private bool isOn = false;

        public bool IsOn {
            get => isOn;
            set {
                isOn = value;
                toggleObject.SetActive(value);
                if (value) FitWithToggle();
                else FitOnlyHolder();
                OnSelectEvent?.Invoke(value);
            }
        }

        private bool active = false;

        public bool Active {
            get => active;
            set {
                active = value;
                gameObject.SetActive(value);
                holder.Active = value;
            }
        }

        private RectTransform toggleRect;
        private RectTransform contentRect;
        private VerticalLayoutGroup contentLayoutGroup;
        
        private void Init() {
            blockerImage.OnPointerClickAsObservable()
                .Subscribe(OnPointerClick).AddTo(this);

            toggleRect = (RectTransform) toggleObject.transform;
            contentRect = (RectTransform) content.transform;
            contentLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
            blockerImage.raycastTarget = false;
        }

        public void InitItemHolder(ToggleGroup toggleGroup, UnityAction<bool> onValueChanged) {
            Init();
            holder.Init();
            holder.ItemToggle.onValueChanged.AddListener(onValueChanged);
            holder.ItemToggle.group = toggleGroup;
        }

        public void SetItem(YisoInteractStoreContentUI.Types type, YisoItem item, double price,
            YisoLocale.Locale currentLocale) {
            holder.SetItem(type, item, price, currentLocale);
        }
        
        public void Clear() {
            Active = false;
            holder.Clear();
            IsOn = false;
            toggleObject.SetActive(false);
            FitOnlyHolder();
        }

        public void UpdateCount(int count, YisoLocale.Locale currentLocale) {
            holder.UpdateCount(count, currentLocale);
        }

        public void SetSelectMode(bool flag) {
            blockerImage.raycastTarget = flag;
            holder.ItemToggle.interactable = !flag;
        }

        private void FitOnlyHolder() {
            var width = GetWidth();
            SetWidth((RectTransform) holder.transform, width);
        }

        private void FitWithToggle() {
            var width = GetWidth();
            var toggleWidth = width * toggleRate;
            var holderWidth = width - toggleWidth;
            SetWidth((RectTransform) holder.transform, holderWidth);
            SetWidth(toggleRect, toggleWidth);
        }

        private float GetWidth() {
            var width = contentRect.rect.width;
            var padding = contentLayoutGroup.padding.left + contentLayoutGroup.padding.right;
            return width - padding;
        }

        private void SetWidth(RectTransform rect, float width) {
            var size = rect.sizeDelta;
            size.x = width;
            rect.sizeDelta = size;
        }
        
        private void OnPointerClick(PointerEventData data) {
            IsOn = !IsOn;
        }
    }
}