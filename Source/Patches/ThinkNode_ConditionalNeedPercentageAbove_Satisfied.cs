using RimWorld;

namespace NeedBarOverflow.Patches;

// Some mods (such as Tweaks Galore) modify the need threshold to be 1f
//   with the expectation that the ThinkNode would never be satisfied
// But this mod allows needs to be > 1f, breaking that assumption
//   we restabilsh that assumption by short circuiting
//   so that if the threshold is exactly 1f, it'd never be satisfied
public sealed class ThinkNode_ConditionalNeedPercentageAbove_Satisfied()
	: Patch_Single(
	original: typeof(ThinkNode_ConditionalNeedPercentageAbove)
		.Method("Satisfied"),
	prefix: PrefixMethod)
{
	public override void Toggle()
		=> Toggle(Setting_Common.AnyEnabled);
	private static bool PrefixMethod(
		ref bool __result, float ___threshold)
	{
		if (___threshold == 1f)
		{
			__result = false;
			return false;
		}
		return true;
	}
}
