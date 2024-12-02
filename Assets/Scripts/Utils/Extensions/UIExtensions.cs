using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Utils.Extensions {
    public static class UIExtensions {
        public static void Visible(this CanvasGroup canvasGroup, bool value) {
            try {
                canvasGroup.alpha = value ? 1 : 0;
                canvasGroup.interactable = value;
                canvasGroup.blocksRaycasts = value;
            } catch (Exception ex) { }
        }

        public static void Visible(this CanvasGroup canvasGroup, float value) {
            try {
                canvasGroup.alpha = value;
                canvasGroup.interactable = value > 0;
                canvasGroup.blocksRaycasts = value > 0;
            } catch (Exception ex) { }
        }

        public static bool IsVisible(this CanvasGroup canvas) => canvas.alpha >= 0.99f;

        public static TweenerCore<float, float, FloatOptions> DOVisible(this CanvasGroup canvas, float endValue,
            float duration) {
            var t = DOTween.To(() => canvas.alpha, canvas.Visible, endValue, duration);
            t.SetTarget(canvas);
            return t;
        }

        public static void CreateTexture(this Sprite sprite, out Texture2D texture) {
            texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );
            
            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}