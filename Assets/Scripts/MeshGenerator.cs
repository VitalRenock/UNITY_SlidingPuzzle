using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    static public Mesh CreateQuad(Vector2 extends, Vector2 uvXMinMax, Vector2 uvYMinMax)
    {
        // Création d'un mesh (sans informations)
        Mesh meshToReturn = new Mesh();

        // Création d'un tableau de Vector3 des futurs vertexs du mesh
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-extends.x, -extends.y, 0f);
        vertices[1] = new Vector3(-extends.x, extends.y, 0f);
        vertices[2] = new Vector3(extends.x, extends.y, 0f);
        vertices[3] = new Vector3(extends.x, -extends.y, 0f);

        Vector3[] normals = new Vector3[4];
        normals[0] = Vector3.back;
        normals[1] = Vector3.back;
        normals[2] = Vector3.back;
        normals[3] = Vector3.back;

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(uvXMinMax.x, uvYMinMax.x);
        uvs[1] = new Vector2(uvXMinMax.x, uvYMinMax.y);
        uvs[2] = new Vector2(uvXMinMax.y, uvYMinMax.y);
        uvs[3] = new Vector2(uvXMinMax.y, uvYMinMax.x);

        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        meshToReturn.vertices = vertices;
        meshToReturn.normals = normals;
        meshToReturn.uv = uvs;
        meshToReturn.triangles = triangles;

        return meshToReturn;
    }
}
