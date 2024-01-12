using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation
{
    public class CodeGenerator<T> where T : ICodeWriter , new()
    {
        private static readonly T CodeWriter = new T();

        public static string GetSoftwareSystemsCodeHeader(string architectureNamespace)
        {
            return CodeWriter.GetSoftwareSystemsCodeHeader(architectureNamespace);
        }

        public static string GetActorsCodeHeader(string architectureNamespace)
        {
            return CodeWriter.GetActorsCodeHeader(architectureNamespace);
        }

        public static string GetBusinessProcessesCodeHeader(string architectureNamespace)
        {
            return CodeWriter.GetBusinessProcessesCodeHeader(architectureNamespace);
        }

        public static string GetSoftwareSystemCode(string architectureNamespace, string name, string label, string? description = null, string? boundary = null)
        {
            return CodeWriter.GetSoftwareSystemCode(architectureNamespace, name, label, description, boundary);
        }

        public static string GetActorCode(string architectureNamespace, string type, string name, string label, string? description = null)
        {
            return CodeWriter.GetActorCode(architectureNamespace, type, name, label, description);
        }

        public static string GetBusinessProcessStartCode(string architectureNamespace, string name, string label, string? description = null)
        {
            return CodeWriter.GetBusinessProcessStartCode(architectureNamespace, name, label, description);
        }

        public static string GetBusinessProcessEndCode(string architectureNamespace, string name, string label, string? description = null)
        {
            return CodeWriter.GetBusinessProcessEndCode(architectureNamespace, name, label, description);
        }

        public static string GetBusinessActivityCode(string name, string actor, string[] uses, string? description = null)
        {
            return CodeWriter.GetBusinessActivityCode(name, actor, uses, description);
        }

        public static string GetContainerCode(string architectureNamespace, string softwareSystemName, string containerName, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null)
        {
            return CodeWriter.GetContainerCode(architectureNamespace, softwareSystemName, containerName, label, type, description, technology, boundary);
        }

        public static string GetComponentCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            return CodeWriter.GetComponentCode(architectureNamespace, softwareSystemName, containerName, name, label, componentType, description, technology);
        }

        public static string GetEntityCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null)
        {
            return CodeWriter.GetEntityCode(architectureNamespace, softwareSystemName, containerName, name, label, type, description, composedOfMany, composedOfOne, extends);
        }

        public static string GetComponentInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string ? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return CodeWriter.GetComponentInterfaceCode(architectureNamespace, softwareSystemName, containerName, componentName, name, label, description, protocol, path, isPrivate, uses, input, inputTemplate, output, outputTemplate);
        }

        public static string GetContainerInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return CodeWriter.GetContainerInterfaceCode(architectureNamespace, softwareSystemName, containerName, name, label, description, protocol, uses, input, inputTemplate, output, outputTemplate);
        }

        public static string GetSoftwareSystemInterfaceCode(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return CodeWriter.GetSoftwareSystemInterfaceCode(architectureNamespace, softwareSystemName, name, label, description, protocol, uses, input, inputTemplate, output, outputTemplate);
        }

        private static void WriteLoopBlock(StringBuilder result, string condition, string blockCode)
        {
            result.AppendLine(CodeWriter.GetLoopFlowCode(condition));
            result.Append(GetFormattedBlock(blockCode));
            result.AppendLine(CodeWriter.GetEndLoopFlowCode());
        }

        private static void HandleWhileStatement(
            StringBuilder result, 
            WhileStatementSyntax whileStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null) 
        {
            var blockCode = HandleBlock(
                            whileStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);
            if (!string.IsNullOrEmpty(blockCode))
            {
                WriteLoopBlock(result, whileStatement.Condition.ToString(), blockCode);
            }
        }

        private static void HandleForStatement(
            StringBuilder result,
            ForStatementSyntax forStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                            forStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);
            if (!string.IsNullOrEmpty(blockCode))
            {
                WriteLoopBlock(result, forStatement.Condition.ToString(), blockCode);
            }
        }

        private static void HandleForEachStatement(
            StringBuilder result,
            ForEachStatementSyntax forEachStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                            forEachStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);
            if (!string.IsNullOrEmpty(blockCode))
            {
                WriteLoopBlock(result, forEachStatement.Expression.ToString(), blockCode);
            }
        }

        private static void HandleDoStatement(
            StringBuilder result,
            DoStatementSyntax doStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                            doStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);
            if (!string.IsNullOrEmpty(blockCode))
            {
                WriteLoopBlock(result, doStatement.Condition.ToString(), blockCode);
            }
        }

        private static void HandleUsingStatement(
            StringBuilder result,
            UsingStatementSyntax usingStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                            usingStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);

            if (!string.IsNullOrEmpty(blockCode))
            {
                foreach (var variable in usingStatement.Declaration?.Variables)
                {
                    foreach (var invocationExpression in variable.Initializer?.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray())
                    {
                        var invocationExpressionBlockCode = HandleInvocationExpression(
                           invocationExpression,
                           methodDeclaration,
                           architectureClassDeclaration,
                           writer,
                           alternativeInvocationMappers);

                        if (!string.IsNullOrEmpty(invocationExpressionBlockCode))
                        {
                            result.AppendLine(invocationExpressionBlockCode);
                        }
                    }
                }

                result.AppendLine(GetFormattedBlock(blockCode));
            }
        }

        private static void HandleIfStatement(
            StringBuilder result,
            IfStatementSyntax ifStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                            ifStatement.Statement,
                            methodDeclaration,
                            architectureClassDeclaration,
                            writer,
                            alternativeInvocationMappers);

            var elseBlockCode = HandleElse(
                ifStatement.Else,
                methodDeclaration,
                architectureClassDeclaration,
                writer,
                alternativeInvocationMappers);

            if (!string.IsNullOrEmpty(blockCode) || !string.IsNullOrEmpty(elseBlockCode))
            {
                if (ifStatement.Condition is InvocationExpressionSyntax invocationExpression)
                {
                    var conditionBlockCode = HandleInvocationExpression(
                        invocationExpression,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);

                    if (!string.IsNullOrEmpty(conditionBlockCode))
                    {
                        result.Append(GetFormattedBlock(conditionBlockCode));
                    }
                }

                result.AppendLine(CodeWriter.GetIfFlowCode(ifStatement.Condition.ToString()));

                if (!string.IsNullOrEmpty(blockCode))
                {
                    result.Append(GetFormattedBlock(blockCode));
                }
                if (!string.IsNullOrEmpty(elseBlockCode))
                {
                    result.Append(GetFormattedBlock(elseBlockCode));
                }

                result.AppendLine(CodeWriter.GetEndIfFlowCode());
            }
        }

        private static void HandleReturnStatement(
            StringBuilder result,
            ReturnStatementSyntax returnStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = string.Empty;
            
            if (returnStatement.Expression == null)
            {
                result.AppendLine(CodeWriter.GetReturnFlowCode());
            }
            else
            {
                foreach (var invocationExpression in returnStatement.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    blockCode = HandleInvocationExpression(
                        invocationExpression,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    if (!string.IsNullOrEmpty(blockCode))
                    {
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(blockCode))
                {
                    result.AppendLine(blockCode);
                }
                else if (returnStatement.Expression is InvocationExpressionSyntax invocationExpression)
                {
                    result.AppendLine(CodeWriter.GetReturnFlowCode(invocationExpression.Expression.ToFullString()));
                }
                else if (returnStatement.Expression is IdentifierNameSyntax identifierNameSyntax)
                {
                    result.AppendLine(CodeWriter.GetReturnFlowCode(identifierNameSyntax.Identifier.Text));
                }
            }
        }

        private static void HandleTryStatement(
            StringBuilder result,
            TryStatementSyntax tryStatement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var blockCode = HandleBlock(
                        tryStatement.Block,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
            var catchCodeBlocks = new Dictionary<CatchClauseSyntax, string>();

            foreach (var catchBlock in tryStatement.Catches)
            {
                catchCodeBlocks.Add(catchBlock, HandleBlock(
                    catchBlock.Block,
                    methodDeclaration,
                    architectureClassDeclaration,
                    writer,
                    alternativeInvocationMappers));
            }

            var finallyBlockCode = string.Empty;
            if (tryStatement.Finally != null)
            {
                finallyBlockCode = HandleBlock(
                    tryStatement.Finally.Block,
                    methodDeclaration,
                    architectureClassDeclaration,
                    writer,
                    alternativeInvocationMappers);
            }

            if (!string.IsNullOrEmpty(blockCode) ||
                catchCodeBlocks.Any(x => !string.IsNullOrEmpty(x.Value)) ||
                !string.IsNullOrEmpty(finallyBlockCode))
            {
                result.AppendLine(CodeWriter.GetTryFlowCode());
                if (!string.IsNullOrEmpty(blockCode))
                {
                    result.Append(GetFormattedBlock(blockCode));
                }

                foreach (var catchBlock in catchCodeBlocks)
                {
                    if (!string.IsNullOrEmpty(catchBlock.Value))
                    {
                        //TODO: Add support for Catch Exception parameter
                        result.AppendLine(CodeWriter.GetCatchFlowCode());
                        result.Append(GetFormattedBlock(catchBlock.Value, SyntaxFactory.Whitespace("\t\t")));
                        result.AppendLine(CodeWriter.GetEndCatchFlowCode());
                    }

                }

                if (tryStatement.Finally != null &&
                    !string.IsNullOrEmpty(finallyBlockCode))
                {
                    result.AppendLine(CodeWriter.GetFinallyFlowCode());
                    result.Append(GetFormattedBlock(finallyBlockCode, SyntaxFactory.Whitespace("\t\t")));
                    result.AppendLine(CodeWriter.GetEndFinallyFlowCode());
                }
                result.AppendLine(CodeWriter.GetEndTryFlowCode());
            }
        }

        private static void HandleThrowStatement(
            StringBuilder result,
            ThrowStatementSyntax throwStatement)
        {
            if (throwStatement.Expression == null)
            {
                result.AppendLine(CodeWriter.GetThrowExceptionFlowCode());
            }
            else if (throwStatement.Expression is InvocationExpressionSyntax invocationExpression)
            {
                result.AppendLine(CodeWriter.GetThrowExceptionFlowCode(invocationExpression.Expression.ToFullString()));
            }
            else if (throwStatement.Expression is ObjectCreationExpressionSyntax objectCreationExpression)
            {
                result.AppendLine(CodeWriter.GetThrowExceptionFlowCode(objectCreationExpression.Type.ToString()));
            }
        }

        private static void HandleOtherStatements(
            StringBuilder result,
            StatementSyntax statement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            foreach (var invocationExpression in statement.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var blockCode = HandleInvocationExpression(
                    invocationExpression,
                    methodDeclaration,
                    architectureClassDeclaration,
                    writer,
                    alternativeInvocationMappers);
                if (!string.IsNullOrEmpty(blockCode))
                {
                    result.AppendLine(blockCode);
                }
            }
        }

        private static void HandleStatement(
            StringBuilder result,
            StatementSyntax statement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            switch (statement)
            {
                case WhileStatementSyntax whileStatement:
                    HandleWhileStatement(
                        result,
                        whileStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;

                case ForStatementSyntax forStatement:
                    HandleForStatement(
                        result,
                        forStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;

                case ForEachStatementSyntax forEachStatement:
                    HandleForEachStatement(
                        result,
                        forEachStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;

                case DoStatementSyntax doStatement:
                    HandleDoStatement(
                        result,
                        doStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;

                case IfStatementSyntax ifStatement:
                    HandleIfStatement(
                        result,
                        ifStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;
                case ReturnStatementSyntax returnStatement:
                    HandleReturnStatement(
                        result,
                        returnStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;
                case TryStatementSyntax tryStatement:
                    HandleTryStatement(
                        result,
                        tryStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;
                case ThrowStatementSyntax throwStatement:
                    HandleThrowStatement(
                        result,
                        throwStatement);
                    break;
                case UsingStatementSyntax usingStatement:
                    HandleUsingStatement(
                        result,
                        usingStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;
                default:
                    HandleOtherStatements(
                        result,
                        statement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    break;
            }
        }



        public static string GetFlowCode(
        MethodDeclarationSyntax methodDeclaration,
        ClassDeclarationSyntax architectureClassDeclaration,
        NetToNetArchitectureAsCodeWriter writer,
        IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
    {
        var result = new StringBuilder().AppendLine(CodeWriter.GetFlowHeader());
        

        if (methodDeclaration.Body == null) 
        { 
            return result.ToString(); 
        }

        foreach (var statement in methodDeclaration.Body.Statements)
        {
            HandleStatement(
                result,
                statement,
                methodDeclaration,
                architectureClassDeclaration,
                writer,
                alternativeInvocationMappers);
        }

        return result.ToString();
    }

        private static string HandleBlock(
            StatementSyntax statement,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var result = new StringBuilder();

            if (statement is BlockSyntax block)
            {
                foreach (var innerStatement in block.Statements)
                {
                    HandleStatement(
                        result,
                        innerStatement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                }
            }

            return result.ToString();
        }


        private static string HandleElse(
            ElseClauseSyntax elseClause,
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax architectureClassDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var result = new StringBuilder();
            var blockCode = string.Empty;
            if (elseClause != null)
            {
                if (elseClause.Statement is IfStatementSyntax elseIfStatement)
                {
                    blockCode = HandleBlock(
                        elseIfStatement.Statement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    var elseBlockCode = HandleElse(
                        elseIfStatement.Else,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    if (!string.IsNullOrEmpty(blockCode) || !string.IsNullOrEmpty(elseBlockCode))
                    {
                        result.AppendLine(CodeWriter.GetElseIfFlowCode(elseIfStatement.Condition.ToFullString()));
                        if (!string.IsNullOrEmpty(blockCode))
                        {
                            result.Append(blockCode);
                        }
                        if (!string.IsNullOrEmpty(elseBlockCode))
                        {
                            result.Append(elseBlockCode);
                        }
                    }
                }
                else
                {
                    blockCode = HandleBlock(
                        elseClause.Statement,
                        methodDeclaration,
                        architectureClassDeclaration,
                        writer,
                        alternativeInvocationMappers);
                    if (!string.IsNullOrEmpty(blockCode))
                    {
                        result.AppendLine(CodeWriter.GetElseFlowCode());
                        result.Append(blockCode);
                    }
                }
            }

            return result.ToString();
        }

        private static string HandleInvocationExpression(
            InvocationExpressionSyntax invocationExpression, 
            MethodDeclarationSyntax methodDeclaration, 
            ClassDeclarationSyntax architectureClassDeclaration, 
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var result = new StringBuilder();
            var blockCode = string.Empty;

            var usesAliases = new List<string>();
            var architectureClassSyntaxTree = architectureClassDeclaration.SyntaxTree;
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var systemMethodDeclarationSyntaxTree = methodDeclaration.SyntaxTree;
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemTypeMap = writer.SoftwareSystemTypeMap;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemMethodDeclarationSyntaxTree.FilePath));
            var syatemMethodCompilation = systemProject.GetCompilationAsync().Result;

            var systemMethodSyntaxTree = syatemMethodCompilation.SyntaxTrees.FirstOrDefault(x => x.FilePath == systemMethodDeclarationSyntaxTree.FilePath);

            if (systemMethodSyntaxTree == null) return result.ToString();

            var systemMethodRoot = systemMethodSyntaxTree.GetRoot();
            var systemMethodSemanticModel = syatemMethodCompilation.GetSemanticModel(systemMethodSyntaxTree);
            var invocationMemberName = invocationExpression.GetInvokedMemberName();
            var invocationReceiverType = invocationExpression.GetReceiverType(systemMethodSemanticModel);

            if (systemTypeMap.Keys.Any(x => x == invocationReceiverType?.ToDisplayString()))
            {
                //TODO: Add support for multiple implementation types per a single interface type
                var invocationImplementationType = systemTypeMap.FirstOrDefault(x => x.Key == invocationReceiverType.ToDisplayString()).Value.FirstOrDefault();
                var invocationTypeDefinition = systemWorkspace.GetTypeDefinition(invocationImplementationType);

                if (invocationTypeDefinition != null)
                {
                    var invocationMethod = invocationTypeDefinition.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(x => x.Identifier.ValueText == invocationMemberName);

                    if (invocationMethod != null && writer.ComponentMethodInterfaceClassMap.Any(x => x.Value == invocationMethod))
                    {
                        var interfaceClassFilePath = writer.ComponentMethodInterfaceClassMap.FirstOrDefault(x => x.Value == invocationMethod).Key;
                        var interfaceClassSyntaxTree = architectureCompilation.SyntaxTrees.FirstOrDefault(x => x.FilePath == interfaceClassFilePath);
                        var interfaceClassSemanticModel = architectureCompilation.GetSemanticModel(interfaceClassSyntaxTree);
                        var interfaceAliasField = interfaceClassSyntaxTree.GetRoot().DescendantNodes()
                            .OfType<FieldDeclarationSyntax>().First(f => f.Declaration.Variables.Any(v => v.Identifier.Text == "ALIAS"));

                        var interfaceAliasValue = interfaceClassSemanticModel.GetConstantValue(
                            interfaceAliasField.Declaration.Variables[0].Initializer.Value).Value as string;

                        if (!string.IsNullOrEmpty(interfaceAliasValue))
                        {
                            usesAliases.Add(interfaceAliasValue);
                        }
                    }
                }

            }
            else if (alternativeInvocationMappers != null)
            {
                foreach (var customInvocationMapper in alternativeInvocationMappers)
                {
                    customInvocationMapper.Mapper.Invoke(invocationExpression, systemMethodSemanticModel, usesAliases, writer, customInvocationMapper.Args);
                }
            }

            var componentInterfaceAlias = architectureClassDeclaration.GetAliasFieldValue();
            // Make sure that usesAliases does not contain references to the componentInterfaceAlias,
            // which could happen in cases when overloading is used.
            usesAliases.RemoveAll(x => x.Equals(componentInterfaceAlias));

            foreach (var usesAlias in usesAliases)
            {
                result.Append(CodeWriter.GetUseFlowCode(usesAlias));
            }

            return result.ToString();
        }

        private static string GetFormattedBlock(string blockCode, SyntaxTrivia leadingTrivia = default)
        {
            if(leadingTrivia == default)
            {
                leadingTrivia = SyntaxFactory.Whitespace("\t");
            }
            return $"{string.Join(Environment.NewLine, blockCode.Split(Environment.NewLine).Select(x => string.IsNullOrEmpty(x) ? x : $"{leadingTrivia}{x}"))}";
        }

        internal static string GetSoftwareSystemsDirectory()
        {
            return "SoftwareSystems";
        }

        internal static string GetActorsDirectory()
        {
            return "Actors";
        }

        internal static string GetBusinessProcessesDirectory()
        {
            return "BusinessProcesses";
        }

        public static string GetSoftwareSystemInterfacesDirectory(string softwareSystemName)
        {
            return Path.Combine($@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Interfaces");
        }

        public static string GetContainersDirectory(string softwareSystemName)
        {
            return Path.Combine($@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Containers");
        }

        public static string GetContainerInterfaceDirectory(string softwareSystemName, string containerName)
        {
            return Path.Combine($@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Containers\{containerName}\Interfaces");
        }

        public static string GetComponentsDirectory(string softwareSystemName, string containerName)
        {
            return Path.Combine($@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Containers\{containerName}\Components");
        }

        public static string GetComponentInterfacesDirectory(string softwareSystemName, string containerName, string componentName)
        {
            return $@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Containers\{containerName}\Components\{componentName}\Interfaces";
        }

        internal static string GetEntitiesDirectory(string softwareSystemName, string containerName)
        {
            return $@"{GetSoftwareSystemsDirectory()}\{softwareSystemName}\Containers\{containerName}\Entities";
        }
    }
}

















