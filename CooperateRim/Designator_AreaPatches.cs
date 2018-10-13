﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CooperateRim
{
    [HarmonyPatch(typeof(Designator_Build))]
    [HarmonyPatch("DesignateSingleCell")]
    public class Designator_build
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Designator __instance, ref IntVec3 c, ref BuildableDef ___entDef, ref Rot4 ___placingRot, ref ThingDef ___stuffDef)
        {
            CooperateRimming.Log("Designator_Build designate single cell " + (___stuffDef == null ? "null" : ___stuffDef.ToString()) + " || " + (___stuffDef == null ? "null" : ___stuffDef.defName));
            ThingDef td = ___stuffDef;

            if (!SyncTickData.AvoidLoop)
            {
                //DefDatabase<ThingDef>.AllDefs
                SyncTickData.AppendSyncTickDataDesignatorSingleCell(__instance, c, ___entDef, ___placingRot, ___stuffDef);
                return false;
            }
            else
            {
                return true;
            }
            /*
            CooperateRimming.Log("DesignateMultiCell_1");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickData(__instance, cells);
                return false;
            }
            else
            {
                return true;
            }*/
        }
    }


    [HarmonyPatch(typeof(Designator_ZoneDelete))]
    [HarmonyPatch("DesignateSingleCell")]
    class Designator_patch__
    {
        [HarmonyPrefix]
        public static bool DesignateSingleCell_1(ref Designator __instance, ref IntVec3 c)
        {
            CooperateRimming.Log("DELETE : " + __instance.GetType().AssemblyQualifiedName + ".DesignateSingleCell");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickData(__instance, c);
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    //[HarmonyPatch(typeof(Designator_AreaBuildRoof))]
    //[HarmonyPatch("DesignateSingleCell")]
    class Designator_patch
    {
        //[HarmonyPrefix]
        public static bool DesignateSingleCell_1(ref Designator __instance, ref IntVec3 c)
        {
            CooperateRimming.Log(__instance.GetType().AssemblyQualifiedName + ".DesignateSingleCell");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickData(__instance, c);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool DesignateSingleCell_2(ref Designator __instance, ref IntVec3 loc)
        {
            CooperateRimming.Log(__instance.GetType().AssemblyQualifiedName + ".DesignateSingleCell");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickData(__instance, loc);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool DesignateMultiCell(ref Designator __instance, IEnumerable<IntVec3> cells)
        {
            if (__instance is Designator_Build)
            {
                return true;
            }
            CooperateRimming.Log("DesignateMultiCell_1");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickData(__instance, cells);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool FinalizeDesignationSucceeded(ref Designator __instance)
        {
            CooperateRimming.Log("FinalizeDesignationSucceeded");
            return true;
            /*
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickDataFinalizeDesignationSucceeded(__instance);
                return false;
            }
            else
            {
                return true;
            }*/
        }

        public static bool DesignateThing(Designator __instance, Thing t)
        {
            CooperateRimming.Log(__instance.GetType().AssemblyQualifiedName + ".DesignateThing");
            if (!SyncTickData.AvoidLoop)
            {
                SyncTickData.AppendSyncTickDesignatorApplyToThing(__instance, t, t.Position);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
