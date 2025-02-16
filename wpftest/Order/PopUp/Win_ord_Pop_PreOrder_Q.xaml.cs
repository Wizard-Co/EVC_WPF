using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Pop_PreOrder
'** 설명       : 계약등록 과거계약조회 상담등록 팝업
'** 작성일자   : 2024.12.11
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.12.11, 최대현, 최초작성, 계약등록 과거계약조회 기능작성, 선택 값 돌려주기  
'**************************************************************************************************/

namespace WizMes_EVC.Order.Pop
{

    public partial class Win_ord_Pop_PreOrder_Q : Window
    {
        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        public Win_ord_Pop_PreOrder_CodeView SelectedItem { get; set; }

        //string sDate = string.Empty;
        //string eDate = string.Empty;
        //DateTime fiveYearsAgo = DateTime.Today.AddYears(-5);



        public Win_ord_Pop_PreOrder_Q()
        {
            InitializeComponent();
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;

        }

        private void Win_ord_Pop_PreOrder_Q_Loaded(object sender, RoutedEventArgs e)
        {
            SetComboBox();
        }


        private void SetComboBox()
        {
            //EVC용
            //지역구분(ZoneID)
            ObservableCollection<CodeView> ovcZoneGbnID = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CSTZneGbn", "Y", "", "");
            cboZoneGbnIdSrh.ItemsSource = ovcZoneGbnID;
            cboZoneGbnIdSrh.DisplayMemberPath = "code_name";
            cboZoneGbnIdSrh.SelectedValuePath = "code_id";
            cboZoneGbnIdSrh.SelectedIndex = 0;

            //검색조건의 사업구분
            ObservableCollection<CodeView> ovcOrderTypeSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDTYPE", "Y", "", "");
            cboOrderTypeIDSrh.ItemsSource = ovcOrderTypeSrh;
            cboOrderTypeIDSrh.DisplayMemberPath = "code_name";
            cboOrderTypeIDSrh.SelectedValuePath = "code_id";
            cboOrderTypeIDSrh.SelectedIndex = 0;

            ////전기조달(검색조건)
            //ObservableCollection<CodeView> ovcElecDeliMethSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            //cboElecDeliMethSrh.ItemsSource = ovcElecDeliMethSrh;
            //cboElecDeliMethSrh.DisplayMemberPath = "code_name";
            //cboElecDeliMethSrh.SelectedValuePath = "code_id";
            //cboElecDeliMethSrh.SelectedIndex = 0;


        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        private void DataGrid_SizeChange(object sender, SizeChangedEventArgs e)
        {
            //DataGrid dgs = sender as DataGrid;
            //if (dgs.ColumnHeaderHeight == 0)
            //{
            //    dgs.ColumnHeaderHeight = 1;
            //}
            //double a = e.NewSize.Height / 100;
            //double b = e.PreviousSize.Height / 100;
            //double c = a / b;

            //if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
            //{
            //    dgs.ColumnHeaderHeight = dgs.ColumnHeaderHeight * c;
            //    dgs.FontSize = dgs.FontSize * c;
            //}
        }

        // 적용버튼 클릭.
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            int selectrow = preOrder.SelectedIndex;
            DataGridRow dgr = lib.GetRow(selectrow, preOrder);
            SelectedItem = dgr.Item as Win_ord_Pop_PreOrder_CodeView;
            DialogResult = true;
            this.Close();
        }

        // 취소버튼 클릭.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Reserve_Info = preOrder.SelectedItem as Win_ord_Pop_PreEstimate_CodeView;
        }

        private void lblDateSrh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChkDateSrh.IsChecked = ChkDateSrh.IsChecked == true ? false : true;
        }

        private void ChkDateSrh_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpSDate != null && dtpEDate != null)
            {
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        private void ChkDateSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }



        //비고 라벨
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
                //cboElecDeliMethSrh.IsEnabled = false;
                txtElecDeliMethSrh.IsEnabled = false;
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

        ////품명 라벨 클릭
        //private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (chkArticleSrh.IsChecked == true)
        //    {
        //        chkArticleSrh.IsChecked = false;
        //        txtArticleSrh.IsEnabled = false;
        //    }
        //    else
        //    {
        //        chkArticleSrh.IsChecked = true;
        //        txtArticleSrh.IsEnabled = true;
        //    }
        //}

        ////품명 체크박스
        //private void chkArticleSrh_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkArticleSrh.IsChecked == true)
        //    {
        //        chkArticleSrh.IsChecked = true;
        //        txtArticleSrh.IsEnabled = true;
        //    }
        //    else
        //    {
        //        chkArticleSrh.IsChecked = false;
        //        txtArticleSrh.IsEnabled = false;
        //    }
        //}


        ////품명 텍스트박스 키다운 이벤트
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

        //견적제목
        private void lblEstSubjectSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkEstSubjectSrh.IsChecked == true)
            {
                chkEstSubjectSrh.IsChecked = false;
                txtEstSubjectSrh.IsEnabled = false;
            }
            else
            {
                chkEstSubjectSrh.IsChecked = true;
                txtEstSubjectSrh.IsEnabled = true;
            }
        }

        //견적제목
        private void chkEstSubjectSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkEstSubjectSrh.IsChecked == true)
            {
                chkEstSubjectSrh.IsChecked = true;
                txtEstSubjectSrh.IsEnabled = true;
            }
            else
            {
                chkEstSubjectSrh.IsChecked = false;
                txtEstSubjectSrh.IsEnabled = false;
            }
        }




        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.IsEnabled = false;
            if (fillGrid())
                btnSearch.IsEnabled = true;
        }

        private bool fillGrid()
        {

            if (preOrder.Items.Count > 0) { preOrder.Items.Clear(); }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ChkDate", ChkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", ChkDateSrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString() : "");
                sqlParameter.Add("EDate", ChkDateSrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString() : "");

                // 운영사
                sqlParameter.Add("ChkManageCustomId", chkManagerCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManageCustomId", chkManagerCustomIdSrh.IsChecked == true ? (txtManagerCustomIdSrh.Tag != null ? txtManagerCustomIdSrh.Tag.ToString() : "") : "");

                //영업사
                //sqlParameter.Add("ChkSalesCustomId", 0);
                //sqlParameter.Add("SalesCustomId", "");

                //담당자명
                sqlParameter.Add("ChkSaledamdangjaName", 0);
                sqlParameter.Add("SaledamdangjaName", "");

                //시공사업체
                sqlParameter.Add("ChkConstrCustomId", chkConstrCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ConstrCustomId", chkConstrCustomIdSrh.IsChecked == true ? (txtConstrCustomIdSrh.Tag != null ? txtConstrCustomIdSrh.Tag.ToString() : "") : "");

                // 품목
                //sqlParameter.Add("ChkArticleId", chkArticleSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ArticleId", chkArticleSrh.IsChecked == true ? (txtArticleSrh.Text == string.Empty ? "" : chkArticleSrh.Tag.ToString()) : "");

                // 마감포함
                sqlParameter.Add("ChkCloseYn", 0);

                // 지역구분
                sqlParameter.Add("ChkZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? cboZoneGbnIdSrh.SelectedValue : "");

                // 전기조달방법
                sqlParameter.Add("ChkElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ElecDeliMeth", txtElecDeliMethSrh.Text);

                // 국소명
                sqlParameter.Add("ChkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstallLocationSrh.Text : "");

                //비고
                //sqlParameter.Add("ChkInstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("InstallLocationAddComments", chkInstallLocationAddCommentsSrh.IsChecked == true ? txtInstallLocationAddCommentsSrh.Text : "");

                // 사업구분
                sqlParameter.Add("ChkOrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderTypeID", chkOrderTypeIDSrh.IsChecked == true ? cboOrderTypeIDSrh.SelectedValue.ToString() : "");

                //견적제목
                sqlParameter.Add("chkEstSubject", chkEstSubjectSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("EstSubject", txtEstSubjectSrh.Text);

                //바로가기용 파라미터
                sqlParameter.Add("orderID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_ord_sOrder", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = null;
                    dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 과거이력 데이터가 없습니다.");
                    }
                    else
                    {
                        preOrder.Items.Clear();
                        DataRowCollection drc = dt.Rows;
                        int num = 1;
                        foreach (DataRow dr in drc)
                        {
                            var PreOrd = new Win_ord_Pop_PreOrder_CodeView()
                            {
                                num = num,
                                orderId = dr["orderId"].ToString(),
                                acptDate = DateTypeHyphen(dr["acptDate"].ToString()),
                                estSubject = dr["estSubject"].ToString(),
                                estID = dr["estID"].ToString(),
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

                                orderAmount = dr["orderAmount"].ToString(),
                                installLocationAddComments = dr["installLocationAddComments"].ToString(),
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

                                mtrCanopyInwareInfo = dr["mtrCanopyInwareInfo"].ToString(),
                                mtrCanopyOrderAmount = stringFormatN0(dr["mtrCanopyOrderAmount"]),



                                //contractFileName = dr["contractFileName"].ToString(),
                                //contractFilePath = dr["contractFilePath"].ToString(),
                            };

                            preOrder.Items.Add(PreOrd);
                            num++;
                        }

                        preOrder.SelectedIndex = 0;
                        tblCount.Text = $"※ {preOrder.Items.Count}건 조회 되었습니다.";
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("과거계약조회 중 오류 : 오류내용\n" + ex.ToString());
                return false;
            }

        }

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
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

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

        }

        public class Win_ord_Pop_PreOrder_CodeView : BaseView
        {
            public int num { get; set; }
            public string orderId { get; set; }
            public string estSubject { get; set; }
            public string estID { get; set; }
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
            public string installLocationAddress { get; set; }
            public string houseHoldCount { get; set; }
            public string carParkingCount { get; set; }
            public string alreadyManageCustom { get; set; }
            public string alreadyManageCustomID { get; set; }
            public string installLocationComments { get; set; }
            public string alReadyChargeCount { get; set; }
            public string contractToDate { get; set; }
            public string contractFromDate { get; set; }
            public string openReqDate { get; set; }
            public string openDate { get; set; }
            public string damdangjaName { get; set; }
            public string damdangjaEMail { get; set; }
            public string damdangjaPhone { get; set; }
            public string electrCarCount { get; set; }
            public string reqChargeCount { get; set; }
            public string saledamdangjaName { get; set; }
            public string saledamdangjaEmail { get; set; }
            public string saledamdangjaPhone { get; set; }
            public string saleCustomAddWork { get; set; }
            public string salegift { get; set; }
            public string salesComments { get; set; }
            public string mtrAmount { get; set; }
            public string mtrShippingCharge { get; set; }
            public string mtrPriceUnitClss { get; set; }
            public string mtrCanopyInwareInfo { get; set; }
            public string mtrCanopyOrderAmount { get; set; }
            public string contractFileName { get; set; }
            public string contractFilePath { get; set; }
        }

   
    }

}
