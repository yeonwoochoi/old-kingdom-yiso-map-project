using System.Collections.Generic;

namespace Core.Domain.Actor.Attack {
    public class YisoAttackResult {
        public bool Death { get; }

        public YisoAttackResult(bool death) {
            Death = death;
        }

        public override string ToString() {
            return $"Death: {Death}";
        }
    }
}