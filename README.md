# 🌍 Pathfinding in a Grid with Dynamic Terrain and Obstacles

This project is a **pathfinding application** that finds the shortest path between a **starting** and **ending** position on a flat map represented by a table of i rows and j columns. The pathfinding algorithms used in this application are **Depth First Search (DFS)**, **Best First Search (BFS)**, and **A-Star Search**. The map also contains **terrain types**, **obstacles**, and **attraction/repulsion effects** that influence movement costs and, consequently, the pathfinding process.

## 📜 Project Description

The task of this application is to compute the shortest path between an initial and a final position on a grid-like map, while considering:
- **Obstacles**: Cells where movement is not possible.
- **Terrain Costs**: Different terrains (e.g., road, plain, desert, forest) have varying movement costs.
- **Attraction and Repulsion Areas**: Certain areas/objects can increase or decrease the cost of movement. Examples include:
    - **Repulsion** (e.g., wolves or spiders) increases the movement cost within a certain radius.
    - **Attraction** (e.g., statues or castles) reduces the movement cost within a certain radius.

The user will be able to define the **map size**, **start and final positions**, **obstacles**, **terrain types**, and the **attraction/repulsion effect**.

## 🗺️ Map Representation

The map is represented as a 2D array or grid where each cell has:
- **Movement Cost**: The cost to move through that cell.
- **Terrain Type**: Defines the terrain (road, plain, desert, forest).
- **Obstacles**: Cells where movement is not allowed (houses, mountains, rivers).
- **Attraction/Repulsion Effects**: Cells near objects or areas causing attraction or repulsion may have increased or decreased movement costs.

## 🛠️ Key Features

1. **Dynamic Costing**:
    - **Default Cost** = 50 (for regular terrain).
    - Terrain types (road, plain, desert, forest) modify the cost of movement.
    - **Attraction** and **Repulsion** areas modify the cost in their radius (3 cells).

2. **Algorithms Implemented**:
   - **Depth First Search (DFS)**: Explores as deeply as possible into the map.
   - **Best First Search (BFS)**: Chooses the path that seems most promising based on a heuristic.
   - **A-Star Algorithm**: Optimizes pathfinding by considering both the path cost and heuristic distance.

3. **Interactive Map**: The user defines the map's parameters, including:
   - **Dimensions** (i x j)
   - **Starting and Ending Positions**
   - **Terrain and Obstacle Data**
   - **Attraction/Repulsion objects or areas** with specified radius and effect.

4. **Pathfinding with/without Attraction/Repulsion Cost**: The user can choose to calculate the shortest path considering or ignoring the effects of attraction/repulsion.

## 📐 User Inputs
- **Map Dimensions**: Define the size of the grid (i rows, j columns).
- **Start and Final Positions**: User defines the starting and ending points.
- **Terrain Types**: Set the cost for various terrain types (e.g., desert = 100, forest = 75, road = 50).
- **Obstacles**: Mark certain grid cells as obstacles where movement is not allowed.
- **Attraction/Repulsion Areas**: Define objects (e.g., wolf or statue) and their effect radius.

## 🏁 How It Works
1. The user specifies the parameters of the map.
2. The map is dynamically generated based on the inputs.
3. The user runs one of the algorithms (DFS, BFS, A*) to find the shortest path.
4. The algorithm calculates the path considering obstacles, terrain costs, and attraction/repulsion areas.
