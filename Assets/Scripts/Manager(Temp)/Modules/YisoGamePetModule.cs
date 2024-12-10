using System;
using System.Collections.Generic;
using System.Linq;
using Character.Core;
using Core.Domain.Actor.Player.Modules.Pet;
using Core.Service;
using Core.Service.Character;
using Tools.Event;

namespace Manager.Modules {
    public struct YisoPetEvent {
        public bool isRegistered;
        public YisoCharacter character;
        public int id;

        public YisoPetEvent(YisoCharacter character, int id, bool isRegistered) {
            this.isRegistered = isRegistered;
            this.id = id;
            this.character = character;
        }

        private static YisoPetEvent e;

        public static void Trigger(YisoCharacter character, int id, bool isRegistered) {
            e.isRegistered = isRegistered;
            e.id = id;
            e.character = character;
            YisoEventManager.TriggerEvent(e);
        }
    }

    public class YisoGamePetModule : YisoGameBaseModule, IYisoEventListener<YisoPetEvent> {
        private readonly Settings settings;
        private readonly Dictionary<int, YisoCharacter> registeredPetRef;

        public YisoPlayerPetModule PetModule =>
            YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().PetModule;

        public YisoGamePetModule(GameManager manager, Settings settings) : base(manager) {
            this.settings = settings;
            registeredPetRef = new Dictionary<int, YisoCharacter>();
        }

        public void OnEvent(YisoPetEvent e) {
            if (e.character == null) return;
            if (e.isRegistered) {
                if (!registeredPetRef.ContainsKey(e.id) || registeredPetRef[e.id] == null) {
                    registeredPetRef[e.id] = e.character;
                }
            }
            else {
                if (registeredPetRef.ContainsKey(e.id)) {
                    registeredPetRef.Remove(e.id);
                }
            }
        }

        public YisoCharacter GetPet(int id) {
            if (registeredPetRef == null) return null;
            if (!registeredPetRef.ContainsKey(id)) return null;
            if (!PetModule.TryGet(id, out var pet)) return null;
            return registeredPetRef[id];
        }

        public List<YisoCharacter> GetPets() {
            var petIDs = PetModule.GetPetIds();
            return petIDs.Select(GetPet).Where(pet => pet != null).ToList();
        }

        public bool TryGetPet(int id, out YisoCharacter pet) {
            pet = GetPet(id);
            return pet != null;
        }

        public override void OnEnabled() {
            this.YisoEventStartListening();
        }

        public override void OnDisabled() {
            this.YisoEventStopListening();
        }

        [Serializable]
        public class Settings {
        }
    }
}