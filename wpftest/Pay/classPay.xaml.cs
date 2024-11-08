
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
using WizMes_Nadaum.PopUp;
using WizMes_Nadaum.PopUP;

namespace WizMes_Nadaum
{
    /// <summary>
    /// Win_Prd_ProcessResult_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class classPay : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;
        string strFlag = string.Empty;
        int rowNum = 0;
        Pay pay = new Pay();

        
        public classPay()
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
                    dtpSDate.SelectedDate = LastMonth31;
                }
                else
                {
                    DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                    DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                    DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                    dtpSDate.SelectedDate = LastMonth1;
                    dtpSDate.SelectedDate = LastMonth31;
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

        //업체명
        private void lblCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCustomSrh.IsChecked == true)
            {
                chkCustomSrh.IsChecked = false;
            }
            else
            {
                chkCustomSrh.IsChecked = true;
            }
        }

        private void chkCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = true;
            btnPfCustomSrh.IsEnabled = true;
            txtCustom.Focus();
        }

        private void chkCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = false;
            btnPfCustomSrh.IsEnabled = false;
        }

        private void txtCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        private void btnPfCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // 프로그램 명 
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = false;
            }
            else
            {
                chkArticleSrh.IsChecked = true;
            }
        }

        private void chkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
            btnPfArticleSrh.IsEnabled = true;
            txtArticleSrh.Focus();
        }

        private void chkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
            btnPfArticleSrh.IsEnabled = false;
        }

        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh, (int)Defind_CodeFind.DCF_Article, "");
            }
        }

        private void btnPfArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, (int)Defind_CodeFind.DCF_Article, "");
        }

        // 강사명 
        private void lblPersonSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPersonSrh.IsChecked == true)
            {
                chkPersonSrh.IsChecked = false;
            }
            else
            {
                chkPersonSrh.IsChecked = true;
            }
        }

        private void chkPersonSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtPersonSrh.IsEnabled = true;
            btnPfPersonSrh.IsEnabled = true;
            txtPersonSrh.Focus();
        }

        private void chkPersonSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPersonSrh.IsEnabled = false;
            btnPfPersonSrh.IsEnabled = false;
        }

        private void txtPersonSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtPersonSrh, (int)Defind_CodeFind.DCF_PERSON, "");
            }
        }

        private void btnPfPersonSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtPersonSrh, (int)Defind_CodeFind.DCF_PERSON, "");
        }




        #endregion

        #region 버튼 
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "U";
            updateMode();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (pay == null)
            {
                MessageBox.Show("삭제할 데이터를 지정해주세요");
                return;
            }

            if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DeleteData(pay))
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

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            defaltMode();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<PaySub> lstSub = new List<PaySub>();

            for (int i = 0; i < dgdSub.Items.Count; i++)
            {
                var sub = dgdSub.Items[i] as PaySub;
                lstSub.Add(sub);
            }
            if (UpdateData(pay, lstSub)) defaltMode();
        }

        //강의료 계산 
        private void btnCount_Click(object sender, RoutedEventArgs e)
        {
            if (pay != null)
            {
                strFlag = "I";
                popCount.IsOpen = true;
            } else
            {
                MessageBox.Show("정산 할 계약이 선택되지 않았습니다");
            }
        }
        private void popBtnOK_Click(object sender, RoutedEventArgs e)
        {
            popCount.IsOpen = false;

            using (Loading ld = new Loading(Pay))
            {
                ld.ShowDialog();
            }

        }
        private void popBtnCancel_Click(object sender, RoutedEventArgs e)
        {
            popCount.IsOpen = false;
        }
        private void popCount_Opend(object sender, EventArgs e)
        {
            dtpPopStartDate.SelectedDate = DateTime.Today;
            dtpPopEndDate.SelectedDate = DateTime.Today;
        }
        private void Pay()
        {
            string sDate = dtpPopStartDate.SelectedDate.Value.ToString("yyyyMMdd");
            string eDate = dtpPopEndDate.SelectedDate.Value.ToString("yyyyMMdd");

            List<String> lstOrderID = getOrder(sDate, eDate);

            for(int i=0 ; i < lstOrderID.Count; i++)
            {
                String orderID = lstOrderID[i];
                List<PaySub> lstSub = calculate(lstOrderID[i]);

                if(lstSub.Count > 0)
                {
                    if (SaveData(orderID, lstSub))
                    {
                        defaltMode();
                        btnSearch_Click(null, null);

                    } else
                    {
                        MessageBox.Show("정산 할 데이터가 없습니다. 사원조회 , 강의료 마스터 확인 필요");
                    
                    }
                }
            }

        }

        private List<String> getOrder(String sDate, String eDate)
        {
            List<String> strlst = new List<String>();

            string sql = "select orderID from [order] where orderDate between "; 
            sql += "'" + sDate + "'" + " and " + "'" + eDate + "'";

            DataSet ds = DataStore.Instance.QueryToDataSet(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                int i = 0;

                if (dt.Rows.Count > 0) {
                    DataRowCollection drc = dt.Rows;
                    foreach(DataRow dr in drc)
                    {
                        strlst.Add(dr[i].ToString());
                        i++;
                    }
                }
            }

            return strlst;

        }

        private List<PaySub> calculate(String orderID)
        {
            List<PaySub> lst = new List<PaySub>();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("orderID", orderID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_DcntFees_calDcntFees", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var paySub = new PaySub()
                            {
                                lectureSubSeq = Convert.ToInt32(dr["lectureSubSeq"]),
                                studSeq = Convert.ToInt32(dr["studSeq"]),
                                lecturerPersonID = dr["lecturerPersonID"].ToString(),
                                classth = dr["classth"].ToString(),
                                docentFees = Convert.ToDouble(dr["docentFees"]),
                                leadExtraPay = Convert.ToDouble((dr["leadExtraPay"])),
                                longdistExtraPay = Convert.ToDouble(dr["longdistExtraPay"]),
                                outcityExtraPay = Convert.ToDouble(dr["outcityExtraPay"]),
                                etcExtraPay = Convert.ToDouble(dr["etcExtraPay"]),
                                totalAmount = Convert.ToDouble(dr["totalAmount"]),
                            };

                            lst.Add(paySub);
                        }

                        return lst;
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

            return lst;
        }
        #endregion

        #region UI MODE
        private void updateMode()
        {
            btnCount.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            btnExcel.IsEnabled = false;

            btnAddRow.IsEnabled = true;
            btnAddDel.IsEnabled = true;

            lblMsg.Visibility = Visibility.Visible;
            grdInput.IsHitTestVisible = true;
            dgdMain.IsHitTestVisible = false;
        }

        private void defaltMode()
        {
            btnCount.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = true;
            btnClose.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnSave.Visibility = Visibility.Hidden; ;
            btnCancel.Visibility = Visibility.Hidden; ;
            btnExcel.IsEnabled = false;

            btnAddRow.IsEnabled = false;
            btnAddDel.IsEnabled = false;

            lblMsg.Visibility = Visibility.Hidden;
            grdInput.IsHitTestVisible = false;
            dgdMain.IsHitTestVisible = true;
        }
        #endregion

        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("chkDate", chkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sDate", chkDateSrh.IsChecked == true && dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("eDate", chkDateSrh.IsChecked == true && dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("chkCustom", chkCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("customID", chkCustomSrh.IsChecked == true && txtCustomSrh.Tag.ToString() != null ? txtCustomSrh.Tag.ToString() : "");

                sqlParameter.Add("chkArticle", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("articleID", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("chkPerson", chkPersonSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("personID", chkPersonSrh.IsChecked == true ? 1 : 0);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_DcntFees_sDcntFees", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var pay = new Pay()
                            {
                                num = i,
                                orderDcntFeesID = dr["orderDcntFeesID"].ToString(),
                                orderID = dr["orderID"].ToString(),
                                orderNo = dr["orderNo"].ToString(),
                                article = dr["article"].ToString(),
                                kCustom = dr["kCustom"].ToString(),
                                name = dr["name"].ToString(),
                                jobFromDate = dr["jobFromDate"].ToString(),
                                jobToDate = dr["jobToDate"].ToString(),
                                dcntFeesCalDate = dr["dcntFeesCalDate"].ToString(),
                                comments = dr["comments"].ToString(),
                            };

                            pay.dcntFeesCalDate = DatePickerFormat(pay.dcntFeesCalDate);
                            pay.jobFromDate = DatePickerFormat(pay.jobFromDate);
                            pay.jobToDate = DatePickerFormat(pay.jobToDate);

                            dgdMain.Items.Add(pay);
                        }
                        txtSearchCount.Text = "▶ 검색결과 : " + i.ToString() + " 건";
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

        private void FillGridSub(String orderDcntFeesID)
        {
            if (dgdSub.Items.Count > 0)
            {
                dgdSub.Items.Clear();
            }
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("orderDcntFeesID", orderDcntFeesID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_DcntFees_sDcntFeesSub", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var paySub = new PaySub()
                            {
                                num = i,
                                orderDcntFeesID = dr["orderDcntFeesID"].ToString(),
                                orderDcntFeesSeq = Convert.ToInt32(dr["orderDcntFeesSeq"]),
                                lecturerPersonID = dr["lecturerPersonID"].ToString(),
                                name = dr["name"].ToString(),
                                position = dr["position"].ToString(),
                                dutyDTL = dr["dutyDTL"].ToString(),
                                classth = dr["classth"].ToString(),
                                docentFees = Convert.ToDouble(dr["docentFees"]),
                                leadExtraPay = Convert.ToDouble((dr["leadExtraPay"])),
                                longdistExtraPay = Convert.ToDouble(dr["longdistExtraPay"]),
                                outcityExtraPay = Convert.ToDouble(dr["outcityExtraPay"]),
                                etcExtraPay = Convert.ToDouble(dr["etcExtraPay"]),
                                etcAmount = Convert.ToDouble(dr["etcAmount"]),
                                totalAmount = Convert.ToDouble(dr["totalAmount"]),
                                comments = dr["comments"].ToString(),
                                lectureSubSeq = Convert.ToInt32(dr["lectureSubSeq"]),
                                studSeq = Convert.ToInt32(dr["studSeq"]),
                            };

                            dgdSub.Items.Add(paySub);

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

        private bool SaveData(String orderID, List<PaySub> lstSub)
        {
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
          
            String orderDcntFeesID = string.Empty;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderDcntFeesID", "" );
                sqlParameter.Add("dcntFeesCalDate", dtpFeeDate.SelectedDate != null ? dtpFeeDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("comments", txtComments.Text ?? "");
                sqlParameter.Add("createUserID", MainWindow.CurrentUser);
                sqlParameter.Add("orderID", orderID);

                Procedure pro = new Procedure();
                pro.Name = "xp_DcntFees_iDcntFees";
                pro.OutputUseYN = "Y";
                pro.OutputName = "orderDcntFeesID";
                pro.OutputLength = "10";
             
                Prolist.Add(pro);
                ListParameter.Add(sqlParameter);

                List<KeyValue> list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

                if (list_Result[0].key.ToLower() == "success")
                {
                    for (int i = 0; i < list_Result.Count; i++)
                    {
                        KeyValue kv = list_Result[i];
                        if (kv.key == "orderDcntFeesID")
                        {
                            orderDcntFeesID = kv.value;
                        } 
                    }

                    SaveSub(orderDcntFeesID, lstSub);
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

        private void SaveSub(String orderDcntFeesID, List<PaySub> lstSub)
        {
            if (String.IsNullOrEmpty(orderDcntFeesID)) return; 

            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            for (int i = 0; i < lstSub.Count; i++)
            {
                var paySub = lstSub[i];

                Procedure pro = new Procedure();
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("orderDcntFeesID", orderDcntFeesID);
                sqlParameter.Add("orderDcntFeesSeq", i + 1);
                sqlParameter.Add("docentFees", paySub.docentFees);
                sqlParameter.Add("leadExtraPay", paySub.leadExtraPay);
                sqlParameter.Add("longdistExtraPay", paySub.longdistExtraPay);
                sqlParameter.Add("outcityExtraPay", paySub.outcityExtraPay);
                sqlParameter.Add("etcExtraPay", paySub.etcExtraPay);
                sqlParameter.Add("etcAmount", paySub.etcExtraPay);
                sqlParameter.Add("totalAmount", paySub.totalAmount);
                sqlParameter.Add("comments", paySub.comments ?? "");
                sqlParameter.Add("lectureSubSeq", paySub.lectureSubSeq);
                sqlParameter.Add("studSeq", paySub.studSeq);
                sqlParameter.Add("lecturerPersonID", paySub.lecturerPersonID);
                sqlParameter.Add("createUserID", MainWindow.CurrentUser);

                pro.Name = "xp_DcntFees_iDcntFeesSub";
                pro.OutputUseYN = "N";
                pro.OutputName = "InstID";
                pro.OutputLength = "12";
                
                Prolist.Add(pro);
                ListParameter.Add(sqlParameter);
            }

            List<KeyValue> list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

            if (!list_Result[0].key.ToLower().Equals("success"))
            {
                MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
            }

            
        }

        private bool UpdateData(Pay pay, List<PaySub> lstSub)
        {
            try
            {
                List<Procedure> Prolist = new List<Procedure>();
                List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
                Procedure pro = new Procedure();
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                sqlParameter.Clear();
                sqlParameter.Add("orderDcntFeesID", pay.orderDcntFeesID);
                sqlParameter.Add("dcntFeesCalDate", dtpFeeDate.SelectedDate != null ? dtpFeeDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("comments", txtComments.Text ?? "");
                sqlParameter.Add("lastUpdateUserID", MainWindow.CurrentUser);

                pro.Name = "xp_DcntFees_uDcntFees";
                pro.OutputUseYN = "N";
                pro.OutputName = "orderDcntFeesID";
                pro.OutputLength = "10";
                Prolist.Add(pro);
                ListParameter.Add(sqlParameter);

                List<KeyValue> list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");

                if (list_Result[0].key.ToLower() == "success")
                {
                    SaveSub(pay.orderDcntFeesID, lstSub);
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

        private Boolean DeleteData(Pay pay)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("orderDcntFeesID", pay.orderDcntFeesID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_DcntFees_dDcntFees", sqlParameter, "D");
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
            bindData();
        }

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            bindData();
        }

        private void bindData()
        {
            pay = dgdMain.SelectedItem as Pay;
            if (pay != null)
            {
                btnUpdate.IsEnabled = true;
                this.DataContext = pay;
                FillGridSub(pay.orderDcntFeesID);
            }
        }

        private void dgdMain_SizeChange(object sender, SizeChangedEventArgs e)
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
        #endregion

        #region 서브그리드 이벤트 

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            dgdSub.Items.Add(new PaySub
            {
                num = dgdSub.Items.Count + 1,
                orderDcntFeesID = "",
                lectureSubSeq = 0,
                studSeq = 0,
                lecturerPersonID = "",
                name = "",
                position = "",
                dutyDTL = "",
                dutyDTLID = "",
                classth = "",
                docentFees = 0,
                leadExtraPay = 0,
                longdistExtraPay = 0,
                outcityExtraPay = 0,
                etcExtraPay = 0,
                etcAmount = 0,
                totalAmount = 0,
                comments = ""
            });
        }

        private void btnAddDel_Click(object sender, RoutedEventArgs e)
        {
            int i = dgdSub.SelectedIndex; 
            if(i > -1)
            {
                dgdSub.Items.RemoveAt(i);
            } else
            {
                MessageBox.Show("삭제할 행을 선택해주세요");
            }
        }

        private void DataGridSub_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    DataGridSub_KeyDown(sender, e);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_PreviewKeyDown " + ee.ToString());
            }
        }

        private void DataGridSub_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var SubItem = dgdSub.CurrentItem as Win_ord_OutWare_Scan_Sub_CodeView;
                int rowCount = dgdSub.Items.IndexOf(dgdSub.CurrentItem);
                int colCount = dgdSub.Columns.IndexOf(dgdSub.CurrentCell.Column);
                int StartColumnCount = 1; 
                int EndColumnCount = 7; 

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount == colCount && dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.SelectedIndex = rowCount + 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[StartColumnCount]);
                    }
                    else if (EndColumnCount > colCount && dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount && dgdSub.Items.Count - 1 == rowCount)
                    {
                        btnSave.Focus();
                    }
                    else if (EndColumnCount > colCount && dgdSub.Items.Count - 1 == rowCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else
                    {

                    }
                }
                else if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.SelectedIndex = rowCount + 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[colCount]);
                    }
                    else if (dgdSub.Items.Count - 1 == rowCount)
                    {
                        if (EndColumnCount > colCount)
                        {
                            dgdSub.SelectedIndex = 0;
                            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[0], dgdSub.Columns[colCount + 1]);
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
                        dgdSub.SelectedIndex = rowCount - 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount - 1], dgdSub.Columns[colCount]);
                    }
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (colCount > 0)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount - 1]);
                    }
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (EndColumnCount > colCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else if (EndColumnCount == colCount)
                    {
                        if (dgdSub.Items.Count - 1 > rowCount)
                        {
                            dgdSub.SelectedIndex = rowCount + 1;
                            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[StartColumnCount]);
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

        private void DataGridSub_TextFocus(object sender, KeyEventArgs e)
        {
            try
            {
                Lib.Instance.DataGridINControlFocus(sender, e);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_TextFocus " + ee.ToString());
            }
        }

        private void DataGridSub_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_GotFocus " + ee.ToString());
            }
        }

        private void DataGridSub_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Lib.Instance.DataGridINBothByMouseUP(sender, e);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridSub_MouseUp " + ee.ToString());
            }
        }


        #endregion

        #region 기타 메서드 
        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            str = str.Trim().Replace("-", "").Replace(".", "");

            if (!str.Equals(""))
            {
                if (str.Length == 8)
                {
                    str = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
                }
                else if (str.Length == 7)
                {
                    str = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-0" + str.Substring(6, 1);
                }
                else if (str.Length == 6)
                {
                    str = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-01";
                }
                else if (str.Length == 5)
                {
                    str = str.Substring(0, 4) + "-0" + str.Substring(4, 1) + "-01";
                }
                else if (str.Length == 4)
                {
                    str = DateTime.Today.ToString("yyyy") + "-" + str.Substring(0, 2) + "-" + str.Substring(2, 2);
                }
            }

            return str;
        }



        #endregion

       
    }

    class Pay
    {
       public int num { get; set; }
       public String orderDcntFeesID { get; set; }
       public String orderID { get; set; }
       public String orderNo { get; set; }
       public String article { get; set; }
       public String kCustom { get; set; }
       public String name { get; set; }
       public String jobFromDate { get; set; } // 계약일자 
       public String jobToDate { get; set; } // 계약일자 
       public String dcntFeesCalDate { get; set; } // 정산일자 
       public double totalAmount { get; set; } // 정산일자 
       public String comments { get; set; }
   
    }

    class PaySub
    {
        public int num { get; set; }
        public String orderDcntFeesID { get; set; }
        public int orderDcntFeesSeq { get; set; }
        public int lectureSubSeq { get; set; }
        public int studSeq { get; set; }
        public String lecturerPersonID { get; set; }
        public String name { get; set; }
        public String position { get; set; }
        public String positionID { get; set; }
        public String dutyDTL { get; set; }
        public String dutyDTLID { get; set; }
        public String classth { get; set; }
        public double docentFees { get; set; }
        public double leadExtraPay { get; set; }
        public double longdistExtraPay { get; set; }
        public double outcityExtraPay { get; set; }
        public double etcExtraPay { get; set; }
        public double etcAmount { get; set; }
        public double totalAmount { get; set; }
        public String comments { get; set; }

    }


}