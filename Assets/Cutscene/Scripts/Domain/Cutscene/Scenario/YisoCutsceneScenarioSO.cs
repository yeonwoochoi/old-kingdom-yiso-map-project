using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene.Scenario
{
    [CreateAssetMenu(fileName = "Scenario", menuName = "Yiso/Cutscene/Scenario")]
    public class YisoCutsceneScenarioSO : ScriptableObject
    {
        public int stage;
        public YisoCutsceneLoadingCommentsSO loadingComments;
        public List<YisoCutsceneStageDirectionSO> stageDirections;
        public List<YisoCutsceneDialogSO> dialogs;

        public YisoCutsceneScenario CreateCutSceneScenario() => new YisoCutsceneScenario(this);
    }

    public class YisoCutsceneScenario {
        public int stage;
        public YisoCutsceneLoadingComments loadingComments;
        public List<YisoCutsceneStageDirection> stageDirections;
        public List<YisoCutsceneDialog> dialogs;

        public YisoCutsceneScenario(YisoCutsceneScenarioSO so)
        {
            stage = so.stage;
            
            loadingComments = new YisoCutsceneLoadingComments(so.loadingComments);
            
            stageDirections = new List<YisoCutsceneStageDirection>();
            foreach (var stageDirection in so.stageDirections.Select(t => t.CreateCutSceneDirections()))
            {
                stageDirections.Add(stageDirection);
            }
            
            dialogs = new List<YisoCutsceneDialog>();
            foreach (var dialog in so.dialogs.Select(t => t.CreateCutSceneDialog()))
            {
                dialogs.Add(dialog);
            }
        }
    }
}