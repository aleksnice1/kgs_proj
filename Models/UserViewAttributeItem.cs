using Kadastr.DomainModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Kadastr.WebApp.Models
{
    public class UserViewAttributeItem: INotifyPropertyChanged
    {
        public long Id
        {
            get { return _id; }
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Id"));
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Checked"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            UserViewAttributeItem item = obj as UserViewAttributeItem;

            return Id == item.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private long _id;
        private string _name;
        private bool _checked;
    }
}