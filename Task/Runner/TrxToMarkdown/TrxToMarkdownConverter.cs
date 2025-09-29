using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TrxToMarkdown;

public static class TrxToMarkdownConverter
{
    public static string ConvertTrxToMarkdown(string trxFilePath)
    {
        if (!File.Exists(trxFilePath))
        {
            return $"❌ **Error:** TRX file not found at `{trxFilePath}`\n";
        }

        try
        {
            var xml = XDocument.Load(trxFilePath);
            var ns = xml.Root?.Name.Namespace ?? XNamespace.None;

            // Extract test run information
            var testRun = xml.Element(ns + "TestRun");
            if (testRun == null)
            {
                return $"❌ **Error:** Invalid TRX format in `{trxFilePath}`\n";
            }

            var resultSummary = testRun.Element(ns + "ResultSummary");
            var counters = resultSummary?.Element(ns + "Counters");
            
            var total = int.Parse(counters?.Attribute("total")?.Value ?? "0");
            var passed = int.Parse(counters?.Attribute("passed")?.Value ?? "0");
            var failed = int.Parse(counters?.Attribute("failed")?.Value ?? "0");
            var skipped = int.Parse(counters?.Attribute("notExecuted")?.Value ?? "0");

            var projectName = Path.GetFileNameWithoutExtension(trxFilePath).Replace("-results", "");
            var passRate = total > 0 ? Math.Round((double)passed / total * 100, 1) : 0;

            var markdown = new StringBuilder();
            
            // Header
            markdown.AppendLine($"## {projectName}");
            markdown.AppendLine();

            // Summary
            var overallResult = failed > 0 ? "❌ Failed" : "✅ Passed";
            markdown.AppendLine($"**Overall Result:** {overallResult}  ");
            markdown.AppendLine($"**Pass Rate:** {passRate}%  ");
            markdown.AppendLine($"**Total Tests:** {total}  ");
            markdown.AppendLine($"**Passed:** {passed} | **Failed:** {failed} | **Skipped:** {skipped}");
            markdown.AppendLine();

            // Test results
            var testResults = xml.Descendants(ns + "UnitTestResult").ToList();
            if (testResults.Any())
            {
                markdown.AppendLine("### Test Results");
                markdown.AppendLine();
                markdown.AppendLine("| Result | Test | Duration |");
                markdown.AppendLine("|--------|------|----------|");

                foreach (var result in testResults.OrderBy(r => r.Attribute("outcome")?.Value).ThenBy(r => r.Attribute("testName")?.Value))
                {
                    var outcome = result.Attribute("outcome")?.Value ?? "Unknown";
                    var testName = result.Attribute("testName")?.Value ?? "Unknown";
                    var duration = result.Attribute("duration")?.Value ?? "0";
                    
                    var icon = outcome switch
                    {
                        "Passed" => "✅",
                        "Failed" => "❌",
                        "NotExecuted" => "⚠️",
                        _ => "❓"
                    };

                    // Clean up test name for better readability
                    var cleanTestName = testName.Replace(projectName + ".", "");
                    
                    markdown.AppendLine($"| {icon} | `{cleanTestName}` | {duration} |");
                }
                markdown.AppendLine();
            }

            // Failed test details
            var failedResults = testResults.Where(r => r.Attribute("outcome")?.Value == "Failed");
            if (failedResults.Any())
            {
                markdown.AppendLine("### Failed Test Details");
                markdown.AppendLine();

                foreach (var result in failedResults)
                {
                    var testName = result.Attribute("testName")?.Value ?? "Unknown";
                    var cleanTestName = testName.Replace(projectName + ".", "");
                    
                    markdown.AppendLine($"#### ❌ {cleanTestName}");
                    
                    var output = result.Element(ns + "Output");
                    var errorInfo = output?.Element(ns + "ErrorInfo");
                    
                    if (errorInfo != null)
                    {
                        var message = errorInfo.Element(ns + "Message")?.Value;
                        var stackTrace = errorInfo.Element(ns + "StackTrace")?.Value;
                        
                        if (!string.IsNullOrEmpty(message))
                        {
                            markdown.AppendLine("**Error Message:**");
                            markdown.AppendLine("```");
                            markdown.AppendLine(message);
                            markdown.AppendLine("```");
                            markdown.AppendLine();
                        }
                        
                        if (!string.IsNullOrEmpty(stackTrace))
                        {
                            markdown.AppendLine("<details>");
                            markdown.AppendLine("<summary>Stack Trace</summary>");
                            markdown.AppendLine();
                            markdown.AppendLine("```");
                            markdown.AppendLine(stackTrace);
                            markdown.AppendLine("```");
                            markdown.AppendLine("</details>");
                            markdown.AppendLine();
                        }
                    }
                }
            }

            return markdown.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ **Error:** Failed to parse TRX file `{trxFilePath}`: {ex.Message}\n";
        }
    }

    public static string CombineMarkdownReports(IEnumerable<string> markdownContents, string title = "Combined Test Results")
    {
        var combined = new StringBuilder();
        
        var contentsList = markdownContents.ToList();
        if (!contentsList.Any())
        {
            combined.AppendLine($"# {title}");
            combined.AppendLine();
            combined.AppendLine("⚠️ **Warning:** No test reports were found.");
            combined.AppendLine();
            return combined.ToString();
        }

        // Calculate overall statistics
        var totalTests = 0;
        var totalPassed = 0;
        var totalFailed = 0;
        var overallPassed = true;

        foreach (var content in contentsList)
        {
            // Extract stats from each report (simple parsing)
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("**Total Tests:**"))
                {
                    // Parse line like: **Total Tests:** 18  
                    var parts = trimmedLine.Split(':');
                    if (parts.Length > 1)
                    {
                        var totalStr = parts[1].Trim().Split(' ')[0];
                        if (int.TryParse(totalStr, out var total))
                        {
                            totalTests += total;
                        }
                    }
                }
                else if (trimmedLine.StartsWith("**Passed:**") && trimmedLine.Contains("|"))
                {
                    // Parse line like: **Passed:** 18 | **Failed:** 0 | **Skipped:** 0
                    var parts = trimmedLine.Split('|');
                    if (parts.Length >= 1)
                    {
                        var passedPart = parts[0].Trim();
                        if (passedPart.Contains("**Passed:**"))
                        {
                            var passedStr = passedPart.Split(':')[1].Trim();
                            if (int.TryParse(passedStr, out var passed))
                            {
                                totalPassed += passed;
                            }
                        }
                    }
                    if (parts.Length >= 2)
                    {
                        var failedPart = parts[1].Trim();
                        if (failedPart.Contains("**Failed:**"))
                        {
                            var failedStr = failedPart.Split(':')[1].Trim();
                            if (int.TryParse(failedStr, out var failed))
                            {
                                totalFailed += failed;
                                if (failed > 0) overallPassed = false;
                            }
                        }
                    }
                }
            }
        }

        // Now build the combined report with corrected overall statistics
        combined.AppendLine($"# {title}");
        combined.AppendLine();

        // Overall summary
        var overallResult = overallPassed ? "✅ Passed" : "❌ Failed";
        var overallPassRate = totalTests > 0 ? Math.Round((double)totalPassed / totalTests * 100, 1) : 0;
        
        combined.AppendLine($"**Overall Result:** {overallResult}  ");
        combined.AppendLine($"**Pass Rate:** {overallPassRate}%  ");
        combined.AppendLine($"**Total Tests:** {totalTests}  ");
        combined.AppendLine($"**Passed:** {totalPassed} | **Failed:** {totalFailed}");
        combined.AppendLine();
        combined.AppendLine("---");
        combined.AppendLine();

        // Add each report
        foreach (var content in contentsList)
        {
            combined.AppendLine(content);
            combined.AppendLine("---");
            combined.AppendLine();
        }

        return combined.ToString();
    }
}