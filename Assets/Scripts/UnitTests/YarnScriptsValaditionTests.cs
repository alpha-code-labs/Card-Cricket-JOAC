using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class YarnScriptsValaditionTests
{
    [MenuItem("Tools/TestYarnScripts")]
    static void TestYarnScripts()
    {
        Yarn.Unity.Tests.TestCommands.CoolCommand();
    }
}
