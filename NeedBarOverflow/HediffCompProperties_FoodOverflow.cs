namespace NeedBarOverflow
{
    using Verse;
    using D = Debug;

    public class HediffCompProperties_FoodOverflow : HediffCompProperties
    {
        public HediffCompProperties_FoodOverflow()
        {
            D.Message("HediffCompProperties_FoodOverflow constructor called");
            compClass = typeof(HediffComp_FoodOverflow);
        }
    }
}
