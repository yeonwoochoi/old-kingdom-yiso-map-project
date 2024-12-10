using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Sirenix.OdinInspector;
using Spawn;
using UnityEngine;

namespace Controller.Map {
    [Serializable]
    public class CutsceneTriggerMapper {
        public int gameModeId;
        public GameObject cutsceneTrigger;
    }

    [AddComponentMenu("Yiso/Controller/Map/Map Data Setter")]
    public class YisoMapDataSetter : RunIBehaviour {
        [Title("Zones")] public List<YisoNavigationZoneController> navigationZones;

        [Title("Camera")] public PolygonCollider2D[] cameraBoundaries;

        [Title("Checkpoints")] public YisoCharacterCheckPoint[] checkPoints;

        [Title("Cutscenes")] public CutsceneTriggerMapper[] stageCutsceneTriggers;
        public CutsceneTriggerMapper[] bountyCutsceneTriggers;

        private YisoLogger LogService =>
            YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoMapDataSetter>();

        public void SetMapData(YisoMapController mapController) {
            if (mapController == null) {
                LogService.Error("[YisoMapDataSetter.SetMapData] MapController is null. Cannot set map data.");
                return;
            }

            // Add navigation zones (avoid duplicates)
            if (navigationZones != null) {
                foreach (var zone in navigationZones) {
                    if (!mapController.AllNavigationZones.Contains(zone)) {
                        mapController.AllNavigationZones.Add(zone);
                    }
                }
            }

            // Add camera boundaries (avoid duplicates)
            if (cameraBoundaries != null) {
                foreach (var boundary in cameraBoundaries) {
                    if (!mapController.AllCameraBoundaries.Contains(boundary)) {
                        mapController.AllCameraBoundaries.Add(boundary);
                    }
                }
            }

            // Add checkpoints (avoid duplicates)
            if (checkPoints != null) {
                foreach (var checkpoint in checkPoints) {
                    if (!mapController.AllCheckPoints.Contains(checkpoint)) {
                        mapController.AllCheckPoints.Add(checkpoint);
                    }
                }
            }

            // Add stage cutscene triggers (avoid duplicates)
            if (stageCutsceneTriggers != null) {
                foreach (var trigger in stageCutsceneTriggers) {
                    if (!mapController.AllStageCutsceneTriggers.Contains(trigger)) {
                        mapController.AllStageCutsceneTriggers.Add(trigger);
                    }
                }
            }

            // Add bounty cutscene triggers (avoid duplicates)
            if (bountyCutsceneTriggers != null) {
                foreach (var trigger in bountyCutsceneTriggers) {
                    if (!mapController.AllBountyCutsceneTriggers.Contains(trigger)) {
                        mapController.AllBountyCutsceneTriggers.Add(trigger);
                    }
                }
            }

            LogService.Debug("[YisoMapDataSetter.SetMapData] Map data has been successfully set.");
        }
    }
}