using System.Reflection;
using Verse;

namespace NeedBarOverflow.Patches
{
    // Disable option to consume food if pawn is too full on food
    using static Common;
    public static class WillIngestFromInventoryNow
    {
        public static readonly MethodInfo Postfix = ((ActionRef_r3<Pawn, Thing, bool>)PostfixMethod).Method;
        // If pawn cannot consume more food, and the item is nutrition-giving (food), pawn will not ingest
        // Otherwise __result will be unchanged
        private static void PostfixMethod(Pawn pawn, Thing inv, ref bool __result)
            => __result &= Need_Food_Helper.CanConsumeMoreFood(pawn)
                || !inv.def.IsNutritionGivingIngestible;
    }
}
