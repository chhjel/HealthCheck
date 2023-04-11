using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class TKJobStartCustomParameters
    {
        /// <summary></summary>
        public Type CustomParametersType { get; set; }

        /// <summary></summary>
        public object CustomParametersInstance { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> CustomParametersRaw { get; set; }

        /// <summary>
        /// Get <see cref="CustomParametersInstance"/> as <typeparamref name="TParameters"/>.
        /// </summary>
        public TParameters GetParametersAs<TParameters>()
            where TParameters : class
            => CustomParametersInstance as TParameters;
    }
}
