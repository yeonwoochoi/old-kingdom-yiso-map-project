using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain.Bounty;
using Core.Domain.Types;
using Core.Logger;
using Core.Service.Character;
using Core.Service.Data.Item;
using Core.Service.Log;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Utils.Extensions;

namespace Core.Service.Bounty {
    public class YisoBountyService : IYisoBountyService {
        private event UnityAction<YisoBountyEventArgs> OnBountyEvent; 
        private readonly Dictionary<int, YisoBounty> bounties;
        private readonly Dictionary<YisoBountyStatus, List<int>> progresses = new();
        public bool IsReady() => true;

        private YisoBounty currentBounty = null;

        public YisoBounty GetCurrentBounty() => currentBounty;

        public void SetCurrentBounty(YisoBounty bounty) {
            currentBounty = bounty;
        }

        public void ReadyBounty(int bountyId) {
            var index = FindBountyIdIndexOrThrow(YisoBountyStatus.IDLE, bountyId);
            var bounty = bounties[bountyId];
            currentBounty = bounty;
        }

        public void StartBounty(int bountyId) {
            var index = FindBountyIdIndexOrThrow(YisoBountyStatus.IDLE, bountyId);
            var bounty = bounties[bountyId];

            progresses[YisoBountyStatus.IDLE].RemoveAt(index);
            progresses[YisoBountyStatus.PROGRESS].Add(bountyId);
            RaiseEvent(new YisoBountyStatusChangeEventArgs(bounty, YisoBountyStatus.PROGRESS));
            // currentBounty = bounty;
        }

        public IReadOnlyList<YisoBounty> GetBountiesByStatus(YisoBountyStatus status) {
            return progresses[status]
                .Select(id => bounties[id])
                .ToList();
        }

        public void DrawBounty() {
            if (currentBounty == null) return;
            var index = FindBountyIdIndexOrThrow(YisoBountyStatus.PROGRESS, currentBounty.Id);
            
            progresses[YisoBountyStatus.PROGRESS].RemoveAt(index);
            progresses[YisoBountyStatus.IDLE].Add(currentBounty.Id);
            RaiseEvent(new YisoBountyStatusChangeEventArgs(currentBounty, YisoBountyStatus.IDLE));
            RaiseEvent(new YisoBountyDrawEventArgs(currentBounty));

            var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoBountyService>();
            logger.Debug($"Draw Bounty(id={currentBounty.Id}, PROGRESS to IDLE");
            currentBounty = null;
        }

        public bool CompleteBounty(out YisoBountyGiveReason reason) {
            var index = FindBountyIdIndexOrThrow(YisoBountyStatus.PROGRESS, currentBounty.Id);
            
            progresses[YisoBountyStatus.PROGRESS].RemoveAt(index);
            progresses[YisoBountyStatus.COMPLETE].Add(currentBounty.Id);
            RaiseEvent(new YisoBountyStatusChangeEventArgs(currentBounty, YisoBountyStatus.COMPLETE));
            var player = YisoServiceProvider.Instance.Get<IYisoCharacterService>().GetPlayer();
            var itemService = YisoServiceProvider.Instance.Get<IYisoItemService>();
            var result = currentBounty.GiveReward(player, itemService.GetItemOrElseThrow, out reason);
            var logger = YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoBountyService>();
            logger.Debug($"Complete Bounty(id={currentBounty.Id}, PROGRESS to COMPLETE");
            return result;
        }

        public void RegisterOnBountyEvent(UnityAction<YisoBountyEventArgs> handler) {
            OnBountyEvent += handler;
        }

        public void UnregisterOnBountyEvent(UnityAction<YisoBountyEventArgs> handler) {
            OnBountyEvent -= handler;
        }

        public void OnDestroy() { }

        private YisoBountyService(Settings settings) {
            bounties = settings.packSO.ToDictionary();
            
            foreach (var status in EnumExtensions.Values<YisoBountyStatus>()) {
                progresses[status] = new List<int>();
            }
            
            // TODO("SAVE AND LOAD")
            progresses[YisoBountyStatus.IDLE].AddRange(bounties.Keys);
            
        }

        private bool TryFindBountyIdIndex(YisoBountyStatus status, int bountyId, out int index) {
            index = -1;

            for (var i = 0; i < progresses[status].Count; i++) {
                if (progresses[status][i] != bountyId) continue;
                index = i;
                break;
            }

            return index != -1;
        }

        private int FindBountyIdIndexOrThrow(YisoBountyStatus status, int bountyId) {
            for (var i = 0; i < progresses[status].Count; i++) {
                if (progresses[status][i] != bountyId) continue;
                return i;
            }

            throw new ArgumentException($"Bounty(id={bountyId}) not in {status} state");
        }

        private void RaiseEvent(YisoBountyEventArgs args) {
            OnBountyEvent?.Invoke(args);
        }
        
        public static YisoBountyService CreateService(Settings settings) => new(settings);

        [Serializable]
        public class Settings {
            public YisoBountyPackSO packSO;
        }
    }
}