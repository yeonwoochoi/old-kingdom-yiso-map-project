using System.Collections.Generic;
using System.Linq;
using Core.Behaviour;
using Core.Domain.CoolDown;
using UnityEngine.Events;
using Utils.ObjectId;

namespace Core.Service.CoolDown {
    public class YisoCoolDownService : IYisoCoolDownService {
        private readonly Dictionary<YisoObjectID, YisoPlayerCoolDownHolder> holders = new();

        private event UnityAction<CoolDownEventArgs> OnCoolDownEvent;

        private readonly RunIBehaviour bootstrap;
        
        private YisoCoolDownService(RunIBehaviour bootstrap) {
            this.bootstrap = bootstrap;
        }

        internal static YisoCoolDownService CreateService(RunIBehaviour bootstrap) => new(bootstrap);

        public void RegisterEvent(UnityAction<CoolDownEventArgs> handler) {
            OnCoolDownEvent += handler;
        }

        public void UnregisterEvent(UnityAction<CoolDownEventArgs> handler) {
            OnCoolDownEvent -= handler;
        }

        public YisoObjectID CreateCoolDown(int sourceId, double coolDown) {
            var id = YisoObjectID.Generate();
            var holder = new YisoPlayerCoolDownHolder(id, sourceId, coolDown);
            holders[id] = holder;
            holder.StartCoolDown(bootstrap, () => {
                RaiseEvent(new CoolDownCompleteEventArgs(holder));
                holders.Remove(id);
            });
            RaiseEvent(new CoolDownStartEventArgs(holder));
            return id;
        }

        public bool RemoveCoolDown(YisoObjectID id) {
            if (!holders.TryGetValue(id, out var holder) || holder.StopCoolDown()) return false;
            
            holders.Remove(id);
            RaiseEvent(new CoolDownStopEventArgs(holder));
            return true;
        }

        public bool ExistCoolDown(int sourceId) => holders.Values.Any(holder => holder.SourceId == sourceId);

        public bool ExistCoolDown(YisoObjectID id) => holders.ContainsKey(id);

        public bool TryGetCoolDown(int sourceId, out YisoPlayerCoolDownHolder holder) {
            holder = holders.Values.FirstOrDefault(h => h.SourceId == sourceId);
            return holder != null;
        }

        public bool TryGetCoolDown(YisoObjectID id, out YisoPlayerCoolDownHolder holder) => holders.TryGetValue(id, out holder);

        private void RaiseEvent(CoolDownEventArgs args) {
            OnCoolDownEvent?.Invoke(args);
        }
        
        public void OnDestroy() { }

        public bool IsReady() => true;
    }
}