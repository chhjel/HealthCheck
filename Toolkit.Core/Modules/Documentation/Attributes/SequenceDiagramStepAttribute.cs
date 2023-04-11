using QoDL.Toolkit.Core.Modules.Documentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Attributes;

/// <summary>
/// This attribute will be detected by <see cref="DefaultSequenceDiagramService"/> and used in a generated sequence diagram.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SequenceDiagramStepAttribute : Attribute
{
    /// <summary>
    /// Name of the diagram.
    /// </summary>
    public string DiagramName { get; private set; }

    /// <summary>
    /// Current step name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The process from the previous step to this one.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Optional note for this step.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Optional remark for this step.
    /// </summary>
    public string Remarks { get; set; }

    /// <summary>
    /// Name of a class where the next step is if any. Use nameof to stay typesafe.
    /// </summary>
    public string NextClass { get; set; }

    /// <summary>
    /// Name of a method within <see cref="NextClass"/> where the next step is if any. Use nameof to stay typesafe.
    /// </summary>
    public string NextMethod { get; set; }

    /// <summary>
    /// Groups this step in an optional-group if with the given name.
    /// </summary>
    public string OptionalGroupName { get; set; }

    /// <summary>
    /// Optional category to place this diagram in.
    /// </summary>
    public string UICategory { get; set; }
    
    internal List<string> Branches { get; private set; }
    internal string ClassName { get; set; }
    internal string MethodName { get; set; }
    internal Type Class { get; set; }
    internal MethodInfo Method { get; set; }
    internal SequenceDiagramStepAttribute Next { get; set; }
    internal SequenceDiagramStepAttribute Previous { get; set; }

    /// <summary>
    /// This attribute will be detected by <see cref="DefaultSequenceDiagramService"/> and used in a generated sequence diagram.
    /// </summary>
    /// <param name="diagramName">Name of the diagram.</param>
    /// <param name="name">Current step name. (Frontend/Web/XService etc)</param>
    /// <param name="description">The process from the previous step to this one.</param>
    /// <param name="note">Optional note for this step.</param>
    /// <param name="remarks">Optional remark for this step.</param>
    /// <param name="optionalGroupName">Groups this step in an optional-group if with the given name.</param>
    /// <param name="uiCategory">Optional category to place this diagram in.</param>
    /// <param name="branches">
    /// Any extra branch names given here will result in a new diagram containing all the
    /// <para>steps in <paramref name="diagramName"/> + any steps with the same branch name.</para>
    /// </param>
    /// <param name="includeInMainBranch">
    /// If set to true the main branch (<paramref name="diagramName"/>) will include this step.
    /// <para>Only has effect if <paramref name="branches"/> is given.</para>
    /// </param>
    /// <param name="nextClass">Name of a class where the next step is if any. Use nameof to stay typesafe.</param>
    /// <param name="nextMethod">Name of a method within <see cref="NextClass"/> where the next step is if any. Use nameof to stay typesafe.</param>
    public SequenceDiagramStepAttribute(
        string diagramName,
        string name,
        string description = null,
        string note = null,
        string remarks = null,
        string optionalGroupName = null,
        string uiCategory = null,
        string[] branches = null,
        bool includeInMainBranch = false,
        string nextClass = null,
        string nextMethod = null
    )
    {
        DiagramName = diagramName;
        Branches = branches?.ToList() ?? new List<string>();
        if (includeInMainBranch || !Branches.Any())
        {
            Branches.Add(diagramName);
        }
        Name = name;
        Description = description;
        Note = note;
        Remarks = remarks;
        OptionalGroupName = optionalGroupName;
        UICategory = uiCategory;
        NextClass = nextClass;
        NextMethod = nextMethod;
    }

    internal void SetNext(SequenceDiagramStepAttribute other)
    {
        Next = other;
        other.Previous = this;
    }

    internal void SetPrevious(SequenceDiagramStepAttribute other)
    {
        Previous = other;
        other.Next = this;
    }
}
