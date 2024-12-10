using System.Collections.Generic;
using System.IO;
using Core.Logger;
using Core.Service;
using Core.Service.Log;
using Tools.SaveSystem;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Manager.Modules {
    public class YisoGameSaveModule : YisoGameBaseModule {
        private const string SaveFilePath = "/gameSaveData.json";
        private List<SaveData> saveDataList;

        private YisoLogger LogService =>
            YisoServiceProvider.Instance.Get<IYisoLogService>().GetLogger<YisoGameSaveModule>();

        public YisoGameSaveModule(GameManager manager) : base(manager) {
            saveDataList = new List<SaveData>();
        }

        public override void OnEnabled() {
            base.OnEnabled();
            LoadData();
        }

        private void LoadData() {
            var fullPath = Application.persistentDataPath + SaveFilePath;

            if (File.Exists(fullPath)) {
                var fileStream = new FileStream(fullPath, FileMode.Open);

                // 바이너리 포맷터를 사용하여 데이터를 역직렬화
                var formatter = new BinaryFormatter();
                saveDataList = (List<SaveData>) formatter.Deserialize(fileStream);
                fileStream.Close();

                LogService.Debug("[YisoGameSaveModule.LoadData] Game data loaded from binary format.");
            }
            else {
                LogService.Debug("[YisoGameSaveModule.LoadData] No save file found. Loading default data.");
            }
        }

        private void SaveData() {
            var fullPath = Application.persistentDataPath + SaveFilePath;
            var fileStream = new FileStream(fullPath, FileMode.Create); // 새 파일로 생성

            // 바이너리 포맷터 사용하여 데이터를 직렬화
            var formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, saveDataList);
            fileStream.Close();

            LogService.Debug("[YisoGameSaveModule.LoadData] Game data saved in binary format.");
        }

        public override void OnDisabled() {
            base.OnDisabled();
            SaveData(); // 게임 종료 또는 모듈 비활성화 시 데이터를 저장
        }

        public override void OnDestroy() {
            base.OnDestroy();
            SaveData(); // 게임 종료 시 저장
        }
    }

    /// <summary>
    /// 직렬화 문제를 해결하기 위한 래퍼 클래스
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class SerializationWrapper<T> {
        public List<T> list;

        public SerializationWrapper(List<T> list) {
            this.list = list;
        }
    }
}