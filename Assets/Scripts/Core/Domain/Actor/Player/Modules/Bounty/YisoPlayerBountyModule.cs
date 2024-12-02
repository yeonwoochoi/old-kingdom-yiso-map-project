using System.Collections.Generic;
using Core.Domain.Actor.Player.Modules.Base;
using Core.Domain.Types;
using Core.Domain.Wanted;
using UnityEngine.Events;

namespace Core.Domain.Actor.Player.Modules.Bounty {
    public class YisoPlayerBountyModule : YisoPlayerBaseModule {
        public event UnityAction<YisoPlayerBountyEventArgs> OnBountyEvent;

        private readonly Dictionary<int, YisoWanted> wanteds = new();

        private readonly Dictionary<YisoBountyStatus, List<int>> progresses = new() {
            { YisoBountyStatus.IDLE, new List<int>() },
            { YisoBountyStatus.PROGRESS, new List<int>() },
            { YisoBountyStatus.COMPLETE, new List<int>() }
        };
        
        public YisoPlayerBountyModule(YisoPlayer player) : base(player) { }

        private void RaiseEvent(YisoPlayerBountyEventArgs args) {
            OnBountyEvent?.Invoke(args);
        }
    }
}