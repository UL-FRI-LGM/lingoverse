using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MatchGame/Game")]
public class MatchPairGameObject : ScriptableObject
{
    // List of match game pairs
    public List<MatchPair> pairs;
    public int reward;
    // todo more, maybe achievement

    public MatchPairGameObject()
    {
        reward = 0;
    }
}
