using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace C4InterFlow.Cli.Configuration;

/// <summary>
/// Validates the content of the c4interflow.config.yaml file against the embedded config-schema.json,
/// which references the generated commands-schema.json for the 'commands' section.
/// </summary>
public static class ConfigValidator
{
    private const string SCHEMA_RESOURCE_NAME = "C4InterFlow.config-schema.json";

    // Only used to resolve the reference from config-schema.json to commands-schema.json.
    private static readonly Uri BaseUri = new("urn:c4interflow:schemas/");

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <param name="commandsSchema">The schema of the 'commands' section, see <see cref="CommandsSchemaBuilder"/>.</param>
    /// <returns>The validation errors, one per problem. Empty if the configuration is valid.</returns>
    public static IReadOnlyList<string> Validate(JToken config, JObject commandsSchema)
    {
        var schema = LoadSchema(commandsSchema);

        if (config.IsValid(schema, out IList<ValidationError> errors))
        {
            return Array.Empty<string>();
        }

        return Flatten(errors)
            .Distinct()
            .Select(x => $"{(string.IsNullOrEmpty(x.Path) ? "(root)" : x.Path)}: {x.Message}")
            .ToList();
    }

    private static JSchema LoadSchema(JObject commandsSchema)
    {
        var assembly = typeof(ConfigValidator).Assembly;
        using var stream = assembly.GetManifestResourceStream(SCHEMA_RESOURCE_NAME)
                           ?? throw new FileNotFoundException($"Embedded resource '{SCHEMA_RESOURCE_NAME}' not found.");
        using var reader = new StreamReader(stream);

        var resolver = new JSchemaPreloadedResolver();
        resolver.Add(new Uri(BaseUri, CommandsSchemaBuilder.FILE_NAME), commandsSchema.ToString());

        return JSchema.Parse(reader.ReadToEnd(), new JSchemaReaderSettings { Resolver = resolver, BaseUri = BaseUri });
    }

    private static IEnumerable<(string Path, string Message)> Flatten(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
        {
            if (error.ChildErrors is { Count: > 0 })
            {
                foreach (var child in Flatten(error.ChildErrors))
                {
                    yield return child;
                }
            }
            else
            {
                // Newtonsoft appends the position, which is meaningless for a converted YAML document.
                yield return (error.Path, Regex.Replace(error.Message, @"\s*Path '.*$", string.Empty));
            }
        }
    }
}
