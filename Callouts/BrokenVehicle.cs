using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage;
using Rage.Native;
using System.Diagnostics;
using System.IO;
using Basic_Callouts.Common;
using System.Windows.Forms;
using System.Drawing;

namespace Basic_Callouts.Callouts
{
    [CalloutInfo("BrokenVehicle", CalloutProbability.Medium)]
    public class BrokenVehicle : Callout
    {
        //ASSETS

        private TupleList<Vector3, float> ValidTrafficStopSpawnPointsWithHeadings = new TupleList<Vector3, float>();
        private Tuple<Vector3, float> ChosenSpawnData;
        private List<Blip> BlipList = new List<Blip>();
        public static Random rnd = new Random(MathHelper.GetRandomInteger(100, 100000));
        private Vehicle myVehicle;
        private Vector3 SpawnPoint;
        private float SpawnHeading;
        private Blip myBlip;
        private bool CalloutRunning = false;
        private bool CalloutRunning2 = false;

        //CALLOUT MESSAGE

        public override bool OnBeforeCalloutDisplayed()
        {
            foreach (Tuple<Vector3, float> tuple in RoadsideSpawns.TrafficStopSpawnPointsWithHeadings)
            {
                //tuple.Item1 = PedInTuple
                //tuple.Item2 = Vector3 in tuple
                //tuple.Item3 = FloatinTuple
                //Game.LogTrivial(tuple.Item1.ToString());
                //Game.LogTrivial(tuple.Item2.ToString());

                if ((Vector3.Distance(tuple.Item1, Game.LocalPlayer.Character.Position) < 750f) && (Vector3.Distance(tuple.Item1, Game.LocalPlayer.Character.Position) > 280f))
                {
                    ValidTrafficStopSpawnPointsWithHeadings.Add(tuple);
                }
            }
            //foreach (Tuple<Ped, Vector3, float> tuple in PedVector3HeadingTupleList)
            //{
            //    //tuple.Item1 = PedInTuple
            //    //tuple.Item2 = Vector3 in tuple
            //    //tuple.Item3 = FloatinTuple
            //}
            if (ValidTrafficStopSpawnPointsWithHeadings.Count == 0) { return false; }
            ChosenSpawnData = ValidTrafficStopSpawnPointsWithHeadings[rnd.Next(ValidTrafficStopSpawnPointsWithHeadings.Count)];


            SpawnPoint = ChosenSpawnData.Item1;
            SpawnHeading = ChosenSpawnData.Item2;
            myVehicle = new Vehicle("TAILGATER", SpawnPoint, SpawnHeading);
            if (!myVehicle.Exists()) return false;

            CalloutMessage = "Broken Down Vehicle"; CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudio("ASSISTANCE_REQUIRED CRIME_CIVILIAN_ASSISTANCE");

            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            myBlip = myVehicle.AttachBlip();
            myBlip.Color = Color.LightSkyBlue;
            myBlip.EnableRoute(Color.LightSkyBlue);
            Game.DisplaySubtitle("~b~Dispatch~w~: Driver of the vehicle has already left the area", 9000);
            CalloutRunning = true;
            CalloutRunning2 = true;
            myVehicle.EngineHealth = MathHelper.GetRandomSingle(100.0f, 10.0f);
            myVehicle.LicensePlate =  MathHelper.GetRandomInteger(0, 9).ToString() +
                                      MathHelper.GetRandomInteger(0, 9).ToString() +
                                      Convert.ToChar(Convert.ToInt32(Math.Floor(26 * MathHelper.GetRandomDouble(0, 1) + 65))) +
                                      Convert.ToChar(Convert.ToInt32(Math.Floor(26 * MathHelper.GetRandomDouble(0, 1) + 65))) +
                                      Convert.ToChar(Convert.ToInt32(Math.Floor(26 * MathHelper.GetRandomDouble(0, 1) + 65))) +
                                      MathHelper.GetRandomInteger(0, 9).ToString() +
                                      MathHelper.GetRandomInteger(0, 9).ToString() +
                                      MathHelper.GetRandomInteger(0, 9).ToString();
            return base.OnCalloutAccepted(); 
        }
        public override void Process()
        {
            base.Process();
            while (CalloutRunning)
            {
                GameFiber.Yield();
                if (Game.LocalPlayer.Character.DistanceTo(CalloutPosition) < 60f)
                {
                    Game.DisplaySubtitle("Clear the vehicle from the road", 3000);
                    Game.DisplayHelp("Once the car is towed away press END to end the callout", 7500);
                    GameFiber.Wait(7500);
                    CalloutRunning = false;
                }
            }
            while (CalloutRunning2)
            {
                GameFiber.Yield();
                if (Game.IsKeyDownRightNow(Keys.End))
                {
                    End();
                    CalloutRunning2 = false;
                }
            }
        }

        public override void End()
        {
            base.End();
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            if (myBlip.Exists()) { myBlip.Delete(); }
            if (myVehicle.Exists()) { myVehicle.Dismiss(); }

        }
    }
}
