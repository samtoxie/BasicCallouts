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
using System.Drawing;

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
            SuspectBlip = new Blip(CalloutPosition, 60f);
            SuspectBlip.Color = Color.Yellow;
            SuspectBlip.Alpha = 0.6f;
            SuspectBlip.EnableRoute(System.Drawing.Color.Yellow);
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            if (Game.LocalPlayer.Character.DistanceTo(CalloutPosition) < 60f)
            { Game.DisplaySubtitle("Investigate the area for any suspicious activity", 4000); GameFiber.Wait(5000); Game.DisplayNotification("~r~Dispatch~w~: Disregard, callout is a prankcall."); Functions.PlayScannerAudio("WE_ARE_CODE_4"); GameFiber.Wait(1000); End(); }
        }

        public override void End()
        {
            base.End();
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

        }
    }
}
