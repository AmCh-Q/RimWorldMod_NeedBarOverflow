using System.Collections.Generic;
using Verse;

namespace NeedBarOverflow
{
	internal static class TranslateBackCompat
	{
		private static readonly IReadOnlyDictionary<string, string> backUpKeys = new Dictionary<string, string>()
		{

		};

		internal static string MyTranslate(
			this string str, params NamedArgument[] args)
			=> MyTranslate(str).Formatted(args);

		internal static string MyTranslate(this string str)
		{
			if (backUpKeys.TryGetValue(str, out var value))
				return str.TranslateWithBackup(value);
			return str.Translate();
		}
	}
}
