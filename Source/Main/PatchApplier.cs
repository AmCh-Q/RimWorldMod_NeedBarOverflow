using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NeedBarOverflow.Patches;

public static class PatchApplier
{
	public static List<Patch> Patches { get; } = [];
	static PatchApplier()
	{
		Debug.WatchStart($"static constructor of type [{typeof(PatchApplier).FullName}] called");
		LoadPatches();
		Debug.WatchStop($"static constructor of type [{typeof(PatchApplier).FullName}]: {Patches.Count} patches loaded");
	}
	public static bool Patched(Type type)
		=> Patches.Any(patch => patch.GetType() == type);
	private static void LoadPatches()
	{
		IEnumerable<Patch> patches = typeof(Patch)
			.AllSubclassesNonAbstract()
			.Select(Patch.TryCreatePatch)
			.Where(patch => patch is not null)
			.Cast<Patch>();
		foreach (Patch patch in patches)
		{
			Patch? existing = Patches
				.FirstOrDefault(existing => existing.Equals(patch));
			if (existing is null)
			{
				Patches.Add(patch);
				Debug.Message("Added Patch " + patch.GetType());
				continue;
			}
			Debug.Error(string.Concat(
				"Multiple conflicting patches: [",
				patch.GetType().Name, "], [",
				existing.GetType().Name, "]."));
		}
	}
	public static void ApplyPatches()
	{
		foreach (Patch patch in Patches)
			patch.Toggle();
	}
}
