#if g1_3
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches;

public sealed class Need_Indoors_NeedInterval() : Patch(
	transpiler: RemoveLastMin.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(Need_Indoors)
		.Method(nameof(Need_Indoors.NeedInterval))];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Indoors)));
}

#endif
