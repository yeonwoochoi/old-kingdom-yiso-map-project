using System.Collections.Generic;
using System.Linq;
using Character.Ability.Skill;
using Character.Core;
using Character.Weapon.Aim;
using Core.Domain.Actor.Player.Modules.Skill;
using Core.Service;
using Core.Service.Character;
using Sirenix.OdinInspector;
using Tools.Inputs;
using UnityEngine;
using UnityEngine.Events;
using Utils.Beagle;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Handle Skill")]
    public class YisoCharacterHandleSkill : YisoCharacterAbility {
        protected int currentSkillId = 0;
        
        protected Dictionary<int, YisoCharacterSkill> skillRefs;
        protected List<YisoCharacterSkill> skills;
        protected List<YisoCharacterSkillAttack> attackSkills;

        protected float lastSkillCastTime = 0f;
        protected readonly float skillCastInterval = 0.1f;

        protected const string SkillCastAnimationParameterName = "IsSkillCast";
        protected const string SkillNumberAnimationParameterName = "SkillNumber";
        protected int skillCastAnimationParameter;
        protected int skillNumberAnimationParameter;
        public event UnityAction OnSkillStartEvent; 
        public event UnityAction OnSkillEndEvent;

        public bool IsSkillCasting => skills != null && skills.Count > 0 && skills.Any(skill => skill.SkillCasting);
        public int CurrentSkillId => currentSkillId;
        public int CurrentSkillAnimatorId {
            get {
                if (!skillRefs.ContainsKey(currentSkillId)) return -1;
                return skillRefs[currentSkillId].skillAnimatorNumber;
            }
        }
        public YisoSkillAim CurrentAttackSkillAim {
            get {
                if (attackSkills == null || attackSkills.Count == 0) return null;
                return attackSkills.Where(attackSkill => attackSkill.SkillCasting).Select(attackSkill => attackSkill.SkillAim).FirstOrDefault();
            }
        }

        public YisoPlayerSkillModule SkillModule => YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer().SkillModule;

        protected override void Initialization() {
            base.Initialization();

            skills = character.FindAbilities<YisoCharacterSkill>();
            attackSkills = skills.OfType<YisoCharacterSkillAttack>().ToList();
            skillRefs = new Dictionary<int, YisoCharacterSkill>();

            if (skills != null && skills.Count > 0) {
                foreach (var skill in skills.Where(skill => skill.SkillId >= 0)) {
                    skillRefs[skill.SkillId] = skill;
                }
            }
        }


        #region Core

        public virtual void StartSkillCast(int skillId) {
            if (Time.time - lastSkillCastTime <= skillCastInterval) return;
            if(skillRefs.ContainsKey(skillId)) skillRefs[skillId].StartSkillCast(ref currentSkillId, OnSkillStartEvent, OnSkillEndEvent);
        }

        public virtual void StopSkillCast(int skillId) {
            if (!skillRefs.ContainsKey(skillId)) return;
            if (!skillRefs[skillId].SkillCasting) return;
            skillRefs[skillId].StopSkillCast();
            currentSkillId = -1;
        }

        public override void ProcessAbility() {
            base.ProcessAbility();
            if (IsSkillCasting) {
                lastSkillCastTime = Time.time;
            }
        }

        public virtual bool CanCastSkill(int skillId) {
            if (!skillRefs.ContainsKey(skillId)) return false;
            return skillRefs[skillId].CanCastSkill();
        }
        

        #endregion

        protected override void HandleInput() {
            base.HandleInput();
            // TODO: 나중에 스킬 HUD Slot이랑 연결하기
            if (inputManager.Skill1Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80011002)) {
                    StartSkillCast(80011002);   
                }
            }
        
            if (inputManager.Skill2Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80011004)) {
                    StartSkillCast(80011004);   
                }
            }
        
            if (inputManager.Skill3Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80012001)) {
                    StartSkillCast(80012001);   
                }
            }
        
            if (inputManager.Skill4Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80011003)) {
                    StartSkillCast(80011003);   
                }
            }
        
            if (inputManager.Skill5Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80011001)) {
                    StartSkillCast(80011001);   
                }
            }
        
            if (inputManager.Skill6Button.State.CurrentState is YisoInput.ButtonStates.ButtonDown or YisoInput
                .ButtonStates.ButtonPressed) {
                if (CanCastSkill(80011005)) {
                    StartSkillCast(80011005);   
                }
            }
        }

        protected override void InitializeAnimatorParameters() {
            base.InitializeAnimatorParameters();
            RegisterAnimatorParameter(SkillCastAnimationParameterName, AnimatorControllerParameterType.Bool,
                out skillCastAnimationParameter);
            RegisterAnimatorParameter(SkillNumberAnimationParameterName, AnimatorControllerParameterType.Int,
                out skillNumberAnimationParameter);
        }

        public override void UpdateAnimator() {
            base.UpdateAnimator();

            // TODO 나중에 바꾸기 (Core system이랑 연결하고)
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                YisoAnimatorUtils.UpdateAnimatorInteger(character.Animator, skillNumberAnimationParameter, CurrentSkillAnimatorId, character.AnimatorParameters);
            }
            YisoAnimatorUtils.UpdateAnimatorBool(character.Animator, skillCastAnimationParameter, IsSkillCasting, character.AnimatorParameters);
        }
        
        [Button("[1] Spin Attack")]
        public void SpinAttack() {
            StartSkillCast(80011002);
        }

        [Button("[2] Teleport Attack")]
        public void TeleportAttack() {
            StartSkillCast(80011004);
        }

        [Button("[3] Shatter Thrust")]
        public void ShatterThrust() {
            StartSkillCast(80012001);
        }

        [Button("[4] Dagger Tempest")]
        public void DaggerTempest() {
            StartSkillCast(80011003);
        }

        [Button("[5] Evasive Attack")]
        public void EvasiveAttack() {
            StartSkillCast(80011001);
        }

        [Button("[6] Divine Strike")]
        public void DivineStrike() {
            StartSkillCast(80011005);
        }

        protected virtual void OnSkillEvent(YisoPlayerSkillEventArgs skillEventArgs) {
            switch (skillEventArgs) {
                case YisoPlayerSkillCastEventArgs yisoPlayerSkillCastEventArgs:
                    StartSkillCast(yisoPlayerSkillCastEventArgs.SkillId);
                    break;
                case YisoPlayerSkillPointChangedEventArgs yisoPlayerSkillPointChangedEventArgs:
                    break;
                case YisoPlayerSkillUpgradedEventArgs yisoPlayerSkillUpgradedEventArgs:
                    break;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            SkillModule.OnSkillEvent += OnSkillEvent;
        }

        protected override void OnDisable() {
            base.OnDisable();
            SkillModule.OnSkillEvent -= OnSkillEvent;
        }
    }
}