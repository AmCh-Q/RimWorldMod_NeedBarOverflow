#if !v1_2
using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Indoors_NeedInterval() : Patch_Single(
		original: typeof(Need_Indoors)
			.Method(nameof(Need_Indoors.NeedInterval)),
		transpiler: RemoveLastMin.d_transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Indoors)));
	}
}
#endif
