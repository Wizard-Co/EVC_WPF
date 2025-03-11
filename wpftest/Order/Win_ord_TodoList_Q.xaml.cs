using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_EVC.PopUP;
using WizMes_EVC.PopUp;
using WPF.MDI;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Security.RightsManagement;
using System.Linq;
using System.Windows.Controls.Primitives;


namespace WizMes_EVC
{
    /// <summary>
    /// Win_ord_TodoList_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_TodoList_Q : UserControl
    {


        public Win_ord_TodoList_Q()
        {
            InitializeComponent();
            scrollHelpers = new ScrollSyncHelper(dgdMainHeaderSh, dgdMain);
            this.GotFocus += Win_ord_TodoList_Q_GotFocus;
        }
        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        string stDate = string.Empty;
        string stTime = string.Empty;

        // 엑셀 활용 용도 (프린트)
        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;
        WizMes_EVC.PopUp.NoticeMessage msg = new PopUp.NoticeMessage();
        DataTable DT;

        string OrderID_global = string.Empty;
        private ScrollSyncHelper scrollHelpers = null;

        //private List<ScrollSyncHelper> scrollHelpers = new List<ScrollSyncHelper>();


        // 첫 로드시.
        private void Win_ord_TodoList_Q_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            //수주일자 체크
            chkInOutDate.IsChecked = true;
            dtpFromDate.SelectedDate = DateTime.Today;
            dtpToDate.SelectedDate = DateTime.Today;

            ComboBoxSetting();
            //제품으로 고정

        }

        private void Win_ord_TodoList_Q_GotFocus(object sender, EventArgs e)
        {
            if (dgdMain.Items.Count > 0 && dgdMain.SelectedIndex != -1)
            {
                var item = dgdMain.SelectedItem as Win_ord_TodoList_Q_View;
                if (item != null)
                {
                    MainWindow.OrderID = item.orderId;
                }
            }
        }

        #region 첫단계 / 날짜버튼 세팅 / 조회용 체크박스 세팅



        // 어제.(전일)
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {


            if (dtpFromDate.SelectedDate != null)
            {
                dtpFromDate.SelectedDate = dtpFromDate.SelectedDate.Value.AddDays(-1);
                dtpToDate.SelectedDate = dtpFromDate.SelectedDate;
            }
            else
            {
                dtpFromDate.SelectedDate = DateTime.Today.AddDays(-1);
                dtpToDate.SelectedDate = DateTime.Today.AddDays(-1);
            }

        }
        // 오늘(금일)
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpFromDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            dtpToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
        }
        // 지난 달(전월)
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            //string[] receiver = lib.BringLastMonthDatetime();

            //dtpFromDate.Text = receiver[0];
            //dtpToDate.Text = receiver[1];

            if (dtpFromDate.SelectedDate != null)
            {
                DateTime ThatMonth1 = dtpFromDate.SelectedDate.Value.AddDays(-(dtpFromDate.SelectedDate.Value.Day - 1)); // 선택한 일자 달의 1일!

                DateTime LastMonth1 = ThatMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThatMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }
            else
            {
                DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }


        }
        // 이번 달(금월)
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            string[] receiver = lib.BringThisMonthDatetime();

            dtpFromDate.Text = receiver[0];
            dtpToDate.Text = receiver[1];
        }

        // 입출일자
        private void chkInOutDate_Click(object sender, RoutedEventArgs e)
        {
            if (chkInOutDate.IsChecked == true)
            {
                dtpFromDate.IsEnabled = true;
                dtpToDate.IsEnabled = true;
            }
            else
            {
                dtpFromDate.IsEnabled = false;
                dtpToDate.IsEnabled = false;
            }
        }
        //입출일자
        private void chkInOutDate_Click(object sender, MouseButtonEventArgs e)
        {
            if (chkInOutDate.IsChecked == true)
            {
                chkInOutDate.IsChecked = false;
                dtpFromDate.IsEnabled = false;
                dtpToDate.IsEnabled = false;
            }
            else
            {
                chkInOutDate.IsChecked = true;
                dtpFromDate.IsEnabled = true;
                dtpToDate.IsEnabled = true;
            }
        }

        #endregion


        #region 콤보박스 세팅
        // 콤보박스 세팅.
        private void ComboBoxSetting()
        {

        }

        #endregion


        #region 조회 , 조회용 프로시저 
        // 조회.
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            using (Loading ld = new Loading(beSearch))
            {
                ld.ShowDialog();
            }
        }

        private void beSearch()
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

        private void FillGrid()
        {
            if(dgdMain.Items.Count > 0) dgdMain.Items.Clear();

            try
            {

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                sqlParameter.Clear();

                sqlParameter.Add("ChkDate", chkInOutDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkInOutDate.IsChecked == true ? dtpFromDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkInOutDate.IsChecked == true ? dtpToDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                //// 거래처
                //sqlParameter.Add("ChkCustom", chkCustom.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("CustomID", chkCustom.IsChecked == true ? (txtCustom.Tag != null ? txtCustom.Tag.ToString() : txtCustom.Text) : "");
                //// 최종고객사
                //sqlParameter.Add("ChkInCustom", chkInCustom.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("InCustomID", chkInCustom.IsChecked == true ? (txtInCustom.Tag != null ? txtInCustom.Tag.ToString() : "") : "");


                //// 품번
                //sqlParameter.Add("ChkArticleID", chkBuyerArticleNo.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ArticleID", chkBuyerArticleNo.IsChecked == true ? (txtBuyerArticleNo.Tag == null ? "" : txtBuyerArticleNo.Tag.ToString()) : "");
                //// 품명
                //sqlParameter.Add("ChkArticle", chkArticle.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("Article", chkArticle.IsChecked == true ? (txtArticle.Text == string.Empty ? "" : txtArticle.Text) : "");

                // 운영사
                sqlParameter.Add("ChkManageCustomId", chkManageCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ManageCustomId", chkManageCustomSrh.IsChecked == true ? (txtManageCustomSrh.Tag != null ? txtManageCustomSrh.Tag.ToString() : "") : "");

                //영업사
                sqlParameter.Add("ChkSalesCustomId", chkSalesCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SalesCustomId", chkSalesCustomSrh.IsChecked == true ? (txtSalesCustomSrh.Tag != null ? txtSalesCustomSrh.Tag.ToString() : "") : "");

                //품번
                sqlParameter.Add("ChkArticleId", chkArticleIdSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleId", chkArticleIdSrh.IsChecked == true ? (chkArticleIdSrh.Tag != null ? chkArticleIdSrh.Tag.ToString() : "") : "");

                sqlParameter.Add("ChkInstallLocation", chkInstallLocationSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InstallLocation", chkInstallLocationSrh.IsChecked == true ? txtInstallLocationSrh.Text : "");

                sqlParameter.Add("ChkCloseYn", chkCloseYN.IsChecked == true ? 1 : 0);

                sqlParameter.Add("ChkKepElecMethod", chkKepDeliMethodSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("KepElecMethod", chkKepDeliMethodSrh.IsChecked == true ? txtKepDeliMethodSrh.Text : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sTodoList", sqlParameter, true, "R");
                DataTable dt = null;

                if (ds != null && ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회결과가 없습니다.");
                        return;
                    }

                    else
                    {

                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow item in drc)
                        {
                            i++;                 
                            var dgdCondition = new Win_ord_TodoList_Q_View() 
                            { 
                                Num = i.ToString(), 
                                orderId = item["orderid"].ToString(),
                                orderno = item["orderno"].ToString(),
                                installLocation = item["installLocation"].ToString(),
                                electrDeliveryMethod = item["electrDeliveryMethod"].ToString(),
                                kepElectrDeliveryMethod = item["kepElectrDeliveryMethod"].ToString(),
                                acptDate = DateTypeHyphen(item["acptDate"].ToString()),
                                saleCustomName = item["saleCustomName"].ToString(),
                                managerCustomName = item["managerCustomName"].ToString(),
                                searchCustomName = item["searchCustomName"].ToString(),
                            };


                            string[] currentDates = {
                               "manageCustomAcptDate",                          //운영사접수일
                               "manageCustomConfirmDate",                       //운영사컨펌완요리
                               "chargeStandInwareDate",                         //충전기입고일 -> 입고일
                               "searchReqDate",                                 //실사요청일
                               "searchDate",                                    //실사일

                               "corpApprovalDate",                              //공단승인일
                               "corpEndDate",                                   //공기마감일
                               "corpLastEndDate",                               //공기최종마감일
                               "localGovBehaviorReportDate",                    //행위신고발급일
                               "kepElectrReqDate",                              //전기사용신청일

                               "kepInApprovalDate",                             //인입승인일   --제거됨
                               "kepPaymentDate",                                //한전불입금납부일
                               "kepMeterInstallDate",                           //계량기부설일
                               "constrDate",                                    //착공일
                               "constrCompleteDate",                            //시공완료일

                               "electrSafeCheckDate",                           //전기안전점검요청일
                               "electrSafeCheckCostPayDate",                    //전기안전점검수수료납부일
                               "electrBeforeUseCheckReqDate",                   //사용전점검요청일
                               "electrBeforeUseCheckPrintDate",                 //사용전검검확인증발급일
                               "electrBeforeInspReqDate",                       //사용전검사 요청일

                               "electrBeforeInspPrintDate",                     //사용전검사 확인증 발급일
                               "electrBeforeInspCostPayDate",                   //사용전검사수수료납부 납부일
                               "superSetTaxPrintDate",                          //감리배치계산서발행
                               "superUseInspReqDate",                           //사용검사신청
                               "superBeforeUseInspDate",                        //사용전 검사일 --제거됨

                               "superBeforeUseInspPrintDate",                   //사용검사 필증 발급일    
                               "compReplyDate",                                 //준공서류회신
                               "suppleCompDate",                                //시공보완완료
                               "compSuppleReportDate",                          //준공서류보완
                               "insurePrintDate",                               //하자보증보험발행

                               "compReportCompDate",                            //준공서류완료
                               "accntMgrWorkPreTaxPrintDate",                   //운영사시공비(선금)
                               "accntMgrSalesPreTaxPrintDate",                  //운영사영업비(선금)
                               "accntWorkPreTaxPrintDate",                      //시공팀(선금)
                               "accntSalesPreTaxPrintDate",                     //영업사원(선금)
                            };

                            //string[] nextDates = {
                            //   "manageCustomConfirmDate",
                            //   "chargeStandInwareDate",
                            //   "searchReqDate",
                            //   "searchDate",
                            //   "corpApprovalDate",
                            //   "corpEndDate",
                            //   "corpLastEndDate",
                            //   "localGovBehaviorReportDate",
                            //   "kepElectrReqDate",
                            //   "kepInApprovalDate",
                            //   "kepPaymentDate",
                            //   "kepMeterInstallDate",
                            //   "constrDate",
                            //   "constrCompleteDate",
                            //   "electrSafeCheckDate",
                            //   "electrSafeCheckCostPayDate",
                            //   "electrBeforeUseCheckReqDate",
                            //   "electrBeforeUseCheckPrintDate",
                            //   "electrBeforeInspReqDate",
                            //   "electrBeforeInspPrintDate",
                            //   "electrBeforeInspCostPayDate",
                            //   "superSetTaxPrintDate",
                            //   "superUseInspReqDate",
                            //   "superBeforeUseInspDate",
                            //   "superBeforeUseInspPrintDate",
                            //   "compReplyDate",
                            //   "suppleCompDate",
                            //   "compSuppleReportDate",
                            //   "insurePrintDate",
                            //   "compReportCompDate",
                            //   "accntMgrWorkPreTaxPrintDate",
                            //   "accntMgrSalesPreTaxPrintDate",
                            //   "accntWorkPreTaxPrintDate",
                            //   "accntSalesPreTaxPrintDate",
                            //};


                            //Win_ord_TodoList_Q_View에서 배열로 넘겨준 이름을 찾아서
                            //true또는 false를 반환
                            //만약 5일이상 다음단계로 미진행시 false를 반환하고 다음행으로 
                            //정상적일 경우 false를 반환할때까지 반복

                            //그냥 일자 있으면 전부 녹색불로 변경 2025.02.11

                            //2025.03.06 김동호
                            //수전방법에따라서 뭘 보여주고 안 보여줘야된다고 함 -> 녹색, 빨간불 자체가 없도록 해야함 -> 클래스 속성값을 null로 처리하면 안 뜸
                            //모자분할(옥내/외) : 전기안전점검수수료납부일 , 전기안전점검요청일 활성화 이외 전기안전공사 정보 & 감리 불필요
                            //한전인입(옥내)수전용량 상관없음: 전기안전공사 진행정보 모두 및 감리 활성화
                            //한전인입(옥외)수전용량 75KW이하: 사용전점검요청일, 사용전점검확인증발급일 활성화 이외 전기안전공사 정보 & 감리 불필요
                            //한전인입(옥외)수전용량 75KW이상: 전기안전공사 진행정보 모두 및 감리 활성화
                            //모자분할(옥내 / 외) + 한전인입(옥내)수전용량 상관없음 : 전기안전공사 진행정보 모두 및 감리 활성화
                            //모자분할(옥내 / 외) + 한전인입(옥외)수전용량 75KW이하: 사용전검사요청일, 사용전검사수수료납부일, 사용전검사확인증발급일&감리 비활성화
                            //모자분할(옥내 / 외) + 한전인입(옥외)수전용량 75KW이상: 전기안전공사 진행정보 모두 및 감리 활성화

                            //기본적으로 적힌 숫자의 곱하기7을 한게 전기용량이라하는데 작성할때 곱하기 한 값을 적도록 하고
                            //서로 차이나는 경우 (모자11 + 한전6) 큰 숫자를 기준으로 함
                            //3월6일 기준으로 적은 내용은 곱하기 7을 하여야 함. 그런데 그렇게하면 헷갈리니 실제 용량만큼 적는다하였음
                            //모자11 + 한전 6 -> 모자77 + 한전 42 (아 용량이 75kw가 넘네) -> 전기안전공사 진행정보 모두 및 감리 활성화

                            //<예외처리>
                            //75이상, 75이하? -> 75미만, 75이상이라함
                            //옥외 옥내 안 적힌 것도 있는데 이런경우 옥내인가?
                            //한전 불입이면 전봇대이기때문에 옥외일 가능성이 많고, 모자분리일때는 옥외 옥내 둘다 가능성이 있다함 -> 그럼 옥내 옥외를 정확하게 적어줘야 합니다
                            //모자분리는 옥내옥외를 가리지 않고, 한전일때만 옥내, 옥외 구분을 하고 용량구분을 함
                            
                            //모자분리, 모자분할.. 이라 적은건 모자만 들어가면 되겠고
                            //한전불입, 인입이라고 적었는데 실제 나뉜건지? 아니면 같은 의미인데 다르게 부르는건지 -> 그냥 같은의미로 알면 된다함                            


                            //for (int j = 0; j < currentDates.Length; j++)
                            //{
                            //    var currentDate = item[currentDates[j]].ToString();
                            //    //var nextDate = item[nextDates[j]].ToString();

                            //    //if (!string.IsNullOrEmpty(currentDate.Trim()) && CheckDays(currentDate, nextDate))
                            //    //모자분할 + 한전인입 
                            //    if (!string.IsNullOrEmpty(currentDate.Trim()))
                            //    {
                            if (dgdCondition.kepElectrDeliveryMethod.Contains("모자"))
                            {                         
                                        
                                if (dgdCondition.kepElectrDeliveryMethod.Contains("한전"))
                                {
                                    int Kepindex = dgdCondition.kepElectrDeliveryMethod.IndexOf("한전");
                                    string KepString = dgdCondition.kepElectrDeliveryMethod.Substring(Kepindex);

                                    string numbers = new string(KepString.Where(char.IsDigit).ToArray());
                                    if (KepString.Contains("옥내"))
                                    {
                                        Condition2(dgdCondition, currentDates, item);
                                    }
                                    else if (KepString.Contains("옥외"))
                                    {
                                        if (numbers != "0" && numbers.Length > 0)
                                        {

                                            //숫자가 있으면 그걸 용량으로 계산
                                            int kiloWatt = Convert.ToInt32(numbers);
                                            if (kiloWatt >= 75)
                                            {
                                                Condition2(dgdCondition, currentDates,item);
                                            }
                                            else if (kiloWatt < 75)
                                            {
                                                Condition4(dgdCondition, currentDates,item);
                                            }
                                        }
                                        if(numbers.Trim() == string.Empty)
                                        {
                                            Condition4(dgdCondition, currentDates,item);
                                        }
                                    }
                                    else
                                    {
                                        dgdCondition.NoKepElectDeliMethodOutOrIn = true;
                                        dgdCondition.NoKepElectDeliMethodOutOrIn_ToolTip = $"수주번호 :{dgdCondition.orderno} : 옥외 또는 옥내 구분이 없습니다.";

                                    }

                                }
                                //모자분할만 있는 경우
                                else if (!dgdCondition.kepElectrDeliveryMethod.Contains("한전"))
                                {
                                    Condition1(dgdCondition, currentDates,item);
                                }

                            }
                            //한전인입 단독
                            else if (dgdCondition.kepElectrDeliveryMethod.Contains("한전"))       
                            {
                                //전기 옥내일 경우 모두 활성화
                                if (dgdCondition.kepElectrDeliveryMethod.Contains("옥내"))
                                {
                                    Condition2(dgdCondition, currentDates,item);
                                }
                                //옥외일경우 용량기준 75kw로 판단
                                else if (dgdCondition.kepElectrDeliveryMethod.Contains("옥외"))
                                {
                                    //글자에서 숫자 추출
                                    string numbers = new string(dgdCondition.kepElectrDeliveryMethod.Where(char.IsDigit).ToArray());
                                    if (numbers != "0" && numbers.Length > 0)
                                    {

                                        //숫자가 있으면 그걸 용량으로 계산
                                        int kiloWatt = Convert.ToInt32(numbers);
                                        if (kiloWatt >= 75)
                                        {
                                            Condition2(dgdCondition, currentDates,item);
                                        }
                                        else if(kiloWatt < 75)
                                        {
                                            Condition3(dgdCondition, currentDates,item);
                                        }
                                    }
                                         
                                }
                                else
                                {
                                    dgdCondition.NoKepElectDeliMethodOutOrIn = true;
                                    dgdCondition.NoKepElectDeliMethodOutOrIn_ToolTip = $"수주번호 :{dgdCondition.orderno} : 옥외 또는 옥내 구분이 없습니다.";
                                }
                            }
                            else
                            {
                                dgdCondition.NoElecDeliMethod = true;
                                dgdCondition.NoElecDeliMethod_ToolTip = $"수주번호 :{dgdCondition.orderno} : 전기조달방법이 입력되지 않았습니다.";
                            }
                                        
                                //    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDates[j])
                                //        ?.SetValue(dgdCondition, true);
                                //    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDates[j] + "_ToolTip")
                                //        ?.SetValue(dgdCondition, DateTypeHyphen(currentDate));
                                //}
                                //else
                                //{
                                //    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDates[j])
                                //        ?.SetValue(dgdCondition, false);                                                         
                                 
                                //}
                            //}                                                 

                            //메인그리드 열 순서에 따라 이미지를 넣기(수동버전)

                            //if (!string.IsNullOrEmpty(item["manageCustomAcptDate"].ToString()) && CheckDays(item["manageCustomAcptDate"].ToString(), item["manageCustomConfirmDate"].ToString()))
                            //{ dgdCondition.manageCustomAcptDate = true; }
                            //else { dgdCondition.manageCustomAcptDate = false; dgdMain.Items.Add(dgdCondition); continue; }

                            //if (!string.IsNullOrEmpty(item["manageCustomConfirmDate"].ToString()) && CheckDays(item["manageCustomConfirmDate"].ToString(), item["chargeStandInwareDate"].ToString()))
                            //{ dgdCondition.manageCustomConfirmDate = true; }
                            //else { dgdCondition.manageCustomConfirmDate = false; dgdMain.Items.Add(dgdCondition); continue; }

                            dgdMain.Items.Add(dgdCondition);

                        }

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
        #endregion

        //한전전기조달방법에 모자분할만 있는 경우("모자"글자가 검출되고 "한전"이 없는 경우)        
        private void Condition1(Win_ord_TodoList_Q_View  dgdCondtion, string[] currentDates, DataRow item)
        {
            for (int i = 0; i < currentDates.Length; i++)
            {
                string currentDate = currentDates[i];               

                if (currentDates[i].ToString() == "electrBeforeUseCheckReqDate" || currentDates[i].ToString() == "electrBeforeUseCheckPrintDate" ||
                    currentDates[i].ToString() == "electrBeforeInspReqDate"     || currentDates[i].ToString() == "electrBeforeInspPrintDate"     ||
                    currentDates[i].ToString() == "electrBeforeInspCostPayDate" || currentDates[i].ToString() == "superSetTaxPrintDate")
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondtion, null);
                }
                else if (!string.IsNullOrWhiteSpace(item[currentDates[i]].ToString().Trim()))
                {
               
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondtion, true);
                    var value = item[currentDate].ToString();
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate + "_ToolTip")
                        ?.SetValue(dgdCondtion, DateTypeHyphen(value));
                }
                else
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                       ?.SetValue(dgdCondtion, false);
                }

            }
        }

        //한전전기조당방법에 한전이면서 옥내일때("모자"글자가 미검출되고 "한전"이 있으면서 "옥내"글자가 있는경우)
        //한전전기조달방법이 한전이면서 옥외, 75KW이상("모자"글자가 미검출되고 "한전"이 있으면서 "옥외"일때 Substring한 글자에 숫자가 검출되어 75이상인경우)
        //한전전기조달방법이 모자+한전이면서 한전인입 방식이 옥내일 때("모자"글자가 검출되고 "한전"이 있으면서 Substring한 글자에 "옥내"가 검출) 
        private void Condition2(Win_ord_TodoList_Q_View dgdCondition, string[] currentDates, DataRow item)
        {
            for (int i = 0; i < currentDates.Length; i++)
            {
                string currentDate = currentDates[i];

                if (!string.IsNullOrWhiteSpace(item[currentDates[i]].ToString().Trim()))
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondition, true);
                    var value = item[currentDate].ToString();
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate + "_ToolTip")
                        ?.SetValue(dgdCondition, DateTypeHyphen(value));
                }
                else
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                       ?.SetValue(dgdCondition, false);
                }
            }
        }

        //한전전기조달방법이 한전이면서 옥외, 75KW미만("모자"글자가 미검출되고 "한전"이 있으면서 "옥외"일때 Substring한 글자에 숫자가 검출되어 75미만 인경우)
        private void Condition3(Win_ord_TodoList_Q_View dgdCondition, string[] currentDates, DataRow item)
        {
            for (int i = 0; i < currentDates.Length; i++)
            {
                string currentDate = currentDates[i];

                if (currentDates[i].ToString() == "electrSafeCheckDate" || currentDates[i].ToString() == "electrSafeCheckCostPayDate" ||
                 currentDates[i].ToString() == "electrBeforeInspReqDate" || currentDates[i].ToString() == "electrBeforeInspPrintDate" ||
                 currentDates[i].ToString() == "electrBeforeInspCostPayDate" || currentDates[i].ToString() == "superSetTaxPrintDate")
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondition, null);
                }
                else if (!string.IsNullOrWhiteSpace(item[currentDates[i]].ToString().Trim()))
                {

                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondition, true);
                    var value = item[currentDate].ToString();
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate + "_ToolTip")
                        ?.SetValue(dgdCondition, DateTypeHyphen(value));
                }
                else
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                       ?.SetValue(dgdCondition, false);
                }
            }
        }

        //한전전기조달방법이 모자+한전이면서 수전용량이 75kw 미만일때("모자"글자가 검출되고 "한전"이 있으면서 Substring한 글자에 "옥외"가 검출되고 숫자도 검출되어 75미만 인경우) 
        private void Condition4(Win_ord_TodoList_Q_View dgdCondition, string[] currentDates, DataRow item)
        {
            for (int i = 0; i < currentDates.Length; i++)
            {
                string currentDate = currentDates[i];

                if (currentDates[i].ToString() == "electrBeforeInspReqDate" || currentDates[i].ToString() == "electrBeforeInspCostPayDate" ||
                 currentDates[i].ToString() == "electrBeforeInspPrintDate" || currentDates[i].ToString() == "superSetTaxPrintDate")
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondition, null);
                }
                else if (!string.IsNullOrWhiteSpace(item[currentDates[i]].ToString().Trim()))
                {

                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                        ?.SetValue(dgdCondition, true);
                    var value = item[currentDate].ToString();
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate + "_ToolTip")
                        ?.SetValue(dgdCondition, DateTypeHyphen(value));
                }
                else
                {
                    typeof(Win_ord_TodoList_Q_View).GetProperty(currentDate)
                       ?.SetValue(dgdCondition, false);
                }
            }
        }

        private bool CheckDays(string date, string nextJobDate)
        {
            bool flag = false;
            DateTime checkDate = DateTime.ParseExact(date, "yyyyMMdd", null);
            DateTime today = DateTime.Today;

            if ((today - checkDate).TotalDays < 5)
            {
                //MessageBox.Show((today - checkDate).TotalDays.ToString());
                flag = true;
            }
            else if((today - checkDate).TotalDays > 5 && !string.IsNullOrEmpty(nextJobDate.Trim()))
            {
                //MessageBox.Show((today - checkDate).TotalDays.ToString());
                flag = true;
            }
            else
            {
                //MessageBox.Show((today - checkDate).TotalDays.ToString());
                flag = false;
            }
          

            return flag;
        }


        // 닫기 버튼클릭.
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            this.GotFocus -= Win_ord_TodoList_Q_GotFocus;

            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.subProgramID.ToString().Contains("MDI"))
                {
                    if (this.ToString().Equals((mvm.subProgramID as MdiChild).Content.ToString()))
                    {
                        (MainWindow.mMenulist[i].subProgramID as MdiChild).Close();
                        break;
                    }
                }
                i++;
            }
        }

        #region 엑셀
        // 엑셀버튼 클릭
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }

            Lib lib3 = new Lib();
            System.Data.DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[4];
            lst[0] = "메인 그리드";
            lst[2] = dgdMain.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    //MessageBox.Show("대분류");
                    //if (ExpExc.Check.Equals("Y"))
                        dt = lib3.DataGridToDTinHidden(dgdMain);
                    //else
                    //    dt = lib3.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;

                    if (lib3.GenerateExcel(dt, Name))
                    {
                        lib3.excel.Visible = true;
                        lib3.ReleaseExcelObject(lib3.excel);
                    }
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }

            lib3 = null;
        }



        #endregion


        #region 플러스 파인더 
        //플러스파인더 _ 거래처_클릭.


        #endregion



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
        }
        //국소명 체크ㄴ
        private void chkInstallLocation_UnChecked(object sender, RoutedEventArgs e)
        {
            chkInstallLocationSrh.IsChecked = false;

            txtInstallLocationSrh.IsEnabled = false;
        }

        ////국소명 엔터
        //private void txtInstallLocation_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        e.Handled = true;
        //        MainWindow.pf.ReturnCode(txtInstallLocationSrh, 76, "");
        //    }
        //}
        ////국소명 pf
        //private void btnInstallLocation_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow.pf.ReturnCode(txtInstallLocationSrh, 76, "");
        //}

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


        //전기수전방법 - 검색-  라벨 - 클릭
        private void lblKepDeliMethodSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkKepDeliMethodSrh.IsChecked == true)
            {
                chkKepDeliMethodSrh.IsChecked = false;
                txtKepDeliMethodSrh.IsEnabled = false;
            }
            else
            {
                chkKepDeliMethodSrh.IsChecked = true;
                txtKepDeliMethodSrh.IsEnabled = true;
            }
        }

        //전기수전방법 - 검색-  체크박스 - 클릭
        private void chkKepDeliMethodSrh_Checked(object sender, RoutedEventArgs e)
        {
            if (chkKepDeliMethodSrh.IsChecked == true)
            {
                chkKepDeliMethodSrh.IsChecked = true;
                txtKepDeliMethodSrh.IsEnabled = true;
            }
            else
            {
                chkKepDeliMethodSrh.IsChecked = false;
                txtKepDeliMethodSrh.IsEnabled = false;
            }
        }

        #endregion


        #region 데이터그리드 스크롤 +  헤더 스크롤 연결 
        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            var dataGridScrollViewer = FindChild<ScrollViewer>(dgdMain);
            if (dataGridScrollViewer != null)
            {
                // DataGrid 스크롤을 헤더 스크롤과 동기화
                dataGridScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);

                //ApplyFrozenColumnsTransform(e.HorizontalOffset);
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
            // DataGrid에 FrozenColumnCount 설정
            dgdMain.FrozenColumnCount = 2;

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

                // 고정 열 헤더 처리
                //ApplyFrozenColumnsTransform(dataGridScrollViewer.HorizontalOffset);
            }
        }


        // 고정 열 헤더에 TranslateTransform 적용
        //private void ApplyFrozenColumnsTransform(double offset)
        //{
        //    var headerGrid = dgdMainHeaderSh.Content as Grid;
        //    if (headerGrid != null)
        //    {
        //        // 각 고정 열 헤더에 TranslateTransform 적용
        //        foreach (int columnIndex in _frozenColumnIndexes)
        //        {
        //            if (columnIndex < headerGrid.Children.Count)
        //            {
        //                var headerElement = headerGrid.Children[columnIndex] as UIElement;
        //                if (headerElement != null)
        //                {
        //                    // TranslateTransform 생성 또는 가져오기
        //                    if (!(headerElement.RenderTransform is TranslateTransform))
        //                    {
        //                        headerElement.RenderTransform = new TranslateTransform();
        //                    }

        //                    var transform = headerElement.RenderTransform as TranslateTransform;
        //                    // 스크롤 오프셋만큼 이동시켜 항상 보이게 함
        //                    transform.X = offset;

        //                    // Z-Index 높게 설정하여 다른 헤더 위에 표시
        //                    Panel.SetZIndex(headerElement, 1000);

        //                    // 배경색 설정하여 항상 보이게 함
        //                    if (headerElement is DataGridColumnHeader header)
        //                    {
        //                        header.Background = new SolidColorBrush(Color.FromRgb(54, 95, 177)); // #365fb1
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion


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


        private void GoOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrderID_global == string.Empty)
                return;

            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("수주등록"))
                {
                    MainWindow.OrderID = OrderID_global;
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

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var OrderInfo = dgdMain.SelectedItem as Win_ord_TodoList_Q_View;
            if(OrderInfo != null)
            {
                OrderID_global = OrderInfo.orderId;
            }
        }

        private void chkColFrozenSrh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkColFrozenSrh.IsChecked == true)
                {
                    scrollHelpers.SetFrozenColumnCount(4);
                }
                else
                {
                 
                    scrollHelpers.SetFrozenColumnCount(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"체크박스 클릭 처리 오류: {ex.Message}");
            }
        }


        private void lblColFrozenSrh_Click(object sender, MouseButtonEventArgs e)
        {
         
            chkColFrozenSrh.IsChecked = !chkColFrozenSrh.IsChecked;

            chkColFrozenSrh_Click(chkColFrozenSrh, null);
        }
    }


    class Win_ord_TodoList_Q_View : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }


        // 조회용
        public string Num { get; set; }
        public string orderId { get; set; }
        public string orderno { get; set; }
        public string installLocation { get; set; }
        public string electrDeliveryMethod { get; set; }
        public string kepElectrDeliveryMethod { get; set; }
        public string CLoseYn { get; set; }
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

        //메인그리드 체크

        public bool? manageCustomAcptDate {get;set;}        //null일때 빈값, true false일때 이미지 출력
        public bool? manageCustomConfirmDate {get;set;}
        public bool? chargeStandInwareDate {get;set;}
        public bool? searchReqDate {get;set;}
        public bool? searchDate {get;set;}
        public bool? corpApprovalDate {get;set;}
        public bool? corpEndDate {get;set;}
        public bool? corpLastEndDate {get;set;}
        public bool? localGovBehaviorReportDate {get;set;}
        public bool? kepElectrReqDate {get;set;}
        public bool? kepInApprovalDate {get;set;}
        public bool? kepPaymentDate {get;set;}
        public bool? kepMeterInstallDate {get;set;}
        public bool? constrDate {get;set;}
        public bool? constrCompleteDate {get;set;}
        public bool? electrSafeCheckDate {get;set;}
        public bool? electrSafeCheckCostPayDate {get;set;}
        public bool? electrBeforeUseCheckReqDate {get;set;}
        public bool? electrBeforeUseCheckPrintDate {get;set;}
        public bool? electrBeforeInspReqDate {get;set;}
        public bool? electrBeforeInspPrintDate {get;set;}
        public bool? electrBeforeInspCostPayDate {get;set;}
        public bool? superSetTaxPrintDate {get;set;}
        public bool? superUseInspReqDate {get;set;}
        public bool? superBeforeUseInspDate {get;set;}
        public bool? superBeforeUseInspPrintDate {get;set;}
        public bool? compReplyDate {get;set;}
        public bool? suppleCompDate {get;set;}
        public bool? compSuppleReportDate {get;set;}
        public bool? insurePrintDate {get;set;}
        public bool? compReportCompDate {get;set;}
        public bool? accntMgrWorkPreTaxPrintDate {get;set;}
        public bool? accntMgrSalesPreTaxPrintDate {get;set;}
        public bool? accntWorkPreTaxPrintDate {get;set;}
        public bool? accntSalesPreTaxPrintDate { get; set; }

        //툴팁날짜
        public string manageCustomAcptDate_ToolTip{get;set;}
        public string manageCustomConfirmDate_ToolTip{get;set;}
        public string chargeStandInwareDate_ToolTip{get;set;}
        public string searchReqDate_ToolTip{get;set;}
        public string searchDate_ToolTip{get;set;}
        public string corpApprovalDate_ToolTip{get;set;}
        public string corpEndDate_ToolTip{get;set;}
        public string corpLastEndDate_ToolTip{get;set;}
        public string localGovBehaviorReportDate_ToolTip{get;set;}
        public string kepElectrReqDate_ToolTip{get;set;}
        public string kepInApprovalDate_ToolTip{get;set;}
        public string kepPaymentDate_ToolTip{get;set;}
        public string kepMeterInstallDate_ToolTip{get;set;}
        public string constrDate_ToolTip{get;set;}
        public string constrCompleteDate_ToolTip{get;set;}
        public string electrSafeCheckDate_ToolTip{get;set;}
        public string electrSafeCheckCostPayDate_ToolTip{get;set;}
        public string electrBeforeUseCheckReqDate_ToolTip{get;set;}
        public string electrBeforeUseCheckPrintDate_ToolTip{get;set;}
        public string electrBeforeInspReqDate_ToolTip{get;set;}
        public string electrBeforeInspPrintDate_ToolTip{get;set;}
        public string electrBeforeInspCostPayDate_ToolTip{get;set;}
        public string superSetTaxPrintDate_ToolTip{get;set;}
        public string superUseInspReqDate_ToolTip{get;set;}
        public string superBeforeUseInspDate_ToolTip{get;set;}
        public string superBeforeUseInspPrintDate_ToolTip{get;set;}
        public string compReplyDate_ToolTip{get;set;}
        public string suppleCompDate_ToolTip{get;set;}
        public string compSuppleReportDate_ToolTip{get;set;}
        public string insurePrintDate_ToolTip{get;set;}
        public string compReportCompDate_ToolTip{get;set;}
        public string accntMgrWorkPreTaxPrintDate_ToolTip{get;set;}
        public string accntMgrSalesPreTaxPrintDate_ToolTip{get;set;}
        public string accntWorkPreTaxPrintDate_ToolTip{get;set;}
        public string accntSalesPreTaxPrintDate_ToolTip { get; set; }

        public bool NoElecDeliMethod { get; set; } = false;
        public string NoElecDeliMethod_ToolTip { get; set; }
        public bool NoKepElectDeliMethodOutOrIn { get; set; } = false;
        public string NoKepElectDeliMethodOutOrIn_ToolTip { get; set; }



        #region orderColumn..
        //public string manageCustomAcptDate { get; set; }
        //public string manageCustomConfirmDate { get; set; }
        //public string installLocation { get; set; }
        //public string installLocationAddress { get; set; }
        //public string InstallLocationPhone { get; set; }
        //public string installLocationPart { get; set; }
        //public string houseHoldCount { get; set; }
        //public string carParkingCount { get; set; }
        //public string electrCarCount { get; set; }
        //public string alReadyChargeCount { get; set; }
        //public string reqChargeCount { get; set; }
        //public string alreadyManageCustomName { get; set; }
        //public string alreadyManageCustomID { get; set; }
        //public string installLocationComments { get; set; }

        //public string contractFromDate { get; set; }
        //public string contractToDate { get; set; }
        //public string openReqDate { get; set; }
        //public string openDate { get; set; }
        //public string damdangjaName { get; set; }
        //public string damdangjaPhone { get; set; }
        //public string damdangjaEMail { get; set; }

        //public string installLocationAddComments { get; set; }
        //public string saledamdangjaPhone { get; set; }
        //public string saleCustomAddWork { get; set; }
        //public string salegift { get; set; }
        //public string article { get; set; }
        //public string chargeOrderDate { get; set; }
        //public string chargeInwareDate { get; set; }
        //public string chargeInwareQty { get; set; }
        //public string chargeInwareLocation { get; set; }
        //public string canopyReqCustom { get; set; }
        //public string chargeModelHelmat { get; set; }
        //public string chargeModelinloc { get; set; }
        //public string chargeModelOneBody { get; set; }
        //public string chargeStandReqDate { get; set; }
        //public string chargeStandInwareDate { get; set; }
        //public string mtrCanopyInwareInfo { get; set; }
        //public string mtrCanopyOrderAmount { get; set; }
        //public string comments { get; set; }
        //public string searchReqDate { get; set; }
        //public string searchDate { get; set; }
        //public string searchQty { get; set; }
        //public string searchDataAcptDate { get; set; }
        //public string installLocationCount { get; set; }
        //public string electrDeliveryMethod { get; set; }
        //public string inspectionNeedYN { get; set; }
        //public string addConstructCostSearch { get; set; }
        //public string addConstructCost { get; set; }
        //public string searchComments { get; set; }
        //public string corpAcptNo { get; set; }
        //public string corpApprovalDate { get; set; }
        //public string corpEndDate { get; set; }
        //public string corpLastEndDate { get; set; }
        //public string corpComments { get; set; }
        //public string kepInstallLocationCount { get; set; }
        //public string kepElectrDeliveryMethod { get; set; }
        //public string kepOutLineConstructContext { get; set; }
        //public string kepInfraPayAmount { get; set; }
        //public string kepManageInfraPayAmount { get; set; }
        //public string kepElectrReqDate { get; set; }
        //public string kepInApprovalYN { get; set; }
        //public string kepInApprovalDate { get; set; }
        //public string kepMeterInstallContext { get; set; }
        //public string kepDamdangjaPhone { get; set; }
        //public string kepCustomNo { get; set; }
        //public string kepPaymentDate { get; set; }
        //public string kepMeterInstallDate { get; set; }
        //public string kepFaucetComments { get; set; }

        //public string constrCustomName { get; set; }
        //public string constrCustomID { get; set; }
        //public string constrOrderDate { get; set; }
        //public string constrDate { get; set; }
        //public string constrDelyReason { get; set; }

        //public string constrCompleteDate { get; set; }
        //public string constrComments { get; set; }
        //public string electrSafeCheckDate { get; set; }
        //public string electrSafeCheckSuppleContext { get; set; }
        //public string electrSafeCheckLocation { get; set; }
        //public string electrSafeCheckCost { get; set; }
        //public string electrSafeCheckCostPayDate { get; set; }
        //public string electrBeforeUseCheckReqDate { get; set; }
        //public string electrSafeCheckPrintDate { get; set; }
        //public string electrBeforeUseCheckSuppleContext { get; set; }
        //public string electrBeforeInspLocation { get; set; }
        //public string electrBeforeInspReqDate { get; set; }
        //public string electrBeforeInspPrintDate { get; set; }
        //public string electrBeforeInspCost { get; set; }
        //public string electrBeforeInspCostPayDate { get; set; }
        //public string electrBeforeInspSuppleContext { get; set; }
        //public string electrSafeCheckComments { get; set; }
        //public string superCustomName { get; set; }
        //public string superCustomID { get; set; }
        //public string superCostPayCustom { get; set; }
        //public string superCostPayCustomID { get; set; }
        //public string superCustomPhoneNo { get; set; }
        //public string safeManageCustomName { get; set; }
        //public string safeManageCustomID { get; set; }
        //public string safeManageCustomPhoneNo { get; set; }
        //public string superSetCost { get; set; }
        //public string superSetTaxPrintDate { get; set; }
        //public string superUseInspPayCustomName { get; set; }
        //public string superUseInspPayCustomID { get; set; }
        //public string superUseInspReqDate { get; set; }
        //public string superFromUseInspReqDate { get; set; }
        //public string superBeforeUseInspDate { get; set; }
        //public string superComments { get; set; }
        //public string compReplyDate { get; set; }
        //public string suppleContext { get; set; }
        //public string suppleCompDate { get; set; }
        //public string compSuppleReportContext { get; set; }
        //public string compSuppleReportDate { get; set; }
        //public string insurePrintDate { get; set; }
        //public string compReportCompDate { get; set; }
        //public string compReportComments { get; set; }
        //public string accntMgrWorkPreTaxPrintDate { get; set; }
        //public string accntMgrWorkPreAmount { get; set; }
        //public string accntMgrWorkPreAmountComments { get; set; }
        //public string accntMgrWorkAfterTaxPrintDate { get; set; }
        //public string accntMgrWorkAfterAmount { get; set; }
        //public string accntMgrWorkAfterAmountComments { get; set; }
        //public string accntMgrWorkTaxPrintDate { get; set; }
        //public string accntMgrWorkAmount { get; set; }
        //public string accntMgrWorkAmountComments { get; set; }
        //public string accntWorkTaxPrintDate { get; set; }
        //public string accntWorkAmount { get; set; }
        //public string accntWorkAmountComments { get; set; }
        //public string accntSalesTaxPrintDate { get; set; }
        //public string accntSalesAmount { get; set; }
        //public string accntSalesAmountComments { get; set; }

        #endregion


    }
}
