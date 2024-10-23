using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_Nadaum.PopUP;
using WizMes_Nadaum.PopUp;
using WPF.MDI;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WizMes_Nadaum
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
        int rowNum = 0;
        int rbnOrder = 0;

        NoticeMessage msg = new NoticeMessage();
        DataTable DT;
        ////private List<DataGridColumn> _dynamicColumns = new List<DataGridColumn>();

        public Win_ord_OrderClose_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            btnToday_Click(null, null);
            SetComboBox();
            Check_bdrOrder();

            chkProductGrpID.IsChecked = true;
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

            ObservableCollection<CodeView> cbOrderFlag = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDFLG", "Y", "", "");
            cbOrderFlag.RemoveAt(2);
            cbOrderFlag.RemoveAt(2);
            cboOrderFlag.ItemsSource = cbOrderFlag;
            cboOrderFlag.DisplayMemberPath = "code_name";
            cboOrderFlag.SelectedValuePath = "code_id";
            cboOrderFlag.SelectedIndex = 1;

            // 제품군
            ObservableCollection<CodeView> ovcProductGrp = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CMPRDGRPID", "Y", "");
            cboProductGrpID.ItemsSource = ovcProductGrp;
            cboProductGrpID.DisplayMemberPath = "code_name";
            cboProductGrpID.SelectedValuePath = "code_id";
            cboProductGrpID.SelectedIndex = 11;            

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

        //수주상태
        private void cboOrderStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(chkProductGrpID.IsChecked ==true && cboProductGrpID.SelectedValue.ToString() == "99")
            {
                if (cboOrderStatus.SelectedIndex == 0)
                {
                    btnFinal.IsEnabled = false;
                }
                else if (cboOrderStatus.SelectedIndex == 1)
                {
                    btnFinal.IsEnabled = true;
                    btnFinal.Content = "마감처리";
                }
                else
                {
                    btnFinal.IsEnabled = true;
                    btnFinal.Content = "진행처리";
                }
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
                                sqlParameter.Add("OrderID", OrderCloseU.OrderID);
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

        //거래처
        private void lblCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCustom.IsChecked == true) { chkCustom.IsChecked = false; }
            else { chkCustom.IsChecked = true; }
        }

        //거래처
        private void chkCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtCustom.IsEnabled = true;
            btnPfCustom.IsEnabled = true;
            txtCustom.Focus();
        }

        //거래처
        private void chkCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustom.IsEnabled = false;
            btnPfCustom.IsEnabled = false;
        }

        //거래처
        private void txtCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //거래처
        private void btnPfCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // 최종고객사
        private void lblInCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInCustom.IsChecked == true) { chkInCustom.IsChecked = false; }
            else { chkInCustom.IsChecked = true; }
        }

        // 최종고객사
        private void chkInCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtInCustom.IsEnabled = true;
            btnPfInCustom.IsEnabled = true;
            txtInCustom.Focus();
        }

        // 최종고객사
        private void chkInCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            txtInCustom.IsEnabled = false;
            btnPfInCustom.IsEnabled = false;
        }

        // 최종고객사
        private void txtInCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtInCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        // 최종고객사
        private void btnPfInCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // 품번
        private void lblBuyerArticleNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkBuyerArticleNo.IsChecked == true) { chkBuyerArticleNo.IsChecked = false; }
            else { chkBuyerArticleNo.IsChecked = true; }
        }

        // 품번
        private void chkBuyerArticleNo_Checked(object sender, RoutedEventArgs e)
        {
            txtBuyerArticleNo.IsEnabled = true;
            btnPfBuyerArticleNo.IsEnabled = true;
            txtBuyerArticleNo.Focus();
        }

        // 품번
        private void chkBuyerArticleNo_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBuyerArticleNo.IsEnabled = false;
            btnPfBuyerArticleNo.IsEnabled = false;
        }

        // 품번
        private void txtBuyerArticleNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtBuyerArticleNo, 76, txtBuyerArticleNo.Text);
        }

        // 품번
        private void btnPfBuyerArticleNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtBuyerArticleNo, 76, txtBuyerArticleNo.Text);
        }

        //품명
        private void lblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true) { chkArticle.IsChecked = false; }
            else { chkArticle.IsChecked = true; }
        }

        //품명
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = true;
            btnPfArticle.IsEnabled = true;
            txtArticle.Focus();
        }

        //품명
        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = false;
            btnPfArticle.IsEnabled = false;
        }

        //품명
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 77, txtArticle.Text);
            }
        }

        //품명
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 77, txtArticle.Text);
        }

        //OrderNo
        private void lblOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrder.IsChecked == true) { chkOrder.IsChecked = false; }
            else { chkOrder.IsChecked = true; }
        }

        //OrderNo
        private void chkOrder_Checked(object sender, RoutedEventArgs e)
        {
            txtOrderNo.IsEnabled = true;
            btnPfOrderNo.IsEnabled = true;
            txtOrderNo.Focus();
        }

        //OrderNo
        private void chkOrder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrderNo.IsEnabled = false;
            btnPfOrderNo.IsEnabled = false;
        }

        //OrderNo
        private void txtOrderNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.Key == Key.Enter)
                {
                    MainWindow.pf.ReturnCode(txtOrderNo, (int)Defind_CodeFind.DCF_ORDER, "");
                }
            }
        }

        //OrderNo
        private void btnPfOrderNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtOrderNo, (int)Defind_CodeFind.DCF_ORDER, "");
        }

        private void rbnOrderNo_Click(object sender, RoutedEventArgs e)
        {

            Check_bdrOrder();
        }

        private void rbnOrderID_Click(object sender, RoutedEventArgs e)
        {
            Check_bdrOrder();
        }

        private void Check_bdrOrder()
        {
            if (rbnOrderID.IsChecked == true)
            {
                tbkOrder.Text = " 관리번호";
                dgdtxtOrderID.Visibility = Visibility.Visible;
                dgdtxtOrderNo.Visibility = Visibility.Hidden;
            }
            else if (rbnOrderNo.IsChecked == true)
            {
                tbkOrder.Text = "Order No";
                dgdtxtOrderID.Visibility = Visibility.Hidden;
                dgdtxtOrderNo.Visibility = Visibility.Visible;
            }
        }

        private void Check_rbnGrpID()
        {
            if (cboProductGrpID.SelectedValue.ToString() != "99" && chkrbnArticleGrpFilter.IsChecked == true)
            {
                if(rbnOrder == 1)
                {
                    if (chkrbnArticleGrpFilter.IsChecked == true && rbnDayAndTime.IsChecked == true && rbnDayAndTime.IsEnabled == true)
                    {
                        ShowColums_ALL();
                        HideColums_rbnDayAndTimeClicked();
                    }
                  
                }
                else if(rbnOrder == 2)
                {
                    if (chkrbnArticleGrpFilter.IsChecked == true && rbnWorkQty.IsChecked == true && rbnWorkQty.IsEnabled == true)
                    {
                        ShowColums_ALL();
                        HideColums_rbnWorkQtyClicked();
                    }
                 
                }      
            }
            else
            {
                
            }
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

        //투입일 라디오버튼 클릭시 열 숨기기
        private void HideColums_rbnDayAndTimeClicked()
        {
            string[] columnsToHide = {
                "   신선\n투입단위",  "   신선\n투입수량",
                "   압연\n투입단위",  "   압연\n투입수량",
                " 와인딩\n투입단위"," 와인딩\n투입수량",
                "   코팅\n투입단위", "   코팅\n투입수량",
                "스트레인딩\n투입단위", "스트레인딩\n투입수량",
                "   절단\n투입단위", "   절단\n투입수량",
                "   사출\n투입단위", "   사출\n투입수량",
                "HOOD조립\n  투입단위", "HOOD조립\n  투입수량",
                "F/F,TL조립\n투입단위", "F/F,TL조립\n 투입수량",
                };

            foreach (string columnName in columnsToHide)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);
                if (column != null)
                {
                    column.Visibility = Visibility.Hidden;
                }
            }
        }

        //투입수량 라디오 버튼 클릭시 열 숨기기
        private void HideColums_rbnWorkQtyClicked()
        {
            string[] columnsToHide = {
                "   신선\n투입단위",  " 신선\n투입일",
                "   압연\n투입단위",  " 압연\n투입일",
                " 와인딩\n투입단위","와인딩\n투입일",
                "   코팅\n투입단위", " 코팅\n투입일",
                "스트레인딩\n투입단위", "스트레인딩\n   투입일",
                "   절단\n투입단위", " 절단\n투입일",
                "   사출\n투입단위", " 사출\n투입일",
                "HOOD조립\n  투입단위", "HOOD조립\n   투입일",
                "F/F,TL조립\n투입단위", "F/F,TL조립\n  투입일",
                };

            foreach (string columnName in columnsToHide)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);
                if (column != null)
                {
                    column.Visibility = Visibility.Hidden;
                }
            }

        }

        //투입일 열 클릭시 열 보이기
        private void ShowColums_rbnDayAndTimeClicked()
        {
            string[] columnsToShow = {
                "   신선\n투입단위",  "   신선\n투입수량",
                "   압연\n투입단위",  "   압연\n투입수량",
                " 와인딩\n투입단위"," 와인딩\n투입수량",
                "   코팅\n투입단위", "   코팅\n투입수량",
                "스트레인딩\n투입단위", "스트레인딩\n투입수량",
                "   절단\n투입단위", "   절단\n투입수량",
                "   사출\n투입단위", "   사출\n투입수량",
                "HOOD조립\n  투입단위", "HOOD조립\n  투입수량",
                "F/F,TL조립\n투입단위", "F/F,TL조립\n 투입수량",
                };

            foreach (string columnName in columnsToShow)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);
                if (column != null)
                {
                    column.Visibility = Visibility.Visible;
                }
            }
        }

        //라디오버튼 필터링 체크버튼 해제시 모든 열 보이기
        private void ShowColums_ALL()
        {
            string[] columnsToShow = {
                "   신선\n투입단위",  " 신선\n투입일", "   신선\n투입수량",
                "   압연\n투입단위",  " 압연\n투입일",  "   압연\n투입수량",
                " 와인딩\n투입단위","와인딩\n투입일", " 와인딩\n투입수량",
                "   코팅\n투입단위", " 코팅\n투입일", "   코팅\n투입수량",
                "스트레인딩\n투입단위", "스트레인딩\n   투입일", "스트레인딩\n투입수량",
                "   절단\n투입단위", " 절단\n투입일", "   절단\n투입수량",
                "   사출\n투입단위", " 사출\n투입일", "   사출\n투입수량",
                "HOOD조립\n  투입단위", "HOOD조립\n   투입일", "HOOD조립\n  투입수량",
                "F/F,TL조립\n투입단위", "F/F,TL조립\n  투입일", "F/F,TL조립\n 투입수량"
                };

            foreach (string columnName in columnsToShow)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);
                if (column != null)
                {
                    column.Visibility = Visibility.Visible;
                }
            }
        }

        //투입수량 라디오 버튼 클릭시 열 보이기
        private void ShowColums_rbnWorkQtyClicked()
        {
            string[] columnsToHide = {
                "   신선\n투입단위",  " 신선\n투입일",
                "   압연\n투입단위",  " 압연\n투입일",
                " 와인딩\n투입단위","와인딩\n투입일",
                "   코팅\n투입단위", " 코팅\n투입일시",
                "스트레인딩\n투입단위", "스트레인딩\n   투입일",
                "   절단\n투입단위", " 절단\n투입일",
                "   사출\n투입단위", " 사출\n투입일",
                "HOOD조립\n  투입단위", "HOOD조립\n   투입일",
                "F/F,TL조립\n투입단위", "F/F,TL조립\n  투입일",
                };

            foreach (string columnName in columnsToHide)
            {
                var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == columnName);
                if (column != null)
                {
                    column.Visibility = Visibility.Visible;
                }
            }
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

        private void UnfreezeColumns()
        {
            dgdMain.FrozenColumnCount = 0;
        }

        // 수주구분
        private void lblOrderFlag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrderFlag.IsChecked == true) { chkOrderFlag.IsChecked = false; }
            else { chkOrderFlag.IsChecked = true; }
        }

        // 수주구분
        private void ChkOrderFlag_Checked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = true;
        }

        // 수주구분
        private void ChkOrderFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = false;
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

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    //MessageBox.Show("대분류");
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

        //실조회 및 하단 합계
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
                dgdMain.Items.Clear();

            if (dgdSum.Items.Count > 0)
                dgdSum.Items.Clear();


            if (cboProductGrpID.SelectedValue.ToString() != "99")
            {

                Style_ColumnHead_Stretch();
            }
            else
            {
                Style_ColumHead_Shrink();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ChkDate", chkOrderDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkOrderDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkOrderDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                // 거래처
                sqlParameter.Add("ChkCustom", chkCustom.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CustomID", chkCustom.IsChecked == true ? (txtCustom.Tag != null ? txtCustom.Tag.ToString() : txtCustom.Text) : "");
                // 최종고객사
                sqlParameter.Add("ChkInCustom", chkInCustom.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InCustomID", chkInCustom.IsChecked == true ? (txtInCustom.Tag != null ? txtInCustom.Tag.ToString() : "") : "");


                // 품번
                sqlParameter.Add("ChkArticleID", chkBuyerArticleNo.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkBuyerArticleNo.IsChecked == true ? (txtBuyerArticleNo.Tag == null ? "" : txtBuyerArticleNo.Tag.ToString()) : "");
                // 품명
                sqlParameter.Add("ChkArticle", chkArticle.IsChecked == true ? 1 : 0);
                sqlParameter.Add("Article", chkArticle.IsChecked == true ? (txtArticle.Text == string.Empty ? "" : txtArticle.Text) : "");


                // 관리번호
                sqlParameter.Add("ChkOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("OrderID", txtOrderNo.Text == string.Empty ? "" : txtOrderNo.Text);
                // 수주상태
                sqlParameter.Add("ChkClose", int.Parse(cboOrderStatus.SelectedValue != null ? cboOrderStatus.SelectedValue.ToString() : ""));


                // 수주구분
                sqlParameter.Add("ChkOrderFlag", chkOrderFlag.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderFlag", chkOrderFlag.IsChecked == true ? (cboOrderFlag.SelectedValue != null ? cboOrderFlag.SelectedValue.ToString() : "") : "");

                sqlParameter.Add("ChkProductGrpID", chkProductGrpID.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ProductGrpID", chkProductGrpID.IsChecked == true ? cboProductGrpID.SelectedValue.ToString() : "");

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
                        int OrderSum = 0;
                        int InsertSum = 0;
                        double InspectSum = 0;
                        double PassSum = 0;
                        double DefectSum = 0;
                        double OutSum = 0;
                        double OasSum = 0;


                        if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                        {
                         
                            foreach (DataRow item in drc)
                            {
                                var Window_OrderClose_DTO = new Win_ord_OrderClose_U_CodeView()
                                {


                                    IsCheck = false,
                                    ProductGrpID = item["ProductGrpID"] as string,
                                    ProductGrpName = item["ProductGrpName"] as string,
                                    OrderID = item["OrderID"].ToString(),
                                    OrderNo = item["OrderNO"] as string,
                                    CustomID = item["CustomID"] as string,
                                    KCustom = item["KCustom"] as string,
                                    UnitClssName = item["UnitClssName"] as string,

                                    p1ProcessID = item["ProcessID1"].ToString(),
                                    p1StartWorkDate = item["ProcessWorkDate1"].ToString(),
                                    p1StartWorkDTime = item["ProcessWorkStartTime1"].ToString(),
                                    //p1WorkQty = item["ProcessWorkQty1"].ToString(),
                                    p1WorkQty = stringFormatN0(item["ProcessWorkQty1"]),

                                    p2ProcessID = item["ProcessID2"].ToString(),
                                    p2StartWorkDate = item["ProcessWorkDate2"].ToString(),
                                    p2StartWorkDTime = item["ProcessWorkStartTime2"].ToString(),
                                    //p2WorkQty = item["ProcessWorkQty2"].ToString(),
                                    p2WorkQty = stringFormatN0(item["ProcessWorkQty2"].ToString()),

                                    p3ProcessID = item["ProcessID3"].ToString(),
                                    p3StartWorkDate = item["ProcessWorkDate3"].ToString(),
                                    p3StartWorkDTime = item["ProcessWorkStartTime3"].ToString(),
                                    //p3WorkQty = item["ProcessWorkQty3"].ToString(),
                                    p3WorkQty = stringFormatN0(item["ProcessWorkQty3"]),

                                    p4ProcessID = item["ProcessID4"].ToString(),
                                    p4StartWorkDate = item["ProcessWorkDate4"].ToString(),
                                    p4StartWorkDTime = item["ProcessWorkStartTime4"].ToString(),
                                    //p4WorkQty = item["ProcessWorkQty4"].ToString(),
                                    p4WorkQty = stringFormatN0(item["ProcessWorkQty4"]),

                                    p5ProcessID = item["ProcessID5"].ToString(),
                                    p5StartWorkDate = item["ProcessWorkDate5"].ToString(),
                                    p5StartWorkDTime = item["ProcessWorkStartTime5"].ToString(),
                                    //p5WorkQty = item["ProcessWorkQty5"].ToString(),
                                    p5WorkQty = stringFormatN0(item["ProcessWorkQty5"]),

                                    p6ProcessID = item["ProcessID6"].ToString(),
                                    p6StartWorkDate = item["ProcessWorkDate6"].ToString(),
                                    p6StartWorkDTime = item["ProcessWorkStartTime6"].ToString(),
                                    //p6WorkQty = item["ProcessWorkQty6"].ToString(),
                                    p6WorkQty = stringFormatN0(item["ProcessWorkQty6"]),

                                    p7ProcessID = item["ProcessID7"].ToString(),
                                    p7StartWorkDate = item["ProcessWorkDate7"].ToString(),
                                    p7StartWorkDTime = item["ProcessWorkStartTime7"].ToString(),
                                    //p7WorkQty = item["ProcessWorkQty7"].ToString(),
                                    p7WorkQty = stringFormatN0(item["ProcessWorkQty7"]),

                                    p8ProcessID = item["ProcessID8"].ToString(),
                                    p8StartWorkDate = item["ProcessWorkDate8"].ToString(),
                                    p8StartWorkDTime = item["ProcessWorkStartTime8"].ToString(),
                                    //p8WorkQty = item["ProcessWorkQty8"].ToString(),
                                    p8WorkQty = stringFormatN0(item["ProcessWorkQty8"]),

                                    p9ProcessID = item["ProcessID9"].ToString(),
                                    p9StartWorkDate = item["ProcessWorkDate9"].ToString(),
                                    p9StartWorkDTime = item["ProcessWorkStartTime9"].ToString(),
                                    //p9WorkQty = item["ProcessWorkQty9"].ToString(),
                                    p9WorkQty = stringFormatN0(item["ProcessWorkQty9"]),

                                    p10ProcessID = item["ProcessID10"].ToString(),
                                    p10StartWorkDate = item["ProcessWorkDate10"].ToString(),
                                    p10StartWorkDTime = item["ProcessWorkStartTime10"].ToString(),
                                    //p10WorkQty = item["ProcessWorkQty10"].ToString(),
                                    p10WorkQty = stringFormatN0(item["ProcessWorkQty10"]),
                                    Num = i + 1,

                                  

                                };

                                if (Window_OrderClose_DTO.OrderID_CV == null)
                                {
                                    Window_OrderClose_DTO.OrderID_CV = Window_OrderClose_DTO.OrderID.Substring(0, 4) + "-" +
                                    Window_OrderClose_DTO.OrderID.Substring(4, 2) + "-" + Window_OrderClose_DTO.OrderID.Substring(6, 4);
                                }

                                i++;

                                //Window_OrderClose_DTO.p1DayAndTime = item["p1ProcessWorkDate"].ToString().Substring(4, 2) + "-" + item["p1ProcessWorkDate"].ToString().Substring(6) + " "
                                //   + item["p1ProcessStartTime"].ToString().Substring(0, 2) + ":" + item["p1ProcessStartTime"].ToString().Substring(2);

                                for (int j= 1; j < 11; j++)
                                {
                                    string dayAndTimeProperty = $"p{j}DayAndTime";
                                    string processWorkDateProperty = $"ProcessWorkDate{j}";
                                    string processWorkStartTimeProperty = $"ProcessWorkStartTime{j}";

                                    string processWorkDate = item[processWorkDateProperty].ToString();
                                    string processWorkStartTime = item[processWorkStartTimeProperty].ToString();

                                    if ((processWorkDate != null && processWorkStartTime != null) && (processWorkDate != "" && processWorkStartTime != ""))
                                    {
                                        string dayAndTime = processWorkDate.Substring(4, 2) + "-" + processWorkDate.Substring(6) + " "
                                                        + processWorkStartTime.Substring(0, 2) + ":" + processWorkStartTime.Substring(2);

                                        Window_OrderClose_DTO.GetType().GetProperty(dayAndTimeProperty).SetValue(Window_OrderClose_DTO, dayAndTime);
                                    }
                                }                               

                          
                                dgdMain.Items.Add(Window_OrderClose_DTO);                                
                            }

                            FreezeColumns();
                        }


                        if (chkProductGrpID.IsChecked ==true && cboProductGrpID.SelectedValue.ToString() == "99")
                        {
                            foreach (DataRow item in drc)
                            {

                                var Window_OrderClose_DTO = new Win_ord_OrderClose_U_CodeView()
                                {

                                    IsCheck = false,
                                    OrderID = item["OrderID"].ToString(),
                                    OrderNo = item["OrderNO"] as string,
                                    CustomID = item["CustomID"] as string,
                                    KCustom = item["KCustom"] as string,

                                    DvlyDate = item["DvlyDate"] as string,
                                    CloseClss = item["CloseClss"] as string,
                                    //ChunkRate = item["ChunkRate"].ToString(),
                                    //LossRate = item["LossRate"] as string,
                                    Article = item["Article"] as string,

                                    WorkName = item["WorkName"].ToString(),
                                    //WorkWidth = item["WorkWidth"] as string,
                                    OrderQty = item["OrderQty"].ToString(),
                                    ColorQty = stringFormatN0(item["ColorQty"]),

                                    UnitClss = item["UnitClss"] as string,  //주문기준 value
                                    InspectQty = item["InspectQty"].ToString(),

                                    PassQty = item["PassQty"].ToString(),
                                    DefectQty = item["DefectQty"].ToString(),
                                    OutQty = item["OutQty"].ToString(),
                                    BuyerModel = item["BuyerModel"] as string,
                                    BuyerModelID = item["BuyerModelID"] as string,

                                    BuyerArticleNo = item["BuyerArticleNo"] as string,
                                    UnitClssName = item["UnitClssName"] as string,

                                    p1StartWorkDate = item["p1StartWorkDate"] as string,
                                    p1StartWorkDTime = item["p1StartWorkDTime"] as string,
                                    p1WorkQty = item["p1WorkQty"].ToString(),
                                    p1ProcessID = item["p1ProcessID"] as string,
                                    p1ProcessName = item["p1ProcessName"] as string,

                                    ProductGrpID = item["ProductGrpID"] as string,
                                    ProductGrpName = item["ProductGrpName"] as string,
                                    //ArticleID = item["ArticleID"] as string,
                                    //AcptDate = item["AcptDate"] as string,
                                    Num = i + 1,

                                };

                                #region 동적추가 프로퍼티 변수들
                                ////if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                                ////{
                                ////    Window_OrderClose_DTO.p2StartWorkDate = item["p2StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p2StartWorkDTime = item["p2StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p2WorkQty = item["p2WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p2ProcessID = item["p2ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p2ProcessName = item["p2ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p3StartWorkDate = item["p3StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p3StartWorkDTime = item["p3StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p3WorkQty = item["p3WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p3ProcessID = item["p3ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p3ProcessName = item["p3ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p4StartWorkDate = item["p4StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p4StartWorkDTime = item["p4StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p4WorkQty = item["p4WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p4ProcessID = item["p4ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p4ProcessName = item["p4ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p5StartWorkDate = item["p5StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p5StartWorkDTime = item["p5StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p5WorkQty = item["p5WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p5ProcessID = item["p5ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p5ProcessName = item["p5ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p6StartWorkDate = item["p6StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p6StartWorkDTime = item["p6StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p6WorkQty = item["p6WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p6ProcessID = item["p6ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p6ProcessName = item["p6ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p7StartWorkDate = item["p7StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p7StartWorkDTime = item["p7StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p7WorkQty = item["p7WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p7ProcessID = item["p7ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p7ProcessName = item["p7ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p8StartWorkDate = item["p8StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p8StartWorkDTime = item["p8StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p8WorkQty = item["p8WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p8ProcessID = item["p8ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p8ProcessName = item["p8ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p9StartWorkDate = item["p9StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p9StartWorkDTime = item["p9StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p9WorkQty = item["p9WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p9ProcessID = item["p9ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p9ProcessName = item["p9ProcessName"] as string;

                                ////    Window_OrderClose_DTO.p10StartWorkDate = item["p10StartWorkDate"] as string;
                                ////    Window_OrderClose_DTO.p10StartWorkDTime = item["p10StartWorkDTime"] as string;
                                ////    Window_OrderClose_DTO.p10WorkQty = item["p10WorkQty"].ToString();
                                ////    Window_OrderClose_DTO.p10ProcessID = item["p10ProcessID"] as string;
                                ////    Window_OrderClose_DTO.p10ProcessName = item["p10ProcessName"] as string;


                                ////}
                                #endregion
                                #region 동적추가해보려다가 이건 아닌거 같아서 주석 
                                //mt_terminal로 걸러진거 공정명과 공정아이디를 가진 null값이아닌 컬럼만 비하인드코드에서 추가

                                ////if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                                ////{
                                ////    for (int j = 1; j <= 10; j++)
                                ////    {
                                ////        string processIDProperty = $"p{j}ProcessID";
                                ////        string processNameProperty = $"p{j}ProcessName";
                                ////        string processStartWorkTimeProperty = $"p{j}StartWorkDTime";
                                ////        string processWorkQtyProperty = $"p{j}WorkQty";

                                ////        if (HasNonNullValue(drc, processIDProperty) && HasNonNullValue(drc, processNameProperty))
                                ////        {
                                ////            // 기존 컬럼이 있는지 확인하고, 없는 경우에만 새로 추가
                                ////            //DataGridColumn processIDColumn = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == processIDProperty);
                                ////            //if (processIDColumn == null)
                                ////            //{
                                ////            //    processIDColumn = new DataGridTextColumn { Header = processIDProperty, Binding = new Binding(processIDProperty) };
                                ////            //    dgdMain.Columns.Add(processIDColumn);
                                ////            //    _dynamicColumns.Add(processIDColumn);
                                ////            //}

                                ////            DataGridColumn processNameColumn = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == $"공정명{j}번");
                                ////            if (processNameColumn == null)
                                ////            {
                                ////                processNameColumn = new DataGridTextColumn { Header = $"공정명{j}번", Binding = new Binding(processNameProperty) };
                                ////                //processNameColumn = new DataGridTextColumn { Header = processNameProperty, Binding = new Binding(processNameProperty) };
                                ////                dgdMain.Columns.Add(processNameColumn);
                                ////                _dynamicColumns.Add(processNameColumn);
                                ////            }

                                ////            DataGridColumn processStartWorkTimeColumn = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == $"투입일시{j}번");
                                ////            if (processStartWorkTimeColumn == null)
                                ////            {
                                ////                processStartWorkTimeColumn = new DataGridTextColumn { Header = $"투입일시{j}번", Binding = new Binding(processStartWorkTimeProperty) };
                                ////                //processStartWorkTimeColumn = new DataGridTextColumn { Header = processStartWorkTimeProperty, Binding = new Binding(processStartWorkTimeProperty) };
                                ////                dgdMain.Columns.Add(processStartWorkTimeColumn);
                                ////                _dynamicColumns.Add(processStartWorkTimeColumn);
                                ////            }

                                ////            DataGridColumn processWorkQtyColumn = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == $"투입{j}번");
                                ////            if (processWorkQtyColumn == null)
                                ////            {
                                ////                processWorkQtyColumn = new DataGridTextColumn { Header = $"투입{j}번", Binding = new Binding(processWorkQtyProperty) };
                                ////                //processWorkQtyColumn = new DataGridTextColumn { Header = processWorkQtyProperty, Binding = new Binding(processWorkQtyProperty) };
                                ////                dgdMain.Columns.Add(processWorkQtyColumn);
                                ////                _dynamicColumns.Add(processWorkQtyColumn);
                                ////            }
                                ////        }
                                ////    }
                                ////}

                                ////if (chkProductGrpID.IsChecked == false || cboProductGrpID.SelectedValue.ToString() == "99")
                                ////{
                                ////    RemoveDynamicColumns();
                                ////}
                                #endregion

                                if (Window_OrderClose_DTO.OrderID_CV == null)
                                {
                                    Window_OrderClose_DTO.OrderID_CV = Window_OrderClose_DTO.OrderID.Substring(0, 4) + "-" +
                                    Window_OrderClose_DTO.OrderID.Substring(4, 2) + "-" + Window_OrderClose_DTO.OrderID.Substring(6, 4);
                                }

                                Window_OrderClose_DTO.OverAndShort = double.Parse(Window_OrderClose_DTO.OrderQty) - double.Parse(Window_OrderClose_DTO.PassQty);

                                i++;

                                if (Window_OrderClose_DTO.OrderQty == null || Window_OrderClose_DTO.OrderQty.Equals("") || Window_OrderClose_DTO.OrderQty.Substring(0, 1).Equals("0"))
                                {
                                    OrderSum += 0;
                                }
                                else
                                {
                                    OrderSum += int.Parse(Window_OrderClose_DTO.OrderQty);
                                }


                                if (Window_OrderClose_DTO.p1WorkQty == null || Window_OrderClose_DTO.p1WorkQty.Equals("") || Window_OrderClose_DTO.p1WorkQty.Substring(0, 1).Equals("0"))
                                {
                                    InsertSum += 0;
                                }
                                else
                                {
                                    InsertSum += (int)(double.Parse(Window_OrderClose_DTO.p1WorkQty));
                                }

                                if (Window_OrderClose_DTO.p1WorkQty != null && Lib.Instance.IsNumOrAnother(Window_OrderClose_DTO.p1WorkQty))
                                {
                                    if (Window_OrderClose_DTO.p1WorkQty.Contains("."))
                                    {
                                        Window_OrderClose_DTO.p1WorkQty = Window_OrderClose_DTO.p1WorkQty.Substring(0, Window_OrderClose_DTO.p1WorkQty.IndexOf("."));
                                    }
                                }

                                if (Window_OrderClose_DTO.InspectQty == null || Window_OrderClose_DTO.InspectQty.Equals("") || Window_OrderClose_DTO.InspectQty.Substring(0, 1).Equals("0"))
                                {
                                    InspectSum += 0;
                                }
                                else
                                {
                                    InspectSum += double.Parse(Window_OrderClose_DTO.InspectQty);
                                }

                                if (Window_OrderClose_DTO.InspectQty != null && Lib.Instance.IsNumOrAnother(Window_OrderClose_DTO.InspectQty))
                                {
                                    if (Window_OrderClose_DTO.InspectQty.Contains("."))
                                    {
                                        Window_OrderClose_DTO.InspectQty = Window_OrderClose_DTO.InspectQty.Substring(0, Window_OrderClose_DTO.InspectQty.IndexOf("."));
                                    }
                                }

                                if (Window_OrderClose_DTO.PassQty == null || Window_OrderClose_DTO.PassQty.Equals("") || Window_OrderClose_DTO.PassQty.Substring(0, 1).Equals("0"))
                                {
                                    PassSum += 0;
                                }
                                else
                                {
                                    PassSum += double.Parse(Window_OrderClose_DTO.PassQty);
                                }

                                if (Window_OrderClose_DTO.PassQty != null && Lib.Instance.IsNumOrAnother(Window_OrderClose_DTO.PassQty))
                                {
                                    if (Window_OrderClose_DTO.PassQty.Contains("."))
                                    {
                                        Window_OrderClose_DTO.PassQty = Window_OrderClose_DTO.PassQty.Substring(0, Window_OrderClose_DTO.PassQty.IndexOf("."));
                                    }
                                }

                                if (Window_OrderClose_DTO.DefectQty == null || Window_OrderClose_DTO.DefectQty.Equals("") || Window_OrderClose_DTO.DefectQty.Substring(0, 1).Equals("0"))
                                {
                                    DefectSum += 0;
                                }
                                else
                                {
                                    DefectSum += double.Parse(Window_OrderClose_DTO.DefectQty);
                                }

                                if (Window_OrderClose_DTO.DefectQty != null && Lib.Instance.IsNumOrAnother(Window_OrderClose_DTO.DefectQty))
                                {
                                    if (Window_OrderClose_DTO.DefectQty.Contains("."))
                                    {
                                        Window_OrderClose_DTO.DefectQty = Window_OrderClose_DTO.DefectQty.Substring(0, Window_OrderClose_DTO.DefectQty.IndexOf("."));
                                    }
                                }

                                if (Window_OrderClose_DTO.OutQty == null || Window_OrderClose_DTO.OutQty.Equals("") || Window_OrderClose_DTO.OutQty.Substring(0, 1).Equals("0"))
                                {
                                    OutSum += 0;
                                }
                                else
                                {
                                    OutSum += double.Parse(Window_OrderClose_DTO.OutQty);
                                }

                                if (Window_OrderClose_DTO.OutQty != null && Lib.Instance.IsNumOrAnother(Window_OrderClose_DTO.OutQty))
                                {
                                    if (Window_OrderClose_DTO.OutQty.Contains("."))
                                    {
                                        Window_OrderClose_DTO.OutQty = Window_OrderClose_DTO.OutQty.Substring(0, Window_OrderClose_DTO.OutQty.IndexOf("."));
                                    }
                                }

                                OasSum += Window_OrderClose_DTO.OverAndShort;

                                //중간 납기일에 들어가는 '-' 를 위해 체크한 후 잘라주거나 그냥 넣어준다.
                                if (Window_OrderClose_DTO.DvlyDate != null && Window_OrderClose_DTO.DvlyDate.ToString().Trim() != "")
                                {
                                    Window_OrderClose_DTO.DvlyDateEdit = item["DvlyDate"].ToString().Substring(0, 4) + "-" + item["DvlyDate"].ToString().Substring(4, 2) + "-" + item["DvlyDate"].ToString().Substring(6, 2);
                                }
                                else
                                {
                                    Window_OrderClose_DTO.DvlyDateEdit = " ";
                                }

                                //중간에 투입일시의 정규식을 넣기가 힘들어 노가다...
                                if (Window_OrderClose_DTO.p1StartWorkDate != null && !Window_OrderClose_DTO.p1StartWorkDate.Equals("") && Window_OrderClose_DTO.p1StartWorkDTime != null && !Window_OrderClose_DTO.p1StartWorkDTime.Equals(""))
                                {
                                    Window_OrderClose_DTO.DayAndTime = item["p1StartWorkDate"].ToString().Substring(4, 2) + "-" + item["p1StartWorkDate"].ToString().Substring(6) + " "
                                    + item["p1StartWorkDTime"].ToString().Substring(0, 2) + ":" + item["p1StartWorkDTime"].ToString().Substring(2);
                                }

                                Window_OrderClose_DTO.DefectQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.DefectQty);
                                Window_OrderClose_DTO.OrderQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.OrderQty);
                                Window_OrderClose_DTO.InspectQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.InspectQty);
                                Window_OrderClose_DTO.OutQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.OutQty);
                                Window_OrderClose_DTO.p1WorkQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.p1WorkQty);
                                Window_OrderClose_DTO.PassQty = Lib.Instance.returnNumStringZero(Window_OrderClose_DTO.PassQty);
                                dgdMain.Items.Add(Window_OrderClose_DTO);
                                rowHeaderNum = i.ToString();
                            }

                            UnfreezeColumns();

                        }


                        //UNION ALL로 총합 보여주는 화면처럼 가로로 보여줄려고 했던 것
                        
                        //if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                        //{
                        //    foreach (DataRow item in drc)
                        //    {
                        //        var Window_OrderClose_DTO = new Win_ord_OrderClose_U_CodeView()
                        //        {
                        //            IsCheck = false,
                        //            cls = item["cls"].ToString(),
                        //            ProductGrpName = item["ProductGrpName"].ToString(),
                        //            p1ProcessName = item["신선"].ToString(),
                        //            p2ProcessName = item["압연"].ToString(),
                        //            p3ProcessName = item["와인딩"].ToString(),
                        //            p4ProcessName = item["코팅"].ToString(),
                        //            p5ProcessName = item["스트레인딩"].ToString(),
                        //            p6ProcessName = item["절단"].ToString(),
                        //            p7ProcessName = item["사출"].ToString(),
                        //            p8ProcessName = item["HOOD조립"].ToString(),
                        //            p9ProcessName = item["F/F&TL조립"].ToString(),
                        //            p10ProcessName = item["검사"].ToString(),
                        //            Num = i + 1,
                        //        };

                        //        if (item["cls"].ToString() == "1" || item["cls"].ToString() == "2")
                        //        {
                        //            Window_OrderClose_DTO.RowColor = true;
                        //        }
                        //        else
                        //        {
                        //            Window_OrderClose_DTO.RowColor = false;
                        //        }

                        //        dgdMain.Items.Add(Window_OrderClose_DTO);
                        //    }
                        //}



                        if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                        {

                            HideColumns();

                            //dgdSum.Items.Add(ThisOrderSum);

                        }


                        if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() == "99")
                        {
                            var ThisOrderSum = new dgOrderSum
                            {
                                Count = i,
                                OrderSum = OrderSum,
                                InsertSum = InsertSum,
                                InspectSum = InspectSum,
                                PassSum = PassSum,
                                DefectSum = DefectSum,
                                OutSum = OutSum,
                                OasSum = OasSum,
                                TextData = "합계"
                            }; 

                            if (chkProductGrpID.IsChecked == true && cboProductGrpID.SelectedValue.ToString() != "99")
                            {

                                HideColumns();

                                //dgdSum.Items.Add(ThisOrderSum);

                            }
                            else
                            {
                                ShowColumns();

                                dgdSum.Items.Add(ThisOrderSum);

                            }
                        }    
                    }

                }
                if(ds.Tables.Count == 0)
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

        private void HideColumns()
        {
            //dgdtpechkChoice.Visibility = Visibility.Hidden;

            //var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "관리번호");
            //if (column != null) column.Visibility = Visibility.Hidden;

            //column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "OrderNo");
            //if (column != null) column.Visibility = Visibility.Hidden;

            

            var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "거래처");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "수주수량");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입수량");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "품번");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "품명");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "차종");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "단위");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "납기일자");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "가공구분");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입일시");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입일시");
            if (column != null) column.Visibility = Visibility.Hidden;

            dgdtxtInspect.Visibility = Visibility.Hidden;
            //column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "검사");
            //if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "합격");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "불량");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "제품출고");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "과부족");
            if (column != null) column.Visibility = Visibility.Hidden;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "수주일시");
            if (column != null) column.Visibility = Visibility.Hidden;


            //고정 공정컬럼
            for (int i = 0; i < 10; i++)
            {
                //string columnName = "dgdtxtProcess"  + i;
                string columnName1 = "dgdtxtProcess" + i + "WorkDateTime";
                string columnName2 = "dgdtxtProcess" + i + "UnitClss";
                string columnName3 = "dgdtxtProcess" + i + "WorkQty";

                //var columninner = this.FindName(columnName) as DataGridTextColumn;
                var columninner1 = this.FindName(columnName1) as DataGridTextColumn;
                var columninner2 = this.FindName(columnName2) as DataGridTextColumn;
                var columninner3 = this.FindName(columnName3) as DataGridTextColumn;

                //if (columninner != null)
                //   columninner.Visibility = Visibility.Visible;
                if (columninner1 != null)
                    columninner1.Visibility = Visibility.Visible;
                if (columninner2 != null)
                    columninner2.Visibility = Visibility.Visible;
                if (columninner3 != null)
                    columninner3.Visibility = Visibility.Visible;
            } 
         
        }

        private void ShowColumns()
        {
            //dgdtpechkChoice.Visibility = Visibility.Visible;

            //var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "관리번호");
            //if (column != null) column.Visibility = Visibility.Visible;

            //column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "OrderNo");
            //if (column != null) column.Visibility = Visibility.Visible;

            var column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "거래처");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "품번");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "수주수량");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입수량");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "품명");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "차종");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "단위");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "납기일자");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "가공구분");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입일시");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "투입일시");
            if (column != null) column.Visibility = Visibility.Visible;

            dgdtxtInspect.Visibility = Visibility.Visible;
            //column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "검사");
            //if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "합격");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "불량");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "제품출고");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "과부족");
            if (column != null) column.Visibility = Visibility.Visible;

            column = dgdMain.Columns.FirstOrDefault(c => c.Header.ToString() == "수주일시");
            if (column != null) column.Visibility = Visibility.Visible;

            //고정 공정컬럼     

            for (int i = 0; i < 10; i++)
            {
                //string columnName = "dgdtxtProcess" + i;
                string columnName1 = "dgdtxtProcess" + i + "WorkDateTime";
                string columnName2 = "dgdtxtProcess" + i + "UnitClss";
                string columnName3 = "dgdtxtProcess" + i + "WorkQty";

                //var columninner = this.FindName(columnName) as DataGridTextColumn;
                var columninner1 = this.FindName(columnName1) as DataGridTextColumn;
                var columninner2 = this.FindName(columnName2) as DataGridTextColumn;
                var columninner3 = this.FindName(columnName3) as DataGridTextColumn;

                //if (columninner != null)
                //    columninner.Visibility = Visibility.Hidden;
                if (columninner1 != null)
                    columninner1.Visibility = Visibility.Hidden;
                if (columninner2 != null)
                    columninner2.Visibility = Visibility.Hidden;
                if (columninner3 != null)
                    columninner3.Visibility = Visibility.Hidden;
            }
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

        #region 동적추가한 것 제거하기
        ////private void RemoveDynamicColumns()
        ////{
        ////    foreach (var column in _dynamicColumns)
        ////    {
        ////        dgdMain.Columns.Remove(column);
        ////    }
        ////    _dynamicColumns.Clear();
        ////}
        #endregion

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


                //오더번호 혹은 관리번호 
                if (rbnOrderNo.IsChecked == true)
                {
                    workrange = worksheet.get_Range("C5", "F5");//셀 범위 지정
                    workrange.Value2 = "오더번호";
                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //workrange.Font.Size = 10;
                }
                else
                {
                    workrange = worksheet.get_Range("C5", "F5");//셀 범위 지정
                    workrange.Value2 = "관리번호";
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

                        if (dgdtxtOrderID.ToString().Equals("오더번호"))
                        {
                            workrange = pastesheet.get_Range("C" + insertline, "F" + insertline);    //오더번호
                            workrange.Value2 = str_OrderID;
                            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            workrange.Font.Size = 9;
                            workrange.ColumnWidth = 1.8;
                        }
                        else
                        {
                            workrange = pastesheet.get_Range("C" + insertline, "F" + insertline);    //관리번호
                            workrange.Value2 = str_OrderID_CV;
                            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                            workrange.Font.Size = 9;
                            workrange.ColumnWidth = 1.8;
                        }

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
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        //데이터 그리드 더블 클릭하면 월 납품계획 등록 화면 호출
        private void DgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //// 넘겨줄 데이터를 넣어주시죠
            //var Order = dgdMain.SelectedItem as Win_ord_OrderClose_U_CodeView;

            //if (Order != null)
            //{
            //    string OrderID = Order.OrderID;
            //    string sDate = dtpSDate.SelectedDate.Value.ToString("yyyyMMdd");
            //    string eDate = dtpEDate.SelectedDate.Value.ToString("yyyyMMdd");
            //    string chkYN = chkOrderDay.IsChecked == true ? "Y" : "N";

            //    MainWindow.tempContent.Clear();
            //    MainWindow.tempContent.Add(OrderID);
            //    MainWindow.tempContent.Add(sDate);
            //    MainWindow.tempContent.Add(eDate);
            //    MainWindow.tempContent.Add(chkYN);

            //    int i = 0;
            //    foreach (MenuViewModel mvm in MainWindow.mMenulist)
            //    {
            //        if (mvm.Menu.Equals("월수주/생산계획 등록"))
            //        //if (mvm.Menu.Equals("월 납품계획 등록"))
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
            //                Title = "Nadaum [" + MainWindow.mMenulist[i].MenuID.Trim() + "] " + MainWindow.mMenulist[i].Menu.Trim() +
            //                        " (→" + MainWindow.mMenulist[i].ProgramID + ")",
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

        private void lblProductGrpID_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if(chkProductGrpID.IsChecked == true)
            //{
            //    chkProductGrpID.IsChecked = false;
            //    cboProductGrpID.IsEnabled = false;
            //}
            //else
            //{
            //    chkProductGrpID.IsChecked = true;
            //    cboProductGrpID.IsEnabled = true;
            //}
        }

        private void ChkProductGrpID_Checked(object sender, RoutedEventArgs e)
        {
            if(chkProductGrpID.IsChecked == true)
            {
                chkProductGrpID.IsChecked = true;
                cboProductGrpID.IsEnabled = true;
            }
        }

        private void ChkProductGrpID_Unchecked(object sender, RoutedEventArgs e)
        {
            if(chkProductGrpID.IsChecked == false)
            {
                chkProductGrpID.IsChecked = false;
                cboProductGrpID.IsEnabled = false;
            }
        }

        private void msgProductGrpID_MouseEnter(object sender, MouseEventArgs e)
        {
            toolTip.Content = "제품군을 '기타'로 설정시\r\n처음 화면 열 설정내용으로\r\n보실 수 있습니다.\r\n\r\n" +
                "제품군을 '기타'이외로 설정 후 검색하면 \r\n열 필터링 기능을 이용하여 화면에 보이는 열을\r\n숨기고 보이게 할 수 있습니다.";

            // ToolTip 위치 설정 (이미지 아래쪽에 표시)
            Image img = sender as Image;
            Point position = img.PointToScreen(new Point(img.ActualWidth / 2, img.ActualHeight));
            toolTip.PlacementRectangle = new Rect(position.X, position.Y, 0, 0);

            // ToolTip 표시
            toolTip.IsOpen = true;
        }

        private void msgProductGrpID_MouseLeave(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }

        private void rbnDayAndTime_Click(object sender, RoutedEventArgs e)
        {
            rbnOrder = 1;
            Check_rbnGrpID();
        }

        private void rbnWorkQty_Click(object sender, RoutedEventArgs e)
        {
            rbnOrder = 2;
            Check_rbnGrpID();
        }

        //private void cboProductGrpID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if(cboProductGrpID.SelectedValue.ToString() != "99")
        //    {
        //        rbnDayAndTime.IsEnabled = true;
        //        rbnWorkQty.IsEnabled = true;
        //    }
        //    else
        //    {
        //        rbnDayAndTime.IsEnabled = false;
        //        rbnWorkQty.IsEnabled = false;
        //    }
        //}

        private void lblrbnArticleGrpFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkrbnArticleGrpFilter.IsChecked == true)
            {
                rbnDayAndTime.IsEnabled = false;
                rbnWorkQty.IsEnabled = false;
                chkrbnArticleGrpFilter.IsChecked = false;
                ShowColums_ALL();
                FreezeColumns();

            }
            else
            {
                rbnDayAndTime.IsEnabled = true;
                rbnWorkQty.IsEnabled = true;
                chkrbnArticleGrpFilter.IsChecked = true;
                if(rbnDayAndTime.IsChecked == true)
                {
                    rbnDayAndTime_Click(null, null);
                }
                else
                {
                    rbnWorkQty_Click(null, null);

                }
                UnfreezeColumns();
            }
        }

        private void chkrbnArticleGrpFilter_Checked(object sender, RoutedEventArgs e)
        {
            if(chkrbnArticleGrpFilter.IsChecked == true)
            {
                rbnDayAndTime.IsEnabled = true;
                rbnWorkQty.IsEnabled = true;
                chkrbnArticleGrpFilter.IsChecked = true;
            }   
        }

        private void chkrbnArticleGrpFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if(chkrbnArticleGrpFilter.IsChecked == false)
            {
                rbnDayAndTime.IsEnabled = false;
                rbnWorkQty.IsEnabled = false;
                chkrbnArticleGrpFilter.IsChecked = false;
            }   
        }

        private void cboProductGrpID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cboProductGrpID.SelectedValue.ToString()!= "99")
            {
                chkrbnArticleGrpFilter.IsEnabled = true;
           
            }
            else
            {

                chkrbnArticleGrpFilter.IsEnabled = false;
                chkrbnArticleGrpFilter.IsChecked = false;

            }

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
    }

    class Win_ord_OrderClose_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public bool IsCheck { get; set; }
        public string cls { get; set; }
        public bool RowColor { get; set; }
        public string OrderNo { get; set; }
        public string OrderID { get; set; }
        public string CustomID { get; set; }
        public string KCustom { get; set; }

        public string DvlyDate { get; set; }
        public string CloseClss { get; set; }
        public string ChunkRate { get; set; }
        public string LossRate { get; set; }
        public string Article { get; set; }

        public string WorkName { get; set; }

        //public string ArticleID { get; set; }
        public string WorkWidth { get; set; }
        public string OrderQty { get; set; }
        public string UnitClss { get; set; }
        public string InspectQty { get; set; }
        public string PassQty { get; set; }
        public string DefectQty { get; set; }
        public string OutQty { get; set; }
        public string ColorQty { get; set; }
        public string BuyerModel { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerArticleNo { get; set; }
        public string UnitClssName { get; set; }


        public string p1StartWorkDate { get; set; }
        public string p1StartWorkDTime { get; set; }
        public string p1WorkQty { get; set; }
        public string p1ProcessID { get; set; }
        public string p1ProcessName { get; set; }
        public string p1DayAndTime { get; set; }


        public string p2StartWorkDate { get; set; }
        public string p2StartWorkDTime { get; set; }
        public string p2WorkQty { get; set; }
        public string p2ProcessID { get; set; }
        public string p2ProcessName { get; set; }
        public string p2DayAndTime { get; set; }


        public string p3StartWorkDate { get; set; }
        public string p3StartWorkDTime { get; set; }
        public string p3WorkQty { get; set; }
        public string p3ProcessID { get; set; }
        public string p3ProcessName { get; set; }
        public string p3DayAndTime { get; set; }


        public string p4StartWorkDate { get; set; }
        public string p4StartWorkDTime { get; set; }
        public string p4WorkQty { get; set; }
        public string p4ProcessID { get; set; }
        public string p4ProcessName { get; set; }
        public string p4DayAndTime { get; set; }


        public string p5StartWorkDate { get; set; }
        public string p5StartWorkDTime { get; set; }
        public string p5WorkQty { get; set; }
        public string p5ProcessID { get; set; }
        public string p5ProcessName { get; set; }
        public string p5DayAndTime { get; set; }

        public string p6StartWorkDate { get; set; }
        public string p6StartWorkDTime { get; set; }
        public string p6WorkQty { get; set; }
        public string p6ProcessID { get; set; }
        public string p6ProcessName { get; set; }
        public string p6DayAndTime { get; set; }

        public string p7StartWorkDate { get; set; }
        public string p7StartWorkDTime { get; set; }
        public string p7WorkQty { get; set; }
        public string p7ProcessID { get; set; }
        public string p7ProcessName { get; set; }
        public string p7DayAndTime { get; set; }

        public string p8StartWorkDate { get; set; }
        public string p8StartWorkDTime { get; set; }
        public string p8WorkQty { get; set; }
        public string p8ProcessID { get; set; }
        public string p8ProcessName { get; set; }
        public string p8DayAndTime { get; set; }

        public string p9StartWorkDate { get; set; }
        public string p9StartWorkDTime { get; set; }
        public string p9WorkQty { get; set; }
        public string p9ProcessID { get; set; }
        public string p9ProcessName { get; set; }
        public string p9DayAndTime { get; set; }

        public string p10StartWorkDate { get; set; }
        public string p10StartWorkDTime { get; set; }
        public string p10WorkQty { get; set; }
        public string p10ProcessID { get; set; }
        public string p10ProcessName { get; set; }
        public string p10DayAndTime { get; set; }

        public string DayAndTime { get; set; }
        public string DvlyDateEdit { get; set; }
        public string ProductGrpID { get; set; }
        public string ProductGrpName { get; set; }
        //public string AcptDate { get; set; }
        public double OverAndShort { get; set; }

        
        public string OrderID_CV { get; set; }
        public int Num { get; set; }
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

        //public int Count { get; set; }
        //public int OrderSum { get; set; }
        //public int InsertSum { get; set; }
        //public double InspectSum { get; set; }
        //public double PassSum { get; set; }
        //public double DefectSum { get; set; }
        //public double OutSum { get; set; }
        //public double OasSum { get; set; }
    }
}

