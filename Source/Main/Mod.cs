using NeedBarOverflow.Patches;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersionAttribute("1.4.2.0")]

namespace NeedBarOverflow
{
	public class NeedBarOverflow : Mod
	{
		public static Settings? settings;
		public static NeedBarOverflow? ModInstance { get; private set; }

		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow mod constructor called");
			ModInstance = this;
		}

		public override string SettingsCategory()
			=> Strings.Name.Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			settings?.DoWindowContents(inRect);
			base.DoSettingsWindowContents(inRect);
		}
	}
}
