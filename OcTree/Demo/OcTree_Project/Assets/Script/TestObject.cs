using System.Collections;
using UnityEngine;

namespace OcTree
{
    public class TestObject
    {
        public BoundingBox m_objBox;
        public Vector3 m_pos; // WS
        public TestObject(BoundingBox boundingBox, Vector3 pos)
        {
            m_objBox = boundingBox;
            m_pos = pos;
        }

        // 这里由于是demo，就直接默认是使用unity自带的sphere模型，且使用默认的Transform
        public TestObject(GameObject go)
        {
            m_pos = go.transform.position;
            m_objBox = new BoundingBox(m_pos - new Vector3(0.25f, 0.25f, 0.25f), 
                                       m_pos + new Vector3(0.25f, 0.25f, 0.25f) );
        }
    }
}