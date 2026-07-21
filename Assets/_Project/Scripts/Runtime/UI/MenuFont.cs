using TMPro;
using UnityEngine;

namespace BurnOut.UI
{
    // Shared loader for the Chiller display font used across the main menu and the in-game pause UI,
    // so every menu caption reads in the same grungy horror tone as the BURN OUT title.
    public static class MenuFont
    {
        // A runtime-created font asset can be unloaded on scene reload, and Unity reports a destroyed
        // object as == null. We null-check the cache (never a stale "loaded" flag) so it rebuilds when
        // needed, and DontUnloadUnusedAsset lets it survive ordinary scene loads.
        private static TMP_FontAsset cache;

        public static TMP_FontAsset Chiller
        {
            get
            {
                if (cache != null) return cache;
                var ttf = Resources.Load<Font>("Fonts/Chiller");
                if (ttf == null) return null;
                cache = TMP_FontAsset.CreateFontAsset(ttf);
                if (cache != null) cache.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return cache;
            }
        }
    }
}
