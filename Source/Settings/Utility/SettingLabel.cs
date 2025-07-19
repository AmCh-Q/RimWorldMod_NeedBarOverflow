using System.Runtime.CompilerServices;
using Verse;

namespace NeedBarOverflow
{
	public class SettingLabel
	{
		public readonly string name, label, tip;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string TranslatedLabel()
		  => label.Translate();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string TranslatedLabel(NamedArgument arg)
		  => label.Translate(arg);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string TranslatedTip()
		  => tip.Translate();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string TranslatedTip(NamedArgument arg)
		  => tip.Translate(arg);

		public SettingLabel(string typeName, string settingsName)
		{
			string text = typeName.NullOrEmpty() ? string.Empty : ".";
			name = settingsName.Replace('_', '.');
			label = string.Concat("NBO.", typeName, text, name);
			tip = label + "_Tip";
		}
	}
}
