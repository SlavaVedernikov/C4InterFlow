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
                public static Container Instance => new Container(typeof(MobileApp))
                {
                    ContainerType = ContainerType.Mobile
                };

                public partial class Interfaces
                {
                    public partial class ViewTasks : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(ViewTasks))
                        {
                            Flow = new Flow(Interface.GetAlias<ViewTasks>())
                                .Use<WebApi.Interfaces.GetTasks>(),
                        };
                    }

                    public partial class AddTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(AddTask))
                        {
                            Flow = new Flow(Interface.GetAlias<AddTask>())
                                .Use<WebApi.Interfaces.AddTask>(),
                        };
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(UpdateTask))
                        {
                            Flow = new Flow(Interface.GetAlias<UpdateTask>())
                                .Use<WebApi.Interfaces.UpdateTask>(),
                        };
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(DeleteTask))
                        {
                            Flow = new Flow(Interface.GetAlias<DeleteTask>())
                                .Use<WebApi.Interfaces.DeleteTask>(),
                        };
                    }

                    public partial class MarkTaskAsDone : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(MarkTaskAsDone))
                        {
                            Flow = new Flow(Interface.GetAlias<MarkTaskAsDone>())
                                .Use<WebApi.Interfaces.MarkTaskAsDone>(),
                        };
                    }
                }
            }
        }
    }
}
