using System.Text.RegularExpressions;
using FlowPilot.Cli.Models;

namespace FlowPilot.Cli.Services;

/// <summary>
/// Parses state.md files and extracts checklist information.
/// </summary>
public partial class StateParser
{
  public List<StateChecklistItem> ParseStateFile(string content)
  {
    var items = new List<StateChecklistItem>();
    var matches = ChecklistItemRegex().Matches(content);

    foreach (Match match in matches)
    {
      var item = new StateChecklistItem
      {
        IsChecked = match.Groups[1].Value == "x",
        Identifier = match.Groups[2].Value,
        Description = match.Groups[3].Value,
      };

      // Check if this is a phase item (phase_N)
      if (item.Identifier.StartsWith("phase_", StringComparison.Ordinal))
      {
        if (int.TryParse(item.Identifier.Substring(6), out int phaseNumber))
        {
          item.PhaseNumber = phaseNumber;
        }
      }

      items.Add(item);
    }

    return items;
  }

  public string UpdateChecklistItem(string content, string identifier, bool isChecked)
  {
    var pattern = $@"^- \[[ x]\] \[{Regex.Escape(identifier)}\] (.+)$";
    var replacement = $"- [{(isChecked ? "x" : " ")}] [{identifier}] $1";

    return Regex.Replace(content, pattern, replacement, RegexOptions.Multiline);
  }

  public string AddPhaseChecklistItems(string content, List<string> phaseNames)
  {
    var lines = content.Split('\n').ToList();

    // Find the position after [phase-n-details]
    var detailsIndex = lines.FindIndex(l => l.Contains("[phase-n-details]"));
    if (detailsIndex == -1)
    {
      return content;
    }

    // Add phase items after the phase-n-details line
    for (int i = 0; i < phaseNames.Count; i++)
    {
      var phaseItem = $"- [ ] [phase_{i + 1}] {phaseNames[i]}";
      lines.Insert(detailsIndex + 1 + i, phaseItem);
    }

    return string.Join('\n', lines);
  }

  [GeneratedRegex(@"^- \[([ x])\] \[(.+?)\] (.+)$", RegexOptions.Multiline)]
  private static partial Regex ChecklistItemRegex();
}
