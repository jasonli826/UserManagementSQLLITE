using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSQLLiteDB.Entity
{
    public class Domain:INotifyPropertyChanged
    {
        public int DomainID { get; set; }
        public string DomainName { get; set; } 
        public string CN { get; set; }
        public string DC1 { get; set; }
        public string DC2 { get; set; }
        public string DC3 { get; set; }
        public string Description { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime? Updated_Date { get; set; }
        public string Status { get; set; }
        public string DomainNme { get; set; }
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
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

}
