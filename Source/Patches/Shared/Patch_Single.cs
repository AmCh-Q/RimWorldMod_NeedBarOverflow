using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.Patches
{
	public readonly struct Patches(
		MethodInfo? prefix,
		MethodInfo? postfix,
		MethodInfo? transpiler,
		MethodInfo? finalizer)
		: IEquatable<Patches>
	{
		public MethodInfo? Prefix { get; } = prefix;
		public MethodInfo? Postfix { get; } = postfix;
		public MethodInfo? Transpiler { get; } = transpiler;
		public MethodInfo? Finalizer { get; } = finalizer;
		public Patches(
			Delegate? prefix,
			Delegate? postfix,
			Delegate? transpiler,
			Delegate? finalizer)
			: this(prefix?.Method,
				  postfix?.Method,
				  transpiler?.Method,
				  finalizer?.Method)
		{ }
		public override readonly int GetHashCode()
		{
			return Gen.HashCombineInt(
				Prefix?.GetHashCode() ?? 0,
				Postfix?.GetHashCode() ?? 0,
				Transpiler?.GetHashCode() ?? 0,
				Finalizer?.GetHashCode() ?? 0);
		}
		public override readonly bool Equals(object obj)
			=> obj is Patches other && Equals(other);
		public readonly bool Equals(Patches other)
		{
			return Prefix == other.Prefix
				&& Postfix == other.Postfix
				&& Transpiler == other.Transpiler
				&& Finalizer == other.Finalizer;
		}
		public static bool operator ==(Patches lhs, Patches rhs)
			=> lhs.Equals(rhs);
		public static bool operator !=(Patches lhs, Patches rhs)
			=> !lhs.Equals(rhs);
	}

	public abstract class Patch_Single(
		MethodBase? original,
		MethodInfo? prefix = null,
		MethodInfo? postfix = null,
		MethodInfo? transpiler = null,
		MethodInfo? finalizer = null)
		: IEquatable<Patch_Single>
	{
		public static readonly Harmony harmony = Utility.harmony;
		public MethodBase? Original { get; } = original;
		public Patches Patches { get; } = new Patches(prefix, postfix, transpiler, finalizer);
		public MethodInfo? Prefix => Patches.Prefix;
		public MethodInfo? Postfix => Patches.Postfix;
		public MethodInfo? Transpiler => Patches.Transpiler;
		public MethodInfo? Finalizer => Patches.Finalizer;
		private bool Patched
			=> PatchProcessor.GetPatchInfo(Original) is HarmonyLib.Patches patches
			&& (Prefix is not null && patches.Prefixes.Any(p => p.owner == harmony.Id)
			|| Postfix is not null && patches.Postfixes.Any(p => p.owner == harmony.Id)
			|| Transpiler is not null && patches.Transpilers.Any(p => p.owner == harmony.Id));
		public Patch_Single(
			MethodBase? original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
			: this(
				  original,
				  prefix?.Method,
				  postfix?.Method,
				  transpiler?.Method,
				  finalizer?.Method)
		{ }
		public override int GetHashCode()
			=> Original?.GetHashCode() ?? 0;
		public override bool Equals(object obj)
			=> obj is Patch_Single other && Equals(other);
		public bool Equals(Patch_Single other)
			=> Original == other.Original;
		public static bool operator ==(Patch_Single lhs, Patch_Single rhs)
			=> lhs.Equals(rhs);
		public static bool operator !=(Patch_Single lhs, Patch_Single rhs)
			=> !lhs.Equals(rhs);
		public abstract void Toggle();
		public virtual void Toggle(bool enable)
		{
			if (enable)
				Patch();
			else
				Unpatch();
		}
		private void Patch()
		{
			if (Patched)
				return;
			Original.NotNull<MethodInfo>("Patch Original " + GetType().Name);
			Debug.Message("Patching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			HarmonyMethod?
				h_prefix = Prefix is null ? null : new(Prefix),
				h_postfix = Postfix is null ? null : new(Postfix),
				h_transpiler = Transpiler is null ? null : new(Transpiler),
				h_finalizer = Finalizer is null ? null : new(Finalizer);
			harmony.Patch(Original,
				prefix: h_prefix,
				postfix: h_postfix,
				transpiler: h_transpiler,
				finalizer: h_finalizer);
		}
		private void Unpatch()
		{
			if (!Patched)
				return;
			Original.NotNull<MethodInfo>("Patch Original " + GetType().Name);
			Debug.Message("Unpatching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			harmony.Unpatch(Original, HarmonyPatchType.All, harmony.Id);
		}
	}
}
