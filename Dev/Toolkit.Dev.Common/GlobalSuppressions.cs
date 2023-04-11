// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Dev files.")]
[assembly: SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Dev files.")]
[assembly: SuppressMessage("Major Bug", "S2201:Return values from functions without side effects should not be ignored", Justification = "Dev files.")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Dev files.", Scope = "member", Target = "~M:QoDL.Toolkit.Dev.Common.Tests.ClassProxyTests.SomeService.WithInheritedReferenceTypes1(QoDL.Toolkit.Dev.Common.Tests.ClassProxyTests.SomeParameterType,QoDL.Toolkit.Dev.Common.Tests.ClassProxyTests.SomeOtherParameterType,QoDL.Toolkit.Dev.Common.Tests.ClassProxyTests.SomeOtherSubParameterType)~QoDL.Toolkit.Dev.Common.Tests.ClassProxyTests.SomeOtherParameterType")]
