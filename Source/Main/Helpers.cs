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
			float scale = Mathf.Floor(Mathf.Log10(Mathf.Abs(d)));
			scale = Mathf.Pow(10, scale - 2f);
			scale = Mathf.Max(scale, 0.01f);
			return scale * Mathf.Round(d / scale);
		}

		internal static string CustomToString(this float d, bool showAsPerc, bool translate)
		{
			if (float.IsInfinity(d) && translate)
				return "∞";
			d = d.CustomRound();
			if (showAsPerc)
				return Mathf.RoundToInt(d * 100f).ToStringCached() + "%";
			if (d >= 10f)
				return Mathf.RoundToInt(d).ToStringCached();
			if (d >= 1f)
				return d.ToString("N1");
			return d.ToString("N2");
		}
	}
}
