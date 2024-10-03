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
                public static Container Instance => new Container(typeof(WebApi))
                {
                    ContainerType = ContainerType.Api
                };

                public partial class Interfaces
                {
                    public partial class GetTasks : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(GetTasks))
                        {
                            Flow = new Flow(Interface.GetAlias<GetTasks>())
                                .Use<Database.Interfaces.SelectTasks>(),
                        };
                    }

                    public partial class AddTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(AddTask))
                        {
                            Flow = new Flow(Interface.GetAlias<AddTask>())
                                .Use<Database.Interfaces.InsertTask>(),
                        };
                    }

                    public partial class UpdateTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(UpdateTask))
                        {
                            Flow = new Flow(Interface.GetAlias<UpdateTask>())
                                .Use<Database.Interfaces.UpdateTask>(),
                        };
                    }

                    public partial class DeleteTask : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(DeleteTask))
                        {
                            Flow = new Flow(Interface.GetAlias<DeleteTask>())
                                .Use<Database.Interfaces.DeleteTask>(),
                        };
                    }

                    public partial class MarkTaskAsDone : IInterfaceInstance
                    {
                        public static Interface Instance => new Interface(typeof(MarkTaskAsDone))
                        {
                            Flow = new Flow(Interface.GetAlias<MarkTaskAsDone>())
                                .Use<Database.Interfaces.UpdateTask>(),
                        };
                    }
                }
            }
        }
    }
}
