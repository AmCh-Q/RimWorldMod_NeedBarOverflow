﻿using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class SavedGameLoaderNow_LoadGameFromSaveFileNow() : Patch_Single(
		original: typeof(SavedGameLoaderNow)
			.Method(nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow)),
		postfix: PostfixMethod)
	{
		public override void Toggle()
			=> Toggle(true);
		private static void PostfixMethod()
			=> Need_Food_NeedInterval.ResetHediff();
	}
}
