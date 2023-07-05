using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse
{
    /// <summary>
    /// Vector4 int
    /// </summary>
    [System.Serializable]
    public struct pVector4Int
    {
        [SerializeField]
        int m_x, m_y, m_z, m_w;

        public int x
        {
            get
            {
                return m_x;
            }
            set
            {
                m_x = value;
            }
        }


        public int y
        {
            get
            {
                return m_y;
            }
            set
            {
                m_y = value;
            }
        }


        public int z
        {
            get
            {
                return m_z;
            }
            set
            {
                m_z = value;
            }
        }

        public int w
        {
            get
            {
                return m_w;
            }
            set
            {
                m_w = value;
            }
        }

        public static readonly pVector4Int one, zero, positiveInfinity, negativeInfinity;

        static pVector4Int ()
        {
            one = new pVector4Int (1,1,1,1);
            zero = new pVector4Int ();
            positiveInfinity = new pVector4Int(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            negativeInfinity = new pVector4Int(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
        }

        public pVector4Int(int x, int y, int z, int w) 
        {
            this.m_x = x;
            this.m_y = y;
            this.m_z = z;
            this.m_w = w;
        }

        public pVector4Int(pVector4Int src) 
        {
            this.m_x = src.m_x;
            this.m_y = src.m_y;
            this.m_z = src.m_z;
            this.m_w = src.m_w;
        }

        public float magnitude 
        {
            get 
            {
                return Mathf.Sqrt (m_x*m_x + m_y*m_y + m_z*m_z + m_w*m_w);
            }
        }

        public float sqrMagnitude 
        {
            get 
            {
                return (m_x*m_x + m_y*m_y + m_z*m_z + m_w*m_w);
            }
        }

        public int this[int index] 
        {
            get 
            {
                switch (index)
                {
                    case 1: 
                        return m_y;
                    case 2:
                        return m_z;
                    case 3:
                        return m_w;
                    case 0:
                        default:
                        return m_x;
                }
            }
            set 
            {
                switch (index)
                {
                    case 1: 
                        m_y = value;
                        return;
                    case 2:
                        m_z = value;
                        return;
                    case 3:
                        m_w = value;
                        return;
                    case 0:
                    default:
                        m_x = value;
                        return;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[Vector4Int: x={0}, y={1}, z={2}, w={3}, magnitude={4}, sqrMagnitude={5}]", x, y, z, w, magnitude, sqrMagnitude);
        }

        /// <summary>
        /// Clamps the Vector4Int to the bounds given by min and max.
        /// </summary>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public void Clamp (pVector4Int min, pVector4Int max)
        {
            this.x = Mathf.Clamp (this.x, min.x, max.x);
            this.y = Mathf.Clamp (this.y, min.y, max.y);
            this.z = Mathf.Clamp (this.z, min.z, max.z);
            this.w = Mathf.Clamp (this.w, min.w, max.w);
        }

        /// <summary>
        /// Returns a vector that is made from the largest components of two vectors.
        /// </summary>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        public static pVector4Int Max (pVector4Int lhs, pVector4Int rhs)
        {
            pVector4Int ret = new pVector4Int (Mathf.Max (lhs.x, rhs.x), Mathf.Max (lhs.y, rhs.y), 
                Mathf.Max (lhs.z, rhs.z), Mathf.Max (lhs.w, rhs.w));
            return ret;
        }

        /// <summary>
        /// Returns a vector that is made from the smallest components of two vectors.
        /// </summary>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        public static pVector4Int Min (pVector4Int lhs, pVector4Int rhs)
        {
            pVector4Int ret = new pVector4Int (Mathf.Min (lhs.x, rhs.x), Mathf.Min (lhs.y, rhs.y), 
                Mathf.Min (lhs.z, rhs.z), Mathf.Min (lhs.w, rhs.w));
            return ret;
        }

      /// <summary>
      /// return distance between a and b
      /// </summary>
      /// <param name="a">The alpha component.</param>
      /// <param name="b">The blue component.</param>
        public static float Distance (pVector4Int a, pVector4Int b)
        {
            pVector4Int ret = new pVector4Int (a.x - b.x, a.y - b.y, 
                a.z - b.z, a.w - b.w);
            return ret.magnitude;
        }
        /// <summary>
        /// Converts a Vector4 to a Vector4Int by doing a Floor to each value.
        /// </summary>
        /// <returns>The to int.</returns>
        /// <param name="v4">V4.</param>
        public static pVector4Int FloorToInt (Vector4 v4)
        {
            pVector4Int ret = new pVector4Int (Mathf.FloorToInt (v4.x), Mathf.FloorToInt (v4.y),
                Mathf.FloorToInt (v4.y), Mathf.FloorToInt (v4.z));
            return ret;
        }

        /// <summary>
        /// Converts a Vector3 to a Vector3Int by doing a Round to each value.
        /// </summary>
        /// <returns>The to int.</returns>
        /// <param name="v4">V4.</param>
        public static pVector4Int RoundToInt (Vector4 v4)
        {
            pVector4Int ret = new pVector4Int (Mathf.RoundToInt (v4.x), Mathf.RoundToInt (v4.y),
                Mathf.RoundToInt (v4.y), Mathf.RoundToInt (v4.z));
            return ret;
        }

        /// <summary>
        /// Converts a Vector3 to a Vector3Int by doing a Ceiling to each value.
        /// </summary>
        /// <returns>The to int.</returns>
        /// <param name="v4">V4.</param>
        public static pVector4Int CeilToInt (Vector4 v4)
        {
            pVector4Int ret = new pVector4Int (Mathf.CeilToInt (v4.x), Mathf.CeilToInt (v4.y),
                Mathf.CeilToInt (v4.y), Mathf.CeilToInt (v4.z));
            return ret;
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// Every component in the result is a component of a multiplied by the same component of b.
        /// </summary>
        /// <returns>The to int.</returns>
        /// <param name="v4">V4.</param>
        public static pVector4Int Scale (pVector4Int a, pVector4Int b)
        {
            pVector4Int ret = new pVector4Int (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
            return ret;
        }

        public static pVector4Int operator -(pVector4Int a, pVector4Int b)
        {
            return new pVector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static pVector4Int operator +(pVector4Int a, pVector4Int b)
        {
            return new pVector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static pVector4Int operator *(pVector4Int a, pVector4Int b)
        {
            return new pVector4Int(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        public static pVector4Int operator *(pVector4Int a, int b)
        {
            return new pVector4Int(a.x * b, a.y * b, a.z * b, a.w * b);
        }

        public static Vector4 operator *(pVector4Int a, float b)
        {
            return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
        }

        public static Vector4 operator /(pVector4Int a, pVector4Int b)
        {
            return new Vector4(a.x / (float)b.x, a.y / (float)b.y, a.z / (float)b.z, a.w / (float)b.w);
        }

        public static Vector4 operator /(pVector4Int a, int b)
        {
            return new Vector4(a.x / (float)b, a.y / (float)b, a.z / (float)b, a.w / (float)b);
        }

        public static Vector4 operator /(pVector4Int a, float b)
        {
            return new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
        }

        public static bool operator ==(pVector4Int a, pVector4Int b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType () == typeof(pVector4Int))
            {
                pVector4Int b = (pVector4Int)obj;
                pVector4Int a = this;
                return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator !=(pVector4Int a, pVector4Int b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z && a.w != b.w;
        }

        public static explicit operator Vector4(pVector4Int a) 
        {
            return new Vector4 (a.x, a.y, a.z, a.w);
        }

        public static explicit operator pVector4Int(Vector4 a) 
        {
            return new pVector4Int ((int)a.x, (int)a.y, (int)a.z, (int)a.w);
        }
    }
}