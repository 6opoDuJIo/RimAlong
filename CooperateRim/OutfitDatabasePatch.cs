﻿using Harmony;
using RimWorld;
using Verse;


namespace CooperateRim
{
    [HarmonyPatch(typeof(OutfitDatabase), "GenerateStartingOutfits")]
    public class OutfitDatabasePatch
    {
        public static void FixStartingOutfits(OutfitDatabase __instance)
        {
            Outfit outfit = __instance.MakeNewOutfit();
            outfit.label = "OutfitAnything".Translate();
            Outfit outfit2 = __instance.MakeNewOutfit();
            outfit2.label = "OutfitWorker".Translate();
            ThingFilterPatch.thingFilterCallerStack.Push(outfit2);
            outfit2.filter.SetDisallowAll(null, null);
            outfit2.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.apparel != null && thingDef.apparel.defaultOutfitTags != null && thingDef.apparel.defaultOutfitTags.Contains("Worker"))
                {
                    outfit2.filter.SetAllow(thingDef, true);
                }
            }
            ThingFilterPatch.thingFilterCallerStack.Pop();
            Outfit outfit3 = __instance.MakeNewOutfit();
            outfit3.label = "OutfitSoldier".Translate();
            ThingFilterPatch.thingFilterCallerStack.Push(outfit3);
            outfit3.filter.SetDisallowAll(null, null);
            outfit3.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (ThingDef thingDef2 in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef2.apparel != null && thingDef2.apparel.defaultOutfitTags != null && thingDef2.apparel.defaultOutfitTags.Contains("Soldier"))
                {
                    outfit3.filter.SetAllow(thingDef2, true);
                }
            }
            ThingFilterPatch.thingFilterCallerStack.Pop();
            Outfit outfit4 = __instance.MakeNewOutfit();
            outfit4.label = "OutfitNudist".Translate();
            ThingFilterPatch.thingFilterCallerStack.Push(outfit4);
            outfit4.filter.SetDisallowAll(null, null);
            outfit4.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, false);
            foreach (ThingDef thingDef3 in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef3.apparel != null && !thingDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !thingDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                {
                    outfit4.filter.SetAllow(thingDef3, true);
                }
            }
            ThingFilterPatch.thingFilterCallerStack.Pop();
        }

        [HarmonyPrefix]
        public static bool Prefix(OutfitDatabase __instance)
        {
            FixStartingOutfits(__instance);
            return false;
        }
    }

    /*
    [HarmonyPatch(typeof(ThingFilter), "SetAllowAll", new System.Type[] { typeof(ThingFilter) })]
    public class ThingFilter_setallowall_wrapper
    {
        internal static bool avoid_internal_loop = false;

        public static void ThingFilter_setallowall_zone(int thingIDNumber, bool actuallyDisallow)
        {
            avoid_internal_loop = true;
            try
            {
                IStoreSettingsParent storeSettings = null;
                Zone z = Find.CurrentMap.zoneManager.AllZones.First(u => u.ID == thingIDNumber);

                if (z as IStoreSettingsParent != null)
                {
                    storeSettings = z as IStoreSettingsParent;
                }

                if (storeSettings != null)
                {
                    if (!actuallyDisallow)
                    {
                        storeSettings.GetStoreSettings().filter.SetAllowAll(null);
                    }
                    else
                    {
                        storeSettings.GetStoreSettings().filter.SetDisallowAll();
                    }
                }
            }
            finally
            {
                avoid_internal_loop = false;
            }
        }

        public static void ThingFilter_setallowall_thing(int thingIDNumber, bool actuallyDisallow)
        {

        }

        public static void ThingFilter_setallowall_bill(int thingIDNumber, int billNumber, bool actuallyDisallow)
        {

        }


        [HarmonyPrefix]
        public static bool Prefix(ThingFilter __instance)
        {
            if (!avoid_internal_loop)
            {
                if (Find.CurrentMap != null)
                {
                    if (ThingFilterPatch.thingFilterCallerStack.Count > 0)
                    {
                        object o = ThingFilterPatch.thingFilterCallerStack.Peek();
                        Utilities.RimLog.Message("SetAllow :::::::::: " + o);

                        if (o is Zone)
                        {
                            ThingFilter_setallowall_zone((o as Zone).ID, false);
                        }

                        if (o is Thing)
                        {
                            ThingFilter_setallowall_thing((o as Thing).thingIDNumber, false);
                        }

                        if (o is Bill)
                        {
                            Bill b = o as Bill;
                            BillStack bs = b.billStack;
                            Thing t = bs.billGiver as Thing;
                            int index = bs.Bills.IndexOf(b);
                            ThingFilter_setallowall_bill(t.thingIDNumber, index, false);
                            Utilities.RimLog.Message("this actions is yet invalid for bills! giver is " + t);
                        }

                    }
                    //SyncTickData.AppendSyncTickDataDeltaFilter(thingDef, Find.Selector.SingleSelectedThing, Find.Selector.SelectedZone, allow);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ThingFilter), "SetDisallowAll")]
    public class ThingFilter_setdisallowall_wrapper
    {
        [HarmonyPrefix]
        public static bool Prefix(ThingFilter __instance)
        {
            if (!ThingFilter_setallowall_wrapper.avoid_internal_loop)
            {
                if (Find.CurrentMap != null)
                {
                    if (ThingFilterPatch.thingFilterCallerStack.Count > 0)
                    {
                        object o = ThingFilterPatch.thingFilterCallerStack.Peek();
                        Utilities.RimLog.Message("SetAllow :::::::::: " + o);

                        if (o is Zone)
                        {
                            ThingFilter_setallowall_wrapper.ThingFilter_setallowall_zone((o as Zone).ID, true);
                        }

                        if (o is Thing)
                        {
                            ThingFilter_setallowall_wrapper.ThingFilter_setallowall_thing((o as Thing).thingIDNumber, true);
                        }

                        if (o is Bill)
                        {
                            Bill b = o as Bill;
                            BillStack bs = b.billStack;
                            Thing t = bs.billGiver as Thing;
                            int index = bs.Bills.IndexOf(b);
                            ThingFilter_setallowall_wrapper.ThingFilter_setallowall_bill(t.thingIDNumber, index, true);
                            Utilities.RimLog.Message("this actions is yet invalid for bills! giver is " + t);
                        }
                    }
                    //SyncTickData.AppendSyncTickDataDeltaFilter(thingDef, Find.Selector.SingleSelectedThing, Find.Selector.SelectedZone, allow);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }


    [HarmonyPatch(typeof(ThingFilter), "SetAllow", new System.Type[] { typeof(ThingDef), typeof(bool) })]
    public class ThingFilter_wrapper
    {
        internal static bool avoid_internal_loop = false;

        public static void ThingFilter_setallow_bill_with_billgiver(string thingDefName, bool allow, int thingIDNumber, bool isSpecial, int billIndex)
        {
            avoid_internal_loop = true;
            try
            {
                IBillGiver billgiver = null;
                List<Thing>[] things = (List<Thing>[])Find.CurrentMap.thingGrid.GetType().GetField("thingGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Find.CurrentMap.thingGrid);
                Utilities.RimLog.Message("ThingFilter_setallow_bill_with_billgiver");
                {
                    Thing issuer = things.Where(u => u.Count != 0).First(u => u.Any(uu => uu.thingIDNumber == thingIDNumber)).First(u => u.thingIDNumber == thingIDNumber);
                    Utilities.RimLog.Message(">>>>>>>>>>> issuer :  " + issuer + " :: " + (issuer as IBillGiver));
                    billgiver = issuer as IBillGiver;
                }

                if (billgiver != null)
                {
                    if (!isSpecial)
                    {
                        Utilities.RimLog.Message(">>>>>>>>>>> billgiver.BillStack.Bills[billIndex].ingredientFilter.SetAllow " + billIndex + " |" + thingDefName + "| " + billgiver.BillStack.Bills[billIndex]);
                        billgiver.BillStack.Bills[billIndex].ingredientFilter.SetAllow(DefDatabase<ThingDef>.GetNamed(thingDefName, true), allow);
                    }
                    else
                    {
                        Utilities.RimLog.Message(">>>>>>>>>>> billgiver.BillStack.Bills[billIndex].ingredientFilter.SetAllow " + billIndex + " |" + thingDefName + "| " + billgiver.BillStack.Bills[billIndex]);
                        billgiver.BillStack.Bills[billIndex].ingredientFilter.SetAllow(DefDatabase<SpecialThingFilterDef>.GetNamed(thingDefName, true), allow);
                    }
                }
                else
                {
                    Utilities.RimLog.Message("missing bill giver!");
                }
            }
            finally
            {
                avoid_internal_loop = false;
            }
        }

        public static void Thingfilter_setallow_wrap(string thingDefName, bool allow, int thingIDNumber, bool isSpecial)
        {
            avoid_internal_loop = true;
            try
            {
                IStoreSettingsParent storeSettings = null;
                List<Thing>[] things = (List<Thing>[])Find.CurrentMap.thingGrid.GetType().GetField("thingGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Find.CurrentMap.thingGrid);

                {
                    Thing issuer = things.Where(u => u.Count != 0).First(u => u.Any(uu => uu.thingIDNumber == thingIDNumber)).First(u => u.thingIDNumber == thingIDNumber);
                    storeSettings = issuer as IStoreSettingsParent;
                }

                if (storeSettings != null)
                {
                    if (!isSpecial)
                    {
                        storeSettings.GetStoreSettings().filter.SetAllow(DefDatabase<ThingDef>.GetNamed(thingDefName, true), allow);
                    }
                    else
                    {
                        storeSettings.GetStoreSettings().filter.SetAllow(DefDatabase<SpecialThingFilterDef>.GetNamed(thingDefName, true), allow);
                    }
                }
            }
            finally
            {
                avoid_internal_loop = false;
            }
        }

        public static void Thingfilter_setallowzone_wrap(string thingDefName, bool allow, int zoneID, bool isSpecial)
        {
            avoid_internal_loop = true;
            try
            {
                IStoreSettingsParent storeSettings = null;
                Zone z = Find.CurrentMap.zoneManager.AllZones.First(u => u.ID == zoneID);

                if (z as IStoreSettingsParent != null)
                {
                    storeSettings = z as IStoreSettingsParent;
                }

                if (storeSettings != null)
                {
                    if (!isSpecial)
                    {
                        storeSettings.GetStoreSettings().filter.SetAllow(DefDatabase<ThingDef>.GetNamed(thingDefName, true), allow);
                    }
                    else
                    {
                        Utilities.RimLog.Message(">>>>>>>>>> SPECIAL : " + thingDefName + " :: " + DefDatabase<SpecialThingFilterDef>.GetNamed(thingDefName, true) + " :: ");
                        storeSettings.GetStoreSettings().filter.SetAllow(DefDatabase<SpecialThingFilterDef>.GetNamed(thingDefName, true), allow);
                    }
                }
            }
            finally
            {
                avoid_internal_loop = false;
            }
        }

        [HarmonyPrefix]
        public static bool SetAllow(ThingDef thingDef, ref bool allow, ThingFilter __instance)
        {
            if (!avoid_internal_loop)
            {
                if (Find.CurrentMap != null)
                {
                    if (ThingFilterPatch.thingFilterCallerStack.Count > 0)
                    {
                        object o = ThingFilterPatch.thingFilterCallerStack.Peek();
                        Utilities.RimLog.Message("SetAllow :::::::::: " + o);

                        if (o is Zone)
                        {
                            Thingfilter_setallowzone_wrap(thingDef.defName, allow, (o as Zone).ID, false);
                        }

                        if (o is Thing)
                        {
                            Thingfilter_setallow_wrap(thingDef.defName, allow, (o as Thing).thingIDNumber, false);
                        }

                        if (o is Bill)
                        {
                            Bill b = o as Bill;
                            BillStack bs = b.billStack;
                            Thing t = bs.billGiver as Thing;
                            int index = bs.Bills.IndexOf(b);
                            ThingFilter_setallow_bill_with_billgiver(thingDef.defName, allow, t.thingIDNumber, false, index);
                            Utilities.RimLog.Message("this actions is yet invalid for bills! giver is " + t);
                        }

                        if (o is FoodRestriction)
                        {
                            FoodRestriction fs = o as FoodRestriction;

                        }
                    }
                    //SyncTickData.AppendSyncTickDataDeltaFilter(thingDef, Find.Selector.SingleSelectedThing, Find.Selector.SelectedZone, allow);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ThingFilter), "SetAllow", new System.Type[] { typeof(SpecialThingFilterDef), typeof(bool) })]
    public class ThingFilter_wrapper_special
    {
        [HarmonyPrefix]
        public static bool SetAllow(SpecialThingFilterDef sfDef, ref bool allow, ThingFilter __instance)
        {
            if (!ThingFilter_wrapper.avoid_internal_loop)
            {
                if (Find.CurrentMap != null)
                {
                    if (ThingFilterPatch.thingFilterCallerStack.Count > 0)
                    {
                        object o = ThingFilterPatch.thingFilterCallerStack.Peek();
                        Utilities.RimLog.Message("SetAllow :::::::::: " + o);

                        if (o is Zone)
                        {
                            ThingFilter_wrapper.Thingfilter_setallowzone_wrap(sfDef.defName, allow, (o as Zone).ID, true);
                        }

                        if (o is Thing)
                        {
                            ThingFilter_wrapper.Thingfilter_setallow_wrap(sfDef.defName, allow, (o as Thing).thingIDNumber, true);
                        }

                        if (o is Bill)
                        {
                            Bill b = o as Bill;
                            BillStack bs = b.billStack;
                            Thing t = bs.billGiver as Thing;
                            int index = bs.Bills.IndexOf(b);
                            ThingFilter_wrapper.ThingFilter_setallow_bill_with_billgiver(sfDef.defName, allow, t.thingIDNumber, true, index);
                            Utilities.RimLog.Message("this actions is yet invalid for bills! giver is " + t);
                        }
                    }
                    //SyncTickData.AppendSyncTickDataDeltaFilter(thingDef, Find.Selector.SingleSelectedThing, Find.Selector.SelectedZone, allow);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }*/

    /*
    [HarmonyPatch(typeof(ThingFilter), "SetAllow", new System.Type[] { typeof(SpecialThingFilterDef), typeof(bool) })]
    public class ThingFilter_special_patch
    {
        [HarmonyPrefix]
        public static bool SetAllow(ref SpecialThingFilterDef sfDef, ref bool allow)
        {
            if (!SyncTickData.AvoidLoop)
            {
                if (Find.CurrentMap != null && (Find.Selector.SelectedZone != null || Find.Selector.SingleSelectedThing != null))
                {
                    SyncTickData.AppendSyncTickDataDeltaFilterSpecial(sfDef, Find.Selector.SingleSelectedThing, Find.Selector.SelectedZone, allow);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }*/
}
