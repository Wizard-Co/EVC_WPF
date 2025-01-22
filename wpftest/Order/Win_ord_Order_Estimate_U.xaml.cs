using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Text;
using WPF.MDI;
using System.Windows.Interop;
using System.ComponentModel.Design;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using WizMes_EVC.PopUp;
using WizMes_EVC.PopUP;
using WizMes_EVC.Order.Pop;
using System.Windows.Data;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Order_Estimate_U.xaml.cs
'** 설명       : 상담등록
'** 작성일자   : 2024.12.09
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.12.09, 최대현, 최초작성 관련 내용이 많은 계약등록 화면을 복사하여 작성
' 2024.12.24, 최대현, 견적서 프린터 메서드 작성
'**************************************************************************************************/

namespace WizMes_EVC
{
    /// <summary>
    /// Win_ord_Order_Reservation_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_Order_Estimate_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        string strFlag = string.Empty;
        string estID_Global = string.Empty;
        string articleID_global = string.Empty;
        int rowNum = 0;
        int intFlag = 0;

        private Win_ord_Pop_PreEstimate_Q preEstimate;

        Win_ord_Order_Estimate_U_CodeView EstView = new Win_ord_Order_Estimate_U_CodeView();
        //Win_ord_Order_Reservation_U_CodeView_Nadaum ReserveView = new Win_ord_Order_Reservation_U_CodeView_Nadaum();

        

        //견적
        ObservableCollection<Win_ord_Order_EstimateSub_U_CodeView> ovcOrder_EstSub
        = new ObservableCollection<Win_ord_Order_EstimateSub_U_CodeView>();

     

        //ObservableCollection<Win_ord_Order_Reservation_U_CodeView_RsrvStudent_Nadaum> ovcOrder_RsvrStudent
        //= new ObservableCollection<Win_ord_Order_Reservation_U_CodeView_RsrvStudent_Nadaum>();

        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;

        ArticleData articleData = new ArticleData();
        SetCompanyData companyData = new SetCompanyData();
        string PrimaryKey = string.Empty;

        string FullPath1 = string.Empty;
        string FullPath2 = string.Empty;
        string FullPath3 = string.Empty;
        string FullPath4 = string.Empty;
        string FullPath5 = string.Empty;

        //과거이력조회
        //private Win_ord_Pop_PreReservation preReservation;

        //FTP 활용모음
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;
        string strDelFileName = string.Empty;

        List<string[]> deleteListFtpFile = new List<string[]>(); // 삭제할 파일 리스트
        List<string[]> lstExistFtpFile = new List<string[]>();

        // 촤! FTP Server 에 있는 폴더 + 파일 경로를 저장해놓고 그걸로 다운 및 업로드하자 마!
        // 이미지 이름 : 폴더이름
        Dictionary<string, string> lstFtpFilePath = new Dictionary<string, string>();


        private FTP_EX _ftp = null;
        string SketchPath = null;

        List<string[]> listFtpFile = new List<string[]>();
        HashSet<string> lstFilesName = new HashSet<string>();
        private List<UploadFileInfo> _listFileInfo = new List<UploadFileInfo>();

        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Estimate";
        //string FTP_ADDRESS_ORDER = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/contract";

        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        internal struct UploadFileInfo          //FTP.
        {
            public string Filename { get; set; }
            public FtpFileType Type { get; set; }
            public DateTime LastModifiedTime { get; set; }
            public long Size { get; set; }
            public string Filepath { get; set; }
        }

        internal enum FtpFileType
        {
            None,
            DIR,
            File
        }

        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/InspectAuto";

        ////string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/McRegularInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/McRegularInspect";



        public Win_ord_Order_Estimate_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");
            //tblReServeID.Text = string.Empty;

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            btnToday_Click(null, null);
            SetComboBox();

            dgdEstItemList.ItemsSource = ovcOrder_EstSub;

            if (!string.IsNullOrEmpty(MainWindow.EstID))
            {
                intFlag = 1;
                tblEstIDHidden.Text = MainWindow.EstID;
                lblDateSrh_MouseLeftButtonDown(null, null);

                FillGrid();

                intFlag = 0;
                tblEstIDHidden.Text = string.Empty;

                if (dgdMain.Items.Count > 0) dgdMain.SelectedIndex = 0;
                MainWindow.EstID = string.Empty;
            }
        }

        //콤보박스 만들기
        private void SetComboBox()
        {
            //EVC용
            //지역구분(ZoneID)
            ObservableCollection<CodeView> ovcZoneGbnID = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CSTZneGbn", "Y", "", "");
            cboZoneGbnIdSrh.ItemsSource = ovcZoneGbnID;
            cboZoneGbnIdSrh.DisplayMemberPath = "code_name";
            cboZoneGbnIdSrh.SelectedValuePath = "code_id";
            cboZoneGbnIdSrh.SelectedIndex = 0;

            //시설구분(ZoneID)
            ObservableCollection<CodeView> ovcFaciliTypeID = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "FCTTypeID", "Y", "", "");
            cboFaciliTypeID.ItemsSource = ovcFaciliTypeID;
            cboFaciliTypeID.DisplayMemberPath = "code_name";
            cboFaciliTypeID.SelectedValuePath = "code_id";
            cboFaciliTypeID.SelectedIndex = 0;

            //시공환경(ZoneID)
            ObservableCollection<CodeView> ovcInstallConID = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "INSConID", "Y", "", "");
            cboInstallLocationConditionID.ItemsSource = ovcInstallConID;
            cboInstallLocationConditionID.DisplayMemberPath = "code_name";
            cboInstallLocationConditionID.SelectedValuePath = "code_id";
            cboInstallLocationConditionID.SelectedIndex = 0;

            //전기조달(검색조건)
            ObservableCollection<CodeView> ovcElecDeliMethSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboElecDeliMethSrh.ItemsSource = ovcElecDeliMethSrh;
            cboElecDeliMethSrh.DisplayMemberPath = "code_name";
            cboElecDeliMethSrh.SelectedValuePath = "code_id";
            cboElecDeliMethSrh.SelectedIndex = 0;

            //전기조달(그리드)
            ObservableCollection<CodeView> ovcElecDeliMeth = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboElectrDeliveryMethodID.ItemsSource = ovcElecDeliMeth;
            cboElectrDeliveryMethodID.DisplayMemberPath = "code_name";
            cboElectrDeliveryMethodID.SelectedValuePath = "code_id";
            cboElectrDeliveryMethodID.SelectedIndex = 0;

            //견적 승인(array)
            List<string[]> strArray = new List<string[]>();
            string[] strOne = { "N", "N" };
            string[] strTwo = { "Y", "Y" };
            strArray.Add(strOne);
            strArray.Add(strTwo);

            // 견적 승인
            ObservableCollection<CodeView> ovcEstApprovalYN = ComboBoxUtil.Instance.Direct_SetComboBox(strArray);
            cboEstApprovalYN.ItemsSource = ovcEstApprovalYN;
            cboEstApprovalYN.DisplayMemberPath = "code_name";
            cboEstApprovalYN.SelectedValuePath = "code_id";
            cboEstApprovalYN.SelectedIndex = 0;

        }

        #region 체크박스 연동동작(상단)

        //수주일자
        private void lblDateSrh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChkDateSrh.IsChecked = ChkDateSrh.IsChecked == true ? false : true;
        }

        //수주일자
        private void ChkDateSrh_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpSDate != null && dtpEDate != null)
            {
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        //수주일자
        private void ChkDateSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpSDate.SelectedDate != null)
                {
                    dtpSDate.SelectedDate = dtpSDate.SelectedDate.Value.AddDays(-1);
                    dtpEDate.SelectedDate = dtpSDate.SelectedDate;
                }
                else
                {
                    dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
                    dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - btnYesterday_Click : " + ee.ToString());
            }
        }

        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpSDate.SelectedDate != null)
                {
                    DateTime ThatMonth1 = dtpSDate.SelectedDate.Value.AddDays(-(dtpSDate.SelectedDate.Value.Day - 1)); // 선택한 일자 달의 1일!

                    DateTime LastMonth1 = ThatMonth1.AddMonths(-1); // 저번달 1일
                    DateTime LastMonth31 = ThatMonth1.AddDays(-1); // 저번달 말일

                    dtpSDate.SelectedDate = LastMonth1;
                    dtpEDate.SelectedDate = LastMonth31;
                }
                else
                {
                    DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                    DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                    DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                    dtpSDate.SelectedDate = LastMonth1;
                    dtpEDate.SelectedDate = LastMonth31;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - btnLastMonth_Click : " + ee.ToString());
            }
        }




        #endregion



        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnSave.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
            btnExcel.Visibility = Visibility.Visible;
            btnUpload.IsEnabled = true;

            grdInput.IsHitTestVisible = false;
            grdFiles.IsHitTestVisible = false;
            lblMsg.Visibility = Visibility.Hidden;
            dgdMain.IsHitTestVisible = true;
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            btnExcel.Visibility = Visibility.Hidden;
            btnUpload.IsEnabled = false;

            grdInput.IsHitTestVisible = true;
            grdFiles.IsHitTestVisible= true;
            lblMsg.Visibility = Visibility.Visible;
            dgdMain.IsHitTestVisible = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "I";
            this.DataContext = new object();
            articleID_global = string.Empty;
            estID_Global = string.Empty;

            //lstFilesName.Clear();

            if (dgdEstItemList.Items.Count > 0) ovcOrder_EstSub.Clear();
         
            SetDatePickerToday();
            SetComboBoxIndex();
            
            CantBtnControl();            
      
            //setFTP_Tag_EmptyString();

            tbkMsg.Text = "자료 입력 중";
            rowNum = Math.Max(0, dgdMain.SelectedIndex);      
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            EstView = dgdMain.SelectedItem as Win_ord_Order_Estimate_U_CodeView;

            if (EstView != null)
            {
                //rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                strFlag = "U";
                CantBtnControl();
                PrimaryKey = EstView.EstID;

            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beDelete))
            {
                ld.ShowDialog();
            }
        }

        //점1
        private void beDelete()
        {
            btnDelete.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {              
            
                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                        if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                        rowNum = dgdMain.SelectedIndex;

        //        FTP_RemoveDir(ReserveView.reServeID);

                    if (DeleteData(estID_Global))
                    {
                        rowNum = Math.Max(0, rowNum - 1);
                        re_Search(rowNum);
                    }
            
                }
            
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnDelete.IsEnabled = true;
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
           
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beSearch))
            {
                ld.ShowDialog();
            }
        }

        private void beSearch()
        {
            rowNum = 0;
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                re_Search(rowNum);
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beSave))
            {
                ld.ShowDialog();
            }
        }

        private void beSave()
        {
            btnSave.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    lblMsg.Visibility = Visibility.Hidden;
                    estID_Global = string.Empty;
                    articleID_global = string.Empty;
                    dgdMain.IsHitTestVisible = true;
                    rowNum = strFlag == "I" ? rowNum + 1 : strFlag == "U" ? rowNum : rowNum - 1;
                    re_Search(rowNum);                  
                    PrimaryKey = string.Empty;
                    //rowNum = 0;
                    MessageBox.Show("저장이 완료되었습니다.");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSave.IsEnabled = true;
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();

            dgdMain.IsHitTestVisible = true;

            if (strFlag.Equals("U"))
            {
                re_Search(rowNum);
            }
            else
            {
                rowNum = 0;
                re_Search(rowNum);
            }

            strFlag = string.Empty;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] lst = new string[2];
            lst[0] = "상담 조회 목록";
            lst[1] = dgdMain.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdMain);
                    else
                        dt = lib.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;

                    if (lib.GenerateExcel(dt, Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                        return;
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }
            lib = null;
        }

        // 주문일괄 업로드
        string upload_fileName = "";

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();
            file.Filter = "Excel files (*.xls,*xlsx)|*.xls;*xlsx|All files (*.*)|*.*";
            file.InitialDirectory = "C:\\";

            if (file.ShowDialog() == true)
            {
                upload_fileName = file.FileName;

                btnUpload.IsEnabled = false;

                using (Loading ld = new Loading("excel", beUpload))
                {
                    ld.ShowDialog();
                }

                re_Search(0);

                btnUpload.IsEnabled = true;
            }
        }

        private void beUpload()
        {
            Lib lib2 = new Lib();

            Excel.Application excelapp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range workrange = null;

            List<OrderExcel> listExcel = new List<OrderExcel>();

            try
            {
                excelapp = new Excel.Application();
                workbook = excelapp.Workbooks.Add(upload_fileName);
                worksheet = workbook.Sheets["Sheet"];
                workrange = worksheet.UsedRange;

                for (int row = 3; row <= workrange.Rows.Count; row++)
                {
                    OrderExcel excel = new OrderExcel();
                    excel.CustomID = workrange.get_Range("A" + row.ToString()).Value2;
                    excel.Model = workrange.get_Range("B" + row.ToString()).Value2;
                    excel.BuyerArticleNo = workrange.get_Range("C" + row.ToString()).Value2;
                    excel.Article = workrange.get_Range("D" + row.ToString()).Value2;
                    excel.UnitClss = workrange.get_Range("E" + row.ToString()).Value2;

                    object objOrderQty = workrange.get_Range("H" + row.ToString()).Value2;
                    if (objOrderQty != null)
                        excel.OrderQty = objOrderQty.ToString();

                    if (!string.IsNullOrEmpty(excel.CustomID)
                        && !string.IsNullOrEmpty(excel.BuyerArticleNo) && !string.IsNullOrEmpty(excel.Article)
                        && !string.IsNullOrEmpty(excel.UnitClss) && !string.IsNullOrEmpty(excel.OrderQty))
                    {
                        listExcel.Add(excel);
                    }

                    if (string.IsNullOrEmpty(excel.CustomID) && string.IsNullOrEmpty(excel.Model)
                        && string.IsNullOrEmpty(excel.BuyerArticleNo) && string.IsNullOrEmpty(excel.Article)
                        && string.IsNullOrEmpty(excel.UnitClss) && string.IsNullOrEmpty(excel.OrderQty))
                    {
                        break;
                    }
                }

                if (listExcel.Count > 0)
                {
                    List<Procedure> Prolist = new List<Procedure>();
                    List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
                    for (int i = 0; i < listExcel.Count; i++)
                    {
                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Add("CustomID", string.IsNullOrEmpty(listExcel[i].CustomID) ? "" : listExcel[i].CustomID);
                        sqlParameter.Add("Model", string.IsNullOrEmpty(listExcel[i].Model) ? "" : listExcel[i].Model);
                        sqlParameter.Add("BuyerArticleNo", string.IsNullOrEmpty(listExcel[i].BuyerArticleNo) ? "" : listExcel[i].BuyerArticleNo);
                        sqlParameter.Add("Article", string.IsNullOrEmpty(listExcel[i].Article) ? "" : listExcel[i].Article);
                        sqlParameter.Add("UnitClss", string.IsNullOrEmpty(listExcel[i].UnitClss) ? "" : listExcel[i].UnitClss);
                        sqlParameter.Add("OrderQty", string.IsNullOrEmpty(listExcel[i].OrderQty) ? "" : listExcel[i].OrderQty);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_Order_iOrderExcel";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(sqlParameter);
                    }

                    string[] Confirm = new string[2];
                    Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "C");
                    if (Confirm[0] != "success")
                        MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                    else
                        MessageBox.Show("업로드가 완료되었습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                excelapp.Visible = true;
                workbook.Close(true);
                excelapp.Quit();

                lib2.ReleaseExcelObject(workbook);
                lib2.ReleaseExcelObject(worksheet);
                lib2.ReleaseExcelObject(excelapp);
                lib2 = null;

                upload_fileName = "";
                listExcel.Clear();
            }
        }

        private int SelectItem(string strPrimary, DataGrid dataGrid)
        {
            int index = 0;

            try
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    //var Item = dataGrid.Items[i] as Win_ord_Order_Reservation_U_CodeView_Nadaum;

                    //if (strPrimary.Equals(Item.reServeID))
                    //{
                    //    index = i;
                    //    break;
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return index;
        }

        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = PrimaryKey.Equals(string.Empty) ?
                    selectedIndex : SelectItem(PrimaryKey, dgdMain);
            }
            else
                DataContext = null;

            //CalculGridSum();
        }

        //실조회
        private void FillGrid()
        {
            ovcOrder_EstSub.Clear();

            if (dgdMain.Items.Count > 0)
            {
                dgdTotal.Items.Clear();
                dgdMain.Items.Clear();
            }            

            double sumAmount = 0;

            try
            { 
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", ChkDateSrh.IsChecked == true ? (intFlag == 1 ? 0 : 1) : 0);
                sqlParameter.Add("sDate", ChkDateSrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("eDate", ChkDateSrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                sqlParameter.Add("chkManagerCustomID", chkManagerCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManagerCustomID", chkManagerCustomIdSrh.IsChecked == true ? txtManagerCustomIdSrh.Tag.ToString() : "");

                sqlParameter.Add("chkArticleID", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleSrh.IsChecked == true ? txtArticleSrh.Tag.ToString() : "");

                sqlParameter.Add("chkElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? cboElecDeliMethSrh.SelectedValue.ToString() : "");

                sqlParameter.Add("chkZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? cboZoneGbnIdSrh.SelectedValue.ToString() : "");

                sqlParameter.Add("chkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstalLocation.Text : "");

                sqlParameter.Add("chkComments", chkCommentsSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("Comments", chkCommentsSrh.IsChecked == true ? txtCommentsSrh.Text : "");

                sqlParameter.Add("chkEstSubject", chkEstSubjectSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("EstSubject", chkEstSubjectSrh.IsChecked == true ? txtEstSubjecSrh.Text : "");
                                  			

                ////수주등록에서 넘어왔을 때 바로 조회용도 textblock에 적어놓고 hidden처리함
                sqlParameter.Add("EstID", tblEstIDHidden.Text.Trim() != string.Empty ? tblEstIDHidden.Text.Trim() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Order_sEstimate", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var estItem = new Win_ord_Order_Estimate_U_CodeView
                            {
                                num = i,
                                EstID = dr["EstID"].ToString(),
                                salesCustomID = dr["salesCustomID"].ToString(),
                                salesCustom = dr["salesCustom"].ToString(),
                                managerCustomID = dr["managerCustomID"].ToString(),
                                managerCustom = dr["managerCustom"].ToString(),
                                zoneGbnID = dr["zoneGbnID"].ToString(),
                                FaciliTypeID = dr["FaciliTypeID"].ToString(),
                                FacliType = dr["FacliType"].ToString(),
                                EstReqDate = dr["EstReqDate"].ToString(),
                                EstDate = DateTypeHyphen(dr["EstDate"].ToString()),
                                InstallSchFromDate = DateTypeHyphen(dr["InstallSchFromDate"].ToString()),
                                InstallSchTODate = DateTypeHyphen(dr["InstallSchTODate"].ToString()),
                                InstalLocation = dr["InstalLocation"].ToString(),
                                smallInstalLocation = dr["smallInstalLocation"].ToString(),
                                InstallLocationPart = dr["InstallLocationPart"].ToString(),
                                InstallLocationConditionID = dr["InstallLocationConditionID"].ToString(),
                                EstSubject = dr["EstSubject"].ToString(),
                                EstDamdangName = dr["EstDamdangName"].ToString(),
                                EstDamdangTelno = dr["EstDamdangTelno"].ToString(),
                                EstApprovalYN = dr["EstApprovalYN"].ToString(),
                                EstApprovalDate = DateTypeHyphen(dr["EstApprovalDate"].ToString()),
                                EstItemList = dr["EstItemList"].ToString(),
                                electrDeliveryMethodID = dr["electrDeliveryMethodID"].ToString(),
                                deliveryCost = stringFormatN0(dr["deliveryCost"]),
                                totalAmount = stringFormatN0(dr["totalAmount"]),
                                Comments = dr["Comments"].ToString(),

                                sketch1File = dr["sketch1File"].ToString(),
                                sketch1FileAlias = dr["sketch1FileAlias"].ToString(),
                                sketch1Path = dr["sketch1Path"].ToString(),

                                sketch2File = dr["sketch2File"].ToString(),
                                sketch2FileAlias = dr["sketch2FileAlias"].ToString(),
                                sketch2Path = dr["sketch2Path"].ToString(),

                                sketch3File = dr["sketch3File"].ToString(),
                                sketch3FileAlias = dr["sketch3FileAlias"].ToString(),
                                sketch3Path = dr["sketch3Path"].ToString(),

                                sketch4File = dr["sketch4File"].ToString(),
                                sketch4FileAlias = dr["sketch4FileAlias"].ToString(),
                                sketch4Path = dr["sketch4Path"].ToString(),

                                sketch5File = dr["sketch5File"].ToString(),
                                sketch5FileAlias = dr["sketch5FileAlias"].ToString(),
                                sketch5Path = dr["sketch5Path"].ToString(),

                            };

                            sumAmount += !string.IsNullOrEmpty(estItem.totalAmount) ? Convert.ToDouble(estItem.totalAmount) : 0;                     

                            dgdMain.Items.Add(estItem);
                        }
                    }
                }

                if (dgdMain.Items.Count > 0)
                {
                    var ReserveCodeView_Total = new Win_ord_Order_Estimate_Total_U_CodeView
                    {
                        count = dgdMain.Items.Count.ToString(),
                        totalAmount = stringFormatN0(sumAmount)
                    };

                    dgdTotal.Items.Add(ReserveCodeView_Total);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

   
       

        //8자리 char형태 날짜 년도-월-일 하이픈 삽입
        //16자리 일경우 8자리 사이에 ~ 삽입
        private string DateTypeHyphen(string DigitsDate)
        {
            string pattern1 = @"(\d{4})(\d{2})(\d{2})";
            string pattern2 = @"(\d{4})(\d{2})(\d{2})(\d{4})(\d{2})(\d{2})";

            if (DigitsDate.Length == 8)
            {
                DigitsDate = Regex.Replace(DigitsDate, pattern1, "$1-$2-$3");
            }
            else if (DigitsDate.Length == 16)
            {
                DigitsDate = Regex.Replace(DigitsDate, pattern2, "$1-$2-$3 ~ $4-$5-$6");
            }
            else if (DigitsDate.Length == 0)
            {
                DigitsDate = string.Empty;
            }

            return DigitsDate;
        }


        private object RemoveComma(object obj, bool returnAsInt = false)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
            {
                return returnAsInt ? (object)0 : "0";
            }

            string digits = obj.ToString().Replace(",", "");

            if (returnAsInt && int.TryParse(digits, out int result))
            {
                return (object)result;
            }

            return digits;
        }

        private object RemoveHyphen(object obj)
        {
            if (obj == null)
                return string.Empty;

            if (obj.ToString().Contains("-"))
            {
                return obj.ToString().Replace("-", "");
            }

            return obj;
        }

        private string SetToDate(object obj)
        {
            if (DateTime.TryParse(obj.ToString(), out DateTime date))
            {
                return date.ToString("yyyyMMdd");
            }
            return obj.ToString();
        }


        private string ConvertDate(DatePicker datePicker)
        {
            if (datePicker.SelectedDate != null)
                return datePicker.SelectedDate.Value.ToString("yyyMMdd");
            else
                return string.Empty;
        }

        private bool IsDatePickerNull(DatePicker datePicker)
        {
            if (datePicker.SelectedDate == null)
                return true;
            else
                return false;
        }


        //셀에 복사 붙여넣기 방지
        private void TextBox_PreventCopyPaste(object sender, KeyEventArgs e)
        {
            //컨트롤키와 V키가 입력되었는지 확인
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true;
            }
        }

        //셀에 숫자만 입력
        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //셀에 숫자와 하이픈과 마침표 입력
        private void TextBox_PhoneNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        //셀에 숫자와 하이픈 입력
        private void TextBox_HyphenAndNumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private string TimeTypeColon(string DigitsTime)
        {
            string pattern1 = @"(\d{2})(\d{2})";

            if (DigitsTime.Length == 4)
            {
                DigitsTime = Regex.Replace(DigitsTime, pattern1, "$1:$2");
            }

            return DigitsTime;
        }
        private DataRow FillOneOrderData(string strOrderID)
        {
            DataRow dr = null;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("OrderID", strOrderID);
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sOrderOne", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        dr = drc[0];
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return dr;
        }


        /// <summary>
        /// 실삭제
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool DeleteData(string strID)
        {
            bool flag = false;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("EstID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Order_dEstimate", sqlParameter, "D");

                if (result[0].Equals("success"))
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        /// <summary>
        /// 실저장
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>

        private bool SaveData(string strFlag)
        {
            PrimaryKey = string.Empty;
            bool flag = false;            
            int subTotal = CalcuSubTotal();
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData() && CheckContractData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();
                    sqlParameter.Add("EstID", strFlag == "I" ? PrimaryKey : txtEstID.Text);
                    sqlParameter.Add("salesCustomID", txtSalesCustomID.Tag != null ? txtSalesCustomID.Tag.ToString() : "");
                    sqlParameter.Add("managerCustomID", txtManagerCustomID.Tag != null ? txtManagerCustomID.Tag.ToString() : "");
                    sqlParameter.Add("zoneGbnID", txtZoneGbnID.Tag != null ? txtZoneGbnID.Tag.ToString() : "");
                    sqlParameter.Add("FaciliTypeID", cboFaciliTypeID.SelectedValue != null ? cboFaciliTypeID.SelectedValue.ToString() :"");
                    sqlParameter.Add("EstDate", IsDatePickerNull(dtpEstDate) ? "" : SetToDate(dtpEstDate));
                    sqlParameter.Add("InstallSchFromDate", IsDatePickerNull(dtpInstallSchFromDate) ? "" : SetToDate(dtpInstallSchFromDate));
                    sqlParameter.Add("InstallSchTODate", IsDatePickerNull(dtpInstallSchTODate) ? "" : SetToDate(dtpInstallSchTODate));
                    sqlParameter.Add("InstalLocation", txtInstalLocation.Text);
                    sqlParameter.Add("smallInstalLocation", txtSmallInstalLocation.Text);
                    sqlParameter.Add("InstallLocationPart",txtInstallLocationPart.Text);
                    sqlParameter.Add("InstallLocationConditionID",cboInstallLocationConditionID.SelectedValue !=null ? cboInstallLocationConditionID.SelectedValue.ToString() : "");
                    sqlParameter.Add("electrDeliveryMethodID", cboElectrDeliveryMethodID.SelectedValue != null ? cboElectrDeliveryMethodID.SelectedValue.ToString() : "");
                    sqlParameter.Add("EstSubject", txtEstSubject.Text);
                    sqlParameter.Add("EstDamdangName", txtEstDamdangName.Text);
                    sqlParameter.Add("EstDamdangTelno",txtEstDamdangTelno.Text);
                    sqlParameter.Add("EstApprovalYN", cboEstApprovalYN.SelectedValue != null ? cboEstApprovalYN.SelectedValue.ToString() : "");
                    sqlParameter.Add("EstApprovalDate", IsDatePickerNull(dtpEstApprovalDate) ? "" : SetToDate(dtpEstApprovalDate));
                    sqlParameter.Add("deliveryCost", RemoveComma(txtDeliveryCost.Text,true));
                    sqlParameter.Add("totalAmount", subTotal);
                    sqlParameter.Add("Comments", txtComments.Text);

                    string sGetID = strFlag.Equals("I") ? string.Empty : txtEstID.Text;
                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("createUserID", MainWindow.CurrentUser);
                         Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Order_iEstimate";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "EstID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "EstID")
                                {
                                    sGetID = kv.value;
                                    PrimaryKey = sGetID;
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("[저장실패]\r\n" + list_Result[1].value.ToString());
                            //flag = false;
                            return false;
                        }

                        Prolist.Clear();
                        ListParameter.Clear();
                    }
                    #endregion

                    #region 수정
                    else if (strFlag.Equals("U"))
                    {
                        sqlParameter.Add("lastUpdateUserID", MainWindow.CurrentUser);

                        Procedure pro3 = new Procedure();
                        pro3.Name = "xp_Order_uEstimate";
                        pro3.OutputUseYN = "N";
                        pro3.OutputName = "EstID";
                        pro3.OutputLength = "10";

                        Prolist.Add(pro3);
                        //ListParameter.Add(sqlParameter);

                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }
                    #endregion

                    #region 견적 저장
                    for (int i = 0; i < dgdEstItemList.Items.Count; i++)
                    {
                        var estItem = dgdEstItemList.Items[i] as Win_ord_Order_EstimateSub_U_CodeView;

                        sqlParameter.Clear();
                        sqlParameter.Add("EstID", strFlag == "I" ? PrimaryKey : txtEstID.Text);   
                        sqlParameter.Add("EstArticleID", estItem.EstArticleID);
                        sqlParameter.Add("EstUnitPrice", RemoveComma(estItem.EstUnitPrice,true));
                        sqlParameter.Add("EstQty", RemoveComma(estItem.EstUnitPrice, true));
                        sqlParameter.Add("EstAmount", RemoveComma(estItem.EstAmount, true));
                        sqlParameter.Add("Comments", estItem.Comments);
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_Order_iEstimateSub";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "EstID";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }


                    #endregion


                    string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "U");
                        if (Confirm[0] != "success")
                        {
                            MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                            flag = false;
                        }
                        else
                            flag = true;

                    string FtpPk_key = strFlag == "I" ? PrimaryKey : txtEstID.Text;

                    //FTP쓰실건가요..?
                    if (FtpPk_key != string.Empty)
                    {
                        if (deleteListFtpFile.Count > 0)
                        {
                            foreach (string[] str in deleteListFtpFile)
                            {
                                FTP_RemoveFile(FtpPk_key + "/" + str[0]);
                            }
                        }

                        if (listFtpFile.Count > 0)
                        {
                            FTP_Save_File(listFtpFile, FtpPk_key);
                        }


                        UpdateDBFtp(FtpPk_key); // 리스트 갯수가 0개 이상일때 해버리면, 수정시에 저장이 안됨
                    }

                    // 파일 List 비워주기
                    listFtpFile.Clear();
                    lstFilesName.Clear();
                    deleteListFtpFile.Clear();


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        //저장 전에 서브그리드를 더하고 배송료를 마지막에 더해줌
        private int CalcuSubTotal()
        {
            int subTotal = 0;
            for(int i=0; i < dgdEstItemList.Items.Count; i++)
            {
                var item = dgdEstItemList.Items[i] as Win_ord_Order_EstimateSub_U_CodeView;

                int subAmount = (int)RemoveComma(item.EstAmount,true);

                subTotal += subAmount;
            }

            subTotal += (int)RemoveComma(txtDeliveryCost.Text, true);

            return subTotal;
        }

        private bool UpdateDBFtp(string EstID)
        {
            bool flag = false;

            string str_localpath = string.Empty;
            List<string[]> UpdateFilesInfo = new List<string[]>();

            //if (CheckDataFTP(txtName.Text, strFlag))
            //{
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("EstID", EstID);
                sqlParameter.Add("Sketch1File", txtSketch1.Text.Trim() != "" ? txtSketch1.Text : "");
                sqlParameter.Add("Sketch1FileAlias", txtSketch1FileAlias.Text.Trim() != "" ? txtSketch1FileAlias.Text : "");
                sqlParameter.Add("Sketch1Path", txtSketch1.Tag != null ? LoadINI.FtpImagePath+ "/Estimate/" + EstID : "");

                sqlParameter.Add("Sketch2File", txtSketch2.Text.Trim() != "" ? txtSketch2.Text : "");
                sqlParameter.Add("Sketch2FileAlias", txtSketch2FileAlias.Text.Trim() != "" ? txtSketch2FileAlias.Text : "");
                sqlParameter.Add("Sketch2Path", txtSketch2.Tag != null ? LoadINI.FtpImagePath + "/Estimate/" + EstID : "");

                sqlParameter.Add("Sketch3File", txtSketch3.Text.Trim() != "" ? txtSketch3.Text : "");
                sqlParameter.Add("Sketch3FileAlias", txtSketch3FileAlias.Text.Trim() != "" ? txtSketch3FileAlias.Text : "");
                sqlParameter.Add("Sketch3Path", txtSketch3.Tag != null ? LoadINI.FtpImagePath + "/Estimate/" + EstID : "");
                
                sqlParameter.Add("Sketch4File", txtSketch4.Text.Trim() != "" ? txtSketch4.Text : "");
                sqlParameter.Add("Sketch4FileAlias", txtSketch4FileAlias.Text.Trim() != "" ? txtSketch4FileAlias.Text : "");
                sqlParameter.Add("Sketch4Path", txtSketch4.Tag != null ? LoadINI.FtpImagePath + "/Estimate/" + EstID : "");
                
                sqlParameter.Add("Sketch5File", txtSketch5.Text.Trim() != "" ? txtSketch5.Text : "");
                sqlParameter.Add("Sketch5FileAlias", txtSketch5FileAlias.Text.Trim() != "" ? txtSketch5FileAlias.Text : "");
                sqlParameter.Add("Sketch5Path", txtSketch4.Tag != null ? LoadINI.FtpImagePath + "/Estimate/" + EstID : "");

                string[] result = DataStore.Instance.ExecuteProcedure("xp_Order_uEstimate_FTP", sqlParameter, true);


                if (result[0].Equals("success"))
                {
                    flag = true;
                }
                else
                {
                    MessageBox.Show("수정 실패 , 내용 : " + result[1]);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
            //}


            return flag;
        }

        //점2
        private bool CheckData()
        {
            string msg = "";
            string msgg = "";

            bool flag = true;

            if (txtManagerCustomID.Text.Length <= 0 || txtManagerCustomID.Tag == null)
                msg = "운영회사가 입력되지 않았습니다. 먼저 운영회사를 검색 입력하세요.";
            if (txtSalesCustomID.Text.Length <= 0 || txtSalesCustomID.Tag == null)
                msg = "영업회사가 입력되지 않았습니다. 먼저 영업회사를 검색 입력하세요.";

            //if(strFlag == "U")
            //    if (!CheckFKkey(reServeID_global))
            //        flag = false;

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(msgg))
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    var result = MessageBox.Show(msg);                
                }
                flag = false;
            }

            return flag;
        }

        private bool CheckContractData()
        {
            string msg = string.Empty;



            if (dgdEstItemList.Items.Count > 0)
            {
              

                foreach (var item in dgdEstItemList.Items)
                {
                                    
                    var estItem = item as Win_ord_Order_EstimateSub_U_CodeView;
                    if (string.IsNullOrWhiteSpace(estItem.EstArticleID))
                        msg += "견적품목을 검색 입력 하세요.(직접입력X)\n";
                    if (string.IsNullOrWhiteSpace(estItem.EstUnitPrice))
                        msg += "견적품목 단가를 입력하세요\n";
                    if (string.IsNullOrWhiteSpace(estItem.EstQty))
                        msg += "견적품목 수량을 입력하세요\n";               

                    if (msg.Trim() != string.Empty)
                        break;

                }
            }


            if (msg.Length > 0)
            {
                var result =  MessageBox.Show(msg);
                      
                return false;
            }

            return true;
        }

        private bool CheckFKkey(string reServeID)
        {
            bool flag = true;

            //string[] sqlList = { "select reServeID from [Order] where reServeID = "};

            //string[] errMsg = {"계약 등록 화면에서 사용중인 상담번호 입니다."};
            //int errSeq = 0;
            //string msg = string.Empty;

            ////반복문을 돌다가 걸리면 종료, 경고문 띄우고 false반환
            //for (int i = 0; i < sqlList.Length; i++)
            //{
            //    DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + reServeID );
            //    if (ds != null && ds.Tables.Count > 0)
            //    {             

            //        DataTable dt = ds.Tables[0];
            //        if (dt.Rows.Count > 0)
            //        {     
            //            flag = false;
            //            errSeq = i;
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        continue;
            //    }
            //}

            //if (flag == false)
            //{
            //    msg = errMsg[errSeq];
            //    MessageBox.Show(msg);
            //}

            return flag;
        }

        #region 입력시 Event
        //거래처
      


      

        private void CallArticleData(string strArticleID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", strArticleID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sArticleData_Estimate", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];

                        articleData = new ArticleData
                        {
                            articleID = dr["articleID"].ToString(),
                            article = dr["article"].ToString(),
                            //ThreadID = dr["ThreadID"].ToString(),
                            //thread = dr["thread"].ToString(),
                            //StuffWidth = dr["StuffWidth"].ToString(),
                            //DyeingID = dr["DyeingID"].ToString(),
                            weight = dr["weight"].ToString(),
                            spec = dr["spec"].ToString(),
                            //ArticleGrpID = dr["ArticleGrpID"].ToString(),
                            //BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                            unitPrice = dr["outUnitPrice"].ToString().Split('.')[0],
                            unitPriceTypeID = dr["outUnitTypeID"].ToString(),
                            unitTypeID = dr["unitTypeID"].ToString(),
                            codeName = dr["codeName"].ToString(),
                            //ProcessName = dr["ProcessName"].ToString(),
                            //HSCode = dr["HSCode"].ToString(),
                            outUnitPrice = dr["OutUnitPrice"].ToString(),
                            //BuyerModelID = dr["BuyerModelID"].ToString(),
                            //BuyerModel = dr["BuyerModel"].ToString(),
                        };
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

        }

        private void btnPreEstimate_Click(object sender, RoutedEventArgs e)
        {
            if (strFlag != "U")
            {
                preEstimate = new Win_ord_Pop_PreEstimate_Q();

                if (preEstimate.ShowDialog() == true)
                {
                    try
                    {
                        var selectedRow = preEstimate.SelectedItem;
                        if (selectedRow != null)
                        {
                            string today = DateTime.Today.ToString("yyyyMMdd");                                                     

                            AutoBindDataToControls(selectedRow, grdInput);
                            
                            txtEstID.Text = string.Empty;
                            FillGridSub(selectedRow.EstID);

                        }

                        MessageBox.Show("지난 견적 데이터를 불러 왔습니다.", "확인");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("과거견적 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
                    }
                }

            }
            else
            {
                MessageBox.Show("새로 추가 중에만 사용 할 수 있습니다.");
            }
        }
        #endregion

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnUpdate.IsEnabled == true)
            {
                if (e.ClickCount == 2)
                {
                    btnUpdate_Click(null, null);
                }
            }
        }

        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 한자리
        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N1}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            string result = "";

            if (str.Length == 8)
            {
                if (!str.Trim().Equals(""))
                    result = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
            }

            return result;
        }

        // Int로 변환
        private int ConvertInt(string str)
        {
            int result = 0;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (int.TryParse(str, out chkInt) == true)
                    result = int.Parse(str);
            }

            return result;
        }

        // 소수로 변환 가능한지 체크 이벤트
        private bool CheckConvertDouble(string str)
        {
            bool flag = false;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                if (double.TryParse(str, out chkDouble) == true)
                    flag = true;
            }

            return flag;
        }

        // 숫자로 변환 가능한지 체크 이벤트
        private bool CheckConvertInt(string str)
        {
            bool flag = false;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Trim().Replace(",", "");

                if (int.TryParse(str, out chkInt) == true)
                    flag = true;
            }

            return flag;
        }

        // 소수로 변환
        private double ConvertDouble(string str)
        {
            double result = 0;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (double.TryParse(str, out chkDouble) == true)
                    result = double.Parse(str);
            }

            return result;
        }

        //오늘 날짜로 셋팅
        private void SetDatePickerToday()
        {         
            FindUiObject(grdInput, child =>
            {
                if (child is DatePicker datePicker)
                {
                    datePicker.SelectedDate = DateTime.Today;
                }
            });
            
        }

        //콤보박스 첫번째 선택
        private void SetComboBoxIndex()
        {
            FindUiObject(grdInput, child =>
            {
                if (child is ComboBox combobox)
                {
                    combobox.SelectedIndex = 0;
                }
            });

        }



        private void setFTP_Tag_EmptyString()
        {
            string[] strArray_FTP_textbox = { "Sketch1", "Sketch2", "Sketch3", "Sketch4", "Sketch5" };

            for (int i = 0; i < 5; i++)
            {

                TextBox currentTextBox = (TextBox)FindName("txt" + strArray_FTP_textbox[i]);
                if (currentTextBox != null)
                {
                    currentTextBox.Text = string.Empty;
                    currentTextBox.Tag = string.Empty;
                }
            }
     
        }


        //UI컨트롤 요소찾기

        private void FindUiObject(DependencyObject parent, Action<DependencyObject> action)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                action?.Invoke(child);

                FindUiObject(child, action);
            }
        }

        //컨트롤 안 특정 타입의 자식 컨트롤을 찾는 함수 (그리드내에서)
        //var parentContainer = VisualTreeHelper.GetParent(checkbox);
        //var datePicker = FindChild<DatePicker>(parentContainer);
        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                // 재귀적으로 자식의 자식들도 검색
                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void CheckDatePickerValue(CheckBox checkbox)
        {
            // CheckBox 이름에서 앞부분(chk)을 dtp로 변경하여 DatePicker 이름 생성
            if (checkbox.Name.StartsWith("chk"))
            {
                string datePickerName = "dtp" + checkbox.Name.Substring(3);
                var datePicker = this.FindName(datePickerName) as DatePicker;

                if (datePicker != null)
                {
                    if (datePicker.SelectedDate == null ||
                        (datePicker.SelectedDate.HasValue && string.IsNullOrWhiteSpace(datePicker.SelectedDate.Value.ToString())))
                    {
                        checkbox.IsChecked = false;
                        datePicker.IsEnabled = false;
                        datePicker.SelectedDate = null;
                    }
                    else
                    {
                        checkbox.IsChecked = true;
                        datePicker.IsEnabled = true;
                    }
                }
            }
        }

        #endregion



        //private void btnSKetch_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileAndSetting(sender, e);
        //}

        //private void btnSKetchDel_Click(object sender, RoutedEventArgs e)
        //{
        //    DeleteFileAndSetting(sender, e);
        //}

        //private void btnSKetchDown_Click(object sender, RoutedEventArgs e)
        //{
        //    DownloadFileAndSetting(sender, e);
        //}

        //private void btnFileAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileAndSetting(sender, e);
        //}

        private void btnFileDel_Click(object sender, RoutedEventArgs e)
        {
            //DeleteFileAndSetting(sender, e);
        }

        //private void btnFileDownload_Click(object sender, RoutedEventArgs e)
        //{
        //    DownloadFileAndSetting(sender, e);
        //}

        private void btnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            // (버튼)sender 마다 tag를 달자.
            string ClickPoint = ((Button)sender).Tag.ToString();
            if (ClickPoint.Equals("btnSketch1")) { FTP_Upload_TextBox(txtSketch1); }  //긴 경로(FULL 사이즈)
            else if (ClickPoint.Equals("btnSketch2")) { FTP_Upload_TextBox(txtSketch2); }
            else if (ClickPoint.Equals("btnSketch3")) { FTP_Upload_TextBox(txtSketch3); }
            else if (ClickPoint.Equals("btnSketch4")) { FTP_Upload_TextBox(txtSketch4); }
            else if (ClickPoint.Equals("btnSketch5")) { FTP_Upload_TextBox(txtSketch5); }
        }


        private void FTP_Upload_TextBox(TextBox textBox)
        {
            if (!textBox.Text.Equals(string.Empty) && strFlag.Equals("U"))
            {
                MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                return;
            }
            else
            {


                Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();
                OFdlg.Filter = MainWindow.OFdlg_Filter_DocAndImg;
                //    "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx, *.pdf) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx; *.pdf | All Files|*.*";

                OFdlg.Filter = MainWindow.OFdlg_Filter_DocAndImg;

                Nullable<bool> result = OFdlg.ShowDialog();
                if (result == true)
                {
                    strFullPath = OFdlg.FileName;

                    string ImageFileName = OFdlg.SafeFileName;  //명.
                    string ImageFilePath = string.Empty;       // 경로

                    ImageFilePath = strFullPath.Replace(ImageFileName, "");

                    StreamReader sr = new StreamReader(OFdlg.FileName);
                    long FileSize = sr.BaseStream.Length;
                    if (sr.BaseStream.Length > (2048 * 1000))
                    {
                        //업로드 파일 사이즈범위 초과
                        MessageBox.Show("업로드하려는 파일사이즈가 2M byte를 초과하였습니다.");
                        sr.Close();
                        return;
                    }
                    if (!FTP_Upload_Name_Cheking(ImageFileName))
                    {
                        MessageBox.Show("업로드 하려는 파일 중, 이름이 중복된 항목이 있습니다." +
                                        "\n파일 이름을 변경하고 다시 시도하여 주세요");
                    }
                    else
                    {
                        textBox.Text = ImageFileName;
                        textBox.Tag = ImageFilePath;

                        string[] strTemp = new string[] { ImageFileName, ImageFilePath.ToString() };
                        listFtpFile.Add(strTemp);
                    }
                }
            }
        }

        private bool FTP_Upload_Name_Cheking(string fileName)
        {
            bool flag = true;

            if (!lstFilesName.Add(fileName))
            {
                flag = false;
                return flag;
            }


            return flag;
        }


        // 파일 저장하기.
        private void FTP_Save_File(List<string[]> listStrArrayFileInfo, string MakeFolderName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            List<string[]> UpdateFilesInfo = new List<string[]>();
            string[] fileListSimple;
            string[] fileListDetail = null;
            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

            // 기존 폴더 확인작업.
            bool MakeFolder = false;
            MakeFolder = FolderInfoAndFlag(fileListSimple, MakeFolderName);

            bool Makefind = false;
            Makefind = FileInfoAndFlag(fileListSimple, MakeFolderName);


            if (MakeFolder == false)
            {


                if (_ftp.createDirectoryWithParentDir(MakeFolderName) == false)
                {
                    MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");
                    return;
                }

            }
            else
            {
                fileListDetail = _ftp.directoryListSimple(MakeFolderName, Encoding.Default);
            }

            for (int i = 0; i < listStrArrayFileInfo.Count; i++)
            {
                bool flag = true;

                if (fileListDetail != null)
                {
                    foreach (string compare in fileListDetail)
                    {
                        if (compare.Equals(listStrArrayFileInfo[i][0]))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    listStrArrayFileInfo[i][0] = MakeFolderName + "/" + listStrArrayFileInfo[i][0];
                    UpdateFilesInfo.Add(listStrArrayFileInfo[i]);
                }
            }

            if (!_ftp.UploadTempFilesToFTP(UpdateFilesInfo))
            {
                MessageBox.Show("파일업로드에 실패하였습니다.");
                return;
            }

        }

     

        private void btnFileSee_Click(object sender, RoutedEventArgs e)
        {
        
                MessageBoxResult msgresult = MessageBox.Show("다운로드 후 파일을 바로 여시겠습니까?", "보기 확인", MessageBoxButton.YesNoCancel);
                if (msgresult == MessageBoxResult.Yes || msgresult == MessageBoxResult.No)
                {
                    //버튼 태그값.
                    string ClickPoint = ((Button)sender).Tag.ToString();

                    string Sketch1 = txtSketch1.Text.Trim() != "" ? txtSketch1.Text : "";
                    string Sketch2 = txtSketch2.Text.Trim() != "" ? txtSketch2.Text : "";
                    string Sketch3 = txtSketch3.Text.Trim() != "" ? txtSketch3.Text : "";
                    string Sketch4 = txtSketch4.Text.Trim() != "" ? txtSketch4.Text : "";
                    string Sketch5 = txtSketch5.Text.Trim() != "" ? txtSketch5.Text : "";


                    if ((ClickPoint == "btnSketch1") && (Sketch1 == string.Empty)
                        || (ClickPoint == "btnSketch2") && (Sketch2 == string.Empty)
                        || (ClickPoint == "btnSketch3") && (Sketch3 == string.Empty)
                        || (ClickPoint == "btnSketch4") && (Sketch4 == string.Empty)
                        || (ClickPoint == "btnSketch5") && (Sketch5 == string.Empty))
                    {
                        MessageBox.Show("파일이 없습니다.");
                        return;
                    }

                try
                    {
                        // 접속 경로
                        _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                        string str_path = string.Empty;
                        str_path = FTP_ADDRESS + '/' + txtEstID.Text;
                        _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        if (ClickPoint == "btnSketch1") { str_remotepath = Sketch1; }
                        else if (ClickPoint == "btnSketch2") { str_remotepath = Sketch2; }
                        else if (ClickPoint == "btnSketch3") { str_remotepath = Sketch3; }
                        else if (ClickPoint == "btnSketch4") { str_remotepath = Sketch4; }
                        else if (ClickPoint == "btnSketch5") { str_remotepath = Sketch5; }



                        if (ClickPoint == "btnSketch1") { str_localpath = LOCAL_DOWN_PATH + "\\" + Sketch1; }
                        else if (ClickPoint == "btnSketch2") { str_localpath = LOCAL_DOWN_PATH + "\\" + Sketch2; }
                        else if (ClickPoint == "btnSketch3") { str_localpath = LOCAL_DOWN_PATH + "\\" + Sketch3; }
                        else if (ClickPoint == "btnSketch4") { str_localpath = LOCAL_DOWN_PATH + "\\" + Sketch4; }
                        else if (ClickPoint == "btnSketch5") { str_localpath = LOCAL_DOWN_PATH + "\\" + Sketch5; }

                         DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                        if (DI.Exists == false)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        if (file.Exists)
                        {
                            file.Delete();
                        }

                        _ftp.download(str_remotepath, str_localpath);

                        //파일 다운로드 후 바로 열기
                        if (File.Exists(str_localpath) && msgresult == MessageBoxResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new ProcessStartInfo
                                {
                                    FileName = str_localpath,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("파일을 여는 중 오류가 발생했습니다:" +
                                    "\n파일을 열기위한 프로그램이 없거나 기본 실행프로그램이 지정이 안 되었을 수도 있습니다." + ex.Message);
                            }
                        }
                        else if ((File.Exists(str_localpath) && msgresult == MessageBoxResult.No))
                        {
                            MessageBox.Show("파일을 다운로드 하였습니다.", "확인");
                            try
                            {
                                string folderPath = Path.GetDirectoryName(str_localpath);
                                //폴더이름의 타이틀명을 찾
                                var openFolders = Process.GetProcessesByName("explorer")
                                    .Where(p =>
                                    {
                                        try
                                        {
                                            return p.MainWindowTitle.Contains(Path.GetFileName(folderPath));
                                        }
                                        catch
                                        {
                                            return false;
                                        }
                                    });

                                if (!openFolders.Any())
                                {
                                    // 폴더가 열려있지 않을 때만 새로 열기
                                    Process.Start("explorer.exe", $"\"{folderPath}\"");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("폴더를 여는 중 오류가 발생했습니다:" + ex.Message);
                            }
                        }

                }
                    catch (Exception ex) // 뭐든 간에 파일 없다고 하자
                    {
                        MessageBox.Show("파일이 존재하지 않습니다.\r관리자에게 문의해주세요.");
                        return;
                    }
                }
            
        }


        private void btnFileDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 삭제 하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                string ClickPoint = ((Button)sender).Tag.ToString();
                string fileName = string.Empty;

                //먼저 클릭한 버튼의 파일명을 삭제할 파일 리스트에 올린다. 리스트에 올리면서 텍스트의 텍스트와 태그를 지운다.
                //lstFileName에는 ftp업로드할때 파일명 중복방지를 위한 리스트(파일명이 중복되면 파일이 업로드 되지 않고 삭제될때 문제생김)
                ////저장할때 리스트에 있다면 FTP삭제 요청을 한다.
                if ((ClickPoint == "btnSketch1") && (txtSketch1.Text != string.Empty)) { fileName = txtSketch1.Text; FileDeleteAndTextBoxEmpty(txtSketch1); lstFilesName.Remove(fileName); txtSketch1FileAlias.Text = string.Empty; }
                else if ((ClickPoint == "btnSketch2") && (txtSketch2.Text != string.Empty)) { fileName = txtSketch2.Text; FileDeleteAndTextBoxEmpty(txtSketch2); lstFilesName.Remove(fileName); txtSketch1FileAlias.Text = string.Empty; }
                else if ((ClickPoint == "btnSketch3") && (txtSketch3.Text != string.Empty)) { fileName = txtSketch3.Text; FileDeleteAndTextBoxEmpty(txtSketch3); lstFilesName.Remove(fileName); txtSketch1FileAlias.Text = string.Empty; }
                else if ((ClickPoint == "btnSketch4") && (txtSketch4.Text != string.Empty)) { fileName = txtSketch4.Text; FileDeleteAndTextBoxEmpty(txtSketch4); lstFilesName.Remove(fileName); txtSketch1FileAlias.Text = string.Empty; }
                else if ((ClickPoint == "btnSketch5") && (txtSketch5.Text != string.Empty)) { fileName = txtSketch5.Text; FileDeleteAndTextBoxEmpty(txtSketch5); lstFilesName.Remove(fileName); txtSketch1FileAlias.Text = string.Empty; }
            }

        }
        private void FileDeleteAndTextBoxEmpty(TextBox txt)
        {
            if (strFlag.Equals("U"))
            {
                var Article = dgdMain.SelectedItem as Win_ord_Order_Estimate_U_CodeView;

                if (Article != null)
                {
                    //FTP_RemoveFile(Article.ArticleID + "/" + txt.Text);

                    // 파일이름, 파일경로
                    string[] strFtp = { txt.Text, txt.Tag != null ? txt.Tag.ToString() : "" };

                    deleteListFtpFile.Add(strFtp);
                }
            }

            txt.Text = "";
            txt.Tag = "";
        }


        //파일 삭제
        private bool FTP_RemoveFile(string strSaveName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp.delete(strSaveName) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //폴더 삭제(내부 파일 자동 삭제)
        private bool FTP_RemoveDir(string strSaveName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp.removeDir(strSaveName) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        /// <summary>
        /// 해당영역에 폴더가 있는지 확인
        /// </summary>
        bool FolderInfoAndFlag(string[] strFolderList, string FolderName)
        {
            bool flag = false;
            foreach (string FolderList in strFolderList)
            {
                if (FolderList == FolderName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 해당영역에 파일 있는지 확인
        /// </summary>
        bool FileInfoAndFlag(string[] strFileList, string FileName)
        {
            bool flag = false;
            foreach (string FileList in strFileList)
            {
                if (FileList == FileName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }



        #region keyDown 이벤트(커서이동)

        //숫자 외에 다른 문자열 못들어오도록
        public bool IsNumeric(string source)
        {

            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(source);
        }

        //총주문량 숫자 외에 못들어가게 
        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        //단가 숫자 외에 못들어가게
        private void TxtUnitPrice_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        private void DataGrid_SizeChange(object sender, SizeChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                if (dataGrid.ColumnHeaderHeight == 0)
                {
                    dataGrid.ColumnHeaderHeight = 1;
                }
                double a = e.NewSize.Height / 100;
                double b = e.PreviousSize.Height / 100;
                double c = a / b;
                if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
                {
                    dataGrid.ColumnHeaderHeight = dataGrid.ColumnHeaderHeight * c;
                    dataGrid.FontSize = dataGrid.FontSize * c;
                }
            }
        }

        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            var dataGridScrollViewer = FindChild_ScrollViewer<ScrollViewer>(dgdMain);
            if (dataGridScrollViewer != null)
            {
                // DataGrid 스크롤을 헤더 스크롤과 동기화
                dataGridScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }

        }


        private T FindChild_ScrollViewer<T>(DependencyObject parent) where T : DependencyObject
        {
            // Initialize result as null
            T foundChild = null;
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
                {
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    foundChild = FindChild_ScrollViewer<T>(child);
                    if (foundChild != null) break;
                }
            }

            return foundChild;
        }

        //private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // DataGrid 내부의 ScrollViewer를 찾아서
        //    var dataGridScrollViewer = FindChild_ScrollViewer<ScrollViewer>(dgdMain);

        //    if (dataGridScrollViewer != null)
        //    {
        //        // ScrollViewer의 ScrollChanged 이벤트를 처리
        //        dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
        //    }
        //}


        // DataGrid의 수평 스크롤이 변경될 때 호출되는 메서드
        //private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    // DataGrid의 ScrollViewer에서 수평 스크롤 오프셋을 가져옴
        //    var dataGridScrollViewer = sender as ScrollViewer;

        //    if (dataGridScrollViewer != null)
        //    {
        //        // 헤더의 ScrollViewer와 수평 오프셋을 동기화 
        //        dgdMainHeaderSh.ScrollToHorizontalOffset(dataGridScrollViewer.HorizontalOffset);
        //    }
        //}






        #endregion keydown 이벤트

        //메인 데이터그리드 선택 이벤트
        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                estID_Global = string.Empty;
                articleID_global = string.Empty;
                //lstFilesName.Clear();

                var estimateInfo = dgdMain.SelectedItem as Win_ord_Order_Estimate_U_CodeView;
                if (estimateInfo != null)
                {
                    rowNum = dgdMain.SelectedIndex;        
        
                    DataContext = estimateInfo;
                    estID_Global = estimateInfo.EstID;

                    FillGridSub(estimateInfo.EstID);
                }

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridMain_SelectionChanged : " + ee.ToString());
            }

        }

 

        private void FillGridSub(string EstID)
        {
            try
            {
                int sumAmount = 0;

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("EstID", EstID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Order_sEstimateSub", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var estSubItem = new Win_ord_Order_EstimateSub_U_CodeView
                            {
                                num = i,
                                EstID = dr["EstID"].ToString(),
                                EstItemSeq = dr["EstItemSeq"].ToString(),
                                EstArticle= dr["EstArticle"].ToString(),
                                EstArticleID = dr["EstArticleID"].ToString(),
                                EstUnitPrice = stringFormatN0(dr["EstUnitPrice"]),
                                EstQty = stringFormatN0(dr["EstQty"]),
                                EstAmount= stringFormatN0(dr["EstAmount"]),
                                Comments = dr["Comments"].ToString(),
                            };

                            sumAmount += (int)RemoveComma(estSubItem.EstAmount, true);

                            ovcOrder_EstSub.Add(estSubItem);
                          
                        }

                        dgdEstItemList.ItemsSource = ovcOrder_EstSub;

                        if (dgdSubTotal.Items.Count > 0)
                            dgdSubTotal.Items.Clear();

                        var subTotal = new Win_ord_Order_EstimateSub_Total_U_CodeView
                        {
                            count = i.ToString(),
                            totalAmount = stringFormatN0(sumAmount)
                        };

                        dgdSubTotal.Items.Add(subTotal);

                    }                
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생(fillgrid_OrderItemList), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        

        private void btnConAdd_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string xName = button.Name;

            if (xName.Equals("btnAdd_EstSubItem"))
            {
                int num = dgdEstItemList.Items.Count + 1;
                var estItem = Win_ord_Order_EstimateSub_U_CodeView.CreateEmpty_EstimateSub();
                estItem.num = num;
                ovcOrder_EstSub.Add(estItem);
            }   

        }

        private void btnConDel_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string xName = button.Name;

            //버튼누른 곳이 어딘지
            if (xName.Equals("btnDel_EstSubItem"))
            {
                if (dgdEstItemList.SelectedItem != null)
                {
                    int rowcount = dgdEstItemList.Items.IndexOf(dgdEstItemList.SelectedItem);
                    ovcOrder_EstSub.RemoveAt(rowcount);

                    if (dgdEstItemList.Items.Count > 0)
                    {
                        if (dgdEstItemList.Items.Count - 1 > rowcount)
                        {
                            dgdEstItemList.SelectedIndex = rowcount;
                        }
                        else
                        {
                            dgdEstItemList.SelectedIndex = 0;
                        }
                    }
                }
                else //행 선택안하고 누르면 마지막줄부터 삭제
                {
                    if (dgdEstItemList.Items.Count > 0)
                    {
                        dgdEstItemList.SelectedIndex = dgdEstItemList.Items.Count - 1;

                        btnConDel_Click(button, e);
                    }
                }
            }


        }

        #region 바인딩을 자동화...   
        private void AutoBindDataToControls(object dataObject, DependencyObject parent)
        {
            var properties = dataObject.GetType().GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            // TextBox 처리
            var textBoxes = FindAllControls<TextBox>(parent);
            foreach (var textBox in textBoxes)
            {
                // Text 바인딩 처리
                var textBinding = BindingOperations.GetBinding(textBox, TextBox.TextProperty);
                if (textBinding != null && !string.IsNullOrEmpty(textBinding.Path.Path))
                {
                    var textPropertyName = textBinding.Path.Path.ToLower();
                    if (properties.TryGetValue(textPropertyName, out var textProperty))
                    {
                        var textValue = textProperty.GetValue(dataObject)?.ToString();
                        if (decimal.TryParse(textValue, out _))
                            textBox.Text = stringFormatN0(textValue);
                        else
                            textBox.Text = textValue;
                    }
                }

                // Tag 바인딩 처리
                var tagBinding = BindingOperations.GetBinding(textBox, TextBox.TagProperty);
                if (tagBinding != null && !string.IsNullOrEmpty(tagBinding.Path.Path))
                {
                    var tagPropertyName = tagBinding.Path.Path.ToLower();
                    if (properties.TryGetValue(tagPropertyName, out var tagProperty))
                    {
                        textBox.Tag = tagProperty.GetValue(dataObject)?.ToString();
                    }
                }
            }

            // DatePicker 처리
            var datePickers = FindAllControls<DatePicker>(parent);
            foreach (var datePicker in datePickers)
            {
                var binding = BindingOperations.GetBinding(datePicker, DatePicker.SelectedDateProperty);
                if (binding != null && !string.IsNullOrEmpty(binding.Path.Path))
                {
                    var propertyName = binding.Path.Path.ToLower();
                    if (properties.TryGetValue(propertyName, out var property))
                    {
                        datePicker.SelectedDate = ConvertToDateTime(property.GetValue(dataObject)?.ToString());
                    }
                }
            }

            // ComboBox 처리
            var comboBoxes = FindAllControls<ComboBox>(parent);
            foreach (var comboBox in comboBoxes)
            {
                var binding = BindingOperations.GetBinding(comboBox, ComboBox.SelectedValueProperty);
                if (binding != null && !string.IsNullOrEmpty(binding.Path.Path))
                {
                    var propertyName = binding.Path.Path.ToLower();
                    if (properties.TryGetValue(propertyName, out var property))
                    {
                        comboBox.SelectedValue = property.GetValue(dataObject)?.ToString();
                    }
                }
            }
        }


        private IEnumerable<T> FindAllControls<T>(DependencyObject parent) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T control)
                    yield return control;

                foreach (var descendant in FindAllControls<T>(child))
                    yield return descendant;
            }
        }

        // 단일 컨트롤을 찾는 메서드도 필요할 수 있다
        private T FindControl<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T control && control.Name == name)
                    return control;

                var result = FindControl<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private DateTime? ConvertToDateTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr?.Trim()))
                return null;

            // 특수문자 제거 (숫자만 남김)
            string cleanDate = new string(dateStr.Where(char.IsDigit).ToArray());

            // 8자리가 아닌 경우 null 반환
            if (cleanDate.Length != 8)
                return null;

            try
            {
                return DateTime.ParseExact(cleanDate, "yyyyMMdd", null);
            }
            catch
            {
                return null;
            }
        }

        #endregion


        #region 데이터그리드 내부 입력 동작 모음


        //셀의 내부 컨트롤에 포커싱
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        //셀의 내부 컨트롤에 포커싱
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        //포커스 오면 셀 EditingMode 전화
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }



        // 데이터그리드 셀 plusfinder이벤트(키 다운)
        private void dgdtpeGetArticleID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = sender as TextBox;
                int nLarge = 0;

                DependencyObject parent = textBox;
                while (parent != null && !(parent is DataGrid))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                var currentGrid = parent as DataGrid;
                if (currentGrid != null)
                {
                    switch (currentGrid.Name)
                    {
                        case "dgdEstItemList":
                            nLarge = 5102;
                            break;
       
                    }
                    MainWindow.pf.ReturnCode(textBox, nLarge, "");
                }

                //if (nLarge == 5103 || nLarge == 5104)
                if(nLarge == 5102)
                {
                    CallArticleData(textBox.Tag.ToString());

                    var item = currentGrid.CurrentItem;
                    var propertyInfo = item.GetType().GetProperty("EstUnitPrice");
                    propertyInfo.SetValue(item, articleData.unitPrice);
                }

            }
        }
        // 데이터그리드 셀 plusfinder이벤트(더블클릭)
        private void dgdtpeGetArticleID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int nLarge = 0;

            // 부모 DataGrid 찾기
            var parent = textBox.Parent;
            while (parent != null && !(parent is DataGrid))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var currentGrid = parent as DataGrid;
            if (currentGrid != null)
            {

                // 그리드 이름에 따라 다른 타입으로 캐스팅
                switch (currentGrid.Name)
                {
                    case "dgdEstItemList":
                        nLarge = 5102;
                        break;

                }

                MainWindow.pf.ReturnCode(textBox, nLarge, "");

                if (nLarge == 5102)
                {
                    CallArticleData(textBox.Tag.ToString());

                    var item = currentGrid.CurrentItem;
                    var propertyInfo = item.GetType().GetProperty("EstUnitPrice");
                    propertyInfo.SetValue(item, articleData.unitPrice);
                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("인쇄할 견적서를 미리보시겠습니까?\n바로 인쇄는 아니오를 눌러주세요", "확인", MessageBoxButton.YesNoCancel);
            if (msgresult == MessageBoxResult.Yes)
                PrintWork(true);
            if (msgresult == MessageBoxResult.No)
                PrintWork(false);
    

        }

        //프린트메서드 수정판
        private void PrintWork(bool previewYN)
        {
            Excel.Application excelapp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Worksheet pastesheet = null;
            Excel.Range workrange = null;
            int excelProcessId = 0;

            try
            {
                //먼저 자사정보를 프로시저로 불러옴
                CallCompanyData();

                /*자사정보 변수*/
                string kCompany = string.Empty;                 //자사명
                string address = string.Empty;                  //주소
                string chief = string.Empty;                    //대표자
                string phone1 = string.Empty;                   //전화번호
                string estID = string.Empty;                    //견적번호
                string kCustom = string.Empty;                  //고객명
                string article = string.Empty;                  //프로그램명
                int estimateAmount = 0;                         //견적하위 서브그리드 합계액


                //서브그리드 합계액 계산
                for(int i = 0; i < dgdEstItemList.Items.Count; i++)
                {
                    var data = dgdEstItemList.Items[i] as Win_ord_Order_EstimateSub_U_CodeView;

                    estimateAmount += (int)RemoveComma(data.EstAmount,true);
                }

                //엑셀에 고정으로 넣을 값을 미리 구하기
                var estData = dgdMain.SelectedItem as Win_ord_Order_Estimate_U_CodeView;

                if (estData != null)
                {
                    estID = estData.EstID;
                    kCustom = estData.managerCustom;
                }

                if (companyData != null)
                {
                    kCompany = companyData.kCompany;
                    address = companyData.address1 +"\n"+companyData.address2;
                    chief = companyData.chief;
                    phone1 = companyData.phone1;
                }

                //엑셀 생성
                excelapp = new Excel.Application();
                //생성한 프로세스 아이디 저장(닫을때 EXCEL COM 정리용으로 사용함)
                excelProcessId = GetExcelProcessId();
                //양식 경로는 exe가 실행된곳에서 combine을 메서드로 실행지점/Report/상담등록_견적서.xlsx
                string templatePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Report",
                    "상담등록_견적서.xlsx"
                );

                //양식이 없으면 오류를 일으키세요
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"시스템에 저장된 견적서 양식을 찾을 수 없습니다.\n관리자에게 문의해주세요","경고");
                }


                workbook = excelapp.Workbooks.Add(templatePath);
                worksheet = workbook.Sheets["Form"];
                pastesheet = workbook.Sheets["Print"];
         
                //먼저 원본 시트의 인쇄영역이 어디까지인지 구합니다.
                Excel.Range printArea = worksheet.Range[worksheet.PageSetup.PrintArea];
                int columnCount = printArea.Columns.Count;          //인쇄영역으로 지정된 원본시트의 컬럼 합계수
                int rowsCount = printArea.Rows.Count;               //인쇄영역으로 지정된 원본시트의 로우 합계수
                int startRow = printArea.Row;                       //인쇄영역 설정 첫 시작지점

                string startColumnLetter = printArea.Columns[1].Address[1, 0][0].ToString();        //인쇄 영역이 무슨 열에서부터 시작되는지 문자를 구합니다.
                //MessageBox.Show($"시작 열 문자: {startColumnLetter}");
             
                string endColumnLetter = printArea.Columns[printArea.Columns.Count].Address[1, 0][0].ToString(); //인쇄 영역이 어디에서 끝나는지 문자를 구합니다.
                //MessageBox.Show($"끝 열 문자: {endColumnLetter}");

                string workSheetStartRow = startRow.ToString();
                string workSheetRowsCount = rowsCount.ToString();

                //원본 조합
                string workSheetX = startColumnLetter + workSheetStartRow;      //위의 내용으로 원본시트의 시작지점과 끝지점을 구합니다.
                string workSheetY = endColumnLetter + workSheetRowsCount;

                //먼저 고정값을 원본시트에 적어놓습니다. 복사시트에 재활용 함   
                FillBaseInfo(worksheet, kCompany, address, chief, phone1, estID, kCustom, article, estimateAmount);          

                //그 다음 원본시트를 복사시트에 복사하며 값을 넣습니다.
                FillDataIntoPasteSheet(worksheet,pastesheet, 20, 17, workSheetX, workSheetY, startColumnLetter,endColumnLetter, columnCount, rowsCount, startRow, dgdEstItemList);

                //복사시트 선택
                pastesheet.Select();

                //미리보기? 바로 인쇄?
                HandlePrintPreview(excelapp, pastesheet, previewYN);

                //프로세스 정리 이벤트 핸들러
                excelapp.WorkbookBeforeClose += (Excel.Workbook wb, ref bool cancel) =>
                {
                    //엑셀 프로그램 껐는데도 살아있으면 생성할때 받아온 프로세스 아이디를 종료
                    if (excelProcessId != 0)
                    {
                        KillExcelProcess(excelProcessId);
                    }
                    ReleaseExcelObject(workrange);
                    ReleaseExcelObject(pastesheet);
                    ReleaseExcelObject(worksheet);
                    ReleaseExcelObject(workbook);
                    ReleaseExcelObject(excelapp);
                };

            }
            catch (Exception ex)
            {
                if (excelProcessId != 0)
                {
                    KillExcelProcess(excelProcessId);
                }
                ReleaseExcelObject(workrange);
                ReleaseExcelObject(pastesheet);
                ReleaseExcelObject(worksheet);
                ReleaseExcelObject(workbook);
                ReleaseExcelObject(excelapp);
                MessageBox.Show($"오류가 발생했습니다\n: {ex.Message}");
            }         
        }



        //원본시트의 서식, 행, 열 높이 등등 복사시트에 복사하는 메서드
        private void BaseCopySheet(Excel.Worksheet worksheet, Excel.Worksheet pastesheet, string worksheetX, string worksheetY, string pasteSheetX, string pasteSheetY)
        {
            //원본 시트의 범위를 소스로 잡습니다.
            Excel.Range sourceRange = worksheet.Range[$"{worksheetX}:{worksheetY}"];

            //복사를 먼저 합니다.(붙여넣기 아님)
            sourceRange.Copy();

            //넘겨받은 복사지점 파라미터로 복사시트에 붙여넣기 합니다.
            Excel.Range destination1 = pastesheet.Range[$"{pasteSheetX}"];  
            destination1.PasteSpecial(Excel.XlPasteType.xlPasteAll);
            destination1.PasteSpecial(Excel.XlPasteType.xlPasteFormats);

            //붙여넣고 나면 원본의 열, 행 높이넓이를 재지정
            int X = Convert.ToInt32(Regex.Replace(pasteSheetX, "[^0-9]", ""));

            X = X - 1;

            for (int i = 1; i <= sourceRange.Rows.Count; i++)
            {
                pastesheet.Rows[X+i].RowHeight = worksheet.Rows[i].RowHeight;
            }

            for (int j = 1; j <= sourceRange.Columns.Count; j++)
            {
                pastesheet.Columns[j].ColumnWidth = worksheet.Columns[j].ColumnWidth;
            }

            #region 용지 크기를 구해서 인쇄너비를 계산하기 버전
            //// 보통 A4
            //// A4 용지 크기 (포인트 단위, 1 inch = 72 points)
            //// A4 = 210mm x 297mm = 8.27 inch x 11.69 inch
            //const double A4_WIDTH_POINTS = 8.27 * 72;  // 약 595 points
            //const double A4_HEIGHT_POINTS = 11.69 * 72; // 약 842 points


            //pastesheet.PageSetup.PrintArea = $"{worksheetX}:{pasteSheetY}";
            //// 인쇄 영역 가져오기
            //Excel.Range printArea = pastesheet.Range[pastesheet.PageSetup.PrintArea];

            //// 인쇄 영역의 너비와 높이
            //double printAreaWidth = printArea.Width;
            //double printAreaHeight = printArea.Height;

            //// 여백 고려 (포인트 단위)
            //double availableWidth = A4_WIDTH_POINTS - (pastesheet.PageSetup.LeftMargin + pastesheet.PageSetup.RightMargin);
            //double availableHeight = A4_HEIGHT_POINTS - (pastesheet.PageSetup.TopMargin + pastesheet.PageSetup.BottomMargin);

            //// 배율 계산
            //double widthScale = (availableWidth / printAreaWidth) * 100;
            //double heightScale = (availableHeight / printAreaHeight) * 100;
            //int zoom = (int)Math.Min(widthScale, heightScale);

            //pastesheet.PageSetup.Zoom = zoom;
            #endregion

            //해보니까 배율을 자동
            //너비는 1페이지, 높이는 자동, 그리고 페이지브레이크만 넣으면 페이지 나누기,
            //그리고 인쇄영역을 인쇄할 부분 끝까지 지정하면
            //페이지 나누기 미리보기(실제 인쇄되면 나오는 부분)에서 딱 원본시트 복사한것 만큼 나온다
            //여러장에 적용 가능
            pastesheet.PageSetup.Zoom = false;
            pastesheet.PageSetup.FitToPagesWide = 1;
            pastesheet.PageSetup.FitToPagesTall = false;

            //페이지로 나눌 부분을 설정합니다.
            string pageBreakPointLetter_Row = Regex.Replace(pasteSheetY, "[^A-Z]", ""); //붙여넣는 부분 끝나는 지점이라 Y
            string pageBreakPointRows = Regex.Replace(pasteSheetY, "[^0-9]", "");
            int pageRowCount = Convert.ToInt32(pageBreakPointRows);

            //지정한 곳(인쇄영역으로 지정한 행 수 다음)으로 페이지 삽입을 합니다.
            Excel.Range nextPageRange = pastesheet.Range[$"{pageBreakPointLetter_Row}" + (pageRowCount + 1)];
            pastesheet.HPageBreaks.Add(nextPageRange);

            //인쇄영역을 처음부터 복사한 부분까지 지정합니다.
            pastesheet.PageSetup.PrintArea = $"{worksheetX}:{pasteSheetY}";
            
        }

       
        //고정부분 채우기
        private void FillBaseInfo(Excel.Worksheet sheet,
            string kCompany, string address, string chief, string phone1,
            string reServeID, string kCustom, string article, int estimateAmount)
        {

            // 셀 값 설정
            workrange = sheet.Range["B2"];
            workrange.Value2 = reServeID;

            workrange = sheet.Range["C6"];
            workrange.Value2 = DateTime.Today.ToString("yyyy.MM.dd");

            workrange = sheet.Range["C7"];
            workrange.Value2 = kCustom;

            workrange = sheet.Range["F6"];
            workrange.Value2 = address;

            workrange = sheet.Range["F7"];
            workrange.Value2 = kCompany;

            workrange = sheet.Range["F8"];
            workrange.Value2 = chief;

            workrange = sheet.Range["F9"];
            workrange.Value2 = phone1;

            workrange = sheet.Range["D12"];
            workrange.Value2 = article;

            workrange = sheet.Range["G14"];
            workrange.Value2 = estimateAmount;            

        }


        private void FillDataIntoPasteSheet(Excel.Worksheet worksheet, Excel.Worksheet pastesheet, 
                                            int perRow, int insertStartRow, string workSheetX, string workSheetY, 
                                            string startColumnLetter, string endColumnLetter, int columnsCount, int RowsCount, int startRow,
                                            DataGrid dt)
        {
            #region 파라미터 설명
            /*받는 파라미터 => (
                                    worksheet = 원본시트 
                                    pastesheet = 복사시트              
                                    perRow = 복사시트에 입력할 수 있는 행 수
                                    insertStartRow = 복사시트에 몇 줄부터 입력을 시작할지 정함
                                    workSheetX = 원본시트 시작행
                                    workSheetY = 원본시트 시작열
                                    startColumnLetter = 원본시트 시작열 문자
                                    endColumnLetter = 원본시트 끝나는 지점 문자
                                    columnsCount = 원본시트 컬럼 수
                                    RowsCount = 인쇄영역으로 지정된 원본시트의 행 수
                                    startRow = 원본시트 시작 행
                                    dt = 입력을 요하는 데이터그리드
                                )*/
            #endregion

            int doneRow = 0;                                    //어느 행까지 입력했는지
            int pageSize = (dt.Items.Count / perRow) +1;        //데이터그리드를 페이지당 입력 가능한 행으로 나누어 페이지 구하기
            int row = RowsCount;                                //시트 복사, 복사 시트에 데이터 입력 용도

            string pasteSheetX = startColumnLetter + startRow;  //원본 - > 복사할 시트 처음 위치X
            string pasteSheetY = endColumnLetter + row;         //원본 - > 복사할 시트 처음 위치Y

            for (int i = 0; i < pageSize; i++)                  //데이터 입력전 원본시트를 복사시트에 페이지 수 만큼 만듭니다.
            {   
                BaseCopySheet(worksheet, pastesheet, workSheetX, workSheetY, pasteSheetX, pasteSheetY);

                pasteSheetX = startColumnLetter + (startRow + row);                     //시작지점에서 인쇄영역으로 지정된 행 수만큼 더합니다.
                pasteSheetY = endColumnLetter + (startRow + row + RowsCount - 1);       //끝 지점도 구합니다.

                row += RowsCount;
            }

            row = 0;

            for (int k = 0; k < pageSize; k++)                                          //복사한 시트에 데이터를 입력합니다. 페이지만큼 반복
            {
                for(int j = 0; j< perRow; j++)                                          //입력 가능한 행 수만큼만 반복합니다.
                {
                    if(doneRow < dt.Items.Count)                                        
                    {
                        var item = dt.Items[doneRow] as Win_ord_Order_EstimateSub_U_CodeView;
                        if (item != null)
                        {
                            pastesheet.Cells[insertStartRow + row + j, 2] = doneRow + 1;
                            pastesheet.Cells[insertStartRow + row + j, 3] = item.EstArticle;
                            pastesheet.Cells[insertStartRow + row + j, 4] = item.EstQty;
                            pastesheet.Cells[insertStartRow + row + j, 5] = item.EstUnitPrice;
                        }
                        doneRow++;
                    }                  
               
                }

                row += RowsCount;
            }
         
        }

        //기본 프린터가 하나라도 지정되었나요?
        private bool IsPrinterAvailable()
        {
            return System.Drawing.Printing.PrinterSettings.InstalledPrinters.Count > 0;
        }


        //프린트 핸들러
        private void HandlePrintPreview(Excel.Application app, Excel.Worksheet sheet, bool preview)
        {
            if (!IsPrinterAvailable())
            {
                throw new Exception("윈도우에 연결된 기본 프린터가 없습니다.\n기본 프린터를 설정한 후 시도하여주세요.");
            }

            app.Visible = true;
            if (preview)
            {
                sheet.PrintPreview();
            }
            else
            {
                sheet.PrintOut();
            }
        }

        //엑셀 리소스 정리
        private void ReleaseExcelObject(object obj)
        {
            if (obj != null)
            {
                try
                {
                    Marshal.ReleaseComObject(obj);
                    obj = null;
                }
                catch
                {
                    obj = null;
                }
                finally
                {
                    GC.Collect();
                }
            }
        }

        // 실행 후 프로세스 아이디를 시간순 정렬해서 가져오기
        private int GetExcelProcessId()
        {
            var process = Process.GetProcessesByName("EXCEL")
                                .OrderByDescending(p => p.StartTime)
                                .FirstOrDefault();
            return process?.Id ?? 0;
        }

        //릴리즈해도 프로세스가 하나는 끝까지 살아남아서...
        private void KillExcelProcess(int processId)
        {
            try
            {
                Process process = Process.GetProcessById(processId);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch { }
        }



        //private void ProcessDataRows(Excel.Worksheet sourceSheet, Excel.Worksheet targetSheet)
        //{
        //    const int MAX_ROWS_PER_PAGE = 10;
        //    int rowCount = dgdRsrvEst.Items.Count; 

        //    int currentPage = 1;
        //    int processedRows = 0;

        //    while (processedRows < rowCount)
        //    {
        //        // 현재 페이지에 들어갈 데이터 수 계산
        //        int rowsForCurrentPage = Math.Min(MAX_ROWS_PER_PAGE, rowCount - processedRows);

        //        // 현재 페이지 데이터 처리
        //        for (int i = 0; i < rowsForCurrentPage; i++)
        //        {
        //            var item = dgdRsrvEst.Items[processedRows + i] as Win_ord_Order_Reservation_U_CodeView_RsrvEstimate_Nadaum;

        //            if(item != null)
        //            {
        //                int excelRow = 16 + i;

        //                // 각 열의 데이터를 Excel에 입력
        //                sourceSheet.Cells[excelRow, 2].Value2 = (processedRows + i + 1).ToString();  
        //                sourceSheet.Cells[excelRow, 4].Value2 = item.article;       
        //                sourceSheet.Cells[excelRow, 5].Value2 = item.qty;        
        //                sourceSheet.Cells[excelRow, 6].Value2 = item.unitPrice;     
        //                sourceSheet.Cells[excelRow, 9].Value2 = item.comments;        

        //                // 필요한 경우 셀 서식 설정
        //                ((Excel.Range)sourceSheet.Cells[excelRow, 5]).NumberFormat = "#,##0";  // 수량 서식
        //                ((Excel.Range)sourceSheet.Cells[excelRow, 6]).NumberFormat = "#,##0";  // 단가 서식
        //                ((Excel.Range)sourceSheet.Cells[excelRow, 9]).NumberFormat = "#,##0";  // 단가 서식
        //                ((Excel.Range)sourceSheet.Cells[excelRow, 10]).NumberFormat = "#,##0";  // 단가 서식
        //            }
        //        }

        //        // 페이지 복사
        //        int targetRow = processedRows * MAX_ROWS_PER_PAGE + 1;
        //        MessageBox.Show($"현재 페이지: {currentPage}\n" +
        //                       $"처리된 행 수: {processedRows}\n" +
        //                       $"복사될 시작 행 위치: {targetRow}\n" +
        //                       $"현재 페이지 데이터 수: {rowsForCurrentPage}");

        //        // 페이지 복사
        //        sourceSheet.UsedRange.Copy();
        //        var targetRange = targetSheet.Cells[targetRow, 1];
        //        targetSheet.Paste(targetRange);

        //        processedRows += rowsForCurrentPage;
        //        currentPage++;
        //        AutoFitRows(targetSheet);
        //    }
        //}

        //private void PrintWork(bool previewYN)
        //{
        //    try
        //    {

        //        CallCompanyData();

        //        string kCompany = string.Empty;
        //        string address = string.Empty;
        //        string chief = string.Empty;
        //        string phone1 = string.Empty;

        //        string reServeID = string.Empty;
        //        string kCustom = string.Empty;

        //        var reServeData = dgdMain.SelectedItem as Win_ord_Order_Reservation_U_CodeView_Nadaum;

        //        if (reServeData != null)
        //        {
        //            reServeID = reServeData.reServeID;
        //            kCustom = reServeData.kCustom;
        //        }

        //        if (companyData != null)
        //        {

        //            kCompany = companyData.kCompany;
        //            address = companyData.address;
        //            chief = companyData.chief;
        //            phone1 = companyData.phone1;

        //        }

        //        excelapp = new Microsoft.Office.Interop.Excel.Application();


        //        string MyBookPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Report\\견적서.xlsx";
        //        workbook = excelapp.Workbooks.Add(MyBookPath);
        //        worksheet = workbook.Sheets["Form"];
        //        pastesheet = workbook.Sheets["Print"];

        //        //상담번호
        //        workrange = worksheet.get_Range("B2");
        //        workrange.Value2 = reServeID;

        //        //날짜
        //        workrange = worksheet.get_Range("C6");
        //        workrange.Value2 = DateTime.Today.ToString("yyyy.MM.dd");

        //        //수신자(거래업체)
        //        workrange = worksheet.get_Range("C7");
        //        workrange.Value2 = reServeID;

        //        //사업장 소재지
        //        workrange = worksheet.get_Range("G6");
        //        workrange.Value2 = address;

        //        //상호
        //        workrange = worksheet.get_Range("G7");
        //        workrange.Value2 = kCompany;

        //        //대표자 성명
        //        workrange = worksheet.get_Range("G8");
        //        workrange.Value2 = chief;

        //        //전화번호
        //        workrange = worksheet.get_Range("G9");
        //        workrange.Value2 = phone1;


        //        // 페이지 계산 등
        //        //int rowCount = ovcArticleBom.Count;
        //        int rowCount = 10;
        //        int excelStartRow = 2;

        //        // 총 데이터를 입력할수 있는 갯수
        //        int totalDataInput = 350;

        //        //// 카피할 다음페이지 인덱스
        //        //int nextCopyLine = 380;


        //        int copyLine = 0;
        //        int Page = 0;
        //        int PageAll = (int)Math.Ceiling(1.0 * rowCount / totalDataInput);
        //        int DataCount = 0;




        //        // 총 금액 계산하기
        //        //double SumAmount = 0;

        //        for (int k = 0; k < PageAll; k++)
        //        {
        //            Page++;
        //            //copyLine = ((Page - 1) * (nextCopyLine - 1));
        //            //copyLine = ((Page - 1) * 37);

        //            int excelNum = 0;

        //            // 기존에 있는 데이터 지우기 "A7", "W41"
        //            //worksheet.Range["A2", "P350"].EntireRow.ClearContents();


        //            for (int i = DataCount; i < rowCount; i++)
        //            {
        //                //11
        //                if (i == totalDataInput * Page)
        //                {
        //                    break;
        //                }

        //                //var OcArticleBom = ovcArticleBom[i];

        //                int excelRow = excelStartRow + excelNum;

        //                int excelRowStairTwo = excelStartRow + excelNum - 1;
        //                int excelRowStairThree = excelStartRow + excelNum - 2;
        //                int excelRowStairFour = excelStartRow + excelNum - 3;



        //                excelNum++;
        //                DataCount = i;
        //            }


        //            // 2장 이상 넘어가면 페이지 넘버 입력
        //            //if (PageAll > 1)
        //            //{
        //            //    pastesheet.PageSetup.CenterFooter = "&P / &N";
        //            //}

        //            //Form 시트 내용 Print 시트에 복사 붙여넣기
        //            worksheet.Select();
        //            worksheet.UsedRange.EntireRow.Copy();
        //            pastesheet.Select();
        //            workrange = pastesheet.Cells[copyLine + 1, 1];
        //            workrange.Select();
        //            pastesheet.Paste();


        //            DataCount++;
        //        }

        //        //// 총금액 입력하기 : 10, 50, 90
        //        //for (int i = 0; i < PageAll; i++)
        //        //{
        //        //    int sumAmount_Index = 10 + (40 * i);

        //        //    workrange = pastesheet.get_Range("E" + sumAmount_Index);
        //        //    workrange.Value2 = SumAmount;
        //        //}

        //        pastesheet.UsedRange.EntireRow.Select();

        //        //
        //        excelapp.Visible = true;
        //        //msg.Hide();

        //        pastesheet.PageSetup.Zoom = false; // 확대/축소 비율 자동
        //        pastesheet.PageSetup.FitToPagesWide = 1; // 용지 너비를 1페이지로
        //        pastesheet.PageSetup.FitToPagesTall = false; // 높이는 자동

        //        previewYN = true;
        //        if (previewYN == true)
        //        {
        //            pastesheet.PrintPreview();
        //        }
        //        else
        //        {
        //            pastesheet.PrintOutEx();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        // Clean up 백그라운드에서 엑셀을 지우자 - 달달

        //        ReleaseExcelObject(workbook);
        //        ReleaseExcelObject(worksheet);
        //        ReleaseExcelObject(pastesheet);
        //        ReleaseExcelObject(workrange);
        //        ReleaseExcelObject(excelapp);


        //    }
        //}

        //private static void ReleaseExcelObject(object obj)
        //{
        //    try
        //    {
        //        if (obj != null)
        //        {
        //            Marshal.ReleaseComObject(obj);
        //            obj = null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        obj = null;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        GC.Collect();
        //    }
        //}

        private void CallCompanyData()
        {
            try
            {
                

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_Estimate_sCompanyData", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];

                        companyData = new SetCompanyData
                        {
                            companyID = dr["companyID"].ToString(),
                            kCompany = dr["kCompany"].ToString(),
                            companyNo = dr["companyNo"].ToString(),
                            address1 = dr["address1"].ToString(),
                            address2 = dr["address2"].ToString(),
                            phone1 = dr["phone1"].ToString(),
                            chief = dr["chief"].ToString(),

                        };
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
                MessageBox.Show("인쇄를 종료합니다.");
                return;
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

        }



        private void dgdtpeAmountUpdate_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // 현재 DataGrid 찾기
                DependencyObject parent = textBox;
                while (parent != null && !(parent is DataGrid))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                var currentGrid = parent as DataGrid;
                if (currentGrid != null && (currentGrid.Name.Contains("dgdEstItemList")))
                {
                    var item = currentGrid.CurrentItem;

                    // 수량과 단가 가져오기
                    var qtyProperty = item.GetType().GetProperty("EstQty");
                    var priceProperty = item.GetType().GetProperty("EstUnitPrice");

                    if ((qtyProperty != null && priceProperty != null))
                    {
                        // 실제 값을 먼저 가져옴
                        var qtyValue = qtyProperty.GetValue(item);
                        var priceValue = priceProperty.GetValue(item);

                        // 둘 다 값이 있는지 확인
                        if (qtyValue != null && !string.IsNullOrEmpty(qtyValue.ToString()) &&
                            priceValue != null && !string.IsNullOrEmpty(priceValue.ToString()))
                        {

                            decimal qty = Convert.ToDecimal(qtyProperty.GetValue(item));
                            decimal unitPrice = Convert.ToDecimal(priceProperty.GetValue(item));

                            // 합계 계산
                            decimal total = qty * unitPrice;

                            var totalProperty = item.GetType().GetProperty("EstAmount");
                            if (totalProperty != null)
                            {
                                totalProperty.SetValue(item, total.ToString());
                            }
                        }
                    }
                }   
            }
        }


        //운영회사
        private void lblManagerCustomIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkManagerCustomIdSrh.IsChecked == true)
            {
                chkManagerCustomIdSrh.IsChecked = false;
                txtManagerCustomIdSrh.IsEnabled = false;
            }
            else
            {
                chkManagerCustomIdSrh.IsChecked = true;
                txtManagerCustomIdSrh.IsEnabled = true;
            }
        }

        //운영회사
        private void chkManagerCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkManagerCustomIdSrh.IsChecked == true)
            {
                txtManagerCustomIdSrh.IsEnabled = true;
                btnManagerCustomIdSrh.IsEnabled = true;
            }
            else
            {
                txtManagerCustomIdSrh.IsEnabled = false;
                btnManagerCustomIdSrh.IsEnabled = false;
            }
        }


        //운영회사
        private void txtManagerCustomIdSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtManagerCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //운영회사
        private void btnManagerCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtManagerCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //품명 라벨 클릭
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = false;
                txtArticleSrh.IsEnabled = false;
            }
            else
            {
                chkArticleSrh.IsChecked = true;
                txtArticleSrh.IsEnabled = true;
            }
        }

        //품명 체크박스
        private void chkArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = true;
                txtArticleSrh.IsEnabled = true;
            }
            else
            {
                chkArticleSrh.IsChecked = false;
                txtArticleSrh.IsEnabled = false;
            }
        }


        //품명 텍스트박스 키다운 이벤트
        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(txtArticleSrh, 5102, txtArticleSrh.Text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 플러스파인더 버튼
        private void btnArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(txtArticleSrh, 5102, txtArticleSrh.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }
        //지역구분 라벨
        private void lblZoneGbnIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkZoneGbnIdSrh.IsChecked == true)
            {
                chkZoneGbnIdSrh.IsChecked = false;
                cboZoneGbnIdSrh.IsEnabled = false;
            }
            else
            {
                chkZoneGbnIdSrh.IsChecked = true;
                cboZoneGbnIdSrh.IsEnabled = true;
            }
        }
        //지역구분 체크박스
        private void chkZoneGbnIdSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkZoneGbnIdSrh.IsChecked == true)
            {
                chkZoneGbnIdSrh.IsChecked = true;
                cboZoneGbnIdSrh.IsEnabled = true;
            }
            else
            {
                chkZoneGbnIdSrh.IsChecked = false;
                cboZoneGbnIdSrh.IsEnabled = false;
            }
        }

        //전기조달방법 라벨
        private void lblElecDeliMethSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkElecDeliMethSrh.IsChecked == true)
            {
                chkElecDeliMethSrh.IsChecked = false;
                cboElecDeliMethSrh.IsEnabled = false;
            }
            else
            {
                chkElecDeliMethSrh.IsChecked = true;
                cboElecDeliMethSrh.IsEnabled = true;
            }
        }

        //전기조달방법 체크
        private void chkElecDeliMethSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkElecDeliMethSrh.IsChecked == true)
            {
                chkElecDeliMethSrh.IsChecked = true;
                cboElecDeliMethSrh.IsEnabled = true;
            }
            else
            {
                chkElecDeliMethSrh.IsChecked = false;
                cboElecDeliMethSrh.IsEnabled = false;
            }
        }

        //국소명 라벨
        private void lblInstallLocationSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInstallLocationSrh.IsChecked == true)
            {
                chkInstallLocationSrh.IsChecked = false;
                txtInstallLocationSrh.IsEnabled = false;
            }
            else
            {
                chkInstallLocationSrh.IsChecked = true;
                txtInstallLocationSrh.IsEnabled = true;
            }
        }


        //국소명 체크박스
        private void chkInstallLocationSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkInstallLocationSrh.IsChecked == true)
            {
                chkInstallLocationSrh.IsChecked = true;
                txtInstallLocationSrh.IsEnabled = true;
            }
            else
            {
                chkInstallLocationSrh.IsChecked = false;
                txtInstallLocationSrh.IsEnabled = false;
            }
        }

        //비고 라벨
        private void lblCommentsSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCommentsSrh.IsChecked == true)
            {
                chkCommentsSrh.IsChecked = false;
                txtCommentsSrh.IsEnabled = false;
            }
            else
            {
                chkCommentsSrh.IsChecked = true;
                txtCommentsSrh.IsEnabled = true;
            }
        }

        //비고 체크박스
        private void chkCommentsSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkCommentsSrh.IsChecked == true)
            {
                chkCommentsSrh.IsChecked = true;
                txtCommentsSrh.IsEnabled = true;
            }
            else
            {
                chkCommentsSrh.IsChecked = false;
                txtCommentsSrh.IsEnabled = false;
            }
        }

        //영업사(그리드 키다운)
        private void txtSalesCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSalesCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //영업사(그리드 버튼)
        private void btnSalesCustomID_Click(object sender, RoutedEventArgs e)
        {
                MainWindow.pf.ReturnCode(txtSalesCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //운영사(그리드 버튼)
        private void btnManagerCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtManagerCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //운영사(그리드 키다운)
        private void txtManagerCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtManagerCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //지역(그리드 키다운)
        private void txtZoneGbnID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtZoneGbnID, 5201, "");
        }

        private void btnZoneGbnID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtZoneGbnID, 5201, "");
        }

        //승인여부에 따른 승인일자 활성화 여부
        private void cboEstApprovalYN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cboEstApprovalYN.SelectedValue != null)
            {
                if (cboEstApprovalYN.SelectedValue.ToString() == "Y")
                    dtpEstApprovalDate.IsEnabled = true;
                else
                    dtpEstApprovalDate.IsEnabled = false;
            }
    
        }

        private void lblEstSubjectSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkEstSubjectSrh.IsChecked == true)
            {
                chkEstSubjectSrh.IsChecked = false;
                txtEstSubjecSrh.IsEnabled = false;
            }
            else
            {
                chkEstSubjectSrh.IsChecked = true;
                txtEstSubjecSrh.IsEnabled = true;
            }
        }

        private void chkEstSubjectSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkEstSubjectSrh.IsChecked == true)
            {
                chkEstSubjectSrh.IsChecked = true;
                txtEstSubjecSrh.IsEnabled = true;
            }
            else
            {
                chkEstSubjectSrh.IsChecked = false;
                txtEstSubjecSrh.IsEnabled = false;
            }
        }




        //private void btnPreReservation_Click(object sender, RoutedEventArgs e)
        //{
        //    if(articleID_global != "")
        //    {
        //        preReservation = new Win_ord_Pop_PreReservation(articleID_global);

        //        if (preReservation.ShowDialog() == true)
        //        {
        //            try
        //            {
        //                var selectedRow = preReservation.SelectedItem;
        //                if (selectedRow != null)
        //                {
        //                    string today = DateTime.Today.ToString("yyyyMMdd");

        //                    //dtpAcptDate.SelectedDate = DateTime.ParseExact(selectedRow.acptDate.Trim() != "" ? selectedRow.acptDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                    dtpReserveDate.SelectedDate = DateTime.ParseExact(selectedRow.reServeDate.Trim() != "" ? selectedRow.reServeDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                    txtCustom.Text = selectedRow.kCustom;
        //                    txtCustom.Tag = selectedRow.customID;
        //                    cboContributeYN.SelectedValue = selectedRow.contributeYN;
        //                    txtEtcRequest.Text = selectedRow.etcRequest;
        //                    txtLasthistory.Text = selectedRow.lastHistory;

        //                    txtDamdangName.Text = selectedRow.damdangName;
        //                    txtdamdangDepartName.Text = selectedRow.damdangDepartName;
        //                    txtdamdangPositionName.Text = selectedRow.damdangPositionName;
        //                    txtdamdangDirPhone.Text = selectedRow.damdangDirPhone;
        //                    txtdamdangPhoneno.Text = selectedRow.damdangPhoneno;
        //                    txtdamdangEMail.Text = selectedRow.eMail;

        //                    if (dgdRsrvEst.Items.Count > 0) ovcOrder_RsvrEstimate.Clear();
        //                    if (dgdRvrsStudent.Items.Count > 0) ovcOrder_RsvrStudent.Clear();

        //                    fillGridContract(selectedRow.reServeID);

        //                }

        //                MessageBox.Show("지난 견적 데이터를 불러 왔습니다.\n(첨부파일 제외)", "확인");
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("과거견적 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
        //            }
        //        }

        //    }
        //    else
        //    {
        //        MessageBox.Show("먼저 프로그램을 검색 입력 하여주세요");
        //    }
        //}

        //private void btnGoOrderCalendar_Click(object sender, RoutedEventArgs e)
        //{
        //    // 있으면 진행, 없으면 리턴
        //    if (ReserveView.reServeID != null)
        //    {
        //        MainWindow.orderDate = string.Empty;
        //        MainWindow.orderID_Calendar = string.Empty;

        //        MainWindow.acptDate = ReserveView.acptDate;
        //        MainWindow.reServeID_Calendar = ReserveView.reServeID;

        //        DateTime acptDate = Convert.ToDateTime(ReserveView.acptDate);

        //        string firstDay = new DateTime(acptDate.Year, acptDate.Month, 1).ToString("yyyyMMdd");
        //        string lastDay = new DateTime(acptDate.Year, acptDate.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd");

        //        MainWindow.sFromDate = firstDay;
        //        MainWindow.sToDate = lastDay;
        //    }
        //    else
        //    {
        //        MessageBox.Show("먼저 데이터를 선택 후 클릭하세요.");
        //        return;
        //    }


        //    int i = 0;
        //    foreach (MenuViewModel mvm in MainWindow.mMenulist)
        //    {
        //        if (mvm.Menu.Equals("계약일정표 조회"))
        //        {
        //            break;
        //        }
        //        i++;
        //    }
        //    try
        //    {
        //        if (MainWindow.MainMdiContainer.Children.Contains(MainWindow.mMenulist[i].subProgramID as MdiChild))
        //        {
        //            (MainWindow.mMenulist[i].subProgramID as MdiChild).Focus();
        //            //혹시나 계약등록에서 달력 열어두고 또 상담등록에서 일정조회하면? 달력 미리 열어두고 하면?
        //            var mdiChild = MainWindow.mMenulist[i].subProgramID as MdiChild;
        //            if (mdiChild.Content is Win_ord_OrderCalendar_Q control)
        //            {
        //                control.setIntFlagOn();
        //                control.FillCalendar();
        //            }
        //        }
        //        else
        //        {
        //            Type type = Type.GetType("WizMes_Nadaum." + MainWindow.mMenulist[i].ProgramID.Trim(), true);
        //            object uie = Activator.CreateInstance(type);

        //            MainWindow.mMenulist[i].subProgramID = new MdiChild()
        //            {
        //                Title = "WizMes_Nadaum_[" + MainWindow.mMenulist[i].MenuID.Trim() + "] " + MainWindow.mMenulist[i].Menu.Trim() +
        //                        " (→" + MainWindow.mMenulist[i].ProgramID.Trim() + ")",
        //                Height = SystemParameters.PrimaryScreenHeight * 0.8,
        //                MaxHeight = SystemParameters.PrimaryScreenHeight * 0.85,
        //                Width = SystemParameters.WorkArea.Width * 0.85,
        //                MaxWidth = SystemParameters.WorkArea.Width,
        //                Content = uie as UIElement,
        //                Tag = MainWindow.mMenulist[i]
        //            };
        //            Lib.Instance.AllMenuLogInsert(MainWindow.mMenulist[i].MenuID, MainWindow.mMenulist[i].Menu, MainWindow.mMenulist[i].subProgramID);
        //            MainWindow.MainMdiContainer.Children.Add(MainWindow.mMenulist[i].subProgramID as MdiChild);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("해당 화면이 존재하지 않습니다.");
        //    }
        //}

    }



    #endregion

    public class Win_ord_Order_Estimate_U_CodeView : BaseView
    {
        public int num { get; set; }
        public string EstID {get;set;}
        public string salesCustomID {get;set;}
        public string salesCustom { get; set; }
        public string managerCustomID {get;set;}
        public string managerCustom { get; set; }
        public string zoneGbnID {get;set;}
        public string FaciliTypeID {get;set;}
        public string FacliType { get; set; }
        public string EstDate {get;set;}
        public string EstReqDate { get; set; } //설치 예정일의 첫번째날로 정함 화면설계서에만 있음
        public string InstallSchFromDate {get;set;}
        public string InstallSchTODate {get;set;}
        public string InstalLocation {get;set;}
        public string smallInstalLocation {get;set;}
        public string InstallLocationPart {get;set;}
        public string InstallLocationCondition { get; set; }
        public string InstallLocationConditionID { get; set; }
        public string electrDeliveryMethodID { get; set;}
        public string EstSubject {get;set;}
        public string EstDamdangName {get;set;}
        public string EstDamdangTelno {get;set;}
        public string EstApprovalYN {get;set;}
        public string EstApprovalDate {get;set;}
        public string EstItemList { get; set; }
        public string deliveryCost {get;set;}
        public string totalAmount {get;set;}
        public string Comments {get;set;}
        public string CreateDate {get;set;}
        public string CreateUserID {get;set;}
        public string LastUpdateDate {get;set;}
        public string LastUpdateUserID { get; set; }

        public string sketch1File {get;set;}
        public string sketch1FileAlias {get;set;}
        public string sketch1Path {get;set;}
        public string sketch2File {get;set;}
        public string sketch2FileAlias {get;set;}
        public string sketch2Path {get;set;}
        public string sketch3File {get;set;}
        public string sketch3FileAlias {get;set;}
        public string sketch3Path {get;set;}
        public string sketch4File {get;set;}
        public string sketch4FileAlias {get;set;}
        public string sketch4Path {get;set;}
        public string sketch5File {get;set;}
        public string sketch5FileAlias {get;set;}
        public string sketch5Path { get; set; }

    }

    public class Win_ord_Order_EstimateSub_U_CodeView : BaseView
    {
       public int num { get; set; }
       public string EstID {get;set;}
       public string EstItemSeq {get;set;}
       public string EstArticle { get; set; }
       public string EstArticleID {get;set;}
       public string EstUnitPrice {get;set;}
       public string EstQty {get;set;}
       public string EstAmount {get;set;}
       public string Comments {get;set;}
       public string CreateDate {get;set;}
       public string CreateUserID {get;set;}
       public string LastUpdateDate {get;set;}
       public string LastUpdateUserID { get; set; }

        public static Win_ord_Order_EstimateSub_U_CodeView CreateEmpty_EstimateSub()
        {
            return new Win_ord_Order_EstimateSub_U_CodeView
            {
                EstID = "",
                EstItemSeq = "",
                EstArticle = "",
                EstArticleID = "",
                EstUnitPrice = "",
                EstQty = "",
                EstAmount = "",
                Comments = "",
            };
        }

    }


    public class Win_ord_Order_Estimate_Total_U_CodeView :BaseView
    {
        public string count { get; set; }
        public string totalAmount { get; set; }
    }


    public class Win_ord_Order_EstimateSub_Total_U_CodeView : BaseView
    {
        public string count { get; set; }
        public string totalAmount { get; set; }
    }

    public class SetCompanyData
    {
        public string companyID { get; set; }
        public string chief { get; set; }
        public string kCompany { get; set; }
        public string companyNo { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string phone1 { get; set; }
    }
}

