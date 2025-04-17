using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathFinding_final
{
    public partial class TileMap : Form
    {
        private int tileSize, startX, startY, endX, endY;
        private int mapWidth, mapHeight;
        private string[,] grid;
        private Image selectedImage; // The currently selected image for the obstacle
        private Image plainImage, mountainImage, houseImage, forestImage, castleImage, statueImage, roadImage, desertImage, wolfImage, spiderImage;
        private readonly Image riverImage;
        private Dictionary<string, double> obstacleCosts;
        private const double baseCost = 50;
        private Font font;  // Font for rendering text in tiles 
        private ContextMenuStrip costContextMenu; // ContextMenuStrip for right-click to change movement cost
        private Panel mapPanel;  // Class-level variable for the map panel
        private Dictionary<Point, double> tileCosts; // Store movement costs for each tile
        private Point lastRightClickedTile; // Store the last tile clicked with the right mouse button
        private HashSet<string> obstacleTypes = new HashSet<string> { "Mountain", "House", "River" };
        private List<Point> bestRouteTiles = new List<Point>();
        private bool attractionRepulsionEnabled = false; // Flag to track if attraction/repulsion effects are enabled
        private bool isFormInitialized = false;
        private Dictionary<Point, double> manualTileCosts; // Store user-set costs for tiles
        Label totalCostLabel = new Label(); // Display the total cost after calculating the best route
        private List<Point> aStarPath = new List<Point>();
        private List<Point> bestFirstPath = new List<Point>();
        private List<Point> depthFirstPath = new List<Point>();
        private Dictionary<Point, double> gScores = new Dictionary<Point, double>(); // G cost for each tile
        private Dictionary<Point, double> fScores = new Dictionary<Point, double>(); // F cost for each tile

        public TileMap(int tileSize, int startX, int startY, int endX, int endY, int mapWidth, int mapHeight)
        {
            // Validate that start and finish are not the same
            if (startX == endX && startY == endY)
            {
                MessageBox.Show("Error: Start and Finish positions cannot be the same.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop the constructor from running further
            }

            // Initialize the form and its properties
            InitializeComponent();
            this.tileSize = tileSize;
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.Font = font;

            // Set the form size based on the map width, height, and space for GroupBox
            this.ClientSize = new Size(mapWidth * tileSize + 300, mapHeight * tileSize + 300); // 300 for the GroupBox width and some padding at the bottom
            this.Text = "Tile Map";

            grid = new string[mapWidth, mapHeight]; // Initialize grid

            // Set start and end points in the grid
            grid[startX, startY] = "start";
            grid[endX, endY] = "end";

            manualTileCosts = new Dictionary<Point, double>();

            // Load images 
            plainImage = Image.FromFile("images/TerrainPlains.png");
            mountainImage = Image.FromFile("images/ObstacleMountain.png");
            houseImage = Image.FromFile("images/ObstacleHouse.png");
            forestImage = Image.FromFile("images/TerrainForest.png");
            riverImage = Image.FromFile("images/ObstacleRiver.png");
            castleImage = Image.FromFile("images/AttractionCastle.png");
            statueImage = Image.FromFile("images/AttractionStatue.png");
            roadImage = Image.FromFile("images/TerrainRoad.png");
            desertImage = Image.FromFile("images/TerrainDesert.png");
            wolfImage = Image.FromFile("images/RepulsionWolf.png");
            spiderImage = Image.FromFile("images/RepulsionSpider.png");

            // Create ContextMenuStrip for right-click to change cost
            costContextMenu = new ContextMenuStrip();
            var increase1MenuItem = new ToolStripMenuItem("Increase by 1") { Tag = 1 };
            var increase5MenuItem = new ToolStripMenuItem("Increase by 5") { Tag = 5 };
            var increase10MenuItem = new ToolStripMenuItem("Increase by 10") { Tag = 10 };
            var decrease1MenuItem = new ToolStripMenuItem("Decrease by 1") { Tag = -1 };
            var decrease5MenuItem = new ToolStripMenuItem("Decrease by 5") { Tag = -5 };
            var decrease10MenuItem = new ToolStripMenuItem("Decrease by 10") { Tag = -10 };

            increase1MenuItem.Click += ChangeCostMenuItem_Click;
            increase5MenuItem.Click += ChangeCostMenuItem_Click;
            increase10MenuItem.Click += ChangeCostMenuItem_Click;
            decrease1MenuItem.Click += ChangeCostMenuItem_Click;
            decrease5MenuItem.Click += ChangeCostMenuItem_Click;
            decrease10MenuItem.Click += ChangeCostMenuItem_Click;

            costContextMenu.Items.AddRange(new ToolStripMenuItem[]
            {
            increase1MenuItem, increase5MenuItem, increase10MenuItem,
            decrease1MenuItem, decrease5MenuItem, decrease10MenuItem
            });

            // Set up the number of image-label pairs
            int numberOfImages = 11; // Number of image-label pairs you're adding
            int imageHeight = 45; // Height of each image with the label
            int groupBoxHeight = numberOfImages * imageHeight; // Total height for GroupBox

            // Create a GroupBox for obstacle and terrain selection
            GroupBox selectionGroupBox = new GroupBox();
            selectionGroupBox.Text = "Painting Tool";
            selectionGroupBox.Location = new Point(10, 10);  // Positioned at the top left
            selectionGroupBox.Size = new Size(160, groupBoxHeight); // Adjust size based on form height

            // PictureBox for selecting images and Labels for descriptions
            AddImageWithLabel(selectionGroupBox, plainImage, "Clear", 20);
            AddImageWithLabel(selectionGroupBox, mountainImage, "Mountain Obstacle", 60);
            AddImageWithLabel(selectionGroupBox, houseImage, "House Obstacle", 100);
            AddImageWithLabel(selectionGroupBox, riverImage, "River Obstacle", 140);
            AddImageWithLabel(selectionGroupBox, forestImage, "Forest Terrain", 180);
            AddImageWithLabel(selectionGroupBox, roadImage, "Road Terrain", 220);
            AddImageWithLabel(selectionGroupBox, desertImage, "Desert Terrain", 260);
            AddImageWithLabel(selectionGroupBox, castleImage, "Castle Attraction", 300);
            AddImageWithLabel(selectionGroupBox, statueImage, "Statue Attraction", 340);
            AddImageWithLabel(selectionGroupBox, wolfImage, "Wolf Repulsion", 380);
            AddImageWithLabel(selectionGroupBox, spiderImage, "Spider Repulsion", 420);

            // Add the selection panel to the form
            this.Controls.Add(selectionGroupBox);

            // Set the default selected image
            selectedImage = plainImage;

            // Create a panel to hold the tile map
            mapPanel = new Panel();
            mapPanel.Size = new Size(mapWidth * tileSize, mapHeight * tileSize);
            mapPanel.Location = new Point(250, 10); // Position it next to the GroupBox
            mapPanel.BackgroundImage = plainImage;
            mapPanel.Paint += DrawTileMap;  // Hook the Paint event to draw the map
            mapPanel.MouseClick += Panel_MouseClick;   // Hook the MouseClick event to set obstacles
            mapPanel.MouseDown += MapPanel_MouseDown; // Handle right click of Mouse
            this.Controls.Add(mapPanel);

            // Create a GroupBox for the Attraction-Repulsion effects
            GroupBox attractionRepulsionGroupBox = new GroupBox();
            attractionRepulsionGroupBox.Text = "Attraction/Repulsion Effects";
            attractionRepulsionGroupBox.Location = new Point(10, selectionGroupBox.Bottom + 10);  // Below the Painting Tool group box
            attractionRepulsionGroupBox.Size = new Size(160, 70); // Set size for the group box

            // Create "Yes" radio button (for enabling attraction-repulsion effect)
            RadioButton yesRadioButton = new RadioButton();
            yesRadioButton.Text = "Yes";
            yesRadioButton.Location = new Point(10, 20);
            yesRadioButton.CheckedChanged += (sender, e) =>
            {
                if (isFormInitialized && yesRadioButton.Checked)
                {
                    attractionRepulsionEnabled = true;  // Enable attraction-repulsion effect
                    UpdateCostsFromAttractionAndRepulsion();  // Recalculate costs with attraction-repulsion effects
                }
            };

            // Create "No" radio button (for disabling attraction-repulsion effect)
            RadioButton noRadioButton = new RadioButton();
            noRadioButton.Text = "No";
            noRadioButton.Location = new Point(10, 40);
            noRadioButton.CheckedChanged += (sender, e) =>
            {
                if (isFormInitialized && noRadioButton.Checked)
                {
                    attractionRepulsionEnabled = false;  // Disable attraction-repulsion effect
                    UpdateCostsFromAttractionAndRepulsion();  // Recalculate costs without attraction-repulsion effects
                }
            };

            // Add the radio buttons to the GroupBox
            attractionRepulsionGroupBox.Controls.Add(yesRadioButton);
            attractionRepulsionGroupBox.Controls.Add(noRadioButton);

            noRadioButton.Checked = true; // Set the "No" radio button as default (checked)

            // Mark the form as initialized
            isFormInitialized = true;

            // Add the Attraction-Repulsion GroupBox to the form
            this.Controls.Add(attractionRepulsionGroupBox);

            // Button to clear the tile map
            Button clearMapButton = new Button();
            clearMapButton.Text = "Clear Map";
            clearMapButton.Location = new Point(10, attractionRepulsionGroupBox.Bottom + 20); // Below the Attraction-Repulsion group box
            clearMapButton.Click += ClearMapButton_Click;
            this.Controls.Add(clearMapButton);

            //Label for "Select algorithm"
            Label algorithmLabel = new Label();
            algorithmLabel.Text = "Select Algorithm";
            algorithmLabel.Font = new Font(algorithmLabel.Font, FontStyle.Bold);
            algorithmLabel.Location = new Point(10, clearMapButton.Bottom + 20); // Below the Clear Map Button
            algorithmLabel.AutoSize = true; // Adjust size based on the text
            this.Controls.Add(algorithmLabel);

            // Select algorithm method through ComboBox
            ComboBox algorithmComboBox = new ComboBox();
            algorithmComboBox.Items.AddRange(new string[] { "A*", "Depth First Search", "Best First Search" });
            algorithmComboBox.Location = new Point(10, algorithmLabel.Bottom + 10); // Below the label
            algorithmComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            algorithmComboBox.SelectedIndex = 0;
            algorithmComboBox.SelectedIndexChanged += (sender, e) =>
            {
                // Get the selected algorithm from the combo box
                string selectedAlgorithm = algorithmComboBox.SelectedItem.ToString();

                // Based on the selected algorithm, run the corresponding pathfinding algorithm
                if (selectedAlgorithm == "A*")
                {
                    // Run the A* algorithm and store the result
                    aStarPath = PathFindingAStar();
                }
                else if (selectedAlgorithm == "Best-First")
                {
                    // Run the Best-First algorithm and store the result
                    bestFirstPath = PathFindingBestFirst();
                }
                else if (selectedAlgorithm == "Depth-First")
                {
                    // Run the Depth-First algorithm and store the result
                    depthFirstPath = PathFindingDFS();
                }

                // Trigger a redraw of the form to highlight the selected algorithm's path
                Invalidate(); // This will cause the form to call the Paint event again
            };
            this.Controls.Add(algorithmComboBox);

            //Button to find the path
            Button findPathButton = new Button();
            findPathButton.Text = "Find Path";
            findPathButton.Location = new Point(140, algorithmLabel.Bottom + 10); // Next to the algorithm combo box
            findPathButton.Click += (s, e) =>
            {
                string selectedAlgorithm = algorithmComboBox.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedAlgorithm))
                {
                    MessageBox.Show("Please select an algorithm.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                aStarPath.Clear();
                bestFirstPath.Clear();
                depthFirstPath.Clear();

                List<Point> selectedPath = null; // Declare selectedPath before the switch

                switch (selectedAlgorithm)
                {
                    case "A*":
                        aStarPath = PathFindingAStar();
                        selectedPath = aStarPath;
                        MessageBox.Show("A* path found with " + aStarPath.Count + " tiles.", "A* Path", MessageBoxButtons.OK);
                        break;
                    case "Best First Search":
                        bestFirstPath = PathFindingBestFirst();
                        selectedPath = bestFirstPath;
                        MessageBox.Show("Best First path found with " + bestFirstPath.Count + " tiles.", "Best First Path", MessageBoxButtons.OK);
                        break;
                    case "Depth First Search":
                        depthFirstPath = PathFindingDFS();
                        selectedPath = depthFirstPath;
                        MessageBox.Show("DFS path found with " + depthFirstPath.Count + " tiles.", "DFS Path", MessageBoxButtons.OK);
                        break;
                    default:
                        MessageBox.Show("Unknown algorithm selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                // Calculate the total cost of the best route
                double totalCost = 0;
                if (selectedPath != null)
                {
                    foreach (Point tile in selectedPath)
                    {
                        if (tileCosts.TryGetValue(tile, out double cost))
                        {
                            totalCost += cost;
                        }
                    }
                }

                // Update the total cost label
                totalCostLabel.Text = $"Total Cost: {totalCost:F1}";

                // Redraw the map to show the best route
                mapPanel.Invalidate();
            };
            this.Controls.Add(findPathButton);

            // Add a label to display the total cost
            totalCostLabel.Text = "Total Cost: 0";
            totalCostLabel.Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold);
            totalCostLabel.Location = new Point(10, algorithmComboBox.Bottom + 50); // Below the algorithm combo box
            totalCostLabel.AutoSize = true;
            this.Controls.Add(totalCostLabel);


            tileCosts = new Dictionary<Point, double>();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    tileCosts[new Point(x, y)] = baseCost; // Default cost

                    // Assign specific costs based on terrain type
                    string terrainType = grid[x, y];
                    if (terrainType == "Forest")
                        tileCosts[new Point(x, y)] = 60;
                    else if (terrainType == "Road")
                        tileCosts[new Point(x, y)] = 40;
                    else if (terrainType == "Desert")
                        tileCosts[new Point(x, y)] = 85;
                }
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the tile position
            int x = e.X / tileSize;
            int y = e.Y / tileSize;

            // If the clicked tile is within bounds, show the context menu
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                if (e.Button == MouseButtons.Right)
                {
                    lastRightClickedTile = new Point(x, y); // Store the clicked position
                    costContextMenu.Show(this, this.PointToClient(Cursor.Position)); // Show the context menu
                }
            }
        }

        private void ChangeCostMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                int costChange = (int)menuItem.Tag;

                // Ensure the last right-clicked tile is valid
                if (tileCosts.ContainsKey(lastRightClickedTile))
                {
                    string terrainType = grid[lastRightClickedTile.X, lastRightClickedTile.Y];

                    // Skip if the tile is an obstacle
                    if (obstacleTypes.Contains(terrainType))
                    {
                        MessageBox.Show("You cannot change the movement cost of obstacle tiles.", "Invalid Action", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Update manual costs
                    if (!manualTileCosts.ContainsKey(lastRightClickedTile))
                        manualTileCosts[lastRightClickedTile] = baseCost;

                    manualTileCosts[lastRightClickedTile] += costChange;

                    // Update tileCosts for immediate UI reflection
                    tileCosts[lastRightClickedTile] = manualTileCosts[lastRightClickedTile];

                    mapPanel.Invalidate(); // Redraw the panel
                }
            }
        }

        // Helper method to add an image with a label
        private void AddImageWithLabel(GroupBox groupBox, Image image, string labelText, int topPosition)
        {
            // PictureBox for the image
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.Size = new Size(30, 30);
            pictureBox.Location = new Point(10, topPosition);  // Position vertically
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Attach click event to select image
            pictureBox.Click += (s, e) =>
            {
                selectedImage = image; // Set the selected image
            };

            groupBox.Controls.Add(pictureBox);

            // Label for the image description
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(40, topPosition + 15);  // Position text to the right of the image
            label.AutoSize = true;  // Adjust size based on text length
            groupBox.Controls.Add(label);
        }

        // Method to draw the tile map
        private void DrawTileMap(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen blackPen = new Pen(Color.Black); // Grid lines
            Pen AStarPen = new Pen(Color.Red, 3); // Pen for highlighting the best route for A* algorithm
            Pen bestFirstPen = new Pen(Color.DarkGreen, 3); // For Best First
            Pen depthFirstPen = new Pen(Color.Blue, 3); // For Depth First
            Font smallFont = new Font(this.Font.FontFamily, 6); // Small font for text

            // Get start and end tiles from user-defined coordinates from Form1
            Point startTile = new Point(startX, startY);
            Point endTile = new Point(endX, endY);

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    Rectangle tile = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    string terrainType = grid[x, y];
                    double movementCost = tileCosts.ContainsKey(new Point(x, y)) ? tileCosts[new Point(x, y)] : baseCost;

                    Point currentTile = new Point(x, y);

                    // Default to plain terrain for start and end tiles
                    if (currentTile == startTile || currentTile == endTile)
                    {
                        // Render plain terrain as the default background
                        g.DrawImage(plainImage, tile);

                        // Highlight the start and end tiles with borders
                        if (currentTile == startTile)
                            g.DrawRectangle(new Pen(Color.Green, 3), tile); // Green border for start
                        else if (currentTile == endTile)
                            g.DrawRectangle(new Pen(Color.Red, 3), tile); // Red border for end

                     // Draw the movement cost in the top-left corner
                        TextRenderer.DrawText(
                            g,
                            movementCost.ToString("F0"),
                            smallFont,
                            new Point(tile.X + 2, tile.Y + 2),
                            Color.Black
                        );
                    }
                    else
                    {
                        // Draw terrain type
                        if (terrainType == "Plain")
                        g.DrawImage(plainImage, tile);
                    else if (terrainType == "Mountain")
                        g.DrawImage(mountainImage, tile);
                    else if (terrainType == "House")
                        g.DrawImage(houseImage, tile);
                    else if (terrainType == "River")
                        g.DrawImage(riverImage, tile);
                    else if (terrainType == "Forest")
                        g.DrawImage(forestImage, tile);
                    else if (terrainType == "Desert")
                        g.DrawImage(desertImage, tile);
                    else if (terrainType == "Road")
                        g.DrawImage(roadImage, tile);
                    else if (terrainType == "Castle")
                        g.DrawImage(castleImage, tile);
                    else if (terrainType == "Statue")
                        g.DrawImage(statueImage, tile);
                    else if (terrainType == "Wolf")
                        g.DrawImage(wolfImage, tile);
                    else if (terrainType == "Spider")
                        g.DrawImage(spiderImage, tile);

                        }
                    // Draw red "X" for obstacles
                    if (obstacleTypes.Contains(terrainType))
                    {
                        DrawRedCross(g, new Rectangle(tile.X, tile.Y, tileSize / 3, tileSize / 3));
                    }

                    // Highlight the shortest path for each algorithm
                    if (aStarPath.Contains(currentTile))
                    {
                        g.DrawRectangle(AStarPen, tile); // Highlight A* path in red
                    }
                    else if (bestFirstPath.Contains(currentTile))
                    {
                        g.DrawRectangle(bestFirstPen, tile); // Highlight Best First path in dark green
                    }
                    else if (depthFirstPath.Contains(currentTile))
                    {
                        g.DrawRectangle(depthFirstPen, tile); // Highlight Depth First path in blue
                    }

                    // Calculate G, H, and F costs
                    double gCost = gScores.ContainsKey(currentTile) ? gScores[currentTile] : double.PositiveInfinity;
                    double hCost = Heuristic(currentTile, endTile);
                    double fCost = gCost + hCost;

                    // Draw movement cost if not an obstacle
                    if (!obstacleTypes.Contains(terrainType) && terrainType != "start" && terrainType != "end")
                    {
                        TextRenderer.DrawText(
                            g,
                            movementCost.ToString("F0"),
                            smallFont,
                            new Point(tile.X + 2, tile.Y + 2),
                            Color.Black
                        );
                    }

                    // Draw grid lines
                    g.DrawRectangle(blackPen, tile);
                }
            }

            // Dispose of pens to free resources
            blackPen.Dispose();
            AStarPen.Dispose();
        }

        // Method for drawing red cross for obstacles
        private void DrawRedCross(Graphics g, Rectangle corner)
        {
            Pen redPen = new Pen(Color.Red, 2); // Red pen with a thicker stroke

            // Draw diagonal lines to create a cross in the corner
            g.DrawLine(redPen, corner.Left, corner.Top, corner.Right, corner.Bottom); // Top-left to bottom-right
            g.DrawLine(redPen, corner.Right, corner.Top, corner.Left, corner.Bottom); // Top-right to bottom-left

            redPen.Dispose();
        }

        // Method to handle mouse clicks inside the panel
        private void Panel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X / tileSize;
                int y = e.Y / tileSize;

                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && grid[x, y] != "start" && grid[x, y] != "end")
                {
                    Point currentTile = new Point(x, y);

                    // Update terrain type
                    if (selectedImage == plainImage)
                        grid[x, y] = "Plain";
                    else if (selectedImage == mountainImage)
                        grid[x, y] = "Mountain";
                    else if (selectedImage == houseImage)
                        grid[x, y] = "House";
                    else if (selectedImage == forestImage)
                    {
                        grid[x, y] = "Forest";
                        tileCosts[new Point(x, y)] = 60;
                    }
                    else if (selectedImage == riverImage)
                        grid[x, y] = "River";
                    else if (selectedImage == castleImage)
                    {
                        grid[x, y] = "Castle";
                        tileCosts[new Point(x, y)] = -25;
                    }
                    else if (selectedImage == statueImage)
                    {
                        grid[x, y] = "Statue";
                        tileCosts[new Point(x, y)] = -90;
                    }
                    else if (selectedImage == desertImage)
                    {
                        grid[x, y] = "Desert";
                        tileCosts[new Point(x, y)] = 85;
                    }
                    else if (selectedImage == roadImage)
                    {
                        grid[x, y] = "Road";
                        tileCosts[new Point(x, y)] = 40;
                    }
                    else if (selectedImage == wolfImage)
                    {
                        grid[x, y] = "Wolf";
                        tileCosts[new Point(x, y)] = 65;
                    }
                    else if (selectedImage == spiderImage)
                    {
                        grid[x, y] = "Spider";
                        tileCosts[new Point(x, y)] = 30;
                    }

                    // Retain manual cost for the tile if it exists
                    if (manualTileCosts.ContainsKey(currentTile))
                        tileCosts[currentTile] = manualTileCosts[currentTile];
                    else
                        tileCosts[currentTile] = baseCost;

                    UpdateCostsFromAttractionAndRepulsion(); // Recalculate costs
                }
            }
        }

        // Method to handle clearing the map
        private void ClearMapButton_Click(object sender, EventArgs e)
        {
            // Reset all tiles in the grid to plain
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (grid[x, y] != "start" && grid[x, y] != "end")
                    {
                        grid[x, y] = "Plain";  // Set all cells to Plain
                    }
                }
            }

            // Reset tile costs to the base cost for all tiles
            tileCosts.Clear(); // Clear the current cost values
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    tileCosts[new Point(x, y)] = baseCost; // Set all tile costs back to base cost
                }
            }

            bestRouteTiles.Clear(); // Clear the best route tiles
            aStarPath.Clear();
            bestFirstPath.Clear();
            depthFirstPath.Clear();
            totalCostLabel.Text = "Total Cost: 0"; // Reset the total cost label

            // Redraw the map
            (sender as Button).Parent.Controls.OfType<Panel>().FirstOrDefault()?.Invalidate();
        }

        // Method to adjust the costs around attraction/repulsion objects
        private void UpdateCostsFromAttractionAndRepulsion()
        {
            // Reset costs first
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Point tile = new Point(x, y);
                    string terrainType = grid[x, y];

                    // Assign base costs for specific terrain types
                    if (terrainType == "Forest")
                        tileCosts[tile] = 60;
                    else if (terrainType == "Road")
                        tileCosts[tile] = 40;
                    else if (terrainType == "Desert")
                        tileCosts[tile] = 85;
                    else
                        tileCosts[tile] = baseCost; // Default cost
                }
            }

            // Apply attraction-repulsion effects
            if (attractionRepulsionEnabled)
            {
                foreach (var point in GetObjectPoints("Castle", "Statue"))
                {
                    ApplyAttractionEffect(point);
                }

                foreach (var point in GetObjectPoints("Wolf", "Spider"))
                {
                    ApplyRepulsionEffect(point);
                }
            }

            // Merge manual costs back into tileCosts
            foreach (var manualCost in manualTileCosts)
            {
                tileCosts[manualCost.Key] = manualCost.Value;
            }

            // Ensure attraction-repulsion is applied to start and end positions
            Point startTile = new Point(startX, startY);
            Point endTile = new Point(endX, endY);

            if (attractionRepulsionEnabled)
            {
                foreach (var point in GetObjectPoints("Castle", "Statue"))
                {
                    ApplyAttractionEffectToSpecialTile(point, startTile);
                    ApplyAttractionEffectToSpecialTile(point, endTile);
                }

                foreach (var point in GetObjectPoints("Wolf", "Spider"))
                {
                    ApplyRepulsionEffectToSpecialTile(point, startTile);
                    ApplyRepulsionEffectToSpecialTile(point, endTile);
                }
            }

            mapPanel.Invalidate(); // Redraw the panel to reflect cost changes
        }

        private void ApplyAttractionEffectToSpecialTile(Point center, Point specialTile)
        {
            int radius = 3;
            int maxCostChange = -20;

            int distance = Math.Abs(center.X - specialTile.X) + Math.Abs(center.Y - specialTile.Y); // Manhattan Distance
            if (distance <= radius)
            {
                int costChange = maxCostChange + (radius - distance);
                tileCosts[specialTile] += costChange;
            }
        }

        private void ApplyRepulsionEffectToSpecialTile(Point center, Point specialTile)
        {
            int radius = 3;
            int maxCostChange = 10;

            int distance = Math.Abs(center.X - specialTile.X) + Math.Abs(center.Y - specialTile.Y); // Manhattan Distance
            if (distance <= radius)
            {
                int costChange = maxCostChange - (radius - distance);
                tileCosts[specialTile] += costChange;
            }
        }

        // Helper method to get the points where specific objects exist
        private IEnumerable<Point> GetObjectPoints(params string[] objectTypes)
        {
            List<Point> points = new List<Point>();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    string terrainType = grid[x, y];
                    if (objectTypes.Contains(terrainType))
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }
            return points;
        }

        // Method to apply the attraction effect
        private void ApplyAttractionEffect(Point center)
        {
            // Attraction radius and cost change per tile
            int radius = 3;
            int maxCostChange = -20;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = center.X + dx;
                    int y = center.Y + dy;
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    {
                        int distance = Math.Abs(dx) + Math.Abs(dy);
                        if (distance <= radius)
                        {
                            int costChange = maxCostChange + (radius - distance);
                            tileCosts[new Point(x, y)] += costChange;
                        }
                    }
                }
            }
        }

        // Method to apply the repulsion effect
        private void ApplyRepulsionEffect(Point center)
        {
            // Repulsion radius and cost change per tile
            int radius = 3;
            int maxCostChange = 10;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = center.X + dx;
                    int y = center.Y + dy;
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    {
                        int distance = Math.Abs(dx) + Math.Abs(dy); //Manhattan Distance
                        if (distance <= radius)
                        {
                            int costChange = maxCostChange - (radius - distance);
                            tileCosts[new Point(x, y)] += costChange;
                        }
                    }
                }
            }
        }

        // PATH FINDING METHODS

        // Path finding method using A*
        private List<Point> PathFindingAStar()
        {
            List<Point> path = new List<Point>();

            // Initialize open and closed lists
            List<Point> openList = new List<Point>(); // Nodes to be evaluated
            HashSet<Point> closedList = new HashSet<Point>(); // Nodes already evaluated

            // Initialize start and goal points
            Point start = new Point(startX, startY);
            Point goal = new Point(endX, endY);

            // Initialize cost dictionaries
            Dictionary<Point, double> gScores = new Dictionary<Point, double>();
            Dictionary<Point, double> fScores = new Dictionary<Point, double>();
            Dictionary<Point, Point?> cameFrom = new Dictionary<Point, Point?>(); // For reconstructing the path

            // Start point setup
            openList.Add(start);
            gScores[start] = 0;
            fScores[start] = Heuristic(start, goal);

            while (openList.Count > 0)
            {
                // Find the point in the open list with the lowest fScore
                Point current = openList.OrderBy(p => fScores.ContainsKey(p) ? fScores[p] : double.MaxValue).First();

                if (current == goal)
                {
                    // Reconstruct the path from goal to start
                    while (cameFrom.ContainsKey(current))
                    {
                        path.Insert(0, current);
                        current = cameFrom[current].Value;
                    }
                    path.Insert(0, start); // Add the start point to the path
                    break;
                }

                openList.Remove(current);
                closedList.Add(current);

                // Check neighbors of the current point
                foreach (Point neighbor in GetNeighbors(current))
                {
                    // Skip already evaluated points
                    if (closedList.Contains(neighbor))
                        continue;

                    // Skip obstacles based on the obstacleTypes HashSet
                    if (obstacleTypes.Contains(grid[neighbor.X, neighbor.Y]))
                        continue;

                    double tentativeGScore = gScores[current] + GetCost(current, neighbor);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);

                    if (!gScores.ContainsKey(neighbor) || tentativeGScore < gScores[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScores[neighbor] = tentativeGScore;
                        fScores[neighbor] = gScores[neighbor] + Heuristic(neighbor, goal);
                    }
                }
            }

            return path; // Return the final path
        }

        // Helper method to calculate neighbors
        private List<Point> GetNeighbors(Point node)
        {
            List<Point> neighbors = new List<Point>();
            int[] dx = { -1, 1, 0, 0 }; // For 4 directions (up, down, left, right)
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < dx.Length; i++)
            {
                int newX = node.X + dx[i];
                int newY = node.Y + dy[i];

                // Check if the neighbor is within bounds
                if (newX >= 0 && newY >= 0 && newX < mapWidth && newY < mapHeight)
                {
                    neighbors.Add(new Point(newX, newY));
                }
            }

            return neighbors;
        }

        private double GetCost(Point current, Point neighbor)
        {
            return tileCosts.ContainsKey(neighbor) ? tileCosts[neighbor] : baseCost; // Default cost if not explicitly set
        }

        // Heuristic method for A* (Manhattan distance)
        private double Heuristic(Point current, Point goal)
        {
            return Math.Abs(current.X - goal.X) + Math.Abs(current.Y - goal.Y);
        }

        private List<Point> PathFindingDFS()
        {
            List<Point> path = new List<Point>();

            // Stack for DFS
            Stack<Point> stack = new Stack<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            Dictionary<Point, Point?> cameFrom = new Dictionary<Point, Point?>(); // For reconstructing the path

            // Initialize start and goal points
            Point start = new Point(startX, startY);
            Point goal = new Point(endX, endY);

            stack.Push(start);
            visited.Add(start);

            while (stack.Count > 0)
            {
                Point current = stack.Pop();

                if (current == goal)
                {
                    // Reconstruct the path from goal to start
                    while (cameFrom.ContainsKey(current))
                    {
                        path.Insert(0, current);
                        current = cameFrom[current].Value;
                    }
                    path.Insert(0, start); // Add the start point to the path
                    break;
                }

                foreach (Point neighbor in GetNeighbors(current))
                {
                    // Skip already visited nodes
                    if (visited.Contains(neighbor))
                        continue;

                    // Skip obstacles
                    if (obstacleTypes.Contains(grid[neighbor.X, neighbor.Y]))
                        continue;

                    stack.Push(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }

            return path; // Return the final path
        }

        private List<Point> PathFindingBestFirst()
        {
            List<Point> path = new List<Point>();

            // Priority Queue for Best-First Search
            SortedDictionary<double, List<Point>> openList = new SortedDictionary<double, List<Point>>();
            HashSet<Point> closedList = new HashSet<Point>();
            Dictionary<Point, Point?> cameFrom = new Dictionary<Point, Point?>(); // For reconstructing the path

            // Initialize start and goal points
            Point start = new Point(startX, startY);
            Point goal = new Point(endX, endY);

            // Start point setup
            double startHeuristic = Heuristic(start, goal);
            openList[startHeuristic] = new List<Point> { start };

            while (openList.Count > 0)
            {
                // Get the node with the lowest heuristic (smallest key in the sorted dictionary)
                double lowestKey = openList.First().Key;
                Point current = openList[lowestKey][0];
                openList[lowestKey].RemoveAt(0);

                // Remove the key if no more points are associated with it
                if (openList[lowestKey].Count == 0)
                    openList.Remove(lowestKey);

                if (current == goal)
                {
                    // Reconstruct the path from goal to start
                    while (cameFrom.ContainsKey(current))
                    {
                        path.Insert(0, current);
                        current = cameFrom[current].Value;
                    }
                    path.Insert(0, start); // Add the start point to the path
                    break;
                }

                closedList.Add(current);

                foreach (Point neighbor in GetNeighbors(current))
                {
                    // Skip already evaluated nodes
                    if (closedList.Contains(neighbor))
                        continue;

                    // Skip obstacles
                    if (obstacleTypes.Contains(grid[neighbor.X, neighbor.Y]))
                        continue;

                    double heuristic = Heuristic(neighbor, goal);

                    if (!openList.ContainsKey(heuristic))
                        openList[heuristic] = new List<Point>();

                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        openList[heuristic].Add(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            return path; // Return the final path
        }

    }
}
