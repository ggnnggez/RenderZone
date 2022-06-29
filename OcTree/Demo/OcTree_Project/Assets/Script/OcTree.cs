/*
 * Author:  NAN GAO
 * Date:    2022/6/28
 * Desc:    OcTree demo in unity. 
 * Ref:     https://www.gamedev.net/tutorials/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcTree 
{
    public class OcTree
    {
        BoundingBox m_region;                           // ����ڵ�ķ�տռ�
        List<TestObject> m_objects;                     // ��տռ��а����Ķ���

        OcTree[] m_childNode = new OcTree[8];           // ����8���ӽڵ�
        OcTree m_parent;                                // ָ�򸸽ڵ�

        const int MIN_SIZE = 1;                         // ���ɷ�տռ���С�ߴ�

        static Queue m_pendingInsertion = new Queue();  // ������Ĳ����������

        static bool m_treeReady = false;                // the tree has a few objects which need to be inserted before it is complete 
        static bool m_treeBuilt = false;                // there is no pre-existing tree yet. 

        ///
        /// This is a bitmask indicating which child nodes are actively being used. 
        /// It adds slightly more complexity, but is faster for performance since there is only one comparison instead of 8. 
        ///
        // byte m_activeNodes = 0;

        public OcTree(BoundingBox region, List<TestObject> objList)
        {
            m_region = region;
            m_objects = objList;
            // m_curLife = -1;
        }

        // Lazy Init.
        public OcTree()
        {
            m_region = new BoundingBox();
            m_objects = new List<TestObject>();
        }

        // Lazy Init.
        public OcTree(BoundingBox region)
        {
            m_region = region;
            m_objects = new List<TestObject>();
        }

        void bulidTree()
        {

            // �ж��Ƿ�ΪҶ�ӽڵ�
            if (m_objects.Count <= 1)
                return;

            // ����Խ���
            Vector3 dimensions = m_region.m_max - m_region.m_min; 

            // region�ĺϷ��Լ��
            if (dimensions.x <= MIN_SIZE && dimensions.y <= MIN_SIZE && dimensions.z <= MIN_SIZE)
            {
                return;
            }

            // ��m_region����Ϊ�˸��ռ�
            BoundingBox[] octant = PartitionSpace(m_region);

            List<List<TestObject>> octList = new List<List<TestObject>>(8);
            List<TestObject> deList = new List<TestObject>();

            //
            // �ж�m_object�еĶ�����m_region��λ��״̬(Contain,Intersect,Exclude)
            // ������״̬��octList��ѹ�����
            //
            foreach (var obj in m_objects)
            {
                if (obj.m_objBox.m_min != obj.m_objBox.m_max)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (octant[i].IsContain(obj.m_objBox) == ContainmentType.Contain)
                        {
                            octList[i].Add(obj);
                            deList.Add(obj);
                            break;
                        }
                    }
                }
            }

            foreach(var obj in deList)
            {
                m_objects.Remove(obj);
            }

            for (int i = 0; i < 8; i++)
            {
                if (octList[i].Count != 0)
                {
                    m_childNode[i] = CreateNode(octant[i], octList[i]);
                    //m_activeNodes |= (byte)(1 << a);
                    m_childNode[i].bulidTree();
                }
            }
        }

        //
        // ����ռ仮����Ϊ
        // ����������Ҫ�ܹ����廮�ֵ�λ�ã�����ֻ�Ǵ��м俪ʼ����
        //
        BoundingBox[] PartitionSpace(BoundingBox boundingBox)
        {
            Vector3 dimensions = boundingBox.m_max - boundingBox.m_min; // ��Խ���
            Vector3 half = dimensions / 2.0f;                           //  ��Խ����е�λ������
            Vector3 center = m_region.m_min + half;                     //  ��������ԭ��ָ��region���ĵ�����

            BoundingBox[] octant = new BoundingBox[8];                  // ������ܻỮ�ֳ��İ˸��ռ�

            octant[0] = new BoundingBox(m_region.m_min, center);
            octant[1] = new BoundingBox(new Vector3(center.x, m_region.m_min.y, m_region.m_min.z), new Vector3(m_region.m_max.x, center.y, center.z));
            octant[2] = new BoundingBox(new Vector3(center.x, m_region.m_min.y, center.z), new Vector3(m_region.m_max.x, center.y, m_region.m_max.z));
            octant[3] = new BoundingBox(new Vector3(m_region.m_min.x, m_region.m_min.y, center.z), new Vector3(center.x, center.y, m_region.m_max.z));
            octant[4] = new BoundingBox(new Vector3(m_region.m_min.x, center.y, m_region.m_min.z), new Vector3(center.x, m_region.m_max.y, center.z));
            octant[5] = new BoundingBox(new Vector3(center.x, center.y, m_region.m_min.z), new Vector3(m_region.m_max.x, m_region.m_max.y, center.z));
            octant[6] = new BoundingBox(center, m_region.m_max);
            octant[7] = new BoundingBox(new Vector3(m_region.m_min.x, center.y, center.z), new Vector3(center.x, m_region.m_max.y, m_region.m_max.z));

            return octant;
        }

        private OcTree CreateNode(BoundingBox region, List<TestObject> objList)
        {
            if (objList.Count == 0)
                return null;
            OcTree ret = new OcTree(region, objList);
            ret.m_parent = this;
            return ret;
        }
    }
}

