using UnityEngine;

namespace BurnOut.Enemies
{
    /// <summary>
    /// A small world-space health bar that floats above an enemy's head.
    /// Generates its own sprites at runtime (no art assets needed) and tracks
    /// the attached <see cref="EnemyHealth"/>. Hides itself when the enemy dies.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private float width = 1.1f;
        [SerializeField] private float height = .16f;
        [SerializeField] private float heightAbove = 1.15f;
        [SerializeField] private Color fillColor = new(.95f, .25f, .3f);

        private EnemyHealth health;
        private Transform root;
        private Transform fill;
        private float fullWidth;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            // Boss reads bigger, so scale the bar with the enemy's footprint.
            float s = Mathf.Max(1f, Mathf.Abs(transform.localScale.x));
            fullWidth = width * s;
            heightAbove *= s;
            Build(s);
            health.HealthChanged += OnHealthChanged;
            health.Died += Hide;
        }

        private void OnDestroy()
        {
            if (health != null) { health.HealthChanged -= OnHealthChanged; health.Died -= Hide; }
            if (root != null) Destroy(root.gameObject);
        }

        // Bar lives as an unparented object so it never inherits the enemy's flip/scale, then follows in LateUpdate.
        private void Build(float enemyScale)
        {
            root = new GameObject("~HealthBar").transform;
            float h = height * enemyScale;
            var back = MakeQuad("bg", new Color(.05f, .04f, .08f, .85f), fullWidth + .06f * enemyScale, h + .05f * enemyScale, 60);
            back.SetParent(root, false);
            var fillGo = MakeQuad("fill", fillColor, fullWidth, h, 61);
            fill = fillGo;
            fill.SetParent(root, false);
        }

        private Transform MakeQuad(string name, Color color, float w, float h, int order)
        {
            var go = new GameObject(name);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = BarSprite();
            r.color = color;
            r.sortingOrder = order;
            r.drawMode = SpriteDrawMode.Sliced;
            r.size = new Vector2(w, h);
            return go.transform;
        }

        private void OnHealthChanged(int current, int maximum)
        {
            if (fill == null || root == null) return;
            float pct = maximum <= 0 ? 0f : Mathf.Clamp01((float)current / maximum);
            // Shrink from the right by scaling the fill and nudging it left so it stays left-anchored.
            fill.localScale = new Vector3(pct, 1f, 1f);
            fill.localPosition = new Vector3(-fullWidth * (1f - pct) * .5f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (root == null) return;
            root.position = transform.position + Vector3.up * heightAbove;
        }

        private static Sprite barSprite;
        private static Sprite BarSprite()
        {
            if (barSprite != null) return barSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px); tex.Apply();
            barSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(1, 1, 1, 1));
            return barSprite;
        }

        private void Hide()
        {
            if (root != null) root.gameObject.SetActive(false);
        }
    }
}
