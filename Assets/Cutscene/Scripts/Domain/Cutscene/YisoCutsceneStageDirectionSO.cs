using System;
using System.Collections.Generic;
using Core.Domain.Locale;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene
{
    [CreateAssetMenu(fileName = "Stage Direction", menuName = "Yiso/Cutscene/Stage Direction")]
    public class YisoCutsceneStageDirectionSO: ScriptableObject
    {
        public List<Direction> directions;
        
        public YisoCutsceneStageDirection CreateCutSceneDirections() => new YisoCutsceneStageDirection(this);

        [Serializable]
        public class Direction
        {
            public bool pause = true;
            public YisoLocale direction = new YisoLocale();
            public YisoLocale readingTime = new YisoLocale();
        }
    }

    public class YisoCutsceneStageDirection
    {
        public List<Direction> directions;

        public YisoCutsceneStageDirection(YisoCutsceneStageDirectionSO so)
        {
            directions = new List<Direction>();
            foreach (var direction in so.directions)
            {
                directions.Add(new Direction(direction));
            }
        }
    }

    public class Direction
    {
        public bool pause = true;
        public YisoLocale direction;
        public YisoLocale readingTime;

        public Direction(YisoCutsceneStageDirectionSO.Direction so)
        {
            direction = so.direction;
            readingTime = so.readingTime;
            pause = so.pause;
        }
    }
}