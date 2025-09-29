using System;
using System.IO;
using System.Linq;

namespace TrxToMarkdown;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: TrxToMarkdown <trx-directory> <output-markdown-file>");
            Console.WriteLine("  trx-directory: Directory containing .trx files");
            Console.WriteLine("  output-markdown-file: Path to output combined markdown file");
            return 1;
        }

        var trxDirectory = args[0];
        var outputFile = args[1];

        if (!Directory.Exists(trxDirectory))
        {
            Console.WriteLine($"Error: Directory '{trxDirectory}' does not exist.");
            return 1;
        }

        try
        {
            var trxFiles = Directory.GetFiles(trxDirectory, "*.trx");
            
            if (trxFiles.Length == 0)
            {
                Console.WriteLine($"Warning: No .trx files found in '{trxDirectory}'.");
                File.WriteAllText(outputFile, "# Test Results\n\n⚠️ **Warning:** No test results found.\n");
                return 0;
            }

            Console.WriteLine($"Found {trxFiles.Length} TRX files in '{trxDirectory}'");
            
            var markdownReports = trxFiles
                .Select(TrxToMarkdownConverter.ConvertTrxToMarkdown)
                .ToList();

            var combinedMarkdown = TrxToMarkdownConverter.CombineMarkdownReports(markdownReports);
            
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputFile, combinedMarkdown);
            
            Console.WriteLine($"Successfully created combined test report: '{outputFile}'");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}