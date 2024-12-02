using System;
using Core.Domain.Actor.Player.Modules.UI;
using Core.Domain.Skill;
using Utils.ObjectId;

namespace Core.Domain.Actor.Player.Modules.Skill {
    public abstract class YisoPlayerSkillEventArgs : EventArgs {
        
    }

    public class YisoPlayerSkillPointChangedEventArgs : YisoPlayerSkillEventArgs {
        public int BeforePoint { get; }
        public int AfterPoint { get; }

        public YisoPlayerSkillPointChangedEventArgs(int beforePoint, int afterPoint) {
            BeforePoint = beforePoint;
            AfterPoint = afterPoint;
        }
    }

    public class YisoPlayerSkillUpgradedEventArgs : YisoPlayerSkillEventArgs {
        public YisoSkill UpgradedSkill { get; }

        public YisoPlayerSkillUpgradedEventArgs(YisoSkill upgradedSkill) {
            UpgradedSkill = upgradedSkill;
        }
    }
    
    public class YisoPlayerSkillCastEventArgs : YisoPlayerSkillEventArgs {
        public int SkillId { get; }

        public YisoPlayerSkillCastEventArgs(int skillId) {
            SkillId = skillId;
        }
    }

    public class YisoPlayerSkillStartCooldownEventArgs : YisoPlayerSkillEventArgs {
        public YisoObjectID HolderId { get; }

        public YisoPlayerSkillStartCooldownEventArgs(YisoObjectID holderId) {
            HolderId = holderId;
        }
    }
}