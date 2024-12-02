using System;
using System.Collections;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Blacksmith.Description {
    public class YisoInteractBlacksmithNormalDescriptionUI : YisoInteractBlacksmithBaseDescriptionUI {
        [SerializeField] private TextMeshProUGUI upgradableText;
        [SerializeField] private TextMeshProUGUI attackUpgradableText;
        [SerializeField] private TextMeshProUGUI probabilityText;
        [SerializeField] private TextMeshProUGUI defenceUpgradableText;
        [SerializeField] private TextMeshProUGUI priceText;

        [SerializeField] private GameObject probabilityObject;

        private YisoPlayerInventoryNormalReinforceResult cachedResult = null;

        [Title("Effect Settings")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private float successBlendingValue = 0.8f;
        [SerializeField] private float successScaleValue = 1.1f;
        [SerializeField] private float failureStrength = 10f;
        [SerializeField] private int failureVibrato = 10;
        [SerializeField] private bool success = true;

        public override void SetItem(YisoEquipItem item, YisoPlayerInventoryReinforceResult result, Image image) {
            base.SetItem(item, result, image);
            SetItem();
            cachedResult = (YisoPlayerInventoryNormalReinforceResult) result;
        }

        private void SetItem() {
            /*var isUpgradable = item.UpgradableSlots > 0;
            
            probabilityObject.gameObject.SetActive(isUpgradable);
            
            var attack = item.GetAttack();
            var defence = item.GetDefence();
            
            if (isUpgradable) {
                var result = (YisoPlayerInventoryNormalReinforceResult) reinforceResult;
                
                var afterAttack = attack + result.AttackInc;
                var afterDefence = defence + result.DefenceInc;
            
                upgradableText.SetText(item.UpgradableSlots.ToCommaString());
                attackUpgradableText.SetText(ToUpgradeString(attack, afterAttack));
                defenceUpgradableText.SetText(ToUpgradeString(defence, afterDefence));
                probabilityText.SetText(result.GetPercentage());
                
                return;
            }
            
            upgradableText.SetText("0");
            attackUpgradableText.SetText(attack.ToCommaString());
            defenceUpgradableText.SetText(defence.ToCommaString());*/
        }

        private string ToUpgradeString(int currentValue, int afterValue) {
            var currentStr = currentValue.ToCommaString();
            var afterStr = afterValue.ToCommaString();
            if (afterValue <= currentValue) return currentStr;
            return $"{currentStr} \u2192 <b>{afterStr}";
        }

        private void OnReinforceSuccess(UnityAction<YisoPlayerInventoryReinforceResult> onComplete, UnityAction coroutineComplete) {
            StartCoroutine(DOReinforceSuccess(onComplete, coroutineComplete));
        }

        private void OnReinforceFailure(UnityAction<YisoPlayerInventoryReinforceResult> onComplete, UnityAction coroutineComplete) {
            itemImage.rectTransform.DOShakePosition(duration, strength: failureStrength, vibrato: failureVibrato)
                .OnComplete(() => {
                    onComplete?.Invoke(cachedResult);
                    coroutineComplete?.Invoke();
                });
        }

        private IEnumerator DOReinforceSuccess(UnityAction<YisoPlayerInventoryReinforceResult> onComplete, UnityAction coroutineComplete) {
            var material = itemImage.material;
            var rectT = itemImage.rectTransform;
            var color = item.Rank == YisoEquipRanks.N ? Color.white : item.Rank.ToColor();
            
            material.ActiveHitEffect(true);
            material.DOHit(color, successBlendingValue, duration / 2f);
            yield return rectT.DOScale(successScaleValue, duration / 2f).WaitForCompletion();

            material.DOHit(color, 0f, duration / 2f);
            yield return rectT.DOScale(1f, duration / 2f).WaitForCompletion();
            material.ActiveHitEffect(false);
            
            onComplete?.Invoke(cachedResult);
            coroutineComplete?.Invoke();
        }
        
        public override Types GetDescriptionType() => Types.NORMAL;

        public override bool CanUpgrade() => item.UpgradableSlots > 0;
        public override void OnClickReinforce(UnityAction cb) {
            if (cachedResult.Success) {
                RaiseReinforceEvent(onReinforce => {
                    OnReinforceSuccess(onReinforce, cb);
                });
            } else {
                RaiseReinforceEvent(onReinforce => {
                    OnReinforceFailure(onReinforce, cb);
                });
            }
        }
    }
}