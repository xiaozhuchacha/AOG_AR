using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System;


public delegate int TreeVisitor(Node nodeData);
public delegate void HighLigher(Node nodeData, bool b);

public class SimpleTree {
	public static string nextActionFirstHalf;
	public static string nextActionSecondHalf;

	public Node data;
    public LinkedList<SimpleTree> children;
    private SimpleTree parent;
    private int childrenNumber;
    private int siblingIndex;	// starts from 1


    public SimpleTree(Node data)
    {
        this.data = data;
        children = new LinkedList<SimpleTree>();
        this.parent = null;
        childrenNumber = 0;
        int siblingIndex = 1;
    }

    public SimpleTree(Node data, SimpleTree parent)
    {
        this.data = data;
        children = new LinkedList<SimpleTree>();
        this.parent = parent;
        this.siblingIndex = parent.childrenNumber + 1;
        childrenNumber = 0;
    }

    private SimpleTree(Node data, LinkedList<SimpleTree> children, SimpleTree parent)
    {
        this.data = data;
        this.children = children;
        this.parent = parent;
        this.siblingIndex = parent.childrenNumber + 1;
        childrenNumber = 0;
    }

    public void AddChild(Node data)
    {
        children.AddLast(new SimpleTree(data, this));
        childrenNumber ++;
    }

    // this child to add has the same children as another tree node
    public void AddChild(Node data, SimpleTree anotherTreeNode){
        children.AddLast(new SimpleTree(data, anotherTreeNode.children, this));
        childrenNumber ++;
    }

    // index starts from 1
    public SimpleTree GetChild(int i)
    {
    	int counter = 0;
        foreach (SimpleTree n in children)
            if (++counter == i)
                return n;
		Debug.Log("only "+ childrenNumber + " children, cannot find child " + i);
        return null;
    }

    static public void Traverse(SimpleTree node, TreeVisitor visitor)
    {
  // ode> kid in node.children)
	 //            Traverse(kid, visitor);
  //   	}
    }

    public string getActionName(){
    	return data.getActionText().text;
    }


    public void highLightNode (bool highLight){
		if (parent == null){
			// don't highlight the box when the node is root
			return;
    	}
    	// highlightFunc(data, highLight);
    	data.highlightBox(highLight);
    }

    // change the color of all the lines that
    // form the path from the root node to this end node
    public void highLightNodePath (bool highLight){
		if (parent == null){
			// final case
			// root node has no parent
    		return;
    	}
    	// highlightFunc(data, highLight);
    	data.switchLineVisual(highLight);
    	parent.highLightNodePath(highLight);

    }

    private string simpleOrNodeChildrenName(){
    	string percent = " " + 100.0f / childrenNumber + "% ";
    	string ret = "";
    	foreach (SimpleTree kid in children){
    		ret += kid.getActionName();
    		ret += percent;
    	}
    	return ret;
    }

    // flag means that the node is explored
    // if flag, stop looking at the children
    // look at the next sibling or parent
    static public SimpleTree TravelToNextEndNode(SimpleTree node, bool flag){
    	if (!flag){
    		// this node has not been visited / children explored
    		if (node.data.option == NodeOptions.EndNode){
    			nextActionFirstHalf = "Next Action: " + node.getActionName();
        		return node;	//final case, found the next end node			
    		}
			else if (node.data.option == NodeOptions.OrNode){
    			// find an OR node
    			// visit one of its children
    			nextActionSecondHalf = "    Chosen From: \n    " + node.simpleOrNodeChildrenName();

    			int childToChoose = Random.Range(1, 1 + node.childrenNumber);
                // int childToChoose = 2;
    			return TravelToNextEndNode(node.GetChild(childToChoose), false);
    		} else {
    			// find an and node
    			// explore its children
    			// start from the first one
    			return TravelToNextEndNode(node.GetChild(1), false);
    		}
    	} 
    	else 
    	{
    		// this node has been visited
    		if (node.data.option == NodeOptions.RootNode){
    			// if the root node is marked visited, then the tree if all explored
    			// there is no next action
    			return null;
    		}

    		SimpleTree p = node.parent;
    		if (p.data.option == NodeOptions.AndNode || p.data.option == NodeOptions.RootNode){
	    		// first visit all its siblings
    			if (node.siblingIndex < p.childrenNumber){
	    			return TravelToNextEndNode(p.GetChild(node.siblingIndex + 1), false);
	    		} 
	    		// else the all the siblings have been visited
	    		// this is the same case as the parent is an or node
    		}

			//  parent is or node or all siblings visited
			// then mark the parent visited
			return TravelToNextEndNode(p, true);
    	}

    }

}
