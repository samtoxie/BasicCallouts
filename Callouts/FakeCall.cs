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
using Basic_Callouts.Callouts;

namespace Basic_Callouts.Callouts
{
    [CalloutInfo("Suspicious Activity", CalloutProbability.Medium)]
    public class FakeCall : Callout
    {
        //ASSETS

        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Vector3 SpawnPoint;
        private Blip SuspectBlip;

        //CALLOUT MESSAGE

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(900f)); ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f); AddMinimumDistanceCheck(20f, SpawnPoint);
            CalloutMessage = "Suspicious Activity"; CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudio("CITIZENS_REPORT CRIME_SUSPICIOUS_ACTIVITY IN_OR_ON_POSITION");

            return base.OnBeforeCalloutDisplayed();
        }


        public override bool OnCalloutAccepted()
        {
            Functions.PlayScannerAudio("UNIT_TAKING_CALL_01 REPORT_RESPONSE_COPY_03");
            Functions.PlayScannerAudio("BEAT_03 XRAY DIV_08 UNITS_RESPOND_CODE_03_02");
            SuspectVehicle = new Vehicle("POLICE", SpawnPoint);
            SuspectVehicle.IsPersistent = true;
            Suspect = SuspectVehicle.CreateRandomDriver();
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = true;
            SuspectBlip.Color = System.Drawing.Color.Red;
            SuspectBlip.EnableRoute(System.Drawing.Color.Red);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            if (Game.LocalPlayer.Character.DistanceTo(Suspect.Position) < 60f)
            { Game.DisplaySubtitle("~b~Dispatch~w~: Disregard, callout is a prankcall.", 4000); GameFiber.Wait(1000); End(); }
        }

        public override void End()
        {
            base.End();
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

        }
    }
}
