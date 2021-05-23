using System;
using UnityEngine;

public class UpdateCaller : MonoBehaviour
{
    private static UpdateCaller instance;
    public static void AddUpdateCallback(Action updateMethod) {
        if (instance == null) {
            instance = new GameObject("[Update Caller]").AddComponent<UpdateCaller>();
        }
        instance.updateCallback += updateMethod;
    }
 
    public static void RemoveUpdateCallback(Action updateMethod) {
        if (instance == null) {
            instance = new GameObject("[Update Caller]").AddComponent<UpdateCaller>();
        }
        instance.updateCallback -= updateMethod;
    }
    
    private Action updateCallback;
 
    private void Update() {
        updateCallback?.Invoke();
    }
}
