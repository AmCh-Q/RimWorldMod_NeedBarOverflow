using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Verse;

namespace NeedBarOverflow
{
	internal static class Debug
	{
		private const string prefix = "[Need Bar Overflow]: ";
		private static Stopwatch? watch;

		[Conditional("DEBUG")]
		internal static void Message(string s)
			=> Log.Message(prefix + s);

		[Conditional("DEBUG")]
		internal static void Warning(string s)
			=> Log.Warning(prefix + s);

		//[Conditional("DEBUG")]
		internal static void Error(string s)
			=> Log.Error(prefix + s);

		//[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, bool assertResult,
			string transpilerName = "Unknown")
		{
			if (assertResult)
				return;
			Log.Error(string.Concat(
				"Patch ", transpilerName,
				" had error applying (state: ", state,
				")"));
		}

		//[Conditional("DEBUG")]
		internal static void CheckTranspiler(
			int state, int expectedState,
			string transpilerName = "Unknown")
		{
			if (state >= expectedState)
				return;
			Log.Error(string.Concat(
				"Patch ", transpilerName,
				"is not fully applied (state: ", state,
				" < ", expectedState, ")"));
		}

		//[Conditional("DEBUG")]
		internal static void Assert(
			this bool assert, string name)
		{
			if (assert)
				return;
			Log.Error(string.Concat(
				"Assertion [",
				name, "] failed"));
		}

		//[Conditional("DEBUG")]
		internal static T NotNull<T>(
			this object? obj, string name = "Unknown")
		{
			if (obj is T item)
				return item;
			Log.Error(string.Concat(
				"Object ", name,
				" is null or not of type ",
				typeof(T).Name));
			return default!;
		}

		//[Conditional("DEBUG")]
		internal static T NotNull<T>(
			this T? obj, string name)
		{
			if (obj is T item)
				return item;
			Log.Error(string.Concat(
				"Object ", name,
				" is null or not of type ",
				typeof(T).Name));
			return default!;
		}

		[Conditional("DEBUG")]
		internal static void WatchStart(string message = "")
		{
			if (!message.NullOrEmpty())
				Message(message);
			if (watch is null)
				watch = Stopwatch.StartNew();
			else
				watch.Restart();
		}

		[Conditional("DEBUG")]
		internal static void WatchLog(string name, string message = "")
		{
			Message(string.Concat(
				name, ": ",
				watch?.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)
				?? "Unknown",
				" ms"));
			if (watch is null)
				watch = Stopwatch.StartNew();
			else
				watch.Restart();
			if (!message.NullOrEmpty())
				Message(message);
		}

		[Conditional("DEBUG")]
		internal static void WatchStop(string message = "")
		{
			if (!message.NullOrEmpty())
				Message(message);
			watch?.Stop();
			watch = null;
		}
	}
}
