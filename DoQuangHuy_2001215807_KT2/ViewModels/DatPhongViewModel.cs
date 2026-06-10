using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DoQuangHuy_2001215807_KT2.Helpers;

namespace DoQuangHuy_2001215807_KT2.ViewModels
{
    public class ChiTietItem : BaseViewModel
    {
        public string MaPT { get; set; }
        public string TenPhuThu { get; set; }
        public decimal GiaPT { get; set; }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set { SetProperty(ref _soLuong, value); OnPropertyChanged(nameof(ThanhTien)); }
        }

        public decimal ThanhTien => GiaPT * SoLuong;
    }

    public class DatPhongViewModel : BaseViewModel
    {
        public ObservableCollection<PHONG> DanhSachPhong { get; set; } = new ObservableCollection<PHONG>();
        public ObservableCollection<KHACHHANG> DanhSachKhach { get; set; } = new ObservableCollection<KHACHHANG>();
        public ObservableCollection<PHUTHU> DanhSachPhuThu { get; set; } = new ObservableCollection<PHUTHU>();
        public ObservableCollection<ChiTietItem> DanhSachChiTiet { get; set; } = new ObservableCollection<ChiTietItem>();

        private DateTime _ngayDat = DateTime.Today;
        public DateTime NgayDat { get => _ngayDat; set => SetProperty(ref _ngayDat, value); }

        private PHONG _selectedPhong;
        public PHONG SelectedPhong
        {
            get => _selectedPhong;
            set
            {
                SetProperty(ref _selectedPhong, value);
                OnPropertyChanged(nameof(GiaPhong));
                OnPropertyChanged(nameof(SucChuaText));
                TinhTong();
            }
        }

        public decimal GiaPhong => SelectedPhong?.GiaPhong ?? 0;

        public string SucChuaText => SelectedPhong == null ? "" : SelectedPhong.SucChua + " người";

        private KHACHHANG _selectedKhach;
        public KHACHHANG SelectedKhach { get => _selectedKhach; set => SetProperty(ref _selectedKhach, value); }

        private PHUTHU _selectedPhuThu;
        public PHUTHU SelectedPhuThu
        {
            get => _selectedPhuThu;
            set { SetProperty(ref _selectedPhuThu, value); OnPropertyChanged(nameof(GiaPTHienThi)); }
        }
        public decimal GiaPTHienThi => SelectedPhuThu?.GiaPT ?? 0;

        private string _soLuongPT = "1";
        public string SoLuongPT { get => _soLuongPT; set => SetProperty(ref _soLuongPT, value); }

        private string _gioVao = "08:00";
        public string GioVao
        {
            get => _gioVao;
            set { SetProperty(ref _gioVao, value); TinhTong(); }
        }

        private string _gioRa = "10:00";
        public string GioRa
        {
            get => _gioRa;
            set { SetProperty(ref _gioRa, value); TinhTong(); }
        }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set => SetProperty(ref _tongTien, value); }

        public ICommand ThemPTCommand { get; }
        public ICommand DatPhongCommand { get; }


        public DatPhongViewModel()
        {
            ThemPTCommand = new RelayCommand(_ => ExecuteThemPT());
            DatPhongCommand = new RelayCommand(_ => ExecuteDatPhong());
            LoadData(); 
        }

        private void LoadData()
        {
            using (var db = new KaraokeQLEntities())
            {
                foreach (var p in db.PHONGs.ToList()) DanhSachPhong.Add(p);
                foreach (var k in db.KHACHHANGs.ToList()) DanhSachKhach.Add(k);
                foreach (var pt in db.PHUTHUs.ToList()) DanhSachPhuThu.Add(pt);
            }
        }

        private void TinhTong()
        {
            double soGio = 0;
            if (TimeSpan.TryParse(GioVao, out TimeSpan tv) &&
                TimeSpan.TryParse(GioRa, out TimeSpan tr))
            {
                soGio = (tr - tv).TotalHours;
                if (soGio < 0) soGio = 0;
            }

            decimal tienHat = (decimal)soGio * GiaPhong;
            decimal tienPhuThu = 0;
            foreach (var c in DanhSachChiTiet) tienPhuThu += c.ThanhTien;

            TongTien = tienHat + tienPhuThu;
        }

        private void ExecuteThemPT()
        {
            if (SelectedPhuThu == null)
            { MessageBox.Show("Vui lòng chọn phụ thu!"); return; }

            if (!int.TryParse(SoLuongPT, out int sl) || sl <= 0)
            { MessageBox.Show("Số lượng không hợp lệ!"); return; }

            ChiTietItem existing = null;
            foreach (var c in DanhSachChiTiet)
                if (c.MaPT == SelectedPhuThu.MaPhuThu) { existing = c; break; }

            if (existing != null)
                existing.SoLuong += sl;
            else
                DanhSachChiTiet.Add(new ChiTietItem
                {
                    MaPT = SelectedPhuThu.MaPhuThu,
                    TenPhuThu = SelectedPhuThu.TenPhuThu,
                    GiaPT = SelectedPhuThu.GiaPT,
                    SoLuong = sl
                });

            TinhTong();
            SoLuongPT = "1";
        }

        private void ExecuteDatPhong()
        {
            if (SelectedPhong == null)
            { MessageBox.Show("Vui lòng chọn phòng!"); return; }
            if (SelectedKhach == null)
            { MessageBox.Show("Vui lòng chọn khách hàng!"); return; }

            using (var db = new KaraokeQLEntities())
            {
                var dp = new DATPHONG
                {
                    MaPh = SelectedPhong.MaPhong,
                    MaKH = SelectedKhach.MaKhachHang,
                    NgayDat = NgayDat,
                    NgayTra = null
                };
                db.DATPHONGs.Add(dp);
                db.SaveChanges();

                foreach (var c in DanhSachChiTiet)
                {
                    db.CHITIETDATPHONGs.Add(new CHITIETDATPHONG
                    {
                        MaDP = dp.MaDatPhong,
                        MaPT = c.MaPT,
                        SL = c.SoLuong
                    });
                }
                db.SaveChanges();

                MessageBox.Show(
                    $"Đặt phòng thành công!\nMã đặt phòng: {dp.MaDatPhong}\nTổng tiền tạm tính: {TongTien:N0} VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            DanhSachChiTiet.Clear();
            SelectedPhong = null;
            SelectedKhach = null;
            GioVao = "08:00";
            GioRa = "10:00";
            TongTien = 0;
        }
    }
}