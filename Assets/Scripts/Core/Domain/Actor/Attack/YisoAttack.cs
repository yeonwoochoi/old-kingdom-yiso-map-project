using System.Collections.Generic;
using System.Linq;

namespace Core.Domain.Actor.Attack {
    public sealed class YisoAttack {
        public List<DamageInfo> Damages { get; } = new();

        public bool IsSingleAttack => Damages.Count == 1;

        public bool ExistCritical => Damages.Any(d => d.IsCritical);

        public double TotalDamage => Damages.Sum(d => d.Damage);
        
        public YisoAttack() { }
        
        public YisoAttack(double damage) {
            Damages.Add(new DamageInfo {
                Damage = damage,
                IsCritical = false
            });
        }
        
        public sealed class DamageInfo {
            public double Damage { get; set; }
            public double DamageRate { get; set; }
            public bool IsCritical { get; set; } = false;
        }
    }
}