using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NeedBarOverflow.DisableNeedOverflow
{
	public static partial class ChecksByType
	{
		public enum MemberShipType
		{
			AllowColonists,
			AllowColonySlaves,
			AllowColonyGuests,
			AllowColonyPrisoners,
			AllowOtherHumans,

			AllowPlayerAnimals,
			AllowWildAnimals,
			AllowOtherAnimals,

			AllowPlayerMutants,
			AllowOtherMutants,

			AllowPlayerOthers,
			AllowOthers,
		}

		public static readonly bool[] dfltMembershipSetting =
			[
			true, true, true, true, false,
			true, false, false,
			true, false,
			true, false,
			];
		public static readonly bool[] membershipSetting = (bool[])dfltMembershipSetting.Clone();

		public static bool Membership(Pawn pawn)
		{
#if g1_5
			if (pawn.IsMutant)
			{
				if (pawn.Faction?.IsPlayer ?? false)
					return membershipSetting[(int)MemberShipType.AllowPlayerMutants];
				else
					return membershipSetting[(int)MemberShipType.AllowOtherMutants];

			}
#endif
			if (pawn.RaceProps?.Humanlike ?? false)
			{
				if (pawn.Faction?.IsPlayer ?? false)
				{
#if l1_2
					return membershipSetting[(int)MemberShipType.AllowColonists];
#else
					if (pawn.IsSlave)
						return !membershipSetting[(int)MemberShipType.AllowColonySlaves];
					else
						return !membershipSetting[(int)MemberShipType.AllowColonists];
#endif
				}
				if (pawn.guest?.HostFaction == Faction.OfPlayer)
				{
					if (pawn.guest.IsPrisoner)
						return membershipSetting[(int)MemberShipType.AllowColonyPrisoners];
					else
						return membershipSetting[(int)MemberShipType.AllowColonyGuests];
				}
				return membershipSetting[(int)MemberShipType.AllowOtherHumans];
			}
			if (pawn.RaceProps?.Animal ?? false)
			{
				if (pawn.Faction is null)
					return membershipSetting[(int)MemberShipType.AllowWildAnimals];
				else if (pawn.Faction.IsPlayer)
					return membershipSetting[(int)MemberShipType.AllowPlayerAnimals];
				else
					return membershipSetting[(int)MemberShipType.AllowOtherAnimals];
			}
			if (pawn.Faction?.IsPlayer ?? false)
				return membershipSetting[(int)MemberShipType.AllowPlayerOthers];
			return membershipSetting[(int)MemberShipType.AllowOthers];
		}

		public static void Membership_AddSettings(Listing_Standard ls)
		{
			Array Enums = Enum.GetValues(typeof(MemberShipType));
			foreach (MemberShipType key in Enums)
			{
				SettingLabel sl = new(Strings.AllowOverf, key.ToString());
				bool setting = membershipSetting[(int)key];
				ls.CheckboxLabeled(sl.TranslatedLabel(), ref setting);
				membershipSetting[(int)key] = setting;
			}
		}

		public static void Membership_ExposeData()
		{
			Array Enums = Enum.GetValues(typeof(MemberShipType));
			// Needs to be a Dictionary with Enum as key here
			// (instead of an array)
			// so that Scribe_Collections can save the Enum by name
			Dictionary<MemberShipType, bool> membership_dict = [];
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (MemberShipType key in Enums)
					membership_dict[key] = membershipSetting[(int)key];
			}
			Scribe_Collections.Look(ref membership_dict,
				Strings.membership, LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Array.Copy(dfltMembershipSetting, membershipSetting, dfltMembershipSetting.Length);
				foreach (MemberShipType key in Enums)
				{
					if (membership_dict is not null &&
						membership_dict.TryGetValue(key, out bool setting))
						membershipSetting[(int)key] = setting;
				}
			}
		}
	}
}
