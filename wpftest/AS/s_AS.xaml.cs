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
    public partial class s_AS : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;
        string strFlag = string.Empty;

        int rowNum = 0;
  
        public s_AS()
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

            setComboBox();

        }

        private void setComboBox()
        {
            List<string[]> lstYN = new List<string[]>();
            string[] yn1 = { "Y", "Y" };
            string[] yn2 = { "N", "N" };

            lstYN.Add(yn1);
            lstYN.Add(yn2);

            //검색조건 처리여부 
            ObservableCollection<CodeView> ovcYN = ComboBoxUtil.Instance.Direct_SetComboBox(lstYN);
            cboCompleteSrh.ItemsSource = ovcYN;
            cboCompleteSrh.DisplayMemberPath = "code_name";
            cboCompleteSrh.SelectedValuePath = "code_id";
            cboCompleteSrh.SelectedIndex = 0;

            ObservableCollection<CodeView> ovcASType = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ASRSNCode", "Y", "", "");
            cboASTypeSrh.ItemsSource = ovcASType;
            cboASTypeSrh.DisplayMemberPath = "code_name";
            cboASTypeSrh.SelectedValuePath = "code_id";
            cboASTypeSrh.SelectedIndex = 0;
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

        //국소명 
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
            btnPfLocationSrh.IsEnabled = true;
            txtLocationSrh.Focus();
        }

        private void chkLocationSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtLocationSrh.IsEnabled = false;
            btnPfLocationSrh.IsEnabled = false;
        }

        private void txtLocationSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtLocationSrh, (int)Defind_CodeFind.DCF_21, "");
            }
        }

        private void btnPfLocationSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtLocationSrh, (int)Defind_CodeFind.DCF_21, "");
        }

        //요청자명
        private void lblReqNameSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkReqNameSrh.IsChecked == true)
            {
                chkReqNameSrh.IsChecked = false;
            }
            else
            {
                chkReqNameSrh.IsChecked = true;
            }
        }

        private void chkReqNameSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtReqNameSrh.IsEnabled = true;
            txtReqNameSrh.Focus();
        }

        private void chkReqNameSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtReqNameSrh.IsEnabled = false;
        }

        //요청자 전화번호
        private void lblReqTelSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkReqTelSrh.IsChecked == true)
            {
                chkReqTelSrh.IsChecked = false;
            }
            else
            {
                chkReqTelSrh.IsChecked = true;
            }
        }

        private void chkReqTelSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtReqTelSrh.IsEnabled = true;
            txtReqTelSrh.Focus();
        }

        private void chkReqTelSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtReqTelSrh.IsEnabled = false;
        }

        //처리여부 
        private void lblCompleteYNSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCompleteYNSrh.IsChecked == true)
            {
                chkCompleteYNSrh.IsChecked = false;
            }
            else
            {
                chkCompleteYNSrh.IsChecked = true;
            }
        }
        private void chkCompleteYNSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboCompleteSrh.IsEnabled = true;
            cboCompleteSrh.Focus();
        }
        private void chkCompleteYNSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboCompleteSrh.IsEnabled = false;
        }

        //처리여부 
        private void lblASTypeSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkASTypeSrh.IsChecked == true)
            {
                chkASTypeSrh.IsChecked = false;
            }
            else
            {
                chkASTypeSrh.IsChecked = true;
            }
        }

        private void chkASTypeSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboASTypeSrh.IsEnabled = true;
            cboASTypeSrh.Focus();
        }

        private void chkASTypeSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboASTypeSrh.IsEnabled = false;
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
            lst[0] = "A/S 조회";
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
            if (dgdMainSum.Items.Count > 0)
            {
                dgdMainSum.Items.Clear();
            }
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("chkDate", chkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sDate", chkDateSrh.IsChecked == true && dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("eDate", chkDateSrh.IsChecked == true && dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                //sqlParameter.Add("chkLocation", chkLocationSrh.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("locationID", chkLocationSrh.IsChecked == true && txtLocationSrh.Tag.ToString() != null ? txtLocationSrh.Tag.ToString() : "");
                sqlParameter.Add("chkAsSmallInstallLocation", chkLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("asSmallInstallLocation", txtLocationSrh.Text);

                sqlParameter.Add("chkReqName", chkReqNameSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqName", chkReqNameSrh.IsChecked == true && txtReqNameSrh.Text != null ? txtReqNameSrh.Text : "");
                sqlParameter.Add("chkReqTel", chkReqTelSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqTel", chkReqTelSrh.IsChecked == true && txtReqTelSrh.Text != null ? txtReqTelSrh.Text : "");
                sqlParameter.Add("chkCompleteYN", chkCompleteYNSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("completeYN", chkCompleteYNSrh.IsChecked == true && cboCompleteSrh.SelectedValue != null ? cboCompleteSrh.SelectedValue.ToString() : "");
                sqlParameter.Add("chkASType", chkASTypeSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ASTypeID", chkASTypeSrh.IsChecked == true && cboASTypeSrh.SelectedValue != null ? cboASTypeSrh.SelectedValue.ToString() : "");
  

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_AS_sAS", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    AS_ORDER sum = new AS_ORDER();

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var item = new AS_ORDER()
                            {
                                num = i,
                                asOrderID = dr["asOrderID"].ToString(),
                                asReqDate = DatePickerFormat(dr["asReqDate"].ToString()),
                                asReqUserName = dr["asReqUserName"].ToString(),
                                asReqUserTelNo = dr["asReqUserTelNo"].ToString(),
                                asLocation = dr["asLocation"].ToString(),
                                constrCustomID = dr["constrCustomID"].ToString(),
                                constrCustom = dr["constrCustom"].ToString(),
                                asSmallLocation = dr["asSmallLocation"].ToString(),
                                asChargerMCNo = dr["asChargerMCNo"].ToString(),
                                costYN = dr["costYN"].ToString(),
                                asAmount = Convert.ToDouble(dr["asAmount"]),
                                asDamDangJa = dr["asDamDangJa"].ToString(),
                                asDate = DatePickerFormat(dr["asDate"].ToString()),
                                asCompleteYN = dr["asCompleteYN"].ToString(),
                                comments = dr["comments"].ToString(),
                                asType = dr["asType"].ToString(),
                                asTypeID = dr["asTypeID"].ToString(),

                            };
                            sum.asAmount += item.asAmount;
                            dgdMain.Items.Add(item);
                        }

                        sum.num = i;
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

     
        

        #region 기타 메서드 
        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }
        private string DatePickerFormat(string str)
        {
            str = str.Trim().Replace("-", "").Replace(".", "");

            if (!str.Equals(""))
            {
                if (str.Length == 8)
                {
                    str = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
                }
            }

            return str;
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

        #endregion

        public class AS_ORDER
        {
            public int num { get; set; }
            public string asOrderID { get; set; }
            public string asReqDate { get; set; }
            public string asReqUserName { get; set; }
            public string asReqUserTelNo { get; set; }
            public string asLocation { get; set; }
            public string constrCustomID { get; set; }
            public string constrCustom { get; set; }
            public string asSmallLocation { get; set; }
            public string asChargerMCNo { get; set; }
            public string costYN { get; set; }
            public double asAmount { get; set; }
            public string asDamDangJa { get; set; }
            public string asDate { get; set; }
            public string asCompleteYN { get; set; }
            public string comments { get; set; }
            public string asTypeID { get; set; }
            public string asType { get; set; }
        }

     
    }

  

}