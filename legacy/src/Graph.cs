using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskB
{
    // class Graph: represents the entire Graph 
    public class Graph
    {
        // list of graphnodes in the graph (represents all the nodes present in the graph) 
        private LinkedList<GraphNode> nodes;
        private int edgesCount;

        // constructor of a Graph. Construct a graph and set  
        // the list of nodes in the graph to be the empty list; 
        // the constructed graph has no nodes 
        public Graph()
        {
            nodes = new LinkedList<GraphNode>();
            edgesCount = 0;
        }

        // Add a new node (with the specific id) to the graph. Use the constructor of graphnode 
        // "AddLast" adds the constructed graphnode to the list of all nodes. 
        // May also use "AddFirst" - as long as the new node is added to the list of the nodes  
        // it does not matter where it is added (first or last) 

        public int EdgesCount
        {
            get { return edgesCount; }
            set { edgesCount = value; }
        }

        public void AddNode(string name)
        {
            nodes.AddLast(new GraphNode(name));
        }

        //Return the graphnode with the specific id passed as input argument 
        public GraphNode GetNodeByID(string name)
        {
            //Search through the list of all nodes in the graph for the node with the specific id 
            foreach (GraphNode n in nodes)
            {
                // found the node n with the specific id; return n 
                if (name == n.Name)
                    return n;
            }

            // the node has not been found (no node with the specific id is present in the graph) 
            return null;
        }

        // add a directed edge between node with id= "from" and the node with id= "to"  
        public void AddEdge(string from, string to)
        {
            // get the graphnode that corresponds to the node with id = from 
            GraphNode n1 = GetNodeByID(from);

            // get the graphnode that corresponds to the node with id = to 
            GraphNode n2 = GetNodeByID(to);

            // add a directed edge going from node n1 to node 2  
            //(use the AddEdge method defined in the class GraphNode) 

            n1.AddEdge(n2);
            // Counting edges 
            edgesCount++;
        }
        // returns the total number of nodes present in the graph 

        public List<string> AllNames()
        {
            List<string> names = new List<string>();
            foreach (GraphNode n in nodes)
            {
                names.Add(n.Name);
            }
            return names;
        }

        // returns the total number of edges present in the graph 
        public List<string> DirectFriends(string name)
        {
            List<string> friends = new List<string>();
            foreach (GraphNode n in nodes)
            {
                // found the node n with the specific id; return n 
                if (name == n.Name)
                {
                    foreach (string s in n.GetAdjList())
                    {
                        friends.Add(s);
                    }
                    break;
                }    
            }   
            return friends;
        }

        public void DeleteNode(GraphNode node, Graph graph)
        {
            string nameToDelete = node.Name;
            //Deleting node from graph
            graph.nodes.Remove(node);

            foreach (GraphNode n in nodes)
                n.GetAdjList().Remove(nameToDelete);
        }

        //perform a DFS traversal starting at startID, leaving a list
        //of visited ID’s in the visited list. 

        public void DepthFirstTraverse(string startName, ref List<string> visited)
        {
            LinkedList<string> adj;
            Stack<string> toVisit = new Stack<string>();

            toVisit.Push(startName);

            //string currentName;
            GraphNode node;

            while (toVisit.Count != 0)
            {
                // get ID of current node from toVisit
                node = GetNodeByID(toVisit.Pop());
                // store ID of current node in “visited”
                visited.Add(node.Name);

                // get adjacency list of the current node
                foreach (string name in node.GetAdjList())
                {
                    //  add to  toVisit  each node n in the adjacency list
                    //  (only if name is not in “visited” and not in “toVisit”)
                    if (!visited.Contains(name) && !toVisit.Contains(name))
                        toVisit.Push(name);
                }
            }
        }
    }  // end of class Graph 
}
