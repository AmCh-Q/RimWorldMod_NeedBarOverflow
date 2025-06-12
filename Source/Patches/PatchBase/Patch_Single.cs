using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace NeedBarOverflow.Patches
{
	public abstract class Patch_Single : Patch<MethodBase?>
	{
		protected Patch_Single(
			MethodBase? original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: base(original,
				  new Patches(prefix, postfix, transpiler, finalizer))
		{ }
		protected Patch_Single(
			Delegate? original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: base(original?.Method,
				  new Patches(prefix, postfix, transpiler, finalizer))
		{ }
		public override bool Patchable
			=> Original is not null;
		public override bool Patched
			=> Patched_Method(Original);
		public override bool Equals(Patch other)
		{
			if (Patches != other.Patches || Original is null)
				return false;
			switch (other)
			{
				case Patch<MethodBase?> patchSingle:
					return Original == patchSingle.Original;
				case Patch<MethodBase[]> patchMulti:
					return patchMulti.Original.Contains(Original);
				default:
					Debug.Error("Not Implemented");
					return false;
			}
		}
		public override int GetHashCode()
			=> Original?.GetHashCode() ?? 0;
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
			if (Original is null)
			{
				Debug.Warning("Patch " + GetType().Name + " has null original");
				return;
			}
			Debug.Message("Patching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			harmony.Patch(Original,
				prefix: Prefix,
				postfix: Postfix,
				transpiler: Transpiler,
				finalizer: Finalizer);
		}
		protected override void Unpatch()
		{
			if (Original is null)
			{
				Debug.Warning("Patch " + GetType().Name + " has null original");
				return;
			}
			Debug.Message("Unpatching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			harmony.Unpatch(Original, HarmonyPatchType.All, harmony.Id);
		}
	}
}
