using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DoQuangHuy_2001215807_KT2.Helpers;

namespace DoQuangHuy_2001215807_KT2.ViewModels
{
    public class TimKiemViewModel : BaseViewModel
    {
        public ObservableCollection<LOAIPHONG> DanhSachTang { get; set; } = new ObservableCollection<LOAIPHONG>();

        private ObservableCollection<PHONG> _ketQua;
        public ObservableCollection<PHONG> KetQua
        {
            get => _ketQua;
            set => SetProperty(ref _ketQua, value);
        }

        private LOAIPHONG _selectedTang;
        public LOAIPHONG SelectedTang
        {
            get => _selectedTang;
            set => SetProperty(ref _selectedTang, value);
        }

        private string _sucChua = "0";
        public string SucChua { get => _sucChua; set => SetProperty(ref _sucChua, value); }


        private PHONG _selectedPhong;
        public PHONG SelectedPhong
        {
            get => _selectedPhong;

            set
            {
                SetProperty(ref _selectedPhong, value);
                OnPropertyChanged(nameof(TenPhong));
                OnPropertyChanged(nameof(SucChuaText));
                OnPropertyChanged(nameof(GiaText));
                OnPropertyChanged(nameof(KieuPhongText));
                OnPropertyChanged(nameof(TinhTrang));
            }
        }

        public string TenPhong =>  SelectedPhong?.TenPhong ?? "";
        public string SucChuaText => SelectedPhong?.SucChua.ToString() ?? "";
        public string GiaText => SelectedPhong?.GiaPhong.ToString("N0") ?? "";

        public string KieuPhongText => SelectedPhong == null ? "" : SelectedPhong.KieuPhong == 1 ? "Phòng quạt" : "Phòng máy lạnh";

        public string TinhTrang
        {
            get
            {
                if (SelectedPhong == null) return "";
                using (var db = new KaraokeQLEntities())
                {
                    bool co = db.DATPHONGs.Any(d =>
                        d.MaPh == SelectedPhong.MaPhong && d.NgayTra == null);
                    return co ? "Khách đang nhận phòng" : "Phòng trống";
                }
            }
        }

        public ICommand TimKiemCommand { get; }

        public TimKiemViewModel()
        {
            TimKiemCommand = new RelayCommand(_ => ExecuteTimKiem());
            LoadTang();
        }

        private void LoadTang()
        {
            using (var db = new KaraokeQLEntities())
            {
                DanhSachTang.Add(new LOAIPHONG { MaNhom = "", TenNhom = "(Tất cả)" });
                foreach (var t in db.LOAIPHONGs.ToList())
                    DanhSachTang.Add(t);
            }
            SelectedTang = DanhSachTang[0];
        }

        private void ExecuteTimKiem()
        {
            int sc = int.TryParse(SucChua, out int v) ? v : 0;

            using (var db = new KaraokeQLEntities())
            {
                var query = db.PHONGs.Include("LOAIPHONG").AsQueryable();

                if (!string.IsNullOrEmpty(SelectedTang?.MaNhom))
                    query = query.Where(p => p.MaNhom == SelectedTang.MaNhom);

                if (sc > 0)
                    query = query.Where(p => p.SucChua >= sc);

                KetQua = new ObservableCollection<PHONG>(query.ToList());
            }
        }

    }
}
