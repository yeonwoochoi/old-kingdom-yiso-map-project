using System;
using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Skill;
using UnityEngine.UIElements;

namespace Core.Domain.Actor.Player.Modules.UI {
    public abstract class YisoPlayerSlotUIEventArgs : EventArgs {
        public int Position { get; }
        
        public YisoWeapon.AttackType Weapon { get; }

        protected YisoPlayerSlotUIEventArgs(YisoWeapon.AttackType weapon, int position) {
            Weapon = weapon;
            Position = position;
        }
    }

    public class YisoPlayerSlotSkillUnSetEventArgs : YisoPlayerSlotUIEventArgs {
        public YisoSkill Skill { get; }
        
        public YisoPlayerSlotSkillUnSetEventArgs(YisoWeapon.AttackType weapon, int position, YisoSkill skill) :
            base(weapon, position) {
            Skill = skill;
        }
    }

    public class YisoPlayerSlotSkillSetEventArgs : YisoPlayerSlotUIEventArgs {
        public YisoSkill Skill { get; }


        public YisoPlayerSlotSkillSetEventArgs(YisoWeapon.AttackType weapon, int position, YisoSkill skill) : base(
            weapon, position) {
            Skill = skill;
        }
     }

    public class YisoPlayerSlotSkillReplaceEventArgs : YisoPlayerSlotUIEventArgs {
        public YisoSkill Skill { get; }

        
        public YisoPlayerSlotSkillReplaceEventArgs(YisoWeapon.AttackType weapon, int position, YisoSkill skill) : base(
            weapon, position) {
            Skill = skill;
        }
    }
}