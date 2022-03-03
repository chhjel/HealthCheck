using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Utilities for any object.
    /// </summary>
    public static class HCObjectUtils
	{
		// Relative or absolute url.
		private static readonly Regex _urlRegex
			= new(@"(https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}|\/)\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
		private static readonly Regex _absoluteUrlRegex
			= new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
		private static readonly string[] _imgExtensions = new[]
			{ "APNG", "AVIF", "GIF", "JPG", "JPEG", "PNG", "SVG", "WEBP", "BMP", "ICO", "TIFF" };
		private static readonly Type[] _stringifiableTypes = new[]
			{ typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid),
		typeof(DateTime?), typeof(DateTimeOffset?), typeof(Guid?) };

		/// <summary>
		/// Attempts to create a html summary of the given object property values.
		/// <para>To ignore properties apply <see cref="HCExcludeFromHtmlSummaryAttribute"/> to them.</para>
		/// <para>Ignores any exception.</para>
		/// </summary>
		public static string TryCreateHtmlSummaryFromProperties(object obj, bool spacifyPropertyNames = true, bool tryParseUrls = true)
		{
			if (obj == null) return null;

			var builder = new StringBuilder();

			var props = obj.GetType().GetProperties()
				.Where(x => x.CanRead);
			foreach (var prop in props)
			{
				try
				{
					if (prop.GetCustomAttribute<HCExcludeFromHtmlSummaryAttribute>() != null)
					{
						continue;
					}

					var rawValue = prop.GetValue(obj);
					var type = DeterminePropertyType(prop, rawValue);
					var value = string.Empty;
					var valueOnNewLine = true;

					if (rawValue != null)
					{
						if (type == ObjectPropertyType.Stringifiable || type == ObjectPropertyType.Unknown)
						{
							value = HttpUtility.HtmlEncode(rawValue.ToString());
							valueOnNewLine = false;
						}
						else if (type == ObjectPropertyType.StringifiableDictionary && rawValue is IDictionary dict)
						{
							var keys = dict.Keys;
							foreach (var key in keys)
							{
								value += $" * <b>{HttpUtility.HtmlEncode(key)}:</b> {HttpUtility.HtmlEncode(dict[key] ?? "")}<br />";
							}
						}
						else if (type == ObjectPropertyType.StringifiableEnumerable && rawValue is IEnumerable enumerable)
						{
							foreach (var val in enumerable)
							{
								value += $" * {HttpUtility.HtmlEncode(val)}<br />";
							}
						}
						else if (type == ObjectPropertyType.Exception && rawValue is Exception ex)
						{
							value = $"<code>{HttpUtility.HtmlEncode(HCExceptionUtils.GetFullExceptionDetails(ex))}</code>";
						}
					}

					var name = spacifyPropertyNames ? prop.Name.SpacifySentence() : prop.Name;
					builder.AppendLine($"<b>{name}</b>:{(valueOnNewLine ? "<br />" : " ")}{value}");

					if (tryParseUrls)
					{
						var urls = ParseUrls(value);
						if (urls?.Any() == true)
						{
							builder.AppendLine("<br />");
							foreach (var url in urls)
							{
								if (IsImageUrl(url))
								{
									builder.AppendLine($"<img src=\"{url}\" width=\"200\" /><br />");
								}
								else
								{
									builder.AppendLine($" * <a href=\"{url}\">{url}</a><br />");
								}
							}
						}
					}

					builder.AppendLine("<br />");
				}
                catch (Exception) {}
			}

			return builder.ToString();
		}

		private static ObjectPropertyType DeterminePropertyType(PropertyInfo prop, object value)
		{
			if (prop == null) return ObjectPropertyType.Unknown;

            static bool isStringifiable(Type t, object v)
				=> t.IsPrimitive || _stringifiableTypes.Contains(t) || v is string;

			var type = prop.PropertyType;
			if (isStringifiable(type, value))
			{
				return ObjectPropertyType.Stringifiable;
			}
			else if (value is Exception)
			{
				return ObjectPropertyType.Exception;
			}
			else if (type.IsGenericType
				&& type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
				&& isStringifiable(type.GetGenericArguments()[1], null))
			{
				return ObjectPropertyType.StringifiableDictionary;
			}
			else if (type.IsGenericType
				&& typeof(IEnumerable).IsAssignableFrom(type)
				&& isStringifiable(type.GetGenericArguments()[0], null))
			{
				return ObjectPropertyType.StringifiableEnumerable;
			}

			return ObjectPropertyType.Unknown;
		}

		private static List<string> ParseUrls(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return new List<string>();
			var absoluteUrls = _absoluteUrlRegex.Matches(value).OfType<Match>().Select(x => x.Value).ToArray();
			var relativeUrlMatch = _urlRegex.Match(value);

			var urls = new List<string>();
			if (absoluteUrls.Any())
			{
				urls.AddRange(absoluteUrls);
			}
			else if (relativeUrlMatch.Success)
			{
				urls.Add(relativeUrlMatch.Value);
			}
			return urls
				.Where(x => x.Length > 6)
				.Select(x => x.Replace("&quot", "").Trim('"', ' '))
				.ToList();
		}

		private static bool IsImageUrl(string url)
			=> _imgExtensions.Any(ext => url.ToLower().EndsWith($".{ext.ToLower()}"));

		private enum ObjectPropertyType
		{
			Unknown,
			Stringifiable,
			StringifiableEnumerable,
			StringifiableDictionary,
			Exception
		}
	}
}
