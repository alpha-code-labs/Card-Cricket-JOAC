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
}
