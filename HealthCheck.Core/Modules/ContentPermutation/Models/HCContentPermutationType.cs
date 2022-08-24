using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    public class HCContentPermutationType
    {
        public Type Type { get; set; }
        public string Id { get; set; }
        public List<HCContentPermutationChoice> Permutations { get; set; }
    }
}
