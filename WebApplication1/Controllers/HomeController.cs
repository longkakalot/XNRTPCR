using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IAdminRepo _iAdminRepo;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env
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

        public async Task<IActionResult> GetAlls(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                var tuNgay1 = tuNgay.ToString("yyyyMMdd");
                var denNgay1 = denNgay.AddDays(1).ToString("yyyyMMdd");

                var query = System.IO.Path.Combine(_env.WebRootPath, "Query\\GetAllKetQuaPCR.txt");


                var resultAwait = await _iAdminRepo.GetAlls(tuNgay1, denNgay1, query);
                var result = resultAwait.ToList();

                return PartialView("", result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GetAlls HomeController" + ex.Message);
                return Content("0");
            }
        }

        public async Task<IActionResult> LayThongTin(string hoTen, string namSinh, string soDienThoai)
        {
            try
            {
                var queryHoTenId = System.IO.Path.Combine(_env.WebRootPath, "Query\\GetHoTenBySoDtNamSinh.txt");
                var resultHoTenIdAwait = await _iAdminRepo.GetHoTenBySoDtNamSinh(soDienThoai, namSinh, queryHoTenId);
                var resultHoTenId = resultHoTenIdAwait.ToList();

                var hoTenKhongDau = String.Concat(convertToUnSign2(hoTen).ToLower().Where(c => !Char.IsWhiteSpace(c)));

                var listId = new List<int>();

                foreach (var item in resultHoTenId)
                {
                    var hoTenKhongDauSql = String.Concat(convertToUnSign2(item.HoTen).ToLower().Where(c => !Char.IsWhiteSpace(c)));
                    if(hoTenKhongDau == hoTenKhongDauSql)
                    {
                        listId.Add(item.Id);
                    }
                }

                var queryById = System.IO.Path.Combine(_env.WebRootPath, "Query\\GetKetQuaPCRById.txt");
                var listKetQuaPcr = new List<KetQuaPCR>();

                foreach (var itemId in listId)
                {
                    var resultAwait = await _iAdminRepo.GetById(itemId, queryById);
                    listKetQuaPcr.Add(resultAwait);
                }

                if(listKetQuaPcr.Count == 0 || listKetQuaPcr is null)
                {
                    return Content("0");
                }
                
                //var resultAwait = await _iAdminRepo.GetById(tuNgay1, denNgay1, query);
                //var result = resultAwait.ToList();

                return PartialView("_GetListKetQuaByBn", listKetQuaPcr);
                //return Content("0");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("GetAlls HomeController" + ex.Message);
                return Content("0");
            }
        }

        private string convertToUnSign2(string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
    }
}
