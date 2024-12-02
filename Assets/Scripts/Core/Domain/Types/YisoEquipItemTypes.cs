using System;
using System.Collections.Generic;
using Character.Weapon;
using Core.Domain.Item;
using Core.Domain.Locale;
using UnityEngine;
using Utils;
using Utils.Extensions;

namespace Core.Domain.Types {
    
    public enum YisoEquipRanks {
        N = 0,
        M = 1,
        C = 2,
        B = 3,
        A = 4,
        S = 5
    }

    public enum YisoEquipSlots {
        WEAPON, // 1
        HAT, // 21
        TOP, // 23
        BOTTOM, // 24 
        SHOES, // 25
        GLOVE, // 26
    }

    public enum YisoEquipFactions {
        COMMONER, // Normal (평민)
        THIEF, // Normal (도적)
        PIRATE, // Normal (수적)
        NAVY, // Magic (수군)
        KOREA_CORPORAL, // Magic (고구려 병사)
        BAEKJAE_LIEUTENANT, // Magic (백제 부장),
        ANPAGYEON,// S (안파견)
    }

    public enum YisoEquipSubTypes {
        // SLASH
        BASILARD, // 뾰족칼
        DAGGER, // 단도
        PIERCER, // 송곳칼
        SMALL_SWORD, // 소검
        SKEWER, // 꼬챙이
        SKIVER, // 회칼
        POCKETKNIFE, // 주머니칼
        SWORD, // 검
        LONGSWORD, // 장도
        CUTLASS, // 흔한 칼
        
        // THRUST
        DISEMBOWELER, // 쑤시개
        IMPALER, // 송곳창
        HARPOON, // 작살
        BAMBOO_SPEAR, // 죽창
        TRIDENT, // 삼지창
        PIKE, // 뾰족한 창
        DOKKAEBI_SPEAR, // 도깨비창
        WAR_SPEAR, // 전쟁창
        SPEAR, // 창
        GREAT_SPEAR, // 장창

        // HAT
        RED_HOOD, // 파란 두건
        HELMET, // 투모
        LEATHER_HOOD, // 가죽 두건
        WOODEN_HELMET, // 나무 투구
        HELL_MASK, // 지옥가면
        CAP, // 모자
        BONE_HELMET, // 뼈투구
        SKULL_HELMET, // 뿔투구
        BRONZE_HELMET, // 철투구
        CLOTH_HOOD, // 천 두건
        DIRTY_SHIRT, // 더러운옷
        
        // TOP
        CHAINMAIL, // 철갑옷
        BATTLE_ARMOR, // 전투갑옷
        LEATHER_TUNIC, // 가죽웃옷
        CLOTH_TUNIC, // 천웃옷
        BRONZE_ARMOR, // 철판 갑옷
        WOODEN_BREASTPLATE, // 나무갑옷
        BONE_BREASTPLATE, // 골편갑옷
        SHARK_SKIN_SHIRT, // 상어가죽옷
        WHALE_SKIN_SHIRT, // 고래가죽옷
        
        // BOTTOM
        CLOTH_PANTS, // 천 바지
        SHARK_SKIN_PANTS, // 상어가죽바지
        SEWN_PANTS, // 무늬바지
        LEG_GUARDS, // 다리보호구
        THIGH_GUARDS, // 허벅지 보호구
        LEGWRAPS, // 다리싸개
        DIRTY_PANTS, // 더러운 바지
        SILK_PANTS, // 비단바지
        WHALE_SKIN_PANTS, // 고래가죽바지
        
        // SHOES
        SHOES, // 신발
        LEATHER_BOOTS, // 가죽장화
        WARLORD_BOOTS, // 군장화
        SILK_SHOES, // 비단신
        HEAVY_SHOES, // 무거운 신발
        BOOTS, // 장화
        STRAW_SHOES, // 짚신
        LEATHER_SHOES, // 가죽 신발
        SHABBY_SHOES, // 떨어진 신발
        COW_LEATHER_SHOES, // 소가죽신
        
        // GLOVES
        GAUNTLETS, // 팔목장갑
        COW_LEATHER_GLOVES, // 소가죽장갑
        CLOTH_GLOVES, // 천장갑
        SILK_GLOVES, // 비단장갑
        SEWN_GLOVES, // 무늬장갑
        HEAVY_GLOVES, // 무거운 장갑
        CHAIN_GLOVES, // 철장갑
        LUCKY_GLOVES, // 행운장갑
        SHARK_SKIN_GLOVES, // 상어가죽장갑
        SHORT_GLOVES, // 짧은 장갑
        
        // SHOOT
        BAMBOO_LONG_BOW, // 대나무 활
        TWIN_BOW, // 쌍궁
        SNIPER_BOW, // 저격궁
        LONG_SHOT_BOW, // 원사궁
        WEATHERED_BOW, // 낡은 활
        GREAT_BOW, // 대궁
        WAR_BOW, // 전투활
        HUNTING_BOW, // 사냥활
        LONGBOW, // 장궁
        SHORT_BOW // 단궁
    }

    public static class YisoEquipItemTypesUtils {
        public static YisoWeapon.AttackType[] VALID_ATTACK_TYPES = new[] {
            YisoWeapon.AttackType.Shoot,
            YisoWeapon.AttackType.Thrust,
            YisoWeapon.AttackType.Slash
        };

        public static string ToString(this YisoWeapon.AttackType type, YisoLocale.Locale locale) => type switch {
            YisoWeapon.AttackType.Shoot => locale == YisoLocale.Locale.KR ? "활" : "Bow",
            YisoWeapon.AttackType.Thrust => locale == YisoLocale.Locale.KR ? "창" : "Spear",
            YisoWeapon.AttackType.Slash => locale == YisoLocale.Locale.KR ? "칼" : "Sword",
            _ => ""
        };
        
        public static YisoWeapon.AttackType GetNextType(this YisoWeapon.AttackType type) => type switch {
            YisoWeapon.AttackType.Shoot => YisoWeapon.AttackType.Thrust,
            YisoWeapon.AttackType.Thrust => YisoWeapon.AttackType.Slash,
            YisoWeapon.AttackType.Slash => YisoWeapon.AttackType.Shoot,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        public static string ToString(this YisoEquipFactions faction, YisoLocale.Locale locale) => faction switch {
            YisoEquipFactions.COMMONER => locale == YisoLocale.Locale.KR ? "평민" : "Commoner",
            YisoEquipFactions.THIEF => locale == YisoLocale.Locale.KR ? "도적" : "Thief",
            YisoEquipFactions.PIRATE => locale == YisoLocale.Locale.KR ? "수적" : "Pirate",
            YisoEquipFactions.NAVY => locale == YisoLocale.Locale.KR ? "수군" : "Navy",
            YisoEquipFactions.KOREA_CORPORAL => locale == YisoLocale.Locale.KR ? "고구려 병사" : "Korea Corporal",
            YisoEquipFactions.BAEKJAE_LIEUTENANT => locale == YisoLocale.Locale.KR ? "백제 부장" : "Baekjae Lieutenant",
            YisoEquipFactions.ANPAGYEON => locale == YisoLocale.Locale.KR ? "안파견" : "Anpagyeon",
            _ => throw new ArgumentOutOfRangeException(nameof(faction), faction, null)
        };
        
        public static YisoEquipRanks ToRank(this YisoEquipFactions faction) => faction switch {
            YisoEquipFactions.COMMONER => YisoEquipRanks.N,
            YisoEquipFactions.THIEF => YisoEquipRanks.N,
            YisoEquipFactions.PIRATE => YisoEquipRanks.N,
            YisoEquipFactions.NAVY => YisoEquipRanks.M,
            YisoEquipFactions.KOREA_CORPORAL => YisoEquipRanks.M,
            YisoEquipFactions.BAEKJAE_LIEUTENANT => YisoEquipRanks.M,
            YisoEquipFactions.ANPAGYEON => YisoEquipRanks.S,
            _ => throw new ArgumentOutOfRangeException(nameof(faction), faction, null)
        };
        
        public static string ToString(this YisoEquipSubTypes subTypes, YisoLocale.Locale locale) =>
            subTypes.GetEquipSubTypeUITitle()[locale];

        public static YisoEquipRanks NextRank(this YisoEquipRanks rank) {
            var index = (int)rank;
            return (index + 1).ToEnum<YisoEquipRanks>();
        }

        public static bool TryGetNextRank(this YisoEquipRanks rank, out YisoEquipRanks nextRank) {
            nextRank = YisoEquipRanks.S;
            var sIndex = (int)YisoEquipRanks.S;
            var index = (int)rank;

            if (index + 1 > sIndex) return false;

            nextRank = (index + 1).ToEnum<YisoEquipRanks>();
            return true;
        }

        public static YisoEquipRanks GetRandomRank(this YisoEquipRanks rank, int step = 1) {
            var minRankIdx = (int)rank;
            var maxRankIdx = Mathf.Clamp(minRankIdx + step, minRankIdx, (int)YisoEquipRanks.S);
            return Randomizer.Next(minRankIdx, maxRankIdx).ToEnum<YisoEquipRanks>();
        }

        public static string ToString(this YisoEquipSlots slot, YisoLocale.Locale locale) => slot switch {
            YisoEquipSlots.WEAPON => locale == YisoLocale.Locale.KR ? "무기": "Weapon",
            YisoEquipSlots.HAT => locale == YisoLocale.Locale.KR ? "모자": "Hat",
            YisoEquipSlots.TOP => locale == YisoLocale.Locale.KR ? "상의": "Top",
            YisoEquipSlots.BOTTOM => locale == YisoLocale.Locale.KR ? "하의": "Bottom",
            YisoEquipSlots.SHOES => locale == YisoLocale.Locale.KR ? "신발": "Shoes",
            YisoEquipSlots.GLOVE => locale == YisoLocale.Locale.KR ? "장갑": "Glove",
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
        };

        public static string ToString(this YisoItem.InventoryType type, YisoLocale.Locale locale) => type switch {
            YisoItem.InventoryType.EQUIP => locale == YisoLocale.Locale.KR ? "장비" : "Equip",
            YisoItem.InventoryType.USE => locale == YisoLocale.Locale.KR ? "소비" : "Use",
            YisoItem.InventoryType.ETC => locale == YisoLocale.Locale.KR ? "기타" : "ETC",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static string ToString(this YisoEquipRanks rank, YisoLocale.Locale locale) =>
            rank switch {
                YisoEquipRanks.S => locale == YisoLocale.Locale.KR ? "레전더리" : "Legendary",
                YisoEquipRanks.A => locale == YisoLocale.Locale.KR ? "유니크" : "Unique",
                YisoEquipRanks.B => locale == YisoLocale.Locale.KR ? "에픽" : "Epic",
                YisoEquipRanks.C => locale == YisoLocale.Locale.KR ? "레어" : "Rare",
                YisoEquipRanks.M => locale == YisoLocale.Locale.KR ? "매직" : "Magic",
                YisoEquipRanks.N => locale == YisoLocale.Locale.KR ? "일반" : "Normal",
                _ => throw new ArgumentOutOfRangeException()
            };

        public static Color ToColor(this YisoEquipRanks rank) => rank switch {
            YisoEquipRanks.S => CreateColor(230, 74, 25),
            YisoEquipRanks.A => CreateColor(255, 167, 38),
            YisoEquipRanks.B => CreateColor(126, 87, 194),
            YisoEquipRanks.C => CreateColor(79, 195, 247),
            YisoEquipRanks.M => CreateColor(255, 238, 88),
            YisoEquipRanks.N => Color.white,
            _ => throw new ArgumentOutOfRangeException(nameof(rank), rank, null)
        };
        
        public static (Color outline, Color pattern) GetRankColorPair(this YisoEquipRanks rank) {
            var outline = Color.white;
            var pattern = Color.white;
            switch (rank) {
                case YisoEquipRanks.S:
                    outline = CreateColor(230, 74, 25);
                    pattern = CreateColor(255, 103, 52);
                    break;
                case YisoEquipRanks.A:
                    outline = CreateColor(255, 167, 38);
                    pattern = CreateColor(255, 188, 58);
                    break;
                case YisoEquipRanks.B:
                    outline = CreateColor(126, 87, 194);
                    pattern = CreateColor(144, 96, 199);
                    break;
                case YisoEquipRanks.C:
                    outline = CreateColor(79, 195, 247);
                    pattern = CreateColor(121, 216, 255);
                    break;
                case YisoEquipRanks.M:
                    outline = CreateColor(255, 238, 88);
                    pattern = CreateColor(255, 255, 153);
                    break;
                case YisoEquipRanks.N:
                    outline = CreateColor(120, 80, 41);
                    pattern = Color.white;
                    break;
            }

            return (outline, pattern);
        }
        
        private static Color CreateColor(int r, int g, int b, int a = 255) =>
            new(r / 255f, g / 255f, b / 255f, a / 255f);

        private static YisoLocale GetEquipSubTypeUITitle(this YisoEquipSubTypes subType) {
            var title = new YisoLocale();
            switch (subType) {
                case YisoEquipSubTypes.BASILARD:
                    title.kr = "뾰족칼";
                    title.en = "Basilard";
                    break;
                case YisoEquipSubTypes.DAGGER:
                    title.kr = "단도";
                    title.en = "Dagger";
                    break;
                case YisoEquipSubTypes.PIERCER:
                    title.kr = "송곳칼";
                    title.en = "Piercer";
                    break;
                case YisoEquipSubTypes.SMALL_SWORD:
                    title.kr = "소검";
                    title.en = "Small Sword";
                    break;
                case YisoEquipSubTypes.SKEWER:
                    title.kr = "꼬챙이";
                    title.en = "Skewer";
                    break;
                case YisoEquipSubTypes.SKIVER:
                    title.kr = "회칼";
                    title.en = "Skiver";
                    break;
                case YisoEquipSubTypes.POCKETKNIFE:
                    title.kr = "주머니칼";
                    title.en = "Pocketknife";
                    break;
                case YisoEquipSubTypes.SWORD:
                    title.kr = "검";
                    title.en = "Sword";
                    break;
                case YisoEquipSubTypes.LONGSWORD:
                    title.kr = "장도";
                    title.en = "Long Sword";
                    break;
                case YisoEquipSubTypes.CUTLASS:
                    title.kr = "흔한 칼";
                    title.en = "Cutlass";
                    break;
                case YisoEquipSubTypes.DISEMBOWELER:
                    title.kr = "쑤시개";
                    title.en = "Disemboweler";
                    break;
                case YisoEquipSubTypes.IMPALER:
                    title.kr = "송곳창";
                    title.en = "Impaler";
                    break;
                case YisoEquipSubTypes.HARPOON:
                    title.kr = "작살";
                    title.en = "Harpoon";
                    break;
                case YisoEquipSubTypes.BAMBOO_SPEAR:
                    title.kr = "죽창";
                    title.en = "Bamboo Spear";
                    break;
                case YisoEquipSubTypes.TRIDENT:
                    title.kr = "삼지창";
                    title.en = "Trident";
                    break;
                case YisoEquipSubTypes.PIKE:
                    title.kr = "뾰족한 창";
                    title.en = "Pike";
                    break;
                case YisoEquipSubTypes.DOKKAEBI_SPEAR:
                    title.kr = "도깨비창";
                    title.en = "Dokkaebi Spear";
                    break;
                case YisoEquipSubTypes.WAR_SPEAR:
                    title.kr = "전쟁창";
                    title.en = "War Spear";
                    break;
                case YisoEquipSubTypes.SPEAR:
                    title.kr = "창";
                    title.en = "Spear";
                    break;
                case YisoEquipSubTypes.GREAT_SPEAR:
                    title.kr = "장창";
                    title.en = "Great Spear";
                    break;
                case YisoEquipSubTypes.RED_HOOD:
                    title.kr = "파란 두건";
                    title.en = "Red Hood";
                    break;
                case YisoEquipSubTypes.HELMET:
                    title.kr = "투모";
                    title.en = "Helmet";
                    break;
                case YisoEquipSubTypes.LEATHER_HOOD:
                    title.kr = "가죽 두건";
                    title.en = "Leather Hood";
                    break;
                case YisoEquipSubTypes.WOODEN_HELMET:
                    title.kr = "나무 투구";
                    title.en = "Wooden Helmet";
                    break;
                case YisoEquipSubTypes.HELL_MASK:
                    title.kr = "지옥가면";
                    title.en = "Hell Mask";
                    break;
                case YisoEquipSubTypes.CAP:
                    title.kr = "모자";
                    title.en = "Cap";
                    break;
                case YisoEquipSubTypes.BONE_HELMET:
                    title.kr = "뼈투구";
                    title.en = "Bone Helmet";
                    break;
                case YisoEquipSubTypes.SKULL_HELMET:
                    title.kr = "뿔투구";
                    title.en = "Skull Helmet";
                    break;
                case YisoEquipSubTypes.BRONZE_HELMET:
                    title.kr = "철투구";
                    title.en = "Bronze Helmet";
                    break;
                case YisoEquipSubTypes.CLOTH_HOOD:
                    title.kr = "천 두건";
                    title.en = "Cloth Hood";
                    break;
                case YisoEquipSubTypes.DIRTY_SHIRT:
                    title.kr = "더러운옷";
                    title.en = "Dirty Shirt";
                    break;
                case YisoEquipSubTypes.CHAINMAIL:
                    title.kr = "철갑옷";
                    title.en = "Chain Mail";
                    break;
                case YisoEquipSubTypes.BATTLE_ARMOR:
                    title.kr = "전투갑옷";
                    title.en = "Battle Armor";
                    break;
                case YisoEquipSubTypes.LEATHER_TUNIC:
                    title.kr = "가죽웃옷";
                    title.en = "Leather Tunic";
                    break;
                case YisoEquipSubTypes.CLOTH_TUNIC:
                    title.kr = "천웃옷";
                    title.en = "Cloth Tunic";
                    break;
                case YisoEquipSubTypes.BRONZE_ARMOR:
                    title.kr = "철판 갑옷";
                    title.en = "Bronze Armor";
                    break;
                case YisoEquipSubTypes.WOODEN_BREASTPLATE:
                    title.kr = "나무갑옷";
                    title.en = "Wooden Breastplate";
                    break;
                case YisoEquipSubTypes.BONE_BREASTPLATE:
                    title.kr = "골편갑옷";
                    title.en = "Bone Breastplate";
                    break;
                case YisoEquipSubTypes.SHARK_SKIN_SHIRT:
                    title.kr = "상어가죽옷";
                    title.en = "Shark Skin Shirt";
                    break;
                case YisoEquipSubTypes.WHALE_SKIN_SHIRT:
                    title.kr = "고래가죽옷";
                    title.en = "Whale Skin Shirt";
                    break;
                case YisoEquipSubTypes.CLOTH_PANTS:
                    title.kr = "천 바지";
                    title.en = "Cloth Pants";
                    break;
                case YisoEquipSubTypes.SHARK_SKIN_PANTS:
                    title.kr = "상어가죽바지";
                    title.en = "Shark Skin Pants";
                    break;
                case YisoEquipSubTypes.SEWN_PANTS:
                    title.kr = "무늬바지";
                    title.en = "Sewn Pants";
                    break;
                case YisoEquipSubTypes.LEG_GUARDS:
                    title.kr = "다리보호구";
                    title.en = "Leg Guards";
                    break;
                case YisoEquipSubTypes.THIGH_GUARDS:
                    title.kr = "허벅지 보호구";
                    title.en = "Thigh Guards";
                    break;
                case YisoEquipSubTypes.LEGWRAPS:
                    title.kr = "다리싸개";
                    title.en = "Leg Wraps";
                    break;
                case YisoEquipSubTypes.DIRTY_PANTS:
                    title.kr = "더러운 바지";
                    title.en = "Dirty Pants";
                    break;
                case YisoEquipSubTypes.SILK_PANTS:
                    title.kr = "비단바지";
                    title.en = "Silk Pants";
                    break;
                case YisoEquipSubTypes.WHALE_SKIN_PANTS:
                    title.kr = "고래가죽바지";
                    title.en = "Whale Skin Pants";
                    break;
                case YisoEquipSubTypes.SHOES:
                    title.kr = "신발";
                    title.en = "Shoes";
                    break;
                case YisoEquipSubTypes.LEATHER_BOOTS:
                    title.kr = "가죽장화";
                    title.en = "Leather Boots";
                    break;
                case YisoEquipSubTypes.WARLORD_BOOTS:
                    title.kr = "군장화";
                    title.en = "Warlord Boots";
                    break;
                case YisoEquipSubTypes.SILK_SHOES:
                    title.kr = "비단신";
                    title.en = "Silk Shoes";
                    break;
                case YisoEquipSubTypes.HEAVY_SHOES:
                    title.kr = "무거운 신발";
                    title.en = "Heavy Shoes";
                    break;
                case YisoEquipSubTypes.BOOTS:
                    title.kr = "장화";
                    title.en = "Boots";
                    break;
                case YisoEquipSubTypes.STRAW_SHOES:
                    title.kr = "짚신";
                    title.en = "Straw Shoes";
                    break;
                case YisoEquipSubTypes.LEATHER_SHOES:
                    title.kr = "가죽 신발";
                    title.en = "Leather Shoes";
                    break;
                case YisoEquipSubTypes.SHABBY_SHOES:
                    title.kr = "떨어진 신발";
                    title.en = "Shabby Shoes";
                    break;
                case YisoEquipSubTypes.COW_LEATHER_SHOES:
                    title.kr = "소가죽신";
                    title.en = "Cow Leather Shoes";
                    break;
                case YisoEquipSubTypes.GAUNTLETS:
                    title.kr = "팔목장갑";
                    title.en = "Gauntlets";
                    break;
                case YisoEquipSubTypes.COW_LEATHER_GLOVES:
                    title.kr = "소가죽장갑";
                    title.en = "Cow Leather Gloves";
                    break;
                case YisoEquipSubTypes.CLOTH_GLOVES:
                    title.kr = "천장갑";
                    title.en = "Cloth Gloves";
                    break;
                case YisoEquipSubTypes.SILK_GLOVES:
                    title.kr = "비단장갑";
                    title.en = "Silk Gloves";
                    break;
                case YisoEquipSubTypes.SEWN_GLOVES:
                    title.kr = "무늬장갑";
                    title.en = "Sewn Gloves";
                    break;
                case YisoEquipSubTypes.HEAVY_GLOVES:
                    title.kr = "무거운 장갑";
                    title.en = "Heavy Gloves";
                    break;
                case YisoEquipSubTypes.CHAIN_GLOVES:
                    title.kr = "철장갑";
                    title.en = "Chain Gloves";
                    break;
                case YisoEquipSubTypes.LUCKY_GLOVES:
                    title.kr = "행운장갑";
                    title.en = "Lucky Gloves";
                    break;
                case YisoEquipSubTypes.SHARK_SKIN_GLOVES:
                    title.kr = "상어가죽장갑";
                    title.en = "Shark Skin Gloves";
                    break;
                case YisoEquipSubTypes.SHORT_GLOVES:
                    title.kr = "짧은 장갑";
                    title.en = "Short Gloves";
                    break;
                case YisoEquipSubTypes.BAMBOO_LONG_BOW:
                    title.kr = "대나무 활";
                    title.en = "Bamboo Longbow";
                    break;
                case YisoEquipSubTypes.TWIN_BOW:
                    title.kr = "쌍궁";
                    title.en = "Twin Bow";
                    break;
                case YisoEquipSubTypes.SNIPER_BOW:
                    title.kr = "저격궁";
                    title.en = "Sniper Bow";
                    break;
                case YisoEquipSubTypes.LONG_SHOT_BOW:
                    title.kr = "원사궁";
                    title.en = "Long Shot Bow";
                    break;
                case YisoEquipSubTypes.WEATHERED_BOW:
                    title.kr = "낡은 활";
                    title.en = "Weathered Bow";
                    break;
                case YisoEquipSubTypes.GREAT_BOW:
                    title.kr = "대궁";
                    title.en = "Great Bow";
                    break;
                case YisoEquipSubTypes.WAR_BOW:
                    title.kr = "전투활";
                    title.en = "War Bow";
                    break;
                case YisoEquipSubTypes.HUNTING_BOW:
                    title.kr = "사냥활";
                    title.en = "Hunting Bow";
                    break;
                case YisoEquipSubTypes.LONGBOW:
                    title.kr = "장궁";
                    title.en = "Long Bow";
                    break;
                case YisoEquipSubTypes.SHORT_BOW:
                    title.kr = "단궁";
                    title.en = "Short Bow";
                    break;
                default:
                    title.kr = "알 수 없는 랭크";
                    title.en = "Unknown Rank";
                    break;
            }

            return title;
        }
    }
    
    public static class YisoEquipItemTypeConst {
        public static readonly int SLOT_COUNT = 6;
        private static Dictionary<YisoEquipSlots, List<YisoEquipSubTypes>> SUB_TYPE_DICT = new();
        private static Dictionary<YisoWeapon.AttackType, List<YisoEquipSubTypes>> ATTACK_TYPE_DICT = new();

        public static IReadOnlyList<YisoEquipSubTypes> GetSubTypesBySlots(YisoEquipSlots slots) => SUB_TYPE_DICT[slots];

        public static IReadOnlyList<YisoEquipSubTypes> GetSubTypesByAttack(YisoWeapon.AttackType type) =>
            ATTACK_TYPE_DICT[type];

        static YisoEquipItemTypeConst() {
            SUB_TYPE_DICT[YisoEquipSlots.HAT] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.RED_HOOD,
                YisoEquipSubTypes.HELMET,
                YisoEquipSubTypes.LEATHER_HOOD,
                YisoEquipSubTypes.WOODEN_HELMET,
                YisoEquipSubTypes.HELL_MASK,
                YisoEquipSubTypes.CAP,
                YisoEquipSubTypes.BONE_HELMET,
                YisoEquipSubTypes.SKULL_HELMET,
                YisoEquipSubTypes.BRONZE_HELMET,
                YisoEquipSubTypes.CLOTH_HOOD,
                YisoEquipSubTypes.DIRTY_SHIRT
            };

            SUB_TYPE_DICT[YisoEquipSlots.TOP] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.CHAINMAIL,
                YisoEquipSubTypes.BATTLE_ARMOR,
                YisoEquipSubTypes.LEATHER_TUNIC,
                YisoEquipSubTypes.CLOTH_TUNIC,
                YisoEquipSubTypes.BRONZE_ARMOR,
                YisoEquipSubTypes.WOODEN_BREASTPLATE,
                YisoEquipSubTypes.BONE_BREASTPLATE,
                YisoEquipSubTypes.SHARK_SKIN_SHIRT,
                YisoEquipSubTypes.WHALE_SKIN_SHIRT
            };

            SUB_TYPE_DICT[YisoEquipSlots.BOTTOM] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.CLOTH_PANTS,
                YisoEquipSubTypes.SHARK_SKIN_PANTS,
                YisoEquipSubTypes.SEWN_PANTS,
                YisoEquipSubTypes.LEG_GUARDS,
                YisoEquipSubTypes.THIGH_GUARDS,
                YisoEquipSubTypes.LEGWRAPS,
                YisoEquipSubTypes.DIRTY_PANTS,
                YisoEquipSubTypes.SILK_PANTS,
                YisoEquipSubTypes.WHALE_SKIN_PANTS
            };

            SUB_TYPE_DICT[YisoEquipSlots.SHOES] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.SHOES,
                YisoEquipSubTypes.LEATHER_BOOTS,
                YisoEquipSubTypes.WARLORD_BOOTS,
                YisoEquipSubTypes.SILK_SHOES,
                YisoEquipSubTypes.HEAVY_SHOES,
                YisoEquipSubTypes.BOOTS,
                YisoEquipSubTypes.STRAW_SHOES,
                YisoEquipSubTypes.LEATHER_SHOES,
                YisoEquipSubTypes.SHABBY_SHOES,
                YisoEquipSubTypes.COW_LEATHER_SHOES
            };

            SUB_TYPE_DICT[YisoEquipSlots.GLOVE] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.GAUNTLETS,
                YisoEquipSubTypes.COW_LEATHER_GLOVES,
                YisoEquipSubTypes.CLOTH_GLOVES,
                YisoEquipSubTypes.SILK_GLOVES,
                YisoEquipSubTypes.SEWN_GLOVES,
                YisoEquipSubTypes.HEAVY_GLOVES,
                YisoEquipSubTypes.CHAIN_GLOVES,
                YisoEquipSubTypes.LUCKY_GLOVES,
                YisoEquipSubTypes.SHARK_SKIN_GLOVES,
                YisoEquipSubTypes.SHORT_GLOVES
            };

            ATTACK_TYPE_DICT[YisoWeapon.AttackType.Slash] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.BASILARD,
                YisoEquipSubTypes.DAGGER,
                YisoEquipSubTypes.PIERCER,
                YisoEquipSubTypes.SMALL_SWORD,
                YisoEquipSubTypes.SKEWER,
                YisoEquipSubTypes.SKIVER,
                YisoEquipSubTypes.POCKETKNIFE,
                YisoEquipSubTypes.SWORD,
                YisoEquipSubTypes.LONGSWORD,
                YisoEquipSubTypes.CUTLASS
            };

            ATTACK_TYPE_DICT[YisoWeapon.AttackType.Thrust] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.DISEMBOWELER,
                YisoEquipSubTypes.IMPALER,
                YisoEquipSubTypes.HARPOON,
                YisoEquipSubTypes.BAMBOO_SPEAR,
                YisoEquipSubTypes.TRIDENT,
                YisoEquipSubTypes.PIKE,
                YisoEquipSubTypes.DOKKAEBI_SPEAR,
                YisoEquipSubTypes.WAR_SPEAR,
                YisoEquipSubTypes.SPEAR,
                YisoEquipSubTypes.GREAT_SPEAR,
            };

            ATTACK_TYPE_DICT[YisoWeapon.AttackType.Shoot] = new List<YisoEquipSubTypes> {
                YisoEquipSubTypes.BAMBOO_LONG_BOW,
                YisoEquipSubTypes.TWIN_BOW,
                YisoEquipSubTypes.SNIPER_BOW,
                YisoEquipSubTypes.LONG_SHOT_BOW,
                YisoEquipSubTypes.WEATHERED_BOW,
                YisoEquipSubTypes.GREAT_BOW,
                YisoEquipSubTypes.WAR_BOW,
                YisoEquipSubTypes.HUNTING_BOW,
                YisoEquipSubTypes.LONGBOW,
                YisoEquipSubTypes.SHORT_BOW
            };
        }
    }
}