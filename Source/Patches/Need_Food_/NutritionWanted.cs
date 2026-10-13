using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class Need_Food_NutritionWanted() : Patch(
	postfix: Add0LowerBound.d_postfix)
{
	protected override void Initialize()
		=> Target ??= [typeof(Need_Food).Getter(nameof(Need_Food.NutritionWanted))];
	public override void Toggle()
		=> Toggle(Setting_Food.Enabled);
}
