using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace NeedBarOverflow.Patches
{
	public static class PatchApplier
	{
		public static Settings? settings;
		public static List<Patch> Patches { get; } = [];
		static PatchApplier()
		{
			Debug.WatchStart("static PatchApplier()");
			List<Patch> patches = GenTypes
				.AllTypes
				//.AsParallel()
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
			Debug.WatchLog($"static PatchApplier(): {Patches.Count} patches loaded");
			Debug.WatchStop();
		}
		public static bool Patched(Type type)
			=> Patches.Any(patch => patch.GetType() == type);
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
				Debug.Warning(ex.GetType().FullName);
				return null;
			}
		}
		public static void ApplyPatches()
		{
			if (settings is null)
				return;
			Debug.WatchStart("Applying Patches...");
			foreach (Patch patch in Patches)
				patch.Toggle();
			Debug.WatchLog("Done Applying Patches");
			Debug.WatchStop();
		}
	}
}
