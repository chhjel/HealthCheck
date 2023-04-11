using QoDL.Toolkit.Core.Modules.MappedData.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Modules.MappedData.Utils
{
    internal static class TKMappedDataMappingParser
	{
		private static readonly Regex _propLineRegex = new(@"(?<name>\w+)\s*(?<arrow><=>)?\s*(?<mappedTo>\[?.+\]?)?");

		public static ParsedMapping ParseMapping(Type type, string mapping, List<TKMappedReferencedTypeDefinition> refDefs)
		{
			var parsed = new ParsedMapping()
			{
				Type = type
			};
			var lines = mapping.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim());

			string nextComment = null;
			ParsedMappingObject currentObject = null;
			var currentType = type;
			void setCurrentObject(ParsedMappingObject newObj)
			{
				currentObject = newObj;
				currentType = newObj == null ? type : newObj?.Type;
			}

			foreach (var lineRaw in lines)
			{
				var line = lineRaw;

				// Comment
				if (line.StartsWith("//"))
				{
					nextComment = line.TrimStart('/').Trim();
					continue;
				}
				// End of object
				else if (line == "}")
				{
					nextComment = null;
					setCurrentObject(currentObject?.Parent);
					continue;
				}

				if (line.Contains("//"))
				{
					var parts = line.Split(new string[] { "//" }, 2, StringSplitOptions.RemoveEmptyEntries);
					line = parts[0].Trim();
					nextComment = parts[1].Trim();
				}

				var match = _propLineRegex.Match(line);
				if (match.Success)
				{
					var name = match.Groups["name"]?.Value;
					var arrow = match.Groups["arrow"]?.Value;
					var mappedToRaw = match.Groups["mappedTo"]?.Value;
					if (mappedToRaw?.StartsWith("[") == true && mappedToRaw?.EndsWith("]") == true)
                    {
						mappedToRaw = mappedToRaw.Substring(1, mappedToRaw.Length - 2);
                    }
					var mappedToValues = mappedToRaw
						?.Split(',')
						?.Select(x => x.Trim())
						?.Where(x => !string.IsNullOrWhiteSpace(x))
						?.ToList() ?? new List<string>();
					var hasBrace = mappedToValues.Count > 0 && mappedToValues.Last().Trim().EndsWith("{");
					if (hasBrace)
                    {
						mappedToValues[mappedToValues.Count - 1] = mappedToValues[mappedToValues.Count - 1].Trim().TrimEnd('{').Trim();
						mappedToValues = mappedToValues.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
					}
					var property = currentType?.GetProperty(name);

					var mappedToRefs = new List<ParsedMappedToReference>();
					foreach (var mappedPath in mappedToValues)
					{
						if (mappedPath.StartsWith("\""))
						{
							mappedToRefs.Add(new ParsedMappedToReference
							{
								HardCodedValue = mappedPath
							});
							continue;
						}

						var parts = mappedPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
						var mappedToRefId = parts[0].Trim();
						var refDef = refDefs.FirstOrDefault(x => x.ReferenceId == mappedToRefId);
						TypeChain chain = null;
						var subPath = string.Join(".", parts.Skip(1));
						if (refDef != null)
						{
							chain = ResolveDottedPath(refDef?.Type, subPath);
						}
						else
						{
							chain = new TypeChain
							{
								Success = false,
								Error = $"Type not found from reference id '{mappedToRefId}'.",
								Path = subPath
							};
						}
						mappedToRefs.Add(new ParsedMappedToReference
						{
							Chain = chain,
							DottedPath = mappedPath,
							Name = parts.Last().Trim()
						});
					}

					var obj = new ParsedMappingObject
					{
						Name = name,
						Comment = nextComment,
						MappedTo = mappedToRefs,
						Parent = currentObject,
						PropertyInfo = property,
						Type = property?.PropertyType,
						IsValid = property != null,
						Error = property == null ? $"Property '{name}' not found." : null
					};
					createObject(obj, hasBrace);
				}
			}

			void createObject(ParsedMappingObject obj, bool? hasBrace = null)
			{
				nextComment = null;

				if (currentObject != null)
				{
					currentObject.Children.Add(obj);
				}
				else
				{
					parsed.Objects.Add(obj);
				}

				if (hasBrace == true) setCurrentObject(obj);
			}

			return parsed;
		}

		private static TypeChain ResolveDottedPath(Type rootType, string path)
		{
			var chain = new TypeChain()
			{
				Path = path,
				RootType = rootType
			};
			var parts = path.Trim().Split('.');
			Type currentType = rootType;
			foreach (var pathSegment in parts)
			{
				var propData = TKReflectionUtils.GetProperty(currentType, pathSegment);
				var prop = propData.Item1;
				var underlyingType = propData.Item2;
				//var prop = currentType?.GetProperty(pathSegment);
				string error = null;
				if (currentType != null && prop == null)
				{
					error = $"Could not find property named '{pathSegment}' on type '{currentType?.Name}'";
				}

				//currentType = prop?.PropertyType;
				currentType = underlyingType ?? prop?.PropertyType;
				chain.Items.Add(new TypeChainItem
				{
					Name = pathSegment,
					PropertyInfo = prop,
					Error = error
				});
			}

			chain.Success = chain.GetErrors().Count() == 0;
			return chain;
		}

		public class TypeChain
		{
			public bool Success { get; set; }
			public string Error { get; set; }
			public string Path { get; set; }
			public Type RootType { get; set; }
			public IEnumerable<string> GetErrors() => Items.Select(x => x.Error).Union(new[] { Error }).Where(x => !string.IsNullOrWhiteSpace(x));
			public List<TypeChainItem> Items { get; set; } = new List<TypeChainItem>();
		}

		public class TypeChainItem
		{
			public bool Success => Error == null;
			public string Error { get; set; }

			public string Name { get; set; }
			public Type DeclaringType => PropertyInfo?.DeclaringType;
			public PropertyInfo PropertyInfo { get; set; }
		}

		public class ParsedMapping
		{
			public Type Type { get; set; }
			public List<ParsedMappingObject> Objects { get; set; } = new List<ParsedMappingObject>();

			public override string ToString()
			{
				var builder = new StringBuilder();
				foreach (var obj in Objects)
				{
					builder.AppendLine(obj.ToString());
				}
				return builder.ToString();
			}
		}

		public class ParsedMappedToReference
		{
			public string Name { get; set; }
			public string DottedPath { get; set; }
			public TypeChain Chain { get; set; }
			public string HardCodedValue { get; set; }
			public bool IsHardCoded => HardCodedValue != null;

			public string CreateSummary()
			{
				if (IsHardCoded)
				{
					return $"{Name} = {HardCodedValue}";
				}
				var errors = string.Join(", ", Chain.GetErrors());
				if (!string.IsNullOrWhiteSpace(errors)) errors = $" ({errors})";
				return $"{Name} ({DottedPath}){errors}";
			}
		}

		public class ParsedMappingObject
		{
			public string Name { get; set; }
			public Type Type { get; set; }
			public ParsedMappingObject Parent { get; set; }
			public string Comment { get; set; }
			public bool IsComplex => Children.Any();
			public PropertyInfo PropertyInfo { get; set; }
			public List<ParsedMappedToReference> MappedTo { get; set; } = new List<ParsedMappedToReference>();
			public List<ParsedMappingObject> Children { get; set; } = new List<ParsedMappingObject>();

			public bool IsValid { get; set; }
			public string Error { get; set; }

			public override string ToString() => CreateSummary(0);

			private string CreateSummary(int level)
			{
				var prefix = new string(' ', level * 2);
				var value = prefix;
				if (!string.IsNullOrWhiteSpace(Comment)) value += $"// {Comment}\n{prefix}";

				value += $"{Name} [{(IsValid ? "Valid" : Error)}]";

				if (MappedTo.Any())
				{
					value += $" <=> [{string.Join(", ", MappedTo.Select(x => x.CreateSummary()))}]";
				}
				if (IsComplex)
				{
					value += " {";

					foreach (var child in Children)
					{
						value += $"\n{child.CreateSummary(level + 1)}";
					}

					value += $"\n{prefix}}}";
				}

				return value;
			}
		}
	}

}
