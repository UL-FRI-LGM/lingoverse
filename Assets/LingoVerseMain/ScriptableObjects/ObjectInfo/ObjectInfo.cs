using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfo : MonoBehaviour
{
    [SerializeField] private ObjectInfoScriptable objectInfoScriptable;

    public ObjectInfoScriptable Data { get => objectInfoScriptable; }
}