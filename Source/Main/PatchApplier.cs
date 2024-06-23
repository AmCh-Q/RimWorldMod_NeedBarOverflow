using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NeedBarOverflow.Patches.ModCompat;
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
			IEnumerable<Patch> patchesCandidate = GenTypes
				.AllTypes
				//.AsParallel()
				.Where(type
					=> !type.IsAbstract
					&& type.IsSubclassOf(typeof(Patch)))
				.Select(type => Activator.CreateInstance(type))
				.Cast<Patch>()
				.Where(patch => patch.Patchable);
			foreach (Patch patch in patchesCandidate)
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
