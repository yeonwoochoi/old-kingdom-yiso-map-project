using System.Collections.Generic;
using System.Linq;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Entity;
using UnityEngine.Events;

namespace Core.Domain.Actor.Player.Modules.Pet {
    public class YisoPlayerPetModule : YisoPlayerBaseModule {
        public event UnityAction<YisoPlayerPetEventArgs> OnPetEvent;

        private readonly Dictionary<int, IYisoCombatableEntity> pets = new();
        
        public YisoPlayerPetModule(YisoPlayer player) : base(player) { }

        public void ResetData() {
            pets.Clear();
        }

        public void Register(IYisoCombatableEntity entity, bool notify = false) {
            var id = entity.GetId();
            if (!pets.TryAdd(id, entity)) return;
            if (notify) RaiseEvent(new YisoPlayerPetRegisterEventArgs(id));
        }

        public void Unregister(IYisoCombatableEntity entity, bool notify = false) {
            var id = entity.GetId();
            if (!pets.ContainsKey(id)) return;
            if (pets.Remove(id) && notify) RaiseEvent(new YisoPlayerPetUnregisterEventArgs(id));
        }

        public void UnregisterAll(bool notify = false) {
            foreach (var (id, pet) in pets) {
                if (!notify) continue; 
                RaiseEvent(new YisoPlayerPetUnregisterEventArgs(id));
            }
            
            pets.Clear();
        }

        public IReadOnlyList<int> GetPetIds() => pets.Keys.ToList();

        public IReadOnlyList<IYisoCombatableEntity> GetPets() => pets.Values.ToList();

        public bool TryGet(int id, out IYisoCombatableEntity pet) => pets.TryGetValue(id, out pet);

        public bool TryGet<T>(int id, out T pet) where T : IYisoCombatableEntity {
            pet = default(T);

            if (!pets.TryGetValue(id, out var entity)) return false;

            pet = (T) entity;
            return true;
        }

        private void RaiseEvent(YisoPlayerPetEventArgs args) {
            OnPetEvent?.Invoke(args);
        }
    }
}