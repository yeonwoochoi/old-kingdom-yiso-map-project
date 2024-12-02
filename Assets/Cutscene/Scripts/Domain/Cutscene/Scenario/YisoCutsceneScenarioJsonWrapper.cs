using System;
using System.Collections.Generic;

namespace Cutscene.Scripts.Domain.Cutscene.Scenario
{
    [Serializable]
    public class YisoCutsceneScenarioJson
    {
        public int stage;
        public LoadingComment loadingComment;
        public List<List<StageDirection>> stageDirections;
        public List<Dialog> dialogs;


        [Serializable]
        public class Dialog
        {
            public string character;
            public string dialogKr;
            public string dialogEn;
            public string readingTimeKr;
            public string readingTimeEn;
        }
    
        [Serializable]
        public class StageDirection
        {
            public string directionKr;
            public string directionEn;
            public string readingTimeKr;
            public string readingTimeEn;
        }
    
        [Serializable]
        public class LoadingComment
        {
            public string commentKr;
            public string commentEn;
            public string readingTimeKr;
            public string readingTimeEn;
        }

        public void ConvertLoadingComment(ref YisoCutsceneLoadingCommentsSO so)
        {
            so.loadingComments = new List<YisoCutsceneLoadingCommentsSO.LoadingComment>();
            var comment = new YisoCutsceneLoadingCommentsSO.LoadingComment
            {
                comment =
                {
                    kr = loadingComment.commentKr,
                    en = loadingComment.commentEn
                },
                readingTime =
                {
                    kr = loadingComment.readingTimeKr,
                    en = loadingComment.readingTimeEn
                }
            };
            so.loadingComments.Add(comment);
        }

        public void ConvertStageDirection(ref YisoCutsceneStageDirectionSO so, int index)
        {
            so.directions = new List<YisoCutsceneStageDirectionSO.Direction>();
            foreach (var direction in stageDirections[index])
            {
                var comment = new YisoCutsceneStageDirectionSO.Direction
                {
                    direction =
                    {
                        kr = direction.directionKr,
                        en = direction.directionEn
                    },
                    readingTime =
                    {
                        kr = direction.readingTimeKr,
                        en = direction.readingTimeEn
                    },
                    pause = true
                };
                so.directions.Add(comment);
            }
        }

        public void ConvertDialog(ref YisoCutsceneDialogSO so)
        {
            foreach (var dialog in dialogs)
            {
                var message = new YisoCutsceneDialogSO.Message
                {
                    message =
                    {
                        kr = dialog.dialogKr,
                        en = dialog.dialogEn
                    },
                    readingTime = 
                    {
                        kr = dialog.readingTimeKr,
                        en = dialog.readingTimeEn
                    },
                    uiPosition = UIPosition.TopLeft,
                    characterSo = null,
                    pause = true
                };

                so.messages.Add(message);
            }
        }
    }
}