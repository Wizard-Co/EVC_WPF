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

    public partial class Win_ord_Pop_PreEstimate_Q : Window
    {
        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        public Win_ord_Pop_PreEstimate_CodeView SelectedItem { get; set; }

        //string sDate = string.Empty;
        //string eDate = string.Empty;
        //DateTime fiveYearsAgo = DateTime.Today.AddYears(-5);



        public Win_ord_Pop_PreEstimate_Q()
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

            //전기조달(검색조건)
            ObservableCollection<CodeView> ovcElecDeliMethSrh = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ElecDeliMth", "Y", "", "");
            cboElecDeliMethSrh.ItemsSource = ovcElecDeliMethSrh;
            cboElecDeliMethSrh.DisplayMemberPath = "code_name";
            cboElecDeliMethSrh.SelectedValuePath = "code_id";
            cboElecDeliMethSrh.SelectedIndex = 0;

   

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
            SelectedItem = dgr.Item as Win_ord_Pop_PreEstimate_CodeView;
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
            if(fillGrid())
                btnSearch.IsEnabled = true;
        }

        private bool fillGrid()
        {

            if(preOrder.Items.Count > 0) { preOrder.Items.Clear(); }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ChkDate", ChkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", ChkDateSrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString() : "");
                sqlParameter.Add("EDate", ChkDateSrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString() : "");

                sqlParameter.Add("chkManagerCustomID", chkManagerCustomIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManagerCustomID", chkManagerCustomIdSrh.IsChecked == true ? txtManagerCustomIdSrh.Tag.ToString() : "");

                sqlParameter.Add("chkArticleID", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleSrh.IsChecked == true ? txtArticleSrh.Tag.ToString() : "");

                sqlParameter.Add("chkElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ElecDeliMeth", chkElecDeliMethSrh.IsChecked == true ? cboElecDeliMethSrh.SelectedValue.ToString() : "");

                sqlParameter.Add("chkZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ZoneGbnID", chkZoneGbnIdSrh.IsChecked == true ? cboZoneGbnIdSrh.SelectedValue.ToString() : "");

                sqlParameter.Add("chkSmallInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("smallInstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstallLocationSrh.Text : "");

                sqlParameter.Add("chkComments", chkCommentsSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("Comments", chkCommentsSrh.IsChecked == true ? txtCommentsSrh.Text : "");
          
                sqlParameter.Add("chkEstSubject", chkEstSubjectSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("EstSubject", txtEstSubjectSrh.Text);

                sqlParameter.Add("EstID", "");


                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sEstimate", sqlParameter, false);

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
                            var PreOrd = new Win_ord_Pop_PreEstimate_CodeView()
                            {
                                num = num,
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
                                totalAmount = stringFormatN0(dr["totalAmount"]),
                                Comments = dr["Comments"].ToString(),
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
            catch(Exception ex)
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

  
       
    }


    public class Win_ord_Pop_PreEstimate_CodeView : BaseView
    {       
        public int num { get; set; }
        public string EstID { get; set; }
        public string Estsubject { get; set; }
        public string salesCustomID { get; set; }
        public string salesCustom { get; set; }
        public string managerCustomID { get; set; }
        public string managerCustom { get; set; }
        public string zoneGbnID { get; set; }
        public string FaciliTypeID { get; set; }
        public string FacliType { get; set; }
        public string EstDate { get; set; }
        public string EstReqDate { get; set; } //설치 예정일의 첫번째날로 정함 화면설계서에만 있음
        public string InstallSchFromDate { get; set; }
        public string InstallSchTODate { get; set; }
        public string InstalLocation { get; set; }
        public string smallInstalLocation { get; set; }
        public string InstallLocationPart { get; set; }
        public string InstallLocationCondition { get; set; }
        public string InstallLocationConditionID { get; set; }
        public string electrDeliveryMethodID { get; set; }
        public string EstSubject { get; set; }
        public string EstDamdangName { get; set; }
        public string EstDamdangTelno { get; set; }
        public string EstApprovalYN { get; set; }
        public string EstApprovalDate { get; set; }
        public string EstItemList { get; set; }
        public string deliveryCost { get; set; }
        public string totalAmount { get; set; }
        public string Comments { get; set; }
        public string CreateDate { get; set; }
        public string CreateUserID { get; set; }
        public string LastUpdateDate { get; set; }
        public string LastUpdateUserID { get; set; }
    }

}
