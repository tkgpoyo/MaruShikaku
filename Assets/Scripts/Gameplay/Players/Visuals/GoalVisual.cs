using UnityEngine;

namespace MaruSikaku.Gameplay.Players.Visuals
{
    [RequireComponent(typeof(Animator))]
    public class GoalVisual : MonoBehaviour
    {
        private const string LIT_TRIGGER = "Lit";
        private const string UNLIT_TRIGGER = "Unlit";

        [SerializeField] private SpriteRenderer _glowRenderer;

        private Animator _anim;

        void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        void Start()
        {
            ApplyGlowAlpha();
        }

        public void LitGoal()
        {
            _anim.SetTrigger(LIT_TRIGGER);
        }

        public void UnlitGoal()
        {
            _anim.SetTrigger(UNLIT_TRIGGER);
        }

        private void ApplyGlowAlpha()
        {
            var originalSprite = _glowRenderer.sprite;
            var glowTexture = originalSprite.texture;
            var height = glowTexture.height;
            var width = glowTexture.width;
            var rad = Mathf.Min(height, width) * 0.5f;
            var pixels = new Color[height * width];
            var newTexture = new Texture2D(width, height);
            var center = new Vector2(width - 1, height - 1) * 0.5f;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    var distance = (new Vector2(w, h) - center).magnitude;
                    var alpha = Mathf.Pow(Mathf.Clamp01(1f - distance / rad), 2);
                    pixels[h * width + w] = new Color(1, 1, 1, alpha);
                }
            }

            newTexture.SetPixels(pixels);
            newTexture.Apply();
            _glowRenderer.sprite = Sprite.Create(newTexture, originalSprite.rect, new Vector2(0.5f, 0.5f), originalSprite.pixelsPerUnit);
        }
    }
}