#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI {
    public class SelectionNavigationDisabler : OdinEditorWindow {

        [MenuItem("Tools/Disable UI Navigation")]
        public static void ShowWindow() {
            GetWindow<SelectionNavigationDisabler>("Disable UI Navigation");
        }

        [SerializeField, Required]
        private GameObject rootObject;

        [Button, EnableIf("@this.rootObject != null")]
        public void Disable() {
            DisableNavigationRecursive(rootObject);
        }

        private void DisableNavigationRecursive(GameObject currentObject) {
            var selectable = currentObject.GetComponent<Selectable>();
            if (selectable != null) {
                var navigation = selectable.navigation;
                navigation.mode = Navigation.Mode.None;
                selectable.navigation = navigation;
            }

            foreach (Transform child in currentObject.transform) {
                DisableNavigationRecursive(child.gameObject);
            }
        }
    }
}

#endif