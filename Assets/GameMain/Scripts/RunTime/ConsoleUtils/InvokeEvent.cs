using System;
using System.Linq;
using System.Reflection;
using Core;
using IngameDebugConsole;
using UnityEngine;

namespace GameMain.RunTime
{
    public partial class ConsoleCommands : MonoBehaviour
    {
        [ConsoleMethod("invoke_event", "invoke an event by its type name, e.g. GmEditorTestEvent")]
        public static void InvokeEvent(string eventName)
        {
            var eventType = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type =>
                    type.Name == eventName && typeof(IEvent).IsAssignableFrom(type)
                );

            if (eventType != null)
            {
                var eventInstance = Activator.CreateInstance(eventType);

                if (eventInstance is IEvent)
                {
                    Type brokerType = MessageBroker.Global.GetType();

                    MethodInfo publishMethod = brokerType
                        .GetMethods()
                        .FirstOrDefault(m => m.Name == "Publish" && m.IsGenericMethod);

                    if (publishMethod != null)
                    {
                        MethodInfo genericPublish = publishMethod.MakeGenericMethod(eventType);
                        genericPublish.Invoke(MessageBroker.Global, new object[] { eventInstance });
                    }
                    else
                    {
                        CLogger.LogError("Could not find generic Publish method.", LogTag.Event);
                    }
                }
            }
            else
            {
                CLogger.LogError(
                    $"Event type '{eventName}' not found or does not implement IEvent.",
                    LogTag.Event
                );
            }
        }
    }
}
