using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Outdoors_NeedInterval() : Patch_Single(
		original: typeof(Need_Outdoors)
			.Method(nameof(Need_Outdoors.NeedInterval)),
		transpiler: RemoveLastMin.d_transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Outdoors)));
	}
}
