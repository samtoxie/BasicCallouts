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
    [CalloutInfo("Abandoned Vehicle", CalloutProbability.Medium)]
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
        private string[] MyModel = new string[] { "DUKES", "BALLER", "BALLER2", "BISON", "BISON2", "BJXL", "CAVALCADE", "CHEETAH", "COGCABRIO", "ASEA", "ADDER", "FELON", "FELON2", "ZENTORNO",
        "WARRENER", "RAPIDGT", "INTRUDER", "FELTZER2", "FQ2", "RANCHERXL", "REBEL", "SCHWARZER", "COQUETTE", "CARBONIZZARE", "EMPEROR", "SULTAN", "EXEMPLAR", "MASSACRO",
        "DOMINATOR", "ASTEROPE", "PRAIRIE", "NINEF", "WASHINGTON", "CHINO", "CASCO", "INFERNUS", "ZTYPE", "DILETTANTE", "VIRGO", "F620", "PRIMO", "SULTAN", "EXEMPLAR", "F620", "FELON2", "FELON", "SENTINEL", "WINDSOR",
            "DOMINATOR", "DUKES", "GAUNTLET", "VIRGO", "ADDER", "BUFFALO", "ZENTORNO", "MASSACRO" };
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
            myVehicle = new Vehicle(MyModel[rnd.Next(MyModel.Length)], SpawnPoint, SpawnHeading);
            if (!myVehicle.Exists()) return false;
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            CalloutMessage = "Abandoned Vehicle"; CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudio("ASSISTANCE_REQUIRED VEHICLE_ON_FIRE");

            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            myBlip = myVehicle.AttachBlip();
            myBlip.Color = Color.LightSkyBlue;
            myBlip.EnableRoute(Color.LightSkyBlue);
            Functions.PlayScannerAudio("UNITS_RESPOND_CODE_03");
            Game.DisplayNotification("~r~Dispatcher~w~: Caller saw the vehicle abandoned with no driver nearby. Smoke coming from engine so possible on fire. Respond Code 3, Fire Department is also en-route.");
            CalloutRunning = true;
            CalloutRunning2 = true;
            myVehicle.EngineHealth = MathHelper.GetRandomSingle(100.0f, 10.0f);
            Rage.Native.NativeFunction.Natives.SMASH_VEHICLE_WINDOW(myVehicle, 0);
            Rage.Native.NativeFunction.Natives.SMASH_VEHICLE_WINDOW(myVehicle, 6);
            myVehicle.IsStolen = true;
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
                    Game.DisplayNotification("~r~Dispatch~w~: Update: Car is not on fire, only smoke coming from engine. Fire Department is cancelled. Car has recently been stolen.");
                    Game.DisplayHelp("If the callout doesn't end iteself you can press END to force end it!", 7500);
                    GameFiber.Wait(2000);
                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY");
                    GameFiber.Wait(5500);
                    CalloutRunning = false;
                }
            }
            while (CalloutRunning2)
            {
                GameFiber.Yield();
                if (Vector3.Distance(myVehicle.Position, ChosenSpawnData.Item1) > 6f)
                {
                    End();
                    CalloutRunning2 = false;
                }
                else if (Game.IsKeyDownRightNow(Keys.End))
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
            if (myVehicle.Exists()) { myVehicle.Delete(); }

        }
    }
}
