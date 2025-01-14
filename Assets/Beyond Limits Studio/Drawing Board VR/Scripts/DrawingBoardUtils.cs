using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeyondLimitsStudios
{
    namespace VRInteractables
    {
        public class DrawingBoardUtils
        {
            /// <summary>
            /// Sets pixels in given distance to line on given RenderTexture using ComputeShader.
            /// </summary>
            public static void SetPixelsInRangeOnLineCS(RenderTexture texture, Vector2Int lineStart, Vector2Int lineEnd, int range, Color color, ComputeShader computeShader)
            {
                int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;

                if (lineStart.x < lineEnd.x)
                {
                    minX = lineStart.x - range;
                    maxX = lineEnd.x + range;
                }
                else
                {
                    minX = lineEnd.x - range;
                    maxX = lineStart.x + range;
                }

                if (lineStart.y < lineEnd.y)
                {
                    minY = lineStart.y - range;
                    maxY = lineEnd.y + range;
                }
                else
                {
                    minY = lineEnd.y - range;
                    maxY = lineStart.y + range;
                }

                computeShader.SetInt("StartX", minX);
                computeShader.SetInt("StartY", minY);

                computeShader.SetVector("Color", color);

                computeShader.SetInt("Width", (maxX - minX));
                computeShader.SetInt("Height", (maxY - minY));

                computeShader.SetVector("Line", new Vector4(lineStart.x, lineStart.y, lineEnd.x, lineEnd.y));

                computeShader.SetInt("Range", range);

                computeShader.SetTexture(3, "tex", texture);

                // computeShader.Dispatch(3, ((maxX - minX) * (maxY - minY)) / 32 + 1, 1, 1);
                computeShader.Dispatch(3, (maxX - minX) / 8 + 1, (maxY - minY) / 8 + 1, 1);
            }

            /// <summary>
            /// Sets pixels in given distance to point on given RenderTexture using ComputeShader.
            /// </summary>
            public static void SetPixelsInRangeCS(RenderTexture texture, Vector2Int pos, int range, Color color, ComputeShader computeShader)
            {
                computeShader.SetInt("StartX", pos.x - range);
                computeShader.SetInt("StartY", pos.y - range);

                computeShader.SetVector("Color", color);

                computeShader.SetInt("Range", range);

                computeShader.SetTexture(2, "tex", texture);

                // computeShader.Dispatch(2, ((range * 2) * (range * 2)) / 32 + 1, 1, 1);
                computeShader.Dispatch(2, range * 2 / 8 + 1, range * 2 / 8 + 1, 1);
            }

            /// <summary>
            /// Sets pixels in given polygon on given RenderTexture using ComputeShader.
            /// </summary>
            public static void SetPixelsInPolygonCS(RenderTexture texture, List<Vector2Int> polygonCorners, Color color, ComputeShader computeShader, ref ComputeBuffer cornersBuffer)
            {
                if (polygonCorners.Count < 3)
                {
                    return;
                }

                int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
                foreach (Vector2Int corner in polygonCorners)
                {
                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }

                SetPixelsInPolygonCS(texture, texture.width, texture.height, polygonCorners, color, computeShader, ref cornersBuffer);
            }

            /// <summary>
            /// Sets pixels in given polygon on given RenderTexture using ComputeShader.
            /// </summary>
            public static void SetPixelsInPolygonCS(RenderTexture texture, Vector2Int[] polygonCorners, Color color, ComputeShader computeShader, ref ComputeBuffer cornersBuffer)
            {
                if (polygonCorners.Length < 3)
                {
                    return;
                }

                int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
                foreach (Vector2Int corner in polygonCorners)
                {
                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }

                SetPixelsInPolygonCS(texture, texture.width, texture.height, polygonCorners, color, computeShader, ref cornersBuffer);
            }

            public static void SetPixelsInPolygonCS_notInUse(Texture2D texture, Vector2Int[] polygonCorners, Color color, ComputeShader computeShader)
            {
                if (polygonCorners.Length < 3)
                {
                    return;
                }

                int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
                foreach (Vector2Int corner in polygonCorners)
                {
                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }

                foreach (var item in GetPixelsInPolygonCS_notInUse(texture.width, texture.height, polygonCorners, color, computeShader))
                {
                    if (item.x != -1)
                    {
                        texture.SetPixel(item.x, item.y, color);
                    }
                }
            }

            public static void SetPixelsInPolygon(Texture2D texture, Vector2Int[] polygonCorners, Color color)
            {
                if (polygonCorners.Length < 3)
                {
                    return;
                }

                int minX = texture.width, maxX = 0, minY = texture.height, maxY = 0;
                foreach (Vector2Int corner in polygonCorners)
                {
                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (IsPointInPolygon(new Vector2Int(x, y), polygonCorners))
                        {
                            // pointsInRectangle.Add(new Vector2Int(x, y));
                            texture.SetPixel(x, y, color);
                        }
                    }
                }
            }

            /// <summary>
            /// Sets pixels in given polygon on given RenderTexture using ComputeShader.
            /// </summary>
            private static void SetPixelsInPolygonCS(RenderTexture renderTexture, int width, int height, List<Vector2Int> corners, Color color, ComputeShader computeShader, ref ComputeBuffer cornersBuffer)
            {
                if (cornersBuffer.count < corners.Count)
                {
                    Debug.LogWarning($"Creating new corners buffer for {corners.Count} vertices. Consider setting bigger cornersBufferSize on DrawingBoard.");
                    cornersBuffer.Release();
                    cornersBuffer = new ComputeBuffer(corners.Count, sizeof(int) * 2);
                }

                cornersBuffer.SetData(corners);
                computeShader.SetInt("CornersCount", corners.Count);

                int minX = width, maxX = 0, minY = height, maxY = 0;
                for (int i = 0; i < corners.Count; i++)
                {
                    if (corners[i].x < minX) minX = corners[i].x;
                    if (corners[i].x > maxX) maxX = corners[i].x;
                    if (corners[i].y < minY) minY = corners[i].y;
                    if (corners[i].y > maxY) maxY = corners[i].y;
                }

                int xDiff = (maxX - minX + 1);
                int yDiff = (maxY - minY + 1);

                computeShader.SetInt("StartX", minX);
                computeShader.SetInt("StartY", minY);

                computeShader.SetVector("Color", color);

                computeShader.SetInt("Width", xDiff);
                computeShader.SetInt("Height", yDiff);

                computeShader.SetTexture(1, "tex", renderTexture);

                computeShader.SetBuffer(1, "Corners", cornersBuffer);

                computeShader.Dispatch(1, xDiff / 8 + 1, yDiff / 8 + 1, 1);
            }

            /// <summary>
            /// Sets pixels in given polygon on given RenderTexture using ComputeShader.
            /// </summary>
            private static void SetPixelsInPolygonCS(RenderTexture renderTexture, int width, int height, Vector2Int[] corners, Color color, ComputeShader computeShader, ref ComputeBuffer cornersBuffer)
            {
                if (cornersBuffer.count < corners.Length)
                {
                    Debug.LogWarning($"Creating new corners buffer for {corners.Length} vertices. Consider setting bigger cornersBufferSize on DrawingBoard.");
                    cornersBuffer.Release();
                    cornersBuffer = new ComputeBuffer(corners.Length, sizeof(int) * 2);
                }

                cornersBuffer.SetData(corners);
                computeShader.SetInt("CornersCount", corners.Length);

                int minX = width, maxX = 0, minY = height, maxY = 0;
                for (int i = 0; i < corners.Length; i++)
                {
                    if (corners[i].x < minX) minX = corners[i].x;
                    if (corners[i].x > maxX) maxX = corners[i].x;
                    if (corners[i].y < minY) minY = corners[i].y;
                    if (corners[i].y > maxY) maxY = corners[i].y;
                }

                int xDiff = (maxX - minX + 1);
                int yDiff = (maxY - minY + 1);

                computeShader.SetInt("StartX", minX);
                computeShader.SetInt("StartY", minY);

                computeShader.SetVector("Color", color);

                computeShader.SetInt("Width", xDiff);
                computeShader.SetInt("Height", yDiff);

                computeShader.SetTexture(1, "tex", renderTexture);

                computeShader.SetBuffer(1, "Corners", cornersBuffer);

                computeShader.Dispatch(1, xDiff / 8 + 1, yDiff / 8 + 1, 1);
            }

            private static Vector2Int[] GetPixelsInPolygonCS_notInUse(int width, int height, Vector2Int[] corners, Color color, ComputeShader computeShader)
            {
                ComputeBuffer cornersBuffer = new ComputeBuffer(corners.Length, sizeof(int) * 2);
                cornersBuffer.SetData(corners);

                int minX = width, maxX = 0, minY = height, maxY = 0;
                for (int i = 0; i < corners.Length; i++)
                {
                    if (corners[i].x < minX) minX = corners[i].x;
                    if (corners[i].x > maxX) maxX = corners[i].x;
                    if (corners[i].y < minY) minY = corners[i].y;
                    if (corners[i].y > maxY) maxY = corners[i].y;
                }

                int xDiff = (maxX - minX + 1);
                int yDiff = (maxY - minY + 1);

                Vector2Int[] result = new Vector2Int[xDiff * yDiff];

                // computeShader.SetTexture(0, "Result", texture);
                computeShader.SetInt("StartX", minX);
                computeShader.SetInt("StartY", minY);

                computeShader.SetVector("Color", color);

                computeShader.SetInt("Width", xDiff);
                computeShader.SetInt("Height", yDiff);

                ComputeBuffer resultBuffer = new ComputeBuffer(result.Length, sizeof(int) * 2);
                resultBuffer.SetData(result);

                computeShader.SetBuffer(0, "Result", resultBuffer);

                computeShader.SetBuffer(0, "Corners", cornersBuffer);

                // computeShader.Dispatch(0, result.Length / 32 + 1, 1, 1);
                computeShader.Dispatch(0, xDiff / 8 + 1, yDiff / 8 + 1, 1);

                resultBuffer.GetData(result);

                resultBuffer.Release();
                cornersBuffer.Release();

                return result;
            }

            /// <summary>
            /// Returns list of texture coordinates inside of given polygon.
            /// </summary>
            public static List<Vector2Int> GetPointsInPolygon(int textureWidth, int textureHeight, Vector2Int[] polygonCorners)
            {
                // Utwórz tablicê z rogami prostok¹ta
                // Vector2Int[] rectangleCorners = { corner1, corner2, corner3, corner4 };

                if (polygonCorners.Length < 3)
                {
                    return new List<Vector2Int>();
                }

                // ZnajdŸ granice prostok¹ta
                int minX = textureWidth, maxX = 0, minY = textureHeight, maxY = 0;
                foreach (Vector2Int corner in polygonCorners)
                {
                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }

                // Utwórz listê z punktami znajduj¹cymi siê w prostok¹cie
                List<Vector2Int> pointsInRectangle = new List<Vector2Int>();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (IsPointInPolygon(new Vector2Int(x, y), polygonCorners))
                        {
                            pointsInRectangle.Add(new Vector2Int(x, y));
                        }
                    }
                }

                return pointsInRectangle;
            }

            /// <summary>
            /// Returns true if given texture coordinate is inside polygon.
            /// </summary>
            public static bool IsPointInPolygon(Vector2Int point, Vector2Int[] polygon)
            {
                int polygonLength = polygon.Length, i = 0;
                bool inside = false;
                float pointX = point.x, pointY = point.y;
                float startX, startY, endX, endY;
                Vector2Int endPoint = polygon[polygonLength - 1];
                endX = endPoint.x;
                endY = endPoint.y;
                while (i < polygonLength)
                {
                    startX = endX;
                    startY = endY;
                    endPoint = polygon[i++];
                    endX = endPoint.x;
                    endY = endPoint.y;
                    // inside ^= (endY > pointY ^ startY > pointY) && ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
                    inside = inside ^ ((endY > pointY ^ startY > pointY) && ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY)));
                }

                return inside;
            }

            public static Vector2Int[] GetPixelsInRectangle(Vector2 boardSize, Vector2Int start, Vector2Int end, int range)
            {
                List<Vector2Int> points = new List<Vector2Int>();

                bool right = start.x < end.x;
                bool up = start.y < end.y;

                for (int i = Mathf.Max(0, (right ? start.x : end.x) - range); i <= Mathf.Min(boardSize.x - 1, (right ? end.x : start.x) + range); i++)
                {
                    for (int j = Mathf.Max(0, (up ? start.y : end.y) - range); j <= Mathf.Min(boardSize.y - 1, (up ? end.y : start.y) + range); j++)
                    {
                        // if (GetDistanceToFiniteLine(new Vector3(i, j, 0), new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0)) < range)
                        if (GetSqrDistanceToFiniteLine(new Vector3(i, j, 0), new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0)) < range * range)
                        {
                            points.Add(new Vector2Int(i, j));
                            // texture.SetPixel(i, j, col);
                            // Debug.Log($"{i} {j}");
                        }
                    }
                }

                return points.ToArray();
            }

            public static Vector2Int[] GetPixelsInRectangle(Vector2Int boardSize, Vector2Int point1, Vector2Int point2, Vector2Int point3, Vector2Int point4)
            {
                Vector2 dir1 = point3 - point1;
                Vector2 dir2 = point4 - point2;

                SortIntoCorners(ref point1, ref point2, ref point3, ref point4);

                Vector2 bottomLeft = point1 + new Vector2(-0.5f, -0.5f);
                Vector2 bottomRight = point4 + new Vector2(0.5f, -0.5f);
                Vector2 TopLeft = point2 + new Vector2(-0.5f, 0.5f);
                Vector2 TopRight = point3 + new Vector2(0.5f, 0.5f);

                // List<Vector2Int> points = new List<Vector2Int>();
                Vector2Int[] points = new Vector2Int[(boardSize.x + 1) * (boardSize.y + 1)];

                float angle = Vector2.Angle(bottomRight - new Vector2(1f, 0f), bottomRight - bottomLeft);

                for (int x = 0; x < boardSize.x; x++)
                {
                    for (int y = 0; y < boardSize.y; y++)
                    {

                    }
                }

                // return points.ToArray();
                return points;
            }
            
            public static void SortIntoCorners(ref Vector2Int p1, ref Vector2Int p2, ref Vector2Int p3, ref Vector2Int p4)
            {
                List<Vector2Int> points = new List<Vector2Int>();

                Vector2Int left1 = p1;
                Vector2Int left2 = p2;

                Vector2Int right1 = p3;
                Vector2Int right2 = p4;

                points.Add(p1);
                points.Add(p2);
                points.Add(p3);
                points.Add(p4);


                foreach (var point in points)
                    if (point.x < left1.x)
                        left1 = point;

                points.Remove(left1);

                foreach (var point in points)
                    if (point.x < left2.x)
                        left2 = point;

                points.Remove(left2);

                right1 = points[0];
                right2 = points[1];

                if (left1.y < left2.y)
                {
                    p1 = left1;
                    p2 = left2;
                }
                else
                {
                    p2 = left1;
                    p1 = left2;
                }

                if (right1.y < right2.y)
                {
                    p3 = right1;
                    p4 = right2;
                }
                else
                {
                    p4 = right1;
                    p3 = right2;
                }
            }

            /// <summary>
            /// (3D space) Returns distance from point to finite line defined by line_start and line_end.
            /// </summary>
            public static float GetSqrDistanceToFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
            {
                Vector3 pos = GetClosestPointOnFiniteLine(point, line_start, line_end);

                return Vector3.SqrMagnitude(point - pos);
            }

            /// <summary>
            /// (2D space) Returns squared distance from point to finite line defined by line_start and line_end.
            /// </summary>
            public static float GetDistanceToFiniteLine(Vector2 point, Vector2 line_start, Vector2 line_end)
            {
                Vector2 pos = GetClosestPointOnFiniteLine(point, line_start, line_end);

                return Vector2.Distance(point, pos);
            }

            /// <summary>
            /// (2D space) Returns point closest to finite line defined by line_start and line_end.
            /// </summary>
            public static Vector2 GetClosestPointOnFiniteLine(Vector2 point, Vector2 line_start, Vector2 line_end)
            {
                Vector2 line_direction = line_end - line_start;
                float line_length = line_direction.magnitude;
                line_direction.Normalize();
                float project_length = Mathf.Clamp(Vector2.Dot(point - line_start, line_direction), 0f, line_length);
                return line_start + line_direction * project_length;
            }

            /// <summary>
            /// (3D space) Returns distance from point to finite line defined by line_start and line_end.
            /// </summary>
            public static float GetDistanceToFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
            {
                Vector3 pos = GetClosestPointOnFiniteLine(point, line_start, line_end);

                return Vector3.Distance(point, pos);
            }

            /// <summary>
            /// (3D space) Returns point closest to finite line defined by line_start and line_end.
            /// </summary>
            public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
            {
                Vector3 line_direction = line_end - line_start;
                float line_length = line_direction.magnitude;
                line_direction.Normalize();
                float dot_product = Vector3.Dot(point - line_start, line_direction);
                float project_length = Mathf.Clamp(dot_product, 0f, line_length);
                return line_start + line_direction * project_length;
            }

            /// <summary>
            /// (3D space) Returns point closest to infinite line defined by line_start and line_end.
            /// </summary>
            public static Vector3 GetClosestPointOnInfiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
            {
                return line_start + Vector3.Project(point - line_start, line_end - line_start);
            }

            /// <summary>
            /// (float Vector2) Orders vertices clockwise or counter clockwise.
            /// </summary>
            public static List<Vector2> OrderVertices(List<Vector2> vertices, bool clockwise = true)
            {
                // Calculate the centroid of the polygon
                float cx = 0, cy = 0;
                int n = vertices.Count;
                foreach (Vector2 vertex in vertices)
                {
                    cx += vertex.x;
                    cy += vertex.y;
                }
                cx /= n;
                cy /= n;

                // Create a list of angles between the centroid and the vertices
                List<float> angles = new List<float>();
                for (int i = 0; i < n; i++)
                {
                    float angle = Mathf.Atan2(vertices[i].y - cy, vertices[i].x - cx);
                    angles.Add(angle);
                }

                // Sort the vertices by angle
                List<Vector2> sortedVertices = new List<Vector2>();
                List<float> sortedAngles = new List<float>(angles);
                sortedAngles.Sort();
                foreach (float angle in sortedAngles)
                {
                    int i = angles.IndexOf(angle);
                    sortedVertices.Add(vertices[i]);
                }

                // Reverse the order of vertices if clockwise is false
                if (!clockwise)
                {
                    sortedVertices.Reverse();
                }

                return sortedVertices;
            }

            /// <summary>
            /// (int Vector2Int) Orders vertices clockwise or counter clockwise.
            /// </summary>
            public static List<Vector2Int> OrderVertices(List<Vector2Int> vertices, bool clockwise = true)
            {
                // Calculate the centroid of the polygon
                float cx = 0, cy = 0;
                int n = vertices.Count;
                foreach (Vector2 vertex in vertices)
                {
                    cx += vertex.x;
                    cy += vertex.y;
                }
                cx /= n;
                cy /= n;

                // Create a list of angles between the centroid and the vertices
                List<float> angles = new List<float>();
                for (int i = 0; i < n; i++)
                {
                    float angle = Mathf.Atan2(vertices[i].y - cy, vertices[i].x - cx);
                    angles.Add(angle);
                }

                // Sort the vertices by angle
                List<Vector2Int> sortedVertices = new List<Vector2Int>();
                List<float> sortedAngles = new List<float>(angles);
                sortedAngles.Sort();
                foreach (float angle in sortedAngles)
                {
                    int i = angles.IndexOf(angle);
                    sortedVertices.Add(vertices[i]);
                }

                // Reverse the order of vertices if clockwise is false
                if (!clockwise)
                {
                    sortedVertices.Reverse();
                }

                return sortedVertices;
            }

            /// <summary>
            /// Converts position from world space to coordinates on texture.
            /// </summary>
            public static Vector2Int GetPointOnTextureByWorldCoordinates(Vector3 position, Transform transform, Vector3[] corners, int resolutionX, int resolutionY)
            {
                Vector2Int result = Vector2Int.zero;
                Vector3 pos;

                pos = position;
                pos = GetClosestPointOnFiniteLine(pos, transform.TransformPoint(corners[0]), transform.TransformPoint(corners[1]));
                result.x = (int)(Vector3.Distance(pos, transform.TransformPoint(corners[0])) / Vector3.Distance(transform.TransformPoint(corners[1]), transform.TransformPoint(corners[0])) * resolutionX);

                pos = position;
                pos = GetClosestPointOnFiniteLine(pos, transform.TransformPoint(corners[0]), transform.TransformPoint(corners[2]));
                result.y = (int)(Vector3.Distance(pos, transform.TransformPoint(corners[0])) / Vector3.Distance(transform.TransformPoint(corners[2]), transform.TransformPoint(corners[0])) * resolutionY);

                return result;
            }

            // Metoda pomocnicza do okreœlania orientacji punktów
            private static int Orientation(Vector2Int p, Vector2Int q, Vector2Int r)
            {
                int val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

                if (val == 0)
                    return 0;  // punkty s¹ wspó³liniowe
                else if (val > 0)
                    return 1;  // zgodnie z ruchem wskazówek zegara (Clockwise)
                else
                    return 2;  // przeciwnie do ruchu wskazówek zegara (Counter-clockwise)
            }

            // static List<Vector2Int> hull = new List<Vector2Int>();
            static List<Vector2Int> points = new List<Vector2Int>();
            public static void ConvexHull(ref List<Vector2Int> hull)
            {
                points.Clear();
                foreach (var item in hull)
                {
                    points.Add(item);
                }

                int n = points.Count;
                if (n < 3)
                    return ;
                    // return null;  // przynajmniej 3 punkty s¹ wymagane do utworzenia otoczki wypuk³ej

                hull.Clear();
                // List<Vector2Int> hull = new List<Vector2Int>();

                // Sortowanie punktów leksykograficznie
                points.Sort((a, b) => {
                    if (a.x == b.x)
                        return a.y.CompareTo(b.y);
                    else
                        return a.x.CompareTo(b.x);
                });

                // Dolna otoczka
                for (int i = 0; i < n; i++)
                {
                    while (hull.Count >= 2 && Orientation(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) != 2)
                        hull.RemoveAt(hull.Count - 1);
                    hull.Add(points[i]);
                }

                // Górna otoczka
                int upperHullSize = hull.Count + 1;
                for (int i = n - 2; i >= 0; i--)
                {
                    while (hull.Count >= upperHullSize && Orientation(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) != 2)
                        hull.RemoveAt(hull.Count - 1);
                    hull.Add(points[i]);
                }

                // Usuniêcie powtarzaj¹cych siê punktów (pierwszy i ostatni punkt s¹ takie same)
                hull.RemoveAt(hull.Count - 1);

                // return hull;
            }

            /// <summary>
            /// Returns convex shape made of given vertices.
            /// </summary>
            public static List<Vector2Int> ConvexHullx(List<Vector2Int> points)
            {
                if (points.Count < 3) return points;

                // Find leftmost and rightmost points
                int leftmost = 0, rightmost = 0;
                for (int i = 1; i < points.Count; i++)
                {
                    if (points[i].x < points[leftmost].x)
                    {
                        leftmost = i;
                    }
                    if (points[i].x > points[rightmost].x)
                    {
                        rightmost = i;
                    }
                }

                // Find points on left and right side of line (leftmost, rightmost)
                List<Vector2Int> hull = new List<Vector2Int>();
                hull.Add(points[leftmost]);
                hull.Add(points[rightmost]);
                List<Vector2Int> leftPoints = new List<Vector2Int>();
                List<Vector2Int> rightPoints = new List<Vector2Int>();
                for (int i = 0; i < points.Count; i++)
                {
                    if (i == leftmost || i == rightmost) continue;
                    if (PointLocation(points[i], points[leftmost], points[rightmost]) < 0)
                    {
                        leftPoints.Add(points[i]);
                    }
                    else
                    {
                        rightPoints.Add(points[i]);
                    }
                }

                // Recursively find hulls of left and right points
                HullPart(hull, leftPoints, points[leftmost], points[rightmost]);
                HullPart(hull, rightPoints, points[rightmost], points[leftmost]);

                return hull;
            }

            /// <summary>
            /// Used to create convex shape made of given vertices.
            /// </summary>
            public static void HullPart(List<Vector2Int> hull, List<Vector2Int> points, Vector2Int a, Vector2Int b)
            {
                if (points.Count == 0) return;

                int insertIndex = hull.IndexOf(b);
                // int farthestIndex = -1;
                int farthestIndex = 0;
                float maxDistance = float.MinValue;
                for (int i = 0; i < points.Count; i++)
                {
                    float distance = PointLineDistance(points[i], a, b);
                    if (distance > maxDistance)
                    {
                        farthestIndex = i;
                        maxDistance = distance;
                    }
                }
                Vector2Int farthestPoint = points[farthestIndex];

                if (insertIndex == -1)
                {
                    insertIndex = hull.Count;
                    hull.Add(b);
                }

                hull.Insert(insertIndex, farthestPoint);

                List<Vector2Int> leftPoints = new List<Vector2Int>();
                List<Vector2Int> rightPoints = new List<Vector2Int>();
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i] == farthestPoint) continue;
                    if (PointLocation(points[i], a, farthestPoint) < 0)
                    {
                        leftPoints.Add(points[i]);
                    }
                    else if (PointLocation(points[i], farthestPoint, b) < 0)
                    {
                        rightPoints.Add(points[i]);
                    }
                }

                HullPart(hull, leftPoints, a, farthestPoint);
                HullPart(hull, rightPoints, farthestPoint, b);
            }

            /// <summary>
            /// Used to create convex shape made of given vertices.
            /// </summary>
            public static int PointLocation(Vector2Int p, Vector2Int a, Vector2Int b)
            {
                return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
            }

            /// <summary>
            /// Used to create convex shape made of given vertices.
            /// </summary>
            public static float PointLineDistance(Vector2Int p, Vector2Int a, Vector2Int b)
            {
                return Mathf.Abs(PointLocation(p, a, b)) / Distance(a, b);
            }

            /// <summary>
            /// Used to create convex shape made of given vertices.
            /// </summary>
            public static float Distance(Vector2Int a, Vector2Int b)
            {
                float dx = b.x - a.x;
                float dy = b.y - a.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            static Vector3[] colliderVertices = new Vector3[DrawingBoardSettings.MaxMeshColliderVertices];
            static List<int> verticesBehindPlane = new List<int>();

            /// <summary>
            /// Returns world coordinates array of MeshCollider vertices and board intersects.
            /// </summary>
            public static Vector3[] GetContactPoints(MeshVertexInfo[] meshVerticesInfo, Transform markerTransform, Transform transform, Vector3[] boardCorners)
            {
                List<Vector3> result = new List<Vector3>();

                // Vector3[] colliderVertices = GetMeshColliderVertices(meshVerticesInfo, markerTransform);

                int c = GetMeshColliderVertices(meshVerticesInfo, markerTransform, ref colliderVertices);

                Plane plane = new Plane(transform.TransformPoint(boardCorners[0]), transform.TransformPoint(boardCorners[1]), transform.TransformPoint(boardCorners[2]));

                verticesBehindPlane.Clear();

                //foreach (var vertex in colliderVertices)
                // for (int i = 0; i < colliderVertices.Length; i++)
                for (int i = 0; i < c; i++)
                {
                    if (plane.GetSide(colliderVertices[i]))
                    {
                        verticesBehindPlane.Add(i);
                    }
                }

                if (verticesBehindPlane.Count == 0)
                {
                    return new Vector3[0];
                }

                Ray ray = new Ray();
                float hitDistance = 0f;

                foreach (var item in verticesBehindPlane)
                {
                    ray.origin = colliderVertices[item];

                    foreach (var neighbour in meshVerticesInfo[item].neighbours)
                    {
                        if (verticesBehindPlane.Contains(neighbour))
                            continue;

                        Vector3 neighbourPos = markerTransform.TransformPoint(meshVerticesInfo[neighbour].position);
                        ray.direction = (neighbourPos - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (neighbourPos - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }
                    }
                }

                return result.ToArray();
            }

            /// <summary>
            /// Returns world coordinates array of MeshCollider vertices and board intersects.
            /// </summary>
            public static int GetContactPoints(MeshVertexInfo[] meshVerticesInfo, Transform markerTransform, Transform transform, Vector3[] boardCorners, ref Vector3[] result)
            {
                // colliderVertices = GetMeshColliderVertices(meshVerticesInfo, markerTransform);
                int c = GetMeshColliderVertices(meshVerticesInfo, markerTransform, ref colliderVertices);

                Plane plane = new Plane(transform.TransformPoint(boardCorners[0]), transform.TransformPoint(boardCorners[1]), transform.TransformPoint(boardCorners[2]));

                verticesBehindPlane.Clear();

                for (int j = 0; j < c; j++)
                {
                    if (plane.GetSide(colliderVertices[j]))
                    {
                        verticesBehindPlane.Add(j);
                    }
                }

                if (verticesBehindPlane.Count == 0)
                {
                    return 0;
                }

                Ray ray = new Ray();
                float hitDistance = 0f;

                int i = 0;
                Vector3 neighbourPos;
                foreach (var item in verticesBehindPlane)
                {
                    ray.origin = colliderVertices[item];

                    foreach (var neighbour in meshVerticesInfo[item].neighbours)
                    {
                        if (verticesBehindPlane.Contains(neighbour))
                            continue;

                        neighbourPos = markerTransform.TransformPoint(meshVerticesInfo[neighbour].position);
                        ray.direction = (neighbourPos - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (neighbourPos - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }
                    }
                }

                return i;
            }

            /// <summary>
            /// Returns world coordinates array of BoxCollider vertices and board intersects.
            /// </summary>
            public static Vector3[] GetContactPoints(BoxCollider collider, Transform transform, Vector3[] boardCorners)
            {
                List<Vector3> result = new List<Vector3>();

                Vector3[] colliderVertices = GetBoxColliderVertices(collider);

                Plane plane = new Plane(transform.TransformPoint(boardCorners[0]), transform.TransformPoint(boardCorners[1]), transform.TransformPoint(boardCorners[2]));

                List<int> verticesBehindPlane = new List<int>();

                //foreach (var vertex in colliderVertices)
                for (int i = 0; i < colliderVertices.Length; i++)
                {
                    if (plane.GetSide(colliderVertices[i]))
                    {
                        verticesBehindPlane.Add(i);
                    }
                }

                if (verticesBehindPlane.Count == 0)
                {
                    return new Vector3[0];
                }

                Ray ray = new Ray();
                float hitDistance = 0f;

                foreach (var item in verticesBehindPlane)
                {
                    bool bottom = item < 4;
                    int neighbourIndex;

                    ray.origin = colliderVertices[item];

                    if (bottom)
                    {
                        neighbourIndex = item + 1;

                        if (neighbourIndex > 3)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if(hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }

                        neighbourIndex = item + 3;

                        if (neighbourIndex > 3)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }

                        neighbourIndex = item + 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }
                    }
                    else
                    {
                        neighbourIndex = item + 1;

                        if (neighbourIndex > 7)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }

                        neighbourIndex = item + 3;

                        if (neighbourIndex > 7)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }

                        neighbourIndex = item - 4;

                        ray.direction = (colliderVertices[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (hitDistance <= (colliderVertices[neighbourIndex] - ray.origin).magnitude)
                                result.Add(ray.origin + ray.direction * hitDistance);
                        }
                    }
                }

                return result.ToArray();
            }

            /// <summary>
            /// Returns world coordinates array of BoxCollider vertices and board intersects.
            /// </summary>
            public static int GetContactPoints(BoxCollider collider, Transform transform, Vector3[] boardCorners, ref Vector3[] result)
            {
                Vector3[] colliderVerticesBox = GetBoxColliderVertices(collider);

                Plane plane = new Plane(transform.TransformPoint(boardCorners[0]), transform.TransformPoint(boardCorners[1]), transform.TransformPoint(boardCorners[2]));

                verticesBehindPlane.Clear();

                //foreach (var vertex in colliderVertices)
                for (int j = 0; j < colliderVerticesBox.Length; j++)
                {
                    if (plane.GetSide(colliderVerticesBox[j]))
                    {
                        verticesBehindPlane.Add(j);
                    }
                }

                if (verticesBehindPlane.Count == 0)
                {
                    return 0;
                }

                Ray ray = new Ray();
                float hitDistance = 0f;

                int i = 0;
                foreach (var item in verticesBehindPlane)
                {
                    bool bottom = item < 4;
                    int neighbourIndex;

                    ray.origin = colliderVerticesBox[item];

                    if (bottom)
                    {
                        neighbourIndex = item + 1;

                        if (neighbourIndex > 3)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }

                        neighbourIndex = item + 3;

                        if (neighbourIndex > 3)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }

                        neighbourIndex = item + 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }
                    }
                    else
                    {
                        neighbourIndex = item + 1;

                        if (neighbourIndex > 7)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }

                        neighbourIndex = item + 3;

                        if (neighbourIndex > 7)
                            neighbourIndex -= 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }

                        neighbourIndex = item - 4;

                        ray.direction = (colliderVerticesBox[neighbourIndex] - ray.origin).normalized;

                        if (plane.Raycast(ray, out hitDistance))
                        {
                            if (i >= DrawingBoardSettings.MaxContactPoints)
                                return DrawingBoardSettings.MaxContactPoints;

                            if (hitDistance <= (colliderVerticesBox[neighbourIndex] - ray.origin).magnitude)
                            {
                                result[i] = ray.origin + ray.direction * hitDistance;
                                i++;
                            }
                        }
                    }
                }

                return i;
            }

            /// <summary>
            /// Returns world coordinates array of MeshCollider vertices.
            /// </summary>
            public static Vector3[] GetMeshColliderVertices(MeshVertexInfo[] meshVerticesInfo, Transform transform)
            {
                Vector3[] vertices = new Vector3[meshVerticesInfo.Length];

                // Calculate the world position of each corner of the BoxCollider.
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = transform.TransformPoint(meshVerticesInfo[i].position);
                }

                return vertices;
            }

            /// <summary>
            /// Returns world coordinates array of MeshCollider vertices.
            /// </summary>
            public static int GetMeshColliderVertices(MeshVertexInfo[] meshVerticesInfo, Transform transform, ref Vector3[] vertices)
            {
                // Vector3[] vertices = new Vector3[meshVerticesInfo.Length];

                // Calculate the world position of each corner of the BoxCollider.
                int i;
                for (i = 0; i < meshVerticesInfo.Length; i++)
                {
                    vertices[i] = transform.TransformPoint(meshVerticesInfo[i].position);
                }

                return i;
            }

            /// <summary>
            /// Returns world coordinates array of BoxCollider vertices.
            /// </summary>
            public static Vector3[] GetBoxColliderVertices(BoxCollider collider)
            {
                Vector3[] vertices = new Vector3[8];

                // Get the world position of the BoxCollider's center.
                Vector3 center = collider.center;
                Vector3 size = collider.size;

                // Calculate the extents of the BoxCollider along each axis.
                vertices[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
                vertices[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;
                vertices[2] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;
                vertices[3] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;

                vertices[4] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;
                vertices[5] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;
                vertices[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;
                vertices[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;

                // Calculate the world position of each corner of the BoxCollider.
                for (int i = 0; i < 8; i++)
                {
                    vertices[i] = collider.transform.TransformPoint(vertices[i]);
                }

                return vertices;
            }

            /// <summary>
            /// Returns point on plane closest to point given plane points.
            /// </summary>
            public static Vector3 ProjectPointOnPlane(Vector3 planePoint1, Vector3 planePoint2, Vector3 planePoint3, Vector3 point)
            {
                Vector3 normal = Vector3.Cross((planePoint1 - planePoint2), (planePoint3 - planePoint2));
                return ProjectPointOnPlane(planePoint1, normal, point);
            }

            /// <summary>
            /// Returns point on plane closest to point given plane point and plane normal.
            /// </summary>
            public static Vector3 ProjectPointOnPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 point)
            {
                float distance;
                Vector3 translationVector;

                //First calculate the distance from the point to the plane:
                distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

                //Reverse the sign of the distance
                distance *= -1;

                //Get a translation vector
                translationVector = SetVectorLength(planeNormal, distance);

                //Translate the point to form a projection
                return point + translationVector;
            }

            /// <summary>
            /// Returns signed distance from point to plane given plane point and plane normal.
            /// </summary>
            public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
            {
                Plane plane = new Plane(planeNormal, planePoint);
                return plane.GetDistanceToPoint(point);
                // return Vector3.Dot(planeNormal, (point - planePoint));
            }

            /// <summary>
            /// Sets length of vector.
            /// </summary>
            public static Vector3 SetVectorLength(Vector3 vector, float size)
            {

                //normalize the vector
                Vector3 vectorNormalized = Vector3.Normalize(vector);

                //scale the vector
                return vectorNormalized *= size;
            }

            /// <summary>
            /// Converts MeshCollider to eshVertexInfo.
            /// </summary>
            public static MeshVertexInfo[] GetMeshVertexInfo(MeshCollider meshCollider)
            {
                MeshVertexInfo[] result = new MeshVertexInfo[meshCollider.sharedMesh.vertexCount];

                int[] triangles = meshCollider.sharedMesh.triangles;

                List<int> indexes = new List<int>(3);
                indexes.Add(0);
                indexes.Add(0);
                indexes.Add(0);

                for (int i = 0; i < result.Length; i++)
                {
                    MeshVertexInfo meshVertexInfo = new MeshVertexInfo();
                    meshVertexInfo.index = i;
                    meshVertexInfo.position = meshCollider.sharedMesh.vertices[i];

                    List<int> n = new List<int>();

                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        indexes[0] = triangles[j];
                        indexes[1] = triangles[j+1];
                        indexes[2] = triangles[j+2];

                        if (indexes.Contains(i))
                        {
                            foreach (var item in indexes)
                            {
                                if (item != i && !n.Contains(item))
                                {
                                    n.Add(item);
                                }
                            }
                        }
                    }

                    meshVertexInfo.neighbours = new List<int>(n);

                    result[i] = meshVertexInfo;
                }                

                return result;
            }

            public static MeshVertexInfo[] SimplifyMeshVertexInfo(MeshVertexInfo[] meshVertexInfo)
            {
                List<MeshVertexInfo> result = new List<MeshVertexInfo>();
                List<Change> changes = new List<Change>();

                int j = 0;
                for (int i = 0; i < meshVertexInfo.Length; i++)
                {
                    Change change = new Change();
                    change.pos = meshVertexInfo[i].position;
                    change.from = meshVertexInfo[i].index;
                    change.neighbours = new List<Vector3>();

                    change.to = j;
                    change.inUse = true;

                    foreach (var item in meshVertexInfo[i].neighbours)
                    {
                        change.neighbours.Add(meshVertexInfo[item].position);
                    }

                    if (changes.Any(Change => Change.pos == meshVertexInfo[i].position))
                    {
                        change.to = changes.Find(Change => Change.pos == meshVertexInfo[i].position).to;
                        //foreach (var item in meshVertexInfo[i].neighbours)
                        //{
                        //    changes.Find(Change => Change.pos == meshVertexInfo[i].position).neighbours.Add(meshVertexInfo[item].position);
                        //}

                        change.inUse = false;
                    }
                    else
                    {
                        j++;
                        //foreach (var item in meshVertexInfo[i].neighbours)
                        //{
                        //    change.neighbours.Add(meshVertexInfo[item].position);
                        //}
                    }

                    changes.Add(change);
                }

                for (int i = 0; i < changes.Count; i++)
                {
                    if (changes[i].inUse)
                    {
                        MeshVertexInfo mvi = new MeshVertexInfo();
                        mvi.index = changes[i].to;
                        mvi.position = changes[i].pos;
                        mvi.neighbours = new List<int>();

                        foreach (var item in changes.FindAll(Change => Change.pos == changes[i].pos))
                        {
                            // mvi.neighbours.Add(item.to);
                            foreach (var item2 in item.neighbours)
                            {
                                int n = changes.Find(Change => Change.pos == item2).to;
                                if (!mvi.neighbours.Contains(n)) mvi.neighbours.Add(n);
                            }
                        }

                        result.Add(mvi);
                    }
                }

                //j = 0;
                //foreach (var item in result)
                //{
                //    Debug.Log($"===={j}====");

                //    Debug.Log($"Index: {item.index}");
                //    Debug.Log($"Position: {item.position}");

                //    string nei = "";

                //    foreach (var item2 in item.neighbours)
                //    {
                //        nei += item2 + " ";
                //    }

                //    Debug.Log($"Neighbours {nei}");

                //    Debug.Log($"====+====");

                //    j++;
                //}

                return result.ToArray();
            }

            [System.Serializable]
            public struct MeshVertexInfo
            {
                public int index;
                public Vector3 position;
                public List<int> neighbours;
            }

            public struct Change
            {
                public bool inUse;
                public int from;
                public int to;
                public Vector3 pos;
                public List<Vector3> neighbours;
            }
        }
    }
}