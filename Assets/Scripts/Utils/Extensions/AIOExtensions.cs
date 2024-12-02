using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Utils.Extensions {
    public static class AIOExtensions {
        private static readonly string HIT_EFFECT = "HITEFFECT_ON";
        private static readonly int HIT_EFFECT_COLOR = Shader.PropertyToID("_HitEffectColor");
        private static readonly int HIT_EFFECT_BLEND = Shader.PropertyToID("_HitEffectBlend");

        private static readonly string OUTLINE = "OUTBASE_ON";
        private static readonly string OUTDIST = "OUTDIST_ON";
        private static readonly int OUTLINE_COLOR = Shader.PropertyToID("_OutlineColor");
        private static readonly int OUTLINE_DISTORTION = Shader.PropertyToID("_OutlineDistortToggle");

        private static readonly string SHINE_EFFECT = "SHINE_ON";
        private static readonly int SHINE_LOCATION = Shader.PropertyToID("_ShineLocation");

        public static void ActiveHitEffect(this Material material, bool flag) {
            ActiveEffect(material, HIT_EFFECT, flag);
        }

        public static void ActiveOutline(this Material material, bool flag) {
            ActiveEffect(material, OUTLINE, flag);
        }

        private static void ActiveEffect(Material material, string key, bool flag) {
            if (flag) material.EnableKeyword(key);
            else material.DisableKeyword(key);
        }

        public static void SetOutlineColor(this Material material, Color color) {
            material.SetColor(OUTLINE_COLOR, color);
        }

        public static void ActiveOutlineDistortion(this Material material, bool flag) {
            if (flag) material.EnableKeyword(OUTDIST);
            else material.DisableKeyword(OUTDIST);
            // material.SetFloat(OUTLINE_DISTORTION, flag ? 1f : 0f);
        }
        
        public static TweenerCore<float, float, FloatOptions> DOHit(this Material material, Color color, float endValue, float duration) {
            material.SetColor(HIT_EFFECT_COLOR, color);
            var t = DOTween.To(() => material.GetFloat(HIT_EFFECT_BLEND), value => material.SetFloat(HIT_EFFECT_BLEND, value),
                endValue, duration);
            t.SetTarget(material);
            return t;
        }

        public static void ActiveShine(this Material material, bool flag) {
            if (flag) material.EnableKeyword(SHINE_EFFECT);
            else material.DisableKeyword(SHINE_EFFECT);
        }

        public static void SetShineLocation(this Material material, float value) {
            if (value is < 0 or > 1) value = 0;
            material.SetFloat(SHINE_LOCATION, value);
        }

        public static TweenerCore<float, float, FloatOptions> DOShine(this Material material, float endValue,
            float duration) {
            var t = DOTween.To(() => material.GetFloat(SHINE_LOCATION), value => material.SetFloat(SHINE_LOCATION, value),
                endValue, duration);
            t.SetTarget(material);
            return t;
        }
    }
}