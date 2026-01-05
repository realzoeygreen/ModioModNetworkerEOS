using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using MelonLoader;
using ModioModNetworker.Data;
using ModioModNetworker.Utilities;
using Il2CppSLZ;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Player;
using LabFusion.Network.Serialization;
using LabFusion.Entities;

namespace ModioModNetworker.Patches
{
    public class PooleeSpawnPatch
    {
        [HarmonyPatch(typeof(Poolee), "OnSpawn")]
        private static class SpawnPatchClass {
            
            public static void Prefix(Poolee __instance)
            {
                if (MainClass.confirmedHostHasIt && NetworkInfo.HasServer)
                {
                    try
                    {
                        Data.ModInfo installedModInfo = ModInfoUtilities.GetModInfoForPoolee(__instance);
                        if (installedModInfo != null)
                        {
                            using (var writer = NetWriter.Create())
                            {
                                var data = ModlistData.Create(PlayerIDManager.LocalID, installedModInfo, ModlistData.ModType.SPAWNABLE);
                                data.Serialize(writer);
                                using (var message = NetMessage.ModuleCreate<ModlistMessage>(writer, CommonMessageRoutes.ReliableToClients))
                                {
                                    MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                    catch (Exception e) {
                        // Ignore
                    }
                }
            }
        }

        // Also patch the catchup spawn
        [HarmonyPatch(typeof(SpawnSender), nameof(SpawnSender.SendCatchupSpawn), typeof(byte), typeof(string), typeof(ushort), typeof(SerializedTransform), typeof(byte), typeof(EntitySource))]
        private static class CatchupSpawnPatch
        {
            public static void Prefix(byte ownerID, string barcode, ushort entityID, SerializedTransform serializedTransform, byte playerID, EntitySource source)
            {
                if (NetworkInfo.IsHost)
                {
                    Data.ModInfo installedModInfo = ModInfoUtilities.GetModInfoForSpawnableBarcode(barcode);
                    if (installedModInfo != null)
                    {
                        using (var writer = NetWriter.Create())
                        {
                            var data = ModlistData.Create(PlayerIDManager.LocalID, installedModInfo, ModlistData.ModType.SPAWNABLE);
                            data.Serialize(writer);
                            using (var message = NetMessage.ModuleCreate<ModlistMessage>(writer, CommonMessageRoutes.ReliableToClients))
                            {
                                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }
    }
}