using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MatchGame/MatchPair")]
public class MatchPair : ScriptableObject
{
    public string pairName;
    // Object that the user picks
    // Example: Pickable apple
    public GameObject objectOne;
    // Object that user matches to
    // Example: A text obejct with text: der apfel, to where the user brings the first object
    public GameObjectOrText objectTwo;

    // Spawn positions
    /*public Vector3 objectOneSpawn;
    public Vector3 objectTwoSpawn;*/
}

[System.Serializable]
public class GameObjectOrText
{
    public string text;
    public GameObject gameObject;

    public GameObject GetGameObject() { return gameObject; }
}