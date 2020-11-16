#if NETFULL
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Util;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Module.DynamicCodeExecution.PreProcessors
{
    /// <summary>
    /// Uses regex and reflection to detect missing using statements and prepends them.
    /// </summary>
    public class BasicAutoCreateUsingsPreProcessor : IDynamicCodePreProcessor
    {
        /// <summary>
        /// Id of the pre-processor used to disable it from the code.
        /// </summary>
        public string Id { get; set; } = "BasicAutoCreateUsings";

        /// <summary>
        /// Optional title returned in the options model.
        /// </summary>
        public string Name { get; set; } = "Attempt to auto-create missing using statements.";

        /// <summary>
        /// Optional description returned in the options model.
        /// </summary>
        public string Description { get; set; } = "Using regex/reflection and is not totally accurate.";

        /// <summary>
        /// Allow the user to disable the pre-processor from the code?
        /// </summary>
        public bool CanBeDisabled { get; set; } = true;

        /// <summary>
        /// List of namespaces to never auto-create.
        /// </summary>
        public List<NamespaceConfig> IgnoreNamespaces { get; set; } = new List<NamespaceConfig>();

        /// <summary>
        /// List of namespaces to check first.
        /// </summary>
        public List<NamespaceConfig> PrioritizeNamespaces { get; set; } = new List<NamespaceConfig>();

        /// <summary>
        /// List of assemblies to check types/namespaces in.
        /// </summary>
        public IEnumerable<Assembly> TargetAssemblies { get; set; }

        /// <summary>
        /// If true, also include any referenced assemblies of the target assemblies.
        /// </summary>
        public bool IncludeReferencedAssemblies { get; set; }

        /// <summary>
        /// Uses regex and reflection to detect missing using statements and prepends them.
        /// </summary>
        public BasicAutoCreateUsingsPreProcessor(IEnumerable<Assembly> targetAssemblies)
        {
            TargetAssemblies = targetAssemblies;
        }

        /// <summary>
        /// Uses regex and reflection to detect missing using statements and prepends them.
        /// </summary>
        public BasicAutoCreateUsingsPreProcessor(Assembly targetAssembly)
        {
            TargetAssemblies = new Assembly[] { targetAssembly };
        }

        /// <summary>
        /// Namespace and recursive option
        /// </summary>
        public class NamespaceConfig
        {
            /// <summary>
            /// Target namespace
            /// </summary>
            public string Namespace { get; set; }

            /// <summary>
            /// Include child namespaces
            /// </summary>
            public bool Recursive { get; set; }

            /// <summary>
            /// Namespace and recursive option
            /// </summary>
            public NamespaceConfig(string ns, bool recursive)
            {
                Namespace = ns;
                Recursive = recursive;
            }

            /// <summary>
            /// Does the given namespace match this rule?
            /// </summary>
            /// <param name="ns">Given namespace</param>
            public bool Matches(string ns)
            {
                ns ??= string.Empty;
                return Recursive ? ns.StartsWith(Namespace) : ns == Namespace;
            }
        }

        /// <summary>
        /// Uses regex and reflection to detect missing using statements and prepends them.
        /// </summary>
        public string PreProcess(CompilerParameters options, string code)
        {
            if (code == null || code.Trim().Length == 0)
                return code;

            var suggestedNamespaces = GetSuggestedNamespaces(code, GetTargetAssemblies());
            if (suggestedNamespaces.Count > 0)
            {
                code = string.Join("\n", suggestedNamespaces.Select(x => $"using {x};")) + "\n" + code;
            }
            return code;
        }

        /// <summary>
        /// Add a namespaces to ignore.
        /// </summary>
        /// <param name="ns">Target namespace</param>
        /// <param name="recursive">Include all child namespaces</param>
        public BasicAutoCreateUsingsPreProcessor AddIgnoredNamespace(string ns, bool recursive)
        {
            IgnoreNamespaces.Add(new NamespaceConfig(ns, recursive));
            return this;
        }

        /// <summary>
        /// Add a list of namespaces to ignore.
        /// </summary>
        /// <param name="nsList">Target namespaces</param>
        /// <param name="recursive">Include all child namespaces</param>
        public BasicAutoCreateUsingsPreProcessor AddIgnoredNamespaces(IEnumerable<string> nsList, bool recursive)
        {
            foreach (var ns in nsList)
            {
                AddIgnoredNamespace(ns, recursive);
            }
            return this;
        }

        /// <summary>
        /// Add a namespaces to check first.
        /// </summary>
        /// <param name="ns">Target namespace</param>
        /// <param name="recursive">Include all child namespaces</param>
        public BasicAutoCreateUsingsPreProcessor AddPrioritizedNamespace(string ns, bool recursive)
        {
            PrioritizeNamespaces.Add(new NamespaceConfig(ns, recursive));
            return this;
        }

        /// <summary>
        /// Add a list of namespaces to check first.
        /// </summary>
        /// <param name="nsList">Target namespaces</param>
        /// <param name="recursive">Include all child namespaces</param>
        public BasicAutoCreateUsingsPreProcessor AddPrioritizedNamespaces(IEnumerable<string> nsList, bool recursive)
        {
            foreach(var ns in nsList)
            {
                AddPrioritizedNamespace(ns, recursive);
            }
            return this;
        }

        private List<Assembly> GetTargetAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(TargetAssemblies);

            if (IncludeReferencedAssemblies)
            {
                foreach(var item in TargetAssemblies)
                {
                    list.AddRange(GetReferencedAssemblies(item));
                }
            }

            return list
                .Distinct()
                .ToList();
        }

        private List<string> GetSuggestedNamespaces(string code, IEnumerable<Assembly> assemblies)
        {
            var existingUsings = GetNamespacesInUsingsIn(code);

            var parser = new CSharpParser();
            var parseResult = parser.Parse(code);

            var requiredNamespaces = new HashSet<string>();

            var types = assemblies
                .SelectMany(x => x.DefinedTypes)
                .Where(x => IgnoreNamespaces == null || !IgnoreNamespaces.Any(n => n.Matches(x.Namespace)))
                .ToList();

            if (PrioritizeNamespaces != null)
            {
                var priList = PrioritizeNamespaces.AsEnumerable().Reverse();
                foreach(var priNamespace in priList)
                {
                    var prioritized = types.Where(x => priNamespace.Matches(x.Namespace)).ToArray();
                    foreach (var p in prioritized)
                    {
                        types.Remove(p);
                    }
                    types.InsertRange(0, prioritized);
                }
            }

            foreach (var type in parseResult.Types)
            {
                TypeInfo matchingType = null;
                if (type.Contains("<"))
                {
                    var typePrefix = type.Substring(0, type.IndexOf("<")) + "`";
                    matchingType = types.FirstOrDefault(x => x.Name.StartsWith(typePrefix));
                }
                else
                {
                    matchingType = types.FirstOrDefault(x => x.Name == type);
                }
                if (matchingType == null)
                {
                    continue;
                }

                requiredNamespaces.Add(matchingType.Namespace);
            }

            foreach (var method in parseResult.Methods)
            {
                var matchingType = types.FirstOrDefault(x => x.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Any(m => m.Name == method));
                if (matchingType == null)
                {
                    continue;
                }

                requiredNamespaces.Add(matchingType.Namespace);
            }

            requiredNamespaces.RemoveWhere(x => existingUsings.Contains(x));
            return requiredNamespaces
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        private List<string> GetNamespacesInUsingsIn(string code)
        {
            return code
                .Replace("\r", "")
                .Split('\n')
                .Where(x => x.Trim().StartsWith("using ") && x.Trim().EndsWith(";"))
                .Select(x => x.Substring(6))
                .Select(x => x.Replace(";", ""))
                .Select(x => x.Trim())
                .ToList();
        }

        private List<Assembly> GetReferencedAssemblies(Assembly assembly, bool applyPolicy = false)
        {
            var list = new HashSet<Assembly>();

            foreach (var refAssemblyName in assembly.GetReferencedAssemblies())
            {
                var ass = applyPolicy
                    ? GetAssemblyWithPolicy(refAssemblyName.FullName)
                    : Assembly.Load(refAssemblyName.FullName);
                list.Add(ass);
            }

            return list.ToList();
        }

        private Assembly GetAssemblyWithPolicy(string assemblyName)
        {
            var newName = AppDomain.CurrentDomain?.ApplyPolicy(assemblyName);
            return Assembly.Load(newName);
        }
    }
}
#endif
