namespace HealthCheck.Dev.Common
{
    public static class RuntimeTestConstants
    {
        public static class Group
        {
            public const string AdminStuff = "Admin: Stuff";
            public const string AlmostTopGroup = "Almost at the top group";
            public const string TopGroup = "Top group";
            public const string BottomGroup = "Bottom group";
            public const string AlmostBottomGroup = "Almost at the bottom group";
            public const string DataRepeater = "Data Repeater";
        }

        public static class Categories
        {
            public const string ScheduledHealthCheck = "healthcheck";
            public const string APIChecks = "api";
        }
    }
}