using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Utilities related to permutation of data.
/// </summary>
public static class TKPermutationUtils
{
    /// <summary>
    /// Create all permutations of the given type.
    /// <para>Supported property types: enum, boolean</para>
    /// <para>Be sure to not use too many properties if you want to keep performance up.</para>
    /// </summary>
    public static List<T> CreatePermutationsOf<T>() where T : class, new()
    {
        var type = typeof(T);
        return CreatePermutationsOf(type).OfType<T>().ToList();
    }

    /// <summary>
    /// Create all permutations of the given type.
    /// <para>Supported property types: enum, boolean</para>
    /// <para>Be sure to not use too many properties if you want to keep performance up.</para>
    /// </summary>
    public static List<object> CreatePermutationsOf(Type type)
{
        // Build available actions from supported property types
        var propertyPermutations = new List<PossiblePropertyPermutation>();
        foreach (var prop in type.GetProperties())
        {
            var permutation = CreatePropertyPermutation(prop);
            if (permutation.Choices.Any())
            {
                propertyPermutations.Add(permutation);
            }
        }

        // Calc combinations using cartesian product
        var cartesianProduct = CartesianProduct(propertyPermutations.Select(x => x.Choices));

        // Create instances from the calculated combinations
        var instances = new List<object>();
        foreach (var permutation in cartesianProduct)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var propertyAction in permutation)
            {
                propertyAction(instance);
            }
            instances.Add(instance);
        }

        return instances;
    }

    private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(emptyProduct,
            (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
    }

    private static PossiblePropertyPermutation CreatePropertyPermutation(PropertyInfo prop)
    {
        var permutations = new PossiblePropertyPermutation();
        var type = prop.PropertyType;

        if (type == typeof(bool))
        {
            permutations.Choices.Add((obj) => prop.SetValue(obj, false));
            permutations.Choices.Add((obj) => prop.SetValue(obj, true));
        }
        else if (type.IsEnum)
        {
            foreach (var enm in Enum.GetValues(type))
            {
                permutations.Choices.Add((obj) => prop.SetValue(obj, enm));
            }
        }

        return permutations;
    }
    private class PossiblePropertyPermutation
    {
        public List<Action<object>> Choices { get; set; } = new();
    }
}
