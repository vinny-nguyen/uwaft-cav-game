using System;
using UnityEngine;

/// <summary>
/// Simple value object for node identification to provide type safety and prevent mixing up node indices with other integers.
/// </summary>
[System.Serializable]
public struct NodeId : IEquatable<NodeId>
{
    [SerializeField] private int value;

    public int Value => value;

    public NodeId(int value)
    {
        this.value = Mathf.Max(0, value); // Ensure non-negative
    }

    public static implicit operator int(NodeId nodeId) => nodeId.value;
    public static implicit operator NodeId(int value) => new NodeId(value);

    public bool Equals(NodeId other) => value == other.value;
    public override bool Equals(object obj) => obj is NodeId other && Equals(other);
    public override int GetHashCode() => value.GetHashCode();
    
    public static bool operator ==(NodeId left, NodeId right) => left.Equals(right);
    public static bool operator !=(NodeId left, NodeId right) => !left.Equals(right);

    public override string ToString() => $"Node({value})";
    
    /// <summary>
    /// Validates that this NodeId is within the valid range for the given node count.
    /// </summary>
    public bool IsValid(int nodeCount) => value >= 0 && value < nodeCount;
    
    /// <summary>
    /// Creates a NodeId with validation against the given node count.
    /// Returns null if the value is out of range.
    /// </summary>
    public static NodeId? CreateValidated(int value, int nodeCount)
    {
        if (value < 0 || value >= nodeCount) return null;
        return new NodeId(value);
    }
}