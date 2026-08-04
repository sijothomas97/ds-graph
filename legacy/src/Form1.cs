using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskB
{
    public partial class Form1 : Form
    {
        Graph graph = new Graph();
        List<string> allFriends = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void Refresh()
        {
            tbName.Clear();
            cbFirstPerson.Text = "";
            cbSecondPerson.Text = "";
            
            //lbNames.Items.Add()
            //lbNames.Items.Clear();
            lbNames.DataSource = graph.AllNames();
            cbDeletePerson.DataSource = lbNames.DataSource;
            cbDeletePerson.Text = "";
            labEdgesCount.Text = graph.EdgesCount.ToString();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbName.Text))
            {
                graph.AddNode(tbName.Text);
                cbFirstPerson.Items.Add(tbName.Text);
                cbSecondPerson.Items.Add(tbName.Text);
                Refresh();
            }
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            string firstPerson = cbFirstPerson.Text;
            string secondPerson = cbSecondPerson.Text;

            if (!string.IsNullOrEmpty(firstPerson) && !string.IsNullOrEmpty(secondPerson))
                graph.AddEdge(firstPerson, secondPerson);
            Refresh();
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            string person = tbDirectFriends.Text;
            //lbDirectFriends.Items.Clear();
            if (!string.IsNullOrEmpty(person))
                lbDirectFriends.DataSource = graph.DirectFriends(person);
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            string deletePersonName = cbDeletePerson.Text;
            
            if (!string.IsNullOrEmpty(deletePersonName))
            {
                GraphNode nodeToDelete = graph.GetNodeByID(deletePersonName);
                graph.DeleteNode(nodeToDelete, graph);
                Refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nameToSearch = tbDirectFriends.Text;
            allFriends.Clear();
            
            if (!string.IsNullOrEmpty(nameToSearch))
            {
                graph.DepthFirstTraverse(nameToSearch, ref allFriends);

                allFriends.Remove(nameToSearch);
                lbDirectFriends.DataSource = null;
                lbDirectFriends.DataSource = allFriends;
            }
            //else
            //{
            //    MessageBox("Enter a name");
            //}
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            lbDirectFriends.DataSource = null;
            tbDirectFriends.Text = string.Empty;
        }
    }
}
