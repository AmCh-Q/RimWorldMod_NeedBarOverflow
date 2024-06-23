using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace NeedBarOverflow.Patches
{
	public abstract class Patch_Single(
		MethodBase original,
		Delegate? prefix = null,
		Delegate? postfix = null,
		Delegate? transpiler = null,
		Delegate? finalizer = null)
		: Patch<MethodBase?>(original, prefix, postfix, transpiler, finalizer)
	{
		public Patch_Single(
			Delegate original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: this(
				  original.Method,
				  prefix,
				  postfix,
				  transpiler,
				  finalizer)
		{ }
		//public Patch_Single(
		//	Expression original,
		//	Delegate? prefix = null,
		//	Delegate? postfix = null,
		//	Delegate? transpiler = null,
		//	Delegate? finalizer = null)
		//	: this(
		//		  null!,
		//		  prefix?.Method,
		//		  postfix?.Method,
		//		  transpiler?.Method,
		//		  finalizer?.Method)
		//{
		//	Original = null;
		//}
		public override bool Patchable
			=> Original is not null;
		public override bool Patched
			=> Patched_Method(Original);
		public override bool Equals(Patch other)
		{
			if (Patches != other.Patches || Original is null)
				return false;
			if (other is Patch<MethodBase?> patchSingle)
				return Original == patchSingle.Original;
			if (other is Patch<MethodBase?[]> patchMulti)
				return patchMulti.Original.Contains(Original);
			Debug.Error("Not Implmented");
			return false;
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
			Original.NotNull("Patch Original " + GetType().Name);
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
			Original.NotNull("Patch Original " + GetType().Name);
			Debug.Message("Unpatching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			harmony.Unpatch(Original, HarmonyPatchType.All, harmony.Id);
		}
	}
}
