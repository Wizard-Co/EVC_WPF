using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_EVC.PopUP;


namespace WizMes_EVC
{
    /// <summary>
    /// Win_Prd_ProcessResult_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class iBuySales : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        int rowNum = 0;
  
        public iBuySales()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            chkDateSrh.IsChecked = true;
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];

           
            defaltMode();

        }

        #region 상단 검색조건 - 날짜  

        // 입고일자 검색
        private void lblDateSrh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkDateSrh.IsChecked == true)
            {
                chkDateSrh.IsChecked = false;
            }
            else
            {
                chkDateSrh.IsChecked = true;
            }
        }
        private void chkDateSrh_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpSDate != null && dtpEDate != null)
            {
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        private void chkDateSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }
        // 전일
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            //dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
            //dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);
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
        // 금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }
        // 전월
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
        // 금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }


        #endregion

        #region 상단 검색조건

        //영업회사
        private void lblSalesCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private void chkSalesCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtSalesCustomSrh.IsEnabled = true;
            btnPfSalesCustomSrh.IsEnabled = true;
            txtSalesCustomSrh.Focus();
        }

        private void chkSalesCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtSalesCustomSrh.IsEnabled = false;
            btnPfSalesCustomSrh.IsEnabled = false;
        }

        private void txtSalesCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtSalesCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        private void btnPfSalesCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSalesCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // 실사업체
        private void lblSearchCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkSearchCustomSrh.IsChecked == true)
            {
                chkSearchCustomSrh.IsChecked = false;
            }
            else
            {
                chkSearchCustomSrh.IsChecked = true;
            }
        }

        private void chkSearchCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtSearchCustomSrh.IsEnabled = true;
            btnPfSearchCustomSrh.IsEnabled = true;
            txtSearchCustomSrh.Focus();
        }

        private void chkSearchCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtSearchCustomSrh.IsEnabled = false;
            btnPfSearchCustomSrh.IsEnabled = false;
        }

        private void txtSearchCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtSearchCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        private void btnPfSearchCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtSearchCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // Order ID  
        private void lblOrderIDSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrderIDSrh.IsChecked == true)
            {
                chkOrderIDSrh.IsChecked = false;
            }
            else
            {
                chkOrderIDSrh.IsChecked = true;
            }
        }

        private void chkOrderIDSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = true;
            btnPfOrderIDSrh.IsEnabled = true;
            txtOrderIDSrh.Focus();
        }

        private void chkOrderIDSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = false;
            btnPfOrderIDSrh.IsEnabled = false;
        }

        private void txtOrderIDSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtOrderIDSrh, 8000, "");
            }
        }

        private void btnPfOrderIDSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtOrderIDSrh, 8000, "");

        }

        // 국소명   
        private void lblLocationSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkLocationSrh.IsChecked == true)
            {
                chkLocationSrh.IsChecked = false;
            }
            else
            {
                chkLocationSrh.IsChecked = true;
            }
        }

        private void chkLocationSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtLocationSrh.IsEnabled = true;
        }

        private void chkLocationSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtLocationSrh.IsEnabled = false;
        }


        #endregion

        #region 버튼 
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            initDgd();
            FillGrid();
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            rowNum = dgdMain.SelectedIndex;
            updateMode();
            this.DataContext = null;
            initDgd();
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            rowNum = dgdMain.SelectedIndex;
            updateMode();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = dgdMain.SelectedItem as BuySales;
            if (item == null)
            {
                MessageBox.Show("삭제할 데이터를 지정해주세요");
                return;
            }

            if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteData(item.orderID))
                {
                    this.DataContext = null;
                    btnSearch_Click(null, null);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            Lib lib = new Lib();

            string[] lst = new string[4];
            lst[0] = "수주별 매입/매출비용 목록";
            lst[1] = "매출금액";
            lst[2] = "매입금액";
            lst[3] = dgdMain.Name;
            lst[4] = dgdSales.Name;
            lst[5] = dgdBuy.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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

                    if (lib.GenerateExcel(dt, dgdMain.Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                        return;
                }
                else if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdSales.Name))
                    {
                        DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");

                        if (ExpExc.Check.Equals("Y"))
                            dt = lib.DataGridToDTinHidden(dgdSales);
                        else
                            dt = lib.DataGirdToDataTable(dgdSales);

                        if (lib.GenerateExcel(dt, dgdSales.Name))
                        {
                            lib.excel.Visible = true;
                            lib.ReleaseExcelObject(lib.excel);
                        }
                        else
                            return;
                    }
                }
                else if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdBuy.Name))
                    {
                        DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");

                        if (ExpExc.Check.Equals("Y"))
                            dt = lib.DataGridToDTinHidden(dgdBuy);
                        else
                            dt = lib.DataGirdToDataTable(dgdBuy);

                        if (lib.GenerateExcel(dt, dgdBuy.Name))
                        {
                            lib.excel.Visible = true;
                            lib.ReleaseExcelObject(lib.excel);
                        }
                        else
                            return;
                    }
                }
                else
                {
                    if (dt != null)
                        dt.Clear();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            defaltMode();
            initInput();
            FillGrid();
            dgdMain.SelectedIndex = rowNum;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (beSave())
            {
                defaltMode();
                FillGrid();
                dgdMain.SelectedIndex = rowNum;
            }
        }

        #endregion

        #region UI MODE
        private void updateMode()
        {
            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            btnExcel.IsEnabled = false;

            lblMsg.Visibility = Visibility.Visible;
            grdAdd.IsHitTestVisible = true;
            dgdMain.IsHitTestVisible = false;

            txtOrderID.IsEnabled = true;
            btnPfOrderID.IsEnabled = true;

        }

        private void defaltMode()
        {
            btnAdd.IsEnabled = true; ;
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnClose.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnSave.Visibility = Visibility.Hidden; 
            btnCancel.Visibility = Visibility.Hidden; 
            btnExcel.IsEnabled = false;

            lblMsg.Visibility = Visibility.Hidden;
            grdAdd.IsHitTestVisible = false;
            dgdMain.IsHitTestVisible = true;

            txtOrderID.IsEnabled = false;
            btnPfOrderID.IsEnabled = false;

        }

        private void initDgd()
        {
            if(dgdBuy.Items.Count > 0)
            {
                dgdBuy.Items.Clear();
            }
            if (dgdSales.Items.Count > 0)
            {
                dgdSales.Items.Clear();
            }
            if (dgdSubSum.Items.Count > 0)
            {
                dgdSubSum.Items.Clear();
            }
        }
        
        private void initInput()
        {
            txtOrderID.Text = "";
            txtOrderID.Tag = "";
            txtSalesCustom.Text = "";
            txtSearchCustom.Text = "";
            txtLocation.Text = "";
            txtSalesSum.Text = "0";
        }
        #endregion

       
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
                this.DataContext = null;
            }
            if (dgdMainSum.Items.Count > 0)
            {
                dgdMainSum.Items.Clear();
            }
            if (dgdSales.Items.Count > 0)
            {
                dgdSales.Items.Clear();
            }
            if (dgdBuy.Items.Count > 0)
            {
                dgdBuy.Items.Clear();
            }
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("chkDate", chkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sDate", chkDateSrh.IsChecked == true && dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("eDate", chkDateSrh.IsChecked == true && dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("chkSalesCustom", chkSalesCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("salesCustomID", chkSalesCustomSrh.IsChecked == true && !String.IsNullOrEmpty(txtSalesCustomSrh.Text) ? txtSalesCustomSrh.Tag.ToString() : "");

                sqlParameter.Add("chkSearchCustom", chkSearchCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("searchCustomID", chkSearchCustomSrh.IsChecked == true && !String.IsNullOrEmpty(txtSearchCustom.Text) ? txtSearchCustomSrh.Tag.ToString() : "");
                sqlParameter.Add("chkOrderID", chkOrderIDSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("orderID", chkOrderIDSrh.IsChecked == true && !String.IsNullOrEmpty(txtOrderIDSrh.Text) ? txtOrderIDSrh.Tag.ToString() : "");
                sqlParameter.Add("chkLocation", chkLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("location", txtLocationSrh.Text ?? "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_OrderCost_sCost", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    BuySales sum = new BuySales();

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var item = new BuySales()
                            {
                                num = i,
                                orderID = dr["orderID"].ToString(),
                                orderNo = dr["orderNo"].ToString(),
                                salesCustom = dr["salesCustom"].ToString(),
                                searchCustom = dr["searchCustom"].ToString(),
                                installLocation = dr["installLocation"].ToString(),
                                totalBuyAmount = Convert.ToDouble(dr["totalBuyAmount"]),
                                totalSalesAmount = Convert.ToDouble(dr["totalSalesAmount"])

                            };
                            sum.totalBuyAmount += item.totalBuyAmount;
                            sum.totalSalesAmount += item.totalSalesAmount;

                            dgdMain.Items.Add(item);
                        }

                        sum.num = i;
                        sum.profit = sum.totalSalesAmount - sum.totalBuyAmount;
                        dgdMainSum.Items.Add(sum);

                    } else
                    {
                        MessageBox.Show("조회된 데이터가 없습니다");
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

        private void GetOrder(String orderID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();


                sqlParameter.Add("orderID", orderID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_OrderCost_sOrder", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            txtOrderID.Text = dr["orderNo"].ToString();
                            txtOrderID.Tag = dr["orderID"].ToString();
                            txtSalesCustom.Text = dr["salesCustom"].ToString();
                            txtSearchCustom.Text = dr["searchCustom"].ToString();
                            txtLocation.Text = dr["installLocation"].ToString();
                            txtSalesSum.Text = dr["totalSalesAmount"].ToString();

                        }

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

        private void FillGridSub(String orderID)
        {
            initDgd();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("orderID", orderID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_OrderCost_sCostSub", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    BuySales sum = new BuySales();

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var sub = new BuySales()
                            {
                                orderID = dr["orderID"].ToString(),
                                costSeq = Convert.ToInt32(dr["costSeq"]),
                                buySalesTypeID = dr["buySalesTypeID"].ToString(),
                                buySalesCodeTypeID = dr["buySalesCodeTypeID"].ToString(),
                                buySalesCode = dr["buySalesCode"].ToString(),
                                buySalesAmount = Convert.ToDouble(dr["buySalesAmount"]),
                                comments = dr["comments"].ToString()
                            };

                            if (sub.buySalesTypeID.Equals("01")){
                                sub.num = dgdBuy.Items.Count + 1;
                                dgdBuy.Items.Add(sub);

                                sum.totalBuyAmount += sub.buySalesAmount;

                            } else
                            {
                                sub.num = dgdSales.Items.Count + 1;
                                dgdSales.Items.Add(sub);

                                sum.totalSalesAmount += sub.buySalesAmount;
                            }
                        }

                        sum.num = i;
                        sum.profit = sum.totalSalesAmount - sum.totalBuyAmount;
                        dgdSubSum.Items.Add(sum);

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
        
        private bool beSave()
        {
            if (dgdSales.Items.Count != 0 || dgdBuy.Items.Count != 0)
            {
                DeleteData(txtOrderID.Tag.ToString());

                for (int i = 0; i < dgdSales.Items.Count; i++)
                {
                    var item = dgdSales.Items[i] as BuySales;

                    if(item.buySalesCodeTypeID == null || String.IsNullOrWhiteSpace(item.buySalesCode))
                    {
                        MessageBox.Show("매출 항목이 입력되지 않았습니다");
                        return false;
                    }
                    SaveData(item);
                }
                for (int i = 0; i < dgdBuy.Items.Count; i++)
                {
                    var item = dgdBuy.Items[i] as BuySales;

                    if(item.buySalesCodeTypeID == null || String.IsNullOrWhiteSpace(item.buySalesCode))
                    {
                        MessageBox.Show("매입 항목이 입력되지 않았습니다");
                        return false;
                    }
                    SaveData(item);
                }
                return true;
            }
            else
            {
                MessageBox.Show("매출/매입 항목이 입력되지 않았습니다");
                return false;
            }
        }
        private bool SaveData(BuySales item)
        {
            //if (item.buySalesCodeTypeID == null) return false;

            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", txtOrderID.Tag.ToString());
                sqlParameter.Add("buySalesTypeID", item.buySalesTypeID ?? "");  
                sqlParameter.Add("buySalesCodeTypeID", item.buySalesCodeTypeID);
                sqlParameter.Add("buySalesAmount", item.buySalesAmount);
                sqlParameter.Add("comments", item.comments);
                sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                Procedure pro = new Procedure();
                pro.Name = "xp_OrderCost_iOrderCost";
                pro.OutputUseYN = "N";
                pro.OutputName = "orderDcntFeesID";
                pro.OutputLength = "10";

                Prolist.Add(pro);
                ListParameter.Add(sqlParameter);

                List<KeyValue> list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

                if (list_Result[0].key.ToLower() == "success")
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
            return false;

        }

        private bool DeleteData(string orderID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderID", orderID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_OrderCost_dOrderCost", sqlParameter, "D");
                DataStore.Instance.CloseConnection();

                if (result[0].Equals("success"))
                {
                    return true;
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
            return false;
        }

        #region 메인 그리드 이벤트 
        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgdMain.SelectedItem as BuySales;
            if (item == null) return; 
            
            btnUpdate.IsEnabled = true;
            this.DataContext = item;
            FillGridSub(item.orderID);
            
        }

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnUpdate_Click(null, null);
        }

        #endregion

        #region 서브그리드 이벤트 

        // 매출항목 cell edit 
        private void Edit_salesCode(object sender)
        {
            if (btnSave.Visibility == Visibility.Visible)
            {
                var item = dgdSales.CurrentItem as BuySales;

                if (item != null)
                {
                    TextBox tb1 = sender as TextBox;

                    MainWindow.pf.ReturnCode(tb1, 8003, "");

                    if (tb1.Tag != null)
                    {
                        item.buySalesCode = tb1.Text;
                        item.buySalesCodeTypeID = tb1.Tag.ToString();
                    }

                    sender = tb1;
                }
            }
        }
        private void salesCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_salesCode(sender);
        }

        private void salesCode_KeyDown(object sender, KeyEventArgs e)
        {
            Edit_salesCode(sender);
        }

        // 매입항목 cell edit
        private void Edit_buyCode(object sender)
        {
            if (btnSave.Visibility == Visibility.Visible)
            {
                var item = dgdBuy.CurrentItem as BuySales;

                if (item != null)
                {
                    TextBox tb1 = sender as TextBox;

                    MainWindow.pf.ReturnCode(tb1, 8004, "");

                    if (tb1.Tag != null)
                    {
                        item.buySalesCode = tb1.Text;
                        item.buySalesCodeTypeID = tb1.Tag.ToString();
                    }

                    sender = tb1;
                }
            }
        }
        private void buyCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_buyCode(sender);
        }

        private void buyCode_KeyDown(object sender, KeyEventArgs e)
        {
            Edit_buyCode(sender);
        }
        private void btnPfOrderID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtOrderID, 8000, "");
            if (!String.IsNullOrEmpty(txtOrderID.Text))
            {
                GetOrder(txtOrderID.Tag.ToString());
                FillGridSub(txtOrderID.Tag.ToString());
            }
        }
        private void txtOrderID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtOrderID, 8000, "");
                if (!String.IsNullOrEmpty(txtOrderID.Text))
                {
                    GetOrder(txtOrderID.Tag.ToString());
                    FillGridSub(txtOrderID.Tag.ToString());
                }
            }
        }
        private int AddRow(DataGrid dataGrid)
        {
            BuySales item = new BuySales
            {
                num = dataGrid.Items.Count + 1,
                buySalesTypeID = "01",
                totalBuyAmount = 0,
                comments = ""
            };

            if (dataGrid.Name.Equals("dgdSales"))
            {
                item.buySalesTypeID = "02";
            } 

            dataGrid.Items.Add(item);
            return dataGrid.Items.Count;
        }
        private void DelRow(DataGrid dataGrid)
        {
            int i = dataGrid.SelectedIndex;
            if (i > -1)
            {
                dataGrid.Items.RemoveAt(i);
            }
            else
            {
                MessageBox.Show("삭제할 행을 선택해주세요");
            }
        }
        private void btnAddRow_Sales(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOrderID.Text))
            {
                AddRow(dgdSales);

            } else
            {
                MessageBox.Show("먼저 수주번호를 검색해주세요");
                return;
            } 
        }
        private void btnAddRow_Buy(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOrderID.Text))
            {
                AddRow(dgdBuy);
            }
            else
            {
                MessageBox.Show("먼저 수주번호를 검색해주세요");
                return;
            }
        }

        private void btnDelRow_Sales(object sender, RoutedEventArgs e)
        {
            DelRow(dgdSales);
        }
        private void btnDelRow_Buy(object sender, RoutedEventArgs e)
        {
            DelRow(dgdBuy);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }
        private void DataGridSub_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        private void DataGridSub_GotFocus(object sender, RoutedEventArgs e)
        {
            DataGrid dgd = Lib.Instance.GetParent<DataGrid>(sender as DataGridCell);

            int currCol = dgd.Columns.IndexOf(dgd.CurrentCell.Column);

            if ((currCol >= 1 && currCol < 4))
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        private void DataGridSub_TextFocus(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINTextBoxFocus(sender, e);
        }

        private void DataGridSub_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid dgd = Lib.Instance.GetParent<DataGrid>(sender as DataGridCell);

                var SubItem = dgd.CurrentItem as BuySales;
                int rowCount = dgd.Items.IndexOf(dgd.CurrentItem);
                int colCount = dgd.Columns.IndexOf(dgd.CurrentCell.Column);
                int StartColumnCount = 1;
                int EndColumnCount = 3;

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount == colCount && dgd.Items.Count - 1 > rowCount)
                    {
                        dgd.SelectedIndex = rowCount + 1;
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount + 1], dgd.Columns[StartColumnCount]);
                    }
                    else if (EndColumnCount > colCount && dgd.Items.Count - 1 > rowCount)
                    {
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount], dgd.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount && dgd.Items.Count - 1 == rowCount)
                    {
                        btnSave.Focus();
                    }
                    else if (EndColumnCount > colCount && dgd.Items.Count - 1 == rowCount)
                    {
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount], dgd.Columns[colCount + 1]);
                    }

                }
                else if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (dgd.Items.Count - 1 > rowCount)
                    {
                        dgd.SelectedIndex = rowCount + 1;
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount + 1], dgd.Columns[colCount]);
                    }
                    else if (dgd.Items.Count - 1 == rowCount)
                    {
                        if (EndColumnCount > colCount)
                        {
                            dgd.SelectedIndex = 0;
                            dgd.CurrentCell = new DataGridCellInfo(dgd.Items[0], dgd.Columns[colCount + 1]);
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
                        dgd.SelectedIndex = rowCount - 1;
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount - 1], dgd.Columns[colCount]);
                    }
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (colCount > 0)
                    {
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount], dgd.Columns[colCount - 1]);
                    }
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount > colCount)
                    {
                        dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount], dgd.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount)
                    {
                        if (dgd.Items.Count - 1 > rowCount)
                        {
                            dgd.SelectedIndex = rowCount + 1;
                            dgd.CurrentCell = new DataGridCellInfo(dgd.Items[rowCount + 1], dgd.Columns[StartColumnCount]);
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
        #endregion

        #region 기타 메서드 
        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        #endregion

       
    }

    class BuySales
    {
        public int num { get; set; }
        public String cls { get; set; }
        public int costSeq { get; set; }
        public String acptDate {  get; set; }
        public String orderID { get; set; }
        public String orderNo { get; set; }
        public string buySalesTypeID { get; set; } // 매입01, 매출02 
        public string buySalesCodeTypeID { get; set; } // 항목 
        public string buySalesCode { get; set; } // 항목 
        public String salesCustom { get; set; }
        public String searchCustom { get; set; }
        public String manageCustom { get; set; }
        public String installLocation { get; set; }
        public String month { get; set; }
        public String orderDate { get; set; }
        public string comments { get; set; }
        public double totalBuyAmount { get; set; }
        public double buyOper { get; set; }
        public double buyConst { get; set; }
        public double buyEtc { get; set; }
        public double totalSalesAmount { get; set; }
        public double salesOper { get; set; }
        public double salesConst { get; set; }
        public double salesEtc { get; set; }
        public double buySalesAmount { get; set; }
        public double profit { get; set; }
        public double profitOper { get; set; }
        public double profitConst { get; set; }
        public double profitEtc { get; set; }

    }


}