# TestRunner
Continuous test runner for NUnit 2 and 3

# test.exe (testx64.exe and testx86.exe)

```
test.exe assembly
```

Runs all `Nunit` 2 and 3 tests found the assembly.

Supports:
* `[TestFixture]` attruibute at the class level
* `[Test]` attruibute
* `[TestCase(...)]` attruibute
* `[Setup]` attruibute
* `[TearDown]` attruibute
* `[SetUpFixture]` attruibute at the class level
* `[Timeout]` attribute on methods and the class level
* `[Ignore]` attribute on methods and the class level
* `[Explicit]` attribute on methods and the class level

Test results are output to StdOut, information is returned on StdErr.
Note that other properties of the `Test` and `TestCase` attribuites are **not** supported, e.g `ExpectException`, `ExceptionMessage`.

# testd.exe

```
testd.exe [--x64] [--x86] assembly
```

Monitors the *current directory* for changes to dll and exe files, and runs tests (via `test.exe` or `testx64.exe` or `testx86.exe`) when changes are found.

# TestGui.exe

```
TestGui.exe [assembly]
```

When started with no arguments `TestGui.exe` discovers unit test projects relative to the *current directory*.  Double-clicking a test project start testing.

![Project](projects.png)

When passed an *assembly* uses `testd.exe` to monitor the selected build for changes to dll and exe files, and runs tests (via `test.exe`or `testx64.exe`) when changes are found.

![Tests](tests.png)

## Why

I don't want to pay to a test runner (e.g. NCrunch) when I can build a simple one myself.
