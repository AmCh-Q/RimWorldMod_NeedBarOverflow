#if DEBUG
using Verse;
#endif

namespace NeedBarOverflow
{
    internal static class Debug
    {
#if DEBUG
		internal static void Message(string s) => Log.Message("[Need Bar Overflow]: " + s);
		internal static void Warning(string s) => Log.Warning("[Need Bar Overflow]: " + s);
		internal static void Error(string s) => Log.Error("[Need Bar Overflow]: " + s);
        internal static void CheckTranspiler(int state, bool assertResult, string transpilerName = "Unknown")
        {
            if (!assertResult)
                Log.Warning(string.Format("[Need Bar Overflow]: Patch {0} had error applying (state: {1})",
                    transpilerName, state));
        }
        internal static void CheckTranspiler(int state, int expectedState, string transpilerName = "Unknown")
        {
			if (state < expectedState)
				Log.Warning(string.Format("[Need Bar Overflow]: Patch {0} is not fully applied (state: {1} < {2})", 
					transpilerName, state, expectedState));
		}
#else
        internal static void Message(string _) { }
        internal static void Warning(string _) { }
        internal static void Error(string _) { }
        internal static void CheckTranspiler(int _, bool __, string ___ = null) { }
        internal static void CheckTranspiler(int _, int __, string ___ = null) { }
#endif
    }
}
