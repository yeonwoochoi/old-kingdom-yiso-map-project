using System;
using Core.Domain.Types;
using Newtonsoft.Json;

namespace Core.Server.Domain {
    [Serializable]
    public class YisoServerEnemyFactors {
        public double normal;
        public double elite;
        [JsonProperty("field_boss")]
        public double fieldBoss;
        public double boss;

        public double this[YisoEnemyTypes type] {
            get {
                return type switch {
                    YisoEnemyTypes.NORMAL => normal,
                    YisoEnemyTypes.ELITE => elite,
                    YisoEnemyTypes.FIELD_BOSS => fieldBoss,
                    YisoEnemyTypes.BOSS => boss,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
            set {
                switch (type) {
                    case YisoEnemyTypes.NORMAL:
                        normal = value;
                        break;
                    case YisoEnemyTypes.ELITE:
                        elite = value;
                        break;
                    case YisoEnemyTypes.FIELD_BOSS:
                        fieldBoss = value;
                        break;
                    case YisoEnemyTypes.BOSS:
                        boss = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }

    [Serializable]
    public class YisoServerItemRankFactors {
        public double n;
        public double m;
        public double c;
        public double b;
        public double a;
        public double s;

        public double this[YisoEquipRanks rank] {
            get {
                switch (rank) {
                    case YisoEquipRanks.N:
                        return n;
                    case YisoEquipRanks.M:
                        return m;
                    case YisoEquipRanks.C:
                        return c;
                    case YisoEquipRanks.B:
                        return b;
                    case YisoEquipRanks.A:
                        return a;
                    case YisoEquipRanks.S:
                        return s;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
                }
            }
            internal set {
                switch (rank) {
                    case YisoEquipRanks.N:
                        n = value;
                        break;
                    case YisoEquipRanks.M:
                        m = value;
                        break;
                    case YisoEquipRanks.C:
                        c = value;
                        break;
                    case YisoEquipRanks.B:
                        b = value;
                        break;
                    case YisoEquipRanks.A:
                        a = value;
                        break;
                    case YisoEquipRanks.S:
                        s = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
                }
            }
        }
    }

    [Serializable]
    public class YisoServerItemFactionFactors {
        public double commoner;
        public double thief;
        public double pirate;
        public double navy;
        [JsonProperty("korea_corporal")]
        public double koreaCorporal;
        [JsonProperty("backjae_lieutenant")]
        public double backjaeLieutemant;
        public double anpagyeon;

        public double this[YisoEquipFactions faction] {
            get => faction switch {
                YisoEquipFactions.COMMONER => commoner,
                YisoEquipFactions.THIEF => thief,
                YisoEquipFactions.PIRATE => pirate,
                YisoEquipFactions.NAVY => navy,
                YisoEquipFactions.KOREA_CORPORAL => koreaCorporal,
                YisoEquipFactions.BAEKJAE_LIEUTENANT => backjaeLieutemant,
                YisoEquipFactions.ANPAGYEON => anpagyeon,
                _ => throw new ArgumentOutOfRangeException(nameof(faction), faction, null)
            };
            internal set {
                switch (faction) {
                    case YisoEquipFactions.COMMONER:
                        commoner = value;
                        break;
                    case YisoEquipFactions.THIEF:
                        thief = value;
                        break;
                    case YisoEquipFactions.PIRATE:
                        pirate = value;
                        break;
                    case YisoEquipFactions.NAVY:
                        navy = value;
                        break;
                    case YisoEquipFactions.KOREA_CORPORAL:
                        koreaCorporal = value;
                        break;
                    case YisoEquipFactions.BAEKJAE_LIEUTENANT:
                        backjaeLieutemant = value;
                        break;
                    case YisoEquipFactions.ANPAGYEON:
                        anpagyeon = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
                }
            }
        }
    }

    [Serializable]
    public class YisoServerItemSlotFactors {
        public double weapon;
        public double hat;
        public double top;
        public double bottom;
        public double shoes;
        public double glove;

        public double this[YisoEquipSlots slot] {
            get => slot switch {
                YisoEquipSlots.WEAPON => weapon,
                YisoEquipSlots.HAT => hat,
                YisoEquipSlots.TOP => top,
                YisoEquipSlots.BOTTOM => bottom,
                YisoEquipSlots.SHOES => shoes,
                YisoEquipSlots.GLOVE => glove,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
            set {
                switch (slot) {
                    case YisoEquipSlots.WEAPON:
                        weapon = value;
                        break;
                    case YisoEquipSlots.HAT:
                        hat = value;
                        break;
                    case YisoEquipSlots.TOP:
                        top = value;
                        break;
                    case YisoEquipSlots.BOTTOM:
                        bottom = value;
                        break;
                    case YisoEquipSlots.SHOES:
                        shoes = value;
                        break;
                    case YisoEquipSlots.GLOVE:
                        glove = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
                }
            }
        }
    }
}