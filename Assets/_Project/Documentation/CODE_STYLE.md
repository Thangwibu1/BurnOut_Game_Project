# Burn Out Code Style

- Namespaces follow `BurnOut.<Module>`; types and methods use PascalCase; private serialized fields use camelCase.
- Use `[SerializeField] private` rather than public mutable fields. Cache references in `Awake`; subscribe in `OnEnable`/`Start` and unsubscribe in `OnDisable`/`OnDestroy`.
- Gameplay may depend on interfaces and events, never on Canvas, Slider, Image, or TMP types.
- Put UnityEditor code only in `Scripts/Editor`; do not call UnityEditor APIs from Runtime.
- Do not use `GameObject.Find`, `FindObjectOfType`, or `GetComponent` in hot loops. Avoid allocations and LINQ in `Update`.
- Keep each component focused. Place balancing values in serialized fields or ScriptableObjects.
