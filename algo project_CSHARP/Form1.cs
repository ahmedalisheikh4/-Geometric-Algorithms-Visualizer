using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;


namespace algo_project_CSHARP
{
    public partial class Form1 : Form
    {
        private Point clickedPoint;
        private List<Point> points = new List<Point>();
        private List<Point> lines = new List<Point>();

        private List<Point> convexHull = new List<Point>();
        private int currentEdgeIndex = -1;
        private Timer animationTimer = new Timer();
        private bool drawEdges = false;
        bool isFullScreen = false;
         HashSet<Point> hull = new HashSet<Point>();
        private Point? firstClick;
        private Point secondClick;
        private bool isDrawing;
        Stopwatch stopwatch1 = new Stopwatch();
        double elapsedTimeInSeconds2 = 0;

        public Form1()
        {
            InitializeComponent();

            // Configure the animation timer
            animationTimer.Interval = 500; // Adjust the interval (in milliseconds) to control animation speed
            animationTimer.Tick += AnimationTimer_Tick;
            // Set the initial size of the picture box to cover the whole screen           

            // Set the location of the buttons and the size of the PictureBox
            grahamscanbutton.Location = new Point(this.ClientSize.Width - grahamscanbutton.Width - 10, this.ClientSize.Height - grahamscanbutton.Height);
            CalculateConvexHullButton.Location = new Point(this.ClientSize.Width - CalculateConvexHullButton.Width - 10, this.ClientSize.Height - CalculateConvexHullButton.Height);
            Reset.Location = new Point(this.ClientSize.Width - Reset.Width - 160, this.ClientSize.Height - Reset.Height);
            bruteforcebutton.Location = new Point(this.ClientSize.Width - bruteforcebutton.Width - 10, this.ClientSize.Height - bruteforcebutton.Height);
            quickhullbutton.Location  = new Point(this.ClientSize.Width - quickhullbutton.Width - 10, this.ClientSize.Height - quickhullbutton.Height);
            researchhullbutton.Location = new Point(this.ClientSize.Width - researchhullbutton.Width - 10, this.ClientSize.Height - researchhullbutton.Height);
            linecheck1.Location = new Point(this.ClientSize.Width - linecheck1.Width - 30, this.ClientSize.Height - linecheck1.Height );
            linecheck2.Location = new Point(this.ClientSize.Width - linecheck2.Width - 30, this.ClientSize.Height - linecheck2.Height);
            linecheck3.Location = new Point(this.ClientSize.Width - linecheck3.Width - 30, this.ClientSize.Height - linecheck3.Height);
            timebox.Location = new Point(250,ClientRectangle.Height - timebox.Height - 10);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            grahamscanbutton.Hide();
            stackPictureBox.Hide();
            CalculateConvexHullButton.Hide();
            bruteforcebutton.Hide();
            quickhullbutton.Hide();
            Reset.Hide();
            researchhullbutton.Hide();
            pictureBox1.Hide();
            pictureBox2.Hide();
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            check1.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            points.Add(e.Location);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Create a Graphics object for drawing
            Graphics g = e.Graphics;

            // Draw all the points in the list
            foreach (Point point in points)
            {
                int radius = 5;
                g.FillEllipse(Brushes.Red, point.X - radius, point.Y - radius, 2 * radius, 2 * radius);
                g.DrawString($"({point.X}, {pictureBox1.Height - point.Y})", new Font("Arial", 12), Brushes.White, point.X + radius, point.Y - radius);
            }

            // Draw the edges of the convex hull up to the currentEdgeIndex

            if (drawEdges)
            {
                Pen pen = new Pen(Brushes.Blue, 2);
                for (int i = 0; i <= currentEdgeIndex; i++)
                {
                    if (i + 1 < convexHull.Count)
                    {
                        g.DrawLine(pen, convexHull[i], convexHull[i + 1]);
                    }
                    if (currentEdgeIndex == convexHull.Count - 1)
                    {
                        g.DrawLine(pen, convexHull[currentEdgeIndex], convexHull[0]);
                    }
                }
            }

        }

        private void CalculateConvexHullButton_Click(object sender, EventArgs e)
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
           

            if (points.Count < 3)
            {
                var result = MessageBox.Show("Less than 3 points are not acceptable!", "Error");
                convexHull.Clear();
                drawEdges = false;
            }
            else
            {
                // Call the Jarvis March algorithm to compute the convex hull
                drawEdges = true;
                stopwatch.Start();
                convexHull = JarvisMarch(points);
                stopwatch.Stop();

                currentEdgeIndex = -1; // Initialize to -1

                // Start the animation timer
                animationTimer.Start();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds; // Convert to milliseconds or seconds as needed
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms" + Environment.NewLine + "O(nh)";

            }

        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            currentEdgeIndex++;

            if (currentEdgeIndex >= convexHull.Count)
            {
                // Animation complete, stop the timer after drawing the last edge
                animationTimer.Stop();
                currentEdgeIndex = convexHull.Count - 1; // Ensure it's not out of bounds
            }

            pictureBox1.Invalidate();
        }




        private List<Point> JarvisMarch(List<Point> inputPoints)
        {
            if (inputPoints.Count < 3)
            {

                // Convex hull not possible with less than 3 points
                return inputPoints;
            }

            List<Point> convexHull = new List<Point>();

            // Find the point with the smallest Y-coordinate (and leftmost if ties)
            Point startPoint = inputPoints[0];
            foreach (var point in inputPoints)
            {
                if (point.Y > startPoint.Y || (point.Y == startPoint.Y && point.X > startPoint.X))
                {
                    startPoint = point;
                }
            }

            Point currentPoint = startPoint;
            do
            {
                convexHull.Add(currentPoint);
                Point nextPoint = inputPoints[0];

                for (int i = 1; i < inputPoints.Count; i++)
                {
                    int orientation = Orientation(currentPoint, nextPoint, inputPoints[i]);
                    if (orientation == -1)
                    {
                        nextPoint = inputPoints[i];
                    }
                    else if (orientation == 0)
                    {
                        // Handle collinear points by choosing the farthest one
                        int distanceA = DistanceSquared(currentPoint, nextPoint);
                        int distanceB = DistanceSquared(currentPoint, inputPoints[i]);
                        if (distanceB > distanceA)
                        {
                            nextPoint = inputPoints[i];
                        }
                    }
                }

                currentPoint = nextPoint;
            } while (currentPoint != startPoint);

            return convexHull;
        }

        private int Orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0)
            {
                return 0; // Collinear
            }
            return (val > 0) ? 1 : -1; // 1 for clockwise, -1 for counterclockwise
        }

        private int DistanceSquared(Point p1, Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return dx * dx + dy * dy;
        }

        private void grahamscanbutton_Click(object sender, EventArgs e)
        {
            timebox.Text = "";
            if (points.Count < 3)
            {
                var result = MessageBox.Show("Less than 3 points are not acceptable!", "Error");
                convexHull.Clear();
                drawEdges = false;
            }
            else
            {
                drawEdges = true;
                convexHull = GrahamScan(points);
                currentEdgeIndex = -1;
                animationTimer.Start();
            }
        }
        private List<Point> GrahamScan(List<Point> inputPoints)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            int n = inputPoints.Count;

            if (n < 3)
            {
                // Convex hull not possible with less than 3 points
                return inputPoints;
            }

            // Find the bottom-most point by comparing y coordinate and x coordinate
            Point startPoint = inputPoints[0];
            for (int i = 1; i < n; i++)
            {
                if (inputPoints[i].Y > startPoint.Y || (inputPoints[i].Y == startPoint.Y && inputPoints[i].X > startPoint.X))
                {
                    startPoint = inputPoints[i];
                }
            }

            // Sort the remaining points by polar angle in counterclockwise order around startPoint
            List<Point> sortedPoints = inputPoints
                .Where(p => p != startPoint)
                .OrderBy(p => -Math.Atan2(p.Y - startPoint.Y, p.X - startPoint.X))
                .ToList();

            // Initialize the convex hull with the first two sorted points
            List<Point> convexHull = new List<Point>();
            convexHull.Add(startPoint);
            convexHull.Add(sortedPoints[0]);

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
            timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms" + Environment.NewLine + "O(nlogn)";
            // Stack for visualization
            List<string> grahamScanStackOperations = new List<string>();
            grahamScanStackOperations.Add($"Push ({sortedPoints[0].X}, {sortedPoints[0].Y})");
            grahamScanStackOperations.Add($"Push ({startPoint.X}, {startPoint.Y})");
           

            // Visualize the initial stack
            DrawStackInPictureBox(stackPictureBox, grahamScanStackOperations, inputPoints);
           

            // Iterate through the sorted points to build the convex hull
            for (int i = 1; i < n - 1; i++)
            {
                while (convexHull.Count > 1 && Orientationg(convexHull[convexHull.Count - 2], convexHull.Last(), sortedPoints[i]) != 1)
                {
                    convexHull.RemoveAt(convexHull.Count - 1);
                    grahamScanStackOperations.Insert(0, "Pop");
                    // Update the stack visualization after popping
                    DrawStackInPictureBox(stackPictureBox, grahamScanStackOperations, inputPoints);
                    currentEdgeIndex++;

                    if (currentEdgeIndex >= convexHull.Count)
                    {
                        // Animation complete, stop the timer after drawing the last edge
                        animationTimer.Stop();
                        currentEdgeIndex = convexHull.Count - 1; // Ensure it's not out of bounds
                    }

                    pictureBox1.Invalidate();
                }

                convexHull.Add(sortedPoints[i]);
                grahamScanStackOperations.Insert(0, $"Push ({sortedPoints[i].X}, {sortedPoints[i].Y})");
                // Update the stack visualization after pushing
                DrawStackInPictureBox(stackPictureBox, grahamScanStackOperations, inputPoints);
               


            }

            return convexHull;
        }


        private void DrawStackInPictureBox(PictureBox pictureBox, List<string> stackOperations, List<Point> inputPoints)
        {
            // Create a Bitmap for drawing the stack visualization
            Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Clear the bitmap
                g.Clear(Color.White);
                 //int x = 10;
                // int y = pictureBox.Height - operationHeight * (stackOperations.Count + 1);
                // Calculate the height of each operation considering the number of operations and text size
                int textHeight = (int)g.MeasureString("Test", new Font("Arial", 10)).Height; // Measure a sample text height
                int operationHeight = textHeight + 5; // 5 is additional space to separate operations visually

                // Calculate the initial y-coordinate for the bottom of the PictureBox
                int y = pictureBox.Height - operationHeight * (stackOperations.Count + 1); // Add extra space for the top margin

                // Define starting position to draw the stack visualization
                int x = 10;

                // Draw each stack operation
                
                foreach (string operation in stackOperations)
                {
                    if (!operation.StartsWith("Push"))
                    {
                        //g.DrawString(operation, new Font("Arial", 10), Brushes.Blue, x, y);
                        //Rectangle rect = new Rectangle(x, y, 120, textHeight + 5); // Rectangle dimensions
                        //g.DrawRectangle(Pens.Black, rect); // Draw a rectangle around each operation
                        int crossX = x; // Adjust this value to place the cross mark properly
                       int crossY = y - operationHeight / 2; // Adjust this value to place the cross mark properly


                        using (Pen redPen = new Pen(Color.Red, 3)) // Set the width to 2
                        {
                            g.DrawLine(redPen, x, y, x + 120, y + (textHeight + 5));
                        }
                        // y += operationHeight; // Move up for the next operation
                    }
                    // Extracting values within parentheses
                    else
                    {
                        int startIndex = operation.IndexOf('(');
                        int endIndex = operation.IndexOf(')');
                        string pointCoords = operation.Substring(startIndex + 1, endIndex - startIndex - 1);

                        string[] coordinates = pointCoords.Split(',');
                        int X = int.Parse(coordinates[0].Trim());
                        int Y = int.Parse(coordinates[1].Trim());

                        // Adjust the Y-coordinate to display correctly
                        int displayedY = pictureBox1.Height - Y;


                        g.DrawString($"Push({X}, {displayedY})", new Font("Arial", 10), Brushes.Black, x, y);


                      

                        Rectangle rect = new Rectangle(x, y, 120, textHeight + 5); // Rectangle dimensions
                        g.DrawRectangle(Pens.Black, rect); // Draw a rectangle around each operation
                        y += operationHeight; // Move up for the next operation
                    }

                }


                // Display points corresponding to the stack operations
                foreach (string operation in stackOperations)
                {
                    if (operation.StartsWith("Push"))
                    {
                        int startIndex = operation.IndexOf('(');
                        int endIndex = operation.IndexOf(')');
                        string pointCoords = operation.Substring(startIndex + 1, endIndex - startIndex - 1);

                        string[] coordinates = pointCoords.Split(',');
                        int pointX = int.Parse(coordinates[0]);
                        int pointY = int.Parse(coordinates[1]);

                        // Adjust the Y-coordinate to display correctly
                        int displayedY = pictureBox.Height - pointY; // Flip Y-coordinate to display properly

                        // Draw the point or perform any other operation with the coordinates if needed
                        // Here, you can draw the point or perform other actions with adjusted coordinates
                    }
                }
            }

            // Display the bitmap in the PictureBox
            
            pictureBox.Image = bmp;
        }



        private int Orientationg(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0)
            {
                return 0; // Collinear
            }
            return (val > 0) ? 1 : -1; // 1 for clockwise, -1 for counterclockwise
        }



        private void jarvisMarchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            pictureBox2.Hide();
            stackPictureBox.Hide();
            Reset.Show();
            grahamscanbutton.Hide();
            bruteforcebutton.Hide();
            researchhullbutton.Hide();
            quickhullbutton.Hide();
            CalculateConvexHullButton.Show();
            pictureBox1.Show();
            convexHull.Clear();
            pictureBox1.Image = null;

        }

        private void grahamScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            pictureBox2.Hide();
            Reset.Show();
            stackPictureBox.Show();
            stackPictureBox.Image = null;
            CalculateConvexHullButton.Hide();
            bruteforcebutton.Hide();
            quickhullbutton.Hide();
            researchhullbutton.Hide();
            grahamscanbutton.Show();
            pictureBox1.Show();
            convexHull.Clear();
            pictureBox1.Image = null;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

            int pictureBoxHeight = this.ClientSize.Height - grahamscanbutton.Height - 40; // You can adjust the 40 value as needed

            // Update the PictureBox size to the calculated height
            pictureBox1.Size = new Size(this.ClientSize.Width, pictureBoxHeight);
            pictureBox2.Size = new Size(this.ClientSize.Width, pictureBoxHeight);


            // Set the location of the buttons to the bottom left corner
            grahamscanbutton.Location = new Point(this.ClientSize.Width - grahamscanbutton.Width - 10, this.ClientSize.Height - grahamscanbutton.Height - 10);
            CalculateConvexHullButton.Location = new Point(this.ClientSize.Width - CalculateConvexHullButton.Width - 10, this.ClientSize.Height - CalculateConvexHullButton.Height - 10);
            Reset.Location = new Point(this.ClientSize.Width - Reset.Width - 160, this.ClientSize.Height - Reset.Height - 10);
            bruteforcebutton.Location = new Point(this.ClientSize.Width - bruteforcebutton.Width - 10, this.ClientSize.Height - bruteforcebutton.Height - 10);
            quickhullbutton.Location = new Point(this.ClientSize.Width - quickhullbutton.Width - 10, this.ClientSize.Height - quickhullbutton.Height - 10);
            researchhullbutton.Location = new Point(this.ClientSize.Width - researchhullbutton.Width - 10, this.ClientSize.Height - researchhullbutton.Height - 10);
            stackPictureBox.Location = new Point(this.ClientSize.Width - stackPictureBox.Width, 0);
            stackPictureBox.Size = new Size(400, this.ClientSize.Height - 70); // Adjust the width as needed
            stackPictureBox.Location = new Point(this.ClientSize.Width - stackPictureBox.Width, stackPictureBox.Location.Y  + 20);
            linecheck1.Location = new Point(this.ClientSize.Width - linecheck1.Width - 30, this.ClientSize.Height - linecheck1.Height - 10);
            linecheck2.Location = new Point(this.ClientSize.Width - linecheck2.Width - 30, this.ClientSize.Height - linecheck2.Height - 10);
            linecheck3.Location = new Point(this.ClientSize.Width - linecheck3.Width - 30, this.ClientSize.Height - linecheck3.Height - 10);

            check1.Location = new Point(40, this.ClientSize.Height - check1.Height - 10);
            timebox.Location = new Point(500, ClientRectangle.Height - timebox.Height - 10);



        }

        private void Reset_Click(object sender, EventArgs e)
        {
            points.Clear();
            convexHull.Clear();
            pictureBox1.Image = null;
            stackPictureBox.Image = null;
            pictureBox2.Image = null;
            lines.Clear();
            check1.Text = "";
            timebox.Text = "";
        }

        private void bruteforcebutton_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
       
            if (points.Count < 3)
            {
                MessageBox.Show("Less than 3 points are not acceptable!", "Error");
                convexHull.Clear();
                drawEdges = false;
            }
            else
            {
                drawEdges = true;
                stopwatch.Start();
                convexHull = BruteForceConvexHull(points);
                stopwatch.Stop();
                currentEdgeIndex = -1;
                animationTimer.Start();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms" + Environment.NewLine  + "O(n^3)";
            }

        }


        private bool IsOnSameSide(Point p1, Point p2, Point a, Point b)
        {
            int cp1 = (b.X - a.X) * (p1.Y - a.Y) - (b.Y - a.Y) * (p1.X - a.X);
            int cp2 = (b.X - a.X) * (p2.Y - a.Y) - (b.Y - a.Y) * (p2.X - a.X);
            return cp1 * cp2 >= 0;
        }

        private bool IsOnLeft(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            return val > 0;
        }

        private List<Point> BruteForceConvexHull(List<Point> inputPoints)
        {
            int n = inputPoints.Count;
            if (n < 3)
                return new List<Point>(inputPoints); // Return all points if less than 3

            List<Point> convexHull = new List<Point>();
            int a = 0;
            while(a < 9999999)
            {
                a++;
            }
            a = 0;
            while (a < 9999999)
            {
                a++;
            }

            // Find the leftmost point
            int leftMost = 0;
            for (int i = 1; i < n; i++)
            {
                if (inputPoints[i].X < inputPoints[leftMost].X)
                    leftMost = i;
            }

            int p = leftMost, q;
            do
            {
                convexHull.Add(inputPoints[p]);
                q = (p + 1) % n;

                for (int i = 0; i < n; i++)
                {
                    if (Orientation(inputPoints[p], inputPoints[i], inputPoints[q]) == -1)
                        q = i;
                }

                p = q;
            }
            while (p != leftMost);

            return convexHull;
        }

        private void bruteforceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            pictureBox2.Hide();
            Reset.Show();
            stackPictureBox.Hide();
            grahamscanbutton.Hide();
            CalculateConvexHullButton.Hide();
            quickhullbutton.Hide();
            researchhullbutton.Hide();
            bruteforcebutton.Show();
            pictureBox1.Show();
            convexHull.Clear();
            pictureBox1.Image = null;
        }


        public List<Point> FindConvexHull(List<Point> points)
        {
            if (points.Count < 3)
            {
                return points; // Convex hull not possible with less than 3 points
            }

            List<Point> convexHull = new List<Point>();

            // Find the min and max x-coordinate points to form the initial line segment
            int minIdx = 0;
            int maxIdx = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].X < points[minIdx].X)
                {
                    minIdx = i;
                }
                if (points[i].X > points[maxIdx].X)
                {
                    maxIdx = i;
                }
            }

            Point minPoint = points[minIdx];
            Point maxPoint = points[maxIdx];

            convexHull.Add(minPoint);
            convexHull.Add(maxPoint);

            // Find the points on the left and right of the line formed by minPoint and maxPoint
            List<Point> leftSet = new List<Point>();
            List<Point> rightSet = new List<Point>();

            for (int i = 0; i < points.Count; i++)
            {
                if (FindSide(minPoint, maxPoint, points[i]) == 1)
                {
                    leftSet.Add(points[i]);
                }
                else if (FindSide(minPoint, maxPoint, points[i]) == -1)
                {
                    rightSet.Add(points[i]);
                }
            }

            // Recursively find the convex hull for the left and right sets
            FindHull(leftSet, minPoint, maxPoint, convexHull);
            FindHull(rightSet, maxPoint, minPoint, convexHull);
            convexHull.Sort((a, b) => Orientation(minPoint, a, b).CompareTo(0));

            return convexHull;
        }

        private void FindHull(List<Point> points, Point p1, Point p2, List<Point> convexHull)
        {
            int ind = -1;
            int maxDist = 0;

            for (int i = 0; i < points.Count; i++)
            {
                int temp = LineDist(p1, p2, points[i]);
                int pointSide = FindSide(p1, p2, points[i]);

                if (pointSide == 1 && temp > maxDist)
                {
                    ind = i;
                    maxDist = temp;
                }
            }

            if (ind == -1)
            {
                // Base case: no points found on this side of the line
                return;
            }

            Point furthestPoint = points[ind];

            convexHull.Add(furthestPoint);

            List<Point> pointsInsideTriangle = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                int orientation = Orientation(p1, p2, furthestPoint, points[i]);
                if (orientation == 0)
                {
                    pointsInsideTriangle.Add(points[i]);
                }
            }

            foreach (var point in pointsInsideTriangle)
            {
                points.Remove(point);
            }

            FindHull(points, p1, furthestPoint, convexHull);
            FindHull(points, furthestPoint, p2, convexHull);

            // Sorting the convex hull based on closest points
        }


        private int Orientation(Point p1, Point p2, Point p3, Point p)
        {
            int val = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            if (val == 0) return 0; // colinear
            return (val > 0) ? 1 : 2; // clock or counterclockwise
        }

        private int FindSide(Point p1, Point p2, Point p)
        {
            int val = (p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X);
            return Math.Sign(val);
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private int LineDist(Point p1, Point p2, Point p)
        {
            return Math.Abs((p.Y - p1.Y) * (p2.X - p1.X) - (p2.Y - p1.Y) * (p.X - p1.X));
        }
        private void quickHullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            pictureBox2.Hide();
            Reset.Show();
            grahamscanbutton.Hide();
            stackPictureBox.Hide();
            bruteforcebutton.Hide();
            quickhullbutton.Show();
            researchhullbutton.Hide();
            CalculateConvexHullButton.Hide();
            pictureBox1.Show();
            convexHull.Clear();
            pictureBox1.Image = null;
        }

        private void quickhullbutton_Click(object sender, EventArgs e)
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
            
            if (points.Count < 3)
            {
                var result = MessageBox.Show("Less than 3 points are not acceptable!", "Error");
                convexHull.Clear();
                drawEdges = false;
            }
            else
            {
                drawEdges = true;
                stopwatch.Start();
                convexHull = FindConvexHull(points);
                stopwatch.Stop();
                currentEdgeIndex = -1;
                animationTimer.Start();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms" + Environment.NewLine + "O(n^2)";
            }
        }

        public List<Point> ResearchHull(List<Point> points)
        {
            int n = points.Count;

            // Find the leftmost point
            Point Leftmost = points[0];
            for (int i = 1; i < n; i++)
            {
                if (points[i].X < Leftmost.X || (points[i].X == Leftmost.X && points[i].Y < Leftmost.Y))
                {
                    Leftmost = points[i];
                }
            }

            // Sort the points by polar angle in counterclockwise order relative to the leftmost point
            points.Sort((a, b) =>
            {
                int orientation = Orientation(Leftmost, a, b);
                if (orientation == 0)
                {
                    return Distance(Leftmost, a).CompareTo(Distance(Leftmost, b));
                }
                return orientation;
            });

            // Initialize the stack to hold the hull points
            Stack<Point> hull = new Stack<Point>();
            hull.Push(points[0]);

            // Traverse the sorted points to construct the convex hull
            for (int i = 1; i < n; i++)
            {
                Point currentPoint = points[i];
                while (hull.Count >= 2 && Orientation(hull.ElementAt(1), hull.Peek(), currentPoint) != 1)
                {
                    hull.Pop();
                }
                hull.Push(currentPoint);
            }

            // Add the initial hull to a list
            List<Point> initialHull = hull.ToList();

            // Create a new hull stack
            hull.Clear();
            hull.Push(points[0]);

            // Traverse the points in reverse order to ensure all boundary points are included
            for (int i = n - 1; i >= 0; i--)
            {
                Point currentPoint = points[i];
                while (hull.Count >= 2 && Orientation(hull.ElementAt(1), hull.Peek(), currentPoint) != 1)
                {
                    hull.Pop();
                }
                hull.Push(currentPoint);
            }

            // Remove any duplicates in the hull stack
            HashSet<Point> uniquePoints = new HashSet<Point>(hull);

            // Add any missing points from the initial hull
            foreach (var point in initialHull)
            {
                uniquePoints.Add(point);
            }

            return uniquePoints.ToList();
        }


        private Point[] FindExtremePoints(Point[] P, int n)
        {
            var Leftmost = P.OrderBy(p => p.X).First();
            var Rightmost = P.OrderBy(p => p.X).Last();
            var Uppermost = P.OrderBy(p => p.Y).First();
            var Lowermost = P.OrderBy(p => p.Y).Last();

            return new Point[] { Leftmost, Rightmost, Uppermost, Lowermost };
        }

        private void researchhullbutton_Click(object sender, EventArgs e) //menustrip
        {
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Hide();
            pictureBox2.Hide();
            Reset.Show();
           
            stackPictureBox.Hide();
            grahamscanbutton.Hide();
            CalculateConvexHullButton.Hide();
            quickhullbutton.Hide();
            bruteforcebutton.Hide();
            researchhullbutton.Show();
            pictureBox1.Show();
            convexHull.Clear();
            pictureBox1.Image = null;
        }

        private void researchhullbutton_Click_1(object sender, EventArgs e)
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
            
            if (points.Count < 3)
            {
                var result = MessageBox.Show("Less than 3 points are not acceptable!", "Error");
                convexHull.Clear();
                drawEdges = false;
            }
            else
            {
                drawEdges = true;
                stopwatch.Start();
                convexHull = ResearchHull(points);
                stopwatch.Stop();

                currentEdgeIndex = -1;
                animationTimer.Start();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms" + Environment.NewLine + "O(nlogn)";
            }
            //https://www.researchgate.net/publication/236898954_A_new_approach_to_compute_convex_hull

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void typeOfConvexHullSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
           
        }

        private void lineIntersectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void vectorCrossProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stackPictureBox.Hide();
            grahamscanbutton.Hide();
            CalculateConvexHullButton.Hide();
            quickhullbutton.Hide();
            bruteforcebutton.Hide();
            researchhullbutton.Hide();
            Reset.Show();
            check1.Text = "";
            pictureBox2.Image = null;
            pictureBox1.Hide();
            pictureBox2.Show();
            linecheck1.Show();
            linecheck2.Hide();
            linecheck3.Hide();

        }

        private void parametricEquationOfLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stackPictureBox.Hide();
            grahamscanbutton.Hide();
            CalculateConvexHullButton.Hide();
            quickhullbutton.Hide();
            bruteforcebutton.Hide();
            researchhullbutton.Hide();
            Reset.Show();
            pictureBox1.Hide();
            pictureBox2.Show();
            pictureBox2.Image = null;
            check1.Text = "";
            linecheck1.Hide();
            linecheck2.Show();
            linecheck3.Hide();
        }

        private void orientationOfPointsToolStripMenuItem_Click(object sender, EventArgs e) //Research
        {
            stackPictureBox.Hide();
            grahamscanbutton.Hide();
            CalculateConvexHullButton.Hide();
            quickhullbutton.Hide();
            bruteforcebutton.Hide();
            researchhullbutton.Hide();
            Reset.Show();
            pictureBox1.Hide();
            pictureBox2.Show();
            pictureBox2.Image = null;
            check1.Text = "";
            linecheck1.Hide();
            linecheck2.Hide();
            linecheck3.Show();
            //https://www.cise.ufl.edu/~sitharam/COURSES/CG/kreveldintrolinesegment.pdf
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            lines.Add(e.Location);
            pictureBox2.Invalidate();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw all the points in the list
            foreach (Point point in lines)
            {
                int radius = 5;
                g.FillEllipse(Brushes.Red, point.X - radius, point.Y - radius, 2 * radius, 2 * radius);
                g.DrawString($"({point.X}, {pictureBox2.Height - point.Y})", new Font("Arial", 12), Brushes.White, point.X + radius, point.Y - radius);
            }

            // Draw lines between every two consecutive points in the list
            if (lines.Count > 1)
            {
                Pen pen = new Pen(Color.Blue, 2);
                for (int i = 0; i < lines.Count - 1; i += 2)
                {
                    g.DrawLine(pen, lines[i], lines[i + 1]);
                }
                pen.Dispose();
            }
        }

        private void linecheck_Click(object sender, EventArgs e) 
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            bool checker = DoIntersect(lines);
            check1.Show();
            check1.Text = "";
            if (checker)
            {
                Point p1 = lines[0];
                Point p2 = lines[1];
                Point p3 = lines[2];
                Point p4 = lines[3];
                int displine1_y = pictureBox2.Height - p1.Y;
                int displine2_y = pictureBox2.Height - p2.Y;
                int displine3_y = pictureBox2.Height - p3.Y;
                int displine4_y = pictureBox2.Height - p4.Y;
                check1.Text = "Lines (" + p1.X + "," + displine1_y  + "), (" + p2.X + "," + displine2_y 
                    + ")" + " and " + "(" + p3.X + "," + displine3_y +  "), (" + p4.X + "," + displine4_y + ")" +" intersect!";
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms";
            }
            else
            {
                check1.Text = "Lines do not intersect!";
            }
        }
        private bool DoIntersect(List<Point> points)
        {
            double test1, test2;
            Point p1 = points[0];
            Point p2 = points[1];
            Point p3 = points[2];
            Point p4 = points[3];

            test1 = ccw(p1, p2, p3) * ccw(p1, p2, p4);
            test2 = ccw(p3, p4, p1) * ccw(p3, p4, p2);

            return (test1 <= 0) && (test2 <= 0);
        }

        private double ccw(Point a, Point b, Point c)
        {
            int val = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);
            if (val == 0) return 0;  // colinear
            return (val > 0) ? 1 : -1; // clock or counterclock wise
        }


        private void linecheck2_Click(object sender, EventArgs e) //2
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
           
            check1.Show();
            bool checker = Intersect(lines);
            check1.Text = "";
            if (checker)
            {
                Point p1 = lines[0];
                Point p2 = lines[1];
                Point p3 = lines[2];
                Point p4 = lines[3];
                int displine1_y = pictureBox2.Height - p1.Y;
                int displine2_y = pictureBox2.Height - p2.Y;
                int displine3_y = pictureBox2.Height - p3.Y;
                int displine4_y = pictureBox2.Height - p4.Y;
                check1.Text = "Lines (" + p1.X + "," + displine1_y + "), (" + p2.X + "," + displine2_y
                    + ")" + " and " + "(" + p3.X + "," + displine3_y + "), (" + p4.X + "," + displine4_y + ")" + " intersect!";
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms";
            }
            else
            {
                check1.Text = "Lines do not intersect!";
            }
        }

        private bool Intersect(List<Point> points)
        {
            // Extracting points for two separate line segments
            Point p1 = points[0];
            Point q1 = points[1];
            Point p2 = points[2];
            Point q2 = points[3];

            // Checking for intersection between two line segments
            bool intersect = Intersect(p1, q1, p2, q2);
            return intersect;
        }

        private bool Intersect(Point p1, Point q1, Point p2, Point q2)
        {
            

            // Find orientations for all endpoints of both sets of points
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case: segments intersect if orientations are different
            if (o1 != o2 && o3 != o4)
                return true;

            // Special cases: segments are collinear and overlapping
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // No intersection
        }

        // Function to check if point q lies on segment pr
        private bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }


        private void linecheck3_Click(object sender, EventArgs e)
        {
            timebox.Text = "";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
           
            bool checker = FindIntersections(lines);
            check1.Show();
            check1.Text = "";
            if (checker)
            {
                Point p1 = lines[0];
                Point p2 = lines[1];
                Point p3 = lines[2];
                Point p4 = lines[3];
                int displine1_y = pictureBox2.Height - p1.Y;
                int displine2_y = pictureBox2.Height - p2.Y;
                int displine3_y = pictureBox2.Height - p3.Y;
                int displine4_y = pictureBox2.Height - p4.Y;
                check1.Text = "Lines (" + p1.X + "," + displine1_y + "), (" + p2.X + "," + displine2_y
                    + ")" + " and " + "(" + p3.X + "," + displine3_y + "), (" + p4.X + "," + displine4_y + ")" + " intersect!";
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double elapsedTimeInSeconds = elapsedTime.TotalMilliseconds;
                timebox.Text = "Elapsed time: " + elapsedTimeInSeconds + "ms";
            }
            else
            {
                check1.Text = "Lines do not intersect!";
            }
        }

        private bool FindIntersections(List<Point> points)
        {
            bool intersectionsFound = false;

            for (int i = 0; i < points.Count - 3; i += 2)
            {
                for (int j = i + 2; j < points.Count - 1; j += 2)
                {
                    if (Intersect3(points[i], points[i + 1], points[j], points[j + 1]))
                    {
                        Point intersection = GetIntersection(points[i], points[i + 1], points[j], points[j + 1]);
                        if (intersection.X != int.MinValue && intersection.Y != int.MinValue)
                        {
                            intersectionsFound = true;
                            break; // Found an intersection, no need to continue checking
                        }
                    }
                }
                if (intersectionsFound) break; // Break the outer loop if intersections were found
            }

            return intersectionsFound;
        }


        private bool Intersect3(Point p1, Point q1, Point p2, Point q2)
        {
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            return (o1 != o2 && o3 != o4);
        }

      

        private Point GetIntersection(Point p1, Point q1, Point p2, Point q2)
        {
            double a1 = q1.Y - p1.Y;
            double b1 = p1.X - q1.X;
            double c1 = a1 * p1.X + b1 * p1.Y;

            double a2 = q2.Y - p2.Y;
            double b2 = p2.X - q2.X;
            double c2 = a2 * p2.X + b2 * p2.Y;

            double determinant = a1 * b2 - a2 * b1;
            if (Math.Abs(determinant) < double.Epsilon) // Parallel or coincident lines
                return new Point(int.MinValue, int.MinValue);

            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;

            return new Point((int)x, (int)y); // Assuming integer coordinates
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
