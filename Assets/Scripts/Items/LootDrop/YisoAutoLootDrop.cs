using System.Collections;
using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Core.Domain.Drop;
using Core.Service;
using Core.Service.Character;
using UnityEngine;

namespace Items.LootDrop {
    /// <summary>
    /// Domain에 등록된 드롭 아이템을 받아와서 Drop
    /// </summary>
    [AddComponentMenu("Yiso/Items/Loot/Auto Loot Drop")]
    public class YisoAutoLootDrop : YisoLootDrop {
        protected YisoCharacter character;
        protected YisoCharacterStat characterStat;

        protected override void Awake() {
            base.Awake();

            character = gameObject.GetComponentInParent<YisoCharacter>();
            characterStat = character?.FindAbility<YisoCharacterStat>();
        }

        protected override IEnumerator SpawnLootCo() {
            yield return new WaitForSeconds(delay);
            var dropItems = GetDropItemsByCharacterRole();
            if (dropItems == null || dropItems.Count == 0) yield break;
            double money = 0;
            foreach (var dropItem in dropItems) {
                if (dropItem.Type == YisoDropItem.Types.MONEY) {
                    money += dropItem.MoneyValue;
                    continue;
                }

                SpawnOneItem(dropItem);
            }

            SpawnMoney(money);
            lootFeedback?.PlayFeedbacks();
        }

        protected virtual List<YisoDropItem> GetDropItemsByCharacterRole() {
            var dropItems = new List<YisoDropItem>();
            switch (characterStat.roleType) {
                case YisoCharacterStat.CharacterRoleType.Player:
                    break;
                case YisoCharacterStat.CharacterRoleType.Enemy:
                    dropItems = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetDropItems(characterStat.EntityStat);
                    break;
                case YisoCharacterStat.CharacterRoleType.Npc:
                    break;
            }

            return dropItems;
        }
    }
}