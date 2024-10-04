using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace ToDoAppExample.SoftwareSystems
{
    public partial class ToDoApp : ISoftwareSystemInstance
    {
        public partial class Containers
        {
            public partial class WebApi : IContainerInstance
            {
                public Container Instance => new Container(GetType())
                {
                    ContainerType = ContainerType.Api
                };

                public partial class Interfaces
                {
                    public partial class GetTasks : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<Database.Interfaces.SelectTasks>(),
                        };
                    }

                    public partial class AddTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<Database.Interfaces.InsertTask>(),
                        };
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<Database.Interfaces.UpdateTask>(),
                        };
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<Database.Interfaces.DeleteTask>(),
                        };
                    }

                    public partial class MarkTaskAsDone : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<Database.Interfaces.UpdateTask>(),
                        };
                    }
                }
            }
        }
    }
}
