using QoDL.Toolkit.Core.Util;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Extensions
{
    public class TKReflectionUtilsTests
    {
        public ITestOutputHelper Output { get; }

        public TKReflectionUtilsTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GetTypeMembersRecursive_WithMaxDepth1_OnlyReturnsRootLevelMembers()
        {
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 1);
            Assert.Contains(nameof(TestSubjectRoot.PropRoot1), members.Select(x => x.Name));
            Assert.Contains(nameof(TestSubjectRoot.PropRoot2), members.Select(x => x.Name));
            Assert.Contains(nameof(TestSubjectRoot.Level2Prop), members.Select(x => x.Name));
            Assert.Contains(nameof(TestSubjectRoot.Level2Field), members.Select(x => x.Name));
            Assert.Equal(5, members.Count);
        }

        [Fact]
        public void GetTypeMembersRecursive_WithMaxDepth2_OnlyReturnsTwoLevels()
        {
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 2);
            Assert.Equal(13, members.Count);
        }

        [Fact]
        public void GetTypeMembersRecursive_WithDepth_UsesDotsInPath()
        {
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 2);
            Assert.Contains($"{nameof(TestSubjectRoot.Level2Prop)}.{nameof(TestSubjectLevel2.PropLevel2)}", members.Select(x => x.Name));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoredMemberPathPrefixes_IgnoresPrefixes()
        {
            var filter = new TKMemberFilterRecursive()
            {
                IgnoredMemberPathPrefixes = new[] { $"{nameof(TestSubjectRoot.Level2Prop)}." }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members.Select(x => x.Name), x => x.StartsWith($"{nameof(TestSubjectRoot.Level2Prop)}."));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoreMembersDeclaredInTypes_IgnoresMembers()
        {
            var filter = new TKMemberFilterRecursive()
            {
                IgnoreMembersDeclaredInTypes = new[] { typeof(TestSubjectRootBase) }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain("PropRoot1", members.Select(x => x.Name));
            Assert.DoesNotContain("PropRoot2", members.Select(x => x.Name));
        }

        [Fact]
        public void GetTypeMembersRecursive_ExcludeSpecialEtcProperties_ExcludesSpecialThings()
        {
            var filter1 = new TKMemberFilterRecursive()
            {
                PropertyFilter = new TKPropertyFilter
                {
                    ExcludeSpecialEtcProperties = true
                }
            };
            var members1 = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter1);
            var filter2 = new TKMemberFilterRecursive()
            {
                PropertyFilter = new TKPropertyFilter
                {
                    ExcludeSpecialEtcProperties = false
                }
            };
            var members2 = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter2);
            Assert.NotEqual(members1.Count, members2.Count);
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoredMemberGenericTypeDefinitions_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IgnoredMemberGenericTypeDefinitions = new[] { typeof(TestSubjectLevel4<>) }
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(TestSubjectLevel4<string>));
            Assert.DoesNotContain(members, x => x.Type == typeof(TestSubjectLevel4<int>));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoredMemberTypeNamespacePrefixes_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IgnoredMemberTypeNamespacePrefixes = new[] { "QoDL.Toolkit.Core.Tests.Extensions.SomeOtherNamespace" }
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(SomeOtherNamespace.TestSubjectLevel5));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoredMemberTypes_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IgnoredMemberTypes = new[] { typeof(SomeOtherNamespace.TestSubjectLevel5) }
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(SomeOtherNamespace.TestSubjectLevel5));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoredMemberTypesIncludingDescendants_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IgnoredMemberTypesIncludingDescendants = new[] { typeof(SomeOtherNamespace.TestSubjectLevel5Base) }
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(SomeOtherNamespace.TestSubjectLevel5));
        }

        [Fact]
        public void GetTypeMembersRecursive_IncludedMemberTypesFilter_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IncludedMemberTypesFilter = (t) => t != typeof(SomeOtherNamespace.TestSubjectLevel5)
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(SomeOtherNamespace.TestSubjectLevel5));
        }

        [Fact]
        public void GetTypeMembersRecursive_IgnoreGenericEnumerableMemberTypes_IgnoresTargets()
        {
            var filter = new TKMemberFilterRecursive()
            {
                TypeFilter = new TKTypeFilter
                {
                    IgnoreGenericEnumerableMemberTypes = true
                }
            };
            var members = TKReflectionUtils.GetTypeMembersRecursive(typeof(TestSubjectRoot), 4, filter);
            Assert.DoesNotContain(members, x => x.Type == typeof(List<string>));
        }

        private class TestSubjectRootBase
        {
            public string PropRoot1 { get; set; }
            public string PropRoot2 { get; set; }
        }
        private class TestSubjectRoot : TestSubjectRootBase
        {
            public TestSubjectLevel2 Level2Prop { get; set; }
            public TestSubjectLevel2 Level2Field;
            public string PropSetterOnlyRoot1 { set { _ = value; } }
        }
        private class TestSubjectLevel2
        {
            public string PropLevel2 { get; set; }
            public string FieldLevel2;
            public TestSubjectLevel3 Level3Prop { get; set; }
            public TestSubjectLevel3 Level3Field;
        }
        private class TestSubjectLevel3
        {
            public string PropLevel3 { get; set; }
            public string FieldLevel3;
            public TestSubjectLevel4<string> Level4Prop { get; set; }
            public TestSubjectLevel4<int> Level4Field;
        }
        private class TestSubjectLevel4<T>
        {
            public string PropLevel4 { get; set; }
            public string FieldLevel4;
            public SomeOtherNamespace.TestSubjectLevel5 Level5Prop { get; set; }
            public SomeOtherNamespace.TestSubjectLevel5 Level5Field;
            public List<string> ListX { get; set; }
        }
    }
}

namespace QoDL.Toolkit.Core.Tests.Extensions.SomeOtherNamespace
{
    public class TestSubjectLevel5Base
    {
    }
    public class TestSubjectLevel5 : TestSubjectLevel5Base
    {
        public string PropLevel5 { get; set; }
        public string FieldLevel5;
    }
}
