using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ximmerse.XR.Asyncoroutine;
using System.Threading;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace Ximmerse
{
    /// <summary>
    /// Polyengine mathf methods library.
    /// </summary>
    public static class PEMathf
    {

        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector3 AveragePosition(Vector3[] positions)
        {
            Vector3 _p = positions[0];
            for (int i = 1, imax = positions.Length; i < imax; i++)
            {
                _p += positions[i];
            }

            return _p / positions.Length;
        }

        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector3 AveragePosition(IList<Vector3> positions)
        {
            Vector3 _p = positions[0];
            for (int i = 1, imax = positions.Count; i < imax; i++)
            {
                _p += positions[i];
            }

            return _p / positions.Count;
        }


        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector2 AveragePosition(Vector2[] positions)
        {
            Vector2 _p = positions[0];
            for (int i = 1, imax = positions.Length; i < imax; i++)
            {
                _p += positions[i];
            }
            return _p / positions.Length;
        }

        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector2 AveragePosition(IList<Vector2> positions)
        {
            Vector2 _p = positions[0];
            for (int i = 1, imax = positions.Count; i < imax; i++)
            {
                _p += positions[i];
            }
            return _p / positions.Count;
        }

        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector4 AveragePosition(Vector4[] positions)
        {
            Vector4 _p = positions[0];
            for (int i = 1, imax = positions.Length; i < imax; i++)
            {
                _p += positions[i];
            }
            return _p / positions.Length;
        }

        /// <summary>
        /// 计算中心点位置
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector4 AveragePosition(IList<Vector4> positions)
        {
            Vector4 _p = positions[0];
            for (int i = 1, imax = positions.Count; i < imax; i++)
            {
                _p += positions[i];
            }
            return _p / positions.Count;
        }


        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector3 AverageDirection(Vector3[] directions)
        {
            Vector3 _d = directions[0];
            for (int i = 1, imax = directions.Length; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }
        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector3 AverageDirection(IList<Vector3> directions)
        {
            Vector3 _d = directions[0];
            for (int i = 1, imax = directions.Count; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }

        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector3 AverageDirection(Vector2[] directions)
        {
            Vector2 _d = directions[0];
            for (int i = 1, imax = directions.Length; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }
        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector3 AverageDirection(IList<Vector2> directions)
        {
            Vector2 _d = directions[0];
            for (int i = 1, imax = directions.Count; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }

        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector4 AverageDirection(Vector4[] directions)
        {
            Vector4 _d = directions[0];
            for (int i = 1, imax = directions.Length; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }
        /// <summary>
        /// 计算平均方向
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        public static Vector4 AverageDirection(IList<Vector4> directions)
        {
            Vector4 _d = directions[0];
            for (int i = 1, imax = directions.Count; i < imax; i++)
            {
                _d = (_d + directions[i]);
            }
            return _d.normalized;
        }

        /// <summary>
        /// Gets if a point falls at the right side of the direction ? 
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="Direction"></param>
        /// <returns></returns>
        public static bool IsRight(Vector3 Point, Vector3 Direction)
        {
            return (-Point.x * Direction.z + Point.z * Direction.x) < 0;
        }

        /// <summary>
        /// is the value between [<paramref name="from"/>] and [<paramref name="to"/>]
        /// </summary>
        /// <returns>The <see cref="T:System.Single"/>.</returns>
        /// <param name="value">Value.</param>
        /// <param name="between1">Between1.</param>
        /// <param name="between2">Between2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(this int value, int from, int to)
        {
            return (value >= from && value <= to)
                || (value >= to && value <= from);
        }

        /// <summary>
        /// is the value between [<paramref name="from"/>] and [<paramref name="to"/>]
        /// </summary>
        /// <returns>The <see cref="T:System.Single"/>.</returns>
        /// <param name="value">Value.</param>
        /// <param name="from">Between1.</param>
        /// <param name="to">Between2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(this float value, float from, float to)
        {
            return (value >= from && value <= to)
                || (value >= to && value <= from);
        }

        /// <summary>
        /// Is the value between between1 and between2 ?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(this Vector2Int value, Vector2Int between1, Vector2Int between2)
        {
            return IsBetween(value.x, between1.x, between2.x) && IsBetween(value.y, between1.y, between2.y);
        }

        /// <summary>
        /// Is the value between between1 and between2 ?
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(this Vector3Int value, Vector3Int between1, Vector3Int between2)
        {
            return IsBetween(value.x, between1.x, between2.x) && IsBetween(value.y, between1.y, between2.y) && IsBetween(value.z, between1.z, between2.z);
        }

        /// <summary>
        /// Approximately the specified a and b.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y));
        }
        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Abs(this Vector3 vector3)
        {
            return new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
        }

        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector4"></param>
        /// <returns></returns>
        public static Vector4 Abs(this Vector4 vector4)
        {
            return new Vector4(Mathf.Abs(vector4.x), Mathf.Abs(vector4.y), Mathf.Abs(vector4.z), Mathf.Abs(vector4.w));
        }
        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2Int Abs(this Vector2Int vector2)
        {
            return new Vector2Int(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y));
        }
        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3Int Abs(this Vector3Int vector3)
        {
            return new Vector3Int(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
        }

        /// <summary>
        /// Gets the normalized vector of the vector3 int.
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 NormalizedAsVector3(this Vector3Int vector3)
        {
            return new Vector3(vector3.x, vector3.y, vector3.z).normalized;
        }

        /// <summary>
        /// Gets the normalized vector of the vector2 int.
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Vector2 NormalizedAsVector2(this Vector2Int vector2)
        {
            return new Vector2(vector2.x, vector2.y).normalized;
        }

        /// <summary>
        /// Gets the abs of a vector
        /// </summary>
        /// <param name="vector4"></param>
        /// <returns></returns>
        public static pVector4Int Abs(this pVector4Int vector4)
        {
            return new pVector4Int(Mathf.Abs(vector4.x), Mathf.Abs(vector4.y), Mathf.Abs(vector4.z), Mathf.Abs(vector4.w));
        }

        /// <summary>
        /// Approximately the specified a and b.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this Vector4 a, Vector4 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z) && Mathf.Approximately(a.w, b.w);
        }

        /// <summary>
        /// Approximately the specified a and b.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }

        /// <summary>
        /// Approximately the specified a and b.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(this Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }

        /// <summary>
        /// Makes the angle a pretty value between [-180 ... 180]
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="angle">Angle.</param>
        public static float PrettyAngle(this float angle)
        {
            if (angle >= -180 && angle <= 180)
            {
                return angle;
            }
            if (angle <= -180)
            {
                float mod = Mathf.Repeat(-angle, 360);
                if (mod >= 180)
                {
                    return -360 + mod;
                }
                else
                {
                    return -mod;
                }
            }
            else if (angle > 180 && angle <= 360)
            {
                return angle - 360;
            }
            else
            {
                float mod = Mathf.Repeat(angle, 360);
                if (mod >= 180)
                {
                    return -360 + mod;
                }
                else
                {
                    return mod;
                }
            }
        }


        /// <summary>
        /// Makes the euler a pretty value between [-180 ... 180]
        /// </summary>
        /// <returns>The euler.</returns>
        /// <param name="euler">Euler.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 PrettyAngle(this Vector3 euler)
        {
            return new Vector3(PrettyAngle(euler.x),
                PrettyAngle(euler.y),
                PrettyAngle(euler.z));
        }

        /// <summary>
        /// Makes the euler a pretty value between [-180 ... 180]
        /// </summary>
        /// <returns>The euler.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 PrettyAngle(this Quaternion q)
        {

            var e = q.eulerAngles;
            return new Vector3(PrettyAngle(e.x),
                PrettyAngle(e.y),
                PrettyAngle(e.z));
        }


        /// <summary>
        /// 将 Position 投影到由 [LineStartPoint, LineEndPoint] 组成的线段上。输出投影点。
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="LineStartPoint"></param>
        /// <param name="LineEndPoint"></param>
        /// <param name="ProjectionPoint">输出的投影坐标</param>
        /// <returns>projectPoint是否在 LineStartPoint - LineEndPoint中间，如果是， 返回 true</returns>
        public static bool ProjectPointToLine(Vector3 Position, Vector3 LineStartPoint, Vector3 LineEndPoint, out Vector3 ProjectionPoint)
        {
            Vector3 lineDirection = (LineEndPoint - LineStartPoint).normalized;
            Vector3 prjVector = Vector3.Project(Position - LineStartPoint, lineDirection);
            ProjectionPoint = LineStartPoint + prjVector;
            return Vector3.Dot(ProjectionPoint - LineStartPoint, ProjectionPoint - LineEndPoint) <= 0 ? true : false; //如果投影点到起始点的方向反向，则代表投影点位于线段中间.
        }

        /// <summary>
        /// 将 Position 投影到由 [LineStartPoint, LineEndPoint] 组成的线段上。输出投影点。
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="LineStartPoint"></param>
        /// <param name="LineEndPoint"></param>
        /// <param name="ProjectionPoint">输出的投影坐标</param>
        /// <param name="ProjectionNormal">输出的投影法线，这个法线从投影点指向原始点</param>
        /// <returns>projectPoint是否在 LineStartPoint - LineEndPoint中间，如果是， 返回 true</returns>
        public static bool ProjectPointToLine(Vector3 Position, Vector3 LineStartPoint, Vector3 LineEndPoint, out Vector3 ProjectionPoint, out Vector3 ProjectionNormal)
        {
            Vector3 lineDirection = (LineEndPoint - LineStartPoint).normalized;
            Vector3 prjVector = Vector3.Project(Position - LineStartPoint, lineDirection);
            ProjectionPoint = LineStartPoint + prjVector;
            ProjectionNormal = (Position - ProjectionPoint).normalized;
            return Vector3.Dot(ProjectionPoint - LineStartPoint, ProjectionPoint - LineEndPoint) <= 0 ? true : false; //如果投影点到起始点的方向反向，则代表投影点位于线段中间.
        }

        /// <summary>
        /// Rotate an 2D point around an 2D center point, in angle.
        /// </summary>
        /// <param name="Point2D"></param>
        /// <param name="PointerCenter"></param>
        /// <param name="Angle"></param>
        /// <returns>The rotated point.</returns>
        public static Vector2 Rotate2DPoint(Vector2 Point2D, Vector2 PointerCenter, float Angle)
        {
            Vector2 v2d = Point2D - PointerCenter;
            float srcAngle = 0;//srcAngle : p2D 到 pCenter的夹角, [0-360]之间
            //居中
            if (v2d.x == 0 && v2d.y == 0)
            {
                return new Vector2(PointerCenter.x, PointerCenter.y);
            }
            else if (v2d.x == 0 && v2d.y > 0)//Y轴+
            {
                srcAngle = 90;
            }
            else if (v2d.x == 0 && v2d.y < 0)//Y轴-
            {
                srcAngle = 270;
            }
            else if (v2d.x > 0 && v2d.y == 0)//X轴+
            {
                srcAngle = 0;
            }
            else if (v2d.x < 0 && v2d.y == 0)//X轴-
            {
                srcAngle = 180;
            }
            //第一象限 :
            else if (v2d.x > 0 && v2d.y > 0)
            {
                srcAngle = Mathf.Rad2Deg * Mathf.Atan(v2d.y / v2d.x);
            }
            //第2象限:
            else if (v2d.x < 0 && v2d.y > 0)
            {
                srcAngle = 180 - Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(v2d.y) / Mathf.Abs(v2d.x));
            }
            //第3象限:
            else if (v2d.x < 0 && v2d.y < 0)
            {
                srcAngle = 180 + Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(v2d.y) / Mathf.Abs(v2d.x));
            }
            //第4象限:
            else if (v2d.x > 0 && v2d.y < 0)
            {
                srcAngle = 360 - Mathf.Rad2Deg * Mathf.Atan(Mathf.Abs(v2d.y) / Mathf.Abs(v2d.x));
            }

            float r = v2d.magnitude;//半径
            float newAngle = srcAngle + Angle;
            //Clamp newAngle to [0...360]
            if (newAngle > 360)
            {
                newAngle = newAngle - 360;
            }
            else if (newAngle < 0)
            {
                newAngle = 360 + newAngle;
            }
            Debug.LogFormat("Src angle: {0}, new angle: {1}", srcAngle, newAngle);
            float sinY = Mathf.Abs(r * Mathf.Sin(newAngle * Mathf.Deg2Rad));
            float sinX = Mathf.Abs(r * Mathf.Cos(newAngle * Mathf.Deg2Rad));
            if (newAngle == 0 || newAngle == 360)//X轴+
            {
                return new Vector2(sinX, 0);
            }
            else if (newAngle == 180)//X轴-
            {
                return new Vector2(-sinX, 0);
            }
            else if (newAngle == 90)//Y轴+
            {
                return new Vector2(0, sinY);
            }
            else if (newAngle == 270)//Y轴-
            {
                return new Vector2(0, -sinY);
            }
            //根据象限来设定:
            else
            {
                //第一象限:
                if (newAngle > 0 && newAngle < 90)
                {
                    return new Vector2(PointerCenter.x + sinX, PointerCenter.y + sinY);
                }
                //第二象限:
                else if (newAngle > 90 && newAngle < 180)
                {
                    return new Vector2(PointerCenter.x - sinX, PointerCenter.y + sinY);
                }
                //第三象限:
                else if (newAngle > 180 && newAngle < 270)
                {
                    return new Vector2(PointerCenter.x - sinX, PointerCenter.y - sinY);
                }
                //第四象限:
                else //if (newAngle > 270 && newAngle < 360)
                {
                    return new Vector2(PointerCenter.x + sinX, PointerCenter.y - sinY);
                }
            }
        }

        /// <summary>
        /// Clamps an angle to circle range : [0 ~ 360].
        /// 将任意角度转换到 [0 ~ 360] 之间。
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float ConvertToCircleAngle(float angle)
        {
            if (angle >= 0 && angle <= 360)
            {
                return angle;
            }
            else if (angle > 360)
            {
                return Mathf.Repeat(angle, 360);
            }
            else // angle < 0
            {
                if (Mathf.Abs(angle) <= 360)
                {
                    return 360 + angle;
                }
                else
                {
                    angle = -Mathf.Repeat(angle, 360);
                    return 360 + angle;
                }
            }
        }

        /// <summary>
        /// 把 rotation 的X和Z轴旋转角度设置为0。
        /// Flatten 之后的方向只有水平旋转角度。
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FlattenXZ(ref Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            rotation.eulerAngles = euler;
        }

        /// <summary>
        /// 把 rotation 的X和Z轴旋转角度设置为0。
        /// Flatten 之后的方向只有水平旋转角度。
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        public static void FlattenXZ(this Quaternion rotation)
        {
            FlattenXZ(ref rotation);
        }

        /// <summary>
        /// Get the XZ d of dir1 and dir2 (on XZ surface)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="pos1">pos1.</param>
        /// <param name="pos2">pos2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceXZ(this Vector3 pos1, Vector3 pos2)
        {
            Vector3 newPos2 = new Vector3(pos2.x, pos1.y, pos2.z);
            return Vector3.Distance(pos1, newPos2);
        }


        /// <summary>
        /// Get the XZ DOT of dir1 and dir2 (on XZ surface)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="dir1">Dir1.</param>
        /// <param name="dir2">Dir2.</param>
        public static float DotXZ(this Vector3 dir1, Vector3 dir2)
        {
            Vector3 newDir1 = new Vector3(dir1.x, dir2.y, dir1.z);
            return Vector3.Dot(newDir1, dir2);
        }


        /// <summary>
        /// Get the XZ signed angle of dir1 and dir2 (on XZ surface)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="dir1">Dir1.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngleXZ(this Vector3 dir1, Vector3 dir2)
        {
            Vector2 newDir1 = new Vector2(dir1.x, dir1.z);
            Vector2 newDir2 = new Vector2(dir2.x, dir2.z);
            return SignedAngle(newDir1, newDir2);
        }

        /// <summary>
        /// Get the signed angle of dir1 and dir2 (on XY axis)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="dir1">Dir1.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngleXY(this Vector3 dir1, Vector3 dir2)
        {
            Vector2 newDir1 = new Vector2(dir1.x, dir1.y);
            Vector2 newDir2 = new Vector2(dir2.x, dir2.y);
            return SignedAngle(newDir1, newDir2);
        }


        /// <summary>
        /// Get the signed angle of dir1 and dir2 (on ZY axis)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="dir1">Dir1.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngleZY(this Vector3 dir1, Vector3 dir2)
        {
            Vector2 newDir1 = new Vector2(dir1.z, dir1.y);
            Vector2 newDir2 = new Vector2(dir2.z, dir2.y);
            return SignedAngle(newDir1, newDir2);
        }

        /// <summary>
        /// Steps the input, return a float that is multiple step to stepValue, and not bigger than input.
        /// For example, input value = 0.7, step = 0.5, return = 0.5. Input vlaue = 1.2. step = 0.5, return = 1
        /// </summary>
        /// <param name="input">input value.</param>
        /// <param name="step">step.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloorStep(float input, float step)
        {
            int stepCount = Mathf.FloorToInt(input / step);
            return stepCount * step;
        }

        /// <summary>
        /// 从 floats 找出距离 value 最近的那个值， 返回值并输出索引。
        /// </summary>
        /// <param name="floats"></param>
        /// <param name="value"></param>
        /// <param name="closestIndex"></param>
        /// <returns></returns>
        public static float FindClosestFloat(this float[] floats, float value, out int closestIndex)
        {
            float closest = Mathf.Infinity;
            int index = 0;
            for (int i = 0; i < floats.Length; i++)
            {
                float f = floats[i];
                float d = PEMathf.Distance(f, value);
                if (d < closest)
                {
                    closest = d;
                    index = i;
                }
            }
            closestIndex = index;
            return floats[index];
        }

        /// <summary>
        /// 从 floats 找出距离 value 最近的那个值， 返回值并输出索引。
        /// </summary>
        /// <param name="floats"></param>
        /// <param name="value"></param>
        /// <param name="closestIndex"></param>
        /// <returns></returns>
        public static float FindClosestFloat(this List<float> floats, float value, out int closestIndex)
        {
            float closest = Mathf.Infinity;
            int index = 0;
            for (int i = 0; i < floats.Count; i++)
            {
                float f = floats[i];
                float d = PEMathf.Distance(f, value);
                if (d < closest)
                {
                    closest = d;
                    index = i;
                }
            }
            closestIndex = index;
            return floats[index];
        }

        /// <summary>
        /// 从 floats 找出距离 value 最近的那个值， 返回值并输出索引。
        /// </summary>
        /// <param name="ints"></param>
        /// <param name="value"></param>
        /// <param name="closestIndex"></param>
        /// <returns></returns>
        public static int FindClosestInt(this int[] ints, int value, out int closestIndex)
        {
            float closest = Mathf.Infinity;
            int index = 0;
            for (int i = 0; i < ints.Length; i++)
            {
                float f = ints[i];
                float d = PEMathf.Distance(f, value);
                if (d < closest)
                {
                    closest = d;
                    index = i;
                }
            }
            closestIndex = index;
            return ints[index];
        }

        /// <summary>
        /// 从 floats 找出距离 value 最近的那个值， 返回值并输出索引。
        /// </summary>
        /// <param name="ints"></param>
        /// <param name="value"></param>
        /// <param name="closestIndex"></param>
        /// <returns></returns>
        public static int FindClosestInt(this List<int> ints, int value, out int closestIndex)
        {
            float closest = Mathf.Infinity;
            int index = 0;
            for (int i = 0; i < ints.Count; i++)
            {
                float f = ints[i];
                float d = PEMathf.Distance(f, value);
                if (d < closest)
                {
                    closest = d;
                    index = i;
                }
            }
            closestIndex = index;
            return ints[index];
        }

        /// <summary>
        /// Gets the fractional part of a float.
        /// E.g. - 10.252 -> 0.252 is return.
        /// </summary>
        /// <param name="floatVal"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFractional(this float floatVal)
        {
            return floatVal - (int)floatVal;
        }

        /// <summary>
        /// 自动进行裁切操作， 获取index的下一个/上一个数组索引。
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="arrayLength">数组长度</param>
        /// <param name="next">下一个或上一个索引</param>
        /// <param name="isIndexClamped">新的索引是否触及了 0 或者最后值。</param>
        /// <param name="loopHeadTrail">是否允许索引交替轮回.</param>
        /// <returns></returns>
        public static int GetArrayIndex(this int index, int arrayLength, bool next, out bool isIndexClamped, bool loopHeadTrail = true)
        {
            if (next)
            {
                int nextIndex = index + 1;
                if (loopHeadTrail && nextIndex >= arrayLength)
                {
                    nextIndex = 0;//轮回到0
                    isIndexClamped = false;//loopHeadTrail 下， 一定不会是 isNewIndexClamped
                }
                else
                {
                    isIndexClamped = index == arrayLength - 1;//如果index本来就是最后一个元素了, is clamped = true
                    nextIndex = Mathf.Clamp(nextIndex, 0, arrayLength - 1);
                }
                return nextIndex;
            }
            else
            {
                int prevIndex = index - 1;
                if (loopHeadTrail && prevIndex < 0)
                {
                    prevIndex = arrayLength - 1;//轮到最后一位
                    isIndexClamped = false;//loopHeadTrail 下， 一定不会是 isNewIndexClamped
                }
                else
                {
                    isIndexClamped = index == 0;//如果index本来就是 0 了, is clamped = true
                    prevIndex = Mathf.Clamp(prevIndex, 0, arrayLength - 1);
                }
                return prevIndex;
            }
        }

        /// <summary>
        /// Gets the fractional part of a double.
        /// E.g. - 10.252 -> 0.252 is return.
        /// </summary>
        /// <param name="doubleVal"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetFractional(this double doubleVal)
        {
            return doubleVal - (long)doubleVal;
        }

        /// <summary>
        /// Gets the min value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minValue">Outputs the min value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinIndex(out byte minValue, params byte[] array)
        {
            int index = 0;
            byte minVal = byte.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minVal)
                {
                    index = i;
                    minVal = array[i];
                }
            }
            minValue = minVal;
            return index;
        }

        /// <summary>
        /// Gets the min value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minValue">Outputs the min value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinIndex(out short minValue, params short[] array)
        {
            int index = 0;
            short minVal = short.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minVal)
                {
                    index = i;
                    minVal = array[i];
                }
            }
            minValue = minVal;
            return index;
        }

        /// <summary>
        /// Gets the min value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minValue">Outputs the min value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinIndex(out int minValue, params int[] array)
        {
            int index = 0;
            int minVal = int.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minVal)
                {
                    index = i;
                    minVal = array[i];
                }
            }
            minValue = minVal;
            return index;
        }

        /// <summary>
        /// Gets the min value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minValue">Outputs the min value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinIndex(out float minValue, params float[] array)
        {
            int index = 0;
            float minVal = float.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minVal)
                {
                    index = i;
                    minVal = array[i];
                }
            }
            minValue = minVal;
            return index;
        }

        /// <summary>
        /// Gets the min value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minValue">Outputs the min value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinIndex(out long minValue, params long[] array)
        {
            int index = 0;
            long minVal = long.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minVal)
                {
                    index = i;
                    minVal = array[i];
                }
            }
            minValue = minVal;
            return index;
        }

        public static bool GetVector2Distance(this Vector2 lhs, Vector2 rhs, out float distance)
        {
            distance = Vector2.Distance(lhs, rhs);
            return true;
        }

        public static bool GetVector3Distance(this Vector3 lhs, Vector3 rhs, out float distance)
        {
            distance = Vector3.Distance(lhs, rhs);
            return true;
        }

        public static bool GetVector4Distance(this Vector4 lhs, Vector4 rhs, out float distance)
        {
            distance = Vector4.Distance(lhs, rhs);
            return true;
        }

        /// <summary>
        /// Gets the max value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="maxValue">Outputs the max value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxIndex(out byte maxValue, params byte[] array)
        {
            int index = 0;
            byte maxVal = byte.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    index = i;
                    maxVal = array[i];
                }
            }
            maxValue = maxVal;
            return index;
        }

        /// <summary>
        /// Gets the max value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="maxValue">Outputs the max value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxIndex(out short maxValue, params short[] array)
        {
            int index = 0;
            short maxVal = short.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    index = i;
                    maxVal = array[i];
                }
            }
            maxValue = maxVal;
            return index;
        }

        /// <summary>
        /// Gets the max value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="maxValue">Outputs the max value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxIndex(out int maxValue, params int[] array)
        {
            int index = 0;
            int maxVal = int.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    index = i;
                    maxVal = array[i];
                }
            }
            maxValue = maxVal;
            return index;
        }

        /// <summary>
        /// Gets the max value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="maxValue">Outputs the max value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxIndex(out float maxValue, params float[] array)
        {
            int index = 0;
            float maxVal = float.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    index = i;
                    maxVal = array[i];
                }
            }
            maxValue = maxVal;
            return index;
        }

        /// <summary>
        /// Gets the max value's index at the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="maxValue">Outputs the max value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxIndex(out long maxValue, params long[] array)
        {
            int index = 0;
            long maxVal = long.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    index = i;
                    maxVal = array[i];
                }
            }
            maxValue = maxVal;
            return index;
        }


        /// <summary>
        /// 获取一个点，位于Radius圆半径上，与Vector2.Forward的夹角为Angle.
        /// </summary>
        /// <param name="Radius"></param>
        /// <param name="Angle"></param>
        /// <returns></returns>
        public static Vector2 GetPointOnCircle(float Radius, float Angle)
        {
            Angle %= 360;
            if (Angle < 0)
            {
                Angle = 360 + Angle;
            }
            if (Angle <= 90 && Angle >= 0)
            {
                float y = Radius * Mathf.Sin(Mathf.Deg2Rad * Angle);
                float x = Mathf.Sqrt((Radius * Radius - y * y));
                return new Vector2(x, y);
            }
            else if (Angle <= 180 && Angle >= 90)
            {
                float y = Radius * Mathf.Sin(Mathf.Deg2Rad * Angle);
                float x = Mathf.Sqrt((Radius * Radius - y * y));
                return new Vector2(-x, y);
            }
            else if (Angle <= 270 && Angle >= 180)
            {
                float y = Radius * Mathf.Sin(Mathf.Deg2Rad * Angle);
                float x = Mathf.Sqrt((Radius * Radius - y * y));
                return new Vector2(-x, y);
            }
            else //if (Angle <= 360 && Angle >= 270)
            {
                float y = Radius * Mathf.Sin(Mathf.Deg2Rad * Angle);
                float x = Mathf.Sqrt((Radius * Radius - y * y));
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Steps the input, return a float that is multiple step to stepValue, and not smaller than input.
        /// For example, input value = 0.7, step = 0.5, return = 1. Input value = 1.2, step = 0.5, return = 1.5
        /// </summary>
        /// <returns>The step.</returns>
        /// <param name="input">Input.</param>
        /// <param name="step">Step.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CeilStep(float input, float step)
        {
            int stepCount = Mathf.CeilToInt(input / step);
            return stepCount * step;
        }

        /// <summary>
        /// Get the XZ angle of dir1 and dir2 (on XZ surface)
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="dir1">Dir1.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleXZ(this Vector3 dir1, Vector3 dir2)
        {
            Vector2 newDir1 = new Vector3(dir1.x, dir1.z);
            Vector2 newDir2 = new Vector3(dir2.x, dir2.z);
            return Vector2.Angle(newDir1, newDir2);
        }

        /// <summary>
        /// Rounds the float.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="f">F.</param>
        /// <param name="digit">Digit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundSingle(this float f, int digit = 2)
        {
            if (digit <= 0)
            {
                throw new System.Exception("Digit must GE 0");
            }
            double d = (double)f;
            d = System.Math.Round(d, digit);
            float round = (float)d;
            return round;
        }


        /// <summary>
        /// Rounds the vector2.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="digit">Digit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RoundVector2(this Vector2 vector2, int digit = 2)
        {
            if (digit <= 0)
            {
                throw new System.Exception("Digit must GE 0");
            }
            float roundX = vector2.x.RoundSingle(digit);
            float roundY = vector2.y.RoundSingle(digit);
            return new Vector2(roundX, roundY);
        }


        /// <summary>
        /// Rounds the vector3.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="digit">Digit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RoundVector3(this Vector3 vector3, int digit = 2)
        {
            if (digit <= 0)
            {
                throw new System.Exception("Digit must GE 0");
            }
            float roundX = vector3.x.RoundSingle(digit);
            float roundY = vector3.y.RoundSingle(digit);
            float roundZ = vector3.z.RoundSingle(digit);
            return new Vector3(roundX, roundY, roundZ);
        }



        /// <summary>
        /// Rounds the vector4.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 RoundVector4(this Vector4 vector4, int digit = 2)
        {
            if (digit <= 0)
            {
                throw new System.Exception("Digit must GE 0");
            }
            float roundX = vector4.x.RoundSingle(digit);
            float roundY = vector4.y.RoundSingle(digit);
            float roundZ = vector4.z.RoundSingle(digit);
            float roundW = vector4.w.RoundSingle(digit);
            return new Vector4(roundX, roundY, roundZ, roundW);
        }

        /// <summary>
        /// 将 Single 按照 SnapLength 长度取整. 所得到的结果是 SnapLength 的整形倍数。
        /// 例如: 
        /// single = 4.2 , SnapLength = 2, 返回 = 4;
        /// single = 4.0 , SnapLength = 1.5, 返回 = 4.5;
        /// </summary>
        /// <param name="single"></param>
        /// <param name="SnapLength">Set X,Y,Z of snap length to zero to ignore snapping at the axe.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SnapSingle(this float single, float SnapLength)
        {
            return Mathf.RoundToInt(single / SnapLength) * SnapLength;
        }


        /// <summary>
        /// Gets the signed angle of baseDir and dir2
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="baseDir">Base dir.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Vector3 baseDir, Vector3 dir2)
        {
            var angle = Vector3.Angle(baseDir, dir2);
            return angle * Mathf.Sign(Vector3.Cross(baseDir, dir2).y);
        }

        /// <summary>
        /// Signed angle betweeen two quaternion.
        /// </summary>
        /// <param name="qBase"></param>
        /// <param name="qDir"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Quaternion qBase, Quaternion qDir)
        {
            Vector3 v1 = qBase * Vector3.forward;
            Vector3 v2 = qDir * Vector3.forward;
            return SignedAngle(v1, v2);
        }

        /// <summary>
        /// 返回 dir2 到 baseDir 的带符号角度。
        /// 如果在dir2在baseDir右边，返回1.
        /// 否则返回-1.
        /// 如果方向相同，返回0
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="baseDir">Base dir.</param>
        /// <param name="dir2">Dir2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Vector2 baseDir, Vector2 dir2)
        {
            Vector3 vB = new Vector3(baseDir.x, 0, baseDir.y);
            Vector3 v2 = new Vector3(dir2.x, 0, dir2.y);
            var angle = Vector3.Angle(vB, v2);
            return angle * Mathf.Sign(Vector3.Cross(vB, v2).y);
        }



        /// <summary>
        /// 投骰子。
        /// Possibility 代表命中率， 在 0..1之间。
        /// 越高代表命中率越高。
        /// 例如, 0.3 代表 30%的命中率。
        /// </summary>
        /// <param name="possiblity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Dice(this float possiblity)
        {
            float rnd = Random.Range(0, 1f);
            return rnd <= possiblity;
        }

        /// <summary>
        /// 投骰子。
        /// Possibility 代表命中率， 在 min..max之间。
        /// 越高代表命中率越高。
        /// 例如, 0.3 代表 30%的命中率。
        /// </summary>
        /// <param name="possiblity"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Dice(this int possiblity, int min, int max)
        {
            int rnd = Random.Range(min, max + 1);
            return rnd <= possiblity;
        }

        /// <summary>
        /// 计算两个 float 的距离
        /// </summary>
        /// <returns>The diff.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        public static float Distance(this float a, float b)
        {
            if (Mathf.Approximately(a, b))
            {
                return 0;
            }
            if ((int)Mathf.Sign(a) == (int)Mathf.Sign(b))
            {
                var a1 = Mathf.Abs(a);
                var a2 = Mathf.Abs(b);
                return Mathf.Abs(a1 - a2);
            }
            else
            {
                var bigger = a > b ? a : b;
                var smaller = a > b ? b : a;
                return bigger - smaller;
            }
        }

        /// <summary>
        /// 计算两个 int 的距离
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Distance(this int a, int b)
        {
            if (a == b)
            {
                return 0;
            }
            if ((a > 0 && b > 0) || (a < 0 && b < 0))
            {
                var a1 = Mathf.Abs(a);
                var a2 = Mathf.Abs(b);
                return Mathf.Abs(a1 - a2);
            }
            else
            {
                var bigger = a > b ? a : b;
                var smaller = a > b ? b : a;
                return bigger - smaller;
            }
        }


        /// <summary>
        /// 对Point1,Point2组成的线段按距离，细分成若干个点。
        /// 返回[Point1 ... Point2]之间的细分点的个数。
        /// 注意: 不会输出 Point1, Point2 到Points列表， 只会输出生成的细分插入点。
        /// </summary>
        /// <param name="Point1"></param>
        /// <param name="Point2"></param>
        /// <param name="SegmentDistance"></param>
        /// <returns></returns>
        public static int DivideSegmentByDistance(Vector3 Point1, Vector3 Point2, float SegmentDistance, List<Vector3> Points)
        {
            Vector3 vector = Point2 - Point1;
            Vector3 direction = vector.normalized;
            float distance = vector.magnitude;
            if (distance <= SegmentDistance)
            {
                return 0;
            }
            else
            {
                float d = SegmentDistance;
                int pointCount = 0;
                while (d < distance)
                {
                    Points.Add(Point1 + direction * d);
                    d += SegmentDistance;
                    pointCount++;
                }
                return pointCount;
            }
        }

        /// <summary>
        /// Minimum of two vectors
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 v1, Vector3 v2)
        {
            Vector3 ret = new Vector3(Mathf.Min(v1.x, v2.x),
                Mathf.Min(v1.y, v2.y),
                Mathf.Min(v1.z, v2.z));
            return ret;
        }

        /// <summary>
        /// Select the minimum part of the vector3s
        /// </summary>
        /// <param name="vector3s"></param>
        /// <returns></returns>
        public static Vector3 Min(this IEnumerable<Vector3> vector3s)
        {
            Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            foreach (var v3 in vector3s)
            {
                min.x = Mathf.Min(min.x, v3.x);
                min.y = Mathf.Min(min.y, v3.y);
                min.z = Mathf.Min(min.z, v3.z);
            }
            return min;
        }

        /// <summary>
        /// Select the maximum part of the vector3s
        /// </summary>
        /// <param name="vector3s"></param>
        /// <returns></returns>
        public static Vector3 Max(this IEnumerable<Vector3> vector3s)
        {
            Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
            foreach (var v3 in vector3s)
            {
                max.x = Mathf.Max(max.x, v3.x);
                max.y = Mathf.Max(max.y, v3.y);
                max.z = Mathf.Max(max.z, v3.z);
            }
            return max;
        }


        /// <summary>
        /// 将两个整形数， 按位做位移操作， 整合为一个Long.
        /// 第一个整形数在低4字节 : 0-3， 第二个整形数在高8个字节: 4-7
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ConcateInts(this int A, int B)
        {
            long b = A + ((long)B << 32);//B 左移32位， 补充4-7个字节
            return b;
        }

        /// <summary>
        /// 将一个long数拆分成两个整形数.
        /// A : 0 - 3 位
        /// B : 4 - 7 位
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitLong(this long A, out int B, out int C)
        {
            B = (int)A;
            C = (int)((A & ~((long)uint.MaxValue)) >> 32);
        }


        /// <summary>
        /// 将两个short， 按位做位移操作， 整合为一个 Int.
        /// 第一个整形数在低4字节 : 0-1， 第二个整形数在高8个字节: 2-3
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConcateShorts(this short A, short B)
        {
            int b = A + ((int)B << 16);//B 左移 16 位， 补充0-1个字节
            return b;
        }


        /// <summary>
        /// 将一个 int 数拆分成两个 short .
        /// A : 0 - 1 位
        /// B : 2 - 3 位
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitInt(this int A, out short B, out short C)
        {
            B = (short)A;
            C = (short)((A & ~((int)ushort.MaxValue)) >> 16);
        }

        /// <summary>
        /// 将两个 byte， 按位做位移操作， 整合为一个 short.
        /// 第一个byte在低1字节 : 0， 第二个byte在高1个字节: 1
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ConcateBytes(this byte A, byte B)
        {
            short b = (short)(A + ((short)B << 8));//B 左移 8 位， 补充第2个字节
            return b;
        }

        /// <summary>
        /// 将一个 short 数拆分成两个 byte .
        /// A : 0  位
        /// B : 1  位
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SplitShort(this short A, out byte B, out byte C)
        {
            B = (byte)A;
            C = (byte)((A & ~((short)byte.MaxValue)) >> 8);
        }


        /// <summary>
        /// 把 vect.x,y,z 的值  Clamp 在 [0..1]
        /// </summary>
        /// <param name="vect">Vect.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp01(this Vector3 vect)
        {
            return new Vector3(Mathf.Clamp01(vect.x),
                Mathf.Clamp01(vect.y),
                Mathf.Clamp01(vect.z));
        }

        /// <summary>
        /// Maximum of two colors
        /// </summary>
        /// <param name="c1">Color1.</param>
        /// <param name="c2">Color2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Max(this Color c1, Color c2)
        {
            return new Color
                (
                Mathf.Max(c1.r, c2.r),
                Mathf.Max(c1.g, c2.g),
                Mathf.Max(c1.b, c2.b),
                Mathf.Max(c1.a, c2.a)
                );
        }

        /// <summary>
        /// Maximum of two color32 
        /// </summary>
        /// <param name="c1">Color1.</param>
        /// <param name="c2">Color2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 Max(this Color32 c1, Color32 c2)
        {
            return new Color32
                (
                (byte)Mathf.Max(c1.r, c2.r),
                (byte)Mathf.Max(c1.g, c2.g),
                (byte)Mathf.Max(c1.b, c2.b),
                (byte)Mathf.Max(c1.a, c2.a)
                );
        }

        /// <summary>
        /// Maximum of two vectors
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 v1, Vector3 v2)
        {
            Vector3 ret = new Vector3(Mathf.Max(v1.x, v2.x),
                Mathf.Max(v1.y, v2.y),
                Mathf.Max(v1.z, v2.z));
            return ret;
        }

        /// <summary>
        /// 对 rotation 做Yaw(水平旋转 N个角度)
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion YawByAngle(this Quaternion rotation, float yaw)
        {
            rotation = rotation * Quaternion.Euler(0, yaw, 0);
            return rotation;
        }

        /// <summary>
        /// 对 rotation 做 Pitch (以X为轴旋转 N个角度)
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion PitchByAngle(this Quaternion rotation, float pitch)
        {
            rotation = rotation * Quaternion.Euler(pitch, 0, 0);
            return rotation;
        }

        /// <summary>
        /// Rolls by angle.
        /// </summary>
        /// <returns>The by angle.</returns>
        /// <param name="rotation">Rotation.</param>
        /// <param name="roll">Roll.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion RollByAngle(this Quaternion rotation, float roll)
        {
            rotation = rotation * Quaternion.Euler(0, 0, roll);
            return rotation;
        }

        /// <summary>
        /// Clamps the vector2.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="v2">V2.</param>
        /// <param name="minV2">Minimum v2.</param>
        /// <param name="maxV2">Max v2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ClampVector2(this Vector2 v2, Vector2 minV2, Vector2 maxV2)
        {
            v2.x = Mathf.Clamp(v2.x, minV2.x, maxV2.x);
            v2.y = Mathf.Clamp(v2.y, minV2.y, maxV2.y);
            return v2;
        }

        /// <summary>
        /// Clamps the vector2int.
        /// Example: Clamp : [2,6] with [0,0] and [5,5] returns : [2,5]
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="v2Int">V2.</param>
        /// <param name="minV2Int">Minimum v2.</param>
        /// <param name="maxV2Int">Max v2. Note: this is inclusive not exclusive.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int ClampVector2Int(this Vector2Int v2Int, Vector2Int minV2Int, Vector2Int maxV2Int)
        {
            v2Int.x = Mathf.Clamp(v2Int.x, minV2Int.x, maxV2Int.y);
            v2Int.y = Mathf.Clamp(v2Int.y, minV2Int.y, maxV2Int.y);
            return v2Int;
        }


        /// <summary>
        /// Clamps the vector3int.
        /// </summary>
        /// <returns>The vector2.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ClampVector3Int(this Vector3Int v3Int, Vector3Int minV3Int, Vector3Int maxV3Int)
        {
            v3Int.x = Mathf.Clamp(v3Int.x, minV3Int.x, maxV3Int.y + 1);
            v3Int.y = Mathf.Clamp(v3Int.y, minV3Int.y, maxV3Int.y + 1);
            v3Int.z = Mathf.Clamp(v3Int.z, minV3Int.z, maxV3Int.z + 1);
            return v3Int;
        }

        /// <summary>
        /// Return the normal of triangle face created by three points
        /// </summary>
        /// <param name="point01"></param>
        /// <param name="point02"></param>
        /// <param name="point03"></param>
        /// <returns></returns>
        public static Vector3 CalculateTriangleFaceNormal(Vector3 point01, Vector3 point02, Vector3 point03)
        {
            Vector3 nrm = Vector3.Cross((point01 - point03).normalized, (point02 - point03).normalized);//tri normal : cross(v1-v3,v2-v3)
            return nrm;
        }

        /// <summary>
        /// Return the normal of quad face created by four points
        /// </summary>
        /// <param name="pointLB"></param>
        /// <param name="pointLT"></param>
        /// <param name="pointRT"></param>
        /// <param name="pointRB"></param>
        /// <returns></returns>
        public static Vector3 CalculateQuadFaceNormal(Vector3 pointLB, Vector3 pointLT, Vector3 pointRT, Vector3 pointRB)
        {
            Vector3 nrm01 = Vector3.Cross((pointLB - pointRT).normalized, (pointLT - pointRT).normalized);
            Vector3 nrm02 = Vector3.Cross((pointLB - pointRB).normalized, (pointRT - pointRB).normalized);
            return (nrm01 + nrm02).normalized;
        }

        /// <summary>
        /// 以 direction 的指向为方向，做一次桶形偏转。
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="torgueAngle">垂直偏角。</param>
        /// <param name="driftAngle">抬起角度。</param>
        public static Vector3 ConeDrift(this Vector3 direction, float torgueAngle, float driftAngle)
        {
            var q = direction.ToRotation();
            var rndQ = q * Quaternion.AngleAxis(torgueAngle, Vector3.forward);//先做X方向转角。
            var rndQ2 = rndQ * Quaternion.AngleAxis(driftAngle, Vector3.left);//再做仰角转动。
            var rndDir = rndQ2 * Vector3.forward;
            return rndDir;
        }

        /// <summary>
        /// Converts Vector3 (XYZ) to Vector4 (XYZW)
        /// </summary>
        /// <param name="vec3"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static Vector4 ToVector4(this Vector3 vec3, float w)
        {
            return new Vector4(vec3.x, vec3.y, vec3.z, w);
        }

        /// <summary>
        /// 给出原数值 single, 和目标值 target, 速度 speed, 令 single 以speed的速度逼近 target。
        /// </summary>
        /// <param name="single">Single.</param>
        /// <param name="target">Target.</param>
        /// <param name="speed">Speed.</param>
        public static float Approach(this float single, float target, float speed, float deltaTime)
        {
            if (Mathf.Approximately(single, target))
            {
                return target;
            }

            int dir = target > single ? 1 : -1;//1:正向逼近, -1:负向逼近
            float velocity = speed * dir * deltaTime;
            single += velocity;
            if (Mathf.Approximately(single, target) ||
                (dir == 1 && single > target)//正向超越
                || (dir == -1 && single < target)) //负向超越
            {
                return target;
            }
            else
            {
                return single;
            }
        }

        /// <summary>
        /// Color's alpha approch to target value at the speed
        /// </summary>
        /// <param name="color"></param>
        /// <param name="targetAlpha"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Color ApproachAlpha(this Color color, float targetAlpha, float speed, float deltaTime)
        {
            float a = color.a.Approach(targetAlpha, speed, deltaTime);
            color.a = a;
            return color;
        }

        /// <summary>
        /// Color approch to target value at the speed
        /// </summary>
        /// <param name="color"></param>
        /// <param name="targetColor"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Color Approach(this Color color, Color targetColor, float speed, float deltaTime)
        {
            float a = color.a.Approach(targetColor.a, speed, deltaTime);
            float r = color.a.Approach(targetColor.r, speed, deltaTime);
            float g = color.a.Approach(targetColor.g, speed, deltaTime);
            float b = color.a.Approach(targetColor.b, speed, deltaTime);
            color.a = a;
            color.r = r;
            color.g = g;
            color.b = b;
            return color;
        }

        /// <summary>
        /// Compare two color , if they are approximate equal.
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static bool Approximate(this Color color1, Color color2, bool compareAlpha)
        {
            return Mathf.Approximately(color1.r, color2.r) &&
                Mathf.Approximately(color1.g, color2.g) &&
                Mathf.Approximately(color1.b, color2.b)
                && (compareAlpha || Approximately(color1.a, color2.a));
        }

        /// <summary>
        /// Compare two color , if their greyscale approximate equal.
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static bool ApproximateGrayscale(this Color color1, Color color2)
        {
            return Mathf.Approximately(color1.grayscale, color2.grayscale);
        }

        ///// <summary>
        ///// Int value approach to target value in speed of delta time.
        ///// </summary>
        ///// <param name="intVal"></param>
        ///// <param name="targetVal"></param>
        ///// <param name="speed"></param>
        ///// <param name="deltaTime"></param>
        ///// <returns></returns>
        //public static int Approach(this int intVal, int targetVal, int speed, float deltaTime)
        //{
        //    return (int)Approach((float)intVal, (float)targetVal, (float)speed, deltaTime);
        //}

        /// <summary>
        /// Damp the int value to target value in speed, then return the final value.
        /// </summary>
        /// <param name="intVal"></param>
        /// <param name="targetValue"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <param name="onUpdate">On damp value update callback.</param>
        /// <param name="onComplete">On damp value complete damping callback.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="onCancel">On cancel callback , with parameter when cancellation is requested. </param>
        /// <returns></returns>
        public static async Task<int> DampTo(this int intVal, int targetValue, uint speed, float deltaTime, System.Action<int> onUpdate = null
            , System.Action<int> onComplete = null, CancellationToken cancellationToken = default(CancellationToken), System.Action<int> onCancel = null)
        {
            try
            {
                if (Mathf.Approximately(speed, 0))
                {
                    throw new UnityException("[DampTo int] : Speed must not be zero!");
                }
                if (intVal == targetValue)
                {
                    return intVal;
                }
                int sign = (targetValue - intVal) > 0 ? 1 : -1;
                int velocity = sign * (int)speed;
                int v = intVal;
                while (true)
                {
                    v += velocity;
                    int newSign = (targetValue - v) > 0 ? 1 : -1;//检查整型过界
                    if (v == targetValue || newSign != sign)//到达
                    {
                        onComplete?.Invoke(targetValue);//complete
                        break;
                    }
                    else
                    {
                        onUpdate?.Invoke(v);
                        await new WaitForGameTime(deltaTime);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            onCancel?.Invoke(v);
                            break;
                        }
                    }
                }
                return targetValue;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
        }

        /// <summary>
        /// Damp the float value to target value in speed, then return the final value.
        /// </summary>
        /// <param name="floatVal"></param>
        /// <param name="targetValue"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <param name="onUpdate">On damp value update callback.</param>
        /// <param name="onComplete">On damp value complete damping callback.</param>
        /// <returns></returns>
        public static async Task<float> DampTo(this float floatVal, float targetValue, float speed, float deltaTime,
            System.Action<float> onUpdate = null, System.Action<float> onComplete = null, CancellationToken cancellationToken = default(CancellationToken), System.Action<float> onCancel = null)
        {
            try
            {
                if (Mathf.Approximately(speed, 0))
                {
                    throw new UnityException("[DampTo float] : Speed must not be zero!");
                }
                if (Mathf.Approximately(floatVal, targetValue))
                {
                    return floatVal;
                }
                if (speed < 0)
                {
                    speed = Mathf.Abs(speed);
                }

                int sign = (targetValue - floatVal) > 0 ? 1 : -1;
                float velocity = sign * speed;
                float v = floatVal;
                Debug.Log("1");
                while (true)
                {
                    v += velocity;
                    int newSign = (targetValue - v) > 0 ? 1 : -1;//检查整型过界
                    if (Mathf.Approximately(v, targetValue) || newSign != sign)//到达
                    {
                        onComplete?.Invoke(targetValue);//complete
                        break;
                    }
                    else
                    {
                        onUpdate?.Invoke(v);
                        await new WaitForGameTime(deltaTime);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            onCancel?.Invoke(v);
                            break;
                        }
                    }
                }
                return targetValue;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
        }

        /// <summary>
        /// Is the intValue Even (return true) or Odd (return false) ? 
        /// </summary>
        /// <returns><c>true</c>, if odd was ised, <c>false</c> otherwise.</returns>
        /// <param name="intValue">Int value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this int intValue)
        {
            return (intValue % 2) == 0;
        }

        /// <summary>
        /// Inverse the matrix4x4, this is a multi-thread available function.
        /// </summary>
        /// <param name="matrix4x4"></param>
        /// <returns></returns>
        public static Matrix4x4 InverseMatrix4x4(Matrix4x4 m)
        {
            float n11 = m[0, 0], n12 = m[1, 0], n13 = m[2, 0], n14 = m[3, 0];
            float n21 = m[0, 1], n22 = m[1, 1], n23 = m[2, 1], n24 = m[3, 1];
            float n31 = m[0, 2], n32 = m[1, 2], n33 = m[2, 2], n34 = m[3, 2];
            float n41 = m[0, 3], n42 = m[1, 3], n43 = m[2, 3], n44 = m[3, 3];

            float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
            float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
            float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
            float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

            float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
            float idet = 1.0f / det;

            Matrix4x4 ret = new Matrix4x4();

            ret[0, 0] = t11 * idet;
            ret[0, 1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
            ret[0, 2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
            ret[0, 3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

            ret[1, 0] = t12 * idet;
            ret[1, 1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
            ret[1, 2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
            ret[1, 3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

            ret[2, 0] = t13 * idet;
            ret[2, 1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
            ret[2, 2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
            ret[2, 3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

            ret[3, 0] = t14 * idet;
            ret[3, 1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
            ret[3, 2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
            ret[3, 3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

            return ret;
        }

        /// <summary>
        /// 转换整形数字为小数点后的浮点值： 12345 --> 0.12345
        /// </summary>
        /// <returns>The fractional float.</returns>
        /// <param name="Int">Int.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Int2FractionalFloat(this int Int)
        {
            if (Int == 0)
                return 0;

            var absInt = Mathf.Abs(Int);
            int seed = 1;
            float sign = Mathf.Sign(Int);
            for (int i = 1; seed <= int.MaxValue; i++)
            {
                seed *= 10;
                if (absInt < seed)
                {
                    float ret = (((float)absInt) / ((float)seed)) * sign;
                    return ret;
                }
            }
            //畸大数: 溢出
            return -1;
        }


        /// <summary>
        /// Same to Matrix.TRS , supports multithread.
        /// </summary>
        /// <param name="p">Position</param>
        /// <param name="q">Rotation</param>
        /// <param name="s">Scale</param>
        /// <returns></returns>
        public static Matrix4x4 GetTRSMatrix(Vector3 p, Quaternion q, Vector3 s)
        {
            Matrix4x4 m = Matrix4x4.zero;
            m.m00 = (1.0f - 2.0f * (q.y * q.y + q.z * q.z)) * s.x;
            m.m10 = (q.x * q.y + q.z * q.w) * s.x * 2.0f;
            m.m20 = (q.x * q.z - q.y * q.w) * s.x * 2.0f;
            m.m30 = 0.0f;
            m.m01 = (q.x * q.y - q.z * q.w) * s.y * 2.0f;
            m.m11 = (1.0f - 2.0f * (q.x * q.x + q.z * q.z)) * s.y;
            m.m21 = (q.y * q.z + q.x * q.w) * s.y * 2.0f;
            m.m31 = 0.0f;
            m.m02 = (q.x * q.z + q.y * q.w) * s.z * 2.0f;
            m.m12 = (q.y * q.z - q.x * q.w) * s.z * 2.0f;
            m.m22 = (1.0f - 2.0f * (q.x * q.x + q.y * q.y)) * s.z;
            m.m32 = 0.0f;
            m.m03 = p.x;
            m.m13 = p.y;
            m.m23 = p.z;
            m.m33 = 1.0f;
            return m;
        }

        /// <summary>
        /// Gets an inversed quaternion, thread safe function.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion GetInverse(this Quaternion q)
        {
            float e = 1 / (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion(-q.x * e, -q.y * e, -q.z * e, q.w * e);
        }

        /// <summary>
        /// Get rotation from transform matrix.
        /// Thread safe function.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Quaternion GetRotation(this Matrix4x4 m)
        {

            float fourXSquaredMinus1 = m.m00 - m.m11 - m.m22;
            float fourYSquaredMinus1 = m.m11 - m.m00 - m.m22;
            float fourZSquaredMinus1 = m.m22 - m.m00 - m.m11;
            float fourWSquaredMinus1 = m.m00 + m.m11 + m.m22;

            int biggestIndex = 0;
            float fourBiggestSquaredMinus1 = fourWSquaredMinus1;
            if (fourXSquaredMinus1 > fourBiggestSquaredMinus1)
            {
                fourBiggestSquaredMinus1 = fourXSquaredMinus1;
                biggestIndex = 1;
            }
            if (fourYSquaredMinus1 > fourBiggestSquaredMinus1)
            {
                fourBiggestSquaredMinus1 = fourYSquaredMinus1;
                biggestIndex = 2;
            }
            if (fourZSquaredMinus1 > fourBiggestSquaredMinus1)
            {
                fourBiggestSquaredMinus1 = fourZSquaredMinus1;
                biggestIndex = 3;
            }

            float biggestVal = Mathf.Sqrt(fourBiggestSquaredMinus1 + 1) * 0.5f;
            float mult = 0.25f / biggestVal;

            Quaternion Result = Quaternion.identity;
            switch (biggestIndex)
            {
                case 0:
                    Result.w = biggestVal;
                    Result.x = (m.m12 - m.m21) * mult;
                    Result.y = (m.m20 - m.m02) * mult;
                    Result.z = (m.m01 - m.m10) * mult;
                    break;
                case 1:
                    Result.w = (m.m12 - m.m21) * mult;
                    Result.x = biggestVal;
                    Result.y = (m.m01 + m.m10) * mult;
                    Result.z = (m.m20 + m.m02) * mult;
                    break;
                case 2:
                    Result.w = (m.m20 - m.m02) * mult;
                    Result.x = (m.m01 + m.m10) * mult;
                    Result.y = biggestVal;
                    Result.z = (m.m12 + m.m21) * mult;
                    break;
                case 3:
                    Result.w = (m.m01 - m.m10) * mult;
                    Result.x = (m.m20 + m.m02) * mult;
                    Result.y = (m.m12 + m.m21) * mult;
                    Result.z = biggestVal;
                    break;

                default:                    // Silence a -Wswitch-default warning in GCC. Should never actually get here. Assert is just for sanity.
                    break;
            }
            return GetInverse(Result);
        }

        /// <summary>
        /// Calculate three plane's intersection position.
        /// </summary>
        /// <returns>The plane intersection.</returns>
        /// <param name="p1">P1.</param>
        /// <param name="p2">P2.</param>
        /// <param name="p3">P3.</param>
        public static Vector3 ThreePlaneIntersection(Plane p1, Plane p2, Plane p3)
        {
            //get the intersection point of 3 planes
            return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
                (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
        }

        /// <summary>
        /// 求 [start, dir] ray 和 rect 的相交点.
        /// 当 start 在 rect 之内的时候， 相交点只会有一个.
        /// 当 start 在 rect 之外,并且射线和Rect有交点的时候， 输出的是较远的那个交点。
        /// 当 start 在 rect 之外，并且射线和 Rect 无交点的时候， 输出的值不可信。
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="boxMin"></param>
        /// <param name="boxMax"></param>
        /// <param name="intersectPoint"></param>
        /// <returns></returns>
        public static float IntersectRayRectSimple(Vector2 start, Vector2 dir, Rect rect, out Vector2 intersectPoint)
        {
            Vector2 invDir = new Vector2(1 / dir.x, 1 / dir.y);

            // Find the ray intersection with box plane
            Vector2 rbmin = (rect.min - start) * invDir;
            Vector2 rbmax = (rect.max - start) * invDir;

            Vector2 rbminmax = new Vector2(dir.x > 0 ? rbmax.x : rbmin.x, dir.y > 0 ? rbmax.y : rbmin.y);
            float rayLength = Mathf.Min(rbminmax.x, rbminmax.y);
            intersectPoint = start + dir * rayLength;
            return rayLength;
        }

        /// <summary>
        /// 给出以 SquadCenter 为中心， 边长 = Edge 的正方形, 计算 Point -> SquadCenter 的线段和此Cube的交点。
        /// 如果 P1 在 Cube 内， 则没有交点，返回 false.
        /// </summary>
        /// <param name="SquadCenter">正方形中心</param>
        /// <param name="Edge">边长</param>
        /// <param name="Point"></param>
        /// <param name="IntersectPoint">相交点</param>
        /// <returns></returns>
        public static bool LineIntersectWithSquad(Vector2 SquadCenter, float Edge, Vector2 Point, out Vector2 IntersectPoint)
        {
            float cubeEdgeHalf = Edge / 2;
            Vector2 LB = SquadCenter.Add(-cubeEdgeHalf, -cubeEdgeHalf);
            Vector2 LT = SquadCenter.Add(-cubeEdgeHalf, cubeEdgeHalf);
            Vector2 RT = SquadCenter.Add(cubeEdgeHalf, cubeEdgeHalf);
            Vector2 RB = SquadCenter.Add(cubeEdgeHalf, -cubeEdgeHalf);
            IntersectPoint = Vector2.zero;

            //如果 P1 处于正方形内部, 返回 false
            if (Point.x > LB.x && Point.x < RT.x && Point.y > LB.y && Point.y < RT.y)
            {
                return false;
            }
            Vector2 vect = Point - SquadCenter;
            //Point 在 SquadCenter 上方:
            if (vect.y > 0)
            {
                //在第一象限:
                if (vect.x > 0)
                {
                    //Point 就在对角边上, 相交点就是 RightTop 点:
                    if (Mathf.Approximately(vect.x, vect.y))
                    {
                        IntersectPoint = RT;
                        return true;
                    }
                    //Point 在第一象限的 45度角下方:
                    else if (vect.x > vect.y)
                    {
                        LineIntersection(Point, SquadCenter, RT, RB, ref IntersectPoint);
                        return true;
                    }
                    //Point 在第一象限的 45度角上方:
                    else
                    {
                        LineIntersection(Point, SquadCenter, LT, RT, ref IntersectPoint);
                        return true;
                    }
                }
                //在Squad Center的正上方:
                else if (Mathf.Approximately(vect.x, 0))
                {
                    IntersectPoint = (RT + LT) / 2;
                    return true;
                }
                //在第二象限:
                else
                {
                    //Point 就在对角边上, 相交点就是 LeftTop 点:
                    if (Mathf.Approximately(-vect.x, vect.y))
                    {
                        IntersectPoint = LT;
                        return true;
                    }
                    //Point 在第二象限的 45度对角边下方:
                    else if (-vect.x > vect.y)
                    {
                        LineIntersection(Point, SquadCenter, LT, LB, ref IntersectPoint);
                        return true;
                    }
                    //Point 在第二象限的 45度对角边上方:
                    else
                    {
                        LineIntersection(Point, SquadCenter, LT, RT, ref IntersectPoint);
                        return true;
                    }
                }
            }
            else if (Mathf.Approximately(vect.y, 0))
            {
                //x > 0:
                if (vect.x > 0)
                {
                    IntersectPoint = (RT + RB) / 2;
                    return true;
                }
                //x < 0
                else
                {
                    IntersectPoint = (LT + LB) / 2;
                    return true;
                }
            }
            //vect.y < 0, 在第三/第四象限:
            else
            {
                //在第四象限:
                if (vect.x > 0)
                {
                    //Point 就在对角边上, 相交点就是 RightTop 点:
                    if (Mathf.Approximately(vect.x, -vect.y))
                    {
                        IntersectPoint = RB;
                        return true;
                    }
                    //Point 在第四象限的 45度角上方:
                    else if (vect.x > -vect.y)
                    {
                        LineIntersection(Point, SquadCenter, RT, RB, ref IntersectPoint);
                        return true;
                    }
                    //Point 在第四象限的 45度角下方:
                    else
                    {
                        LineIntersection(Point, SquadCenter, RB, LB, ref IntersectPoint);
                        return true;
                    }
                }
                //在Squad Center的正下方:
                else if (Mathf.Approximately(vect.x, 0))
                {
                    IntersectPoint = (RB + LB) / 2;
                    return true;
                }
                //在第三象限:
                else //(vect.x < 0)
                {
                    //Point 就在对角边上, 相交点就是 LeftBottom 点:
                    if (Mathf.Approximately(-vect.x, -vect.y))
                    {
                        IntersectPoint = LB;
                        return true;
                    }
                    //Point 在第二象限的 45度对角边上方:
                    else if (-vect.x > -vect.y)
                    {
                        LineIntersection(Point, SquadCenter, LT, LB, ref IntersectPoint);
                        return true;
                    }
                    //Point 在第二象限的 45度对角边下方:
                    else
                    {
                        LineIntersection(Point, SquadCenter, LB, RB, ref IntersectPoint);
                        return true;
                    }
                }
            }
        }


        /// <summary>
        /// 2D 点是否可以投影到 LineStart - end 之间的线段范围内.
        /// </summary>
        /// <param name="Point2D"></param>
        /// <param name="LineStart"></param>
        /// <param name="LineEnd"></param>
        /// <param name="perpendicular">是否允许Point2D和line之间构成垂直角度,默认 true</param>
        /// <returns></returns>
        public static bool IsPointProjectionInsideLineSegment(Vector2 Point2D, Vector2 LineStart, Vector2 LineEnd, bool perpendicular)
        {
            float a1 = Vector2.Angle(Point2D - LineStart, LineEnd - LineStart);
            float a2 = Vector2.Angle(Point2D - LineEnd, LineStart - LineEnd);
            return perpendicular ? (a1 <= 90 && a2 <= 90) : (a1 < 90 && a2 < 90);
        }

        /// <summary>
        /// Calculate 2D Point's distance to a 2D line from start to end.
        /// </summary>
        /// <param name="Point2D"></param>
        /// <param name="LineStart"></param>
        /// <param name="LineEnd"></param>
        /// <returns></returns>
        public static float PointDistanceToLine2D(Vector2 Point2D, Vector2 LineStart, Vector2 LineEnd)
        {
            // Translate the whole situation to the origin.
            Point2D -= LineStart;

            Vector2 lineDirection = (LineEnd - LineStart).normalized;

            // Construct a unit vector pointing 90 degrees right of our lineDirection.
            Vector2 perpendicular = (new Vector2(
                      lineDirection.y,
                     -lineDirection.x)).normalized;

            // Extract the component of our offset pointing in this perpendicular direction.
            return Mathf.Abs(Vector2.Dot(Point2D, perpendicular));
        }

        /// <summary>
        /// Calculate 2D Point's distance to a 2D line from start to end.
        /// 如果点在线段方向的左边，返回负值。否则返回正值。
        /// </summary>
        /// <param name="Point2D"></param>
        /// <param name="LineStart"></param>
        /// <param name="LineEnd"></param>
        /// <returns></returns>
        public static float PointSignedDistanceToLine2D(Vector2 Point2D, Vector2 LineStart, Vector2 LineEnd)
        {
            // Translate the whole situation to the origin.
            Point2D -= LineStart;

            Vector2 lineDirection = (LineEnd - LineStart).normalized;

            // Construct a unit vector pointing 90 degrees right of our lineDirection.
            Vector2 perpendicular = (new Vector2(
                      lineDirection.y,
                     -lineDirection.x)).normalized;

            // Extract the component of our offset pointing in this perpendicular direction.
            return Vector2.Dot(Point2D, perpendicular);
        }

        /// <summary>
        /// Calculate two line segment's intersection point.
        /// Do not calculate the intersection point, faster than another version. 
        /// </summary>
        /// <returns><c>true</c>, if intersection was lined, <c>false</c> otherwise.</returns>
        /// <param name="p1">P1 - Line 1 start point</param>
        /// <param name="p2">P2 - Line 1 end point</param>
        /// <param name="p3">P3 - Line 2 start point</param>
        /// <param name="p4">P4 - Line 2 end point</param>
        public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f/*, num,offset*/;
            float x1lo, x1hi, y1lo, y1hi;
            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;
            // X bound box test/
            if (Ax < 0)
            {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else
            {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0)
            {
                if (x1hi < p4.x || p3.x < x1lo)
                    return false;
            }
            else
            {

                if (x1hi < p3.x || p4.x < x1lo)
                    return false;
            }
            Ay = p2.y - p1.y;
            By = p3.y - p4.y;
            // Y bound box test//
            if (Ay < 0)
            {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else
            {
                y1hi = p2.y;
                y1lo = p1.y;
            }
            if (By > 0)
            {
                if (y1hi < p4.y || p3.y < y1lo)
                    return false;
            }
            else
            {
                if (y1hi < p3.y || p4.y < y1lo)
                    return false;
            }
            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//
            // alpha tests//
            if (f > 0)
            {
                if (d < 0 || d > f)
                    return false;
            }
            else
            {
                if (d > 0 || d < f)
                    return false;
            }
            e = Ax * Cy - Ay * Cx;  // beta numerator//
            // beta tests //
            if (f > 0)
            {
                if (e < 0 || e > f)
                    return false;
            }
            else
            {
                if (e > 0 || e < f)
                    return false;
            }
            // check if they are parallel
#pragma warning disable RECS0018 // 使用相等运算符进行的浮点数比较
            if (f == 0)
#pragma warning restore RECS0018 // 使用相等运算符进行的浮点数比较
                return false;

            return true;
        }
        /// <summary>
        /// Calculate two line segment's intersection point : [P1, P2] Intersect with [P3, P4].
        /// </summary>
        /// <returns><c>true</c>, if intersection was lined, <c>false</c> otherwise.</returns>
        /// <param name="p1">P1 - Line 1 start point</param>
        /// <param name="p2">P2 - Line 1 end point</param>
        /// <param name="p3">P3 - Line 2 start point</param>
        /// <param name="p4">P4 - Line 2 end point</param>
        /// <param name="intersection">Intersection.</param>
        public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
        {
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;
            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;
            // X bound box test/
            if (Ax < 0)
            {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else
            {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0)
            {
                if (x1hi < p4.x || p3.x < x1lo)
                    return false;
            }
            else
            {

                if (x1hi < p3.x || p4.x < x1lo)
                    return false;
            }
            Ay = p2.y - p1.y;
            By = p3.y - p4.y;
            // Y bound box test//
            if (Ay < 0)
            {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else
            {
                y1hi = p2.y;
                y1lo = p1.y;
            }
            if (By > 0)
            {
                if (y1hi < p4.y || p3.y < y1lo)
                    return false;
            }
            else
            {
                if (y1hi < p3.y || p4.y < y1lo)
                    return false;
            }
            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//
            // alpha tests//
            if (f > 0)
            {
                if (d < 0 || d > f)
                    return false;
            }
            else
            {
                if (d > 0 || d < f)
                    return false;
            }
            e = Ax * Cy - Ay * Cx;  // beta numerator//
            // beta tests //
            if (f > 0)
            {
                if (e < 0 || e > f)
                    return false;
            }
            else
            {
                if (e > 0 || e < f)
                    return false;
            }
            // check if they are parallel
#pragma warning disable RECS0018 // 使用相等运算符进行的浮点数比较
            if (f == 0)
#pragma warning restore RECS0018 // 使用相等运算符进行的浮点数比较
                return false;

            // compute intersection coordinates //
            num = d * Ax; // numerator //
            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
            //    intersection.x = p1.x + (num+offset) / f;
            intersection.x = p1.x + num / f;
            num = d * Ay;
            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
            //    intersection.y = p1.y + (num+offset) / f;
            intersection.y = p1.y + num / f;
            return true;
        }


        /// <summary>
        /// Remove roll from euler angle.
        /// </summary>
        /// <returns>The roll.</returns>
        /// <param name="quaternion">Quaternion.</param>
        public static Quaternion PitchNYaw(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return Quaternion.Euler(euler.x, euler.y, 0);
        }

        /// <summary>
        /// Remove yaw and roll from euler angle.
        /// </summary>
        /// <returns>The roll.</returns>
        /// <param name="quaternion">Quaternion.</param>
        public static Quaternion Pitch(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return Quaternion.Euler(euler.x, 0, 0);
        }

        /// <summary>
        /// Remove pitch and roll from euler angle.
        /// </summary>
        /// <returns>The roll.</returns>
        /// <param name="quaternion">Quaternion.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Yaw(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return Quaternion.Euler(0, euler.y, 0);
        }

        /// <summary>
        /// 返回一个随机的符号 ，1, 或者 -1.
        /// </summary>
        /// <returns>The signed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandomSigned()
        {
            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                return 1;
            }
            else return -1;
        }

        /// <summary>
        /// Remove pitch and yaw from euler angle.
        /// </summary>
        /// <returns>The roll.</returns>
        /// <param name="quaternion">Quaternion.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Roll(this Quaternion quaternion)
        {
            Vector3 euler = quaternion.eulerAngles;
            return Quaternion.Euler(0, 0, euler.z);
        }

        /// <summary>
        /// Calculate quaternion diff = lhs - rhs
        /// </summary>
        /// <returns>The iff.</returns>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion QDiff(this Quaternion lhs, Quaternion rhs)
        {
            return Quaternion.Inverse(rhs) * lhs;
        }

        /// <summary>
        /// Calculate quaternion sum : lhs + rhs[0] + rhs[1] + rhs[2] ...
        /// </summary>
        /// <returns>The plus.</returns>
        /// <param name="lhs">Lhs.</param>
        /// <param name="rhs">Rhs.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion QSum(this Quaternion lhs, params Quaternion[] rhs)
        {
            Quaternion qSum = lhs;
            for (int i = 0; i < rhs.Length; i++)
            {
                qSum = qSum * rhs[i];
            }
            return qSum;
        }

        /// <summary>
        /// yaw angle diff : lhs.yaw - rhs.yaw
        /// </summary>
        /// <returns>The diff.</returns>
        /// <param name="lhs">lhs.</param>
        /// <param name="rhs">rhs.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float YawDiff(this Quaternion lhs, Quaternion rhs)
        {
            Quaternion yawlhs = lhs.Yaw();
            Quaternion yawrhs = rhs.Yaw();
            var qdiff = yawlhs.QDiff(yawrhs);
            return PrettyAngle(qdiff.eulerAngles.y);
        }



        /// <summary>
        /// 设置四元数的 Pitch (俯仰角)
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="Pitch"></param>
        public static Quaternion SetPitch(this Quaternion lhs, float Pitch)
        {
            var e = lhs.eulerAngles;
            e.x = Pitch;
            return Quaternion.Euler(e);
        }


        /// <summary>
        /// 设置四元数的 Yaw (航向)
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="Yaw"></param>
        public static Quaternion SetYaw(this Quaternion lhs, float Yaw)
        {
            var e = lhs.eulerAngles;
            e.y = Yaw;
            return Quaternion.Euler(e);
        }

        /// <summary>
        /// 设置四元数的 Roll (航向)
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="Yaw"></param>
        public static Quaternion SetRoll(this Quaternion lhs, float Roll)
        {
            var e = lhs.eulerAngles;
            e.z = Roll;
            return Quaternion.Euler(e);
        }

        /// <summary>
        /// Distribute given points in circle with radius as 1 at (0,0,0)
        /// </summary>
        /// <param name="pointCount"></param>
        /// <param name="smoothRate"></param>
        /// <returns></returns>
        public static List<Vector2> SunflowerDistribution(int pointCount, int smoothRate)
        {
            List<Vector2> points = new List<Vector2>();

            int boundary = Mathf.RoundToInt(smoothRate * Mathf.Sqrt(pointCount)); // number of boundary points
            float phi = (Mathf.Sqrt(5) + 1) / 2; //golden ratio

            for (int i = 1; i <= pointCount; i++)
            {
                float radius = 1;
                if (i < pointCount - boundary)
                {
                    radius = Mathf.Sqrt(i - 1 / 2) / Mathf.Sqrt(pointCount - (boundary + 1) / 2);
                }
                float theta = 2 * Mathf.PI * i / Mathf.Pow(phi, 2);

                points.Add(new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta)));
            }

            return points;
        }

        /// <summary>
        /// Get value at normalizedT in a bezier spline control by the 4 given points
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="startPointControlPoint"></param>
        /// <param name="endPointControlPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="normalizedT"></param>
        /// <returns></returns>
        public static Vector3 BezierLerp(Vector3 startPoint, Vector3 startPointControlPoint, Vector3 endPointControlPoint, Vector3 endPoint, float normalizedT)
        {
            var oneMinusLocalT = 1 - normalizedT;
            return (oneMinusLocalT * oneMinusLocalT * oneMinusLocalT * startPoint +
                   3f * oneMinusLocalT * oneMinusLocalT * normalizedT * startPointControlPoint +
                   3f * oneMinusLocalT * normalizedT * normalizedT * endPointControlPoint +
                   normalizedT * normalizedT * normalizedT * endPoint);
        }
    }
}