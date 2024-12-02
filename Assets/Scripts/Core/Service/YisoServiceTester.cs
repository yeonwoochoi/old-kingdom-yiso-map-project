using Core.Behaviour;
using Core.Service.UI;
using Core.Service.UI.Event;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Service {
    public class YisoServiceTester : RunIBehaviour {
        
        [Button]
        public void TestDialogue(string speaker = "사내") {
            var contents = new[] {
                "어이, 예쁜 아가씨. 왜 울고 그래. 오빠랑 좋은 시간 보낼까?",
                "맞아. 슬픈 일은 모두 잊어버리고 오빠들이랑 놀자.",
                "의외로 앙칼지네? 소박 맞아서 그런가? 저 봇짐 좀 봐봐.",
                "머리를 안 올린 거 보면, 처녀 같은 데? 어이, 아가씨, 가출한 거야?"
            };
        }

        public void HandleJoystickInput(bool value) {
            Debug.Log($"Value: => {value}");
        }
    }
}