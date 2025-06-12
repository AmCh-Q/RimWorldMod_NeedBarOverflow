using NeedBarOverflow.Patches;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersionAttribute("1.2.8.0")]

namespace NeedBarOverflow
{
	public class NeedBarOverflow : Mod
	{
		public static bool Initialized => PatchApplier.settings is not null;

		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow constructor called");
			// Old settings file compatibility, will be removed for 1.6
			Savefilebackcompat.ModifySettingFileClass(content);
			PatchApplier.settings = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(Refs.Init,
				"NeedBarOverflow.Mod.ctor", false, null);
		}

		public override string SettingsCategory()
			=> Strings.Name.Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			PatchApplier.settings?.DoWindowContents(inRect);
		}
	}
}
