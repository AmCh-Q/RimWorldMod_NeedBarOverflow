using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Joy_GainJoy() : Patch_Single(
		original: typeof(Need_Joy).Method(nameof(Need_Joy.GainJoy)),
		prefix: PrefixMethod,
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
		{
			Toggle(Setting_Common.Enabled(typeof(Need_Joy)),
				OverflowStats<Need_Joy>.EffectEnabled(StatName_DG.SlowGain));
		}
		public override void Toggle(bool enable)
		{
			Toggle(enable,
				enable && OverflowStats<Need_Joy>
				.EffectEnabled(StatName_DG.SlowGain));
		}
		public void Toggle(bool enable, bool enableGain)
		{
			if (enable)
				Dopatch(enable, enableGain);
			if (!enable || !enableGain)
				Unpatch(enable, enableGain);
		}
		protected override void Dopatch()
			=> Dopatch(true, OverflowStats<Need_Joy>.EffectEnabled(StatName_DG.SlowGain));
		protected override void Unpatch()
			=> Unpatch(false, false);
		private void Dopatch(bool enable, bool enableGain)
		{
			Debug.Message("Patching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			HarmonyLib.Patches? patches = PatchProcessor.GetPatchInfo(Original);
			bool patchTranspiler = enable &&
				(patches is null || patches.Transpilers.All(p => p.owner != harmony.Id));
			bool patchPrefix = enable && enableGain &&
				(patches is null || patches.Prefixes.All(p => p.owner != harmony.Id));
			if (!patchTranspiler && !patchPrefix)
				return;
			harmony.Patch(Original,
				prefix: Prefix,
				transpiler: Transpiler);
		}
		private void Unpatch(bool enable, bool enableGain)
		{
			HarmonyLib.Patches? patches = PatchProcessor.GetPatchInfo(Original);
			if (patches is null)
				return;
			bool unpatchTranspiler = !enable &&
				patches.Transpilers.Any(p => p.owner == harmony.Id);
			bool unpatchPrefix = !(enable && enableGain) &&
				patches.Prefixes.Any(p => p.owner == harmony.Id);
			if (!unpatchTranspiler && !unpatchPrefix)
				return;
			HarmonyPatchType unpatchType = HarmonyPatchType.All;
			if (unpatchTranspiler && !unpatchPrefix)
				unpatchType = HarmonyPatchType.Transpiler;
			else if (!unpatchTranspiler && unpatchPrefix)
				unpatchType = HarmonyPatchType.Prefix;
			harmony.Unpatch(Original, unpatchType, harmony.Id);
		}
		private static void PrefixMethod(Need_Joy __instance, ref float amount)
		{
			amount = AdjustGain.AdjustMethod(amount,
				OverflowStats<Need_Joy>.EffectStat(StatName_DG.SlowGain),
				__instance.CurInstantLevelPercentage);
		}
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			Label[] jumpLabels = new Label[2];
			for (int i = 0; i < 2; i++)
				jumpLabels[i] = ilg.DefineLabel();
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				if (state == 0 && i >= 1 && i < instructionList.Count - 4 &&// Haven't Patched yet, and not at the end of instructions
					instructionList[i - 1].opcode == OpCodes.Ldarg_1 &&  // Vanilla would load amount of joy to add
					codeInstruction.LoadsConstant(1d) &&                 // Vanilla would load const 1f
					instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 2].Calls(Refs.get_CurLevel) &&// Vanilla would get CurLevel
					instructionList[i + 3].opcode == OpCodes.Sub &&      // Vanilla would calculate 1f - CurLevel
					instructionList[i + 4].Calls(Refs.m_Min))         // Vanilla would calculate Min(amount, 1f - CurLevel)
				{
					state = 1;
					// Load the setting max joy instead of 1f
					// So that Vanilla will calculate Min(amount, MaxJoy - CurLevel) instead
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, Refs.m_CanOverflow);
					yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabels[0]);
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Br_S, jumpLabels[1]);
					yield return new CodeInstruction(OpCodes.Call,
						Setting<Need_Joy>.mget_MaxValue).WithLabels(jumpLabels[0]);
					yield return instructionList[i + 1].WithLabels(jumpLabels[1]);
					i++;
					// Skip the load Constant
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
