using HarmonyLib;
using System;
using System.Reflection;
using Verse;
using static NeedBarOverflow.Patches.Utility;

namespace NeedBarOverflow.Patches
{
	public readonly struct Patches(
		Delegate? prefix,
		Delegate? postfix,
		Delegate? transpiler,
		Delegate? finalizer)
		: IEquatable<Patches>
	{
		public Delegate? Prefix { get; } = prefix;
		public Delegate? Postfix { get; } = postfix;
		public Delegate? Transpiler { get; } = transpiler;
		public Delegate? Finalizer { get; } = finalizer;
		public override readonly int GetHashCode()
		{
			return Gen.HashCombineInt(
				Prefix?.Method?.GetHashCode() ?? 0,
				Postfix?.Method?.GetHashCode() ?? 0,
				Transpiler?.Method?.GetHashCode() ?? 0,
				Finalizer?.Method?.GetHashCode() ?? 0);
		}
		public override readonly bool Equals(object obj)
			=> obj is Patches other && Equals(other);
		public readonly bool Equals(Patches other)
		{
			return Prefix?.Method == other.Prefix?.Method
				&& Postfix?.Method == other.Postfix?.Method
				&& Transpiler?.Method == other.Transpiler?.Method
				&& Finalizer?.Method == other.Finalizer?.Method;
		}
		public static bool operator ==(Patches lhs, Patches rhs)
			=> lhs.Equals(rhs);
		public static bool operator !=(Patches lhs, Patches rhs)
			=> !lhs.Equals(rhs);
	}
	public abstract class Patch_Single(
		MethodInfo? original,
		Delegate? prefix = null,
		Delegate? postfix = null,
		Delegate? transpiler = null,
		Delegate? finalizer = null)
		: IEquatable<Patch_Single>
	{
		public HarmonyPatchType? Patched { get; protected set; }
		public MethodInfo? Original { get; } = original;
		public Patches Patches { get; } = new Patches(prefix, postfix, transpiler, finalizer);
		public Delegate? Prefix => Patches.Prefix;
		public Delegate? Postfix => Patches.Postfix;
		public Delegate? Transpiler => Patches.Transpiler;
		public Delegate? Finalizer => Patches.Finalizer;
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
			Original.NotNull<MethodInfo>("Patch Original " + GetType().Name);
			if (Patched is not null)
				return;
			Debug.Message("Patching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			HarmonyPatchType lastPatch = HarmonyPatchType.All;
			int numPatches = 0;
			HarmonyMethod? h_prefix = null;
			if (Prefix is not null)
			{
				h_prefix = new(Prefix.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Prefix;
			}
			HarmonyMethod? h_postfix = null;
			if (Postfix is not null)
			{
				h_postfix = new(Postfix.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Postfix;
			}
			HarmonyMethod? h_transpiler = null;
			if (Transpiler is not null)
			{
				h_transpiler = new(Transpiler.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Transpiler;
			}
			HarmonyMethod? h_finalizer = null;
			if (Finalizer is not null)
			{
				h_finalizer = new(Finalizer.Method);
				numPatches++;
				lastPatch = HarmonyPatchType.Finalizer;
			}
			harmony.Patch(Original,
				prefix: h_prefix,
				postfix: h_postfix,
				transpiler: h_transpiler,
				finalizer: h_finalizer);
			if (numPatches == 0)
				throw new ArgumentException("No patch loaded for method " + Original.Name);
			else if (numPatches == 1)
				Patched = lastPatch;
			else
				Patched = HarmonyPatchType.All;
		}
		private void Unpatch()
		{
			Original.NotNull<MethodInfo>("Patch Original " + GetType().Name);
			if (Patched is null)
				return;
			Debug.Message("Unpatching Method "
				+ Original!.DeclaringType.Name
				+ ":" + Original.Name);
			harmony.Unpatch(Original, (HarmonyPatchType)Patched, harmony.Id);
			Patched = null;
		}
	}
}
