using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches
{
	public static class PatchApplier
	{
		public static List<Patch> Patches { get; } = [];
		static PatchApplier()
		{
			Debug.WatchStart($"static constructor of type [{typeof(PatchApplier).FullName}] called");
			LoadPatches();
			Debug.WatchStop($"static constructor: {Patches.Count} patches loaded");
		}
		public static bool Patched(Type type)
			=> Patches.Any(patch => patch.GetType() == type);
		private static void LoadPatches()
		{
			List<Patch> patches = typeof(Patch)
				.AllSubclassesNonAbstract()
				.Select(CreatePatch)
				.Where(patch => patch is not null && patch.Patchable)
				.Cast<Patch>()
				.ToList();
			foreach (Patch patch in patches)
			{
				Patch? existing = Patches
					.Where(existing => existing.Equals(patch))
					.FirstOrDefault();
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
		private static Patch? CreatePatch(Type patchClass)
		{
			try
			{
				Patch patch = (Patch)Activator.CreateInstance(patchClass);
				return patch;
			}
			catch (Exception e)
			{
				Debug.Warning($"Error in patch {patchClass}: {e}");
				return null;
			}
		}
		public static void ApplyPatches()
		{
			foreach (Patch patch in Patches)
				patch.Toggle();
		}
	}
}
