﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
//using WizMes_EVC.Order.Pop;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WizMes_EVC.Order.Pop;
using WizMes_EVC.PopUp;
using WizMes_EVC.PopUP;
using Excel = Microsoft.Office.Interop.Excel;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Order_U
'** 설명       : 수주등록
'** 작성일자   : 2024.12.31
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.12.31, 최대현,                              최초생성 나다음에듀 order활용
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

        string strFlag = string.Empty;
        string orderID_global = string.Empty;
        int rowNum = 0;
        int intFlag = 0;
        bool tab2_clicked = false;
        bool tab3_clicked = false;
        bool tab4_clicked = false;

        //Win_ord_Pop_PreOrder preOrder = new Win_ord_Pop_PreOrder();

        private Win_ord_Pop_PreOrder_Q preOrder;
        private Win_ord_Pop_PreEstimate_Q preEstimate;

        Win_ord_Order_U_CodeView_dgdMain OrderView = new Win_ord_Order_U_CodeView_dgdMain();

        //계약내용 쪽
        ObservableCollection<Win_ord_Order_U_CodeView_OrderItemList_Nadaum> ovcOrder_OrderItemList
        = new ObservableCollection<Win_ord_Order_U_CodeView_OrderItemList_Nadaum>();

        ObservableCollection<Win_ord_Order_U_CodeView_OrderColor_Nadaum> ovcOrder_OrderColor
        = new ObservableCollection<Win_ord_Order_U_CodeView_OrderColor_Nadaum>();

        ObservableCollection<Win_order_Order_U_CodView_dgdAcc> ovcOrder_Acc
        = new ObservableCollection<Win_order_Order_U_CodView_dgdAcc>();

        ObservableCollection<Win_order_Order_U_CodView_localGov> ovcOrder_localGov
        = new ObservableCollection<Win_order_Order_U_CodView_localGov>();

        private List<ScrollSyncHelper> scrollHelpers = new List<ScrollSyncHelper>();

        ObservableCollection<CodeView> ovcOrderTypeAcc = null;

        ArticleData articleData = new ArticleData();
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
            scrollHelpers.Add(new ScrollSyncHelper(dgdAccSV, dgdAcc));
            SetupLastColumnResize(dgdAcc, dgdAccSV, grdAcc);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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


            //if (!string.IsNullOrEmpty(MainWindow.orderID))
            //{
            //    intFlag = 1;
            //    tblOrderID.Text = MainWindow.orderID;
            //    lblDateSrh_MouseLeftButtonDown(null, null);

            //    FillGrid();

            //    intFlag = 0;
            //    tblOrderID.Text = string.Empty;

            //    if (dgdMain.Items.Count > 0) dgdMain.SelectedIndex = 0;
            //    MainWindow.orderID = string.Empty;
            //}



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
            ObservableCollection<CodeView> ovcElecDeliMethSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboElecDeliMethSrh.ItemsSource = ovcElecDeliMethSrh;
            cboElecDeliMethSrh.DisplayMemberPath = "code_name";
            cboElecDeliMethSrh.SelectedValuePath = "code_id";
            cboElecDeliMethSrh.SelectedIndex = 0;

            //시공지자체 전기조달
            ObservableCollection<CodeView> ovcElecDeliMeth = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboElectrDeliveryMethodID.ItemsSource = ovcElecDeliMeth;
            cboElectrDeliveryMethodID.DisplayMemberPath = "code_name";
            cboElectrDeliveryMethodID.SelectedValuePath = "code_id";
            cboElectrDeliveryMethodID.SelectedIndex = 0;

            //한전전기조달
            ObservableCollection<CodeView> ovcKepElecDeliMeth = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboKepElectrDeliveryMethodID.ItemsSource = ovcKepElecDeliMeth;
            cboKepElectrDeliveryMethodID.DisplayMemberPath = "code_name";
            cboKepElectrDeliveryMethodID.SelectedValuePath = "code_id";
            cboKepElectrDeliveryMethodID.SelectedIndex = 0;


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

            //인입승인여부(YN)
            ObservableCollection<CodeView> ovcInspectionNeedYN = ComboBoxUtil.Instance.Direct_SetComboBox(strInspectionNeedYn);
            cboInspectionNeedYN.ItemsSource = ovcInspectionNeedYN;
            cboInspectionNeedYN.DisplayMemberPath = "code_name";
            cboInspectionNeedYN.SelectedValuePath = "code_id";
            cboInspectionNeedYN.SelectedIndex = 0;

            //감리배치여부(YN)
            List<string[]> strKepInApprove = new List<string[]>();
            string[] strApprove = { "Y", "Y" };
            string[] strDenied = { "N", "N" };
            strKepInApprove.Add(strApprove);
            strKepInApprove.Add(strDenied);

            //감리배치여부
            ObservableCollection<CodeView> ovcKepInApprove = ComboBoxUtil.Instance.Direct_SetComboBox(strKepInApprove);
            cboKepInApprovalYN.ItemsSource = ovcKepInApprove;
            cboKepInApprovalYN.DisplayMemberPath = "code_name";
            cboKepInApprovalYN.SelectedValuePath = "code_id";
            cboKepInApprovalYN.SelectedIndex = 0;



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
            }
            else
            {
                chkManagerCustomIdSrh.IsChecked = true;
                txtManagerCustomIdSrh.IsEnabled =true;
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

        //영업회사
        private void lblSalesCustomIdSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkSalesCustomIdSrh.IsChecked == true)
            {
                chkSalesCustomIdSrh.IsChecked = false;
                txtSalesCustomIdSrh.IsEnabled = false;
            }
            else
            {
                chkSalesCustomIdSrh.IsChecked = true;
                txtSalesCustomIdSrh.IsEnabled = true;
            }
        }

        //영업회사
        private void chkSalesCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkSalesCustomIdSrh.IsChecked == true)
            {
                txtSalesCustomIdSrh.IsEnabled = true;
                btnSalesCustomIdSrh.IsEnabled = true;
            }
            else
            {
                txtSalesCustomIdSrh.IsEnabled = false;
                btnSalesCustomIdSrh.IsEnabled = false;
            }
        }
        //영업회사
        private void txtSalesCustomIdSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSalesCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //영업회사
        private void btnSalesCustomIdSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSalesCustomIdSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

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

        //비고 라벨
        private void lblInstallLocationAddCommentsSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInstallLocationAddCommentsSrh.IsChecked == true)
            {
                chkInstallLocationAddCommentsSrh.IsChecked = false;
                txtInstallLocationAddCommentsSrh.IsEnabled = false;
            }
            else
            {
                chkInstallLocationAddCommentsSrh.IsChecked = true;
                txtInstallLocationAddCommentsSrh.IsEnabled = true;
            }
        }

        //비고 체크박스
        private void chkInstallLocationAddCommentsSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkInstallLocationAddCommentsSrh.IsChecked == true)
            {
                chkInstallLocationAddCommentsSrh.IsChecked = true;
                txtInstallLocationAddCommentsSrh.IsEnabled = true;
            }
            else
            {
                chkInstallLocationAddCommentsSrh.IsChecked = false;
                txtInstallLocationAddCommentsSrh.IsEnabled = false;
            }
        }

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

        //품명 라벨 클릭
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkArticleSrh.IsChecked == true)
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
            if(chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = true;
                txtArticleSrh.IsEnabled = true;
            }
            else
            {
                chkArticleSrh.IsChecked= false;
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
            lblMsg.Visibility = Visibility.Visible;
            dgdMain.IsHitTestVisible = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "I";
            chkEoAddSrh.IsEnabled = false;
            orderID_global = string.Empty;
            //btnPreOrder.IsEnabled = true;
            //tabBasicData.Focus();            
            lstFilesName.Clear();          
            ClearGrdInput();      
            this.DataContext = new object();
            rowAddAccnt();

            //혹시 모르니까 납기일자의 체크박스가 체크되어 있을 수도 있으니까 해제

            CantBtnControl();
            //UncheckDatePicker();
            SetDatePickerToday();
            SetComboBoxIndexZero();
            setFTP_Tag_EmptyString();

            //계약기간 오늘~금월 마지막일
            //dtpJobFromDate.SelectedDate = DateTime.Today;                                               //계약시작일
            //dtpJobToDate.SelectedDate = DateTime.Today.AddMonths(1).AddDays(-DateTime.Today.Day);       //계약종료일   

            tbkMsg.Text = "자료 입력 중";
            rowNum = Math.Max(0, dgdMain.SelectedIndex);
       
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            OrderView = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_dgdMain;
            //btnPreOrder.IsEnabled = false;
            if (OrderView != null)
            {
                //rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                strFlag = "U";
                CantBtnControl();
                PrimaryKey = OrderView.orderId;
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

                //나다음 생산이 없는거 같음...
                //강의료정산 화면에서 orderID사용중
                if (OrderView != null)
                {

                    if (CheckDelete(OrderView))
                    {

                        if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                                rowNum = dgdMain.SelectedIndex;

                            FTP_RemoveDir(OrderView.orderId);

                            if (DeleteData(OrderView.orderId))
                            {
                                rowNum = Math.Max(0, rowNum - 1);
                                re_Search(rowNum);
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
                //로직
                re_Search(rowNum);
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
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
                }

            }
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
                if (dgdAcc.Items.Count > 0) DataGrid_LostFocus_Calculate(dgdAcc, new RoutedEventArgs());

                //로직
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    lblMsg.Visibility = Visibility.Hidden;
                    dgdMain.IsHitTestVisible = true;
                    PrimaryKey = string.Empty;
                    orderID_global = string.Empty;       
                    rowNum = strFlag == "I" ? rowNum + 1 : strFlag == "U" ? rowNum : rowNum - 1;
                    chkEoAddSrh.IsChecked = false;
                    re_Search(rowNum);               
                    MessageBox.Show("저장이 완료되었습니다.");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

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
            chkEoAddSrh.IsEnabled = true;
            chkEoAddSrh.IsChecked = false;
            //btnPreOrder.IsEnabled = false;

            if (strFlag.Equals("U"))
            {
                re_Search(rowNum);
            }
            else
            {
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

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = PrimaryKey.Equals(string.Empty) ?
                    selectedIndex : SelectItem(PrimaryKey, dgdMain);
            }
            else
                DataContext = new object();

            //CalculGridSum();
        }

        //실조회
        private void FillGrid()
        {
         

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
                sqlParameter.Add("ChkSalesCustomId", chkSalesCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SalesCustomId", chkSalesCustomIdSrh.IsChecked == true ? (txtSalesCustomIdSrh.Tag != null ? txtSalesCustomIdSrh.Tag.ToString() : "") : "");                

                // 품목
                sqlParameter.Add("ChkArticleId", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleId", chkArticleSrh.IsChecked == true ? (txtArticleSrh.Text == string.Empty ? "" : chkArticleSrh.Tag.ToString()) : "");

                // 마감포함
                sqlParameter.Add("ChkCloseYn", chkCloseClssSrh.IsChecked == true ? 1 : 0);  

                // 지역구분
                sqlParameter.Add("ChkZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? cboZoneGbnIdSrh.SelectedValue : "");

                // 전기조달방법
                sqlParameter.Add("ChkElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? cboElecDeliMethSrh.SelectedValue.ToString() : "");

                // 국소명
                sqlParameter.Add("ChkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstallLocationSrh.Text : "");

                //비고
                sqlParameter.Add("ChkInstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? txtInstallLocationAddCommentsSrh.Text : "");

                //계약진행관리에서 넘어왔을 때 바로 조회용도 textblock에 적어놓고 hidden처리함
                //sqlParameter.Add("orderID", tblOrderID.Text.Trim());

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
                                orderId  = dr["orderId"].ToString(),
                                estID  = dr["estID"].ToString(),
                                orderNo = dr["orderNo"].ToString(),
                                saleCustom  = dr["saleCustom"].ToString(),
                                saleCustomID  = dr["saleCustomID"].ToString(),

                                managerCustom  = dr["managerCustom"].ToString(),
                                managerCustomID = dr["managerCustomID"].ToString(),
                                searchCustomID  = dr["searchCustomID"].ToString(),
                                searchCustom  = dr["searchCustom"].ToString(),
                                manageCustomAcptDate  = DateTypeHyphen(dr["manageCustomAcptDate"].ToString()),
                                manageCustomConfirmDate = DateTypeHyphen(dr["manageCustomConfirmDate"].ToString()),

                                installLocation = dr["installLocation"].ToString(),
                                installLocationPart  = dr["installLocationPart"].ToString(),
                                InstallLocationPhone = dr["InstallLocationPhone"].ToString(),
                                articleList = dr["articleList"].ToString(),
                                closeYn  = dr["closeYn"].ToString(),

                                orderAmount  = dr["orderAmount"].ToString(),
                                acptDate  = dr["acptDate"].ToString(),
                                installLocationAddComments = dr["installLocationAddComments"].ToString(),
                                installLocationAddress = dr["installLocationAddress"].ToString(),
                                houseHoldCount  = stringFormatN0(dr["houseHoldCount"]),

                                carParkingCount  = stringFormatN0(dr["carParkingCount"]),
                                alreadyManageCustom  = dr["alreadyManageCustom"].ToString(),
                                alreadyManageCustomID  = dr["alreadyManageCustomID"].ToString(),
                                installLocationComments = dr["installLocationComments"].ToString(),
                                alReadyChargeCount  = dr["alReadyChargeCount"].ToString(),

                                contractToDate  = DateTypeHyphen(dr["contractToDate"].ToString()),
                                contractFromDate  = DateTypeHyphen(dr["contractFromDate"].ToString()),
                                openReqDate  = DateTypeHyphen(dr["openReqDate"].ToString()),
                                openDate = DateTypeHyphen(dr["openDate"].ToString()),
                                damdangjaName  = dr["damdangjaName"].ToString(),

                                damdangjaEMail  = dr["damdangjaEMail"].ToString(),
                                damdangjaPhone  = dr["damdangjaPhone"].ToString(),
                                electrCarCount  = stringFormatN0(dr["electrCarCount"]),
                                reqChargeCount  = stringFormatN0(dr["reqChargeCount"]),
                                saledamdangjaPhone  = dr["saledamdangjaPhone"].ToString(),

                                saleCustomAddWork  = dr["saleCustomAddWork"].ToString(),
                                salegift  = dr["salegift"].ToString(),
                                mtrAmount  = stringFormatN0(dr["mtrAmount"]),
                                mtrShippingCharge = stringFormatN0(dr["mtrShippingCharge"]),
                                mtrPriceUnitClss = dr["mtrPriceUnitClss"].ToString(),

                                mtrCanopyInwareInfo = dr["mtrCanopyInwareInfo"].ToString(),
                                mtrCanopyOrderAmount = stringFormatN0(dr["mtrCanopyOrderAmount"]),

                                contractFileName = dr["contractFileName"].ToString(),
                                contractFilePath = dr["contractFilePath"].ToString(),

                             
                            };

                            sumAmount += Convert.ToInt32(RemoveComma(OrderCodeView.orderAmount));


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
            if(dgdAcc.Items.Count > 0) ovcOrder_Acc.Clear();

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
                                chargeInwareDate = DateTypeHyphen(dr["chargeInwareDate"].ToString()),
                                chargeInwareQty = stringFormatN0(dr["chargeInwareQty"]),
                                chargeInwareLocation = dr["chargeInwareLocation"].ToString(),
                                canopyReqCustom = dr["canopyReqCustom"].ToString(),
                                chargeModelHelmat = dr["chargeModelHelmat"].ToString(),
                                chargeModelinloc = dr["chargeModelinloc"].ToString(),
                                chargeModelOneBody = dr["chargeModelOneBody"].ToString(),
                                chargeStandReqDate = DateTypeHyphen(dr["chargeStandReqDate"].ToString()),
                                chargeStandInwareDate= DateTypeHyphen(dr["chargeStandInwareDate"].ToString()),
                                Comments = dr["Comments"].ToString(),
               
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
                                electrDeliveryMethodID = dr["electrDeliveryMethodID"].ToString(),
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
                                superBeforeUseInspDate = DateTypeHyphen(dr["superBeforeUseInspDate"].ToString()),
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
                MessageBox.Show("오류 발생(fillgrid_OrderItemList), 오류 내용 : " + ex.ToString());
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
                                    kepElectrDeliveryMethodID = dr["kepElectrDeliveryMethodID"].ToString(),
                                    kepInstallLocationCount = stringFormatN0(dr["kepInstallLocationCount"]),
                                    kepOutLineConstructContext = dr["kepOutLineConstructContext"].ToString(),
                                    kepInfraPayAmount = stringFormatN0(dr["kepInfraPayAmount"]),
                                    kepManageInfraPayAmount = stringFormatN0(dr["kepManageInfraPayAmount"]),
                                    kepElectrReqDate = dr["kepElectrReqDate"].ToString(),
                                    kepInApprovalYN = dr["kepInApprovalYN"].ToString(),
                                    kepInApprovalDate = dr["kepInApprovalDate"].ToString(),
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
                                    electrCoWorkFilePath = dr["electrCoWorkFilePath"].ToString(),
                                    electrCoWorkFileName = dr["electrCoWorkFileName"].ToString(),
                                    electrCostFilePath = dr["electrCostFilePath"].ToString(),
                                    electrCostFileName  = dr["electrCostFileName"].ToString(),

                            };

                            AutoBindDataToControls(tab3Data, grd3);                            

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
                                superCostPayCustomID = dr["superCostPayCustomID"].ToString(),
                                superCostPayCustom = dr["superCostPayCustom"].ToString(),
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
                                superCostFilePath = dr["superCostFilePath"].ToString(),
                                superCostFileName = dr["superCostFileName"].ToString(),
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
                MessageBox.Show("오류 발생(fillgrid_OrderItemList), 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void fillGridTab4_Accnt(string orderId)
        {
            if(dgdAccnt.Items.Count > 0) dgdAccnt.Items.Clear();    

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

                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {

                            var accntList = new Win_order_Order_U_CodView_dgdAccnt
                            {
                                column1Date = DateTypeHyphen(dr["column1Date"].ToString()),
                                column2Amount = stringFormatN0(dr["column2Amount"]),
                                column3Amount = stringFormatN0(dr["column3Amount"]),
                                column4Amount = stringFormatN0(dr["column4Amount"]),
                                column5Amount = stringFormatN0(dr["column5Amount"]),
                                column6Amount = stringFormatN0(dr["column6Amount"]),
                                column7Comment = dr["column3Comment"].ToString()

                            };

                            if (accntList.column2Amount == "0") accntList.column2Amount = "";

                            dgdAccnt.Items.Add(accntList);
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


        private string TimeTypeColon(string DigitsTime)
        {
            string pattern1 = @"(\d{2})(\d{2})";

            if (DigitsTime.Length == 4)
            {
                DigitsTime = Regex.Replace(DigitsTime, pattern1, "$1:$2");
            }

            return DigitsTime;
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

        //그리드 하단 합계 표시
        private void CalculGridSum()
        {
            Int64 numYDS = 0;
            double numTotal = 0;

            //txtOrderCount.Text = string.Format("{0:N0}", dgdMain.Items.Count) + " 건";
            if (dgdMain.Items.Count > 0)
            {
                Win_ord_Order_U_CodeView WinOrder = new Win_ord_Order_U_CodeView();

                for (int i = 0; i < dgdMain.Items.Count; i++)
                {
                    WinOrder = dgdMain.Items[i] as Win_ord_Order_U_CodeView;

                    if (WinOrder.UnitClss.Equals("0"))
                    {
                        numYDS += long.Parse(lib.CheckNullZero(WinOrder.OrderQty.Replace(",", "")));
                        numTotal += double.Parse(lib.CheckNullZero(WinOrder.UnitPrice.Replace(",", "")));
                    }
                    else
                    {
                        numYDS += long.Parse(lib.CheckNullZero(WinOrder.OrderQty.Replace(",", "")));
                        numTotal += double.Parse(lib.CheckNullZero(WinOrder.UnitPrice.Replace(",", "")));
                    }
                }
            }

            //txtOrderYds.Text = string.Format("{0:N0}", numYDS) + " EA";
            //txtOrderAmount.Text = string.Format("{0:N0}", numTotal) + " 원";
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
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData() && CheckContractData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();

                    sqlParameter.Add("orderID", string.IsNullOrEmpty(txtOrderID.Text) ? "" : txtOrderID.Text);
                    sqlParameter.Add("orderNo", string.IsNullOrEmpty(txtOrderNo.Text) ? "" : txtOrderNo.Text);
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
                    sqlParameter.Add("installLocationPart", txtInstallLocationPart.Text);
                    sqlParameter.Add("carParkingCount", RemoveComma(txtCarParkingCount.Text, true));
                    sqlParameter.Add("alreadyManageCustomID", txtAlreadyManageCustomID.Tag != null ? txtAlreadyManageCustomID.Tag.ToString() : "");
                    sqlParameter.Add("electrCarCount", RemoveComma(txtElectrCarCount.Text, true));
                    sqlParameter.Add("installLocationComments", txtInstallLocationComments.Text);
                    sqlParameter.Add("alreadyChargeCount", RemoveComma(txtAlReadyChargeCount.Text, true));
                    sqlParameter.Add("contractFromDate", IsDatePickerNull(dtpContractFromDate) ? "" : ConvertDate(dtpContractFromDate));
                    sqlParameter.Add("contractToDate", IsDatePickerNull(dtpContractToDate) ? "" : ConvertDate(dtpContractToDate));
                    sqlParameter.Add("reqChargeCount", RemoveComma(txtReqChargeCount.Text, true));
                    sqlParameter.Add("openDate", IsDatePickerNull(dtpOpenDate) ? "" : ConvertDate(dtpOpenDate));
                    sqlParameter.Add("openReqDate", IsDatePickerNull(dtpOpenDate) ? "" : ConvertDate(dtpOpenReqDate));
                    sqlParameter.Add("damdangjaName", txtDamdangjaName.Text);
                    sqlParameter.Add("damdangjaEmail", txtDamdangjaEMail.Text);
                    sqlParameter.Add("damdangjaPhone", txtDamdangjaPhone.Text);
                    sqlParameter.Add("installLocationAddComments", txtInstallLocationAddComments.Text);
                    sqlParameter.Add("saledamdangjaPhone", txtSaledamdangjaPhone.Text);
                    sqlParameter.Add("saleCustomAddWork", RemoveComma(txtSaleCustomAddWork.Text, true));
                    sqlParameter.Add("salegift",txtsalegift.Text);
                    sqlParameter.Add("mtrAmount", (int)RemoveComma(txtdgdAccTotal.Text,true) + (int)RemoveComma(txtMtrCanopyOrderAmount.Text,true));
                    sqlParameter.Add("mtrShippingCharge", RemoveComma(txtMtrShippingCharge.Text, true));
                    sqlParameter.Add("mtrPriceUnitClss", cboMtrPriceUnitClss.SelectedValue != null ? cboMtrPriceUnitClss.SelectedValue.ToString() : "");
                    sqlParameter.Add("mtrCanopyInwareInfo", txtMtrCanopyInwareInfo.Text);
                    sqlParameter.Add("mtrCanopyOrderAmount", RemoveComma(txtMtrCanopyOrderAmount.Text,true));
                        
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
                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? sGetID : txtOrderID.Text);
                        sqlParameter.Add("searchReqDate", IsDatePickerNull(dtpSearchReqDate) ? "" : ConvertDate(dtpSearchReqDate));
                        sqlParameter.Add("searchDate", IsDatePickerNull(dtpSearchDate) ? "" : ConvertDate(dtpSearchDate));
                        sqlParameter.Add("searchDataAcptDate", IsDatePickerNull(dtpSearchDataAcptDate) ? "" : ConvertDate(dtpSearchDataAcptDate));
                        sqlParameter.Add("installLocationCount", RemoveComma(txtInstallLocationCount.Text,true));
                        sqlParameter.Add("electrDeliveryMethodID", cboElectrDeliveryMethodID.SelectedValue != null ? cboElectrDeliveryMethodID.SelectedValue.ToString() : "");
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
                        sqlParameter.Add("superBeforeUseInspDate", ConvertDate(dtpSuperBeforeUseInspDate));
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
                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? sGetID : txtOrderID.Text);
                        sqlParameter.Add("kepElectrDeliveryMethodID",cboKepElectrDeliveryMethodID.SelectedValue != null ? cboKepElectrDeliveryMethodID.SelectedValue.ToString() : "");
                        sqlParameter.Add("kepInstallLocationCount",RemoveComma(txtKepInstallLocationCount.Text, true));
                        sqlParameter.Add("kepOutLineConstructContext",txtKepOutLineConstructContext.Text);
                        sqlParameter.Add("kepInfraPayAmount",RemoveComma(txtKepInfraPayAmount.Text, true));
                        sqlParameter.Add("kepManageInfraPayAmount",RemoveComma(txtKepManageInfraPayAmount.Text, true));
                        sqlParameter.Add("kepElectrReqDate", IsDatePickerNull(dtpKepElectrReqDate) ? "" : ConvertDate(dtpKepElectrReqDate));
                        sqlParameter.Add("kepInApprovalYN", cboKepInApprovalYN.SelectedValue != null ? cboKepInApprovalYN.SelectedValue.ToString() : "");
                        sqlParameter.Add("kepInApprovalDate", IsDatePickerNull(dtpKepInApprovalDate) ? "" : ConvertDate(dtpKepInApprovalDate));
                        sqlParameter.Add("kepMeterInstallContext", txtKepMeterInstallContext.Text);
                        sqlParameter.Add("kepDamdangjaPhone", txtDamdangjaPhone.Text);
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
                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? sGetID : txtOrderID.Text);
                        sqlParameter.Add("superCustomID", txtSuperCustomID.Tag != null ? txtSuperCustomID.Tag.ToString() : "");
                        sqlParameter.Add("superCostPayCustomID", txtSuperCostPayCustomID.Tag !=null? txtSuperCostPayCustomID.Tag.ToString() : "");
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
                            for (int i = 0; i < dgdAccnt.Items.Count; i++)
                            {
                                var accntItem = dgdAccnt.Items[i] as Win_order_Order_U_CodView_dgdAccnt;

                                switch (i)
                                {
                                    case 0:
                                        sqlParameter.Add("accntMgrWorkPreTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkPreAmount", RemoveComma(accntItem.column2Amount, true));
                                        //sqlParameter.Add("accntMgrWorkPreSubsidyAmount", RemoveComma(accntItem.column3Amount, true));
                                        //sqlParameter.Add("accntMgrWorkPreInvestAmount", RemoveComma(accntItem.column4Amount, true));
                                        //sqlParameter.Add("accntMgrWorkPreSellAmount", RemoveComma(accntItem.column5Amount, true));
                                        //sqlParameter.Add("accntMgrWorkPreLeftAmount	", RemoveComma(accntItem.column6Amount, true));
                                        sqlParameter.Add("accntMgrWorkPreAmountComments", accntItem.column7Comment);
                                        break;
                                    case 1:
                                        sqlParameter.Add("accntMgrtWorkAfterTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkAfterSubsidy", RemoveComma(accntItem.column2Amount, true));
                                        //sqlParameter.Add("accntMgrWorkInvestAmount", RemoveComma(accntItem.column3Amount, true));
                                        //sqlParameter.Add("accntMgrWorkSellAmount", RemoveComma(accntItem.column4Amount, true));
                                        //sqlParameter.Add("accntMgrWorkAfterAmount", RemoveComma(accntItem.column5Amount, true));
                                        //sqlParameter.Add("accntMgrWorkAfterLeftAmount", RemoveComma(accntItem.column6Amount, true));
                                        sqlParameter.Add("accntMgrWorkAfterAmountComments", accntItem.column7Comment);
                                        break;
                                    case 2:
                                        sqlParameter.Add("accntMgrWorkTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntMgrWorkSubsidyAmount", RemoveComma(accntItem.column2Amount, true));
                                        //sqlParameter.Add("accntMgrWorkInvestAmount", RemoveComma(accntItem.column3Amount, true));
                                        //sqlParameter.Add("accntMgrWorkSellAmount", RemoveComma(accntItem.column4Amount, true));
                                        //sqlParameter.Add("accntMgrWorkAmount", RemoveComma(accntItem.column5Amount, true));
                                        //sqlParameter.Add("accntMgrWorkLeftAmount", RemoveComma(accntItem.column6Amount, true));
                                        sqlParameter.Add("accntMgrWorkAmountComments", accntItem.column7Comment);
                                        break;
                                    case 3:
                                        sqlParameter.Add("accntWorkTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkSubsidyAmount", RemoveComma(accntItem.column2Amount, true));
                                        //sqlParameter.Add("accntWorkInvestAmount", RemoveComma(accntItem.column3Amount, true));
                                        //sqlParameter.Add("accntWorkSellAmount", RemoveComma(accntItem.column4Amount, true));
                                        //sqlParameter.Add("accntWorkAmount", RemoveComma(accntItem.column5Amount, true));
                                        //sqlParameter.Add("accntWorkLeftAmount", RemoveComma(accntItem.column6Amount, true));
                                        sqlParameter.Add("accntWorkComments", accntItem.column7Comment);
                                        break;
                                    case 4:
                                        sqlParameter.Add("accntSaleTaxPrintDate", RemoveHyphen(accntItem.column1Date));
                                        sqlParameter.Add("accntWorkSubsidyAmount", RemoveComma(accntItem.column2Amount, true));
                                        //sqlParameter.Add("accntWorkInvestAmount", RemoveComma(accntItem.column3Amount, true));
                                        //sqlParameter.Add("accntWorkSellAmount", RemoveComma(accntItem.column4Amount, true));
                                        //sqlParameter.Add("accntWorkAmount", RemoveComma(accntItem.column5Amount, true));
                                        //sqlParameter.Add("accntWorkLeftAmount", RemoveComma(accntItem.column6Amount, true));
                                        sqlParameter.Add("accntSaleComments", accntItem.column7Comment);
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

                    for(int i = 0; i < dgdAcc.Items.Count; i++)
                    {
                        var accItem = dgdAcc.Items[i] as Win_order_Order_U_CodView_dgdAcc;

                        sqlParameter.Clear();
                        sqlParameter.Add("orderID", strFlag == "I" ? sGetID : txtOrderID.Text);
                        sqlParameter.Add("orderSeq", i+1);
                        sqlParameter.Add("articleID", accItem.articleID.Trim() != string.Empty ? accItem.articleID : "");
                        sqlParameter.Add("orderTypeID", accItem.orderTypeID.Trim() != string.Empty ? accItem.orderTypeID : "");
                        sqlParameter.Add("chargeInwareUnitPrice", RemoveComma(accItem.chargeInwareUnitPrice,true));
                        sqlParameter.Add("chargeOrderDate", RemoveHyphen(accItem.chargeOrderDate));
                        sqlParameter.Add("chargeInwareDate", RemoveHyphen(accItem.chargeInwareDate));
                        sqlParameter.Add("chargeInwareQty", RemoveComma(accItem.chargeInwareQty, true));
                        sqlParameter.Add("chargeInwareLocation", accItem.chargeInwareLocation != null ? accItem.chargeInwareLocation : "");
                        sqlParameter.Add("canopyReqCustom", accItem.canopyReqCustom != null ? accItem.canopyReqCustom : "");
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

                    if(tab2_clicked == true)
                    {
                        for(int i = 0; i < dgdLocalGov.Items.Count; i++)
                        {
                            var localGovItem = dgdLocalGov.Items[i] as Win_order_Order_U_CodView_localGov;

                            sqlParameter.Clear();
                            sqlParameter.Add("orderID", strFlag == "I" ? sGetID : txtOrderID.Text);
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



                    if (!PrimaryKey.Trim().Equals(""))
                    {
                        if (deleteListFtpFile.Count > 0)
                        {
                            foreach (string[] str in deleteListFtpFile)
                            {
                                FTP_RemoveFile(PrimaryKey + "/" + str[0]);
                            }
                        }

                        if (listFtpFile.Count > 0)
                        {
                            FTP_Save_File(listFtpFile, PrimaryKey);
                        }


                        UpdateDBFtp(PrimaryKey); // 리스트 갯수가 0개 이상일때 해버리면, 수정시에 저장이 안됨
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

        private bool UpdateDBFtp(string orderID)
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
                sqlParameter.Add("orderID", orderID);

                //tab1
                sqlParameter.Add("ContractFileName", txtContractFileName.Text.Trim() != "" ? txtContractFileName.Text : "");
                sqlParameter.Add("ContractFilePath", txtContractFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                //tab2
                sqlParameter.Add("beforeSearchConsultFileName", txtBeforeSearchConsultFileName.Text.Trim() != "" ? txtBeforeSearchConsultFileName.Text : "");
                sqlParameter.Add("beforeSearchConsultFilePath", txtBeforeSearchConsultFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("pictureEarthFileName", txtPictureEarthFileName.Text.Trim() != "" ? txtPictureEarthFileName.Text : "");
                sqlParameter.Add("pictureEarthFilePath", txtPictureEarthFileName.Tag !=null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("drawFileName", txtDrawFileName.Text.Trim() != "" ? txtDrawFileName.Text : "");
                sqlParameter.Add("drawFilePath", txtDrawFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("searchFileName", txtSearchFileName.Text.Trim() != "" ? txtSearchFileName.Text : "");
                sqlParameter.Add("searchFilePath", txtSearchFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("searchChecksheetFileName", txtSearchChecksheetFile.Text.Trim() != "" ? txtSearchChecksheetFile.Text : "");
                sqlParameter.Add("searchChecksheetFilePath", txtSearchChecksheetFile.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("installLocationSheetFileName", txtInstallLocationSheetFile.Text.Trim() != "" ? txtInstallLocationSheetFile.Text : "");
                sqlParameter.Add("installLocationSheetFilePath", txtInstallLocationSheetFile.Tag != null ? "/ImageData/Order/" + orderID : "");
               
                sqlParameter.Add("localGoTaxFileName", txtLocalGoTaxFile.Text.Trim() != "" ? txtLocalGoTaxFile.Text : "");
                sqlParameter.Add("localGoTaxFilePath", txtLocalGoTaxFile.Tag != null ? "/ImageData/Order/" + orderID : "");

                //tab3
                sqlParameter.Add("kepElectrLineFileName", txtKepElectrLineFileName.Text.Trim() != "" ? txtKepElectrLineFileName.Text : "");
                sqlParameter.Add("kepElectrLineFilePath", txtKepElectrLineFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("kepFaucetAcptFileName", txtKepFaucetAcptFileName.Text.Trim() != "" ? txtKepFaucetAcptFileName.Text : "");
                sqlParameter.Add("kepFaucetAcptFilePath", txtKepFaucetAcptFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrSafeInspPrintFileName", txtElectrSafeInspPrintFileName.Text.Trim() != "" ? txtElectrSafeInspPrintFileName.Text : "");
                sqlParameter.Add("electrSafeInspPrintFilePath", txtElectrSafeInspPrintFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrBeforeUseCheckPrintFileName", txtElectrBeforeUseCheckPrintFileName.Text.Trim() != "" ? txtElectrBeforeUseCheckPrintFileName.Text : "");
                sqlParameter.Add("electrBeforeUseCheckPrintFilePath", txtElectrBeforeUseCheckPrintFileName.Tag !=null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrBeforeUseInspFileName", txtElectrBeforeUseInspFileName.Text.Trim() != "" ? txtElectrBeforeUseInspFileName.Text : "");
                sqlParameter.Add("electrBeforeUseInspFilePath", txtElectrBeforeUseInspFileName.Tag !=null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrCoWorkFileName", txtElectrCoWorkFileName.Text.Trim() != "" ? txtElectrCoWorkFileName.Text : "");
                sqlParameter.Add("electrCoWorkFilePath", txtElectrCoWorkFileName.Tag !=null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("electrCostFileName", txtElectrCostFileName.Text.Trim() != "" ? txtElectrCostFileName.Text : "");
                sqlParameter.Add("electrCostFilePath", txtElectrCostFileName.Tag !=null? "/ImageData/Order/" + orderID : "");

                //tab4
                sqlParameter.Add("superSetCheckFileName", txtSuperSetCheckFileName.Text.Trim() != "" ? txtSuperSetCheckFileName.Text : "");
                sqlParameter.Add("superSetCheckFilePath", txtSuperSetCheckFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("superBeforeUseInspectFileNamet", txtSuperBeforeUseInspectFileName.Text.Trim() != "" ? txtSuperBeforeUseInspectFileName.Text : "");
                sqlParameter.Add("superBeforeUseInspectFilePath", txtSuperBeforeUseInspectFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("superCostFileName", txtSuperCostFileName.Text.Trim() != "" ? txtSuperCostFileName.Text : "");
                sqlParameter.Add("superCostFilePath", txtSuperCostFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("superReportFileName", txtSuperReportFileName.Text.Trim() != "" ? txtSuperReportFileName.Text : "");
                sqlParameter.Add("superReportFilePath", txtSuperReportFileName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("compReportFIleName", txtCompReportFIleName.Text.Trim() != "" ? txtCompReportFIleName.Text : "");
                sqlParameter.Add("compReportFIlePath", txtCompReportFIleName.Tag != null ? "/ImageData/Order/" + orderID : "");

                sqlParameter.Add("insurePrintFileName", txtInsurePrintFileName.Text.Trim() != "" ? txtInsurePrintFileName.Text : "");
                sqlParameter.Add("insurePrintFilePath", txtInsurePrintFileName.Tag != null ? "/ImageData/Order/" + orderID : "");


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
                    //if(result == MessageBoxResult.OK)
                    //{
                    //    tabBasicData.Focus();
                    //}
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


            if (msg.Length > 0)
            {
                var result =  MessageBox.Show(msg);
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

        private void DataGrid_LostFocus_Calculate(object sender, RoutedEventArgs e)
        {
            var element = sender as DependencyObject;
            while (element != null && !(element is DataGrid))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            var currentGrid = element as DataGrid;
            if (currentGrid == null || currentGrid.ItemsSource == null)
                return;

            if (currentGrid.Name.Contains("dgdAcc"))
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
            FindUiObject(grdInput, child =>
            {
                if (child is TextBox textbox)
                {
                    textbox.Text = string.Empty;
                    textbox.Tag = null;
                }
            });

            FindUiObject(grdInput, child =>
            {
                if (child is DatePicker dtp)
                {   
                    if(dtp.Name == "dtpOrderDate")
                    {
                        dtp.SelectedDate = DateTime.Now;
                    }
                    else if(dtp.Name != "dtpOrderDate")
                    {
                        dtp.SelectedDate = null;
                    }
                    
                }
            });
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


        // 자식요소 안에서 부모요서 찾기
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
            if (ClickPoint.Contains("Contract")) { FTP_Upload_TextBox(txtContractFileName); }  //긴 경로(FULL 사이즈)
            else if (ClickPoint.Contains("BeforeSearchConsult")) { FTP_Upload_TextBox(txtBeforeSearchConsultFileName); }
            else if (ClickPoint.Contains("PictureEarth")) { FTP_Upload_TextBox(txtPictureEarthFileName); }
            else if (ClickPoint.Contains("Draw")) { FTP_Upload_TextBox(txtDrawFileName); }
            else if (ClickPoint.Contains("Search")) { FTP_Upload_TextBox(txtSearchFileName); }
            else if (ClickPoint.Contains("SearchChecksheet")) { FTP_Upload_TextBox(txtSearchChecksheetFile); }
            else if (ClickPoint.Contains("InstallLocationSheet")) { FTP_Upload_TextBox(txtInstallLocationSheetFile); }
            else if (ClickPoint.Contains("LocalGoTax")) { FTP_Upload_TextBox(txtLocalGoTaxFile); }
            else if (ClickPoint.Contains("kepElectrLine")) { FTP_Upload_TextBox(txtKepElectrLineFileName); }
            else if (ClickPoint.Contains("kepFaucetAcpt")) { FTP_Upload_TextBox(txtKepFaucetAcptFileName); }
            else if (ClickPoint.Contains("ElectrSafeInspPrint")) { FTP_Upload_TextBox(txtElectrSafeInspPrintFileName); }
            else if (ClickPoint.Contains("ElectrBeforeUseCheckPrint")) { FTP_Upload_TextBox(txtElectrBeforeUseCheckPrintFileName); }
            else if (ClickPoint.Contains("ElectrBeforeUseInsp")) { FTP_Upload_TextBox(txtElectrBeforeUseInspFileName); }
            else if (ClickPoint.Contains("ElectrCoWork")) { FTP_Upload_TextBox(txtElectrCoWorkFileName); }
            else if (ClickPoint.Contains("ElectrCost")) { FTP_Upload_TextBox(txtElectrCostFileName); }
            else if (ClickPoint.Contains("SuperSetCheck")) { FTP_Upload_TextBox(txtSuperSetCheckFileName); }
            else if (ClickPoint.Contains("SuperBeforeUseInspect")) { FTP_Upload_TextBox(txtSuperBeforeUseInspectFileName); }
            else if (ClickPoint.Contains("SuperCostFile")) { FTP_Upload_TextBox(txtSuperCostFileName); }
            else if (ClickPoint.Contains("SuperReportFile")) { FTP_Upload_TextBox(txtSuperReportFileName); }
            else if (ClickPoint.Contains("CompReport")) { FTP_Upload_TextBox(txtCompReportFIleName); }
            else if (ClickPoint.Contains("InsurePrint")) { FTP_Upload_TextBox(txtInsurePrintFileName); }
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
                //OFdlg.Filter =
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
                    if (sr.BaseStream.Length > (4096 * 1000))
                    {
                        //업로드 파일 사이즈범위 초과
                        MessageBox.Show("이미지의 파일사이즈가 4M byte를 초과하였습니다.");
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
                MessageBoxResult msgresult = MessageBox.Show("파일을 보시겠습니까?", "보기 확인", MessageBoxButton.YesNo);
                if (msgresult == MessageBoxResult.Yes)
                {
                    //버튼 태그값.
                    string ClickPoint = ((Button)sender).Tag.ToString();

                    string contractFileName = txtContractFileName.Text.Trim() != "" ? txtContractFileName.Text : "";
                    string beforeSearchConsultFileName = txtBeforeSearchConsultFileName.Text.Trim() != "" ? txtBeforeSearchConsultFileName.Text : "";
                    string pictureEarthFileName = txtPictureEarthFileName.Text.Trim() != "" ? txtPictureEarthFileName.Text : "";
                    string drawFileName = txtDrawFileName.Text.Trim() != "" ? txtDrawFileName.Text : "";
                    string searchFileName = txtSearchFileName.Text.Trim() != "" ? txtSearchFileName.Text : "";
                    string searchChecksheetFileName = txtSearchChecksheetFile.Text.Trim() != "" ? txtSearchChecksheetFile.Text : "";
                    string installLocationSheetFileName = txtInstallLocationSheetFile.Text.Trim() != "" ? txtInstallLocationSheetFile.Text : "";
                    string localGoTaxFileName = txtLocalGoTaxFile.Text.Trim() != "" ? txtLocalGoTaxFile.Text : "";
                    string kepElectrLineFileName = txtKepElectrLineFileName.Text.Trim() != "" ? txtKepElectrLineFileName.Text : "";
                    string kepFaucetAcptFileName = txtKepFaucetAcptFileName.Text.Trim() != "" ? txtKepFaucetAcptFileName.Text : "";
                    string electrSafeInspPrintFileName = txtElectrSafeInspPrintFileName.Text.Trim() != "" ? txtElectrSafeInspPrintFileName.Text : "";
                    string electrBeforeUseCheckPrintFileName = txtElectrBeforeUseCheckPrintFileName.Text.Trim() != "" ? txtElectrBeforeUseCheckPrintFileName.Text : "";
                    string electrBeforeUseInspFileName = txtElectrBeforeUseInspFileName.Text.Trim() != "" ? txtElectrBeforeUseInspFileName.Text : "";
                    string electrCoWorkFileName = txtElectrCoWorkFileName.Text.Trim() != "" ? txtElectrCoWorkFileName.Text : "";
                    string electrCostFileName = txtElectrCostFileName.Text.Trim() != "" ? txtElectrCostFileName.Text : "";
                    string superSetCheckFileName = txtSuperSetCheckFileName.Text.Trim() != "" ? txtSuperSetCheckFileName.Text : "";
                    string superBeforeUseInspectFileName = txtSuperBeforeUseInspectFileName.Text.Trim() != "" ? txtSuperBeforeUseInspectFileName.Text : "";
                    string superCostFileFileName = txtSuperCostFileName.Text.Trim() != "" ? txtSuperCostFileName.Text : "";
                    string superReportFileFileName = txtSuperReportFileName.Text.Trim() != "" ? txtSuperReportFileName.Text : "";
                    string compReportFileName = txtCompReportFIleName.Text.Trim() != "" ? txtCompReportFIleName.Text : "";
                    string insurePrintFileName = txtInsurePrintFileName.Text.Trim() != "" ? txtInsurePrintFileName.Text : "";



                    if (((ClickPoint == "Contract") && (txtContractFileName.Text == string.Empty))
                       || ((ClickPoint == "BeforeSearchConsult") && (txtBeforeSearchConsultFileName.Text == string.Empty))
                       || ((ClickPoint == "PictureEarth") && (txtPictureEarthFileName.Text == string.Empty))
                       || ((ClickPoint == "Draw") && (txtDrawFileName.Text == string.Empty))
                       || ((ClickPoint == "Search") && (txtSearchFileName.Text == string.Empty))
                       || ((ClickPoint == "SearchChecksheet") && (txtSearchChecksheetFile.Text == string.Empty))
                       || ((ClickPoint == "InstallLocationSheet") && (txtInstallLocationSheetFile.Text == string.Empty))
                       || ((ClickPoint == "LocalGoTax") && (txtLocalGoTaxFile.Text == string.Empty))
                       || ((ClickPoint == "kepElectrLine") && (txtKepElectrLineFileName.Text == string.Empty))
                       || ((ClickPoint == "kepFaucetAcpt") && (txtKepFaucetAcptFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrSafeInspPrint") && (txtElectrSafeInspPrintFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrBeforeUseCheckPrint") && (txtElectrBeforeUseCheckPrintFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrBeforeUseInsp") && (txtElectrBeforeUseInspFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrCoWork") && (txtElectrCoWorkFileName.Text == string.Empty))
                       || ((ClickPoint == "ElectrCost") && (txtElectrCostFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperSetCheck") && (txtSuperSetCheckFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperBeforeUseInspect") && (txtSuperBeforeUseInspectFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperCostFile") && (txtSuperCostFileName.Text == string.Empty))
                       || ((ClickPoint == "SuperReportFile") && (txtSuperReportFileName.Text == string.Empty))
                       || ((ClickPoint == "CompReport") && (txtCompReportFIleName.Text == string.Empty))
                       || ((ClickPoint == "InsurePrint") && (txtInsurePrintFileName.Text == string.Empty)))         
                      
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

                        if (ClickPoint == "Contract") { str_remotepath = contractFileName; }
                        else if (ClickPoint == "BeforeSearchConsult") { str_remotepath = beforeSearchConsultFileName; }
                        else if (ClickPoint == "PictureEarth") { str_remotepath = pictureEarthFileName; }
                        else if (ClickPoint == "Draw") { str_remotepath = drawFileName; }
                        else if (ClickPoint == "Search") { str_remotepath = searchFileName; }
                        else if (ClickPoint == "SearchChecksheet") { str_remotepath = searchChecksheetFileName; }
                        else if (ClickPoint == "InstallLocationSheet") { str_remotepath = installLocationSheetFileName; }
                        else if (ClickPoint == "LocalGoTax") { str_remotepath = localGoTaxFileName; }
                        else if (ClickPoint == "kepElectrLine") { str_remotepath = kepElectrLineFileName; }
                        else if (ClickPoint == "kepFaucetAcpt") { str_remotepath = kepFaucetAcptFileName; }
                        else if (ClickPoint == "ElectrSafeInspPrint") { str_remotepath = electrSafeInspPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseCheckPrint") { str_remotepath = electrBeforeUseCheckPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInsp") { str_remotepath = electrBeforeUseInspFileName; }
                        else if (ClickPoint == "ElectrCoWork") { str_remotepath = electrCoWorkFileName; }
                        else if (ClickPoint == "ElectrCost") { str_remotepath = electrCostFileName; }
                        else if (ClickPoint == "SuperSetCheck") { str_remotepath = superSetCheckFileName; }
                        else if (ClickPoint == "SuperBeforeUseInspect") { str_remotepath = superBeforeUseInspectFileName; }
                        else if (ClickPoint == "SuperCostFile") { str_remotepath = superCostFileFileName; }
                        else if (ClickPoint == "SuperReportFile") { str_remotepath = superReportFileFileName; }
                        else if (ClickPoint == "CompReport") { str_remotepath = compReportFileName; }
                        else if (ClickPoint == "InsurePrint") { str_remotepath = insurePrintFileName; }
                   


             

                        if (ClickPoint == "Contract") { str_localpath = LOCAL_DOWN_PATH + "\\" + contractFileName; }
                        else if (ClickPoint == "BeforeSearchConsult") { str_localpath = LOCAL_DOWN_PATH + "\\" + beforeSearchConsultFileName; }
                        else if (ClickPoint == "PictureEarth") { str_localpath = LOCAL_DOWN_PATH + "\\" + pictureEarthFileName; }
                        else if (ClickPoint == "Draw") { str_localpath = LOCAL_DOWN_PATH + "\\" + drawFileName; }
                        else if (ClickPoint == "Search") { str_localpath = LOCAL_DOWN_PATH + "\\" + searchFileName; }
                        else if (ClickPoint == "SearchChecksheet") { str_localpath = LOCAL_DOWN_PATH + "\\" + searchChecksheetFileName; }
                        else if (ClickPoint == "InstallLocationSheet") { str_localpath = LOCAL_DOWN_PATH + "\\" + installLocationSheetFileName; }
                        else if (ClickPoint == "LocalGoTax") { str_localpath = LOCAL_DOWN_PATH + "\\" + localGoTaxFileName; }
                        else if (ClickPoint == "kepElectrLine") { str_localpath = LOCAL_DOWN_PATH + "\\" + kepElectrLineFileName; }
                        else if (ClickPoint == "kepFaucetAcpt") { str_localpath = LOCAL_DOWN_PATH + "\\" + kepFaucetAcptFileName; }
                        else if (ClickPoint == "ElectrSafeInspPrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrSafeInspPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseCheckPrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrBeforeUseCheckPrintFileName; }
                        else if (ClickPoint == "ElectrBeforeUseInsp") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrBeforeUseInspFileName; }
                        else if (ClickPoint == "ElectrCoWork") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrCoWorkFileName; }
                        else if (ClickPoint == "ElectrCost") { str_localpath = LOCAL_DOWN_PATH + "\\" + electrCostFileName; }
                        else if (ClickPoint == "SuperSetCheck") { str_localpath = LOCAL_DOWN_PATH + "\\" + superSetCheckFileName; }
                        else if (ClickPoint == "SuperBeforeUseInspect") { str_localpath = LOCAL_DOWN_PATH + "\\" + superBeforeUseInspectFileName; }
                        else if (ClickPoint == "SuperCostFile") { str_localpath = LOCAL_DOWN_PATH + "\\" + superCostFileFileName; }
                        else if (ClickPoint == "SuperReportFile") { str_localpath = LOCAL_DOWN_PATH + "\\" + superReportFileFileName; }
                        else if (ClickPoint == "CompReport") { str_localpath = LOCAL_DOWN_PATH + "\\" + compReportFileName; }
                        else if (ClickPoint == "InsurePrint") { str_localpath = LOCAL_DOWN_PATH + "\\" + insurePrintFileName; }
                  

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
                        if (File.Exists(str_localpath))
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

                //먼저 클릭한 버튼의 파일명을 삭제할 파일 리스트에 올린다. 리스트에 올리면서 텍스트의 텍스트와 태그를 지운다.
                //lstFileName에는 ftp업로드할때 파일명 중복방지를 위한 리스트(파일명이 중복되면 파일이 업로드 되지 않고 삭제될때 문제생김)
                //저장할때 리스트에 있다면 FTP삭제 요청을 한다.
                if ((ClickPoint == "BeforeSearchConsult") && (txtBeforeSearchConsultFileName.Text != string.Empty)) { fileName = txtBeforeSearchConsultFileName.Text; FileDeleteAndTextBoxEmpty(txtBeforeSearchConsultFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "PictureEarth") && (txtPictureEarthFileName.Text != string.Empty)) { fileName = txtPictureEarthFileName.Text; FileDeleteAndTextBoxEmpty(txtPictureEarthFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Draw") && (txtDrawFileName.Text != string.Empty)) { fileName = txtDrawFileName.Text; FileDeleteAndTextBoxEmpty(txtDrawFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Search") && (txtSearchFileName.Text != string.Empty)) { fileName = txtSearchFileName.Text; FileDeleteAndTextBoxEmpty(txtSearchFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SearchChecksheet") && (txtSearchChecksheetFile.Text != string.Empty)) { fileName = txtSearchChecksheetFile.Text; FileDeleteAndTextBoxEmpty(txtSearchChecksheetFile); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "InstallLocationSheet") && (txtInstallLocationSheetFile.Text != string.Empty)) { fileName = txtInstallLocationSheetFile.Text; FileDeleteAndTextBoxEmpty(txtInstallLocationSheetFile); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "LocalGoTax") && (txtLocalGoTaxFile.Text != string.Empty)) { fileName = txtLocalGoTaxFile.Text; FileDeleteAndTextBoxEmpty(txtLocalGoTaxFile); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "kepElectrLine") && (txtKepElectrLineFileName.Text != string.Empty)) { fileName = txtKepElectrLineFileName.Text; FileDeleteAndTextBoxEmpty(txtKepElectrLineFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "kepFaucetAcpt") && (txtKepFaucetAcptFileName.Text != string.Empty)) { fileName = txtKepFaucetAcptFileName.Text; FileDeleteAndTextBoxEmpty(txtKepFaucetAcptFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrSafeInspPrint") && (txtElectrSafeInspPrintFileName.Text != string.Empty)) { fileName = txtElectrSafeInspPrintFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrSafeInspPrintFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrBeforeUseCheckPrint") && (txtElectrBeforeUseCheckPrintFileName.Text != string.Empty)) { fileName = txtElectrBeforeUseCheckPrintFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrBeforeUseCheckPrintFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrBeforeUseInsp") && (txtElectrBeforeUseInspFileName.Text != string.Empty)) { fileName = txtElectrBeforeUseInspFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrBeforeUseInspFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrCoWork") && (txtElectrCoWorkFileName.Text != string.Empty)) { fileName = txtElectrCoWorkFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrCoWorkFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "ElectrCost") && (txtElectrCostFileName.Text != string.Empty)) { fileName = txtElectrCostFileName.Text; FileDeleteAndTextBoxEmpty(txtElectrCostFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperSetCheck") && (txtSuperSetCheckFileName.Text != string.Empty)) { fileName = txtSuperSetCheckFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperSetCheckFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperBeforeUseInspect") && (txtSuperBeforeUseInspectFileName.Text != string.Empty)) { fileName = txtSuperBeforeUseInspectFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperBeforeUseInspectFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperCostFile") && (txtSuperCostFileName.Text != string.Empty)) { fileName = txtSuperCostFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperCostFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "SuperReportFile") && (txtSuperReportFileName.Text != string.Empty)) { fileName = txtSuperReportFileName.Text; FileDeleteAndTextBoxEmpty(txtSuperReportFileName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "CompReport") && (txtCompReportFIleName.Text != string.Empty)) { fileName = txtCompReportFIleName.Text; FileDeleteAndTextBoxEmpty(txtCompReportFIleName); lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "InsurePrint") && (txtInsurePrintFileName.Text != string.Empty)) { fileName = txtInsurePrintFileName.Text; FileDeleteAndTextBoxEmpty(txtInsurePrintFileName); lstFilesName.Remove(fileName); }
            }




            // 보기 버튼체크
            //btnImgSeeCheckAndSetting();
        }
        private void FileDeleteAndTextBoxEmpty(TextBox txt)
        {
            if (strFlag.Equals("U"))
            {
                var Article = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_Nadaum;

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
            if (dgdLocalGov.Items.Count > 0) ovcOrder_localGov.Clear();

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
                tab2_clicked = false;
                tab3_clicked = false;
                tab4_clicked = false;


                var OrderInfo = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_dgdMain;
                if (OrderInfo != null)
                {
                    rowNum = dgdMain.SelectedIndex;    
                    this.DataContext = OrderInfo;
                  
                    orderID_global = OrderInfo.orderId;       
                    fillAccGrid(OrderInfo.orderId);

                    CheckTabClicked();
                    FillTabs(OrderInfo.orderId);

                }


            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridMain_SelectionChanged : " + ee.ToString());
            }

        }

        private void FillTabs(string orderId)
        {
            if (tab2.IsSelected == true)
            {
                fillGridTab2(orderId);
                fillGridTab2_LocalGov(orderId);
                tab2_clicked = true;
            }
            if (tab3.IsSelected == true)
            {
                fillGridTab3(orderId);
                tab3_clicked = true;
            }
            if (tab4.IsSelected == true)
            {
                fillGridTab4(orderId);
                fillGridTab4_Accnt(orderId);
                tab4_clicked=true;
            }
        }

        private void BringdLastOrder(string orderId)
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
        

            int count = 5;
            for (int i = 0; i < count; i++)
            {
                var dgdAccntItem = Win_order_Order_U_CodView_dgdAccnt.CreateEmpty_dgdAccnt_row();
                dgdAccnt.Items.Add(dgdAccntItem);
            }
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
            if (chkEoAddSrh.IsChecked == true)
            {
                chkEoAddSrh.IsChecked = false;
            }
            else
            {
                chkEoAddSrh.IsChecked = true;
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
                    txtEstSubject.Text = txtEstID.Text;
                    txtEstID.Text = txtEstID.Tag.ToString();
                    callEstAccData(txtEstID.Tag.ToString());
                }


                e.Handled = true;    

            }
        }
        

        //견적번호(입력그리드) - 버튼
        private void btnEstID_Click(object sender, RoutedEventArgs e)
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
                            txtEstID.Text = selectedRow.EstID;

                            txtManagerCustomID.Text = selectedRow.managerCustom;
                            txtManagerCustomID.Tag = selectedRow.managerCustomID;

                            txtSalesCustomID.Text = selectedRow.salesCustom;
                            txtSalesCustomID.Tag = selectedRow.salesCustomID;

                            dtpContractFromDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchFromDate);
                            dtpContractToDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchTODate);
                            dtpOpenReqDate.SelectedDate = ConvertToDateTime(selectedRow.InstallSchFromDate);

                            txtInstallLocation.Text = selectedRow.InstalLocation;
                            txtInstallLocationPart.Text = selectedRow.InstallLocationPart;

                            txtDamdangjaName.Text = selectedRow.EstDamdangName;
                            txtDamdangjaPhone.Text = selectedRow.EstDamdangTelno;
                            txtInstallLocationAddComments.Text = selectedRow.Comments;

                            txtMtrAmount.Text = selectedRow.totalAmount;
                            txtMtrShippingCharge.Text = selectedRow.deliveryCost;

                            int count = CountEstSub(selectedRow.EstID);

                      
                        }

                        MessageBox.Show("견적 데이터를 불러 왔습니다.", "확인");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("견적 복사 중 오류가 발생했습니다. 오류내용\n" + ex.ToString());
                    }
                }

            }
            else
            {
                MessageBox.Show("새로 추가 중에만 사용 할 수 있습니다.");
            }
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
                            txtOrderNo.Text = selectedRow.orderId;

                            BringdLastOrder(selectedRow.orderId);

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
                MessageBox.Show("새로 추가 중에만 사용 할 수 있습니다.");
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
                MainWindow.pf.ReturnCode(txtSuperCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //감리업체 버튼
        private void btnSuperCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSuperCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //감리비용 지출업체
        private void txtSuperCostPayCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSuperCostPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //감리비용 지출업체
        private void btnSuperCostPayCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSuperCostPayCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //안전관리 업체명
        private void txtSafeManageCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtSafeManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }
        //안전관리 업체명
        private void btnSafeManageCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSafeManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
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

        //기설충전업체
        private void txtAlreadyManageCustomID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
              MainWindow.pf.ReturnCode(txtAlreadyManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //기설충전업체
        private void btnAlreadyManageCustomID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtAlreadyManageCustomID, (int)Defind_CodeFind.DCF_CUSTOM, "");

        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (e.Source is TabControl)  
            {
                TabItem selectedTab = ((TabControl)sender).SelectedItem as TabItem;
                if (selectedTab != null)
                {
                    if (selectedTab.Name == "tab2")
                    {
                        fillGridTab2(orderID_global);
                        fillGridTab2_LocalGov(orderID_global);
                        tab2_clicked = true;
                    }
                    if (selectedTab.Name == "tab3")
                    {
                        fillGridTab3(orderID_global);
                        tab3_clicked = true;
                    }
                    if (selectedTab.Name == "tab4")
                    {
                        fillGridTab4(orderID_global);
                        fillGridTab4_Accnt(orderID_global);
                        tab4_clicked = true;
                    }
              
                }
            }
        }

        private void chkEoAddSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkEoAddSrh.IsChecked == true)
            {
                strFlag = "I";
                tbkMsg.Text = "자료 유지 추가 중";
                txtOrderID.Text = string.Empty;
            }
            else
            {
                strFlag = "U";
                tbkMsg.Text = "자료 수정 중";
                txtOrderID.Text = orderID_global;
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
        public string estID { get; set; }
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
        public string saledamdangjaPhone {get;set;}
        public string saleCustomAddWork {get;set;}
        public string salegift { get; set; }
        public string mtrAmount{get;set;}
        public string mtrShippingCharge{get;set;}
        public string mtrPriceUnitClss{get;set;}
        public string mtrCanopyInwareInfo{get;set;}
        public string mtrCanopyOrderAmount { get; set; }
        public string contractFileName { get; set; }
        public string contractFilePath { get; set; }

    }

    public class Win_order_Order_U_CodView_dgdAccnt : BaseView
    {
        public string column1Date { get; set; }
        public string column2Amount { get; set; }
        public string column3Amount { get; set; }
        public string column4Amount { get; set; }
        public string column5Amount { get; set; }
        public string column6Amount { get; set; }        
        public string column7Comment { get; set; }

        public static Win_order_Order_U_CodView_dgdAccnt CreateEmpty_dgdAccnt_row()
        {
            return new Win_order_Order_U_CodView_dgdAccnt()
            {
                column1Date =  "",// DateTime.Now.ToString("yyyy-MM-dd"),
                column2Amount = "",
                column3Amount = "",
                column4Amount = "",
                column5Amount = "",
                column6Amount = "",
                column7Comment = ""
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
        public string chargeInwareDate {get;set;}
        public string chargeInwareQty {get;set;}
        public string chargeInwareUnitPrice { get; set; }
        public string chargeInwareLocation {get;set;}
        public string canopyReqCustom {get;set;}
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

    public class ScrollSyncHelper
    {
        private ScrollViewer _headerScrollViewer;
        private DataGrid _dataGrid;
        private bool _isUpdatingScroll = false;

        public ScrollSyncHelper(ScrollViewer headerScrollViewer, DataGrid dataGrid)
        {
            _headerScrollViewer = headerScrollViewer;
            _dataGrid = dataGrid;

            // 헤더 스크롤뷰어의 이벤트 등록
            _headerScrollViewer.ScrollChanged += HeaderScrollViewer_ScrollChanged;

            // DataGrid가 로드되면 스크롤뷰어를 찾아서 이벤트 연결
            _dataGrid.Loaded += DataGrid_Loaded;
        }

        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;
                var dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                if (dataGridScrollViewer != null)
                {
                    dataGridScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
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
                if (scrollViewer != null)
                {
                    _headerScrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
                }
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
            if (dataGridScrollViewer != null)
            {
                dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
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
                var dataGridScrollViewer = FindVisualChild<ScrollViewer>(_dataGrid);
                if (dataGridScrollViewer != null)
                {
                    dataGridScrollViewer.ScrollChanged -= DataGrid_ScrollChanged;
                }
                _dataGrid.Loaded -= DataGrid_Loaded;
            }

            _headerScrollViewer = null;
            _dataGrid = null;
        }
    }

    //테이블 컬럼 171개 분리하여 관리시도
    public class Win_ord_Order_U_CodeView_Tab2 : BaseView
    {
        public string searchReqDate {get;set;}
        public string searchDate {get;set;}
        public string searchDataAcptDate {get;set;}
        public string installLocationCount {get;set;}
        public string electrDeliveryMethodID {get;set;}
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


    }

    public class Win_ord_Order_U_CodeView_Tab3 : BaseView
    {
        public string kepElectrDeliveryMethodID {get;set;}
        public string kepInstallLocationCount {get;set;}
        public string kepOutLineConstructContext {get;set;}
        public string kepInfraPayAmount {get;set;}
        public string kepManageInfraPayAmount {get;set;}
        public string kepElectrReqDate {get;set;}
        public string kepInApprovalYN {get;set;}
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
        public string electrCoWorkFilePath{get;set;}
        public string electrCoWorkFileName{get;set;}
        public string electrCostFilePath{get;set;}
        public string electrCostFileName { get; set; }



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
       public string superCostFilePath{get;set;}
       public string superCostFileName{get;set;}
       public string superReportFilePath{get;set;}
       public string superReportFileName{get;set;}
       public string insurePrintFilePath{get;set;}
       public string insurePrintFileName { get; set; }


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

    public class Win_ord_Order_U_CodeView_Nadaum : BaseView
    {
        public int num { get; set; }
        public string orderID { get; set; }
        public string orderNo { get; set; }
        public string reServeID { get; set; }
        public string article { get; set; }
        public string articleID { get; set; }
        public string customID { get; set; }
        public string kCustom { get; set; }
        public string orderDate { get; set; }
        public string jobFromDate { get; set; }
        public string jobToDate { get;set; }  
        public string jobPeriod { get; set; }
        public string closeYN{get;set;}
        public string orderAmount{get;set;}
        public string vatIndYN{get;set;}
        public string priceUnitClss{get;set;}
        public string contractMethodID{get;set;}
        public string contractMethod { get; set; }
        public string contractProgressID{get;set;}
        public string contractProgress { get; set; }
        public string settingCompDate{get;set;}
        public string reportSubmitDate{get;set;}
        public string pictureSubmitYN{get;set;}
        public string taxPrintReqDate{get;set;}
        public string taxPrintDate{get;set;}
        public string taxAmount{get;set;}
        public string depositDate{get;set;}
        public string depositAmount{get;set;}
        public string damdangName{get;set;}
        public string damdangDepartName{get;set;}
        public string damdangPositionName{get;set;}
        public string damdangDirPhone{get;set;}
        public string damdangHandPhone{get;set;}
        public string damdangEMail{get;set;}
        public string customjobReqContext{get;set;}
        public string memoContext{get;set;}
        public string customjobReqFileList{get;set;}
        public string contractFileName{get;set;}
        public string contractFilePath{get;set;}
        public string contractOkFileName{get;set;}
        public string contractOkFilePath{get;set;}
        public string rightOkFileName{get;set;}
        public string rightOkFilePath{get;set;}
        public string sexAssaultAcptFileName{get;set;}
        public string sexAssaultAcptFilePath{get;set;}
        public string customjobAcptFileName{get;set;}
        public string customjobAcptFilePath{get;set;}
        public string comments{get;set;}
        public string classTh { get; set; }
        public string articleList { get; set; }
        public string articleCount { get; set; }
        public string readyMaterial { get; set; }
        public string createDate{get;set;}
        public string createUserID{get;set;}
        public string lastUpdateDate{get;set;}
        public string lastUpdateUserID { get; set; }

    }

    public class Win_ord_Order_U_CodeView_OrderItemList_Nadaum : BaseView
    {
        public int num { get; set; }        
        public string itemSeq { get; set; }
        public string articleID { get; set; }
        public string article { get; set; }
        public string itemUnitPrice { get; set; }
        public string itemQty { get; set; }
        public string itemAmount { get; set; }
        public string comments { get; set; }

        public static Win_ord_Order_U_CodeView_OrderItemList_Nadaum CreateEmpty_OrderItemList()
        {           

            return new Win_ord_Order_U_CodeView_OrderItemList_Nadaum()
            {
                num = 0,
                itemSeq = "",
                articleID = "",
                article = "",
                itemUnitPrice = "",
                itemQty = "",
                itemAmount = "",
                comments = ""
            };
        }
    }


    public class Win_ord_Order_U_CodeView_OrderColor_Nadaum : BaseView
    {
        public int num { get; set; }
        public string orderSeq { get; set; }
        public string articleID { get; set; }
        public string article { get; set; }
        public string spec { get; set; }
        public string itemUnitPrice { get; set; }
        public string itemQty { get; set; }
        public string itemAmount { get; set; }
        public string comments { get; set; }

        public static Win_ord_Order_U_CodeView_OrderColor_Nadaum CreateEmpty_OrderColor()
        {

            return new Win_ord_Order_U_CodeView_OrderColor_Nadaum()
            {
                num = 0,
                orderSeq = "",
                articleID = "",
                article = "",
                spec = "",
                itemUnitPrice = "",
                itemQty = "",
                itemAmount = "",
                comments = ""
            };
        }
    }

    public class Win_ord_Order_U_CodeView_OrderStudent_Nadaum : BaseView
    {
        public int num {get; set; }
        public string studSeq { get; set; }
        public string grade { get; set; }
        public string classTh { get; set; }     
        public string ban { get; set; }         //DB에는 컬럼명이 class입니다. 컬럼설명은 반
        public string manCountPerClass { get; set; }
        public string totalManCount { get; set; }
        public string jobStartTime { get; set; }
        public string jobEndTime { get; set; }
        public string comments { get; set; }

        public static Win_ord_Order_U_CodeView_OrderStudent_Nadaum CreateEmpty_OrderStudent()
        {

            return new Win_ord_Order_U_CodeView_OrderStudent_Nadaum()
            {
                num = 0,
                studSeq = "",
                grade= "",
                classTh= "",
                ban="",
                manCountPerClass = "",
                totalManCount="",
                jobStartTime="",
                jobEndTime="",
                comments=""
            };
        }

    }

    public class Win_order_Order_U_CodeView_Total_Nadaum : BaseView
    {
        public string count { get; set; }
        public string totalSum { get; set; }
    }

    #region 기존 Hanyoung 속성

    public class Win_ord_Order_U_CodeView : BaseView
    {
        public string OrderID { get; set; }
        public string OrderNO { get; set; }
        public string CustomID { get; set; }
        public string KCustom { get; set; }
        public string CloseClss { get; set; }

        public string OrderQty { get; set; }
        public string UnitClss { get; set; }
        public string Article { get; set; }
        public string ChunkRate { get; set; }
        public string PatternID { get; set; }

        public string Amount { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerModel { get; set; }
        public string BuyerArticleNo { get; set; }
        public string PONO { get; set; }

        public string OrderForm { get; set; }
        public string OrderClss { get; set; }
        public string InCustomID { get; set; }
        public string AcptDate { get; set; }
        public string DvlyDate { get; set; }

        public string ArticleID { get; set; }
        public string DvlyPlace { get; set; }
        public string WorkID { get; set; }
        public string PriceClss { get; set; }
        public string ExchRate { get; set; }

        public string Vat_IND_YN { get; set; }
        public string ColorCnt { get; set; }
        public string StuffWidth { get; set; }
        public string StuffWeight { get; set; }
        public string CutQty { get; set; }

        public string WorkWidth { get; set; }
        public string WorkWeight { get; set; }
        public string WorkDensity { get; set; }
        public string LossRate { get; set; }
        public string ReduceRate { get; set; }

        public string TagClss { get; set; }
        public string LabelID { get; set; }
        public string BandID { get; set; }
        public string EndClss { get; set; }
        public string MadeClss { get; set; }

        public string SurfaceClss { get; set; }
        public string ShipClss { get; set; }
        public string AdvnClss { get; set; }
        public string LotClss { get; set; }
        public string EndMark { get; set; }

        public string TagArticle { get; set; }
        public string TagArticle2 { get; set; }
        public string TagOrderNo { get; set; }
        public string TagRemark { get; set; }
        public string Tag { get; set; }

        public string BasisID { get; set; }
        public string BasisUnit { get; set; }
        public string SpendingClss { get; set; }
        public string DyeingID { get; set; }
        public string WorkingClss { get; set; }

        public string BTID { get; set; }
        public string BTIDSeq { get; set; }
        public string ChemClss { get; set; }
        public string AccountClss { get; set; }
        public string ModifyClss { get; set; }

        public string ModifyRemark { get; set; }
        public string CancelRemark { get; set; }
        public string Remark { get; set; }
        public string ActiveClss { get; set; }
        public string ModifyDate { get; set; }

        public string OrderFlag { get; set; }
        public string TagRemark2 { get; set; }
        public string TagRemark3 { get; set; }
        public string TagRemark4 { get; set; }
        public string UnitPriceClss { get; set; }

        public string WeightPerYard { get; set; }
        public string WorkUnitClss { get; set; }
        public string ArticleGrpID { get; set; }
        public string OrderSpec { get; set; }
        public string UnitPrice { get; set; }

        public string CompleteArticleFile { get; set; }
        public string CompleteArticlePath { get; set; }
        public string FirstArticleFile { get; set; }
        public string FirstArticlePath { get; set; }
        public string MediumArticleFIle { get; set; }

        public string MediumArticlePath { get; set; }
        public string sketch1Path { get; set; }
        public string sketch1file { get; set; }
        public string sketch2Path { get; set; }
        public string sketch2file { get; set; }

        public string sketch3Path { get; set; }
        public string sketch3file { get; set; }
        public string sketch4Path { get; set; }
        public string sketch4file { get; set; }
        public string sketch5Path { get; set; }

        public string sketch5file { get; set; }
        public string sketch6Path { get; set; }
        public string sketch6file { get; set; }
        public string ProductAutoInspectYN { get; set; }
        public string kBuyer { get; set; }

        public string BuyerID { get; set; }
        public int Num { get; set; }
        public string AcptDate_CV { get; set; }
        public string DvlyDate_CV { get; set; }
        public string Amount_CV { get; set; }

        public string KInCustom { get; set; }
        public string SketchFile { get; set; }
        public string SketchPath { get; set; }
        public string ImageName { get; set; }

        public string CompanyID { get; set; }
        public string OrderNo { get; set; }
        public string PoNo { get; set; }
        public string OrderFormName { get; set; }
        public string BrandClss { get; set; }
        public string WorkName { get; set; }
        public string OrderClssName { get; set; }

        public string NewArticleQty { get; set; }
        public string RePolishingQty { get; set; }
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

    public class ArticleNeedStockQty : BaseView
    {
        public string BuyerArticleNo { get; set; }
        public string Article { get; set; }
        public string NeedQty { get; set; }
        public string FinalNeedQty { get; set; }
        public string UnitClss { get; set; }
        public string UnitClssName { get; set; }
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

    #endregion
}

