using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class SavedGameLoaderNow_LoadGameFromSaveFileNow() : Patch_Single(
		original: SavedGameLoaderNow.LoadGameFromSaveFileNow,
		postfix: PostfixMethod)
	{
		public override void Toggle()
			=> Toggle(true);
		private static void PostfixMethod()
		{
			Need_Food_NeedInterval.ResetHediff();
			DisableNeedOverflow.Cache.CanOverflow_Clear();
		}
	}
}
