#if g1_4
using System;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches;

// Caller of this method may expect 1f to be the max mood possible
//   and thus pass in a mood range of (somevalue, 1f)
// This assumption would be broken by our mod, so we fix that
public sealed class ThoughtWorker_RelatedChildMoodBase_IsChildWithMood()
	: Patch_Single(
	original:
		((Delegate)ThoughtWorker_RelatedChildMoodBase.IsChildWithMood).Method,
	prefix: PrefixMethod)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
	private static void PrefixMethod(ref FloatRange moodRange)
	{
		if (moodRange.max == 1f)
			moodRange.max = float.PositiveInfinity;
	}
}
#endif
