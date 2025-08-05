using NeedBarOverflow.Patches;
using RimWorld;
using Verse;

namespace NeedBarOverflow.ModCompat;

// CM Color Coded Mood Bar [1.1+]
// https://steamcommunity.com/sharedfiles/filedetails/?id=2006605356
public sealed class CM_Color_Coded_Mood_Bar() : Patch_Single(
	original: ModsConfig.IsActive("CM Color Coded Mood Bar [1.1+]")
	? GenTypes.GetTypeInAnyAssembly("ColoredMoodBar13.MoodCache")?
		.MethodNullable("DoMood") : null,
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
