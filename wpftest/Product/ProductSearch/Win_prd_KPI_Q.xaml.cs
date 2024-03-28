using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WizMes_HanYoung.PopUP;
using WizMes_HanYoung.PopUp;

namespace WizMes_HanYoung
{
    /// <summary>
    /// Win_prd_KPI_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_KPI_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        int rowNum = 0;

        public Win_prd_KPI_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            lib.UiLoading(sender);
            DatePickerStartDateSearch.SelectedDate = DateTime.Today;
            DatePickerEndDateSearch.SelectedDate = DateTime.Today;
        }

        #region 상단 검색조건
        //전년
        private void ButtonLastYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DatePickerStartDateSearch.SelectedDate = DatePickerStartDateSearch.SelectedDate.Value.AddYears(-1);
                }
                else
                {
                    DatePickerStartDateSearch.SelectedDate = DateTime.Today.AddDays(-1);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //전월
        private void ButtonLastMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DateTime FirstDayOfMonth = DatePickerStartDateSearch.SelectedDate.Value.AddDays(-(DatePickerStartDateSearch.SelectedDate.Value.Day - 1));
                    DateTime FirstDayOfLastMonth = FirstDayOfMonth.AddMonths(-1);

                    DatePickerStartDateSearch.SelectedDate = FirstDayOfLastMonth;
                }
                else
                {
                    DateTime FirstDayOfMonth = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));

                    DatePickerStartDateSearch.SelectedDate = FirstDayOfMonth;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //금년
        private void ButtonThisYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DatePickerStartDateSearch.SelectedDate = lib.BringThisYearDatetimeFormat()[0];
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //금월
        private void ButtonThisMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatePickerStartDateSearch.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        #endregion

        #region Re_Search
        private void re_Search(int selectedIndex)
        {
            try
            {
                if (dgdOut.Items.Count > 0)
                {
                    dgdOut.Items.Clear();
                }

                if (dgdGonsu.Items.Count > 0)
                {
                    dgdGonsu.Items.Clear();
                }

                FillGrid();

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        #endregion

        #region 공수조회
        private void FillGrid()
        {
            try
            {
                string ArticleID = null;

                if (dgdOut.Items.Count > 0)
                {
                    dgdOut.Items.Clear();
                }
                if (dgdGonsu.Items.Count > 0)
                {
                    dgdGonsu.Items.Clear();
                }

                if (chkBuyerArticleNo.IsChecked == true)
                {
                    ArticleID = txtBuyerArticleNoSearch.Tag.ToString();
                }
                else if (CheckBoxArticleSearch.IsChecked == true)
                {
                    ArticleID = TextBoxArticleSearch.Tag.ToString();
                }


                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("FromDate", DatePickerStartDateSearch.SelectedDate == null ? "" : DatePickerStartDateSearch.SelectedDate.Value.ToString().Replace("-", ""));
                sqlParameter.Add("ToDate", DatePickerEndDateSearch.SelectedDate == null ? "" : DatePickerEndDateSearch.SelectedDate.Value.ToString().Replace("-", ""));
                sqlParameter.Add("ArticleNo", ArticleID != null ? ArticleID : ""); //품번
                ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sKPI_KPI", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WPKQC = new Win_prd_KPI_Q_CodeView()
                            {
                                Num = i + 1,

                                GbnName = dr["GbnName"].ToString(),
                                ArticleNo = dr["ARTICLENO"].ToString(),
                                Article = dr["article"].ToString(),
                                WorkQty = stringFormatN0(dr["WorkQty"]),
                                WorkTime = lib.returnNumStringOne(dr["WorkTime"].ToString()),
                                WorkQtyPerHour = stringFormatN1(dr["WorkQtyPerHour"]),
                                OrderQty = stringFormatN0(dr["OrderQty"]),
                                DiffOutDate = stringFormatN0(dr["DiffOutDate"]),
                                DiffOutDayPerQty = stringFormatN1(dr["DiffOutDayPerQty"]),
                                DiffOutUpRate = stringFormatN1(dr["DiffOutUpRate"]),
                                DiffOutGoalRate = stringFormatN1(dr["DiffOutGoalRate"]),
                                gbn = dr["gbn"].ToString(),
                                Sort = dr["Sort"].ToString(),
                                WorkUpRate = dr["WorkUpRate"].ToString(),
                                WorkGoalRate = dr["WorkGoalRate"].ToString()
                            };

                            if (WPKQC.gbn == "D")
                            {
                                WPKQC.Goal = "19.1";
                                dgdOut.Items.Add(WPKQC);
                            }

                            if (WPKQC.gbn == "P")
                            {
                                WPKQC.Goal = "213";
                                dgdGonsu.Items.Add(WPKQC);
                            }

                            i++;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>

            {
                try
                {
                    rowNum = 0;
                    using (Loading lw = new Loading(FillGrid))
                    {
                        lw.ShowDialog();
                        
                        if (dgdGonsu.Items.Count <= 0 || dgdOut.Items.Count <= 0)
                        {
                            MessageBox.Show("조회된 내용이 없습니다.");
                        }
                        btnSearch.IsEnabled = true;
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("예외처리 - " + ee.ToString());
                }

            }), System.Windows.Threading.DispatcherPriority.Background);


        }

        private void btiClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
                lib.ChildMenuClose(this.ToString());
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void btiExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if(dgdOut.Items.Count == 0 && dgdGonsu.Items.Count == 0)
                //{
                //    MessageBox.Show("먼저 검색해 주세요.");
                //    return;
                //}

                DataTable dt = null;
                string Name = string.Empty;

                string[] lst = new string[4];
                lst[0] = "생산성 향상";
                lst[1] = "품질 향상";
                lst[2] = dgdGonsu.Name;
                lst[3] = dgdOut.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.choice.Equals(dgdGonsu.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdGonsu);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdGonsu);

                        Name = dgdGonsu.Name;
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                    }
                    else if (ExpExc.choice.Equals(dgdOut.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdOut);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdOut);

                        Name = dgdOut.Name;
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                    }
                    else
                    {
                        if (dt != null)
                        {
                            dt.Clear();
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void lblBuyerArticleNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkBuyerArticleNo.IsChecked == true)
            {
                chkBuyerArticleNo.IsChecked = false;
            }
            else
            {
                chkBuyerArticleNo.IsChecked = true;
            }
        }
        // 거래처 체크박스 이벤트
        private void chkBuyerArticleNo_Checked(object sender, RoutedEventArgs e)
        {
            chkBuyerArticleNo.IsChecked = true;
            txtBuyerArticleNoSearch.IsEnabled = true;
            btnBuyerArticleNoSearch.IsEnabled = true;
        }
        private void chkBuyerArticleNo_UnChecked(object sender, RoutedEventArgs e)
        {
            chkBuyerArticleNo.IsChecked = false;
            txtBuyerArticleNoSearch.IsEnabled = false;
            btnBuyerArticleNoSearch.IsEnabled = false;
        }
        // 거래처 텍스트박스 엔터 → 플러스파인더
        private void txtBuyerArticleNoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtBuyerArticleNoSearch, 76, "");
            }
        }
        // 거래처 플러스파인더 이벤트
        private void btnBuyerArticleNoSearch_Click(object sender, RoutedEventArgs e)
        {
            // 거래처 : 0
            MainWindow.pf.ReturnCode(txtBuyerArticleNoSearch, 76, "");
        }

        //품명 라벨 클릭
        private void LabelArticleSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CheckBoxArticleSearch.IsChecked == true)
            {
                CheckBoxArticleSearch.IsChecked = false;
            }
            else
            {
                CheckBoxArticleSearch.IsChecked = true;
            }
        }

        private void CheckBoxArticleSearch_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxArticleSearch.IsEnabled = true;
            ButtonArticleSearch.IsEnabled = true;
        }

        private void CheckBoxArticleSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxArticleSearch.IsEnabled = false;
            ButtonArticleSearch.IsEnabled = false;
        }

        private void TextBoxArticleSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        private void ButtonArticleSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }
        // 천마리 콤마, 소수점 한자리
        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N1}", obj);
        }
    }

    #region CodeView
    class Win_prd_KPI_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }

        public string GbnName { get; set; }
        public string ArticleNo { get; internal set; }
        public string Article { get; internal set; }
        public string WorkQty { get; internal set; }
        public string WorkTime { get; internal set; }
        public string WorkQtyPerHour { get; internal set; }
        public string WorkMan { get; set; }
        public string WorkUpRate { get; set; }
        public string WorkGoalRate { get; set; }
        public string Gonsu { get; set; }
        public string OrderQty { get; set; }
        public string DiffOutDate { get; set; }
        public string DiffOutDayPerQty { get; set; }
        public string DiffOutUpRate { get; set; }
        public string DiffOutGoalRate { get; set; }
        public string DefectRate { get; set; }
        public string gbn { get; set; }
        public string Sort { get; set; }
        public string Goal { get; set; }


    }

    #endregion

}