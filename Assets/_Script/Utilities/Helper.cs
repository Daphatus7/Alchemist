using System;
using UnityEditor;
using UnityEngine;

namespace _Script.Utilities
{
    public static class Helper
    {
        public static bool IsFaceLeft(float angle)
        {
            if(angle < 0)
            {
                angle += 360;
            }
            return angle is (>= 90 or <= 0) and (<= 270 or >= 360);
        }
        
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, Color color = default, TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int sortingOrder = 5000)
        {
            if(color == default)
            {
                color = Color.white;
            }
            return CreateWorldText(parent, text, localPosition, fontSize, color, textAnchor, textAlignment, sortingOrder);
        }

        private static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.characterSize = 0.1f;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }
        
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0;
            return vec;
        }
        
        public static Vector3 GetMouseWorldPositionInEditor()
        {
#if UNITY_EDITOR
            if (SceneView.lastActiveSceneView != null)
            {
                Event e = Event.current;
                if (e != null && e.isMouse)
                {
                    // 获取鼠标在 Scene 视图中的位置
                    Vector2 mousePosition = e.mousePosition;

                    // 使用 HandleUtility 将 GUI 坐标转换为世界射线
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);

                    // 定义一个在 Z = 0 的平面（即 2D 游戏的 XY 平面）
                    Plane plane = new Plane(Vector3.forward, Vector3.zero);

                    if (plane.Raycast(worldRay, out float distance))
                    {
                        // 获取射线与平面的交点
                        Vector3 worldPosition = worldRay.GetPoint(distance);
                        worldPosition.z = 0; // 确保 Z 轴为 0
                        return worldPosition;
                    }
                }
            }
#endif
            return Vector3.zero;
        }        
        
        
        public static Mesh AddToMesh(Mesh mesh, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) {
            if (mesh == null) {
                mesh = CreateEmptyMesh();
            }
            Vector3[] vertices = new Vector3[4 + mesh.vertices.Length];
            Vector2[] uvs = new Vector2[4 + mesh.uv.Length];
            int[] triangles = new int[6 + mesh.triangles.Length];
            
            mesh.vertices.CopyTo(vertices, 0);
            mesh.uv.CopyTo(uvs, 0);
            mesh.triangles.CopyTo(triangles, 0);

            int index = vertices.Length / 4 - 1;
            //Relocate vertices
            int vIndex = index*4;
            int vIndex0 = vIndex;
            int vIndex1 = vIndex+1;
            int vIndex2 = vIndex+2;
            int vIndex3 = vIndex+3;

            baseSize *= .5f;

            bool skewed = baseSize.x != baseSize.y;
            if (skewed) {
                vertices[vIndex0] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x,  baseSize.y);
                vertices[vIndex1] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x, -baseSize.y);
                vertices[vIndex2] = pos+GetQuaternionEuler(rot)*new Vector3( baseSize.x, -baseSize.y);
                vertices[vIndex3] = pos+GetQuaternionEuler(rot)*baseSize;
            } else {
                vertices[vIndex0] = pos+GetQuaternionEuler(rot-270)*baseSize;
                vertices[vIndex1] = pos+GetQuaternionEuler(rot-180)*baseSize;
                vertices[vIndex2] = pos+GetQuaternionEuler(rot- 90)*baseSize;
                vertices[vIndex3] = pos+GetQuaternionEuler(rot-  0)*baseSize;
            }
		
            //Relocate UVs
            uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
            uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
            uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
            uvs[vIndex3] = new Vector2(uv11.x, uv11.y);
		
            //Create triangles
            int tIndex = index*6;
		
            triangles[tIndex+0] = vIndex0;
            triangles[tIndex+1] = vIndex3;
            triangles[tIndex+2] = vIndex1;
		
            triangles[tIndex+3] = vIndex1;
            triangles[tIndex+4] = vIndex3;
            triangles[tIndex+5] = vIndex2;
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            //mesh.bounds = bounds;

            return mesh;
        }
        
        private static Quaternion GetQuaternionEuler(float rotFloat) {
            int rot = Mathf.RoundToInt(rotFloat);
            rot = rot % 360;
            if (rot < 0) rot += 360;
            //if (rot >= 360) rot -= 360;
            if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
            return cachedQuaternionEulerArr[rot];
        }
        
        private static readonly Vector3 Vector3zero = Vector3.zero;
        private static readonly Vector3 Vector3one = Vector3.one;
        private static readonly Vector3 Vector3yDown = new Vector3(0,-1);

    
        private static Quaternion[] cachedQuaternionEulerArr;
        private static void CacheQuaternionEuler() {
            if (cachedQuaternionEulerArr != null) return;
            cachedQuaternionEulerArr = new Quaternion[360];
            for (int i=0; i<360; i++) {
                cachedQuaternionEulerArr[i] = Quaternion.Euler(0,0,i);
            }
        }
        
        public static void AddMeshToMeshArrays(Vector3[] vertices, Vector2[] uv, int[] triangles, int index, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
        {
            int triangleIndex = index * 6;
            vertices[index * 4] = bottomLeft;
            vertices[index * 4 + 1] = topLeft;
            vertices[index * 4 + 2] = topRight;
            vertices[index * 4 + 3] = bottomRight;
            uv[index * 4] = new Vector2(0, 0);
            uv[index * 4 + 1] = new Vector2(0, 1);
            uv[index * 4 + 2] = new Vector2(1, 1);
            uv[index * 4 + 3] = new Vector2(1, 0);
            triangles[triangleIndex] = index * 4;
            triangles[triangleIndex + 1] = index * 4 + 1;
            triangles[triangleIndex + 2] = index * 4 + 2;
            triangles[triangleIndex + 3] = index * 4;
            triangles[triangleIndex + 4] = index * 4 + 2;
            triangles[triangleIndex + 5] = index * 4 + 3;
        }
        
        
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            return worldCamera.ScreenToWorldPoint(screenPosition);
        }
        
        public static Mesh CreateEmptyMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = Array.Empty<Vector3>(),
                uv = Array.Empty<Vector2>(),
                triangles = Array.Empty<int>()
            };
            return mesh;
        }
        
        public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uv, out int[] triangles)
        {
            vertices = new Vector3[quadCount * 4];
            uv = new Vector2[quadCount * 4];
            triangles = new int[quadCount * 6];
        }
        
        public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) {
            //Relocate vertices
            int vIndex = index*4;
            int vIndex0 = vIndex;
            int vIndex1 = vIndex+1;
            int vIndex2 = vIndex+2;
            int vIndex3 = vIndex+3;

            baseSize *= .5f;

            bool skewed = baseSize.x != baseSize.y;
            if (skewed) {
                vertices[vIndex0] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x,  baseSize.y);
                vertices[vIndex1] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x, -baseSize.y);
                vertices[vIndex2] = pos+GetQuaternionEuler(rot)*new Vector3( baseSize.x, -baseSize.y);
                vertices[vIndex3] = pos+GetQuaternionEuler(rot)*baseSize;
            } else {
                vertices[vIndex0] = pos+GetQuaternionEuler(rot-270)*baseSize;
                vertices[vIndex1] = pos+GetQuaternionEuler(rot-180)*baseSize;
                vertices[vIndex2] = pos+GetQuaternionEuler(rot- 90)*baseSize;
                vertices[vIndex3] = pos+GetQuaternionEuler(rot-  0)*baseSize;
            }
		
            //Relocate UVs
            uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
            uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
            uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
            uvs[vIndex3] = new Vector2(uv11.x, uv11.y);
		
            //Create triangles
            int tIndex = index*6;
		
            triangles[tIndex+0] = vIndex0;
            triangles[tIndex+1] = vIndex3;
            triangles[tIndex+2] = vIndex1;
		
            triangles[tIndex+3] = vIndex1;
            triangles[tIndex+4] = vIndex3;
            triangles[tIndex+5] = vIndex2;
        }

    }
    
    
}