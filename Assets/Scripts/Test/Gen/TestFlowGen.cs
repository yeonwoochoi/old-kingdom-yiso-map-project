using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Domain.Locale;
using Core.Domain.Stage;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using Newtonsoft.Json;
#endif

namespace Test.Gen {
    public class TestFlowGen : MonoBehaviour {
        public YisoStageFlowsSO flowsSO;
        [Sirenix.OdinInspector.FilePath] public string jsonPath;

        [Button]
        public void Gen() {
#if UNITY_EDITOR
            var texts = File.ReadAllText(jsonPath);
            var contents = JsonConvert.DeserializeObject<List<Content>>(texts);

            var items = contents.Select(item => new YisoStageFlowsSO.Content {
                stage = item.id,
                flow = new YisoLocale {
                    kr = item.korean,
                    en = item.english
                }
            });

            flowsSO.contents = new List<YisoStageFlowsSO.Content>(items);
            EditorUtility.SetDirty(flowsSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        [Serializable]
        internal class Content {
            public int id;
            public string korean;
            public string english;
        }  
    }
}