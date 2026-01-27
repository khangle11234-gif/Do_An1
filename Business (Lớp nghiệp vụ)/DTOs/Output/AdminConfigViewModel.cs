using Core.Entities; // Dùng để nhận diện ThongTinCongTy và LichSuHeThong

namespace Business.DTOs.Output
{
    public class AdminConfigViewModel
    {
        public ThongTinCongTy CongTy { get; set; }
        public List<LichSuHeThong> LichSu { get; set; }

        // Constructor khởi tạo để tránh lỗi null
        public AdminConfigViewModel()
        {
            CongTy = new ThongTinCongTy();
            LichSu = new List<LichSuHeThong>();
        }
    }
}