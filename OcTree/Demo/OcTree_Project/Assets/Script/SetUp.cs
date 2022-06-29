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

    GameObject region; // �ü��������Ŀ��ӻ��ռ�
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
    // 1. ȷ���������������С��ͳһʹ�������壩������Ԥ����Region���ӻ�����¶Lenght��
    // 2. ȷ�������ģ��ΪԤ����Object����������ֲ������湹����Region�У���¶Count��
    // 3. Ҫ��OcTree��ȫ���롣���ر�ע�ⲻҪ��BoundingBox��д�����
    // 
    void BuildEnviroment()
    {
        // ����
        gos = new List<GameObject>(objectCount);

        // ����Ԥ���� Region
        region = Instantiate(Resources.Load("Region")) as GameObject;

        // ����region�Ĵ�С
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
