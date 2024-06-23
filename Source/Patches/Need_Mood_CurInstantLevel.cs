using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Mood_CurInstantLevel() : Patch_Single(
		original: typeof(Need_Mood)
			.Getter(nameof(Need_Mood.CurInstantLevel)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
			=> ModifyClamp01.TranspilerMethod(instructions, ilg,
				Setting<Need_Mood>.mget_MaxValue);
	}
}
