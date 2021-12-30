using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using ScintillaNET;
using System.Xml.Linq;


namespace Settlers
{
    public partial class GlobalMapScreen : Form
    {
        private static GlobalMap globalMap;
        private Scintilla TextArea;
        private Scintilla LogTextArea;
        private int squareSize = 5;
        private float fontSize = 6;
        private int RouteCalc_MaxWaitMS = 1000;
        private Bitmap orig_pnlObjectMapBitmap;
        private List<string> origLogLines = new List<string>();
        private DataTable dgRouteRequestSummaryTable_Orig;

        private string DebugLocation = @"C:\SETTLERS STUFF\Debug\";



        public GlobalMapScreen()
        {
            InitializeComponent();
            try
            {
                // ** Post header **            
                string[] versionArray = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
                this.Text = "Global Map Screen";
                this.Text += " | " + "v" + versionArray[0] + "." + versionArray[1] + "." + versionArray[2];

                #region ** FORMAT SUMMARIES **
                String[] DGnames = new string[] { "dgRouteRequestSummary", "dgCoreObjectMap", "dgPerfDataSummary" };
                foreach (String dgName in DGnames)
                {
                    DataGridView dgv = (DataGridView)(this.Controls.Find(dgName, true)[0]);
                    dgv.AllowUserToAddRows = false;
                    dgv.AllowUserToDeleteRows = false;
                    dgv.AllowUserToOrderColumns = true;
                    dgv.MultiSelect = true;
                    dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
                    dgv.EnableHeadersVisualStyles = false;
                    //dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                    dgv.ColumnHeadersHeight = 30;
                }
                #endregion

                #region ** ADD ROUTE REQUEST SUMMARY TOOLSTRIP ITEMS **
                toolStrip11.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                lblRequestingObjectAc,
                                fldRequestingObjectAc,                                
                                new ToolStripControlHost(chkLongRunningAc)
                                });
                #endregion

                fldLogsLineCount.Text = "";
                fldLogsLineFilteredCount.Text = "";
                lblRouteRequestCount.Text = "";
                lblObjectMapBitmapStatus.Text = "";
                lblCoreSummaryStatus.Text = "";
                lblPerfDataSummaryCount.Text = "";
                lblRouteRequestSummaryItemFilteredCount.Text = "";

                // ** Set up Scintilla **
                SetupScintillaPanel1();
                SetupLogScintilla();
            }
            catch (Exception ex)
            {
                //log.Error(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        #region ** SCINTILLA FUNCTIONS **

        private const int NUMBER_MARGIN = 1;
        private const int FOLDING_MARGIN = 3;
        private const bool CODEFOLDING_CURCULAR = false;

        private void SetupScintillaPanel1()
        {
            try
            {
                // ** Initialise Scintilla Control Surface **
                TextArea = new ScintillaNET.Scintilla();
                pnlGlobalMapXML.Controls.Add(TextArea);
                TextArea.Dock = System.Windows.Forms.DockStyle.Fill;
                TextArea.WrapMode = WrapMode.None;
                TextArea.IndentationGuides = IndentView.LookBoth;

                // Configuring the default style with properties **
                TextArea.StyleResetDefault();
                TextArea.Styles[Style.Default].Font = "Consolas";
                TextArea.Styles[Style.Default].Size = 8;
                TextArea.Styles[Style.Default].BackColor = Color.Black;
                TextArea.Styles[Style.Default].ForeColor = Color.White;

                // ** Number Margin **
                TextArea.Styles[Style.LineNumber].ForeColor = Color.White;
                TextArea.Styles[Style.LineNumber].BackColor = Color.Black;
                TextArea.Styles[Style.IndentGuide].ForeColor = Color.White;
                TextArea.Styles[Style.IndentGuide].BackColor = Color.Black;
                var nums = TextArea.Margins[NUMBER_MARGIN];
                nums.Width = 30;
                nums.Type = MarginType.Number;
                nums.Sensitive = true;
                nums.Mask = 0;

                // ** Code Folding **
                //TextArea.SetFoldMarginColor(true, Color.Black);
                //TextArea.SetFoldMarginHighlightColor(true, Color.Black);
                //TextArea.SetProperty("fold", "1");
                //TextArea.SetProperty("fold.compact", "1");

                //TextArea.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
                //TextArea.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
                //TextArea.Margins[FOLDING_MARGIN].Sensitive = true;
                //TextArea.Margins[FOLDING_MARGIN].Width = 20;
                //for (int i = 25; i <= 31; i++)
                //{
                //    TextArea.Markers[i].SetForeColor(Color.Black);
                //    TextArea.Markers[i].SetBackColor(Color.White);
                //}

                //TextArea.Markers[Marker.Folder].Symbol = CODEFOLDING_CURCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
                //TextArea.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CURCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
                //TextArea.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CURCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
                //TextArea.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
                //TextArea.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CURCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
                //TextArea.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
                //TextArea.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

                //TextArea.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);



                // ** UPDATE LEXER TO XML **
                TextArea.Lexer = Lexer.Xml;
                TextArea.StyleClearAll();

                // ** Configure the CPP Lexer styles **
                TextArea.Styles[Style.Xml.Comment].ForeColor = Color.Gray;
                TextArea.Styles[Style.Xml.Tag].ForeColor = Color.White;
                TextArea.Styles[Style.Xml.Attribute].ForeColor = Color.Red;
                TextArea.Styles[Style.Xml.DoubleString].ForeColor = Color.Yellow;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupLogScintilla()
        {
            try
            {
                // ** Initialise Scintilla Control Surface **
                LogTextArea = new ScintillaNET.Scintilla();
                pnlLogs.Controls.Add(LogTextArea);
                LogTextArea.Dock = System.Windows.Forms.DockStyle.Fill;
                LogTextArea.WrapMode = WrapMode.None;
                LogTextArea.IndentationGuides = IndentView.LookBoth;

                // Configuring the default style with properties **
                LogTextArea.StyleResetDefault();
                LogTextArea.Styles[Style.Default].Font = "Consolas";
                LogTextArea.Styles[Style.Default].Size = 8;
                LogTextArea.Styles[Style.Default].BackColor = Color.Black;
                LogTextArea.Styles[Style.Default].ForeColor = Color.White;

                // ** Number Margin **
                LogTextArea.Styles[Style.LineNumber].ForeColor = Color.White;
                LogTextArea.Styles[Style.LineNumber].BackColor = Color.Black;
                LogTextArea.Styles[Style.IndentGuide].ForeColor = Color.White;
                LogTextArea.Styles[Style.IndentGuide].BackColor = Color.Black;
                var nums = LogTextArea.Margins[NUMBER_MARGIN];
                nums.Width = 30;
                nums.Type = MarginType.Number;
                nums.Sensitive = true;
                nums.Mask = 0;

                // ** UPDATE LEXER TO XML **
                LogTextArea.Lexer = Lexer.Cpp;
                LogTextArea.StyleClearAll();


                // ** Configure the CPP Lexer styles **
                LogTextArea.Styles[Style.Cpp.Word].ForeColor = Color.CornflowerBlue;
                LogTextArea.Styles[Style.Cpp.Word2].ForeColor = Color.Orange;
                LogTextArea.Styles[Style.Cpp.String].ForeColor = Color.Yellow;
                LogTextArea.Styles[Style.Cpp.GlobalClass].ForeColor = Color.Red;

                // Set Keywords for Language **
                LogTextArea.SetKeywords(0, "INFO DEBUG");
                LogTextArea.SetKeywords(1, "WARN");
                LogTextArea.SetKeywords(3, "ERROR");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region ** BUTTON FUNCTIONS **

        private void btnExit_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshScreen();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            try
            {
                int factor = 3;
                //Bitmap original = (Bitmap)pnlObjectMapBitmap.BackgroundImage;
                Bitmap resized = new Bitmap(orig_pnlObjectMapBitmap, new Size(orig_pnlObjectMapBitmap.Width * factor, orig_pnlObjectMapBitmap.Height * factor));
                //int currentWidth = orig_pnlObjectMapBitmap.Width * factor;
                //int currentHeight = orig_pnlObjectMapBitmap.Height * factor;
                //Bitmap resized = ResizeImage(orig_pnlObjectMapBitmap, currentWidth, currentHeight);
                //pnlObjectMapBitmap.BackgroundImage = resized;
                pbObjectMapBitmap.Image = resized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnResetZoom_Click(object sender, EventArgs e)
        {
            try
            {
                //pnlObjectMapBitmap.BackgroundImage = orig_pnlObjectMapBitmap;
                pbObjectMapBitmap.Image = orig_pnlObjectMapBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgRouteRequestSummary_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Handle_dgRouteRequestSummary_CellClick(sender, e);
        }

        private void btnPerfDataCopyToClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgPerfDataSummary.Rows.Count == 0)
                {
                    throw new Exception("No data to copy from " + dgPerfDataSummary.Name + "...");
                }
                StringBuilder sb = GenerateClipboardStringFromDataTable(dgPerfDataSummary, false);
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void fldLogsFilterText_TextChanged(object sender, EventArgs e)
        {
            Handle_fldLogsFilterText_TextChanged();
        }

        private void fldRequestingObjectAc_TextChanged(object sender, EventArgs e)
        {
            ProcessRouteRequestSummaryFilter();
        }

        private void chkLongRunningAc_CheckedChanged(object sender, EventArgs e)
        {
            ProcessRouteRequestSummaryFilter();
        }

        #endregion

        #region ** REFRESH FUNCTINS **

        private void RefreshScreen()
        {
            try
            {
                // ** CLEAR FIELDS **
                fldLogsFilterText.Text = "";
                fldLogsLineFilteredCount.Text = "";
                fldRequestingObjectAc.Text = "";
                chkLongRunningAc.Checked = false;

                #region ** LOAD GLOBAL MAP XML INTO OBJECT **
                string xmlString = File.ReadAllText(@"C:\SETTLERS STUFF\Debug\Current_GlobalMap.xml");
                globalMap = new GlobalMap().DeserialiseFromXMLString(xmlString);
                TextArea.Text = XDocument.Parse(xmlString).ToString();
                #endregion

                #region ** POST LOG FILE DFETAILS **
                string logFilePath = @"C:\SETTLERS STUFF\Debug\Logs\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                origLogLines = new List<string>();                
                using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (sr.Peek() >= 0) // reading the old data
                        {
                            origLogLines.Add(sr.ReadLine() + Environment.NewLine);
                        }
                    }
                }
                string fileText = string.Join("", origLogLines);
                LogTextArea.Text = fileText;
                fldLogsLineCount.Text = origLogLines.Count.ToString("#,##0") + " row(s)";
                #endregion

                #region ** POPULATE GlobalMap BOXES **                
                fldMapFactor.Text = globalMap.MapFactor.ToString();
                fldTopLeft.Text = "(" + globalMap.MapTopLeftX + "," + globalMap.MapTopLeftY + ")";
                fldMapWidth.Text = globalMap.MapWidth.ToString();
                fldMapHeight.Text = globalMap.MapHeight.ToString();                
                fldUnityTopLeft.Text = "(" + globalMap.MapScreenTopLeftX + "," + globalMap.MapScreenTopLeftY + ")";
                fldUnityMapWidth.Text = globalMap.MapScreenWidth.ToString();
                fldUnityMapHeight.Text = globalMap.MapScreenHeight.ToString();
                pnlMapScreenShot.BackgroundImage = null;
                pnlObjectMapBitmap.BackgroundImage = null;
                dgCoreObjectMap.DataSource = null;               
                dgPerfDataSummary.DataSource = null;
                #endregion

                #region ** GENERATE ROUTE REQUEST SUMMARY **

                // ** GENERATE ROUTE REQUEST COLUMNS **
                dgRouteRequestSummaryTable_Orig = new DataTable("routeRequestTable", "routeRequestTable");
                dgRouteRequestSummaryTable_Orig.Columns.Add("ID", typeof(int));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Requesting Object", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Created TS", typeof(DateTime));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Status", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Priority", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Route Found", typeof(bool));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Time Taken - Request", typeof(double));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Time Taken - Calculation", typeof(double));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Actual Start Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Actual Target Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Real Start Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Real Target Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Core Start Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Core Target Point", typeof(string));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Use Diagonals", typeof(bool));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Node Count (Open)", typeof(int));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Node Count (Closed)", typeof(int));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Long Running", typeof(bool));
                dgRouteRequestSummaryTable_Orig.Columns.Add("Ignored Object Refs", typeof(string));

                // ** POPULATE ROUTE REQUEST SUMMARY **                
                List<RouteRequest> rrList = (from r in globalMap.RouteRequestList_Closed.RouteRequestList
                                            select r).ToList();
                foreach (RouteRequest rr in rrList)
                {
                    // ** Build row **
                    object[] row = new object[dgRouteRequestSummaryTable_Orig.Columns.Count];
                    row[0] = rr.requestID;
                    row[1] = rr.requestingObject;
                    row[2] = rr.CreatedTS;
                    row[3] = rr.requestStatus;
                    row[4] = rr.requestPriority;
                    row[5] = rr.routeFound;
                    row[6] = rr.timeTaken_Request;
                    row[7] = rr.timeTaken_Calculation;
                    row[8] = rr.ActualStartPoint;
                    row[9] = rr.ActualTargetPoint;
                    row[10] = rr.RealStartPoint;
                    row[11] = rr.RealTargetPoint;
                    row[12] = rr.CoreStartPoint;
                    row[13] = rr.CoreTargetPoint;
                    row[14] = rr.useDiagonals;
                    row[15] = rr.nodeCountOpen;
                    row[16] = rr.nodeCountClosed;
                    bool longRunning = false;
                    if (rr.timeTaken_Request > RouteCalc_MaxWaitMS) longRunning = true;                    
                    row[17] = longRunning;
                    row[18] = rr.ignoredObjectRefs;
                    dgRouteRequestSummaryTable_Orig.Rows.Add(row);
                }
                dgRouteRequestSummary.DataSource = dgRouteRequestSummaryTable_Orig;

                // ** ADJUST ROUTE REQUEST SUMMARY SCREEN **
                AdjustSummaryRowFormatting(dgRouteRequestSummary);
                lblRouteRequestCount.Text = rrList.Count.ToString("#,##0") + " Request(s)";

                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AdjustSummaryRowFormatting(DataGridView dg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => AdjustSummaryRowFormatting(dg)));
            }
            else
            {
                // ** Format columns **
                dg.Columns["Created TS"].DefaultCellStyle.Format = "dd-MMM-yy HH:mm:ss";
                dg.Columns["Time Taken - Request"].DefaultCellStyle.Format = "#,###.00";
                dg.Columns["Time Taken - Calculation"].DefaultCellStyle.Format = "#,###.00";
                dg.Columns["Node Count (Open)"].DefaultCellStyle.Format = "#,##0";
                dg.Columns["Node Count (Closed)"].DefaultCellStyle.Format = "#,##0";
                //dg.Columns["Part Image"].HeaderText = "";
                //((DataGridViewImageColumn)dg.Columns["Part Image"]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                //dg.Columns["Colour Image"].HeaderText = "";
                //((DataGridViewImageColumn)dg.Columns["Colour Image"]).ImageLayout = DataGridViewImageCellLayout.Zoom;
                dg.AutoResizeColumns();

                // ** HIGHLIGHT LONG RUNNING ROUTES AS ORANGE **
                foreach (DataGridViewRow row in dg.Rows)
                {
                    if ((bool)row.Cells["Long Running"].Value == true)
                    //if ((double)row.Cells["Time Taken - Request"].Value > RouteCalc_MaxWaitMS)
                    {
                        row.DefaultCellStyle.BackColor = Color.Orange;
                    }  
                }

            }
        }

        #endregion

        #region ** MOUSE MOVE/ENTER/CLICK FUNCTIONS **

        private void pnlObjectMapBitmap_MouseLeave(object sender, EventArgs e)
        {
            lblObjectMapBitmapStatus.Text = "";
        }

        private void pnlObjectMapBitmap_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (globalMap != null)
                {
                    int mapWidth = globalMap.MapWidth;
                    int mapHeight = globalMap.MapHeight;
                    Point corePoint = new Point(e.X, e.Y);
                    if (corePoint.X < mapWidth && corePoint.Y < mapHeight)
                    {
                        // ** Get variables **
                        Point topLeft = new Point(globalMap.MapTopLeftX, globalMap.MapTopLeftY);
                        Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                        double mapFactor = globalMap.MapFactor;
                        double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                        double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                        // Post label details **
                        lblObjectMapBitmapStatus.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";
                    }
                    else
                    {
                        lblObjectMapBitmapStatus.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pnlObjectMapBitmap_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                fldCoreObjectMapLastSelectedPoint.Text = "";
                if (globalMap != null)
                {
                    int mapWidth = globalMap.MapWidth;
                    int mapHeight = globalMap.MapHeight;
                    Point corePoint = new Point(e.X, e.Y);
                    if (corePoint.X < mapWidth && corePoint.Y < mapHeight)
                    {
                        // ** Get variables **
                        Point topLeft = new Point(globalMap.MapTopLeftX, globalMap.MapTopLeftY);
                        Point realPoint = Point.ConvertCorePointToRealPoint(corePoint, topLeft);
                        double mapFactor = globalMap.MapFactor;
                        double actualPointX = Convert.ToDouble(realPoint.X) / mapFactor;
                        double actualPointY = Convert.ToDouble(realPoint.Y) / mapFactor;

                        // Post label details **
                        fldObjectMapLastSelectedPoint.Text = "Core={" + corePoint.X + "," + corePoint.Y + "}  |  Real={" + realPoint.X + "," + realPoint.Y + "}  |  Actual={" + actualPointX + "," + actualPointY + "}";
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        private void fldSquareSize_Leave(object sender, EventArgs e)
        {
            try
            {
                // ** Update square size **
                squareSize = int.Parse(fldSquareSize.Text);

                // ** Update column/row widths/heights **                
                for (int a = 0; a < dgCoreObjectMap.Columns.Count; a++) dgCoreObjectMap.Columns[a].Width = squareSize;               
                for (int a = 0; a < dgCoreObjectMap.Rows.Count; a++) dgCoreObjectMap.Rows[a].Height = squareSize;                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Handle_dgRouteRequestSummary_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // ** Clear existing data **
                pbMapScreenShot.Image = null;
                pbObjectMapBitmap.Image = null;
                dgCoreObjectMap.DataSource = null;
                orig_pnlObjectMapBitmap = null;               
                dgPerfDataSummary.DataSource = null;

                // ** GET ROUTE REQUEST DETAILS **
                int requestID = (int)dgRouteRequestSummary.SelectedCells[0].OwningRow.Cells["ID"].Value;
                RouteRequest rr = (from r in globalMap.RouteRequestList_Closed.RouteRequestList
                                   where r.requestID == requestID
                                   select r).FirstOrDefault();
                
                // ** POST SCREENSHOT IMAGE **
                if(File.Exists(DebugLocation + rr.MapScreenShotFilename))
                {
                    pbMapScreenShot.Image = Image.FromFile(DebugLocation + @"Screenshots\" + rr.MapScreenShotFilename);
                }

                #region ** BUILD PERF DATA SUMMARY **
                DataTable PerfDataTable = new DataTable("PerfDataTable", "PerfDataTable");
                PerfDataTable.Columns.Add("Node", typeof(string));
                PerfDataTable.Columns.Add("Check for Time", typeof(double));
                PerfDataTable.Columns.Add("Get Lowest F Node", typeof(double));
                PerfDataTable.Columns.Add("Mark Node as Closed", typeof(double));
                PerfDataTable.Columns.Add("Get Current Node Surrounding Positions", typeof(double));
                PerfDataTable.Columns.Add("Create Entries for New Positions", typeof(double));
                PerfDataTable.Columns.Add("Nodes (Open)", typeof(int));
                PerfDataTable.Columns.Add("Nodes (Closed)", typeof(int));                 
                if(rr.PerformanceData != null)
                {
                    List<string> PerfDataList = rr.PerformanceData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    PerfDataList.RemoveAt(0);   // Remove header
                    foreach (string line in PerfDataList)
                    {
                        List<string> valueList = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        object[] row = new object[valueList.Count];
                        for (int a = 0; a < valueList.Count; a++)
                        {
                            row[a] = valueList[a];
                        }
                        PerfDataTable.Rows.Add(row);
                    }
                }
                dgPerfDataSummary.DataSource = PerfDataTable;
                lblPerfDataSummaryCount.Text = PerfDataTable.Rows.Count.ToString("#,##0") + " row(s)";
                #endregion


                #region ** PROCESS IF CORE DATA IS AVAILABLE **                
                if (rr.ObjectMapByte != null)
                {
                    // ** GET VARIABLES **
                    Bitmap ObjectMapBitmap = new Bitmap(new MemoryStream(rr.ObjectMapByte));
                    int[,] ObjectMap = GlobalMap.GenerateObjectMapFromBitMap(ObjectMapBitmap);
                    Point topLeft = new Point(globalMap.MapTopLeftX, globalMap.MapTopLeftY);
                    Point coreStartPoint = new Point(rr.CoreStartPoint);
                    Point coreTargetPoint = new Point(rr.CoreTargetPoint);
                    bool routeFound = rr.routeFound;

                    // ** GET PACKAGE DETAILS **
                    List<Point> routeCorePointList = new List<Point>();
                    List<Node> nodeList_Open = new List<Node>();
                    List<Node> nodeList_Closed = new List<Node>();
                    string packagePath = DebugLocation + @"RouteRequestPackages\" + rr.PackageFilename;
                    if (File.Exists(packagePath))
                    {
                        RouteRequestPackage rrp = new RouteRequestPackage().DeserialiseFromXMLString(File.ReadAllText(packagePath));
                        routeCorePointList = rrp.routeCorePointList;
                        nodeList_Open = rrp.nodeList_Open;
                        nodeList_Closed = rrp.nodeList_Closed;
                    }






                    #region ** GENERATE CORE OBJECT MAP DATATABLE **

                    // ** Save CoreObjectMap to DataTable **                
                    DataTable CoreObjectMapTable = new DataTable("CoreObjectMapTable", "CoreObjectMapTable");
                    for (int a = 0; a < globalMap.MapWidth; a++)
                    {
                        CoreObjectMapTable.Columns.Add(a.ToString(), typeof(string));
                    }
                    for (int a = 0; a < globalMap.MapHeight; a++)
                    {
                        object[] row = new object[CoreObjectMapTable.Columns.Count];
                        for (int b = 0; b < globalMap.MapWidth; b++)
                        {
                            Point corePoint = new Point(b, a);
                            //row[b] = "{" + corePoint.X + "," + corePoint.Y + "}";
                            row[b] = "";
                        }
                        CoreObjectMapTable.Rows.Add(row);
                    }
                    dgCoreObjectMap.DataSource = CoreObjectMapTable;

                    // ** Update column/row widths/heights **                
                    for (int a = 0; a < globalMap.MapWidth; a++)
                    {
                        dgCoreObjectMap.Columns[a].Width = squareSize;
                        dgCoreObjectMap.Columns[a].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgCoreObjectMap.Columns[a].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgCoreObjectMap.Columns[a].DefaultCellStyle.Font = new Font(this.Font.FontFamily, fontSize);
                    }
                    for (int a = 0; a < globalMap.MapHeight; a++) dgCoreObjectMap.Rows[a].Height = squareSize;

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

                    // ** Add obstacles - CoreObjectMap **
                    if (rr.ObjectMapByte != null)
                    {
                        for (int a = 0; a < globalMap.MapWidth; a++)
                        {
                            for (int b = 0; b < globalMap.MapHeight; b++)
                            {
                                int walkValue = ObjectMap[a, b];
                                if (walkValue == -1)
                                {
                                    Point corePoint = new Point(a, b);
                                    dgCoreObjectMap.Rows[corePoint.Y].Cells[corePoint.X].Style.BackColor = Color.Gray;
                                }
                            }
                        }
                    }

                    #endregion




                    // ** RUN CALCULATION TO GET ROUTE  **
                    // ##### (TO BE REPLACED WITH PERSISTED DATA) #####
                    // ** Calculate route from Start Point to Target Point **
                    //bool allowDiagonals = true;
                    //bool routeFound = false;
                    //List<Point> routeCorePointList = new List<Point>();
                    //List<Node> nodeList_Open = new List<Node>();
                    //List<Node> nodeList_Closed = new List<Node>();
                    //DateTime overallStartTime = DateTime.Now;
                    //int maxRouteCalcWaitSecs = 20;
                    //GlobalMap.CalculateRoute(coreStartPoint, coreTargetPoint, ObjectMap, allowDiagonals, maxRouteCalcWaitSecs, out routeFound, out routeCorePointList, out nodeList_Open, out nodeList_Closed);
                    //TimeSpan ts = DateTime.Now - overallStartTime;
                    // ##### (TO BE REPLACED WITH PERSISTED DATA) #####





                    // ** RENDER DATA TO BITMAP AND DATATABLE **
                    Bitmap markedBitmap = (Bitmap)ObjectMapBitmap.Clone();

                    #region ** Mark CLOSED Nodes **                
                    for (int a = 0; a < nodeList_Closed.Count; a++)
                    {
                        Node n = nodeList_Closed[a];
                        Point nodePosition = n.nodePosition;

                        // ** Update ObjectMap bitmap **
                        markedBitmap.SetPixel(nodePosition.X, nodePosition.Y, Color.LightBlue);

                        // ** Update CoreObjectMap table **            
                        //dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightBlue;
                        //dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = n.F;
                        //DataGridViewCell coreCell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                        //coreCell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;
                    }
                    #endregion

                    #region ** Mark OPEN Nodes **                
                    for (int a = 0; a < nodeList_Open.Count; a++)
                    {
                        Node n = nodeList_Open[a];
                        Point nodePosition = n.nodePosition;

                        // ** Update ObjectMap bitmap **
                        markedBitmap.SetPixel(nodePosition.X, nodePosition.Y, Color.LightGray);

                        // ** Update CoreObjectMap table **
                        //dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Style.BackColor = Color.LightGray;
                        //dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X].Value = nodeList_Open[a].F;
                        //DataGridViewCell cell = dgCoreObjectMap.Rows[nodePosition.Y].Cells[nodePosition.X];
                        //cell.ToolTipText = "{" + nodePosition.X + "," + nodePosition.Y + "} | F=" + n.F + ", G=" + n.G + ", H=" + n.H;
                    }
                    #endregion

                    #region ** CHECK IF ROUTE FOUND **
                    if (routeFound)
                    {
                        // ** Mark route **
                        for (int a = 0; a < routeCorePointList.Count; a++)
                        {
                            Point wayPoint = routeCorePointList[a];

                            // ** Update ObjectMap bitmap **
                            markedBitmap.SetPixel(wayPoint.X, wayPoint.Y, Color.Green);

                            // ** Update CoreObjectMap table **
                            //dgCoreObjectMap.Rows[wayPoint.Y].Cells[wayPoint.X].Style.BackColor = Color.Green;
                        }
                    }
                    #endregion

                    #region ** Set START and END points **
                    // ** Update ObjectMap bitmap **
                    markedBitmap.SetPixel(coreStartPoint.X, coreStartPoint.Y, Color.Blue);
                    markedBitmap.SetPixel(coreTargetPoint.X, coreTargetPoint.Y, Color.Red);

                    // ** Update CoreObjectMap table **
                    //dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.BackColor = Color.Blue;
                    //dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.ForeColor = Color.White;
                    //dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                    //dgCoreObjectMap.Rows[coreStartPoint.Y].Cells[coreStartPoint.X].Value = "S";
                    //dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.BackColor = Color.Red;
                    //dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.ForeColor = Color.White;
                    //dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Style.Font = new Font(this.Font.FontFamily, 14f);
                    //dgCoreObjectMap.Rows[coreTargetPoint.Y].Cells[coreTargetPoint.X].Value = "F";
                    #endregion

                    pbObjectMapBitmap.Image = markedBitmap;
                    orig_pnlObjectMapBitmap = markedBitmap;
                }
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static StringBuilder GenerateClipboardStringFromDataTable(DataGridView dg, bool includeHeader)
        {
            StringBuilder sb = new StringBuilder();
            if (includeHeader)
            {
                foreach (DataGridViewColumn column in dg.Columns)
                {
                    if (column.Visible && column.ValueType.Name != "Bitmap") sb.Append(column.Name + ",");
                }
                sb.Append(Environment.NewLine);
            }
            foreach (DataGridViewRow row in dg.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn.Visible && cell.ValueType.Name != "Bitmap") sb.Append(cell.Value + ",");
                }
                sb.Append(Environment.NewLine);
            }
            return sb;
        }

        private void Handle_fldLogsFilterText_TextChanged()
        {
            try
            {
                string[] stringArray = fldLogsFilterText.Text.Split('|');
                fldLogsLineFilteredCount.Text = "";
                List<string> newLogLines = new List<string>();
                foreach (string line in origLogLines)
                {
                    if (stringArray.All(line.Contains)) newLogLines.Add(line);
                }
                LogTextArea.Text = string.Join("", newLogLines);
                fldLogsLineCount.Text = origLogLines.Count.ToString("#,##0") + " row(s)";
                if (newLogLines.Count != origLogLines.Count) fldLogsLineFilteredCount.Text = newLogLines.Count.ToString("#,##0") + " filtered row(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProcessRouteRequestSummaryFilter()
        {
            try
            {
                if (dgRouteRequestSummaryTable_Orig.Rows.Count > 0)
                {
                    // ** Reset summaey screen **
                    lblRouteRequestSummaryItemFilteredCount.Text = "";
                    dgRouteRequestSummary.DataSource = dgRouteRequestSummaryTable_Orig;
                    AdjustSummaryRowFormatting(dgRouteRequestSummary);

                    // ** Determine what filters have been applied **
                    if (fldRequestingObjectAc.Text != "" || chkLongRunningAc.Checked == true)
                    {
                        List<DataRow> filteredRows = dgRouteRequestSummaryTable_Orig.AsEnumerable().CopyToDataTable().AsEnumerable().ToList();

                        #region ** Apply filtering for Requesting Object **
                        if (filteredRows.Count > 0)
                        {
                            //if (chkLDrawRefAcEquals.Checked)
                            //{
                            //    filteredRows = filteredRows.CopyToDataTable().AsEnumerable()
                            //                                .Where(row => row.Field<string>("LDraw Ref").ToUpper().Equals(fldLDrawRefAc.Text.ToUpper()))
                            //                                .ToList();
                            //}
                            //else
                            //{
                            filteredRows = filteredRows.CopyToDataTable().AsEnumerable()
                                                        .Where(row => row.Field<string>("Requesting Object").ToUpper().Contains(fldRequestingObjectAc.Text.ToUpper()))
                                                        .ToList();
                            //}
                        }
                        #endregion

                        #region ** Apply filtering for Long Running **
                        if (chkLongRunningAc.Checked)
                        {
                            filteredRows = filteredRows.CopyToDataTable().AsEnumerable().Where(row => row.Field<bool>("Long Running") == true).ToList();
                        }
                        #endregion

                        #region ** Apply filters **
                        if (filteredRows.Count > 0)
                        {
                            //Delegates.DataGridView_SetDataSource(this, dgPartSummary, filteredRows.CopyToDataTable());
                            dgRouteRequestSummary.DataSource = filteredRows.CopyToDataTable();
                            AdjustSummaryRowFormatting(dgRouteRequestSummary);
                        }
                        else
                        {
                            //Delegates.DataGridView_SetDataSource(this, dgPartSummary, null);
                            dgRouteRequestSummary.DataSource = null;
                        }
                        lblRouteRequestSummaryItemFilteredCount.Text = filteredRows.Count + " filtered Request(s)";
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

       

        
        
    }
}




