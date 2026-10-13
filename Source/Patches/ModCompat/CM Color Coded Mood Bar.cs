using NeedBarOverflow.Patches;
using RimWorld;
using System;
using Verse;

namespace NeedBarOverflow.ModCompat;

// CM Color Coded Mood Bar [1.1+]
// https://steamcommunity.com/sharedfiles/filedetails/?id=2006605356
public sealed class CM_Color_Coded_Mood_Bar() : Patch(
	transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
	{
		if (Target is not null)
			return;
		if (!ModsConfig.IsActive("CrashM.ColorCodedMoodBar.11"))
			return;
		Type? type = GenTypes.GetTypeInAnyAssembly("ColoredMoodBar13.MoodCache");
		if (type is null)
		{
			Debug.Warning("ColoredMoodBar13.MoodCache is null");
			return;
		}
		Target = [type.Method("DoMood")];
	}
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
