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
    public partial class i_AS : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;
        string strFlag = string.Empty;

        int rowNum = 0;
  
        public i_AS()
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
            defaltMode();

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
            //인풋 검색조건 
            cboCompleteYN.ItemsSource = ovcYN;
            cboCompleteYN.DisplayMemberPath = "code_name";
            cboCompleteYN.SelectedValuePath = "code_id";
            cboCompleteYN.SelectedIndex = 0;

            ObservableCollection<CodeView> ovcASType = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ASRSNCode", "Y", "", "");
            cboASType.ItemsSource = ovcASType;
            cboASType.DisplayMemberPath = "code_name";
            cboASType.SelectedValuePath = "code_id";
            cboASType.SelectedIndex = 0;

            List<string[]> lstCost = new List<string[]>();
            string[] lst1 = { "Y", "유상" };
            string[] lst2 = { "N", "무상" };

            lstCost.Add(lst1);
            lstCost.Add(lst2);

            ObservableCollection<CodeView> ovcCost = ComboBoxUtil.Instance.Direct_SetComboBox(lstCost);
            cboCostYN.ItemsSource = ovcCost;
            cboCostYN.DisplayMemberPath = "code_name";
            cboCostYN.SelectedValuePath = "code_id";
            cboCostYN.SelectedIndex = 0;
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
                MainWindow.pf.ReturnCode(txtLocation, (int)Defind_CodeFind.DCF_21, "");
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



        #endregion

        #region 버튼 
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            rowNum = dgdMain.SelectedIndex;
            updateMode();
            this.DataContext = null;
            strFlag = "I";
            
            // 추가시 인풋들 설정 
            txtReqDate.SelectedDate = DateTime.Today;
            cboASType.SelectedIndex = 0;
            cboCostYN.SelectedIndex = 0;
            cboCompleteYN.SelectedIndex = 0;
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "U";
            rowNum = dgdMain.SelectedIndex;
            updateMode();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = dgdMain.SelectedItem as AS_ORDER;
            if (item == null)
            {
                MessageBox.Show("삭제할 데이터를 지정해주세요");
                return;
            }

            if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteData(item.asOrderID))
                {
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            defaltMode();
            FillGrid();
            dgdMain.SelectedIndex = rowNum;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData(strFlag))
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
            btnExcel.IsEnabled = true;

            lblMsg.Visibility = Visibility.Hidden;
            grdAdd.IsHitTestVisible = false;
            dgdMain.IsHitTestVisible = true;

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
                sqlParameter.Add("chkLocation", chkLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("locationID", chkLocationSrh.IsChecked == true && txtLocationSrh.Tag.ToString() != null ? txtLocationSrh.Tag.ToString() : "");

                sqlParameter.Add("chkReqName", chkReqNameSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqName", chkReqNameSrh.IsChecked == true && txtReqNameSrh.Text != null ? txtReqNameSrh.Text : "");
                sqlParameter.Add("chkReqTel", chkReqTelSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqTel", chkReqTelSrh.IsChecked == true && txtReqTelSrh.Text != null ? txtReqTelSrh.Text : "");
                sqlParameter.Add("chkCompleteYN", chkCompleteYNSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("completeYN", chkCompleteYNSrh.IsChecked == true && cboCompleteSrh.SelectedValue != null ? cboCompleteSrh.SelectedValue.ToString() : "");
                sqlParameter.Add("chkASType", 0);
                sqlParameter.Add("ASTypeID", "");

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

        private bool SaveData(string strFlag)
        {
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("asReqDate", txtReqDate.SelectedDate != null ? txtReqDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("asReqUserName", txtReqName.Text ?? "");
                sqlParameter.Add("asReqUserTelNo", txtReqTel.Text?? "");
                sqlParameter.Add("asLocation", txtLocation.Text ?? "");
                sqlParameter.Add("constrCustomID", String.IsNullOrEmpty(txtContruct.Text) ?  "": txtContruct.Tag.ToString());
                sqlParameter.Add("asSmallLocation", "");
                sqlParameter.Add("asChargerMCNo", txtChargerMCNo.Text ?? "");
                sqlParameter.Add("costYN", cboCostYN.SelectedValue != null ? cboCostYN.SelectedValue.ToString() : "");
                sqlParameter.Add("asAmount", String.IsNullOrEmpty(txtAmount.Text) ? 0 :Convert.ToDouble(txtAmount.Text));
                sqlParameter.Add("asDamDangJa", txtDamDang.Text ?? "");
                sqlParameter.Add("asDate", txtasDate.SelectedDate != null ? txtasDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("asCompleteYN", cboCompleteYN.SelectedValue.ToString() ?? "");
                sqlParameter.Add("asTypeID", cboASType.SelectedValue != null ? cboASType.SelectedValue.ToString() : "");
                sqlParameter.Add("comments", txtComments.Text ?? "");

                Procedure pro = new Procedure();

                if (strFlag == "I")
                {
                    sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                    pro.Name = "xp_AS_iAS";
                    pro.OutputUseYN = "N";
                    pro.OutputName = "orderDcntFeesID";
                    pro.OutputLength = "10";
                } else
                {
                    sqlParameter.Add("asOrderID", txtASID.Text ?? "");
                    sqlParameter.Add("lastUpdateUserID", MainWindow.CurrentUser);

                    pro.Name = "xp_AS_uAS";
                    pro.OutputUseYN = "N";
                    pro.OutputName = "orderDcntFeesID";
                    pro.OutputLength = "10";
                }

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

        private bool DeleteData(string asOrderID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("asOrderID", asOrderID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_AS_dAS", sqlParameter, "D");
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
            var item = dgdMain.SelectedItem as AS_ORDER;

            btnUpdate.IsEnabled = true;
            this.DataContext = item;
            
        }

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnUpdate_Click(null, null);
        }

        #endregion

        private void txtLocation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtLocation, txtContruct, 7078, txtLocation.Text);
            }

        }

        private void btnPfLocation_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtContruct, 7078, txtLocation.Text);
        }

        #region 유지추가 
        private void btnStay_Click(object sender, RoutedEventArgs e)
        {
            popStay.IsOpen = true;
        }
        private void popStay_Opened(object sender, EventArgs e)
        {
            dtpPreviousMonth.SelectedDate = DateTime.Today.AddMonths(-1);
            dtpThisMonth.SelectedDate = DateTime.Today;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("chkDate", 1);
                sqlParameter.Add("sDate", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "1");
                sqlParameter.Add("eDate", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "31");
                sqlParameter.Add("chkLocation", 0);
                sqlParameter.Add("locationID", "");
                sqlParameter.Add("chkReqName", 0);
                sqlParameter.Add("reqName", "");
                sqlParameter.Add("chkReqTel", 0);
                sqlParameter.Add("reqTel", "");
                sqlParameter.Add("chkCompleteYN", 0);
                sqlParameter.Add("completeYN", "");
                sqlParameter.Add("chkASType", 0);
                sqlParameter.Add("ASTypeID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_AS_sAS", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        if (MessageBox.Show(dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 AS 접수가 " + dt.Rows.Count.ToString() + " 건이 있습니다. " +
                            "무시하고 진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            copyAS();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show(dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 AS 접수가 " + dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "월의 AS 접수로 복사됩니다." +
                            "진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            copyAS();
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 AS 접수가 " + dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "월의 AS 접수로 복사됩니다." +
                        "진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        copyAS();
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

            popStay.IsOpen = false;
        }

        private void btnNO_Click(object sender, RoutedEventArgs e)
        {
            popStay.IsOpen = false;
        }

        private void copyAS()
        {
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("FromYYYYMM", dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM"));
                sqlParameter.Add("ToYYYYMM", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM")); 
                sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                Procedure pro1 = new Procedure();
                pro1.Name = "xp_AS_iASCopy";
                pro1.OutputUseYN = "N";
                pro1.OutputName = "OrderID";
                pro1.OutputLength = "10";

                Prolist.Add(pro1);
                ListParameter.Add(sqlParameter);

                string[] Confirm = new string[2];
                Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                if (Confirm[0] != "success")
                {
                    MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                }
                else
                {
                    MessageBox.Show("유지추가가 완료 되었습니다.");
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