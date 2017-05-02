namespace Test
{
    struct Stats
    {
        public static readonly Stats Zero = new Stats();
        public int Tests { get; }
        public int Passed { get; }
        public int Failed { get; }
        public int Ignored { get; }

        public Stats(int tests, int passed, int failed, int ignored)
        {
            Tests = tests;
            Passed = passed;
            Failed = failed;
            Ignored = ignored;
        }

        public static Stats operator+(Stats left, Stats right) =>
            new Stats(left.Tests + right.Tests, left.Passed + right.Passed, left.Failed + right.Failed, left.Ignored + right.Ignored);
    }
}
