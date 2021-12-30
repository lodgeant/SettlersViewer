using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;



namespace Generator
{
    public partial class AStarScreen : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static int[,] ObjectMap = new int[0, 0];
        private static Bitmap MapBitmap = null;       
        private static int squareSize = 30;        
        private static float fontSize = 6;
        

        public AStarScreen()
        {
            InitializeComponent();
            try
            {
                // ** Post header **            
                string[] versionArray = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
                this.Text = "A Star Screen";
                this.Text += " | " + "v" + versionArray[0] + "." + versionArray[1] + "." + versionArray[2];

                #region ** FORMAT SUMMARIES **
                    string[] DGnames = new string[] { "dgCoreObjectMap", "dgRealObjectMap" };
                    foreach (String dgName in DGnames)
                    {
                        DataGridView dgv = (DataGridView)(this.Controls.Find(dgName, true)[0]);
                        dgv.AllowUserToAddRows = false;
                        dgv.AllowUserToDeleteRows = false;
                        dgv.AllowUserToOrderColumns = true;
                        dgv.MultiSelect = true;
                        dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
                        dgv.EnableHeadersVisualStyles = false;
                        dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
                        //dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                        dgv.ColumnHeadersHeight = 30;
                    }
                #endregion

                #region ** ADD MAIN HEADER LINE TOOLSTRIP ITEMS **
                toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                btnExit,
                                toolStripSeparator1,
                                btnRefreshScreen,
                                toolStripSeparator4,
                                lblSquareSize,
                                fldSquareSize,
                                toolStripSeparator5,
                                btnLoadMapFromCode,
                                toolStripSeparator3,
                                fldBitmapFilePath,
                                btnSelectBitmapFile,
                                btnLoadMapFromBitmap,
                                toolStripSeparator2,
                                lblMapFactor,
                                fldMapFactor,
                                lblTopLeft,
                                fldTopLeft,                                
                                lblMapWidth,
                                fldMapWidth,
                                lblMapHeight,
                                fldMapHeight,
                                toolStripSeparator6,

                                lblUnityTopLeft,
                                fldUnityTopLeft,

                                lblUnityMapWidth,
                                fldUnityMapWidth,
                                lblUnityMapHeight,
                                fldUnityMapHeight
                                });
                #endregion

                #region ** ADD 2ND LINE **
                    toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {                               
                                    fldPointType,
                                    lblStartPoint,
                                    fldStartPoint,
                                    lblTargetPoint,
                                    fldTargetPoint,
                                    btnGetRoute,
                                    toolStripSeparator7,
                                    new ToolStripControlHost(chkAllowDiagonals),
                                    //new ToolStripControlHost(chkShowCoords),
                                    new ToolStripControlHost(chkMarkNodes),
                                    lblLastSelectedPoint,
                                    fldLastSelectedPoint,

                                    toolStripSeparator8,
                                    fldLoggingXMLFilePath,
                                    btnSelectLoggingFile,
                                    btnLoadLoggingEntry
                                    });
                #endregion

                // ** Clear initial labels **
                lblCoreSummaryStatus.Text = "";
                lblRealSummaryStatus.Text = "";                
                lblBitmapSummaryStatus.Text = "";
                fldSquareSize.Text = squareSize.ToString();

                //fldBitmapFilePath.Text = @"C:\SETTLERS STUFF\MAP3.png";
                //fldTopLeft.Text = "(-40, 25)";
            }
            catch (Exception ex)
            {               
                MessageBox.Show(ex.Message);
            }
        }

        #region ** BUTTON FUNCTIONS **

            private void btnExit_Click(object sender, EventArgs e)
            {
                Dispose();
            }

            private void btnLoadMapFromCode_Click(object sender, EventArgs e)
            {
                LoadMapFromCode();
                RefreshScreen();
            }

            private void btnRefreshScreen_Click(object sender, EventArgs e)
            {
                RefreshScreen();
            }

            private void btnGetRoute_Click(object sender, EventArgs e)
            {
                GetRoute();
            }

            private void btnLoadMapFromBitmap_Click(object sender, EventArgs e)
            {
                LoadMapFromBitmap();
            }

            private void dgCoreObjectMap_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    // ** Get variables **
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    Point corePoint = new Point(e.ColumnIndex, e.RowIndex);
                    Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                    double mapFactor = double.Parse(fldMapFactor.Text);
                    double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                    double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                    // Post label details **
                    lblCoreSummaryStatus.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
            private void dgRealObjectMap_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    // ** Get variables **
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    Point corePoint = new Point(e.ColumnIndex, e.RowIndex);
                    Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                    double mapFactor = double.Parse(fldMapFactor.Text);
                    double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                    double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                    // Post label details **
                    lblRealSummaryStatus.Text = "Real={" + realPoint.X + "," + realPoint.Y + "}  |  Core={" + corePoint.X + "," + corePoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
            private void dgCoreObjectMap_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    // ** Get variables **
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    Point corePoint = new Point(e.ColumnIndex, e.RowIndex);
                    Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                    double mapFactor = double.Parse(fldMapFactor.Text);
                    double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                    double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                    // Post label details **               
                    fldLastSelectedPoint.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
            private void dgRealObjectMap_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    // ** Get variables **
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    Point corePoint = new Point(e.ColumnIndex, e.RowIndex);
                    Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                    double mapFactor = double.Parse(fldMapFactor.Text);
                    double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                    double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                    // Post label details **                
                    fldLastSelectedPoint.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


            private void panel1_MouseMove(object sender, MouseEventArgs e)
            {
                try
                {
                    if (MapBitmap != null)
                    {
                        int mapWidth = int.Parse(fldMapWidth.Text);
                        int mapHeight = int.Parse(fldMapHeight.Text);
                        Point corePoint = new Point(e.X, e.Y);
                        if (corePoint.X < mapWidth && corePoint.Y < mapHeight)
                        {
                            // ** Get variables **
                            string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                            Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                            Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                            double mapFactor = double.Parse(fldMapFactor.Text);
                            double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                            double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                            // Post label details **
                            lblBitmapSummaryStatus.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";
                        }
                        else
                        {
                            lblBitmapSummaryStatus.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
            private void fldSquareSize_Leave(object sender, EventArgs e)
                {
                    try
                    {
                        // ** Update square size **
                        squareSize = int.Parse(fldSquareSize.Text);

                        // ** Update column/row widths/heights **                
                        for (int a = 0; a < dgCoreObjectMap.Columns.Count; a++)
                        {
                            dgCoreObjectMap.Columns[a].Width = squareSize;
                            //dgRealObjectMap.Columns[a].Width = squareSize;
                        }
                        for (int a = 0; a < dgCoreObjectMap.Rows.Count; a++)
                        {
                            dgCoreObjectMap.Rows[a].Height = squareSize;
                            //dgRealObjectMap.Rows[a].Height = squareSize;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            private void dgCoreObjectMap_MouseLeave(object sender, EventArgs e)
            {
                lblCoreSummaryStatus.Text = "";
            }

            private void dgRealObjectMap_MouseLeave(object sender, EventArgs e)
            {
                lblRealSummaryStatus.Text = "";
            }

            private void panel1_MouseLeave(object sender, EventArgs e)
            {
                lblBitmapSummaryStatus.Text = "";
            }

            private void btnLoadLoggingEntry_Click(object sender, EventArgs e)
            {
                LoadLoggingEntry();
            }

        private void btnSelectBitmapFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"C:\SETTLERS STUFF\Debug";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fldBitmapFilePath.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSelectLoggingFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"C:\SETTLERS STUFF\Debug";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fldLoggingXMLFilePath.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        #endregion

        #region ** MAIN FUNCTIONS **

            private void LoadMapFromCode()
            {
                try
                {
                    // ** Set field values **
                    fldPointType.Text = "REAL";
                    fldTopLeft.Text = "(0, 0)";
                    fldMapWidth.Text = "8";
                    fldMapHeight.Text = "7";
                    fldStartPoint.Text = "(2, -3)";
                    fldTargetPoint.Text = "(6, -3)";
                    fldMapFactor.Text = "1";

                    // ** Get variables **
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    int MapWidth = int.Parse(fldMapWidth.Text);
                    int MapHeight = int.Parse(fldMapHeight.Text);

                    // ** Generate Object Map (manual) **                              
                    ObjectMap = new int[MapWidth, MapHeight];
                    for (int a = 0; a < MapWidth; a++)
                    {
                        for (int b = 0; b < MapHeight; b++)
                        {
                            ObjectMap[a, b] = 0;
                        }
                    }
                    Point obPoint;
                    obPoint = Point.ConvertRealPointToCorePoint(new Point(4, -2), topLeft);
                    ObjectMap[obPoint.X, obPoint.Y] = -1;
                    obPoint = Point.ConvertRealPointToCorePoint(new Point(4, -3), topLeft);
                    ObjectMap[obPoint.X, obPoint.Y] = -1;
                    obPoint = Point.ConvertRealPointToCorePoint(new Point(4, -4), topLeft);
                    ObjectMap[obPoint.X, obPoint.Y] = -1;

                    // ** Generate bitmap from ObjectMap **
                    MapBitmap = GlobalMap.GenerateBitMapFromObjectMap(ObjectMap);
                
                    // ** Refresh Screen **
                    RefreshScreen();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        
            private void LoadMapFromBitmap()
            {
                try
                {
                    // ** Validation **
                    if (fldTopLeft.Text.Equals(""))
                    {
                        throw new Exception("No TopLeft value entered...");
                    }
                    if (fldMapFactor.Text.Equals(""))
                    {
                        throw new Exception("No Map Factor value entered...");
                    }
                
                    // ** Get variables **
                    string[] coords;
                    coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    int mapFactor = int.Parse(fldMapFactor.Text);                
                    Point UnityTopLeft = new Point(topLeft.X / mapFactor, topLeft.Y / mapFactor);
                
                    // ** Load Bitmap into Object Map **
                    using (Bitmap bMap = (Bitmap)Image.FromFile(fldBitmapFilePath.Text))
                    {
                        // ** Generate ObjectMap[,] **
                        ObjectMap = GlobalMap.GenerateObjectMapFromBitMap(bMap);

                        // ** Set bitmap ** 
                        MapBitmap = (Bitmap)bMap.Clone();
                    
                        // ** Set field values **
                        fldMapWidth.Text = bMap.Width.ToString();
                        fldMapHeight.Text = bMap.Height.ToString();                      
                        fldUnityMapWidth.Text = (bMap.Width / mapFactor).ToString();
                        fldUnityMapHeight.Text = (bMap.Height / mapFactor).ToString();
                    }
                    fldUnityTopLeft.Text = "(" + UnityTopLeft.X + "," + UnityTopLeft.Y + ")";
                    fldStartPoint.Text = "";
                    fldTargetPoint.Text = "";
                
                    // ** Refresh Screen **
                    RefreshScreen();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            private void RefreshScreen()
            {
                try
                {
                    // ** Get variables **
                    int MapWidth = ObjectMap.GetLength(0);
                    int MapHeight = ObjectMap.GetLength(1);
                    string[] coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));

                    // ** Clear existing data **
                    dgCoreObjectMap.DataSource = null;
                    dgRealObjectMap.DataSource = null;
                    panel1.BackgroundImage = null;
                
                    // ** Save CoreObjectMap to DataTable **                
                    DataTable CoreObjectMapTable = new DataTable("CoreObjectMapTable", "CoreObjectMapTable");
                    for (int a = 0; a < MapWidth; a++)
                    {
                        CoreObjectMapTable.Columns.Add(a.ToString(), typeof(string));
                    }
                    for (int a = 0; a < MapHeight; a++)
                    {
                        object[] row = new object[CoreObjectMapTable.Columns.Count];
                        //if (chkShowCoords.Checked)
                        //{
                            for (int b = 0; b < MapWidth; b++)
                            {
                                Point corePoint = new Point(b, a);
                                row[b] = "{" + corePoint.X + "," + corePoint.Y + "}";
                            }
                        //}
                        CoreObjectMapTable.Rows.Add(row);
                    }
                    dgCoreObjectMap.DataSource = CoreObjectMapTable;

                    // ** Save RealObjectMap to DataTable **                
                    //DataTable RealObjectMapTable = new DataTable("RealObjectMapTable", "RealObjectMapTable");
                    //for (int a = 0; a < MapWidth; a++)
                    //{
                    //    int xIndex = a + topLeft.X;
                    //    RealObjectMapTable.Columns.Add(xIndex.ToString(), typeof(string));
                    //}
                    //for (int a = 0; a < MapHeight; a++)
                    //{
                    //    object[] row = new object[RealObjectMapTable.Columns.Count];
                    //    if (chkShowCoords.Checked)
                    //    {
                    //        for (int b = 0; b < MapWidth; b++)
                    //        {
                    //            Point corePoint = new Point(b, a);
                    //            Point realPoint = ConvertCorePointToRealPoint(corePoint, topLeft);
                    //            row[b] = "{" + realPoint.X + "," + realPoint.Y + "}";
                    //        }
                    //    }
                    //    RealObjectMapTable.Rows.Add(row);
                    //}
                    //dgRealObjectMap.DataSource = RealObjectMapTable;

                    // ** Update column/row widths/heights **                
                    for (int a = 0; a < MapWidth; a++)
                    {
                        dgCoreObjectMap.Columns[a].Width = squareSize;
                        dgCoreObjectMap.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgCoreObjectMap.Columns[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgCoreObjectMap.Columns[a].DefaultCellStyle.Font = new Font(this.Font.FontFamily, fontSize);
                        //dgRealObjectMap.Columns[a].Width = squareSize;
                        //dgRealObjectMap.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        //dgRealObjectMap.Columns[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        //dgRealObjectMap.Columns[a].DefaultCellStyle.Font = new Font(this.Font.FontFamily, fontSize);
                    }
                    for (int a = 0; a < MapHeight; a++) dgCoreObjectMap.Rows[a].Height = squareSize;
                    //for (int a = 0; a < MapHeight; a++) dgRealObjectMap.Rows[a].Height = squareSize;

                    // ** Add tooltips to each cell - CoreObjectMap **
                    for (int a = 0; a < dgCoreObjectMap.Columns.Count; a++)
                    {
                        for (int b = 0; b < dgCoreObjectMap.Rows.Count; b++)
                        {
                            DataGridViewCell cell = dgCoreObjectMap.Rows[b].Cells[a];
                            Point corePoint = new Point(a, b);
                            cell.ToolTipText = "{" + corePoint.X + "," + corePoint.Y + "}";
                            cell.Tag = "{" + corePoint.X + "," + corePoint.Y + "}";
                        }
                    }

                    // ** Add tooltips to each cell - RealObjectMap **
                    //for (int a = 0; a < dgRealObjectMap.Columns.Count; a++)
                    //{
                    //    for (int b = 0; b < dgRealObjectMap.Rows.Count; b++)
                    //    {
                    //        DataGridViewCell cell = dgRealObjectMap.Rows[b].Cells[a];
                    //        Point corePoint = new Point(a, b);
                    //        Point realPoint = ConvertCorePointToRealPoint(corePoint, topLeft);
                    //        cell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "}";
                    //        cell.Tag = "{" + realPoint.X + "," + realPoint.Y + "}";
                    //    }
                    //}

                    // ** Add obstacles - CoreObjectMap & RealObjectMap **
                    for (int a = 0; a < MapWidth; a++)
                    {
                        for (int b = 0; b < MapHeight; b++)
                        {
                            int walkValue = ObjectMap[a, b];
                            if (walkValue == -1)
                            {
                                Point corePoint = new Point(a, b);
                                dgCoreObjectMap.Rows[corePoint.Y].Cells[corePoint.X].Style.BackColor = Color.Gray;
                                //dgRealObjectMap.Rows[corePoint.Y].Cells[corePoint.X].Style.BackColor = Color.Gray;
                            }
                        }
                    }


                    // ** Post zoomed bitmap image **
                    panel1.BackgroundImage = MapBitmap;
                    //int zoomFactor = 2;
                    //Size newSize = new Size((int)(MapBitmap.Width * zoomFactor), (int)(MapBitmap.Height * zoomFactor));
                    //Bitmap bmp = new Bitmap(MapBitmap, newSize);
                    //panel1.BackgroundImage = bmp;                




                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            private void GetRoute()
            {
                try
                {
                    log.Info("***************************************");
                    log.Info("Get Route - START");

                    // ** Get variables **
                    string[] coords;
                    coords = fldTopLeft.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point topLeft = new Point(int.Parse(coords[0]), int.Parse(coords[1]));                
                    coords = fldStartPoint.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point startPoint = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
                    coords = fldTargetPoint.Text.Replace("(", "").Replace(")", "").Split(',');
                    Point targetPoint = new Point(int.Parse(coords[0]), int.Parse(coords[1]));                
                    Point coreStartPoint = startPoint;
                    Point coreTargetPoint = targetPoint;
                    if (fldPointType.Text.Equals("REAL"))
                    {
                        coreStartPoint = Point.ConvertRealPointToCorePoint(startPoint, topLeft);
                        coreTargetPoint = Point.ConvertRealPointToCorePoint(targetPoint, topLeft);
                    }                
               
                    #region ** Calculate route from Start Point to Target Point **
                        log.Info("Calculate route - START");                    
                        bool routeFound = false;
                        List<Point> routeCorePointList = new List<Point>();
                        List<Node> nodeList_Open = new List<Node>();
                        List<Node> nodeList_Closed = new List<Node>();
                        DateTime overallStartTime = DateTime.Now;
                        int maxRouteCalcWaitSecs = 20;
                        GlobalMap.CalculateRoute(coreStartPoint, coreTargetPoint, ObjectMap, chkAllowDiagonals.Checked, maxRouteCalcWaitSecs, out routeFound, out routeCorePointList, out nodeList_Open, out nodeList_Closed);                    
                        TimeSpan ts = DateTime.Now - overallStartTime;                    
                        log.Info("Calculate route - END");
                    #endregion
                


                    // ##### THE FOLLOWING STUFF JUST RENDERS THE DETAILS THAT HAVE BEEN GENERATED BY THE CALCULATE METHOD #####

                    // ** Convert Core point list to Real point list **
                    //List<Point> routeRealPointList = new List<Point>();
                    //foreach (Point p in routeCorePointList)
                    //{
                    //    routeRealPointList.Add(Point.ConvertCorePointToRealPoint(p, topLeft));
                    //}

                    Bitmap markedBitmap = (Bitmap)MapBitmap.Clone();

                    #region ** Mark CLOSED Nodes **
                        log.Info("Mark CLOSED Nodes - START");
                        if (chkMarkNodes.Checked)
                        {
                            for (int a = 0; a < nodeList_Closed.Count; a++)
                            {
                                Node n = nodeList_Closed[a];
                                Point nodePosition = n.nodePosition;

                                // ** CORE **                  
                                dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightBlue;
                                dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = n.F;
                                DataGridViewCell coreCell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                                coreCell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

                                // ** REAL **
                                //dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightBlue;
                                //dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = nodeList_Closed[a].F;
                                //DataGridViewCell realCell = dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                                //Point corePoint = new Point(nodePosition.X, nodePosition.Y);
                                //Point realPoint = ConvertCorePointToRealPoint(corePoint, topLeft);
                                //realCell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

                                markedBitmap.SetPixel(nodePosition.X, nodePosition.Y, Color.LightBlue);
                            }
                        }
                        log.Info("Mark CLOSED Nodes - END");
                    #endregion

                    #region ** Mark OPEN Nodes **
                        log.Info("Mark OPEN Nodes - START");
                        if (chkMarkNodes.Checked)
                        {
                            for (int a = 0; a < nodeList_Open.Count; a++)
                            {
                                Node n = nodeList_Open[a];
                                Point nodePosition = n.nodePosition;

                                // ** CORE **
                                dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightGray;
                                dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = nodeList_Open[a].F;
                                DataGridViewCell cell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                                cell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

                                // ** REAL **
                                //dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightGray;
                                //dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = n.F;
                                //DataGridViewCell realCell = dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                                //Point corePoint = new Point(nodePosition.X, nodePosition.Y);
                                //Point realPoint = ConvertCorePointToRealPoint(corePoint, topLeft);
                                //realCell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

                                markedBitmap.SetPixel(nodePosition.X, nodePosition.Y, Color.LightGray);
                            }
                        }
                        log.Info("Mark OPEN Nodes - END");
                    #endregion
                
                    #region ** CHECK IF ROUTE FOUND **
                        log.Info("Mark route - START");
                        if (routeFound)
                        {
                            // ** Mark route **
                            for (int a = 0; a < routeCorePointList.Count; a++)
                            {
                                Point wayPoint = routeCorePointList[a];
                                dgCoreObjectMap.Rows[wayPoint.Y].Cells[wayPoint.X].Style.BackColor = Color.Green;
                                //dgRealObjectMap.Rows[wayPoint.Y].Cells[wayPoint.X].Style.BackColor = Color.Green;
                        
                                markedBitmap.SetPixel(wayPoint.X, wayPoint.Y, Color.Green);
                            }
                        }
                        log.Info("Mark route - END");
                    #endregion

                    #region ** Set START and END points **  
                        log.Info("Set START and END points - START");
                        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.BackColor = Color.Blue;
                        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.ForeColor = Color.White;
                        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Value = "S";
                        //dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.BackColor = Color.Blue;
                        //dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.ForeColor = Color.White;
                        //dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                        //dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Value = "S";
                        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.BackColor = Color.Red;
                        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.ForeColor = Color.White;
                        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Value = "F";
                        //dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.BackColor = Color.Red;
                        //dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.ForeColor = Color.White;
                        //dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                        //dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Value = "F";

                        markedBitmap.SetPixel(coreStartPoint.X, coreStartPoint.Y, Color.Blue);
                        markedBitmap.SetPixel(coreTargetPoint.X, coreTargetPoint.Y, Color.Red);

                        log.Info("Set START and END points - END");
                    #endregion

                    // ** Set updated bitmap image **
                    panel1.BackgroundImage = markedBitmap;
                
                    // ** Show confirmation **
                    if (routeFound) MessageBox.Show("Route found. Took " + ts.TotalMilliseconds.ToString("#,###") + "ms...");                
                    else MessageBox.Show("No Route found. Took " + ts.TotalMilliseconds.ToString("#,###") + "ms..."); 

                    log.Info("Get Route - END");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
                   
        #endregion



        //private void Execute_OLD()
        //{
        //    try
        //    {
        //        // ** Clear existing data **
        //        dgCoreObjectMap.DataSource = null;
        //        dgRealObjectMap.DataSource = null;

        //        // ** Generate variables **
        //        //Point topLeft = new Point(0, 0);
        //        //int MapWidth = 8;
        //        //int MapHeight = 7;
        //        Point topLeft = new Point(-10, 10);
        //        int MapWidth = 20;
        //        int MapHeight = 20;
        //        Point realStartPoint = new Point(2, 3);
        //        //Point realStartPoint = new Point(-9, -1);
        //        //Point realTargetPoint = new Point(6, 3);
        //        Point realTargetPoint = new Point(7, 8);
                
        //        //Point topLeft = new Point(0, 0);                
        //        //Point realStartPoint = new Point(5, -200);
        //        //Point realTargetPoint = new Point(140, -100);



        //        // ** Generate fixed variables **
        //        //string bitmapLocation = @"C:\SETTLERS STUFF\Bitmap_Test.bmp";
        //        int squareSize = 30;
        //        float fontSize = 6;
        //        bool allowDiagonals = chkAllowDiagonals.Checked;

        //        // ** Generate Object Map (manual) **                              
        //        int[,] ObjectMap = new int[MapWidth, MapHeight];
        //        for (int a = 0; a < MapWidth; a++)
        //        {
        //            for (int b = 0; b < MapHeight; b++)
        //            {
        //                ObjectMap[a, b] = 0;
        //            }
        //        }
        //        Point obPoint;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 2), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 3), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 4), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;

        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 5), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 6), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 7), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(3, 7), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(2, 7), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(1, 7), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(0, 7), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;

        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 1), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(4, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;

        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(3, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(2, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(1, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(0, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(-1, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(-2, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(-3, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(-4, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;
        //        obPoint = Point.ConvertRealPointToCorePoint(new Point(-5, 0), topLeft); ObjectMap[obPoint.X, obPoint.Y] = -1;



        //        // ** Save Object Map to Bitmap ** 
        //        ////Bitmap bMap = GenerateBitMapFromObjectMap(ObjectMap);
        //        ////bMap.Save(bitmapLocation);               
        //        //using (Bitmap bMap = GenerateBitMapFromObjectMap(ObjectMap))
        //        //{
        //        //    bMap.Save(bitmapLocation);
        //        //}

        //        // ** Load Bitmap into Object Map **
        //        //string mapLocation = @"C:\SETTLERS STUFF\MAP1.bmp";
        //        //Bitmap bMap = (Bitmap)Image.FromFile(mapLocation);
        //        //int[,] ObjectMap = GenerateObjectMapFromBitMap(bMap);
        //        //int MapWidth = bMap.Width;
        //        //int MapHeight = bMap.Height;





        //        // ** Save CoreObjectMap to DataTable **                
        //        DataTable CoreObjectMapTable = new DataTable("CoreObjectMapTable", "CoreObjectMapTable");
        //        for (int a = 0; a < MapWidth; a++)               
        //        {                    
        //            CoreObjectMapTable.Columns.Add(a.ToString(), typeof(string));
        //        }                
        //        for (int a = 0; a < MapHeight; a++)
        //        {
        //            object[] row = new object[CoreObjectMapTable.Columns.Count];
        //            //for (int b = 0; b < MapWidth; b++)
        //            //{                        
        //            //    Point corePoint = new Point(b, a);                        
        //            //    row[b] = "{" + corePoint.X + "," + corePoint.Y + "}";
        //            //}
        //            CoreObjectMapTable.Rows.Add(row);
        //        }
        //        dgCoreObjectMap.DataSource = CoreObjectMapTable;
                
        //        // ** Save RealObjectMap to DataTable **                
        //        DataTable RealObjectMapTable = new DataTable("RealObjectMapTable", "RealObjectMapTable");
        //        for (int a = 0; a < MapWidth; a++)
        //        {                    
        //            int xIndex = a + topLeft.X;
        //            RealObjectMapTable.Columns.Add(xIndex.ToString(), typeof(string));
        //        }
        //        for (int a = 0; a < MapHeight; a++)
        //        {
        //            object[] row = new object[RealObjectMapTable.Columns.Count];
        //            //for (int b = 0; b < MapWidth; b++)
        //            //{                       
        //            //    Point corePoint = new Point(b, a);
        //            //    Point realPoint = ConvertCorePointToRealPoint(corePoint, topLeft);                                            
        //            //    row[b] = "{" + realPoint.X + "," + realPoint.Y + "}";
        //            //}
        //            RealObjectMapTable.Rows.Add(row);
        //        }
        //        dgRealObjectMap.DataSource = RealObjectMapTable;

        //        // ** Update column/row widths/heights **                
        //        for (int a = 0; a < MapWidth; a++)
        //        {
        //            dgCoreObjectMap.Columns[a].Width = squareSize;
        //            dgCoreObjectMap.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //            dgCoreObjectMap.Columns[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //            dgCoreObjectMap.Columns[a].DefaultCellStyle.Font = new Font(this.Font.FontFamily, fontSize);                    
        //            dgRealObjectMap.Columns[a].Width = squareSize;
        //            dgRealObjectMap.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //            dgRealObjectMap.Columns[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //            dgRealObjectMap.Columns[a].DefaultCellStyle.Font = new Font(this.Font.FontFamily, fontSize);
        //        }
        //        for (int a = 0; a < MapHeight; a++) dgCoreObjectMap.Rows[a].Height = squareSize;
        //        for (int a = 0; a < MapHeight; a++) dgRealObjectMap.Rows[a].Height = squareSize;
                
        //        // ** Add tooltips to each cell - CoreObjectMap **
        //        for (int a = 0; a < dgCoreObjectMap.Columns.Count; a++)
        //        {
        //            for (int b = 0; b < dgCoreObjectMap.Rows.Count; b++)
        //            {                        
        //                DataGridViewCell cell = dgCoreObjectMap.Rows[b].Cells[a]; 
        //                Point corePoint = new Point(a, b);
        //                cell.ToolTipText = "{" + corePoint.X + "," + corePoint.Y + "}";
        //                cell.Tag = "{" + corePoint.X + "," + corePoint.Y + "}";
        //            }
        //        }

        //        // ** Add tooltips to each cell - RealObjectMap **
        //        for (int a = 0; a < dgRealObjectMap.Columns.Count; a++)
        //        {
        //            for (int b = 0; b < dgRealObjectMap.Rows.Count; b++)
        //            {                       
        //                DataGridViewCell cell = dgRealObjectMap.Rows[b].Cells[a];                        
        //                Point corePoint = new Point(a, b);
        //                Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
        //                cell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "}";
        //                cell.Tag = "{" + realPoint.X + "," + realPoint.Y + "}";
        //            }
        //        }
                
        //        // ** Add obstacles - CoreObjectMap & RealObjectMap **
        //        for (int a = 0; a < MapWidth; a++)
        //        {
        //            for (int b = 0; b < MapHeight; b++)
        //            {
        //                int walkValue = ObjectMap[a, b];
        //                if (walkValue == -1)
        //                {                            
        //                    Point corePoint = new Point(a, b);                            
        //                    dgCoreObjectMap.Rows[corePoint.Y].Cells[corePoint.X].Style.BackColor = Color.Gray;
        //                    dgRealObjectMap.Rows[corePoint.Y].Cells[corePoint.X].Style.BackColor = Color.Gray;
        //                }
        //            }
        //        }


              






        //        // ** Calculate route from Start Point to Target Point **                                  
        //        Point coreStartPoint = Point.ConvertRealPointToCorePoint(realStartPoint, topLeft);
        //        Point coreTargetPoint = Point.ConvertRealPointToCorePoint(realTargetPoint, topLeft);                
        //        bool routeFound = false;
        //        List<Point> routePointList = new List<Point>();
        //        List<Node> nodeList_Open = new List<Node>();
        //        List<Node> nodeList_Closed = new List<Node>();
        //        CalculateRoute(coreStartPoint, coreTargetPoint, ObjectMap, allowDiagonals, out routeFound, out routePointList, out nodeList_Open, out nodeList_Closed);








        //        // ** Mark CLOSED Nodes **
        //        for (int a = 0; a < nodeList_Closed.Count; a++)
        //        {
        //            Node n = nodeList_Closed[a];
        //            Point nodePosition = n.nodePosition;  
                    
        //            // ** CORE **                  
        //            dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightBlue;                    
        //            dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = n.F;
        //            DataGridViewCell coreCell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
        //            coreCell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

        //            // ** REAL **
        //            dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightBlue;
        //            dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = nodeList_Closed[a].F;                    
        //            DataGridViewCell realCell = dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
        //            Point corePoint = new Point(nodePosition.X, nodePosition.Y);
        //            Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);                    
        //            realCell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;
        //        }

        //        // ** Mark OPEN Nodes **
        //        for (int a = 0; a < nodeList_Open.Count; a++)
        //        {
        //            Node n = nodeList_Open[a];
        //            Point nodePosition = n.nodePosition;

        //            // ** CORE **
        //            dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightGray;
        //            dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = nodeList_Open[a].F;
        //            DataGridViewCell cell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
        //            cell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;

        //            // ** REAL **
        //            dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightGray;
        //            dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = n.F;
        //            DataGridViewCell realCell = dgRealObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
        //            Point corePoint = new Point(nodePosition.X, nodePosition.Y);
        //            Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
        //            realCell.ToolTipText = "{" + realPoint.X + "," + realPoint.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;                    
        //        }




               




        //        // ** CHECK IF ROUTE FOUND **
        //        if (routeFound)
        //        {
        //            // ** Mark route **
        //            for (int a = 0; a < routePointList.Count; a++)
        //            {
        //                Point wayPoint = routePointList[a];
        //                dgCoreObjectMap.Rows[wayPoint.Y].Cells[wayPoint.X].Style.BackColor = Color.Green;
        //                dgRealObjectMap.Rows[wayPoint.Y].Cells[wayPoint.X].Style.BackColor = Color.Green;                        
        //            }
        //        }

        //        // ** Set start and End points **                
        //        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.BackColor = Color.Blue;
        //        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.ForeColor = Color.White;
        //        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
        //        dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Value = "S";
        //        dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.BackColor = Color.Blue;
        //        dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.ForeColor = Color.White;
        //        dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
        //        dgRealObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Value = "S";
        //        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.BackColor = Color.Red;
        //        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.ForeColor = Color.White;
        //        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
        //        dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Value = "F";
        //        dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.BackColor = Color.Red;
        //        dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.ForeColor = Color.White;
        //        dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
        //        dgRealObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Value = "F";




        //        // ** CHECK IF ROUTE FOUND **
        //        //if (routeFound)
        //        //{                    
        //        //    // ** Update Bitmap and save **
        //        //    using (Bitmap bMap = GenerateBitMapFromObjectMap(ObjectMap))
        //        //    {                        
        //        //        for (int a = 0; a < routePointList.Count; a++)
        //        //        {
        //        //            if (a == 0)
        //        //            {
        //        //                bMap.SetPixel(routePointList[a].X, routePointList[a].Y, Color.Green);
        //        //            }
        //        //            else if (a == routePointList.Count - 1)
        //        //            {
        //        //                bMap.SetPixel(routePointList[a].X, routePointList[a].Y, Color.Blue);
        //        //            }
        //        //            else
        //        //            {
        //        //                bMap.SetPixel(routePointList[a].X, routePointList[a].Y, Color.Brown);
        //        //            }
        //        //        }
        //        //        bMap.Save(@"C:\SETTLERS STUFF\Bitmap_Outcome.bmp");
        //        //    }                    
        //        //}






        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        
        //private static Bitmap GenerateBitMapFromObjectMap(int[,] ObjectMap)
        //{
        //    int mapWidth = ObjectMap.GetLength(0);
        //    int mapHeight = ObjectMap.GetLength(1);
        //    Bitmap bMap = new Bitmap(mapWidth, mapHeight);            
        //    for (int a = 0; a < mapWidth; a++)
        //    {
        //        for (int b = 0; b < mapHeight; b++)
        //        {
        //            int walkValue = ObjectMap[a, b];
        //            if (walkValue == 0)
        //            {
        //                bMap.SetPixel(a, b, Color.White);
        //            }
        //            else if (walkValue == -1)
        //            {
        //                bMap.SetPixel(a, b, Color.Black);
        //            }
        //        }
        //    }
        //    return bMap;            
        //}
        
        //private static int[,] GenerateObjectMapFromBitMap(Bitmap bMap)
        //{
        //    int mapWidth = bMap.Width;
        //    int mapHeight = bMap.Height;
        //    int[,] ObjectMap = new int[mapWidth, mapHeight];
        //    for (int a = 0; a < mapWidth; a++)
        //    {
        //        for (int b = 0; b < mapHeight; b++)
        //        {
        //            Color pixelColor = bMap.GetPixel(a, b);                   
        //            if (pixelColor.A == 255 && pixelColor.B == 255 && pixelColor.G == 255 && pixelColor.R == 255)
        //            {
        //                ObjectMap[a, b] = 0;
        //            }
        //            else if (pixelColor.A == 255 && pixelColor.B == 0 && pixelColor.G == 0 && pixelColor.R == 0)
        //            {
        //                ObjectMap[a, b] = -1;
        //            }                    
        //         }
        //    }
        //    return ObjectMap;
        //}

        //private static Point ConvertCorePointToRealPoint(Point corePoint, Point topLeft)
        //{
        //    Point realPoint = new Point();
        //    realPoint.X = corePoint.X + topLeft.X;
        //    realPoint.Y = topLeft.Y - corePoint.Y;
        //    return realPoint;
        //}

        //private static Point ConvertRealPointToCorePoint(Point realPoint, Point topLeft)
        //{
        //    Point corePoint = new Point();
        //    corePoint.X = realPoint.X - topLeft.X;
        //    corePoint.Y = topLeft.Y - realPoint.Y;
        //    return corePoint;
        //}

        //private static void CalculateRoute_OLD(Point coreStartPoint, Point coreTargetPoint, int[,] ObjectMap, bool allowDiagonals, out bool routeFound, out List<Point> routeCorePointList, out List<Node> nodeList_Open, out List<Node> nodeList_Closed)
        //{
        //    log.Info("Calculate Route - START");


        //    // ** Define variables **
        //    routeFound = false;
        //    routeCorePointList = new List<Point>();
        //    nodeList_Open = new List<Node>();
        //    nodeList_Closed = new List<Node>();

        //    // ** Add startPoint to NodeList_Open **
        //    Node n = new Node();
        //    n.nodePosition = coreStartPoint;
        //    nodeList_Open.Add(n);

        //    // ** Cycle through all nodes and calculate values **   
        //    int MapWidth = ObjectMap.GetLength(0);
        //    int MapHeight = ObjectMap.GetLength(1);
        //    int nodeCount = MapWidth * MapHeight;
        //    for (int a = 0; a < nodeCount; a++)
        //    {
        //        // ** Take the first entry in the list - this should be the lowest F value                    
        //        Node currentNode = nodeList_Open[0];
        //        //if (currentNode.nodePosition.X == 4 && currentNode.nodePosition.Y == 5)
        //        //{
        //        //    string test2 = "";
        //        //}
        //        try
        //        {
        //            // Mark Node as Closed 
        //            nodeList_Open.Remove(currentNode);
        //            nodeList_Closed.Add(currentNode);

        //            // ** Check if current Node is the target **
        //            if (currentNode.nodePosition.X == coreTargetPoint.X && currentNode.nodePosition.Y == coreTargetPoint.Y)
        //            {
        //                // ** Work out route of Points **
        //                List<Point> prelim_routePointList = new List<Point>();
        //                prelim_routePointList.Add(currentNode.nodePosition);
        //                Node currentRouteNode = currentNode;
        //                for (int b = 0; b < nodeCount; b++)
        //                {
        //                    Node parentNode = currentRouteNode.parentNode;
        //                    if (parentNode == null)
        //                    {
        //                        routeFound = true;
        //                        break;
        //                    }
        //                    prelim_routePointList.Add(parentNode.nodePosition);
        //                    currentRouteNode = parentNode;
        //                }

        //                // ** Reverse route list **
        //                for (int i = prelim_routePointList.Count - 1; i >= 0; i--)
        //                {
        //                    routeCorePointList.Add(prelim_routePointList[i]);
        //                }

        //                break;
        //            }

        //            // ** Get all surrounding Positions for Current Node **                                
        //            Dictionary<Point, int> surroundingPointDict = new Dictionary<Point, int>();
        //            surroundingPointDict.Add(new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 0), 10);




        //            if (allowDiagonals) surroundingPointDict.Add(new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 1), 14);
        //            surroundingPointDict.Add(new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + 1), 10);
        //            if (allowDiagonals) surroundingPointDict.Add(new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 1), 14);
        //            surroundingPointDict.Add(new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 0), 10);
        //            if (allowDiagonals) surroundingPointDict.Add(new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + -1), 14);
        //            surroundingPointDict.Add(new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + -1), 10);
        //            if (allowDiagonals) surroundingPointDict.Add(new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + -1), 14);






        //            // ** Cycle through new entries and check if they are not valid - this could include Closed Nodes **
        //            Dictionary<Point, int> validPointDict = new Dictionary<Point, int>();
        //            foreach (Point p in surroundingPointDict.Keys)
        //            {
        //                int walkValue = ObjectMap[p.X, p.Y];
        //                if (walkValue == 0) validPointDict.Add(p, surroundingPointDict[p]);
        //            }

        //            // ** Create entries for any new Positions **
        //            foreach (Point p in validPointDict.Keys)
        //            {
        //                // ** Check if Node already exists in Closed List **
        //                var existingNodeInClosedList = (from r in nodeList_Closed
        //                                                where r.nodePosition.X == p.X && r.nodePosition.Y == p.Y
        //                                                select r).FirstOrDefault();
        //                if (existingNodeInClosedList == null)
        //                {
        //                    // ** Check if Node already exists **
        //                    var existingNodeInOpenList = (from r in nodeList_Open
        //                                                  where r.nodePosition.X == p.X && r.nodePosition.Y == p.Y
        //                                                  select r).FirstOrDefault();
        //                    if (existingNodeInOpenList == null)
        //                    {
        //                        Node newNode = new Node();
        //                        newNode.nodePosition = p;
        //                        newNode.G = currentNode.G + surroundingPointDict[p];
        //                        newNode.H = 10 * (Math.Abs(coreTargetPoint.X - p.X) + Math.Abs(coreTargetPoint.Y - p.Y));
        //                        newNode.F = newNode.G + newNode.H;
        //                        newNode.parentNode = currentNode;
        //                        nodeList_Open.Add(newNode);
        //                    }
        //                    else
        //                    {
        //                        // NODE ALREADY EXISTS IN OPEN LIST
        //                        string ae = "";
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string test = "";
        //        }    

        //        // ** Sort the OPEN Node list **            
        //        nodeList_Open.Sort((x, y) => x.F.CompareTo(y.F));

        //        #region ** Debug variables **
        //            //List<string> OpenNodeList = new List<string>();
        //            //List<string> ClosedNodeList = new List<string>();
        //            //string OpenNodeList_s = "";
        //            //string ClosedNodeList_s = "";
        //            //foreach (Node openNode in nodeList_Open)
        //            //{
        //            //    string parentPosition = "";
        //            //    if (openNode.parentNode != null) parentPosition = openNode.parentNode.nodePosition.ToString();
        //            //    string nodeString = openNode.nodePosition.ToString() + "|" + parentPosition + "|" + openNode.F + "-" + openNode.G + "-" + openNode.H;
        //            //    OpenNodeList.Add(nodeString);
        //            //    OpenNodeList_s += nodeString + Environment.NewLine;
        //            //}
        //            //foreach (Node closedNode in nodeList_Closed)
        //            //{
        //            //    string parentPosition = "";
        //            //    if (closedNode.parentNode != null) parentPosition = closedNode.parentNode.nodePosition.ToString();
        //            //    string nodeString = closedNode.nodePosition.ToString() + "|" + parentPosition + "|" + closedNode.F + "-" + closedNode.G + "-" + closedNode.H;
        //            //    ClosedNodeList.Add(nodeString);
        //            //    ClosedNodeList_s += nodeString + Environment.NewLine;
        //            //}
        //        #endregion

        //    }

        //    log.Info("Calculate Route - END");


        //}

        //private static void CalculateRoute_OLD(Point coreStartPoint, Point coreTargetPoint, int[,] ObjectMap, bool allowDiagonals, out bool routeFound, out List<Point> routeCorePointList, out List<Node> nodeList_Open, out List<Node> nodeList_Closed)
        //{
        //    log.Info("Calculate Route - START");

        //    // ** Define variables **
        //    routeFound = false;
        //    routeCorePointList = new List<Point>();
        //    nodeList_Open = new List<Node>();
        //    nodeList_Closed = new List<Node>();
        //    //HashSet<Point> openHS = new HashSet<Point>();
        //    //HashSet<Point> closedHS = new HashSet<Point>();
        //    HashSet<string> openHS = new HashSet<string>();
        //    HashSet<string> closedHS = new HashSet<string>();            
        //    DateTime overallStartTime = DateTime.Now;
        //    DateTime overallEndTime;

        //    // ** Add startPoint to NodeList_Open **
        //    Node n = new Node();
        //    n.nodePosition = coreStartPoint;
        //    n.nodePosition_s = coreStartPoint.ToString();
        //    nodeList_Open.Add(n);
        //    openHS.Add(n.nodePosition.ToString());
        //    openHS.Add(n.nodePosition_s);

        //    // ** Cycle through all nodes and calculate values **   
        //    int MapWidth = ObjectMap.GetLength(0);
        //    int MapHeight = ObjectMap.GetLength(1);
        //    int nodeCount = MapWidth * MapHeight;
        //    List<string> erroredNodeList = new List<string>();            
        //    for (int a = 0; a < nodeCount; a++)
        //    {
        //        // ** If there are no open Nodes left then break **
        //        if (nodeList_Open.Count == 0)
        //        {
        //            break;
        //        }

        //        #region ** Take the first entry in the list and process - this should be the lowest F value
        //        Node currentNode = null;
        //        try
        //        {
        //            //if (a == 27472)
        //            //{
        //            //    string test2 = "";
        //            //}
        //            currentNode = nodeList_Open[0]; // ##### IF OPEN LIST HAS NO ENTRIES THEN EXIT AS NO ROUTE AVAILABLE
        //            //if (currentNode.nodePosition.X == 2 && currentNode.nodePosition.Y == 2)
        //            //{
        //            //    string test2 = "";
        //            //}

        //            // ** Mark Node as Closed **
        //            nodeList_Open.Remove(currentNode);
        //            //openHS.Remove(currentNode.nodePosition);                    
        //            //openHS.Remove(currentNode.nodePosition.ToString());
        //            openHS.Remove(currentNode.nodePosition_s);

        //            nodeList_Closed.Add(currentNode);
        //            //closedHS.Add(currentNode.nodePosition);
        //            //closedHS.Add(currentNode.nodePosition.ToString());
        //            closedHS.Add(currentNode.nodePosition_s);




        //            #region ** Check if current Node is the target **
        //            if (currentNode.nodePosition.X == coreTargetPoint.X && currentNode.nodePosition.Y == coreTargetPoint.Y)
        //                {
        //                    // ** Work out route of Points **
        //                    List<Point> prelim_routePointList = new List<Point>();
        //                    prelim_routePointList.Add(currentNode.nodePosition);
        //                    Node currentRouteNode = currentNode;
        //                    for (int b = 0; b < nodeCount; b++)
        //                    {
        //                        Node parentNode = currentRouteNode.parentNode;
        //                        if (parentNode == null)
        //                        {
        //                            routeFound = true;
        //                            break;
        //                        }
        //                        prelim_routePointList.Add(parentNode.nodePosition);
        //                        currentRouteNode = parentNode;
        //                    }

        //                    // ** Reverse route list **
        //                    for (int i = prelim_routePointList.Count - 1; i >= 0; i--)
        //                    {
        //                        routeCorePointList.Add(prelim_routePointList[i]);
        //                    }

        //                    break;
        //                }
        //            #endregion

        //            #region ** Get all surrounding Positions for Current Node **
        //                //Dictionary<Point, int> surroundingPointDict = new Dictionary<Point, int>();
        //                Dictionary<string, int> surroundingPointDict = new Dictionary<string, int>();
        //                Point sp;                                                               
        //                //sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 0);
        //                int npX = currentNode.nodePosition.X + 1;
        //                int npY = currentNode.nodePosition.Y + 0;
        //                //if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                if (npX >= 0 && npY >= 0 && npX < MapWidth && npY < MapHeight)
        //                {
        //                    //surroundingPointDict.Add(sp, 10);
        //                    //string np1 = sp.ToString();
        //                    //string np2 = "{" + npX + "," + npY + "}";                                                 
        //                    //surroundingPointDict.Add(np2, 10);
        //                    surroundingPointDict.Add("{" + npX + "," + npY + "}", 10);
        //                }           
        //                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + 1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    //surroundingPointDict.Add(sp, 10);
        //                    surroundingPointDict.Add(sp.ToString(), 10);
        //                }                    
        //                sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 0);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    //surroundingPointDict.Add(sp, 10);
        //                    surroundingPointDict.Add(sp.ToString(), 10);
        //                }                    
        //                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + -1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    //surroundingPointDict.Add(sp, 10);
        //                    surroundingPointDict.Add(sp.ToString(), 10);
        //                }                        
        //            #endregion

        //            #region ** Check if diagonals are allowed **                       
        //                if (allowDiagonals)
        //                {                            
        //                    //sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 1);
        //                    //if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    //{
        //                    //    surroundingPointDict.Add(sp, 14);
        //                    //}
        //                    //sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 1);
        //                    //if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    //{
        //                    //    surroundingPointDict.Add(sp, 14);
        //                    //}
        //                    //sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + -1);
        //                    //if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    //{
        //                    //    surroundingPointDict.Add(sp, 14);
        //                    //}
        //                    //sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + -1);
        //                    //if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    //{
        //                    //    surroundingPointDict.Add(sp, 14);
        //                    //}
        //                }                        
        //            #endregion

        //            // ** Cycle through new entries and check if they are not valid - this could include Closed Nodes **
        //            //Dictionary<Point, int> validPointDict = new Dictionary<Point, int>();
        //            //foreach (Point p in surroundingPointDict.Keys)
        //            //{
        //            //    int walkValue = ObjectMap[p.X, p.Y];
        //            //    if (walkValue == 0)
        //            //    {
        //            //        validPointDict.Add(p, surroundingPointDict[p]);
        //            //    }
        //            //}
        //            Dictionary<string, int> validPointDict = new Dictionary<string, int>();
        //            //foreach (Point p in surroundingPointDict.Keys)
        //            foreach (string p in surroundingPointDict.Keys)
        //            {
        //                //int walkValue = ObjectMap[p.X, p.Y];
        //                //if (walkValue == 0)
        //                //{
        //                //    validPointDict.Add(p.ToString(), surroundingPointDict[p]);
        //                //}
        //                string[] coords = p.Replace("{", "").Replace("}", "").Split(',');
        //                Point np = new Point(int.Parse(coords[0]), int.Parse(coords[1]));                        
        //                int walkValue = ObjectMap[np.X, np.Y];
        //                if (walkValue == 0)
        //                {
        //                    validPointDict.Add(p, surroundingPointDict[p]);
        //                }
        //            }

        //            #region ** Create entries for any new Positions **                        
        //                //foreach (Point p in validPointDict.Keys)
        //                foreach(string p in validPointDict.Keys)
        //                {
        //                    // ** Check if Node already exists in Closed List **
        //                    //if(closedHS.Contains(p) == false)
        //                    //if (closedHS.Contains(p.ToString()) == false)
        //                    if (closedHS.Contains(p) == false)
        //                    //var existingNodeInClosedList = (from r in nodeList_Closed
        //                    //                                where r.nodePosition.X == p.X && r.nodePosition.Y == p.Y
        //                    //                                select r).FirstOrDefault();
        //                    //if (existingNodeInClosedList == null)
        //                    {
        //                        // ** Check if Node already exists **
        //                        //if (openHS.Contains(p) == false)
        //                        //if (openHS.Contains(p.ToString()) == false)
        //                        if (openHS.Contains(p) == false)
        //                        //var existingNodeInOpenList = (from r in nodeList_Open
        //                        //                              where r.nodePosition.X == p.X && r.nodePosition.Y == p.Y
        //                        //                              select r).FirstOrDefault();
        //                        //if (existingNodeInOpenList == null)
        //                        {
        //                            string[] coords = p.Replace("{", "").Replace("}", "").Split(',');
        //                            Point np = new Point(int.Parse(coords[0]), int.Parse(coords[1]));

        //                            Node newNode = new Node();
        //                            //newNode.nodePosition = p;
        //                            newNode.nodePosition = np;
        //                            newNode.nodePosition_s = p;                                
        //                            //newNode.G = currentNode.G + surroundingPointDict[p];
        //                            //newNode.G = currentNode.G + surroundingPointDict[newNode.nodePosition];
        //                            newNode.G = currentNode.G + surroundingPointDict[newNode.nodePosition_s];
        //                            //newNode.H = 10 * (Math.Abs(coreTargetPoint.X - p.X) + Math.Abs(coreTargetPoint.Y - p.Y));
        //                            newNode.H = 10 * (Math.Abs(coreTargetPoint.X - newNode.nodePosition.X) + Math.Abs(coreTargetPoint.Y - newNode.nodePosition.Y));
        //                            newNode.F = newNode.G + newNode.H;
        //                            newNode.parentNode = currentNode;
        //                            nodeList_Open.Add(newNode);
        //                            //openHS.Add(newNode.nodePosition);
        //                            //openHS.Add(newNode.nodePosition.ToString());
        //                            openHS.Add(p);
        //                        }
        //                        else
        //                        {
        //                            // NODE ALREADY EXISTS IN OPEN LIST
        //                            //string ae = "";
        //                        }
        //                    }
        //                }                        
        //            #endregion

        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "";
        //            if (currentNode == null) errorMessage = a + "| NULL |" + ex.Message;                    
        //            else errorMessage = a + "|" + currentNode.nodePosition + "|" + ex.Message;                   
        //            erroredNodeList.Add(errorMessage);
        //        }
        //        #endregion

        //        // ** Sort the OPEN Node list ** 
        //        nodeList_Open.Sort((x, y) => x.F.CompareTo(y.F));

        //        #region ** DEBUG VARIABLES **
        //        //List<string> OpenNodeList = new List<string>();
        //        //List<string> ClosedNodeList = new List<string>();
        //        //string OpenNodeList_s = "";
        //        //string ClosedNodeList_s = "";
        //        //foreach (Node openNode in nodeList_Open)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (openNode.parentNode != null) parentPosition = openNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = openNode.nodePosition.ToString() + "|" + parentPosition + "|" + openNode.F + "-" + openNode.G + "-" + openNode.H;
        //        //    OpenNodeList.Add(nodeString);
        //        //    OpenNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        //foreach (Node closedNode in nodeList_Closed)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (closedNode.parentNode != null) parentPosition = closedNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = closedNode.nodePosition.ToString() + "|" + parentPosition + "|" + closedNode.F + "-" + closedNode.G + "-" + closedNode.H;
        //        //    ClosedNodeList.Add(nodeString);
        //        //    ClosedNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        #endregion

        //        string test = "";

        //    }            
        //    overallEndTime = DateTime.Now;
        //    TimeSpan ts = overallEndTime - overallStartTime;
        //    log.Info("Calculate Route - END");
        //}

        //private static void CalculateRoute_OLD1(Point coreStartPoint, Point coreTargetPoint, int[,] ObjectMap, bool allowDiagonals, out bool routeFound, out List<Point> routeCorePointList, out List<Node> nodeList_Open, out List<Node> nodeList_Closed)
        //{
        //    log.Info("Calculate Route - START");

        //    // ** Define variables **
        //    routeFound = false;
        //    routeCorePointList = new List<Point>();
        //    nodeList_Open = new List<Node>();
        //    nodeList_Closed = new List<Node>();           
        //    HashSet<string> openHS = new HashSet<string>();
        //    HashSet<string> closedHS = new HashSet<string>();
        //    DateTime overallStartTime = DateTime.Now;
        //    DateTime overallEndTime;

        //    // ** Add startPoint to NodeList_Open **
        //    Node n = new Node();
        //    n.nodePosition = coreStartPoint;
        //    nodeList_Open.Add(n);
        //    openHS.Add(n.nodePosition.ToString());
                        
        //    // ** Cycle through all nodes and calculate values **   
        //    int MapWidth = ObjectMap.GetLength(0);
        //    int MapHeight = ObjectMap.GetLength(1);
        //    int nodeCount = MapWidth * MapHeight;
        //    List<string> erroredNodeList = new List<string>();
        //    for (int a = 0; a < nodeCount; a++)
        //    {
        //        // ** If there are no open Nodes left then break **
        //        if (nodeList_Open.Count == 0)
        //        {
        //            break;
        //        }

        //        #region ** Take the first entry in the list and process - this should be the lowest F value
        //        Node currentNode = null;
        //        try
        //        {
        //            //if (a == 27472)
        //            //{
        //            //    string test2 = "";
        //            //}
        //            currentNode = nodeList_Open[0];
        //            //if (currentNode.nodePosition.X == 2 && currentNode.nodePosition.Y == 2)
        //            //{
        //            //    string test2 = "";
        //            //}

        //            // ** Mark Node as Closed **
        //            nodeList_Open.Remove(currentNode);
        //            openHS.Remove(currentNode.nodePosition.ToString());                    
        //            nodeList_Closed.Add(currentNode);                    
        //            closedHS.Add(currentNode.nodePosition.ToString());
                    
        //            #region ** Check if current Node is the target **
        //            if (currentNode.nodePosition.X == coreTargetPoint.X && currentNode.nodePosition.Y == coreTargetPoint.Y)
        //            {
        //                // ** Work out route of Points **
        //                List<Point> prelim_routePointList = new List<Point>();
        //                prelim_routePointList.Add(currentNode.nodePosition);
        //                Node currentRouteNode = currentNode;
        //                for (int b = 0; b < nodeCount; b++)
        //                {
        //                    Node parentNode = currentRouteNode.parentNode;
        //                    if (parentNode == null)
        //                    {
        //                        routeFound = true;
        //                        break;
        //                    }
        //                    prelim_routePointList.Add(parentNode.nodePosition);
        //                    currentRouteNode = parentNode;
        //                }

        //                // ** Reverse route list **
        //                for (int i = prelim_routePointList.Count - 1; i >= 0; i--)
        //                {
        //                    routeCorePointList.Add(prelim_routePointList[i]);
        //                }

        //                break;
        //            }
        //            #endregion

        //            #region ** Get all surrounding Positions for Current Node **
        //                Dictionary<Point, int> surroundingPointDict = new Dictionary<Point, int>();
        //                Point sp;
        //                sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 0);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 10);                        
        //                }
        //                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + 1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 10);
        //                    //surroundingPointDict.Add(sp.ToString(), 10);
        //                }
        //                sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 0);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 10);
        //                    //surroundingPointDict.Add(sp.ToString(), 10);
        //                }
        //                sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + -1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 10);
        //                    //surroundingPointDict.Add(sp.ToString(), 10);
        //                }
        //            #endregion

        //            #region ** Check if diagonals are allowed **                       
        //                if (allowDiagonals)
        //                {
        //                    sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 1);
        //                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    {
        //                        surroundingPointDict.Add(sp, 14);
        //                    }
        //                    sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 1);
        //                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    {
        //                        surroundingPointDict.Add(sp, 14);
        //                    }
        //                    sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + -1);
        //                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    {
        //                        surroundingPointDict.Add(sp, 14);
        //                    }
        //                    sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + -1);
        //                    if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                    {
        //                        surroundingPointDict.Add(sp, 14);
        //                    }
        //                }
        //            #endregion

        //            #region ** Cycle through new entries and check if they are INVALID or CLOSED **
        //                Dictionary<Point, int> validPointDict = new Dictionary<Point, int>();
        //                foreach (Point p in surroundingPointDict.Keys)
        //                {
        //                    int walkValue = ObjectMap[p.X, p.Y];
        //                    if (walkValue == 0)
        //                    {
        //                        //validPointDict.Add(p, surroundingPointDict[p]);
        //                        if (closedHS.Contains(p.ToString()) == false)
        //                        {
        //                            validPointDict.Add(p, surroundingPointDict[p]);
        //                        }
        //                    }
        //                }
        //            #endregion

        //            #region ** Create entries for any new Positions **                        
        //                foreach (Point p in validPointDict.Keys)                    
        //                {
        //                    // ** Check if Node already exists in Closed List **
        //                    //if (closedHS.Contains(p.ToString()) == false)                        
        //                    //{
        //                        // ** Check if Node already exists **                            
        //                        if (openHS.Contains(p.ToString()) == false)                            
        //                        {                               
        //                            Node newNode = new Node();
        //                            newNode.nodePosition = p;                                
        //                            newNode.G = currentNode.G + surroundingPointDict[newNode.nodePosition];                                
        //                            newNode.H = 10 * (Math.Abs(coreTargetPoint.X - newNode.nodePosition.X) + Math.Abs(coreTargetPoint.Y - newNode.nodePosition.Y));
        //                            newNode.F = newNode.G + newNode.H;
        //                            newNode.parentNode = currentNode;
        //                            nodeList_Open.Add(newNode);                                
        //                            openHS.Add(newNode.nodePosition.ToString());                                
        //                        }
        //                        else
        //                        {
        //                            // NODE ALREADY EXISTS IN OPEN LIST
        //                            //string ae = "";
        //                        }
        //                    //}
        //                }
        //            #endregion

        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "";
        //            if (currentNode == null) errorMessage = a + "| NULL |" + ex.Message;
        //            else errorMessage = a + "|" + currentNode.nodePosition + "|" + ex.Message;
        //            erroredNodeList.Add(errorMessage);
        //        }
        //        #endregion

        //        // ** Sort the OPEN Node list ** 
        //        nodeList_Open.Sort((x, y) => x.F.CompareTo(y.F));

        //        #region ** DEBUG VARIABLES **
        //        //List<string> OpenNodeList = new List<string>();
        //        //List<string> ClosedNodeList = new List<string>();
        //        //string OpenNodeList_s = "";
        //        //string ClosedNodeList_s = "";
        //        //foreach (Node openNode in nodeList_Open)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (openNode.parentNode != null) parentPosition = openNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = openNode.nodePosition.ToString() + "|" + parentPosition + "|" + openNode.F + "-" + openNode.G + "-" + openNode.H;
        //        //    OpenNodeList.Add(nodeString);
        //        //    OpenNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        //foreach (Node closedNode in nodeList_Closed)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (closedNode.parentNode != null) parentPosition = closedNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = closedNode.nodePosition.ToString() + "|" + parentPosition + "|" + closedNode.F + "-" + closedNode.G + "-" + closedNode.H;
        //        //    ClosedNodeList.Add(nodeString);
        //        //    ClosedNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        #endregion

        //        //string test = "";

        //    }
        //    overallEndTime = DateTime.Now;
        //    TimeSpan ts = overallEndTime - overallStartTime;
        //    log.Info("Calculate Route - END");
        //}
        
        //private static void CalculateRoute_OLD2(Point coreStartPoint, Point coreTargetPoint, int[,] ObjectMap, bool allowDiagonals, out bool routeFound, out List<Point> routeCorePointList, out List<Node> nodeList_Open, out List<Node> nodeList_Closed)
        //{
        //    log.Info("Calculate Route - START");

        //    // ** Define variables **
        //    DateTime overallStartTime = DateTime.Now;
        //    DateTime overallEndTime;
        //    HashSet<double> processingMap = new HashSet<double>();
        //    routeFound = false;
        //    routeCorePointList = new List<Point>();
        //    nodeList_Open = new List<Node>();
        //    nodeList_Closed = new List<Node>();
        //    HashSet<string> openHS = new HashSet<string>();
        //    HashSet<string> closedHS = new HashSet<string>();

        //    // ** Add startPoint to NodeList_Open **
        //    Node n = new Node();
        //    n.nodePosition = coreStartPoint;
        //    nodeList_Open.Add(n);
        //    openHS.Add(n.nodePosition.ToString());

        //    // ** Cycle through all nodes and calculate values **   
        //    int MapWidth = ObjectMap.GetLength(0);
        //    int MapHeight = ObjectMap.GetLength(1);
        //    int nodeCount = MapWidth * MapHeight;
        //    List<string> erroredNodeList = new List<string>();
        //    for (int a = 0; a < nodeCount; a++)
        //    {
        //        DateTime startTime = DateTime.Now;
                
        //        // ** If there are no open Nodes left then break **
        //        if (nodeList_Open.Count == 0) break;
                
        //        #region ** Take the first entry in the list and process - this should be the lowest F value
        //        Node currentNode = null;
        //        try
        //        {
        //            // ** Infer current node **
        //            currentNode = nodeList_Open[0];
                   
        //            // ** Mark Node as Closed **
        //            nodeList_Open.Remove(currentNode);                    
        //            nodeList_Closed.Add(currentNode);
        //            openHS.Remove(currentNode.nodePosition.ToString());
        //            closedHS.Add(currentNode.nodePosition.ToString());

        //            #region ** Check if current Node is the target **
        //            if (currentNode.nodePosition.X == coreTargetPoint.X && currentNode.nodePosition.Y == coreTargetPoint.Y)
        //            {
        //                // ** Work out route of Points **
        //                List<Point> prelim_routePointList = new List<Point>();
        //                prelim_routePointList.Add(currentNode.nodePosition);
        //                Node currentRouteNode = currentNode;
        //                for (int b = 0; b < nodeCount; b++)
        //                {
        //                    Node parentNode = currentRouteNode.parentNode;
        //                    if (parentNode == null)
        //                    {
        //                        routeFound = true;
        //                        break;
        //                    }
        //                    prelim_routePointList.Add(parentNode.nodePosition);
        //                    currentRouteNode = parentNode;
        //                }

        //                // ** Reverse route list **
        //                for (int i = prelim_routePointList.Count - 1; i >= 0; i--)
        //                {
        //                    routeCorePointList.Add(prelim_routePointList[i]);
        //                }

        //                break;
        //            }
        //            #endregion

        //            #region ** Get all surrounding Positions for Current Node **
        //            Dictionary<Point, int> surroundingPointDict = new Dictionary<Point, int>();
        //            Point sp;
        //            sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 0);
        //            if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //            {
        //                surroundingPointDict.Add(sp, 10);
        //            }
        //            sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + 1);
        //            if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //            {
        //                surroundingPointDict.Add(sp, 10);                       
        //            }
        //            sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 0);
        //            if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //            {
        //                surroundingPointDict.Add(sp, 10);                        
        //            }
        //            sp = new Point(currentNode.nodePosition.X + 0, currentNode.nodePosition.Y + -1);
        //            if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //            {
        //                surroundingPointDict.Add(sp, 10);                        
        //            }
        //            #endregion

        //            #region ** Check if diagonals are allowed **                       
        //            if (allowDiagonals)
        //            {
        //                sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + 1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 14);
        //                }
        //                sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + 1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 14);
        //                }
        //                sp = new Point(currentNode.nodePosition.X + -1, currentNode.nodePosition.Y + -1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 14);
        //                }
        //                sp = new Point(currentNode.nodePosition.X + 1, currentNode.nodePosition.Y + -1);
        //                if (sp.X >= 0 && sp.Y >= 0 && sp.X < MapWidth && sp.Y < MapHeight)
        //                {
        //                    surroundingPointDict.Add(sp, 14);
        //                }
        //            }
        //            #endregion
                    


        //            #region ** Cycle through new entries and check if they are INVALID or CLOSED **
        //                Dictionary<Point, int> validPointDict = new Dictionary<Point, int>();
        //                foreach (Point p in surroundingPointDict.Keys)
        //                {
        //                    int walkValue = ObjectMap[p.X, p.Y];
        //                    if (walkValue == 0)
        //                    {                                
        //                        if (closedHS.Contains(p.ToString()) == false)
        //                        {
        //                            validPointDict.Add(p, surroundingPointDict[p]);
        //                        }
        //                    }
        //                }
        //            #endregion



        //            #region ** Create entries for any new Positions **                        
        //                foreach (Point p in validPointDict.Keys)
        //                {                       
        //                    // ** Check if Node already exists **                            
        //                    if (openHS.Contains(p.ToString()) == false)
        //                    {
        //                        Node newNode = new Node();
        //                        newNode.nodePosition = p;
        //                        newNode.G = currentNode.G + surroundingPointDict[newNode.nodePosition];
        //                        newNode.H = 10 * (Math.Abs(coreTargetPoint.X - newNode.nodePosition.X) + Math.Abs(coreTargetPoint.Y - newNode.nodePosition.Y));
        //                        newNode.F = newNode.G + newNode.H;
        //                        newNode.parentNode = currentNode;
        //                        nodeList_Open.Add(newNode);
        //                        openHS.Add(newNode.nodePosition.ToString());
        //                    }
        //                    else
        //                    {
        //                        // NODE ALREADY EXISTS IN OPEN LIST
        //                        //string ae = "";
        //                    }                        
        //                }
        //            #endregion



        //        }
        //        catch (Exception ex)
        //        {
        //            string errorMessage = "";
        //            if (currentNode == null) errorMessage = a + "| NULL |" + ex.Message;
        //            else errorMessage = a + "|" + currentNode.nodePosition + "|" + ex.Message;
        //            erroredNodeList.Add(errorMessage);
        //        }
        //        #endregion

        //        // ** Sort the OPEN Node list ** 
        //        nodeList_Open.Sort((x, y) => x.F.CompareTo(y.F));

        //        #region ** DEBUG VARIABLES **
        //        //List<string> OpenNodeList = new List<string>();
        //        //List<string> ClosedNodeList = new List<string>();
        //        //string OpenNodeList_s = "";
        //        //string ClosedNodeList_s = "";
        //        //foreach (Node openNode in nodeList_Open)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (openNode.parentNode != null) parentPosition = openNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = openNode.nodePosition.ToString() + "|" + parentPosition + "|" + openNode.F + "-" + openNode.G + "-" + openNode.H;
        //        //    OpenNodeList.Add(nodeString);
        //        //    OpenNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        //foreach (Node closedNode in nodeList_Closed)
        //        //{
        //        //    string parentPosition = "";
        //        //    if (closedNode.parentNode != null) parentPosition = closedNode.parentNode.nodePosition.ToString();
        //        //    string nodeString = closedNode.nodePosition.ToString() + "|" + parentPosition + "|" + closedNode.F + "-" + closedNode.G + "-" + closedNode.H;
        //        //    ClosedNodeList.Add(nodeString);
        //        //    ClosedNodeList_s += nodeString + Environment.NewLine;
        //        //}
        //        #endregion
                                
        //        processingMap.Add((DateTime.Now - startTime).TotalMilliseconds);

        //    }
        //    overallEndTime = DateTime.Now;
        //    TimeSpan ts = overallEndTime - overallStartTime;
        //    log.Info("Calculate Route - END");
        //}




        private void LoadLoggingEntry()
        {
            try
            {
                // ** Validation **
                //if (fldLoggingXMLFilePath.Text.Equals(""))
                //{
                //    throw new Exception("No logging XML filepath entered...");
                //}

                // ** Determine XML filename **
                //string xmlFilePath = fldLoggingXMLFilePath.Text;
                //string loggingSource = Path.GetFileName(xmlFilePath).Replace("_details.xml", "");
                //string ObjectMapFilePath = @"C:\SETTLERS STUFF\Debug\ObjectMaps\" + loggingSource + "_objectmap.png";
                //fldBitmapFilePath.Text = ObjectMapFilePath;
                //chkAllowDiagonals.Checked = true;
                string xmlFilePath = @"C:\SETTLERS STUFF\Debug\Current_GlobalMap.xml";
                string ObjectMapFilePath = @"C:\SETTLERS STUFF\Debug\RouteMaps\RR27_01Nov2021_152819_WOODCUTTER.1_objectmap.png";


                // ** Get XML details **
                XmlDocument loggingXML = new XmlDocument();
                loggingXML.Load(xmlFilePath);

                // ** Get variables **
                //string mapFactor = loggingXML.SelectSingleNode("//LoggingDetails/@MapFactor").InnerXml;
                //string topLeft = loggingXML.SelectSingleNode("//LoggingDetails/@TopLeft").InnerXml;
                //string realStartPoint = loggingXML.SelectSingleNode("//LoggingDetails/RealStartPoint").InnerXml;
                //string realTargetPoint = loggingXML.SelectSingleNode("//LoggingDetails/RealTargetPoint").InnerXml;                
                string mapFactor = loggingXML.SelectSingleNode("//GlobalMap/@MapFactor").InnerXml;
                string topLeft = "{" + loggingXML.SelectSingleNode("//GlobalMap/@MapTopLeftX").InnerXml + "," + loggingXML.SelectSingleNode("//GlobalMap/@MapTopLeftY").InnerXml + "}";
                //string realStartPoint = loggingXML.SelectSingleNode("//GlobalMap/RealStartPoint").InnerXml;
                //string realTargetPoint = loggingXML.SelectSingleNode("//GlobalMap/RealTargetPoint").InnerXml;


                // ** Post variables **
                fldMapFactor.Text = mapFactor;
                fldTopLeft.Text = topLeft.Replace("{", "(").Replace("}", ")");
                fldBitmapFilePath.Text = ObjectMapFilePath;
                chkAllowDiagonals.Checked = true;

                // ** Refresh Screen **
                LoadMapFromBitmap();

                //fldPointType.Text = "REAL";
                //fldStartPoint.Text = realStartPoint.Replace("{", "(").Replace("}", ")");
                //fldTargetPoint.Text = realTargetPoint.Replace("{", "(").Replace("}", ")");
                fldPointType.Text = "CORE";
                fldStartPoint.Text = "(9,10)";
                fldTargetPoint.Text = "(90,200)";


                GetRoute();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


       


    }




}
