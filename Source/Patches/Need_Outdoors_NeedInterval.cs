using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class Need_Outdoors_NeedInterval() : Patch(
	transpiler: RemoveLastMin.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(Need_Outdoors)
		.Method(nameof(Need_Outdoors.NeedInterval))];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Outdoors)));
}
