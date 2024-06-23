using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeedBarOverflow.Patches
{
	public abstract class Patch_Multi : Patch<MethodBase[]>
	{
		public Patch_Multi(
			IEnumerable<MethodBase> original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: base(
				  [.. original.Where(p => p is not null).OrderBy(p => p!.Name)],
				  prefix, postfix, transpiler, finalizer)
		{ }
		public Patch_Multi(
			IEnumerable<Delegate> original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: base(
				  [.. original.OfType<Delegate>().Select(d => d.Method).OrderBy(p => p!.Name)],
				  prefix, postfix, transpiler, finalizer)
		{ }
		public override bool Patchable
			=> Original.Length != 0;
		public override bool Patched
			=> Original.Any(Patched_Method);
		public override bool Equals(Patch other)
		{
			if (Patches != other.Patches)
				return false;
			if (other is Patch<MethodBase?> patchSingle)
				return Original.Contains(patchSingle.Original);
			if (other is Patch<MethodBase?[]> patchMulti)
				return Original.Intersect(patchMulti.Original).Any();
			Debug.Error("Not Implmented");
			return false;
		}
		public override int GetHashCode()
		{
			if (Original.Length == 0)
				return 0;
			return Original[0]?.GetHashCode() ?? 0;
		}
		public override void Toggle(bool enable)
		{
			if (enable == Patched)
				return;
			if (enable)
				Dopatch();
			else
				Unpatch();
		}
		protected override void Dopatch()
		{
			foreach (MethodBase original in Original)
			{
				Debug.Message("Patching Method "
					+ original.DeclaringType.Name
					+ ":" + original.Name);
				harmony.Patch(original,
					prefix: Prefix,
					postfix: Postfix,
					transpiler: Transpiler,
					finalizer: Finalizer);
			}
		}
		protected override void Unpatch()
		{
			foreach (MethodBase original in Original)
			{
				Debug.Message("Unpatching Method "
					+ original.DeclaringType.Name
					+ ":" + original.Name);
				harmony.Unpatch(original, HarmonyPatchType.All, harmony.Id);
			}
		}
	}
}
