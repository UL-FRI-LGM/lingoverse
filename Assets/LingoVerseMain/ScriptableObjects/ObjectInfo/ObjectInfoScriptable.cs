// The ObjectInfoScriptable script should be placed in a separate .cs file.
using UnityEngine;

// TODO CHANGE THIS
[CreateAssetMenu(fileName = "NewObjectInfo", menuName = "Custom/ObjectInfo")]
public class ObjectInfoScriptable : ScriptableObject
{
    [SerializeField]
    private string objectName;
    [SerializeField]
    private string info;
    //[SerializeField]
    //private Vector3 offset; // Offset for positioning the InfoPanel relative to the object
    [SerializeField]
    private AudioClip audioClip;

    private void OnValidate()
    {

#if UNITY_EDITOR
        objectName = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif

    }

    public string Name { get => objectName; }
    public string Info { get => info; }
    //public Vector3 Offset { get => offset; }
    public AudioClip AudioClip { get => audioClip; }
}