using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace ToDoAppExample.SoftwareSystems
{
    public partial class ToDoApp : ISoftwareSystemInstance
    {
        public static SoftwareSystem Instance => new SoftwareSystem(typeof(ToDoApp));
    }
}