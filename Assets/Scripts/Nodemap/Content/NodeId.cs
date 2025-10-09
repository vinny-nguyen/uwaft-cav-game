using System;
using UnityEngine;

/// <summary>
/// Type-safe node identifier that prevents mixing node indices with other integers.
/// Provides validation and clear API for node operations.
/// </summary>
[System.Serializable]
public struct NodeId : IEquatable<NodeId>, IComparable<NodeId>
{
    [SerializeField] private int value;

    public int Value => value;

    public NodeId(int value)
    {
        this.value = Mathf.Max(0, value); // Ensure non-negative
    }

    // Explicit conversion only - prevents accidental mixing of ints and NodeIds
    public static explicit operator int(NodeId nodeId) => nodeId.value;
    public static explicit operator NodeId(int value) => new NodeId(value);

    public bool Equals(NodeId other) => value == other.value;
    public override bool Equals(object obj) => obj is NodeId other && Equals(other);
    public override int GetHashCode() => value.GetHashCode();
    
    public int CompareTo(NodeId other) => value.CompareTo(other.value);
    
    public static bool operator ==(NodeId left, NodeId right) => left.Equals(right);
    public static bool operator !=(NodeId left, NodeId right) => !left.Equals(right);
    public static bool operator <(NodeId left, NodeId right) => left.CompareTo(right) < 0;
    public static bool operator >(NodeId left, NodeId right) => left.CompareTo(right) > 0;
    public static bool operator <=(NodeId left, NodeId right) => left.CompareTo(right) <= 0;
    public static bool operator >=(NodeId left, NodeId right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"Node({value})";
    
    /// <summary>
    /// Validates that this NodeId is within the valid range for the given node count.
    /// </summary>
    public bool IsValid(int nodeCount) => value >= 0 && value < nodeCount;
    
    /// <summary>
    /// Gets the next NodeId in sequence, or null if at the end.
    /// </summary>
    public NodeId? GetNext(int nodeCount)
    {
        int nextValue = value + 1;
        return nextValue < nodeCount ? new NodeId(nextValue) : null;
    }
    
    /// <summary>
    /// Gets the previous NodeId in sequence, or null if at the beginning.
    /// </summary>
    public NodeId? GetPrevious()
    {
        return value > 0 ? new NodeId(value - 1) : null;
    }
    
    /// <summary>
    /// Creates a NodeId with validation against the given node count.
    /// Returns null if the value is out of range.
    /// </summary>
    public static NodeId? CreateValidated(int value, int nodeCount)
    {
        if (value < 0 || value >= nodeCount) return null;
        return new NodeId(value);
    }

    /// <summary>
    /// Creates a NodeId for the first node (index 0).
    /// </summary>
    public static NodeId First => new NodeId(0);
    
    /// <summary>
    /// Creates a NodeId for the last node in a collection of given size.
    /// </summary>
    public static NodeId Last(int nodeCount) => new NodeId(Mathf.Max(0, nodeCount - 1));
}