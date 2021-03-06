/**
 * Copyright (c) 2017 Melown Technologies SE
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * *  Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * *  Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using UnityEngine;
using vts;

public class VtsColliderProbe : MonoBehaviour
{
    public VtsColliderProbe()
    {
        CollOverrideCenterDel = CollOverrideCenter;
        CollOverrideDistanceDel = CollOverrideDistance;
        CollOverrideLodDel = CollOverrideLod;
    }

    private void Start()
    {
        probTrans = GetComponent<Transform>();
        mapTrans = mapObject.GetComponent<Transform>();
    }

    private readonly Map.DoubleArrayHandler CollOverrideCenterDel;
    private void CollOverrideCenter(ref double[] values)
    {
        values = VtsUtil.U2V3(probTrans.position);
        { // convert from unity world to (local) vts physical
            double[] point4 = new double[4] { values[0], values[1], values[2], 1 };
            point4 = Math.Mul44x4(VtsUtil.U2V44(mapTrans.worldToLocalMatrix), point4);
            values[0] = point4[0]; values[1] = point4[1]; values[2] = point4[2];
        }
        { // swap YZ
            double tmp = values[1];
            values[1] = values[2];
            values[2] = tmp;
        }
    }

    private readonly Map.DoubleHandler CollOverrideDistanceDel;
    private void CollOverrideDistance(ref double value)
    {
        value = collidersDistance;
    }

    private readonly Map.Uint32Handler CollOverrideLodDel;
    private void CollOverrideLod(ref uint value)
    {
        value = collidersLod;
    }

    private void Update()
    {
        Map map = mapObject.GetComponent<VtsMap>().map;
        map.EventCollidersCenter += CollOverrideCenterDel;
        map.EventCollidersDistance += CollOverrideDistanceDel;
        map.EventCollidersLod += CollOverrideLodDel;
        map.RenderTickColliders();
        map.EventCollidersCenter -= CollOverrideCenterDel;
        map.EventCollidersDistance -= CollOverrideDistanceDel;
        map.EventCollidersLod -= CollOverrideLodDel;
        draws.Load(map);
        UpdateParts();
    }

    private void UpdateParts()
    {
        double[] conv = Math.Mul44x44(Math.Mul44x44(VtsUtil.U2V44(mapTrans.localToWorldMatrix), VtsUtil.U2V44(VtsUtil.SwapYZ)), Math.Inverse44(draws.camera.view));

        Dictionary<VtsMesh, DrawTask> tasksByMesh = new Dictionary<VtsMesh, DrawTask>();
        foreach (DrawTask t in draws.colliders)
        {
            VtsMesh k = t.mesh as VtsMesh;
            if (!tasksByMesh.ContainsKey(k))
                tasksByMesh.Add(k, t);
        }

        HashSet<VtsMesh> partsToRemove = new HashSet<VtsMesh>(partsCache.Keys);

        foreach (KeyValuePair<VtsMesh, DrawTask> tbm in tasksByMesh)
        {
            if (!partsCache.ContainsKey(tbm.Key))
            {
                GameObject o = Instantiate(colliderPrefab);
                partsCache.Add(tbm.Key, o);
                UnityEngine.Mesh msh = (tbm.Value.mesh as VtsMesh).Get();
                o.GetComponent<MeshCollider>().sharedMesh = msh;
                VtsUtil.Matrix2Transform(o.transform, VtsUtil.V2U44(Math.Mul44x44(conv, System.Array.ConvertAll(tbm.Value.data.mv, System.Convert.ToDouble))));
            }
            partsToRemove.Remove(tbm.Key);
        }

        foreach (VtsMesh m in partsToRemove)
        {
            Destroy(partsCache[m]);
            partsCache.Remove(m);
        }
    }

    public GameObject mapObject;
    public GameObject colliderPrefab;

    public double collidersDistance = 200;
    public uint collidersLod = 18;

    private readonly Draws draws = new Draws();
    private readonly Dictionary<VtsMesh, GameObject> partsCache = new Dictionary<VtsMesh, GameObject>();

    private Transform probTrans;
    private Transform mapTrans;
}

