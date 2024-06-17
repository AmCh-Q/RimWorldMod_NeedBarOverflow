using NeedBarOverflow.Patches;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersionAttribute("1.2.7")]

namespace NeedBarOverflow
{
	public class NeedBarOverflow : Mod
	{
		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow constructor called");
			Savefilebackcompat.ModifySettingFileClass(content);
			PatchApplier.s = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				Debug.Message("NeedBarOverflow LongEvent called");
				Refs.Init();
				Needs.Setting_Common.DisablingDefs.LoadDisabledDefs();
				Needs.Setting_Food.ApplyFoodHediffSettings();
				PatchApplier.ApplyPatches();
				if (Settings.migrateSettings == 2)
					PatchApplier.s.Write();
			}, "NeedBarOverflow.Mod.ctor", false, null);
		}

		public override string SettingsCategory() => Strings.Name.Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			PatchApplier.s?.DoWindowContents(inRect);
		}
	}
}
