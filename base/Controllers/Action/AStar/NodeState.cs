﻿namespace AStar
{
    /// <summary>
    /// Represents the search state of a Node
    /// </summary>
    public enum NodeState
    {
        /// <summary>
        /// The node has been identified as a possible step in a path
        /// </summary>
        Open,
        /// <summary>
        /// The node has already been included in a path and will not be considered again
        /// </summary>
        Closed
    }
}