﻿using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Landscape.FoliagePipeline
{
    [Serializable]
    public struct FMeshLODInfo : IEquatable<FMeshLODInfo>
    {
        public float screenSize;
        public int[] materialSlot;

        public bool Equals(FMeshLODInfo Target)
        {
            return screenSize.Equals(Target.screenSize) && materialSlot.Equals(Target.materialSlot);
        }

        public override bool Equals(object obj)
        {
            return Equals((FMeshLODInfo)obj);
        }

        public override int GetHashCode()
        {
            int hashCode = screenSize.GetHashCode();
            hashCode += materialSlot.GetHashCode();

            return hashCode;
        }
    }

    [Serializable]
    public struct FMesh : IEquatable<FMesh>
    {
        public bool IsCreated;

        public Mesh[] meshes;

        public Material[] materials;

        public FMeshLODInfo[] lODInfo;
        

        public FMesh(Mesh[] meshes, Material[] materials, FMeshLODInfo[] lODInfo)
        {
            this.IsCreated = true;
            this.meshes = meshes;
            this.materials = materials;
            this.lODInfo = lODInfo;
        }

        public bool Equals(FMesh Target)
        {
            return IsCreated.Equals(Target.IsCreated) && meshes.Equals(Target.meshes) && lODInfo.Equals(Target.lODInfo) && materials.Equals(Target.materials);
        }

        public override bool Equals(object obj)
        {
            return Equals((FMesh)obj);
        }

        public override int GetHashCode()
        {
            int hashCode = IsCreated ? 0 : 1;
            hashCode += meshes.GetHashCode();
            hashCode += lODInfo.GetHashCode();
            hashCode += materials.GetHashCode();

            return hashCode;
        }
    }

    [CreateAssetMenu(menuName = "Landscape/MeshAsset", order = 256)]
    public class MeshAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [Header("Target")]
        [HideInInspector]
        public GameObject target;
#endif

        [Header("Mesh")]
        public Mesh[] meshes;

        [Header("Material")]
        public Material[] materials;

        [Header("Culling")]
        public FMeshLODInfo[] lODInfo;

        /*[HideInInspector]
        public FMesh Tree;*/


        public MeshAsset()
        {

        }

        void Awake()
        {
            //Debug.Log("Awake");
        }

        void Reset()
        {
            //Debug.Log("Reset");
        }

        void OnEnable()
        {
            //Debug.Log("OnEnable");
        }

        void OnValidate()
        {
            //Debug.Log("OnValidate");
        }

        void OnDisable()
        {
            //Debug.Log("OnDisable");
        }

        void OnDestroy()
        {
            //Debug.Log("OnDestroy");
        }

        public void BuildMeshAsset(Mesh[] meshes, Material[] materials, FMeshLODInfo[] lODInfo)
        {
            this.meshes = meshes;
            this.materials = materials;
            this.lODInfo = lODInfo;
        }

        public static void BuildMeshAsset(GameObject cloneTarget, MeshAsset meshAsset)
        {
            if (cloneTarget == null)
            {
                Debug.LogWarning("source prefab is null");
                return;
            }

            List<Mesh> meshes = new List<Mesh>();
            List<Material> materials = new List<Material>();
            LOD[] lods = cloneTarget.GetComponent<LODGroup>().GetLODs();

            //Collector Meshes&Materials
            for (int j = 0; j < lods.Length; ++j)
            {
                ref LOD lod = ref lods[j];
                Renderer renderer = lod.renderers[0];
                MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();

                meshes.AddUnique(meshFilter.sharedMesh);
                for (int k = 0; k < renderer.sharedMaterials.Length; ++k)
                {
                    materials.AddUnique(renderer.sharedMaterials[k]);
                }
            }

            //Build LODInfo
            FMeshLODInfo[] lODInfos = new FMeshLODInfo[lods.Length];
            for (int l = 0; l < lods.Length; ++l)
            {
                ref LOD lod = ref lods[l];
                ref FMeshLODInfo lODInfo = ref lODInfos[l];
                Renderer renderer = lod.renderers[0];

                lODInfo.screenSize = 1 - (l * 0.125f);
                lODInfo.materialSlot = new int[renderer.sharedMaterials.Length];

                for (int m = 0; m < renderer.sharedMaterials.Length; ++m)
                {
                    ref int MaterialSlot = ref lODInfo.materialSlot[m];
                    MaterialSlot = materials.IndexOf(renderer.sharedMaterials[m]);
                }
            }

            meshAsset.BuildMeshAsset(meshes.ToArray(), materials.ToArray(), lODInfos);
            EditorUtility.SetDirty(meshAsset);
        }
    }
}