using Core.Entities;
using Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Presentation.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly QuanLyKhoContext _context;
        public NhaCungCapController(QuanLyKhoContext context) { _context = context; }

        [HttpPost]
        public IActionResult CreateQuick([FromBody] NhaCungCap model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.TenNCC)) return Json(new { success = false, msg = "Tên NCC không được để trống" });

                // Tự sinh mã nếu chưa có
                model.MaNCC = "NCC" + DateTime.Now.Ticks.ToString().Substring(10);
                model.TrangThai = true; // Mặc định hoạt động

                _context.NhaCungCap.Add(model);
                _context.SaveChanges();

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}