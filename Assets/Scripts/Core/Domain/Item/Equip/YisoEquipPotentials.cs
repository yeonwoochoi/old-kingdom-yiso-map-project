using System;
using System.Linq;
using Core.Domain.Data;
using Core.Domain.Locale;
using Core.Domain.Types;
using Unity.VisualScripting;
using Utils.Extensions;

namespace Core.Domain.Item.Equip {
    [Serializable]
    public sealed class YisoEquipPotentials {
        private YisoEquipPotential potential1 = null;
        private YisoEquipPotential potential2 = null;
        private YisoEquipPotential potential3 = null;
        
        public static readonly int[] KEYS = { 1, 2, 3 };
        
        public bool PotentialExist => potential1 != null || potential2 != null || potential3 != null;

        public bool TryGetValue(int key, out YisoEquipPotential potential) {
            potential = null;
            switch (key) {
                case 1:
                    if (potential1 == null) return false;
                    potential = potential1;
                    break;
                case 2:
                    if (potential2 == null) return false;
                    potential = potential2;
                    break;
                case 3:
                    if (potential3 == null) return false;
                    potential = potential3;
                    break;
                default:
                    throw new Exception($"{key} not in 1, 2, 3");
            }

            return true;
        }

        public void SetPotential(int key, YisoBuffEffectTypes type, int value) {
            switch (key) {
                case 1:
                    potential1 ??= new YisoEquipPotential();
                    potential1.SetValue(type, value);
                    break;
                case 2:
                    if (potential1 == null) throw new Exception("Potential 1 must not be empty");
                    potential2 ??= new YisoEquipPotential();
                    potential2.SetValue(type, value);
                    break;
                case 3:
                    if (potential1 == null || potential2 == null) throw new Exception("Potential 1 or 2 must not be empty");
                    potential3 ??= new YisoEquipPotential();
                    potential3.SetValue(type, value);
                    break;
                default:
                    throw new Exception($"{key} not in 1, 2, 3");
            }
        }

        public YisoEquipPotentials DeepCopy(YisoEquipPotentials other) {
            var newPotentials = new YisoEquipPotentials();
            if (other.potential1 != null)
                newPotentials.potential1 = other.potential1.Copy();
            if (other.potential2 != null)
                newPotentials.potential2 = other.potential2.Copy();
            if (other.potential3 != null)
                newPotentials.potential3 = other.potential3.Copy();
            
            return newPotentials;
        }

        public YisoPlayerEquipPotentialsData Save() {
            var data = new YisoPlayerEquipPotentialsData();
            if (potential1 != null)
                data.potential1 = potential1.Save();
            if (potential2 != null)
                data.potential2 = potential2.Save();
            if (potential3 != null)
                data.potential3 = potential3.Save();
            return data;
        }

        public void Load(YisoPlayerEquipPotentialsData data) {
            if (data.potential1 != null)
                potential1 = new YisoEquipPotential(data.potential1);
            if (data.potential2 != null)
                potential2 = new YisoEquipPotential(data.potential2);
            if (data.potential3 != null)
                potential3 = new YisoEquipPotential(data.potential3);
        }

        public YisoEquipPotential this[int key] {
            get {
                switch (key) {
                    case 1:
                        return potential1;
                    case 2:
                        return potential2;
                    case 3:
                        return potential3;
                    default:
                        throw new Exception($"{key} not in 1, 2, 3");
                }
            }
            set {
                switch (key) {
                    case 1:
                        potential1 = value.Copy();
                        break;
                    case 2:
                        if (potential1 == null) throw new Exception("Potential 1 must not be empty");
                        potential2 = value.Copy();
                        break;
                    case 3:
                        if (potential1 == null || potential2 == null) throw new Exception("Potential 1 or 2 must not be empty");
                        potential3 = value.Copy();
                        break;
                    default:
                        throw new Exception($"{key} not in 1, 2, 3");
                }
            }
        }
    }

    [Serializable]
    public class YisoEquipPotential {
        private YisoBuffEffectTypes type;
        private int value;

        public YisoBuffEffectTypes Type => type;
        public int Value => value;

        public void SetValue(YisoBuffEffectTypes type, int value) {
            this.type = type;
            this.value = value;
        }
        
        public YisoEquipPotential() { }

        public YisoEquipPotential(YisoPlayerEquipPotentialData data) {
            type = data.type;
            value = data.value;
        }

        public string ToUIText(YisoLocale.Locale locale) => type.ToString(value, locale);
        public string ToUITitle(YisoLocale.Locale locale) => type.ToString(locale);

        public string ToUIValue() {
            switch (type) {
                case YisoBuffEffectTypes.MOVE_SPEED_INC:
                case YisoBuffEffectTypes.ATTACK_SPEED_INC:
                    return value.ToCommaString();
            }

            return value.ToPercentage();
        }

        public YisoEquipPotential Copy() {
            return new YisoEquipPotential {
                type = type,
                value = value
            };
        }

        public YisoPlayerEquipPotentialData Save() => new YisoPlayerEquipPotentialData {
            type = type,
            value = value
        };
    }
}