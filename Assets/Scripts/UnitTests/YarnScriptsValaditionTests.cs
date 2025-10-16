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
        var issues = new List<string>();
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
            //Test to Run
            issues.AddRange(TestYarnCommands(text));
        }
        issues = issues.Distinct().ToList();
        issues.Sort();
        string result = $"Tested {totalFiles} .yarn files under {yarnFolder}. Found {issues.Count} issues.\n{string.Join("\n", issues)}";
        EditorUtility.DisplayDialog("Yarn script validation", result, "OK");
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
            if (DialogueScriptCommandHandler.GetSpriteByName(combined, false) == null)
            {
                // if (!issues.Contains($"No sprite found for character expression: {combined}"))
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
            if (DialogueScriptCommandHandler.GetSpriteByName(bgName, false) == null)
            {
                // if (!issues.Contains($"No sprite found for background: {bgName}"))
                issues.Add($"No sprite found for background: {bgName}");
            }
        }
        return issues;
    }
}
