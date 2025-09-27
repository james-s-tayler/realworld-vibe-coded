using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.IO;

public static class NukeLinter
{
    public static void RunLintChecks()
    {
        Console.WriteLine("Running Nuke build linting checks...");
        
        var lintErrors = new List<string>();
        
        // Get the build file path
        var buildFilePath = GetBuildFilePath();
        
        if (!File.Exists(buildFilePath))
        {
            throw new FileNotFoundException($"Could not find Build.cs file at: {buildFilePath}");
        }

        Console.WriteLine($"Analyzing build file: {buildFilePath}");

        // Read and analyze the source code
        var buildSourceCode = File.ReadAllText(buildFilePath);
        
        // Extract all target definitions from the source code
        var targetDefinitions = ExtractTargetDefinitions(buildSourceCode);

        Console.WriteLine($"Found {targetDefinitions.Count} target definitions to analyze");

        foreach (var targetName in targetDefinitions.Keys)
        {
            lintErrors.AddRange(CheckTargetDocumentation(targetName, targetDefinitions[targetName]));
            lintErrors.AddRange(CheckTargetNamingConventions(targetName));
        }

        if (lintErrors.Any())
        {
            Console.WriteLine($"Found {lintErrors.Count} linting error(s):");
            foreach (var error in lintErrors)
            {
                Console.WriteLine($"  • {error}");
            }
            throw new Exception($"Nuke build linting failed with {lintErrors.Count} error(s)");
        }
        
        Console.WriteLine("✓ All Nuke build linting checks passed!");
        Console.WriteLine($"✓ Verified {targetDefinitions.Count} targets have proper documentation and naming");
    }

    private static string GetBuildFilePath()
    {
        // Try multiple possible locations for Build.cs
        var possiblePaths = new[]
        {
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Build.cs"),
            Path.Combine(Environment.CurrentDirectory, "Build.cs"),
            Path.Combine(Environment.CurrentDirectory, "_build", "Build.cs"),
            Path.Combine(Environment.CurrentDirectory, "build", "_build", "Build.cs")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return possiblePaths[0]; // Return first path for error reporting
    }

    private static Dictionary<string, string> ExtractTargetDefinitions(string sourceCode)
    {
        var targets = new Dictionary<string, string>();
        
        // Pattern to match target definitions: Target TargetName => _ => _...;
        var targetPattern = @"Target\s+(\w+)\s*=>\s*_\s*=>\s*_([^;]+);";
        var regex = new Regex(targetPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        var matches = regex.Matches(sourceCode);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                var targetName = match.Groups[1].Value.Trim();
                var targetDefinition = match.Groups[2].Value.Trim();
                targets[targetName] = targetDefinition;
            }
        }
        
        return targets;
    }

    private static List<string> CheckTargetDocumentation(string targetName, string targetDefinition)
    {
        var errors = new List<string>();
        
        var hasDescription = targetDefinition.Contains(".Description(");
        if (!hasDescription)
        {
            errors.Add($"Target '{targetName}' is missing .Description() call");
        }
        else
        {
            Console.WriteLine($"  ✓ Target '{targetName}' has description");
        }

        return errors;
    }

    private static List<string> CheckTargetNamingConventions(string targetName)
    {
        var errors = new List<string>();
        
        if (IsValidTargetName(targetName))
        {
            Console.WriteLine($"  ✓ Target '{targetName}' follows PascalCase naming convention");
        }
        else
        {
            errors.Add($"Target '{targetName}' should follow PascalCase naming (e.g., 'BuildServer', 'TestServer')");
        }
        
        return errors;
    }



    private static bool IsValidTargetName(string name)
    {
        // Valid Nuke target names should be PascalCase
        // Examples: ShowHelp, BuildServer, TestServer, LintServer, etc.
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return false;
            
        return name.All(c => char.IsLetterOrDigit(c)) &&
               !name.Contains("_") &&
               !name.Contains("-");
    }
}