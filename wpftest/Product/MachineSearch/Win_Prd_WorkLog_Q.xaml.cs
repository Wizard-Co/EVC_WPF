using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WizMes_HanYoung.PopUp;
using WizMes_HanYoung.PopUP;
using System.Windows.Input;

namespace WizMes_HanYoung
{
    /// <summary>
    /// Win_Prd_ProcessResult_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Prd_WorkLog_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        public Win_Prd_WorkLog_Q()
        {
            InitializeComponent();
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;

            setComboBox();

            cboProcess.SelectedIndex = 0;
            cboMachine.SelectedIndex = 0;
            cboGubun.SelectedIndex = 0;
        }

        #region 콤보박스 세팅 setComboBox

        private void setComboBox()
        {
            ObservableCollection<CodeView> ovcProcess = ComboBoxUtil.Instance.GetWorkProcess(0, "");
            this.cboProcess.ItemsSource = ovcProcess;
            this.cboProcess.DisplayMemberPath = "code_name";
            this.cboProcess.SelectedValuePath = "code_id";

            ObservableCollection<CodeView> ovcMachine = GetMachineByProcessID("");
            this.cboMachine.ItemsSource = ovcMachine;
            this.cboMachine.DisplayMemberPath = "code_name";
            this.cboMachine.SelectedValuePath = "code_id";

            //라벨발행품 여부(입력)
            List<string[]> lstGubun = new List<string[]>();
            lstGubun.Add(new string[] { "0", "전체" });
            lstGubun.Add(new string[] { "1", "실적처리건" });
            lstGubun.Add(new string[] { "2", "실적처리 대기건" });
            lstGubun.Add(new string[] { "3", "오류건" });
            lstGubun.Add(new string[] { "4", "오류비포함" });

            ObservableCollection<CodeView> ovcGugunSearch = ComboBoxUtil.Instance.Direct_SetComboBox(lstGubun);
            this.cboGubun.ItemsSource = ovcGugunSearch;
            this.cboGubun.DisplayMemberPath = "code_name";
            this.cboGubun.SelectedValuePath = "code_id";
        }

        #endregion // 콤보박스 세팅 setComboBox

        #region mt_Machine - 호기 세팅

        /// <summary>
        /// 호기ID 가져오기
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ObservableCollection<CodeView> GetMachineByProcessID(string value)
        {
            ObservableCollection<CodeView> ovcMachine = new ObservableCollection<CodeView>();

            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Add("sProcessID", value);

            DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sMachineForComboBoxAndUsing", sqlParameter, false);

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    CodeView CV = new CodeView();
                    CV.code_id = "";
                    CV.code_name = "전체";
                    ovcMachine.Add(CV);

                    DataRowCollection drc = dt.Rows;

                    foreach (DataRow dr in drc)
                    {
                        CodeView mCodeView = new CodeView()
                        {
                            code_id = dr["Code"].ToString().Trim(),
                            code_name = dr["Name"].ToString().Trim()
                        };

                        ovcMachine.Add(mCodeView);
                    }
                }
            }

            return ovcMachine;
        }

        #endregion // mt_Machine - 호기 세팅

        #region 날짜버튼 클릭 이벤트

        // 전일 금일 전월 금월 버튼
        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }
        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
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
        //전일
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            //dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
            //dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);

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

        #endregion

        #region 검색조건 - 공정 콤보박스 선택 이벤트

        // 공정 콤보박스 선택 이벤트
        private void cboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProcess.SelectedValue != null)
            {
                ObservableCollection<CodeView> ovcMachine = GetMachineByProcessID(cboProcess.SelectedValue.ToString());
                this.cboMachine.ItemsSource = ovcMachine;
                this.cboMachine.DisplayMemberPath = "code_name";
                this.cboMachine.SelectedValuePath = "code_id";

                cboMachine.SelectedIndex = 0;
            }
        }

        #endregion

        #region 버튼 클릭 이벤트

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = false;

                using (Loading lw = new Loading(beSearch))
                {
                    lw.ShowDialog();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void beSearch()
        {
            FillGrid();

            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("조회된 데이터가 없습니다.");
                return;
            }
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        // 엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] dgdStr = new string[2];
            dgdStr[0] = "자동수집 자료";
            dgdStr[1] = dgdMain.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(dgdStr);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMain);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;
                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
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
        }

        #endregion

        #region 주요 메서드 - 조회 FillGrid

        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

            try
            {

                // 공정 호기 세팅
                string ProcessID = "";
                string MachineID = "";
                
                // 공정을 전체나 선택하지 않았을시 → 호기는 공정 + 호기로 출력 → 공정과 호기를 검색하기 위해서
                if (chkMachine.IsChecked == true
                    && cboMachine.SelectedValue != null
                    && cboMachine.SelectedValue.ToString().Trim().Length == 6)
                {
                    ProcessID = cboMachine.SelectedValue.ToString().Trim().Substring(0, 4);
                    MachineID = cboMachine.SelectedValue.ToString().Trim().Substring(4, 2);
                }
                else
                {
                    ProcessID = chkProcess.IsChecked == true && cboProcess.SelectedValue != null ? cboProcess.SelectedValue.ToString() : "";
                    MachineID = chkMachine.IsChecked == true && cboMachine.SelectedValue != null ? cboMachine.SelectedValue.ToString() : "";
                }

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("sFromDate", dtpSDate.SelectedDate.Value.ToString("yyyyMMdd"));
                sqlParameter.Add("sToDate", dtpEDate.SelectedDate.Value.ToString("yyyyMMdd"));
                sqlParameter.Add("sProcessID", ProcessID);
                sqlParameter.Add("sMachineID", MachineID);
                sqlParameter.Add("nErrGbn", ConvertInt(cboGubun.SelectedValue.ToString()));

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_prd_sWorkLog", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var WinR = new Win_Prd_WorkLog_Q_CodeView()
                            {
                                Num = i.ToString(),

                                LogID = dr["LogID"].ToString(),
                                WorkDate = dr["WorkDate"].ToString(),
                                WorkDate_CV = DatePickerFormat(dr["WorkDate"].ToString()),
                                WorkQty = Convert.ToDouble(dr["WorkQty"]),
                                DefectQty = Convert.ToDouble(dr["DefectQty"]),
                                WorkTime = dr["WorkTime"].ToString(),
                                WorkTime_CV = ConvertTimeFormat(dr["WorkTime"].ToString()),
                                StationNO = dr["StationNO"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),

                                Process = dr["Process"].ToString(),
                                MachineID = dr["MachineID"].ToString(),
                                Machine = dr["Machine"].ToString(),
                                MachineNo = dr["MachineNo"].ToString(),

                                SachulPressure = dr["SachulPressure"].ToString(),
                                SachulSpeed = dr["SachulSpeed"].ToString(),
                                SachulLocation = dr["SachulLocation"].ToString(),

                                BoApPressure = dr["BoApPressure"].ToString(),
                                BoApSpeed = dr["BoApSpeed"].ToString(),
                                BoApTime = dr["BoApTime"].ToString(),
                                Cushun = dr["Cushun"].ToString(),
                                SachulTime = dr["SachulTime"].ToString(),
                                CoolingTime = dr["CoolingTime"].ToString(),
                                MeasurSpeed = dr["MeasurSpeed"].ToString(),
                                BaeAp = dr["BaeAp"].ToString(),
                                SukBaekMeasureBefore = dr["SukBaekMeasureBefore"].ToString(),
                                SukBaekMeasureAfter = dr["SukBaekMeasureAfter"].ToString(),
                                MeasurLocation = dr["MeasurLocation"].ToString(),
                                OilTemper = dr["OilTemper"].ToString(),

                                JobID = dr["JobID"].ToString(),
                                SetDateTime = dr["SetDateTime"].ToString(),
                                TotalWorkQty = dr["TotalWorkQty"].ToString(),
                                WorkComments = dr["WorkComments"].ToString(),
                                BuyerArticleID = dr["BuyerArticleID"].ToString(),
                                Jakup1CutLength = dr["Jakup1CutLength"].ToString(),
                                Jakup1SetQty = dr["Jakup1SetQty"].ToString(),
                                Jakup1WorkQty = dr["Jakup1WorkQty"].ToString(),
                                Jakup2CutLength = dr["Jakup2CutLength"].ToString(),
                                Jakup2SetQty = dr["Jakup2SetQty"].ToString(),
                                Jakup2WorkQty = dr["Jakup2WorkQty"].ToString(),
                                Jakup3CutLength = dr["Jakup3CutLength"].ToString(),
                                Jakup3SetQty = dr["Jakup3SetQty"].ToString(),
                                Jakup3WorkQty = dr["Jakup3WorkQty"].ToString(),
                                Jakup4CutLength = dr["Jakup4CutLength"].ToString(),
                                Jakup4SetQty = dr["Jakup4SetQty"].ToString(),
                                Jakup4WorkQty = dr["Jakup4WorkQty"].ToString(),
                                Jakup5CutLength = dr["Jakup5CutLength"].ToString(),
                                Jakup5SetQty = dr["Jakup5SetQty"].ToString(),
                                Jakup5WorkQty = dr["Jakup5WorkQty"].ToString(),
                                Cavity = dr["Cavity"].ToString(),
                                CycleTime = dr["CycleTime"].ToString(),
                                BoAp1Time = dr["BoAp1Time"].ToString(),
                                BoAp2Time = dr["BoAp2Time"].ToString(),
                                BoAp3Time = dr["BoAp3Time"].ToString(),
                                MeasurTime = dr["MeasurTime"].ToString(),
                                MeasurDelyTime = dr["MeasurDelyTime"].ToString(),
                                HangJungTime = dr["HangJungTime"].ToString(),
                                Sachul1Pressure = dr["Sachul1Pressure"].ToString(),
                                Sachul2Pressure = dr["Sachul2Pressure"].ToString(),
                                Sachul3Pressure = dr["Sachul3Pressure"].ToString(),
                                Sachul4Pressure = dr["Sachul4Pressure"].ToString(),
                                Sachul5Pressure = dr["Sachul5Pressure"].ToString(),
                                BoAp1Pressure = dr["BoAp1Pressure"].ToString(),
                                BoAp2Pressure = dr["BoAp2Pressure"].ToString(),
                                BoAp3Pressure = dr["BoAp3Pressure"].ToString(),
                                Measur1BaeAp = dr["Measur1BaeAp"].ToString(),
                                Measur2BaeAp = dr["Measur2BaeAp"].ToString(),
                                Sachul1Speed = dr["Sachul1Speed"].ToString(),
                                Sachul2Speed = dr["Sachul2Speed"].ToString(),
                                Sachul3Speed = dr["Sachul3Speed"].ToString(),
                                Sachul4Speed = dr["Sachul4Speed"].ToString(),
                                Sachul5Speed = dr["Sachul5Speed"].ToString(),
                                BoAp1Speed = dr["BoAp1Speed"].ToString(),
                                BoAp2Speed = dr["BoAp2Speed"].ToString(),
                                Measur1Speed = dr["Measur1Speed"].ToString(),
                                Measur2Speed = dr["Measur2Speed"].ToString(),
                                Measur3Speed = dr["Measur3Speed"].ToString(),
                                BaeAp1 = dr["BaeAp1"].ToString(),
                                BaeAp2 = dr["BaeAp2"].ToString(),
                                BaeAp3 = dr["BaeAp3"].ToString(),
                                Sachul1JulHWan = dr["Sachul1JulHWan"].ToString(),
                                Sachul2JulHWan = dr["Sachul2JulHWan"].ToString(),
                                Sachul3JulHWan = dr["Sachul3JulHWan"].ToString(),
                                Sachul1Location = dr["Sachul1Location"].ToString(),
                                Sachul2Location = dr["Sachul2Location"].ToString(),
                                Sachul3Location = dr["Sachul3Location"].ToString(),
                                Sachul4Location = dr["Sachul4Location"].ToString(),
                                Sachul5Location = dr["Sachul5Location"].ToString(),
                                BoApLocation = dr["BoApLocation"].ToString(),
                                CushunLocation = dr["CushunLocation"].ToString(),
                                Measur1Location = dr["Measur1Location"].ToString(),
                                Measur2Location = dr["Measur2Location"].ToString(),
                                Measur3Location = dr["Measur3Location"].ToString(),
                                FlowPreventLocation = dr["FlowPreventLocation"].ToString(),
                                CushunLowLimit = dr["CushunLowLimit"].ToString(),
                                CushunMaxLimit = dr["CushunMaxLimit"].ToString(),
                                NozzleTemper = dr["NozzleTemper"].ToString(),
                                ClinderHESetTemper = dr["ClinderHESetTemper"].ToString(),
                                ClinderHNZSetTemper = dr["ClinderHNZSetTemper"].ToString(),
                                Clinder1SetTemper = dr["Clinder1SetTemper"].ToString(),
                                Clinder2SetTemper = dr["Clinder2SetTemper"].ToString(),
                                Clinder3SetTemper = dr["Clinder3SetTemper"].ToString(),
                                Clinder4SetTemper = dr["Clinder4SetTemper"].ToString(),
                                Clinder5SetTemper = dr["Clinder5SetTemper"].ToString(),
                                ClinderHETemper = dr["ClinderHETemper"].ToString(),
                                ClinderHNZTemper = dr["ClinderHNZTemper"].ToString(),
                                Clinder1Temper = dr["Clinder1Temper"].ToString(),
                                Clinder2Temper = dr["Clinder2Temper"].ToString(),
                                Clinder3Temper = dr["Clinder3Temper"].ToString(),
                                Clinder4Temper = dr["Clinder4Temper"].ToString(),
                                Clinder5Temper = dr["Clinder5Temper"].ToString(),
                                CoolWaterInTemper = dr["CoolWaterInTemper"].ToString(),
                                CoolWaterOutTemper = dr["CoolWaterOutTemper"].ToString(),
                                CoolWaterTemper = dr["CoolWaterTemper"].ToString(),
                                MachineSate = dr["MachineSate"].ToString(),
                                MachineRunState = dr["MachineRunState"].ToString(),
                                DefectYN = dr["DefectYN"].ToString(),
                                LOTID = dr["LOTID"].ToString(),
                                Reset = dr["Reset"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                CreateDate = dr["CreateDate"].ToString(),
                                CreateUserID = dr["CreateUserID"].ToString(),
                                UpdateDate = dr["UpdateDate"].ToString(),
                                UpdateUserID = dr["UpdateUserID"].ToString(),
                            };

                            dgdMain.Items.Add(WinR);
                        }

                        tblCnt.Text = "▶검색 결과 : " + i.ToString() + "건";
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

        #endregion // 주요 메서드 - 조회 FillGrid

        private string StartTimeAndEndTime(string SDate, string STime, string EDate, string ETime)
        {
            string STandET = string.Empty;
            
            STandET += STime.Substring(0, 2) + ":" + STime.Substring(2, 2) + " ~ ";
            STandET += ETime.Substring(0, 2) + ":" + ETime.Substring(2, 2);

            return STandET;
        }


        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N1}", obj);
        }

        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        private string stringFormatNN(object obj, int length)
        {
            return string.Format("{0:N" + length + "}", obj);
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

        // Int로 변환
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

        // 소수로 변환 가능한지 체크 이벤트
        private bool CheckConvertDouble(string str)
        {
            bool flag = false;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                if (Double.TryParse(str, out chkDouble) == true)
                {
                    flag = true;
                }
            }

            return flag;
        }

        // 숫자로 변환 가능한지 체크 이벤트
        private bool CheckConvertInt(string str)
        {
            bool flag = false;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Trim().Replace(",", "");

                if (Int32.TryParse(str, out chkInt) == true)
                {
                    flag = true;
                }
            }

            return flag;
        }

        // 소수로 변환
        private double ConvertDouble(string str)
        {
            double result = 0;
            double chkDouble = 0;

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

        // 시간 : 분 으로 변환
        private string ConvertTimeFormat(string str)
        {
            string result = "";

            str = str.Trim().Replace(":", "");
            if (str.Length > 5)
            {
                string hour = str.Substring(0, 2);
                string min = str.Substring(2, 2);
                string sec = str.Substring(4, 2);

                result = hour + ":" + min;
            }
            else if (str.Length > 3 && str.Length < 5)
            {
                string hour = str.Substring(0, 2);
                string min = str.Substring(2, 2);

                result = hour + ":" + min;
            }

            return result;
        }

        #endregion

    }

    class Win_Prd_WorkLog_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }
        public string Num { get; set; }
        public string LogID { get; set; }
        public string WorkDate { get; set; }
        public string WorkDate_CV { get; set; }
        public string WorkTime { get; set; }
        public string WorkTime_CV { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public double WorkQty { get; set; }
        public double DefectQty { get; set; }
        public string MachineID { get; set; }
        public string MachineNo { get; set; }
        public string SachulPressure { get; set; }
        public string SachulSpeed { get; set; }
        public string SachulLocation { get; set; }
        public string BoApPressure { get; set; }
        public string BoApSpeed { get; set; }
        public string BoApTime { get; set; }
        public string Cushun { get; set; }
        public string SachulTime { get; set; }
        public string CoolingTime { get; set; }
        public string MeasurSpeed { get; set; }
        public string BaeAp { get; set; }
        public string SukBaekMeasureBefore { get; set; }
        public string SukBaekMeasureAfter { get; set; }
        public string MeasurLocation { get; set; }
        public string OilTemper { get; set; }
        public string Machine { get; set; }
        public string StationNO { get; set; }
        public string JobID { get; set; }
        public string SetDateTime { get; set; }
        public string TotalWorkQty { get; set; }
        public string WorkComments { get; set; }
        public string BuyerArticleID { get; set; }
        public string Jakup1CutLength { get; set; }
        public string Jakup1SetQty { get; set; }
        public string Jakup1WorkQty { get; set; }
        public string Jakup2CutLength { get; set; }
        public string Jakup2SetQty { get; set; }
        public string Jakup2WorkQty { get; set; }
        public string Jakup3CutLength { get; set; }
        public string Jakup3SetQty { get; set; }
        public string Jakup3WorkQty { get; set; }
        public string Jakup4CutLength { get; set; }
        public string Jakup4SetQty { get; set; }
        public string Jakup4WorkQty { get; set; }
        public string Jakup5CutLength { get; set; }
        public string Jakup5SetQty { get; set; }
        public string Jakup5WorkQty { get; set; }
        public string Cavity { get; set; }
        public string CycleTime { get; set; }
        public string BoAp1Time { get; set; }
        public string BoAp2Time { get; set; }
        public string BoAp3Time { get; set; }
        public string MeasurTime { get; set; }
        public string MeasurDelyTime { get; set; }
        public string HangJungTime { get; set; }
        public string Sachul1Pressure { get; set; }
        public string Sachul2Pressure { get; set; }
        public string Sachul3Pressure { get; set; }
        public string Sachul4Pressure { get; set; }
        public string Sachul5Pressure { get; set; }
        public string BoAp1Pressure { get; set; }
        public string BoAp2Pressure { get; set; }
        public string BoAp3Pressure { get; set; }
        public string Measur1BaeAp { get; set; }
        public string Measur2BaeAp { get; set; }
        public string Sachul1Speed { get; set; }
        public string Sachul2Speed { get; set; }
        public string Sachul3Speed { get; set; }
        public string Sachul4Speed { get; set; }
        public string Sachul5Speed { get; set; }
        public string BoAp1Speed { get; set; }
        public string BoAp2Speed { get; set; }
        public string Measur1Speed { get; set; }
        public string Measur2Speed { get; set; }
        public string Measur3Speed { get; set; }
        public string BaeAp1 { get; set; }
        public string BaeAp2 { get; set; }
        public string BaeAp3 { get; set; }
        public string Sachul1JulHWan { get; set; }
        public string Sachul2JulHWan { get; set; }
        public string Sachul3JulHWan { get; set; }
        public string Sachul1Location { get; set; }
        public string Sachul2Location { get; set; }
        public string Sachul3Location { get; set; }
        public string Sachul4Location { get; set; }
        public string Sachul5Location { get; set; }
        public string BoApLocation { get; set; }
        public string CushunLocation { get; set; }
        public string Measur1Location { get; set; }
        public string Measur2Location { get; set; }
        public string Measur3Location { get; set; }
        public string FlowPreventLocation { get; set; }
        public string CushunLowLimit { get; set; }
        public string CushunMaxLimit { get; set; }
        public string NozzleTemper { get; set; }
        public string ClinderHESetTemper { get; set; }
        public string ClinderHNZSetTemper { get; set; }
        public string Clinder1SetTemper { get; set; }
        public string Clinder2SetTemper { get; set; }
        public string Clinder3SetTemper { get; set; }
        public string Clinder4SetTemper { get; set; }
        public string Clinder5SetTemper { get; set; }
        public string ClinderHETemper { get; set; }
        public string ClinderHNZTemper { get; set; }
        public string Clinder1Temper { get; set; }
        public string Clinder2Temper { get; set; }
        public string Clinder3Temper { get; set; }
        public string Clinder4Temper { get; set; }
        public string Clinder5Temper { get; set; }
        public string CoolWaterInTemper { get; set; }
        public string CoolWaterOutTemper { get; set; }
        public string CoolWaterTemper { get; set; }
        public string MachineSate { get; set; }
        public string MachineRunState { get; set; }
        public string DefectYN { get; set; }
        public string LOTID { get; set; }
        public string Reset { get; set; }
        public string Comments { get; set; }
        public string CreateDate { get; set; }
        public string CreateUserID { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateUserID { get; set; }



    }


}
