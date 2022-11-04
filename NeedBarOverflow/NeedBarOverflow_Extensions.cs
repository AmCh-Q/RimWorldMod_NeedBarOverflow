using System.Text;
using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
    public static class NeedBarOverflow_Extensions
    {
		public static string Cats(this StringBuilder sb, params string[] strs)
		{
			for (int i = 0; i < strs.Length; i++)
				sb.Append(strs[i]);
			return sb.ToString();
		}
		public static string Trans(this StringBuilder sb, params string[] strs) => sb.Cats(strs).Translate().ToString();
		public static float Round(this float d)
		{
			if (Mathf.Abs(d) <= 0.005f || float.IsNaN(d))
				return 0;
			else if (float.IsInfinity(d))
				return d;
			float scale = Mathf.Max(Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(d))) - 2), 0.01f);
			return scale * Mathf.Round(d / scale);
		}
	}
}
