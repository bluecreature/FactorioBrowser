using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Castle.Core.Internal;
using FactorioBrowser.Mod.Loader;
using FactorioBrowser.Prototypes;
using FactorioBrowser.UI.ViewModel;

namespace FactorioBrowser.UI {

   public sealed class ModSettingControlTemplateSelector : DataTemplateSelector {

      public DataTemplate BooleanSettingTemplate { get; set; }

      public DataTemplate IntegerSettingTemplate { get; set; }

      public DataTemplate DoubleSettingTemplate { get; set; }

      public DataTemplate StringSettingTemplate { get; set; }

      public DataTemplate ValueListTemplate { get; set; }

      public override DataTemplate SelectTemplate(object item, DependencyObject container) {
         FcModSettingValue sv = item as FcModSettingValue;
         Debug.Assert(sv != null);

         if (sv.Definition is FcBooleanSetting) {
            return BooleanSettingTemplate;

         } else if (sv.Definition is FcIntegerSetting) {
            return ((FcIntegerSetting) sv.Definition).AllowedValues.IsNullOrEmpty() ?
               IntegerSettingTemplate : ValueListTemplate;

         } else if (sv.Definition is FcDoubleSetting) {
            return ((FcDoubleSetting) sv.Definition).AllowedValues.IsNullOrEmpty() ?
               DoubleSettingTemplate : ValueListTemplate;

         } else if (sv.Definition is FcStringSetting) {
            return ((FcStringSetting) sv.Definition).AllowedValues.IsNullOrEmpty() ?
               StringSettingTemplate : ValueListTemplate;

         } else {
            throw new NotImplementedException();
         }
      }
   }

   /// <summary>
   /// Interaction logic for SettingsView.xaml
   /// </summary>
   public partial class SettingsView {
      private readonly SettingsViewModel _viewModel;

      public SettingsView(SettingsViewModel viewModel) {
         _viewModel = viewModel;
         InitializeComponent();
         DataContext = _viewModel;
      }

      internal delegate void SettingsConfirmedEventHandler(
         IImmutableDictionary<string, object> settings);

      internal event SettingsConfirmedEventHandler SelectionConfirmed;

      private async void SettingsView_OnLoaded(object sender, RoutedEventArgs e) {
         await _viewModel.LoadData();
      }

      private void Next_Click(object sender, RoutedEventArgs e) {
         SelectionConfirmed?.Invoke(_viewModel.GetSettingsValues());
      }
   }
}
