using System.Globalization;
using System.Text.RegularExpressions;
using System.CommandLine;
using System.CommandLine.Parsing;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using Parser = System.CommandLine.Parsing.Parser;
using YamlDotNet.RepresentationModel;

namespace C4InterFlow.Cli.Configuration;

/// <summary>
/// Loads the c4interflow.config.yaml file: reads the YAML, validates it against the schema
/// and deserializes it into a <see cref="C4InterFlowConfig"/>.
/// </summary>
public static class ConfigLoader
{
    public const string FILE_NAME = "c4interflow.config.yaml";

    /// <summary>
    /// Loads the configuration from the given file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <param name="parser">The CLI parser. The commands that can be used in the configuration are taken from it.</param>
    /// <param name="config">The configuration, if it was loaded.</param>
    /// <param name="errors">The reasons why the configuration could not be loaded.</param>
    /// <returns>True if the configuration was loaded.</returns>
    public static bool TryLoad(string path, Parser parser, out C4InterFlowConfig? config, out IReadOnlyList<string> errors)
    {
        config = null;

        JToken document;
        try
        {
            document = ReadYaml(path);
        }
        catch (Exception e) when (e is YamlException or InvalidDataException)
        {
            errors = new[] { $"Could not read YAML: {e.Message}" };
            return false;
        }

        errors = ConfigValidator.Validate(document, CommandsSchemaBuilder.Build(parser));
        if (errors.Count > 0)
        {
            return false;
        }

        config = ToConfig((JObject)document, CommandsSchemaBuilder.GetSupportedCommands(parser));
        return true;
    }

    // The document is valid, so every command, option and value in it is known.
    private static C4InterFlowConfig ToConfig(JObject document, IReadOnlyList<Command> supportedCommands)
    {
        var commands = new List<CommandConfig>();

        foreach (var entry in ((JArray)document["commands"]!).Cast<JObject>())
        {
            var command = supportedCommands.First(x => x.Name == entry.Value<string>("command"));
            var options = new List<CommandOption>();

            if (entry["options"] is JObject optionValues)
            {
                foreach (var property in optionValues.Properties())
                {
                    var option = command.Options.First(x => CommandsSchemaBuilder.ToKey(x.Name) == property.Name);

                    options.Add(new CommandOption(
                        property.Name,
                        $"--{option.Name}",
                        property.Value is JArray array ? array.Select(ToText).ToList() : new List<string> { ToText(property.Value) },
                        property.Value is JArray));
                }
            }

            commands.Add(new CommandConfig(command.Name, entry.Value<bool?>("clearOutputDir") ?? false, options));
        }

        return new C4InterFlowConfig { Commands = commands };
    }

    private static string ToText(JToken value)
    {
        return value is JValue { Value: bool flag }
            ? (flag ? "true" : "false")
            : Convert.ToString((value as JValue)?.Value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static JToken ReadYaml(string path)
    {
        var yamlStream = new YamlStream();
        using (var reader = new StreamReader(path))
        {
            yamlStream.Load(reader);
        }

        if (yamlStream.Documents.Count == 0)
        {
            throw new InvalidDataException("The file is empty.");
        }

        return ToJToken(yamlStream.Documents[0].RootNode);
    }

    private static JToken ToJToken(YamlNode node)
    {
        switch (node)
        {
            case YamlMappingNode mapping:
                var result = new JObject();
                foreach (var child in mapping.Children)
                {
                    if (child.Key is not YamlScalarNode key)
                    {
                        throw new InvalidDataException("Only scalar keys are supported.");
                    }
                    result[key.Value ?? string.Empty] = ToJToken(child.Value);
                }
                return result;
            case YamlSequenceNode sequence:
                return new JArray(sequence.Children.Select(ToJToken));
            case YamlScalarNode scalar:
                return ToJValue(scalar);
            default:
                return JValue.CreateNull();
        }
    }

    private static JValue ToJValue(YamlScalarNode scalar)
    {
        var value = scalar.Value;

        // Quoted and block scalars are always strings. Plain scalars follow the YAML core schema.
        if (scalar.Style != ScalarStyle.Plain)
        {
            return new JValue(value ?? string.Empty);
        }

        if (string.IsNullOrEmpty(value) || value == "~" || value.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return JValue.CreateNull();
        }
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase)) return new JValue(true);
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase)) return new JValue(false);
        if (long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integer))
        {
            return new JValue(integer);
        }
        if (Regex.IsMatch(value, @"^[-+]?(\d+\.\d*|\.\d+|\d+)([eE][-+]?\d+)?$") &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return new JValue(number);
        }

        return new JValue(value);
    }
}
