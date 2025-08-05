using RimWorld;
using System.Reflection;

namespace NeedBarOverflow.Needs;

public class Setting<T> where T : Need
{
	public static bool Enabled => Setting_Common.GetOverflow(typeof(T)) > 0f;
	public static float MaxValue => Setting_Common.GetOverflow(typeof(T));

	public static readonly MethodInfo
		mget_MaxValue = typeof(Setting<T>).Getter(nameof(MaxValue));
}
