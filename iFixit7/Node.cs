using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;// ADd to project

namespace iFixit7
{
    public class Node
    {
        private List<Node> children = new List<Node>();
        private string name;

        public Node(string Name, List<Node> Children = null)
        {
            name = Name;
            children = Children;
        }

        public string getName()
        {
            return name;
        }

        public List<Node> getChildrenList()
        {
            return children;
        }

        public Object getChildren(int childrenNum)
        {
            return children[childrenNum];
        }

        public void setName(string Name)
        {
            name = Name;
        }

        public void setChildrenList(List<Node> ChildrenList)
        {
            children = ChildrenList;
        }

        public void setChildren(int childrenNum, Node Children)
        {
            children[childrenNum] = Children;
        }
    }
}
