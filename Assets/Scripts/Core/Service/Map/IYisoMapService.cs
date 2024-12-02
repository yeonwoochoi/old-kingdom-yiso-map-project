using Core.Domain.Map;

namespace Core.Service.Map {
    public interface IYisoMapService : IYisoService {
        public bool TryGetMap(int id, out YisoMap map);
    }
}