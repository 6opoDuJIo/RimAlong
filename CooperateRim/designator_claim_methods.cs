﻿using Verse;
using RimWorld;
using System.Runtime.Serialization;
using System.Linq;

public class designator_claim_methods : common_patch_fields
{
    public static void parrot_designate_thing(Thing t)
    {
        use_native = true;
        try
        {
            DesignatorUtility.FindAllowedDesignator<Designator_Claim>().DesignateThing(t);
        }
        finally
        {
            use_native = false;
        }
    }

    public static bool prefix_designate_thing(Thing t)
    {
        if (use_native)
        {
            return true;
        }
        else
        {
            parrot_designate_thing(t);
            return false;
        }
    }
}
