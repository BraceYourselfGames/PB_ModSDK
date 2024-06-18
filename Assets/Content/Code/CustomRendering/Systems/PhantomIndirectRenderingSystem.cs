using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using System.Text;
using PhantomBrigade.Data;

namespace CustomRendering
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateInGroup (typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PhantomRendererSyncSystemV2))]
    public class PhantomIndirectRenderingSystem : ComponentSystem
    {
        private const int defaultBatchAllocation = 1000;
        private static PhantomIndirectRenderingSystem instance;

        //Batches
        private static List<IndirectRenderingBatch> allBatches =
            new List<IndirectRenderingBatch> (defaultBatchAllocation);
        
        private static List<IIndirectLayout> batchCleanup = new List<IIndirectLayout> (defaultBatchAllocation);
        

        //System Instance Data
        public static Camera ActiveCamera;


        //Debugging
        private StringBuilder debugLog = new StringBuilder ();
        public static bool syncLocked;

        protected override void OnCreate()
        {
            instance = this;
        }

        public static void CleanupRenderer ()
        {
            instance.ReleaseBatches ();
            PhantomRendererSyncSystemV2.CleanupSyncSystem ();
        }


        protected override void OnDestroy()
        {
            ReleaseBatches ();
        }

        public static void ResetCounts()
        {
            int batchCount = allBatches.Count;
            for (int i = 0; i < batchCount; ++i)
            {
                allBatches[i].ResetCounts ();
            }
        }

        public void ReleaseBatches()
        {
            int batchCount = batchCleanup.Count;
            for (int i = 0; i < batchCount; ++i)
            {
                batchCleanup[i].Dispose ();
            }

            batchCleanup.Clear ();
            allBatches.Clear ();
        }

        public static void AddBatch(IndirectRenderingBatch batch, IIndirectLayout batchLayout)
        {
            allBatches.Add (batch);
            batchCleanup.Add (batchLayout);
        }
        
        protected override void OnUpdate()
        {
            bool enableRenderer = true;
            if (DataLinkerRendering.data != null)
                enableRenderer = DataLinkerRendering.data.enablePhantomRenderer;

            if (syncLocked || !enableRenderer) 
                return;

            if (allBatches == null) 
                return;

            if (instance == null) 
                return;


            Profiler.BeginSample ("Draw");

            bool drawAllCameras = false;
            //bool combineShadowPass = false;

            if (DataLinkerRendering.data != null)
                drawAllCameras = DataLinkerRendering.data.drawAllCameras;

            if (drawAllCameras)
            {
                for (int i = 0; i < allBatches.Count; ++i)
                {
                    try
                    {
                        allBatches[i].DrawAllCameras ();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine (e);
                    }
                }
            }
            else
            {
                if (ActiveCamera == null) return;


                for (int i = 0; i < allBatches.Count; ++i)
                {
                    try
                    {
                        allBatches[i].DrawSingleCamera (ActiveCamera);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine (e);
                    }
                }
            }
            
            Profiler.EndSample ();

            if (debugLog.Length != 0)
            {
                Debug.Log (debugLog.ToString ());
                debugLog.Clear ();
            }
        }
    }
}