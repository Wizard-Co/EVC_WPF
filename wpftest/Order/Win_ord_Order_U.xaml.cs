﻿using System.Windows.Media.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WizMes_EVC.Order.Pop;
using WizMes_EVC.PopUp;
using WizMes_EVC.PopUP;
using WPF.MDI;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Order_U
'** 설명       : 수주등록
'** 작성일자   : 2024.12.31
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.12.31, 최대현, 강경단 책임                 최초생성 나다음에듀 order활용
' 2025.02.04, 최대현, 강경단 책임                 첨부파일 추가 및 디자인 변경
' 2025.02.10, 최대현, 강경단 책임                 한전수전정보 기본정보 입력란 위치 변경및 textbox -> datePicker로 변경
' 2025.02.11, 최대현, 강경단 책임                 검색조건 품목 주석처리 -> 시공사업체 검색조건으로 변경
' 2025.02.17, 최대현,                             탭별 클릭 저장로직 처리를 풀었음(btnupdate, btnadd시에 무조건 각 탭별 true에 fillgridTabs() 실행) 차후 저장 느리다고 하면 다시 수정해야합니다.
                                                  중점 확인사항은 tab클릭시, 클릭 후 저장 후 보고있는 화면 tab_Click= true로 해주기 및 refresh, 탭 클릭 안한 데이터 보존처리, FTP저장 처리 등...
'**************************************************************************************************/

namespace WizMes_EVC
{
    /// <summary>
    /// Win_ord_Order_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_Order_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;
        DataTable DT;

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        private const double A_AREA_HEIGHT = 88.0;

        string strFlag = string.Empty;
        string orderID_global = string.Empty;
        int rowNum = 0;
        int intFlag = 0;
        bool tab2_clicked = false;
        bool tab3_clicked = false;
        bool tab4_clicked = false;
        bool tab5_clicked = false;

        bool isBringLastOrder = false;
        bool boolCallEst = false;

        private ToolTip currentToolTip;
        private System.Windows.Threading.DispatcherTimer currentTimer;
        private SpeechBubbleAdorner _speechBubble;
        private bool _speechBubbleMouseEntered = false;
        //Win_ord_Pop_PreOrder preOrder = new Win_ord_Pop_PreOrder();

        private Win_ord_Pop_PreOrder_Q preOrder;
        private Win_ord_Pop_PreEstimate_Q preEstimate;



        Win_ord_Order_U_CodeView_dgdMain OrderView = new Win_ord_Order_U_CodeView_dgdMain();

        //계약내용 쪽
        ObservableCollection<Win_order_Order_U_CodView_dgdAcc> ovcOrder_Acc
        = new ObservableCollection<Win_order_Order_U_CodView_dgdAcc>();

        ObservableCollection<Win_order_Order_U_CodView_localGov> ovcOrder_localGov
        = new ObservableCollection<Win_order_Order_U_CodView_localGov>();

        private List<ScrollSyncHelper> scrollHelpers = new List<ScrollSyncHelper>();

        ObservableCollection<CodeView> ovcOrderTypeAcc = null;

        //ArticleData articleData = new ArticleData();

        private bool _isAnimating = false;

        string PrimaryKey = string.Empty;

        string FullPath1 = string.Empty;
        string FullPath2 = string.Empty;
        string FullPath3 = string.Empty;
        string FullPath4 = string.Empty;
        string FullPath5 = string.Empty;

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

        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Order";
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


        public Win_ord_Order_U()
        {


            InitializeComponent();
            this.SizeChanged += UserControl_SizeChanged;
            scrollHelpers.Add(new ScrollSyncHelper(dgdAccSV, dgdAcc));
            SetupLastColumnResize(dgdAcc, dgdAccSV, grdAcc);
            if (!isUserInWorkTeam())
            {
                
                hideGridForWorkTeam1.Visibility = Visibility.Hidden;
                hideGridForWorkTeam2.Visibility = Visibility.Hidden;
            }
            else
            {
                hideGridForWorkTeam1.Visibility = Visibility.Visible;
                hideGridForWorkTeam2.Visibility = Visibility.Visible;
            }
        }

        //로그인 한 사람이 시공팀 소속인지 
        private bool isUserInWorkTeam()
        {
            bool flag = true;

            string[] sqlList = { "select mp.personID ,md.depart from mt_Person mp " +
                                 "LEFT JOIN mt_Depart md on md.departID = mp.departID " +
                                 "where PersonID =  "

            };
     

            //반복문을 돌다가 걸리면 종료, 경고문 띄우고 false반환
            for (int i = 0; i < sqlList.Length; i++)
            {
                DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + "'"+ MainWindow.CurrentPersonID.Trim()+"'");
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][1].ToString().Contains("시공"))
                        {
                            flag = false;
                            break;
                        }
                      
                    }
                }
                else
                {
                    continue;
                }
            }

      

            return flag;
        }    

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 윈도우 크기 변경 시 토글버튼 위치 업데이트
            UpdateToggleButtonPosition();
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
         
            UpdateToggleButtonPosition();

            if (_speechBubble == null)
            {
                _speechBubble = brLine_Top.AddSpeechBubble("체크된 추가 검색 조건이 있습니다.",
                    BubblePosition.BottomRight, TailDirection.Up, TextAlign.Center, new Thickness(0, 0, 0, 50));
                _speechBubble.Hide();
            }

            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            btnToday_Click(null, null);
            SetComboBox();

            dgdAcc.ItemsSource = ovcOrder_Acc;
            dgdLocalGov.ItemsSource = ovcOrder_localGov;

            //txtOrderCount.Text = "0 건";
            //txtOrderYds.Text = "0";


            if (!string.IsNullOrEmpty(MainWindow.OrderID))
            {
                intFlag = 1;
                tblOrderID.Text = MainWindow.OrderID;
                lblDateSrh_MouseLeftButtonDown(null, null);

                FillGrid();

                intFlag = 0;
                tblOrderID.Text = string.Empty;

                if (dgdMain.Items.Count > 0) dgdMain.SelectedIndex = 0;
                MainWindow.OrderID = string.Empty;
            }



            if (MainWindow.tempContent != null
                && MainWindow.tempContent.Count > 0)
            {
                string OrderId = MainWindow.tempContent[0];
                string sDate = MainWindow.tempContent[1];
                string eDate = MainWindow.tempContent[2];
                string chkYN = MainWindow.tempContent[3];


                if (chkYN.Equals("Y"))
                {
                    ChkDateSrh.IsChecked = true;
                }
                else
                {
                    ChkDateSrh.IsChecked = false;
                }

                dtpSDate.SelectedDate = DateTime.Parse(sDate.Substring(0, 4) + "-" + sDate.Substring(4, 2) + "-" + sDate.Substring(6, 2));
                dtpEDate.SelectedDate = DateTime.Parse(eDate.Substring(0, 4) + "-" + eDate.Substring(4, 2) + "-" + eDate.Substring(6, 2));

                chkOrderIDSrh.IsChecked = true;
                txtOrderIDSrh.Text = OrderId;

                //;

                rowNum = 0;
                re_Search(rowNum);

                MainWindow.tempContent.Clear();
            }
        }

        //콤보박스 만들기
        private void SetComboBox()
        {

            List<string[]> strArray = new List<string[]>();
            string[] strOne = { "", "진행" };
            string[] strTwo = { "1", "완료" };
            strArray.Add(strOne);
            strArray.Add(strTwo);

            // 완료 구분
            ObservableCollection<CodeView> ovcCloseClssSrh = ComboBoxUtil.Instance.Direct_SetComboBox(strArray);
            cboCloseClssSrh.ItemsSource = ovcCloseClssSrh;
            cboCloseClssSrh.DisplayMemberPath = "code_name";
            cboCloseClssSrh.SelectedValuePath = "code_id";
            cboCloseClssSrh.SelectedIndex = 0;            


            // 수주 구분
            ObservableCollection<CodeView> oveOrderFlag = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDFLG", "Y", "", "");
            //영업, 생산오더만 보여주기 위해.            
            oveOrderFlag.RemoveAt(2);
            //카운트 4에서 하나 지우고 나면 카운트 3돼서 또 2번 지움
            oveOrderFlag.RemoveAt(2);

            cboOrderFlag.ItemsSource = oveOrderFlag;
            cboOrderFlag.DisplayMemberPath = "code_name";
            cboOrderFlag.SelectedValuePath = "code_id";
            cboOrderFlag.SelectedIndex = 1;

            //검색조건의 사업구분
            ObservableCollection<CodeView> ovcOrderTypeSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDTYPE", "Y", "", "");
            cboOrderTypeIDSrh.ItemsSource = ovcOrderTypeSrh;
            cboOrderTypeIDSrh.DisplayMemberPath = "code_name";
            cboOrderTypeIDSrh.SelectedValuePath = "code_id";
            cboOrderTypeIDSrh.SelectedIndex = 0;

            //inputGrid의 사업구분
            ObservableCollection<CodeView> ovcOrderType = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDTYPE", "Y", "", "");
            cboOrderType.ItemsSource = ovcOrderType;
            cboOrderType.DisplayMemberPath = "code_name";
            cboOrderType.SelectedValuePath = "code_id";
            cboOrderType.SelectedIndex = 0;

            //dgdacc의 사업구분
            ovcOrderTypeAcc = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDTYPE", "Y", "", "");


            //EVC용
            //지역구분(ZoneID)
            ObservableCollection<CodeView> ovcZoneGbnID = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CSTZneGbn", "Y", "", "");
            cboZoneGbnIdSrh.ItemsSource = ovcZoneGbnID;
            cboZoneGbnIdSrh.DisplayMemberPath = "code_name";
            cboZoneGbnIdSrh.SelectedValuePath = "code_id";
            cboZoneGbnIdSrh.SelectedIndex = 0;

            //하나의 ovc를 돌려쓰면 저장할때 문제가 생길 수 있기에 따로 생성
            //전기조달
            //ObservableCollection<CodeView> ovcElecDeliMethSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            //cboElecDeliMethSrh.ItemsSource = ovcElecDeliMethSrh;
            //cboElecDeliMethSrh.DisplayMemberPath = "code_name";
            //cboElecDeliMethSrh.SelectedValuePath = "code_id";
            //cboElecDeliMethSrh.SelectedIndex = 0;

            //시공지자체 전기조달
            //ObservableCollection<CodeView> ovcElecDeliMeth = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            //cboElectrDeliveryMethodID.ItemsSource = ovcElecDeliMeth;
            //cboElectrDeliveryMethodID.DisplayMemberPath = "code_name";
            //cboElectrDeliveryMethodID.SelectedValuePath = "code_id";
            //cboElectrDeliveryMethodID.SelectedIndex = 0;

            //한전전기조달
            //ObservableCollection<CodeView> ovcKepElecDeliMeth = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            //cboKepElectrDeliveryMethodID.ItemsSource = ovcKepElecDeliMeth;
            //cboKepElectrDeliveryMethodID.DisplayMemberPath = "code_name";
            //cboKepElectrDeliveryMethodID.SelectedValuePath = "code_id";
            //cboKepElectrDeliveryMethodID.SelectedIndex = 0;


            //화폐단위      
            ObservableCollection<CodeView> ovcUnitPriceClss = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CMMPRC", "Y", "", "");
            cboMtrPriceUnitClss.ItemsSource = ovcUnitPriceClss;
            cboMtrPriceUnitClss.DisplayMemberPath = "code_name";
            cboMtrPriceUnitClss.SelectedValuePath = "code_id";
            cboMtrPriceUnitClss.SelectedIndex = 0;
            cboMtrPriceUnitClss.IsEnabled = true;

            //감리배치여부(YN)
            List<string[]> strInspectionNeedYn = new List<string[]>();
            string[] strNeedY = { "Y", "Y" };
            string[] strNeedN = { "N", "N" };
            strInspectionNeedYn.Add(strNeedY);
            strInspectionNeedYn.Add(strNeedN);

        
            ObservableCollection<CodeView> ovcInspectionNeedYN = ComboBoxUtil.Instance.Direct_SetComboBox(strInspectionNeedYn);
            cboInspectionNeedYN.ItemsSource = ovcInspectionNeedYN;
            cboInspectionNeedYN.DisplayMemberPath = "code_name";
            cboInspectionNeedYN.SelectedValuePath = "code_id";
            cboInspectionNeedYN.SelectedIndex = 0;

            //인입승인여부(YN)
            //List<string[]> strKepInApprove = new List<string[]>();
            //string[] strApprove = { "Y", "Y" };
            //string[] strDenied = { "N", "N" };
            //strKepInApprove.Add(strApprove);
            //strKepInApprove.Add(strDenied);

            ////감리배치여부
            //ObservableCollection<CodeView> ovcKepInApprove = ComboBoxUtil.Instance.Direct_SetComboBox(strKepInApprove);
            //cboKepInApprovalYN.ItemsSource = ovcKepInApprove;
            //cboKepInApprovalYN.DisplayMemberPath = "code_name";
            //cboKepInApprovalYN.SelectedValuePath = "code_id";
            //cboKepInApprovalYN.SelectedIndex = 0;



            //계약 단계(그리드용)
            //cboContractProgressID.ItemsSource = ovcContractProgressID;
            //cboContractProgressID.DisplayMemberPath = "code_name";
            //cboContractProgressID.SelectedValuePath = "code_id";
            //cboContractProgressID.SelectedIndex = 0;


            //나다음(Nadaum)용
            //마감
            //List<string[]> strArrayCloseYN = new List<string[]>();
            //string[] strNotClosed = { "N", "진행" };
            //string[] strClosed = { "Y", "마감" };
            //strArrayCloseYN.Add(strNotClosed);
            //strArrayCloseYN.Add(strClosed);

            //// 완료 구분
            //ObservableCollection<CodeView> ovcCloseYN = ComboBoxUtil.Instance.Direct_SetComboBox(strArrayCloseYN);
            //cboCloseYN.ItemsSource = ovcCloseYN;
            //cboCloseYN.DisplayMemberPath = "code_name";
            //cboCloseYN.SelectedValuePath = "code_id";
            //cboCloseYN.SelectedIndex = 0;

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

        //운영회사
        private void lblManagerCustomIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkManagerCustomIdSrh.IsChecked == true)
            {
                chkManagerCustomIdSrh.IsChecked = false;
                txtManagerCustomIdSrh.IsEnabled = false;
                btnManagerCustomIdSrh.IsEnabled = false;
            }
            else
            {
                chkManagerCustomIdSrh.IsChecked = true;
                txtManagerCustomIdSrh.IsEnabled =true;
                btnManagerCustomIdSrh.IsEnabled = true;
            }
        }

        //운영회사
        private void chkManagerCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            if(chkManagerCustomIdSrh.IsChecked == true)
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

        ////영업회사
        //private void lblSalesCustomIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if(chkSalesCustomIdSrh.IsChecked == true)
        //    {
        //        chkSalesCustomIdSrh.IsChecked = false;
        //        txtSalesCustomIdSrh.IsEnabled = false;
        //        btnSalesCustomIdSrh.IsEnabled = false;
        //    }
        //    else
        //    {
        //        chkSalesCustomIdSrh.IsChecked = true;
        //        txtSalesCustomIdSrh.IsEnabled = true;
        //        btnSalesCustomIdSrh.IsEnabled = true;
        //    }
        //}

        ////영업회사
        //private void chkSalesCustomIdSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkSalesCustomIdSrh.IsChecked == true)
        //    {
        //        txtSalesCustomIdSrh.IsEnabled = true;
        //        btnSalesCustomIdSrh.IsEnabled = true;
        //    }
        //    else
        //    {
        //        txtSalesCustomIdSrh.IsEnabled = false;
        //        btnSalesCustomIdSrh.IsEnabled = false;
        //    }
        //}
        ////영업회사
        //private void txtSalesCustomIdSrh_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //        MainWindow.pf.ReturnCode(txtSalesCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        //}
        ////영업회사
        //private void btnSalesCustomIdSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow.pf.ReturnCode(txtSalesCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        //}

        //지역구분 콤보박스
        private void chkZoneIdSrh_Click(object sender, RoutedEventArgs e)
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

        ////비고 라벨
        //private void lblInstallLocationAddCommentsSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (chkInstallLocationAddCommentsSrh.IsChecked == true)
        //    {
        //        chkInstallLocationAddCommentsSrh.IsChecked = false;
        //        txtInstallLocationAddCommentsSrh.IsEnabled = false;
        //    }
        //    else
        //    {
        //        chkInstallLocationAddCommentsSrh.IsChecked = true;
        //        txtInstallLocationAddCommentsSrh.IsEnabled = true;
        //    }
        //}

        ////비고 체크박스
        //private void chkInstallLocationAddCommentsSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkInstallLocationAddCommentsSrh.IsChecked == true)
        //    {
        //        chkInstallLocationAddCommentsSrh.IsChecked = true;
        //        txtInstallLocationAddCommentsSrh.IsEnabled = true;
        //    }
        //    else
        //    {
        //        chkInstallLocationAddCommentsSrh.IsChecked = false;
        //        txtInstallLocationAddCommentsSrh.IsEnabled = false;
        //    }
        //}

        //최종고객사
        private void lblInCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkInCustomSrh.IsChecked = chkInCustomSrh.IsChecked == true ? false : true;
        }

        //최종고객사
        private void chkInCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtInCustomSrh.IsEnabled = true;
            btnPfInCustomSrh.IsEnabled = true;
        }

        //최종고객사
        private void chkInCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtInCustomSrh.IsEnabled = false;
            btnPfInCustomSrh.IsEnabled = false;
        }

        //최종고객사
        private void txtInCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtInCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //최종고객사
        private void btnPfInCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //검색조건 - 품번 라벨 클릭
        private void LabelBuyerArticleNoSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CheckBoxBuyerArticleNoSearch.IsChecked == true)
                {
                    CheckBoxBuyerArticleNoSearch.IsChecked = false;
                }
                else
                {
                    CheckBoxBuyerArticleNoSearch.IsChecked = true;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 체크박스 체크
        private void CheckBoxBuyerArticleNoSearch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBuyerArticleNoSearch.IsEnabled = true;
                ButtonBuyerArticleNoSearch.IsEnabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 체크박스 체크해제
        private void CheckBoxBuyerArticleNoSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBuyerArticleNoSearch.IsEnabled = false;
                ButtonBuyerArticleNoSearch.IsEnabled = false;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 텍스트박스 키다운 이벤트
        private void TextBoxBuyerArticleNoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 플러스파인더 버튼
        private void ButtonBuyerArticleNoSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //강경단 책임 주석처리 요청 2025-02-10
        //품명 라벨 클릭
        //private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if(chkArticleSrh.IsChecked == true)
        //    {
        //        chkArticleSrh.IsChecked = false;
        //        txtArticleSrh.IsEnabled = false;
        //        btnArticleSrh.IsEnabled = false;
        //    }
        //    else
        //    {
        //        chkArticleSrh.IsChecked = true;
        //        txtArticleSrh.IsEnabled = true;
        //        btnArticleSrh.IsEnabled = true;
        //    }
        //}

        ////품명 체크박스
        //private void chkArticleSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    if(chkArticleSrh.IsChecked == true)
        //    {            
        //        txtArticleSrh.IsEnabled = true;
        //        btnArticleSrh.IsEnabled = true;
        //    }
        //    else
        //    {
               
        //        txtArticleSrh.IsEnabled = false;
        //        btnArticleSrh.IsEnabled = false;
        //    }
        //}


        //품명 텍스트박스 키다운 이벤트
        //private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Key == Key.Enter)
        //        {
        //            pf.ReturnCode(txtArticleSrh, 5102, txtArticleSrh.Text);
        //        }
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("오류지점 - " + ee.ToString());
        //    }
        //}

        ////품명 플러스파인더 버튼
        //private void btnArticleSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        pf.ReturnCode(txtArticleSrh, 5102, txtArticleSrh.Text);
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("오류지점 - " + ee.ToString());
        //    }
        //}

        //수주번호
        private void lblOrderIDSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderIDSrh.IsChecked = chkOrderIDSrh.IsChecked == true ? false : true;
        }

        //수주번호
        private void chkOrderIDSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = true;
        }

        //수주번호
        private void chkOrderIDSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = false;
        }

        //완료구분
        private void lblCloseClssSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCloseClssSrh.IsChecked == true) { chkCloseClssSrh.IsChecked = false; }
            else { chkCloseClssSrh.IsChecked = true; }
        }

        //완료구분
        private void chkCloseClssSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboCloseClssSrh.IsEnabled = true;
        }

        //완료구분
        private void chkCloseClssSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboCloseClssSrh.IsEnabled = false;
        }

        //가공구분
        private void lblWorkSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkWorkSrh.IsChecked = chkWorkSrh.IsChecked == true ? false : true;
        }

        //가공구분
        private void chkWorkSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboWorkSrh.IsEnabled = true;
        }

        //가공구분
        private void chkWorkSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboWorkSrh.IsEnabled = false;
        }

        //주문구분
        private void lblOrderClassSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderClassSrh.IsChecked = chkOrderClassSrh.IsChecked == true ? false : true;
        }

        //주문구분
        private void chkOrderClassSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboOrderClassSrh.IsEnabled = true;
        }

        //주문구분
        private void chkOrderClassSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboOrderClassSrh.IsEnabled = false;
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
            grd2.IsHitTestVisible = false;
            grd3.IsHitTestVisible = false;
            grd4.IsHitTestVisible = false;
            grd5.IsHitTestVisible = false;
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
            grd2.IsHitTestVisible = true;
            grd3.IsHitTestVisible =true;
            grd4.IsHitTestVisible =true;
            grd5.IsHitTestVisible = true;
            lblMsg.Visibility = Visibility.Visible;
            dgdMain.IsHitTestVisible = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "I";
            boolCallEst = false;

            SetExtraSearchGrid_InitialState();
            HideExtraSearchConditionGrid();
            //UncheckCheckBox();

            //btnPreOrder.IsEnabled = true;
            //tabBasicData.Focus();            
            lstFilesName.Clear();

            //유지추가 활성화 여부 확인
            if (chkEoAddSrh.IsChecked == false) 
            {
                tbkMsg.Text = "자료 입력 중";
                orderID_global = string.Empty; 
                this.DataContext = new object(); 
                ClearGrdInput();

                //ovcOrder_Acc.Clear();
                //ovcOrder_localGov.Clear();
                //dgdAcc.ItemsSource = ovcOrder_Acc;
                //dgdLocalGov.ItemsSource = ovcOrder_localGov;

                rowAddAccnt();
                //var args = new SelectionChangedEventArgs(
                //           TabControl.SelectionChangedEvent,
                //           new List<object>(), // 이전 선택 항목들
                //           new List<object> { grdTabs.SelectedItem } // 새로 선택된 항목들
                //);
                //grdTabs.RaiseEvent(args);
                tabCheckTrue();
            }
            else { BringLastOrder(orderID_global); ClearGrdFtpTextBox(); }

            chkEoAddSrh.IsEnabled = false;    
            CantBtnControl();

            //탭순서대로 일을 진행하므로 초기화가 불필요하다고 함..
            //UncheckDatePicker();
            //SetDatePickerToday();
            //SetComboBoxIndexZero();
            setFTP_Tag_EmptyString();

            DatePickerSetToday_EventHandler();

            dtpAcptDate.SelectedDate = DateTime.Today; //견적번호 가지고 오는거 때문에 우선 오늘날짜로 셋팅
            //계약기간 오늘~금월 마지막일
            //dtpJobFromDate.SelectedDate = DateTime.Today;                                               //계약시작일
            //dtpJobToDate.SelectedDate = DateTime.Today.AddMonths(1).AddDays(-DateTime.Today.Day);       //계약종료일   

   
       
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //수정시에는 유지추가를 못하도록
            strFlag = "U";
            chkEoAddSrh.IsChecked = false;
            chkEoAddSrh.IsEnabled = false;
            boolCallEst = false;

            SetExtraSearchGrid_InitialState();
            HideExtraSearchConditionGrid();
            //UncheckCheckBox();

            OrderView = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_dgdMain;
            //btnPreOrder.IsEnabled = false;
            if (OrderView != null)
            {
                
                FillTabs(OrderView.orderId);
                //rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                
                CantBtnControl();
                Tab5TextBoxEnable();
                DatePickerSetToday_EventHandler();
                PrimaryKey = OrderView.orderId;
                tabCheckTrue();

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
                OrderView = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_dgdMain;

                string sql = string.Empty;

                //강의료정산 화면에서 orderID사용중
                if (OrderView != null)
                {

                    if (CheckDelete(OrderView))
                    {

                        if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {

                            if(MessageBox.Show($"수주번호 :{OrderView.orderNo} 데이터를 삭제합니다!\n정말 진행하시겠습니까?","경고",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                                    rowNum = dgdMain.SelectedIndex;

                                FTP_RemoveDir(OrderView.orderId);

                                if (DeleteData(OrderView.orderId))
                                {
                                    rowNum = Math.Max(0, rowNum - 1);
                                    re_Search(rowNum);
                                    MessageBox.Show("삭제 되었습니다.", "확인");
                                }
                            }
                        }
                    }              
                  
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnDelete.IsEnabled = true;
        }

        //삭제 전 확인
        private bool CheckDelete(Win_ord_Order_U_CodeView_dgdMain OrderView)
        {
            bool flag = true;

            //string[] sqlList = { "select orderID from Order_DcntFees where orderID = ",
            //                     "select orderid from Order_Lecturer where orderid = ",
            //                     "select orderid from HR_DcntEvaluate where orderid = ",
            //                     "select orderid from StuffIN where OrderID = ",
            //                     "select orderid from Order_DcntFees where orderid = ",
            //                     "select orderid from OutWare where OrderID = ",
            //};

            //string[] errMsg = {"강의료 정산 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //                   "강의매칭 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //                   "강사평가등록 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //                   "교구입고등록 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //                   "강의료정산 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //                   "교구출고 화면에서 사용중인 계약관리번호 입니다. 삭제할 수 없습니다.",
            //};
            //int errSeq = 0;
            //string msg = string.Empty;

            ////반복문을 돌다가 걸리면 종료, 경고문 띄우고 false반환
            //for(int i = 0; i < 2; i++)
            //{
            //    DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + OrderView.orderId);
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

            //if(flag == false)
            //{                
            //    msg = errMsg[errSeq];
            //    MessageBox.Show(msg);
            //}
            
            return flag;
        }

        private void tabCheckTrue()
        {
            tab2_clicked = true;
            tab3_clicked = true;
            tab4_clicked = true;
            tab5_clicked = true;
        }

        private void tabCheckFalse()
        {
            tab2_clicked = false;
            tab3_clicked = false;
            tab4_clicked = false;
            tab5_clicked = false;
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

            CheckTabClicked();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if(chkUniversalSearhSrh.IsChecked == true)
                {
                    if(txtUniversalSearhSrh.Text.Trim().Length >= 3)
                    {
                        Fillgrid_re_Search();
                    }
                    else
                    {
                        MessageBox.Show("검색어는 3글자 이상 입력하세요", "확인");
                    }
                  
                }
                else
                {
                    re_Search(rowNum);
                }

            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        private void Fillgrid_re_Search()
        {
            string searchText = txtUniversalSearhSrh.Text.Equals("검색어를 입력하세요") ? string.Empty : txtUniversalSearhSrh.Text;  

            if (string.IsNullOrEmpty(searchText.Trim()))
            {
                // 모든 행 보이기
                ShowAllRows();
                return;
            }

            List<string> orderID_List = new List<string>();
            for (int i = 0; i < dgdMain.Items.Count; i++)
            {
                var item = dgdMain.Items[i] as Win_ord_Order_U_CodeView_dgdMain;
                if (item != null)
                {
                    orderID_List.Add(item.orderId);
                }
            }

   
            var keywordMatchedOrders = CheckKeyWordsInTabs(orderID_List, searchText);

            for (int i = 0; i < dgdMain.Items.Count; i++)
            {
                var item = dgdMain.Items[i] as Win_ord_Order_U_CodeView_dgdMain;
                var row = dgdMain.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;

                if (item != null && row != null)
                {
                    bool isMatch = keywordMatchedOrders.Contains(item.orderId);
                    row.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private HashSet<string> CheckKeyWordsInTabs(List<string> orderid_list, string keywords)
        {
            var matchesOrderIDs = new HashSet<string>();

            try
            {

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("OrderIDs", string.Join(",",orderid_list));
                sqlParameter.Add("KeyWords", keywords);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder_KeywordSearch", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];                    

                    DataRowCollection drc = dt.Rows;

                    foreach (DataRow dr in drc)
                    {
                        matchesOrderIDs.Add(dr["orderID"].ToString());                            
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생 : "+ ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return matchesOrderIDs;
        }


        private void ShowAllRows()
        {
            for (int i = 0; i < dgdMain.Items.Count; i++)
            {
                var row = dgdMain.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (row != null)
                    row.Visibility = Visibility.Visible;
            }
        }

        private void CheckTabClicked()
        {
            TabItem selectedTab = grdTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                switch (selectedTab.Name)
                {
                    case "tab2":
                        tab2_clicked = true;
                        break;
                    case "tab3":
                        tab3_clicked = true;
                        break;
                    case "tab4":
                        tab4_clicked = true;
                        break;
                    case "tab5":
                        tab5_clicked = true;
                        break;
                }

            }
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {         
            //using (Loading ld = new Loading(beSave))
            //{
            //    ld.ShowDialog();
            //}
            beSave();
        }

        //메세지를 업데이트
        private void UpdateTbkMessage(string message)
        {
            tbkMsg.Text = message;
            tbkMsg.UpdateLayout();
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            // UI 업데이트를 위한 짧은 대기
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
            Thread.Sleep(10);
        }


        private void beSave()
        {
            btnSave.IsEnabled = false;


            //Dispatcher.BeginInvoke(new Action(() =>
            //{
                if (dgdAcc.Items.Count > 0) DataGrid_LostFocus_Calculate(dgdAcc, new RoutedEventArgs());
                else { txtdgdAccTotal.Text = ""; }

                //로직
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    lblMsg.Visibility = Visibility.Hidden;
                    dgdMain.IsHitTestVisible = true;
                    PrimaryKey = string.Empty;
                    orderID_global = string.Empty;                   
                    rowNum = strFlag == "I" ? dgdMain.Items.Count : strFlag == "U" ? rowNum : 0;
                    chkEoAddSrh.IsChecked = false;
                    chkEoAddSrh.IsEnabled = true;
                    re_Search(rowNum);
                    DatePickerSetToday_RemoveHandler();
                    MessageBox.Show("저장이 완료되었습니다.");
                    strFlag = string.Empty;
                    boolCallEst = false;
                    // 현재 선택된 탭 데이터 새로고침
                 }
            //}), System.Windows.Threading.DispatcherPriority.Background);

            btnSave.IsEnabled = true;
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {   
            CanBtnControl();

            ovcOrder_Acc.Clear();            
            ovcOrder_localGov.Clear();
            dgdAccnt.Items.Clear();

            dgdMain.IsHitTestVisible = true;       
            chkEoAddSrh.IsChecked = false;
            chkEoAddSrh.IsEnabled = true;
            //btnPreOrder.IsEnabled = false;

            if (strFlag.Equals("U"))
            {
                re_Search(rowNum);
            }
            else
            {
                re_Search(rowNum);
            }

            DatePickerSetToday_RemoveHandler();
            strFlag = string.Empty;
            boolCallEst = false;

        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] lst = new string[2];
            lst[0] = "계약등록 조회 목록";
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

        //private void btnGoReservation_Click(object sender, RoutedEventArgs e)
        //{
        //    // 있으면 진행, 없으면 리턴
        //    if (txtReserveID.Text != "")
        //    {
        //        MainWindow.reServeID = txtReserveID.Text;
        //    }
        //    else
        //    {
        //        MessageBox.Show("상담번호가 없습니다.");
        //        return;
        //    }


        //    // 재고현황(제품포함)
        //    int i = 0;
        //    foreach (MenuViewModel mvm in MainWindow.mMenulist)
        //    {
        //        if (mvm.Menu.Equals("상담등록"))
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


        // 주문일괄 업로드
        string upload_fileName = "";

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();
            //file.Filter = "Excel files (*.xls,*xlsx)|*.xls;*xlsx|All files (*.*)|*.*";
            //file.InitialDirectory = "C:\\";

            //if (file.ShowDialog() == true)
            //{
            //    upload_fileName = file.FileName;

            //    btnUpload.IsEnabled = false;

            //    using (Loading ld = new Loading("excel", beUpload))
            //    {
            //        ld.ShowDialog();
            //    }

            //    re_Search(0);

            //    btnUpload.IsEnabled = true;
            //}
        }

        //private void beUpload()
        //{
        //    Lib lib2 = new Lib();

        //    Excel.Application excelapp = null;
        //    Excel.Workbook workbook = null;
        //    Excel.Worksheet worksheet = null;
        //    Excel.Range workrange = null;

        //    List<OrderExcel> listExcel = new List<OrderExcel>();

        //    try
        //    {
        //        excelapp = new Excel.Application();
        //        workbook = excelapp.Workbooks.Add(upload_fileName);
        //        worksheet = workbook.Sheets["Sheet"];
        //        workrange = worksheet.UsedRange;

        //        for (int row = 3; row <= workrange.Rows.Count; row++)
        //        {
        //            OrderExcel excel = new OrderExcel();
        //            excel.CustomID = workrange.get_Range("A" + row.ToString()).Value2;
        //            excel.Model = workrange.get_Range("B" + row.ToString()).Value2;
        //            excel.BuyerArticleNo = workrange.get_Range("C" + row.ToString()).Value2;
        //            excel.Article = workrange.get_Range("D" + row.ToString()).Value2;
        //            excel.UnitClss = workrange.get_Range("E" + row.ToString()).Value2;

        //            object objOrderQty = workrange.get_Range("H" + row.ToString()).Value2;
        //            if (objOrderQty != null)
        //                excel.OrderQty = objOrderQty.ToString();

        //            if (!string.IsNullOrEmpty(excel.CustomID)
        //                && !string.IsNullOrEmpty(excel.BuyerArticleNo) && !string.IsNullOrEmpty(excel.Article)
        //                && !string.IsNullOrEmpty(excel.UnitClss) && !string.IsNullOrEmpty(excel.OrderQty))
        //            {
        //                listExcel.Add(excel);
        //            }

        //            if (string.IsNullOrEmpty(excel.CustomID) && string.IsNullOrEmpty(excel.Model)
        //                && string.IsNullOrEmpty(excel.BuyerArticleNo) && string.IsNullOrEmpty(excel.Article)
        //                && string.IsNullOrEmpty(excel.UnitClss) && string.IsNullOrEmpty(excel.OrderQty))
        //            {
        //                break;
        //            }
        //        }

        //        if (listExcel.Count > 0)
        //        {
        //            List<Procedure> Prolist = new List<Procedure>();
        //            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
        //            for (int i = 0; i < listExcel.Count; i++)
        //            {
        //                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
        //                sqlParameter.Add("CustomID", string.IsNullOrEmpty(listExcel[i].CustomID) ? "" : listExcel[i].CustomID);
        //                sqlParameter.Add("Model", string.IsNullOrEmpty(listExcel[i].Model) ? "" : listExcel[i].Model);
        //                sqlParameter.Add("BuyerArticleNo", string.IsNullOrEmpty(listExcel[i].BuyerArticleNo) ? "" : listExcel[i].BuyerArticleNo);
        //                sqlParameter.Add("Article", string.IsNullOrEmpty(listExcel[i].Article) ? "" : listExcel[i].Article);
        //                sqlParameter.Add("UnitClss", string.IsNullOrEmpty(listExcel[i].UnitClss) ? "" : listExcel[i].UnitClss);
        //                sqlParameter.Add("OrderQty", string.IsNullOrEmpty(listExcel[i].OrderQty) ? "" : listExcel[i].OrderQty);

        //                Procedure pro2 = new Procedure();
        //                pro2.Name = "xp_Order_iOrderExcel";
        //                pro2.OutputUseYN = "N";
        //                pro2.OutputName = "";
        //                pro2.OutputLength = "10";

        //                Prolist.Add(pro2);
        //                ListParameter.Add(sqlParameter);
        //            }

        //            string[] Confirm = new string[2];
        //            Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "C");
        //            if (Confirm[0] != "success")
        //                MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
        //            else
        //                MessageBox.Show("업로드가 완료되었습니다.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message.ToString());
        //    }
        //    finally
        //    {
        //        excelapp.Visible = true;
        //        workbook.Close(true);
        //        excelapp.Quit();

        //        lib2.ReleaseExcelObject(workbook);
        //        lib2.ReleaseExcelObject(worksheet);
        //        lib2.ReleaseExcelObject(excelapp);
        //        lib2 = null;

        //        upload_fileName = "";
        //        listExcel.Clear();
        //    }
        //}

        private int SelectItem(string strPrimary, DataGrid dataGrid)
        {
            int index = 0;

            try
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    var Item = dataGrid.Items[i] as Win_ord_Order_U_CodeView_dgdMain;

                    if (strPrimary.Equals(Item.orderId))
                    {
                        index = i;
                        break;
                    }
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
            strFlag = string.Empty;
            if (dgdMain.Items.Count > 0)
            {                 
                dgdMain.SelectedIndex = selectedIndex;

//                var args = new SelectionChangedEventArgs(
//                           TabControl.SelectionChangedEvent,
//                           new List<object>(), // 이전 선택 항목들
//                           new List<object> { grdTabs.SelectedItem } // 새로 선택된 항목들
//);
//                grdTabs.RaiseEvent(args);

            }
            else
                this.DataContext = new object();


            SetExtraSearchGrid_InitialState();
            //CalculGridSum();
        }

        //실조회
        private void FillGrid()
        {

            ClearGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdTotal.Items.Clear();
                dgdMain.Items.Clear();
            }

            int sumAmount = 0;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ChkDate", ChkDateSrh.IsChecked == true ? (intFlag == 1 ? 0 : 1) : 0);
                sqlParameter.Add("SDate", ChkDateSrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", ChkDateSrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                // 운영사
                sqlParameter.Add("ChkManageCustomId", chkManagerCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManageCustomId", chkManagerCustomIdSrh.IsChecked == true ? (txtManagerCustomIdSrh.Tag != null ? txtManagerCustomIdSrh.Tag.ToString() : "") : "");

                //영업사
                //sqlParameter.Add("ChkSalesCustomId", chkSalesCustomIdSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("SalesCustomId", chkSalesCustomIdSrh.IsChecked == true ? (txtSalesCustomIdSrh.Tag != null ? txtSalesCustomIdSrh.Tag.ToString() : "") : "");

                //영업담당
                sqlParameter.Add("ChkSaledamdangjaName", chkSaledamdangjaNameSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SaledamdangjaName", chkSaledamdangjaNameSrh.IsChecked == true ? txtSaledamdangjaNameSrh.Text : "");

                // 품목 강경단 책임 주석처리 요청 2025-02-11
                //sqlParameter.Add("ChkArticleId", chkArticleSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ArticleId", chkArticleSrh.IsChecked == true ? (txtArticleSrh.Tag != null ? txtArticleSrh.Tag.ToString() : "") : "");

                //한전&전기공사 탭 시공사업체
                sqlParameter.Add("ChkConstrCustomId", chkConstrCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ConstrCustomId", chkConstrCustomIdSrh.IsChecked == true ? (txtConstrCustomIdSrh.Tag != null ? txtConstrCustomIdSrh.Tag.ToString() : "") : "");

                // 마감포함
                sqlParameter.Add("ChkCloseYn", chkCloseClssSrh.IsChecked == true ? 1 : 0);  

                // 지역구분
                sqlParameter.Add("ChkZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? cboZoneGbnIdSrh.SelectedValue : "");

                // 전기조달방법
                sqlParameter.Add("ChkElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? cboElecDeliMethSrh.SelectedValue.ToString() : "");
                sqlParameter.Add("ElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? txtElecDeliMethSrh.Text : "");

                // 국소명
                sqlParameter.Add("ChkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstallLocationSrh.Text : "");

                // 사업구분
                sqlParameter.Add("ChkOrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? cboOrderTypeIDSrh.SelectedValue.ToString(): "");

                //비고
                //sqlParameter.Add("ChkInstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("InstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? txtInstallLocationAddCommentsSrh.Text : "");

                //견적제목
                sqlParameter.Add("chkEstSubject", chkEstSubjectSrh.IsChecked == true ? 1:0);
                sqlParameter.Add("EstSubject", chkEstSubjectSrh.IsChecked == true ? !string.IsNullOrWhiteSpace(txtEstSubjectSrh.Text) ? txtEstSubjectSrh.Text : string.Empty : string.Empty);


              sqlParameter.Add("orderID", intFlag == 1 ? tblOrderID.Text : "");

              sqlParameter.Add("ChkManageCustomConfirmDate", chkManageCustomConfirmDateSrh.IsChecked == true ? 1: 0);
              sqlParameter.Add("ManageCustomConfirmDateTo", chkManageCustomConfirmDateSrh.IsChecked == true ? !IsDatePickerNull(dtpManageCustomConfirmDateToSrh) ? ConvertDate(dtpManageCustomConfirmDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ManageCustomConfirmDateEnd",chkManageCustomConfirmDateSrh.IsChecked == true ? !IsDatePickerNull(dtpManageCustomConfirmDateEndSrh) ? ConvertDate(dtpManageCustomConfirmDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkInstallLocationAdress", chkInstallLocationAdressSrh.IsChecked == true? 1:0);
              sqlParameter.Add("InstallLocationAdress", chkInstallLocationAdressSrh.IsChecked == true ? !string.IsNullOrWhiteSpace(txtInstallLocationAdressSrh.Text) ? txtInstallLocationAdressSrh.Text : string.Empty: string.Empty);

              sqlParameter.Add("ChkOpenReqDate", chkOpenReqDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("OpenReqDateTo", chkOpenReqDateSrh.IsChecked == true? !IsDatePickerNull(dtpOpenReqDateToSrh) ? ConvertDate(dtpOpenReqDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("OpenReqDateEnd", chkOpenReqDateSrh.IsChecked ==true ? !IsDatePickerNull(dtpOpenReqDateEndSrh) ? ConvertDate(dtpOpenReqDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkOpenDate", chkOpenDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("OpenDateTo", chkOpenDateSrh.IsChecked == true? !IsDatePickerNull(dtpOpenDateToSrh) ? ConvertDate(dtpOpenDateToSrh) : string.Empty : string.Empty );
              sqlParameter.Add("OpenDateEnd", chkOpenDateSrh.IsChecked == true ? !IsDatePickerNull(dtpOpenDateEndSrh) ? ConvertDate(dtpOpenDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkCpoCalcuDate", chkCpoCalcuDateSrh.IsChecked == true ? 1 : 0);
              sqlParameter.Add("CpoCalcuDateTo", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuDateToSrh) ? ConvertDate(dtpCpoCalcuDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("CpoCalcuDateEnd", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuDateEndSrh) ? ConvertDate(dtpCpoCalcuDateEndSrh) : string.Empty : string.Empty );

              sqlParameter.Add("ChkConstrCalcuDate", chkConstrCalcuDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("ConstrCalcuDateTo", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuDateToSrh) ? ConvertDate(dtpConstrCalcuDateToSrh)  : string.Empty : string.Empty);
              sqlParameter.Add("ConstrCalcuDateEnd", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuDateEndSrh) ? ConvertDate(dtpConstrCalcuDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSearchReqDate", chkSearchReqDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("SearchReqDateTo", chkSearchReqDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchReqDateToSrh) ? ConvertDate(dtpSearchReqDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SearchReqDateEnd", chkSearchReqDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchReqDateEndSrh) ? ConvertDate(dtpSearchReqDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSearchDate", chkSearchDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("SearchDateTo", chkSearchDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchDateToSrh) ? ConvertDate(dtpSearchDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SearchDateEnd", chkSearchDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchDateEndSrh) ? ConvertDate(dtpSearchDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSearchDataAcptDate", chkSearchDataAcptDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("SearchDataAcptDateTo", chkSearchDataAcptDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchDataAcptDateToSrh) ? ConvertDate(dtpSearchDataAcptDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SearchDataAcptDateEnd", chkSearchDataAcptDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSearchDataAcptDateEndSrh) ? ConvertDate(dtpSearchDataAcptDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkCorpAcptNo", chkCorpAcptNoSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("CorpAcptNo", chkCorpAcptNoSrh.IsChecked ==true ? txtCorpAcptNo.Text : string.Empty);

              sqlParameter.Add("ChkCorpApprovalDate", chkCorpApprovalDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("CorpApprovalDateTo", chkCorpApprovalDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCorpApprovalDateToSrh) ? ConvertDate(dtpCorpApprovalDateToSrh) :  string.Empty : string.Empty);
              sqlParameter.Add("CorpApprovalDateEnd", chkCorpApprovalDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCorpApprovalDateEndSrh) ? ConvertDate(dtpCorpApprovalDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkCorpEndDate",chkCorpEndDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("CorpEndDateTo", chkCorpEndDateSrh.IsChecked == true? !IsDatePickerNull(dtpCorpApprovalDateToSrh) ? ConvertDate(dtpCorpApprovalDateEndSrh) : string.Empty : string.Empty);
              sqlParameter.Add("CorpEndDateEnd", chkCorpEndDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCorpApprovalDateEndSrh) ? ConvertDate(dtpCorpApprovalDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSuperUseInspReqDate", chkSuperUseInspReqDateSrh.IsChecked == true ?  1:0);
              sqlParameter.Add("SuperUseInspReqDateTo", chkSuperUseInspReqDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperUseInspReqDateToSrh) ? ConvertDate(dtpSuperUseInspReqDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SuperUseInspReqDateEnd", chkSuperUseInspReqDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperUseInspReqDateEndSrh) ? ConvertDate(dtpSuperUseInspReqDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSuperBeforeUseInspPrintDate",chkSuperBeforeUseInspPrintDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("SuperBeforeUseInspPrintDateTo", chkSuperBeforeUseInspPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperUseInspReqDateToSrh) ? ConvertDate(dtpSuperUseInspReqDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SuperBeforeUseInspPrintDateEnd", chkSuperBeforeUseInspPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperUseInspReqDateEndSrh) ? ConvertDate(dtpSuperUseInspReqDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkKepOutLineConstructContext", chkKepOutLineConstructContextSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("KepOutLineConstructContext", chkKepOutLineConstructContextSrh.IsChecked == true? txtKepOutLineConstructContextSrh.Text : string.Empty);

              sqlParameter.Add("ChkKepElectrReqDate", chkKepElectrReqDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("KepElectrReqDateTo", chkKepElectrReqDateSrh.IsChecked == true? !IsDatePickerNull(dtpKepElectrReqDateToSrh) ? ConvertDate(dtpKepElectrReqDateEndSrh) : string.Empty : string.Empty);
              sqlParameter.Add("KepElectrReqDateEnd", chkKepElectrReqDateSrh.IsChecked == true? !IsDatePickerNull(dtpKepElectrReqDateEndSrh) ? ConvertDate(dtpKepElectrReqDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkKepParentChildCapacity", chkKepParentChildCapacitySrh.IsChecked == true ? 1:0);
              sqlParameter.Add("KepParentChildCapacityTo", chkKepParentChildCapacitySrh.IsChecked == true ? lib.RemoveComma(txtKepParentChildCapacityToSrh.Text,0) : 0);
              sqlParameter.Add("KepParentChildCapacityEnd",chkKepParentChildCapacitySrh.IsChecked == true ? lib.RemoveComma(txtKepParentChildCapacityEndSrh.Text,0) : 0);

              sqlParameter.Add("ChkKepPowerSupplyCapacity", chkKepPowerSupplyCapacitySrh.IsChecked ==true ? 1:0);
              sqlParameter.Add("KepPowerSupplyCapacityTo", chkKepPowerSupplyCapacitySrh.IsChecked == true ?lib.RemoveComma(txtKepPowerSupplyCapacityToSrh.Text,0) : 0);
              sqlParameter.Add("KepPowerSupplyCapacityEnd", chkKepPowerSupplyCapacitySrh.IsChecked == true ? lib.RemoveComma(txtKepPowerSupplyCapacityEndSrh.Text, 0) : 0);

              sqlParameter.Add("ChkKepCustomNo", chkKepCustomNoSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("KepCustomNo", chkKepCustomNoSrh.IsChecked == true ? txtKepCustomNoSrh.Text : string.Empty);

              sqlParameter.Add("ChkConstrDate", chkConstrDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("ConstrDateTo", chkConstrDateSrh.IsChecked == true? !IsDatePickerNull(dtpConstrDateToSrh) ? ConvertDate(dtpConstrDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ConstrDateEnd", chkConstrDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrDateEndSrh) ? ConvertDate(dtpConstrDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkConstrCompleteDate", chkConstrCompleteDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("ConstrCompleteDateTo", chkConstrCompleteDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCompleteDateToSrh) ? ConvertDate(dtpConstrCompleteDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ConstrCompleteDateEnd", chkConstrCompleteDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCompleteDateEndSrh) ? ConvertDate(dtpConstrCompleteDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkElectrSafeCheckPrintDate", chkElectrSafeCheckPrintDateSrh.IsChecked == true? 1:0);
              sqlParameter.Add("ElectrSafeCheckPrintDateTo", chkElectrSafeCheckPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpElectrSafeCheckPrintDateToSrh) ? ConvertDate(dtpElectrSafeCheckPrintDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ElectrSafeCheckPrintDateEnd", chkElectrSafeCheckPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpElectrSafeCheckPrintDateEndSrh) ? ConvertDate(dtpElectrSafeCheckPrintDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkElectrBeforeUseCheckPrintDate", chkElectrBeforeUseCheckPrintDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("ElectrBeforeUseCheckPrintDateTo", chkElectrBeforeUseCheckPrintDateSrh.IsChecked ==  true ? !IsDatePickerNull(dtpElectrBeforeUseCheckPrintDateToSrh) ? ConvertDate(dtpElectrBeforeUseCheckPrintDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ElectrBeforeUseCheckPrintDateEnd", chkElectrBeforeUseCheckPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpElectrBeforeUseCheckPrintDateEndSrh) ? ConvertDate(dtpElectrBeforeUseCheckPrintDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkElectrBeforeInspPrintDate", chkElectrBeforeInspPrintDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("ElectrBeforeInspPrintDateTo", chkElectrBeforeInspPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpElectrBeforeInspPrintDateToSrh) ? ConvertDate(dtpElectrBeforeInspPrintDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("ElectrBeforeInspPrintDateEnd", chkElectrBeforeInspPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpElectrBeforeInspPrintDateEndSrh) ? ConvertDate(dtpElectrBeforeInspPrintDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkSuperCustomID", chkSuperCustomIDSrh.IsChecked == true?1:0);
              sqlParameter.Add("SuperCustomID", chkSuperCustomIDSrh.IsChecked == true ? txtSuperCustomIDSrh.Tag != null? txtSuperCustomIDSrh.Tag.ToString() : string.Empty : string.Empty);

              sqlParameter.Add("ChkSafeManageCustomID", chkSafeManageCustomIDSrh.IsChecked == true? 1:0);
              sqlParameter.Add("SafeManageCustomID", chkSafeManageCustomIDSrh.IsChecked == true ? txtSuperCustomIDSrh.Tag != null? txtSuperCustomIDSrh.Tag.ToString() : string.Empty : string.Empty);

              sqlParameter.Add("ChkSuperUseInspPayCustomID", chkSuperUseInspPayCustomIDSrh.IsChecked ==true ? 1:0);
              sqlParameter.Add("SuperUseInspPayCustomID", chkSuperUseInspPayCustomIDSrh.IsChecked == true ? txtSuperUseInspPayCustomIDSrh.Tag != null? txtSuperUseInspPayCustomIDSrh.Tag.ToString() : string.Empty : string.Empty);

              sqlParameter.Add("ChkSuperSetTaxPrintDate", chkSuperSetTaxPrintDateSrh.IsChecked == true? 1:0);
              sqlParameter.Add("SuperSetTaxPrintDateTo", chkSuperSetTaxPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperSetTaxPrintDateToSrh) ? ConvertDate(dtpSuperSetTaxPrintDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SuperSetTaxPrintDateEnd", chkSuperSetTaxPrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuperSetTaxPrintDateEndSrh) ? ConvertDate(dtpSuperSetTaxPrintDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkCompReplyDate", chkCompReplyDateSrh.IsChecked == true? 1:0);
              sqlParameter.Add("CompReplyDateTo", chkCompReplyDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCompReplyDateToSrh) ? ConvertDate(dtpCompReplyDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("CompReplyDateEnd", chkCompReplyDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCompReplyDateEndSrh) ? ConvertDate(dtpCompReplyDateEndSrh) :  string.Empty : string.Empty);

              sqlParameter.Add("ChkSuppleCompDate", chkSuppleCompDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("SuppleCompDateTo", chkSuppleCompDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuppleCompDateToSrh) ? ConvertDate(dtpSuppleCompDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("SuppleCompDateEnd", chkSuppleCompDateSrh.IsChecked == true ? !IsDatePickerNull(dtpSuppleCompDateEndSrh) ? ConvertDate(dtpSuppleCompDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkCompSuppleReportDate", chkCompSuppleReportDateSrh.IsChecked == true ? 1 : 0);
              sqlParameter.Add("CompSuppleReportDateTo", chkCompSuppleReportDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCompSuppleReportDateToSrh) ? ConvertDate(dtpCompSuppleReportDateToSrh) : string.Empty : string.Empty);
              sqlParameter.Add("CompSuppleReportDateEnd", chkCompSuppleReportDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCompSuppleReportDateEndSrh) ? ConvertDate(dtpCompSuppleReportDateEndSrh) : string.Empty : string.Empty);

              sqlParameter.Add("ChkInsurePrintDate", chkInsurePrintDateSrh.IsChecked == true ? 1:0);
              sqlParameter.Add("InsurePrintDateTo", chkInsurePrintDateSrh.IsChecked ==true? !IsDatePickerNull(dtpInsurePrintDateToSrh) ? ConvertDate(dtpInsurePrintDateEndSrh) : string.Empty : string.Empty);
              sqlParameter.Add("InsurePrintDateEnd", chkInsurePrintDateSrh.IsChecked == true ? !IsDatePickerNull(dtpInsurePrintDateEndSrh) ? ConvertDate(dtpInsurePrintDateEndSrh) : string.Empty : string.Empty);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //ClearGrdInput();
                        //UncheckDatePicker();
                        MessageBox.Show("조회된 데이터가 없습니다.");
                      
                    }
                    else
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var OrderCodeView = new Win_ord_Order_U_CodeView_dgdMain
                            {
                                num = i,
                                orderId = dr["orderId"].ToString(),
                                acptDate = DateTypeHyphen(dr["acptDate"].ToString()),
                                estSubject = dr["estSubject"].ToString(),                               
                                estID = dr["estID"].ToString(),
                                orderType = dr["orderType"].ToString(),
                                orderTypeID = dr["orderTypeID"].ToString(),
                                orderNo = dr["orderNo"].ToString(),
                                saleCustom = dr["saleCustom"].ToString(),
                                saleCustomID = dr["saleCustomID"].ToString(),

                                managerCustom = dr["managerCustom"].ToString(),
                                managerCustomID = dr["managerCustomID"].ToString(),
                                searchCustomID = dr["searchCustomID"].ToString(),
                                searchCustom = dr["searchCustom"].ToString(),
                                manageCustomAcptDate = DateTypeHyphen(dr["manageCustomAcptDate"].ToString()),
                                manageCustomConfirmDate = DateTypeHyphen(dr["manageCustomConfirmDate"].ToString()),

                                installLocation = dr["installLocation"].ToString(),
                                //installLocationPart = dr["installLocationPart"].ToString(),
                                InstallLocationPhone = dr["InstallLocationPhone"].ToString(),
                                articleList = dr["articleList"].ToString(),
                                closeYn = dr["closeYn"].ToString(),

                                orderAmount = stringFormatN0(dr["orderAmount"]),
                                installLocationAddComments = dr["installLocationAddComments"].ToString(),
                                cpoCalcuDate = DateTypeHyphen(dr["cpoCalcuDate"].ToString()),
                                constrCalcuDate = DateTypeHyphen(dr["constrCalcuDate"].ToString()),
                                installLocationAddress = dr["installLocationAddress"].ToString(),
                                houseHoldCount = stringFormatN0(dr["houseHoldCount"]),

                                carParkingCount = stringFormatN0(dr["carParkingCount"]),
                                alreadyManageCustom = dr["alreadyManageCustom"].ToString(),
                                //alreadyManageCustomID = dr["alreadyManageCustomID"].ToString(),
                                installLocationComments = dr["installLocationComments"].ToString(),
                                alReadyChargeCount = dr["alReadyChargeCount"].ToString(),

                                contractToDate = DateTypeHyphen(dr["contractToDate"].ToString()),
                                contractFromDate = DateTypeHyphen(dr["contractFromDate"].ToString()),
                                openReqDate = DateTypeHyphen(dr["openReqDate"].ToString()),
                                openDate = DateTypeHyphen(dr["openDate"].ToString()),
                                //damdangjaName = dr["damdangjaName"].ToString(),

                                damdangjaEMail = dr["damdangjaEMail"].ToString(),
                                //damdangjaPhone = dr["damdangjaPhone"].ToString(),
                                electrCarCount = stringFormatN0(dr["electrCarCount"]),
                                reqChargeCount = stringFormatN0(dr["reqChargeCount"]),
                                saledamdangjaName = dr["saledamdangjaName"].ToString(),
                                saledamdangjaEmail = dr["saledamdangjaEmail"].ToString(),
                                saledamdangjaPhone = dr["saledamdangjaPhone"].ToString(),

                                saleCustomAddWork = dr["saleCustomAddWork"].ToString(),
                                salegift = dr["salegift"].ToString(),
                                salesComments = dr["salesComments"].ToString(),
                                mtrAmount = stringFormatN0(dr["mtrAmount"]),
                                mtrShippingCharge = stringFormatN0(dr["mtrShippingCharge"]),
                                mtrPriceUnitClss = dr["mtrPriceUnitClss"].ToString(),
                                mtrPriceUnit = dr["mtrPriceUnit"].ToString(),

                                mtrCanopyInwareInfo = dr["mtrCanopyInwareInfo"].ToString(),
                                mtrCanopyOrderAmount = stringFormatN0(dr["mtrCanopyOrderAmount"]),

                                contractFileName = dr["contractFileName"].ToString(),
                                contractFilePath = dr["contractFilePath"].ToString(),


                            };

                            sumAmount += (int)RemoveComma(OrderCodeView.orderAmount, true);


                            dgdMain.Items.Add(OrderCodeView);
                        }
                    }
                }

                if (dgdMain.Items.Count > 0)
                {
                    var OrderCodeView_Total = new Win_order_Order_U_CodView_dgdTotal
                    {
                        count = dgdMain.Items.Count.ToString(),
                        totalSum = stringFormatN0(sumAmount)
                    };

                    dgdTotal.Items.Add(OrderCodeView_Total);
                }

                //if (dgdMain.Items.Count > 0)
                //{
                //    dgdMain.Focus();
                //    dgdMain.SelectedIndex = rowNum;
                //    dgdMain.CurrentCell = dgdMain.SelectedCells[0];
                //}
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

        private void fillAccGrid(string orderId)
        {
            dgdAcc.ItemsSource = null;
            ovcOrder_Acc.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrderSub_dgdAcc", sqlParameter, true, "R");

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
                        int sum = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var accList = new Win_order_Order_U_CodView_dgdAcc
                            {
                                num = i,
                                orderSeq = dr["orderSeq"].ToString(),
                                chargeInwareUnitPrice = stringFormatN0(dr["chargeInwareUnitPrice"]),
                                articleID = dr["articleID"].ToString(),
                                article = dr["article"].ToString(),
                                orderTypeID = dr["orderTypeID"].ToString(),
                                orderType = dr["orderType"].ToString(),
                                chargeOrderDate= DateTypeHyphen(dr["chargeOrderDate"].ToString()),
                                chargeOrderQty = stringFormatN0(dr["chargeOrderQty"]),
                                chargeInwareDate = DateTypeHyphen(dr["chargeInwareDate"].ToString()),
                                chargeInwareQty = stringFormatN0(dr["chargeInwareQty"]),
                                chargeInwareLocation = dr["chargeInwareLocation"].ToString(),
                                canopyReqCustom = dr["canopyReqCustom"].ToString(),
                                canopyReqCustomID = dr["canopyReqCustomID"].ToString(),
                                chargeModelHelmat = dr["chargeModelHelmat"].ToString(),
                                chargeModelinloc = dr["chargeModelinloc"].ToString(),
                                chargeModelOneBody = dr["chargeModelOneBody"].ToString(),
                                chargeStandReqDate = DateTypeHyphen(dr["chargeStandReqDate"].ToString()),
                                chargeStandInwareDate= DateTypeHyphen(dr["chargeStandInwareDate"].ToString()),
                                Comments = dr["Comments"].ToString(),
                                ovcOrderTypeAcc = ovcOrderTypeAcc

                            };
                            sum += (int)RemoveComma(accList.chargeInwareUnitPrice,true) * (int)RemoveComma(accList.chargeInwareQty,true);

                            ovcOrder_Acc.Add(accList);
                        }

                        dgdAccTotal.Items.Clear();
                        var accTotal = new Win_order_Order_U_CodView_dgdAcc_Total
                        {
                            num = i,
                            totalSum = stringFormatN0(sum),
                        };

                        dgdAccTotal.Items.Add(accTotal);
                    }

                    dgdAcc.ItemsSource = ovcOrder_Acc;
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
        private void callEstData(string estID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", 0);
                sqlParameter.Add("sDate",  "");
                sqlParameter.Add("eDate",  "");

                sqlParameter.Add("chkManagerCustomID", 0);
                sqlParameter.Add("ManagerCustomID", "");

                sqlParameter.Add("chkArticleID", 0);
                sqlParameter.Add("ArticleID", "");

                sqlParameter.Add("chkElecDeliMeth", 0);
                sqlParameter.Add("ElecDeliMeth", "");

                sqlParameter.Add("chkZoneGbnID", 0);
                sqlParameter.Add("ZoneGbnID", "");

                sqlParameter.Add("chkSmallInstallLocation", 0);
                sqlParameter.Add("smallInstallLocation", "");

                sqlParameter.Add("chkComments", 0);
                sqlParameter.Add("Comments", "");

                sqlParameter.Add("chkEstSubject", 0);
                sqlParameter.Add("EstSubject", "");

                // 사업구분
                sqlParameter.Add("chkOrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? cboOrderTypeIDSrh.SelectedValue.ToString() : "");

                ////수주등록에서 넘어왔을 때 바로 조회용도 textblock에 적어놓고 hidden처리함
                sqlParameter.Add("EstID", estID);

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

                        DataRow dr = dt.Rows[0];


                        var estItem = new Win_ord_Order_Estimate_U_CodeView
                        {

                            EstID = dr["EstID"].ToString(),
                            salesCustomID = dr["salesCustomID"].ToString(),
                            salesCustom = dr["salesCustom"].ToString(),
                            managerCustomID = dr["managerCustomID"].ToString(),
                            managerCustom = dr["managerCustom"].ToString(),
                            zoneGbn = dr["zoneGbn"].ToString(),
                            zoneGbnID = dr["zoneGbnID"].ToString(),
                            FaciliTypeID = dr["FaciliTypeID"].ToString(),
                            FacliType = dr["FacliType"].ToString(),
                            EstReqDate = dr["EstReqDate"].ToString(),
                            EstDate = DateTypeHyphen(dr["EstDate"].ToString()),
                            InstallSchFromDate = DateTypeHyphen(dr["InstallSchFromDate"].ToString()),
                            InstallSchTODate = DateTypeHyphen(dr["InstallSchTODate"].ToString()),
                            InstalLocation = dr["InstalLocation"].ToString(),
                            smallInstalLocation = dr["smallInstalLocation"].ToString(),
                            //InstallLocationPart = dr["InstallLocationPart"].ToString(),
                            InstallLocationConditionID = dr["InstallLocationConditionID"].ToString(),
                            EstSubject = dr["EstSubject"].ToString(),
                            EstDamdangName = dr["EstDamdangName"].ToString(),
                            EstDamdangTelno = dr["EstDamdangTelno"].ToString(),
                            EstApprovalYN = dr["EstApprovalYN"].ToString(),
                            EstApprovalDate = DateTypeHyphen(dr["EstApprovalDate"].ToString()),
                            EstItemList = dr["EstItemList"].ToString(),
                            electrDeliveryMethodID = dr["electrDeliveryMethodID"].ToString(),
                            electrDeliveryMethod = dr["electrDeliveryMethod"].ToString(),
                            deliveryCost = stringFormatN0(dr["deliveryCost"]),
                            totalAmount = stringFormatN0(dr["totalAmount"]),
                            Comments = dr["Comments"].ToString(),
                            orderTypeID = dr["orderTypeID"].ToString(),
                            orderType = dr["orderType"].ToString(),

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

                            sketch6File = dr["sketch6File"].ToString(),
                            sketch6FileAlias = dr["sketch6FileAlias"].ToString(),
                            sketch6Path = dr["sketch6Path"].ToString(),

                        };

                        //데이터 세팅
                        cboOrderType.SelectedValue = estItem.orderTypeID; //사업구분

                        txtManagerCustomID.Text = estItem.managerCustom;
                        txtManagerCustomID.Tag = estItem.managerCustomID;
                        txtSalesCustomID.Text = estItem.salesCustom;
                        txtSalesCustomID.Tag = estItem.managerCustomID;

                        txtInstallLocation.Text = estItem.InstalLocation; //현장명
                        txtInstallLocationAddComments.Text = estItem.Comments; //신청국소정보 비고
                        txtInstallLocationPhone.Text = estItem.EstDamdangTelno; //담당자 전화번호 -> 현장 전화번호

                        txtElectrDeliveryMethod.Text = estItem.electrDeliveryMethod;    //시공사&지자체 전기수전방법
                        txtKepElectrDeliveryMethod.Text = estItem.electrDeliveryMethod; //한전&전기공사 전기수전방법

                        txtBeforeSearchConsultFileName.Text = estItem.sketch1File;      //사전컨설팅결과서 첨부파일
                        txtDrawFileName.Text = estItem.sketch2File;                     //도면 첨부파일
                        txtSearchChecksheetFileName.Text = estItem.sketch3File;         //실사점검표 첨부파일
                        txtPictureEarthFileName.Text = estItem.sketch4File;             //사전대지파일 첨부파일
                        txtSearchFileName.Text = estItem.sketch5File;                   //실사내역서 첨부파일
                        txtInstallLocationSheetFileName.Text = estItem.sketch6File;     //거점투자기안 첨부파일

                        tab2_clicked = true;        //tab2저장하세요
                        tab3_clicked = true;        //tab3저장하세요

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

        private void callEstAccData(string estID)
        {
            if (dgdAcc.Items.Count > 0) ovcOrder_Acc.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("EstID", estID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrderSub_dgdAcc_Estimate", sqlParameter, true, "R");

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
                            var accList = new Win_order_Order_U_CodView_dgdAcc
                            {
                                num = i,
                                orderType = ovcOrderTypeAcc[0].code_name,
                                orderTypeID = ovcOrderTypeAcc[0].code_id,
                                ovcOrderTypeAcc = ovcOrderTypeAcc,
                                articleID = dr["articleID"].ToString(),
                                article = dr["article"].ToString(),
                                chargeInwareUnitPrice = dr["chargeInwareUnitPrice"].ToString(),

                            };
                            ovcOrder_Acc.Add(accList);
                        }

                        //dgdAccTotal.Items.Clear();
                        //var accTotal = new Win_order_Order_U_CodView_dgdAcc_Total
                        //{
                        //    num = i,
                        //    totalSum = txtMtrAmount.Text,
                        //};

                        //dgdAccTotal.Items.Add(accTotal);
                    }

                    dgdAcc.ItemsSource = ovcOrder_Acc;
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

        //전화번호 가져오기
        private string callCustomData(string customID)
        {

            string PhoneNo = string.Empty;

            if (customID == null || customID.Trim() == string.Empty)
            {
                try
                {                   
                    string[] sqlList = { "select phone1, phone2, phone, damdangPhone1, damdangPhone2 from mt_custom where customID = " };

                    for (int i = 0; i < sqlList.Length; i++)
                    {
                        DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + customID);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            DataTable dt = ds.Tables[0];
                            if (dt.Rows.Count > 0)
                            {
                                // 각 컬럼을 순회하면서 첫 번째로 값이 있는(null이나 빈 문자열이 아닌) 컬럼의 값을 반환
                                foreach (DataColumn column in dt.Columns)
                                {
                                    string value = dt.Rows[0][column.ColumnName].ToString();
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        PhoneNo = value;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }                    
                }
                catch
                {
                    return string.Empty;
                }
            }

            return PhoneNo;
        }

        private string GetDeliCost(string estID)
        {
            string deliCost = string.Empty;

            string[] sqlList = { "select deliveryCost from EST_Estimate where EstID = "

            };     
 

            for (int i = 0; i < sqlList.Length; i++)
            {
                DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + estID);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        deliCost = dt.Rows[0][0].ToString();
                        break;
                    }
                }
                else
                {
                    continue;
                }
            }     

            return deliCost;
        }

        

        //텍스트박스 , DatePicker, 콤보박스의 바인딩 값과 넘겨주는 오브젝트 value가 일치하는 곳에
        //자동으로 바인딩
        //사용하려하면 바인딩하려는 UI개체에 updateSourceTrigger를 propertyChange, Tag값도 변경하려면 mode=TwoWay를 작성하세요
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
                    if (properties.TryGetValue(tagPropertyName, out var tagProperty) )
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

        // 단일 컨트롤을 찾는 메서드도 필요할 수 있습니다
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
        private void fillGridTab2(string orderId)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder_tab2", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                   
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                      
                            var tab2Data = new Win_ord_Order_U_CodeView_Tab2
                            {
                                searchReqDate = dr["searchReqDate"].ToString(),
                                searchDate = dr["searchDate"].ToString(),
                                searchDataAcptDate = dr["searchDataAcptDate"].ToString(),
                                installLocationCount = stringFormatN0(dr["installLocationCount"]),
                                electrDeliveryMethod = dr["electrDeliveryMethod"].ToString(),
                                inspectionNeedYN = dr["inspectionNeedYN"].ToString(),
                                addConstructCostSearch = stringFormatN0(dr["addConstructCostSearch"]),
                                addConstructCost = stringFormatN0(dr["addConstructCost"]),
                                searchComments = dr["searchComments"].ToString(),
                                corpAcptNo = dr["corpAcptNo"].ToString(),
                                corpApprovalDate = dr["corpApprovalDate"].ToString(),
                                corpEndDate = dr["corpEndDate"].ToString(),
                                corpLastEndDate = dr["corpLastEndDate"].ToString(),
                                corpComments = dr["corpComments"].ToString(),
                                //localGovPermissionNo = dr["localGovPermissionNo"].ToString(),
                                //localGovBehaviorReportDate = dr["localGovBehaviorReportDate"].ToString(),
                                localGoComments = dr["localGoComments"].ToString(),
                                //superBeforeUseInspDate = DateTypeHyphen(dr["superBeforeUseInspDate"].ToString()),
                                superBeforeUseInspPrintDate = DateTypeHyphen(dr["superBeforeUseInspPrintDate"].ToString()),
                                superUseInspReqDate   = DateTypeHyphen(dr["superUseInspReqDate"].ToString()),

                                beforeSearchConsultFilePath = dr["beforeSearchConsultFilePath"].ToString(),
                                beforeSearchConsultFileName = dr["beforeSearchConsultFileName"].ToString(),
                                pictureEarthFilePath = dr["pictureEarthFilePath"].ToString(),
                                pictureEarthFileName = dr["pictureEarthFileName"].ToString(),
                                drawFilePath = dr["drawFilePath"].ToString(),
                                drawFileName = dr["drawFileName"].ToString(),
                                searchFilePath = dr["searchFilePath"].ToString(),
                                searchFileName = dr["searchFileName"].ToString(),
                                searchChecksheetFilePath = dr["searchChecksheetFilePath"].ToString(),
                                searchChecksheetFileName = dr["searchChecksheetFileName"].ToString(),
                                installLocationSheetFilePath = dr["installLocationSheetFilePath"].ToString(),
                                installLocationSheetFileName = dr["installLocationSheetFileName"].ToString(),
                                localGoTaxFilePath = dr["localGoTaxFilePath"].ToString(),
                                localGoTaxFileName = dr["localGoTaxFileName"].ToString(),
                                LocalGovProveFilePath = dr["LocalGovProveFilePath"].ToString(),
                                LocalGovProveFileName = dr["LocalGovProveFileName"].ToString(),

                            };

                            #region 자동화할 순 없을까?
                            //dtpSearchReqDate.SelectedDate = ConvertToDateTime(tab2Data.searchReqDate);
                            //dtpSearchDate.SelectedDate = ConvertToDateTime(tab2Data.searchDate);
                            //dtpSearchDataAcptDate.SelectedDate = ConvertToDateTime(tab2Data.searchDataAcptDate);
                            //txtInstallLocationCount.Text = stringFormatN0(tab2Data.installLocationCount);                            
                            //cboElecDeliMeth.SelectedValue = tab2Data.electrDeliveryMethodID;
                            //cboInspectionNeedYN.SelectedValue = tab2Data.inspectionNeedYN;
                            //txtAddConstructCost.Text = stringFormatN0(tab2Data.addConstructCost);
                            //txtAddConstructCostSearch.Text = stringFormatN0(tab2Data.addConstructCostSearch);
                            //txtSearchComments.Text = tab2Data.searchComments;
                            //txtCorpAcptNo.Text = tab2Data.corpAcptNo;
                            //dtpCorpApprovalDate.SelectedDate = ConvertToDateTime(tab2Data.corpApprovalDate);
                            //dtpCorpEndDate.SelectedDate = ConvertToDateTime(tab2Data.corpEndDate);
                            //dtpCorpLastEndDate.SelectedDate = ConvertToDateTime(tab2Data.corpLastEndDate);
                            //txtCorpComments.Text = tab2Data.corpComments;
                            //txtLocalGovPermissionNo.Text = tab2Data.localGovPermissionNo;
                            //dtpLocalGovBehaviorReportDate.SelectedDate = ConvertToDateTime(tab2Data.localGovBehaviorReportDate);
                            //txtLocalGoComments.Text = tab2Data.localGoComments;
                            #endregion

                            //클래스객체와 이 값을 바인딩할 그리드를 넣으세요
                            AutoBindDataToControls(tab2Data, grd2);
                        }

                    }

        
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생(fillGridTab2), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void fillGridTab3(string orderId)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder_tab3", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {

                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {

                            var tab3Data = new Win_ord_Order_U_CodeView_Tab3
                            {
                                    kepElectrDeliveryMethod = dr["kepElectrDeliveryMethod"].ToString(),
                                    kepInstallLocationCount = stringFormatN0(dr["kepInstallLocationCount"]),
                                    kepOutLineConstructContext = dr["kepOutLineConstructContext"].ToString(),
                                    kepInfraPayAmount = stringFormatN0(dr["kepInfraPayAmount"]),
                                    //kepManageInfraPayAmount = stringFormatN0(dr["kepManageInfraPayAmount"]),
                                    kepManageInfraPayDate = dr["kepManageInfraPayDate"].ToString(),
                                    kepElectrReqDate = dr["kepElectrReqDate"].ToString(),
                                    //kepInApprovalYN = dr["kepInApprovalYN"].ToString(),
                                    //kepInApprovalDate = dr["kepInApprovalDate"].ToString(),
                                    kepParentChildCapacity= dr["kepParentChildCapacity"].ToString(),
                                    kepPowerSupplyCapacity= stringFormatN0(dr["kepPowerSupplyCapacity"].ToString()),
                                    kepMeterInstallContext = dr["kepMeterInstallContext"].ToString(),
                                    kepDamdangjaPhone = dr["kepDamdangjaPhone"].ToString(),
                                    kepCustomNo = dr["kepCustomNo"].ToString(),
                                    kepPaymentDate = dr["kepPaymentDate"].ToString(),
                                    kepMeterInstallDate = dr["kepMeterInstallDate"].ToString(),
                                    kepFaucetComments = dr["kepFaucetComments"].ToString(),
                                    constrCustomID = dr["constrCustomID"].ToString(),
                                    constrCustom = dr["constrCustom"].ToString(),
                                    constrOrderDate = dr["constrOrderDate"].ToString(),
                                    constrDate = dr["constrDate"].ToString(),
                                    constrDelyReason = dr["constrDelyReason"].ToString(),
                                    constrCompleteDate = dr["constrCompleteDate"].ToString(),
                                    constrComments = dr["constrComments"].ToString(),
                                    electrSafeCheckPrintDate = dr["electrSafeCheckPrintDate"].ToString(), //20250213
                                    electrSafeCheckDate = dr["electrSafeCheckDate"].ToString(),
                                    electrSafeCheckSuppleContext = dr["electrSafeCheckSuppleContext"].ToString(),
                                    electrSafeCheckLocation = dr["electrSafeCheckLocation"].ToString(),
                                    electrSafeCheckCost = stringFormatN0(dr["electrSafeCheckCost"]),
                                    electrSafeCheckCostPayDate = dr["electrSafeCheckCostPayDate"].ToString(),
                                    electrBeforeUseCheckReqDate = dr["electrBeforeUseCheckReqDate"].ToString(),
                                    electrBeforeUseCheckPrintDate = dr["electrBeforeUseCheckPrintDate"].ToString(),
                                    electrBeforeUseCheckSuppleContext = dr["electrBeforeUseCheckSuppleContext"].ToString(),
                                    electrBeforeInspLocation = dr["electrBeforeInspLocation"].ToString(),
                                    electrBeforeInspReqDate = dr["electrBeforeInspReqDate"].ToString(),
                                    electrBeforeInspPrintDate = dr["electrBeforeInspPrintDate"].ToString(),
                                    electrBeforeInspCost = stringFormatN0(dr["electrBeforeInspCost"]),
                                    electrBeforeInspCostPayDate = dr["electrBeforeInspCostPayDate"].ToString(),
                                    electrBeforeInspSuppleContext = dr["electrBeforeInspSuppleContext"].ToString(),
                                    electrSafeCheckComments = dr["electrSafeCheckComments"].ToString(),

                                    kepElectrLineFilePath = dr["kepElectrLineFilePath"].ToString(),
                                    kepElectrLineFileName = dr["kepElectrLineFileName"].ToString(),
                                    kepFaucetAcptFilePath = dr["kepFaucetAcptFilePath"].ToString(),
                                    kepFaucetAcptFileName = dr["kepFaucetAcptFileName"].ToString(),
                                    electrSafeInspPrintFilePath = dr["electrSafeInspPrintFilePath"].ToString(),
                                    electrSafeInspPrintFileName = dr["electrSafeInspPrintFileName"].ToString(),
                                    electrBeforeUseCheckPrintFilePath = dr["electrBeforeUseCheckPrintFilePath"].ToString(),
                                    electrBeforeUseCheckPrintFileName = dr["electrBeforeUseCheckPrintFileName"].ToString(),
                                    electrBeforeUseInspFilePath = dr["electrBeforeUseInspFilePath"].ToString(),
                                    electrBeforeUseInspFileName = dr["electrBeforeUseInspFileName"].ToString(),
                                    electrKepAcptFilePath = dr["electrKepAcptFilePath"].ToString(),
                                    electrKepAcptFileName = dr["electrKepAcptFileName"].ToString(),
                                    electrKepInfraPayBillFilePath = dr["electrKepInfraPayBillFilePath"].ToString(),
                                    electrKepInfraPayBillFileName = dr["electrKepInfraPayBillFileName"].ToString(),
                                    electrUseContractFilePath = dr["electrUseContractFilePath"].ToString(),
                                    electrUseContractFileName = dr["electrUseContractFileName"].ToString(),
                                    electrBeforeUseInspCostFilePath = dr["electrBeforeUseInspCostFilePath"].ToString(),
                                    electrBeforeUseInspCostFileName = dr["electrBeforeUseInspCostFileName"].ToString(),
                                    electrCoWorkFilePath = dr["electrCoWorkFilePath"].ToString(),
                                    electrCoWorkFileName = dr["electrCoWorkFileName"].ToString(),
                                    electrCostFilePath = dr["electrCostFilePath"].ToString(),
                                    electrCostFileName  = dr["electrCostFileName"].ToString(),
                                    electrTransCoUseFilePath = dr["electrTransCoUseFilePath"].ToString(),
                                    electrTransCoUseFileName = dr["electrTransCoUseFileName"].ToString(),

                            };

                            AutoBindDataToControls(tab3Data, grd3);                            

                        }

                    }


                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생(fillGridTab3), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void fillGridTab4(string orderId)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder_tab4", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {

                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {

                            var tab4Data = new Win_ord_Order_U_CodeView_Tab4
                            {

                                superCustomID = dr["superCustomID"].ToString(),
                                superCustom = dr["superCustom"].ToString(),
                                //superCostPayCustomID = dr["superCostPayCustomID"].ToString(),
                                //superCostPayCustom = dr["superCostPayCustom"].ToString(),
                                superCustomPhoneNo = dr["superCustomPhoneNo"].ToString(),
                                safeManageCustomID = dr["safeManageCustomID"].ToString(),
                                safeManageCustom = dr["safeManageCustom"].ToString(),
                                safeManageCustomPhoneNo = dr["safeManageCustomPhoneNo"].ToString(),
                                superSetCost =  stringFormatN0(dr["superSetCost"]),
                                superSetTaxPrintDate = dr["superSetTaxPrintDate"].ToString(),
                                superUseInspPayCustomID = dr["superUseInspPayCustomID"].ToString(),
                                superUseInspPayCustom = dr["superUseInspPayCustom"].ToString(),
                                //superUseInspReqDate = dr["superUseInspReqDate"].ToString(),
                                //superBeforeUseInspDate = dr["superBeforeUseInspDate"].ToString(),
                                //superBeforeUseInspPrintDate = dr["superBeforeUseInspPrintDate"].ToString(),
                                superComments = dr["superComments"].ToString(),
                                compReplyDate = dr["compReplyDate"].ToString(),
                                suppleContext = dr["suppleContext"].ToString(),
                                suppleCompDate = dr["suppleCompDate"].ToString(),
                                compSuppleReportContext = dr["compSuppleReportContext"].ToString(),
                                compSuppleReportDate = dr["compSuppleReportDate"].ToString(),
                                insurePrintDate = dr["insurePrintDate"].ToString(),
                                compReportCompDate = dr["compReportCompDate"].ToString(),
                                compReportComments = dr["compReportComments"].ToString(),
                                accntComments = dr["accntComments"].ToString(),

                                superSetCheckFilePath = dr["superSetCheckFilePath"].ToString(),
                                superSetCheckFileName = dr["superSetCheckFileName"].ToString(),
                                superBeforeUseInspectFilePath = dr["superBeforeUseInspectFilePath"].ToString(),
                                superBeforeUseInspectFileName = dr["superBeforeUseInspectFileName"].ToString(),
                                compReportFileName = dr["compReportFileName"].ToString(),
                                compReportFilePath = dr["compReportFilePath"].ToString(),
                                //superCostFilePath = dr["superCostFilePath"].ToString(),
                                //superCostFileName = dr["superCostFileName"].ToString(),
                                safeManagerCertiFileName  = dr["safeManagerCertiFileName"].ToString(),
                                safeManagerCertiFilePath  = dr["safeManagerCertiFilePath"].ToString(),
                                superReportFilePath = dr["superReportFilePath"].ToString(),
                                superReportFileName = dr["superReportFileName"].ToString(),
                                insurePrintFilePath = dr["insurePrintFilePath"].ToString(),
                                insurePrintFileName  = dr["insurePrintFileName"].ToString(),
                            };

                            AutoBindDataToControls(tab4Data, grd4);

                        }

                    }


                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생(fillGridTab4), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void fillGridTab4_Accnt(string orderId)
        {
            if(dgdAccnt.Items.Count > 0) dgdAccnt.Items.Clear();

            if (strFlag == "I" && chkEoAddSrh.IsChecked != true && string.IsNullOrEmpty(txtOrderID.Text))
            {
                rowAddAccnt();
                return;
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrderSub_dgdAccnt", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        int rowCount = 0;  
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {

                            var amount = stringFormatN0(dr["column2Amount"]);
                            decimal decimalAmount = 0;
                            decimal.TryParse(amount.Replace(",", ""), out decimalAmount);

                            var accntList = new Win_order_Order_U_CodView_dgdAccnt
                            {
                                column1Date = DateTypeHyphen(dr["column1Date"].ToString()),
                                column2Amount = stringFormatN0(dr["column2Amount"]),
                                column3Comment = dr["column3Comment"].ToString(),                             

                                // 스타일 관련 속성 추가
                                isBold = (rowCount % 4 == 3),  // 4번째 행
                                isNegative = (rowCount % 4 == 2 && decimalAmount < 0)
                            };                          

                            if(chkEoAddSrh.IsChecked != true && isBringLastOrder != true)
                            {
                                accntList.column4FilePath = dr["column4FilePath"].ToString();
                                accntList.column5FileName = dr["column5FileName"].ToString();
                            }
                            else
                            {
                                accntList.column4FilePath = string.Empty;
                                accntList.column5FileName = string.Empty;
                            }

                            dgdAccnt.Items.Add(accntList);
                            rowCount++;
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생(fillGridTab4_Accnt), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void fillgridTab5(string orderId)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder_tab5", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {

                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {

                            var tab5Data = new Win_ord_Order_U_CodeView_Tab5
                            {
                                sketch1FilePath  = dr["sketch1FilePath"].ToString(),
                                sketch1FileName  = dr["sketch1FileName"].ToString(),
                                sketch1FileAlias = dr["sketch1FileAlias"].ToString(),
                                sketch2FilePath  = dr["sketch2FilePath"].ToString(),
                                sketch2FileName  = dr["sketch2FileName"].ToString(),
                                sketch2FileAlias = dr["sketch2FileAlias"].ToString(),
                                sketch3FilePath  = dr["sketch3FilePath"].ToString(),
                                sketch3FileName  = dr["sketch3FileName"].ToString(),
                                sketch3FileAlias = dr["sketch3FileAlias"].ToString(),
                                sketch4FilePath  = dr["sketch4FilePath"].ToString(),
                                sketch4FileName  = dr["sketch4FileName"].ToString(),
                                sketch4FileAlias = dr["sketch4FileAlias"].ToString(),
                                sketch5FilePath  = dr["sketch5FilePath"].ToString(),
                                sketch5FileName  = dr["sketch5FileName"].ToString(),
                                sketch5FileAlias = dr["sketch5FileAlias"].ToString(),
                                sketch6FilePath  = dr["sketch6FilePath"].ToString(),
                                sketch6FileName  = dr["sketch6FileName"].ToString(),
                                sketch6FileAlias = dr["sketch6FileAlias"].ToString(),
                                sketch7FilePath  = dr["sketch7FilePath"].ToString(),
                                sketch7FileName  = dr["sketch7FileName"].ToString(),
                                sketch7FileAlias = dr["sketch7FileAlias"].ToString(),
                                sketch8FilePath  = dr["sketch8FilePath"].ToString(),
                                sketch8FileName  = dr["sketch8FileName"].ToString(),
                                sketch8FileAlias = dr["sketch8FileAlias"].ToString(),
                                sketch9FilePath  = dr["sketch9FilePath"].ToString(),
                                sketch9FileName  = dr["sketch9FileName"].ToString(),
                                sketch9FileAlias = dr["sketch9FileAlias"].ToString(),
                                sketch10FilePath = dr["sketch10FilePath"].ToString(),
                                sketch10FileName = dr["sketch10FileName"].ToString(),
                                sketch10FileAlias= dr["sketch10FileAlias"].ToString(),
                                sketch11FilePath = dr["sketch11FilePath"].ToString(),
                                sketch11FileName = dr["sketch11FileName"].ToString(),
                                sketch11FileAlias= dr["sketch11FileAlias"].ToString(),
                                sketch12FilePath = dr["sketch12FilePath"].ToString(),
                                sketch12FileName = dr["sketch12FileName"].ToString(),
                                sketch12FileAlias = dr["sketch12FileAlias"].ToString(),

                                sketch13FileName   = dr["sketch13FileName"].ToString(),
                                sketch13FilePath   = dr["sketch13FilePath"].ToString(),
                                sketch13FileAlias  = dr["sketch13FileAlias"].ToString(),
                                sketch14FileName   = dr["sketch14FileName"].ToString(),
                                sketch14FilePath   = dr["sketch14FilePath"].ToString(),
                                sketch14FileAlias  = dr["sketch14FileAlias"].ToString(),
                                sketch15FileName   = dr["sketch15FileName"].ToString(),
                                sketch15FilePath   = dr["sketch15FilePath"].ToString(),
                                sketch15FileAlias  = dr["sketch15FileAlias"].ToString(),
                                sketch16FileName   = dr["sketch16FileName"].ToString(),
                                sketch16FilePath   = dr["sketch16FilePath"].ToString(),
                                sketch16FileAlias  = dr["sketch16FileAlias"].ToString(),
                                sketch17FileName   = dr["sketch17FileName"].ToString(),
                                sketch17FilePath   = dr["sketch17FilePath"].ToString(),
                                sketch17FileAlias  = dr["sketch17FileAlias"].ToString(),
                                sketch18FileName   = dr["sketch18FileName"].ToString(),
                                sketch18FilePath   = dr["sketch18FilePath"].ToString(),
                                sketch18FileAlias  = dr["sketch18FileAlias"].ToString(),
                                sketch19FileName   = dr["sketch19FileName"].ToString(),
                                sketch19FilePath   = dr["sketch19FilePath"].ToString(),
                                sketch19FileAlias  = dr["sketch19FileAlias"].ToString(),
                                sketch20FileName   = dr["sketch20FileName"].ToString(),
                                sketch20FilePath   = dr["sketch20FilePath"].ToString(),
                                sketch20FileAlias  = dr["sketch20FileAlias"].ToString(),
                                sketch21FileName   = dr["sketch21FileName"].ToString(),
                                sketch21FilePath   = dr["sketch21FilePath"].ToString(),
                                sketch21FileAlias  = dr["sketch21FileAlias"].ToString(),
                                sketch22FileName   = dr["sketch22FileName"].ToString(),
                                sketch22FilePath   = dr["sketch22FilePath"].ToString(),
                                sketch22FileAlias  = dr["sketch22FileAlias"].ToString(),

                            };

                            AutoBindDataToControls(tab5Data, grd5);

                        }


                
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

        
        private void SetControlBindings(DependencyObject parent, Type modelType, object dataContext)
        {
            FindUiObject(parent, obj =>
            {
                if (obj is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
                {
                    string controlName = element.Name;
                    string[] prefixes = new[] { "txt", "cbo", "dtp", "btn", "lbl", "dgd", "rad" };

                    foreach (var prefix in prefixes)
                    {
                        if (controlName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            controlName = controlName.Substring(prefix.Length).ToLower();
                            break;
                        }
                    }

                    var property = modelType.GetProperties()
                                          .FirstOrDefault(p => p.Name.ToLower() == controlName);

                    if (property != null)
                    {
                        var value = property.GetValue(dataContext);

                        if (obj is ComboBox combo)
                        {
                            combo.SelectedValue = value;
                        }
                        else if (obj is DatePicker dtp && value != null)
                        {
                            string dateStr = DateTypeHyphen(value.ToString());
                            if (!string.IsNullOrEmpty(dateStr) && dateStr.Length >= 10)  // YYYY-MM-DD 형식이 있는지 확인
                            {
                                dtp.Text = dateStr.Substring(0, 10);  // YYYY-MM-DD 부분만 추출
                            }
                        }
                        else
                        {
                            element.DataContext = dataContext;
                        }
                    }
                }
            });
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



        //null오류 방지를 위해서 우선 value파라미터는 object type으로 받습니다.
        //기본 사용방법 ☞ RemoveComma(value) 콤마를 제거하여 string으로 내보냅니다
        //Remove(value, true) int타입으로 콤마를 제거하여 int로 내보냅니다.(타입 지정하지 않으면 기본 int)
        //Remove(value, true, typeof(decimal)) decimal타입으로 콤마를 제거하여 decimal로 내보냅니다(큰 숫자 필요시)
        //다른 숫자 자료형에 대입하려면 형변환을 해주세요
        //int intVal = (int)RemoveComma(value, true)
        //private object RemoveComma(object obj, bool returnNumeric = false, Type returnType = null)
        //{
        //    if (returnType == null) returnType = typeof(int);
        //    if (obj == null || string.IsNullOrEmpty(obj.ToString()))
        //    {
        //        if (returnNumeric)
        //        {
        //            if (returnType == typeof(int)) return 0;
        //            if (returnType == typeof(decimal)) return 0m;
        //            if (returnType == typeof(long)) return 0L;
        //            if (returnType == typeof(double)) return 0d;
        //            return 0;
        //        }
        //        return "0";
        //    }
        //    string digits = obj.ToString().Replace(",", "");
        //    // 추출된 숫자가 없는 경우
        //    if (string.IsNullOrEmpty(digits))
        //    {
        //        if (returnNumeric)
        //        {
        //            if (returnType == typeof(int)) return 0;
        //            if (returnType == typeof(decimal)) return 0m;
        //            if (returnType == typeof(long)) return 0L;
        //            if (returnType == typeof(double)) return 0d;
        //            return 0;
        //        }
        //        return "0";
        //    }
        //    if (returnNumeric)
        //    {
        //        if (returnType == typeof(int))
        //        {
        //            return int.TryParse(digits, out int intResult) ? intResult : 0;
        //        }
        //        else if (returnType == typeof(decimal))
        //        {
        //            return decimal.TryParse(digits, out decimal decimalResult) ? decimalResult : 0m;
        //        }
        //        else if (returnType == typeof(long))
        //        {
        //            return long.TryParse(digits, out long longResult) ? longResult : 0L;
        //        }
        //        else if (returnType == typeof(double))
        //        {
        //            return double.TryParse(digits, out double doubleResult) ? doubleResult : 0d;
        //        }
        //    }
        //    return digits;
        //}

        private object RemoveComma(object obj, bool returnAsNumber = false, Type returnType = null)
        {
            if (obj == null)
            {
                if (!returnAsNumber) return "0";

                // null일 때도 returnType에 따라 적절한 타입의 0 반환
                switch (returnType?.Name)
                {
                    case "Decimal": return (object)0m;
                    case "Double": return (object)0d;
                    case "Int64": return (object)0L;
                    default: return (object)0;
                }
            }

            string digits = obj.ToString()
                              .Trim()
                              .Replace(",", "");

            if (string.IsNullOrEmpty(digits))
            {
                if (!returnAsNumber) return "0";

                // returnType을 활용해서 적절한 타입으로 반환
                switch (returnType?.Name)
                {
                    case "Decimal": return (object)0m;
                    case "Double": return (object)0d;
                    case "Int64": return (object)0L;
                    default: return (object)0;
                }
            }


            try
            {
                Type targetType = returnType ?? typeof(int);

                switch (targetType.Name)
                {
                    case "Int32":
                        if (decimal.TryParse(digits, out decimal intParsed))
                        {
                            if (intParsed > int.MaxValue) return int.MaxValue;
                            if (intParsed < int.MinValue) return int.MinValue;
                            return (int)intParsed;
                        }
                        return int.MaxValue;

                    case "Int64":
                        if (decimal.TryParse(digits, out decimal longParsed))
                        {
                            if (longParsed > long.MaxValue) return long.MaxValue;
                            if (longParsed < long.MinValue) return long.MinValue;
                            return (long)longParsed;
                        }
                        return long.MaxValue;

                    case "Double":
                        if (double.TryParse(digits, out double doubleParsed))
                        {
                            return doubleParsed;
                        }
                        return double.MaxValue;

                    case "Decimal":
                        if (decimal.TryParse(digits, out decimal decimalParsed))
                        {
                            return decimalParsed;
                        }
                        return decimal.MaxValue;

                    default:
                        return int.MaxValue;
                }
            }
            catch
            {
                if (returnType != null)
                {
                    switch (returnType.Name)
                    {
                        case "Int32":
                            return int.MaxValue;
                        case "Int64":
                            return long.MaxValue;
                        case "Double":
                            return double.MaxValue;
                        case "Decimal":
                            return decimal.MaxValue;
                        default:
                            return int.MaxValue;
                    }
                }
                return int.MaxValue;
            }
        }

        private object RemoveHyphen(object obj)
        {
            if (obj == null)
                return string.Empty;

            if (obj.ToString() == string.Empty) 
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
                return datePicker.SelectedDate.Value.ToString("yyyyMMdd");
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


        private string TimeTypeColon(string DigitsTime)
        {
            string pattern1 = @"(\d{2})(\d{2})";

            if (DigitsTime.Length == 4)
            {
                DigitsTime = Regex.Replace(DigitsTime, pattern1, "$1:$2");
            }

            return DigitsTime;
        }

        private void CloseToolTip()
        {
            if (currentToolTip != null && currentToolTip.IsOpen)
            {
                currentToolTip.IsOpen = false;
                if (currentTimer != null)
                {
                    currentTimer.Stop();
                    currentTimer = null;
                }
            }
        }

        private void ShowTooltipMessage(FrameworkElement element, string message, MessageBoxImage iconType = MessageBoxImage.None, PlacementMode placement = PlacementMode.Bottom)
        {
            // 이미 열려있는 툴팁이 있다면 닫기
            if (currentToolTip != null && currentToolTip.IsOpen)
            {
                currentToolTip.IsOpen = false;
                if (currentTimer != null)
                {
                    currentTimer.Stop();
                    currentTimer = null;
                }
            }

            object tooltipContent;

            // 아이콘이 필요 없는 경우
            if (iconType == MessageBoxImage.None)
            {
                tooltipContent = message;
            }
            else
            {
                // StackPanel 생성
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // 시스템 아이콘 설정
                System.Drawing.Icon systemIcon;
                switch (iconType)
                {
                    case MessageBoxImage.Information:
                        systemIcon = System.Drawing.SystemIcons.Information;
                        break;
                    case MessageBoxImage.Warning:
                        systemIcon = System.Drawing.SystemIcons.Warning;
                        break;
                    case MessageBoxImage.Error:
                        systemIcon = System.Drawing.SystemIcons.Error;
                        break;
                    case MessageBoxImage.Question:
                        systemIcon = System.Drawing.SystemIcons.Question;
                        break;
                    default:
                        systemIcon = null;
                        break;
                }

                if (systemIcon != null)
                {
                    // System.Drawing에서 아이콘 가져오기
                    System.Windows.Media.Imaging.BitmapSource iconSource =
                        System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            systemIcon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    // 이미지 생성
                    var image = new Image
                    {
                        Source = iconSource,
                        Width = 16,
                        Height = 16,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    // StackPanel에 추가
                    stackPanel.Children.Add(image);
                }

                // 텍스트블록 생성
                var textBlock = new TextBlock
                {
                    Text = message,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(textBlock);
                tooltipContent = stackPanel;
            }

            // 새 툴팁 생성
            var tooltip = new ToolTip
            {
                Content = tooltipContent,
                PlacementTarget = element,
                Placement = placement,
                IsOpen = true
            };

            // 위치에 따른 설정
            if (placement == PlacementMode.Bottom)
            {
                tooltip.VerticalOffset = 5;
            }
            else if (placement == PlacementMode.Right)
            {
                tooltip.Placement = PlacementMode.Bottom;
                tooltip.VerticalOffset = 5;
                element.Dispatcher.BeginInvoke(new Action(() =>
                {
                    double offset = element.ActualWidth - tooltip.ActualWidth;
                    tooltip.HorizontalOffset = offset;
                }));
            }

            currentToolTip = tooltip;

            // 3초 후 툴팁 자동 닫기
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, eventArgs) =>
            {
                tooltip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
            currentTimer = timer;
        }

        //오른쪽 클릭으로 복사 붙여넣기 방지
        private void TextBox_PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        //셀에 복사 붙여넣기 방지
        private void TextBox_PreventCopyPaste(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // IME 비활성화
                InputMethod.SetIsInputMethodEnabled(textBox, false);
            }

            //컨트롤키와 V키가 입력되었는지 확인
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true;
            }
        }
        //셀에 숫자만 입력
        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e)
        {
            // 입력된 텍스트가 숫자인지 확인 (간단한 방법)
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    ShowTooltipMessage(sender as FrameworkElement, "숫자만 입력 가능합니다.", MessageBoxImage.Error, PlacementMode.Bottom);
                    break;
                }
            }
                       
        }

        private void CheckElectrTextboxValue(TextBox currentTextbox, int currentValue)
        {
            string currentName = currentTextbox.Name ?? "";

            // 이웃 TextBox 찾기
            var siblingTextBox = lib.FindSiblingControl<TextBox>(currentTextbox);
            if (siblingTextBox == null || string.IsNullOrEmpty(siblingTextBox.Text))
                return;

            if (!int.TryParse(siblingTextBox.Text, out int siblingValue))
                return;

            // End 텍스트박스 (끝값) - 시작값보다 이하로 입력 불가
            if (currentName.Contains("End"))
            {
                if (currentValue < siblingValue)
                {
                    ShowTooltipMessage(currentTextbox,
                        $"기준점 시작 {siblingValue}보다 아래로 입력할 수 없습니다.",
                        MessageBoxImage.Warning, PlacementMode.Bottom);

                    // 잘못된 값을 시작값으로 자동 수정
                    currentTextbox.Text = siblingValue.ToString();
                    currentTextbox.SelectAll();
                }
            }
            // To 텍스트박스 (시작값) - 끝값보다 초과로 입력 불가
            else if (currentName.Contains("To"))
            {
                if (currentValue > siblingValue)
                {
                    ShowTooltipMessage(currentTextbox,
                        $"기준점 끝 {siblingValue}보다 위로 입력할 수 없습니다.",
                        MessageBoxImage.Warning, PlacementMode.Bottom);

                    // 잘못된 값을 끝값으로 자동 수정
                    currentTextbox.Text = siblingValue.ToString();
                    currentTextbox.SelectAll();
                }
            }
        }

        private void ElectrTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox currentTextbox = sender as TextBox;
            if (currentTextbox != null && !string.IsNullOrEmpty(currentTextbox.Text))
            {
                if (int.TryParse(currentTextbox.Text, out int currentValue))
                {
                    CheckElectrTextboxValue(currentTextbox, currentValue);
                }
            }
        }

        //셀에 숫자와 음수 기호 입력 허용
        private void TextBox_NumberValidationWithMinus(object sender, TextCompositionEventArgs e)
        {
            // 이미 입력된 텍스트 가져오기
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text;

            // 새로운 텍스트 (현재 입력 포함)
            string newText = currentText.Substring(0, textBox.SelectionStart) + e.Text + currentText.Substring(textBox.SelectionStart + textBox.SelectionLength);

            // '-'는 첫 번째 위치에만 허용
            if (e.Text == "-" && textBox.SelectionStart == 0 && !currentText.Contains("-"))
            {
                e.Handled = false;
                return;
            }

            // 나머지 문자는 숫자만 허용
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                ShowTooltipMessage(sender as FrameworkElement, "음수를 포함한 숫자형태만 입력 가능합니다.", MessageBoxImage.Error, PlacementMode.Right);
            }
        }

        private void TextBox_DatePicker(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            if (datePicker != null && datePicker.SelectedDate == null)
            {
                datePicker.SelectedDate = DateTime.Today;
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null && textBlock.Text != null)
            {
                DateTime date;
                if (DateTime.TryParse(textBlock.Text, out date))
                {
                    textBlock.Text = date.ToString("yyyy-MM-dd");
                }
            }
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
                sqlParameter.Add("orderID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Order_dOrder", sqlParameter, "D");

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
        /// 먼저 order테이블에 값을 저장/수정 할때 프로시저에서 계약내용 탭 내용들을 다 비워주고
        /// 새로 다시 넣도록 하였습니다.xp_Order_iOrder, xp_Order_uOrder
        /// 그 뒤에 계약내용 탭 데이터그리드에 있는 내용을 그리드별로 프로시저를 호출해서 저장합니다.
        private bool SaveData(string strFlag)
        {     

            PrimaryKey = string.Empty;
            bool flag = false;

            tabCheckTrue();

            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData() && CheckContractData())
                {
                    UpdateTbkMessage("기본 정보 저장 중...");

                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();                   

                    sqlParameter.Add("orderID", string.IsNullOrEmpty(txtOrderID.Text) ? "" : txtOrderID.Text);
                    sqlParameter.Add("orderNo", string.IsNullOrEmpty(txtOrderNo.Text) ? "" : txtOrderNo.Text);
                    sqlParameter.Add("estSubject", txtEstSubject.Text);
                    sqlParameter.Add("estID", txtEstID.Tag !=null ? txtEstID.Tag.ToString() : "");                    
                    sqlParameter.Add("managerCustomID", txtManagerCustomID.Tag != null ? txtManagerCustomID.Tag.ToString() : "");
                    sqlParameter.Add("saleCustomID", txtSalesCustomID.Tag != null ? txtSalesCustomID.Tag.ToString() : "");
                    sqlParameter.Add("searchCustomID", txtSearchCustomID.Tag != null ? txtSearchCustomID.Tag.ToString() : "");
                    sqlParameter.Add("acptDate", IsDatePickerNull(dtpAcptDate) ? "" : ConvertDate(dtpAcptDate));
                    sqlParameter.Add("manageCustomAcptDate", IsDatePickerNull(dtpManageCustomAcptDate) ? "" : ConvertDate(dtpManageCustomAcptDate));
                    sqlParameter.Add("manageCustomConfirmDate", IsDatePickerNull(dtpManageCustomConfirmDate) ? "" : ConvertDate(dtpManageCustomConfirmDate));
                    sqlParameter.Add("installLocation", txtInstallLocation.Text);
                    sqlParameter.Add("installLocationAddress", txtInstallLocationAddress.Text);
                    sqlParameter.Add("installLocationPhone", txtInstallLocationPhone.Text);
                    sqlParameter.Add("houseHoldCount", RemoveComma(txthouseHoldCount.Text,true));
                    //sqlParameter.Add("installLocationPart", txtInstallLocationPart.Text);
                    sqlParameter.Add("carParkingCount", RemoveComma(txtCarParkingCount.Text, true));
                    //sqlParameter.Add("alreadyManageCustomID", txtAlreadyManageCustomID.Tag != null ? txtAlreadyManageCustomID.Tag.ToString() : "");
                    sqlParameter.Add("alreadyManageCustom", txtAlreadyManageCustom.Text);
                    sqlParameter.Add("electrCarCount", RemoveComma(txtElectrCarCount.Text, true));
                    sqlParameter.Add("installLocationComments", txtInstallLocationComments.Text);
                    sqlParameter.Add("cpoCalcuDate",IsDatePickerNull(dtpCpoCalcuDate) ? "" :ConvertDate(dtpCpoCalcuDate));
                    sqlParameter.Add("constrCalcuDate", IsDatePickerNull(dtpConstrCalcuDate) ? "" : ConvertDate(dtpConstrCalcuDate));
                    sqlParameter.Add("alreadyChargeCount", txtAlReadyChargeCount.Text);
                    sqlParameter.Add("contractFromDate", IsDatePickerNull(dtpContractFromDate) ? "" : ConvertDate(dtpContractFromDate));
                    sqlParameter.Add("contractToDate", IsDatePickerNull(dtpContractToDate) ? "" : ConvertDate(dtpContractToDate));
                    sqlParameter.Add("reqChargeCount", txtReqChargeCount.Text);
                    sqlParameter.Add("openDate", IsDatePickerNull(dtpOpenDate) ? "" : ConvertDate(dtpOpenDate));
                    sqlParameter.Add("openReqDate", IsDatePickerNull(dtpOpenDate) ? "" : ConvertDate(dtpOpenReqDate));
                    //sqlParameter.Add("damdangjaName", txtDamdangjaName.Text);
                    sqlParameter.Add("damdangjaEmail", txtDamdangjaEMail.Text);
                    //sqlParameter.Add("damdangjaPhone", txtDamdangjaPhone.Text);
                    sqlParameter.Add("installLocationAddComments", txtInstallLocationAddComments.Text);
                    sqlParameter.Add("saledamdangjaName", txtSaledamdangjaName.Text);
                    sqlParameter.Add("saledamdangjaEmail", txtSaledamdangjaEmail.Text);
                    sqlParameter.Add("saledamdangjaPhone", txtSaledamdangjaPhone.Text);
                    sqlParameter.Add("saleCustomAddWork", txtSaleCustomAddWork.Text);
                    sqlParameter.Add("salegift",txtsalegift.Text);
                    sqlParameter.Add("salesComments", txtsalesComments.Text);
                    sqlParameter.Add("mtrAmount", (int)RemoveComma(txtdgdAccTotal.Text,true) + (int)RemoveComma(txtMtrCanopyOrderAmount.Text,true) + (int)RemoveComma(txtMtrShippingCharge.Text,true));
                    sqlParameter.Add("mtrShippingCharge", RemoveComma(txtMtrShippingCharge.Text, true));
                    sqlParameter.Add("mtrPriceUnitClss", cboMtrPriceUnitClss.SelectedValue != null ? cboMtrPriceUnitClss.SelectedValue.ToString() : "");
                    sqlParameter.Add("mtrCanopyInwareInfo", txtMtrCanopyInwareInfo.Text);
                    sqlParameter.Add("mtrCanopyOrderAmount", RemoveComma(txtMtrCanopyOrderAmount.Text,true));               
                    sqlParameter.Add("orderTypeID", cboOrderType.SelectedValue != null ? cboOrderType.SelectedValue.ToString() : "");

                    sqlParameter.Add("delOrderGov", strFlag == "U" ? tab2_clicked != true ? 0 : 1 : 1);

                    string sGetID = strFlag.Equals("I") ? string.Empty : txtOrderID.Text;
                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Order_iOrder";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "orderID";
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
                                if (kv.key == "orderID")
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
                        pro3.Name = "xp_Order_uOrder";
                        pro3.OutputUseYN = "N";
                        pro3.OutputName = "orderID";
                        pro3.OutputLength = "10";

                        Prolist.Add(pro3);
                        //ListParameter.Add(sqlParameter);

                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }
                    #endregion


                    if (tab2_clicked == true)
                    {
                        UpdateTbkMessage("시공사 정보 저장 중...");

                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? PrimaryKey : txtOrderID.Text);
                        sqlParameter.Add("searchReqDate", IsDatePickerNull(dtpSearchReqDate) ? "" : ConvertDate(dtpSearchReqDate));
                        sqlParameter.Add("searchDate", IsDatePickerNull(dtpSearchDate) ? "" : ConvertDate(dtpSearchDate));
                        sqlParameter.Add("searchDataAcptDate", IsDatePickerNull(dtpSearchDataAcptDate) ? "" : ConvertDate(dtpSearchDataAcptDate));
                        sqlParameter.Add("installLocationCount", RemoveComma(txtInstallLocationCount.Text,true));
                        //sqlParameter.Add("electrDeliveryMethodID", cboElectrDeliveryMethodID.SelectedValue != null ? cboElectrDeliveryMethodID.SelectedValue.ToString() : "");
                        sqlParameter.Add("electrDeliveryMethod", txtElectrDeliveryMethod.Text);
                        sqlParameter.Add("inspectionNeedYN", cboInspectionNeedYN.SelectedValue != null ? cboInspectionNeedYN.SelectedValue.ToString() : "");
                        sqlParameter.Add("addConstructCostSearch", RemoveComma(txtAddConstructCostSearch.Text,true));
                        sqlParameter.Add("addConstructCost", RemoveComma(txtAddConstructCost.Text,true));
                        sqlParameter.Add("searchComments", txtSearchComments.Text);
                        sqlParameter.Add("corpAcptNo", txtCorpAcptNo.Text);
                        sqlParameter.Add("corpApprovalDate", IsDatePickerNull(dtpCorpApprovalDate) ? "" : ConvertDate(dtpCorpApprovalDate));
                        sqlParameter.Add("corpEndDate", IsDatePickerNull(dtpCorpEndDate) ? "" : ConvertDate(dtpCorpEndDate));
                        sqlParameter.Add("corpLastEndDate", IsDatePickerNull(dtpCorpLastEndDate) ? "" : ConvertDate(dtpCorpLastEndDate));
                        sqlParameter.Add("corpComments", txtCorpComments.Text);
                        sqlParameter.Add("localGovPermissionNo", txtLocalGovPermissionNo.Text);
                        sqlParameter.Add("localGovBehaviorReportDate", IsDatePickerNull(dtpLocalGovBehaviorReportDate) ? "" : ConvertDate(dtpLocalGovBehaviorReportDate));
                        sqlParameter.Add("localGoComments", txtLocalGoComments.Text);
                        //sqlParameter.Add("superBeforeUseInspDate", ConvertDate(dtpSuperBeforeUseInspDate));
                        sqlParameter.Add("superBeforeUseInspPrintDate", ConvertDate(dtpSuperBeforeUseInspPrintDate));
                        sqlParameter.Add("superUseInspReqDate", ConvertDate(dtpSuperUseInspReqDate));

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_ord_uOrder_tab2";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "orderID";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }

                    if(tab3_clicked == true)
                    {
                        UpdateTbkMessage("한전&전기공사 정보 저장 중...");

                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? PrimaryKey : txtOrderID.Text);
                        //sqlParameter.Add("kepElectrDeliveryMethodID",cboKepElectrDeliveryMethodID.SelectedValue != null ? cboKepElectrDeliveryMethodID.SelectedValue.ToString() : "");
                        sqlParameter.Add("kepElectrDeliveryMethod",txtKepElectrDeliveryMethod.Text);
                        sqlParameter.Add("kepInstallLocationCount",RemoveComma(txtKepInstallLocationCount.Text, true));
                        sqlParameter.Add("kepOutLineConstructContext",txtKepOutLineConstructContext.Text);
                        sqlParameter.Add("kepInfraPayAmount",RemoveComma(txtKepInfraPayAmount.Text, true));
                        //sqlParameter.Add("kepManageInfraPayAmount",RemoveComma(txtKepManageInfraPayAmount.Text, true)); 운영사 시설부담금 
                        sqlParameter.Add("kepManageInfraPayDate", IsDatePickerNull(dtpKepManageInfraPayDate) ? "" : ConvertDate(dtpKepManageInfraPayDate));
                        sqlParameter.Add("kepElectrReqDate", IsDatePickerNull(dtpKepElectrReqDate) ? "" : ConvertDate(dtpKepElectrReqDate));
                        //sqlParameter.Add("kepInApprovalYN", cboKepInApprovalYN.SelectedValue != null ? cboKepInApprovalYN.SelectedValue.ToString() : ""); 인입승인여부 2025.02.27 김동호 팀장 요청 제거
                        //sqlParameter.Add("kepInApprovalDate", IsDatePickerNull(dtpKepInApprovalDate) ? "" : ConvertDate(dtpKepInApprovalDate)); 인입승인일 2025.02.27 김동호 팀장 요청 제거
                        sqlParameter.Add("kepParentChildCapacity",txtKepParentChildCapacity.Text);
                        sqlParameter.Add("kepPowerSupplyCapacity",RemoveComma(txtkepPowerSupplyCapacity.Text,true));
                        sqlParameter.Add("kepMeterInstallContext", txtKepMeterInstallContext.Text);
                        sqlParameter.Add("kepDamdangjaPhone", txtKepDamdangjaPhone.Text);
                        sqlParameter.Add("kepCustomNo", txtKepCustomNo.Text);
                        sqlParameter.Add("kepPaymentDate", IsDatePickerNull(dtpKepPaymentDate) ? "" : ConvertDate(dtpKepPaymentDate));
                        sqlParameter.Add("kepMeterInstallDate", IsDatePickerNull(dtpKepMeterInstallDate) ? "" : ConvertDate(dtpKepMeterInstallDate));
                        sqlParameter.Add("kepFaucetComments", txtKepFaucetComments.Text);
                        sqlParameter.Add("constrCustomID", txtConstrCustomID.Tag != null ? txtConstrCustomID.Tag.ToString() : "");
                        sqlParameter.Add("constrOrderDate", IsDatePickerNull(dtpConstrOrderDate) ? "" : ConvertDate(dtpConstrOrderDate));
                        sqlParameter.Add("constrDate", IsDatePickerNull(dtpConstrDate) ? "" : ConvertDate(dtpConstrDate));
                        sqlParameter.Add("constrDelyReason", txtConstrDelyReason.Text);
                        sqlParameter.Add("constrCompleteDate", IsDatePickerNull(dtpConstrCompleteDate) ? "" : ConvertDate(dtpConstrCompleteDate));
                        sqlParameter.Add("constrComments", txtConstrComments.Text);
                        sqlParameter.Add("electrSafeCheckDate", IsDatePickerNull(dtpElectrSafeCheckDate)?"":ConvertDate(dtpElectrSafeCheckDate));
                        sqlParameter.Add("electrSafeCheckSuppleContext", txtElectrBeforeInspSuppleContext.Text);
                        sqlParameter.Add("electrSafeCheckPrintDate", IsDatePickerNull(dtpElectrSafeCheckPrintDate) ? "" : ConvertDate(dtpElectrSafeCheckPrintDate));
                        sqlParameter.Add("electrSafeCheckLocation", txtElectrSafeCheckLocation.Text);
                        sqlParameter.Add("electrSafeCheckCost", RemoveComma(txtElectrSafeCheckCost.Text, true));
                        sqlParameter.Add("electrSafeCheckCostPayDate", IsDatePickerNull(dtpElectrSafeCheckCostPayDate)? "" : ConvertDate(dtpElectrSafeCheckCostPayDate));
                        sqlParameter.Add("electrBeforeUseCheckReqDate", IsDatePickerNull(dtpElectrBeforeUseCheckReqDate)? "" : ConvertDate(dtpElectrBeforeUseCheckReqDate));
                        sqlParameter.Add("electrBeforeUseCheckPrintDate", IsDatePickerNull(dtpElectrBeforeUseCheckPrintDate)? "" : ConvertDate(dtpElectrBeforeUseCheckReqDate));
                        sqlParameter.Add("electrBeforeUseCheckSuppleContext", txtElectrBeforeUseCheckSuppleContext.Text);
                        sqlParameter.Add("electrBeforeInspLocation", txtElectrBeforeInspLocation.Text);
                        sqlParameter.Add("electrBeforeInspReqDate", IsDatePickerNull(dtpElectrBeforeInspPrintDate)? "" : ConvertDate(dtpElectrBeforeInspPrintDate));
                        sqlParameter.Add("electrBeforeInspPrintDate", IsDatePickerNull(dtpElectrBeforeInspPrintDate)? "" : ConvertDate(dtpElectrBeforeInspPrintDate));
                        sqlParameter.Add("electrBeforeInspCost", RemoveComma(txtElectrBeforeInspCost.Text,true));
                        sqlParameter.Add("electrBeforeInspCostPayDate", IsDatePickerNull(dtpElectrBeforeInspCostPayDate)? "" : ConvertDate(dtpElectrBeforeInspCostPayDate));
                        sqlParameter.Add("electrBeforeInspSuppleContext", txtElectrBeforeInspSuppleContext.Text);
                        sqlParameter.Add("electrSafeCheckComments", txtElectrSafeCheckComments.Text);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_ord_uOrder_tab3";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "orderID";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }

                    if(tab4_clicked == true)
                    {

                        UpdateTbkMessage("감리&준공 정보 저장 중...");

                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? PrimaryKey : txtOrderID.Text);
                        sqlParameter.Add("superCustomID", txtSuperCustomID.Tag != null ? txtSuperCustomID.Tag.ToString() : "");
                        //sqlParameter.Add("superCostPayCustomID", txtSuperCostPayCustomID.Tag !=null? txtSuperCostPayCustomID.Tag.ToString() : ""); //감리비용 지출업체 주석처리 요청 2025.02.11
                        sqlParameter.Add("superCustomPhoneNo", txtSuperCustomPhoneNo.Text);
                        sqlParameter.Add("safeManageCustomID", txtSafeManageCustomID.Tag != null? txtSafeManageCustomID.Tag.ToString() :"");
                        sqlParameter.Add("safeManageCustomPhoneNo", txtSafeManageCustomPhoneNo.Text);
                        sqlParameter.Add("superSetCost",RemoveComma(txtSuperSetCost.Text,true));
                        sqlParameter.Add("superSetTaxPrintDate", IsDatePickerNull(dtpSuperSetTaxPrintDate)? "" : ConvertDate(dtpSuperSetTaxPrintDate));
                        sqlParameter.Add("superUseInspPayCustomID", txtSuperUseInspPayCustomID.Tag != null ? txtSuperUseInspPayCustomID.Tag.ToString() : "");
                        //sqlParameter.Add("superUseInspReqDate", IsDatePickerNull(dtpSuperUseInspReqDate) ? "" : ConvertDate(dtpSuperUseInspReqDate));
                        //sqlParameter.Add("superBeforeUseInspDate", IsDatePickerNull(dtpSuperBeforeUseInspDate) ? "" : ConvertDate(dtpSuperBeforeUseInspDate));
                        //sqlParameter.Add("superBeforeUseInspPrintDate", IsDatePickerNull(dtpSuperBeforeUseInspPrintDate)? "" : ConvertDate(dtpSuperBeforeUseInspPrintDate));
                        sqlParameter.Add("superComments", txtSuperComments.Text);
                        sqlParameter.Add("compReplyDate", IsDatePickerNull(dtpCompReplyDate) ? "" : ConvertDate(dtpCompReplyDate));
                        sqlParameter.Add("suppleContext", txtSuppleContext.Text);
                        sqlParameter.Add("suppleCompDate", IsDatePickerNull(dtpSuppleCompDate) ? "" : ConvertDate(dtpSuppleCompDate));
                        sqlParameter.Add("compSuppleReportContext", txtCompSuppleReportContext.Text);
                        sqlParameter.Add("compSuppleReportDate", IsDatePickerNull(dtpCompSuppleReportDate) ? "" : ConvertDate(dtpCompSuppleReportDate));
                        sqlParameter.Add("insurePrintDate", IsDatePickerNull(dtpInsurePrintDate)? "" : ConvertDate(dtpInsurePrintDate));
                        sqlParameter.Add("compReportCompDate", IsDatePickerNull(dtpCompReportCompDate)? "" :ConvertDate(dtpCompReportCompDate));
                        sqlParameter.Add("compReportComments", txtCompReportComments.Text);
                        sqlParameter.Add("accntComments", txtAccntComments.Text);


                        if(dgdAccnt.Items.Count > 0)
                        {
                            UpdateTbkMessage("정산 경리 정보 저장 중...");

                            string orderID = strFlag == "I" ? PrimaryKey : txtOrderID.Text;

                            for (int i = 0; i < dgdAccnt.Items.Count; i++)
                            {
                                var accntItem = dgdAccnt.Items[i] as Win_order_Order_U_CodView_dgdAccnt;

                                switch (i)
                                {

                                    //운영사 시공비
                                    case 0: //운영사시공비 선금
                                        sqlParameter.Add("accntMgrWorkPreTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkPreAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrWorkPreAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrWorkPreTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrWorkPreTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 1: //운영사시공비 중도금
                                        sqlParameter.Add("accntMgrWorkInterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkInterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrWorkInterComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrWorkInterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrWorkInterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 2: //운영사시공비 잔금
                                        sqlParameter.Add("accntMgrWorkAfterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkAfterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrWorkAfterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrWorkAfterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrWorkAfterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 3: //운영사시공비 총액
                                        sqlParameter.Add("accntMgrWorkTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrWorkAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrWorkTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrWorkTaxFileName", accntItem.column5FileName);
                                        break;
                                    //운영사 영업비
                                    case 4: //운영사 영업비 선금
                                        sqlParameter.Add("accntMgrSalesPreTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrSalesPreAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrSalesPreAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrSalesPreTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrSalesPreTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 5: //운영사 영업비 중도금
                                        sqlParameter.Add("accntMgrSalesInterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrSalesInterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrSalesInterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrSalesInterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrSalesInterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 6: //운영사 영업비 잔금
                                        sqlParameter.Add("accntMgrSalesAfterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrSalesAfterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrSalesAfterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrSalesAfterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrSalesAfterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 7: //운영사 영업비 총액
                                        sqlParameter.Add("accntMgrSalesTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrSalesAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntMgrSalesAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntMgrSalesTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntMgrSalesTaxFileName", accntItem.column5FileName);
                                        break;
                                    //시공팀
                                    case 8: //시공팀 선금
                                        sqlParameter.Add("accntWorkPreTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkPreAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntWorkPreAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntWorkPreTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntWorkPreTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 9: //시공팀 중도금
                                        sqlParameter.Add("accntWorkInterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkInterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntWorkInterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntWorkInterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntWorkInterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 10: //시공팀 잔금
                                        sqlParameter.Add("accntWorkAfterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkAfterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntWorkAfterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntWorkAfterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntWorkAfterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 11: //시공팀 총액
                                        sqlParameter.Add("accntWorkTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntWorkAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntWorkTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntWorkTaxFileName", accntItem.column5FileName);
                                        break;
                                    //영업팀
                                    case 12: //영업팀 선금
                                        sqlParameter.Add("accntSalesPreTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntSalesPreAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntSalesPreAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntSalesPreTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntSalesPreTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 13: //영업팀 중도금
                                        sqlParameter.Add("accntSalesInterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntSalesInterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntSalesInterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntSalesInterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntSalesInterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 14: //영업팀 잔금
                                        sqlParameter.Add("accntSalesAfterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntSalesAfterAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntSalesAfterAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntSalesAfterTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntSalesAfterTaxFileName", accntItem.column5FileName);
                                        break;
                                    case 15: //영업팀 총액
                                        sqlParameter.Add("accntSalesTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntSalesAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntSalesAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntSalesTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntSalesTaxFileName", accntItem.column5FileName);
                                        break;

                                    case 16: //도색 총액
                                        sqlParameter.Add("accntPaintTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntPaintAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntPaintAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntPaintTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntPaintTaxFileName", accntItem.column5FileName);
                                        break;

                                    case 17: //기타 총액
                                        sqlParameter.Add("accntEtcTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntEtcAmount", Convert.ToDecimal(RemoveComma(accntItem.column2Amount, true, typeof(decimal))));
                                        sqlParameter.Add("accntEtcAmountComments", accntItem.column3Comment);
                                        sqlParameter.Add("accntEtcTaxFilePath", !string.IsNullOrEmpty(accntItem.column4FilePath) ? "/ImageData/Order" + orderID : "");
                                        sqlParameter.Add("accntEtcTaxFileName", accntItem.column5FileName);
                                        break;
                                }
                            }
                        }
                       

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_ord_uOrder_tab4";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "orderID";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                    }

                    #region 계약내용 저장/수정

                    if(dgdAcc.Items.Count > 0)
                    {
                        UpdateTbkMessage("기기&액서서리 정보 저장 중...");

                        for (int i = 0; i < dgdAcc.Items.Count; i++)
                        {
                            var accItem = dgdAcc.Items[i] as Win_order_Order_U_CodView_dgdAcc;

                            sqlParameter.Clear();
                            sqlParameter.Add("orderID", strFlag == "I" ? PrimaryKey : txtOrderID.Text);
                            sqlParameter.Add("orderSeq", i + 1);
                            sqlParameter.Add("articleID", accItem.articleID.Trim() != string.Empty ? accItem.articleID : "");
                            sqlParameter.Add("orderTypeID", accItem.orderTypeID.Trim() != string.Empty ? accItem.orderTypeID : "");
                            sqlParameter.Add("chargeInwareUnitPrice", RemoveComma(accItem.chargeInwareUnitPrice, true));
                            sqlParameter.Add("chargeOrderDate", RemoveHyphen(accItem.chargeOrderDate));
                            sqlParameter.Add("chargeInwareDate", RemoveHyphen(accItem.chargeInwareDate));
                            sqlParameter.Add("chargeInwareQty", RemoveComma(accItem.chargeInwareQty, true));
                            sqlParameter.Add("chargeInwareLocation", accItem.chargeInwareLocation != null ? accItem.chargeInwareLocation : "");
                            sqlParameter.Add("canopyReqCustomID", accItem.canopyReqCustomID != null ? accItem.canopyReqCustomID : "");
                            sqlParameter.Add("chargeOrderQty", RemoveComma(accItem.chargeOrderQty, true));
                            sqlParameter.Add("chargeModelHelmat", accItem.chargeModelHelmat != null ? accItem.chargeModelHelmat : "");
                            sqlParameter.Add("chargeModelinLoc", accItem.chargeModelinloc != null ? accItem.chargeModelinloc : "");
                            sqlParameter.Add("chargeModelOneBody", accItem.chargeModelOneBody != null ? accItem.chargeModelOneBody : "");
                            sqlParameter.Add("chargeStandReqDate", RemoveHyphen(accItem.chargeStandReqDate));
                            sqlParameter.Add("chargeStandInwareDate", RemoveHyphen(accItem.chargeStandInwareDate));
                            sqlParameter.Add("comments", accItem.Comments != null ? accItem.Comments : "");
                            sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_Order_iOrder_dgdAcc";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "orderID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                        }
                    }

                 

                    if(tab2_clicked == true)
                    {
                        UpdateTbkMessage("지자체 사항 저장 중...");
                        //MessageBox.Show("dgdLocalGov Count : "+ dgdLocalGov.Items.Count.ToString());
                        for (int i = 0; i < dgdLocalGov.Items.Count; i++)
                        {
                            var localGovItem = dgdLocalGov.Items[i] as Win_order_Order_U_CodView_localGov;

                            sqlParameter.Clear();
                            sqlParameter.Add("orderID", strFlag == "I" ? PrimaryKey : txtOrderID.Text);
                            sqlParameter.Add("localGovSeq", i + 1);
                            sqlParameter.Add("localGovPermissionNo", localGovItem.localGovPermissionNo.Trim());
                            sqlParameter.Add("localGovBehaviorReportDate", RemoveHyphen(localGovItem.localGovBehaviorReportDate));
                            sqlParameter.Add("localGovBehaviorDate", RemoveHyphen(localGovItem.localGovBehaviorDate));
                            sqlParameter.Add("localGovSuppleContext", localGovItem.localGovSuppleContext);
                            sqlParameter.Add("localGovSuppleDate", RemoveHyphen(localGovItem.localGovSuppleDate));
                            sqlParameter.Add("localGovComments", localGovItem.localGovComments);
                            sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_Order_iOrder_dgdLocalGov";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "orderID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(new Dictionary<string, object>(sqlParameter));
                        }    
                    }

                    //for (int i = 0; i < dgdOrderStudent.Items.Count; i++)
                    //{
                    //    var orderStudent = dgdOrderStudent.Items[i] as Win_ord_Order_U_CodeView_OrderStudent_Nadaum;

                    //    sqlParameter.Clear();
                    //    sqlParameter.Add("orderID", string.IsNullOrEmpty(PrimaryKey) ? orderID_global : PrimaryKey);
                    //    sqlParameter.Add("grade", orderStudent.grade);
                    //    sqlParameter.Add("classTh", orderStudent.classTh.Trim() != "" ? Convert.ToInt32(orderStudent.classTh) : 0);
                    //    sqlParameter.Add("class", orderStudent.ban);
                    //    sqlParameter.Add("manCountPerClass", orderStudent.manCountPerClass.Trim() != "" ? Convert.ToInt32(orderStudent.manCountPerClass) : 0);
                    //    sqlParameter.Add("totalManCount", orderStudent.totalManCount.Trim() != "" ? Convert.ToInt32(RemoveComma(orderStudent.totalManCount)) : 0);
                    //    sqlParameter.Add("jobStartTime", orderStudent.jobStartTime.Replace(":", ""));
                    //    sqlParameter.Add("jobEndTime", orderStudent.jobEndTime.Replace(":", ""));
                    //    sqlParameter.Add("comments", orderStudent.comments);
                    //    sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                    //    Procedure pro2 = new Procedure();
                    //    pro2.Name = "xp_Order_iOrder_OrderStudent";
                    //    pro2.OutputUseYN = "N";
                    //    pro2.OutputName = "orderID";
                    //    pro2.OutputLength = "10";

                    //    Prolist.Add(pro2);
                    //    ListParameter.Add(new Dictionary<string, object>(sqlParameter));

                    //}

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



                    string FtpPk_key = strFlag == "I" ? PrimaryKey : txtOrderID.Text;

                    if (FtpPk_key.Trim() != string.Empty)
                    {
                        UpdateTbkMessage("견적 첨부파일 처리 중... 시간이 걸릴 수 있습니다.");

                        if (boolCallEst == true)
                        {
                            List<string> estFiles = new List<string>();
                            estFiles.Add(txtBeforeSearchConsultFileName.Text);
                            estFiles.Add(txtDrawFileName.Text);
                            estFiles.Add(txtSearchChecksheetFileName.Text);
                            estFiles.Add(txtPictureEarthFileName.Text);
                            estFiles.Add(txtSearchFileName.Text);
                            estFiles.Add(txtInstallLocationSheetFileName.Text);

                            if (!FTP_copyFiles(estFiles))
                                MessageBox.Show("견적 첨부파일 복사를 실패했습니다.");
                        }

                        if (deleteListFtpFile.Count > 0)
                        {
                            UpdateTbkMessage("첨부파일 처리 중...시간이 걸릴 수 있습니다.");

                            foreach (string[] str in deleteListFtpFile)
                            {
                                FTP_RemoveFile(FtpPk_key + "/" + str[0]);
                            }
                        }

                        if (listFtpFile.Count > 0)
                        {
                            UpdateTbkMessage("첨부파일 처리 중...시간이 걸릴 수 있습니다.");

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
                //lblMsg.Visibility = Visibility.Hidden;
                //UpdateTbkMessage("");

            }

            return flag;
        }

        private bool UpdateDBFtp(string orderID)
        {
            bool flag = false;

            string str_localpath = string.Empty;
            List<string[]> UpdateFilesInfo = new List<string[]>();

            //if (CheckDataFTP(txtName.Text, strFlag))
            //{
            try
            {
                //UpdateTbkMessage("첨부파일 처리 중...시간이 걸릴 수 있습니다.");

                //tab을 클릭할때 tab별 fillgrid()가 실행되는데 실행이 안되면 바인딩이 안 되었으므로 Text는 string.Empty, Tag는 Null임
                //이때 프로시저에서는 기존의 값을 Update함. 파일이 있는 곳에서 삭제버튼을 눌러서 첨부파일을 삭제하면 Tag를 String.Empty로 만듬
                //이때는 DEL로 전달해서 프로시저에서 처리함. 삼항연산자를 안써도 첨부파일 삭제버튼을 눌러 tag를 null로 처리해도 파일명만 DB에서 삭제되므로 없어진 것처럼 보이지만
                //경로는 그대로 남음. EVC는 첨부파일을 많이 다루므로 축적되면 용량낭비를 초래 할 수 있으므로 확실히 빈 값으로 두도록 함..

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderID);

                //tab1
                sqlParameter.Add("ContractFileName", txtContractFileName.Text.Trim() != "" ? txtContractFileName.Text : "");
                sqlParameter.Add("ContractFilePath", txtContractFileName.Tag != null ? txtContractFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
              
                //tab2
                sqlParameter.Add("beforeSearchConsultFileName", txtBeforeSearchConsultFileName.Text.Trim() != "" ? txtBeforeSearchConsultFileName.Text : "");
                sqlParameter.Add("beforeSearchConsultFilePath", txtBeforeSearchConsultFileName.Tag != null ? txtBeforeSearchConsultFileName.Tag.ToString().Trim() != string.Empty ?  "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("pictureEarthFileName", txtPictureEarthFileName.Text.Trim() != "" ? txtPictureEarthFileName.Text : "");
                sqlParameter.Add("pictureEarthFilePath", txtPictureEarthFileName.Tag !=null ? txtPictureEarthFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("drawFileName", txtDrawFileName.Text.Trim() != "" ? txtDrawFileName.Text : "");
                sqlParameter.Add("drawFilePath", txtDrawFileName.Tag != null ? txtDrawFileName.Tag.ToString().Trim() != string.Empty ?  "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("searchFileName", txtSearchFileName.Text.Trim() != "" ? txtSearchFileName.Text : "");
                sqlParameter.Add("searchFilePath", txtSearchFileName.Tag != null ? txtSearchFileName.Tag.ToString().Trim() != string.Empty?  "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("searchChecksheetFileName", txtSearchChecksheetFileName.Text.Trim() != "" ? txtSearchChecksheetFileName.Text : "");
                sqlParameter.Add("searchChecksheetFilePath", txtSearchChecksheetFileName.Tag != null ? txtSearchChecksheetFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("installLocationSheetFileName", txtInstallLocationSheetFileName.Text.Trim() != "" ? txtInstallLocationSheetFileName.Text : "");
                sqlParameter.Add("installLocationSheetFilePath", txtInstallLocationSheetFileName.Tag != null ? txtInstallLocationSheetFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
               
                sqlParameter.Add("localGoTaxFileName", txtLocalGoTaxFileName.Text.Trim() != "" ? txtLocalGoTaxFileName.Text : "");      
                sqlParameter.Add("localGoTaxFilePath", txtLocalGoTaxFileName.Tag != null ? txtLocalGoTaxFileName.Tag.ToString().Trim() != string.Empty ?  "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("localGovProveFileName", txtLocalGovProveFileName.Text.Trim() != "" ? txtLocalGovProveFileName.Text : "");
                sqlParameter.Add("localGovProveFilePath", txtLocalGovProveFileName.Tag != null ? txtLocalGovProveFileName.Tag.ToString().Trim() != string.Empty ? "/Image/Order/" + orderID : "DEL" : "");           
                

                //tab3
                sqlParameter.Add("kepElectrLineFileName", txtKepElectrLineFileName.Text.Trim() != "" ? txtKepElectrLineFileName.Text : "");
                sqlParameter.Add("kepElectrLineFilePath", txtKepElectrLineFileName.Tag != null ? txtKepElectrLineFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("kepFaucetAcptFileName", txtKepFaucetAcptFileName.Text.Trim() != "" ? txtKepFaucetAcptFileName.Text : "");
                sqlParameter.Add("kepFaucetAcptFilePath", txtKepFaucetAcptFileName.Tag != null ? txtKepFaucetAcptFileName.Tag.ToString().Trim() != string.Empty  ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("electrSafeInspPrintFileName", txtElectrSafeInspPrintFileName.Text.Trim() != "" ? txtElectrSafeInspPrintFileName.Text : "");
                sqlParameter.Add("electrSafeInspPrintFilePath", txtElectrSafeInspPrintFileName.Tag != null ? txtElectrSafeInspPrintFileName.Tag.ToString().Trim() !=string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("electrBeforeUseCheckPrintFileName", txtElectrBeforeUseCheckPrintFileName.Text.Trim() != "" ? txtElectrBeforeUseCheckPrintFileName.Text : "");
                sqlParameter.Add("electrBeforeUseCheckPrintFilePath", txtElectrBeforeUseCheckPrintFileName.Tag !=null ? txtElectrBeforeUseCheckPrintFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("electrBeforeUseInspFileName", txtElectrBeforeUseInspFileName.Text.Trim() != "" ? txtElectrBeforeUseInspFileName.Text : "");
                sqlParameter.Add("electrBeforeUseInspFilePath", txtElectrBeforeUseInspFileName.Tag !=null ? txtElectrBeforeUseInspFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("electrKepAcptFileName", txtElectrKepAcptFileName.Text.Trim() != "" ? txtElectrKepAcptFileName.Text : "");
                sqlParameter.Add("electrKepAcptFilePath", txtElectrKepAcptFileName.Tag != null ? txtElectrKepAcptFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                
                sqlParameter.Add("electrKepInfraPayBillFileName", txtElectrKepInfraPayBillFileName.Text.Trim() != "" ? txtElectrKepInfraPayBillFileName.Text : "");
                sqlParameter.Add("electrKepInfraPayBillFilePath", txtElectrKepInfraPayBillFileName.Tag != null ? txtElectrKepInfraPayBillFileName.Tag.ToString().Trim() !=string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "" );

                //sqlParameter.Add("electrUseContractFileName", txtElectrUseContractFileName.Text.Trim() != "" ? txtElectrUseContractFileName.Text : "");
                //sqlParameter.Add("electrUseContractFilePath", txtElectrUseContractFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrBeforeUseInspCostFileName", txtElectrBeforeUseInspCostFileName.Text.Trim() != "" ? txtElectrBeforeUseInspCostFileName.Text : "");
                sqlParameter.Add("electrBeforeUseInspCostFilePath", txtElectrBeforeUseInspCostFileName.Tag != null ? txtElectrBeforeUseInspCostFileName.Tag.ToString().Trim() != string.Empty ?"/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("electrTransCoUseFileName", txtElectrTransCoUseFileName.Text.Trim() != "" ? txtElectrTransCoUseFileName.Text : "");
                sqlParameter.Add("electrTransCoUseFilePath", txtElectrTransCoUseFileName.Tag != null ? txtElectrTransCoUseFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                //sqlParameter.Add("electrCoWorkFileName", txtElectrCoWorkFileName.Text.Trim() != "" ? txtElectrCoWorkFileName.Text : "");
                //sqlParameter.Add("electrCoWorkFilePath", txtElectrCoWorkFileName.Tag !=null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrCostFileName", txtElectrCostFileName.Text.Trim() != "" ? txtElectrCostFileName.Text : "");
                sqlParameter.Add("electrCostFilePath", txtElectrCostFileName.Tag !=null? txtElectrCostFileName.Tag.ToString() != string.Empty ?  "/ImageData/Order/" + orderID : "DEL" : "");

                //tab4
                sqlParameter.Add("superSetCheckFileName", txtSuperSetCheckFileName.Text.Trim() != "" ? txtSuperSetCheckFileName.Text : "");
                sqlParameter.Add("superSetCheckFilePath", txtSuperSetCheckFileName.Tag != null ? txtSuperSetCheckFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("superBeforeUseInspectFileName", txtSuperBeforeUseInspectFileName.Text.Trim() != "" ? txtSuperBeforeUseInspectFileName.Text : "");
                sqlParameter.Add("superBeforeUseInspectFilePath", txtSuperBeforeUseInspectFileName.Tag != null ? txtSuperBeforeUseInspectFileName.Tag.ToString().Trim() != string.Empty ?"/ImageData/Order/" + orderID : "DEL" : "");

                //sqlParameter.Add("superCostFileName", txtSuperCostFileName.Text.Trim() != "" ? txtSuperCostFileName.Text : "");                   --(감리) 수수료 내역서 2025.02.11 주석처리 요청
                //sqlParameter.Add("superCostFilePath", txtSuperCostFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("safeManagerCertiFileName", txtSafeManagerCertiFileName.Text.Trim() != "" ? txtSafeManagerCertiFileName.Text: "");
                sqlParameter.Add("safeManagerCertiFilePath", txtSafeManagerCertiFileName.Tag != null ? txtSafeManagerCertiFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                

                sqlParameter.Add("superReportFileName", txtSuperReportFileName.Text.Trim() != "" ? txtSuperReportFileName.Text : "");
                sqlParameter.Add("superReportFilePath", txtSuperReportFileName.Tag != null ? txtSuperReportFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("compReportFileName", txtCompReportFileName.Text.Trim() != "" ? txtCompReportFileName.Text : "");
                sqlParameter.Add("compReportFilePath", txtCompReportFileName.Tag != null ? txtCompReportFileName.Tag.ToString().Trim() != string.Empty? "/ImageData/Order/" + orderID : "DEL" : "");

                sqlParameter.Add("insurePrintFileName", txtInsurePrintFileName.Text.Trim() != "" ? txtInsurePrintFileName.Text : "");
                sqlParameter.Add("insurePrintFilePath", txtInsurePrintFileName.Tag != null ? txtInsurePrintFileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");

                //tab5
                sqlParameter.Add("sketch1FileName", txtSketch1.Text.Trim() != "" ? txtSketch1.Text : "");
                sqlParameter.Add("sketch1FilePath", txtSketch1.Tag != null ? txtSketch1.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch1FileAlias",txtSketch1FileAlias.Text.Trim() != "" ? txtSketch1FileAlias.Text : "");

                sqlParameter.Add("sketch2FileName", txtSketch2.Text.Trim() != "" ? txtSketch2.Text : "");
                sqlParameter.Add("sketch2FilePath", txtSketch2.Tag != null ? txtSketch2.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch2FileAlias", txtSketch2FileAlias.Text.Trim() != "" ? txtSketch2FileAlias.Text : "");

                sqlParameter.Add("sketch3FileName", txtSketch3.Text.Trim() != "" ? txtSketch3.Text : "");
                sqlParameter.Add("sketch3FilePath", txtSketch3.Tag != null ? txtSketch3.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch3FileAlias", txtSketch3FileAlias.Text.Trim() != "" ? txtSketch3FileAlias.Text : "");

                sqlParameter.Add("sketch4FileName", txtSketch4.Text.Trim() != "" ? txtSketch4.Text : "");
                sqlParameter.Add("sketch4FilePath", txtSketch4.Tag != null ? txtSketch4.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch4FileAlias", txtSketch4FileAlias.Text.Trim() != "" ? txtSketch4FileAlias.Text : "");

                sqlParameter.Add("sketch5FileName", txtSketch5.Text.Trim() != "" ? txtSketch5.Text : "");
                sqlParameter.Add("sketch5FilePath", txtSketch5.Tag != null ? txtSketch5.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch5FileAlias", txtSketch5FileAlias.Text.Trim() != "" ? txtSketch5FileAlias.Text : "");

                sqlParameter.Add("sketch6FileName", txtSketch6.Text.Trim() != "" ? txtSketch6.Text : "");
                sqlParameter.Add("sketch6FilePath", txtSketch6.Tag != null ? txtSketch6.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch6FileAlias", txtSketch6FileAlias.Text.Trim() != "" ? txtSketch6FileAlias.Text : "");

                sqlParameter.Add("sketch7FileName", txtSketch7.Text.Trim() != "" ? txtSketch7.Text : "");
                sqlParameter.Add("sketch7FilePath", txtSketch7.Tag != null ? txtSketch7.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch7FileAlias", txtSketch7FileAlias.Text.Trim() != "" ? txtSketch7FileAlias.Text : "");

                sqlParameter.Add("sketch8FileName", txtSketch8.Text.Trim() != "" ? txtSketch8.Text : "");
                sqlParameter.Add("sketch8FilePath", txtSketch8.Tag != null ? txtSketch8.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch8FileAlias", txtSketch8FileAlias.Text.Trim() != "" ? txtSketch8FileAlias.Text : "");

                sqlParameter.Add("sketch9FileName", txtSketch9.Text.Trim() != "" ? txtSketch9.Text : "");
                sqlParameter.Add("sketch9FilePath", txtSketch9.Tag != null ? txtSketch9.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch9FileAlias", txtSketch9FileAlias.Text.Trim() != "" ? txtSketch9FileAlias.Text : "");

                sqlParameter.Add("sketch10FileName", txtSketch10.Text.Trim() != "" ? txtSketch10.Text : "");
                sqlParameter.Add("sketch10FilePath", txtSketch10.Tag != null ? txtSketch10.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch10FileAlias", txtSketch10FileAlias.Text.Trim() != "" ? txtSketch10FileAlias.Text : "");

                sqlParameter.Add("sketch11FileName", txtSketch11.Text.Trim() != "" ? txtSketch11.Text : "");
                sqlParameter.Add("sketch11FilePath", txtSketch11.Tag != null ? txtSketch11.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch11FileAlias", txtSketch11FileAlias.Text.Trim() != "" ? txtSketch11FileAlias.Text : "");

                sqlParameter.Add("sketch12FileName", txtSketch12.Text.Trim() != "" ? txtSketch12.Text : "");
                sqlParameter.Add("sketch12FilePath", txtSketch12.Tag != null ? txtSketch12.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch12FileAlias", txtSketch12FileAlias.Text.Trim() != "" ? txtSketch12FileAlias.Text : "");

                //양식
                sqlParameter.Add("sketch13FileName", txtSketch13.Text.Trim() != "" ? txtSketch13.Text : "");
                sqlParameter.Add("sketch13FilePath", txtSketch13.Tag != null ? txtSketch13.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch13FileAlias", txtSketch13FileAlias.Text.Trim() != "" ? txtSketch13FileAlias.Text : "");

                sqlParameter.Add("sketch14FileName", txtSketch14.Text.Trim() != "" ? txtSketch14.Text : "");
                sqlParameter.Add("sketch14FilePath", txtSketch14.Tag != null ? txtSketch14.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch14FileAlias", txtSketch14FileAlias.Text.Trim() != "" ? txtSketch14FileAlias.Text : "");

                sqlParameter.Add("sketch15FileName", txtSketch15.Text.Trim() != "" ? txtSketch15.Text : "");
                sqlParameter.Add("sketch15FilePath", txtSketch15.Tag != null ? txtSketch15.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch15FileAlias", txtSketch15FileAlias.Text.Trim() != "" ? txtSketch15FileAlias.Text : "");

                sqlParameter.Add("sketch16FileName", txtSketch16.Text.Trim() != "" ? txtSketch16.Text : "");
                sqlParameter.Add("sketch16FilePath", txtSketch16.Tag != null ? txtSketch16.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch16FileAlias", txtSketch16FileAlias.Text.Trim() != "" ? txtSketch16FileAlias.Text : "");

                sqlParameter.Add("sketch17FileName", txtSketch17.Text.Trim() != "" ? txtSketch17.Text : "");
                sqlParameter.Add("sketch17FilePath", txtSketch17.Tag != null ? txtSketch17.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch17FileAlias", txtSketch17FileAlias.Text.Trim() != "" ? txtSketch17FileAlias.Text : "");
                                         
                sqlParameter.Add("sketch18FileName", txtSketch18.Text.Trim() != "" ? txtSketch18.Text : "");
                sqlParameter.Add("sketch18FilePath", txtSketch18.Tag != null ? txtSketch18.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch18FileAlias", txtSketch18FileAlias.Text.Trim() != "" ? txtSketch18FileAlias.Text : "");

                sqlParameter.Add("sketch19FileName", txtSketch19.Text.Trim() != "" ? txtSketch19.Text : "");
                sqlParameter.Add("sketch19FilePath", txtSketch19.Tag != null ? txtSketch19.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch19FileAlias", txtSketch19FileAlias.Text.Trim() != "" ? txtSketch19FileAlias.Text : "");

                sqlParameter.Add("sketch20FileName", txtSketch20.Text.Trim() != "" ? txtSketch20.Text : "");
                sqlParameter.Add("sketch20FilePath", txtSketch20.Tag != null ? txtSketch20.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch20FileAlias", txtSketch20FileAlias.Text.Trim() != "" ? txtSketch20FileAlias.Text : "");

                sqlParameter.Add("sketch21FileName", txtSketch21.Text.Trim() != "" ? txtSketch21.Text : "");
                sqlParameter.Add("sketch21FilePath", txtSketch21.Tag != null ? txtSketch21.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch21FileAlias", txtSketch21FileAlias.Text.Trim() != "" ? txtSketch21FileAlias.Text : "");

                sqlParameter.Add("sketch22FileName", txtSketch22.Text.Trim() != "" ? txtSketch22.Text : "");
                sqlParameter.Add("sketch22FilePath", txtSketch22.Tag != null ? txtSketch22.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + orderID : "DEL" : "");
                sqlParameter.Add("sketch22FileAlias", txtSketch22FileAlias.Text.Trim() != "" ? txtSketch22FileAlias.Text : "");




                string[] result = DataStore.Instance.ExecuteProcedure("xp_order_uOrder_FTP", sqlParameter, true);


                if (result[0].Equals("success"))
                {
                    //MessageBox.Show("성공 ㅎㅎㅎㅎㅎㅎㅎㅎㅎ");
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

        
        private bool CheckData()
        {
            string msg = "";
            string msgg = "";

            bool flag = true;

            if (txtManagerCustomID.Text.Length <= 0 || txtManagerCustomID.Tag == null)
                msg = "운영회사가 입력되지 않았습니다. 먼저 운영회사를 검색 입력하세요.";
            if (txtSalesCustomID.Text.Length <= 0 || txtSalesCustomID.Tag == null)
                msg = "영업회사가 입력되지 않았습니다. 먼저 영업회사를 검색 입력하세요.";
            if (txtSearchCustomID.Text.Length <= 0 || txtSearchCustomID.Tag == null)
                msg = "실사업체가 입력되지 않았습니다. 먼저 실사업체를 검색 입력하세요.";

            //if (txtOrderNo.Text.Length <= 0)
            //    msg = "수주번호는 필수입력 사항입니다. 먼저 수주번호를 입력하세요.";

            //if(strFlag == "U")
            //    if (!CheckFKkey(orderID_global))
            //        flag = false;

            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrEmpty(msgg))
            {
                string Message = string.IsNullOrEmpty(msg.Trim()) ? msgg : msg;

                if (!string.IsNullOrEmpty(Message))
                {
                    var result = MessageBox.Show(msg);
                    tabBasicData.Focus();
                }
                flag = false;
            }

            return flag;
        }

        private bool CheckFKkey(string orderID)
        {
            bool flag = true;

            string[] sqlList = { "select orderid from HR_DcntEvaluate where orderid = ",
                                 "select orderID from OutWare where orderID = ",
                                 "select orderid from Order_Lecturer where orderid = ",
                                 "select orderID from Order_Evaluate where orderID = ",
                                 "select orderID from Order_DcntFees where orderID = ",
                            
            };

            string[] errMsg = {"강사평가 등록 화면에서 사용중인 계약관리번호 입니다.",
                               "교구출고 등록 화면에서 사용중인 계약관리번호 입니다.",
                                "강의매칭 화면에서 사용중인 계약관리번호 입니다.",
                                "결과보고등록 화면에서 사용중인 계약관리번호 입니다.",
                                "강의료정산 화면에서 사용중인 계약관리번호 입니다.",
            };
            int errSeq = 0;
            string msg = string.Empty;

            //반복문을 돌다가 걸리면 종료, 경고문 띄우고 false반환
            for (int i = 0; i < sqlList.Length; i++)
            {
                DataSet ds = DataStore.Instance.QueryToDataSet(sqlList[i] + orderID);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        flag = false;
                        errSeq = i;
                        break;
                    }
                }
                else
                {
                    continue;
                }
            }

            if (flag == false)
            {
                msg = errMsg[errSeq];
                MessageBox.Show(msg);
            }

            return flag; 
        }

        private bool CheckContractData()
        {
            string msg = string.Empty;

            //string ElecTypeMgrWork = (cboElectrDeliveryMethodID.SelectedItem as CodeView)?.code_name;
            //if(tab2_clicked== true && ElecTypeMgrWork != null && ElecTypeMgrWork.Contains("한전"))
            //{
            //    if (dtpSuperBeforeUseInspDate.SelectedDate == null || dtpSuperBeforeUseInspDate.SelectedDate.ToString() == string.Empty)
            //    {
            //        msg += "[시공 및 실사정보] 전기수전방법이 *한전*이면\n반드시 사용전검사를 진행해야합니다.\n[시공 및 실사정보]탭의 *사용전검사 확인증 발급일*을 확인하세요";
            //    }
            //}

            if (msg.Length > 0)
            {
                var result =  MessageBox.Show(msg,"확인");
                if(result == MessageBoxResult.OK)
                {
                    //tabContractData.Focus();
                }               
                return false;
            }

            return true;
        }

        #region 입력시 Event
        //실사업체


        //품명
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //품명
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    //if (txtCustom != null && txtCustom.Text != "")
            //    //{   //선택된 납품거래처에 따른 품명만 보여주게
            //    //    MainWindow.pf.ReturnCodeGLS(txtArticle, 5102, txtCustom.Tag.ToString().Trim());
            //    //}
            //    //else
            //    //{   //선택된 납품거래처가 없다면 전체 품명 다 보여주게
            //        MainWindow.pf.ReturnCodeGLS(txtArticle, 5103, "");
            //    //}

            //    if (txtArticle.Tag != null)
            //    {
            //        CallArticleData(txtArticle.Tag.ToString());
            //        //품명종류 대입(ex.제품 등)
            //        //cboArticleGroup.SelectedValue = articleData.productTypeID;

            //        //품번 대입
            //        //txtBuyerArticleNO.Text = articleData.BuyerArticleNo;
            //        //품명 대입
            //        //txtBuyerArticleNO.Text = articleData.Article;
            //        //단가 대입
            //        //txtUnitPrice.Text = articleData.outUnitPrice;
            //    }

            //    //플러스 파인더 작동 후 규격으로 커서 이동
            //    txtSpec.Focus();
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            //}
            //finally
            //{
            //    DataStore.Instance.CloseConnection();
            //}
        }

        //차종 키다운 
        private void txtModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //MainWindow.pf.ReturnCode(txtModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");

                //cboOrderForm.Focus();
            }
        }

        //차종
        private void btnPfModel_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.pf.ReturnCode(txtModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");

            //주문 형태 콤보박스 열기
            //cboOrderForm.IsDropDownOpen = true; //2020.02.14 장가빈, 수정시 콤보박스 자동 열리는 것 불편하대서 주석처리 함
        }

        private void CallArticleData(string strArticleID)
        {
            //try
            //{
            //    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            //    sqlParameter.Add("ArticleID", strArticleID);

            //    DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sArticleData_Estimate", sqlParameter, false);

            //    if (ds != null && ds.Tables.Count > 0)
            //    {
            //        DataTable dt = ds.Tables[0];

            //        if (dt.Rows.Count > 0)
            //        {
            //            DataRow dr = dt.Rows[0];

            //            articleData = new ArticleData
            //            {
            //                articleID = dr["articleID"].ToString(),
            //                article = dr["article"].ToString(),
            //                //ThreadID = dr["ThreadID"].ToString(),
            //                //thread = dr["thread"].ToString(),
            //                //StuffWidth = dr["StuffWidth"].ToString(),
            //                //DyeingID = dr["DyeingID"].ToString(),
            //                weight = dr["weight"].ToString(),
            //                spec = dr["spec"].ToString(),
            //                //ArticleGrpID = dr["ArticleGrpID"].ToString(),
            //                //BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
            //                unitPrice = dr["outUnitPrice"].ToString().Split('.')[0],
            //                unitPriceTypeID = dr["outUnitTypeID"].ToString(),
            //                unitTypeID = dr["unitTypeID"].ToString(),
            //                codeName = dr["codeName"].ToString(),
            //                //ProcessName = dr["ProcessName"].ToString(),
            //                //HSCode = dr["HSCode"].ToString(),
            //                outUnitPrice = dr["OutUnitPrice"].ToString(),
            //                //BuyerModelID = dr["BuyerModelID"].ToString(),
            //                //BuyerModel = dr["BuyerModel"].ToString(),
            //            };
            //        }


            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            //}
            //finally
            //{
            //    DataStore.Instance.CloseConnection();
            //}

        }
        #endregion



        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ToString()))
            {
                return "0";
            }
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

        //private void UncheckDatePicker()
        //{
        //    FindUiObject(grdInput);
        //}
        private void UncheckDatePicker()
        {
            FindUiObject(grdInput, child =>
            {
                if (child is CheckBox checkBox)
                {                    
                    datePickerEnable(checkBox);
                }
            });
        }

        //이벤트 핸들러 등록
        private void DatePickerSetToday_EventHandler()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4 };

            foreach(Grid grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is DatePicker datePicker)
                    {
                        // 이벤트 핸들러 등록
                        datePicker.PreviewMouseDown += DatePicker_PreviewMouseDown;
                        datePicker.PreviewKeyDown += DatePicker_PreviewKeyDown;
                    }
                });
            }
  
        }

        //이벤트 핸들러 등록 해제
        private void DatePickerSetToday_RemoveHandler()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4 };

            foreach (Grid grid in grids)
            {
                 FindUiObject(grid, child =>
                {
                    if (child is DatePicker datePicker)
                    {
                        datePicker.PreviewMouseDown -= DatePicker_PreviewMouseDown;
                        datePicker.PreviewKeyDown -= DatePicker_PreviewKeyDown;
                    }
                });

            }

        }


        //DatePicker 프리뷰마우스다운
        private void DatePicker_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                if (datePicker.SelectedDate == null)
                    datePicker.SelectedDate = DateTime.Today;
            }
        }

        //DatePicker 프리뷰키다운
        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is DatePicker datePicker)
            {
                if (datePicker.SelectedDate == null)
                    datePicker.SelectedDate = DateTime.Today;
            }
        }


        private void DataGrid_LostFocus_Calculate(object sender, RoutedEventArgs e)
        {
            var element = sender as DependencyObject;
            while (element != null && !(element is DataGrid))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            var currentGrid = element as DataGrid;
            //if (currentGrid == null || currentGrid.ItemsSource == null)
            //    return;

            if (currentGrid == null)
                return;

            if (currentGrid.Items.Count == 0)  // Items 컬렉션이 비어있는지 체크
                return;

            if (currentGrid.Name.Equals("dgdAcc"))
            {
                int sumTotal = 0;

                txtdgdAccTotal.Text = "";

                foreach (Win_order_Order_U_CodView_dgdAcc item in currentGrid.Items)
                {
                    int item1 = (int)RemoveComma(item.chargeInwareUnitPrice, true);
                    int item2 = (int)RemoveComma(item.chargeInwareQty, true);                 

                    int total = item1 * item2;

                    sumTotal += total;
                   
                }

                txtdgdAccTotal.Text = sumTotal.ToString();

            }

            if (currentGrid.Name.Equals("dgdAccnt"))
            {
                var cell = e.OriginalSource as DataGridCell;
                if (cell == null) return;

                int currentRowIndex = currentGrid.Items.IndexOf(cell.DataContext);
                int currentColumnIndex = cell.Column.DisplayIndex;

                if (currentColumnIndex == 1 && currentRowIndex < 16)
                {
                    var currentItem = currentGrid.Items[currentRowIndex] as Win_order_Order_U_CodView_dgdAccnt;
                    if (currentItem == null) return;

                    int groupStartIndex = (currentRowIndex / 4) * 4;
                    int totalRowIndex = groupStartIndex + 3;
                    int balanceRowIndex = groupStartIndex + 2;

                    var totalItem = currentGrid.Items[totalRowIndex] as Win_order_Order_U_CodView_dgdAccnt;
                    if (totalItem == null) return;

                    decimal sum = 0;
                    for (int i = groupStartIndex; i < groupStartIndex + 2; i++)
                    {
                        var item = currentGrid.Items[i] as Win_order_Order_U_CodView_dgdAccnt;
                        if (item != null && !string.IsNullOrEmpty(item.column2Amount))
                        {
                            sum += (decimal)RemoveComma(item.column2Amount, true, typeof(decimal));
                        }
                    }

                    var balanceItem = currentGrid.Items[balanceRowIndex] as Win_order_Order_U_CodView_dgdAccnt;
                    if (balanceItem != null)
                    {
                        decimal totalAmount = (decimal)RemoveComma(totalItem.column2Amount, true, typeof(decimal));
                        decimal balance = totalAmount - sum;
                        balanceItem.column2Amount = balance.ToString();

                        // 잔액 행(3번째) 셀 스타일 설정
                        DataGridRow balanceRow = (DataGridRow)currentGrid.ItemContainerGenerator.ContainerFromIndex(balanceRowIndex);
                        if (balanceRow != null)
                        {
                            var presenter = currentGrid.Columns[1].GetCellContent(balanceRow);
                            if (presenter != null)
                            {
                                var textBlock = VisualTreeHelper.GetChild(presenter, 0) as TextBlock;
                                if (textBlock != null)
                                {
                                    textBlock.Foreground = balance < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
                                }
                            }
                        }

                        // 총액 행(4번째) 셀 스타일 설정
                        DataGridRow totalRow = (DataGridRow)currentGrid.ItemContainerGenerator.ContainerFromIndex(totalRowIndex);
                        if (totalRow != null)
                        {
                            var presenter = currentGrid.Columns[1].GetCellContent(totalRow);
                            if (presenter != null)
                            {
                                var textBlock = VisualTreeHelper.GetChild(presenter, 0) as TextBlock;
                                if (textBlock != null)
                                {
                                    textBlock.FontWeight = FontWeights.Bold;
                                }
                            }
                        }
                    }
                }
            }

        }

        private void SetDatePickerToday()
        {
            List<Grid> grids = new List<Grid>();

            grids.Add(grdInput);
            grids.Add(grd2);
            grids.Add(grd3);
            grids.Add(grd4);
            
            foreach(var grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is DatePicker datePicker)
                    {
                        datePicker.SelectedDate = DateTime.Today;
                    }
                });
            }
        }

        private void ClearGrdInput()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4, grd5 };

            foreach(Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if (child is TextBox textbox)
                    {
                        textbox.Text = string.Empty;
                        textbox.Tag = null;
                    }
                    else if (child is DatePicker datePicker)
                    {
                        datePicker.SelectedDate = null;
                    }
                    else if (child is ComboBox combo)
                    {
                        if (combo.Name == "cboOrderType" && combo != null)
                        {
                            combo.SelectedIndex = 0;

                        }
                    }
                    else if (child is DataGrid dgd)
                    {
                        if (dgd.ItemsSource != null)
                        {
                            var originalCollection = dgd.ItemsSource;
                            dgd.ItemsSource = null; // 연결 해제

                            if (originalCollection is IList list)
                            {
                                list.Clear();                          
                                dgd.ItemsSource = originalCollection;
                            }
                            else if (originalCollection is ObservableCollection<object> ovc)
                            {
                                ovc.Clear();                     
                                dgd.ItemsSource = originalCollection;
                            }
                     
                        }
                    }

                });
            }           
        }

        private void UncheckCheckBox()
        {
            List<Grid> grids = new List<Grid> {grdExtraSearchCondtions};
            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if(child is TextBox txtBox)
                    {
                        txtBox.Text = string.Empty;
                        txtBox.Tag = null;
                    }
                    else if(child is CheckBox chk)
                    {
                        chk.IsChecked = false;
                    }
                });
            }

        }

        private void DisablekCheckBox()
        {
            List<Grid> grids = new List<Grid> { grdExtraSearchCondtions, grdSearchConditionsTop};
            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if(child is CheckBox checkbox)
                    {
                        if(!checkbox.Name.Equals("chkUniversalSearhSrh"))
                        checkbox.IsEnabled = false;
                    }
                });
            }

        }

        private void EnablekCheckBox()
        {
            List<Grid> grids = new List<Grid> { grdExtraSearchCondtions, grdSearchConditionsTop };
            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if (child is CheckBox checkbox)
                    {
                        if (!checkbox.Name.Equals("chkUniversalSearhSrh"))
                            checkbox.IsEnabled = true;
                    }
                });
            }

        }

        private void addLstFile_FTP()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4, grd5 };

            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if (child is TextBox textbox)
                    {
                        if (textbox.Name.Contains("FileName") || textbox.Name.Contains("txtSketch"))
                        {
                            if (!string.IsNullOrWhiteSpace(textbox.Text))
                            {
                                lstFilesName.Add(textbox.Text.Trim());
                            }
                        }
                    }                

                });
            }
        }

        private void ClearGrdFtpTextBox()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4, grd5 };

            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if ((chkEoAddSrh.IsChecked==true || isBringLastOrder == true)&&(child is TextBox textbox))
                    {
                        if(textbox.Name.Contains("FileName") || textbox.Name.Contains("txtSketch"))
                        {
                            textbox.Text = string.Empty;
                            textbox.Tag = null;
                        }               
                    }
                });
            }
        }



        private void ClearGrid()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4, grd5 };
            foreach (var grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is DataGrid dgd)
                    {
                        if (dgd.Items.Count > 0)
                        {
                            dgd.ItemsSource = null;
                            dgd.Items.Clear();
                        }
                    }
                });
            }
         }

        private void  ClearFTP_TextBox()
        {            
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3, grd4, grd5 };
            foreach (var grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is TextBox txtbox)
                    {
                        if (txtbox.Name.Contains("FileName") || txtbox.Name.Contains("txtSketch"))
                        {
                            txtbox.Text = string.Empty;
                            txtbox.Tag = null;
                        }
                    }
                });
            }
        }

        private void Tab5TextBoxEnable()
        {
            List<Grid> grids = new List<Grid> { grd5 };

            foreach (var grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is TextBox txtbox && txtbox.Name.StartsWith("txtSketch"))
                    {
                        string numberPart = txtbox.Name.Substring("txtSketch".Length);

                        //이름의 남은부분이 숫자일때.. 
                        if (numberPart.All(char.IsDigit) && !string.IsNullOrEmpty(numberPart))
                        {
                            // 파일명이 있는 경우에만 별칭 활성화
                            if (!string.IsNullOrWhiteSpace(txtbox.Text?.Trim()))
                            {
                                var fileAliasTextBox = FindName($"txtSketch{numberPart}FileAlias") as TextBox;
                                if (fileAliasTextBox != null)
                                {
                                    fileAliasTextBox.IsReadOnly = false;
                                    fileAliasTextBox.IsEnabled = true;
                                }
                            }
                        }
                    }
                });
            }
        }

        private void SetComboBoxIndexZero()
        {
            List<Grid> grids = new List<Grid> { grdInput, grd2, grd3,grd4 };

            foreach(var grid in grids)
            {
                FindUiObject(grid, child =>
                {
                    if (child is ComboBox combo)
                    {
                        if (combo.ItemsSource != null && combo.Items.Count > 0)
                        {
                            combo.SelectedIndex = 0;
                        }
                    }
                });
            }

        }

        private void setFTP_Tag_EmptyString()
        {
            txtContractFileName.Text = string.Empty;
            txtContractFileName.Tag = string.Empty;
            //txtContractOkFileName.Text = string.Empty;
            //txtContractOkFileName.Tag = string.Empty;
            //txtRightOkFileName.Text = string.Empty;
            //txtRightOkFileName.Tag = string.Empty;
            //txtSexAssaultAcptFileName.Text = string.Empty;
            //txtSexAssaultAcptFileName.Tag = string.Empty;
            //txtCustomjobAcptFileName.Text = string.Empty;
            //txtCustomjobAcptFileName.Tag = string.Empty;
        }

        //UI컨트롤 요소찾기
        //private void FindUiObject(DependencyObject parent)
        //{
        //    int childCount = VisualTreeHelper.GetChildrenCount(parent);

        //    for (int i = 0; i < childCount; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);

        //        if (child is CheckBox checkbox)
        //        {
        //            if(lblMsg.Visibility == Visibility.Hidden)
        //            {
        //                CheckDatePickerValue(checkbox);
        //            }
        //            else if(checkbox.Name != "chkOrderDate" && strFlag == "I")
        //            {
        //                string datePickerName = "dtp" + checkbox.Name.Substring(3);
        //                var datePicker = this.FindName(datePickerName) as DatePicker;

        //                checkbox.IsChecked = false;                     
        //                datePicker.IsEnabled = false;
        //            }
        //        }

        //        FindUiObject(child);
        //    }
        //}

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


        // 자식요소 안에서 부모요소 찾기
        //DataGridRow row = FindVisualParent<DataGridRow>(checkBox); 데이터그리드안의 행속 체크박스의 부모행 찾기
        //DataGrid parentGrid = FindVisualParent<DataGrid>(row); 데이터그리드 행의 부모 데이터그리드 찾기
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }


        

        private void datePickerEnable(CheckBox childCheckbox)
        {
            if (lblMsg.Visibility == Visibility.Hidden)
            {
                CheckDatePickerValue(childCheckbox);
            }
            else if (!string.IsNullOrEmpty(childCheckbox.Name) && childCheckbox.Name != "chkOrderDate" && strFlag == "I")
            {
                string datePickerName = "dtp" + childCheckbox.Name.Substring(3);
                var datePicker = this.FindName(datePickerName) as DatePicker;

                if (datePicker != null)
                {
                    childCheckbox.IsChecked = false;
                    datePicker.IsEnabled = false;
                }
            }
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
            if (ClickPoint.Equals("Contract")) { FTP_Upload_TextBox(txtContractFileName); }  //긴 경로(FULL 사이즈)
            else if (ClickPoint.Equals("BeforeSearchConsult")) { FTP_Upload_TextBox(txtBeforeSearchConsultFileName); }
            else if (ClickPoint.Equals("PictureEarth")) { FTP_Upload_TextBox(txtPictureEarthFileName); }
            else if (ClickPoint.Equals("Draw")) { FTP_Upload_TextBox(txtDrawFileName); }
            else if (ClickPoint.Equals("Search")) { FTP_Upload_TextBox(txtSearchFileName); }
            else if (ClickPoint.Equals("SearchChecksheet")) { FTP_Upload_TextBox(txtSearchChecksheetFileName); }
            else if (ClickPoint.Equals("InstallLocationSheet")) { FTP_Upload_TextBox(txtInstallLocationSheetFileName); }
            else if (ClickPoint.Equals("LocalGoTax")) { FTP_Upload_TextBox(txtLocalGoTaxFileName); }                      
            else if (ClickPoint.Equals("LocalGovProve")) { FTP_Upload_TextBox(txtLocalGovProveFileName); }
            else if (ClickPoint.Equals("kepElectrLine")) { FTP_Upload_TextBox(txtKepElectrLineFileName); }
            else if (ClickPoint.Equals("kepFaucetAcpt")) { FTP_Upload_TextBox(txtKepFaucetAcptFileName); }
            else if (ClickPoint.Equals("ElectrSafeInspPrint")) { FTP_Upload_TextBox(txtElectrSafeInspPrintFileName); }
            else if (ClickPoint.Equals("ElectrBeforeUseCheckPrint")) { FTP_Upload_TextBox(txtElectrBeforeUseCheckPrintFileName); }
            else if (ClickPoint.Equals("ElectrBeforeUseInsp")) { FTP_Upload_TextBox(txtElectrBeforeUseInspFileName); }
            else if (ClickPoint.Equals("ElectrKepAcpt")) { FTP_Upload_TextBox(txtElectrKepAcptFileName); }
            else if (ClickPoint.Equals("ElectrKepInfraPayBill")) { FTP_Upload_TextBox(txtElectrKepInfraPayBillFileName); }
            //else if (ClickPoint.Contains("ElectrUseContract")) { FTP_Upload_TextBox(txtElectrUseContractFileName); }
            else if (ClickPoint.Equals("ElectrBeforeUseInspCost")) { FTP_Upload_TextBox(txtElectrBeforeUseInspCostFileName); }
           // else if (ClickPoint.Contains("ElectrCoWork")) { FTP_Upload_TextBox(txtElectrCoWorkFileName); }
            else if (ClickPoint.Equals("ElectrTransCoUse")) { FTP_Upload_TextBox(txtElectrTransCoUseFileName); } //변압기공동이용계약서
            else if (ClickPoint.Equals("ElectrCost")) { FTP_Upload_TextBox(txtElectrCostFileName); }
            else if (ClickPoint.Equals("SuperSetCheck")) { FTP_Upload_TextBox(txtSuperSetCheckFileName); }
            else if (ClickPoint.Equals("SuperBeforeUseInspect")) { FTP_Upload_TextBox(txtSuperBeforeUseInspectFileName); }
            //else if (ClickPoint.Contains("SuperCostFile")) { FTP_Upload_TextBox(txtSuperCostFileName); }
            else if (ClickPoint.Equals("SafeManagerCerti")) { FTP_Upload_TextBox(txtSafeManagerCertiFileName); }
            else if (ClickPoint.Equals("SuperReportFile")) { FTP_Upload_TextBox(txtSuperReportFileName); }
            else if (ClickPoint.Equals("CompReport")) { FTP_Upload_TextBox(txtCompReportFileName); }
            else if (ClickPoint.Equals("InsurePrint")) { FTP_Upload_TextBox(txtInsurePrintFileName); }
            

            else if(ClickPoint.Equals("btnSketch1")) { FTP_Upload_TextBox(txtSketch1); txtSketch1FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch2")) { FTP_Upload_TextBox(txtSketch2); txtSketch2FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch3")) { FTP_Upload_TextBox(txtSketch3); txtSketch3FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch4")) { FTP_Upload_TextBox(txtSketch4); txtSketch4FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch5")) { FTP_Upload_TextBox(txtSketch5); txtSketch5FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch6")) { FTP_Upload_TextBox(txtSketch6); txtSketch6FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch7")) { FTP_Upload_TextBox(txtSketch7); txtSketch7FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch8")) { FTP_Upload_TextBox(txtSketch8); txtSketch8FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch9")) { FTP_Upload_TextBox(txtSketch9); txtSketch9FileAlias.IsReadOnly = false; }
            else if(ClickPoint.Equals("btnSketch10")) { FTP_Upload_TextBox(txtSketch10); txtSketch10FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch11")) { FTP_Upload_TextBox(txtSketch11); txtSketch11FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch12")) { FTP_Upload_TextBox(txtSketch12); txtSketch12FileAlias.IsReadOnly = false; }

            //양식
            else if (ClickPoint.Equals("btnSketch13")) { FTP_Upload_TextBox(txtSketch13); txtSketch13FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch14")) { FTP_Upload_TextBox(txtSketch14); txtSketch14FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch15")) { FTP_Upload_TextBox(txtSketch15); txtSketch15FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch16")) { FTP_Upload_TextBox(txtSketch16); txtSketch16FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch17")) { FTP_Upload_TextBox(txtSketch17); txtSketch17FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch18")) { FTP_Upload_TextBox(txtSketch18); txtSketch18FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch19")) { FTP_Upload_TextBox(txtSketch19); txtSketch19FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch20")) { FTP_Upload_TextBox(txtSketch20); txtSketch20FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch21")) { FTP_Upload_TextBox(txtSketch21); txtSketch21FileAlias.IsReadOnly = false; }
            else if (ClickPoint.Equals("btnSketch22")) { FTP_Upload_TextBox(txtSketch22); txtSketch22FileAlias.IsReadOnly = false; }

        }


        private void FTP_Upload_TextBox(TextBox textBox)
        {

            try
            {
                if (!textBox.Text.Equals(string.Empty) && strFlag.Equals("U"))
                {
                    MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                    return;
                }
                else
                {


                    Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();
                    //OFdlg.Filter =
                    //    "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx, *.pdf) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx; *.pdf | All Files|*.*";

                    OFdlg.Filter = "모든 파일 (*.*)|*.*";

                    Nullable<bool> result = OFdlg.ShowDialog();
                    if (result == true)
                    {
                        // 선택된 파일의 확장자 체크
                        if (MainWindow.OFdlg_Filter_NotAllowed.Contains(Path.GetExtension(OFdlg.FileName).ToLower()))
                        {
                            MessageBox.Show("보안상의 이유로 해당 파일은 업로드할 수 없습니다.");
                            return;
                        }

                        strFullPath = OFdlg.FileName;

                        string ImageFileName = OFdlg.SafeFileName;  //명.
                        string ImageFilePath = string.Empty;       // 경로


                        //프로세스 점유중인 파일도 스트림 가능한데 이거하려면 FTP업로드하는 메서드도 다 바꿔야함
                        //long FileSize;
                        //using (FileStream fs = new FileStream(OFdlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        //{
                        //    FileSize = fs.Length;
                        //}

                        // 파일명 유효성 검사 추가
                        //if (!IsValidFileName(ImageFileName))
                        //{
                        //    MessageBox.Show("파일명에 허용되지 않는 특수문자가 포함되어 있습니다.\n시스템 저장시 오류를 일으킬 수 있으므로 변경 후 첨부하여 주세요");
                        //    return;
                        //}

                        ImageFilePath = strFullPath.Replace(ImageFileName, "");

                        StreamReader sr = new StreamReader(OFdlg.FileName);
                        long FileSize = sr.BaseStream.Length;
                        //if (sr.BaseStream.Length > (4096 * 1000))
                        //{
                        //    //업로드 파일 사이즈범위 초과
                        //    MessageBox.Show("이미지의 파일사이즈가 4M byte를 초과하였습니다.");
                        //    sr.Close();
                        //    return;
                        //}
                        if (sr.BaseStream.Length > (1024 * 1024 * 500))  
                        {
                            //업로드 파일 사이즈범위 초과기
                            MessageBox.Show("첨부파일 크기는 500Mb 미만 이어야 합니다.");
                            sr.Close();
                            return;
                        }
                        if (!FTP_Upload_Name_Cheking(ImageFileName))
                        {
                            MessageBox.Show("업로드 하려는 파일 중, 이름이 중복된 항목이 있습니다." +
                                            "\n파일 이름을 변경하고 다시 시도하여 주세요\n다른 탭에 중복된 파일이 있는지 확인하세요.");
                        }
                        else
                        {
                            textBox.Text = ImageFileName;
                            textBox.Tag = ImageFilePath;

                            string[] strTemp = new string[] { ImageFileName, ImageFilePath.ToString() };
                            listFtpFile.Add(strTemp);
                            lstFilesName.Add(ImageFileName);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (ex.ToString().Contains("사용 중"))
                    MessageBox.Show("업로드하려는 파일이 열려 있습니다.\n먼저 파일을 닫고 첨부하여 주세요");
            }
           
        }

        private bool IsValidFileName(string fileName)
        {
            // Windows에서 파일명으로 사용할 수 없는 문자들만 체크
            string pattern = $"^[^{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+$";
            try
            {
                bool isValid = Regex.IsMatch(fileName, pattern);
                if (!isValid)
                {
                    // 어떤 문자가 불가능한지 확인
                    var invalidChars = fileName.Where(c => Path.GetInvalidFileNameChars().Contains(c)).ToList();
                    Console.WriteLine($"Invalid characters found: {string.Join(", ", invalidChars)}");
                }
                return isValid;
            }
            catch
            {
                return false;
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


        //private void OpenFileAndSetting(object sender, RoutedEventArgs e)
        //{
        //    // (버튼)sender 마다 tag를 달자.
        //    string ClickPoint = ((Button)sender).Tag.ToString();
        //    string[] strTemp = null;
        //    Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();

        //    OFdlg.DefaultExt = ".jpg";
        //    OFdlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png | All Files|*.*";

        //    Nullable<bool> result = OFdlg.ShowDialog();
        //    if (result == true)
        //    {
        //        if (ClickPoint == "Contract") { FullPath1 = OFdlg.FileName; }  //긴 경로(FULL 사이즈)
        //        if (ClickPoint == "ContractOk") { FullPath2 = OFdlg.FileName; }
        //        if (ClickPoint == "RightOk") { FullPath3 = OFdlg.FileName; }
        //        if (ClickPoint == "SexAssaultAcpt") { FullPath4 = OFdlg.FileName; }
        //        if (ClickPoint == "CustomJobAcpt") { FullPath5 = OFdlg.FileName; }

        //        string AttachFileName = OFdlg.SafeFileName;  //명.
        //        string AttachFilePath = string.Empty;       // 경로

        //        if (ClickPoint == "Contract") { AttachFilePath = FullPath1.Replace(AttachFileName, ""); }
        //        if (ClickPoint == "ContractOk") { AttachFilePath = FullPath2.Replace(AttachFileName, ""); }
        //        if (ClickPoint == "RightOk") { AttachFilePath = FullPath3.Replace(AttachFileName, ""); }
        //        if (ClickPoint == "SexAssaultAcpt") { AttachFilePath = FullPath4.Replace(AttachFileName, ""); }
        //        if (ClickPoint == "CustomJobAcpt") { AttachFilePath = FullPath5.Replace(AttachFileName, ""); }


        //        StreamReader sr = new StreamReader(OFdlg.FileName);
        //        long File_size = sr.BaseStream.Length;
        //        if (sr.BaseStream.Length > (2048 * 1000))
        //        {
        //            // 업로드 파일 사이즈범위 초과
        //            MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
        //            sr.Close();
        //            return;
        //        }
        //        if (ClickPoint == "Contract")
        //        {
        //            txtContractFileName.Text = AttachFileName;
        //            txtContractFileName.Tag = AttachFilePath.ToString();
        //        }
        //        else if (ClickPoint == "ContractOk")
        //        {
        //            txtContractOkFileName.Text = AttachFileName;
        //            txtContractOkFileName.Tag = AttachFilePath.ToString();
        //        }
        //        else if (ClickPoint == "RigthOk")
        //        {
        //            txtRightOkFileName.Text = AttachFileName;
        //            txtRightOkFileName.Tag = AttachFilePath.ToString();
        //        }
        //        else if (ClickPoint == "SexAssaultAcpt")
        //        {
        //            txtSexAssaultAcptFileName.Text = AttachFileName;
        //            txtSexAssaultAcptFileName.Tag = AttachFilePath.ToString();
        //        }
        //        else if (ClickPoint == "CustomJobAcpt")
        //        {
        //            txtCustomjobAcptFileName.Text = AttachFileName;
        //            txtCustomjobAcptFileName.Tag = AttachFilePath.ToString();
        //        }
        //        strTemp = new string[] { AttachFileName, AttachFilePath.ToString() };
        //        listFtpFile.Add(strTemp);
        //    }
        //}

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


                if (_ftp.createDirectory(MakeFolderName) == false)
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
            if (txtOrderID.Text != "")
            {
                MessageBoxResult msgresult = MessageBox.Show("다운로드 후 파일을 바로 여시겠습니까?", "보기 확인", MessageBoxButton.YesNoCancel);
                if (msgresult == MessageBoxResult.Yes || msgresult == MessageBoxResult.No)
                {
                    //버튼 태그값.
                    string ClickPoint = ((Button)sender).Tag.ToString();

                    string contractFileName = txtContractFileName.Text.Trim() != "" ? txtContractFileName.Text : "";
                    string beforeSearchConsultFileName = txtBeforeSearchConsultFileName.Text.Trim() != "" ? txtBeforeSearchConsultFileName.Text : "";
                    string pictureEarthFileName = txtPictureEarthFileName.Text.Trim() != "" ? txtPictureEarthFileName.Text : "";
                    string drawFileName = txtDrawFileName.Text.Trim() != "" ? txtDrawFileName.Text : "";
                    string searchFileName = txtSearchFileName.Text.Trim() != "" ? txtSearchFileName.Text : "";
                    string searchChecksheetFileName = txtSearchChecksheetFileName.Text.Trim() != "" ? txtSearchChecksheetFileName.Text : "";
                    string installLocationSheetFileName = txtInstallLocationSheetFileName.Text.Trim() != "" ? txtInstallLocationSheetFileName.Text : "";
                    string localGoTaxFileName = txtLocalGoTaxFileName.Text.Trim() != "" ? txtLocalGoTaxFileName.Text : "";
                    string localGovProveFileName = txtLocalGovProveFileName.Text.Trim() != "" ? txtLocalGovProveFileName.Text : "";
                    string kepElectrLineFileName = txtKepElectrLineFileName.Text.Trim() != "" ? txtKepElectrLineFileName.Text : "";
                    string kepFaucetAcptFileName = txtKepFaucetAcptFileName.Text.Trim() != "" ? txtKepFaucetAcptFileName.Text : "";
                    string electrSafeInspPrintFileName = txtElectrSafeInspPrintFileName.Text.Trim() != "" ? txtElectrSafeInspPrintFileName.Text : "";
                    string electrBeforeUseCheckPrintFileName = txtElectrBeforeUseCheckPrintFileName.Text.Trim() != "" ? txtElectrBeforeUseCheckPrintFileName.Text : "";
                    string electrBeforeUseInspFileName = txtElectrBeforeUseInspFileName.Text.Trim() != "" ? txtElectrBeforeUseInspFileName.Text : "";

                    string electrKepAcptFileName = txtElectrKepAcptFileName.Text.Trim() != "" ? txtElectrKepAcptFileName.Text : "";
                    string electrKepInfraPayBillFileName = txtElectrKepInfraPayBillFileName.Text.Trim() != "" ? txtElectrKepInfraPayBillFileName.Text : "";
                    //string eectrUseContractFileName = txtElectrUseContractFileName.Text.Trim() != "" ? txtElectrUseContractFileName.Text : "";
                    string electrBeforeUseInspCostFileName = txtElectrBeforeUseInspCostFileName.Text.Trim() != "" ? txtElectrBeforeUseInspCostFileName.Text : "";
                    string electrTransCoUseFileName = txtElectrTransCoUseFileName.Text.Trim() != "" ? txtElectrTransCoUseFileName.Text : "";

                    //string electrCoWorkFileName = txtElectrCoWorkFileName.Text.Trim() != "" ? txtElectrCoWorkFileName.Text : "";
                    string electrCostFileName = txtElectrCostFileName.Text.Trim() != "" ? txtElectrCostFileName.Text : "";
                    string superSetCheckFileName = txtSuperSetCheckFileName.Text.Trim() != "" ? txtSuperSetCheckFileName.Text : "";
                    string superBeforeUseInspectFileName = txtSuperBeforeUseInspectFileName.Text.Trim() != "" ? txtSuperBeforeUseInspectFileName.Text : "";
                    //string superCostFileFileName = txtSuperCostFileName.Text.Trim() != "" ? txtSuperCostFileName.Text : "";
                    string safeManagerCertiFileName = txtSafeManagerCertiFileName.Text.Trim() != "" ? txtSafeManagerCertiFileName.Text : "";
                    string superReportFileFileName = txtSuperReportFileName.Text.Trim() != "" ? txtSuperReportFileName.Text : "";
                    string compReportFileName = txtCompReportFileName.Text.Trim() != "" ? txtCompReportFileName.Text : "";
                    string insurePrintFileName = txtInsurePrintFileName.Text.Trim() != "" ? txtInsurePrintFileName.Text : "";

                    string sketch1 = txtSketch1.Text.Trim() != "" ? txtSketch1.Text : "";
                    string sketch2 = txtSketch2.Text.Trim() != "" ? txtSketch2.Text : "";
                    string sketch3 = txtSketch3.Text.Trim() != "" ? txtSketch3.Text : "";
                    string sketch4 = txtSketch4.Text.Trim() != "" ? txtSketch4.Text : "";
                    string sketch5 = txtSketch5.Text.Trim() != "" ? txtSketch5.Text : "";
                    string sketch6 = txtSketch6.Text.Trim() != "" ? txtSketch6.Text : "";
                    string sketch7 = txtSketch7.Text.Trim() != "" ? txtSketch7.Text : "";
                    string sketch8 = txtSketch8.Text.Trim() != "" ? txtSketch8.Text : "";
                    string sketch9 = txtSketch9.Text.Trim() != "" ? txtSketch9.Text : "";
                    string sketch10 = txtSketch10.Text.Trim() != "" ? txtSketch10.Text : "";
                    string sketch11 = txtSketch11.Text.Trim() != "" ? txtSketch11.Text : "";
                    string sketch12 = txtSketch12.Text.Trim() != "" ? txtSketch12.Text : "";

                    string sketch13 = txtSketch13.Text.Trim() != "" ? txtSketch13.Text : "";
                    string sketch14 = txtSketch14.Text.Trim() != "" ? txtSketch14.Text : "";
                    string sketch15 = txtSketch15.Text.Trim() != "" ? txtSketch15.Text : "";
                    string sketch16 = txtSketch16.Text.Trim() != "" ? txtSketch16.Text : "";
                    string sketch17 = txtSketch17.Text.Trim() != "" ? txtSketch17.Text : "";
                    string sketch18 = txtSketch18.Text.Trim() != "" ? txtSketch18.Text : "";
                    string sketch19 = txtSketch19.Text.Trim() != "" ? txtSketch19.Text : "";
                    string sketch20 = txtSketch20.Text.Trim() != "" ? txtSketch20.Text : "";
                    string sketch21 = txtSketch21.Text.Trim() != "" ? txtSketch21.Text : "";
                    string sketch22 = txtSketch22.Text.Trim() != "" ? txtSketch22.Text : "";



                    string btnAccntDown = string.Empty;
                    if(dgdAccnt.SelectedCells.Count > 0)
                    {
                        var cell = dgdAccnt.SelectedCells[dgdAccnt.SelectedCells.Count - 1];
                        btnAccntDown = (cell.Item as Win_order_Order_U_CodView_dgdAccnt)?.column5FileName ?? "";
                    }

                    

                    if (((ClickPoint == "Contract") && (txtContractFileName.Text == string.Empty))
                       || ((ClickPoint == "BeforeSearchConsult") && (txtBeforeSearchConsultFileName.Text == string.Empty))
                       || ((ClickPoint == "PictureEarth") && (txtPictureEarthFileName.Text == string.Empty))
                       || ((ClickPoint == "Draw") && (txtDrawFileName.Text == string.Empty))
                       || ((ClickPoint == "Search") && (txtSearchFileName.Text == string.Empty))
                       || ((ClickPoint == "SearchChecksheet") && (txtSearchChecksheetFileName.Text == string.Empty))
                       || ((ClickPoint == "InstallLocationSheet") && (txtInstallLocationSheetFileName.Text == string.Empty))
                       || ((ClickPoint == "LocalGoTax") && (txtLocalGoTaxFileName.Text == string.Empty))
                       || ((ClickPoint == "LocalGovProve") && (txtLocalGovProveFileName.Text == string.Empty))
                       || ((ClickPoint == "kepElectrLine") && (txtKepElectrLineFileName.Text == string.Empty))
                       || ((ClickPoint == "kepFaucetAcpt") && (txtKepFaucetAcptFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrSafeInspPrint") && (txtElectrSafeInspPrintFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrBeforeUseCheckPrint") && (txtElectrBeforeUseCheckPrintFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrBeforeUseInsp") && (txtElectrBeforeUseInspFileName.Text == string.Empty))

                       || ((ClickPoint == "ElectrKepAcpt") && (txtElectrKepAcptFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrKepInfraPayBill") && (txtElectrKepInfraPayBillFileName.Text == string.Empty))
                       //|| ((ClickPoint == "ElectrUseContract") && (txtElectrUseContractFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrBeforeUseInspCost") && (txtElectrBeforeUseInspCostFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrTransCoUse") && (txtElectrTransCoUseFileName.Text == string.Empty))


                       //|| ((ClickPoint == "ElectrCoWork") && (txtElectrCoWorkFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrCost") && (txtElectrCostFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperSetCheck") && (txtSuperSetCheckFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperBeforeUseInspect") && (txtSuperBeforeUseInspectFileName.Text == string.Empty))
                       //|| ((ClickPoint == "SuperCostFile") && (txtSuperCostFileName.Text == string.Empty))
                       || ((ClickPoint == "SafeManagerCerti") && (txtSafeManagerCertiFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperReportFile") && (txtSuperReportFileName.Text == string.Empty))
                       || ((ClickPoint == "CompReport") && (txtCompReportFileName.Text == string.Empty))
                       || ((ClickPoint == "InsurePrint") && (txtInsurePrintFileName.Text == string.Empty))

                       || ((ClickPoint == "btnSketch1") && (txtSketch1.Text == string.Empty))
                       || ((ClickPoint == "btnSketch2") && (txtSketch2.Text == string.Empty))
                       || ((ClickPoint == "btnSketch3") && (txtSketch3.Text == string.Empty))
                       || ((ClickPoint == "btnSketch4") && (txtSketch4.Text == string.Empty))
                       || ((ClickPoint == "btnSketch5") && (txtSketch5.Text == string.Empty))
                       || ((ClickPoint == "btnSketch6") && (txtSketch6.Text == string.Empty))
                       || ((ClickPoint == "btnSketch7") && (txtSketch7.Text == string.Empty))
                       || ((ClickPoint == "btnSketch8") && (txtSketch8.Text == string.Empty))
                       || ((ClickPoint == "btnSketch9") && (txtSketch9.Text == string.Empty))
                       || ((ClickPoint == "btnSketch10") && (txtSketch10.Text == string.Empty))
                       || ((ClickPoint == "btnSketch11") && (txtSketch11.Text == string.Empty))
                       || ((ClickPoint == "btnSketch12") && (txtSketch12.Text == string.Empty))

                       || ((ClickPoint == "btnSketch13") && (txtSketch13.Text == string.Empty))
                       || ((ClickPoint == "btnSketch14") && (txtSketch14.Text == string.Empty))
                       || ((ClickPoint == "btnSketch15") && (txtSketch15.Text == string.Empty))
                       || ((ClickPoint == "btnSketch16") && (txtSketch16.Text == string.Empty))
                       || ((ClickPoint == "btnSketch17") && (txtSketch17.Text == string.Empty))
                       || ((ClickPoint == "btnSketch18") && (txtSketch18.Text == string.Empty))
                       || ((ClickPoint == "btnSketch19") && (txtSketch19.Text == string.Empty))
                       || ((ClickPoint == "btnSketch20") && (txtSketch20.Text == string.Empty))
                       || ((ClickPoint == "btnSketch21") && (txtSketch21.Text == string.Empty))
                       || ((ClickPoint == "btnSketch22") && (txtSketch22.Text == string.Empty))


                       || ((ClickPoint == "AccntDown") && (btnAccntDown == string.Empty)))


                    {
                        MessageBox.Show("파일이 없습니다.");
                        return;
                    }


                    try
                    {
                        // 접속 경로
                        _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                        string str_path = string.Empty;
                        str_path = FTP_ADDRESS + '/' + txtOrderID.Text;
                        _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        //원격경로
                        if (ClickPoint == "Contract") { str_remotepath = contractFileName; }
                        else if (ClickPoint == "BeforeSearchConsult") { str_remotepath = beforeSearchConsultFileName; }
                        else if (ClickPoint == "PictureEarth") { str_remotepath = pictureEarthFileName; }
                        else if (ClickPoint == "Draw") { str_remotepath = drawFileName; }
                        else if (ClickPoint == "Search") { str_remotepath = searchFileName; }
                        else if (ClickPoint == "SearchChecksheet") { str_remotepath = searchChecksheetFileName; }
                        else if (ClickPoint == "InstallLocationSheet") { str_remotepath = installLocationSheetFileName; }
                        else if (ClickPoint == "LocalGoTax") { str_remotepath = localGoTaxFileName; }
                        else if (ClickPoint == "LocalGovProve") { str_remotepath = localGovProveFileName; }
                        else if (ClickPoint == "kepElectrLine") { str_remotepath = kepElectrLineFileName; }
                        else if (ClickPoint == "kepFaucetAcpt") { str_remotepath = kepFaucetAcptFileName; }
                        else if (ClickPoint == "ElectrSafeInspPrint") { str_remotepath = electrSafeInspPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseCheckPrint") { str_remotepath = electrBeforeUseCheckPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInsp") { str_remotepath = electrBeforeUseInspFileName; }
                        
                        else if (ClickPoint == "ElectrKepAcpt") { str_remotepath = electrKepAcptFileName; }
                        else if (ClickPoint == "ElectrKepInfraPayBill") { str_remotepath = electrKepInfraPayBillFileName; }
                        //else if (ClickPoint == "ElectrUseContract") { str_remotepath = eectrUseContractFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInspCost") { str_remotepath = electrBeforeUseInspCostFileName; }
                        else if (ClickPoint == "ElectrTransCoUse") { str_remotepath = electrTransCoUseFileName; }

                        //else if (ClickPoint == "ElectrCoWork") { str_remotepath = electrCoWorkFileName; }
                        else if (ClickPoint == "ElectrCost") { str_remotepath = electrCostFileName; }
                        else if (ClickPoint == "SuperSetCheck") { str_remotepath = superSetCheckFileName; }
                        else if (ClickPoint == "SuperBeforeUseInspect") { str_remotepath = superBeforeUseInspectFileName; }
                        //else if (ClickPoint == "SuperCostFile") { str_remotepath = superCostFileFileName; }
                        else if (ClickPoint == "SafeManagerCerti") { str_remotepath = safeManagerCertiFileName; }
                        else if (ClickPoint == "SuperReportFile") { str_remotepath = superReportFileFileName; }
                        else if (ClickPoint == "CompReport") { str_remotepath = compReportFileName; }
                        else if (ClickPoint == "InsurePrint") { str_remotepath = insurePrintFileName; }

                        else if (ClickPoint == "btnSketch1") { str_remotepath = sketch1; }
                        else if (ClickPoint == "btnSketch2") { str_remotepath = sketch2; }
                        else if (ClickPoint == "btnSketch3") { str_remotepath = sketch3; }
                        else if (ClickPoint == "btnSketch4") { str_remotepath = sketch4; }
                        else if (ClickPoint == "btnSketch5") { str_remotepath = sketch5; }
                        else if (ClickPoint == "btnSketch6") { str_remotepath = sketch6; }
                        else if (ClickPoint == "btnSketch7") { str_remotepath = sketch7; }
                        else if (ClickPoint == "btnSketch8") { str_remotepath = sketch8; }
                        else if (ClickPoint == "btnSketch9") { str_remotepath = sketch9; }
                        else if (ClickPoint == "btnSketch10") { str_remotepath = sketch10; }
                        else if (ClickPoint == "btnSketch11") { str_remotepath = sketch11; }
                        else if (ClickPoint == "btnSketch12") { str_remotepath = sketch12; }

                        else if (ClickPoint == "btnSketch13") { str_remotepath = sketch13; }
                        else if (ClickPoint == "btnSketch14") { str_remotepath = sketch14; }
                        else if (ClickPoint == "btnSketch15") { str_remotepath = sketch15; }
                        else if (ClickPoint == "btnSketch16") { str_remotepath = sketch16; }
                        else if (ClickPoint == "btnSketch17") { str_remotepath = sketch17; }
                        else if (ClickPoint == "btnSketch18") { str_remotepath = sketch18; }
                        else if (ClickPoint == "btnSketch19") { str_remotepath = sketch19; }
                        else if (ClickPoint == "btnSketch20") { str_remotepath = sketch20; }
                        else if (ClickPoint == "btnSketch21") { str_remotepath = sketch21; }
                        else if (ClickPoint == "btnSketch22") { str_remotepath = sketch22; }


                        else if (ClickPoint == "AccntDown") { str_remotepath = btnAccntDown; }

                        //로컬경로
                        if (ClickPoint == "Contract") { str_localpath = LOCAL_DOWN_PATH + "\\" + contractFileName; }
                        else if (ClickPoint == "BeforeSearchConsult") { str_localpath = LOCAL_DOWN_PATH + "\\" + beforeSearchConsultFileName; }
                        else if (ClickPoint == "PictureEarth") { str_localpath = LOCAL_DOWN_PATH + "\\" + pictureEarthFileName; }
                        else if (ClickPoint == "Draw") { str_localpath = LOCAL_DOWN_PATH + "\\" + drawFileName; }
                        else if (ClickPoint == "Search") { str_localpath = LOCAL_DOWN_PATH + "\\" + searchFileName; }
                        else if (ClickPoint == "SearchChecksheet") { str_localpath = LOCAL_DOWN_PATH + "\\" + searchChecksheetFileName; }
                        else if (ClickPoint == "InstallLocationSheet") { str_localpath = LOCAL_DOWN_PATH + "\\" + installLocationSheetFileName; }
                        else if (ClickPoint == "LocalGoTax") { str_localpath = LOCAL_DOWN_PATH + "\\" + localGoTaxFileName; }
                        else if (ClickPoint == "LocalGovProve") { str_localpath = LOCAL_DOWN_PATH + "\\" + localGovProveFileName; }
                        else if (ClickPoint == "kepElectrLine") { str_localpath = LOCAL_DOWN_PATH + "\\" + kepElectrLineFileName; }
                        else if (ClickPoint == "kepFaucetAcpt") { str_localpath = LOCAL_DOWN_PATH + "\\" + kepFaucetAcptFileName; }
                        else if (ClickPoint == "ElectrSafeInspPrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrSafeInspPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseCheckPrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrBeforeUseCheckPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInsp") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrBeforeUseInspFileName; }

                        else if (ClickPoint == "ElectrKepAcpt") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrKepAcptFileName; }
                        else if (ClickPoint == "ElectrKepInfraPayBill") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrKepInfraPayBillFileName; }
                        //else if (ClickPoint == "ElectrUseContract") { str_localpath = LOCAL_DOWN_PATH + "\\" + eectrUseContractFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInspCost") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrBeforeUseInspCostFileName; }
                        else if (ClickPoint == "ElectrTransCoUse") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrTransCoUseFileName; }

                        //else if (ClickPoint == "ElectrCoWork") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrCoWorkFileName; }
                        else if (ClickPoint == "ElectrCost") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrCostFileName; }
                        else if (ClickPoint == "SuperSetCheck") { str_localpath = LOCAL_DOWN_PATH + "\\" + superSetCheckFileName; }
                        else if (ClickPoint == "SuperBeforeUseInspect") { str_localpath = LOCAL_DOWN_PATH + "\\" + superBeforeUseInspectFileName; }
                        //else if (ClickPoint == "SuperCostFile") { str_localpath = LOCAL_DOWN_PATH + "\\" + superCostFileFileName; }
                        else if (ClickPoint == "SafeManagerCerti") { str_localpath = LOCAL_DOWN_PATH + "\\" + safeManagerCertiFileName; }
                        else if (ClickPoint == "SuperReportFile") { str_localpath = LOCAL_DOWN_PATH + "\\" + superReportFileFileName; }
                        else if (ClickPoint == "CompReport") { str_localpath = LOCAL_DOWN_PATH + "\\" + compReportFileName; }
                        else if (ClickPoint == "InsurePrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + insurePrintFileName; }

                        else if (ClickPoint == "btnSketch1") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch1; }
                        else if (ClickPoint == "btnSketch2") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch2; }
                        else if (ClickPoint == "btnSketch3") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch3; }
                        else if (ClickPoint == "btnSketch4") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch4; }
                        else if (ClickPoint == "btnSketch5") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch5; }
                        else if (ClickPoint == "btnSketch6") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch6; }
                        else if (ClickPoint == "btnSketch7") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch7; }
                        else if (ClickPoint == "btnSketch8") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch8; }
                        else if (ClickPoint == "btnSketch9") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch9; }
                        else if (ClickPoint == "btnSketch10") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch10; }
                        else if (ClickPoint == "btnSketch11") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch11; }
                        else if (ClickPoint == "btnSketch12") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch12; }

                        else if (ClickPoint == "btnSketch13") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch13; }
                        else if (ClickPoint == "btnSketch14") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch14; }
                        else if (ClickPoint == "btnSketch15") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch15; }
                        else if (ClickPoint == "btnSketch16") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch16; }
                        else if (ClickPoint == "btnSketch17") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch17; }
                        else if (ClickPoint == "btnSketch18") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch18; }
                        else if (ClickPoint == "btnSketch19") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch19; }
                        else if (ClickPoint == "btnSketch20") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch20; }
                        else if (ClickPoint == "btnSketch21") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch21; }
                        else if (ClickPoint == "btnSketch22") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch22; }


                        else if (ClickPoint == "AccntDown") { str_localpath = LOCAL_DOWN_PATH + "\\" + btnAccntDown; }


                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                        if (DI.Exists == false)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        //if (file.Exists)
                        //{
                        //    file.Delete();
                        //}
                        try
                        {
                            file.Delete();
                        }
                        catch (IOException)
                        {
                            // 파일명과 확장자 분리
                            string directory = Path.GetDirectoryName(str_localpath);
                            string fileName = Path.GetFileNameWithoutExtension(str_localpath);
                            string extension = Path.GetExtension(str_localpath);

                            // 복사본 파일명 생성 (예: test.hwp -> test - 복사본.hwp)
                            int copyNum = 1;
                            string newPath = Path.Combine(directory, $"{fileName} - 복사본{extension}");

                            // 복사본 파일이 이미 존재하면 번호 추가 (예: test - 복사본 (2).hwp)
                            while (File.Exists(newPath))
                            {
                                copyNum++;
                                newPath = Path.Combine(directory, $"{fileName} - 복사본 ({copyNum}){extension}");
                            }

                            str_localpath = newPath; // 새로운 경로로 업데이트
                            MessageBox.Show("파일이 사용 중이어서 복사본으로 다운로드 했습니다.", "알림");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("파일 처리 중 오류가 발생했습니다: " + ex.Message);
                            return;
                        }

                        _ftp.download(str_remotepath, str_localpath);

                        //파일 다운로드 후 바로 열기
                        if (File.Exists(str_localpath)&& msgresult == MessageBoxResult.Yes)
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
                        else if((File.Exists(str_localpath) && msgresult == MessageBoxResult.No))
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
        }


        private void btnFileDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 삭제 하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                string ClickPoint = ((Button)sender).Tag.ToString();
                string fileName = string.Empty;

                string btnAccntDown = string.Empty;
                DataGridCellInfo cell;
                if (dgdAccnt.SelectedCells.Count > 0)
                {
                    cell = dgdAccnt.SelectedCells[dgdAccnt.SelectedCells.Count - 1];
                    btnAccntDown = (cell.Item as Win_order_Order_U_CodView_dgdAccnt).column5FileName;
                }

                //먼저 클릭한 버튼의 파일명을 삭제할 파일 리스트에 올린다. 리스트에 올리면서 텍스트의 텍스트와 태그를 지운다.
                //lstFileName에는 ftp업로드할때 파일명 중복방지를 위한 리스트(파일명이 중복되면 파일이 업로드 되지 않고 삭제될때 문제생김)
                //저장할때 리스트에 있다면 FTP삭제 요청을 한다.
                if ((ClickPoint == "BeforeSearchConsult") && (txtBeforeSearchConsultFileName.Text != string.Empty)) { fileName = txtBeforeSearchConsultFileName.Text; FileDeleteAndTextBoxEmpty(txtBeforeSearchConsultFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Contract") && (txtContractFileName.Text != string.Empty)) { fileName = txtContractFileName.Text; FileDeleteAndTextBoxEmpty(txtContractFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "PictureEarth") && (txtPictureEarthFileName.Text != string.Empty)) { fileName = txtPictureEarthFileName.Text; FileDeleteAndTextBoxEmpty(txtPictureEarthFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Draw") && (txtDrawFileName.Text != string.Empty)) { fileName = txtDrawFileName.Text; FileDeleteAndTextBoxEmpty(txtDrawFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Search") && (txtSearchFileName.Text != string.Empty)) { fileName = txtSearchFileName.Text; FileDeleteAndTextBoxEmpty(txtSearchFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SearchChecksheet") && (txtSearchChecksheetFileName.Text != string.Empty)) { fileName = txtSearchChecksheetFileName.Text; FileDeleteAndTextBoxEmpty(txtSearchChecksheetFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "InstallLocationSheet") && (txtInstallLocationSheetFileName.Text != string.Empty)) { fileName = txtInstallLocationSheetFileName.Text; FileDeleteAndTextBoxEmpty(txtInstallLocationSheetFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "LocalGoTax") && (txtLocalGoTaxFileName.Text != string.Empty)) { fileName = txtLocalGoTaxFileName.Text; FileDeleteAndTextBoxEmpty(txtLocalGoTaxFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "LocalGovProve") && (txtLocalGovProveFileName.Text != string.Empty)) { fileName = txtLocalGovProveFileName.Text; FileDeleteAndTextBoxEmpty(txtLocalGovProveFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "kepElectrLine") && (txtKepElectrLineFileName.Text != string.Empty)) { fileName = txtKepElectrLineFileName.Text; FileDeleteAndTextBoxEmpty(txtKepElectrLineFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "kepFaucetAcpt") && (txtKepFaucetAcptFileName.Text != string.Empty)) { fileName = txtKepFaucetAcptFileName.Text; FileDeleteAndTextBoxEmpty(txtKepFaucetAcptFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrSafeInspPrint") && (txtElectrSafeInspPrintFileName.Text != string.Empty)) { fileName = txtElectrSafeInspPrintFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrSafeInspPrintFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrBeforeUseCheckPrint") && (txtElectrBeforeUseCheckPrintFileName.Text != string.Empty)) { fileName = txtElectrBeforeUseCheckPrintFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrBeforeUseCheckPrintFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrBeforeUseInsp") && (txtElectrBeforeUseInspFileName.Text != string.Empty)) { fileName = txtElectrBeforeUseInspFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrBeforeUseInspFileName); lstFilesName.Remove(fileName); }

                else if ((ClickPoint == "ElectrKepAcpt") && (txtElectrKepAcptFileName.Text != string.Empty)) { fileName = txtElectrKepAcptFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrKepAcptFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrKepInfraPayBill") && (txtElectrKepInfraPayBillFileName.Text != string.Empty)) { fileName = txtElectrKepInfraPayBillFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrKepInfraPayBillFileName); lstFilesName.Remove(fileName); }
                //else if ((ClickPoint == "ElectrUseContract") && (txtElectrUseContractFileName.Text != string.Empty)) { fileName = txtElectrUseContractFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrUseContractFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrBeforeUseInspCost") && (txtElectrBeforeUseInspCostFileName.Text != string.Empty)) { fileName = txtElectrBeforeUseInspCostFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrBeforeUseInspCostFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrTransCoUse") && (txtElectrTransCoUseFileName.Text != string.Empty)) { fileName = txtElectrTransCoUseFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrTransCoUseFileName); lstFilesName.Remove(fileName); }

                //else if ((ClickPoint == "ElectrCoWork") && (txtElectrCoWorkFileName.Text != string.Empty)) { fileName = txtElectrCoWorkFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrCoWorkFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrCost") && (txtElectrCostFileName.Text != string.Empty)) { fileName = txtElectrCostFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrCostFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperSetCheck") && (txtSuperSetCheckFileName.Text != string.Empty)) { fileName = txtSuperSetCheckFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperSetCheckFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperBeforeUseInspect") && (txtSuperBeforeUseInspectFileName.Text != string.Empty)) { fileName = txtSuperBeforeUseInspectFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperBeforeUseInspectFileName); lstFilesName.Remove(fileName); }
                //else if ((ClickPoint == "SuperCostFile") && (txtSuperCostFileName.Text != string.Empty)) { fileName = txtSuperCostFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperCostFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SafeManagerCerti") && (txtSafeManagerCertiFileName.Text != string.Empty)) { fileName = txtSafeManagerCertiFileName.Text; FileDeleteAndTextBoxEmpty(txtSafeManagerCertiFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperReportFile") && (txtSuperReportFileName.Text != string.Empty)) { fileName = txtSuperReportFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperReportFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "CompReport") && (txtCompReportFileName.Text != string.Empty)) { fileName = txtCompReportFileName.Text; FileDeleteAndTextBoxEmpty(txtCompReportFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "InsurePrint") && (txtInsurePrintFileName.Text != string.Empty)) { fileName = txtInsurePrintFileName.Text; FileDeleteAndTextBoxEmpty(txtInsurePrintFileName); lstFilesName.Remove(fileName); }

                else if ((ClickPoint == "btnSketch1") && (txtSketch1.Text != string.Empty)) { fileName = txtSketch1.Text; txtSketch1FileAlias.IsReadOnly = true; txtSketch1FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch1); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch2") && (txtSketch2.Text != string.Empty)) { fileName = txtSketch2.Text; txtSketch2FileAlias.IsReadOnly = true; txtSketch2FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch2); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch3") && (txtSketch3.Text != string.Empty)) { fileName = txtSketch3.Text; txtSketch3FileAlias.IsReadOnly = true; txtSketch3FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch3); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch4") && (txtSketch4.Text != string.Empty)) { fileName = txtSketch4.Text; txtSketch4FileAlias.IsReadOnly = true; txtSketch4FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch4); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch5") && (txtSketch5.Text != string.Empty)) { fileName = txtSketch5.Text; txtSketch5FileAlias.IsReadOnly = true; txtSketch5FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch5); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch6") && (txtSketch6.Text != string.Empty)) { fileName = txtSketch6.Text; txtSketch6FileAlias.IsReadOnly = true; txtSketch6FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch6); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch7") && (txtSketch7.Text != string.Empty)) { fileName = txtSketch7.Text; txtSketch7FileAlias.IsReadOnly = true; txtSketch7FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch7); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch8") && (txtSketch8.Text != string.Empty)) { fileName = txtSketch8.Text; txtSketch8FileAlias.IsReadOnly = true; txtSketch8FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch8); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch9") && (txtSketch9.Text != string.Empty)) { fileName = txtSketch9.Text; txtSketch9FileAlias.IsReadOnly = true; txtSketch9FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch9); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch10") && (txtSketch10.Text != string.Empty)) { fileName = txtSketch10.Text; txtSketch10FileAlias.IsReadOnly = false; txtSketch10FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch10); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch11") && (txtSketch11.Text != string.Empty)) { fileName = txtSketch11.Text; txtSketch11FileAlias.IsReadOnly = false; txtSketch11FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch11); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch12") && (txtSketch12.Text != string.Empty)) { fileName = txtSketch12.Text; txtSketch12FileAlias.IsReadOnly = false; txtSketch12FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch12); lstFilesName.Remove(fileName); }

                else if ((ClickPoint == "btnSketch13") && (txtSketch13.Text != string.Empty)) { fileName = txtSketch13.Text; txtSketch13FileAlias.IsReadOnly = false; txtSketch13FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch13); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch14") && (txtSketch14.Text != string.Empty)) { fileName = txtSketch14.Text; txtSketch14FileAlias.IsReadOnly = false; txtSketch14FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch14); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch15") && (txtSketch15.Text != string.Empty)) { fileName = txtSketch15.Text; txtSketch15FileAlias.IsReadOnly = false; txtSketch15FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch15); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch16") && (txtSketch16.Text != string.Empty)) { fileName = txtSketch16.Text; txtSketch16FileAlias.IsReadOnly = false; txtSketch16FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch16); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch17") && (txtSketch17.Text != string.Empty)) { fileName = txtSketch17.Text; txtSketch17FileAlias.IsReadOnly = false; txtSketch17FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch17); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch18") && (txtSketch18.Text != string.Empty)) { fileName = txtSketch18.Text; txtSketch18FileAlias.IsReadOnly = false; txtSketch18FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch18); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch19") && (txtSketch19.Text != string.Empty)) { fileName = txtSketch19.Text; txtSketch19FileAlias.IsReadOnly = false; txtSketch19FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch19); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch20") && (txtSketch20.Text != string.Empty)) { fileName = txtSketch20.Text; txtSketch20FileAlias.IsReadOnly = false; txtSketch20FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch20); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch21") && (txtSketch21.Text != string.Empty)) { fileName = txtSketch21.Text; txtSketch21FileAlias.IsReadOnly = false; txtSketch21FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch21); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "btnSketch22") && (txtSketch22.Text != string.Empty)) { fileName = txtSketch22.Text; txtSketch22FileAlias.IsReadOnly = false; txtSketch22FileAlias.Text = string.Empty; FileDeleteAndTextBoxEmpty(txtSketch22); lstFilesName.Remove(fileName); }


                else if ((ClickPoint == "AccntDelete") && (btnAccntDown != string.Empty))
                {
                    fileName = btnAccntDown;
                    // 임시 TextBox를 생성하고 값을 복사
                    TextBox tempTextBox = new TextBox();
                    tempTextBox.Text = btnAccntDown;
                    FileDeleteAndTextBoxEmpty(tempTextBox);
                    lstFilesName.Remove(fileName);

                    if (cell.Item != null)
                    {
                        var item = cell.Item as Win_order_Order_U_CodView_dgdAccnt;
                        if (item != null)
                        {
                            item.column5FileName = string.Empty;
                            item.column4FilePath = string.Empty;
                        }
                    }
                }




            }




            // 보기 버튼체크
            //btnImgSeeCheckAndSetting();
        }
        private void FileDeleteAndTextBoxEmpty(TextBox txt)
        {
            if (strFlag.Equals("U"))
            {
                //var Article = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_Nadaum;

                //if (Article != null)
                //{
                    //FTP_RemoveFile(Article.ArticleID + "/" + txt.Text);

                    // 파일이름, 파일경로
                    string[] strFtp = { txt.Text, txt.Tag != null ? txt.Tag.ToString() : "" };

                    deleteListFtpFile.Add(strFtp);
                //}
            }

            txt.Text = string.Empty;
            txt.Tag = string.Empty;
        }

        //FTP 복사
        private bool FTP_copyFiles(List<string> files)
        {
            //INI의 고정된 주소를 넘김
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            bool result = _ftp.FTP_copyFiles(LoadINI.FtpImagePath + "/Estimate/" + txtEstID.Text, PrimaryKey.Trim(), files);

            return result;
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


        private void PrintWork(bool preview_click)
        {
            //Lib lib2 = new Lib();

            //try
            //{
            //    excelapp = new Microsoft.Office.Interop.Excel.Application();

            //    string MyBookPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Report\\수주진행현황(영업관리).xls";
            //    //MyBookPath = MyBookPath.Substring(0, MyBookPath.LastIndexOf("\\")) + "\\order_standard.xls";
            //    //string MyBookPath = "C:/Users/Administrator/Desktop/order_standard.xls";
            //    workbook = excelapp.Workbooks.Add(MyBookPath);
            //    worksheet = workbook.Sheets["Form"];

            //    //상단의 일자 
            //    if (chkOrderDay.IsChecked == true)
            //    {
            //        workrange = worksheet.get_Range("E2", "Q2");//셀 범위 지정
            //        workrange.Value2 = dtpSDate.Text + "~" + dtpEDate.Text;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        //workrange.Font.Size = 10;
            //    }
            //    else
            //    {
            //        workrange = worksheet.get_Range("E2", "K2");//셀 범위 지정
            //        workrange.Value2 = "전체"; //"" + "~" + "";
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        //workrange.Font.Size = 10;
            //    }


            //    ////오더번호 혹은 관리번호 
            //    //if (rbnOrderNo.IsChecked == true)
            //    //{
            //    //    workrange = worksheet.get_Range("C5", "F5");//셀 범위 지정
            //    //    workrange.Value2 = "오더번호";
            //    //    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //    //    //workrange.Font.Size = 10;
            //    //}
            //    //else
            //    //{
            //    //    workrange = worksheet.get_Range("C5", "F5");//셀 범위 지정
            //    //    workrange.Value2 = "관리번호";
            //    //    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //    //    //workrange.Font.Size = 10;
            //    //}

            //    //하단의 회사명
            //    workrange = worksheet.get_Range("AN35", "AU35");//셀 범위 지정
            //    workrange.Value2 = "주식회사 지엘에스";
            //    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //    workrange.Font.Size = 11;


            //    /////////////////////////
            //    int Page = 0;
            //    int DataCount = 0;
            //    int copyLine = 0;

            //    copysheet = workbook.Sheets["Form"];
            //    pastesheet = workbook.Sheets["Print"];

            //    DT = lib2.DataGirdToDataTable(dgdMain);

            //    string str_Num = string.Empty;
            //    string str_OrderID = string.Empty;
            //    string str_OrderID_CV = string.Empty;
            //    string str_KCustom = string.Empty;
            //    string str_Article = string.Empty;
            //    string str_Model = string.Empty;
            //    string str_ArticleNo = string.Empty;
            //    string str_DvlyDate = string.Empty;
            //    string str_Work = string.Empty;
            //    string str_OrderQty = string.Empty;
            //    string str_UnitClssName = string.Empty;
            //    string str_DayAndTime = string.Empty;
            //    string str_p1WorkQty = string.Empty;
            //    string str_InspectQty = string.Empty;
            //    string str_PassQty = string.Empty;
            //    string str_DefectQty = string.Empty;
            //    string str_OutQty = string.Empty;

            //    int TotalCnt = dgdMain.Items.Count;
            //    int canInsert = 27; //데이터가 입력되는 행 수 27개

            //    int PageCount = (int)Math.Ceiling(1.0 * TotalCnt / canInsert);

            //    var Sum = new dgOrderSum();

            //    //while (dgdMain.Items.Count > DataCount + 1)
            //    for (int k = 0; k < PageCount; k++)
            //    {
            //        Page++;
            //        if (Page != 1) { DataCount++; }  //+1
            //        copyLine = (Page - 1) * 38;
            //        copysheet.Select();
            //        copysheet.UsedRange.Copy();
            //        pastesheet.Select();
            //        workrange = pastesheet.Cells[copyLine + 1, 1];
            //        workrange.Select();
            //        pastesheet.Paste();

            //        int j = 0;
            //        for (int i = DataCount; i < dgdMain.Items.Count; i++)
            //        {
            //            if (j == 27) { break; }
            //            int insertline = copyLine + 7 + j;

            //            str_Num = (j + 1).ToString();
            //            str_OrderID = DT.Rows[i][1].ToString();
            //            str_OrderID_CV = DT.Rows[i][2].ToString();
            //            str_KCustom = DT.Rows[i][3].ToString();
            //            str_Article = DT.Rows[i][4].ToString();
            //            str_Model = DT.Rows[i][5].ToString();
            //            str_ArticleNo = DT.Rows[i][6].ToString();
            //            str_DvlyDate = DT.Rows[i][7].ToString();
            //            str_Work = DT.Rows[i][8].ToString();
            //            str_OrderQty = DT.Rows[i][9].ToString();
            //            str_UnitClssName = DT.Rows[i][10].ToString();
            //            str_DayAndTime = DT.Rows[i][11].ToString();
            //            str_p1WorkQty = DT.Rows[i][12].ToString();
            //            str_InspectQty = DT.Rows[i][13].ToString();
            //            str_PassQty = DT.Rows[i][14].ToString();
            //            str_DefectQty = DT.Rows[i][15].ToString();
            //            str_OutQty = DT.Rows[i][16].ToString();

            //            workrange = pastesheet.get_Range("A" + insertline, "B" + insertline);    //순번
            //            workrange.Value2 = str_Num;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.3;

            //            if (dgdtxtOrderID.ToString().Equals("오더번호"))
            //            {
            //                workrange = pastesheet.get_Range("C" + insertline, "F" + insertline);    //오더번호
            //                workrange.Value2 = str_OrderID;
            //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //                workrange.Font.Size = 9;
            //                workrange.ColumnWidth = 1.8;
            //            }
            //            else
            //            {
            //                workrange = pastesheet.get_Range("C" + insertline, "F" + insertline);    //관리번호
            //                workrange.Value2 = str_OrderID_CV;
            //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //                workrange.Font.Size = 9;
            //                workrange.ColumnWidth = 1.8;
            //            }

            //            workrange = pastesheet.get_Range("G" + insertline, "J" + insertline);     //거래처
            //            workrange.Value2 = str_KCustom;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 9;
            //            workrange.ColumnWidth = 2.7;

            //            workrange = pastesheet.get_Range("K" + insertline, "N" + insertline);    //품명
            //            workrange.Value2 = str_Article;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 2.7;

            //            workrange = pastesheet.get_Range("O" + insertline, "R" + insertline);    //차종
            //            workrange.Value2 = str_Model;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 0.9;

            //            workrange = pastesheet.get_Range("S" + insertline, "V" + insertline);    //품번
            //            workrange.Value2 = str_ArticleNo;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 2.7;

            //            workrange = pastesheet.get_Range("W" + insertline, "Y" + insertline);    //가공구분
            //            workrange.Value2 = str_Work;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.8;

            //            workrange = pastesheet.get_Range("Z" + insertline, "AA" + insertline);    //납기일
            //            workrange.Value2 = str_DvlyDate;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 3.8;

            //            workrange = pastesheet.get_Range("AB" + insertline, "AC" + insertline);    //투입일

            //            if (str_DayAndTime.Length > 5)
            //            {
            //                workrange.Value2 = str_DayAndTime.Substring(0, 5);
            //            }
            //            else
            //            {
            //                workrange.Value2 = str_DayAndTime;
            //            }

            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 3.8;

            //            workrange = pastesheet.get_Range("AD" + insertline, "AF" + insertline);    //수주량
            //            workrange.Value2 = str_OrderQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.7;

            //            workrange = pastesheet.get_Range("AG" + insertline, "AI" + insertline);    //투입량
            //            workrange.Value2 = str_p1WorkQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.2;

            //            workrange = pastesheet.get_Range("AJ" + insertline, "AL" + insertline);    //검사량
            //            workrange.Value2 = str_InspectQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.2;

            //            workrange = pastesheet.get_Range("AM" + insertline, "AO" + insertline);    //합격량
            //            workrange.Value2 = str_PassQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.2;

            //            workrange = pastesheet.get_Range("AP" + insertline, "AR" + insertline);    //불합격량
            //            workrange.Value2 = str_DefectQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.2;

            //            workrange = pastesheet.get_Range("AS" + insertline, "AU" + insertline);    //출고량
            //            workrange.Value2 = str_OutQty;
            //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //            workrange.Font.Size = 10;
            //            workrange.ColumnWidth = 1.2;

            //            DataCount = i;
            //            j++;

            //            // 합계 누적
            //            Sum.OrderSum += ConvertInt(str_OrderQty);
            //            Sum.InsertSum += ConvertInt(str_p1WorkQty);

            //            Sum.InspectSum += ConvertDouble(str_InspectQty);
            //            Sum.PassSum += ConvertDouble(str_PassQty);
            //            Sum.DefectSum += ConvertDouble(str_DefectQty);
            //            Sum.OutSum += ConvertDouble(str_OutQty);


            //        }

            //        // 합계 출력
            //        int totalLine = 34 + ((Page - 1) * 38);

            //        Sum.Count = DataCount + 1;


            //        workrange = pastesheet.get_Range("AB" + totalLine, "AC" + totalLine);    // 건수
            //        workrange.Value2 = Sum.Count + " 건";
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AD" + totalLine, "AF" + totalLine);    // 총 수주량
            //        workrange.Value2 = Sum.OrderSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AG" + totalLine, "AI" + totalLine);    // 총 투입량
            //        workrange.Value2 = Sum.InsertSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AJ" + totalLine, "AL" + totalLine);    // 총 검일시
            //        workrange.Value2 = Sum.InspectSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AM" + totalLine, "AO" + totalLine);    // 총 통과량
            //        workrange.Value2 = Sum.PassSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AP" + totalLine, "AR" + totalLine);    // 총 불합격량
            //        workrange.Value2 = Sum.DefectSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //        workrange = pastesheet.get_Range("AS" + totalLine, "AU" + totalLine);    // 총 출고량
            //        workrange.Value2 = Sum.OutSum;
            //        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //        workrange.Font.Size = 10;

            //    }

            //    pastesheet.PageSetup.TopMargin = 0;
            //    pastesheet.PageSetup.BottomMargin = 0;
            //    //pastesheet.PageSetup.Zoom = 43;

               

            //    if (preview_click == true)
            //    {
            //        excelapp.Visible = true;
            //        pastesheet.PrintPreview();
            //    }
            //    else
            //    {
            //        excelapp.Visible = true;
            //        pastesheet.PrintOutEx();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            //}
            //finally
            //{
            //    lib2.ReleaseExcelObject(workbook);
            //    lib2.ReleaseExcelObject(worksheet);
            //    lib2.ReleaseExcelObject(pastesheet);
            //    lib2.ReleaseExcelObject(excelapp);
            //    lib2 = null;
            //}
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
            DataGrid dgs = sender as DataGrid;
            if (dgs.ColumnHeaderHeight == 0)
            {
                dgs.ColumnHeaderHeight = 1;
            }
            double a = e.NewSize.Height / 100;
            double b = e.PreviousSize.Height / 100;
            double c = a / b;

            if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
            {
                dgs.ColumnHeaderHeight = dgs.ColumnHeaderHeight * c;
                dgs.FontSize = dgs.FontSize * c;
            }
        }

        //멀티헤더 동기화 사용시에 xaml에서 각 열에 minwidth를 지정해야 하는데
        //사용자가 화면 로드했을때 나머지 열이 이쁘게 꽉 찼으면 좋겠다 하여 마지막 열의 width="*"를 하면 해결이 되나
        //스크롤이 생기지 않는 문제가 있음, 아래 코드는 현재 그리드 안의 데이터그리드의 각 열의 minwidth값을 더하고 뺀
        //나머지 공간을 마지막 열에 할당하는 코드임, 사용할때는 xaml에 마지막 열에 넓이를 지정하지 마세요
        private void SetupLastColumnResize(DataGrid dataGrid, ScrollViewer headerScrollViewer, Grid parentGrid)
        {
            dataGrid.SizeChanged += (s, e) =>
            {
                var lastColumn = dataGrid.Columns.Last() as DataGridTemplateColumn;
                if (lastColumn == null) return;

                double otherColumnsWidth = 0;
                foreach (var column in dataGrid.Columns)
                {
                    // Hidden이거나 MaxWidth가 0인 열은 계산에서 제외
                    if (column != lastColumn &&
                        column.Visibility == Visibility.Visible &&
                        column.MaxWidth != 0)
                    {
                        otherColumnsWidth += Math.Max(column.ActualWidth, column.MinWidth);
                    }
                }

                double remainingWidth = Math.Max(lastColumn.MinWidth, parentGrid.ActualWidth - otherColumnsWidth);
                lastColumn.MinWidth = remainingWidth;

                var headerGrid = headerScrollViewer.Content as Grid;
                if (headerGrid?.ColumnDefinitions.Count > 0)
                {
                    headerGrid.ColumnDefinitions[headerGrid.ColumnDefinitions.Count - 1].MinWidth = remainingWidth;
                }
            };
        }

        //수주구분 라벨
        private void LblOrderFlag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderFlag.IsChecked = chkOrderFlag.IsChecked == true ? false : true;
        }

        //수주구분 체크박스 체크
        private void ChkOrderFlag_Checked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = true;
        }

        //수주구분 체크박스 체크 해제
        private void ChkOrderFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = false;
        }

        //매출거래처 
        private void txtInCustom_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //    MainWindow.pf.ReturnCode(txtInCustom, 72, "");
        }

        //매출거래처
        private void btnPfInCustom_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.pf.ReturnCode(txtInCustom, 72, "");
        }
        #endregion keydown 이벤트

        //자재필요량조회
        private void btnNeedStuff_Click(object sender, RoutedEventArgs e)
        {
            //if (txtBuyerArticleNO.Tag == null   )
            //{
            //    MessageBox.Show("먼저 품명을 선택해주세요");
            //    return;
            //}

            //if (txtAmount.Text.Replace(" ", "").Equals(""))
            //{
            //    MessageBox.Show("먼저 총 주문량을 입력해주세요");
            //    return;
            //}

            //자재필요량조회에 필요한 파라미터 값을 넘겨주자, 품명이랑 주문량
            //FillNeedStockQty(txtBuyerArticleNO.Tag.ToString(), txtAmount.Text.Replace(",", ""));
        }


        private void fillGridTab2_LocalGov(string orderId)
        {
             ovcOrder_localGov.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderId);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrderSub_dgdLocalGov", sqlParameter, true, "R");

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
                            var localGovList = new Win_order_Order_U_CodView_localGov
                            {
                                num = i,
                                orderID = dr["orderID"].ToString(),
                                localGovPermissionNo = dr["localGovPermissionNo"].ToString(),
                                localGovBehaviorReportDate = DateTypeHyphen(dr["localGovBehaviorReportDate"].ToString()),
                                localGovSeq = dr["localGovSeq"].ToString(),
                                localGovBehaviorDate = DateTypeHyphen(dr["localGovBehaviorDate"].ToString()),
                                localGovSuppleContext = dr["localGovSuppleContext"].ToString(),
                                localGovSuppleDate = DateTypeHyphen(dr["localGovSuppleDate"].ToString()),
                                localGovComments = dr["localGovComments"].ToString(),

                            };
                            ovcOrder_localGov.Add(localGovList);
                        }

                    }

                    dgdLocalGov.ItemsSource = ovcOrder_localGov;
                }

                //MessageBox.Show("dgdLocalGov count(After) :" + dgdLocalGov.Items.Count.ToString());


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

        //메인 데이터그리드 선택 이벤트
        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                orderID_global = string.Empty;      
                lstFilesName.Clear();
              

                var OrderInfo = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_dgdMain;
                if (OrderInfo != null)
                {
                    rowNum = dgdMain.SelectedIndex;
                    AutoBindDataToControls(OrderInfo, grdInput);
                    //this.DataContext = OrderInfo;

                    orderID_global = OrderInfo.orderId;       
                    fillAccGrid(OrderInfo.orderId);

                    CheckTabClicked();
                    FillTabs(OrderInfo.orderId);

                    addLstFile_FTP();
                }


            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridMain_SelectionChanged : " + ee.ToString());
            }

        }

        private void FillTabs(string orderId)
        {
         
            fillGridTab2(orderId);
            fillGridTab2_LocalGov(orderId);
            tab2_clicked = true;        
          
            fillGridTab3(orderId);
            tab3_clicked = true;        
         
            fillGridTab4(orderId);
            fillGridTab4_Accnt(orderId);
            tab4_clicked=true;
       
            fillgridTab5(orderId);
            tab5_clicked = true;
        
        }

        private void BringLastOrder(string orderId)
        {
            fillAccGrid(orderId);

            fillGridTab2(orderId);
            fillGridTab2_LocalGov(orderId);
            tab2_clicked = true;

            fillGridTab3(orderId);
            tab3_clicked = true;

            fillGridTab4(orderId);
            fillGridTab4_Accnt(orderId);
            tab4_clicked = true;

            fillgridTab5(orderId);
            tab5_clicked = true;

        }

        private void txtBuyerArticle_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void btnPfBuyerArticle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
    
            }
            catch (Exception ex)
            {
                //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void rbnOrderNo_Click(object sender, RoutedEventArgs e)
        {
            tbkOrderSrh.Text = " 발주번호";
            //dgdtxtOrderID.Visibility = Visibility.Hidden;
            //dgdtxtOrderNo.Visibility = Visibility.Visible;
        }

        private void rbnOrderID_Click(object sender, RoutedEventArgs e)
        {
            tbkOrderSrh.Text = " 관리번호";
            //dgdtxtOrderID.Visibility = Visibility.Visible;
            //dgdtxtOrderNo.Visibility = Visibility.Hidden;
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
                chkElecDeliMethSrh.IsChecked = true;
                //cboElecDeliMethSrh.IsEnabled = false;
                txtElecDeliMethSrh.IsEnabled = true;

            }
            else
            {
                chkElecDeliMethSrh.IsChecked = true;
                //cboElecDeliMethSrh.IsEnabled = true;
                txtElecDeliMethSrh.IsEnabled = true;

            }
        }

        //전기조달방법 체크
        private void chkElecDeliMethSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkElecDeliMethSrh.IsChecked == true)
            {
                chkElecDeliMethSrh.IsChecked = true;
                //cboElecDeliMethSrh.IsEnabled = true;
                txtElecDeliMethSrh.IsEnabled = true;
            }
            else
            {
                chkElecDeliMethSrh.IsChecked = false;
                //cboElecDeliMethSrh.IsEnabled = false;
                txtElecDeliMethSrh.IsEnabled = false;

            }
        }


        //상담번호
        private void txtReserveID_KeyDown(object sender, KeyEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtEstID, 5101, txtEstID.Text);
        }

        //상담번호
        private void btnReserveID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtEstID, 5101, txtEstID.Text);
        }


        //그리드별 행추가,삭제 통합관리를 해보자..
        //행추가
        private void btnConAdd_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string xName = button.Name;

            
            if (xName.Contains("AccItem"))
            {
                int num = dgdAcc.Items.Count + 1;
                var dgdAccItem = Win_order_Order_U_CodView_dgdAcc.CreateEmpty_dgdAcc(ovcOrderTypeAcc);
                dgdAccItem.num = num;
                ovcOrder_Acc.Add(dgdAccItem);
            }
            else if (xName.Contains("LocalGov"))
            {
                int num = dgdLocalGov.Items.Count + 1;
                var dgdlocalGovItem = Win_order_Order_U_CodView_localGov.CreateEmpty_localGov();
                dgdlocalGovItem.num = num;
                ovcOrder_localGov.Add(dgdlocalGovItem);
            }
     
            //if (xName.Contains("orderStudent"))
            //{
            //    int num = dgdOrderStudent.Items.Count + 1;
            //    var orderStudent = Win_ord_Order_U_CodeView_OrderStudent_Nadaum.CreateEmpty_OrderStudent();
            //    orderStudent.num = num;
            //    ovcOrder_OrderStudent.Add(orderStudent);
            //}
        }

        private void rowAddAccnt() 
        {

            if (dgdAccnt.Items.Count > 0) dgdAccnt.Items.Clear();        

            int count = 18;
            for (int i = 0; i < count; i++)
            {
                var dgdAccntItem = Win_order_Order_U_CodView_dgdAccnt.CreateEmpty_dgdAccnt_row();
                dgdAccnt.Items.Add(dgdAccntItem);
            }

            dgdAccnt.Items.Refresh();
        }


        //행삭제
        private void btnConDel_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string xName = button.Name;

            #region 기기 및 액서서리
            if (xName.Contains("AccItem"))
            {
                if (dgdAcc.SelectedItem != null)
                {
                    int rowcount = dgdAcc.Items.IndexOf(dgdAcc.SelectedItem);
                    ovcOrder_Acc.RemoveAt(rowcount);

                    if (dgdAcc.Items.Count > 0)
                    {
                        if (dgdAcc.Items.Count - 1 > rowcount)
                        {
                            dgdAcc.SelectedIndex = rowcount;
                        }
                        else
                        {
                            dgdAcc.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    if (dgdAcc.Items.Count > 0)
                    {
                        dgdAcc.SelectedIndex = dgdAcc.Items.Count - 1;

                        btnConDel_Click(button, e);
                    }
                }
            }
            #endregion
            #region 계약 지자체 행위
            else if (xName.Contains("LocalGov"))
            {
                if (dgdLocalGov.SelectedItem != null)
                {
                    int rowcount = dgdLocalGov.Items.IndexOf(dgdLocalGov.SelectedItem);
                    ovcOrder_localGov.RemoveAt(rowcount);

                    if (dgdLocalGov.Items.Count > 0)
                    {
                        if (dgdLocalGov.Items.Count - 1 > rowcount)
                        {
                            dgdLocalGov.SelectedIndex = rowcount;
                        }
                        else
                        {
                            dgdLocalGov.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    if (dgdLocalGov.Items.Count > 0)
                    {
                        dgdLocalGov.SelectedIndex = dgdLocalGov.Items.Count - 1;

                        btnConDel_Click(button, e);
                    }
                }
            }
            #endregion
        }




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
                        case "dgdAcc":
                            nLarge = 5102;
                            break;
                        //case "dgdOrderColor":
                        //    nLarge = 5104;
                            //break;
                    }
                    MainWindow.pf.ReturnCode(textBox, nLarge, "");
                }

                //if (nLarge == 5103 || nLarge == 5104)
                //{
                //    CallArticleData(textBox.Tag.ToString());

                //    var item = currentGrid.CurrentItem;
                //    //var propertyInfo = item.GetType().GetProperty("itemUnitPrice");
                //    //propertyInfo.SetValue(item, articleData.unitPrice);
                //}

            }
        }
        //// 데이터그리드 셀 plusfinder이벤트(더블클릭)
        //private void dgdtpeGetArticleID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    TextBox textBox = sender as TextBox;
        //    int nLarge = 0;

        //    // 부모 DataGrid 찾기
        //    var parent = textBox.Parent;
        //    while (parent != null && !(parent is DataGrid))
        //    {
        //        parent = VisualTreeHelper.GetParent(parent);
        //    }

        //    var currentGrid = parent as DataGrid;
        //    if (currentGrid != null)
        //    {

        //        // 그리드 이름에 따라 다른 타입으로 캐스팅
        //        switch (currentGrid.Name)
        //        {
        //            case "dgdOrdItemList":
        //                nLarge = 5103;
        //                break;
        //            case "dgdOrderColor":
        //                nLarge = 5104;
        //                break;
        //                //case "dgdOrderStudent":
        //                //    ViewReceiver = currentGrid.CurrentItem as Win_ord_Order_U_CodeView_OrderStudent_Nadaum;
        //                //    type = 3;
        //                //    break;
        //        }

        //        MainWindow.pf.ReturnCode(textBox, nLarge, "");
        //    }
        //}

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
                if (currentGrid != null && (currentGrid.Name.Contains("OrdItemList")) || currentGrid.Name.Contains("OrderColor"))
                {
                    var item = currentGrid.CurrentItem;

                    // 수량과 단가 가져오기
                    var qtyProperty = item.GetType().GetProperty("itemQty");
                    var priceProperty = item.GetType().GetProperty("itemUnitPrice");

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

                            var totalProperty = item.GetType().GetProperty("itemAmount");
                            if (totalProperty != null)
                            {
                                totalProperty.SetValue(item, total.ToString());
                            }
                        }
                    }
                }
                else if (currentGrid != null && (currentGrid.Name.Contains("OrderStudent")))
                {
                    var item = currentGrid.CurrentItem;

                    var manPerProperty = item.GetType().GetProperty("manCountPerClass");
                    var classProperty = item.GetType().GetProperty("ban");

                    if ((manPerProperty != null && classProperty != null))
                    {
                        // 실제 값을 먼저 가져옴
                        var manPerValue = manPerProperty.GetValue(item);
                        var classValue = classProperty.GetValue(item);

                        // 둘 다 값이 있는지 확인
                        if (manPerValue != null && !string.IsNullOrEmpty(manPerValue.ToString()) &&
                            classValue != null && !string.IsNullOrEmpty(classValue.ToString()))
                        {

                            decimal manPer = Convert.ToDecimal(manPerProperty.GetValue(item));
                            decimal classQty = Convert.ToDecimal(classProperty.GetValue(item));

                            // 합계 계산
                            decimal total = manPer * classQty;

                            var totalProperty = item.GetType().GetProperty("totalManCount");
                            if (totalProperty != null)
                            {
                                totalProperty.SetValue(item, total.ToString());
                            }
                        }
                    }
                }

            }
        }

        private void btnImgSeeCheckAndSetting()
        {
            if (!txtContractFileName.Text.Trim().Equals(""))
            {
                btnViewContractFile.IsEnabled = true;
            }
            else
            {
                btnViewContractFile.IsEnabled = false;
            }

            //if (!txtContractOkFileName.Text.Trim().Equals(""))
            //{
            //    btnViewContractOkFile.IsEnabled = true;
            //}
            //else
            //{
            //    btnViewContractOkFile.IsEnabled = false;
            //}

            //if (!txtRightOkFileName.Text.Trim().Equals(""))
            //{
            //    btnViewRightOkFile.IsEnabled = true;
            //}
            //else
            //{
            //    btnViewRightOkFile.IsEnabled = false;
            //}

            //if (!txtSexAssaultAcptFileName.Text.Trim().Equals(""))
            //{
            //    btnViewSexAssaultAcptFile.IsEnabled = true;
            //}
            //else
            //{
            //    btnViewSexAssaultAcptFile.IsEnabled = false;
            //}

            //if (!txtCustomjobAcptFileName.Text.Trim().Equals(""))
            //{
            //    btnViewCustomAcptFile.IsEnabled = true;
            //}
            //else
            //{
            //    btnViewCustomAcptFile.IsEnabled = false;
            //}
        }




        //private void btnPreOrder_Click(object sender, RoutedEventArgs e)
        //{
        //    preOrder = new Win_ord_Pop_PreOrder();

        //    if (preOrder.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            var selectedRow = preOrder.SelectedItem;
        //            if (selectedRow != null)
        //            {
        //                string today = DateTime.Today.ToString("yyyyMMdd");

        //                txtOrderNo.Text = selectedRow.orderNo;
        //                txtArticle.Text = selectedRow.article;
        //                txtArticle.Tag = selectedRow.articleID;
        //                txtCustom.Text = selectedRow.kCustom;
        //                txtCustom.Tag = selectedRow.customID;

        //                //dtpOrderDate.SelectedDate = DateTime.ParseExact(selectedRow.orderDate.Trim() != "" ? selectedRow.orderDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                dtpJobFromDate.SelectedDate = DateTime.ParseExact(selectedRow.jobFromDate.Trim() != "" ? selectedRow.jobFromDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                dtpJobToDate.SelectedDate = DateTime.ParseExact(selectedRow.jobToDate.Trim() != "" ? selectedRow.jobToDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                txtComments.Text = selectedRow.comments;
        //                txtDamdangName.Text = selectedRow.damdangName;
        //                txtdamdangDepartName.Text = selectedRow.damdangDepartName;
        //                txtdamdangPositionName.Text = selectedRow.damdangPositionName;
        //                txtdamdangDirPhone.Text = selectedRow.damdangDirPhone;
        //                txtdamdangHandPhone.Text = selectedRow.damdangHandPhone;
        //                txtdamdangEMail.Text = selectedRow.damdangEMail;

        //                cboPictureSubmitYN.SelectedValue = selectedRow.pictureSubmitYN;
        //                cboContractMethodID.SelectedValue = selectedRow.contractMethodID;
        //                cboContractProgressID.SelectedValue = selectedRow.contractProgressID;
        //                txtTaxAmount.Text = selectedRow.taxAmount;
        //                txtOrderAmount.Text = selectedRow.orderAmount;
        //                txtDepositAmount.Text = selectedRow.depositAmount;

        //                dtpSettingCompDate.SelectedDate = DateTime.ParseExact(selectedRow.settingCompDate.Trim() != "" ? selectedRow.settingCompDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                dtpReportSubmitDate.SelectedDate = DateTime.ParseExact(selectedRow.reportSubmitDate.Trim() != "" ? selectedRow.reportSubmitDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                dtpTaxPrintReqDate.SelectedDate = DateTime.ParseExact(selectedRow.taxPrintReqDate.Trim() != "" ? selectedRow.taxPrintReqDate.Replace("-", "") : today, "yyyyMMdd", null);
        //                dtpTaxPrintDate.SelectedDate = DateTime.ParseExact(selectedRow.taxPrintDate.Trim() != "" ? selectedRow.taxPrintDate.Replace("-", "") : today, "yyyyMMdd", null);

        //                cboVatIndYN.SelectedValue = selectedRow.vatIndYN;
        //                cboPriceUnitClss.SelectedValue = selectedRow.priceUnitClss;

        //                dtpDepositDate.SelectedDate = DateTime.ParseExact(selectedRow.depositDate.Trim() != "" ? selectedRow.depositDate.Replace("-", "") : today, "yyyyMMdd", null);

        //                txtCustomjobReqContext.Text = selectedRow.customjobReqContext;
        //                txtMemoContext.Text = selectedRow.memoContext;
        //                txtcustomjobReqFileList.Text = selectedRow.customjobReqFileList;



        //                if (dgdOrdItemList.Items.Count > 0) ovcOrder_OrderItemList.Clear();
        //                if (dgdOrderColor.Items.Count > 0) ovcOrder_OrderColor.Clear();
        //                if(dgdOrderStudent.Items.Count > 0) ovcOrder_OrderStudent.Clear();

        //                fillGridContract(selectedRow.orderID);

        //            }

        //            MessageBox.Show("지난 견적 데이터를 불러 왔습니다.\n(첨부파일 제외)", "확인");
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("과거견적 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
        //        }


        //    }
        //}

        private void Win_ord_Order_U_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (dgdMain.Items.Count > 0)
            //{
            //    var orderInfo = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_Nadaum;

            //    if (orderInfo != null)
            //    {
            //        MainWindow.reServeID = orderInfo.reServeID;
            //    }
            //}
        }

        private void lblCloseYnSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkCloseYnSrh.IsChecked == true)
            {
                chkCloseYnSrh.IsChecked = false;
            }
            else
            {
                chkCloseYnSrh.IsChecked = true;
            }
        }



        private void lblEoAddSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(lblMsg.Visibility != Visibility.Visible)
            {
                if (chkEoAddSrh.IsChecked == true)
                {
                    chkEoAddSrh.IsChecked = false;
                    chkEoAddSrh_Click(null, null);
                }
                else
                {
                    chkEoAddSrh.IsChecked = true;
                    chkEoAddSrh_Click(null, null);
                }
            }
   
        }

        //견적번호(입력그리드) - 텍스트박스
        private void txtEstID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtEstID, 5103, "");
                if(txtEstID.Tag != null && txtEstID.Text.Trim() != string.Empty)
                {
                    boolCallEst = true;
                    txtEstSubject.Text = txtEstID.Text;
                    txtEstID.Text = txtEstID.Tag.ToString();
                    callEstData(txtEstID.Tag.ToString());
                    callEstAccData(txtEstID.Tag.ToString());
                    string DeliCost = GetDeliCost(txtEstID.Tag.ToString());
                    txtMtrShippingCharge.Text = stringFormatN0(DeliCost);
                }


                e.Handled = true;    

            }
        }
        

        //견적번호(입력그리드) - 버튼
        private void btnEstID_Click(object sender, RoutedEventArgs e)
        {
            if(txtEstID.Tag != null && txtEstID.Tag.ToString().Trim() != string.Empty)
            {
                MainWindow.EstID = txtEstID.Text;
            }
            else
            {
                MessageBox.Show("견적번호가 없습니다.");
                return;
            }

            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("견적등록"))
                {
                    break;
                }
                i++;
            }
            try
            {
                if (MainWindow.MainMdiContainer.Children.Contains(MainWindow.mMenulist[i].subProgramID as MdiChild))
                {
                    (MainWindow.mMenulist[i].subProgramID as MdiChild).Focus();
                }
                else
                {
                    Type type = Type.GetType("WizMes_EVC." + MainWindow.mMenulist[i].ProgramID.Trim(), true);
                    object uie = Activator.CreateInstance(type);

                    MainWindow.mMenulist[i].subProgramID = new MdiChild()
                    {
                        Title = "WizMes_EVC_[" + MainWindow.mMenulist[i].MenuID.Trim() + "] " + MainWindow.mMenulist[i].Menu.Trim() +
                                " (→" + MainWindow.mMenulist[i].ProgramID.Trim() + ")",
                        Height = SystemParameters.PrimaryScreenHeight * 0.9,
                        MaxHeight = SystemParameters.PrimaryScreenHeight * 0.95,
                        Width = SystemParameters.WorkArea.Width * 0.9,
                        MaxWidth = SystemParameters.WorkArea.Width,
                        Content = uie as UIElement,
                        Tag = MainWindow.mMenulist[i]
                    };
                    Lib.Instance.AllMenuLogInsert(MainWindow.mMenulist[i].MenuID, MainWindow.mMenulist[i].Menu, MainWindow.mMenulist[i].subProgramID);
                    MainWindow.MainMdiContainer.Children.Add(MainWindow.mMenulist[i].subProgramID as MdiChild);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("해당 화면이 존재하지 않습니다.");
            }

            //if (strFlag != "U")
            //{
            //    preEstimate = new Win_ord_Pop_PreEstimate_Q();

            //    if (preEstimate.ShowDialog() == true)
            //    {
            //        try
            //        {
            //            var selectedRow = preEstimate.SelectedItem;
            //            if (selectedRow != null)
            //            {
            //                string today = DateTime.Today.ToString("yyyyMMdd");
            //                txtEstID.Text = selectedRow.EstID;

            //                txtManagerCustomID.Text = selectedRow.managerCustom;
            //                txtManagerCustomID.Tag = selectedRow.managerCustomID;

            //                txtSalesCustomID.Text = selectedRow.salesCustom;
            //                txtSalesCustomID.Tag = selectedRow.salesCustomID;

            //                dtpContractFromDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchFromDate);
            //                dtpContractToDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchTODate);
            //                dtpOpenReqDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchFromDate);

            //                txtInstallLocation.Text = selectedRow.InstalLocation;
            //                txtInstallLocationPart.Text = selectedRow.InstallLocationPart;

            //                txtDamdangjaName.Text = selectedRow.EstDamdangName;
            //                txtDamdangjaPhone.Text = selectedRow.EstDamdangTelno;
            //                txtInstallLocationAddComments.Text = selectedRow.Comments;

            //                txtMtrAmount.Text = selectedRow.totalAmount;
            //                txtMtrShippingCharge.Text = selectedRow.deliveryCost;

            //                int count = CountEstSub(selectedRow.EstID);


            //            }

            //            MessageBox.Show("견적 데이터를 불러 왔습니다.", "확인");
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show("견적 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
            //        }
            //    }

            //}
            //else
            //{
            //    MessageBox.Show("새로 추가 중에만 사용 할 수 있습니다.");
            //}
        }

        
        private int CountEstSub(string EstID)
        {
            int count = 0;
            string sql = "SELECT cnt = COUNT(*) FROM EST_EstimateSub WHERE EstID =";

            DataSet ds = DataStore.Instance.QueryToDataSet(sql + EstID);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return (int)dt.Rows[0]["cnt"];
                }
            }

            return count;
        }


        //운영회사(입력그리드) - 텍스트박스
        private void txtManagerCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtManagerCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

                e.Handled = true;
            }
        }

        //운영회사(입력그리드) - 버튼
        private void btnManagerCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtManagerCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

        }

        //영업회사(입력그리드) - 텍스트박스
        private void txtSalesCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtSalesCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

                e.Handled = true;
            }
        }

        //영업회사(입력그리드) - 버튼
        private void btnSalesCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSalesCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

        }

        //실사업체(입력그리드) - 텍스트박스
        private void txtSearchCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtSearchCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

                e.Handled = true;
            }
        }

        //실사업체(입력그리드) - 버튼
        private void btnSearchCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSearchCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

        }

        private void btnOrderNo_Click(object sender, RoutedEventArgs e)
        {
            if (strFlag != "U")
            {
                preOrder = new Win_ord_Pop_PreOrder_Q();

                if (preOrder.ShowDialog() == true)
                {
                    try
                    {
                        var selectedRow = preOrder.SelectedItem;
                        if (selectedRow != null)
                        {
                        
                            AutoBindDataToControls(selectedRow, grdInput);

                            txtOrderID.Text = string.Empty;

                            isBringLastOrder = true;
                            if(isBringLastOrder == true)
                            BringLastOrder(selectedRow.orderId);

                            ClearFTP_TextBox();
                            lstFilesName.Clear();
                            isBringLastOrder = false;
                        }
                        MessageBox.Show("지난 수주 데이터를 불러 왔습니다.", "확인");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("과거수주 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
                    }
                }

            }
            else
            {
                MessageBox.Show("신규 추가 중에만 사용 할 수 있습니다.");
            }
        }

        private void DataGridSub_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    DataGridSub_KeyDown(sender, e);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_PreviewKeyDown " + ee.ToString());
            }
        }

        private void DataGridSub_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var cell = sender as DataGridCell;
                TextBox txt = cell.Content as TextBox;

                string txx = txt.Name;

            }
            catch (Exception ex)
            {
                MessageBox.Show("오류지점 - 데이터 계산 " + ex.ToString());
            }
        }

        private void DataGridSub_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid datagrid = sender as DataGrid;
                if (datagrid == null) return;

                var subItem = datagrid.CurrentItem;

                int rowCount = datagrid.Items.IndexOf(subItem);
                int colCount = datagrid.Columns.IndexOf(datagrid.CurrentCell.Column);
;
                int StartColumnCount = 0;
                int EndColumnCount = datagrid.Columns.Count - 1;

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount == colCount && datagrid.Items.Count - 1 > rowCount)
                    {
                        datagrid.SelectedIndex = rowCount + 1;
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount + 1], datagrid.Columns[StartColumnCount]);
                    }
                    else if (EndColumnCount > colCount && datagrid.Items.Count - 1 > rowCount)
                    {
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount], datagrid.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount && datagrid.Items.Count - 1 == rowCount)
                    {
                        btnSave.Focus();
                    }
                    else if (EndColumnCount > colCount && datagrid.Items.Count - 1 == rowCount)
                    {
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount], datagrid.Columns[colCount + 1]);
                    }
                    else
                    {

                    }
                }
                else if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (datagrid.Items.Count - 1 > rowCount)
                    {
                        datagrid.SelectedIndex = rowCount + 1;
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount + 1], datagrid.Columns[colCount]);
                    }
                    else if (datagrid.Items.Count - 1 == rowCount)
                    {
                        if (EndColumnCount > colCount)
                        {
                            datagrid.SelectedIndex = 0;
                            datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[0], datagrid.Columns[colCount + 1]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (rowCount > 0)
                    {
                        datagrid.SelectedIndex = rowCount - 1;
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount - 1], datagrid.Columns[colCount]);
                    }
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (colCount > 0)
                    {
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount], datagrid.Columns[colCount - 1]);
                    }
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount > colCount)
                    {
                        datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount], datagrid.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount)
                    {
                        if (datagrid.Items.Count - 1 > rowCount)
                        {
                            datagrid.SelectedIndex = rowCount + 1;
                            datagrid.CurrentCell = new DataGridCellInfo(datagrid.Items[rowCount + 1], datagrid.Columns[StartColumnCount]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_KeyDown " + ee.ToString());
            }
        }

        private void DataGridSub_TextFocus(object sender, KeyEventArgs e)
        {
            try
            {
                Lib.Instance.DataGridINControlFocus(sender, e);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_TextFocus " + ee.ToString());
            }
        }

        private void DataGridSub_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_GotFocus " + ee.ToString());
            }
        }

        private void DataGridSub_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Lib.Instance.DataGridINBothByMouseUP(sender, e);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_MouseUp " + ee.ToString());
            }
        }

        //한전정보 탭 시공사업체
        private void txtConstrCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtConstrCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //한전정보 탭 시공사업체
        private void btnConstrCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtConstrCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //감리업체(거래구분에 감리업체를 들고오도록 할 것 CMMTRADE)
        private void txtSuperCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSuperCustomID, 5104, "");
            if(txtSuperCustomID.Tag != null)
            {
                txtSuperCustomPhoneNo.Text = callCustomData(txtSuperCustomID.Tag.ToString());
            }
            
        }

        //감리업체 버튼
        private void btnSuperCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSuperCustomID, 5104, "");
            if (txtSuperCustomID.Tag != null)
            {
                txtSuperCustomPhoneNo.Text = callCustomData(txtSuperCustomID.Tag.ToString());
            }
        }
        ////감리비용 지출업체
        //private void txtSuperCostPayCustomID_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //        MainWindow.pf.ReturnCode(txtSuperCostPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        //}
        ////감리비용 지출업체
        //private void btnSuperCostPayCustomID_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow.pf.ReturnCode(txtSuperCostPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        //}
        //안전관리 업체명
        private void txtSafeManageCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSafeManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
            if(txtSafeManageCustomID.Tag != null)
            {
                txtSafeManageCustomPhoneNo.Text = callCustomData(txtSafeManageCustomID.Tag.ToString());
            }
        }
        //안전관리 업체명
        private void btnSafeManageCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSafeManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
            if (txtSafeManageCustomID.Tag != null)
            {
                txtSafeManageCustomPhoneNo.Text = callCustomData(txtSafeManageCustomID.Tag.ToString());
            }
        }
        //사용검사 지출업체
        private void txtSuperUseInspPayCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSuperUseInspPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //사용검사 지출업체
        private void btnSuperUseInspPayCustomID_Click(object sender, RoutedEventArgs e)
        {
             MainWindow.pf.ReturnCode(txtSuperUseInspPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }


        //2025.02.10 자유입력식으로 변경해달라함
        ////기설충전업체
        //private void txtAlreadyManageCustomID_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if(e.Key == Key.Enter)
        //      MainWindow.pf.ReturnCode(txtAlreadyManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        //}

        ////기설충전업체
        //private void btnAlreadyManageCustomID_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow.pf.ReturnCode(txtAlreadyManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

        //}

        //한전&전기공사 검색조건 라벨클릭
        private void lblConstrCustomIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkConstrCustomIdSrh.IsChecked == true)
            {
                chkConstrCustomIdSrh.IsChecked = false;
                txtConstrCustomIdSrh.IsEnabled = false;
                btnConstrCustomIdSrh.IsEnabled = false;
            }
            else
            {
                chkConstrCustomIdSrh.IsChecked = true;
                txtConstrCustomIdSrh.IsEnabled = true;
                btnConstrCustomIdSrh.IsEnabled = true;
            }
        }

        //한전&전기공사 검색조건 체크박스클릭
        private void chkConstrCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkConstrCustomIdSrh.IsChecked == true)
            {
                chkConstrCustomIdSrh.IsChecked = true;
                txtConstrCustomIdSrh.IsEnabled = true;
                btnConstrCustomIdSrh.IsEnabled = true;
            }
            else
            {
                chkConstrCustomIdSrh.IsChecked = false;
                txtConstrCustomIdSrh.IsEnabled = false;
                btnConstrCustomIdSrh.IsEnabled = false;
            }
        }

        private void txtConstrCustomIdSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtConstrCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        private void btnConstrCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtConstrCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }



        //검색조건 - 사업구분 라벨
        private void lblOrderTypeIDSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrderTypeIDSrh.IsChecked == true)
            {
                chkOrderTypeIDSrh.IsChecked = false;
                cboOrderTypeIDSrh.IsEnabled = false;
            }
            else
            {
                chkOrderTypeIDSrh.IsChecked = true;
                cboOrderTypeIDSrh.IsEnabled = true;
            }
        }
        //검색조건 -사업구분 체크박스

        private void chkOrderTypeIDSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkOrderTypeIDSrh.IsChecked == true)
            {
                chkOrderTypeIDSrh.IsChecked = true;
                cboOrderTypeIDSrh.IsEnabled = true;
            }
            else
            {
                chkOrderTypeIDSrh.IsChecked = false;
                cboOrderTypeIDSrh.IsEnabled = false;
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (e.Source is TabControl)  
            {
                TabItem selectedTab = ((TabControl)sender).SelectedItem as TabItem;
                if (selectedTab != null && strFlag != "I" && strFlag != "U") 
                {
                    if (selectedTab.Name == "tab2")
                    {
                        fillGridTab2(orderID_global);
                        fillGridTab2_LocalGov(orderID_global);
                        tab2_clicked = true;
                    }
                    else if (selectedTab.Name == "tab3")
                    {
                        fillGridTab3(orderID_global);
                        tab3_clicked = true;
                    }
                    else if(selectedTab.Name == "tab4" )
                    {
                        fillGridTab4(orderID_global);
                        fillGridTab4_Accnt(orderID_global);
                        tab4_clicked = true;
                    }
                    else if (selectedTab.Name == "tab5")
                    {
                        fillgridTab5(orderID_global);
                        tab5_clicked = true;
                    }
                    
                }
            }
        }

        private void chkEoAddSrh_Click(object sender, RoutedEventArgs e)
        {
            string lastStrFlag = strFlag;

            if (dgdMain.Items.Count < 0 && dgdMain.SelectedIndex < 0) return;

            if (lblMsg.Visibility != Visibility.Visible)
            {
                if (chkEoAddSrh.IsChecked == true)
                {
                    tbkMsg.Text = "자료 유지 추가 중";                    
                    txtOrderID.Text = string.Empty;
                    strFlag = "I";
                    ClearGrdFtpTextBox();
                }
                else
                {
                    tbkMsg.Text = "자료 입력 중";
                    txtOrderID.Text = orderID_global;
                    CheckTabClicked();
                    if(txtOrderID.Text != string.Empty)
                    FillTabs(txtOrderID.Text);
                    strFlag = lastStrFlag;
                }
            }
      
        }

        //셀 업로드 이벤트
        private void btnAccntUpload_Click(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                var AccntView = dgdAccnt.CurrentItem as Win_order_Order_U_CodView_dgdAccnt;
                if (AccntView != null)
                {
                    if (AccntView.column4FilePath != string.Empty
                           && strFlag.Equals("U"))
                    {
                        MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                        return;
                    }
                    else
                    {               
                        var button = sender as Button;
                        var parent = button.Parent as StackPanel;
                        var textBox = parent.Children.OfType<TextBox>().FirstOrDefault();

                        if (textBox != null)
                        {
                            
                            FTP_Upload_TextBox(textBox);
                        }
                    }
                }
            }
        }

        private void txtEstID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtEstID.Text.Length == 0)
            {
                txtEstID.Text = string.Empty;
                txtEstID.Tag = null;

                txtEstSubject.Text = string.Empty;
            }
        }

        //영업담당자 라벨클릭
        private void lblSaledamdangjaNameSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkSaledamdangjaNameSrh.IsChecked == true)
            {
                chkSaledamdangjaNameSrh.IsChecked = false;
                txtSaledamdangjaNameSrh.IsEnabled = false;
            }
            else
            {
                chkSaledamdangjaNameSrh.IsChecked = true; ;
                txtSaledamdangjaNameSrh.IsEnabled = true;
            }
        }

        //영업담당자 체크
        private void chkSaledamdangjaNameSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkSaledamdangjaNameSrh.IsChecked == true)
            {
                chkSaledamdangjaNameSrh.IsChecked = true;
                txtSaledamdangjaNameSrh.IsEnabled = true;
            }
            else
            {
                chkSaledamdangjaNameSrh.IsChecked = false; ;
                txtSaledamdangjaNameSrh.IsEnabled = false;
            }
        }

        private void txtFileTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBox textBlock = sender as TextBox;
            if(textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
            {
                ToolTip tooltip = new ToolTip();
                tooltip.Content = textBlock.Text;
                textBlock.ToolTip = tooltip;
                tooltip.StaysOpen = true;
            }
        }

        private void TextBox_SearchFirst(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag == null || textBox.Text.Trim() == string.Empty)
            {
                ShowTooltipMessage(sender as FrameworkElement, "먼저 엔터 Key를 눌러 검색하세요", MessageBoxImage.Information);
                e.Handled = true;
            }
        }

        private void AccGrid_canopyReqCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textbox = sender as TextBox;
                MainWindow.pf.ReturnCode(textbox, (int)Defind_CodeFind.DCF_CUSTOM, "");

            }
        }

        //걍 버전
        //private void SearchToggleButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ToggleButton toggleButton = sender as ToggleButton;
        //    if (toggleButton == null) return;

        //    if (toggleButton.IsChecked == true)
        //    {
        //        grdExtraSearchCondtions.Visibility = Visibility.Visible;
        //        brLine_Bottom.Visibility = Visibility.Visible;
        //        brLine_Top.Visibility = Visibility.Hidden;
        //        this.Dispatcher.BeginInvoke(new Acㅇtion(() =>
        //        {
        //            UpdateToggleButtonPosition();
        //        }), DispatcherPriority.Loaded);
        //    }
        //    else
        //    {
        //        grdExtraSearchCondtions.Visibility = Visibility.Hidden;
        //        brLine_Bottom.Visibility = Visibility.Hidden;
        //        brLine_Top.Visibility = Visibility.Visible;
        //        UpdateToggleButtonPosition();
        //    }
        //}

        //애니메이션 접기펼치기
        private void SearchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton == null || _isAnimating) return;
     

            if (toggleButton.IsChecked == true)
            {
                _speechBubble.Hide();

                grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, null); 

                grdExtraSearchCondtions.Visibility = Visibility.Visible;
                brLine_Bottom.Visibility = Visibility.Visible;
                brLine_Top.Visibility = Visibility.Hidden;

                grdExtraSearchCondtions.MaxHeight = double.PositiveInfinity;
                grdExtraSearchCondtions.UpdateLayout();
                double targetHeight = grdExtraSearchCondtions.ActualHeight;

                grdExtraSearchCondtions.MaxHeight = 0;

                var heightAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = targetHeight,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                _isAnimating = true;
                CompositionTarget.Rendering += OnRenderingDuringAnimation; //렌더링중에 지속적으로 발생되는 이벤트

                heightAnimation.Completed += (s, args) =>
                {
                    _isAnimating = false;
                    CompositionTarget.Rendering -= OnRenderingDuringAnimation;

                    grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, null);
                    grdExtraSearchCondtions.MaxHeight = double.PositiveInfinity;

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UpdateToggleButtonPosition();
                    }), DispatcherPriority.Loaded);
                    
                };

                grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, heightAnimation);
            }
            else
            {
                grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, null);
                CloseToolTip();

                double currentHeight = grdExtraSearchCondtions.ActualHeight;

                var heightAnimation = new DoubleAnimation
                {
                    From = currentHeight,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(350),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                _isAnimating = true;
                CompositionTarget.Rendering += OnRenderingDuringAnimation;

                heightAnimation.Completed += (s, args) =>
                {
                    _isAnimating = false;
                    CompositionTarget.Rendering -= OnRenderingDuringAnimation;

                    grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, null);

                    grdExtraSearchCondtions.Visibility = Visibility.Hidden;
                    brLine_Bottom.Visibility = Visibility.Hidden;
                    brLine_Top.Visibility = Visibility.Visible;

                    grdExtraSearchCondtions.MaxHeight = double.PositiveInfinity;

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UpdateToggleButtonPosition();
                    }), DispatcherPriority.Loaded);

                    ShowSpeechBubble();
                };

                grdExtraSearchCondtions.BeginAnimation(FrameworkElement.MaxHeightProperty, heightAnimation);                
            }

   
        }

        //추가 검색조건 체크했을 때 사용자에게 알려주느 말풍선
        private void ShowSpeechBubble()
        {

            if (_speechBubble != null && (strFlag.Equals("I") || strFlag.Equals("U")))
                _speechBubble.Hide();

            if (CheckCheckBoxAddtionalCondtions() && (!strFlag.Equals("I") && !strFlag.Equals("U")))
            {    
                if (_speechBubble != null)
    
                _speechBubble.Show();
                if (!_speechBubbleMouseEntered)
                {
                    _speechBubble.Container.MouseEnter += OnSpeechBubbleMouseEnter;
                    _speechBubble.Container.MouseLeave += OnSpeechBubbleMouseLeave;
                    _speechBubbleMouseEntered = true;
                }

            }
            else
            {
                if(_speechBubble != null)
                _speechBubble.Hide();
            }
   
        }

        //Hidden또는 Hide를 해버리면 코드 무한 반복이라 투명도로 해결
        private void OnSpeechBubbleMouseEnter(object sender, MouseEventArgs e)
        {
            _speechBubble.Container.Opacity = 0;
        }

        private void OnSpeechBubbleMouseLeave(object sender, MouseEventArgs e)
        {
            _speechBubble.Container.Opacity = 1;
        }

        private bool CheckCheckBoxAddtionalCondtions()
        {
            bool flag = false;

            FindUiObject(grdExtraSearchCondtions, child =>
            {
                if (child is CheckBox checkBox)
                {
                    if (checkBox.IsChecked == true)
                        flag = true;
                }
            });

            return flag;
            
        }
        

        private void OnRenderingDuringAnimation(object sender, EventArgs e)
        {
            if (_isAnimating)
            {
                UpdateToggleButtonPosition();
            }
        }

        private void UpdateToggleButtonPosition()
        {
            if (!IsLoaded) return;

            double topMargin = 0;
            double leftMargin = 169; 

            if (grdExtraSearchCondtions.Visibility == Visibility.Visible)
            {

                grdExtraSearchCondtions.UpdateLayout();
         
                var border = grdExtraSearchCondtions.Children.OfType<Border>().FirstOrDefault();
                if (border != null)
                {
                    topMargin = border.ActualHeight;

                    var grid = border.Child as Grid;
                    if (grid != null && grid.ColumnDefinitions.Count > 0)
                    {
                        // 첫 번째 컬럼의 실제 너비 계산
                        leftMargin = grid.ColumnDefinitions[0].ActualWidth;
                    }
                }
                else
                {
                    // grdExtraSearchCondtions이 숨겨졌을 때의 기본 leftMargin
                    leftMargin = 169;
                }

            }

            tgnExtraSearchConditions.Margin = new Thickness(0, topMargin, 0, 0);
            brLine_Bottom.Margin = new Thickness(leftMargin, 0, 0, 0);


        }


    
        public void ToggleExpandedSearch()
        {
            tgnExtraSearchConditions.IsChecked = !tgnExtraSearchConditions.IsChecked;
            SearchToggleButton_Click(tgnExtraSearchConditions, new RoutedEventArgs());
        }
  
        public bool IsExpandedSearchOpen => tgnExtraSearchConditions.IsChecked == true;

        protected override void OnKeyDown(KeyEventArgs e)
        {
           
            if (e.Key == Key.Escape && IsExpandedSearchOpen)
            {
                ToggleExpandedSearch();
                e.Handled = true;
                return;
            }
     
            base.OnKeyDown(e); 
        }

        private void grdSearConditionsTop_GotFocus(object sender, RoutedEventArgs e)
        {
            if (grdSearchConditionsTop.IsFocused)
            {
                tgnExtraSearchConditions.Visibility = Visibility.Visible;
                UpdateToggleButtonPosition();
            }
        }

        private void grdSearConditionsTop_LostFocus(object sender, RoutedEventArgs e)
        {
            if(tgnExtraSearchConditions.Visibility == Visibility.Visible)
            {
                tgnExtraSearchConditions.Visibility = Visibility.Hidden;
                tgnExtraSearchConditions.IsChecked = false;
                SearchToggleButton_Click(null,null);
                UpdateToggleButtonPosition();
            }
        }

        private void MainGrid_Click(object sender, MouseButtonEventArgs e)
        {
            Grid grd = sender as Grid;
            if(grd != null)
            {
                //MessageBox.Show("입력 공간 클릭");
                SetExtraSearchGrid_InitialState();
                HideExtraSearchConditionGrid();
                CloseToolTip();
            }
        }

        private void SearchGrid_Click(object sender, MouseButtonEventArgs e)
        {
            Grid grd = sender as Grid;
            if (grd != null)
            {
                //MessageBox.Show("검색 공간 클릭");
                ShowExtraSearchConditionGrid();
            }
        }

        private void ShowExtraSearchConditionGrid()
        {
            if(!strFlag.Equals("I") && !strFlag.Equals("U"))
            {
                SetExtraSearchGrid_InitialState();
            }
        }

        private void SetExtraSearchGrid_InitialState()
        {         
            brLine_Top.Visibility = Visibility.Visible;
            tgnExtraSearchConditions.Visibility = Visibility.Visible;

            tgnExtraSearchConditions.IsChecked = false;
            grdExtraSearchCondtions.Visibility = Visibility.Hidden;
            brLine_Bottom.Visibility = Visibility.Hidden;
            UpdateToggleButtonPosition();
            ShowSpeechBubble();
         
        }

        private void HideExtraSearchConditionGrid()
        {
            brLine_Top.Visibility = Visibility.Hidden;
            tgnExtraSearchConditions.Visibility = Visibility.Hidden;
            ShowSpeechBubble();
        }

        private void CommonControl_Click(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            //var datePicker = FindChild<DatePicker>(parentContainer);
            CheckBox checkbox = FindChild<CheckBox>(label);
            List<DatePicker> dtps = lib.FindAllSiblingControls<DatePicker>(label);

            lib.CommonControl_Click(sender, e);

            if (checkbox != null && checkbox.IsChecked == true && checkbox.Name.Equals("chkUniversalSearhSrh"))
            {
                DisablekCheckBox();
                ShowTooltipMessage(sender as FrameworkElement, "기존 검색된 내용에서 키워드 재검색 합니다.", MessageBoxImage.Information, PlacementMode.Bottom);
            }
            else if (checkbox != null && checkbox.IsChecked == false && checkbox.Name.Equals("chkUniversalSearhSrh"))
            {
                EnablekCheckBox();
            }

            foreach(DatePicker dtp in dtps)
            {
                if (dtp != null && dtp.SelectedDate == null)
                {
                    dtp.SelectedDate = DateTime.Today;
                }
            }

        }

        private void CommonControl_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            List<DatePicker> dtps = lib.FindAllSiblingControls<DatePicker>(checkbox);

            lib.CommonControl_Click(sender, e);          
            
            if(checkbox != null && checkbox.IsChecked == true && checkbox.Name.Equals("chkUniversalSearhSrh"))
            {
                DisablekCheckBox();
                ShowTooltipMessage(sender as FrameworkElement, "기존 검색된 내용에서 키워드 재검색 합니다.", MessageBoxImage.Information, PlacementMode.Bottom);
            }
            else if(checkbox != null && checkbox.IsChecked == false && checkbox.Name.Equals("chkUniversalSearhSrh"))
            {
                EnablekCheckBox();
            }

            foreach (DatePicker dtp in dtps)
            {
                if (dtp != null && dtp.SelectedDate == null)
                {
                    dtp.SelectedDate = DateTime.Today;
                }
            }
        }

        private void txtSuperCustomIDSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSuperCustomIDSrh, 5104, "");
        }

        private void btnSuperCustomIDSrh_Click(object sender, RoutedEventArgs e)
        {
           
             MainWindow.pf.ReturnCode(txtSuperCustomIDSrh, 5104, "");
        }

        private void txtSafeManageCustomIDSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSafeManageCustomIDSrh, 0, "");
        }

        private void btnSafeManageCustomIDSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSafeManageCustomIDSrh, 0, "");
        }

        private void txtSuperUseInspPayCustomIDSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSuperUseInspPayCustomIDSrh, 0, "");
        }

        private void btnSuperUseInspPayCustomIDSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSuperUseInspPayCustomIDSrh, 0, "");

        }

        //통합검색 텍스트박스 - 포커스
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "검색어를 입력하세요")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
                tb.FontStyle = FontStyles.Normal;

            }
        }

        //통합검색 텍스트박스 - 포커스
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "검색어를 입력하세요";
                tb.Foreground = Brushes.Gray;
                tb.FontStyle = FontStyles.Italic;
            }
        }



        //    private void btnGoOrderCalendar_Click(object sender, RoutedEventArgs e)
        //    {
        //        // 있으면 진행, 없으면 리턴
        //        if (OrderView.orderID != null)
        //        {
        //            MainWindow.orderDate = OrderView.orderDate;
        //            MainWindow.orderID_Calendar = OrderView.orderID;

        //            MainWindow.acptDate = string.Empty;
        //            MainWindow.reServeID_Calendar = string.Empty;

        //            DateTime orderDate = Convert.ToDateTime(OrderView.orderDate);

        //            string firstDay = new DateTime(orderDate.Year, orderDate.Month, 1).ToString("yyyyMMdd");
        //            string lastDay = new DateTime(orderDate.Year, orderDate.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd");

        //            MainWindow.sFromDate = firstDay;
        //            MainWindow.sToDate = lastDay;
        //        }
        //        else
        //        {
        //            MessageBox.Show("먼저 데이터를 선택 후 클릭하세요.");
        //            return;
        //        }


        //        int i = 0;
        //        foreach (MenuViewModel mvm in MainWindow.mMenulist)
        //        {
        //            if (mvm.Menu.Equals("계약일정표 조회"))
        //            {
        //                break;
        //            }
        //            i++;
        //        }
        //        try
        //        {
        //            if (MainWindow.MainMdiContainer.Children.Contains(MainWindow.mMenulist[i].subProgramID as MdiChild))
        //            {
        //                (MainWindow.mMenulist[i].subProgramID as MdiChild).Focus();
        //                //혹시나 상담등록에서 달력 열어두고 또 계약등록에서 일정조회하면? 달력 미리 열어두고 하면?
        //                var mdiChild = MainWindow.mMenulist[i].subProgramID as MdiChild;
        //                if (mdiChild.Content is Win_ord_OrderCalendar_Q control) 
        //                {
        //                    control.setIntFlagOn();
        //                    control.FillCalendar();
        //                }
        //            }
        //            else
        //            {
        //                Type type = Type.GetType("WizMes_Nadaum." + MainWindow.mMenulist[i].ProgramID.Trim(), true);
        //                object uie = Activator.CreateInstance(type);

        //                MainWindow.mMenulist[i].subProgramID = new MdiChild()
        //                {
        //                    Title = "WizMes_Nadaum_[" + MainWindow.mMenulist[i].MenuID.Trim() + "] " + MainWindow.mMenulist[i].Menu.Trim() +
        //                            " (→" + MainWindow.mMenulist[i].ProgramID.Trim() + ")",
        //                    Height = SystemParameters.PrimaryScreenHeight * 0.8,
        //                    MaxHeight = SystemParameters.PrimaryScreenHeight * 0.85,
        //                    Width = SystemParameters.WorkArea.Width * 0.85,
        //                    MaxWidth = SystemParameters.WorkArea.Width,
        //                    Content = uie as UIElement,
        //                    Tag = MainWindow.mMenulist[i]
        //                };
        //                Lib.Instance.AllMenuLogInsert(MainWindow.mMenulist[i].MenuID, MainWindow.mMenulist[i].Menu, MainWindow.mMenulist[i].subProgramID);
        //                MainWindow.MainMdiContainer.Children.Add(MainWindow.mMenulist[i].subProgramID as MdiChild);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("해당 화면이 존재하지 않습니다.");
        //        }
        //    }
    }



    #endregion

    public class Win_ord_Order_U_CodeView_dgdMain : BaseView
    {
        public int num { get; set; }
        public string orderId {get;set;}
        public string estSubject { get; set; }
        public string estID { get; set; }
        public string orderType { get; set; }
        public string orderTypeID { get; set; }
        public string orderNo { get; set; }
        public string saleCustom { get; set; }
        public string saleCustomID { get; set; }
        public string managerCustom { get; set; }
        public string managerCustomID { get; set; }
        public string searchCustomID { get; set; }
        public string searchCustom { get; set; }
        public string manageCustomAcptDate { get; set; }
        public string manageCustomConfirmDate { get; set; }
        public string installLocation { get; set; }
        public string installLocationPart { get; set; }
        public string InstallLocationPhone { get; set; }
        public string articleList { get; set; }
        public string closeYn { get; set; }
        public string orderAmount { get; set; }
        public string acptDate { get; set; }
        public string installLocationAddComments { get; set; }
        public string cpoCalcuDate{get;set;}
        public string constrCalcuDate { get; set; }
        public string installLocationAddress {get;set;}
        public string houseHoldCount {get;set;}
        public string carParkingCount {get;set;}
        public string alreadyManageCustom {get;set;}
        public string alreadyManageCustomID {get;set;}
        public string installLocationComments {get;set;}
        public string alReadyChargeCount {get;set;}
        public string contractToDate { get; set; }
        public string contractFromDate {get;set;}
        public string openReqDate {get;set;}
        public string openDate {get;set;}
        public string damdangjaName {get;set;}
        public string damdangjaEMail {get;set;}
        public string damdangjaPhone {get;set;}
        public string electrCarCount {get;set;}
        public string reqChargeCount {get;set;}
        public string saledamdangjaName {get;set;}
        public string saledamdangjaEmail { get; set; }
        public string saledamdangjaPhone {get;set;}
        public string saleCustomAddWork {get;set;}
        public string salegift { get; set; }
        public string salesComments { get; set; }
        public string mtrAmount{get;set;}
        public string mtrShippingCharge{get;set;}
        public string mtrPriceUnitClss{get;set;}
        public string mtrPriceUnit { get; set; }
        public string mtrCanopyInwareInfo{get;set;}
        public string mtrCanopyOrderAmount { get; set; }
        public string contractFileName { get; set; }
        public string contractFilePath { get; set; }

    }

    public class Win_order_Order_U_CodView_dgdAccnt : BaseView
    {
        public string column1Date { get; set; }
        public string column2Amount { get; set; }
        public string column3Comment { get; set; }
        public string column4FilePath { get; set; }
        public string column5FileName { get; set; }
        //public string column4Amount { get; set; }
        //public string column5Amount { get; set; }
        //public string column6Amount { get; set; }        
        //public string column7Comment { get; set; }
        public bool isBold { get; set; }
        public bool isNegative { get; set; }

        public static Win_order_Order_U_CodView_dgdAccnt CreateEmpty_dgdAccnt_row()
        {
            return new Win_order_Order_U_CodView_dgdAccnt()
            {
                column1Date = "",// DateTime.Now.ToString("yyyy-MM-dd"),
                column2Amount = "",
                column3Comment = "",
                column4FilePath = "",
                column5FileName = "",
                //column4Amount = "",
                //column5Amount = "",
                //column6Amount = "",
                //column7Comment = ""
                isBold = false,
                isNegative = false
            };
        }
    }

    public class Win_order_Order_U_CodView_dgdAcc : BaseView
    {
        public int num { get; set; }
        public string orderSeq { get; set; }
        public string articleID { get; set; }
        public string article { get; set; }
        public string orderTypeID { get; set; }
        public string orderType { get; set; }
        public string chargeOrderDate {get;set;}
        public string chargeOrderQty { get; set; }
        public string chargeInwareDate {get;set;}
        public string chargeInwareQty {get;set;}
        public string chargeInwareUnitPrice { get; set; }
        public string chargeInwareLocation {get;set;}
        public string canopyReqCustom {get;set;}
        public string canopyReqCustomID { get; set; }
        public string chargeModelHelmat {get;set;}
        public string chargeModelinloc {get;set;}
        public string chargeModelOneBody {get;set;}
        public string chargeStandReqDate {get;set;}
        public string chargeStandInwareDate {get;set;}
        public string Comments {get;set;}
        public string CreateDate {get;set;}
        public string CreateUserID {get;set;}
        public string LastUpdateDate {get;set;}
        public string LastUpdateUserID { get; set; }

        public ObservableCollection<CodeView> ovcOrderTypeAcc { get; set; }

        public static Win_order_Order_U_CodView_dgdAcc CreateEmpty_dgdAcc(ObservableCollection<CodeView> ovcOrderTypeID)
        {
            return new Win_order_Order_U_CodView_dgdAcc()
            {
                num = 0,
                orderSeq = "",
                articleID = "",
                article = "",
                orderType = ovcOrderTypeID[0].code_name,
                orderTypeID = ovcOrderTypeID[0].code_id,
                chargeOrderDate = "",
                chargeInwareDate = "",
                chargeInwareQty = "",
                chargeInwareLocation = "",
                canopyReqCustom = "",
                chargeModelHelmat = "",
                chargeModelinloc = "",
                chargeModelOneBody = "",
                chargeStandReqDate = "",
                chargeStandInwareDate = "",
                Comments = "",
                ovcOrderTypeAcc = ovcOrderTypeID
            };
        }

    }

    public class Win_order_Order_U_CodView_dgdAcc_Total : BaseView
    {
        public int num { get; set; }
        public string totalSum { get; set; }
    }

    public class Win_order_Order_U_CodView_dgdTotal : BaseView
    {
        public string count { get; set; }
        public string totalSum { get; set; }
    }

    public class Win_order_Order_U_CodView_localGov : BaseView
    {
        public int num { get; set; }
        public string orderID {get;set;}
        public string localGovSeq {get;set;}
        public string localGovPermissionNo { get; set; }
        public string localGovBehaviorDate {get;set;}
        public string localGovBehaviorReportDate { get; set; }
        public string localGovSuppleContext {get;set;}
        public string localGovSuppleDate {get;set;}
        public string localGovComments {get;set;}
        public string CreateDate {get;set;}
        public string CreateUserID {get;set;}
        public string LastUpdateDate {get;set;}
        public string LastUpdateUserID { get; set; }


        public static Win_order_Order_U_CodView_localGov CreateEmpty_localGov()
        {
            return new Win_order_Order_U_CodView_localGov()
            {
                num = 0,               
                localGovSeq = "",
                localGovPermissionNo = "",
                localGovBehaviorDate = "",
                localGovBehaviorReportDate= "",
                localGovSuppleContext = "",
                localGovSuppleDate = "",
                localGovComments = "",
                CreateDate = "",
                CreateUserID = "",
                LastUpdateDate = "",
                LastUpdateUserID = "",

            };
        }
    }


    #region 말풍선

    public enum BubblePosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        LeftCenter,
        Center,
        RightCenter,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum TailDirection
    {
        None,
        Up,
        UpLeft,
        UpRight,
        Down,
        DownLeft,
        DownRight,
        Left,
        LeftUp,
        LeftDown,
        Right,
        RightUp,
        RightDown
    }

    public enum TextAlign
    {
        Auto,      // 꼬리 방향에 따라 자동 조정
        Center,    // 강제 중앙
        Left,      // 왼쪽 정렬
        Right,     // 오른쪽 정렬
        Top,       // 위쪽 정렬
        Bottom     // 아래쪽 정렬
    }

    //어도너는 zindex 설정할 필요없이 항상 보이는 화면 가장 위에 표시된다고 합니다.
    //드래그앤드롭, 말풍선에 활용..
    public class SpeechBubbleAdorner : Adorner
    {
        private readonly VisualCollection _children;
        private readonly Grid _container;
        private readonly System.Windows.Shapes.Path _speechBubblePath;
        private readonly TextBlock _textBlock;

        private BubblePosition _position = BubblePosition.TopCenter;
        private TailDirection _tailDirection = TailDirection.Down;
        private TextAlign _textAlign = TextAlign.Auto;
        private Thickness _margin;
        private string _text = "";
        private bool _isVisible = true;

        public Grid Container => _container;

        public SpeechBubbleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _children = new VisualCollection(this);

            // 예쁜 말풍선 Path
            _speechBubblePath = new System.Windows.Shapes.Path
            {
                Fill = Brushes.White,
                Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = 1,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Opacity = 0.3,
                    ShadowDepth = 2,
                    BlurRadius = 4
                }
            };

            // 텍스트 (Grid에서 자동 중앙 정렬)
            _textBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            // Grid 컨테이너 (자동 중앙 정렬)
            _container = new Grid();
            _container.Children.Add(_speechBubblePath);
            _container.Children.Add(_textBlock);

            _children.Add(_container);

            // 기본 말풍선 모양 설정
            UpdateBubbleShape();
        }

        public SpeechBubbleAdorner SetText(string text)
        {
            _text = text ?? "빈 텍스트"; // null 방지
            _textBlock.Text = _text;

            // 즉시 업데이트해서 디버깅
            //System.Diagnostics.Debug.WriteLine($"SetText called: '{_text}'");

            UpdateBubbleShape();
            InvalidateArrange();
            return this;
        }

        public SpeechBubbleAdorner SetPosition(BubblePosition position, TailDirection tailDirection = TailDirection.Down)
        {
            _position = position;
            _tailDirection = tailDirection;
            UpdateBubbleShape();
            InvalidateArrange();
            return this;
        }

        public SpeechBubbleAdorner SetTextAlign(TextAlign textAlign)
        {
            _textAlign = textAlign;
            UpdateBubbleShape(); // Canvas 위치 재계산 필요
            return this;
        }

        public SpeechBubbleAdorner SetMargin(Thickness margin)
        {
            _margin = margin;
            InvalidateArrange();
            return this;
        }

        public SpeechBubbleAdorner Show()
        {
            _isVisible = true;
            _container.Visibility = Visibility.Visible;
            return this;
        }

        public SpeechBubbleAdorner Hide()
        {
            _isVisible = false;
            _container.Visibility = Visibility.Collapsed;
            return this;
        }

        public SpeechBubbleAdorner Configure(string text, BubblePosition position, TailDirection tailDirection = TailDirection.Down, TextAlign textAlign = TextAlign.Auto, Thickness? margin = null)
        {
            _text = text ?? "";
            _position = position;
            _tailDirection = tailDirection;
            _textAlign = textAlign;
            if (margin.HasValue)
                _margin = margin.Value;

            _textBlock.Text = _text;
            UpdateBubbleShape();
            InvalidateArrange();
            return this;
        }

        private void UpdateBubbleShape()
        {
            // 텍스트 크기 측정
            _textBlock.Measure(new Size(300, double.PositiveInfinity));
            var textSize = _textBlock.DesiredSize;

            // 최소 크기 보장
            var estimatedWidth = _text.Length * 12;
            if (textSize.Width < estimatedWidth) textSize.Width = estimatedWidth;

            double bubbleWidth = Math.Max(textSize.Width + 30, 100);
            double bubbleHeight = Math.Max(textSize.Height + 10, 40);

            // 꼬리에 따른 여백
            double extraWidth = _tailDirection == TailDirection.None ? 0 : 20;
            double extraHeight = _tailDirection == TailDirection.None ? 0 : 20;

            // 말풍선 Path 생성
            string pathData = GeneratePathData(bubbleWidth, bubbleHeight);
            _speechBubblePath.Data = Geometry.Parse(pathData);

            // 텍스트 마진 조정 (꼬리 피하기)
            UpdateTextMargin();

            // 컨테이너 크기 설정
            _container.Width = bubbleWidth + extraWidth;
            _container.Height = bubbleHeight + extraHeight;
        }

        //꼬리때문에 패딩이 특정 방향으로 더 생겨서 
        private void UpdateTextMargin()
        {
            double leftMargin = 15;
            double topMargin = 10;
            double rightMargin = 15;
            double bottomMargin = 10;

            
            switch (_tailDirection)
            {
                case TailDirection.Up:
                case TailDirection.UpLeft:
                case TailDirection.UpRight:
                    /*topMargin += 5;*/ 
                    break;

                case TailDirection.Down:
                case TailDirection.DownLeft:
                case TailDirection.DownRight:
                    //bottomMargin += 10; // 아래쪽 꼬리 길이만큼
                    break;

                case TailDirection.Left:
                case TailDirection.LeftUp:
                case TailDirection.LeftDown:
                    //leftMargin += 10; // 왼쪽 꼬리 길이만큼
                    break;

                case TailDirection.Right:
                case TailDirection.RightUp:
                case TailDirection.RightDown:
                    //rightMargin += 10; // 오른쪽 꼬리 길이만큼
                    break;
            }

            // 강제 정렬 적용
            if (_textAlign != TextAlign.Auto)
            {
                switch (_textAlign)
                {
                    case TextAlign.Left:
                        _textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                        leftMargin += 10;
                        break;
                    case TextAlign.Right:
                        _textBlock.HorizontalAlignment = HorizontalAlignment.Right;
                        rightMargin += 10;
                        break;
                    case TextAlign.Top:
                        _textBlock.VerticalAlignment = VerticalAlignment.Top;
                        topMargin += 5;
                        break;
                    case TextAlign.Bottom:
                        _textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                        bottomMargin += 5;
                        break;
                    case TextAlign.Center:
                        _textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                        _textBlock.VerticalAlignment = VerticalAlignment.Center;
                        break;
                }
            }
            else
            {
                // Auto 모드에서는 중앙 정렬
                _textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                _textBlock.VerticalAlignment = VerticalAlignment.Center;
            }

            _textBlock.Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        }

        private string GeneratePathData(double bubbleWidth, double bubbleHeight)
        {
            double tailSize = 12;
            double cornerRadius = 8;

            // 모든 말풍선 몸체를 동일한 위치(10, 10)에서 시작하도록 통일
            double baseX = 10;
            double baseY = 10;

            switch (_tailDirection)
            {
                case TailDirection.None:
                    return $"M {baseX + cornerRadius},5 L {baseX + bubbleWidth - cornerRadius},5 Q {baseX + bubbleWidth - 5},5 {baseX + bubbleWidth - 5},{5 + cornerRadius} L {baseX + bubbleWidth - 5},{bubbleHeight - 5 - cornerRadius} Q {baseX + bubbleWidth - 5},{bubbleHeight - 5} {baseX + bubbleWidth - cornerRadius},{bubbleHeight - 5} L {baseX + cornerRadius},{bubbleHeight - 5} Q {baseX + 5},{bubbleHeight - 5} {baseX + 5},{bubbleHeight - 5 - cornerRadius} L {baseX + 5},{5 + cornerRadius} Q {baseX + 5},5 {baseX + cornerRadius},5 Z";

                // Up 계열 (꼬리가 위로) - 몸체는 baseY 위치에, 꼬리만 위로
                case TailDirection.Up:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth / 2 - 6},{baseY} L {baseX + bubbleWidth / 2},{baseY - 10} L {baseX + bubbleWidth / 2 + 6},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.UpLeft:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth / 3 - 6},{baseY} L {baseX + bubbleWidth / 3},{baseY - 10} L {baseX + bubbleWidth / 3 + 6},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.UpRight:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth * 2 / 3 - 6},{baseY} L {baseX + bubbleWidth * 2 / 3},{baseY - 10} L {baseX + bubbleWidth * 2 / 3 + 6},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                // Down 계열 (꼬리가 아래로) - 몸체는 baseY 위치에, 꼬리만 아래로
                case TailDirection.Down:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + bubbleWidth / 2 + 6},{baseY + bubbleHeight} L {baseX + bubbleWidth / 2},{baseY + bubbleHeight + 10} L {baseX + bubbleWidth / 2 - 6},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.DownLeft:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + bubbleWidth / 3 + 6},{baseY + bubbleHeight} L {baseX + bubbleWidth / 3},{baseY + bubbleHeight + 10} L {baseX + bubbleWidth / 3 - 6},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.DownRight:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + bubbleWidth * 2 / 3 + 6},{baseY + bubbleHeight} L {baseX + bubbleWidth * 2 / 3},{baseY + bubbleHeight + 10} L {baseX + bubbleWidth * 2 / 3 - 6},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                // Left 계열 (꼬리가 왼쪽으로) - 몸체는 baseX 위치에, 꼬리만 왼쪽으로
                case TailDirection.Left:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + bubbleHeight / 2 + 6} L {baseX - 5},{baseY + bubbleHeight / 2} L {baseX + 5},{baseY + bubbleHeight / 2 - 6} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.LeftUp:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + bubbleHeight / 3 + 6} L {baseX - 5},{baseY + bubbleHeight / 3} L {baseX + 5},{baseY + bubbleHeight / 3 - 6} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.LeftDown:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + bubbleHeight * 2 / 3 + 6} L {baseX - 5},{baseY + bubbleHeight * 2 / 3} L {baseX + 5},{baseY + bubbleHeight * 2 / 3 - 6} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                // Right 계열 (꼬리가 오른쪽으로) - 몸체는 baseX 위치에, 꼬리만 오른쪽으로
                case TailDirection.Right:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight / 2 - 6} L {baseX + bubbleWidth + 5},{baseY + bubbleHeight / 2} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight / 2 + 6} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.RightUp:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight / 3 - 6} L {baseX + bubbleWidth + 5},{baseY + bubbleHeight / 3} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight / 3 + 6} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                case TailDirection.RightDown:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight * 2 / 3 - 6} L {baseX + bubbleWidth + 5},{baseY + bubbleHeight * 2 / 3} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight * 2 / 3 + 6} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";

                default:
                    return $"M {baseX + cornerRadius},{baseY} L {baseX + bubbleWidth - cornerRadius},{baseY} Q {baseX + bubbleWidth - 5},{baseY} {baseX + bubbleWidth - 5},{baseY + cornerRadius} L {baseX + bubbleWidth - 5},{baseY + bubbleHeight - cornerRadius} Q {baseX + bubbleWidth - 5},{baseY + bubbleHeight} {baseX + bubbleWidth - cornerRadius},{baseY + bubbleHeight} L {baseX + cornerRadius},{baseY + bubbleHeight} Q {baseX + 5},{baseY + bubbleHeight} {baseX + 5},{baseY + bubbleHeight - cornerRadius} L {baseX + 5},{baseY + cornerRadius} Q {baseX + 5},{baseY} {baseX + cornerRadius},{baseY} Z";
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!_isVisible) return finalSize;

            var elementBounds = AdornedElement.RenderSize;
            var containerSize = new Size(_container.Width, _container.Height);

            // AdornedElement의 실제 화면 위치를 구함
            var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            var elementPosition = AdornedElement.TranslatePoint(new Point(0, 0), adornerLayer);

            // 위치 계산 (실제 element 위치 기준)
            var relativePosition = CalculateRelativePosition(elementBounds, containerSize);

            // 절대 위치 계산
            var absoluteX = elementPosition.X + relativePosition.X + _margin.Left - _margin.Right;
            var absoluteY = elementPosition.Y + relativePosition.Y + _margin.Top - _margin.Bottom;

            // 컨테이너 배치
            _container.Arrange(new Rect(absoluteX, absoluteY, containerSize.Width, containerSize.Height));

            return finalSize;
        }

        private Point CalculateRelativePosition(Size elementBounds, Size containerSize)
        {
            double x = 0, y = 0;
            const double offset = 10;

            switch (_position)
            {
                case BubblePosition.TopLeft:
                    x = -containerSize.Width - offset;
                    y = -containerSize.Height - offset;
                    break;
                case BubblePosition.TopCenter:
                    x = (elementBounds.Width - containerSize.Width) / 2;
                    y = -containerSize.Height - offset;
                    break;
                case BubblePosition.TopRight:
                    x = elementBounds.Width - containerSize.Width - offset;
                    y = -containerSize.Height - offset;
                    break;
                case BubblePosition.LeftCenter:
                    x = -containerSize.Width - offset;
                    y = (elementBounds.Height - containerSize.Height) / 2;
                    break;
                case BubblePosition.Center:
                    x = (elementBounds.Width - containerSize.Width) / 2;
                    y = (elementBounds.Height - containerSize.Height) / 2;
                    break;
                case BubblePosition.RightCenter:
                    x = elementBounds.Width - containerSize.Width - offset;
                    y = (elementBounds.Height - containerSize.Height) / 2;
                    break;
                case BubblePosition.BottomLeft:
                    x = -containerSize.Width - offset;
                    y = elementBounds.Height + offset;
                    break;
                case BubblePosition.BottomCenter:
                    x = (elementBounds.Width - containerSize.Width) / 2;
                    y = elementBounds.Height + offset;
                    break;
                case BubblePosition.BottomRight:
                    x = elementBounds.Width - containerSize.Width - offset;
                    y = elementBounds.Height - containerSize.Height - offset;
                    break;
            }

            return new Point(x, y);
        }

        protected override int VisualChildrenCount => _children.Count;
        protected override Visual GetVisualChild(int index) => _children[index];
    }

    // 확장 메서드 (체이닝 지원)
    public static class AdornerExtensions
    {
        public static SpeechBubbleAdorner AddSpeechBubble(this UIElement element, string text,
            BubblePosition position = BubblePosition.TopCenter,
            TailDirection tailDirection = TailDirection.Down,
            TextAlign textAlign = TextAlign.Auto,
            Thickness? margin = null)
        {
            var adorner = new SpeechBubbleAdorner(element);
            adorner.Configure(text, position, tailDirection, textAlign, margin);

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            adornerLayer?.Add(adorner);

            return adorner;
        }
    }


    #endregion

    //멀티헤더 스크롤뷰어와 데이터컬럼헤더 + 데이터그리드(컬럼Visibility = Hidden)
    //동기화 클래스
    //사용 방법
    //클래스 수준변수로 선언
    //한 화면에 멀티헤더가 여러개 있으면 List로 받고 private List<ScrollSyncHelper> scrollHelpers = new List<ScrollSyncHelper>();
    //하나면 ScrollSyncHelper scrollHelpers = new ScrollSyncHelper(스크롤뷰어X:Name , 데이터그리드 x:Name,  고정할 열갯수(왼쪽부터 히든컬럼 포함) = 기본 0)로 선언합니다.
    // InitializeComponent(); 이후에 호출하면 됩니다.
    public class ScrollSyncHelper
    {
        private ScrollViewer _headerScrollViewer;
        private DataGrid _dataGrid;
        private bool _isUpdatingScroll = false;
        private int _frozenColumnCount = 0;
        private List<UIElement> _frozenHeaderElements = new List<UIElement>();
        private ScrollViewer _dataGridScrollViewer = null; // 캐시된 스크롤뷰어 참조


        public ScrollSyncHelper(ScrollViewer headerScrollViewer, DataGrid dataGrid, int frozenColumnCount = 0)
        {
            _headerScrollViewer = headerScrollViewer;
            _dataGrid = dataGrid;
            _frozenColumnCount = frozenColumnCount;

            headerScrollViewer.ScrollChanged += HeaderScrollViewer_ScrollChanged;
            dataGrid.Loaded += DataGrid_Loaded;
            LimitHeaderScroll();

            // 디버깅용 로그
            Console.WriteLine($"ScrollSyncHelper 생성: DataGrid={dataGrid.Name}, FrozenColumnCount={frozenColumnCount}");

            // 데이터그리드가 이미 로드된 경우 초기화 실행
            if (dataGrid.IsLoaded)
            {
                InitializeScrollViewers();
            }
        }

        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;
            try
            {
                _isUpdatingScroll = true;

                // 캐시된 스크롤뷰어 사용 또는 필요 시 검색
                if (_dataGridScrollViewer == null)
                {
                    _dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                }

                if (_dataGridScrollViewer != null)
                {
                    // 스크롤이 최대치에 가까울 때 특별 처리
                    double maxScroll = _dataGridScrollViewer.ScrollableWidth;
                    double offset = e.HorizontalOffset;

                    // 스크롤이 끝에 매우 가까울 때 정확히 최대값으로 고정
                    if (Math.Abs(offset - maxScroll) < 2 && offset > maxScroll - 5)
                    {
                        offset = maxScroll;
                    }

                    _dataGridScrollViewer.ScrollToHorizontalOffset(offset);

                    // 고정 열 헤더 처리
                    if (_frozenColumnCount > 0)
                    {
                        ApplyFrozenHeaderTransform(offset);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HeaderScrollViewer_ScrollChanged 오류: {ex.Message}");
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;
            try
            {
                _isUpdatingScroll = true;
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null && _headerScrollViewer != null)
                {
                    // 스크롤이 최대치에 가까울 때 특별 처리
                    double maxScroll = scrollViewer.ScrollableWidth;
                    double offset = e.HorizontalOffset;

                    // 스크롤이 끝에 매우 가까울 때 정확히 최대값으로 고정
                    if (Math.Abs(offset - maxScroll) < 2 && offset > maxScroll - 5)
                    {
                        offset = maxScroll;
                    }

                    _headerScrollViewer.ScrollToHorizontalOffset(offset);

                    // 고정 열 헤더 처리
                    if (_frozenColumnCount > 0)
                    {
                        ApplyFrozenHeaderTransform(offset);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataGrid_ScrollChanged 오류: {ex.Message}");
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeScrollViewers();
            EnsureHeaderSyncConsistency();
        }

        private void InitializeScrollViewers()
        {
            try
            {
                // DataGrid의 FrozenColumnCount 설정
                if (_frozenColumnCount > 0)
                {
                    // 최대값 체크 (총 열 개수보다 많으면 안 됨)
                    int maxColumns = _dataGrid.Columns.Count;
                    if (_frozenColumnCount > maxColumns - 1) // 최소 하나의 열은 스크롤 가능해야 함
                    {
                        _frozenColumnCount = maxColumns - 1;
                        Console.WriteLine($"FrozenColumnCount 조정됨: {_frozenColumnCount}");
                    }

                    _dataGrid.FrozenColumnCount = _frozenColumnCount;
                    Console.WriteLine($"DataGrid.FrozenColumnCount 설정됨: {_frozenColumnCount}");

                    // 고정 헤더 요소 식별 및 초기화
                    InitializeFrozenHeaderElements();
                }

                // 스크롤뷰어 찾기 및 이벤트 연결
                _dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                if (_dataGridScrollViewer != null)
                {
                    // 기존 이벤트 핸들러 제거 후 다시 추가 (중복 방지)
                    _dataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                    _dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                }
                else
                {
                    Console.WriteLine("DataGrid의 ScrollViewer를 찾을 수 없음");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitializeScrollViewers 오류: {ex.Message}");
            }
        }

        // 고정 헤더 요소 초기화
        private void InitializeFrozenHeaderElements()
        {
            _frozenHeaderElements.Clear();

            // ScrollViewer 내용이 Panel인지 확인
            Panel headerPanel = null;

            if (_headerScrollViewer.Content is Panel panel)
            {
                headerPanel = panel;
            }
            else if (_headerScrollViewer.Content is Grid grid)
            {
                headerPanel = grid;
            }
            else
            {
                Console.WriteLine($"지원되지 않는 헤더 유형: {_headerScrollViewer.Content?.GetType().Name}");
                return;
            }

            // 고정 헤더 요소 식별
            for (int i = 0; i < _frozenColumnCount && i < headerPanel.Children.Count; i++)
            {
                if (headerPanel.Children[i] is UIElement element)
                {
                    _frozenHeaderElements.Add(element);
                    Console.WriteLine($"고정 헤더 요소 추가: {element.GetType().Name}");
                }
            }

            foreach (var element in _frozenHeaderElements)
            {
                Panel.SetZIndex(element, 100); // Z-Index 설정

                // 오버랩 효과를 위해 약간 넓게 설정
                if (element is DataGridColumnHeader header)
                {
                    // 테두리를 약간 두껍게 하고 오른쪽 여백을 추가
                    //header.BorderThickness = new Thickness(0, 0, 2, 1); // 오른쪽 테두리 두께 증가
                    //header.BorderBrush = new SolidColorBrush(Colors.White);

                    // 배경색 설정 (원래 배경색과 동일하게)
                    if (header.Background is SolidColorBrush brush)
                    {
                        SolidColorBrush newBrush = new SolidColorBrush(brush.Color);
                        header.Background = newBrush;
                    }
                    else
                    {
                        // 기본 배경색 설정 (테마에 맞게 조정)
                        header.Background = new SolidColorBrush(Color.FromRgb(51, 102, 204)); // 파란색 계열
                    }

                    // 마진 약간 변경하여 오버랩 효과
                    header.Padding = new Thickness(header.Padding.Left, header.Padding.Top,
                                                 header.Padding.Right + 1, header.Padding.Bottom);
                }
            }
        }

        // 고정 열 헤더에 TranslateTransform 적용
        private void ApplyFrozenHeaderTransform(double offset)
        {
            if (_frozenColumnCount <= 0)
            {
                // 고정 열이 없는 경우 변환 명시적으로 Zero로 설정 (null이 아님)
                foreach (var element in _frozenHeaderElements)
                {
                    try
                    {
                        // 명시적으로 0으로 설정된 변환 적용
                        TranslateTransform zeroTransform = new TranslateTransform(0, 0);
                        element.RenderTransform = zeroTransform;
                    }
                    catch (Exception) { }
                }
                return;
            }

            foreach (var element in _frozenHeaderElements)
            {
                try
                {
                    // TranslateTransform 생성 또는 가져오기
                    TranslateTransform transform;
                    if (!(element.RenderTransform is TranslateTransform))
                    {
                        transform = new TranslateTransform();
                        element.RenderTransform = transform;
                    }
                    else
                    {
                        transform = element.RenderTransform as TranslateTransform;
                    }

                    transform.X = offset; // 스크롤 오프셋만큼 이동
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TranslateTransform 적용 오류: {ex.Message}");
                }
            }
        }

        private void EnsureHeaderSyncConsistency()
        {
            if (_dataGridScrollViewer == null || _headerScrollViewer == null) return;

            try
            {
                // 스크롤 제약 조건 확인
                double dgMaxScroll = _dataGridScrollViewer.ScrollableWidth;
                double headerMaxScroll = _headerScrollViewer.ScrollableWidth;

                // 헤더 스크롤뷰어의 최대 스크롤 범위를 데이터그리드와 일치시킴
                if (Math.Abs(dgMaxScroll - headerMaxScroll) > 0.5)
                {
                    // 이를 위해 추가 여백 또는 패딩을 조정해야 할 수 있음
                    var content = _headerScrollViewer.Content as FrameworkElement;
                    if (content != null)
                    {
                        double requiredWidth = content.ActualWidth + (dgMaxScroll - headerMaxScroll);
                        if (requiredWidth > content.ActualWidth)
                        {
                            content.Width = requiredWidth;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"헤더 동기화 조정 오류: {ex.Message}");
            }
        }

        private void LimitHeaderScroll()
        {
            _headerScrollViewer.PreviewMouseWheel += (sender, e) => {
                if (_dataGridScrollViewer == null) return;

                // 현재 스크롤 위치
                double current = _headerScrollViewer.HorizontalOffset;
                double max = _dataGridScrollViewer.ScrollableWidth;

                // 끝에 도달했는지 확인
                if (current >= max - 1)
                {
                    // 추가 스크롤 차단
                    e.Handled = true;
                }
            };
        }

        // FrozenColumnCount 동적 변경 메서드
        public void SetFrozenColumnCount(int count)
        {
            try
            {
                EnsureHeaderSyncConsistency();

                if (count < 0) count = 0;

                // 현재 스크롤 위치 저장
                double currentOffset = 0;
                var dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                if (dataGridScrollViewer != null)
                {
                    currentOffset = dataGridScrollViewer.HorizontalOffset;
                }

                // 이벤트 핸들러 일시적으로 제거 (해제와 설정 모두 동일하게 처리)
                if (dataGridScrollViewer != null)
                {
                    dataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                }
                if (_headerScrollViewer != null)
                {
                    _headerScrollViewer.ScrollChanged -= HeaderScrollViewer_ScrollChanged;
                }

                // 고정 열 해제 시 특별 처리
                if (_frozenColumnCount > 0 && count == 0)
                {
                    // 고정 열 수 설정
                    _frozenColumnCount = 0;
                    _dataGrid.FrozenColumnCount = 0;

                    // 변환 완전히 제거 및 스타일 초기화
                    foreach (var element in _frozenHeaderElements)
                    {
                        // 명시적으로 새 TranslateTransform 생성하여 X=0으로 설정
                        element.RenderTransform = new TranslateTransform(0, 0);

                        // Z-Index 초기화
                        Panel.SetZIndex(element, 0);

                        // 헤더인 경우 추가 스타일 초기화
                        if (element is DataGridColumnHeader header)
                        {
                            // 원래 패딩으로 복원
                            header.ClearValue(Control.PaddingProperty);

                            // 배경 초기화 (필요한 경우)
                            header.ClearValue(Control.BackgroundProperty);

                            // 테두리 초기화 (필요한 경우)
                            header.ClearValue(Control.BorderThicknessProperty);
                            header.ClearValue(Control.BorderBrushProperty);
                        }
                    }
                    _frozenHeaderElements.Clear();

                    // UI 업데이트를 기다린 후 스크롤 위치 강제 동기화
                    _dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 스크롤 위치 복원 - 중요: 먼저 헤더를 설정한 다음 데이터그리드를 설정
                        _headerScrollViewer.ScrollToHorizontalOffset(0);
                        if (dataGridScrollViewer != null)
                        {
                            dataGridScrollViewer.ScrollToHorizontalOffset(0);
                        }

                        // 그 다음 원래 위치로 다시 스크롤
                        _headerScrollViewer.ScrollToHorizontalOffset(currentOffset);
                        if (dataGridScrollViewer != null)
                        {
                            dataGridScrollViewer.ScrollToHorizontalOffset(currentOffset);
                        }

                        // 이벤트 핸들러 다시 연결
                        _headerScrollViewer.ScrollChanged += HeaderScrollViewer_ScrollChanged;
                        if (dataGridScrollViewer != null)
                        {
                            dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                        }
                    }), System.Windows.Threading.DispatcherPriority.Render);

                    return; // 여기서 메서드 종료
                }
                else if (count > 0)
                {
                    // 고정 열 설정 (활성화) 로직
                    _frozenColumnCount = count; // 이 부분이 누락됨

                    if (_dataGrid != null)
                    {
                        _dataGrid.FrozenColumnCount = count;

                        // UI 업데이트를 위한 Dispatcher 사용
                        _dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                // 고정 헤더 요소 다시 초기화
                                InitializeFrozenHeaderElements();

                                // 스크롤 위치 복원을 위한 추가 지연
                                _dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _isUpdatingScroll = true; // 이벤트 핸들러 순환 방지

                                    try
                                    {
                                        // 우선 데이터그리드 스크롤 위치 복원
                                        if (dataGridScrollViewer != null)
                                        {
                                            dataGridScrollViewer.ScrollToHorizontalOffset(currentOffset);
                                        }

                                        // 헤더 스크롤 위치도 복원
                                        if (_headerScrollViewer != null)
                                        {
                                            _headerScrollViewer.ScrollToHorizontalOffset(currentOffset);
                                        }

                                        // 고정 헤더에 변환 적용
                                        ApplyFrozenHeaderTransform(currentOffset);

                                        // 이벤트 핸들러 다시 연결
                                        _headerScrollViewer.ScrollChanged += HeaderScrollViewer_ScrollChanged;
                                        if (dataGridScrollViewer != null)
                                        {
                                            dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
                                        }

                                        Console.WriteLine($"스크롤 위치 복원: {currentOffset}");
                                    }
                                    finally
                                    {
                                        _isUpdatingScroll = false;
                                    }
                                }), System.Windows.Threading.DispatcherPriority.Input);

                                Console.WriteLine($"FrozenColumnCount 변경됨: {count}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Dispatcher 콜백 오류: {ex.Message}");
                            }
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetFrozenColumnCount 오류: {ex.Message}");
            }
        }




        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T foundChild = null;
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    foundChild = FindVisualChild<T>(child);
                    if (foundChild != null)
                        break;
                }
            }
            return foundChild;
        }

        public void Detach()
        {
            if (_headerScrollViewer != null)
            {
                _headerScrollViewer.ScrollChanged -= HeaderScrollViewer_ScrollChanged;
            }
            if (_dataGrid != null)
            {
                if (_dataGridScrollViewer != null)
                {
                    _dataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                }
                else
                {
                    var dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                    if (dataGridScrollViewer != null)
                    {
                        dataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                    }
                }
                _dataGrid.Loaded -= DataGrid_Loaded;
            }
            _headerScrollViewer = null;
            _dataGridScrollViewer = null;
            _dataGrid = null;
            _frozenHeaderElements.Clear();
        }
    }

    //테이블 컬럼 171개 분리하여 관리시도
    public class Win_ord_Order_U_CodeView_Tab2 : BaseView
    {
        public string searchReqDate {get;set;}
        public string searchDate {get;set;}
        public string searchDataAcptDate {get;set;}
        public string installLocationCount {get;set;}
        public string electrDeliveryMethod {get;set;}      
        public string inspectionNeedYN {get;set;}
        public string addConstructCostSearch {get;set;}
        public string addConstructCost {get;set;}
        public string searchComments {get;set;}
        public string corpAcptNo {get;set;}
        public string superBeforeUseInspDate { get; set; }              //Tab4에 있던거 Tab2로 이동(우선Tab4에 있던거는 냅둠)
        public string superBeforeUseInspPrintDate { get; set; }         //Tab4에 있던거 Tab2로 이동(우선Tab4에 있던거는 냅둠)
        public string superUseInspReqDate { get; set; }                 //Tab4에 있던거 Tab2로 이동(우선Tab4에 있던거는 냅둠)
        public string corpApprovalDate {get;set;}
        public string corpEndDate {get;set;}
        public string corpLastEndDate {get;set;}
        public string corpComments {get;set;}
        public string localGovPermissionNo {get;set;}
        public string localGovBehaviorReportDate {get;set;}
        public string localGoComments { get; set; }
        public string beforeSearchConsultFilePath{get;set;}
        public string beforeSearchConsultFileName{get;set;}
        public string pictureEarthFilePath{get;set;}
        public string pictureEarthFileName{get;set;}
        public string drawFilePath{get;set;}
        public string drawFileName{get;set;}
        public string searchFilePath{get;set;}
        public string searchFileName{get;set;}
        public string searchChecksheetFilePath{get;set;}
        public string searchChecksheetFileName{get;set;}
        public string installLocationSheetFilePath{get;set;}
        public string installLocationSheetFileName{get;set;}
        public string localGoTaxFilePath{get;set;}
        public string localGoTaxFileName { get; set; }
        public string LocalGovProveFilePath { get; set; }
        public string LocalGovProveFileName { get; set; }


    }

    public class Win_ord_Order_U_CodeView_Tab3 : BaseView
    {
        public string kepElectrDeliveryMethod {get;set;}
        public string kepInstallLocationCount {get;set;}
        public string kepOutLineConstructContext {get;set;}
        public string kepInfraPayAmount {get;set;}
        public string kepManageInfraPayAmount {get;set;}
        public string kepManageInfraPayDate{get;set;}
        public string kepElectrReqDate {get;set;}
        public string kepInApprovalYN {get;set;}
        public string kepParentChildCapacity{get;set;}
        public string kepPowerSupplyCapacity { get; set; }
        public string kepInApprovalDate {get;set;}
        public string kepMeterInstallContext {get;set;}
        public string kepDamdangjaPhone {get;set;}
        public string kepCustomNo {get;set;}
        public string kepPaymentDate {get;set;}
        public string kepMeterInstallDate {get;set;}
        public string kepFaucetComments {get;set;}
        public string constrCustomID {get;set;}
        public string constrCustom { get; set; }
        public string constrOrderDate {get;set;}
        public string constrDate {get;set;}
        public string constrDelyReason {get;set;}
        public string constrCompleteDate {get;set;}
        public string constrComments {get;set;}
        public string electrSafeCheckDate {get;set;}
        public string electrSafeCheckSuppleContext {get;set;}
        public string electrSafeCheckLocation {get;set;}
        public string electrSafeCheckPrintDate { get; set; } 
        public string electrSafeCheckCost {get;set;}
        public string electrSafeCheckCostPayDate {get;set;}
        public string electrBeforeUseCheckReqDate {get;set;}
        public string electrBeforeUseCheckPrintDate {get;set;}
        public string electrBeforeUseCheckSuppleContext {get;set;}
        public string electrBeforeInspLocation {get;set;}
        public string electrBeforeInspReqDate {get;set;}
        public string electrBeforeInspPrintDate {get;set;}
        public string electrBeforeInspCost {get;set;}
        public string electrBeforeInspCostPayDate {get;set;}
        public string electrBeforeInspSuppleContext {get;set;}
        public string electrSafeCheckComments { get; set; }
     
        public string kepElectrLineFilePath{get;set;}
        public string kepElectrLineFileName{get;set;}
        public string kepFaucetAcptFilePath{get;set;}
        public string kepFaucetAcptFileName{get;set;}
        public string electrSafeInspPrintFilePath{get;set;}
        public string electrSafeInspPrintFileName{get;set;}
        public string electrBeforeUseCheckPrintFilePath{get;set;}
        public string electrBeforeUseCheckPrintFileName{get;set;}
        public string electrBeforeUseInspFilePath{get;set;}
        public string electrBeforeUseInspFileName{get;set;}
        public string electrKepAcptFilePath{get;set;}
        public string electrKepAcptFileName{get;set;}
        public string electrKepInfraPayBillFilePath{get;set;}
        public string electrKepInfraPayBillFileName{get;set;}
        public string electrUseContractFilePath{get;set;}
        public string electrUseContractFileName{get;set;}
        public string electrBeforeUseInspCostFilePath{get;set;}
        public string electrBeforeUseInspCostFileName { get; set; }
        public string electrCoWorkFilePath{get;set;}
        public string electrCoWorkFileName{get;set;}
        public string electrCostFilePath{get;set;}
        public string electrCostFileName { get; set; }
        public string electrTransCoUseFilePath { get; set; }
        public string electrTransCoUseFileName { get; set; }

    }

    public class Win_ord_Order_U_CodeView_Tab4 : BaseView
    {
       public string superCustomID{get;set;}
       public string superCustom{get;set;}
       public string superCostPayCustomID{get;set;}
       public string superCostPayCustom{get;set;}
       public string superCustomPhoneNo{get;set;}
       public string safeManageCustomID{get;set;}
       public string safeManageCustom{get;set;}
       public string safeManageCustomPhoneNo{get;set;}
       public string superSetCost{get;set;}
       public string superSetTaxPrintDate{get;set;}
       public string superUseInspPayCustomID{get;set;}
       public string superUseInspPayCustom{get;set;}
       public string superUseInspReqDate{get;set;}
       public string superBeforeUseInspDate{get;set;}
       public string superBeforeUseInspPrintDate{get;set;}
       public string superComments{get;set;}
       public string compReplyDate{get;set;}
       public string suppleContext{get;set;}
       public string suppleCompDate{get;set;}
       public string compSuppleReportContext{get;set;}
       public string compSuppleReportDate{get;set;}
       public string insurePrintDate{get;set;}
       public string compReportCompDate{get;set;}
       public string compReportComments{get;set;}
       public string accntComments { get; set; }

       public string superSetCheckFilePath{get;set;}
       public string superSetCheckFileName{get;set;}
       public string superBeforeUseInspectFilePath{get;set;}
       public string superBeforeUseInspectFileName{get;set;}
       public string compReportFileName { get; set; }
       public string compReportFilePath { get; set; }
       public string superCostFilePath{get;set;}
       public string superCostFileName{get;set;}
       public string safeManagerCertiFileName{get;set;}
       public string safeManagerCertiFilePath { get; set; }
       public string superReportFilePath{get;set;}
       public string superReportFileName{get;set;}
       public string insurePrintFilePath{get;set;}
       public string insurePrintFileName { get; set; }

    }
    public class Win_ord_Order_U_CodeView_Tab5 : BaseView
    {
        public string sketch1FilePath{get;set;}
        public string sketch1FileName{get;set;}
        public string sketch1FileAlias{get;set;}
        public string sketch2FilePath{get;set;}
        public string sketch2FileName{get;set;}
        public string sketch2FileAlias{get;set;}
        public string sketch3FilePath{get;set;}
        public string sketch3FileName{get;set;}
        public string sketch3FileAlias{get;set;}
        public string sketch4FilePath{get;set;}
        public string sketch4FileName{get;set;}
        public string sketch4FileAlias{get;set;}
        public string sketch5FilePath{get;set;}
        public string sketch5FileName{get;set;}
        public string sketch5FileAlias{get;set;}
        public string sketch6FilePath{get;set;}
        public string sketch6FileName{get;set;}
        public string sketch6FileAlias{get;set;}
        public string sketch7FilePath{get;set;}
        public string sketch7FileName{get;set;}
        public string sketch7FileAlias{get;set;}
        public string sketch8FilePath{get;set;}
        public string sketch8FileName{get;set;}
        public string sketch8FileAlias{get;set;}
        public string sketch9FilePath{get;set;}
        public string sketch9FileName{get;set;}
        public string sketch9FileAlias{get;set;}
        public string sketch10FilePath{get;set;}
        public string sketch10FileName{get;set;}
        public string sketch10FileAlias{get;set;}
        public string sketch11FilePath{get;set;}
        public string sketch11FileName{get;set;}
        public string sketch11FileAlias { get; set; }
        public string sketch12FilePath { get; set; }
        public string sketch12FileName { get; set; }
        public string sketch12FileAlias { get; set; }
        public string sketch13FilePath {get;set;}
        public string sketch13FileName {get;set;}
        public string sketch13FileAlias {get;set;}
        public string sketch14FilePath {get;set;}
        public string sketch14FileName {get;set;}
        public string sketch14FileAlias {get;set;}
        public string sketch15FilePath {get;set;}
        public string sketch15FileName {get;set;}
        public string sketch15FileAlias {get;set;}
        public string sketch16FilePath {get;set;}
        public string sketch16FileName {get;set;}
        public string sketch16FileAlias {get;set;}
        public string sketch17FilePath {get;set;}
        public string sketch17FileName {get;set;}
        public string sketch17FileAlias {get;set;}
        public string sketch18FilePath {get;set;}
        public string sketch18FileName {get;set;}
        public string sketch18FileAlias {get;set;}
        public string sketch19FilePath {get;set;}
        public string sketch19FileName {get;set;}
        public string sketch19FileAlias {get;set;}
        public string sketch20FilePath {get;set;}
        public string sketch20FileName {get;set;}
        public string sketch20FileAlias {get;set;}
        public string sketch21FilePath {get;set;}
        public string sketch21FileName {get;set;}
        public string sketch21FileAlias {get;set;}
        public string sketch22FilePath {get;set;}
        public string sketch22FileName {get;set;}
        public string sketch22FileAlias { get; set; }
    }
    public class Win_order_OrderColor_U_CodeView : BaseView
    {
        public int num { get; set; }
        public string  OrderID {get;set;}
        public string  OrderSeq {get;set;}
        public string  ArticleID {get;set;}
        public string  chargeOrderDate {get;set;}
        public string  chargeInwareDate {get;set;}
        public string  chargeInwareQty {get;set;}
        public string  chargeInwareLocation {get;set;}
        public string  canopyReqCustom {get;set;}
        public string  chargeModelHelmat {get;set;}
        public string  chargeModelinloc {get;set;}
        public string  chargeModelOneBody {get;set;}
        public string  chargeStandReqDate {get;set;}
        public string  chargeStandInwareDate {get;set;}
        public string  Comments {get;set;}
        public string  CreateDate {get;set;}
        public string  CreateUserID {get;set;}
        public string  LastUpdateDate {get;set;}
        public string  LastUpdateUserID { get; set; }


    }

    public class ArticleData : BaseView
    {
        public string articleID { get; set; }
        public string article { get; set; }
        //public string ThreadID { get; set; }
        //public string thread { get; set; }
        //public string StuffWidth { get; set; }
        //public string DyeingID { get; set; }
        public string weight { get; set; }
        public string spec { get; set; }
        //public string ArticleGrpID { get; set; }
        public string articleTypeID { get; set; }
        public string buyerArticleNo { get; set; }
        public string unitPrice { get; set; }
        //public string UnitPriceClss { get; set; }
        public string unitPriceTypeID { get; set; }
        public string unitTypeID { get; set; }

        public string productTypeID { get; set; }
        //public string ProcessName { get; set; }
        //public string HSCode { get; set; }
        public string outUnitPrice { get; set; }
        public string codeName { get; set; }
        //public string BuyerModelID { get; set; }
        //public string BuyerModel { get; set; }
    }

    public class OrderExcel : BaseView
    {
        public string CustomID { get; set; }
        public string Model { get; set; }
        public string BuyerArticleNo { get; set; }
        public string Article { get; set; }
        public string UnitClss { get; set; }
        public string OrderQty { get; set; }
    }


}

