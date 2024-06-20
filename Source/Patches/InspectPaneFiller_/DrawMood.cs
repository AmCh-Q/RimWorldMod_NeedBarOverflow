using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches.InspectPaneFiller_
{
	public sealed class DrawMood() : Patch_Single(
		original: typeof(InspectPaneFiller).Method("DrawMood"),
		transpiler: Add1UpperBound.transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
	}
}
