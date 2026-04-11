using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core
{
    public static class AddressableCommands
    {
        public class PrintAddressableInfoCommand : ITriggerCommand
        {
            public bool Execute()
            {
                AddressableResourceLoader.PrintAddressableInfo();
                return true;
            }
        }

        public class LoadAddressableCommand<T> : IAsyncCommand<T>
            where T : Object
        {
            public LoadAddressableCommand(string addressableName)
            {
                m_Address = addressableName;
            }

            public Task<T> ExecuteAsync()
            {
                return AddressableResourceLoader.GetAsset<T>(m_Address);
            }

            public T Execute()
            {
                var handle = AddressableResourceLoader.GetAsset<T>(m_Address);
                return handle.Result;
            }

            private string m_Address;
        }
    }
}
