using Business.Interfaces;
using Business.DTOs.Input;
using Core.Entities;
using Core.Interfaces;

namespace Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SaleService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public string TaoHoaDon(CreateHoaDonRequest request)
        {
            // 1. Tạo hóa đơn
            var hd = new HoaDon
            {
                MaHD = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                MaND = request.MaND,
                MaKH = request.MaKH,
                MaCN = request.MaKho,
                NgayLap = DateTime.Now,
                PhuongThucTT = request.PhuongThucTT,
                CT_HoaDons = new List<CT_HoaDon>()
            };

            decimal tongTien = 0;

            // 2. Xử lý từng Serial được chọn bán
            foreach (var serialCode in request.DanhSachMaSerial)
            {
                // Kiểm tra xem Serial này có trong kho không
                var serialDB = _unitOfWork.SerialSP.Find(s => s.SoSerial == serialCode).FirstOrDefault();

                if (serialDB != null && serialDB.TrangThai == "TonKho")
                {
                    // Cập nhật trạng thái Serial -> Đã Bán
                    serialDB.TrangThai = "DaBan";
                    _unitOfWork.SerialSP.Update(serialDB);

                    // Lấy giá bán từ bảng Sản Phẩm
                    var sp = _unitOfWork.SanPham.GetById(serialDB.MaSP);
                    decimal giaBan = sp.GiaBan ?? 0;

                    // Tạo chi tiết hóa đơn
                    var ct = new CT_HoaDon
                    {
                        MaSerial = serialDB.MaSerial,
                        GiaBanThucTe = giaBan
                    };
                    hd.CT_HoaDons.Add(ct);
                    tongTien += giaBan;
                }
            }

            hd.TongThanhToan = tongTien;
            _unitOfWork.HoaDon.Add(hd);

            _unitOfWork.Complete(); // Lưu Hóa đơn + Cập nhật Serial cùng lúc
            return hd.MaHD;
        }
    }
}