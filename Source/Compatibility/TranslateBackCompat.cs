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

		internal static string MyTranslate(this string key)
		{
			if (key.TryTranslate(out TaggedString result))
				return result;
			if (backUpKeys.TryGetValue(key, out string backup) &&
				backup.TryTranslate(out result))
				return result;
			return key.Translate();
		}
	}
}
