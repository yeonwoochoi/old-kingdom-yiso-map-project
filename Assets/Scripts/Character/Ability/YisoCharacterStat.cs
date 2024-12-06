using System;
using Character.Core;
using Controller.Map;
using Core.Domain.Actor.Ally;
using Core.Domain.Actor.Enemy;
using Core.Domain.Actor.Erry;
using Core.Domain.Actor.Npc;
using Core.Domain.Actor.Player;
using Core.Domain.Entity;
using Core.Domain.Quest;
using Core.Service;
using Core.Service.Character;
using Core.Service.Temp;
using Manager_Temp_;
using Manager_Temp_.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.Ability {
    [AddComponentMenu("Yiso/Character/Abilities/Character Stat")]
    public class YisoCharacterStat : YisoCharacterAbility {
        public enum CharacterRoleType {
            Player,
            Erry,
            Enemy,
            Ally,
            Npc
        }

        public bool isQuestCharacter = true; // true이면 퀘스트 requirement update함
        public CharacterRoleType roleType = CharacterRoleType.Enemy;

        [ShowIf("roleType", CharacterRoleType.Enemy), SerializeField]
        protected YisoEnemySO enemyStatSO;

        [ShowIf("roleType", CharacterRoleType.Ally), SerializeField]
        protected YisoAllySO allyStatSO;

        [ShowIf("roleType", CharacterRoleType.Npc), SerializeField]
        protected YisoNpcSO npcStatSO;

        protected bool initialized = false;

        protected YisoPlayer playerStat;
        protected YisoErry erryStat;
        protected YisoEnemy enemyStat;
        protected YisoAlly allyStat;
        protected YisoNpc npcStat;

        public IYisoCombatableEntity CombatStat {
            get {
                return roleType switch {
                    CharacterRoleType.Player => playerStat,
                    CharacterRoleType.Erry => erryStat,
                    CharacterRoleType.Enemy => enemyStat,
                    CharacterRoleType.Ally => allyStat,
                    CharacterRoleType.Npc => null,
                    _ => null
                };
            }
        }

        public IYisoEntity EntityStat {
            get {
                return roleType switch {
                    CharacterRoleType.Player => playerStat,
                    CharacterRoleType.Erry => erryStat,
                    CharacterRoleType.Enemy => enemyStat,
                    CharacterRoleType.Ally => allyStat,
                    CharacterRoleType.Npc => npcStat,
                    _ => null
                };
            }
        }

        public bool IsBountyTarget {
            get {
                if (!GameManager.HasInstance || !BountyManager.HasInstance) return false;
                if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Bounty) return false;
                return BountyManager.Instance.CurrentBounty.TargetId == EntityStat.GetId();
            }
        }

        public YisoPlayer PlayerStat => roleType != CharacterRoleType.Player ? null : playerStat;
        public YisoErry ErryStat => roleType != CharacterRoleType.Erry ? null : erryStat;
        public YisoEnemy EnemyStat => roleType != CharacterRoleType.Enemy ? null : enemyStat;
        public YisoAlly AllyStat => roleType != CharacterRoleType.Ally ? null : allyStat;
        public YisoNpc NpcStat => roleType != CharacterRoleType.Npc ? null : npcStat;
        private IYisoTempService TempService => YisoServiceProvider.Instance.Get<IYisoTempService>();
        private YisoGameQuestModule QuestModule => TempService.GetGameManager().GameModules.QuestModule;
        private GameManager.GameMode GameMode => GameManager.Instance.CurrentGameMode;

        protected override void PreInitialization() {
            base.PreInitialization();
            InitializeStat();
        }

        protected override void Initialization() {
            base.Initialization();
            InitializeHealth();
        }

        protected virtual void InitializeStat() {
            if (character.characterType == YisoCharacter.CharacterTypes.Player) {
                roleType = CharacterRoleType.Player;
            }

            var characterService = YisoServiceProvider.Instance.Get<IYisoCharacterService>();
            if (characterService.IsReady()) {
                switch (roleType) {
                    case CharacterRoleType.Player:
                        playerStat = characterService.GetPlayer();
                        break;
                    case CharacterRoleType.Erry:
                        erryStat = characterService.GetPlayer().Erry;
                        break;
                    case CharacterRoleType.Enemy:
                        enemyStat = characterService.CreateEnemy(enemyStatSO, true);
                        break;
                    case CharacterRoleType.Ally:
                        allyStat = characterService.CreateAlly(allyStatSO, true);
                        break;
                    case CharacterRoleType.Npc:
                        npcStat = characterService.CreateNpc(npcStatSO);
                        break;
                }
            }

            initialized = true;
        }

        protected virtual void InitializeHealth() {
            if (character.characterHealth == null) return;
            character.characterHealth.maximumHealth = CombatStat.GetMaxHp();
            character.characterHealth.initialHealth = CombatStat.GetMaxHp();
            character.characterHealth.ResetHealthToMaxHealth();
        }

        protected override void OnRespawn() {
            base.OnRespawn();
            if (!initialized) InitializeStat();
            InitializeHealth();
            if (roleType == CharacterRoleType.Player) {
                playerStat.Revive();
            }
        }

        protected override void OnDeath() {
            base.OnDeath();
            if (!initialized) InitializeStat();
            if (isQuestCharacter) UpdateQuestRequirement();
        }

        protected virtual void UpdateQuestRequirement() {
            if (TempService.GetGameManager() == null) return;
            switch (GameMode) {
                case GameManager.GameMode.Bounty:
                    if (IsBountyTarget) BountyManager.Instance.CompleteBounty();
                    break;
                case GameManager.GameMode.Story:
                    switch (roleType) {
                        case CharacterRoleType.Enemy:
                            if (EnemyStat != null) {
                                QuestModule.UpdateQuestRequirement(YisoQuestRequirement.Types.ENEMY, EnemyStat.Id);
                            }

                            break;
                        case CharacterRoleType.Npc:
                            if (NpcStat != null) {
                                QuestModule.UpdateQuestRequirement(YisoQuestRequirement.Types.NPC, NpcStat.Id);
                            }

                            break;
                    }

                    break;
            }
        }
    }
}