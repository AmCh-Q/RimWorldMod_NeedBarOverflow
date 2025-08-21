using RimWorld;

namespace NeedBarOverflow.Patches;

// Some outlier pawns overflowing may skew the average result
// Enabling this does make the game slightly harder
// (maybe more balanced?)
// I am not sure if I should enable this patch yet...
#if false
public sealed class QuestPart_FactionGoodwillForMoodChange_AveragePawnMoodPercent()
	: Patch_Single(
	original: typeof(QuestPart_FactionGoodwillForMoodChange)
		.Getter("AveragePawnMoodPercent"),
	transpiler: Add1UpperBound.d_transpiler)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
#endif
