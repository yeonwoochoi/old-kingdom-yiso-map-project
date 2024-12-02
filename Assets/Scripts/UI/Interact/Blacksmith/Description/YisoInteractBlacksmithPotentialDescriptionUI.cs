using System.Collections;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Interact.Blacksmith.Description.Potential;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace UI.Interact.Blacksmith.Description {
    public class YisoInteractBlacksmithPotentialDescriptionUI : YisoInteractBlacksmithBaseDescriptionUI {
        [SerializeField, Title("Ranks")] private TextMeshProUGUI rankText;
        
        [SerializeField, Title("Potentials")] private YisoInteractBlacksmithPotentialItemUI potentialItem1UI;
        [SerializeField] private YisoInteractBlacksmithPotentialItemUI potentialItem2UI;
        [SerializeField] private YisoInteractBlacksmithPotentialItemUI potentialItem3UI;

        private YisoPlayerInventoryPotentialReinforceResult potentialResult;
        
        public override void Clear() {
            base.Clear();
            rankText.SetText(string.Empty);
            potentialItem1UI.SetPotential();
            potentialItem2UI.SetPotential();
            potentialItem3UI.SetPotential();
        }

        public override void SetItem(YisoEquipItem item, YisoPlayerInventoryReinforceResult result, Image itemImage) {
            base.SetItem(item, result, itemImage);

            potentialResult = (YisoPlayerInventoryPotentialReinforceResult) result;

            if (item.Potentials.TryGetValue(1, out var potential1)) {
                potentialItem1UI.SetPotential(potential1, CurrentLocale);
            } else potentialItem1UI.SetPotential();
            
            if (item.Potentials.TryGetValue(2, out var potential2)) {
                potentialItem2UI.SetPotential(potential2, CurrentLocale);
            } else potentialItem2UI.SetPotential();
            
            if (item.Potentials.TryGetValue(3, out var potential3)) {
                potentialItem3UI.SetPotential(potential3, CurrentLocale);
            } else potentialItem3UI.SetPotential();
            
            SetRank();
        }

        protected override void SetRank() {
            base.SetRank();
            var rank = item.Rank;
            SetRankText(rank);
        }

        private void SetRankText(YisoEquipRanks rank) {
            rankText.SetText($"{rank.ToString(CurrentLocale)}({rank.ToString()})");
        }

        private void OnReinforce(UnityAction<YisoPlayerInventoryReinforceResult> onComplete = null, UnityAction coroutineComplete = null) {
            if (potentialResult.UpgradeRank) {
                var nextRank = item.Rank.NextRank();
                StartCoroutine(DORankUpgrade(() => {
                    SetRankText(nextRank);
                }));
            }

            StartCoroutine(DOPotentialUpgrade(() => {
                coroutineComplete?.Invoke();
                onComplete?.Invoke(potentialResult);
            }));
        }

        private IEnumerator DORankUpgrade(UnityAction onComplete = null) {
            var itemRank = item.Rank;
            var itemMaterial = itemImage.material;
            itemMaterial.ActiveOutline(false);
            var itemImageRect = itemImage.rectTransform;
            itemMaterial.ActiveHitEffect(true);
            var rankColor = itemRank.ToColor();
                
            itemMaterial.DOHit(rankColor, 0.8f, 0.25f);
            yield return itemImageRect.DOScale(1.1f, 0.25f).WaitForCompletion();
                
            itemMaterial.DOHit(rankColor, 0f, 0.25f);
            yield return itemImageRect.DOScale(1f, 0.25f).WaitForCompletion();
            
            itemMaterial.ActiveOutline(true);
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
            onComplete?.Invoke();
        }
        
        private IEnumerator DOPotentialUpgrade(UnityAction onComplete = null) {

            var potential1 = potentialResult.Potential1;
            var potential2 = potentialResult.Potential2;
            var potential3 = potentialResult.Potential3;

            if (potential1 == null) {
                onComplete?.Invoke();
                yield break;
            }

            potentialItem1UI.ItemCanvas.DOVisible(0f, 0.25f);
            potentialItem2UI.ItemCanvas.DOVisible(0f, 0.25f);
            yield return potentialItem3UI.ItemCanvas.DOVisible(0f, 0.25f).WaitForCompletion();
            
            potentialItem1UI.SetPotential(potential1);
            potentialItem2UI.SetPotential(potential2);
            potentialItem3UI.SetPotential(potential3);
            
            potentialItem1UI.ItemCanvas.DOVisible(1f, 0.25f);
            potentialItem2UI.ItemCanvas.DOVisible(1f, 0.25f);
            yield return potentialItem3UI.ItemCanvas.DOVisible(1f, 0.25f).WaitForCompletion();

            
            onComplete?.Invoke();
        }

        public override Types GetDescriptionType() => Types.POTENTIAL;

        public override bool CanUpgrade() => true;

        public override void OnClickReinforce(UnityAction cb) {
            RaiseReinforceEvent(onReinforce => {
                OnReinforce(onReinforce, cb);
            });
        }
    }
}