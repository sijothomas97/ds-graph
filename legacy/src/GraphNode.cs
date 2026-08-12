using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskB
{
    public class GraphNode
    {
        private string name; // data stored in the node.  
        private LinkedList<string> adjList; // list of the IDs of the nodes that are adjacent 

        // constructor of a GraphNode (pass the id of the node to be constructed) 
        public GraphNode(string name)
        {
            this.name = name;
            adjList = new LinkedList<string>();
        }

        // set and get the data stored in the node  
        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        //add edge from this node to the node "to";  
        // it is a *directed* graph.  
        public void AddEdge(GraphNode to)
        {
            adjList.AddLast(to.Name);
        }

        // return the adjacent list of the node 
        public LinkedList<string> GetAdjList()
        {
            return adjList;
        }
    }  //end of class GraphNode
}
