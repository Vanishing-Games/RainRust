// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Core;
// using NUnit.Framework;
// using R3;
// using UnityEngine;

// namespace Test.Editor
// {
//     public class SaveSystemTests
//     {
//         private class MockSavable : ISavable
//         {
//             public string SaveID { get; set; } = "MockID";
//             public object State { get; set; }

//             public object CaptureState() => State;

//             public void RestoreState(object state) => State = state;
//         }

//         private class ComplexData
//         {
//             public List<int> IntList = new();
//             public Dictionary<string, string> StringMap = new();
//             public float Value;
//         }

//         // For Polymorphic testing
//         public abstract class BaseData
//         {
//             public string TypeName;
//         }

//         public class DerivedA : BaseData
//         {
//             public int A;
//         }

//         public class DerivedB : BaseData
//         {
//             public string B;
//         }

//         // For Circular Reference testing
//         public class Node
//         {
//             public string Name;
//             public Node Next;
//         }

//         private VgSaveSystem m_VgSaveSystem;
//         private StatsManager m_StatsManager;
//         private string m_TestSaveDir;

//         [SetUp]
//         public void SetUp()
//         {
//             // Create a new VgSaveSystem instance
//             var go = new GameObject("VgSaveSystem");
//             m_VgSaveSystem = go.AddComponent<VgSaveSystem>();

//             // Create StatsManager
//             var goStats = new GameObject("StatsManager");
//             m_StatsManager = goStats.AddComponent<StatsManager>();

//             m_TestSaveDir = m_VgSaveSystem.SaveDirectory;
//             if (Directory.Exists(m_TestSaveDir))
//                 Directory.Delete(m_TestSaveDir, true);
//             Directory.CreateDirectory(m_TestSaveDir);
//         }

//         [TearDown]
//         public void TearDown()
//         {
//             if (m_VgSaveSystem != null)
//                 GameObject.DestroyImmediate(m_VgSaveSystem.gameObject);

//             if (m_StatsManager != null)
//                 GameObject.DestroyImmediate(m_StatsManager.gameObject);

//             if (Directory.Exists(m_TestSaveDir))
//                 Directory.Delete(m_TestSaveDir, true);

//             MessageBroker.Global.Clear();
//         }

//         [Test]
//         public void Register_ValidSavable_AddsToManager()
//         {
//             var savable = new MockSavable { SaveID = "TestID" };
//             m_VgSaveSystem.Register(savable);

//             savable.State = "TestState";
//             m_VgSaveSystem.Save("test_slot");

//             var file = Path.Combine(m_TestSaveDir, "test_slot.json");
//             Assert.IsTrue(File.Exists(file), "Save file should be created");
//             StringAssert.Contains("TestState", File.ReadAllText(file));
//         }

//         [Test]
//         public void Save_PolymorphicData_RestoresCorrectType()
//         {
//             var savable = new MockSavable { SaveID = "PolyID" };
//             var list = new List<BaseData>
//             {
//                 new DerivedA { TypeName = "A", A = 10 },
//                 new DerivedB { TypeName = "B", B = "Hello" },
//             };
//             savable.State = list;

//             m_VgSaveSystem.Register(savable);
//             m_VgSaveSystem.Save("poly_slot");

//             savable.State = null;
//             m_VgSaveSystem.Load("poly_slot");

//             // Newtonsoft.Json with TypeNameHandling.Auto should restore the actual types
//             // However, in tests, if they are JArrays/JObjects we might need to cast
//             // But VgSaveSystem uses ISavable.RestoreState(object state)

//             var restoredList = (savable.State as IEnumerable<object>).Cast<BaseData>().ToList();
//             Assert.IsInstanceOf<DerivedA>(restoredList[0]);
//             Assert.IsInstanceOf<DerivedB>(restoredList[1]);
//             Assert.AreEqual(10, ((DerivedA)restoredList[0]).A);
//             Assert.AreEqual("Hello", ((DerivedB)restoredList[1]).B);
//         }

//         [Test]
//         public void Save_CircularReference_DoesNotCrash()
//         {
//             var savable = new MockSavable { SaveID = "CircularID" };
//             var n1 = new Node { Name = "Node1" };
//             var n2 = new Node { Name = "Node2" };
//             n1.Next = n2;
//             n2.Next = n1; // Circular!

//             savable.State = n1;
//             m_VgSaveSystem.Register(savable);

//             // Should not throw StackOverflowException because ReferenceLoopHandling.Ignore is set
//             Assert.DoesNotThrow(() => m_VgSaveSystem.Save("circular_slot"));
//         }

//         [Test]
//         public void Save_PublishesSuccessEvent()
//         {
//             bool received = false;
//             bool success = false;
//             string slot = "";

//             using var sub = MessageBroker.Global.Subscribe<SaveEvent>(e =>
//             {
//                 received = true;
//                 success = e.Success;
//                 slot = e.Slot;
//             });

//             m_VgSaveSystem.Save("event_test");

//             Assert.IsTrue(received);
//             Assert.IsTrue(success);
//             Assert.AreEqual("event_test", slot);
//         }

//         [Test]
//         public void Load_PublishesSuccessEvent()
//         {
//             m_VgSaveSystem.Save("event_test_load");

//             bool received = false;
//             bool success = false;

//             using var sub = MessageBroker.Global.Subscribe<LoadEvent>(e =>
//             {
//                 received = true;
//                 success = e.Success;
//             });

//             m_VgSaveSystem.Load("event_test_load");

//             Assert.IsTrue(received);
//             Assert.IsTrue(success);
//         }

//         [Test]
//         public void Save_CapturesPlayTime()
//         {
//             StatsManager.Increment(StatKeys.GameDuration, 123.45f);

//             m_VgSaveSystem.Save("time_test");

//             var path = Path.Combine(m_TestSaveDir, "time_test.json");
//             var json = File.ReadAllText(path);
//             Assert.IsTrue(json.Contains("123.45"));
//         }

//         [Test]
//         public void Register_DuplicateID_LastOneWins()
//         {
//             var s1 = new MockSavable { SaveID = "Dup", State = "State1" };
//             var s2 = new MockSavable { SaveID = "Dup", State = "State2" };

//             m_VgSaveSystem.Register(s1);
//             m_VgSaveSystem.Register(s2);

//             m_VgSaveSystem.Save("dup_test");

//             var file = Path.Combine(m_TestSaveDir, "dup_test.json");
//             string content = File.ReadAllText(file);
//             Assert.IsTrue(content.Contains("State2"));
//             Assert.IsFalse(content.Contains("State1"));
//         }

//         [Test]
//         public void Load_CorruptedFile_PublishesFailureEvent()
//         {
//             var slot = "corrupted";
//             var path = Path.Combine(m_TestSaveDir, slot + ".json");
//             File.WriteAllText(path, "{ \"invalid\": json... }");

//             bool received = false;
//             bool success = true;

//             using var sub = MessageBroker.Global.Subscribe<LoadEvent>(e =>
//             {
//                 received = true;
//                 success = e.Success;
//             });

//             m_VgSaveSystem.Load(slot);

//             Assert.IsTrue(received);
//             Assert.IsFalse(success, "Load should fail for corrupted file");
//         }

//         [Test]
//         public void Load_HigherVersion_HandlesGracefully()
//         {
//             var slot = "version_test";
//             var container = new SaveContainer();
//             container.Meta = new SaveMeta(slot);
//             container.Meta.SaveFileVersion = "99.9.9"; // Future version
//             container.Data["ID"] = "FutureData";

//             var settings = new Newtonsoft.Json.JsonSerializerSettings
//             {
//                 TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
//             };
//             string json = Newtonsoft.Json.JsonConvert.SerializeObject(container, settings);
//             File.WriteAllText(Path.Combine(m_TestSaveDir, slot + ".json"), json);

//             Assert.DoesNotThrow(() => m_VgSaveSystem.Load(slot));
//         }

//         [Test]
//         public void Save_PathTraversalSlotName_SavesToSubfolder()
//         {
//             // If slotName is "sub/slot", Path.Combine will create a subfolder if it doesn't exist
//             // If slotName is "../slot", it goes up!

//             var trickySlot = "sub_folder/test_slot";
//             m_VgSaveSystem.Save(trickySlot);

//             var expectedPath = Path.Combine(m_TestSaveDir, trickySlot + ".json");
//             Assert.IsTrue(File.Exists(expectedPath), "Should support subfolders in slot names");
//         }

//         [Test]
//         public void Save_NullData_SerializesAsNull()
//         {
//             var savable = new MockSavable { SaveID = "NullID", State = null };
//             m_VgSaveSystem.Register(savable);
//             m_VgSaveSystem.Save("null_test");

//             savable.State = "Something Else";
//             m_VgSaveSystem.Load("null_test");

//             Assert.IsNull(savable.State, "Should restore as null");
//         }

//         [Test]
//         public void Unregister_Savable_StopsSavingIt()
//         {
//             var savable = new MockSavable { SaveID = "UnregID", State = "State" };
//             m_VgSaveSystem.Register(savable);
//             m_VgSaveSystem.Unregister(savable);

//             m_VgSaveSystem.Save("unreg_test");

//             var file = Path.Combine(m_TestSaveDir, "unreg_test.json");
//             Assert.IsFalse(File.ReadAllText(file).Contains("UnregID"));
//         }
//     }
// }
