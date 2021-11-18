using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.AutoComplete.MCA.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution.AutoComplete.MCA
{
    /// <summary>
    /// Uses microsoft code analysis to provide autocomplete suggestions.
    /// </summary>
    public class DynamicCodeExecutionMicrosoftCodeAnalysisAutoCompleter : IDynamicCodeAutoCompleter
    {
        /// <summary>
        /// Uses microsoft code analysis to provide autocomplete suggestions.
        /// </summary>
        /// <param name="code">Full c# code</param>
        /// <param name="assemblyLocations">Referenced assembly locations</param>
        /// <param name="position">Cursor position within the code</param>
        /// <returns></returns>
        public Task<IEnumerable<IDynamicCodeCompletionData>> GetAutoCompleteSuggestionsAsync(string code, string[] assemblyLocations, int position)
        {
            // Force MSBuild to not strip away the assembly
            var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

            var assemblies = assemblyLocations
                ?.Where(x => File.Exists(x))
                ?.Select(x => Assembly.ReflectionOnlyLoadFrom(x))
                ?.ToList() ?? new List<Assembly>();
            return GetAutoCompleteAsync(assemblies, code, position);
        }

        private async Task<IEnumerable<IDynamicCodeCompletionData>> GetAutoCompleteAsync(List<Assembly> assemblies, string code, int position)
        {
            var cancellationToken = new CancellationToken();
            var data = new List<DynamicCodeExecutionAutoCompletionData>();
            var ws = new AdhocWorkspace();
            var project = ws.AddProject("Sample", "C#");

            //Add reference to assemblies
            foreach (var assembly in assemblies)
            {
                XmlDocumentationProvider docProvider = null;
                var xmlDocPath = Path.Combine(Directory.GetParent(assembly.Location).FullName, Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");
                if (File.Exists(xmlDocPath))
                {
                    docProvider = XmlDocumentationProvider.CreateFromFile(xmlDocPath);
                }
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(assembly.Location, documentation: docProvider));
            }
            ws.TryApplyChanges(project.Solution);

            //Add document to project
            var sourceText = SourceText.From(code);
            var doc = ws.AddDocument(project.Id, "NewDoc", sourceText);

            // Find symbols at position
            var semanticModel = await doc.GetSemanticModelAsync();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, position, ws, null, cancellationToken);
            var partialText = GetTextBeforeUpToFirstNeedle(code, position, new[] { ';', '.' }, new[] { '(' })?.ToLower()?.Trim() ?? String.Empty;
            symbols = symbols.Where(s => s.Name.ToLower().StartsWith(partialText));

            foreach (var symbol in symbols)
            {
                var items = CreateCompletionDataFor(symbol, semanticModel, position);
                data.AddRange(items);
            }

            return data;
        }

        private List<DynamicCodeExecutionAutoCompletionData> CreateCompletionDataFor(ISymbol symbol, SemanticModel semanticModel, int position)
        {
            var items = new List<DynamicCodeExecutionAutoCompletionData>();

            var xmlDoc = symbol.GetDocumentationCommentXml() ?? String.Empty;
            var kind = GetSymbolKind(symbol);
            
            if (symbol is INamedTypeSymbol nts)
            {
                if (nts.Constructors.Length == 0)
                {
                    items.Add(CreateMethodCompletionData(symbol, null, semanticModel, position, kind, xmlDoc, true));
                }

                foreach (var ctr in nts.Constructors)
                {
                    items.Add(CreateMethodCompletionData(symbol, ctr.Parameters, semanticModel, position, kind, xmlDoc, true));
                }
            }
            else if(symbol is IMethodSymbol ms)
            {
                items.Add(CreateMethodCompletionData(symbol, ms.Parameters, semanticModel, position, kind, xmlDoc, true));
            }
            else
            {
                items.Add(CreateMethodCompletionData(symbol, null, semanticModel, position, kind, xmlDoc, false));
            }

            return items;
        }

        private DynamicCodeExecutionAutoCompletionData CreateMethodCompletionData(ISymbol symbol, IEnumerable<IParameterSymbol> parameters, SemanticModel semanticModel, 
            int position, string kind, string xmlDoc, bool isMethod)
        {
            var name = symbol.Name;
            var ns = symbol.ContainingNamespace.Name;
            if (!String.IsNullOrWhiteSpace(ns))
            {
                ns += ".";
            }

            var access = symbol.DeclaredAccessibility.ToString().ToLower();
            var typename = GetTypeName(symbol);

            var hasParams = (parameters != null && parameters.Any());
            var parameterString = (hasParams)
                ? "(" + String.Join(", ", parameters.Select(x => x.ToMinimalDisplayString(semanticModel, position))) + ")"
                : String.Empty;

            var doc = $"{ns}{name}{parameterString}\n{access} {typename}";
            var label = $"{name}{parameterString}";
            var insert = $"{symbol.Name}" + (isMethod ? "(" : String.Empty) + ((isMethod && !hasParams) ? ")" : String.Empty);
            return new DynamicCodeExecutionAutoCompletionData(kind, label, doc, xmlDoc, insert);
        }

        private string GetTypeName(ISymbol symbol)
        {
            string kind = "Unknown";

            if (symbol is IFieldSymbol)
            {
                kind = "Field";
            }
            else if (symbol is IPropertySymbol)
            {
                kind = "Property";
            }
            else if (symbol is INamespaceSymbol)
            {
                kind = "Namespace";
            }
            else if (symbol is IMethodSymbol)
            {
                kind = "Method";
            }
            else if (symbol is INamedTypeSymbol)
            {
                kind = "Class";
            }
            else if (symbol is Microsoft.CodeAnalysis.IArrayTypeSymbol)
            {
                kind = "Array";
            }
            return kind;
        }

        private string GetSymbolKind(ISymbol symbol)
        {
            const string prefix = "monaco.languages.CompletionItemKind.";
            string kind = "Reference";

            if (symbol is IFieldSymbol)
            {
                kind = "Field";
            }
            else if (symbol is IPropertySymbol)
            {
                kind = "Property";
            }
            else if (symbol is INamespaceSymbol)
            {
                kind = "Folder";
            }
            else if (symbol is IMethodSymbol)
            {
                kind = "Function";
            }
            else if (symbol is INamedTypeSymbol)
            {
                kind = "Class";
            }
            else if (symbol is Microsoft.CodeAnalysis.IArrayTypeSymbol)
            {
                kind = "Value";
            }
            return $"{prefix}{kind}";
        }

        private string GetTextBeforeUpToFirstNeedle(string code, int position, char[] needles, char[] endNeedles)
        {
            var needlePos = GetLastPositionOfAnyNeedleBefore(code, position, needles);
            if (needlePos == -1)
            {
                return null;
            }
            else
            {
                var text = code.Substring(needlePos + 1, position - needlePos - 1);
                if (endNeedles != null && endNeedles.Length > 0)
                {
                    var endIndex = text.IndexOfAny(endNeedles);
                    if (endIndex > -1)
                    {
                        text = text.Substring(0, endIndex);
                    }
                }
                return text;
            }
        }

        private int GetLastPositionOfAnyNeedleBefore(string text, int position, params char[] needles)
        {
            text = text.Substring(0, Math.Min(text.Length, position));
            return text.LastIndexOfAny(needles);
        }
    }
}
