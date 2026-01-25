using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;
using Business.DTOs.Input;

namespace Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // KHU VỰC ĐĂNG NHẬP (Mặc định khi vào Web)
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã có Session (đã đăng nhập) -> Đá thẳng vào Dashboard
            if (HttpContext.Session.GetString("MaND") != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(); // Hiện form đăng nhập
        }

        [HttpPost]
        public IActionResult Index(LoginRequest request)
        {
            // Gọi Business kiểm tra User/Pass
            var user = _authService.Login(request);

            if (user != null)
            {
                // Lưu thông tin người dùng vào Session
                HttpContext.Session.SetString("MaND", user.MaND);
                HttpContext.Session.SetString("HoTen", user.HoTen ?? "Người dùng");

                // Đăng nhập đúng -> Chuyển sang Dashboard
                return RedirectToAction("Dashboard");
            }

            // Đăng nhập sai -> Báo lỗi
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View(request);
        }

        // ==========================================
        // KHU VỰC DASHBOARD (Sau khi đăng nhập)
        // ==========================================
        [HttpGet]
        public IActionResult Dashboard()
        {
            // Kiểm tra bảo mật: Chưa đăng nhập mà cố vào -> Đá về trang Login
            if (HttpContext.Session.GetString("MaND") == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // ==========================================
        // ĐĂNG XUẤT
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch Session
            return RedirectToAction("Index"); // Quay về trang đăng nhập
        }
    }
}