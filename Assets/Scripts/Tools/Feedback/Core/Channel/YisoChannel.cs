using System;
using UnityEngine;

namespace Tools.Feedback.Core.Channel {
    public enum YisoChannelModes {
        Int,
        Channel
    }

    [Serializable]
    public class YisoChannelData {
        public YisoChannelModes channelMode;
        public int channel;
        public YisoChannel channelDefinition;

        public YisoChannelData(YisoChannelModes channelMode, int channel, YisoChannel channelDefinition) {
            this.channelMode = channelMode;
            this.channel = channel;
            this.channelDefinition = channelDefinition;
        }
    }

    [CreateAssetMenu(menuName = "Yiso/Channel", fileName = "YisoChannel")]
    public class YisoChannel : ScriptableObject {
        public static bool Match(YisoChannelData dataA, YisoChannelModes modeB, int channelB,
            YisoChannel channelDefinitionB) {
            if (dataA == null) return true; // channel data가 없으면 항상 일치로 간주.
            if (dataA.channelMode != modeB) return false;
            if (dataA.channelMode == YisoChannelModes.Int) return dataA.channel == channelB;
            return dataA.channelDefinition == channelDefinitionB;
        }
    }
}