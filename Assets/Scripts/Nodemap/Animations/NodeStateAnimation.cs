using UnityEngine;
using System.Collections;

/// <summary>
/// Node animation component - deprecated, animations removed for simplicity.
/// Kept as stub to avoid breaking prefabs that reference it.
/// </summary>
public class NodeStateAnimation : MonoBehaviour
{
    // All animation functionality removed
    public IEnumerator Shake() { yield break; }
    public IEnumerator AnimateStateChange(NodeState _) { yield break; }
}
