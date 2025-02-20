using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WizMes_EVC.PopUp;
using WizMes_EVC;
using System.Security.Policy;
using WizMes_EVC.PopUP;


namespace WizMes_EVC
{
    /// <summary>
    /// Win_Prd_ProcessResult_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class sBuySales : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        int rowNum = 0;
  
        public sBuySales()
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
            FillGrid();
        }
      
        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] lst = new string[2];
            lst[0] = "수주별 매입/매출비용 조회";
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

     

        #endregion

        

       
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }
            if (dgdSum.Items.Count > 0)
            {
                dgdSum.Items.Clear();
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
                sqlParameter.Add("searchCustomID", chkSearchCustomSrh.IsChecked == true && !String.IsNullOrEmpty(txtSearchCustomSrh.Text) ? txtSearchCustomSrh.Tag.ToString() : "");
                sqlParameter.Add("chkOrderID", chkOrderIDSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("orderID", chkOrderIDSrh.IsChecked == true && !String.IsNullOrEmpty(txtOrderIDSrh.Text) ? txtOrderIDSrh.Tag.ToString() : "");
                sqlParameter.Add("chkLocation", chkLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("location", txtLocationSrh.Text ?? "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_OrderCost_sOrder", sqlParameter, true, "R");

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
                                acptDate = dr["acptDate"].ToString(),
                                salesCustom = dr["salesCustom"].ToString(),
                                searchCustom = dr["searchCustom"].ToString(),
                                installLocation = dr["installLocation"].ToString(),
                                totalBuyAmount = Convert.ToDouble(dr["totalBuyAmount"]),
                                buyOper = Convert.ToDouble(dr["buyOper"]),
                                buyConst = Convert.ToDouble(dr["buyConst"]),
                                buyEtc = Convert.ToDouble(dr["buyEtc"]),
                                totalSalesAmount = Convert.ToDouble(dr["totalSalesAmount"]),
                                salesOper = Convert.ToDouble(dr["salesOper"]),
                                salesConst = Convert.ToDouble(dr["salesConst"]),
                                salesEtc = Convert.ToDouble(dr["salesEtc"]),

                            };

                            item.acptDate = DatePickerFormat(item.acptDate);
                            item.profit = item.totalSalesAmount - item.totalBuyAmount;
                            item.profitConst = item.salesConst - item.buyConst;
                            item.profitOper = item.salesOper - item.buyOper;
                            item.profitEtc = item.salesEtc - item.buyEtc;

                            sum.totalSalesAmount += item.totalSalesAmount;
                            sum.totalBuyAmount += item.totalBuyAmount;
                            sum.profit += item.profit;

                           dgdMain.Items.Add(item);
                        }
                        sum.num = i; 
                        dgdSum.Items.Add(sum);

                    }
                    else
                    {
                        MessageBox.Show("조회할 데이터가 없습니다");
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

       

        #region 기타 메서드 
        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }
        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            string result = "";

            if (str.Length == 8)
            {
                if (!str.Trim().Equals(""))
                {
                    result = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
                }
            }

            return result;
        }

        #endregion


    }

  


}