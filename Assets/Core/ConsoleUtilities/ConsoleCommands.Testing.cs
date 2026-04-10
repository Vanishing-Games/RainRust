using IngameDebugConsole;
using UnityEngine;

namespace Core.ConsoleUtilities
{
    public partial class ConsoleCommands : MonoBehaviour
    {
        [ConsoleMethod("resload", "load a addressable resource and spawn it at given position")]
        public static void LoadAndPlace(string address, float x, float y, float z)
        {
            AddressableCommands.LoadAddressableCommand<GameObject> command = new(address);
            GameObjectCommands.InstantiateGoCommand goCommand = new(
                command.Execute(),
                new Vector3(x, y, z)
            );

            goCommand.Execute();
        }

        [ConsoleMethod("addressable_info", "print addressable system info")]
        public static void PrintAddressableInfo()
        {
            var command = new AddressableCommands.PrintAddressableInfoCommand();
            command.Execute();
        }
    }
}
