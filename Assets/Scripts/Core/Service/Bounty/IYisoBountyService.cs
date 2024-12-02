using System.Collections.Generic;
using Core.Domain.Bounty;
using Core.Domain.Types;
using UnityEngine.Events;

namespace Core.Service.Bounty {
    public interface IYisoBountyService : IYisoService {
        public IReadOnlyList<YisoBounty> GetBountiesByStatus(YisoBountyStatus status);
        public YisoBounty GetCurrentBounty();
        public void SetCurrentBounty(YisoBounty bounty);
        public void ReadyBounty(int bountyId);
        public void StartBounty(int bountyId);
        public void DrawBounty();
        public bool CompleteBounty(out YisoBountyGiveReason reason);

        public void RegisterOnBountyEvent(UnityAction<YisoBountyEventArgs> handler);

        public void UnregisterOnBountyEvent(UnityAction<YisoBountyEventArgs> handler);
    }
}