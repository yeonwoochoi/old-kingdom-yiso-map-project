using System;
using System.Collections.Generic;
using Core.Domain.Locale;
using UnityEngine;

namespace Cutscene.Scripts.Domain.Cutscene.Scenario
{
    [CreateAssetMenu(fileName = "Cartoon", menuName = "Yiso/Cutscene/Cartoon")]
    public class YisoCutsceneCartoonSO: ScriptableObject
    {
        public List<Cartoon> cartoons = new List<Cartoon>();

        public YisoCutsceneCartoon CreateCartoon() => new YisoCutsceneCartoon(this);

        [Serializable]
        public class Cartoon
        {
            public YisoLocale direction;
            public Sprite image;
            public YisoLocale readingTime = new YisoLocale();
        }
    }

    public class YisoCutsceneCartoon
    {
        public List<Cartoon> cartoons;
        public YisoCutsceneCartoon(YisoCutsceneCartoonSO so)
        {
            cartoons = new List<Cartoon>();
            foreach (var cartoon in so.cartoons)
            {
                cartoons.Add(new Cartoon(cartoon));
            }
        }
    }
    
    [Serializable]
    public class Cartoon
    {
        public YisoLocale direction;
        public Sprite image;
        public YisoLocale readingTime;

        public Cartoon(YisoCutsceneCartoonSO.Cartoon cartoonImage)
        {
            direction = cartoonImage.direction;
            image = cartoonImage.image;
            readingTime = cartoonImage.readingTime;
        }
    }
}