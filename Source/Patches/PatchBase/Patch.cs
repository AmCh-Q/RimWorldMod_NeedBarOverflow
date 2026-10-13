using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.Patches;

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

public abstract class Patch(
	Delegate? prefix = null,
	Delegate? postfix = null,
	Delegate? transpiler = null,
	Delegate? finalizer = null) : IEquatable<Patch>
{
	public static readonly Harmony harmony = new(id: "AmCh.NeedBarOverflow");
	public Patches Patches { get; } = new(prefix, postfix, transpiler, finalizer);
	public MethodBase[] Target { get; protected set; } = default!;
	public HarmonyMethod? Prefix => Patches.Prefix;
	public HarmonyMethod? Postfix => Patches.Postfix;
	public HarmonyMethod? Transpiler => Patches.Transpiler;
	public HarmonyMethod? Finalizer => Patches.Finalizer;
	public static bool operator !=(Patch lhs, Patch rhs)
		=> !lhs.Equals(rhs);
	public static bool operator ==(Patch lhs, Patch rhs)
		=> lhs.Equals(rhs);
	public override bool Equals(object? obj)
		=> obj is Patch other && Equals(other);
	public virtual bool Equals(Patch? other)
	{
		if (other is null)
			return false;
		if (Patches != other.Patches)
			return false;
		if (Target is null || other.Target is null)
			return false;
		return Target.Intersect(other.Target).Any();
	}
	public override int GetHashCode()
	{
		if (Target.Length == 0)
			return 0;
		return Target[0]?.GetHashCode() ?? 0;
	}
	public virtual bool Patched
		=> Target.Any(HasPatchedMethod);
	public bool HasPatchedMethod(MethodBase? method)
	{
		if (method is null)
			return false;
		if (PatchProcessor.GetPatchInfo(method) is not HarmonyLib.Patches patches)
			return false;
		return (Prefix is not null && patches.Prefixes.Any(p => p.PatchMethod == Prefix.method))
			|| (Postfix is not null && patches.Postfixes.Any(p => p.PatchMethod == Postfix.method))
			|| (Transpiler is not null && patches.Transpilers.Any(p => p.PatchMethod == Transpiler.method))
			|| (Finalizer is not null && patches.Finalizers.Any(p => p.PatchMethod == Finalizer.method));
	}
	protected abstract void Initialize();
	public abstract void Toggle();
	public virtual void Toggle(bool enable)
	{
		if (enable == Patched)
			return;
		if (enable)
			Dopatch();
		else
			Unpatch();
	}
	protected virtual void Dopatch()
	{
		foreach (MethodBase original in Target)
		{
			Debug.Message("Patching Method " + original.FullDescription());
			harmony.Patch(original,
				prefix: Prefix,
				postfix: Postfix,
				transpiler: Transpiler,
				finalizer: Finalizer);
		}
	}
	protected virtual void Unpatch()
	{
		foreach (MethodBase original in Target)
		{
			Debug.Message("Unpatching Method " + original.FullDescription());
			harmony.Unpatch(original,
				HarmonyPatchType.All,
				harmony.Id);
		}
	}
	public static Patch? TryCreatePatch(Type patchClass)
	{
		Patch? patch = (Patch?)Activator.CreateInstance(patchClass);
		if (patch is null)
			return null;
		patch.Initialize();

		MethodBase[]? result = patch.Target;
		if (result is null)
			return null;
		int count = 0;
		foreach (MethodBase? method in result)
		{
			if (method is not null)
				count++;
		}
		if (count == 0)
			return null;
		if (count == result.Length)
			return patch;

		patch.Target = new MethodBase[count];
		int i = 0;
		foreach (MethodBase? method in result)
		{
			if (method is not null)
				patch.Target[i++] = method;
		}
		return patch;
	}
}
