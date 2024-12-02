using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Feedback.Sequencing {
    public enum SequenceTrackStates {
        Idle,
        Down,
        Up
    }

    /// <summary>
    /// 시퀀스 내의 개별적인 이벤트
    /// Track 내부에 여러 Sequence Note 보유 가능 (중복도 가능)
    /// </summary>
    [Serializable]
    public class YisoSequenceNote {
        public float timestamp;
        public int id;

        public virtual YisoSequenceNote Copy() {
            var newNote = new YisoSequenceNote {
                id = id,
                timestamp = timestamp
            };
            return newNote;
        }
    }

    /// <summary>
    /// 시퀀스의 구조 (Sequence = Track-Track-Track-Track)
    /// 한 Track에는 여러 Sequence Note를 가지고 실행시킬 수도 있음
    /// </summary>
    [Serializable]
    public class YisoSequenceTrack {
        public int id = 0;
        public Color trackColor;
        public KeyCode key = KeyCode.Space;
        public bool active;
        [ReadOnly] public SequenceTrackStates state = SequenceTrackStates.Idle;
        [HideInInspector] public bool initialized = false;

        public virtual void SetDefaults(int index) {
            if (initialized) return;
            id = index;
            trackColor = YisoSequence.RandomSequenceColor();
            key = KeyCode.Space;
            active = true;
            state = SequenceTrackStates.Idle;
            initialized = true;
        }
    }

    [Serializable]
    public class YisoSequenceList {
        public List<YisoSequenceNote> line;
    }

    /// <summary>
    /// This scriptable object holds "sequences", data used to record and play events in sequence
    /// YisoSequences can be played by YisoFeedbacks from their Timing section, by Sequencers and potentially other classes
    /// </summary>
    [CreateAssetMenu(menuName = "Yiso/Sequencer/Sequence", fileName = "Sequence")]
    public class YisoSequence : ScriptableObject {
        [Header("Sequence")] [ReadOnly] public float length;
        public YisoSequenceList originalSequence;
        public float endSilenceDuration = 0f;

        [Header("Sequence Contents")] public List<YisoSequenceTrack> sequenceTracks;

        [Header("Quantizing")] public int targetBPM = 120;
        public List<YisoSequenceList> quantizedSequence;

        protected float[] quantizedBeats;
        protected List<YisoSequenceNote> deleteList;

        static int SortByTimestamp(YisoSequenceNote p1, YisoSequenceNote p2) {
            return p1.timestamp.CompareTo(p2.timestamp);
        }

        /// <summary>
        /// Sorts the original sequence based on timestamps
        /// </summary>
        public virtual void SortOriginalSequence() {
            originalSequence.line.Sort(SortByTimestamp);
        }

        public virtual void ComputeLength() {
            length = originalSequence.line[^1].timestamp + endSilenceDuration;
        }

        public virtual void QuantizeOriginalSequence() {
            ComputeLength();
            QuantizeSequenceToBPM(originalSequence.line);
        }

        public virtual void QuantizeSequenceToBPM(List<YisoSequenceNote> baseSequence) {
            var sequenceLength = length;
            var beatDuration = 60f / targetBPM;
            var numberOfBeatsInSequence = (int) (sequenceLength / beatDuration);
            quantizedSequence = new List<YisoSequenceList>();
            deleteList = new List<YisoSequenceNote>();
            deleteList.Clear();

            // we fill the BPM track with the computed timestamps
            quantizedBeats = new float[numberOfBeatsInSequence];
            for (var i = 0; i < numberOfBeatsInSequence; i++) {
                quantizedBeats[i] = i * beatDuration;
            }

            for (var i = 0; i < sequenceTracks.Count; i++) {
                quantizedSequence.Add(new YisoSequenceList());
                quantizedSequence[i].line = new List<YisoSequenceNote>();
                for (var j = 0; j < numberOfBeatsInSequence; j++) {
                    var newNote = new YisoSequenceNote {
                        id = -1,
                        timestamp = quantizedBeats[j]
                    };
                    quantizedSequence[i].line.Add(newNote);

                    foreach (var note in baseSequence) {
                        var newTimestamp = RoundFloatToArray(note.timestamp, quantizedBeats);
                        if ((newTimestamp == quantizedBeats[j]) && (note.id == sequenceTracks[i].id)) {
                            quantizedSequence[i].line[j].id = note.id;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// On validate, Initialize our track's properties
        /// </summary>
        protected virtual void OnValidate() {
            for (var i = 0; i < sequenceTracks.Count; i++) {
                sequenceTracks[i].SetDefaults(i);
            }
        }

        /// <summary>
        /// Randomizes track colors
        /// </summary>
        protected virtual void RandomizeTrackColors() {
            foreach (var track in sequenceTracks) {
                track.trackColor = RandomSequenceColor();
            }
        }

        public static Color RandomSequenceColor() {
            var random = UnityEngine.Random.Range(0, 32);
            return random switch {
                0 => new Color32(240, 248, 255, 255),
                1 => new Color32(127, 255, 212, 255),
                2 => new Color32(245, 245, 220, 255),
                3 => new Color32(95, 158, 160, 255),
                4 => new Color32(255, 127, 80, 255),
                5 => new Color32(0, 255, 255, 255),
                6 => new Color32(255, 215, 0, 255),
                7 => new Color32(255, 0, 255, 255),
                8 => new Color32(50, 128, 120, 255),
                9 => new Color32(173, 255, 47, 255),
                10 => new Color32(255, 105, 180, 255),
                11 => new Color32(75, 0, 130, 255),
                12 => new Color32(255, 255, 240, 255),
                13 => new Color32(124, 252, 0, 255),
                14 => new Color32(255, 160, 122, 255),
                15 => new Color32(0, 255, 0, 255),
                16 => new Color32(245, 255, 250, 255),
                17 => new Color32(255, 228, 225, 255),
                18 => new Color32(218, 112, 214, 255),
                19 => new Color32(255, 192, 203, 255),
                20 => new Color32(255, 0, 0, 255),
                21 => new Color32(196, 112, 255, 255),
                22 => new Color32(250, 128, 114, 255),
                23 => new Color32(46, 139, 87, 255),
                24 => new Color32(192, 192, 192, 255),
                25 => new Color32(135, 206, 235, 255),
                26 => new Color32(0, 255, 127, 255),
                27 => new Color32(210, 180, 140, 255),
                28 => new Color32(0, 128, 128, 255),
                29 => new Color32(255, 99, 71, 255),
                30 => new Color32(64, 224, 208, 255),
                31 => new Color32(255, 255, 0, 255),
                32 => new Color32(154, 205, 50, 255),
                _ => new Color32(240, 248, 255, 255)
            };
        }

        public static float RoundFloatToArray(float value, float[] array) {
            var min = 0;
            if (array[min] >= value) return array[min];

            var max = array.Length - 1;
            if (array[max] <= value) return array[max];

            while (max - min > 1) {
                var mid = (max + min) / 2;

                if (array[mid] == value) {
                    return array[mid];
                }
                else if (array[mid] < value) {
                    min = mid;
                }
                else {
                    max = mid;
                }
            }

            return array[max] - value <= value - array[min] ? array[max] : array[min];
        }
    }
}