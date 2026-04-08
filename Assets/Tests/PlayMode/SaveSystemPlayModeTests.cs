using System.Collections;
using System.IO;
using Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test.PlayMode
{
    public class SaveSystemPlayModeTests
    {
        [UnityTest]
        public IEnumerator SaveManager_PersistsAcrossSceneLoads()
        {
            var instance = SaveManager.Instance;
            Assert.IsNotNull(instance);

            var go = instance.gameObject;
            Assert.IsTrue(go.transform.parent == null, "Persistent singleton should be at root");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveManager_UsesPersistentDataPath_InRuntimeMode()
        {
            var go = new GameObject("SaveManager_Runtime");
            var sm = go.AddComponent<SaveManager>();

            var modeField = typeof(SaveManager).GetField(
                "m_SaveMode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            modeField.SetValue(sm, SaveMode.Runtime);

            var rootField = typeof(SaveManager).GetField(
                "m_RootPathType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            rootField.SetValue(sm, RootPathType.PersistentDataPath);

            string expectedRoot = Path.Combine(Application.persistentDataPath, "Saves");
            Assert.AreEqual(expectedRoot, sm.SaveDirectory);

            Object.Destroy(go);
            yield return null;
        }

        /* Commented out due to Save System refactor (removal of ISavable and Register system)
        [UnityTest]
        public IEnumerator SaveManager_AutomaticRegistration_FromMonoBehaviour()
        {
            // We need a class that registers itself in Start
            var go = new GameObject("SavableObject");
            var testSavable = go.AddComponent<TestSavableComponent>();
            testSavable.saveID = "AutoRegTest";

            // Wait for Start()
            yield return null;

            // Save and see if it's there
            SaveManager.Instance.WriteSlotSaveAsync().Forget();

            string path = Path.Combine(SaveManager.Instance.SaveDirectory, "default.json");
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(File.ReadAllText(path).Contains("AutoRegTest"));

            Object.Destroy(go);
            yield return null;
        }

        private class TestSavableComponent : MonoBehaviour
        {
            public string saveID;
            public string SaveID => saveID;

            public object CaptureState() => "SomeState";

            public void RestoreState(object state) { }

            private void Start()
            {
                // SaveManager.Instance.Register(this);
            }

            private void OnDestroy()
            {
                // if (SaveManager.Instance != null)
                //     SaveManager.Instance.Unregister(this);
            }
        }
        */
    }
}
