// SceneDumper.cs
//
// History:
// version 1.0 - December 2010 - Yossarian King

using System.Text;
using StreamWriter = System.IO.StreamWriter;
 
using UnityEngine;
 
public static class SceneDumper
{
    public static string DumpScene(params GameObject[] gameObjects)
    {
        if ((gameObjects == null) || (gameObjects.Length == 0))
        {
            Debug.LogError("Please select the object(s) you wish to dump.");
            return "";
        }

        StringBuilder sb = new StringBuilder();
        foreach (GameObject gameObject in gameObjects)
        {
            DumpGameObject(gameObject, sb, "");
        }

        return sb.ToString();
    }
 
    private static void DumpGameObject(GameObject gameObject, StringBuilder sb, string indent)
    {
        sb.Append($"o: {indent}{gameObject.name}\n");
 
        foreach (Component component in gameObject.GetComponents<Component>())
        {
            DumpComponent(component, sb, indent + "  ");
        }
 
        foreach (Transform child in gameObject.transform)
        {
            DumpGameObject(child.gameObject, sb, indent + "  ");
        }
    }
 
    private static void DumpComponent(Component component, StringBuilder sb, string indent)
    {
       sb.Append($"c: {indent}{(component == null ? "(null)" : component.GetType().Name)}\n");
    }
}