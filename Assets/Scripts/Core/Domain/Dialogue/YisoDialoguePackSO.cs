using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Domain.Dialogue {
    [CreateAssetMenu(fileName = "DialoguePack", menuName = "Yiso/Dialogue/Pack")]
    public class YisoDialoguePackSO : ScriptableObject {
        public List<YisoDialogueSO> dialogueSos;

        public Dictionary<int, YisoDialogue> ToDictionary(Sprite yisoIcon, Sprite erryIcon) =>
            dialogueSos.ToDictionary(d => d.id, d => d.CreateDialogue(yisoIcon, erryIcon));
    }
}