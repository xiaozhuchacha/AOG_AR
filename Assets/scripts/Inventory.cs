using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using HoloToolkit.Unity.InputModule;

public class Inventory : MonoBehaviour {
    public List<Transform> layer1 = new List<Transform>();
    [SerializeField] Transform layer2;
    public List<Transform> layer3 = new List<Transform>();
    [SerializeField] Transform ActionList;
    public List<Transform> backup;
    public Transform orNode;
    public Button start;
    public Text text;
    //public Text text1;
    //public Text text2;
    //public Text text3;
    public 
    //public Transform pos1;
    string[] action2 = { "", "" };
    static string[] noaction1 = { "", "" };
    static string[] noaction2 = { "", "" };
    List<string[]> action3 = new List<string[]>() {noaction1, noaction2};
    private Slot[] slotList;
    private bool editable = false;
    private GameObject nodeSidePanel = null;
    private GraphicRaycaster gr = null;
    private GameObject actionListCanvas = null;
    private bool isShowingActionList = false;

    private SimpleTree treeRoot = null;
    public Transform lineObjectRoot = null;
    public Text statusText = null;

    public static Sprite redBox = null;
    public static Sprite whiteBox = null;
    public static Sprite redLine = null;
    public static Sprite redCurve = null;

    private static SimpleTree currentAction;
    private static SimpleTree nextAction;

    public GameObject manager = null;
    private TCPManager udpManager = null;

    public static bool alwaysSecondChild = false;

    public static void toggleChildChoice() {
        alwaysSecondChild = !alwaysSecondChild;
    }



    private void initStepper(){
        currentAction = treeRoot;
        nextAction = treeRoot.GetChild(1);
    }

    private void resetStepper(){
        nextAction = treeRoot.GetChild(1);    
    }

    // go from the one end node to the next end node in the AOG graph
    // un-highlight the previous step
    // highlight the current step
    public void goOneStep(){
        currentAction.highLightNode(false);
        nextAction.highLightNodePath(false);
        SimpleTree.nextActionSecondHalf = "";       

        currentAction = nextAction;


        nextAction = SimpleTree.TravelToNextEndNode(nextAction, true);

        currentAction.highLightNode(true);

        if (nextAction == null){
            SimpleTree.nextActionFirstHalf = "Sequence Finished!";
            resetStepper();
        }
        nextAction.highLightNodePath(true);

        Debug.Log(SimpleTree.nextActionFirstHalf);
        Debug.Log(SimpleTree.nextActionSecondHalf);

        statusText.text = SimpleTree.nextActionFirstHalf + "\n" + SimpleTree.nextActionSecondHalf;

        udpManager.executeAction(currentAction.getActionName().ToLower());
    }
    

    // creates a tree of Slots
    // the slots don't change so this only needs to run once
    // to update, traverse to check if there is item under the slot
    private void createTree()
    {
        // building up the tree
        Node tempRef = new Node(NodeOptions.RootNode);

        treeRoot = new SimpleTree(tempRef);

        tempRef = new Node(layer1[0].gameObject, lineObjectRoot.Find("11").gameObject, NodeOptions.EndNode);
        treeRoot.AddChild(tempRef);
        tempRef = new Node(layer1[1].gameObject, lineObjectRoot.Find("12").gameObject, NodeOptions.AndNode, true);
        treeRoot.AddChild(tempRef);
        tempRef = new Node(layer1[2].gameObject, lineObjectRoot.Find("13").gameObject, NodeOptions.EndNode);
        treeRoot.AddChild(tempRef);
        tempRef = new Node(layer1[3].gameObject, lineObjectRoot.Find("14").gameObject, NodeOptions.EndNode);
        treeRoot.AddChild(tempRef);
        tempRef = new Node(layer1[4].gameObject, lineObjectRoot.Find("15").gameObject, NodeOptions.AndNode);
        // treeRoot.AddChild(tempRef, treeRoot.GetChild(2));
        treeRoot.AddChild(tempRef);
        tempRef = new Node(layer1[5].gameObject, lineObjectRoot.Find("16").gameObject, NodeOptions.EndNode);
        treeRoot.AddChild(tempRef);
        // Debug.Log(tempRef.getActionText().text);


        SimpleTree tempAndNode = treeRoot.GetChild(2);

        tempRef = new Node(layer2.Find("1").gameObject, lineObjectRoot.Find("21").gameObject, NodeOptions.OrNode);
        tempAndNode.AddChild(tempRef);
        tempRef = new Node(layer2.Find("2").gameObject, lineObjectRoot.Find("22").gameObject, NodeOptions.EndNode);
        tempAndNode.AddChild(tempRef);

        SimpleTree tempOrNode = tempAndNode.GetChild(1);
        tempRef = new Node(layer3[0].Find("1").gameObject, lineObjectRoot.Find("311").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);        
        tempRef = new Node(layer3[0].Find("2").gameObject, lineObjectRoot.Find("312").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef); 

        tempOrNode = tempAndNode.GetChild(2);
        tempRef = new Node(layer3[1].Find("1").gameObject, lineObjectRoot.Find("321").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);
        tempRef = new Node(layer3[1].Find("2").gameObject, lineObjectRoot.Find("322").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);

        tempAndNode = treeRoot.GetChild(5);

        tempRef = new Node(layer2.Find("1").gameObject, lineObjectRoot.Find("21").gameObject, NodeOptions.OrNode);
        tempAndNode.AddChild(tempRef);
        tempRef = new Node(layer2.Find("2").gameObject, lineObjectRoot.Find("22").gameObject, NodeOptions.EndNode);
        tempAndNode.AddChild(tempRef);

        tempOrNode = tempAndNode.GetChild(1);
        tempRef = new Node(layer3[0].Find("1").gameObject, lineObjectRoot.Find("311").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);        
        tempRef = new Node(layer3[0].Find("2").gameObject, lineObjectRoot.Find("312").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef); 

        tempOrNode = tempAndNode.GetChild(2);
        tempRef = new Node(layer3[1].Find("1").gameObject, lineObjectRoot.Find("321").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);
        tempRef = new Node(layer3[1].Find("2").gameObject, lineObjectRoot.Find("322").gameObject, NodeOptions.EndNode);
        tempOrNode.AddChild(tempRef);

    }

    public void toggleEditAOG(){
        editable = !editable;
        setEditableAOG();
    }

    private void setEditableAOG(){
        nodeSidePanel.SetActive(editable);
        gr.enabled = editable;
    }

    public void toggleActionList(){
        isShowingActionList = !isShowingActionList;
        actionListCanvas.SetActive(isShowingActionList);
        if(isShowingActionList){
            // place the action list below the AOG actionListCanvas
            actionListCanvas.transform.position = transform.position + new Vector3(0, -0.3f, 0);
        }
    }

    void Update() {
    }

    void Start () {
        redLine = Resources.Load("redStraightArrow", typeof(Sprite)) as Sprite;
        redCurve = Resources.Load("redCurveArrow", typeof(Sprite)) as Sprite;
        redBox = Resources.Load("redBox", typeof(Sprite)) as Sprite;
        whiteBox = Resources.Load("whiteBox", typeof(Sprite)) as Sprite;

        createTree();
        HasChanged();

        findAllSlots();
        nodeSidePanel = transform.Find("SelectNodePanel").gameObject;
        if (nodeSidePanel == null){
            Debug.Log("ERROR: cannot find component SelectNodePanel");
        }
        gr = GetComponent<GraphicRaycaster>();
        setEditableAOG();
        actionListCanvas = ActionList.transform.parent.gameObject;
        actionListCanvas.SetActive(isShowingActionList);
        initStepper();
        statusText.text = "";

        udpManager = manager.GetComponent<TCPManager>();
	}

    private void findAllSlots(){
        slotList = FindObjectsOfType(typeof(Slot)) as Slot[];
    }


    // if the item is dropped in a slot area
    // assign the transform of the slot to be the parent transform of the dropped item 
    // and call HasChanged() 
    public void dropToSlot(Transform t){
        Transform oldParent = t.parent;
        foreach (Slot s in slotList){
            RectTransform slotTransform = (RectTransform)s.gameObject.transform;
            t.SetParent(slotTransform);
            if (slotTransform.rect.Contains(t.localPosition)){
                // Debug.Log(slotTransform.rect);
                // Debug.Log("inside");
                // Debug.Log(layer2.Find((1).ToString()).GetComponent<Slot>().item.name);
                HasChanged();
                // ifChanged = true;
                return;
            }
        }
        // Debug.Log("not inside");
        t.SetParent(oldParent);
    }



    
    public void HasChanged()
    {
        int cnt = 0;
        SimpleTree layer1And = treeRoot.GetChild(2);
        SimpleTree layer1And1 = treeRoot.GetChild(5);

        foreach (Transform slotTransform in layer2)
        {

            GameObject item = layer2.Find((cnt+1).ToString()).GetComponent<Slot>().item;
            GameObject item0 = layer3[cnt].GetChild(0).GetComponent<Slot>().item;
            GameObject item1 = layer3[cnt].GetChild(1).GetComponent<Slot>().item;
            SimpleTree layer3Or = layer1And.GetChild(cnt+1);
            SimpleTree layer3Or1 = layer1And1.GetChild(cnt+1);
            if (item && item.name == "OrNode")
            {

                if (item0 && !item1)
                {
                    layer2.transform.GetChild(cnt).GetComponent<Slot>().item.transform.parent = backup[cnt];
                    layer3[cnt].transform.GetChild(0).GetComponent<Slot>().item.transform.parent = slotTransform;
                    layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(false);
                    layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(false);
                    action3[cnt][0] = action3[cnt][1] = item0.name;

                    layer3Or.data.option = NodeOptions.EndNode;
                    layer3Or1.data.option = NodeOptions.EndNode;
                    Debug.Log("changed to endnode " + cnt);        
                }
                else if (!item0 && item1)
                {
                    layer2.transform.GetChild(cnt).GetComponent<Slot>().item.transform.parent = backup[cnt];
                    layer3[cnt].transform.GetChild(1).GetComponent<Slot>().item.transform.parent = slotTransform;
                    layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(false);
                    layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(false);
                    action3[cnt][0] = action3[cnt][1] = item1.name;

                    layer3Or.data.option = NodeOptions.EndNode;
                    layer3Or1.data.option = NodeOptions.EndNode;
                    Debug.Log("changed to endnode " + cnt);       
                }

                else
                {
                    layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(item0 != null);
                    layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(item1 != null);
                    if (item0)  // both item0 and item1 exist
                    {
                        // to do:
                        // change here to use the Text component content instead of object name
                        action3[cnt][0] = item0.name;
                        action3[cnt][1] = item1.transform.name;
                    }
                }
            }
            else   // the node is not currently or node 
            {                
                if (!item) // if the content in the node is dragged away
                {
                    layer3Or.data.option = NodeOptions.OrNode;
                    layer3Or1.data.option = NodeOptions.OrNode;
                    Debug.Log("change to OrNode " + cnt );
                    if (item0 && !item1)
                    {
                        //layer2.transform.GetChild(cnt).GetComponent<Slot>().item.transform.parent = layer3[1];
                        backup[cnt].GetComponent<Slot>().item.transform.parent = slotTransform;
                        layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(true);
                        layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(false);

                        action3[cnt][0] = item0.transform.name;
                        action3[cnt][1] = item0.transform.name;

                    }
                    else if (!item0 && item1)
                    {
                        //layer2.transform.GetChild(cnt).GetComponent<Slot>().item.transform.parent = layer3[0];
                        backup[cnt].GetComponent<Slot>().item.transform.parent = slotTransform;
                        layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(false);
                        layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(true);
                        action3[cnt][1] = item1.transform.name;
                        action3[cnt][0] = item1.transform.name;
                    }
                    else
                    {
                        layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(item0 != null);
                        layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(item1 != null);
                    }
                }
                else    // if the node is an end node
                {

                    layer3[cnt].GetChild(0).GetComponent<Slot>().arrow.SetActive(item0 != null);
                    layer3[cnt].GetChild(1).GetComponent<Slot>().arrow.SetActive(item1 != null);
                    action3[cnt][0] = action3[cnt][1] = layer2.GetChild(cnt).GetComponent<Slot>().item.transform.name;
                }
            }

            // if (layer2.GetChild(1).GetComponent<Slot>().item)
            // {
            //     text.text = layer2.GetChild(1).GetComponent<Slot>().item.transform.name;
            // }
            action2[cnt] = layer2.GetChild(cnt).GetComponent<Slot>().item.transform.name;
            cnt++; 
        }
    }

    public void parse()
    {
        int[] waitIdx = { 1,2,5,6}; 
        ActionList.GetChild(1).GetComponent<Image>().transform.GetChild(0).GetComponent<Text>().text = action3[0][Random.Range(0,2)];
        ActionList.GetChild(2).GetComponent<Image>().transform.GetChild(0).GetComponent<Text>().text = action3[1][Random.Range(0, 2)];
        ActionList.GetChild(5).GetComponent<Image>().transform.GetChild(0).GetComponent<Text>().text = action3[0][Random.Range(0, 2)];
        ActionList.GetChild(6).GetComponent<Image>().transform.GetChild(0).GetComponent<Text>().text = action3[1][Random.Range(0, 2)];
    
    }
}