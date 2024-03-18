using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Visualisation;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class DiagramOptions
    {
        public DiagramOptions(string[] scopes, string[] types, string[] levelsOfDetails)
        {
            if(scopes.Any(x => x == DiagramScopesOption.ALL_SCOPES))
            {
                Scopes = DiagramScopesOption.GetAllScopes();
            }
            else
            {
                Scopes = scopes.Distinct().ToArray();
            }

            if (types.Any(x => x == DiagramTypesOption.ALL_DIADRAM_TYPES))
            {
                Types = DiagramTypesOption.GetAllDiagramTypes();
            }
            else
            {
                Types = types.Distinct().ToArray();
            }

            if (levelsOfDetails.Any(x => x == DiagramLevelsOfDetailsOption.ALL_LEVELS_OF_DETAILS))
            {
                LevelsOfDetails = DiagramLevelsOfDetailsOption.GetAllLevelsOfDetails();
            }
            else
            {
                LevelsOfDetails = levelsOfDetails.Distinct().ToArray();
            }
        }

        public string[] Scopes { get; private set; }
        public string[] Types { get; private set; }
        public string[] LevelsOfDetails { get; private set; }

        public static bool IsSupported(string scope, string type, string levelOfDetail)
        {
            var supportedTypes = new List<string>();
            var supportedLevelsOfDetail = new List<string>();

            switch (scope)
            {
                case DiagramScopesOption.ALL_SOFTWARE_SYSTEMS:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT,
                        DiagramLevelsOfDetailsOption.CONTAINER
                    });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4
                    });

                    break;
                }
                case DiagramScopesOption.SOFTWARE_SYSTEM:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT,
                        DiagramLevelsOfDetailsOption.CONTAINER, 
                        DiagramLevelsOfDetailsOption.COMPONENT
                    });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4
                    });

                    break;
                }
                case DiagramScopesOption.SOFTWARE_SYSTEM_INTERFACE:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT
                        });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4,
                        DiagramTypesOption.SEQUENCE,
                        DiagramTypesOption.C4_SEQUENCE
                    });

                    break;
                }
                case DiagramScopesOption.CONTAINER:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTAINER,
                        DiagramLevelsOfDetailsOption.COMPONENT
                        });
                        supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4
                    });

                    break;
                }
                case DiagramScopesOption.CONTAINER_INTERFACE:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT,
                        DiagramLevelsOfDetailsOption.CONTAINER
                        });
                        supportedTypes.AddRange(new[] {
                            DiagramTypesOption.C4_STATIC,
                            DiagramTypesOption.C4,
                            DiagramTypesOption.SEQUENCE,
                            DiagramTypesOption.C4_SEQUENCE
                        });

                    break;
                }
                case DiagramScopesOption.COMPONENT:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.COMPONENT
                    });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4
                    });

                    break;
                }
                case DiagramScopesOption.COMPONENT_INTERFACE:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT,
                        DiagramLevelsOfDetailsOption.CONTAINER,
                        DiagramLevelsOfDetailsOption.COMPONENT
                    });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4,
                        DiagramTypesOption.SEQUENCE,
                        DiagramTypesOption.C4_SEQUENCE
                    });

                    break;
                }
                case DiagramScopesOption.BUSINESS_PROCESS:
                {
                    supportedLevelsOfDetail.AddRange(new[] {
                        DiagramLevelsOfDetailsOption.CONTEXT,
                        DiagramLevelsOfDetailsOption.CONTAINER,
                        DiagramLevelsOfDetailsOption.COMPONENT
                    });
                    supportedTypes.AddRange(new[] {
                        DiagramTypesOption.C4_STATIC,
                        DiagramTypesOption.C4,
                        DiagramTypesOption.SEQUENCE,
                        DiagramTypesOption.C4_SEQUENCE
                    });

                    break;
                }
                default:
                    break;
            }

            return supportedTypes.Contains(type) && supportedLevelsOfDetail.Contains(levelOfDetail);
        }
    }

    public class DiagramOptionsBinder : BinderBase<DiagramOptions>
    {
        private readonly Option<string[]> _scopes;
        private readonly Option<string[]> _types;
        private readonly Option<string[]> _levelsOfDetails;

        public DiagramOptionsBinder(Option<string[]> scopes, Option<string[]> types, Option<string[]> levelsOfDetails)
        {
            _scopes = scopes;
            _types = types;
            _levelsOfDetails = levelsOfDetails;
        }

        protected override DiagramOptions GetBoundValue(BindingContext bindingContext) =>
            new DiagramOptions(
                bindingContext.ParseResult.GetValueForOption(_scopes),
                bindingContext.ParseResult.GetValueForOption(_types),
                bindingContext.ParseResult.GetValueForOption(_levelsOfDetails));
    }
}
