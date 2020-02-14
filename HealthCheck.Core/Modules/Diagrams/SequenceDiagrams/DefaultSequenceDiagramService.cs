using HealthCheck.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.Diagrams.SequenceDiagrams
{
    /// <summary>
    /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s.
    /// </summary>
    public class DefaultSequenceDiagramService : ISequenceDiagramService
    {
        private DefaultSequenceDiagramServiceOptions Options { get; }

        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s.
        /// </summary>
        public DefaultSequenceDiagramService(DefaultSequenceDiagramServiceOptions options)
        {
            Options = options ?? new DefaultSequenceDiagramServiceOptions();
        }

        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assemblies.
        /// </summary>
        public List<SequenceDiagram> Generate(params Assembly[] sourceAssemblies)
            => Generate(sourceAssemblies.ToList());

        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assembly.
        /// </summary>
        public List<SequenceDiagram> Generate(Assembly sourceAssembly)
            => Generate(new List<Assembly> { sourceAssembly });

        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assemblies.
        /// </summary>
        public List<SequenceDiagram> Generate(IEnumerable<Assembly> sourceAssemblies = null)
        {
            sourceAssemblies = sourceAssemblies
                ?? Options.DefaultSourceAssemblies
                ?? Enumerable.Empty<Assembly>();

            var attributeGroups = FindAttributes(sourceAssemblies);
            var diagrams = new List<SequenceDiagram>();
            foreach (var group in attributeGroups)
            {
                var groupDiagrams = group.Attributes
                    .SelectMany(x => x.Branches)
                    .Distinct()
                    .ToDictionary(x => x, x => new SequenceDiagram()
                    {
                        Name = x
                    });

                foreach (var kvp in groupDiagrams)
                {
                    var diagram = kvp.Value;
                    var attributes = group.Attributes
                        .Where(x => x.Branches.Contains(diagram.Name) || x.Branches.Contains(x.DiagramName))
                        .ToArray();

                    for (int i = 1; i < attributes.Length; i++)
                    {
                        var fromAtr = attributes[i - 1];
                        var toAtr = attributes[i];

                        if (fromAtr.OptionalGroupName != toAtr.OptionalGroupName && fromAtr.OptionalGroupName != null)
                        {
                            for (int j = i-2; j >= 0; j--)
                            {
                                fromAtr = attributes[j];
                                if (fromAtr.OptionalGroupName == toAtr.OptionalGroupName || fromAtr.OptionalGroupName == null)
                                {
                                    break;
                                }
                            }
                        }

                        diagram.Steps.Add(new SequenceDiagramStep()
                        {
                            Index = i - 1,
                            Branches = toAtr.Branches,
                            From = fromAtr.Name,
                            To = toAtr.Name,
                            Description = toAtr.Description,
                            Note = toAtr.Note,
                            Remarks = toAtr.Remarks,
                            OptionalGroupName = toAtr.OptionalGroupName,
                            ClassNameFrom = fromAtr.Class?.FullName,
                            MethodNameFrom = fromAtr.Method?.ToString(),
                            ClassNameTo = toAtr.Class?.FullName,
                            MethodNameTo = toAtr.Method?.ToString()
                        });
                    }

                    // Postprocess data with remark numbers and directions
                    PostProcessDiagramData(diagram);
                }

                diagrams.AddRange(groupDiagrams
                    .Where(x => x.Value.Steps.Count >= 1)
                    .Select(x => x.Value)
                );
            }

            return diagrams;
        }

        private void PostProcessDiagramData(SequenceDiagram diagram)
        {
            var remarkNumber = 1;
            foreach (var step in diagram.Steps)
            {
                if (step.From == step.To)
                {
                    step.Direction = SequenceDiagramStepDirection.Still;
                }
                else
                {
                    step.Direction = diagram.Steps.First(x => step.To == x.To || step.To == x.From).Index >= step.Index
                        ? SequenceDiagramStepDirection.Forward : SequenceDiagramStepDirection.Backward;
                }

                if (step.Remarks != null)
                {
                    step.RemarkNumber = remarkNumber;
                    diagram.Remarks.Add(new SequenceDiagramRemark()
                    {
                        Number = remarkNumber,
                        Text = step.Remarks
                    });
                    remarkNumber++;
                }
            }
        }

        private List<SequenceDiagramAttributeGroup> FindAttributes(IEnumerable<Assembly> sourceAssemblies)
        {
            var types = sourceAssemblies.SelectMany(x => x.DefinedTypes).ToArray();
            var methods = types.SelectMany(x => x.GetMethods()).ToList();
            var methodAttributes = methods
                .Where(x => x.GetCustomAttributes<SequenceDiagramStepAttribute>().Count() > 0)
                .Select(x => new
                {
                    Method = x,
                    Attributes = x.GetCustomAttributes<SequenceDiagramStepAttribute>().ToList()
                })
                .ToList();

            var groups = new Dictionary<string, SequenceDiagramAttributeGroup>();
            foreach (var method in methodAttributes)
            {
                for (int i = 0; i < method.Attributes.Count; i++)
                {
                    var atr = method.Attributes[i];
                    SequenceDiagramAttributeGroup group = null;
                    if (groups.ContainsKey(atr.DiagramName))
                    {
                        group = groups[atr.DiagramName];
                    }
                    else
                    {
                        group = new SequenceDiagramAttributeGroup()
                        {
                            DiagramId = atr.DiagramName,
                            Method = method.Method,
                            Attributes = new List<SequenceDiagramStepAttribute>()
                        };
                        groups[atr.DiagramName] = group;
                    }

                    atr.Class = method.Method.DeclaringType;
                    atr.ClassName = method.Method.DeclaringType.Name;
                    atr.Method = method.Method;
                    atr.MethodName = method.Method.Name;

                    var previousAttributeOnSameMethod = group.Attributes
                        .LastOrDefault(x => x.ClassName == atr.ClassName && x.MethodName == atr.MethodName);
                    if (previousAttributeOnSameMethod != null)
                    {
                        atr.SetPrevious(previousAttributeOnSameMethod);
                    }

                    group.Attributes.Add(atr);
                }
            }

            var attributeGroups = groups
                .Select(x => x.Value)
                .ToList();

            foreach (var group in attributeGroups)
            {
                foreach (var atr in group.Attributes)
                {
                    if (atr.NextClass != null)
                    {
                        var match = group.Attributes.FirstOrDefault(x =>
                            atr.DiagramName == x.DiagramName
                            && atr.NextClass == x.ClassName && atr.NextMethod == x.MethodName);

                        if (match != null)
                        {
                            atr.SetNext(match);
                        }
                    }
                }
            }

            // Order steps
            foreach (var group in attributeGroups)
            {
                var start = group.Attributes.FirstOrDefault(x => x.Previous == null);
                if (start == null)
                {
                    group.Attributes.Clear();
                    continue;
                }

                var newList = new List<SequenceDiagramStepAttribute>()
            {
                start
            };
                var current = start;
                while (current != null)
                {
                    current = current.Next;
                    if (current != null)
                    {
                        newList.Add(current);
                    }
                }

                group.Attributes = newList;
            }

            return attributeGroups
                .Where(x => x.Attributes.Count > 0)
                .ToList();
        }
    }
}
