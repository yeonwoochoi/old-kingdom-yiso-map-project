using System;
using System.Text;
using Core.Domain.Actor;
using Core.Domain.Actor.Player;
using Core.Domain.Effect;
using Core.Domain.Entity;
using Core.Domain.Locale;
using Core.Domain.Types;
using Core.Service;
using Core.Service.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Extensions;

namespace Core.Domain.Item {
    [CreateAssetMenu(fileName = "UseItem", menuName = "Yiso/Item/Use Item")]
    public class YisoUseItemSO : YisoItemSO {
        public YisoUseItem.Effect effect;
        
        public override YisoItem CreateItem() => new YisoUseItem(this);
    }

    /// <summary>
    /// ID
    /// 00-99: Effect Item (Potion, Buff etc)
    /// 100-199: Arrow Item
    /// </summary>
    [Serializable]
    public class YisoUseItem : YisoItem, IYisoBuff {
        private readonly Effect itemEffect;
        
        public YisoUseItem(YisoUseItemSO so) : base(so) {
            itemEffect = so.effect;
        }

        public YisoUseItem(YisoItem item) : base(item) {
            var useItem = (YisoUseItem) item;
            itemEffect = useItem.itemEffect;
        }

        public int Duration => itemEffect.duration;

        public bool CanOverlap => itemEffect.overlapEffect;
        
        public double TotalValue => itemEffect.value;

        public bool IsArrow => itemEffect.arrow;

        public int ArrowAttack => itemEffect.arrowAttack;
        
        public void OnStart(IYisoEntity entity) {
            var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            var title = itemEffect.GetName(currentLocale);
            Debug.Log($"[USE:{title}] Started");
            var player = (YisoPlayer) entity;
            var value = itemEffect.value;
            player.StatModule.StartBuff(SourceId, itemEffect.buffStat, value);
        }

        public void OnProgress(IYisoEntity entity, float progress) {
            
        }

        public void OnComplete(IYisoEntity entity) {
            var currentLocale = YisoServiceProvider.Instance.Get<IYisoGameService>().GetCurrentLocale();
            var title = itemEffect.GetName(currentLocale);
            Debug.Log($"[USE:{title}] Completed");
            var player = (YisoPlayer) entity;
            player.StatModule.CompleteBuff(SourceId);
        }
        
        public void OnOverTimeProgress(IYisoEntity entity, double valuePerSeconds) {
            if (itemEffect.buffStat != YisoBuffEffectTypes.HP_REC_INC) return;
            
        }

        public int CoolDown => itemEffect.coolDown;

        public bool IsOverTime {
            get {
                if (itemEffect.buff) return false;
                return itemEffect.buffStat == YisoBuffEffectTypes.HP_REC_INC;
            }
        }

        public string GetUIDescription(YisoLocale.Locale locale) {
            var builder = new StringBuilder();
            if (itemEffect.arrow) {
                builder.Append(locale == YisoLocale.Locale.KR ? "활 공격에 필요한 화살입니다." : "A arrow that shoot ");
                builder.Append("\n\n");
                builder.Append($"공격력: {itemEffect.arrowAttack.ToCommaString()}");
                return builder.ToString();
            }
            var (type, value) = (itemEffect.buffStat, itemEffect.value);
            var uiStr = type.ToDescription(locale);
            var uiValueStr = type.ToUIValue(value);
            if (type == YisoBuffEffectTypes.HP_REC_INC) {
                builder.Append(locale == YisoLocale.Locale.KR ? "체력을 회복시키는 물약입니다." : "A potion that recovery hp");
            }
            else {
                if (locale == YisoLocale.Locale.KR) {
                    builder.Append(uiStr).Append(" 물약입니다.");
                } else {
                    builder.Append("A potion that ").Append(uiStr);
                }
            }
            
            builder.Append("\n\n");
            
            builder.Append($"{type.ToString(locale)}: {uiValueStr}").Append("\n");

            if (type == YisoBuffEffectTypes.HP_REC_INC) {
                builder.Append(locale == YisoLocale.Locale.KR
                    ? $"회복 시간: {itemEffect.duration}초"
                    : $"Recovery Time: {itemEffect.duration}s").Append("\n");
            } else {
                builder.Append(locale == YisoLocale.Locale.KR
                    ? $"지속 시간: {itemEffect.duration}초"
                    : $"Duration: {itemEffect.duration}s").Append("\n");
            }

            if (CoolDown > 0) {
                builder.Append(locale == YisoLocale.Locale.KR
                    ? $"재사용 대기시간: {CoolDown}초"
                    : $"CoolDown: {CoolDown}s").Append("\n");
            }


            return builder.ToString();
        }
        
        public int SourceId => Id;

        [Serializable]
        public class Effect {
            [Title("Buff")]
            public bool buff = false;
            [ShowIf("@this.buff && !this.arrow")] public YisoBuffEffectTypes buffStat;
            [Title("Amount"), ShowIf("@!this.arrow")]
            [Required] public int value;
            [Required, ShowIf("@!this.arrow")] public int duration;
            [Title("Cool Down"), ShowIf("@!this.arrow")] public bool cool;
            [ShowIf("@this.cool && !this.arrow")] public int coolDown;
            [Title("Overlap"), ShowIf("@!this.arrow")] public bool overlapEffect;
            [Title("Arrow")] public bool arrow = false;
            [ShowIf("arrow")] public int arrowAttack = 5;

            public string GetName(YisoLocale.Locale locale) => buffStat.ToString(locale);

            public string ToUITitle(YisoLocale.Locale locale) => buffStat.ToString(locale);

            public string ToUIValue() => buffStat.ToUIValue(value);
        }
    }
}