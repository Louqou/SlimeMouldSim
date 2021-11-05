using UnityEngine;
using System;
using UnityEngine.Experimental.Rendering;

public class Slime : MonoBehaviour
{
    private struct Agent
    {
        public Vector2 position;
        public float angle;
    }

    [SerializeField]
    private ComputeShader slimeShader = null;

    [SerializeField]
    private int width = 800;
    [SerializeField]
    private int height = 800;
    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    private int numAgents = 100;
    [SerializeField]
    private float evaporateSpeed = 0.1f;
    [SerializeField]
    private float diffuseSpeed = 10f;
    [SerializeField]
    private float sensorAngleSpacing = 10f;
    [SerializeField]
    private float turnSpeed = 10f;
    [SerializeField]
    private float sensorOffsetDst = 1;
    [SerializeField]
    private int sensorSize = 1;
    [SerializeField]
    private bool run = true;
    private Agent[] agents;
    private ComputeBuffer agentBuffer;

    private RenderTexture trailTexture;
    private RenderTexture diffuseTexture;

    private System.Random random = new System.Random();
    private void Start()
    {
        trailTexture = CreateRenderTexture();
        diffuseTexture = CreateRenderTexture();

        slimeShader.SetInt("width", width);
        slimeShader.SetInt("height", height);
        slimeShader.SetInt("numAgents", numAgents);
        slimeShader.SetTexture(0, "TrailMap", trailTexture);
        slimeShader.SetTexture(1, "TrailMap", trailTexture);
        //slimeShader.SetTexture(1, "DiffusedTrailMap", diffuseTexture);

        InitAgents();
        agentBuffer = new ComputeBuffer(agents.Length, sizeof(float) * 3);
        agentBuffer.SetData(agents);
        slimeShader.SetBuffer(0, "agents", agentBuffer);
    }

    private RenderTexture CreateRenderTexture()
    {
        RenderTexture renderTexture = new RenderTexture(width, height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.autoGenerateMips = false;
        renderTexture.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.Create();
        return renderTexture;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        slimeShader.SetFloat("moveSpeed", moveSpeed);
        slimeShader.SetFloat("evaporateSpeed", evaporateSpeed);
        slimeShader.SetFloat("deltaTime", run ? Time.deltaTime : 0);
        slimeShader.SetFloat("diffuseSpeed", diffuseSpeed);
        slimeShader.SetFloat("sensorAngleSpacing", Mathf.Deg2Rad * sensorAngleSpacing);
        slimeShader.SetFloat("turnSpeed", turnSpeed);
        slimeShader.SetFloat("sensorOffsetDst", sensorOffsetDst);
        slimeShader.SetInt("sensorSize", sensorSize);
        slimeShader.SetFloat("time", Time.time);

        slimeShader.Dispatch(0, numAgents / 64, 1, 1);
        slimeShader.Dispatch(1, width / 8, height / 8, 1);
        // Graphics.Blit(diffuseTexture, trailTexture);
        Graphics.Blit(trailTexture, dest);
    }

    private void InitAgents()
    {
        agents = new Agent[numAgents];
        //Vector2[] circlePoints = fibonacci_spiral_disc(numAgents, 0.03f);
        for (int i = 0; i < agents.Length; i++)
        {
            // agents[i].position.x = width / 2f;
            // agents[i].position.y = height / 2f;
            Vector2 offset = new Vector2(width / 2f, height / 2f);
            Vector2 randomInCircle = RandomInUnitCircle();
            agents[i].position = randomInCircle * (height / 3f) + offset;
            agents[i].angle = Mathf.Atan2(randomInCircle.y, randomInCircle.x) + Mathf.PI;//2 * Mathf.PI * (float)random.NextDouble();
        }
        Debug.Log(agents.Length);
    }

    private void OnDisable()
    {
        agentBuffer.Dispose();
    }

    private Vector2 RandomInUnitCircle()
    {
        float radius = Mathf.Sqrt((float)random.NextDouble());
        float theta = (float)random.NextDouble() * 2 * Mathf.PI;
        return new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
    }

    Vector2[] fibonacci_spiral_disc(int num_points, float k)
    {
        Vector2[] vectors = new Vector2[num_points];

        float gr = (Mathf.Sqrt(5.0f) + 1.0f) / 2.0f;  // golden ratio = 1.6180339887498948482
        float ga = (2.0f - gr) * (2.0f * Mathf.PI);  // golden angle = 2.39996322972865332

        for (int i = 1; i <= num_points; ++i)
        {
            float r = Mathf.Sqrt(i) * k;
            float theta = ga * i;

            float x = Mathf.Cos(theta) * r;
            float y = Mathf.Sin(theta) * r;

            vectors[i - 1] = new Vector2(x, y);
        }

        return vectors;
    }
}
