using System;
using Core.Behaviour;
using Core.Domain.Quest;
using Core.Domain.Quest.SO;
using Core.Logger;
using Core.Service;
using Core.Service.Character;
using Core.Service.Log;
using Sirenix.OdinInspector;
using Tools.Event;
using UnityEngine;

namespace Controller.Quest {
    public struct YisoQuestTargetPositionRegisterEvent {
        public YisoQuestTargetPositionRegisterer.QuestTarget questTarget;

        public YisoQuestTargetPositionRegisterEvent(YisoQuestTargetPositionRegisterer.QuestTarget questTarget) {
            this.questTarget = questTarget;
        }

        static YisoQuestTargetPositionRegisterEvent e;

        public static void Trigger(YisoQuestTargetPositionRegisterer.QuestTarget questTarget) {
            e.questTarget = questTarget;
            YisoEventManager.TriggerEvent(e);
        }
    }

    [AddComponentMenu("Yiso/Controller/Quest/QuestTargetPositionRegisterer")]
    public class YisoQuestTargetPositionRegisterer : RunIBehaviour {
        public QuestTarget questTarget;

        protected override void Start() {
            YisoQuestTargetPositionRegisterEvent.Trigger(questTarget);
        }

        [Serializable]
        public class QuestTarget {
            [SerializeField] private YisoQuestSO questSO;
            [SerializeField] private bool isSamePosition = true;

            [SerializeField, ShowIf("isSamePosition")]
            public Transform target;

            [SerializeField, ShowIf("@!isSamePosition")]
            public Transform startTarget;

            [SerializeField, ShowIf("@!isSamePosition")]
            public Transform completeTarget;

            public int QuestId => questSO.id;
            public YisoLogger LogService => YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<QuestTarget>();

            public Vector2 GetTargetPosition() {
                if (isSamePosition) {
                    return target.position;
                }

                var currentState = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().QuestModule
                    .GetStatusByQuestId(QuestId);
                switch (currentState) {
                    case YisoQuestStatus.IDLE:
                    case YisoQuestStatus.READY:
                        return startTarget.position;
                    case YisoQuestStatus.PROGRESS:
                    case YisoQuestStatus.PRE_COMPLETE:
                    case YisoQuestStatus.COMPLETE:
                        return completeTarget.position;
                    default:
                        LogService.Warn($"[QuestTarget] : Could not find Quest Status '${QuestId}'");
                        return Vector2.zero;
                }
            }
        }
    }
}