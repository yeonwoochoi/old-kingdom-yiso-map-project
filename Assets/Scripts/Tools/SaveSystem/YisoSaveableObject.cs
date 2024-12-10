using System.Numerics;
using Core.Behaviour;

namespace Tools.SaveSystem {
    [System.Serializable]
    public class SaveData {
        public enum ObjectState {
            Active,
            Inactive,
            Destroy
        }

        public ObjectState state;
        public Vector3 position;
        public string objectName;

        // 기본 생성자
        public SaveData(string name, Vector3 position, ObjectState state) {
            this.objectName = name;
            this.position = position;
            this.state = state;
        }
        
        public override string ToString() {
            return $"Object Name: {objectName}, Position: {position}, State: {state}";
        }
    }

    public class YisoSaveableObject : RunIBehaviour {
        public SaveData saveData;
    }
}