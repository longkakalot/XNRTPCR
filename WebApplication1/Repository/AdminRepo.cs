using WebApplication1.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApplication1.Models;
using System.Linq;

namespace WebApplication1.Repository
{
    public class AdminRepo : IAdminRepo
    {
        private readonly IDapperService _dapper;
        private readonly ILogger<AdminRepo> _logger;
        public AdminRepo(IDapperService dapper, ILogger<AdminRepo> logger, ILoggerFactory logFactory)
        {
            _logger = logger;
            _dapper = dapper;
            _logger = logFactory.CreateLogger<AdminRepo>();
        }       
              

        public async Task<int> UpdateKetQua(int id, bool ketQua, string query)
        {
            try
            {                
                string text = File.ReadAllText(query);

                var sql = String.Format(text, ketQua, id);

                var resultAwait = await _dapper.Update<int>(sql, null, CommandType.Text);
                var result = resultAwait;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateKetQua KetQuaRepo" + ex.Message);
                return 0;
            }
        }

        public async Task<int> HuyKetQua(int id, string query)
        {
            try
            {
                string text = File.ReadAllText(query);

                var sql = String.Format(text, id);

                var resultAwait = await _dapper.Update<int>(sql, null, CommandType.Text);
                var result = resultAwait;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("HuyKetQua KetQuaRepo" + ex.Message);
                return 0;
            }
        }

        public async Task<IList<KetQuaPCR>> GetAlls(string tuNgay, string denNgay, string query)
        {
            try
            {
                string text = File.ReadAllText(query);
                var sql = String.Format(text, tuNgay, denNgay);

                var resultAwait = await _dapper.GetAll<KetQuaPCR>(sql, null, CommandType.Text);
                var result = resultAwait.ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAlls AdminRepo" + ex.Message);
                return null;
            }
        }

        public async Task<IList<KetQuaPCR>> GetByMaLisSoDt(string maLis, string soDienThoai, string query)
        {
            try
            {
                string text = File.ReadAllText(query);
                var sql = String.Format(text, maLis, soDienThoai);

                var resultAwait = await _dapper.GetAll<KetQuaPCR>(sql, null, CommandType.Text);
                var result = resultAwait.ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetByMaLisSoDt AdminRepo" + ex.Message);
                return null;
            }
        }

        public async Task<KetQuaPCR> GetById(int Id, string query)
        {
            try
            {
                string text = File.ReadAllText(query);
                var sql = String.Format(text, Id);

                var resultAwait = await _dapper.Get<KetQuaPCR>(sql, null, CommandType.Text);
                var result = resultAwait;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetById AdminRepo" + ex.Message);
                return null;
            }
        }

        public async Task<IList<KetQuaPCR>> GetHoTenBySoDtNamSinh(string soDienThoai, string namSinh, string query)
        {
            try
            {
                string text = File.ReadAllText(query);
                var sql = String.Format(text, soDienThoai, namSinh);

                var resultAwait = await _dapper.GetAll<KetQuaPCR>(sql, null, CommandType.Text);
                var result = resultAwait.ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetHoTenBySoDtNamSinh AdminRepo" + ex.Message);
                return null;
            }
        }
    }
}
