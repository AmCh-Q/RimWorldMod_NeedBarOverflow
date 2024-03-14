using System.Text;
using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	internal static class NeedBarOverflow_Extensions
	{
		internal static string Cats(this StringBuilder sb, params string[] strs)
		{
			for (int i = 0; i < strs.Length; i++)
				sb.Append(strs[i]);
			return sb.ToString();
		}
		internal static string Trans(this StringBuilder sb, params string[] strs) => sb.Cats(strs).Translate().ToString();
		internal static float Round(this float d)
		{
			if (Mathf.Abs(d) <= 0.005f || float.IsNaN(d))
				return 0;
			else if (float.IsInfinity(d))
				return d;
			float scale = Mathf.Max(Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(d))) - 2), 0.01f);
			return scale * Mathf.Round(d / scale);
		}
	}
	internal static class NeedBarOverflow_Debug
	{
#if DEBUG
		internal static void Message(string s) => Log.Message(s);
		internal static void Warning(string s) => Log.Warning(s);
		internal static void Error(string s) => Log.Error(s);
		internal static void CheckTranspiler(int state, int expectedState, string transpilerName = "Unknown")
        {
			if (state < expectedState)
				Log.Warning(string.Format("[Need Bar Overflow]: Patch {0} is not fully applied (state: {1} < {2})", 
					transpilerName, state, expectedState));
		}
#else
		internal static void Message(string _) {}
		internal static void Warning(string _) {}
		internal static void Error(string _) {}
		internal static void CheckTranspiler(int _, int __) {}
#endif
	}
}
