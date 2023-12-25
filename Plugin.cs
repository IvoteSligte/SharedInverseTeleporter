using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SharedInverseTeleporter
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance { get; private set; }
        
        internal static ManualLogSource log;
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            instance = this;
            log = this.Logger;
            harmony.PatchAll();
            log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}

namespace SharedInverseTeleporter.patches
{
    [HarmonyPatch]
    class SharedInverseTeleporter
    {
        private static Vector3 _teleportPos;
        private static bool teleportValueSet = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
        public static void TeleportPlayerOutWithInverseTeleporter(int playerObj, ref Vector3 teleportPos)
        {
            if (teleportValueSet)
            {
                teleportPos = _teleportPos;
            }
            else
            {
                _teleportPos = teleportPos;
                teleportValueSet = true;
            }

            Plugin.log.LogInfo($"Teleporting player {playerObj} to {_teleportPos}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipTeleporter), "beamOutPlayer")]
        public static void ResetTeleportPosition()
        {
            teleportValueSet = false;
        }

        [HarmonyPrefix]
		[HarmonyPatch(typeof(ShipTeleporter), "Awake")]
		private static void Awake(ShipTeleporter __instance)
		{
			if (__instance.isInverseTeleporter)
			{
				__instance.cooldownAmount = 10f;
			}
		}
    }
}
