using System.Collections.Generic;
using System.Linq;
using Character.Core;
using Core.Behaviour;
using Manager;
using UnityEngine;

namespace Controller.Map {
    /// <summary>
    /// Trigger 영역 내부로 target Layer에 해당하는 character가 들어오면 등록된 캐릭터를 unfreeze, 나가면 freeze시킴 
    /// </summary>
    [AddComponentMenu("Yiso/Controller/Map/ActivationZoneController")]
    public class YisoActivationZoneController : RunIBehaviour {
        public List<YisoCharacter> registeredCharacters;
        public LayerMask targetLayerMask = LayerManager.PlayerLayerMask;
        public bool playOnce = false;

        private bool isPlayed = false;

        protected virtual void FreezeCharacters() {
            if (playOnce && isPlayed) return;
            if (registeredCharacters == null || registeredCharacters.Count <= 0) return;
            foreach (var character in registeredCharacters.Where(character => character.gameObject.activeInHierarchy)
                .Where(character =>
                    character.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                character.Freeze(YisoCharacterStates.FreezePriority.FieldEnter);
            }

            isPlayed = true;
            if (playOnce) {
                Destroy(gameObject);
            }
        }

        protected virtual void UnfreezeCharacters() {
            if (playOnce && isPlayed) return;
            if (registeredCharacters == null || registeredCharacters.Count <= 0) return;
            foreach (var character in registeredCharacters.Where(character => character.gameObject.activeInHierarchy)
                .Where(character =>
                    character.conditionState.CurrentState != YisoCharacterStates.CharacterConditions.Dead)) {
                character.UnFreeze(YisoCharacterStates.FreezePriority.Respawn |
                                   YisoCharacterStates.FreezePriority.FieldEnter);
            }

            isPlayed = true;
            if (playOnce) {
                Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            UnfreezeCharacters();
        }

        protected virtual void OnTriggerExit2D(Collider2D other) {
            if (!LayerManager.CheckIfInLayer(other.gameObject, targetLayerMask)) return;
            if (playOnce) return;
            FreezeCharacters();
        }
    }
}