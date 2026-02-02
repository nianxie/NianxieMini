using System;
using System.Collections.Generic;
using Nianxie.Utils;
using UnityEngine;

namespace Nianxie.Craft
{
    // code from https://discussions.unity.com/t/generate-physics-shape-from-sprite/809122/4
    public class ContourTracer
    {
        private const float TOLERANCE_RATIO = 0.015f; // 0.01f 大约表示在LineUtility.Simplify的时候1%的精度
        private const uint OUTLINER_GAP_LENGTH = 3; // How much difference in pixels in a straight line is considered a gap. This can help smooth out the outline a bit.
        private const float OUTLINER_PRODUCT = 0.99f; // Product for optimizing the outline based on angle. 1 means no optimization. This value should be kept pretty high if you want to maintain round shapes. Note that some points (e.g. outer angles) are never optimized.
        public static List<Vector2[]> CalcPolygon(Color32[] pixels, Vector2Int textureSize, Vector2 pivot, float pixelsPerUnit)
        {
            var tracer = new ContourTracer();
            tracer.Trace(pixels, textureSize, pivot, pixelsPerUnit, OUTLINER_GAP_LENGTH, OUTLINER_PRODUCT);
            var path = new List<Vector2[]>();
            var points = new List<Vector2>();
            for (var i = 0; i < tracer.pathCount; i++)
            {
                tracer.GetPath(i, ref points);
                var result = new List<Vector2>();
                // 基于包围盒大小动态计算 tolerance
                Bounds bounds = new Bounds(points[0], Vector3.zero);
                for (int j = 1; j < points.Count; j++)
                {
                    bounds.Encapsulate(points[j]);
                }
                float sizeMetric = bounds.size.magnitude;
                if (sizeMetric < 0.0001f) sizeMetric = 1f;
                float adaptiveTolerance = sizeMetric * TOLERANCE_RATIO;

                // 进行简化
                LineUtility.Simplify(points, adaptiveTolerance, result);
                // 如果点数过多(>30)，将tolerance乘以1.5倍再次计算
                for(int tryCount=0;tryCount<5 && result.Count>30;tryCount++)
                {
                    var tryNextTolerance = adaptiveTolerance * 1.5f;
                    var tryNextResult = new List<Vector2>();
                    LineUtility.Simplify(points, tryNextTolerance, tryNextResult);
                    if (tryNextResult.Count < 3)
                    {
                        break;
                    }
                    result = tryNextResult;
                    adaptiveTolerance = tryNextTolerance;
                }
                if (result.Count >= 3)
                {
                    path.Add(result.ToArray());
                }
            }
            return path;
        }

        private List<Stack<Vector2Int>> pixelPaths = new List<Stack<Vector2Int>>();
        /// <include file='../Documentation.xml' path='docs/ContourTracer/pathCount/*'/>
        public int pathCount { get; private set; }
        private float pointMultiplier;
        private Vector2 pointOffset;

        private enum Direction
        {
            Front,
            Right,
            Rear,
            Left
        }

        private enum Code
        {
            Inner,
            InnerOuter,
            Straight,
            Outer
        }

        /// <include file='../Documentation.xml' path='docs/ContourTracer/GetPath/*'/>
        public Vector2[] GetPath(int index)
        {
            var points = pixelPaths[index].ToArray();
            return Array.ConvertAll(points, point => (Vector2)point * pointMultiplier - pointOffset);
        }

        /// <include file='../Documentation.xml' path='docs/ContourTracer/GetPath/*'/>
        public int GetPath(int index, ref List<Vector2> path)
        {
            var points = pixelPaths[index].ToArray();
            int pointIndex;

            Vector2 Point() => (Vector2)points[pointIndex] * pointMultiplier - pointOffset;

            if(points.Length > path.Count)
            {
                for(pointIndex = 0; pointIndex < path.Count; ++pointIndex)
                {
                    path[pointIndex] = Point();
                }
                for(; pointIndex < points.Length; ++pointIndex)
                {
                    path.Add(Point());
                }
            }
            else
            {
                for(pointIndex = 0; pointIndex < points.Length; ++pointIndex)
                {
                    path[pointIndex] = Point();
                }
                path.RemoveRange(pointIndex, path.Count - points.Length);
            }

            return pointIndex;
        }

        private void Trace(Color32[] pixels, Vector2Int textureSize, Vector2 pivot, float pixelsPerUnit, uint gapLength, float product)
        {
            //Debug.LogWarning($"Trace method is still missing support for InnerOuter points.");
            //Debug.LogWarning($"Trace method is still missing support for Rect.");

            var textureWidth = textureSize.x;
            var textureHeight = textureSize.y;

            pathCount = 0;
            pointMultiplier = 1 / pixelsPerUnit;
            pivot.x *= textureWidth - 1f;
            pivot.y *= textureHeight - 1f;
            pointOffset = pivot * pointMultiplier;

            Code code;
            var point = Vector2Int.zero;
            Direction direction = Direction.Front;

            Code lastLineCode;
            float lineLength;
            float maxLineLength;
            Vector2 lastDir;

            var found = new HashSet<Vector2Int>();
            Stack<Vector2Int> stack;
            var inside = false;

            #region Methods
            bool IsBorder(int _x, int _y)
            {
                int pixelIndex = _y * textureWidth + _x;
                return pixels[pixelIndex].a != 0f;
            }
            bool IsBorderSafe(int _x, int _y) => _y >= 0 && _y < textureHeight && _x >= 0 && _x < textureWidth && IsBorder(_x, _y);

            void TurnPos(ref int _x, ref int _y)
            {
                int tempX;
                switch(direction)
                {
                    case Direction.Right:
                        tempX = _x;
                        _x = _y;
                        _y = -tempX;
                        break;
                    case Direction.Rear:
                        _x = -_x;
                        _y = -_y;
                        break;
                    case Direction.Left:
                        tempX = _x;
                        _x = -_y;
                        _y = tempX;
                        break;
                }
            }

            bool NOffset(int _x, int _y)
            {
                TurnPos(ref _x, ref _y);
                return IsBorderSafe(point.x + _x, point.y + _y);
            }

            bool N0() => IsBorder(point.x, point.y);
            bool N1() => NOffset(-1, -1);
            bool N2() => NOffset(-1, 0);
            bool N3() => NOffset(-1, 1);
            bool N4() => NOffset(0, 1);

            void Encode(Code _code)
            {
                switch(_code)
                {
                    case Code.Inner:
                        if(code != Code.Outer)
                        {
                            if(code == Code.Straight)
                            {
                                if(lastLineCode == Code.Inner)
                                {
                                    var lastPoint = stack.Pop();
                                    Smooth();
                                    stack.Push(lastPoint);

                                    lastDir = Vector2.zero;
                                    stack.Push(point);

                                    //var lastPoint = list[list.Count - 1];
                                    //list.RemoveAt(list.Count - 1);
                                    //Smooth();
                                    //list.Add(lastPoint);

                                    //lastDir = Vector2.zero;
                                    //list.Add(point);
                                }
                                else if(lineLength >= maxLineLength)
                                {
                                    stack.Push(point);
                                    //list.Add(point);
                                }
                            }
                            else
                            {
                                stack.Push(point);
                                //list.Add(point);
                            }
                        }

                        maxLineLength = lineLength + gapLength;
                        lineLength = 0;
                        break;
                    case Code.InnerOuter:
                        if(code != Code.InnerOuter)
                        {
                            stack.Push(point);
                            //list.Add(point);
                        }
                        break;
                    case Code.Straight:
                        if(code != Code.Straight)
                        {
                            lastLineCode = code;

                            if(code == Code.Outer)
                            {
                                if(stack.Peek() == point)
                                {
                                    break;
                                }

                                Smooth();
                            }

                            stack.Push(point);
                            //list.Add(point);
                        }

                        ++lineLength;
                        break;
                    case Code.Outer:
                        if(code != Code.Inner)
                        {
                            if(code == Code.Straight)
                            {
                                if(lastLineCode != Code.Inner || lineLength > maxLineLength)
                                {
                                    maxLineLength = float.PositiveInfinity;
                                    lastDir = Vector2.zero;
                                }
                                else
                                {
                                    //list.RemoveAt(list.Count - 1);
                                    stack.Pop();
                                    Smooth();
                                }
                            }
                            else if(code == Code.Outer)
                            {
                                if(stack.Peek() == point)
                                {
                                    break;
                                }

                                Smooth();
                                lastDir = Vector2.zero;
                            }

                            stack.Push(point);
                            //list.Add(point);
                            lineLength = float.PositiveInfinity;
                        }
                        break;
                }

                code = _code;
            }

            void Move(int _x, int _y)
            {
                TurnPos(ref _x, ref _y);
                point.x += _x;
                point.y += _y;

                found.Add(point);
            }

            void Turn(Direction _direction)
            {
                direction = (Direction)(((int)direction + (int)_direction) % 4);
            }

            void Smooth()
            {
                //Vector2 lastPoint = list[list.Count - 1];
                var dir = (point - (Vector2)stack.Peek()).normalized;
                if(Vector2.Dot(dir, lastDir) > product)
                {
                    //list.RemoveAt(list.Count - 1);
                    stack.Pop();
                }

                lastDir = dir;
            }
            #endregion

            for(point.x = 0; point.x < textureWidth; ++point.x)
            {
                for(point.y = 0; point.y < textureHeight; ++point.y)
                {
                    // Scan for non-transparent pixel
                    if(found.Contains(point))
                    {
                        // Entering an already discovered border
                        inside = true;
                        continue;
                    }

                    bool isBorder = N0();
                    if(inside)
                    {
                        inside = isBorder;
                        continue;
                    }
                    else if(isBorder)
                    {
                        if(pathCount >= pixelPaths.Count)
                        {
                            //pixelPaths.Add(new List<Vector2Int>());
                            pixelPaths.Add(new Stack<Vector2Int>());
                        }
                        else
                        {
                            pixelPaths[pathCount].Clear();
                        }
                        stack = pixelPaths[pathCount];

                        var startPoint = point;
                        var startDirection = direction;
                        code = Code.InnerOuter;
                        lastLineCode = Code.Straight;
                        lineLength = 0;
                        maxLineLength = float.PositiveInfinity;
                        lastDir = Vector2.zero;
                        do
                        {
                            // Stage 1
                            if(N1())
                            {
                                if(N2())
                                {
                                    // Case 1
                                    Encode(Code.Inner);
                                    Move(-1, -1);
                                    Turn(Direction.Rear);
                                }
                                else
                                {
                                    // Case 2
                                    Encode(Code.InnerOuter);
                                    Move(-1, -1);
                                    Turn(Direction.Rear);
                                }
                            }
                            else
                            {
                                if(N2())
                                {
                                    // Case 3
                                    Encode(Code.Straight);
                                    Move(-1, 0);
                                    Turn(Direction.Left);
                                }
                                else
                                {
                                    // Case 4
                                    Encode(Code.Outer);
                                }
                            }

                            // Stage 2
                            if(N3())
                            {
                                if(N4())
                                {
                                    // Case 6
                                    Encode(Code.Inner);
                                    Move(-1, 1);
                                }
                                else
                                {
                                    // Case 5
                                    Encode(Code.InnerOuter);
                                    Move(-1, 1);
                                }
                            }
                            else if(N4())
                            {
                                // Case 7
                                Encode(Code.Straight);
                                Move(0, 1);
                                Turn(Direction.Right);
                            }
                            else
                            {
                                // Case 8
                                Encode(Code.Outer);
                                Turn(Direction.Rear);
                            }
                        } while(point != startPoint || direction != startDirection);

                        if(code == Code.Straight && lastLineCode == Code.Inner)
                        {
                            //list.RemoveAt(list.Count - 1);
                            stack.Pop();
                        }

                        if (stack.Count > 0)
                        {
                            Smooth();
                        }

                        if(stack.Count >= 3)
                        {
                            ++pathCount;
                        }

                        inside = true;
                    }
                }
            }
        }
    }
}
