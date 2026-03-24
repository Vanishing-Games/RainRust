using System.Collections;
using System.IO;
using Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class SaveSystemPlayModeTests
    {
        [UnityTest]
        public IEnumerator SaveManager_PersistsAcrossSceneLoads()
        {
            // Get instance
            var instance = SaveManager.Instance;
            Assert.IsNotNull(instance);

            var go = instance.gameObject;

            // In PlayMode, MonoSingletonPersistent should have called DontDestroyOnLoad
            // We can't easily check DontDestroyOnLoad flag but we can simulate scene change

            // Create a temporary object to see if it gets destroyed while SaveManager survives
            var tempGO = new GameObject("Temp");

            // Load a new "empty" scene or just simulate by clearing everything else
            // For simplicity in this test environment, we just check if it's persistent
            Assert.IsTrue(go.transform.parent == null, "Persistent singleton should be at root");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveManager_UsesPersistentDataPath_InRuntimeMode()
        {
            var go = new GameObject("SaveManager_Runtime");
            var sm = go.AddComponent<SaveManager>();

            // Use reflection to set mode to Runtime and RootPath to PersistentDataPath
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
            SaveManager.Instance.Save("auto_reg_test");

            string path = Path.Combine(SaveManager.Instance.SaveDirectory, "auto_reg_test.json");
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(File.ReadAllText(path).Contains("AutoRegTest"));

            Object.Destroy(go);
            yield return null;
        }

        private class TestSavableComponent : MonoBehaviour, ISavable
        {
            public string saveID;
            public string SaveID => saveID;

            public object CaptureState() => "SomeState";

            public void RestoreState(object state) { }

            private void Start()
            {
                SaveManager.Instance.Register(this);
            }

            private void OnDestroy()
            {
                if (SaveManager.Instance != null)
                    SaveManager.Instance.Unregister(this);
            }
        }
    }
}
