using System;
using System.Collections;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Item;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Interact.Blacksmith.Description.Potential;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Extensions;

namespace Test.UI {
    public class TestReinforcePotentialUI : MonoBehaviour {
        [SerializeField] private YisoEquipItemSO testItemSO;
        
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI rankText;
        
        [SerializeField, Title("Potentials")] private YisoInteractBlacksmithPotentialItemUI potentialItem1UI;
        [SerializeField] private YisoInteractBlacksmithPotentialItemUI potentialItem2UI;
        [SerializeField] private YisoInteractBlacksmithPotentialItemUI potentialItem3UI;

        private float duration = 1f;

        private Material itemMaterial;
        private YisoEquipItem item;

        private readonly YisoPlayerInventoryPotentialReinforcer reinforcer = new();
        
        private void Start() {
            itemMaterial = itemImage.material;
            item = (YisoEquipItem) testItemSO.CreateItem();
            SetItem();
        }

        private void SetItem() {
            itemImage.sprite = item.Icon;
            itemNameText.SetText(item.GetName());
            SetPotential();
            SetRank();
        }

        private void SetPotential() {
            /*if (item.Potentials.TryGetValue(1, out var potential1)) {
                potentialItem1UI.SetPotential(potential1);
            } else potentialItem1UI.SetPotential();
            
            if (item.Potentials.TryGetValue(2, out var potential2)) {
                potentialItem2UI.SetPotential(potential2);
            } else potentialItem2UI.SetPotential();
            
            if (item.Potentials.TryGetValue(3, out var potential3)) {
                potentialItem3UI.SetPotential(potential3);
            } else potentialItem3UI.SetPotential();*/
        }

        private void SetRank() {
            var rank = item.Rank;
            rankText.SetText($"{rank.ToString(YisoLocale.Locale.KR)}({rank.ToString()})");
            if (rank == YisoEquipRanks.N) return;
            itemMaterial.ActiveOutline(true);
            var rankColor = rank.ToColor();
            itemMaterial.SetOutlineColor(rankColor);
            itemMaterial.ActiveOutlineDistortion(true);
        }

        public void OnClickReinforce(bool success = true) {
            if (success) {
                var itemRank = item.Rank;
                if (itemRank.TryGetNextRank(out var nextRank)) {
                    item.Rank = item.Rank.NextRank();
                    StartCoroutine(DORankUpgraded(() => {
                        rankText.SetText($"{nextRank.ToString(YisoLocale.Locale.KR)}({nextRank.ToString()})");
                    }));
                } else {
                    success = false;
                }
            }

            StartCoroutine(DOPotentialUpgrade(success, () => {
                // TODO(ENABLE BUTTON)
            }));
        }

        private IEnumerator DORankUpgraded(UnityAction onCompleted = null) {
            var itemRank = item.Rank;
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
            onCompleted?.Invoke();
        }

        private IEnumerator DOPotentialUpgrade(bool rankSuccess, UnityAction onComplete = null) {
            /*var (potential1, potential2, potential3) = reinforcer.GetRandomPotentials(item, rankSuccess);
            
            if (potential1 == null) yield break;

            potentialItem1UI.ItemCanvas.DOVisible(0f, 0.25f);
            potentialItem2UI.ItemCanvas.DOVisible(0f, 0.25f);
            yield return potentialItem3UI.ItemCanvas.DOVisible(0f, 0.25f).WaitForCompletion();
            
            potentialItem1UI.SetPotential(potential1);
            potentialItem2UI.SetPotential(potential2);
            potentialItem3UI.SetPotential(potential3);
            
            potentialItem1UI.ItemCanvas.DOVisible(1f, 0.25f);
            potentialItem2UI.ItemCanvas.DOVisible(1f, 0.25f);*/
            yield return potentialItem3UI.ItemCanvas.DOVisible(1f, 0.25f).WaitForCompletion();

            
            onComplete?.Invoke();
        }
        
        private IEnumerator DOReinforce(bool rankSuccess, bool potentialSuccess, UnityAction onComplete) {
            if (rankSuccess) {
                var itemRank = item.Rank;
                itemMaterial.ActiveOutline(false);
                var itemImageRect = itemImage.rectTransform;
                itemMaterial.ActiveHitEffect(true);
                var rankColor = itemRank.ToColor();
                
                itemMaterial.DOHit(rankColor, 0.8f, 0.25f);
                yield return itemImageRect.DOScale(1.1f, 0.25f).WaitForCompletion();
                
                itemMaterial.DOHit(rankColor, 0f, 0.25f);
                yield return itemImageRect.DOScale(1f, 0.25f).WaitForCompletion();
                
                itemMaterial.ActiveHitEffect(false);
            }
            
            onComplete?.Invoke();
        }
    }
}