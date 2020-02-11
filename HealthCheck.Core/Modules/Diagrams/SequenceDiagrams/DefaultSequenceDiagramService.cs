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
        public List<SequenceDiagram> Generate(IEnumerable<Assembly> sourceAssemblies)
        {
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
                        .Where(x => x.Branches.Contains(diagram.Name) || x.Branches.Contains(x.DiagramId))
                        .ToArray();

                    for (int i = 1; i < attributes.Length; i++)
                    {
                        var a = attributes[i - 1];
                        var b = attributes[i];
                        diagram.Steps.Add(new SequenceDiagramStep()
                        {
                            Index = i - 1,
                            Branches = b.Branches,
                            From = a.Name,
                            To = b.Name,
                            Description = b.Description,
                            Note = b.Note,
                            Remarks = b.Remarks,
                            OptionalId = b.OptionalId
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
                    if (groups.ContainsKey(atr.DiagramId))
                    {
                        group = groups[atr.DiagramId];
                    }
                    else
                    {
                        group = new SequenceDiagramAttributeGroup()
                        {
                            DiagramId = atr.DiagramId,
                            Method = method.Method,
                            Attributes = new List<SequenceDiagramStepAttribute>()
                        };
                        groups[atr.DiagramId] = group;
                    }

                    atr.ClassName = method.Method.DeclaringType.Name;
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
                    if (atr.NextId != null)
                    {
                        var match = group.Attributes.FirstOrDefault(x =>
                            atr.DiagramId == x.DiagramId
                            && atr.NextId == $"{x.ClassName}.{x.MethodName}");
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
