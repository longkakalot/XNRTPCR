using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Repository
{
    public interface IAdminRepo
    {
        Task<IList<KetQuaPCR>> GetAlls(string tuNgay, string denNgay, string query);
        Task<IList<KetQuaPCR>> GetByMaLisSoDt(string maLis, string soDienThoai, string query);
        Task<IList<KetQuaPCR>> GetHoTenBySoDtNamSinh(string soDienThoai, string namSinh, string query);

        Task<KetQuaPCR> GetById(int Id, string query);


        //Task<IList<LayMauTestNhanh>> GetListChuaLayMau(string tuNgay, string denNgay, string query);
        //Task<IList<LayMauTestNhanh>> GetListDaLayMau(string tuNgay, string denNgay, string query);
        //Task<IList<LayMauTestNhanh>> GetListChuaKetQua(string tuNgay, string denNgay, string query);
        //Task<IList<LayMauTestNhanh>> GetListDaKetQua(string tuNgay, string denNgay, string query);
        //Task<int> UpdateKetQua(int id, bool ketQua, string query);
        //Task<int> HuyKetQua(int id, string query);
        //Task<DataTable> GetDataTableFromExcel();
    }
}
