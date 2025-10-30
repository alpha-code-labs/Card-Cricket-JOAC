using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yarn.Compiler;
using Yarn.Unity;
using Yarn;
using System;

public static class YarnScriptsValaditionTests
{
    [MenuItem("Tools/Test Yarn Scripts")]
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
            issues.AddRange(TestCharacterSpriteShownDuringNarration(text, path));
        }
        issues.AddRange(TestCalanderSystemAndYarnNodes());
        issues = issues.Distinct().ToList();
        issues.Sort();
        string result = $"Tested {totalFiles} .yarn files under {yarnFolder}. Found {issues.Count} issues.\n{string.Join("\n", issues)}";
        Debug.Log(result);
        EditorUtility.DisplayDialog("Yarn script validation", "Tests Finished Check Output in Console", "OK");
    }
    static List<string> TestCalanderSystemAndYarnNodes()
    {
        List<string> issues = new List<string>();
        // Load the specific YarnProject asset
        string yarnProjectPath = "Assets/Yarnscript/NewProject.yarnproject";
        YarnProject yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(yarnProjectPath);

        if (yarnProject == null)
        {
            Debug.LogError($"Failed to load YarnProject at {yarnProjectPath}");
            EditorUtility.DisplayDialog("Yarn script validation", $"Failed to load YarnProject at {yarnProjectPath}", "OK");
            return issues;
        }

        CalanderRecord calanderRecord = AssetDatabase.LoadAssetAtPath<CalanderRecord>("Assets/ScriptableObjects/CalanderRecord.asset");

        foreach (var dateRecord in calanderRecord.dates)
        {
            foreach (var eventRecord in dateRecord.events)
            {
                if (eventRecord.eventType == TypeOfEvent.ForcedCutscene)
                {
                    // Check if the start node exists in the Yarn project
                    if (!yarnProject.NodeNames.Contains(eventRecord.eventName))
                    {
                        issues.Add($"Calander event '{eventRecord.eventName}' on date '{dateRecord.date}' has invalid start node.");
                    }
                }
            }
        }
        if (issues.Count == 0)
        {
            Debug.Log("All CalanderRecord events have valid Yarn nodes.");
        }
        return issues;
    }
    static List<string> TestYarnCommands(string text)
    {
        List<string> issues = new List<string>();

        //Match and Identify Yarn Commands to Test for n Number of Parameters
        // Example Command <<SetCharacterExpression "Ramu" "Excited">>
        // Example Command << SetCharacterExpression Ramu Excited>>
        //Example Command <<SetBGSprite hutInterior >>

        var GenericCommandRegex = new Regex(@"<<\s*([a-zA-Z0-9_]+)([^>]*)>>", RegexOptions.IgnoreCase);
        var genericMatches = GenericCommandRegex.Matches(text);
        foreach (Match match in genericMatches)
        {
            var commandName = match.Groups[1].Value.Trim();
            var parameters = match.Groups[2].Value.Trim();

            switch (commandName)
            {
                case "SetCharacterExpression":
                    {
                        // Expecting 2 parameters
                        var paramRegex = new Regex(@"['""]?([^'""]+)['""]?\s+['""]?([^'""]+)['""]?");
                        var paramMatch = paramRegex.Match(parameters);
                        if (!paramMatch.Success)
                        {
                            issues.Add($"SetCharacterExpression command expects 2 parameters. Found: {parameters}. Line: {match.Value}");
                        }
                        else
                        {
                            var characterName = paramMatch.Groups[1].Value.Trim();
                            var expression = paramMatch.Groups[2].Value.Trim();

                            // Validate character name enum
                            if (!System.Enum.TryParse(characterName, out Characters _))
                            {
                                issues.Add($"Invalid character name: {characterName}. Line: {match.Value}");
                            }

                            // Validate expression enum
                            if (!System.Enum.TryParse(expression, out EmotionType _))
                            {
                                issues.Add($"Invalid character expression: {expression}. Line: {match.Value}");
                            }

                            // Check if sprite exists for the combined name+expression
                            var combined = (characterName + expression).Replace(" ", "");
                            if (DialogueScriptCommandHandler.GetSpriteByName(combined, false) == null)
                            {
                                issues.Add($"No sprite found for character expression: {combined}. Line: {match.Value}");
                            }
                        }
                        break;
                    }
                case "SetBGSprite":
                    {
                        // Expecting 1 parameter
                        var paramRegex = new Regex(@"['""]?([^'""]+)['""]?");
                        var paramMatch = paramRegex.Match(parameters);
                        if (!paramMatch.Success)
                        {
                            issues.Add($"SetBGSprite command expects 1 parameter. Found: {parameters}. Line: {match.Value}");
                        }
                        else
                        {
                            var bgName = paramMatch.Groups[1].Value.Trim();

                            // Check if sprite exists for the background
                            if (DialogueScriptCommandHandler.GetSpriteByName(bgName, false) == null)
                            {
                                issues.Add($"No sprite found for background: {bgName}. Line: {match.Value}");
                            }
                        }
                        break;
                    }
                default:
                    // Other commands can be added here as needed
                    break;
            }
        }
        return issues;
    }
    static List<string> TestCharacterSpriteShownDuringNarration(string text, string filePath)
    {
        List<string> issues = new List<string>();

        // Split the text into lines for analysis
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Track if characters are currently shown
        var shownCharacters = new HashSet<string>();
        var hideAllCharactersPattern = new Regex(@"<<\s*HideAllCharacters\s*>>", RegexOptions.IgnoreCase);
        var setCharacterExpressionPattern = new Regex(@"<<\s*SetCharacterExpression\s+['""]?([^'""]+)['""]?\s+['""]?([^'""]+)['""]?\s*>>", RegexOptions.IgnoreCase);

        string fileName = Path.GetFileName(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Check if this line hides all characters
            if (hideAllCharactersPattern.IsMatch(line))
            {
                shownCharacters.Clear();
                continue;
            }

            // Check if this line shows a character
            var characterMatch = setCharacterExpressionPattern.Match(line);
            if (characterMatch.Success)
            {
                var characterName = characterMatch.Groups[1].Value.Trim();
                shownCharacters.Add(characterName);
                continue;
            }

            // Check if this is a narration line (doesn't start with << and doesn't contain :)
            bool isNarration = !line.StartsWith("<<") && !line.Contains(":");

            // Additional check for common Yarn syntax patterns that are not narration
            bool isYarnSyntax = line.StartsWith("->") || // choice
                               line.StartsWith("===") || // node header
                               line.StartsWith("---") || // node separator
                               line.StartsWith("//") || // comment
                               line.StartsWith("if") || // conditional
                               line.StartsWith("endif") || // end conditional
                               line.StartsWith("else") || // else conditional
                               line.Contains("jump") || // jump command
                               line.Contains("stop") || // stop command
                               line.StartsWith("title:") || // node title
                               line.StartsWith("tags:"); // node tags

            if (isNarration && !isYarnSyntax)
            {
                // Check if any characters are currently shown during this narration
                if (shownCharacters.Count > 0)
                {
                    // Check if the previous line was HideAllCharacters
                    bool hasHideAllCharactersAbove = false;
                    if (i > 0)
                    {
                        var previousLine = lines[i - 1].Trim();
                        hasHideAllCharactersAbove = hideAllCharactersPattern.IsMatch(previousLine);
                    }

                    if (!hasHideAllCharactersAbove)
                    {
                        var characterList = string.Join(", ", shownCharacters);
                        issues.Add($"Narration line has visible character sprites without <<HideAllCharacters>> above it. File: {fileName}, Line {lineNumber}: '{line}'. Visible characters: {characterList}");
                    }
                }
            }
        }

        return issues;
    }
}
