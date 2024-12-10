using System;
using System.Collections.Generic;
using Character.Ability;
using Character.Core;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Cutscene.Actions {
    [AddComponentMenu("Yiso/Tools/Cutscene/Action/CutsceneActionPositionSetter")]
    public class YisoCutsceneActionPositionSetter : YisoCutsceneAction {
        [Serializable]
        public struct CharacterTransform {
            public bool player;
            [ShowIf("@!player")] public Transform character;
            public PositionType positionType;

            [ShowIf("positionType", PositionType.Vector)]
            public Vector3 position;

            [ShowIf("positionType", PositionType.Transform)]
            public Transform transform;

            public YisoCharacter.FacingDirections facingDirection;

            public enum PositionType {
                Vector,
                Transform,
                Player
            }
        }

        public List<CharacterTransform> transforms;


        public override void PerformAction() {
            foreach (var characterTransform in transforms) {
                var position = characterTransform.positionType switch {
                    CharacterTransform.PositionType.Vector => characterTransform.position,
                    CharacterTransform.PositionType.Transform => characterTransform.transform.position,
                    CharacterTransform.PositionType.Player => GameManager.Instance.Player.transform.position,
                    _ => Vector3.zero
                };

                if (characterTransform.player) {
                    if (!GameManager.HasInstance) return;
                    GameManager.Instance.SetPlayerPosition(position, characterTransform.facingDirection);
                }
                else {
                    characterTransform.character.position = position;
                    if (!characterTransform.character.gameObject.activeInHierarchy) return;
                    var character = characterTransform.character.GetComponent<YisoCharacter>();
                    if (character != null) {
                        var orientation2D = character.FindAbility<YisoCharacterOrientation2D>();
                        if (orientation2D != null) orientation2D.Face(characterTransform.facingDirection);
                    }
                }
            }
        }
    }
}