using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NodeOptions
{
    RootNode = 0,
    AndNode = 1,
    OrNode = 2,
    EndNode = 3
}

public class Node
    {
        public GameObject self;
        public GameObject lineToObject;
        public NodeOptions option;
        private Sprite altLine;
        private Sprite defaultLine;
        private static Sprite defaultBox = Inventory.whiteBox;
        private static Sprite altBox = Inventory.redBox;

        public Transform child{
            get {
                if (self.transform.childCount > 0){
                    return self.transform.GetChild(0);
                }
                return null;
            }
        }

        public Node(GameObject self, GameObject line, NodeOptions option, bool flag = false)
        {
            this.option = option;
            this.self = self;
            this.lineToObject = line;
            if (flag){
                 this.altLine = Inventory.redCurve;
            } else {
                this.altLine = Inventory.redLine;            
            }
            defaultLine = lineToObject.GetComponent<Image>().sprite;
        }

        // change the color of the line that points to this node
        public void switchLineVisual(bool highlight){
        	Sprite placeHolder;
            if (highlight){
                 placeHolder = altLine;
            } else {
                placeHolder = defaultLine;
            }

            lineToObject.GetComponent<Image>().sprite = placeHolder;
        }

        // assuming the node has a child with the box
        public void highlightBox(bool highlight){
        	if (highlight){
        		child.GetComponent<Image>().sprite = altBox;
        	} else{
        		child.GetComponent<Image>().sprite = defaultBox;
        	}
        }

        public Node(NodeOptions option)
        {
            this.option = option;
            this.self = null;
            this.lineToObject = null;
            this.altLine = null;
        }

        public Text getActionText(){
            if (option != NodeOptions.EndNode){
                return null;
            } 
            if (child == null){
                return null;
            }
            return child.GetChild(0).GetComponent<Text>();
        }

    }