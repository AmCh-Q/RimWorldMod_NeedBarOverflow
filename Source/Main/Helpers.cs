using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	internal static class Helpers
	{
		internal static float CustomRound(this float d)
		{
			if (Mathf.Abs(d) <= 0.005f || float.IsNaN(d))
				return 0;
			else if (float.IsInfinity(d))
				return d;
			float scale = Mathf.Max(Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(d))) - 2), 0.01f);
			return scale * Mathf.Round(d / scale);
        }

        internal static string CustomToString(this float d, bool showAsPerc, bool translate)
        {
            if (float.IsInfinity(d) && translate)
                return "∞";
            d = d.CustomRound();
            if (showAsPerc)
                return d.ToStringPercent();
            return d.ToString((d < 1f) ? "N2" : ((d < 10f) ? "N1" : "0"));
        }
    }
}
