using Core.Logger;

namespace Core.Service.Log {
    public interface IYisoLogService : IYisoService {
        public YisoLogger GetLogger(string name);
        public YisoLogger GetLogger<T>();
    }
}