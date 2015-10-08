using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class FieldDialogViewModel : BaseDialogViewModel, IDataErrorInfo
    {
        #region Data

        private ObservableCollection<StringViewModel> _choicesSource;
        private IEnumerable<string> _existingNames;
        private FieldDialogViewModel _unmodified;
        private bool _choicesChanged;
        private bool _isAdd;

        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties
        
        #region Name
        private string _name = string.Empty;
        public string Name
        {
            get { return _name ?? string.Empty; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Default
        private string _defaultString = string.Empty;
        public string DefaultString
        {
            get { return _defaultString ?? string.Empty; }
            set { SetProperty(ref _defaultString, value); }
        }
        #endregion Default

        public string DefaultConditional
        {
            get
            {
                if (SelectedFieldType == FieldType.CheckBox)
                    return DefaultBoolean.ToString();
                return DefaultString;
            }
        }

        #region DefaultBoolean
        private bool _defaultBoolean;
        public bool DefaultBoolean
        {
            get { return _defaultBoolean; }
            set { SetProperty(ref _defaultBoolean, value); }
        }
        #endregion DefaultBoolean

        #region Prompt
        private string _prompt = string.Empty;
        public string Prompt
        {
            get { return _prompt ?? string.Empty; }
            set { SetProperty(ref _prompt, value); }
        }
        #endregion Prompt

        #region Description
        private string _description;
        public string Description
        {
            get { return _description ?? string.Empty; }
            set { SetProperty(ref _description, value); }
        }
        #endregion Description

        #region Choices
        private ListCollectionView _choices;
        public ListCollectionView Choices
        {
            get { return _choices; }
            set { SetProperty(ref _choices, value); }
        }
        #endregion Choices

        #region SelectedFieldType
        private FieldType? _selectedFieldType;
        public FieldType? SelectedFieldType
        {
            get { return _selectedFieldType; }
            set
            {
                if (SetProperty(ref _selectedFieldType, value))
                {
                    if (value != FieldType.CheckBox && (_defaultString == "False" || _defaultString == "True"))
                        _defaultString = string.Empty;

                    NotifyPropertyChanged(nameof(SelectedFieldTypeDescription));
                    NotifyPropertyChanged(nameof(ShowChoices));
                    NotifyPropertyChanged(nameof(ShowDefaultCheckBox));
                    NotifyPropertyChanged(nameof(DefaultStringMultiLine));
                }
            }
        }
        #endregion SelectedFieldType

        #region ShowChoices
        public bool ShowChoices => SelectedFieldType == FieldType.ComboBox || SelectedFieldType == FieldType.ComboBoxOpen;
        #endregion ShowChoices

        #region ShowDefaultCheckBox
        public bool ShowDefaultCheckBox => SelectedFieldType == FieldType.CheckBox;
        #endregion ShowDefaultCheckBox

        #region SelectedFieldTypeDescription
        public string SelectedFieldTypeDescription => SelectedFieldType.HasValue ? Enums.GetDescription(SelectedFieldType.Value) : null;
        #endregion SelectedFieldTypeDescription

        #region DefaultStringMultiLine
        public bool DefaultStringMultiLine => SelectedFieldType == FieldType.TextBoxMultiLine;

        #endregion DefaultStringMultiLine

        #region FieldTypes
        public ObservableCollection<ValueDescriptor<FieldType?>> FieldTypes { get; set; }
        #endregion FieldTypes

        #region ChoicesAsCommaDelimited
        public string ChoicesAsCommaDelimited
        {
            get
            {
                if (SelectedFieldType == null)
                    return null;
                switch (SelectedFieldType.Value)
                {
                    case FieldType.ComboBox:
                    case FieldType.ComboBoxOpen:
                        var choices = string.Join(" | ", _choicesSource.Select(c => c.Value));
                        return choices;
                    default:
                        return null;
                }
            }
        }
        #endregion ChoicesAsCommaDelimited

        #region IsInternal
        private bool _isInternal;
        public bool IsInternal
        {
            get { return _isInternal; }
            set { SetProperty(ref _isInternal, value); }
        }
        #endregion IsInternal

        #region DialogService
        [Inject]
        public IDialogService DialogService { get; set; }
        #endregion DialogService

        #endregion Properties

        #region Commands

        #region OkCommand
        DelegateCommand _okCommand;
        public DelegateCommand OkCommand => _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand, CanOkCommand));
        public bool CanOkCommand() => Error == null && (_isAdd || !IsUnchanged());
        public void ExecuteOkCommand()
        {
            if (string.IsNullOrEmpty(Error))
            {
                if (SelectedFieldType != FieldType.ComboBox && SelectedFieldType != FieldType.ComboBoxOpen)
                    Choices = null;

                _unmodified = null;
                DialogResult = true;
            }
        }
        #endregion OkCommand

        #region AddChoiceCommand
        DelegateCommand _addChoiceCommand;
        public DelegateCommand AddChoiceCommand => _addChoiceCommand ?? (_addChoiceCommand = new DelegateCommand(ExecuteAddChoiceCommand, CanAddChoiceCommand));
        public bool CanAddChoiceCommand() => !IsInternal;
        public void ExecuteAddChoiceCommand()
        {
            var vm = Kernel.Get<StringDialogViewModel>();
            vm.Add("Add Choice", "Choice:");

            if (DialogService.ShowDialog(vm))
            {
                var newItem = StringViewModel.CreateFromString(Kernel, vm.Value);
                _choicesSource.Add(newItem);
                // ReSharper disable once PossibleNullReferenceException
                Choices.MoveCurrentTo(newItem);

                _choicesChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        
        #region EditChoiceCommand
        DelegateCommand _editChoiceCommand;
        public DelegateCommand EditChoiceCommand => _editChoiceCommand ?? (_editChoiceCommand = new DelegateCommand(ExecuteEditChoiceCommand, CanEditChoiceCommand));
        public bool CanEditChoiceCommand() => Choices?.CurrentItem != null && !IsInternal;
        public void ExecuteEditChoiceCommand()
        {
            var cur = Choices.CurrentItem as StringViewModel;
            Debug.Assert(cur != null);

            var vm = Kernel.Get<StringDialogViewModel>();
            vm.Edit("Edit Choice", "Choice:", cur.Value);
            if (DialogService.ShowDialog(vm))
            {
                cur.Value = vm.Value;
                _choicesChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        
        #region DeleteChoiceCommand
        DelegateCommand _deleteChoiceCommand;
        public DelegateCommand DeleteChoiceCommand => _deleteChoiceCommand ?? (_deleteChoiceCommand = new DelegateCommand(ExecuteDeleteChoiceCommand, CanDeleteChoiceCommand));
        public bool CanDeleteChoiceCommand() => Choices?.CurrentItem != null && !IsInternal;
        public async void ExecuteDeleteChoiceCommand()
        {
            var vs = (StringViewModel) Choices?.CurrentItem;
            if ((await DialogService.Ask("Delete Choice?", $"Delete choice \"{vs?.Value}?\"", AskButton.OKCancel)) ==
                AskResult.OK)
            {
                Choices?.Remove(Choices.CurrentItem);
                _choicesChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #endregion Commands

        #region Virtuals

        protected override void TakePropertyChanged(string propertyName)
        {
            // If field changes, make sure read only fields are notified changed.
            switch (propertyName)
            {
                case nameof(SelectedFieldType):
                case nameof(DefaultString):
                case nameof(Choices):
                    NotifyPropertyChanged(nameof(ChoicesAsCommaDelimited));
                    break;
            }
        }

        #endregion Virtuals

        #region Public Methods

        public void CopyFrom(Field field)
        {
            _name = field.Name;
            if (field.FieldType == FieldType.CheckBox)
            {
                // Try method won't throw exception on failure.
                bool.TryParse(field.Default, out _defaultBoolean);
            }
            else
                _defaultString = field.Default;
            _prompt = field.Prompt;
            _description = field.Description;

            _choicesSource = new ObservableCollection<StringViewModel>(field.Choices.Select(s => StringViewModel.CreateFromString(Kernel, s)));
            _choices = new ListCollectionView(_choicesSource);

            _selectedFieldType = field.FieldType;
        }

        public void CopyFrom(FieldDialogViewModel field)
        {
            _name = field.Name;
            _defaultString = field._defaultString;
            _defaultBoolean = field._defaultBoolean;
            _prompt = field.Prompt;
            _description = field.Description;

            _choicesSource = field._choicesSource == null ? new ObservableCollection<StringViewModel>() : new ObservableCollection<StringViewModel>(field._choicesSource);
            _choices = new ListCollectionView(_choicesSource);

            _selectedFieldType = field.SelectedFieldType;
        }

        public void Add(bool isInternal, IEnumerable<string> existingNames)
        {
            _isAdd = true;
            Title = "New Field";
            IsInternal = isInternal;
            InitAddEditCommon();
            _existingNames = existingNames;
        }

        public void Edit(bool isInternal, IEnumerable<string> existingNames)
        {
            Title = $"Editing Field \"{Name}\"";
            IsInternal = isInternal;
            InitAddEditCommon();

            _existingNames = existingNames.Where(t => !string.Equals(t, Name, StringComparison.OrdinalIgnoreCase)).ToList();

            // Save unmodified property values.
            _unmodified = Kernel.Get<FieldDialogViewModel>();
            _unmodified.CopyFrom(this);
        }

        public static FieldDialogViewModel CreateFrom(IKernel kernel, FieldDialogViewModel vm)
        {
            var copy = kernel.Get<FieldDialogViewModel>();
            copy.CopyFrom(vm);
            return copy;
        }

        #endregion Public Methods

        #region Private Helpers

        private void InitAddEditCommon()
        {
            // Initialize fields common to both add and edit operations.

            if (FieldTypes == null)
            {
                FieldTypes = new ObservableCollection<ValueDescriptor<FieldType?>>()
                {
                    new ValueDescriptor<FieldType?>(null, ""),
                    new ValueDescriptor<FieldType?>(FieldType.CheckBox, Enums.GetDescription(FieldType.CheckBox)),
                    new ValueDescriptor<FieldType?>(FieldType.TextBox, Enums.GetDescription(FieldType.TextBox)),
                    new ValueDescriptor<FieldType?>(FieldType.TextBoxMultiLine, Enums.GetDescription(FieldType.TextBoxMultiLine)),
                    new ValueDescriptor<FieldType?>(FieldType.ComboBox, Enums.GetDescription(FieldType.ComboBox)),
                    new ValueDescriptor<FieldType?>(FieldType.ComboBoxOpen, Enums.GetDescription(FieldType.ComboBoxOpen))
                };
            }
        }

        #endregion Private Helpers

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Required";

                        if (ValidationUtilities.ValidateName(Name) != null)
                            return "Must be a valid identifier";

                        if (Name.StartsWith(" "))
                            return "Can't start with a space";

                        if (Name.EndsWith(" "))
                            return "Can't end with a space";

                        if (_existingNames != null && _existingNames.Any(s => string.Equals(s, Name, StringComparison.OrdinalIgnoreCase)))
                            return "Duplicated name";

                        return null;

                    case nameof(SelectedFieldType):
                        if (SelectedFieldType == null)
                            return "Required";
                        return null;

                    case nameof(Prompt):
                        if (string.IsNullOrWhiteSpace(Prompt))
                            return "Required";

                        if (ValidationUtilities.ValidateFieldPrompt(Prompt) != null)
                            return "Cannot end in a colon or space";
                        
                        return null;
                }
                return null;
            }
        }

        public string Error
        {
            get
            {
                if (this[nameof(Name)] != null || 
                    this[nameof(SelectedFieldType)] != null ||
                    this[nameof(Prompt)] != null)
                    return "Error";

                return null;
            }
        }

        private bool IsUnchanged()
        {
            if (_choicesSource == null || _unmodified == null)
                return true;

            return string.Equals(Name, _unmodified.Name, StringComparison.Ordinal) &&
                   string.Equals(Prompt, _unmodified.Prompt, StringComparison.Ordinal) &&
                   string.Equals(Description, _unmodified.Description, StringComparison.Ordinal) &&
                   SelectedFieldType == _unmodified.SelectedFieldType &&
                   (
                     SelectedFieldType != null &&
                     (SelectedFieldType == FieldType.CheckBox && DefaultBoolean == _unmodified.DefaultBoolean) ||
                     (SelectedFieldType != FieldType.CheckBox && DefaultString == _unmodified.DefaultString)
                   ) &&
                   !_choicesChanged;
        }

        #endregion IDataErrorInfo
    }
}