using System;
using UnityEngine;

/// <summary>
/// Type-safe node identifier that prevents mixing node indices with other integers.
/// </summary>
[System.Serializable]
public struct NodeId : IEquatable<NodeId>
{
    [SerializeField] private int value;

    public int Value => value;

    public NodeId(int value)
    {
        this.value = Mathf.Max(0, value);
    }

    public bool Equals(NodeId other) => value == other.value;
    public override bool Equals(object obj) => obj is NodeId other && Equals(other);
    public override int GetHashCode() => value.GetHashCode();
    public override string ToString() => $"Node({value})";
}