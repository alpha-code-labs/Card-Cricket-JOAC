using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class YarnScriptsValaditionTests
{
    [MenuItem("Tools/TestYarnScripts")]
    static void TestYarnScripts()
    {
        DialogueScriptCommandHandler.InitializeSpriteMapping(); //Prepring for Test

        string yarnFolder = Path.Combine(Application.dataPath, "Yarnscript");
        if (!Directory.Exists(yarnFolder))
        {
            Debug.LogError($"Yarn folder not found: {yarnFolder}");
            return;
        }

        var files = Directory.GetFiles(yarnFolder, "*.yarn", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Debug.Log($"No .yarn files found under {yarnFolder}");
            EditorUtility.DisplayDialog("Yarn script validation", $"No .yarn files found under {yarnFolder}", "OK");
            return;
        }

        int totalFiles = files.Length;
        int filesWithIssues = 0;
        int totalIssues = 0;
        var summaryLines = new List<string>();



        foreach (var path in files.OrderBy(p => p))
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read {path}: {ex.Message}");
                continue;
            }

            var issues = new List<string>();
            //Test to Run
            issues.AddRange(TestYarnCommands(text));

            //Summarise Results
            if (issues.Count > 0)
            {
                filesWithIssues++;
                totalIssues += issues.Count;
                string relPath = "Assets" + path.Replace(Application.dataPath, "").Replace('\\', '/');
                // Indent each issue in the log and include the full issue string
                var indentedIssues = string.Join("\n  - ", issues);
                Debug.LogWarning($"Yarn issues in {relPath}:\n  - {indentedIssues}");
                // Include the first few issues (or all) in the summary for quick overview
                string summaryIssues = issues.Count <= 5 ? indentedIssues : string.Join("\n  - ", issues.Take(5)) + $"\n  - ...(+{issues.Count - 5} more)";
                summaryLines.Add($"{Path.GetFileName(path)}: {issues.Count} issue(s)\n  - {summaryIssues}");
            }
        }

        string summary = $"Scanned {totalFiles} .yarn file(s). Files with issues: {filesWithIssues}. Total issues: {totalIssues}.";
        Debug.Log(summary);
        if (summaryLines.Count > 0)
            Debug.Log(string.Join("\n", summaryLines));

        EditorUtility.DisplayDialog("Yarn script validation", summary, "OK");
    }

    static List<string> TestPlaceHoldeIssues(string text)
    {
        List<string> issues = new List<string>();

        // Matches common label markers like "title: MyLabel" or lines starting with == Label ==
        var labelRegex = new Regex(@"^	*(?:title:|==)	*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);


        // 1) Duplicate labels in the same file
        var matches = labelRegex.Matches(text);
        var labels = matches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        var dupes = labels.GroupBy(l => l).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupes.Count > 0)
            issues.Add($"Duplicate labels: {string.Join(", ", dupes)}");

        // 2) Unmatched double quotes
        int quoteCount = text.Count(c => c == '"');
        if ((quoteCount & 1) != 0)
            issues.Add("Unmatched double-quote (\") found");

        // 3) Unbalanced braces
        int openBraces = text.Count(c => c == '{');
        int closeBraces = text.Count(c => c == '}');
        if (openBraces != closeBraces)
            issues.Add($"Unbalanced braces: '{{'={openBraces}, '}}'={closeBraces}");

        // 4) Simple check: lines longer than 200 characters (heuristic)
        var longLines = text.Split('\n').Select((l, i) => new { l, i }).Where(x => x.l.Length > 200).Take(5).ToList();
        if (longLines.Count > 0)
            issues.Add($"{longLines.Count} lines longer than 200 chars (e.g. line {longLines[0].i + 1})");

        return issues;
    }

    static List<string> TestYarnCommands(string text)
    {
        List<string> issues = new List<string>();

        // Test SetCharacterExpression //Example Command <<SetCharacterExpression "Ramu" "Excited">>
        var setCharExprRegex = new Regex(@"<<\s*SetCharacterExpression\s+['""]([^'""]+)['""]\s+['""]([^'""]+)['""]\s*>>", RegexOptions.IgnoreCase);
        var setCharMatches = setCharExprRegex.Matches(text);
        var nameExpressionPairs = setCharMatches.Cast<Match>()
            .Select(m => new { name = m.Groups[1].Value.Trim(), expr = m.Groups[2].Value.Trim() })
            .Where(x => !string.IsNullOrEmpty(x.name) && !string.IsNullOrEmpty(x.expr))
            .ToList();

        // Check SetCharacterExpression combined name+expression (e.g., RamuNeutral)
        // Remove internal spaces when building combined key (e.g., "Ramu Kumar" + "Neutral" -> "RamuKumarNeutral")
        foreach (var pair in nameExpressionPairs.GroupBy(p => (p.name + p.expr).Replace(" ", "")).Select(g => g.First()))
        {
            var combined = (pair.name + pair.expr).Replace(" ", "");
            if (DialogueScriptCommandHandler.GetSpriteByName(combined) == null)
            {
                if (!issues.Contains($"No sprite found for character expression: {combined}"))
                    issues.Add($"No sprite found for character expression: {combined}");
            }
        }

        // Test SetBGSprite //Example Command <<SetBGSprite hutInterior>>
        var setBgSpriteRegex = new Regex(@"<<\s*SetBGSprite\s+([a-zA-Z0-9_]+)\s*>>", RegexOptions.IgnoreCase);
        var setBgMatches = setBgSpriteRegex.Matches(text);
        var bgNames = setBgMatches.Cast<Match>()
            .Select(m => m.Groups[1].Value.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();
        foreach (var bgName in bgNames)
        {
            if (DialogueScriptCommandHandler.GetSpriteByName(bgName) == null)
            {
                if (!issues.Contains($"No sprite found for background: {bgName}"))
                    issues.Add($"No sprite found for background: {bgName}");
            }
        }
        

        return issues;
    }
}
