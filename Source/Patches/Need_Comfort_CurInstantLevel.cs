using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches;

public sealed class Need_Comfort_CurInstantLevel() : Patch_Single(
	original: typeof(Need_Comfort)
		.Getter(nameof(Need_Comfort.CurInstantLevel)),
	transpiler: TranspilerMethod)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Comfort)));
	private static IEnumerable<CodeInstruction> TranspilerMethod(
		IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		=> ModifyClamp01.TranspilerMethod(instructions, ilg,
			Setting<Need_Comfort>.mget_MaxValue);
}
