using Core.Domain.Locale;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene
{
    [CreateAssetMenu(fileName = "Character", menuName = "Yiso/Cutscene/CharacterInfo")]
    public class YisoCutSceneCharacterInfoSO : ScriptableObject
    {
        public Sprite characterIcon;
        public YisoLocale name;

        public YisoCutsceneCharacterInfo CreateCutSceneCharacterInfo() => new YisoCutsceneCharacterInfo(this);
    }

    public class YisoCutsceneCharacterInfo {
        public Sprite characterIcon;
        public YisoLocale name;

        public YisoCutsceneCharacterInfo(YisoCutSceneCharacterInfoSO so)
        {
            characterIcon = so.characterIcon;
            name = so.name;
        }
    }
}