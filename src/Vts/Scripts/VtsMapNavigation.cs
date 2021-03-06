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

using UnityEngine;
using vts;

[RequireComponent(typeof(VtsMap))]
public class VtsMapNavigation : MonoBehaviour
{
    void Update ()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        Map map = GetComponent<VtsMap>().map;
        if (Input.GetMouseButton(0))
        {
            double[] pan = new double[3];
            pan[0] = Input.GetAxis("Mouse X") * mousePanSpeed;
            pan[1] = -Input.GetAxis("Mouse Y") * mousePanSpeed;
            map.Pan(pan);
            map.SetOptions("{\"navigationType\":1}"); // quick navigation mode
        }
        if (Input.GetMouseButton(1))
        {
            double[] rot = new double[3];
            rot[0] = Input.GetAxis("Mouse X") * mouseRotateSpeed;
            rot[1] = -Input.GetAxis("Mouse Y") * mouseRotateSpeed;
            map.Rotate(rot);
            map.SetOptions("{\"navigationType\":1}"); // quick navigation mode
        }
        {
            double zoom = Input.GetAxis("Mouse ScrollWheel") * mouseZoomSpeed;
            map.Zoom(zoom);
        }
    }

    public double mousePanSpeed = 30;
    public double mouseRotateSpeed = 30;
    public double mouseZoomSpeed = 10;
}
