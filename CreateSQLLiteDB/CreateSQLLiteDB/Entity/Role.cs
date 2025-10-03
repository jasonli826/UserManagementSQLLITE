using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSQLLiteDB.Entity
{
    public class Role : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public int RoleID { get; set; }
        public string Role_Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime? Updated_Date { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }



}
