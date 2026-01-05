using HarmonyLib;
using LabFusion.Downloading.ModIO;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.RPC;
using MelonLoader;
using ModioModNetworker.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModioModNetworker.Patches
{

    /*[HarmonyPatch(typeof(NetworkModRequester), "RequestMod")]
    public class FusionDownloaderCancel
    {
        public static bool Prefix() {
            return !MainClass.overrideFusionDL;
        }
    }

    [HarmonyPatch(typeof(NetworkModRequester), "RequestAndInstallMod")]
    public class FusionDownloaderCancelRequest
    {
        public static bool Prefix()
        {
            return !MainClass.overrideFusionDL;
        }
    }*/

    [HarmonyPatch(typeof(RigProgressBar), nameof(RigProgressBar.Visible), MethodType.Setter)]
    public class RigProgressBarPatchReport
    {
        public static void Prefix(RigProgressBar __instance, ref bool value)
        {
            if (MainClass.overrideFusionDL)
            {
                value = false;
            }
        }
    }

    [HarmonyPatch(typeof(ModIODownloader), nameof(ModIODownloader.EnqueueDownload))]
    public class ModIoDownloaderEnqueuePatch {
        public static bool Prefix(ModTransaction transaction) {
            if (MainClass.overrideFusionDL) {
                string attemptedModId = transaction.ModFile.ModID.ToString();

                if (!MainClass.modNumericalsDownloadedDuringLobbySession.Contains(attemptedModId)) {
                    string destination = "install_spawnable";

                    if (transaction.Reporter != null) {
                        if (transaction.Reporter.GetType() == typeof(RigProgressBar))
                        {
                            destination = "install_avatar;";

                            PlayerID attemptedId = GetOwnerOfProgressBar((RigProgressBar) transaction.Reporter);
                            if (attemptedId != null)
                            {
                                destination += attemptedId.SmallID;
                            }
                            else
                            {
                                destination = "install_spawnable";
                            }
                        }
                    }

                    ModInfo.RequestModInfoNumerical(transaction.ModFile.ModID.ToString(), destination);
                }
                return false;
            }
            return true;
        }

        private static PlayerID GetOwnerOfProgressBar(RigProgressBar bar) {
            foreach (var playerId in PlayerIDManager.PlayerIDs) {
                if (NetworkPlayerManager.TryGetPlayer(playerId, out var netPlayer)) {
                    if (netPlayer.AvatarSetter.ProgressBar == bar)
                    {
                        return playerId;
                    }
                }
            }

            return null;
        }
    }
}
