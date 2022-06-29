using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Custom
using OcTree;

//[ExecuteInEditMode]
public class SetUp : MonoBehaviour
{
    public int regionLength = 1;
    public int objectCount;

    GameObject region; // 用几何体代表的可视化空间
    List<GameObject> gos;
    List<TestObject> objs;

    private void Start()
    {
        BuildEnviroment();
    }

    private void OnDrawGizmos()
    {
        
    }

    // 
    // 1. 确定构建树的区域大小（统一使用正方体），并用预制体Region可视化，暴露Lenght。
    // 2. 确定对象的模型为预制体Object，将其随机分布在上面构建的Region中，暴露Count。
    // 3. 要与OcTree完全隔离。（特别注意不要将BoundingBox类写在这里）
    // 
    void BuildEnviroment()
    {
        // 定义
        gos = new List<GameObject>(objectCount);

        // 引入预制体 Region
        region = Instantiate(Resources.Load("Region")) as GameObject;

        // 设置region的大小
        region.transform.localScale = new Vector3(regionLength, regionLength, regionLength);

        for (int i = 0; i < objectCount; i++)
        {
            GameObject go = Instantiate(Resources.Load("Object")) as GameObject;
            go.transform.Translate(new Vector3(Random.Range(-3.5f,3.5f), Random.Range(-3.5f, 3.5f), Random.Range(-3.5f, 3.5f)));
            gos.Add(go);
        }
    }

    //
    // 
    //
    void CreateTestObject(List<GameObject> gos, List<TestObject> objs)
    {
        foreach(var go in gos)
        {

        }
    }
}
