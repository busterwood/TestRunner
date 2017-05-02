namespace Test
{
    struct Stats
    {
        public static readonly Stats Zero = new Stats();
        public int Tests { get; }
        public int Passed { get; }
        public int Failed { get; }

        public Stats(int tests, int passed, int failed)
        {
            Tests = tests;
            Passed = passed;
            Failed = failed;
        }
        public static Stats operator+(Stats left, Stats right) =>
            new Stats(left.Tests + right.Tests, left.Passed + right.Passed, left.Failed + right.Failed);
    }
}
