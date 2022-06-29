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
        BoundingBox m_region;                           // 定义节点的封闭空间
        List<TestObject> m_objects;                     // 封闭空间中包含的对象

        OcTree[] m_childNode = new OcTree[8];           // 包含8个子节点
        OcTree m_parent;                                // 指向父节点

        const int MIN_SIZE = 1;                         // 生成封闭空间最小尺寸

        static Queue m_pendingInsertion = new Queue();  // 待处理的插入命令队列

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

            // 判断是否为叶子节点
            if (m_objects.Count <= 1)
                return;

            // 求体对角线
            Vector3 dimensions = m_region.m_max - m_region.m_min; 

            // region的合法性检测
            if (dimensions.x <= MIN_SIZE && dimensions.y <= MIN_SIZE && dimensions.z <= MIN_SIZE)
            {
                return;
            }

            // 将m_region划分为八个空间
            BoundingBox[] octant = PartitionSpace(m_region);

            List<List<TestObject>> octList = new List<List<TestObject>>(8);
            List<TestObject> deList = new List<TestObject>();

            //
            // 判断m_object中的对象与m_region的位置状态(Contain,Intersect,Exclude)
            // 并根据状态向octList中压入对象
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
        // 定义空间划分行为
        // 后续可能需要能够定义划分的位置，不再只是从中间开始划分
        //
        BoundingBox[] PartitionSpace(BoundingBox boundingBox)
        {
            Vector3 dimensions = boundingBox.m_max - boundingBox.m_min; // 体对角线
            Vector3 half = dimensions / 2.0f;                           //  体对角线中点位置坐标
            Vector3 center = m_region.m_min + half;                     //  世界坐标原点指向region中心的向量

            BoundingBox[] octant = new BoundingBox[8];                  // 定义可能会划分出的八个空间

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

