namespace Core.Domain.Actor.Player.Modules.Stat {
    public class YisoPlayerStatModuleDeprecated {
        #region DEPRECATED_STATS

        /*private readonly Dictionary<int, int> equipSetHandles = new();
        private readonly YisoStats baseStats = new();
        private readonly YisoStats localStats = new();
        private readonly YisoStats buffStats = new(true);

        public YisoStats BaseStats => baseStats;
        public YisoStats LocalStats => localStats;
        public YisoStats BuffStats => buffStats;

        public int GetBaseStatValue(YisoEquipStat stat) => baseStats.GetValue(stat);

        public int GetLocalStatValue(YisoEquipStat stat) => localStats.GetValue(stat);

        public double GetNormalizedLocalStatValue(YisoEquipStat stat) => localStats.GetNormalizedValue(stat);

        public void SetBaseStatValue(YisoEquipStat stat, int value) {
            baseStats.SetValue(stat, value, false);
        }

        public void SetLocalStatValue(YisoEquipStat stat, int value, bool notify = true) {
            localStats.SetValue(stat, value, notify);
        }

        public int GetBuffStatValue(YisoEquipStat stat) => buffStats.GetValue(stat);

        public double GetNormalizedBuffStatValue(YisoEquipStat stat) => buffStats.GetNormalizedValue(stat);

        public void SetBuffStatValue(YisoEquipStat stat, int value, bool notify = true) {
            buffStats.SetValue(stat, value, notify);
        }
        
        private void CalculateStats(bool withBuff = false, bool useDefaultStats = false, bool notify = false) {
            equipSetHandles.Clear();
            localStats.Copy(baseStats);

            LocalMaxHp = MaxHp;

            var dataService = YisoServiceProvider.Instance.Get<IYisoDataService>();
            foreach (var equip in player.InventoryModule.EquippedItemPositions.Values.Select(position =>
                             player.InventoryModule.InventoryUnits[YisoItem.InventoryType.EQUIP][position])
                         .Cast<YisoEquipItem>()) {
                if (equip.Slot == YisoEquipSlots.WEAPON && useDefaultStats) {
                    SetLocalStatValue(YisoEquipStat.ATTACK, YisoGameConstants.DEFAULT_ATTACK, notify);
                    SetLocalStatValue(YisoEquipStat.DEFENCE, YisoGameConstants.DEFAULT_DEFENCE, notify);
                    SetLocalStatValue(YisoEquipStat.CRI_PERCENT, 30, false);
                    continue;
                }

                SetLocalStatValue(YisoEquipStat.ATTACK, equip.GetAttack());
                SetLocalStatValue(YisoEquipStat.DEFENCE, equip.GetDefence());

                if (equip.ExistPotential) {
                    foreach (var potential in equip.Potentials.Values) {
                        HandlePotential(localStats, potential, notify);
                    }
                }

                var setItemId = equip.SetItemId;
                if (setItemId == -1) continue;
                if (!equipSetHandles.TryAdd(setItemId, 1))
                    equipSetHandles[setItemId]++;
            }

            foreach (var (id, value) in equipSetHandles) {
                if (!dataService.TryGetSetItem(id, out var setItem)) continue;
                if (value == 1) continue;
                var keys = new int[value];
                for (var i = 0; i < value; i++) keys[i] = i + 2;

                var effects = keys.SelectMany(k => setItem.Effects[k]).ToArray();
                foreach (var effect in effects) {
                    var e = effect.effect;
                    var v = effect.value;
                    HandleSetItemEffect(localStats, e, v, notify);
                }
            }

            if (withBuff) {
                foreach (var stat in YisoStats.ACTIVE_STATS) {
                    var buffValue = GetBuffStatValue(stat);
                    if (buffValue <= 0) {
                        SetBuffStatValue(stat, 0, notify);
                        continue;
                    }

                    SetLocalStatValue(stat, buffValue, notify);
                }
            }

            var incMaxHp = Mathf.CeilToInt(LocalMaxHp * (localStats.GetValue(YisoEquipStat.MHP_INC) * 0.01f));
            LocalMaxHp += incMaxHp;

            OnLocalMaxHpChangedEvent?.Invoke(LocalMaxHp);
        }

        public void CalculateCombatRatingDeprecated(bool useDefaultStats = false, bool notify = false) {
            CalculateStats(useDefaultStats: useDefaultStats, notify: notify);
            var finalAttack = GetLocalStatValue(YisoEquipStat.ATTACK)
                              * (1 + GetNormalizedLocalStatValue(YisoEquipStat.ATTACK_INC))
                              * (1 + GetNormalizedLocalStatValue(YisoEquipStat.ATTACK_DMG_INC))
                              * (1 + GetNormalizedLocalStatValue(YisoEquipStat.BOSS_DMG_INC));
            if (GetNormalizedLocalStatValue(YisoEquipStat.CRI_PERCENT) > 0) {
                finalAttack *= (1 + GetNormalizedLocalStatValue(YisoEquipStat.CRI_DMG_INC));
            }
            finalAttack *= (1 + GetNormalizedLocalStatValue(YisoEquipStat.FINAL_DMG_INC));

            var finalDefence = GetLocalStatValue(YisoEquipStat.DEFENCE)
                               * (1 + GetNormalizedLocalStatValue(YisoEquipStat.DEFENCE_INC))
                               * (1 - GetNormalizedLocalStatValue(YisoEquipStat.DEFENCE_DMG_DEC));

            CombatRating = finalAttack + finalDefence + LocalMaxHp;

            YisoServiceProvider.Instance.Get<IYisoCombatRatingService>()
                .CalculatePlayerCombatRating(player);

            if (!notify) return;
            OnCombatRatingChangedEvent?.Invoke(CombatRating);
            OnStatsUIEvent?.Invoke(new StatsUIEventArgs(
                GetLocalStatValue(YisoEquipStat.ATTACK),
                GetLocalStatValue(YisoEquipStat.DEFENCE),
                CombatRating
            ));
        }
        
        public double CalculateDamage(IYisoCombatableEntity entity, out bool isCritical) {
            isCritical = Randomizer.Below(GetNormalizedLocalStatValue(YisoEquipStat.CRI_PERCENT));
            var enemy = (YisoEnemy) entity;
            var isBoss = enemy.Type == YisoEnemyTypes.BOSS;
            var targetDefence = enemy.GetDefence();

            var effectiveAttack = GetLocalStatValue(YisoEquipStat.ATTACK)
                                  * (1 + GetNormalizedLocalStatValue(YisoEquipStat.ATTACK_INC))
                                  * (1 + GetNormalizedLocalStatValue(YisoEquipStat.ATTACK_DMG_INC));

            if (isBoss) effectiveAttack *= (1 + GetNormalizedLocalStatValue(YisoEquipStat.BOSS_DMG_INC));

            if (isCritical) effectiveAttack *= (1 + GetNormalizedLocalStatValue(YisoEquipStat.CRI_DMG_INC));

            var finalDamage = effectiveAttack * (1 + GetNormalizedLocalStatValue(YisoEquipStat.FINAL_DMG_INC));
            var effectiveDefence = targetDefence * (1 - GetNormalizedLocalStatValue(YisoEquipStat.IGNORE_TARGET_DEF));
            finalDamage -= effectiveDefence;
            finalDamage = Mathf.Max(0, (float) finalDamage);

            var minDamage = finalDamage * 0.8f;
            var maxDamage = finalDamage;

            return minDamage + (Randomizer.Next() * (maxDamage - minDamage));
        }

        private void HandlePotential(YisoStats stats, YisoEquipItem.Potential potential, bool notify = false) {
            stats.SetValue(potential.stat, potential.value, notify);
        }

        private void HandleSetItemEffect(YisoStats stats, YisoEquipStat effect, int value, bool notify = false) {
            stats.SetValue(effect, value, notify);
        }*/

        

        #endregion

        #region DEPRECATED_LEVEL

        
        
        /*public event UnityAction<int> OnLevelChangedEvent;
        public event UnityAction<double> OnExpChangedEvent;
        public event UnityAction<int> OnLocalMaxHpChangedEvent;
        
        public int Level { get; private set; } = 1;
        
        public void GainExp(double exp) {
            return;
            /*var currentExp = Exp;
            if (!TryGetNextLevelExp(out var neededExp)) return;
            var changed = 0d;
            var totalExp = currentExp + exp;
            if (totalExp > neededExp) {
                LevelUp();
                var diff = totalExp - neededExp;
                Exp = diff;
                changed = diff / neededExp;
            }
            else if (Math.Abs(totalExp - neededExp) < 0.001d) {
                LevelUp();
                Exp = 0;
            }
            else {
                Exp = totalExp;
                changed = totalExp / neededExp;
            }

            OnExpChangedEvent?.Invoke(changed);#1#
        }
        
        private void LevelUp() {
            Level += 1;

            OnLevelChangedEvent?.Invoke(Level);

            hp = LocalMaxHp;
            RaiseHpChanged();
        }
        
        private bool TryGetNextLevelExp(out double exp) {
            exp = -1;
            if (Level == 100) return false;
            exp = YisoServiceProvider.Instance.Get<IYisoCombatRatingService>().CalculateExp(Level + 1);
            return true;
        }*/
        
        #endregion
        
        #region DEPRECATED_CR_V2
        
        /*public void CalculateCombatRating(bool applyHp = false) {
            var itemService = GetService<IYisoItemService>();
            var stageService = GetService<IYisoStageService>();
            var stageCR = stageService.GetCurrentStageCR();
            BaseCombatRating = stageCR * 0.1;
            MaxHp = Mathf.CeilToInt((float)BaseCombatRating * 10);
            var equippedItems = player.InventoryModule.EquippedUnit.GetEquippedItems().ToList();
            var itemCombatRating = 0d;

            equipSetHandles.Clear();
            ResetAdditionalStats();

            foreach (var item in equippedItems) {
                itemCombatRating += item.CombatRating;

                if (item.Potentials.PotentialExist) {
                    foreach (var key in YisoEquipPotentials.KEYS) {
                        if (!item.Potentials.TryGetValue(key, out var potential)) continue;
                        additionalStats[potential.Type] += potential.Value;
                    }
                }

                var setItemId = item.SetItemId;
                if (setItemId == -1) continue;
                if (!equipSetHandles.TryAdd(setItemId, 1))
                    equipSetHandles[setItemId]++;
            }

            foreach (var (id, value) in equipSetHandles) {
                if (!itemService.TryGetSetItem(id, out var setItem)) continue;
                if (value == 1) continue;
                var keys = new int[value - 1];
                for (var i = 0; i < keys.Length; i++) keys[i] = i + 2;

                var effects = keys.SelectMany(k => setItem.Effects[k]).ToArray();
                foreach (var effect in effects) {
                    additionalStats[effect.effect] += effect.value;
                }
            }

            baseWithItemCombatRating = Mathf.CeilToInt((float)BaseCombatRating + (float)itemCombatRating);
            CombatRating = baseWithItemCombatRating;
            if (applyHp) {
                SetHp(MaxHp);
            }

            var appendLogStr = "";
            if (YisoServiceProvider.Instance.Get<IYisoGameService>().IsDevelopMode()) {
                CombatRating = 99999999;
                appendLogStr = "Develop";
            }
            else
                CombatRating *= (1 + GetBuffAndAdditionalValue(YisoBuffEffectTypes.CR_INC));
            
            YisoLogger.Log($"[Player] Stage: {stageService.GetCurrentStageId()} {appendLogStr} Combat Rating: {Mathf.CeilToInt((float) CombatRating)}, MaxHp: {MaxHp}");
            OnCombatRatingChangedEvent?.Invoke(CombatRating);
            const float factor = 0.7654321f;
            var attack = Mathf.CeilToInt((float)CombatRating * factor);
            var defence = Mathf.CeilToInt((float)CombatRating * (1 - factor));
            OnStatsUIEvent?.Invoke(new StatsUIEventArgs(attack, defence, CombatRating));
        }*/
        
        #endregion
    }
}