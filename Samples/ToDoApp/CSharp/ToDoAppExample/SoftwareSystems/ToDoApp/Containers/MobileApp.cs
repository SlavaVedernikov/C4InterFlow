using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace ToDoAppExample.SoftwareSystems
{
    public partial class ToDoApp : ISoftwareSystemInstance
    {
        public partial class Containers
        {
            public partial class MobileApp : IContainerInstance
            {
                public Container Instance => new Container(GetType())
                {
                    ContainerType = ContainerType.Mobile,
                    Icon = "font-awesome-5/apple"
                };

                public partial class Interfaces
                {
                    public partial class ViewTasks : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<WebApi.Interfaces.GetTasks>(),
                        };
                    }

                    public partial class AddTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<WebApi.Interfaces.AddTask>(),
                        };
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<WebApi.Interfaces.UpdateTask>(),
                        };
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<WebApi.Interfaces.DeleteTask>(),
                        };
                    }

                    public partial class MarkTaskAsDone : IInterfaceInstance
                    {
                        public Interface Instance => new Interface(GetType())
                        {
                            Flow = new Flow(Interface.GetAlias(GetType()))
                                .Use<WebApi.Interfaces.MarkTaskAsDone>(),
                        };
                    }
                }
            }
        }
    }
}
