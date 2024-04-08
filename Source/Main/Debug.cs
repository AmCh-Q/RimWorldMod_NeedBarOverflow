using System.Reflection;
using System.Diagnostics;
using Verse;

namespace NeedBarOverflow
{
	internal static class Debug
    {
		private const string prefix = "[Need Bar Overflow]: ";
        [Conditional("DEBUG")]
		internal static void Message(string s) 
			=> Log.Message(prefix + s);
		[Conditional("DEBUG")]
		internal static void Warning(string s) 
			=> Log.Warning(prefix + s);
		[Conditional("DEBUG")]
		internal static void Error(string s) 
			=> Log.Error(prefix + s);
		[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, bool assertResult, 
			string transpilerName = "Unknown")
		{
			if (!assertResult)
				Log.Error(string.Concat(
                    "Patch ", transpilerName,
					" had error applying (state: ", state,
					")"));
		}
		[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, int expectedState, 
			string transpilerName = "Unknown")
		{
			if (state < expectedState)
				Log.Error(string.Concat(
                    "Patch ", transpilerName,
					"is not fully applied (state: ", state,
					" < ", expectedState, ")"));
        }
        [Conditional("DEBUG")]
        internal static void NotNull<T>(
			this T obj, string name) where T : MemberInfo
		{
			if (obj == null)
				Log.Error("MemberInfo "
                    + name + " is null");
        }
    }
}
