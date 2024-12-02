using System.Collections.Generic;
using Core.Domain.Locale;
using Core.Domain.Skill;
using Core.Domain.Types;
using TMPro;
using UI.Base;
using UnityEngine;
using Utils.Extensions;

namespace UI.Menu.Skill.Description {
    public class YisoMenuSkillDiffDescriptionUI : YisoUIController {
        [SerializeField] private GameObject textPrefab;
        [SerializeField] private TextMeshProUGUI currentLevelDiffText;
        [SerializeField] private GameObject currentLevelDiffContent;
        [SerializeField] private TextMeshProUGUI nextLevelDiffText;
        [SerializeField] private GameObject nextLevelDiffContent;
        
        private readonly Dictionary<DiffTypes, List<TextItem>> textItems = new();
        private readonly List<string> effectStrings = new();

        protected override void Start() {
            base.Start();
            
            foreach (var type in EnumExtensions.Values<DiffTypes>()) {
                textItems[type] = new List<TextItem>();
            }
        }

        public void SetSkill(YisoSkill skill, int stageId) {
            if (skill.IsMasterLevel) {
                var maxStr = CurrentLocale == YisoLocale.Locale.KR ? "최고" : "MAX";
                currentLevelDiffText.SetText($"{GetCurrentLevelStr()} ({maxStr})");
                nextLevelDiffText.SetText($"{GetNextLevelStr()} (-)");
                SetLevelEffect(skill,DiffTypes.CURRENT, skill.MasterLevel);
                return;
            }

            if (skill.IsLocked(stageId) || !skill.IsLearned) {
                currentLevelDiffText.SetText($"{GetCurrentLevelStr()} (-)");
                nextLevelDiffText.SetText($"{GetNextLevelStr()} (1)");
                SetLevelEffect(skill,DiffTypes.NEXT, 1);
                return;
            }
            
            currentLevelDiffText.SetText($"{GetCurrentLevelStr()} ({skill.Level})");
            nextLevelDiffText.SetText($"{GetNextLevelStr()} ({skill.Level + 1})");
            SetLevelEffect(skill, DiffTypes.CURRENT, skill.Level);
            SetLevelEffect(skill, DiffTypes.NEXT, skill.Level + 1);
        }
        
        public void Clear() {
            foreach (var type in EnumExtensions.Values<DiffTypes>()) {
                ClearTextItems(type);
            }
        }
        
        private void SetLevelEffect(YisoSkill skill, DiffTypes type, int level) {
            ClearTextItems(type);
            effectStrings.Clear();
            switch (skill) {
                case YisoPassiveSkill passiveSkill:
                    foreach (var info in passiveSkill.EffectInfos) {
                        var effectTypes = info.Effect;
                        var value = info.GetCurrentValue(level);
                        var toString = effectTypes.ToString(value, CurrentLocale);
                        effectStrings.Add(toString);
                    }
                    break;
                case YisoActiveSkill activeSkill:
                    effectStrings.AddRange(activeSkill.GetEffectStrings(level, CurrentLocale));
                    break;
            }

            foreach (var str in effectStrings) {
                var index = GetEmptyTextIndexTextOrCreate(type);
                textItems[type][index].SetText(str);
                textItems[type][index].Active = true;
            }
        }
        
        private void ClearTextItems(DiffTypes type) {
            foreach (var item in textItems[type]) {
                if (!item.Active) continue;
                item.Active = false;
            }
        }
        
        private int GetEmptyTextIndexTextOrCreate(DiffTypes type) {
            for (var i = 0; i < textItems[type].Count; i++) {
                if (textItems[type][i].Active) continue;
                return i;
            }

            var newItem =
                new TextItem(CreateText(type == DiffTypes.CURRENT ? currentLevelDiffContent : nextLevelDiffContent));
            textItems[type].Add(newItem);
            return textItems[type].Count - 1;
        }
        
        private string GetMasterLevelStr() => CurrentLocale == YisoLocale.Locale.KR ? "최종 레벨" : "Master Level";
        private string GetCurrentLevelStr() => CurrentLocale == YisoLocale.Locale.KR ? "현재 레벨" : "Current Level";
        private string GetNextLevelStr() => CurrentLocale == YisoLocale.Locale.KR ? "다음 레벨" : "Next Level";
        
        private TextMeshProUGUI CreateText(GameObject content) =>
            CreateObject<TextMeshProUGUI>(textPrefab, content.transform);
        
        private enum DiffTypes {
            CURRENT,
            NEXT
        }
        
        private class TextItem {
            private bool active = false;
            private readonly TextMeshProUGUI text;

            public bool Active {
                get => active;
                set {
                    active = value;
                    text.gameObject.SetActive(value);
                    if (!value) text.SetText("");
                }
            }

            public TextItem(TextMeshProUGUI text) {
                this.text = text;
                Active = false;
            }

            public void SetText(string text) {
                this.text.SetText(text);
            }
        }
    }
}