using System.Reflection;
using RimWorld;

namespace NeedBarOverflow.Needs
{
	public class Setting<T> where T : Need
	{
		public static bool Enabled => Setting_Common.Enabled(typeof(T));
        public static readonly MethodInfo
            MaxValue_get = typeof(Setting<T>).GetProperty(nameof(MaxValue)).GetGetMethod();
        public static float MaxValue => Setting_Common.Overflow(typeof(T));
        protected Setting() { }
    }
}
