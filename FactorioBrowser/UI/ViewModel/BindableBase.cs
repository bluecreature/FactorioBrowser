using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FactorioBrowser.UI.ViewModel {

   public abstract class BindableBase : INotifyPropertyChanged {

      public event PropertyChangedEventHandler PropertyChanged;

      protected void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
         if (!Equals(storage, value)) {
            storage = value;
            FirePropertyChanged(propertyName);
         }
      }

      protected void FirePropertyChanged(string propertyName) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
