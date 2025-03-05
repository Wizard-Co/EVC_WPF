using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_EVC.PopUP;
using WizMes_EVC.PopUp;
using WPF.MDI;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Text.RegularExpressions;
using WizMes_EVC.Order.Pop;
using System.Threading;
using System.Windows.Threading;


/**************************************************************************************************
'** 프로그램명 : Win_ord_Close_U
'** 설명       : 수주등록
'** 작성일자   : 2024.12.31
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2025.02.17, 최대현,                              첨부파일 팝업 추가
' 2025.02.28, 최대현,  김동호 팀장                 CPO정산일, 시공사정산일 컬럼 추가, 정산할때 필요하다고 하고 배경색 따로 줌
*/

namespace WizMes_EVC
{
    /// <summary>
    /// Win_ord_OrderClose_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_OrderClose_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;

        private ToolTip toolTip = new ToolTip();
        Win_ord_OrderClose_U_CodeView WinOrderClose = new Win_ord_OrderClose_U_CodeView();
        Lib lib = new Lib();
        string rowHeaderNum = string.Empty;
        string orderID_global = string.Empty;
        int rowNum = 0;
        int rbnOrder = 0;

        private Win_ord_Pop_OrderClose_File_Q OrderClose_filePop;

        NoticeMessage msg = new NoticeMessage();
        DataTable DT;
        ////private List<DataGridColumn> _dynamicColumns = new List<DataGridColumn>();




        public Win_ord_OrderClose_U()
        {
            InitializeComponent();
            this.GotFocus += Win_ord_OrderClose_U_GotFocus;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            btnToday_Click(null, null);
            SetComboBox();

        }

        private void Win_ord_OrderClose_U_GotFocus(object sender, EventArgs e)
        {
            if(dgdMain.Items.Count > 0 && dgdMain.SelectedIndex != -1)
            {
                var item = dgdMain.SelectedItem as Win_ord_OrderClose_U_CodeView;
                if(item != null)
                {
                    MainWindow.OrderID = item.orderid;
                }
            }
        }

        //콤보박스 세팅
        private void SetComboBox()
        {
            List<string> strValue = new List<string>();
            strValue.Add("전체");
            strValue.Add("진행건");
            strValue.Add("마감건");

            ObservableCollection<CodeView> cbOrderStatus = ComboBoxUtil.Instance.Direct_SetComboBox(strValue);
            cboOrderStatus.ItemsSource = cbOrderStatus;
            cboOrderStatus.DisplayMemberPath = "code_name";
            cboOrderStatus.SelectedValuePath = "code_id";
            cboOrderStatus.SelectedIndex = 0;

        }

        #region 라벨 체크박스 이벤트 관련

        //일자
        private void lblOrderDay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrderDay.IsChecked == true) { chkOrderDay.IsChecked = false; }
            else { chkOrderDay.IsChecked = true; }
        }

        //일자
        private void chkOrderDay_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpSDate != null && dtpEDate != null)
            {
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        //일자
        private void chkOrderDay_Unchecked(object sender, RoutedEventArgs e)
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

        //전월
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            //dtpSDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[0];
            //dtpEDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[1];

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

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        ////금년
        //private void btnThisYear_Click(object sender, RoutedEventArgs e)
        //{
        //    dtpSDate.SelectedDate = Lib.Instance.BringThisYearDatetimeFormat()[0];
        //    dtpEDate.SelectedDate = Lib.Instance.BringThisYearDatetimeFormat()[1];
        //}

        //전일
        private void BtnYesterDay_Click(object sender, RoutedEventArgs e)
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


        //수주 진행 건은 마감처리 / 마감 건은 진행처리로 변경하는 버튼
        private void BtnFinal_Click(object sender, RoutedEventArgs e)
        {
            //string OrderID = string.Empty;

            // 다중선택 했을 때 각각 OrderID 들어가도록 설정했으므로 이건 안써도 돼
            //var Order = dgdMain.SelectedItem as Win_ord_OrderClose_U_CodeView;
            //if (Order != null)
            //{
            //    OrderID = Order.OrderID;
            //}

            string CloseFlag = string.Empty;
            string CloseClss = string.Empty;

            if (btnFinal.Content.ToString().Equals("마감처리"))
            {
                CloseFlag = "1";
                CloseClss = "1";

                if (MessageBox.Show("해당 건을 마감처리 하시겠습니까?", "처리 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                    {
                        rowNum = dgdMain.SelectedIndex;
                    }
                }
            }
            else if (btnFinal.Content.ToString().Equals("진행처리"))
            {
                CloseFlag = "2";
                CloseClss = "";

                if (MessageBox.Show("해당 건을 진행처리 하시겠습니까?", "처리 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                    {
                        rowNum = dgdMain.SelectedIndex;
                    }
                }
            }

            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
            try
            {
                //일괄처리할 때 쓰는 변수
                int CheckCount = 0;

                //데이터그리드의 체크박스 true된 수 많음 CheckCount 수 늘리기
                foreach (Win_ord_OrderClose_U_CodeView OrderCloseU in dgdMain.Items)
                {
                    if (OrderCloseU.IsCheck == true)
                    {
                        CheckCount++;
                    }
                }

                //체크된 그리드가 하나 이상일 경우(1개라도 체크가 되어 있을 경우)
                if (CheckCount > 0)
                {
                    foreach (Win_ord_OrderClose_U_CodeView OrderCloseU in dgdMain.Items)
                    {
                        if (OrderCloseU != null)
                        {
                            if (OrderCloseU.IsCheck == true)
                            {
                                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter.Add("CloseFlag", CloseFlag);
                                //sqlParameter.Add("OrderID", OrderCloseU.OrderID);
                                sqlParameter.Add("CloseClss", CloseClss);

                                Procedure pro1 = new Procedure();
                                pro1.Name = "xp_OrderClose_uCloseClss";     //마감처리 누르면 CloseClss에 1 저장, 진행처리 누르면 '' 저장 Order테이블에.
                                pro1.OutputUseYN = "N";
                                pro1.OutputName = "OrderID";
                                pro1.OutputLength = "10";

                                Prolist.Add(pro1);
                                ListParameter.Add(sqlParameter);
                            }
                        }
                    }

                    string[] Confirm = new string[2];
                    Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                    if (Confirm[0] != "success")
                    {
                        MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("[저장실패]\r\n 처리할 체크항목이 없습니다.");
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

            dgdMain.Items.Clear();
            FillGrid();
        }





        //헤더 높이 늘이기
        private void Style_ColumnHead_Stretch()
        {
            // 높이를 32으로 설정하고 너비를 Auto로 설정
            Style headerStyle = dgdMain.ColumnHeaderStyle;
            if (headerStyle != null)
            {
                // 높이를 30으로 설정하고 너비를 Auto로 설정
                Style newHeaderStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader), dgdMain.ColumnHeaderStyle);
                newHeaderStyle.Setters.Add(new System.Windows.Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HeightProperty, 32.0));

                dgdMain.ColumnHeaderStyle = newHeaderStyle;
            }
        }

        //헤더높이 줄이기
        private void Style_ColumHead_Shrink()
        {
            // 높이를 22로 설정하고 너비를 Auto로 설정
            Style newHeaderStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader), dgdMain.ColumnHeaderStyle);
            newHeaderStyle.Setters.Add(new System.Windows.Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HeightProperty, 22.0));
            dgdMain.ColumnHeaderStyle = newHeaderStyle;
        }



        private void FreezeColumns()
        {
            // 고정할 열의 이름 배열
            string[] columnsToFreeze = { "관리번호" };

            int frozenColumnCount = 0;

            foreach (string columnName in columnsToFreeze)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);

                if (column != null)
                {
                    frozenColumnCount = column.DisplayIndex + 1;
                }
            }

            dgdMain.FrozenColumnCount = frozenColumnCount;
        }



        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(re_Search))
            {
                ld.ShowDialog();
            }
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            this.GotFocus -= Win_ord_OrderClose_U_GotFocus;
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //인쇄
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = btnPrint.ContextMenu;
            menu.StaysOpen = true;
            menu.IsOpen = true;
        }

        //인쇄 미리보기
        private void menuSeeAhead_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }

            msg.Show();
            msg.Topmost = true;
            msg.Refresh();

            PrintWork(true);
        }

        //바로 인쇄
        private void menuRightPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }

            DataStore.Instance.InsertLogByForm(this.GetType().Name, "P");
            msg.Show();
            msg.Topmost = true;
            msg.Refresh();

            PrintWork(false);
        }

        private void menuClose_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = btnPrint.ContextMenu;
            menu.StaysOpen = false;
            menu.IsOpen = false;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {          
            DataTable dt = null;
            string[] lst = new string[2];
            lst[0] = "수주조회";
            lst[1] = dgdMain.Name;
            Lib lib = new Lib();
            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if(dgdMain.Items.Count == 0)
            {
                MessageBox.Show("내보낼 데이터가 없습니다.");
                return;
            }

            if (ExpExc.DialogResult.HasValue)
            {
                lblMsg.Visibility = Visibility.Visible;
                UpdateTbkMessage("엑셀을 내보내는 중");
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");

                    // 임시 파일 이름 생성 (현재 날짜와 시간을 포함)
                    string tempFileName = $"{dgdMain.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";

                    //if (ExpExc.Check.Equals("Y"))
                    //{
                     // 다중 헤더 내보내기 새 메서드 사용
                    if (lib.ExportToExcelWithMultiLevelHeaders(dgdMainHeaderSh, dgdMain, tempFileName))
                    {
                        UpdateTbkMessage("내보내기 완료");
                        MessageBox.Show("엑셀 내보내기가 완료 되었습니다.");
                    }
                    //}
                    //else
                    //{
                    //    // 기존 방식 사용
                    //    dt = lib.DataGirdToDataTable(dgdMain);
                    //    lib.GenerateExcel(dt, dgdMain.Name);
                    //}
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }

            // lib 변수는 해제하되 Excel 객체는 살려둠 (사용자가 계속 작업하기 위해)
            lib = null;
            lblMsg.Visibility = Visibility.Hidden;

        }

        private void UpdateTbkMessage(string message)
        {
            tbkMsg.Text = message;
            tbkMsg.UpdateLayout();
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            // UI 업데이트를 위한 짧은 대기
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
            Thread.Sleep(10);
        }

        //실조회 및 하단 합계
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
                dgdMain.Items.Clear();

            if (dgdSum.Items.Count > 0)
                dgdSum.Items.Clear();



            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ChkDate", chkOrderDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkOrderDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkOrderDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                sqlParameter.Add("chkSaleCustomID", chkSalesCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SaleCustomID", chkSalesCustomSrh.IsChecked == true ? txtSalesCustomSrh.Tag != null ? txtSalesCustomSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkManageCustomID", chkManageCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManageCustomID", chkManageCustomSrh.IsChecked == true ? txtManageCustomSrh.Tag != null ? txtManageCustomSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkArticleID", chkArticleIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleIdSrh.IsChecked == true ? txtArticleIdSrh.Tag != null ? txtArticleIdSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", txtInstallLocationSrh.Text);

                sqlParameter.Add("chkCpoCalcuDate", chkCpoCalcuDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CpoCalcuSdate", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuSdateSrh) ? ConvertDate(dtpCpoCalcuSdateSrh) : "" : "");
                sqlParameter.Add("CpoCalcuEdate", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuEdateSrh) ? ConvertDate(dtpCpoCalcuEdateSrh) : "" : "");

                sqlParameter.Add("chkConstrCalcuDate", chkConstrCalcuDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ConstrCalcuSdate", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuSdateSrh) ? ConvertDate(dtpConstrCalcuSdateSrh) : "" : "");
                sqlParameter.Add("ConstrCalcuEdate", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuEdateSrh) ? ConvertDate(dtpConstrCalcuEdateSrh) : "" : "");



                // 수주상태
                sqlParameter.Add("ChkClose", int.Parse(cboOrderStatus.SelectedValue != null ? cboOrderStatus.SelectedValue.ToString() : ""));



                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Order_sOrderTotal", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    //dataGrid.Items.Clear();
                    if (dt.Rows.Count == 0)
                    {
                        Style_ColumHead_Shrink();
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        int i = 0;
                  

                        foreach (DataRow item in drc)
                        {
                            var Window_OrderClose_DTO = new Win_ord_OrderClose_U_CodeView()
                            {


                                IsCheck = false,
                                Num = i + 1,

                                //기본정보
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

                                //국소정보
                                cpoCalcuDate = DateTypeHyphen(item["cpoCalcuDate"].ToString()),
                                constrCalcuDate = DateTypeHyphen(item["constrCalcuDate"].ToString()),
                                installLocation = item["installLocation"].ToString(),
                                installLocationAddress = item["installLocationAddress"].ToString(),
                                InstallLocationPhone = item["InstallLocationPhone"].ToString(),
                                installLocationPart = item["installLocationPart"].ToString(),
                                houseHoldCount = stringFormatN0(item["houseHoldCount"]),
                                carParkingCount = stringFormatN0(item["carParkingCount"]),
                                electrCarCount = stringFormatN0(item["electrCarCount"]),
                                alReadyChargeCount = stringFormatN0(item["alReadyChargeCount"]),

                                reqChargeCount = stringFormatN0(item["reqChargeCount"]),
                                alreadyManageCustomName = item["alreadyManageCustomName"].ToString(),

                                alreadyManageCustomID = item["alreadyManageCustomID"].ToString(),
                                installLocationComments = item["installLocationComments"].ToString(),
                                contractFromDate = DateTypeHyphen(item["contractFromDate"].ToString()),
                                contractToDate = DateTypeHyphen(item["contractToDate"].ToString()),
                                openReqDate = DateTypeHyphen(item["openReqDate"].ToString()),
                                openDate = DateTypeHyphen(item["openDate"].ToString()),
                                damdangjaName = item["damdangjaName"].ToString(),
                                damdangjaPhone = item["damdangjaPhone"].ToString(),
                                damdangjaEMail = item["damdangjaEMail"].ToString(),
                                installLocationAddComments = item["installLocationAddComments"].ToString(),

                                //영업회사
                                saledamdangjaPhone = item["saledamdangjaPhone"].ToString(),
                                saleCustomAddWork = item["saleCustomAddWork"].ToString(),
                                salegift = item["salegift"].ToString(),

                                //기기 및 액서사리 정보
                                article = item["article"].ToString(),
                                //chargeOrderDate = DateTypeHyphen(item["chargeOrderDate"].ToString()), //충전기발주
                                //chargeInwareDate = DateTypeHyphen(item["chargeInwareDate"].ToString()),//충전기입고
                                chargeInwareQty = item["chargeInwareQty"].ToString(),
                                chargeInwareLocation = item["chargeInwareLocation"].ToString(),
                                canopyReqCustom = item["canopyReqCustom"].ToString(),
                                chargeModelHelmat = item["chargeModelHelmat"].ToString(),
                                chargeModelinloc = item["chargeModelinloc"].ToString(),
                                chargeModelOneBody = item["chargeModelOneBody"].ToString(),
                                chargeStandReqDate = DateTypeHyphen(item["chargeStandReqDate"].ToString()),
                                chargeStandInwareDate = DateTypeHyphen(item["chargeStandInwareDate"].ToString()),
                                mtrCanopyInwareInfo = item["mtrCanopyInwareInfo"].ToString(),
                                mtrCanopyOrderAmount = stringFormatN0(item["mtrCanopyOrderAmount"]),
                                comments = item["comments"].ToString(),

                                //시공 및 실사 정보
                                searchReqDate = DateTypeHyphen(item["searchReqDate"].ToString()),
                                searchDate = DateTypeHyphen(item["searchDate"].ToString()),
                                searchQty = stringFormatN0(item["searchQty"]),
                                searchDataAcptDate = DateTypeHyphen(item["searchDataAcptDate"].ToString()),
                                installLocationCount = stringFormatN0(item["installLocationCount"]),
                                electrDeliveryMethod = item["electrDeliveryMethod"].ToString(),
                                inspectionNeedYN = item["inspectionNeedYN"].ToString(),
                                addConstructCostSearch = stringFormatN0(item["addConstructCostSearch"]),
                                addConstructCost = stringFormatN0(item["addConstructCost"]),
                                searchComments = stringFormatN0(item["searchComments"]),

                                //지자체사항
                                superUseInspReqDate = DateTypeHyphen(item["superUseInspReqDate"].ToString()),
                                superBeforeUseInspPrintDate = DateTypeHyphen(item["superBeforeUseInspPrintDate"].ToString()),

                                //공단사항
                                corpAcptNo = item["corpAcptNo"].ToString(),
                                corpApprovalDate = DateTypeHyphen(item["corpApprovalDate"].ToString()),
                                corpEndDate = DateTypeHyphen(item["corpEndDate"].ToString()),
                                corpLastEndDate = DateTypeHyphen(item["corpLastEndDate"].ToString()),
                                corpComments = item["corpComments"].ToString(),
                                kepInstallLocationCount = stringFormatN0(item["kepInstallLocationCount"]),
                                kepElectrDeliveryMethod = item["kepElectrDeliveryMethod"].ToString(),
                                kepOutLineConstructContext = item["kepOutLineConstructContext"].ToString(),
                                kepInfraPayAmount = stringFormatN0(item["kepInfraPayAmount"]),
                                kepManageInfraPayAmount = stringFormatN0(item["kepManageInfraPayAmount"]),
                                kepManageInfraPayDate = DateTypeHyphen(item["kepManageInfraPayDate"].ToString()),
                                kepElectrReqDate = DateTypeHyphen(item["kepElectrReqDate"].ToString()),
                                kepInApprovalYN = item["kepInApprovalYN"].ToString(),
                                kepInApprovalDate = DateTypeHyphen(item["kepInApprovalDate"].ToString()),
                                //kepMeterInstallContext = item["kepMeterInstallContext"].ToString(),
                                kepDamdangjaPhone = item["kepDamdangjaPhone"].ToString(),
                                kepCustomNo = item["kepCustomNo"].ToString(),
                                kepPaymentDate = DateTypeHyphen(item["kepPaymentDate"].ToString()),
                                kepMeterInstallDate = DateTypeHyphen(item["kepMeterInstallDate"].ToString()),
                                kepFaucetComments = item["kepFaucetComments"].ToString(),

                                //전기안전공사진행정보
                                constrCustomName = item["constrCustomName"].ToString(),
                                constrCustomID = item["constrCustomID"].ToString(),
                                constrOrderDate = DateTypeHyphen(item["constrOrderDate"].ToString()),
                                constrDate = DateTypeHyphen(item["constrDate"].ToString()),
                                constrDelyReason = item["constrDelyReason"].ToString(),
                                constrCompleteDate = DateTypeHyphen(item["constrCompleteDate"].ToString()),
                                constrComments = item["constrComments"].ToString(),
                                electrSafeCheckDate = DateTypeHyphen(item["electrSafeCheckDate"].ToString()),
                                electrSafeCheckSuppleContext = item["electrSafeCheckSuppleContext"].ToString(),
                                electrSafeCheckLocation = item["electrSafeCheckLocation"].ToString(),
                                electrSafeCheckCost = stringFormatN0(item["electrSafeCheckCost"]),
                                electrSafeCheckCostPayDate = DateTypeHyphen(item["electrSafeCheckCostPayDate"].ToString()),
                                electrBeforeUseCheckReqDate = DateTypeHyphen(item["electrBeforeUseCheckReqDate"].ToString()),
                                electrSafeCheckPrintDate = DateTypeHyphen(item["electrSafeCheckPrintDate"].ToString()),
                                electrBeforeUseCheckSuppleContext = item["electrBeforeUseCheckSuppleContext"].ToString(),
                                electrBeforeInspLocation = item["electrBeforeInspLocation"].ToString(),
                                electrBeforeInspReqDate = DateTypeHyphen(item["electrBeforeInspReqDate"].ToString()),
                                electrBeforeInspPrintDate = item["electrBeforeInspPrintDate"].ToString(),
                                electrBeforeInspCost = stringFormatN0(item["electrBeforeInspCost"]),
                                electrBeforeInspCostPayDate = DateTypeHyphen(item["electrBeforeInspCostPayDate"].ToString()),
                                electrBeforeInspSuppleContext = item["electrBeforeInspSuppleContext"].ToString(),
                                electrSafeCheckComments = item["electrSafeCheckComments"].ToString(),

                                //감리
                                superCustomName = item["superCustomName"].ToString(),
                                superCustomID = item["superCustomID"].ToString(),
                                //superCostPayCustom = item["superCostPayCustom"].ToString(),
                                //superCostPayCustomID = item["superCostPayCustomID"].ToString(),
                                superCustomPhoneNo = item["superCustomPhoneNo"].ToString(),
                                safeManageCustomName = item["safeManageCustomName"].ToString(),
                                safeManageCustomID = item["safeManageCustomID"].ToString(),
                                safeManageCustomPhoneNo = item["safeManageCustomPhoneNo"].ToString(),
                                superSetCost = stringFormatN0(item["superSetCost"]),
                                superSetTaxPrintDate = DateTypeHyphen(item["superSetTaxPrintDate"].ToString()),
                                superUseInspPayCustomName = item["superUseInspPayCustomName"].ToString(),
                                superUseInspPayCustomID = item["superUseInspPayCustomID"].ToString(),
                                //superFromUseInspReqDate = DateTypeHyphen(item["superFromUseInspReqDate"].ToString()),
                                //superBeforeUseInspDate = DateTypeHyphen(item["superBeforeUseInspDate"].ToString()),
                                superComments = item["superComments"].ToString(),

                                //준공서류
                                compReplyDate = DateTypeHyphen(item["compReplyDate"].ToString()),
                                suppleContext = item["suppleContext"].ToString(),
                                suppleCompDate = DateTypeHyphen(item["suppleCompDate"].ToString()),
                                compSuppleReportContext = item["compSuppleReportContext"].ToString(),
                                compSuppleReportDate = DateTypeHyphen(item["compSuppleReportDate"].ToString()),
                                insurePrintDate = DateTypeHyphen(item["insurePrintDate"].ToString()),
                                compReportCompDate = DateTypeHyphen(item["compReportCompDate"].ToString()),
                                compReportComments = item["compReportComments"].ToString(),

                                //정산경리 정보
                                //운영사시공비
                                accntMgrWorkPreTaxPrintDate = DateTypeHyphen(item["accntMgrWorkPreTaxPrintDate"].ToString()),
                                accntMgrWorkPreAmount = stringFormatN0(item["accntMgrWorkPreAmount"]),
                                accntMgrWorkPreAmountComments = item["accntMgrWorkPreAmountComments"].ToString(),
                                //운영사영업비
                                accntMgrSalesPreTaxPrintDate = DateTypeHyphen(item["accntMgrSalesPreTaxPrintDate"].ToString()),
                                accntMgrSalesPreAmount = stringFormatN0(item["accntMgrSalesPreAmount"]),
                                accntMgrSalesPreAmountComments = item["accntMgrSalesPreAmountComments"].ToString(),
                                //시공팀
                                accntWorkPreTaxPrintDate = DateTypeHyphen(item["accntWorkPreTaxPrintDate"].ToString()),
                                accntWorkPreAmount = stringFormatN0(item["accntWorkPreAmount"]),
                                accntWorkPreAmountComments = item["accntWorkPreAmountComments"].ToString(),
                                //영업사원
                                accntSalesPreTaxPrintDate = DateTypeHyphen(item["accntSalesPreTaxPrintDate"].ToString()),
                                accntSalesPreAmount = stringFormatN0(item["accntSalesPreAmount"]),
                                accntSalesPreAmountComments = item["accntSalesPreAmountComments"].ToString(),
                                //accntMgrWorkPreTaxPrintDate = DateTypeHyphen(item["accntMgrWorkPreTaxPrintDate"].ToString()),
                                //accntMgrWorkPreAmount = item["accntMgrWorkPreAmount"].ToString(),
                                //accntMgrWorkPreAmountComments = item["accntMgrWorkPreAmountComments"].ToString(),

                                //accntMgrWorkAfterTaxPrintDate = DateTypeHyphen(item["accntMgrWorkAfterTaxPrintDate"].ToString()),
                                //accntMgrWorkAfterAmount = item["accntMgrWorkAfterAmount"].ToString(),
                                //accntMgrWorkAfterAmountComments = item["accntMgrWorkAfterAmountComments"].ToString(),

                                //accntMgrWorkTaxPrintDate = DateTypeHyphen(item["accntMgrWorkTaxPrintDate"].ToString()),
                                //accntMgrWorkAmount = item["accntMgrWorkAmount"].ToString(),
                                //accntMgrWorkAmountComments = item["accntMgrWorkAmountComments"].ToString(),

                                //accntWorkTaxPrintDate = DateTypeHyphen(item["accntWorkTaxPrintDate"].ToString()),
                                //accntWorkAmount = item["accntWorkAmount"].ToString(),
                                //accntWorkAmountComments = item["accntWorkAmountComments"].ToString(),

                                //accntSalesTaxPrintDate = item["accntSalesTaxPrintDate"].ToString(),
                                //accntSalesAmount = item["accntSalesAmount"].ToString(),
                                //accntSalesAmountComments = item["accntSalesAmountComments"].ToString(),

                            };
                            dgdMain.Items.Add(Window_OrderClose_DTO);

                            i++;
                        }
                    }

                }
                if (ds.Tables.Count == 0)
                {
                    MessageBox.Show("조회된 데이터가 없습니다.");

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








        //실조회 및 하단 합계
        private void FillGridSub()
        {

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ChkDate", chkOrderDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkOrderDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkOrderDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                sqlParameter.Add("chkSaleCustomID", chkSalesCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SaleCustomID", chkSalesCustomSrh.IsChecked == true ? txtSalesCustomSrh.Tag != null ? txtSalesCustomSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkManageCustomID", chkManageCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManageCustomID", chkManageCustomSrh.IsChecked == true ? txtManageCustomSrh.Tag != null ? txtManageCustomSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkArticleID", chkArticleIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleIdSrh.IsChecked == true ? txtArticleIdSrh.Tag != null ? txtArticleIdSrh.Tag.ToString() : "" : "");

                sqlParameter.Add("chkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", txtInstallLocationSrh.Text);

                sqlParameter.Add("chkCpoCalcuDate", chkCpoCalcuDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CpoCalcuSdate", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuSdateSrh) ? ConvertDate(dtpCpoCalcuSdateSrh) : "" : "");
                sqlParameter.Add("CpoCalcuEdate", chkCpoCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpCpoCalcuEdateSrh) ? ConvertDate(dtpCpoCalcuEdateSrh) : "" : "");

                sqlParameter.Add("chkConstrCalcuDate", chkConstrCalcuDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ConstrCalcuSdate", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuSdateSrh) ? ConvertDate(dtpConstrCalcuSdateSrh) : "" : "");
                sqlParameter.Add("ConstrCalcuEdate", chkConstrCalcuDateSrh.IsChecked == true ? !IsDatePickerNull(dtpConstrCalcuEdateSrh) ? ConvertDate(dtpConstrCalcuEdateSrh) : "" : "");

                // 수주상태
                sqlParameter.Add("ChkClose", int.Parse(cboOrderStatus.SelectedValue != null ? cboOrderStatus.SelectedValue.ToString() : ""));



                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Order_sOrderTotal_Sum", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    //dataGrid.Items.Clear();
                    if (dt.Rows.Count == 0)
                    {
                        Style_ColumHead_Shrink();
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                          
                        double dateDiff= 0;
                        double zeroDiffCount = 0; //orderColor 입고일이 아에 없는 것 카운트(KPI 계산할때 빼야됨)

                        DataRow dr = dt.Rows[0];
                     
                        //foreach (DataRow dr in drc)
                        //{
                            var Window_OrderClose_DTO = new dgOrderSum()
                            {


                                orderSum = dr["orderSum"].ToString(),
                                dateDiffSum = dr["dateDiffSum"].ToString(),
                                mtrAmount = stringFormatN0(dr["mtrAmount"]),
                                zeroDiffSumCount = dr["zeroDiffSumCount"].ToString(),

                            };

                            if (!string.IsNullOrEmpty(Window_OrderClose_DTO.zeroDiffSumCount))
                            {
                                zeroDiffCount = Convert.ToDouble(Window_OrderClose_DTO.zeroDiffSumCount);
                                dateDiff = Window_OrderClose_DTO.dateDiffSum != string.Empty ?  Convert.ToDouble(Window_OrderClose_DTO.dateDiffSum) / Convert.ToDouble(dgdMain.Items.Count - zeroDiffCount) : 0;
                                dateDiff = double.IsNaN(dateDiff) ? 0 : dateDiff;
                            }

                            Window_OrderClose_DTO.dateDiffSum = dateDiff.ToString();
                            //PersonSum = 3 * ConvertInt(Window_OrderClose_DTO.Treat) * ConvertInt(Window_OrderClose_DTO.orderSum);
                            Window_OrderClose_DTO.PersonSum = stringFormatN0(SetJobDoneByPerson());

                            Window_OrderClose_DTO.Count = dgdMain.Items.Count;
                            dgdSum.Items.Add(Window_OrderClose_DTO);

                            //i++;

                        //}



                    }

                }
                if (ds.Tables.Count == 0)
                {
                    MessageBox.Show("조회된 데이터가 없습니다.");

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


        private int SetJobDoneByPerson()
        {
            int value = 0;
            int person = 3; //KPI 작업인원

            if(dgdMain.Items.Count > 0)
            {
                foreach(Win_ord_OrderClose_U_CodeView item in dgdMain.Items)
                {
                    
                    int step = 0;
                    if (!string.IsNullOrEmpty(item.acptDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.corpApprovalDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.localGovBehaviorReportDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.chargeStandReqDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.kepElectrReqDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.constrDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.constrCompleteDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.superBeforeUseInspPrintDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.kepMeterInstallDate)) { step++; }
                    if (!string.IsNullOrEmpty(item.accntMgrWorkPreTaxPrintDate) 
                        || !string.IsNullOrEmpty(item.accntMgrSalesPreTaxPrintDate)
                        || !string.IsNullOrEmpty(item.accntSalesPreTaxPrintDate)
                        || !string.IsNullOrEmpty(item.accntWorkPreTaxPrintDate)) { step++; }

                    value += person * step * dgdMain.Items.Count;
                }
            }

            if(value != 0 && value > 0)
            {
                value = value / dgdMain.Items.Count;
            }

            return value;
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


        private bool HasNonNullValue(DataRowCollection drc, string propertyName)
        {
            foreach (DataRow row in drc)
            {
                if (row[propertyName] != null && !string.IsNullOrEmpty(row[propertyName].ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        //전체선택
        private void btnAllCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (Win_ord_OrderClose_U_CodeView woccv in dgdMain.Items)
            {
                woccv.IsCheck = true;
            }
        }

        //선택해제
        private void btnAllNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (Win_ord_OrderClose_U_CodeView woccv in dgdMain.Items)
            {
                woccv.IsCheck = false;
            }
        }

        //인쇄 실질 동작
        private void PrintWork(bool preview_click)
        {
            Lib lib2 = new Lib();

            try
            {
                excelapp = new Microsoft.Office.Interop.Excel.Application();

                string MyBookPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Report\\수주진행현황(영업관리).xls";
                //MyBookPath = MyBookPath.Substring(0, MyBookPath.LastIndexOf("\\")) + "\\order_standard.xls";
                //string MyBookPath = "C:/Users/Administrator/Desktop/order_standard.xls";
                workbook = excelapp.Workbooks.Add(MyBookPath);
                worksheet = workbook.Sheets["Form"];

                //상단의 일자 
                if (chkOrderDay.IsChecked == true)
                {
                    workrange = worksheet.get_Range("E2", "Q2");//셀 범위 지정
                    workrange.Value2 = dtpSDate.Text + "~" + dtpEDate.Text;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //workrange.Font.Size = 10;
                }
                else
                {
                    workrange = worksheet.get_Range("E2", "K2");//셀 범위 지정
                    workrange.Value2 = "전체"; //"" + "~" + "";
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //workrange.Font.Size = 10;
                }


                //하단의 회사명
                workrange = worksheet.get_Range("AN35", "AU35");//셀 범위 지정
                workrange.Value2 = "주식회사 지엘에스";
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                workrange.Font.Size = 11;


                /////////////////////////
                int Page = 0;
                int DataCount = 0;
                int copyLine = 0;

                copysheet = workbook.Sheets["Form"];
                pastesheet = workbook.Sheets["Print"];

                DT = lib2.DataGirdToDataTable(dgdMain);

                string str_Num = string.Empty;
                string str_OrderID = string.Empty;
                string str_OrderID_CV = string.Empty;
                string str_KCustom = string.Empty;
                string str_Article = string.Empty;
                string str_Model = string.Empty;
                string str_ArticleNo = string.Empty;
                string str_DvlyDate = string.Empty;
                string str_Work = string.Empty;
                string str_OrderQty = string.Empty;
                string str_UnitClssName = string.Empty;
                string str_DayAndTime = string.Empty;
                string str_p1WorkQty = string.Empty;
                string str_InspectQty = string.Empty;
                string str_PassQty = string.Empty;
                string str_DefectQty = string.Empty;
                string str_OutQty = string.Empty;

                int TotalCnt = dgdMain.Items.Count;
                int canInsert = 27; //데이터가 입력되는 행 수 27개

                int PageCount = (int)Math.Ceiling(1.0 * TotalCnt / canInsert);

                var Sum = new dgOrderSum();

                //while (dgdMain.Items.Count > DataCount + 1)
                for (int k = 0; k < PageCount; k++)
                {
                    Page++;
                    if (Page != 1) { DataCount++; }  //+1
                    copyLine = (Page - 1) * 38;
                    copysheet.Select();
                    copysheet.UsedRange.Copy();
                    pastesheet.Select();
                    workrange = pastesheet.Cells[copyLine + 1, 1];
                    workrange.Select();
                    pastesheet.Paste();

                    int j = 0;
                    for (int i = DataCount; i < dgdMain.Items.Count; i++)
                    {
                        if (j == 27) { break; }
                        int insertline = copyLine + 7 + j;

                        str_Num = (j + 1).ToString();
                        str_OrderID = DT.Rows[i][1].ToString();
                        str_OrderID_CV = DT.Rows[i][2].ToString();
                        str_KCustom = DT.Rows[i][3].ToString();
                        str_Article = DT.Rows[i][4].ToString();
                        str_Model = DT.Rows[i][5].ToString();
                        str_ArticleNo = DT.Rows[i][6].ToString();
                        str_DvlyDate = DT.Rows[i][7].ToString();
                        str_Work = DT.Rows[i][8].ToString();
                        str_OrderQty = DT.Rows[i][9].ToString();
                        str_UnitClssName = DT.Rows[i][10].ToString();
                        str_DayAndTime = DT.Rows[i][11].ToString();
                        str_p1WorkQty = DT.Rows[i][12].ToString();
                        str_InspectQty = DT.Rows[i][13].ToString();
                        str_PassQty = DT.Rows[i][14].ToString();
                        str_DefectQty = DT.Rows[i][15].ToString();
                        str_OutQty = DT.Rows[i][16].ToString();

                        workrange = pastesheet.get_Range("A" + insertline, "B" + insertline);    //순번
                        workrange.Value2 = str_Num;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.3;



                        workrange = pastesheet.get_Range("G" + insertline, "J" + insertline);     //거래처
                        workrange.Value2 = str_KCustom;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 9;
                        workrange.ColumnWidth = 2.7;

                        workrange = pastesheet.get_Range("K" + insertline, "N" + insertline);    //품명
                        workrange.Value2 = str_Article;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 2.7;

                        workrange = pastesheet.get_Range("O" + insertline, "R" + insertline);    //차종
                        workrange.Value2 = str_Model;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 0.9;

                        workrange = pastesheet.get_Range("S" + insertline, "V" + insertline);    //품번
                        workrange.Value2 = str_ArticleNo;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 2.7;

                        workrange = pastesheet.get_Range("W" + insertline, "Y" + insertline);    //가공구분
                        workrange.Value2 = str_Work;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.8;

                        workrange = pastesheet.get_Range("Z" + insertline, "AA" + insertline);    //납기일
                        workrange.Value2 = str_DvlyDate;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 3.8;

                        workrange = pastesheet.get_Range("AB" + insertline, "AC" + insertline);    //투입일

                        if (str_DayAndTime.Length > 5)
                        {
                            workrange.Value2 = str_DayAndTime.Substring(0, 5);
                        }
                        else
                        {
                            workrange.Value2 = str_DayAndTime;
                        }

                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 3.8;

                        workrange = pastesheet.get_Range("AD" + insertline, "AF" + insertline);    //수주량
                        workrange.Value2 = str_OrderQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.7;

                        workrange = pastesheet.get_Range("AG" + insertline, "AI" + insertline);    //투입량
                        workrange.Value2 = str_p1WorkQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.2;

                        workrange = pastesheet.get_Range("AJ" + insertline, "AL" + insertline);    //검사량
                        workrange.Value2 = str_InspectQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.2;

                        workrange = pastesheet.get_Range("AM" + insertline, "AO" + insertline);    //합격량
                        workrange.Value2 = str_PassQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.2;

                        workrange = pastesheet.get_Range("AP" + insertline, "AR" + insertline);    //불합격량
                        workrange.Value2 = str_DefectQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.2;

                        workrange = pastesheet.get_Range("AS" + insertline, "AU" + insertline);    //출고량
                        workrange.Value2 = str_OutQty;
                        workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        workrange.Font.Size = 10;
                        workrange.ColumnWidth = 1.2;

                        DataCount = i;
                        j++;

                        // 합계 누적
                        Sum.OrderSum += ConvertInt(str_OrderQty);
                        Sum.InsertSum += ConvertInt(str_p1WorkQty);

                        Sum.InspectSum += ConvertDouble(str_InspectQty);
                        Sum.PassSum += ConvertDouble(str_PassQty);
                        Sum.DefectSum += ConvertDouble(str_DefectQty);
                        Sum.OutSum += ConvertDouble(str_OutQty);


                    }

                    // 합계 출력
                    int totalLine = 34 + ((Page - 1) * 38);

                    Sum.Count = DataCount + 1;


                    workrange = pastesheet.get_Range("AB" + totalLine, "AC" + totalLine);    // 건수
                    workrange.Value2 = Sum.Count + " 건";
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AD" + totalLine, "AF" + totalLine);    // 총 수주량
                    workrange.Value2 = Sum.OrderSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AG" + totalLine, "AI" + totalLine);    // 총 투입량
                    workrange.Value2 = Sum.InsertSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AJ" + totalLine, "AL" + totalLine);    // 총 검일시
                    workrange.Value2 = Sum.InspectSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AM" + totalLine, "AO" + totalLine);    // 총 통과량
                    workrange.Value2 = Sum.PassSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AP" + totalLine, "AR" + totalLine);    // 총 불합격량
                    workrange.Value2 = Sum.DefectSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                    workrange = pastesheet.get_Range("AS" + totalLine, "AU" + totalLine);    // 총 출고량
                    workrange.Value2 = Sum.OutSum;
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    workrange.Font.Size = 10;

                }

                pastesheet.PageSetup.TopMargin = 0;
                pastesheet.PageSetup.BottomMargin = 0;
                //pastesheet.PageSetup.Zoom = 43;

                msg.Hide();

                if (preview_click == true)
                {
                    excelapp.Visible = true;
                    pastesheet.PrintPreview();
                }
                else
                {
                    excelapp.Visible = true;
                    pastesheet.PrintOutEx();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                lib2.ReleaseExcelObject(workbook);
                lib2.ReleaseExcelObject(worksheet);
                lib2.ReleaseExcelObject(pastesheet);
                lib2.ReleaseExcelObject(excelapp);
                lib2 = null;
            }
        }

        private int ConvertInt(string str)
        {
            int result = 0;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (Int32.TryParse(str, out chkInt) == true)
                {
                    result = Int32.Parse(str);
                }
            }

            return result;
        }

        private Double ConvertDouble(string str)
        {
            Double result = 0;
            Double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (Double.TryParse(str, out chkDouble) == true)
                {
                    result = Double.Parse(str);
                }
            }

            return result;
        }


        private void re_Search()
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                FillGrid();
                FillGridSub();
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
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

        // 천자리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }





        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);

            if (scrollViewer != null)
            {
                var mousePosition = e.GetPosition(scrollViewer);
                var horizontalScrollBar = FindVisualChild<ScrollBar>(scrollViewer, orientation: Orientation.Horizontal);
                var verticalScrollBar = FindVisualChild<ScrollBar>(scrollViewer, orientation: Orientation.Vertical);

                if (horizontalScrollBar != null && IsMouseOverScrollBar(mousePosition, horizontalScrollBar, scrollViewer))
                {
                    // 가로 스크롤
                    if (e.Delta < 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 100);
                    }
                    else if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 100);
                    }
                    e.Handled = true;
                }
                else if (verticalScrollBar != null && IsMouseOverScrollBar(mousePosition, verticalScrollBar, scrollViewer))
                {
                    // 세로 스크롤
                    if (e.Delta < 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 3);
                    }
                    else if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 3);
                    }
                    e.Handled = true;
                }
            }
        }

        private bool IsMouseOverScrollBar(Point mousePosition, ScrollBar scrollBar, ScrollViewer scrollViewer)
        {
            var scrollBarBounds = scrollBar.TransformToAncestor(scrollViewer).TransformBounds(new Rect(0, 0, scrollBar.ActualWidth, scrollBar.ActualHeight));
            return scrollBarBounds.Contains(mousePosition);
        }


        private static T FindVisualChild<T>(DependencyObject obj, Orientation orientation = Orientation.Vertical) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t && (orientation == Orientation.Vertical || (child is ScrollBar scrollBar && scrollBar.Orientation == orientation)))
                {
                    return t;
                }
                else
                {
                    var grandChild = FindVisualChild<T>(child, orientation);
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            return null;
        }

        #region 데이터그리드 스크롤 +  헤더 스크롤 연결 
        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            var dataGridScrollViewer = FindChild<ScrollViewer>(dgdMain);
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
            var dataGridScrollViewer = FindChild<ScrollViewer>(dgdMain);

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


        //마감여부 
        private void cboOrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
                MainWindow.pf.ReturnCode(txtManageCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }

        }
        //운영사 플러스파인더
        private void btnManageCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtManageCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
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
                MainWindow.pf.ReturnCode(txtSalesCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }
        //영업사 pf
        private void btnSalesCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSalesCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
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
                MainWindow.pf.ReturnCode(txtArticleIdSrh, 5102, "");
            }
        }
        //제품명 pf
        private void btnCustomer_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleIdSrh, 5102, "");
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

        private void GoOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderID_global == string.Empty)
                return;

            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("수주등록"))
                {
                    MainWindow.OrderID = orderID_global;
                    break;
                }
                i++;
            }
            try
            {
                if (MainWindow.MainMdiContainer.Children.Contains(MainWindow.mMenulist[i].subProgramID as MdiChild))
                {
                    MainWindow.MainMdiContainer.Children.Remove(MainWindow.mMenulist[i].subProgramID as MdiChild);

                    // 다시 창 열기
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
                    MainWindow.MainMdiContainer.Children.Add(MainWindow.mMenulist[i].subProgramID as MdiChild);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("해당 화면이 존재하지 않습니다.");
            }
        }

        //첨부파일문서 조회
        private void AttachFileSearch_Click(object sender, RoutedEventArgs e)
        {
            if (orderID_global != string.Empty)
            {
                OrderClose_filePop = new Win_ord_Pop_OrderClose_File_Q(orderID_global);

                if (OrderClose_filePop.ShowDialog() == true)
                {
                    try
                    {
                        var selectedRow = OrderClose_filePop.SelectedItem;
                        if (selectedRow != null)
                        {

                            //AutoBindDataToControls(selectedRow, grdInput);

                     
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("첨부파일을 받는 중 오류가 발생했습니다.. 오류내용\n" + ex.ToString());
                    }
                }

            }
            else
            {
                MessageBox.Show("먼저 데이터를 선택해 주세요");
            }
        }

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var OrderInfo = dgdMain.SelectedItem as Win_ord_OrderClose_U_CodeView;
            if (OrderInfo != null)
            {
                orderID_global = OrderInfo.orderid;
            }
        }

        //CPO정산 - 라벨 클릭
        private void lblCpoCalcuDateSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkCpoCalcuDateSrh.IsChecked == true)
            {
                chkCpoCalcuDateSrh.IsChecked = false;
                dtpCpoCalcuEdateSrh.IsEnabled = false;
                dtpCpoCalcuSdateSrh.IsEnabled = false;
            }
            else
            {
                chkCpoCalcuDateSrh.IsChecked = true;
                dtpCpoCalcuEdateSrh.IsEnabled = true;
                dtpCpoCalcuSdateSrh.IsEnabled = true;
            }
        }

        //CPO정산 - 체크박스 클릭

        private void chkCpoCalcuDateSrh_Click(object sender, RoutedEventArgs e)
        {
            if(chkCpoCalcuDateSrh.IsChecked == true)
            {
                chkCpoCalcuDateSrh.IsChecked = true;
                dtpCpoCalcuEdateSrh.IsEnabled = true;
                dtpCpoCalcuSdateSrh.IsEnabled = true;
            }
            else
            {
                chkCpoCalcuDateSrh.IsChecked = false;
                dtpCpoCalcuEdateSrh.IsEnabled = false;
                dtpCpoCalcuSdateSrh.IsEnabled = false;
            }
        }

        private void lblConstrCalcuDateSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkConstrCalcuDateSrh.IsChecked == true)
            {
                chkConstrCalcuDateSrh.IsChecked = false;
                dtpConstrCalcuEdateSrh.IsEnabled = false;
                dtpConstrCalcuSdateSrh.IsEnabled = false;
            }
            else
            {
                chkConstrCalcuDateSrh.IsChecked = true;
                dtpConstrCalcuEdateSrh.IsEnabled = true;
                dtpConstrCalcuSdateSrh.IsEnabled = true;
            }
        }

        private void chkConstrCalcuDateSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkConstrCalcuDateSrh.IsChecked == true)
            {
                chkConstrCalcuDateSrh.IsChecked = true;
                dtpConstrCalcuEdateSrh.IsEnabled = true;
                dtpConstrCalcuSdateSrh.IsEnabled = true;
            }
            else
            {
                chkConstrCalcuDateSrh.IsChecked = false;
                dtpConstrCalcuEdateSrh.IsEnabled = false;
                dtpConstrCalcuSdateSrh.IsEnabled = false;
            }
        }
    }

    class Win_ord_OrderClose_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public bool IsCheck { get; set; }
        public string cls { get; set; }

        public int Num { get; set; }
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
        public string cpoCalcuDate {get;set;}
        public string constrCalcuDate { get; set; }
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
        public string kepManageInfraPayDate { get; set; } //시설부담금 운영사 전달일(신규 추가)
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
        public string superBeforeUseInspPrintDate { get; set; } //사용검사필증발급일
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
        //정산경리정보
        public string accntMgrWorkPreTaxPrintDate {get;set;}
        public string accntMgrWorkPreAmount {get;set;}
        public string accntMgrWorkPreAmountComments {get;set;}
        public string accntMgrSalesPreTaxPrintDate {get;set;}
        public string accntMgrSalesPreAmount {get;set;}
        public string accntMgrSalesPreAmountComments {get;set;}
        public string accntWorkPreTaxPrintDate {get;set;}
        public string accntWorkPreAmount {get;set;}
        public string accntWorkPreAmountComments {get;set;}
        public string accntSalesPreTaxPrintDate {get;set;}
        public string accntSalesPreAmount {get;set;}
        public string accntSalesPreAmountComments { get; set; }

        //KPI 단계집계용 컬럼
        public string localGovBehaviorReportDate { get; set; }






    }

    public class dgOrderSum
    {
        public int Count { get; set; }
        public int OrderSum { get; set; }
        public int InsertSum { get; set; }
        public double InspectSum { get; set; }
        public double PassSum { get; set; }
        public double DefectSum { get; set; }
        public double OutSum { get; set; }
        public double OasSum { get; set; }

        public string TextData { get; set; }


        public string acptDate { get; set; }
        public string corpApprovalDate { get; set; }
        public string chargeOrderDate { get; set; }
        public string kepElectrReqDate { get; set; }
        public string constrDate { get; set; }
        public string kepMeterInstallDate { get; set; }
        public string accntMgrWorkTaxPrintDate { get; set; }
        public string Treat { get; set; }
        public string orderSum { get; set; }
        public string dateDiffSum { get; set; }
        public string PersonSum { get; set; }
        public string accntMgrWorkAmount { get; set; }
        public string mtrAmount { get; set; }
        public string zeroDiffSumCount { get; set; }

    }
}

