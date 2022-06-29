using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcTree
{
    /// <summary>
    /// 描述两个BoundingBox之间的位置关系
    /// </summary>
    public enum ContainmentType
    {
        Contain,    // 包含
        Intersect,  // 相交
        Exclude,    // 相离

        Unkown      // 用于处理错误
    }

    public class BoundingBox
    {
        public Vector3 m_max;
        public Vector3 m_min;

        public bool visible;

        public BoundingBox(Vector3 min, Vector3 max)
        {
            m_min = min;
            m_max = max;
        }
        public BoundingBox()
        {
            m_max = new Vector3();
            m_min = new Vector3();
        }

        public ContainmentType IsContain(BoundingBox boundingBox)
        {
            if (PointInterset(boundingBox.m_min) && PointInterset(boundingBox.m_max))
                return ContainmentType.Contain;
            if (PointInterset(boundingBox.m_min) ^ PointInterset(boundingBox.m_max))
                return ContainmentType.Intersect;
            if (!PointInterset(boundingBox.m_min) && !PointInterset(boundingBox.m_max))
                return ContainmentType.Exclude;

            return ContainmentType.Unkown;
        }

        private bool PointInterset(Vector3 p)
        {
            if (p.x < m_min.x || p.x > m_max.x) return false;
            if (p.y < m_min.y || p.y > m_max.y) return false;
            if (p.z < m_min.z || p.z > m_max.z) return false;
            return true;
        }
        public void visualization()
        {

        }
    }

}

