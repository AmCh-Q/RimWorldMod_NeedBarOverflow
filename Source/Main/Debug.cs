using System.Reflection;
using System.Diagnostics;
using Verse;

namespace NeedBarOverflow
{
	internal static class Debug
	{
		[Conditional("DEBUG")]
		internal static void Message(string s) 
			=> Log.Message("[Need Bar Overflow]: " + s);
		[Conditional("DEBUG")]
		internal static void Warning(string s) 
			=> Log.Warning("[Need Bar Overflow]: " + s);
		[Conditional("DEBUG")]
		internal static void Error(string s) 
			=> Log.Error("[Need Bar Overflow]: " + s);
		[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, bool assertResult, 
			string transpilerName = "Unknown")
		{
			if (!assertResult)
				Log.Error("[Need Bar Overflow]: " + string.Format(
					"Patch {0} had error applying (state: {1})",
					transpilerName, state));
		}
		[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, int expectedState, 
			string transpilerName = "Unknown")
		{
			if (state < expectedState)
				Log.Error("[Need Bar Overflow]: " + string.Format(
					"Patch {0} is not fully applied (state: {1} < {2})", 
					transpilerName, state, expectedState));
		}
		internal static T NotNull<T>(
			this T method, string name) where T : MemberInfo
		{
#if DEBUG
			if (method == null)
				Log.Error("[Need Bar Overflow]: MethodInfo "
					+ name + " is null");
#endif
			return method;
		}
	}
}
