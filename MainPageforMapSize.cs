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
    public partial class MainPageforMapSize : Form
    {
        public MainPageforMapSize()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Define tile size
            int tileSize = 32;


            // Parse user input for tile size, start and end positions, and map dimensions
            if (!int.TryParse(textBoxStartX.Text, out int startX) || startX < 0)
            {
                MessageBox.Show("Please enter a valid start X position.");
                return;
            }

            if (!int.TryParse(textBoxStartY.Text, out int startY) || startY < 0)
            {
                MessageBox.Show("Please enter a valid start Y position.");
                return;
            }

            if (!int.TryParse(textBoxFinishX.Text, out int endX) || endX < 0)
            {
                MessageBox.Show("Please enter a valid end X position.");
                return;
            }

            if (!int.TryParse(textBoxFinishY.Text, out int endY) || endY < 0)
            {
                MessageBox.Show("Please enter a valid end Y position.");
                return;
            }

            if (!int.TryParse(textBoxWidth.Text, out int mapWidth) || mapWidth <= 0)
            {
                MessageBox.Show("Please enter a valid map width (number of columns).");
                return;
            }

            if (!int.TryParse(textBoxHeight.Text, out int mapHeight) || mapHeight <= 0)
            {
                MessageBox.Show("Please enter a valid map height (number of rows).");
                return;
            }

            // Adjust the user input to 0-based indexing
            startX -= 1; //Αφαιρώ το 1 από κάθε συντεταγμένη γιατί οι συντεταγμένες του grid ξεκινούν από το 0,0 και όχι από το 1,1
            startY -= 1;
            endX -= 1;
            endY -= 1;

            // Ensure the start and end positions are within bounds of the map
            if (startX >= mapWidth || startY >= mapHeight)
            {
                MessageBox.Show("The start position is outside the map boundaries.");
                return;
            }

            if (endX >= mapWidth || endY >= mapHeight)
            {
                MessageBox.Show("The end position is outside the map boundaries.");
                return;
            }


            // Open a new form with the tile map
            TileMap tileMapForm = new TileMap(tileSize, startX, startY, endX, endY, mapWidth, mapHeight);
            tileMapForm.Show();
        }
    }
}
