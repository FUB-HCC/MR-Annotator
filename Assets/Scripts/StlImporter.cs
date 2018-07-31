using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class StlImporter {

    public Mesh ImportFile(byte[] txt)
    {
        if(txt.ToString().Substring(0,5) == "solid")
        {
            return loadASCII(txt.ToString());
        }
        else
        {
            return loadBinary(txt);
        }
    }

    private Mesh loadASCII(string txt)
    {
        return null;
    }

    private Mesh loadBinary(byte[] txt)
    {
        uint nTriangles = 0;
        byte[] stl = txt;
        BinaryReader reader = new BinaryReader(new MemoryStream(stl));
        reader.ReadBytes(80);
        nTriangles = reader.ReadUInt32();

        Vector3[] verts = new Vector3[nTriangles * 3];
        Vector3[] norms = new Vector3[nTriangles * 3];
        int[] tris = new int[nTriangles*3];

        Mesh mesh = new Mesh();
        int i = 0;
        for(i=0;i<nTriangles;i++)
        {
            Vector3 v1 = new Vector3();
            Vector3 v2 = new Vector3();
            Vector3 v3 = new Vector3();
            Vector3 normal = new Vector3();

            normal.x = reader.ReadSingle();
            normal.y = reader.ReadSingle();
            normal.z = reader.ReadSingle();
            norms[i * 3 + 0] = normal;
            norms[i * 3 + 1] = normal;
            norms[i * 3 + 2] = normal;

            v1.x = reader.ReadSingle();
            v1.y = reader.ReadSingle();
            v1.z = reader.ReadSingle();
            verts[i * 3 + 0] = v1;

            v2.x = reader.ReadSingle();
            v2.y = reader.ReadSingle();
            v2.z = reader.ReadSingle();
            verts[i * 3 + 1] = v2;

            v3.x = reader.ReadSingle();
            v3.y = reader.ReadSingle();
            v3.z = reader.ReadSingle();
            verts[i * 3 + 2] = v3;

            tris[i * 3 + 0] = i * 3 + 0;
            tris[i * 3 + 1] = i * 3 + 1;
            tris[i * 3 + 2] = i * 3 + 2;
            reader.ReadUInt16();
        }

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        i = 0;
        foreach(Vector3 v in verts)
        {
            verts[i] = verts[i] - mesh.bounds.center;
            i++;
        }

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        return mesh;
    }
}
