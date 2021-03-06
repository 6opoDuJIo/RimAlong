﻿using CooperateRim;
using CooperateRim.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Verse;

public class NetDemo
{
    public static Action<string> log = u => { new Action(Console.WriteLine).BeginInvoke(k => { Console.WriteLine(u); }, u); };
    public static int dispatchedActionCounter;
    static TcpClient tc;
    public static NetworkStream ns;
    public static LinkedList<NetworkStream> allClients = new LinkedList<NetworkStream>();
    static int streamLocker;

    [Serializable]
    public class SaveFileData
    {
        public string partial_name;
        public string tcontext;
    }

    static SaveFileData m_sfd;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetSFD(SaveFileData sfd)
    {
        Console.WriteLine("Received savefile data for game " + sfd.partial_name + " with length of " + sfd.tcontext.Length);
        m_sfd = sfd;
    }

    public static void LoadFromRemoteSFD()
    {
        PirateRPC.PirateRPC.SendInvocation
        (
            ns, u => 
            {
                var ldata = GetSFD();

                for (; ldata == null; ldata = GetSFD())
                {
                    Thread.Sleep(100);
                }

                PirateRPC.PirateRPC.SendInvocation(u, uu =>
                {
                    SetSFD(ldata);
                    PerformSaveFileLoad();
                });
            }
        );
    }

    public static int ServerGetRoundLength()
    {
        return TickManagerPatch.syncRoundLength;
    }

    public static void ClientSetRoundLength(int roundLen)
    {
        TickManagerPatch.syncRoundLength = roundLen;
    }

    public static void PerformSaveFileLoad()
    {
        System.IO.File.WriteAllText(GenFilePaths.FilePathForSavedGame(m_sfd.partial_name), m_sfd.tcontext);
        ThingFilterPatch.avoidThingFilterUsage = true;
        GameDataSaveLoader.LoadGame(m_sfd.partial_name);
        ThingFilterPatch.avoidThingFilterUsage = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static SaveFileData GetSFD()
    {
        return m_sfd;
    }

    static void Log(string s)
    {
        log(s);
    }

    static bool hasClientID;

    public static void WaitForConnection(string host)
    {
        tc = new TcpClient();
        tc.Client.NoDelay = true;
        tc.Connect(host, 12345);
        ns = tc.GetStream();
        hasClientID = false;
        forcedDisconnect = false;
        try
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                PirateRPC.PirateRPC.SendInvocation(ns, u =>
                {
                    int cid = SyncTickData.cliendID;
                    Interlocked.Increment(ref SyncTickData.cliendID);
                    NetDemo.ServerHandlePlayerJoined(cid, u);
                });

                for (; !hasClientID && !forcedDisconnect; )
                {

                }
            }, "Waiting for server".Translate(), true, e => { RimLog.Error(e.ToString()); });
        }
        catch (Exception ee)
        {
            log(ee.ToString());
        }

        Thread t = new Thread( ()=> 
        {
            for (;  ; )
            {
                PirateRPC.PirateRPC.ReceiveInvocation(ns);
            }
        } );

        t.Start();
    }

    public static bool HasAllDataForFrame(int frameID)
    {
        return ns.DataAvailable;// ns.DataAvailable;
    }

    public static void Receive()
    {
       // PirateRPC.PirateRPC.ReceiveInvocation(ns);
    }

    public static void setupCallbacks()
    {
        LocalDB.log = log;
    }
    
    public static void PushStateToDirectory(int sourceID, int tickID, SyncTickData data, int channelID)
    {
        PirateRPC.PirateRPC.SendInvocation(ns, u => 
        {
            LocalDB.PushData(tickID, sourceID, data);
            LocalDB.TryDistributeData(tickID);
        });
    }

    public static int desiredPlayerCount;

    public static void SetDesiredPlayerCount(int playercount)
    {
        desiredPlayerCount = playercount;
    }

    static Action<Stream> GetCallbackForSuccessfulPlayerJoin(int id, int roundLen)
    {
        return k => 
        {
            SyncTickData.SetClientID(id);
            NetDemo.ClientSetRoundLength(roundLen);
            hasClientID = true;
            LongEventHandler.QueueLongEvent(CooperateRimming.GenerateWorld, "Waiting to make a world", true, e => { NetDemo.Log(e.ToString()); });
        };
    }

    public static void ServerHandlePlayerJoined(int id, Stream s)
    {
        if (id >= desiredPlayerCount)
        {
            Log("Client tried to connect, but server is full" + id);
            PirateRPC.PirateRPC.SendInvocation(s, u => { NetDemo.ForcePlayerDisconnect("Connection refused : server full"); });
        }
        else
        {
            Log("sending client id " + id);
            int roundLen = ServerGetRoundLength();
            Log("sending round len " + roundLen);
            PirateRPC.PirateRPC.SendInvocation(s, GetCallbackForSuccessfulPlayerJoin(id, roundLen));
        }
    }

    public static bool forcedDisconnect = false;

    private static void ForcePlayerDisconnect(string reason)
    {
        forcedDisconnect = true;
        Messages.Message(reason, RimWorld.MessageTypeDefOf.ThreatBig, true);
    }
}
