using System.Collections.Generic;
using Character.Core;
using Core.Domain.Actor.Player.Modules.Inventory;
using Core.Service;
using Core.Service.Character;
using Core.Service.UI.HUD;
using Items;
using Items.LootDrop;
using Items.Pickable;
using Sirenix.OdinInspector;
using Tools.Inputs;
using UI.HUD.Interact;
using UnityEngine;
using Utils.Beagle;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Pick Object")]
    public class YisoCharacterPickObject : YisoCharacterAbility {
        public enum PickupType {
            PickAllInRange,
            PickOneByOne
        }

        public bool autoPick = true;
        public PickupType pickupType = PickupType.PickAllInRange;

        [ReadOnly] public List<YisoPickableObject> pickableObjects = new();
        protected YisoLootDetector lootDetector;
        protected YisoInventoryLootDrop inventoryLootDrop;
        protected bool initialized = false;

        public IYisoHUDUIService HUDUIService => YisoServiceProvider.Instance.Get<IYisoHUDUIService>();

        protected override void Initialization() {
            base.Initialization();
            pickableObjects = new List<YisoPickableObject>();

            // Initialize Simple loot
            if (!gameObject.TryGetComponent(out inventoryLootDrop)) {
                inventoryLootDrop = gameObject.AddComponent<YisoInventoryLootDrop>();
            }

            var lootDetectorObj = new GameObject("Loot Detector");
            lootDetectorObj.transform.SetParent(transform);
            lootDetectorObj.transform.localPosition = Vector3.zero;
            lootDetector = lootDetectorObj.AddComponent<YisoLootDetector>();
            lootDetector.Initialization(TriggerEnter, TriggerExit);

            initialized = true;
        }

        protected override void HandleInput() {
            base.HandleInput();
            if (!initialized || pickableObjects == null) return;
            if (autoPick) {
                Pick();
            }
            else {
                if (inputManager.IsMobile) {
                    if (pickableObjects.Count > 0) {
                        if (!HUDUIService.IsReady()) return;
                        HUDUIService.ShowInteractButton(YisoHudUIInteractTypes.GRAB, Pick);
                    }
                    else {
                        if (!HUDUIService.IsReady()) return;
                        HUDUIService.HideInteractButton(YisoHudUIInteractTypes.GRAB);
                    }
                }
                else {
                    if (inputManager.PickButton.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                        .ButtonStates.ButtonPressed) {
                        Pick();
                    }
                }
            }
        }

        public override void ProcessAbility() {
            if (!initialized || pickableObjects == null) return;
            RemoveInvalidObject();
        }

        protected virtual void TriggerEnter(Collider2D collider) {
            if (!initialized) return;
            if (collider == null) return;
            if (!collider.TryGetComponent(out YisoPickableObject pickable)) return;
            pickable.ShowObjectName();
            if (!pickableObjects.Contains(pickable)) {
                pickableObjects.Add(pickable);
            }
        }

        protected virtual void TriggerExit(Collider2D collider) {
            if (!initialized) return;
            if (collider == null) return;
            if (!collider.TryGetComponent(out YisoPickableObject pickable)) return;
            pickable.HideObjectName();
            if (pickableObjects.Contains(pickable)) {
                pickableObjects.Remove(pickable);
            }
        }

        protected virtual void Pick() {
            if (pickableObjects == null || pickableObjects.Count == 0) return;
            switch (pickupType) {
                case PickupType.PickAllInRange:
                    for (var i = pickableObjects.Count - 1; i >= 0; i--) {
                        pickableObjects[i].PickObject(character.gameObject);
                    }

                    break;
                case PickupType.PickOneByOne:
                    YisoPhysicsUtils.GetClosestObject(pickableObjects, character.transform)
                        ?.PickObject(character.gameObject);
                    break;
            }
        }

        protected virtual void RemoveInvalidObject() {
            for (var i = pickableObjects.Count - 1; i >= 0; i--) {
                var pickableObject = pickableObjects[i];
                if (pickableObject == null) {
                    pickableObjects.RemoveAt(i);
                }
                else if (!pickableObject.gameObject.activeInHierarchy) {
                    pickableObjects.RemoveAt(i);
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.OnInventoryEvent +=
                OnItemDrop;
        }

        protected override void OnDisable() {
            base.OnDisable();
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().InventoryModule.OnInventoryEvent -=
                OnItemDrop;
        }

        protected virtual void OnItemDrop(YisoPlayerInventoryEventArgs args) {
            if (character.characterType != YisoCharacter.CharacterTypes.Player) return;
            if (args is not YisoPlayerInventoryDropEventArgs dropArgs) return;
            // auto drop인 경우 아이템을 땅에 스폰시키지 않음 (바로 자동으로 먹어지니까)
            // 자동으로 못 먹게 설정해두면 굳이 스폰시킬 필요가 없음
            if (autoPick) return;
            inventoryLootDrop.DropOneItem(dropArgs.DroppedItem);
        }
    }
}