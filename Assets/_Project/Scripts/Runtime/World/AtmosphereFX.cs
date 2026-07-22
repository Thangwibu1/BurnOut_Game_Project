using UnityEngine;

namespace BurnOut.World
{
    /// <summary>
    /// Fills the space around the camera with dense falling snow to add depth and mood.
    /// Generates its own soft dot sprite; needs no art assets. Purely cosmetic.
    /// </summary>
    public sealed class AtmosphereFX : MonoBehaviour
    {
        [SerializeField] private int moteCount = 140;
        [SerializeField] private Color moteColor = new(.92f, .95f, 1f, .55f);
        [SerializeField] private float driftSpeed = 1.4f;
        [SerializeField] private float area = 16f;
        [SerializeField] private int sortingOrder = -8;

        private Camera cam;
        private Transform[] motes;
        private Vector2[] velocities;

        private void Start()
        {
            cam = Camera.main;
            var dot = MakeDot();
            motes = new Transform[Mathf.Max(1, moteCount)];
            velocities = new Vector2[motes.Length];
            for (int i = 0; i < motes.Length; i++)
            {
                var go = new GameObject("~mote");
                go.transform.SetParent(transform);
                var r = go.AddComponent<SpriteRenderer>();
                r.sprite = dot;
                r.color = new Color(moteColor.r, moteColor.g, moteColor.b, moteColor.a * Random.Range(.4f, 1.3f));
                r.sortingOrder = sortingOrder;
                go.transform.localScale = Vector3.one * Random.Range(.06f, .22f);
                go.transform.position = RandomPosition();
                motes[i] = go.transform;
                // Snow falls: gentle sideways sway plus a steady downward drift.
                velocities[i] = new Vector2(Random.Range(-.35f, .35f), Random.Range(-1f, -.5f)) * driftSpeed;
            }
        }

        private Vector3 RandomPosition()
        {
            var c = cam != null ? cam.transform.position : Vector3.zero;
            return new Vector3(c.x + Random.Range(-area, area), c.y + Random.Range(-area * .6f, area * .6f), 0f);
        }

        private void Update()
        {
            if (cam == null) { cam = Camera.main; if (cam == null) return; }
            var c = cam.transform.position;
            for (int i = 0; i < motes.Length; i++)
            {
                if (motes[i] == null) continue;
                motes[i].position += (Vector3)(velocities[i] * Time.deltaTime);
                var p = motes[i].position;
                // Recycle flakes back to the top once they fall past the bottom or drift out sideways.
                if (p.y < c.y - area * .6f || Mathf.Abs(p.x - c.x) > area)
                    motes[i].position = new Vector3(c.x + Random.Range(-area, area), c.y + area * .6f, 0f);
            }
        }

        private static Sprite MakeDot()
        {
            const int size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            float c = (size - 1) * .5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d)));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }
    }
}
