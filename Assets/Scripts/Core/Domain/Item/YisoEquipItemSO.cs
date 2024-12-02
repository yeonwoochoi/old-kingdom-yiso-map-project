using System;
using System.Collections.Generic;
using Character.Weapon;
using Core.Domain.Actor.Player.Modules.Inventory.Reinforce;
using Core.Domain.Data;
using Core.Domain.Item.Equip;
using Core.Domain.Item.Utils;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Factor.HonorRating;
using Core.Service.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "EquipItem", menuName = "Yiso/Item/Equip Item")]
    public class YisoEquipItemSO : YisoItemSO {
        public YisoEquipSlots slot;
        public int setItemId = -1;
        public int upgradableSlots = 7;
        public int reqLevel;
        public int attack;
        public int defence;
        public YisoEquipFactions faction;
        public YisoEquipSubTypes subType;
        public YisoWeapon.AttackType attackType = YisoWeapon.AttackType.None;
        public YisoEquipRanks rank = YisoEquipRanks.N;
        public YisoEquipRanks minRank;
        public YisoEquipRanks maxRank;

        [ShowIf("slot", YisoEquipSlots.WEAPON)]
        public GameObject equippedPrefab;

        public override YisoItem CreateItem() => new YisoEquipItem(this);

        [Button]
        public void SetId() {
            var itemConst = 3 * (int)(Math.Pow(10, 6));
            var factionConst = (int)faction * (int)(Math.Pow(10, 5));
            var slotConst = (int)slot * (int)(Math.Pow(10, 3));
            var subTypeConst = subType.IndexOf() * 10;
            var rankConst = (int)rank + 1;
            id = itemConst + factionConst + slotConst + subTypeConst + rankConst;
        }
    }

    [Serializable]
    public class YisoEquipItem : YisoItem {
        public YisoEquipPotentials Potentials { get; private set; } = new();
        
        public YisoEquipSlots Slot { get; private set; }
        public int SetItemId { get; internal set; }
        public int UpgradedSlots { get; private set; }
        public int UpgradableSlots { get; private set; }
        public YisoEquipRanks Rank { get; set; }
        public YisoEquipRanks MinRank { get; private set; }
        public YisoEquipRanks MaxRank { get; private set; }
        
        public YisoEquipFactions Faction { get; private set; }
        
        public YisoEquipSubTypes SubType { get; private set; }
        
        public YisoWeapon.AttackType AttackType { get; private set; }
        
        public GameObject EquippedPrefab { get; private set; }
        
        public bool Equipped { get; set; }
        
        public double CombatRating { get; private set; }
        
        public YisoEquipItem(YisoEquipItemSO so) : base(so) {
            Slot = so.slot;
            SetItemId = so.setItemId;
            UpgradableSlots = so.upgradableSlots;
            Rank = so.rank;
            MinRank = so.minRank;
            MaxRank = so.maxRank;
            AttackType = so.attackType;
            EquippedPrefab = so.equippedPrefab;

            Faction = so.faction;
            SubType = so.subType;
            
            if (UpgradableSlots == 0 && UpgradedSlots == 0) {
                UpgradableSlots = 7;
                UpgradedSlots = 0;
            }
        }

        public YisoEquipItem(YisoItem item) : base(item) {
            var equipItem = (YisoEquipItem) item;
            Slot = equipItem.Slot;
            SetItemId = equipItem.SetItemId;
            UpgradableSlots = equipItem.UpgradableSlots;
            UpgradedSlots = equipItem.UpgradedSlots;
            Rank = equipItem.Rank;
            MinRank = equipItem.MinRank;
            MaxRank = equipItem.MaxRank;
            AttackType = equipItem.AttackType;
            Equipped = equipItem.Equipped;
            EquippedPrefab = equipItem.EquippedPrefab;
            Faction = equipItem.Faction;
            SubType = equipItem.SubType;
            CombatRating = equipItem.CombatRating;
            
            Potentials = equipItem.Potentials.DeepCopy();
        }

        public void Reinforce(YisoPlayerInventoryReinforceResult result) {
            switch (result) {
                case YisoPlayerInventoryNormalReinforceResult normalResult:
                    if (!normalResult.Success) {
                        UpdateUpgradable(false);
                        return;
                    }

                    UpdateUpgradable();
                    break;
                case YisoPlayerInventoryPotentialReinforceResult potentialResult:
                    if (potentialResult.UpgradeRank) {
                        Rank = Rank.NextRank();
                    }
                    
                    if (!potentialResult.ExistPotential) return;
                    Potentials[1] = potentialResult.Potential1.Copy();
                    Potentials[2] = potentialResult.Potential2.Copy();
                    Potentials[3] = potentialResult.Potential3.Copy();
                    break;
            }
        }

        private void UpdateUpgradable(bool updateUpgrade = true) {
            UpgradableSlots -= 1;
            if (updateUpgrade) {
                UpgradedSlots += 1;
            }

            if (UpgradableSlots == 0) UpgradableSlots = -1;
        }

        public override string GetName(YisoLocale.Locale locale = YisoLocale.Locale.KR) {
            var upgraded = UpgradedSlots > 0 ? $"(+{UpgradedSlots})" : "";
            var baseName = base.GetName(locale);
            if (baseName != string.Empty)
                return $"{baseName} {upgraded}";
            var faction = Faction.ToString(locale);
            var subType = SubType.ToString(locale);
            var middle = locale == YisoLocale.Locale.KR ? "의" : "'";
            return $"{faction}{middle} {subType} {upgraded}";
        }

        public override void Load(YisoPlayerItemData data, YisoItemSO so) {
            base.Load(data, so);
            var equipData = (YisoPlayerEquipItemData) data;
            var equipItemSO = (YisoEquipItemSO) so;
            EquippedPrefab = equipItemSO.equippedPrefab;
            Rank = equipData.rank;
            Slot = equipData.slot;
            UpgradableSlots = equipData.upgradableSlots;
            UpgradedSlots = equipData.upgradedSlots;
            Equipped = equipData.equipped;
            
            Potentials.Load(equipData.potentials);
            
            Faction = equipData.faction;
            SubType = equipData.subType;
            CombatRating = equipData.combatRating;

            if (UpgradableSlots == 0 && UpgradedSlots == 0) {
                UpgradableSlots = 7;
                UpgradedSlots = 0;
            }
        }

        public override YisoPlayerItemData Save() {
            var equipData = new YisoPlayerEquipItemData() {
                position = Position,
                quantity = Quantity,
                itemId = Id,
                upgradableSlots = UpgradableSlots,
                upgradedSlots = UpgradedSlots,
                rank = Rank,
                potentials = Potentials.Save(),
                equipped = Equipped,
                faction = Faction,
                subType = SubType,
                combatRating = CombatRating,
                objectId = ObjectId,
                slot = Slot
            };

            return equipData;
        }

        public void CalculateCombatRating(IYisoHonorRatingFactorService honorRatingFactorService) {
            var factor = honorRatingFactorService.GetItemFactors();
            var stageCR = YisoServiceProvider.Instance.Get<IYisoStageService>().GetCurrentStageCR();
            stageCR *= factor.honorRating;
            var crRate = factor.rankHonorRatingFactors[Rank];
            var diffValue = (stageCR * crRate) / YisoEquipItemTypeConst.SLOT_COUNT;
            var minError = Mathf.RoundToInt((float)(diffValue * factor.honorRatingErrorFactors.minError));
            var maxError = Mathf.RoundToInt((float)(diffValue * factor.honorRatingErrorFactors.maxError));
            var error = Randomizer.NextInt(-minError, maxError);
            diffValue += error;
            CombatRating = diffValue;
        }
    }
}