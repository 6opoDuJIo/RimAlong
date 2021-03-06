﻿using CooperateRim.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Verse;

namespace CooperateRim
{
    [HarmonyPatch(typeof(Verse.TickManager))]
    [HarmonyPatch("TickManagerUpdate")]
    public class TickManagerPatch
    {
        public static bool isNetworkLaunch = false;
        public static bool shouldReallyTick = false;
        public static DateTime nextFrameTime;
        // public static int myTicksValue;

        public static int nextProcessionTick = 0;
        public static int nextCommunicationTick = 0;
        public static int clientsInSync = 0;
        public static bool imInSync;
        public static int syncRoundLength = 60;
        public static int syncTickRoundOffset = 4;
        public static bool IsSyncTick;
        static Stopwatch sw;
        static Stopwatch ACKSW;

        public static void SetSyncTickValue(int val)
        {
            //RimLog.Message("new sync tick value :" + val);
            nextProcessionTick = val;
        }

        public static SyncTickData[] cachedData;
        public static void SetCachedData(SyncTickData[] __cd)
        {
            cachedData = __cd;
        }

        public static int expectedSRoundRealtimeLenMiliseconds = 200;
        public static int ticksPerSecond = 250;
        static float lastLogTime = -9000;
        
        [HarmonyPrefix]
        public static bool Prefix(ref int ___ticksGameInt, ref TickManager __instance)
        {
            Rand.PushState(___ticksGameInt);
            getValuePatch.SendDiagDataToServer();
            getValuePatch.diagData.Clear();
            CooperateRimming.dumpRand = true;

            if (sw == null)
            {
                sw = new Stopwatch();
                ACKSW = new Stopwatch();
                sw.Start();
                ACKSW.Start();

                /*fix for newly loaded game*/
                TickManagerPatch.nextCommunicationTick = Verse.Find.TickManager.TicksGame;
                TickManagerPatch.nextProcessionTick = Verse.Find.TickManager.TicksGame;

                if (!imInSync)
                {

                    if (nextCommunicationTick == Verse.Find.TickManager.TicksGame)
                    {
                        
                        imInSync = SyncTickData.FlushSyncTickData(nextProcessionTick + syncRoundLength * syncTickRoundOffset);
                    }


                }

                if (cachedData == null)
                {
                    return false;
                }
                //for (; cachedData == null;)
                {

                }
            }
            shouldReallyTick = false;
            
            if (NetDemo.HasAllDataForFrame(Verse.Find.TickManager.TicksGame))
            {
                NetDemo.Receive();
            }

            if (sw.ElapsedMilliseconds > (1000 / ticksPerSecond) && !__instance.Paused)
            {
                sw.Reset();
                sw.Start();
                bool canNormallyTick = nextProcessionTick > Verse.Find.TickManager.TicksGame;

                //RimLog.Message("Tick " + ___ticksGameInt + " canNormallyTick " + canNormallyTick + "| comm " + nextCommunicationTick + " | " + nextProcessionTick);

                if (canNormallyTick)
                {

                    //Utilities.RimLog.Message("normal tick at " + Verse.Find.TickManager.TicksGame + " nsync " + nextSyncTickValue);
                    ulong i = 0;
                    foreach (var act in RandContextCounter.replacement_seed)
                    {
                        act((ulong)___ticksGameInt + (i++));
                    }
                    __instance.DoSingleTick();
                }

                if (SyncTickData.cliendID > -1 && ACKSW.ElapsedMilliseconds > 50)
                {
                    ACKSW.Reset();
                    ACKSW.Start();
                    //RimLog.Message("Sending state request for " + nextSyncTickValue);
                    //NetDemo.SendStateRequest(nextSyncTickValue, SyncTickData.cliendID);
                }


                if (!imInSync)
                {
                    if (nextCommunicationTick == Verse.Find.TickManager.TicksGame)
                    {
                        imInSync = SyncTickData.FlushSyncTickData(nextProcessionTick + syncRoundLength * syncTickRoundOffset);
                    }
                }

                {
                    if (nextProcessionTick == Verse.Find.TickManager.TicksGame)
                    {
                       
#if FILE_TRANSFER
                    bool allSyncDataAvailable = SyncTickData.tickFileNames(___ticksGameInt).All(u => System.IO.File.Exists(u + ".sync"));
#else
                        
#endif

                        //RimLog.Message("Frame " + ___ticksGameInt + " : " + " ::: " + allSyncDataAvailable + "[" + ___ticksGameInt + "] :: " + nextSyncTickValue + " [is synced : ] " + imInSync);

                        Action onA = LocalDB.OnApply;

                        if(cachedData != null)
                        {
                            IsSyncTick = true;

                            //RimLog.Message("Synctick happened at " + ___ticksGameInt);

                            SyncTickData.IsDeserializing = true;
                            //JobTrackerPatch.FlushCData();
                            shouldReallyTick = true;
                            streamholder.WriteLine("pre-deserialize tick at " + Verse.Find.TickManager.TicksGame, "tickstate");
                            streamholder.WriteLine("data applied at " + Verse.Find.TickManager.TicksGame, "tickstate");

                            //lock (LocalDB.OnApply)
                            {

                                Rand.PushState(___ticksGameInt);
                                try
                                {
                                    //RimLog.Message("applying at tick " + Verse.Find.TickManager.TicksGame);

                                    var cd = cachedData;
                                    cachedData = null;

                                    ulong i = 0;
                                    foreach (var act in RandContextCounter.replacement_seed)
                                    {
                                        act((ulong)___ticksGameInt + (i++));
                                    }

                                    foreach (var a in cd)
                                    {
                                        a.AcceptResult();
                                    }
                                    
                                    TickManagerPatch.SetSyncTickValue(nextProcessionTick + syncRoundLength);
                                    nextCommunicationTick += syncRoundLength;
                                    //Interlocked.Exchange(ref LocalDB.OnApply, null);
                                    //LocalDB.clientLocalStorage.ForEach(u => u.Value.AcceptResult());

                                    //nextSyncTickValue = Verse.Find.TickManager.TicksGame + syncRoundLength;
                                }
                                catch (Exception ee)
                                {
                                    RimLog.Message(ee.ToString());
                                }
                                Rand.PopState();

                                LocalDB.clientLocalStorage.Clear();
                            }
                            //SyncTickData.Apply(___ticksGameInt);
                            //__instance.DoSingleTick();

                            imInSync = false;
                        }
                    }
                }
            }

            //ReferenceTranspilerMethod(ref ___ticksGameInt);
            Rand.PopState();
            return false;
        }
    }
}
