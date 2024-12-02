#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using Core.Domain.Locale;
using Core.UI.Story;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Test {
    public class TestStoryLoadingComments : MonoBehaviour {
        [Sirenix.OdinInspector.FilePath] public string jsonFilePath;
        public YisoStoryLoadingCommentsSO updateSO;
        
        [Button]
        public void LoadComments() {
            var texts = File.ReadAllText(jsonFilePath);
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<Comment>>>(texts);

            foreach (var (key, comments) in data) {
                var stage = int.Parse(key);
                var com = new YisoStoryLoadingCommentsSO.Comment {
                    stage = stage,
                    comments = new()
                };

                foreach (var comment in comments) {
                    var locale = new YisoLocale {
                        kr = comment.kor,
                        en = comment.en
                    };
                    com.comments.Add(locale);
                }
                
                updateSO.comments.Add(com);
            }
            
            EditorUtility.SetDirty(updateSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Serializable]
        public class Comments {
            public string key;
            public List<Comment> comments;
        }

        [Serializable]
        public class Comment {
            public string kor;
            public string en;
        }
    }
}

#endif