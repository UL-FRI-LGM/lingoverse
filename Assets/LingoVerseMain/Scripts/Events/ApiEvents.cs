using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiEvents
{
    public Action<string> onCompletedJson;
    public void CompletedJson(string json)
    {
        onCompletedJson?.Invoke(json);
    }

    public Action<string> onInDatabase;
    public void InDatabase(string key)
    {
        onInDatabase?.Invoke(key);
    }

    public Action<string, string> onNotInDatabase;
    public void NotInDatabase(string apiPath, string key)
    {
        onNotInDatabase?.Invoke(apiPath, key);
    }
}
