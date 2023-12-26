using System.Runtime.CompilerServices;
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
            log = Logger;
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
        private static int _teleportPosIndex = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
        public static void TeleportPlayerOutWithInverseTeleporter(int playerObj, ref Vector3 teleportPos)
        {
            teleportPos = _teleportPos;
            Plugin.log.LogInfo($"Teleporting player {playerObj} to {_teleportPos}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipTeleporter), "beamOutPlayer")]
        public static void SetTeleportPosition(bool ___isInverseTeleporter)
        {
            if (___isInverseTeleporter) {
                _teleportPos = RoundManager.Instance.insideAINodes[_teleportPosIndex].transform.position;
                _teleportPosIndex += 1;
                _teleportPosIndex %= RoundManager.Instance.insideAINodes.Length;
            }
        }

        [HarmonyPrefix]
		[HarmonyPatch(typeof(ShipTeleporter), "Awake")]
		private static void Awake(ShipTeleporter __instance)
		{
			if (__instance.isInverseTeleporter)
			{
				__instance.cooldownAmount = 10f;
                Plugin.log.LogInfo("Reduced the cooldown of the inverse teleporter to 10 seconds.");
			}
		}
    }
}
