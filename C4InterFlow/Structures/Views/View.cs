using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Structures.Views
{
    public record View : Structure
    {
        public View(string alias, string label) : base(alias, label)
        {
        }

        public View(string alias, string label, string description) : this(alias, label)
        {
            Description = description;
        }

        private string[]? _scopes = null;
        public string[]? Scopes
        {
            get { return _scopes; }
            init
            {
                _scopes = GetPropertyValue(value, DiagramScopesOption.GetAllValues(), nameof(Scopes));
            }
        }

        private string[]? _types = null;
        public string[]? Types
        {
            get { return _types; }
            init
            {
                _types = GetPropertyValue(value, DiagramTypesOption.GetAllValues(), nameof(Types));
            }
        }

        private string[]? _levelsOfDetails = null;
        public string[]? LevelsOfDetails
        {
            get { return _levelsOfDetails; }
            init
            {
                _levelsOfDetails = GetPropertyValue(value, DiagramLevelsOfDetailsOption.GetAllValues(), nameof(LevelsOfDetails));
            }
        }

        private string[]? _formats = null;
        public string[]? Formats
        {
            get { return _formats; }
            init
            {
                _formats = GetPropertyValue(value, DiagramFormatsOption.GetAllDiagramFormats(), nameof(Formats));
            }
        }

        public string[]? Interfaces { get; init; }

        public string[]? Activities { get; init; }
        public string[]? BusinessProcesses { get; init; }
        public string[]? Namespaces { get; init; }
        public int? MaxLineLabels { get; init; }
        public bool? ExpandUpstream { get; init; }
        

        private string[]? GetPropertyValue(string[]? value, string[] supportedValues, string property)
        {
            if (value != null && value.All(x => !supportedValues.Contains(Cli.Utils.ToKebabCase(x))))
                throw new ArgumentOutOfRangeException(property);
            return value?.Select(x => Cli.Utils.ToKebabCase(x)).ToArray();
        }
    }
}
