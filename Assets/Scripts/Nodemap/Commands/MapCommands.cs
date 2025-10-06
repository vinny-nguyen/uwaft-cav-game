using System;
using UnityEngine;

namespace Nodemap.Commands
{
    /// <summary>
    /// Command pattern interface for all map actions.
    /// Promotes loose coupling and enables features like undo/redo, queuing, etc.
    /// </summary>
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
    }

    /// <summary>
    /// Base implementation with common validation patterns.
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        protected readonly NodeId nodeId;

        protected BaseCommand(NodeId nodeId)
        {
            this.nodeId = nodeId;
        }

        public abstract bool CanExecute();
        public abstract void Execute();

        protected bool ValidateNodeId(int nodeCount)
        {
            return nodeId.IsValid(nodeCount);
        }
    }

    /// <summary>
    /// Commands for node interactions
    /// </summary>
    public class ClickNodeCommand : BaseCommand
    {
        private readonly Core.MapState mapState;
        private readonly Action<NodeId> onNodeClicked;

        public ClickNodeCommand(NodeId nodeId, Core.MapState mapState, Action<NodeId> onNodeClicked) 
            : base(nodeId)
        {
            this.mapState = mapState;
            this.onNodeClicked = onNodeClicked;
        }

        public override bool CanExecute()
        {
            return ValidateNodeId(mapState.NodeCount) && mapState.IsNodeUnlocked(nodeId);
        }

        public override void Execute()
        {
            if (!CanExecute()) return;
            onNodeClicked?.Invoke(nodeId);
        }
    }

    public class CompleteNodeCommand : BaseCommand
    {
        private readonly Core.MapState mapState;

        public CompleteNodeCommand(NodeId nodeId, Core.MapState mapState) : base(nodeId)
        {
            this.mapState = mapState;
        }

        public override bool CanExecute()
        {
            return ValidateNodeId(mapState.NodeCount) && 
                   mapState.IsNodeUnlocked(nodeId) && 
                   !mapState.IsNodeCompleted(nodeId);
        }

        public override void Execute()
        {
            if (!CanExecute()) return;
            mapState.TryCompleteNode(nodeId);
        }
    }

    /// <summary>
    /// Commands for car movement
    /// </summary>
    public class MoveCarCommand : BaseCommand
    {
        private readonly Core.MapState mapState;

        public MoveCarCommand(NodeId nodeId, Core.MapState mapState) : base(nodeId)
        {
            this.mapState = mapState;
        }

        public override bool CanExecute()
        {
            return ValidateNodeId(mapState.NodeCount) && mapState.IsNodeUnlocked(nodeId);
        }

        public override void Execute()
        {
            if (!CanExecute()) return;
            mapState.TryMoveCarTo(nodeId);
        }
    }

    /// <summary>
    /// Commands for UI actions
    /// </summary>
    public class OpenPopupCommand : BaseCommand
    {
        private readonly NodeData nodeData;
        private readonly Core.MapState mapState;
        private readonly Action<NodeData, bool> onOpenPopup;

        public OpenPopupCommand(NodeId nodeId, NodeData nodeData, Core.MapState mapState, Action<NodeData, bool> onOpenPopup) 
            : base(nodeId)
        {
            this.nodeData = nodeData;
            this.mapState = mapState;
            this.onOpenPopup = onOpenPopup;
        }

        public override bool CanExecute()
        {
            return ValidateNodeId(mapState.NodeCount) && 
                   nodeData != null && 
                   mapState.IsNodeUnlocked(nodeId);
        }

        public override void Execute()
        {
            if (!CanExecute()) return;
            bool isCompleted = mapState.IsNodeCompleted(nodeId);
            onOpenPopup?.Invoke(nodeData, isCompleted);
        }
    }

    public class ShakeNodeCommand : BaseCommand
    {
        private readonly Action<NodeId> onShakeNode;

        public ShakeNodeCommand(NodeId nodeId, Action<NodeId> onShakeNode) : base(nodeId)
        {
            this.onShakeNode = onShakeNode;
        }

        public override bool CanExecute()
        {
            return true; // Shake can always happen
        }

        public override void Execute()
        {
            onShakeNode?.Invoke(nodeId);
        }
    }

    /// <summary>
    /// Command executor that provides consistent execution with error handling.
    /// </summary>
    public static class CommandExecutor
    {
        public static bool TryExecute(ICommand command)
        {
            if (command?.CanExecute() == true)
            {
                try
                {
                    command.Execute();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Command execution failed: {ex.Message}");
                }
            }
            return false;
        }
    }
}