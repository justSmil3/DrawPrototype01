using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public ComputeBuffer numbers;
    public ComputeShader shader;

    private void Awake()
    {
        numbers = new ComputeBuffer(10, sizeof(float));

        shader.SetBuffer(0, "Result", numbers);

        shader.Dispatch(0, 10, 1, 1);
        float[] testNum = new float[10];
        numbers.GetData(testNum);
        for(int i = 0; i < 10; i++)
        {
            Debug.Log(testNum[i]);
        }
    }
}
