using NeedBarOverflow.Needs;
using RimWorld;

namespace NeedBarOverflow.Patches.ModCompat
{
	// CM Color Coded Mood Bar [1.1+]
	// https://steamcommunity.com/sharedfiles/filedetails/?id=2006605356
	public sealed class CM_Color_Coded_Mood_Bar() : Patch_Single(
		original: Helpers
			.TypeByName("ColoredMoodBar13.MoodCache")?
			.Method("DoMood")!,
		transpiler: Add1UpperBound.d_transpiler)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
	}
}
