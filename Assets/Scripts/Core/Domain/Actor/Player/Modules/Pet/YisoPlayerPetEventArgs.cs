using System;

namespace Core.Domain.Actor.Player.Modules.Pet {
    public abstract class YisoPlayerPetEventArgs : EventArgs {
        public int Id { get; }

        protected YisoPlayerPetEventArgs(int id) {
            Id = id;
        }
    }

    public class YisoPlayerPetRegisterEventArgs : YisoPlayerPetEventArgs {
        public YisoPlayerPetRegisterEventArgs(int id) : base(id) { }
    }

    public class YisoPlayerPetUnregisterEventArgs : YisoPlayerPetEventArgs {
        public YisoPlayerPetUnregisterEventArgs(int id) : base(id) { }
    }
}