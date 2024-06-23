using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Beauty_LevelFromBeauty() : Patch_Single(
		original: typeof(Need_Beauty).Method("LevelFromBeauty"),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Beauty)));
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
			=> ModifyClamp01.TranspilerMethod(instructions, ilg,
				Setting<Need_Beauty>.mget_MaxValue);
	}
}
