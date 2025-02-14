using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Pop_dgdFile
'** 설명       : 계약등록 과거계약조회 상담등록 팝업
'** 작성일자   : 2024.12.11
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.12.11, 최대현, 최초작성, 계약등록 과거계약조회 기능작성, 선택 값 돌려주기  
'**************************************************************************************************/

namespace WizMes_EVC.Order.Pop
{

    public partial class Win_ord_Pop_OrderClose_File_Q : Window
    {
        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        string orderID = string.Empty;
        int fileDown_Success = 0;

        public Win_ord_Pop_OrderClose_File SelectedItem { get; set; }

        //string sDate = string.Empty;
        //string eDate = string.Empty;
        //DateTime fiveYearsAgo = DateTime.Today.AddYears(-5);

        ObservableCollection<Win_ord_Pop_OrderClose_File> ovcFile_OrderClose
        = new ObservableCollection<Win_ord_Pop_OrderClose_File>();

        private FTP_EX _ftp = null;
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Order";

        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";


        public Win_ord_Pop_OrderClose_File_Q(string orderID)
        {
            InitializeComponent();
            this.orderID = orderID;
        }

        private void Win_ord_Pop_dgdFile_Q_Loaded(object sender, RoutedEventArgs e)
        {
            fillGrid(orderID);
        }




        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        private void DataGrid_SizeChange(object sender, SizeChangedEventArgs e)
        {
            //DataGrid dgs = sender as DataGrid;
            //if (dgs.ColumnHeaderHeight == 0)
            //{
            //    dgs.ColumnHeaderHeight = 1;
            //}
            //double a = e.NewSize.Height / 100;
            //double b = e.PreviousSize.Height / 100;
            //double c = a / b;

            //if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
            //{
            //    dgs.ColumnHeaderHeight = dgs.ColumnHeaderHeight * c;
            //    dgs.FontSize = dgs.FontSize * c;
            //}
        }

        // 적용버튼 클릭.
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            int count = ovcFile_OrderClose.Count;

            MessageBoxResult msgresult = MessageBox.Show($"선택하신 {count}개의 파일을 다운로드 하시겠습니까??", "보기 확인", MessageBoxButton.YesNo);
            if(msgresult == MessageBoxResult.Yes)
            {
                if (PopUp_FTPDownload(count))
                {
                    MessageBox.Show($"{fileDown_Success} 의 파일을 다운로드 하였습니다.");
                }

                fileDown_Success = 0;
            }
            //int selectrow = dgdFile.SelectedIndex;
            //DataGridRow dgr = lib.GetRow(selectrow, dgdFile);
            //SelectedItem = dgr.Item as Win_ord_Pop_OrderClose_File;
            //DialogResult = true;
            //this.Close();
        }
        private bool PopUp_FTPDownload(int count)
        {
            bool flag = true;
            fileDown_Success = 0;

            try
            {
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                string str_path = string.Empty;
                str_path = FTP_ADDRESS + '/' + orderID;
                _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                foreach(var item in ovcFile_OrderClose)
                {
                    string str_remotepath = string.Empty;
                    string str_localpath = string.Empty;

                    str_remotepath = item.fileName;
                    str_localpath = LOCAL_DOWN_PATH + "\\" + orderID + "\\" + item.fileName;

                    DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH + "\\" + orderID);     
                    if (DI.Exists == false)
                    {
                        DI.Create();
                    }

                    FileInfo file = new FileInfo(str_localpath);
                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    if (_ftp.download(str_remotepath, str_localpath)) fileDown_Success++;
                }
            }
            catch
            {
                return false;
            }
            

            return flag;
        }

        // 취소버튼 클릭.
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Reserve_Info = dgdFile.SelectedItem as Win_ord_Pop_OrderClose_File;
        }

        private void btnSelAll_Click(object sender, RoutedEventArgs e)
        {
            if(dgdFile.Items.Count > 0)
            {
                foreach(Win_ord_Pop_OrderClose_File item in dgdFile.Items)
                {
                    if(item.chk == false)
                    {
                        item.chk = true;
                        ovcFile_OrderClose.Add(item);
                    }
                }
            }
        }
        
        private void btnDeSelAll_Click(object sender, RoutedEventArgs e)
        {
            if (dgdFile.Items.Count > 0)
            {
                foreach (Win_ord_Pop_OrderClose_File item in dgdFile.Items)
                {
                    if (item.chk == true)
                    {
                        item.chk = false;
                        ovcFile_OrderClose.Remove(item);
                    }
                }
            }
        }


        private bool fillGrid(string orderID)
        {

            if (dgdFile.Items.Count > 0) { dgdFile.Items.Clear(); }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();


                sqlParameter.Add("orderID", orderID);


                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sOrderTotal_PopUp_File", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = null;
                    dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 과거이력 데이터가 없습니다.");
                    }
                    else
                    {
                        dgdFile.Items.Clear();
                        DataRowCollection drc = dt.Rows;
                        int num = 1;
                        foreach (DataRow dr in drc)
                        {
                            var fileList = new Win_ord_Pop_OrderClose_File()
                            {
                                num = num,
                                fileName = dr["fileName"].ToString(),
                                filePath = dr["filePath"].ToString(),
                            };

                            if(fileList.fileName.Trim() != string.Empty)
                            dgdFile.Items.Add(fileList);
                            num++;
                        }

      
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("과거계약조회 중 오류 : 오류내용\n" + ex.ToString());
                return false;
            }

        }

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        //8자리 char형태 날짜 년도-월-일 하이픈 삽입
        //16자리 일경우 8자리 사이에 ~ 삽입
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


        public class Win_ord_Pop_OrderClose_File : BaseView
        {

            public bool chk { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
            public int num { get; set; }

            ////tab1
            //public string contractFileName { get; set; }    //시공발주서
            //public string contractFilePath { get; set; }
          

            ////tab2
            //public string searchChecksheetFilePath{get;set;}
            //public string searchChecksheetFileName{get;set;}
            //public string installLocationSheetFilePath{get;set;}
            //public string installLocationSheetFileName{get;set;}
            //public string localGoTaxFilePath{get;set;}
            //public string localGoTaxFileName {get;set;}
            //public string LocalGovProveFilePath {get;set;}
            //public string LocalGovProveFileName { get; set; }

            ////tab3
            //public string kepElectrLineFilePath { get; set; }
            //public string kepElectrLineFileName { get; set; }
            //public string kepFaucetAcptFilePath { get; set; }
            //public string kepFaucetAcptFileName { get; set; }
            //public string electrSafeInspPrintFilePath { get; set; }
            //public string electrSafeInspPrintFileName { get; set; }
            //public string electrBeforeUseCheckPrintFilePath { get; set; }
            //public string electrBeforeUseCheckPrintFileName { get; set; }
            //public string electrBeforeUseInspFilePath { get; set; }
            //public string electrBeforeUseInspFileName { get; set; }
            //public string electrKepAcptFilePath { get; set; }
            //public string electrKepAcptFileName { get; set; }
            //public string electrKepInfraPayBillFilePath { get; set; }
            //public string electrKepInfraPayBillFileName { get; set; }
            //public string electrUseContractFilePath { get; set; }
            //public string electrUseContractFileName { get; set; }
            //public string electrBeforeUseInspCostFilePath { get; set; }
            //public string electrBeforeUseInspCostFileName { get; set; }
            //public string electrCoWorkFilePath { get; set; }
            //public string electrCoWorkFileName { get; set; }
            //public string electrCostFilePath { get; set; }
            //public string electrCostFileName { get; set; }
            //public string electrTransCoUseFilePath { get; set; }
            //public string electrTransCoUseFileName { get; set; }

            ////tab4

            //public string superSetCheckFilePath { get; set; }
            //public string superSetCheckFileName { get; set; }
            //public string superBeforeUseInspectFilePath { get; set; }
            //public string superBeforeUseInspectFileName { get; set; }
            //public string compReportFIleName { get; set; }
            //public string compReportFIlePath { get; set; }
            //public string superCostFilePath { get; set; }
            //public string superCostFileName { get; set; }
            //public string safeManagerCertiFileName { get; set; }
            //public string safeManagerCertiFilePath { get; set; }
            //public string superReportFilePath { get; set; }
            //public string superReportFileName { get; set; }
            //public string insurePrintFilePath { get; set; }
            //public string insurePrintFileName { get; set; }

            ////정산경리
            //public string accntMgrWorkPreTaxFilePath { get; set; }              //운영사시공팀 선금 세금계산서 파일
            //public string accntMgrWorkPreTaxFileName { get; set; }
            //public string accntMgrWorkInterTaxFilePath { get; set; }            //운영사시공팀 중도금 세금계산서 파일
            //public string accntMgrWorkInterTaxFileName { get; set; }
            //public string accntMgrWorkAfterTaxFilePath { get; set; }            //운영사시공팀 잔금 세금계산서 파일
            //public string accntMgrWorkAfterTaxFileName { get; set; }
            //public string accntMgrWorkTaxFilePath { get; set; }                 //운영사시공팀 
            //public string accntMgrWorkTaxFileName { get; set; }
            //public string accntMgrSalesPreTaxFilePath { get; set; }
            //public string accntMgrSalesPreTaxFileName { get; set; }
            //public string accntMgrSalesInterTaxFilePath { get; set; }
            //public string accntMgrSalesInterTaxFileName { get; set; }
            //public string accntMgrSalesAfterTaxFilePath { get; set; }
            //public string accntMgrSalesAfterTaxFileName { get; set; }
            //public string accntMgrSalesTaxFilePath { get; set; }
            //public string accntMgrSalesTaxFileName { get; set; }
            //public string accntWorkPreTaxFilePath { get; set; }
            //public string accntWorkPreTaxFileName { get; set; }
            //public string accntWorkInterTaxFilePath { get; set; }
            //public string accntWorkInterTaxFileName { get; set; }
            //public string accntWorkAfterTaxFilePath { get; set; }
            //public string accntWorkAfterTaxFileName { get; set; }
            //public string accntWorkTaxFilePath { get; set; }
            //public string accntWorkTaxFileName { get; set; }
            //public string accntSalesPreTaxFilePath { get; set; }
            //public string accntSalesPreTaxFileName { get; set; }
            //public string accntSalesInterTaxFilePath { get; set; }
            //public string accntSalesInterTaxFileName { get; set; }
            //public string accntSalesAfterTaxFilePath { get; set; }
            //public string accntSalesAfterTaxFileName { get; set; }
            //public string accntSalesTaxFilePath { get; set; }
            //public string accntSalesTaxFileName { get; set; }

            ////tab5
            //public string sketch1FilePath { get; set; }
            //public string sketch1FileName { get; set; }
            //public string sketch1FileAlias { get; set; }
            //public string sketch2FilePath { get; set; }
            //public string sketch2FileName { get; set; }
            //public string sketch2FileAlias { get; set; }
            //public string sketch3FilePath { get; set; }
            //public string sketch3FileName { get; set; }
            //public string sketch3FileAlias { get; set; }
            //public string sketch4FilePath { get; set; }
            //public string sketch4FileName { get; set; }
            //public string sketch4FileAlias { get; set; }
            //public string sketch5FilePath { get; set; }
            //public string sketch5FileName { get; set; }
            //public string sketch5FileAlias { get; set; }
            //public string sketch6FilePath { get; set; }
            //public string sketch6FileName { get; set; }
            //public string sketch6FileAlias { get; set; }
            //public string sketch7FilePath { get; set; }
            //public string sketch7FileName { get; set; }
            //public string sketch7FileAlias { get; set; }
            //public string sketch8FilePath { get; set; }
            //public string sketch8FileName { get; set; }
            //public string sketch8FileAlias { get; set; }
            //public string sketch9FilePath { get; set; }
            //public string sketch9FileName { get; set; }
            //public string sketch9FileAlias { get; set; }
            //public string sketch10FilePath { get; set; }
            //public string sketch10FileName { get; set; }
            //public string sketch10FileAlias { get; set; }
            //public string sketch11FilePath { get; set; }
            //public string sketch11FileName { get; set; }
            //public string sketch11FileAlias { get; set; }
            //public string sketch12FilePath { get; set; }
            //public string sketch12FileName { get; set; }
            //public string sketch12FileAlias { get; set; }
        }
    }

}
