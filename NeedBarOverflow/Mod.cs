using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	using C = Consts;
	using static Patches.PatchApplier;
	public class NeedBarOverflow : Mod
	{
		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow constructor called");
			s = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				Refs.Init();
				ApplyPatches();
				s.ApplyFoodHediffSettings();
				s.ApplyFoodDisablingSettings<ThingDef>(C.ThingDef);
				s.ApplyFoodDisablingSettings<HediffDef>(C.HediffDef);
			}, "NeedBarOverflow.Mod.ctor", false, null);
		}
		public override string SettingsCategory() => "NBO.Name".Translate();
		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			s.DoWindowContents(inRect);
		}
	}
}
