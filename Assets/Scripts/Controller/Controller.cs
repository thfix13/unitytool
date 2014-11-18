using Common;
using Exploration;
using System.Collections.Generic;

namespace RRTController {

	public interface Controller {

		void onStart (Node start, RRTKDTreeCombat context);

		/// <summary>
		/// After a point is sampled, this method is called, which should block the continuation of the execution or not.
		/// Changes on either nodes are not recommended.
		/// </summary>
		/// <returns><c>true</c>, if the sampled node is valid, <c>false</c> if this node should be discarted.</returns>
		/// <param name="sampled">The sampled node.</param>
		/// <param name="closest">The closest node to the node that was sampled.</param>
		bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context);

		/// <summary>
		/// Before the line of sight between the 'from' and 'to' nodes, 
		/// some of the cells that should not be ignored must be added to the LoS computation.
		/// </summary>
		/// <returns>The line of sight.</returns>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="context">Context.</param>
		List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context);

		bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context);

		/// <summary>
		/// Before the RRT connects the 'from' node to the 'to' node, this is invoked to modify the connection between those nodes.
		/// A new node may be returned to replace the 'to' node, but the 'from' node can't be modified.
		/// The new returned node will be the one passed to the next instances of 'Controller's and will replace the 'to' node.
		/// Changes done on the 'from' node are not recommended.
		/// </summary>
		/// <returns>'null' if no new node should be returned, or an 'Node' instance to the node that should replace the 'to' node.</returns>
		/// <param name="from">The node that is connecting from.</param>
		/// <param name="to">The target node that the from is connecting to.</param>
		/// <param name="hit">The node that was returned from the LoS computation. May be null if it didn't collide with anything.</param>
		Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context);
		
		/// <summary>
		/// After the node is connected, other actions may be taken to guarantee the best desired behaviour.
		/// A new node may be returned to replace the 'to' node, but the 'from' node can't be modified.
		/// The new returned node will be the one passed to the next instances of 'Controller's as the new 'to' node, the current 'to' node will be passed as the new 'from' node. The returned node will be added to the Tree.
		/// Changes done on the 'from' node are not recommended.
		/// </summary>
		/// <returns>'null' if no new node should be returned, or an 'Node' instance to the node that should replace the 'to' node.</returns>
		/// <param name="from">The node that is connecting from.</param>
		/// <param name="to">The target node that the from is connecting to.</param>
		Node afterConnect (Node from, Node to, RRTKDTreeCombat context);
	}
}

