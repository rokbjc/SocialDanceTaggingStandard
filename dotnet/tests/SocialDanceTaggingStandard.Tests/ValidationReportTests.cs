using SocialDanceTaggingStandard.Validation;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class ValidationReportTests
{
    [Fact]
    public void EmptyReport_IsValidWithNoErrorsOrWarnings()
    {
        var report = new ValidationReport([]);
        Assert.True(report.IsValid);
        Assert.Empty(report.Errors);
        Assert.Empty(report.Warnings);
    }

    [Fact]
    public void WarningAlone_KeepsReportValid()
    {
        var report = new ValidationReport([new ValidationMessage(ValidationSeverity.Warning, "dropped")]);
        Assert.True(report.IsValid);
        Assert.Equal(["dropped"], report.Warnings);
        Assert.Empty(report.Errors);
    }

    [Fact]
    public void Error_MakesReportInvalid()
    {
        var report = new ValidationReport([new ValidationMessage(ValidationSeverity.Error, "bad")]);
        Assert.False(report.IsValid);
        Assert.Equal(["bad"], report.Errors);
    }

    [Fact]
    public void ErrorsAndWarnings_ProjectOnlyOwnSeverityInOrder()
    {
        var report = new ValidationReport(
        [
            new ValidationMessage(ValidationSeverity.Error, "e1"),
            new ValidationMessage(ValidationSeverity.Warning, "w1"),
            new ValidationMessage(ValidationSeverity.Error, "e2"),
        ]);
        Assert.Equal(["e1", "e2"], report.Errors);
        Assert.Equal(["w1"], report.Warnings);
    }
}
