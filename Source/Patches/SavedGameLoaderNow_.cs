using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches.SavedGameLoaderNow_
{
	using static Utility;
	public static class LoadGameFromSaveFileNow
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(SavedGameLoaderNow)
			.Method(nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow));
		private static readonly Action postfix = Postfix;
		public static void Toggle()
			=> Toggle(true);
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					postfix: postfix);
			else
				Unpatch(ref patched, original: original);
		}
		private static void Postfix()
		{
			Need_Food_.NeedInterval.ResetHediff();
		}
	}
}
