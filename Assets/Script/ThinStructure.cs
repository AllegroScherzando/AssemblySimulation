using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Edge {
    public Edge(int idx1, int idx2) {
        if (idx1 > idx2) {
            int temp = idx1;
            idx1 = idx2;
            idx2 = temp;
        }
        this.idx1 = idx1;
        this.idx2 = idx2;
    }
    public int idx1;
    public int idx2;
}
public class ThinStructure : MonoBehaviour {
    public static List<int> ordering;
    public static List<Vector3> orderSplitNorms;
    public static List<Vector3> vertices;
    public static List<Edge> edges;
    public static List<Vector3> edgeVecs;
    public static List<Vector3> edgeCents;
    public static List<Vector3> splitNorms;
    public static List<GameObject> verticeGOs;
    public static List<GameObject> edgeGOs;
    public static List<GameObject> objGOs;
    public static int verticeNum;
    public static int edgeNum;
    public static int orderingNum;
    float div = 20;
    float tubesize = 0.5f;
    int curOrderingSplit = 0;
    // Use this for initialization
    void Start () {
        vertices = new List<Vector3>();
        edges = new List<Edge>();
        splitNorms = new List<Vector3>();
        edgeVecs = new List<Vector3>();
        edgeCents = new List<Vector3>();
        verticeGOs = new List<GameObject>();
        edgeGOs = new List<GameObject>();
        objGOs = edgeGOs = new List<GameObject>();
        ordering = new List<int>() ;
        orderSplitNorms = new List<Vector3>();
        //genByMyself();
        loadObj();
        genOrdering();
    }

    void genOrdering() {
        readOrdering();
        for (int i = 1; i <= orderingNum * 2; i++)
        {
            loadObj(i);
            changeColor(i - 1, 0);
        }
        changeColor(ordering[0], 1);
        changeColor(ordering[0] + orderingNum, 1);
    }
    void genByMyself() {
        read();
        readCut();
        put();
    }

    private void loadObj()
    {
        UnityEngine.UI.Dropdown dd = GameObject.Find("Canvas/Dropdown").GetComponent<UnityEngine.UI.Dropdown>();
        int curObj = dd.value;
        GameObject obj = Instantiate(Resources.Load("Glass"), Vector3.zero, Quaternion.identity) as GameObject;
        Mesh holderMesh = new Mesh();
        ObjImporter newMesh = new ObjImporter();
        holderMesh = newMesh.ImportFile("./inputSet/"+ curObj + "/inputObj/output_0" + ".obj");
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        holderMesh.RecalculateNormals();
        filter.mesh = holderMesh;
        obj.transform.parent = GameObject.Find("Collect").transform;
        obj.name = "Obj-0";
    }

    private void loadObj(int taridx)
    {
        UnityEngine.UI.Dropdown dd = GameObject.Find("Canvas/Dropdown").GetComponent<UnityEngine.UI.Dropdown>();
        int curObj = dd.value;
        GameObject obj = Instantiate(Resources.Load("Empty"), Vector3.zero, Quaternion.identity) as GameObject;
        Mesh holderMesh = new Mesh();
        ObjImporter newMesh = new ObjImporter();
        holderMesh = newMesh.ImportFile("./inputSet/" + curObj + "/inputObj/output_" + taridx + ".obj");
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        MeshFilter filter = obj.GetComponent<MeshFilter>();
        holderMesh.RecalculateNormals();
        filter.mesh = holderMesh;
        obj.transform.parent = GameObject.Find("Collect").transform;
        obj.name = "Obj-" + taridx;
        objGOs.Add(obj);
    }
    void changeColor(int idx, int state) {
        GameObject obj = objGOs[idx];
        if (state == 2) obj.GetComponent<Renderer>().material.color = Color.gray;
        else if (state == 1) obj.GetComponent<Renderer>().material.color = Color.green;
        else if (state == 0) obj.GetComponent<Renderer>().material.color = Color.red;
        else if(state == -1) obj.GetComponent<Renderer>().material.color = new Color(1,1,1,0);
    }
    void readOrdering()//no detect space
    {
        string line;
        string[] items;
        UnityEngine.UI.Dropdown dd = GameObject.Find("Canvas/Dropdown").GetComponent<UnityEngine.UI.Dropdown>();
        int curObj = dd.value;
        System.IO.StreamReader file = new System.IO.StreamReader("./inputSet/" + curObj + "/input/ordering.txt");
        line = file.ReadLine(); items = line.Split(' ');
        orderingNum = int.Parse(items[0]);

        for (int i = 0; i < orderingNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            ordering.Add(int.Parse(items[0]));
        }

        for (int i = 0; i < orderingNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            orderSplitNorms.Add(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
        }
        file.Close();
    }
    public void splitAndNext() {
        if (curOrderingSplit >= orderingNum) return;
        spliting = ordering[curOrderingSplit];
        splitingTime = splitingTimeSamp;
        curOrderingSplit++;
        if (curOrderingSplit < orderingNum) nextSpliting = ordering[curOrderingSplit];
        else nextSpliting = -1;
    }
    int spliting = -1;
    int nextSpliting = -1;
    float splitingTime = 0;
    float splitingTimeSamp = 0.35f;
    float splitSpeed = 0.1f;
    float splitSpeedup = 2;
    float splitSpeedTune = 10;
    bool destoryOrNot = false;
    bool destoryed = false;
    // Update is called once per frame
    void Update () {
        if (spliting >= 0) {
            if (!destoryed&& curOrderingSplit > 1)
            {
                Destroy(objGOs[ordering[curOrderingSplit - 2]]);
                Destroy(objGOs[ordering[curOrderingSplit - 2] + orderingNum]);
                destoryed = true;
            }
            GameObject obj1 = objGOs[spliting];
            GameObject obj2 = objGOs[spliting + orderingNum];
            Vector3 up = orderSplitNorms[spliting].normalized;
            Vector3 down = orderSplitNorms[spliting].normalized*-1;
            if (splitingTime > 0)
            {
                obj1.transform.position += up * splitSpeed * Mathf.Pow((splitingTimeSamp - splitingTime)/ splitingTimeSamp * splitSpeedTune, splitSpeedup);
                obj2.transform.position += down * splitSpeed * Mathf.Pow((splitingTimeSamp - splitingTime)/ splitingTimeSamp * splitSpeedTune, splitSpeedup);
                splitingTime -= Time.deltaTime;
            }
            else {
                if (destoryOrNot)
                {
                    Destroy(objGOs[spliting]);
                    Destroy(objGOs[spliting + orderingNum]);
                }
                else {
                    changeColor(spliting, 2);
                    changeColor(spliting + orderingNum, 2);
                    destoryed = false;
                }

                if (nextSpliting >= 0) {
                    changeColor(nextSpliting, 1);
                    changeColor(nextSpliting + orderingNum, 1);
                }
                splitingTime = 0;
                spliting = -1;
            }

        }
	}

    public void restart() {
        GameObject collect = GameObject.Find("Collect");
        for (int i = 0; i < collect.transform.childCount; i++)
        {
            GameObject go = collect.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        vertices.Clear();
        edges.Clear();
        splitNorms.Clear();
        edgeVecs.Clear();
        edgeCents.Clear();
        verticeGOs.Clear();
        edgeGOs.Clear();
        objGOs.Clear();
        ordering.Clear();
        orderSplitNorms.Clear();
        //genByMyself();
        loadObj();
        genOrdering();
        curOrderingSplit = 0;
    }

    void read() {

        string line;
        string[] items;
        System.IO.StreamReader file = new System.IO.StreamReader(".\\input\\thinstruct.txt");
        line = file.ReadLine();items = line.Split(' ');
        verticeNum = int.Parse(items[0]);
        edgeNum = int.Parse(items[1]);
        for (int i = 0; i < verticeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <=1) { i--; continue; }
            vertices.Add(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
        }
        for (int i = 0; i < edgeNum; i++) {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            edges.Add(new Edge(int.Parse(items[0]), int.Parse(items[1])));
        }
        file.Close();
    }

    void readCut()
    {
        string line;
        string[] items;
        System.IO.StreamReader file = new System.IO.StreamReader(".\\input\\splitinfo.txt");
        for (int i = 0; i < edgeNum; i++)
        {
            line = file.ReadLine(); items = line.Split(' ');
            if (items.Length <= 1) { i--; continue; }
            splitNorms.Add(new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2])));
        }
        file.Close();
    }

    void put() {
        for (int i = 0; i < verticeNum; i++)
        {
            Vector3 vertice = vertices[i];
            GameObject go = GameObject.Instantiate(Resources.Load("Sphere"), vertice / div, Quaternion.identity) as GameObject;
            go.transform.localScale = new Vector3(tubesize, tubesize, tubesize);
            go.transform.parent = GameObject.Find("Collect").transform;
            verticeGOs.Add(go);
        }
        for (int i = 0; i < edgeNum; i++)
        {
            Edge edge = edges[i];
            Vector3 v1 = vertices[edge.idx1];
            Vector3 v2 = vertices[edge.idx2];
            Vector3 vec = (v2 - v1).normalized; edgeVecs.Add(vec);
            Vector3 cent = (v2 + v1)/2; edgeCents.Add(cent);
            Vector3 norm = splitNorms[i].normalized;
            norm = Vector3.Cross(Vector3.Cross(vec, norm), vec);
            Quaternion fromto = Quaternion.FromToRotation(new Vector3(0, 1, 0), vec);
            Quaternion fromto2 = Quaternion.LookRotation(norm, vec);

            GameObject go = GameObject.Instantiate(Resources.Load("Column"), cent / div, fromto2) as GameObject;
            go.transform.localScale = new Vector3(tubesize, (v1-v2).magnitude/div/2, tubesize);
            go.transform.parent = GameObject.Find("Collect").transform;
            edgeGOs.Add(go);
        }
    }
}
