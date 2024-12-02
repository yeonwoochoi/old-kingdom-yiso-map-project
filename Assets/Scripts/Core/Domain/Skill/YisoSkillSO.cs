using System;
using System.Collections.Generic;
using Character.Weapon;
using Core.Domain.Locale;
using Core.Domain.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Domain.Skill {
    [CreateAssetMenu(fileName = "Skill", menuName = "Yiso/Skill/Skill")]
    public class YisoSkillSO : ScriptableObject {
        [Title("Basic Info")]
        public int id;
        [PreviewField] public Sprite icon;
        public new YisoLocale name;
        public YisoLocale description;
        [Range(1, 100)]
        public int unlockStageId = 1;
        public int masterLevel = 10;
        [Title("Relevant Skill")]
        public YisoSkillSO requiredSkillSO = null;
        public YisoSkill.Types type;
        public YisoWeapon.AttackType attackType;

        [ShowIf("@this.requiredSkillSO != null")]
        public int requiredSkillLevel;

        [Title("Type Specific")]
        [ShowIf("type", YisoSkill.Types.PASSIVE), HideLabel]
        public List<Effects> passiveEffects;

        [ShowIf("type", YisoSkill.Types.ACTIVE), HideLabel]
        public Active active;

        [Serializable]
        public class Effects {
            [InfoBox("패시브 종류")]
            public YisoBuffEffectTypes type;
            [InfoBox("패시브 초기 값(%)")]
            public int initValue;
            [InfoBox("패시브 증가량(레벨,%)")]
            public int increasePerLevelValue;
        }

        [Serializable]
        public class Active {
            [FoldoutGroup("초기 값"), LabelText("스킬 공격 타수")]
            public int attackCount = 1;
            [FoldoutGroup("초기 값"), LabelText("스킬 대미지"), SuffixLabel("%")]
            public int damageRate;
            [FoldoutGroup("초기 값"), LabelText("치명타 확률"), SuffixLabel("%")]
            public int criticalRate;
            [FoldoutGroup("초기 값"), LabelText("치명타 대미지"), SuffixLabel("%")]
            public int criticalDamageRate;
            [FoldoutGroup("초기 값"), LabelText("쿨타임")]
            public double cooldown;

            [FoldoutGroup("레벨 증가값"), LabelText("스킬 공격 타수")] public int attackIncCount = 0;
            [FoldoutGroup("레벨 증가값"), LabelText("스킬 대미지"), SuffixLabel("%")] public int damageIncRate;
            [FoldoutGroup("레벨 증가값"), LabelText("치명타 확률"), SuffixLabel("%")] public int criticalIncRate;
            [FoldoutGroup("레벨 증가값"), LabelText("치명타 대미지"), SuffixLabel("%")] public int criticalDamageIncRate;
            [FoldoutGroup("레벨 증가값"), LabelText("쿨타임 감소"), SuffixLabel("%")] public int coolDownDec;
        }

        public YisoSkill CreateSkill() => type == YisoSkill.Types.ACTIVE
                ? new YisoActiveSkill(this)
                : new YisoPassiveSkill(this);
    }
}