using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_EVC.PopUP;
using WizMes_EVC.PopUp;
using WPF.MDI;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace WizMes_EVC
{
    /// <summary>
    /// Win_ord_TodoList_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_TodoList_Q : UserControl
    {
        public Win_ord_TodoList_Q()
        {
            InitializeComponent();
        }
        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        string stDate = string.Empty;
        string stTime = string.Empty;

        // 엑셀 활용 용도 (프린트)
        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;
        WizMes_EVC.PopUp.NoticeMessage msg = new PopUp.NoticeMessage();
        DataTable DT;


        // 첫 로드시.
        private void Win_ord_TodoList_Q_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            //수주일자 체크
            chkInOutDate.IsChecked = true;
            dtpFromDate.SelectedDate = DateTime.Today;
            dtpToDate.SelectedDate = DateTime.Today;

            ComboBoxSetting();
            //제품으로 고정

        }

        #region 첫단계 / 날짜버튼 세팅 / 조회용 체크박스 세팅



        // 어제.(전일)
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {


            if (dtpFromDate.SelectedDate != null)
            {
                dtpFromDate.SelectedDate = dtpFromDate.SelectedDate.Value.AddDays(-1);
                dtpToDate.SelectedDate = dtpFromDate.SelectedDate;
            }
            else
            {
                dtpFromDate.SelectedDate = DateTime.Today.AddDays(-1);
                dtpToDate.SelectedDate = DateTime.Today.AddDays(-1);
            }

        }
        // 오늘(금일)
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpFromDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            dtpToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }
        // 지난 달(전월)
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            //string[] receiver = lib.BringLastMonthDatetime();

            //dtpFromDate.Text = receiver[0];
            //dtpToDate.Text = receiver[1];

            if (dtpFromDate.SelectedDate != null)
            {
                DateTime ThatMonth1 = dtpFromDate.SelectedDate.Value.AddDays(-(dtpFromDate.SelectedDate.Value.Day - 1)); // 선택한 일자 달의 1일!

                DateTime LastMonth1 = ThatMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThatMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }
            else
            {
                DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }


        }
        // 이번 달(금월)
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            string[] receiver = lib.BringThisMonthDatetime();

            dtpFromDate.Text = receiver[0];
            dtpToDate.Text = receiver[1];
        }

        // 입출일자
        private void chkInOutDate_Click(object sender, RoutedEventArgs e)
        {
            if (chkInOutDate.IsChecked == true)
            {
                dtpFromDate.IsEnabled = true;
                dtpToDate.IsEnabled = true;
            }
            else
            {
                dtpFromDate.IsEnabled = false;
                dtpToDate.IsEnabled = false;
            }
        }
        //입출일자
        private void chkInOutDate_Click(object sender, MouseButtonEventArgs e)
        {
            if (chkInOutDate.IsChecked == true)
            {
                chkInOutDate.IsChecked = false;
                dtpFromDate.IsEnabled = false;
                dtpToDate.IsEnabled = false;
            }
            else
            {
                chkInOutDate.IsChecked = true;
                dtpFromDate.IsEnabled = true;
                dtpToDate.IsEnabled = true;
            }
        }

        #endregion


        #region 콤보박스 세팅
        // 콤보박스 세팅.
        private void ComboBoxSetting()
        {

        }

        #endregion


        #region 조회 , 조회용 프로시저 
        // 조회.
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            using (Loading ld = new Loading(beSearch))
            {
                ld.ShowDialog();
            }
        }

        private void beSearch()
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                FillGrid();
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        private void FillGrid()
        {


            try
            {

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                sqlParameter.Clear();

                sqlParameter.Add("ChkDate", chkInOutDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkInOutDate.IsChecked == true ? dtpFromDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkInOutDate.IsChecked == true ? dtpToDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                //// 거래처
                //sqlParameter.Add("ChkCustom", chkCustom.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("CustomID", chkCustom.IsChecked == true ? (txtCustom.Tag != null ? txtCustom.Tag.ToString() : txtCustom.Text) : "");
                //// 최종고객사
                //sqlParameter.Add("ChkInCustom", chkInCustom.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("InCustomID", chkInCustom.IsChecked == true ? (txtInCustom.Tag != null ? txtInCustom.Tag.ToString() : "") : "");


                //// 품번
                //sqlParameter.Add("ChkArticleID", chkBuyerArticleNo.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ArticleID", chkBuyerArticleNo.IsChecked == true ? (txtBuyerArticleNo.Tag == null ? "" : txtBuyerArticleNo.Tag.ToString()) : "");
                //// 품명
                //sqlParameter.Add("ChkArticle", chkArticle.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("Article", chkArticle.IsChecked == true ? (txtArticle.Text == string.Empty ? "" : txtArticle.Text) : "");






                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sTodoList", sqlParameter, true, "R");
                DataTable dt = null;

                if (ds != null && ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회결과가 없습니다.");
                        return;
                    }

                    else
                    {

                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow item in drc)
                        {
                            i++;

                            var dgdCondition = new Win_ord_TodoList_Q_View()
                            {
                                Num = i + "",
                                orderNo = item["orderNo"].ToString(),
                                CLoseYn = item["CLoseYn"].ToString(),
                                orderid = item["orderid"].ToString(),
                                acptDate = DateTypeHyphen(item["acptDate"].ToString()),
                                EstID = item["EstID"].ToString(),
                                saleCustomName = item["saleCustomName"].ToString(),
                                saleCustomID = item["saleCustomID"].ToString(),
                                managerCustomName = item["managerCustomName"].ToString(),
                                managerCustomID = item["managerCustomID"].ToString(),
                                searchCustomName = item["searchCustomName"].ToString(),
                                searchCustomID = item["searchCustomID"].ToString(),
                                zoneGbnName = item["zoneGbnName"].ToString(),
                                zoneGbnID = item["zoneGbnID"].ToString(),
                                manageCustomAcptDate = DateTypeHyphen(item["manageCustomAcptDate"].ToString()),
                                manageCustomConfirmDate = DateTypeHyphen(item["manageCustomConfirmDate"].ToString()),
                                installLocation = item["installLocation"].ToString(),
                                installLocationAddress = item["installLocationAddress"].ToString(),
                                InstallLocationPhone = item["InstallLocationPhone"].ToString(),
                                installLocationPart = item["installLocationPart"].ToString(),
                                houseHoldCount = item["houseHoldCount"].ToString(),
                                carParkingCount = item["carParkingCount"].ToString(),
                                electrCarCount = item["electrCarCount"].ToString(),
                                alReadyChargeCount = item["alReadyChargeCount"].ToString(),
                                reqChargeCount = item["reqChargeCount"].ToString(),
                                alreadyManageCustomName = item["alreadyManageCustomName"].ToString(),
                                alreadyManageCustomID = item["alreadyManageCustomID"].ToString(),
                                installLocationComments = item["installLocationComments"].ToString(),
                                contractFromDate = item["contractFromDate"].ToString(),
                                contractToDate = item["contractToDate"].ToString(),
                                openReqDate = item["openReqDate"].ToString(),
                                openDate = item["openDate"].ToString(),
                                damdangjaName = item["damdangjaName"].ToString(),
                                damdangjaPhone = item["damdangjaPhone"].ToString(),
                                damdangjaEMail = item["damdangjaEMail"].ToString(),
                                installLocationAddComments = item["installLocationAddComments"].ToString(),
                                saledamdangjaPhone = item["saledamdangjaPhone"].ToString(),
                                saleCustomAddWork = item["saleCustomAddWork"].ToString(),
                                salegift = item["salegift"].ToString(),
                                article = item["article"].ToString(),
                                chargeOrderDate = item["chargeOrderDate"].ToString(),
                                chargeInwareDate = item["chargeInwareDate"].ToString(),
                                chargeInwareQty = item["chargeInwareQty"].ToString(),
                                chargeInwareLocation = item["chargeInwareLocation"].ToString(),
                                canopyReqCustom = item["canopyReqCustom"].ToString(),
                                chargeModelHelmat = item["chargeModelHelmat"].ToString(),
                                chargeModelinloc = item["chargeModelinloc"].ToString(),
                                chargeModelOneBody = item["chargeModelOneBody"].ToString(),
                                chargeStandReqDate = item["chargeStandReqDate"].ToString(),
                                chargeStandInwareDate = item["chargeStandInwareDate"].ToString(),
                                mtrCanopyInwareInfo = item["mtrCanopyInwareInfo"].ToString(),
                                mtrCanopyOrderAmount = item["mtrCanopyOrderAmount"].ToString(),
                                comments = item["comments"].ToString(),
                                searchReqDate = item["searchReqDate"].ToString(),
                                searchDate = item["searchDate"].ToString(),
                                searchQty = item["searchQty"].ToString(),
                                searchDataAcptDate = item["searchDataAcptDate"].ToString(),
                                installLocationCount = item["installLocationCount"].ToString(),
                                electrDeliveryMethod = item["electrDeliveryMethod"].ToString(),
                                inspectionNeedYN = item["inspectionNeedYN"].ToString(),
                                addConstructCostSearch = item["addConstructCostSearch"].ToString(),
                                addConstructCost = item["addConstructCost"].ToString(),
                                searchComments = item["searchComments"].ToString(),
                                corpAcptNo = item["corpAcptNo"].ToString(),
                                corpApprovalDate = item["corpApprovalDate"].ToString(),
                                corpEndDate = item["corpEndDate"].ToString(),
                                corpLastEndDate = item["corpLastEndDate"].ToString(),
                                corpComments = item["corpComments"].ToString(),
                                kepInstallLocationCount = item["kepInstallLocationCount"].ToString(),
                                kepElectrDeliveryMethod = item["kepElectrDeliveryMethod"].ToString(),
                                kepOutLineConstructContext = item["kepOutLineConstructContext"].ToString(),
                                kepInfraPayAmount = item["kepInfraPayAmount"].ToString(),
                                kepManageInfraPayAmount = item["kepManageInfraPayAmount"].ToString(),
                                kepElectrReqDate = item["kepElectrReqDate"].ToString(),
                                kepInApprovalYN = item["kepInApprovalYN"].ToString(),
                                kepInApprovalDate = item["kepInApprovalDate"].ToString(),
                                kepMeterInstallContext = item["kepMeterInstallContext"].ToString(),
                                kepDamdangjaPhone = item["kepDamdangjaPhone"].ToString(),
                                kepCustomNo = item["kepCustomNo"].ToString(),
                                kepPaymentDate = item["kepPaymentDate"].ToString(),
                                kepMeterInstallDate = item["kepMeterInstallDate"].ToString(),
                                kepFaucetComments = item["kepFaucetComments"].ToString(),
                                constrCustomName = item["constrCustomName"].ToString(),
                                constrCustomID = item["constrCustomID"].ToString(),
                                constrOrderDate = item["constrOrderDate"].ToString(),
                                constrDate = item["constrDate"].ToString(),
                                constrDelyReason = item["constrDelyReason"].ToString(),
                                constrCompleteDate = item["constrCompleteDate"].ToString(),
                                constrComments = item["constrComments"].ToString(),
                                electrSafeCheckDate = item["electrSafeCheckDate"].ToString(),
                                electrSafeCheckSuppleContext = item["electrSafeCheckSuppleContext"].ToString(),
                                electrSafeCheckLocation = item["electrSafeCheckLocation"].ToString(),
                                electrSafeCheckCost = item["electrSafeCheckCost"].ToString(),
                                electrSafeCheckCostPayDate = item["electrSafeCheckCostPayDate"].ToString(),
                                electrBeforeUseCheckReqDate = item["electrBeforeUseCheckReqDate"].ToString(),
                                electrSafeCheckPrintDate = item["electrSafeCheckPrintDate"].ToString(),
                                electrBeforeUseCheckSuppleContext = item["electrBeforeUseCheckSuppleContext"].ToString(),
                                electrBeforeInspLocation = item["electrBeforeInspLocation"].ToString(),
                                electrBeforeInspReqDate = item["electrBeforeInspReqDate"].ToString(),
                                electrBeforeInspPrintDate = item["electrBeforeInspPrintDate"].ToString(),
                                electrBeforeInspCost = item["electrBeforeInspCost"].ToString(),
                                electrBeforeInspCostPayDate = item["electrBeforeInspCostPayDate"].ToString(),
                                electrBeforeInspSuppleContext = item["electrBeforeInspSuppleContext"].ToString(),
                                electrSafeCheckComments = item["electrSafeCheckComments"].ToString(),
                                superCustomName = item["superCustomName"].ToString(),
                                superCustomID = item["superCustomID"].ToString(),
                                superCostPayCustom = item["superCostPayCustom"].ToString(),
                                superCostPayCustomID = item["superCostPayCustomID"].ToString(),
                                superCustomPhoneNo = item["superCustomPhoneNo"].ToString(),
                                safeManageCustomName = item["safeManageCustomName"].ToString(),
                                safeManageCustomID = item["safeManageCustomID"].ToString(),
                                safeManageCustomPhoneNo = item["safeManageCustomPhoneNo"].ToString(),
                                superSetCost = item["superSetCost"].ToString(),
                                superSetTaxPrintDate = item["superSetTaxPrintDate"].ToString(),
                                superUseInspPayCustomName = item["superUseInspPayCustomName"].ToString(),
                                superUseInspPayCustomID = item["superUseInspPayCustomID"].ToString(),
                                superUseInspReqDate = item["superUseInspReqDate"].ToString(),
                                superFromUseInspReqDate = item["superFromUseInspReqDate"].ToString(),
                                superBeforeUseInspDate = item["superBeforeUseInspDate"].ToString(),
                                superComments = item["superComments"].ToString(),
                                compReplyDate = item["compReplyDate"].ToString(),
                                suppleContext = item["suppleContext"].ToString(),
                                suppleCompDate = item["suppleCompDate"].ToString(),
                                compSuppleReportContext = item["compSuppleReportContext"].ToString(),
                                compSuppleReportDate = item["compSuppleReportDate"].ToString(),
                                insurePrintDate = item["insurePrintDate"].ToString(),
                                compReportCompDate = item["compReportCompDate"].ToString(),
                                compReportComments = item["compReportComments"].ToString(),
                                accntMgrWorkPreTaxPrintDate = item["accntMgrWorkPreTaxPrintDate"].ToString(),
                                accntMgrWorkPreAmount = item["accntMgrWorkPreAmount"].ToString(),
                                accntMgrWorkPreAmountComments = item["accntMgrWorkPreAmountComments"].ToString(),
                                accntMgrWorkAfterTaxPrintDate = item["accntMgrWorkAfterTaxPrintDate"].ToString(),
                                accntMgrWorkAfterAmount = item["accntMgrWorkAfterAmount"].ToString(),
                                accntMgrWorkAfterAmountComments = item["accntMgrWorkAfterAmountComments"].ToString(),
                                accntMgrWorkTaxPrintDate = item["accntMgrWorkTaxPrintDate"].ToString(),
                                accntMgrWorkAmount = item["accntMgrWorkAmount"].ToString(),
                                accntMgrWorkAmountComments = item["accntMgrWorkAmountComments"].ToString(),
                                accntWorkTaxPrintDate = item["accntWorkTaxPrintDate"].ToString(),
                                accntWorkAmount = item["accntWorkAmount"].ToString(),
                                accntWorkAmountComments = item["accntWorkAmountComments"].ToString(),
                                accntSalesTaxPrintDate = item["accntSalesTaxPrintDate"].ToString(),
                                accntSalesAmount = item["accntSalesAmount"].ToString(),
                                accntSalesAmountComments = item["accntSalesAmountComments"].ToString(),






                            };

                            dgdStock.Items.Add(dgdCondition);


                        }

                    }

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
        }

        #endregion


        // 닫기 버튼클릭.
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");

            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.subProgramID.ToString().Contains("MDI"))
                {
                    if (this.ToString().Equals((mvm.subProgramID as MdiChild).Content.ToString()))
                    {
                        (MainWindow.mMenulist[i].subProgramID as MdiChild).Close();
                        break;
                    }
                }
                i++;
            }
        }

        #region 엑셀
        // 엑셀버튼 클릭
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgdStock.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }

            Lib lib3 = new Lib();
            System.Data.DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[4];
            lst[0] = "메인 그리드";
            lst[2] = dgdStock.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdStock.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    //MessageBox.Show("대분류");
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib3.DataGridToDTinHidden(dgdStock);
                    else
                        dt = lib3.DataGirdToDataTable(dgdStock);

                    Name = dgdStock.Name;

                    if (lib3.GenerateExcel(dt, Name))
                    {
                        lib3.excel.Visible = true;
                        lib3.ReleaseExcelObject(lib3.excel);
                    }
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }

            lib3 = null;
        }



        #endregion


        #region 플러스 파인더 
        //플러스파인더 _ 거래처_클릭.


        #endregion



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

        #region 상단버튼 모음
        //운영사 라벨클릭 
        private void chkManageCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkManageCustomSrh.IsChecked == true)
            {
                chkManageCustomSrh.IsChecked = false;
            }
            else
            {
                chkManageCustomSrh.IsChecked = true;
            }
        }
        //운영사 체크
        private void chkManageCustom_Checked(object sender, RoutedEventArgs e)
        {
            chkManageCustomSrh.IsChecked = true;

            txtManageCustomSrh.IsEnabled = true;
            btnManageCustomSrh.IsEnabled = true;
        }
        //운영사 체크ㄴㄴ
        private void chkManageCustom_UnChecked(object sender, RoutedEventArgs e)
        {
            chkManageCustomSrh.IsChecked = false;

            txtManageCustomSrh.IsEnabled = false;
            btnManageCustomSrh.IsEnabled = false;

        }
        //운영사 엔터
        private void txtManageCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MainWindow.pf.ReturnCode(txtManageCustomSrh, 76, "");
            }

        }
        //운영사 플러스파인더
        private void btnManageCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtManageCustomSrh, 76, "");
        }


        //영업사 라벨클릭
        private void chkSalesCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkSalesCustomSrh.IsChecked == true)
            {
                chkSalesCustomSrh.IsChecked = false;
            }
            else
            {
                chkSalesCustomSrh.IsChecked = true;
            }
        }
        //영업사 췍
        private void chkSalesCustom_Checked(object sender, RoutedEventArgs e)
        {
            chkSalesCustomSrh.IsChecked = true;

            txtSalesCustomSrh.IsEnabled = true;
            btnSalesCustomSrh.IsEnabled = true;
        }
        //영업사 췍ㄴㄴ
        private void chkSalesCustom_UnChecked(object sender, RoutedEventArgs e)
        {
            chkSalesCustomSrh.IsChecked = false;

            txtSalesCustomSrh.IsEnabled = false;
            btnSalesCustomSrh.IsEnabled = false;
        }
        //영업사 엔터
        private void txtSalesCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MainWindow.pf.ReturnCode(txtSalesCustomSrh, 76, "");
            }
        }
        //영업사 pf
        private void btnSalesCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSalesCustomSrh, 76, "");
        }

        //제품명 라벨클릭
        private void chkArticleId_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleIdSrh.IsChecked == true)
            {
                chkArticleIdSrh.IsChecked = false;
            }
            else
            {
                chkArticleIdSrh.IsChecked = true;
            }
        }

        //제품명 체크
        private void chkArticleId_Checked(object sender, RoutedEventArgs e)
        {
            chkArticleIdSrh.IsChecked = true;

            txtArticleIdSrh.IsEnabled = true;
            btnArticleIdSrh.IsEnabled = true;
        }
        //제품명 체크ㄴ
        private void chkArticleId_UnChecked(object sender, RoutedEventArgs e)
        {
            chkArticleIdSrh.IsChecked = false;

            txtArticleIdSrh.IsEnabled = false;
            btnArticleIdSrh.IsEnabled = false;
        }
        //제품명 엔터
        private void txtArticleId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MainWindow.pf.ReturnCode(txtArticleIdSrh, 76, "");
            }
        }
        //제품명 pf
        private void btnCustomer_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleIdSrh, 76, "");
        }

        //국소명 라벨클릭
        private void chkInstallLocation_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInstallLocationSrh.IsChecked == true)
            {
                chkInstallLocationSrh.IsChecked = false;
            }
            else
            {
                chkInstallLocationSrh.IsChecked = true;
            }
        }

        //국소명 체크
        private void chkInstallLocation_Checked(object sender, RoutedEventArgs e)
        {
            chkInstallLocationSrh.IsChecked = true;

            txtInstallLocationSrh.IsEnabled = true;
            btnInstallLocationSrh.IsEnabled = true;
        }
        //국소명 체크ㄴ
        private void chkInstallLocation_UnChecked(object sender, RoutedEventArgs e)
        {
            chkInstallLocationSrh.IsChecked = false;

            txtInstallLocationSrh.IsEnabled = false;
            btnInstallLocationSrh.IsEnabled = false;
        }
        //국소명 엔터
        private void txtInstallLocation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MainWindow.pf.ReturnCode(txtInstallLocationSrh, 76, "");
            }
        }
        //국소명 pf
        private void btnInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInstallLocationSrh, 76, "");
        }

        //마감건포함 라벨클릭
        private void chkCloseYN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCloseYN.IsChecked == true)
            {
                chkCloseYN.IsChecked = false;
            }
            else
            {
                chkCloseYN.IsChecked = true;
            }
        }
        //마감건포함 체크
        private void chkCloseYN_Checked(object sender, RoutedEventArgs e)
        {
            chkCloseYN.IsChecked = true;

        }
        //마감건포함 체크ㄴ 
        private void chkCloseYN_UnChecked(object sender, RoutedEventArgs e)
        {
            chkCloseYN.IsChecked = false;

        }

        #endregion


        #region 데이터그리드 스크롤 +  헤더 스크롤 연결 
        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            var dataGridScrollViewer = FindChild<ScrollViewer>(dgdStock);
            if (dataGridScrollViewer != null)
            {
                // DataGrid 스크롤을 헤더 스크롤과 동기화
                dataGridScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }

        }


        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
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
                    foundChild = FindChild<T>(child);
                    if (foundChild != null) break;
                }
            }

            return foundChild;
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // DataGrid 내부의 ScrollViewer를 찾아서
            var dataGridScrollViewer = FindChild<ScrollViewer>(dgdStock);

            if (dataGridScrollViewer != null)
            {
                // ScrollViewer의 ScrollChanged 이벤트를 처리
                dataGridScrollViewer.ScrollChanged += DataGrid_ScrollChanged;
            }
        }


        // DataGrid의 수평 스크롤이 변경될 때 호출되는 메서드
        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // DataGrid의 ScrollViewer에서 수평 스크롤 오프셋을 가져옴
            var dataGridScrollViewer = sender as ScrollViewer;

            if (dataGridScrollViewer != null)
            {
                // 헤더의 ScrollViewer와 수평 오프셋을 동기화 
                dgdMainHeaderSh.ScrollToHorizontalOffset(dataGridScrollViewer.HorizontalOffset);
            }
        }
        #endregion


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


        private void GoOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // 수주등록 ㄱㄱㄱ
            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("수주등록"))
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
                        Title = "WizMes_EVC [" + MainWindow.mMenulist[i].MenuID.Trim() + "] " + MainWindow.mMenulist[i].Menu.Trim() +
                                " (→" + MainWindow.mMenulist[i].ProgramID + ")",
                        Height = SystemParameters.PrimaryScreenHeight * 0.8,
                        MaxHeight = SystemParameters.PrimaryScreenHeight * 0.85,
                        Width = SystemParameters.WorkArea.Width * 0.85,
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
        }
    }


    class Win_ord_TodoList_Q_View : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }


        // 조회용
        public string Num { get; set; }
        public string orderNo { get; set; }

        public string CLoseYn { get; set; }
        public string orderid { get; set; }
        public string acptDate { get; set; }
        public string EstID { get; set; }
        public string saleCustomName { get; set; }
        public string saleCustomID { get; set; }
        public string managerCustomName { get; set; }
        public string managerCustomID { get; set; }
        public string searchCustomName { get; set; }
        public string searchCustomID { get; set; }
        public string zoneGbnName { get; set; }
        public string zoneGbnID { get; set; }

        public string manageCustomAcptDate { get; set; }
        public string manageCustomConfirmDate { get; set; }
        public string installLocation { get; set; }
        public string installLocationAddress { get; set; }
        public string InstallLocationPhone { get; set; }
        public string installLocationPart { get; set; }
        public string houseHoldCount { get; set; }
        public string carParkingCount { get; set; }
        public string electrCarCount { get; set; }
        public string alReadyChargeCount { get; set; }
        public string reqChargeCount { get; set; }
        public string alreadyManageCustomName { get; set; }
        public string alreadyManageCustomID { get; set; }
        public string installLocationComments { get; set; }

        public string contractFromDate { get; set; }
        public string contractToDate { get; set; }
        public string openReqDate { get; set; }
        public string openDate { get; set; }
        public string damdangjaName { get; set; }
        public string damdangjaPhone { get; set; }
        public string damdangjaEMail { get; set; }

        public string installLocationAddComments { get; set; }
        public string saledamdangjaPhone { get; set; }
        public string saleCustomAddWork { get; set; }
        public string salegift { get; set; }
        public string article { get; set; }
        public string chargeOrderDate { get; set; }
        public string chargeInwareDate { get; set; }
        public string chargeInwareQty { get; set; }
        public string chargeInwareLocation { get; set; }
        public string canopyReqCustom { get; set; }
        public string chargeModelHelmat { get; set; }
        public string chargeModelinloc { get; set; }
        public string chargeModelOneBody { get; set; }
        public string chargeStandReqDate { get; set; }
        public string chargeStandInwareDate { get; set; }
        public string mtrCanopyInwareInfo { get; set; }
        public string mtrCanopyOrderAmount { get; set; }
        public string comments { get; set; }
        public string searchReqDate { get; set; }
        public string searchDate { get; set; }
        public string searchQty { get; set; }
        public string searchDataAcptDate { get; set; }
        public string installLocationCount { get; set; }
        public string electrDeliveryMethod { get; set; }
        public string inspectionNeedYN { get; set; }
        public string addConstructCostSearch { get; set; }
        public string addConstructCost { get; set; }
        public string searchComments { get; set; }
        public string corpAcptNo { get; set; }
        public string corpApprovalDate { get; set; }
        public string corpEndDate { get; set; }
        public string corpLastEndDate { get; set; }
        public string corpComments { get; set; }
        public string kepInstallLocationCount { get; set; }
        public string kepElectrDeliveryMethod { get; set; }
        public string kepOutLineConstructContext { get; set; }
        public string kepInfraPayAmount { get; set; }
        public string kepManageInfraPayAmount { get; set; }
        public string kepElectrReqDate { get; set; }
        public string kepInApprovalYN { get; set; }
        public string kepInApprovalDate { get; set; }
        public string kepMeterInstallContext { get; set; }
        public string kepDamdangjaPhone { get; set; }
        public string kepCustomNo { get; set; }
        public string kepPaymentDate { get; set; }
        public string kepMeterInstallDate { get; set; }
        public string kepFaucetComments { get; set; }

        public string constrCustomName { get; set; }
        public string constrCustomID { get; set; }
        public string constrOrderDate { get; set; }
        public string constrDate { get; set; }
        public string constrDelyReason { get; set; }

        public string constrCompleteDate { get; set; }
        public string constrComments { get; set; }
        public string electrSafeCheckDate { get; set; }
        public string electrSafeCheckSuppleContext { get; set; }
        public string electrSafeCheckLocation { get; set; }
        public string electrSafeCheckCost { get; set; }
        public string electrSafeCheckCostPayDate { get; set; }
        public string electrBeforeUseCheckReqDate { get; set; }
        public string electrSafeCheckPrintDate { get; set; }
        public string electrBeforeUseCheckSuppleContext { get; set; }
        public string electrBeforeInspLocation { get; set; }
        public string electrBeforeInspReqDate { get; set; }
        public string electrBeforeInspPrintDate { get; set; }
        public string electrBeforeInspCost { get; set; }
        public string electrBeforeInspCostPayDate { get; set; }
        public string electrBeforeInspSuppleContext { get; set; }
        public string electrSafeCheckComments { get; set; }
        public string superCustomName { get; set; }
        public string superCustomID { get; set; }
        public string superCostPayCustom { get; set; }
        public string superCostPayCustomID { get; set; }
        public string superCustomPhoneNo { get; set; }
        public string safeManageCustomName { get; set; }
        public string safeManageCustomID { get; set; }
        public string safeManageCustomPhoneNo { get; set; }
        public string superSetCost { get; set; }
        public string superSetTaxPrintDate { get; set; }
        public string superUseInspPayCustomName { get; set; }
        public string superUseInspPayCustomID { get; set; }
        public string superUseInspReqDate { get; set; }
        public string superFromUseInspReqDate { get; set; }
        public string superBeforeUseInspDate { get; set; }
        public string superComments { get; set; }
        public string compReplyDate { get; set; }
        public string suppleContext { get; set; }
        public string suppleCompDate { get; set; }
        public string compSuppleReportContext { get; set; }
        public string compSuppleReportDate { get; set; }
        public string insurePrintDate { get; set; }
        public string compReportCompDate { get; set; }
        public string compReportComments { get; set; }
        public string accntMgrWorkPreTaxPrintDate { get; set; }
        public string accntMgrWorkPreAmount { get; set; }
        public string accntMgrWorkPreAmountComments { get; set; }
        public string accntMgrWorkAfterTaxPrintDate { get; set; }
        public string accntMgrWorkAfterAmount { get; set; }
        public string accntMgrWorkAfterAmountComments { get; set; }
        public string accntMgrWorkTaxPrintDate { get; set; }
        public string accntMgrWorkAmount { get; set; }
        public string accntMgrWorkAmountComments { get; set; }
        public string accntWorkTaxPrintDate { get; set; }
        public string accntWorkAmount { get; set; }
        public string accntWorkAmountComments { get; set; }
        public string accntSalesTaxPrintDate { get; set; }
        public string accntSalesAmount { get; set; }
        public string accntSalesAmountComments { get; set; }







    }
}
