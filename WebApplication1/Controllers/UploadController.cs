using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    public class UploadController : Controller
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IAdminRepo _iAdminRepo;
        private readonly IConfiguration _configuration;

        public UploadController(ILogger<UploadController> logger, IWebHostEnvironment env
             , IAdminRepo iAdminRepo, IConfiguration configuration)
        {
            _logger = logger;
            _env = env;
            _configuration = configuration;
            _iAdminRepo = iAdminRepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        [SupportedOSPlatform("windows")]
        [HttpPost]
        public IActionResult Upload(IFormFile postedFile)
        {
            try
            {
                if (postedFile != null)
                {
                    //Create a Folder.
                    string path = Path.Combine(_env.WebRootPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    //Save the uploaded Excel file.
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string filePath = Path.Combine(path, fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    //Read the connection string for the Excel file.
                    string conString = _configuration.GetConnectionString("ExcelConString");
                    DataTable dt = new DataTable();
                    conString = string.Format(conString, filePath);

                    try
                    {
                        using (OleDbConnection connExcel = new OleDbConnection(conString))
                        {
                            using (OleDbCommand cmdExcel = new OleDbCommand())
                            {
                                using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                                {
                                    cmdExcel.Connection = connExcel;

                                    //Get the name of First Sheet.
                                    connExcel.Open();
                                    DataTable dtExcelSchema;
                                    dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                    string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                    //string col = dtExcelSchema.Columns[16].ToString();
                                    connExcel.Close();

                                    //Read Data from First Sheet.
                                    connExcel.Open();
                                    cmdExcel.CommandText = "SELECT * From [" + sheetName + "] where [Kết Quả] <> '' ";
                                    odaExcel.SelectCommand = cmdExcel;
                                    odaExcel.Fill(dt);
                                    connExcel.Close();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("File Excel lỗi: " + ex.Message);
                        return Content("1");
                    }                    

                    //Insert the Data read from the Excel file to Database Table.
                    conString = _configuration.GetConnectionString("SqlFPT2");
                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                        {
                            //Set the database table name.
                            sqlBulkCopy.DestinationTableName = "dbo.KQXN_COVID";

                            //////[OPTIONAL]: Map the Excel columns with that of the database table.
                            //sqlBulkCopy.ColumnMappings.Add("Lý do", "LyDo");
                            //sqlBulkCopy.ColumnMappings.Add("Đối tượng", "DoiTuong");
                            //sqlBulkCopy.ColumnMappings.Add("Tiếp xúc với ca bệnh", "TiepXuc");
                            //sqlBulkCopy.ColumnMappings.Add("Họ và tên", "HoTen");
                            //sqlBulkCopy.ColumnMappings.Add("Nam", "NamSinhNam");
                            //sqlBulkCopy.ColumnMappings.Add("Nữ", "NamSinhNu");
                            //sqlBulkCopy.ColumnMappings.Add("CMND", "CMND");
                            //sqlBulkCopy.ColumnMappings.Add("SDT", "SoDT");
                            //sqlBulkCopy.ColumnMappings.Add("Số nhà", "SoNha");
                            //sqlBulkCopy.ColumnMappings.Add("Phường/Xã", "PhuongXa");
                            //sqlBulkCopy.ColumnMappings.Add("Quận/Huyện", "QuanHuyen");
                            //sqlBulkCopy.ColumnMappings.Add("Tỉnh/Thành", "TinhThanh");
                            //sqlBulkCopy.ColumnMappings.Add("Nơi xét nghiệm", "NoiXN");
                            //sqlBulkCopy.ColumnMappings.Add("Mã LIS", "MaLIS");
                            //sqlBulkCopy.ColumnMappings.Add("Kết quả", "KetQua");
                            //sqlBulkCopy.ColumnMappings.Add("CT E", "CTE");
                            //sqlBulkCopy.ColumnMappings.Add("CT N", "CTN");
                            //sqlBulkCopy.ColumnMappings.Add("CT RdRp", "CTRdRp");
                            //sqlBulkCopy.ColumnMappings.Add("Ngày có kết quả", "NgayKetQua");
                            //sqlBulkCopy.ColumnMappings.Add("Ngày lấy mẫu", "NgayLayMau");
                            //sqlBulkCopy.ColumnMappings.Add("Nơi lấy mẫu", "NoiLayMau");
                            //sqlBulkCopy.ColumnMappings.Add("Loại hình", "LoaiHinh");
                            

                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);
                            con.Close();
                        }
                    }
                }
                return Content("0");
            }
            catch (Exception ex)
            {
                _logger.LogError("SQL lỗi: " + ex.Message);
                return Content("2");
            }
        }

        public async Task<IActionResult> GetAlls(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                var tuNgay1 = tuNgay.ToString("yyyyMMdd");
                var denNgay1 = denNgay.AddDays(1).ToString("yyyyMMdd");

                var query = System.IO.Path.Combine(_env.WebRootPath, "Query\\GetAllKetQuaPCR.txt");


                var resultAwait = await _iAdminRepo.GetAlls(tuNgay1, denNgay1, query);
                var result = resultAwait.ToList();

                return PartialView("_GetAll", result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GetAlls UploadController" + ex.Message);
                return Content("0");
            }
        }

        public async Task<IActionResult> InKetQua(int id)
        {
            try
            {
                string folder = _env.WebRootPath + "\\ReportPrint";

                // Delete all files in a directory    
                string[] files = Directory.GetFiles(folder);
                foreach (string file in files)
                {
                    System.IO.File.Delete(file);
                    //Console.WriteLine($"{file} is deleted.");
                }

                //GetReport
                var reportResult = await GetReport(id);
                if (reportResult != null)
                {
                    //Inreport
                    var fileName = PrintReport(reportResult);
                    return Content(fileName);
                }
                else
                {
                    return Content("1"); //Lấy kết quả in bị lỗi
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("UpdateDaIn HomeController" + ex.Message);
                return Content("0");
            }
        }





        private async Task<KetQuaPCR> GetReport(int id)
        {
            try
            {
                //var thoiGian = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var query = System.IO.Path.Combine(_env.WebRootPath, "Query\\GetKetQuaPCRById.txt");

                var resultAwait = await _iAdminRepo.GetById(id, query);
                var result = resultAwait;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GetReport UploadController" + ex.Message);
                return null;
            }
        }

        private string PrintReport(KetQuaPCR ketQuaPCR)
        {
            try
            {
                var reportName = System.IO.Path.Combine(_env.WebRootPath, "Reports\\rptKetQua.repx");
                var filename = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff_tt") + ".pdf";
                var pathName = System.IO.Path.Combine(_env.WebRootPath, "ReportPrint\\" + filename);

                var report = XtraReport.FromFile(reportName, true);
                var reportData = new List<KetQuaPCR>();
                reportData.Add(ketQuaPCR);
                report.DataSource = reportData;
                report.CreateDocument(true);
                _logger.LogInformation(pathName);
                //var printTool = new PrintToolBase(report.PrintingSystem);

                report.ExportToPdf(pathName);
                return filename;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("PrintReport UploadController" + ex.Message);
                return null;
            }
        }
    }
}
