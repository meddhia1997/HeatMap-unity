# Heatmap Generator for Unity

## Overview
The **Heatmap Generator** script generates a 3D grid-based heatmap that visualizes the intensity of tracked objects' positions within a given area. It utilizes Unity’s **Burst Compiler**, **Jobs System**, and **Native Collections** for optimized performance when handling large datasets or many tracked objects.

---

## Features

- **3D Grid Visualization**: A 3D grid where each cell's color represents the heat intensity based on the proximity of tracked objects.
- **High-Performance Calculation**: Utilizes **Burst Compiler** and **Jobs System** for multi-threaded heatmap calculation, offering better performance.
- **Dynamic Heat Contribution**: Each tracked object contributes heat to surrounding grid cells, with closer objects contributing more heat.
- **Normalized Heatmap**: The heatmap values are normalized for a consistent visual experience.
- **Real-Time Debugging**: Visualizes the heatmap in Unity’s **Scene View** using **Gizmos**, providing a clear view of the heat distribution.
- **Customizable**: Modify the grid size, bounds, and tracked objects to fit the needs of your scene.

---

## Setup and Installation

### Prerequisites

Ensure you have the following Unity packages installed:

- **Unity.Burst**
- **Unity.Collections**
- **Unity.Jobs**

These packages are essential for the multi-threaded performance and optimization.

---

## How to Use

1. **Attach the Script**: Attach the `HeatmapGenerator` script to any GameObject in your Unity scene.
2. **Assign Tracked Objects**: In the Inspector, populate the `trackedObjects` array with the `Transform` components of the objects you want to track.
3. **Configure Grid**: Set the `gridSize` (number of cells) and `gridBounds` (size of the grid) to match your scene requirements.
4. **Visualize the Heatmap**: The heatmap will be drawn in the Scene view as **Gizmos**. The color of each grid cell will represent its intensity, ranging from blue (low heat) to red (high heat).

---

## Script Breakdown

### **HeatmapGenerator Class**

This class handles the initialization, updating, and visualization of the heatmap.

#### Public Fields:
- **`gridSize`**: Defines the number of grid cells along the X and Z axes.
- **`gridBounds`**: Specifies the bounds of the grid in world space (X, Y, Z).
- **`trackedObjects`**: An array of tracked `Transform` components.

#### Private Fields:
- **`heatmapData`**: Stores heat values for each grid cell.
- **`positions`**: Holds the positions of the tracked objects.
- **`maxHeat`**: The maximum heat value used for normalization.

#### Key Methods:
- **`Start()`**: Initializes the heatmap and positions arrays.
- **`Update()`**: Updates the positions of tracked objects and schedules the heatmap job.
- **`LateUpdate()`**: Finalizes the job and normalizes the heatmap data.
- **`OnDestroy()`**: Disposes of allocated memory to prevent memory leaks.
- **`OnDrawGizmos()`**: Visualizes the heatmap in the Scene view using Gizmos.

---

### **HeatmapJob Struct**

The `HeatmapJob` struct is responsible for calculating heatmap data in parallel using Unity’s Job System.

#### Fields:
- **`gridSize`**: Number of grid cells.
- **`gridBounds`**: The bounds of the grid in world space.
- **`positions`**: Array of positions for tracked objects.
- **`heatmap`**: The array storing the heatmap data.

#### `Execute()` Method:
- Clears the heatmap data.
- Iterates over each tracked object, calculating the heat contribution for each grid cell based on proximity.
- The closer an object is to a cell, the more heat it contributes.

---

## Troubleshooting

- **Gizmos Not Showing**: Ensure that **Gizmos** are enabled in the Unity Scene view (top-right of the Scene view window).
- **Heatmap Not Updating**: Check that `trackedObjects` is populated and that the objects are moving or changing positions.
- **Performance Issues**: If working with a large number of tracked objects or a large grid, ensure that the **Burst Compiler** and **Jobs System** are correctly enabled for optimal performance.

---

## Future Improvements

- **Heat Decay**: Add an adjustable heat decay rate for more dynamic heatmap visuals over time.
- **Non-Square Grids**: Support grids that are not square or can be customized to different shapes based on the game's needs.
- **Real-Time Updates**: Implement continuous heatmap updates as objects move, ensuring the heatmap reflects real-time changes.

---

## License

This script is free to use under the [MIT License](LICENSE).

---

## Contact

For more information, please feel free to contact me .

---

