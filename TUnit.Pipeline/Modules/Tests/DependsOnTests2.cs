﻿using System.Text.Json;
using FluentAssertions;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Parsers.NUnitTrx;
using ModularPipelines.Extensions;
using ModularPipelines.Git.Extensions;

namespace TUnit.Pipeline.Modules.Tests;

public class DependsOnTests2 : TestModule
{
    protected override async Task<TestResult?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        return await RunTestsWithFilter(context, 
            "/*/*/DependsOnTests2/*",
            [
                result => result.Successful.Should().BeTrue(),
                result => result.Total.Should().Be(2),
                result => result.Passed.Should().Be(2),
                result => result.Failed.Should().Be(0),
                result => result.Skipped.Should().Be(0),
                result =>
                {
                    var test1Start = result.TrxReport.UnitTestResults.First(x => x.TestName!.StartsWith("Test1")).StartTime!.Value;
                    var test2Start = result.TrxReport.UnitTestResults.First(x => x.TestName!.StartsWith("Test2")).StartTime!.Value;

                    test2Start.Should().BeOnOrAfter(test1Start.AddSeconds(5));
                }
            ], cancellationToken);
    }
}