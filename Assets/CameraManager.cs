using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public List<Camera> cameras; 
    private List<GameObject> planes;
    private List<RenderTexture> textures;
    public GameObject planePrefab;
    public ComputeShader DebugShader;
    public Camera activeCam;
    public TestDraw draw;
    public TreeMesh tmpConnectionMesh;
    private float dist2Cam = 1;

    private void Awake()
    {
        activeCam = Camera.main;
        planes = new List<GameObject>();
        textures = new List<RenderTexture>();
        for (int i = 0; i < cameras.Count; i++)
        {
            Camera cam = cameras[i];
            cam.depthTextureMode = DepthTextureMode.Depth;
            GameObject plane = cam.transform.GetChild(0).gameObject;
            planes.Add(plane);
            plane.transform.rotation = cam.transform.rotation;
            plane.transform.Rotate(cam.transform.right, -90, Space.World);
            plane.transform.position = cam.transform.position + cam.transform.forward * dist2Cam;
            float planeHeight = 2.0f * Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * dist2Cam;
            planeHeight /= 10;
            float planeWidth = planeHeight * cam.aspect;
            plane.transform.localScale = new Vector3(planeWidth, plane.transform.localScale.y, planeHeight);

            plane.transform.parent = cam.transform;

            int screenWidth = (int)(Screen.width * cam.rect.width);
            int screenHeight = (int)(Screen.height * cam.rect.height);
            RenderTexture planeTexture = new RenderTexture(screenWidth, screenHeight, 4);
            textures.Add(planeTexture);
            planeTexture.enableRandomWrite = true;
            plane.GetComponent<Renderer>().material.mainTexture = planeTexture;
            // DebugShader.SetTexture(0, "Result", planeTexture);
            // DebugShader.Dispatch(0, screenWidth / 8, screenHeight / 8, 1);
        }
        draw.RegisterCameras(textures, cameras);
    }

    private void FixedUpdate()
    {
        Vector2 mousePos = Input.mousePosition;
        float respectiveMousePosX = mousePos.x / Screen.width;
        float respectiveMousePosY = mousePos.y / Screen.height;
        Vector2 respectiveMousePos = new Vector2(respectiveMousePosX, respectiveMousePosY);


        for (int i = 0; i < cameras.Count; i++)
        {
            Camera cam = cameras[i];
            cam.GetComponent<CameraBase>().enabled = false;
            Rect camRect = cam.rect;
            Vector2 minCam = camRect.position;
            Vector2 maxCam = minCam + camRect.size;

            if(respectiveMousePos.x >= minCam.x && respectiveMousePos.x <= maxCam.x &&
               respectiveMousePos.y >= minCam.y && respectiveMousePos.y <= maxCam.y)
            {
                activeCam = cam;
                draw.SetActiveCamera(cam, textures[i]);
                tmpConnectionMesh.ChangeCam(cam);
                cam.GetComponent<CameraBase>().enabled = true;
            }
        }
    }
}
