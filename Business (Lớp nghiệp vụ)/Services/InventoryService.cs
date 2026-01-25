using Business.Interfaces;
using Business.DTOs.Input;
using Business.DTOs.Output;
using Core.Entities;
using Core.Interfaces;

namespace Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public bool NhapHang(CreatePhieuNhapRequest request)
        {
            try
            {
                // 1. Tạo Phiếu Nhập
                var phieuNhap = new PhieuNhap
                {
                    MaPN = "PN" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    MaND = request.MaND,
                    MaNCC = request.MaNCC,
                    MaCN = request.MaKho,
                    NgayNhap = DateTime.Now,
                    GhiChu = request.GhiChu,
                    CT_PhieuNhaps = new List<CT_PhieuNhap>()
                };

                decimal tongTien = 0;

                // 2. Duyệt qua từng sản phẩm để tạo chi tiết & Serial
                foreach (var item in request.ChiTiet)
                {
                    // Tạo chi tiết phiếu nhập
                    var ct = new CT_PhieuNhap
                    {
                        MaSP = item.MaSP,
                        SoLuong = item.SoLuong,
                        DonGia = item.DonGia,
                        ThanhTien = item.SoLuong * item.DonGia
                    };
                    phieuNhap.CT_PhieuNhaps.Add(ct);
                    tongTien += (decimal)ct.ThanhTien;

                    // 3. LOGIC TỰ SINH SERIAL (Quan trọng)
                    for (int i = 0; i < item.SoLuong; i++)
                    {
                        var serial = new SerialSP
                        {
                            MaSerial = Guid.NewGuid().ToString().Substring(0, 10), // Sinh mã ngẫu nhiên
                            MaSP = item.MaSP,
                            MaCN = request.MaKho,
                            SoSerial = item.MaSP + "_" + DateTime.Now.Ticks + "_" + i, // Ví dụ: IPHONE_123456_1
                            TrangThai = "TonKho"
                        };
                        _unitOfWork.SerialSP.Add(serial);
                    }
                }

                phieuNhap.TongTien = tongTien;
                _unitOfWork.PhieuNhap.Add(phieuNhap);

                // 4. Lưu tất cả xuống DB trong 1 Transaction
                _unitOfWork.Complete();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<PhieuNhapViewModel> GetLichSuNhap()
        {
            return _unitOfWork.PhieuNhap.GetAll().Select(pn => new PhieuNhapViewModel
            {
                MaPN = pn.MaPN,
                NgayNhap = pn.NgayNhap,
                TongTien = pn.TongTien
            });
        }
    }
}