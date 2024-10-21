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
                public Container Instance => new Container(GetType())
                {
                    ContainerType = ContainerType.Database,
                    Icon = "devicons/msql_server"
                };

                public partial class Interfaces
                {
                    public partial class SelectTasks : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType());
                    }

                    public partial class InsertTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType());
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType());
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType());
                    }
                }
            }
        }
    }
}
