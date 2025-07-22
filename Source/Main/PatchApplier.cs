using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches
{
	[StaticConstructorOnStartup]
	public static class PatchApplier
	{
		public static List<Patch> Patches { get; } = [];
		static PatchApplier()
		{
			Debug.WatchStart("static PatchApplier()");
			LoadPatches();
			Debug.WatchLog($"static PatchApplier(): {Patches.Count} patches loaded");
			ApplyPatches();
			Debug.WatchLog("Done Applying Patches");
			Debug.WatchStop();
		}
		public static bool Patched(Type type)
			=> Patches.Any(patch => patch.GetType() == type);
		public static void LoadPatches()
		{
			List<Patch> patches = GenTypes
				.AllTypes
				.AsParallel()
				.Where(type
					=> !type.IsAbstract
					&& type.IsSubclassOf(typeof(Patch)))
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
		public static Patch? CreatePatch(Type patchClass)
		{
			try
			{
				Patch patch = (Patch)Activator.CreateInstance(patchClass);
				return patch;
			}
			catch (Exception ex)
			{
				Debug.Warning("Error in patch " + patchClass);
				Debug.Warning(ex.ToString());
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
