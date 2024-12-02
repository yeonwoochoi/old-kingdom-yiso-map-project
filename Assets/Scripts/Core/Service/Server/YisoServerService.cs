namespace Core.Service.Server {
    public class YisoServerService : IYisoServerService {
        private bool connected = false;
        public bool IsReady() => connected;

        public void OnDestroy() { }

        private YisoServerService() { }

        internal static YisoServerService CreateService() => new();
    }
}