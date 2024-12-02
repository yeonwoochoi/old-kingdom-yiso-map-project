using System;
using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.Quest;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Menu.Quest {
    public class YisoMenuQuestItemsUI : RunIBehaviour {
        [SerializeField, Title("Prefabs")] private GameObject questItemPrefab;
        [SerializeField] private GameObject content;
        [SerializeField, Title("Toggle")] private ToggleGroup toggleGroup;
        [SerializeField, Title("Defaults")] private YisoMenuQuestItemUI[] defaultQuestItems;

        public event UnityAction<YisoQuest> OnEventRaised; 
        
        private readonly List<YisoMenuQuestItemUI> questItems = new();
        
        private void InitItems() {
            questItems.AddRange(defaultQuestItems);
            for (var i = 0; i < questItems.Count; i++) {
                questItems[i].Init();
                questItems[i].QuestToggle.onValueChanged.AddListener(OnClickQuest(i));
            }
        }
        
        public void Clear() {
            foreach (var item in questItems) {
                item.Clear();
                item.gameObject.SetActive(false);
            }
        }

        public void SetQuest(YisoQuest quest) {
            if (questItems.Count == 0) InitItems();
            
            if (!TryGetNonActiveIndex(out var index)) {
                var newItem = CreateQuestItem();
                newItem.QuestToggle.onValueChanged.AddListener(OnClickQuest(questItems.Count - 1));
                questItems.Add(newItem);
                newItem.gameObject.SetActive(true);
                newItem.Active = true;
                index = questItems.Count - 1;
            }
            
            questItems[index].SetQuest(quest);
        }

        public void UpdateQuest(YisoQuest quest) {
            var item = questItems.Find(i => i.Quest.Id == quest.Id);
            if (item == null) return;
            item.UpdateQuest(quest);
        }

        private bool TryGetNonActiveIndex(out int index) {
            index = -1;

            for (var i = 0; i < questItems.Count; i++) {
                if (questItems[i].Active) continue;
                questItems[i].gameObject.SetActive(true);
                questItems[i].Active = true;
                index = i;
                break;
            }

            return index != -1;
        }

        private YisoMenuQuestItemUI CreateQuestItem() {
            var controller = CreateObject<YisoMenuQuestItemUI>(questItemPrefab, content.transform);
            controller.QuestToggle.group = toggleGroup;
            return controller;
        }
        
        private UnityAction<bool> OnClickQuest(int index) => flag => {
            if (!flag) return;
            var quest = questItems[index].Quest;
            OnEventRaised?.Invoke(quest);
        };
    }
}