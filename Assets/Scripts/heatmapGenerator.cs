using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour
{
    // ===========================
    // Heatmap Configuration
    // ===========================
    public int gridSize = 10;
    public Vector3 gridBounds = new Vector3(50, 10, 50); // Adjusted for 3D bounds
    public Transform[] trackedObjects; // List of objects contributing to the heatmap
    public float[] objectHeatContributions; // Define each object's heat contribution (optional)

    private NativeArray<float> heatmapData; // Holds the heatmap values
    private NativeArray<Vector3> positions; // Positions of tracked objects
    private NativeArray<float> contributions; // Heat contributions as NativeArray
    private JobHandle jobHandle; // Job handle for scheduling
    private float maxHeat; // The maximum heat value used for normalization
    private float heatDecayRate = 0.05f; // Decay rate for heat over time

    // ===========================
    // Initialization
    // ===========================
    void Start()
    {
        heatmapData = new NativeArray<float>(gridSize * gridSize, Allocator.Persistent);

        if (trackedObjects.Length > 0)
        {
            positions = new NativeArray<Vector3>(trackedObjects.Length, Allocator.Persistent);
        }
        else
        {
            Debug.LogWarning("No tracked objects assigned.");
        }

        // Convert objectHeatContributions to NativeArray<float> if it's not empty
        if (objectHeatContributions.Length > 0)
        {
            contributions = new NativeArray<float>(objectHeatContributions, Allocator.Persistent);
        }
        else
        {
            // Default contribution if no specific heat contributions are set
            contributions = new NativeArray<float>(trackedObjects.Length, Allocator.Persistent);
            for (int i = 0; i < trackedObjects.Length; i++)
            {
                contributions[i] = 1.0f; // Default heat contribution value
            }
        }
    }

    // ===========================
    // Update Loop (Handle Heat Decay, Position Updates, and Scheduling)
    // ===========================
    void Update()
    {
        // Resize positions array if the number of tracked objects changes
        if (!positions.IsCreated || trackedObjects.Length != positions.Length)
        {
            if (positions.IsCreated) positions.Dispose();
            positions = new NativeArray<Vector3>(trackedObjects.Length, Allocator.Persistent);
        }

        // Update positions array with current tracked object positions
        for (int i = 0; i < trackedObjects.Length; i++)
        {
            positions[i] = trackedObjects[i].position;
        }

        // Apply heat decay over time before updating
        for (int i = 0; i < heatmapData.Length; i++)
        {
            heatmapData[i] = Mathf.Max(0, heatmapData[i] - heatDecayRate); // Decay the heat value
        }

        // Schedule heatmap job with the updated data
        HeatmapJob heatmapJob = new HeatmapJob
        {
            gridSize = gridSize,
            gridBounds = gridBounds,
            positions = positions,
            heatmap = heatmapData,
            heatContributions = contributions // Pass NativeArray<float> for contributions
        };

        jobHandle = heatmapJob.Schedule();
    }

    // ===========================
    // Finalization (Complete Job and Normalize Heatmap)
    // ===========================
    void LateUpdate()
    {
        jobHandle.Complete(); // Wait for job completion before proceeding

        // Find the maximum heat value for normalization
        maxHeat = 0;
        for (int i = 0; i < heatmapData.Length; i++)
        {
            if (heatmapData[i] > maxHeat)
            {
                maxHeat = heatmapData[i];
            }
        }
    }

    // ===========================
    // Cleanup (Dispose of Native Arrays)
    // ===========================
    void OnDestroy()
    {
        if (heatmapData.IsCreated) heatmapData.Dispose();
        if (positions.IsCreated) positions.Dispose();
        if (contributions.IsCreated) contributions.Dispose(); // Dispose contributions array
    }

    // ===========================
    // Gizmos for Debugging (Draw the Heatmap in Editor View)
    // ===========================
    void OnDrawGizmos()
    {
        if (heatmapData.IsCreated)
        {
            float cellSizeX = gridBounds.x / gridSize;
            float cellSizeZ = gridBounds.z / gridSize;

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    int index = x + z * gridSize;
                    float heat = heatmapData[index];

                    // Normalize heat based on the maximum value
                    float normalizedHeat = maxHeat > 0 ? heat / maxHeat : 0;

                    // Adjust color intensity based on normalized heat value
                    Color heatColor = Color.Lerp(Color.blue, Color.red, normalizedHeat);
                    Gizmos.color = heatColor;

                    Vector3 cellCenter = new Vector3(
                        x * cellSizeX - gridBounds.x / 2 + cellSizeX / 2,
                        0,
                        z * cellSizeZ - gridBounds.z / 2 + cellSizeZ / 2
                    );

                    // Draw a cube in the center of each cell to represent heat
                    Gizmos.DrawCube(cellCenter, new Vector3(cellSizeX, 0.1f, cellSizeZ));
                }
            }
        }
    }

    // ===========================
    // Heatmap Job (Calculates Heatmap Data)
    // ===========================
    [BurstCompile]
    public struct HeatmapJob : IJob
    {
        public int gridSize;
        public Vector3 gridBounds;
        [ReadOnly] public NativeArray<Vector3> positions;
        [ReadOnly] public NativeArray<float> heatContributions; // Heat contribution from each object
        public NativeArray<float> heatmap;

        // ===========================
        // Job Execution (Calculate Heatmap Contributions)
        // ===========================
        public void Execute()
        {
            // Clear heatmap data before recalculating
            for (int i = 0; i < heatmap.Length; i++) heatmap[i] = 0;

            float cellSizeX = gridBounds.x / gridSize;
            float cellSizeZ = gridBounds.z / gridSize;

            // Iterate over positions and contribute heat to the grid cells
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 pos = positions[i];
                float contribution = heatContributions.Length > i ? heatContributions[i] : 1.0f; // Default heat contribution value if not set

                // Contribute heat to each grid cell based on distance to each tracked object
                for (int x = 0; x < gridSize; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        Vector3 cellCenter = new Vector3(
                            x * cellSizeX - gridBounds.x / 2 + cellSizeX / 2,
                            pos.y, // Include the object's height for distance calculations
                            z * cellSizeZ - gridBounds.z / 2 + cellSizeZ / 2
                        );

                        // Calculate distance between the object and the cell center
                        float distance = Vector3.Distance(pos, cellCenter);

                        // Skip contribution if the object is too far
                        if (distance > Mathf.Max(cellSizeX, cellSizeZ)) continue;

                        // Heat contribution is inversely proportional to the distance
                        float heat = contribution / (distance + 1.0f); // Avoid division by zero

                        // Add the heat to the respective grid cell
                        int index = x + z * gridSize;
                        heatmap[index] += heat;
                    }
                }
            }
        }
    }
}
