using System;
using Core.Domain.Skill;
using UI.Menu.Skill.Holder;

namespace UI.Menu.Skill {
    public abstract class YisoMenuSkillUIEventArgs : EventArgs { }

    public class YisoMenuSkillSelectedUIEventArgs : YisoMenuSkillUIEventArgs {
        public YisoSkill Skill { get; }

        public YisoMenuSkillSelectedUIEventArgs(YisoSkill skill) {
            Skill = skill;
        }
    }

    public class YisoMenuSkillUnSelectedUIEventArgs : YisoMenuSkillUIEventArgs {
        
    }
}