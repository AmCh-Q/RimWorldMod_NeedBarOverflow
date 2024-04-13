using System.Reflection;
using RimWorld;

namespace NeedBarOverflow.Needs
{
	public class Setting<T> where T : Need
	{
		public static bool Enabled => Setting_Common.overflow[typeof(T)] > 0f;
		public static float MaxValue => Setting_Common.overflow[typeof(T)];
		public static readonly MethodInfo
			MaxValue_get = typeof(Setting<T>).Getter(nameof(MaxValue));
	}
}
