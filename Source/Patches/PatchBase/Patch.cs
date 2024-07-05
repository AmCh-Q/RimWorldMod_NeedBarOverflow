using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace NeedBarOverflow.Patches
{
	public readonly struct Patches(
		Delegate? prefix,
		Delegate? postfix,
		Delegate? transpiler,
		Delegate? finalizer)
		: IEquatable<Patches>
	{
		public HarmonyMethod? Prefix { get; } = ToHarmonyMethod(prefix);
		public HarmonyMethod? Postfix { get; } = ToHarmonyMethod(postfix);
		public HarmonyMethod? Transpiler { get; } = ToHarmonyMethod(transpiler);
		public HarmonyMethod? Finalizer { get; } = ToHarmonyMethod(finalizer);
		public static bool operator !=(Patches lhs, Patches rhs)
			=> !lhs.Equals(rhs);
		public static bool operator ==(Patches lhs, Patches rhs)
			=> lhs.Equals(rhs);
		public override bool Equals(object? obj)
			=> obj is Patches other && Equals(other);
		public bool Equals(Patches other)
		{
			return Prefix is not null && Prefix.method == other.Prefix?.method
				|| Postfix is not null && Postfix.method == other.Postfix?.method
				|| Transpiler is not null && Transpiler.method == other.Transpiler?.method
				|| Finalizer is not null && Finalizer.method == other.Finalizer?.method;
		}
		public override int GetHashCode()
		{
			return Prefix?.method.GetHashCode()
				?? Postfix?.method.GetHashCode()
				?? Transpiler?.method.GetHashCode()
				?? Finalizer?.method.GetHashCode()
				?? 0;
		}
		public static HarmonyMethod? ToHarmonyMethod(Delegate? deleg)
			=> deleg is null ? null : new HarmonyMethod(deleg.Method);
	}
	public abstract class Patch : IEquatable<Patch>
	{
		public static readonly Harmony harmony = new(id: "AmCh.NeedBarOverflow");
		public abstract Patches Patches { get; }
		public abstract bool Patchable { get; }
		public abstract bool Patched { get; }
		public HarmonyMethod? Prefix => Patches.Prefix;
		public HarmonyMethod? Postfix => Patches.Postfix;
		public HarmonyMethod? Transpiler => Patches.Transpiler;
		public HarmonyMethod? Finalizer => Patches.Finalizer;
		public bool Patched_Method(MethodBase? method)
		{
			if (method is null)
				return false;
			if (PatchProcessor.GetPatchInfo(method) is not HarmonyLib.Patches patches)
				return false;
			return (Prefix is not null
					&& patches.Prefixes.Any(p => p.PatchMethod == Prefix.method))
				|| (Postfix is not null
					&& patches.Postfixes.Any(p => p.PatchMethod == Postfix.method))
				|| (Transpiler is not null
					&& patches.Transpilers.Any(p => p.PatchMethod == Transpiler.method))
				|| (Finalizer is not null
					&& patches.Finalizers.Any(p => p.PatchMethod == Finalizer.method));
		}
		public static bool operator !=(Patch lhs, Patch rhs)
			=> !lhs.Equals(rhs);
		public static bool operator ==(Patch lhs, Patch rhs)
			=> lhs.Equals(rhs);
		public override bool Equals(object? obj)
			=> obj is Patch other && Equals(other);
		public abstract bool Equals(Patch other);
		public override int GetHashCode()
			=> Patches.GetHashCode();
		public abstract void Toggle();
		public abstract void Toggle(bool enable);
		protected abstract void Dopatch();
		protected abstract void Unpatch();
	}
	public abstract class Patch<T> : Patch
	{
		public override Patches Patches { get; }
		public T Original { get; }
		protected Patch(
			T original,
			Patches patches)
		{
			Original = original;
			Patches = patches;
		}
		protected Patch(
			T original,
			Delegate? prefix = null,
			Delegate? postfix = null,
			Delegate? transpiler = null,
			Delegate? finalizer = null)
		{
			Original = original;
			Patches = new Patches(prefix, postfix, transpiler, finalizer);
		}
	}
}
