using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Food_NutritionWanted() : Patch_Single(
		original: typeof(Need_Food).Getter(nameof(Need_Food.NutritionWanted)),
		postfix: Add0LowerBound.postfix)
	{
		public override void Toggle()
			=> Toggle(Setting_Food.Enabled);
	}
}
