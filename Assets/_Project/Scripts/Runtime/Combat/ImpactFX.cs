using UnityEngine;

namespace BurnOut.Combat
{
    /// <summary>
    /// Cheap runtime impact visuals — hit sparks, bursts and expanding rings.
    /// Generates its own soft sprites, so no art assets are needed.
    /// </summary>
    public static class ImpactFX
    {
        private static Sprite dot;
        private static Sprite ring;

        private static Sprite Dot() => dot != null ? dot : dot = MakeRadial(32, false);
        private static Sprite Ring() => ring != null ? ring : ring = MakeRadial(48, true);

        private static Sprite MakeRadial(int size, bool hollow)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = hollow ? Mathf.Clamp01(1f - Mathf.Abs(d - 0.78f) * 6f) : Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        /// <summary>A radial spray of fading motes.</summary>
        public static void Burst(Vector3 position, Color color, int count = 8, float speed = 4.5f, float size = .35f, int sortingOrder = 40)
        {
            for (int i = 0; i < count; i++)
            {
                float ang = i / (float)count * Mathf.PI * 2f + Random.value;
                var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                Spawn(position, color, dir * speed * (0.6f + Random.value * 0.7f), size * (0.7f + Random.value * 0.6f), sortingOrder, .38f, Dot(), false);
            }
        }

        /// <summary>A bright flash plus a few shards — good for landing a hit.</summary>
        public static void Spark(Vector3 position, Color color, int sortingOrder = 45)
        {
            Spawn(position, color, Vector2.zero, .9f, sortingOrder, .12f, Dot(), false);
            Burst(position, color, 5, 6f, .18f, sortingOrder);
        }

        /// <summary>An expanding hollow ring — good for dashes, skills and slams.</summary>
        public static void Expand(Vector3 position, Color color, float size = 1.5f, int sortingOrder = 44)
        {
            Spawn(position, color, Vector2.zero, size, sortingOrder, .3f, Ring(), true);
        }

        private static void Spawn(Vector3 position, Color color, Vector2 velocity, float size, int sortingOrder, float life, Sprite sprite, bool grow)
        {
            if (sprite == null) return;
            var go = new GameObject("~fx");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.color = color;
            r.sortingOrder = sortingOrder;
            go.AddComponent<FxParticle>().Init(velocity, life, grow);
        }
    }

    /// <summary>Drives one runtime impact mote: drift, scale and fade, then self-destruct.</summary>
    public sealed class FxParticle : MonoBehaviour
    {
        private Vector2 velocity;
        private float life;
        private float age;
        private bool grow;
        private SpriteRenderer r;
        private Vector3 startScale;

        public void Init(Vector2 v, float l, bool g)
        {
            velocity = v;
            life = Mathf.Max(0.01f, l);
            grow = g;
            r = GetComponent<SpriteRenderer>();
            startScale = transform.localScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / life);
            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity *= 0.9f;
            transform.localScale = grow ? startScale * (1f + t * 2.4f) : startScale * (1f - 0.4f * t);
            if (r != null)
            {
                var c = r.color;
                c.a = (grow ? 1f - t : Mathf.Clamp01(1f - t)) * (grow ? 0.9f : 1f);
                r.color = c;
            }
            if (age >= life) Destroy(gameObject);
        }
    }
}
