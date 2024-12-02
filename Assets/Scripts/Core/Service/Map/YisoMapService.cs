using System;
using System.Collections.Generic;
using Core.Domain.Map;

namespace Core.Service.Map {
    public class YisoMapService : IYisoMapService {
        private readonly Dictionary<int, YisoMap> maps;

        public bool TryGetMap(int id, out YisoMap map) => maps.TryGetValue(id, out map);
        
        public bool IsReady() => true;

        private YisoMapService(Settings settings) {
            maps = settings.mapPackSO.CreateDict();
        }
        
        internal static YisoMapService CreateService(Settings settings) => new(settings);
        public void OnDestroy() { }
        [Serializable]
        public class Settings {
            public YisoMapPackSO mapPackSO;
        }
    }
}