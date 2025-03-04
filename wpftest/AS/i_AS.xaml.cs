using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using WizMes_EVC.PopUP;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

/**************************************************************************************************
'** 프로그램명 : i_AS
'** 설명       : 수주등록
'** 작성일자   : 2024.12.31
'** 작성자     : 김수정
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2025.03.04, 최대현, 강경단 책임                 , A/S사유 자유입력으로 변경 및 첨부파일 추가                              
' 
*/

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
        string asOrderID_global = string.Empty;


        int rowNum = 0;


        private ToolTip currentToolTip;
        private System.Windows.Threading.DispatcherTimer currentTimer;


        //FTP 활용모음
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;
        string strDelFileName = string.Empty;

        List<string[]> deleteListFtpFile = new List<string[]>(); // 삭제할 파일 리스트
        List<string[]> lstExistFtpFile = new List<string[]>();

        Dictionary<string, string> lstFtpFilePath = new Dictionary<string, string>();

        private FTP_EX _ftp = null;
        string SketchPath = null;

        List<string[]> listFtpFile = new List<string[]>();
        HashSet<string> lstFilesName = new HashSet<string>();

        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/AsOrder";

        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

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

            //ObservableCollection<CodeView> ovcASType = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ASRSNCode", "Y", "", "");
            //cboASType.ItemsSource = ovcASType;
            //cboASType.DisplayMemberPath = "code_name";
            //cboASType.SelectedValuePath = "code_id";
            //cboASType.SelectedIndex = 0;

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
            txtLocationSrh.Focus();
        }

        private void chkLocationSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtLocationSrh.IsEnabled = false;
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

        private void cboCostYN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cboCostYN.SelectedValue != null && cboCostYN.SelectedValue.ToString() == "Y")
            {
                dtpAsDepositDate.IsEnabled = true;
                if (IsDatePickerNull(dtpAsDepositDate))
                {
                    dtpAsDepositDate.SelectedDate = DateTime.Today;
                }
            }
            else if(cboCostYN.SelectedValue != null && cboCostYN.SelectedValue.ToString() == "N")
            {
                dtpAsDepositDate.IsEnabled = false;
            }
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
            //cboASType.SelectedIndex = 0;
            cboCostYN.SelectedIndex = 0;
            cboCompleteYN.SelectedIndex = 0;
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(asOrderID_global)) return;
         
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
                    FTP_RemoveDir(asOrderID_global);                   
                    FillGrid();
                    dgdMain.SelectedIndex = rowNum - 1;
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
            strFlag = string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData(strFlag))
            {
                defaltMode();
                FillGrid();
                rowNum = strFlag == "I" ? dgdMain.Items.Count - 1 : rowNum;
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
                sqlParameter.Add("locationID", chkLocationSrh.IsChecked == true ? txtLocationSrh.Text : ""); 

                sqlParameter.Add("chkReqName", chkReqNameSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqName", chkReqNameSrh.IsChecked == true && txtReqNameSrh.Text != null ? txtReqNameSrh.Text : "");
                sqlParameter.Add("chkReqTel", chkReqTelSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("reqTel", chkReqTelSrh.IsChecked == true && txtReqTelSrh.Text != null ? txtReqTelSrh.Text : "");
                sqlParameter.Add("chkCompleteYN", chkCompleteYNSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("completeYN", chkCompleteYNSrh.IsChecked == true && cboCompleteSrh.SelectedValue != null ? cboCompleteSrh.SelectedValue.ToString() : "");

                sqlParameter.Add("chkAsReason", 0);
                sqlParameter.Add("asReason", "");
              

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
                                asReason = dr["asReason"].ToString(),
                                asDepositDate = DatePickerFormat(dr["asDepositDate"].ToString()),
                                //asType = dr["asType"].ToString(),
                                //asTypeID = dr["asTypeID"].ToString(),

                                sketch1FileName = dr["sketch1FileName"].ToString(),
                                sketch1FilePath = dr["sketch1FilePath"].ToString(),
                                sketch1FileAlias = dr["sketch1FileAlias"].ToString(),
                                sketch2FileName = dr["sketch2FileName"].ToString(),
                                sketch2FilePath = dr["sketch2FilePath"].ToString(),
                                sketch2FileAlias = dr["sketch2FileAlias"].ToString(),

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
                sqlParameter.Add("constrCustomID", string.IsNullOrEmpty(txtContruct.Text) ?  "": txtContruct.Tag.ToString());
                sqlParameter.Add("asSmallLocation", "");
                sqlParameter.Add("asChargerMCNo", txtChargerMCNo.Text ?? "");
                sqlParameter.Add("costYN", cboCostYN.SelectedValue != null ? cboCostYN.SelectedValue.ToString() : "");
                sqlParameter.Add("asAmount", string.IsNullOrEmpty(txtAmount.Text) ? 0 :Convert.ToDouble(txtAmount.Text));
                sqlParameter.Add("asDamDangJa", txtDamDang.Text ?? "");
                sqlParameter.Add("asDate", txtasDate.SelectedDate != null ? txtasDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("asCompleteYN", cboCompleteYN.SelectedValue.ToString() ?? "");
                sqlParameter.Add("asReason", txtAsReason.Text);
                sqlParameter.Add("asDepositDate", !IsDatePickerNull(dtpAsDepositDate) ? ConvertDate(dtpAsDepositDate) : "");
                //sqlParameter.Add("asTypeID", cboASType.SelectedValue != null ? cboASType.SelectedValue.ToString() : "");
                sqlParameter.Add("comments", txtComments.Text ?? "");

                Procedure pro = new Procedure();

                if (strFlag == "I")
                {
                    sqlParameter.Add("asOrderID", "");
                    sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                    pro.Name = "xp_AS_iAS";
                    pro.OutputUseYN = "Y";
                    pro.OutputName = "asOrderID";
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

                string PrimaryKey = string.Empty;

                List<KeyValue> list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

                if (list_Result[0].key.ToLower() == "success")
                {
                    PrimaryKey = strFlag == "I" ? list_Result[1].value.Trim() : txtASID.Text.Trim();
                    if(PrimaryKey != string.Empty)
                    {
                        if (FTP_Update(PrimaryKey))
                            MessageBox.Show("저장 되었습니다.", "확인");
                        else
                            MessageBox.Show("데이터는 저장되었으나\nFTP업로드는 실패하였습니다.\nini설정 파일 및 네트워크가 올바른지 확인하세요", "확인");
                    }
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

        private bool FTP_Update(string PKKey)
        {
            bool flag = true;

            try
            {

                if (PKKey.Trim() != string.Empty)
                {
                    if (deleteListFtpFile.Count > 0)
                    {
                        foreach (string[] str in deleteListFtpFile)
                        {
                            FTP_RemoveFile(PKKey + "/" + str[0]);
                        }
                    }

                    if (listFtpFile.Count > 0)
                    {

                        FTP_Save_File(listFtpFile, PKKey);
                    }


                    UpdateDBFtp(PKKey); // 리스트 갯수가 0개 이상일때 해버리면, 수정시에 저장이 안됨
                }

                // 파일 List 비워주기
                listFtpFile.Clear();
                lstFilesName.Clear();
                deleteListFtpFile.Clear();
            }
            catch (Exception e)
            {   
                flag = false;
                throw e;
            }


            return flag;
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
            lstFilesName.Clear();
            asOrderID_global = string.Empty;

            var item = dgdMain.SelectedItem as AS_ORDER;


            this.DataContext = item;
            if(item != null)
            {
                rowNum = dgdMain.SelectedIndex;
                asOrderID_global = item.asOrderID;
            }

            addLstFile_FTP();

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
            MainWindow.pf.ReturnCode(txtLocation,txtContruct, 7078, txtLocation.Text);
        }

        private void txtContruct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtContruct, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        private void btnPfContruct_Click(object sender, RoutedEventArgs e)
        {

            MainWindow.pf.ReturnCode(txtContruct, (int)Defind_CodeFind.DCF_CUSTOM, "");
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                ShowTooltipMessage(sender as FrameworkElement, "숫자만 입력 가능합니다.", MessageBoxImage.Error, PlacementMode.Right);
            }
        }


        //툴팁
        private void ShowTooltipMessage(FrameworkElement element, string message, MessageBoxImage iconType = MessageBoxImage.None, PlacementMode placement = PlacementMode.Bottom)
        {
            // 이미 열려있는 툴팁이 있다면 닫기
            if (currentToolTip != null && currentToolTip.IsOpen)
            {
                currentToolTip.IsOpen = false;
                if (currentTimer != null)
                {
                    currentTimer.Stop();
                    currentTimer = null;
                }
            }

            object tooltipContent;

            // 아이콘이 필요 없는 경우
            if (iconType == MessageBoxImage.None)
            {
                tooltipContent = message;
            }
            else
            {
                // StackPanel 생성
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // 시스템 아이콘 설정
                System.Drawing.Icon systemIcon;
                switch (iconType)
                {
                    case MessageBoxImage.Information:
                        systemIcon = System.Drawing.SystemIcons.Information;
                        break;
                    case MessageBoxImage.Warning:
                        systemIcon = System.Drawing.SystemIcons.Warning;
                        break;
                    case MessageBoxImage.Error:
                        systemIcon = System.Drawing.SystemIcons.Error;
                        break;
                    case MessageBoxImage.Question:
                        systemIcon = System.Drawing.SystemIcons.Question;
                        break;
                    default:
                        systemIcon = null;
                        break;
                }

                if (systemIcon != null)
                {
                    // System.Drawing에서 아이콘 가져오기
                    System.Windows.Media.Imaging.BitmapSource iconSource =
                        System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            systemIcon.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    // 이미지 생성
                    var image = new Image
                    {
                        Source = iconSource,
                        Width = 16,
                        Height = 16,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    // StackPanel에 추가
                    stackPanel.Children.Add(image);
                }

                // 텍스트블록 생성
                var textBlock = new TextBlock
                {
                    Text = message,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(textBlock);
                tooltipContent = stackPanel;
            }

            // 새 툴팁 생성
            var tooltip = new ToolTip
            {
                Content = tooltipContent,
                PlacementTarget = element,
                Placement = placement,
                IsOpen = true
            };

            // 위치에 따른 설정
            if (placement == PlacementMode.Bottom)
            {
                tooltip.VerticalOffset = 5;
            }
            else if (placement == PlacementMode.Right)
            {
                tooltip.Placement = PlacementMode.Bottom;
                tooltip.VerticalOffset = 5;
                element.Dispatcher.BeginInvoke(new Action(() =>
                {
                    double offset = element.ActualWidth - tooltip.ActualWidth;
                    tooltip.HorizontalOffset = offset;
                }));
            }

            currentToolTip = tooltip;

            // 3초 후 툴팁 자동 닫기
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, eventArgs) =>
            {
                tooltip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
            currentTimer = timer;
        }

        #endregion

        #region 그리드안에서 요소 찾기

        private void addLstFile_FTP()
        {
            List<Grid> grids = new List<Grid> { grdAdd };

            foreach (Grid grd in grids)
            {
                FindUiObject(grd, child =>
                {
                    if (child is TextBox textbox)
                    {
                        if (textbox.Name.Contains("FileName") || textbox.Name.Contains("txtSketch"))
                        {
                            if (!string.IsNullOrWhiteSpace(textbox.Text))
                            {
                                lstFilesName.Add(textbox.Text.Trim());
                            }
                        }
                    }

                });
            }
        }
         

        //UI컨트롤 요소찾기
        private void FindUiObject(DependencyObject parent, Action<DependencyObject> action)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                action?.Invoke(child);

                FindUiObject(child, action);
            }
        }

        //컨트롤 안 특정 타입의 자식 컨트롤을 찾는 함수 (그리드내에서)
        //var parentContainer = VisualTreeHelper.GetParent(checkbox);
        //var datePicker = FindChild<DatePicker>(parentContainer);
        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                // 재귀적으로 자식의 자식들도 검색
                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }


        // 자식요소 안에서 부모요소 찾기
        //DataGridRow row = FindVisualParent<DataGridRow>(checkBox); 데이터그리드안의 행속 체크박스의 부모행 찾기
        //DataGrid parentGrid = FindVisualParent<DataGrid>(row); 데이터그리드 행의 부모 데이터그리드 찾기
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        #endregion

        #region FTP

        private void btnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            // (버튼)sender 마다 tag를 달자.
            string ClickPoint = ((Button)sender).Tag.ToString();
            if (ClickPoint.Equals("Sketch1")) { FTP_Upload_TextBox(txtSketch1FileName); txtSketch1FileAlias.IsReadOnly = false; }  //긴 경로(FULL 사이즈)
            else if (ClickPoint.Equals("Sketch2")) { FTP_Upload_TextBox(txtSketch2FileName); txtSketch2FileAlias.IsReadOnly = false; }  

        }

        //미리 맵핑한 텍스트박스에 Text =파일명, Tag =파일경로
        //listFtpFile에는 FTP에 업로드할 파일정보를 담음(파일명, FullPath)
        private void FTP_Upload_TextBox(TextBox textBox)
        {

            try
            {
                if (!textBox.Text.Equals(string.Empty) && strFlag.Equals("U"))
                {
                    MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                    return;
                }
                else
                {


                    Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();
                    //OFdlg.Filter =
                    //    "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx, *.pdf) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx; *.pdf | All Files|*.*";

                    OFdlg.Filter = "모든 파일 (*.*)|*.*";

                    Nullable<bool> result = OFdlg.ShowDialog();
                    if (result == true)
                    {
                        // 선택된 파일의 확장자 체크
                        if (MainWindow.OFdlg_Filter_NotAllowed.Contains(System.IO.Path.GetExtension(OFdlg.FileName).ToLower()))
                        {
                            MessageBox.Show("보안상의 이유로 해당 파일은 업로드할 수 없습니다.");
                            return;
                        }

                        strFullPath = OFdlg.FileName;

                        string ImageFileName = OFdlg.SafeFileName;  //명.
                        string ImageFilePath = string.Empty;       // 경로


                        //프로세스 점유중인 파일도 스트림 가능한데 이거하려면 FTP업로드하는 메서드도 다 바꿔야함
                        //long FileSize;
                        //using (FileStream fs = new FileStream(OFdlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        //{
                        //    FileSize = fs.Length;
                        //}

                        // 파일명 유효성 검사 추가
                        //if (!IsValidFileName(ImageFileName))
                        //{
                        //    MessageBox.Show("파일명에 허용되지 않는 특수문자가 포함되어 있습니다.\n시스템 저장시 오류를 일으킬 수 있으므로 변경 후 첨부하여 주세요");
                        //    return;
                        //}

                        ImageFilePath = strFullPath.Replace(ImageFileName, "");

                        StreamReader sr = new StreamReader(OFdlg.FileName);
                        long FileSize = sr.BaseStream.Length;
                   
                        if (sr.BaseStream.Length > (1024 * 1024 * 500))  // 100MB in bytes
                        {
                            //업로드 파일 사이즈범위 초과기
                            MessageBox.Show("첨부파일 크기는 500Mb 미만 이어야 합니다.");
                            sr.Close();
                            return;
                        }
                        if (!FTP_Upload_Name_Cheking(ImageFileName))
                        {
                            MessageBox.Show("업로드 하려는 파일 중, 이름이 중복된 항목이 있습니다." +
                                            "\n파일 이름을 변경하고 다시 시도하여 주세요\n다른 탭에 중복된 파일이 있는지 확인하세요.");
                        }
                        else
                        {
                            textBox.Text = ImageFileName;
                            textBox.Tag = ImageFilePath;

                            string[] strTemp = new string[] { ImageFileName, ImageFilePath.ToString() };
                            listFtpFile.Add(strTemp);
                            lstFilesName.Add(ImageFileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("사용 중"))
                    MessageBox.Show("업로드하려는 파일이 열려 있습니다.\n먼저 파일을 닫고 첨부하여 주세요");
            }

        }

        //이름중복
        private bool FTP_Upload_Name_Cheking(string fileName)
        {
            bool flag = true;

            if (!lstFilesName.Add(fileName))
            {
                flag = false;
                return flag;
            }


            return flag;
        }

        private bool FTP_Save_File(List<string[]> listStrArrayFileInfo, string MakeFolderName)
        {
            bool flagResult = true;

            try
            {
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                List<string[]> UpdateFilesInfo = new List<string[]>();
                string[] fileListSimple;
                string[] fileListDetail = null;
                fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

                // 기존 폴더 확인작업.
                bool MakeFolder = false;
                MakeFolder = FolderInfoAndFlag(fileListSimple, MakeFolderName);

                bool Makefind = false;
                Makefind = FileInfoAndFlag(fileListSimple, MakeFolderName);


                if (MakeFolder == false)
                {


                    if (_ftp.createDirectory(MakeFolderName) == false)
                    {
                        MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");
                        return flagResult =false;
                    }

                }
                else
                {
                    fileListDetail = _ftp.directoryListSimple(MakeFolderName, Encoding.Default);
                }

                for (int i = 0; i < listStrArrayFileInfo.Count; i++)
                {
                    bool flag = true;

                    if (fileListDetail != null)
                    {
                        foreach (string compare in fileListDetail)
                        {
                            if (compare.Equals(listStrArrayFileInfo[i][0]))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        listStrArrayFileInfo[i][0] = MakeFolderName + "/" + listStrArrayFileInfo[i][0];
                        UpdateFilesInfo.Add(listStrArrayFileInfo[i]);
                    }
                }

                if (!_ftp.UploadTempFilesToFTP(UpdateFilesInfo))
                {
                    MessageBox.Show("파일업로드에 실패하였습니다.");
                    return flagResult = false;
                }
            }
            catch
            {
                flagResult = false;
                throw;
            }

            return flagResult;
           
        }

        private void btnFileDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 삭제 하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                string ClickPoint = ((Button)sender).Tag.ToString();
                string fileName = string.Empty;
             
                if ((ClickPoint == "Sketch1") && (txtSketch1FileName.Text != string.Empty)) { fileName = txtSketch1FileName.Text; FileDeleteAndTextBoxEmpty(txtSketch1FileName);txtSketch1FileAlias.IsReadOnly = true; txtSketch1FileAlias.Text = string.Empty; lstFilesName.Remove(fileName); }
                else if ((ClickPoint == "Sketch2") && (txtSketch2FileName.Text != string.Empty)) { fileName = txtSketch2FileName.Text; FileDeleteAndTextBoxEmpty(txtSketch2FileName); txtSketch2FileAlias.IsReadOnly = true; txtSketch2FileAlias.Text = string.Empty; lstFilesName.Remove(fileName); } 

            }

        }
        private void FileDeleteAndTextBoxEmpty(TextBox txt)
        {
            if (strFlag.Equals("U"))
            {
                //var Article = dgdMain.SelectedItem as Win_ord_Order_U_CodeView_Nadaum;

                //if (Article != null)
                //{
                //FTP_RemoveFile(Article.ArticleID + "/" + txt.Text);

                // 파일이름, 파일경로
                string[] strFtp = { txt.Text, txt.Tag != null ? txt.Tag.ToString() : "" };

                deleteListFtpFile.Add(strFtp);
                //}
            }

            txt.Text = string.Empty;
            txt.Tag = string.Empty;
        }



        //파일 삭제
        private bool FTP_RemoveFile(string strSaveName)
        {
            try
            {
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
                if (_ftp.delete(strSaveName) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                throw;
            }           
        
        }

        //폴더 삭제(내부 파일 자동 삭제)
        private bool FTP_RemoveDir(string strSaveName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp.removeDir(strSaveName) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //보기
        private void btnFileSee_Click(object sender, RoutedEventArgs e)
        {
            if(txtASID.Text.Trim() != string.Empty)
            {
                MessageBoxResult msgresult = MessageBox.Show("다운로드 후 파일을 바로 여시겠습니까?", "보기 확인", MessageBoxButton.YesNoCancel);
                if (msgresult == MessageBoxResult.Yes || msgresult == MessageBoxResult.No)
                {
                    //버튼 태그값.
                    string ClickPoint = ((Button)sender).Tag.ToString();

                    string sketch1 = txtSketch1FileName.Text.Trim() != "" ? txtSketch1FileName.Text : "";
                    string sketch2 = txtSketch2FileName.Text.Trim() != "" ? txtSketch2FileName.Text : "";

                    if ((ClickPoint == "Sketch1") && (txtSketch1FileName.Text == string.Empty)
                       || (ClickPoint == "Sketch2") && (txtSketch2FileName.Text == string.Empty))

                    {
                        MessageBox.Show("파일이 없습니다.");
                        return;
                    }


                    try
                    {
                        // 접속 경로
                        _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                        string str_path = string.Empty;
                        str_path = FTP_ADDRESS + '/' + txtASID.Text;
                        _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        //원격경로
                        if (ClickPoint == "Sketch1") { str_remotepath = sketch1; }
                        else if (ClickPoint == "Sketch2") { str_remotepath = sketch2; }


                        //로컬경로
                        if (ClickPoint == "Sketch1") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch1; }
                        else if (ClickPoint == "Sketch2") { str_localpath = LOCAL_DOWN_PATH + "\\" + sketch2; }


                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                        if (DI.Exists == false)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        //if (file.Exists)
                        //{
                        //    file.Delete();
                        //}
                        try
                        {
                            file.Delete();
                        }
                        catch (IOException)
                        {
                            // 파일명과 확장자 분리
                            string directory = System.IO.Path.GetDirectoryName(str_localpath);
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(str_localpath);
                            string extension = System.IO.Path.GetExtension(str_localpath);

                            // 복사본 파일명 생성 (예: test.hwp -> test - 복사본.hwp)
                            int copyNum = 1;
                            string newPath = System.IO.Path.Combine(directory, $"{fileName} - 복사본{extension}");

                            // 복사본 파일이 이미 존재하면 번호 추가 (예: test - 복사본 (2).hwp)
                            while (File.Exists(newPath))
                            {
                                copyNum++;
                                newPath = System.IO.Path.Combine(directory, $"{fileName} - 복사본 ({copyNum}){extension}");
                            }

                            str_localpath = newPath; // 새로운 경로로 업데이트
                            MessageBox.Show("파일이 사용 중이어서 복사본으로 다운로드 했습니다.", "알림");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("파일 처리 중 오류가 발생했습니다: " + ex.Message);
                            return;
                        }

                        _ftp.download(str_remotepath, str_localpath);

                        //파일 다운로드 후 바로 열기
                        if (File.Exists(str_localpath) && msgresult == MessageBoxResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new ProcessStartInfo
                                {
                                    FileName = str_localpath,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("파일을 여는 중 오류가 발생했습니다:" +
                                    "\n파일을 열기위한 프로그램이 없거나 기본 실행프로그램이 지정이 안 되었을 수도 있습니다." + ex.Message);
                            }
                        }
                        else if ((File.Exists(str_localpath) && msgresult == MessageBoxResult.No))
                        {
                            MessageBox.Show("파일을 다운로드 하였습니다.", "확인");
                            try
                            {
                                string folderPath = System.IO.Path.GetDirectoryName(str_localpath);
                                //폴더이름의 타이틀명을 찾
                                var openFolders = Process.GetProcessesByName("explorer")
                                    .Where(p =>
                                    {
                                        try
                                        {
                                            return p.MainWindowTitle.Contains(System.IO.Path.GetFileName(folderPath));
                                        }
                                        catch
                                        {
                                            return false;
                                        }
                                    });

                                if (!openFolders.Any())
                                {
                                    // 폴더가 열려있지 않을 때만 새로 열기
                                    Process.Start("explorer.exe", $"\"{folderPath}\"");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("폴더를 여는 중 오류가 발생했습니다:" + ex.Message);
                            }
                        }


                    }
                    catch (Exception ex) // 뭐든 간에 파일 없다고 하자
                    {

                        MessageBox.Show("파일이 존재하지 않습니다.\r관리자에게 문의해주세요.");
                        return;
                    }
                }
            }
        
        }

        /// <summary>
        /// 해당영역에 폴더가 있는지 확인
        /// </summary>
        bool FolderInfoAndFlag(string[] strFolderList, string FolderName)
        {
            bool flag = false;
            foreach (string FolderList in strFolderList)
            {
                if (FolderList == FolderName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 해당영역에 파일 있는지 확인
        /// </summary>
        bool FileInfoAndFlag(string[] strFileList, string FileName)
        {
            bool flag = false;
            foreach (string FileList in strFileList)
            {
                if (FileList == FileName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }


        private bool UpdateDBFtp(string asOrderID)
        {
            bool flag = false;

            string str_localpath = string.Empty;
            List<string[]> UpdateFilesInfo = new List<string[]>();

            try
            {
               

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("asOrderID", asOrderID);
           
                sqlParameter.Add("sketch1FileName", txtSketch1FileName.Text.Trim() != "" ? txtSketch1FileName.Text : "");
                sqlParameter.Add("sketch1FilePath", txtSketch1FileName.Tag != null ? txtSketch1FileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + asOrderID : "DEL" : "");
                sqlParameter.Add("sketch1FileAlias", txtSketch1FileAlias.Text.Trim() != "" ? txtSketch1FileAlias.Text : "");

                sqlParameter.Add("sketch2FileName", txtSketch2FileName.Text.Trim() != "" ? txtSketch2FileName.Text : "");
                sqlParameter.Add("sketch2FilePath", txtSketch2FileName.Tag != null ? txtSketch2FileName.Tag.ToString().Trim() != string.Empty ? "/ImageData/Order/" + asOrderID : "DEL" : "");
                sqlParameter.Add("sketch2FileAlias", txtSketch2FileAlias.Text.Trim() != "" ? txtSketch2FileAlias.Text : "");
            

                string[] result = DataStore.Instance.ExecuteProcedure("xp_AS_uAS_FTP", sqlParameter, true);


                if (result[0].Equals("success"))
                {
                  
                    flag = true;
                }
                else
                {
                    MessageBox.Show("수정 실패 , 내용 : " + result[1]);
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
      


            return flag;
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
            public string asReason { get; set; }
            public string asDepositDate { get; set; }

            public string sketch1FileName { get; set; }
            public string sketch1FilePath { get; set; }
            public string sketch1FileAlias { get; set; }
            public string sketch2FileName { get; set; }
            public string sketch2FilePath { get; set; }
            public string sketch2FileAlias { get; set; }

        }

     
    }

  

}