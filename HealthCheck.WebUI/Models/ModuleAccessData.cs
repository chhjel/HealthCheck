﻿using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Models
{
    internal class ModuleAccessData<TAccessRole>
    {
        public TAccessRole Roles { get; set; }
        public Type AccessOptionsType { get; set; }
        public object AccessOptions { get; set; }
        public bool FullAccess { get; set; }

        public List<object> GetAllSelectedAccessOptions()
        {
            if (FullAccess)
            {
                return Enum.GetValues(AccessOptionsType)
                    .OfType<object>()
                    .Where(x => (int)x != 0)
                    .ToList();
            }
            else if (AccessOptions == null)
            {
                return new List<object>();
            }
            else
            {
                return EnumUtils.GetFlaggedEnumValues(AccessOptions);
            }
        }
    }
}