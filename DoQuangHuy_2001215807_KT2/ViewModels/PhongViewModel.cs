using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DoQuangHuy_2001215807_KT2.Helpers;

namespace DoQuangHuy_2001215807_KT2.ViewModels
{
    public class PhongViewModel : BaseViewModel
    {
        private ObservableCollection<PHONG> _danhSachPhong;
        public ObservableCollection<PHONG> DanhSachPhong
        {
            get => _danhSachPhong;
            set => SetProperty(ref _danhSachPhong, value);
        }

        private ObservableCollection<LOAIPHONG> _danhSachTang;
        public ObservableCollection<LOAIPHONG> DanhSachTang
        {
            get => _danhSachTang;
            set => SetProperty(ref _danhSachTang, value);
        }

        private string _maPhong;
        public string MaPhong { get => _maPhong; set => SetProperty(ref _maPhong, value); }

        private string _tenPhong;
        public string TenPhong { get => _tenPhong; set => SetProperty(ref _tenPhong, value); }

        private string _giaPhong;
        public string GiaPhong { get => _giaPhong; set => SetProperty(ref _giaPhong, value); }

        private string _sucChua;
        public string SucChua { get => _sucChua; set => SetProperty(ref _sucChua, value); }

        private bool _isQuat = true;
        public bool IsQuat { get => _isQuat; set => SetProperty(ref _isQuat, value); }

        private bool _isMayLanh;
        public bool IsMayLanh { get => _isMayLanh; set => SetProperty(ref _isMayLanh, value); }

        private LOAIPHONG _selectedTang;
        public LOAIPHONG SelectedTang
        {
            get => _selectedTang;
            set => SetProperty(ref _selectedTang, value);
        }

        private PHONG _selectedPhong;
        public PHONG SelectedPhong
        {
            get => _selectedPhong;
            set
            {
                SetProperty(ref _selectedPhong, value);
                if (value != null) LoadToForm(value);
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        public PhongViewModel()
        {
            AddCommand = new RelayCommand(_ => ExecuteAdd());
            EditCommand = new RelayCommand(_ => ExecuteEdit(), _ => SelectedPhong != null);
            SaveCommand = new RelayCommand(_ => ExecuteSave());
            ClearCommand = new RelayCommand(_ => ExecuteClear());

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new KaraokeQLEntities())
            {
                DanhSachTang = new ObservableCollection<LOAIPHONG>(db.LOAIPHONGs.ToList());
                ReloadPhong(db);
            }
        }

        private void ReloadPhong(KaraokeQLEntities db = null)
        {
            bool ownContext = db == null;
            if (ownContext) db = new KaraokeQLEntities();

            DanhSachPhong = new ObservableCollection<PHONG>(
                db.PHONGs.Include("LOAIPHONG").ToList()
            );

            if (ownContext) db.Dispose();
        }

        private void LoadToForm(PHONG p)
        {
            MaPhong = p.MaPhong;
            TenPhong = p.TenPhong;
            GiaPhong = p.GiaPhong.ToString();
            SucChua = p.SucChua.ToString();
            IsQuat = p.KieuPhong == 1;
            IsMayLanh = p.KieuPhong == 2;
            SelectedTang = DanhSachTang.FirstOrDefault(t => t.MaNhom == p.MaNhom);
        }

        private void ExecuteAdd()
        {
            if (!Validate()) return;

            using (var db = new KaraokeQLEntities())
            {
                if (db.PHONGs.Any(p => p.MaPhong == MaPhong))
                {
                    MessageBox.Show("Mã phòng đã tồn tại!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                db.PHONGs.Add(BuildPhong());
                db.SaveChanges();
            }

            ReloadPhong();
            ExecuteClear();
            MessageBox.Show("Thêm phòng thành công!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteEdit()
        {
            if (SelectedPhong == null)
            {
                MessageBox.Show("Vui lòng chọn một phòng trên danh sách!", "Thông báo");
                return;
            }
            MessageBox.Show("Hãy chỉnh sửa thông tin rồi nhấn Lưu.", "Hướng dẫn");
        }

        private void ExecuteSave()
        {
            if (!Validate()) return;

            using (var db = new KaraokeQLEntities())
            {
                var phong = db.PHONGs.Find(MaPhong);
                if (phong == null)
                {
                    MessageBox.Show("Không tìm thấy phòng cần sửa!", "Lỗi");
                    return;
                }

                phong.TenPhong = TenPhong;
                phong.GiaPhong = decimal.Parse(GiaPhong);
                phong.SucChua = int.Parse(SucChua);
                phong.KieuPhong = IsQuat ? 1 : 2;
                phong.MaNhom = SelectedTang?.MaNhom;

                db.SaveChanges();
            }

            ReloadPhong();
            ExecuteClear();
            MessageBox.Show("Lưu thành công!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteClear()
        {
            MaPhong = "";
            TenPhong = "";
            GiaPhong = "";
            SucChua = "";
            IsQuat = true;
            IsMayLanh = false;
            SelectedTang = null;
            SelectedPhong = null;
        }

        private PHONG BuildPhong() => new PHONG
        {
            MaPhong = MaPhong,
            TenPhong = TenPhong,
            GiaPhong = decimal.Parse(GiaPhong),
            SucChua = int.Parse(SucChua),
            KieuPhong = IsQuat ? 1 : 2,
            MaNhom = SelectedTang?.MaNhom
        };

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(MaPhong))
            { MessageBox.Show("Vui lòng nhập Mã phòng!"); return false; }
            if (string.IsNullOrWhiteSpace(TenPhong))
            { MessageBox.Show("Vui lòng nhập Tên phòng!"); return false; }
            if (!decimal.TryParse(GiaPhong, out _) || decimal.Parse(GiaPhong) <= 0)
            { MessageBox.Show("Giá phòng không hợp lệ!"); return false; }
            if (!int.TryParse(SucChua, out _) || int.Parse(SucChua) <= 0)
            { MessageBox.Show("Sức chứa không hợp lệ!"); return false; }
            if (SelectedTang == null)
            { MessageBox.Show("Vui lòng chọn tầng!"); return false; }
            return true;
        }
    }
}