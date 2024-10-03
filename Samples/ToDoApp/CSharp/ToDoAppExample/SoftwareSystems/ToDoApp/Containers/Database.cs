using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace ToDoAppExample.SoftwareSystems
{
    public partial class ToDoApp : ISoftwareSystemInstance
    {
        public partial class Containers
        {
            public partial class Database : IContainerInstance
            {
                public static Container Instance => new Container(typeof(Database))
                {
                    ContainerType = ContainerType.Database
                };

                public partial class Interfaces
                {
                    public partial class SelectTasks : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(SelectTasks));
                    }

                    public partial class InsertTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(InsertTask));
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(UpdateTask));
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(DeleteTask));
                    }
                }
            }
        }
    }
}
