using AutoCode.Presentation.Model;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace AutoCode.Presentation
{
    public partial class AngularForm : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string FormName { get; set; }
        private ObservableCollection<FieldProperties> Fields { get; set; }
        public List<string> FieldTypes { get; set; }
        public AngularForm(List<string> angularParamList)
        {
            try
            {
                log.Info("Enter on Angular Form.xaml file.");
                InitializeComponent();
                if (angularParamList.Count > 1)
                {
                    this.FormName = angularParamList[0];
                    this.Fields = JsonConvert.DeserializeObject<ObservableCollection<FieldProperties>>(angularParamList[1]);
                    setFieldValues();
                    dataGrid.ItemsSource = this.Fields;
                    this.FieldTypes = new List<string>
                {
                    "TextBox","Number","email","Dropdown", "RadioButton", "CheckBox", "Calendar"
                };
                }
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                log.Error("Exception on Angular Controller: " + ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                log.Info("Clicked on Generate button of angular code.");
                if (formCheckBox.IsChecked == true)
                {
                    txtBlockCode.Text = string.Empty;
                    //create form
                    txtBlockCode.Text = generateForm();
                }
                if (listCheckBox.IsChecked == true)
                {
                    txtBlockListCode.Text = string.Empty;
                    //create list
                    txtBlockListCode.Text = generateTable();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception on Angular Code generate butrton : " + ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        private void setFieldValues()
        {
            foreach (var field in this.Fields)
            {
                //textbox check
                if (field.Type.IndexOf("string", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "TextBox";
                //for number value
                else if (field.Type.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("long", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("double", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "Number";
                //checkbox check
                else if (field.Type.IndexOf("bool", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "CheckBox";
                //date check
                else if (field.Type.IndexOf("datetime", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("timespan", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "Calender";
                else
                    field.FieldType = "TextBox";
                //email check
                if (field.Name.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "email";
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                // Get the selected value directly from the ComboBox
                string selectedFieldType = comboBox.SelectedItem as string;

                // Get the DataContext of the ComboBox (FieldProperties object)
                var fieldProperties = comboBox.DataContext as FieldProperties;
                if (fieldProperties != null && !string.IsNullOrEmpty(selectedFieldType))
                {
                    // Update the FieldType property based on the selected value
                    fieldProperties.FieldType = selectedFieldType;
                }
            }
        }
        private void listCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            stackActions.Visibility = Visibility.Visible;
        }
        private void listCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            stackActions.Visibility = Visibility.Collapsed;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked
            var button = sender as Button;
            if (button != null)
            {
                // Get the DataGridRow that contains this button
                var row = DataGridRow.GetRowContainingElement(button);
                // Retrieve the item (data object) from the row's DataContext
                var item = row.DataContext as FieldProperties;
                if (item != null)
                {
                    // Assuming you have a collection that backs your DataGrid
                    var collection = (ObservableCollection<FieldProperties>)dataGrid.ItemsSource;
                    if (collection != null)
                    {
                        // Remove the item from the collection
                        collection.Remove(item);
                    }
                }
            }
        }
        //----Form Code Generater Function------
        private string generateForm()
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                builder.AppendLine("------------------------Your Add Form HTML's Code Starts Here---------------------------\n\n");
                builder.AppendLine($"<form [formGroup]=\"myForm\" class=\"container mt-5\">");
                builder.AppendLine($"\t<div class=\"row\">");

                foreach (var field in this.Fields)
                {
                    //TextBox
                    if (field.FieldType == "TextBox")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label for=\"{field.Name}\" class=\"form-label\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($"<input\r\ntype=\"text\"\r\nid=\"{field.Name}\"\r\nclass=\"form-control\"\r\nformControlName=\"{field.Name}\"\r\n" +
                            $"[ngClass] = \"{{ 'is-invalid': isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')}}\" />");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div>");
                    }
                    //Email
                    if (field.FieldType == "email")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label for=\"{field.Name}\" class=\"form-label\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($"<input\r\ntype=\"text\"\r\nid=\"{field.Name}\"\r\nclass=\"form-control\"\r\nformControlName=\"{field.Name}\"\r\n" +
                            $"[ngClass] = \"{{ 'is-invalid': isFormSubmitted && (myForm.get('{field.Name}')?.hasError('required') || myForm.get('email')?.hasError('email'))}}\"/>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"<span *ngIf = \"myForm.get('{field.Name}')?.hasError('email')\" class=\"error\">\r\n" +
                            $"Enter a valid {field.Label}.\r\n</span>");

                        builder.AppendLine($"</div>");
                    }
                    //Number
                    if (field.FieldType == "Number")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label for=\"{field.Name}\" class=\"form-label\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($"<input\r\ntype=\"number\"\r\nid=\"{field.Name}\"\r\nclass=\"form-control\"\r\nformControlName=\"{field.Name}\"\r\n" +
                            $"[ngClass] = \"{{ 'is-invalid': isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')}}\"/>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div>");
                    }
                    //RadioButton
                    if (field.FieldType == "RadioButton")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label class=\"form-label d-block\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($"<div class=\"form-check form-check-inline\">");
                        builder.AppendLine($"<input type = \"radio\" id=\"genderMale\" class=\"form-check-input\" value=\"male\" formControlName=\"gender\"/>");
                        builder.AppendLine($"<label for=\"genderMale\" class=\"form-check-label\">Male</label>");
                        builder.AppendLine($"</div>");
                        builder.AppendLine($"<div class=\"form-check form-check-inline\">");
                        builder.AppendLine($"<input type = \"radio\" id=\"genderFemale\" class=\"form-check-input\" value=\"female\" formControlName=\"gender\"/>");
                        builder.AppendLine($"<label for=\"genderFemale\" class=\"form-check-label\">Female</label>");
                        builder.AppendLine($"</div>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div>");
                    }
                    //DropDown
                    if (field.FieldType == "Dropdown")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label for=\"{field.Name}\" class=\"form-label\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($" <select id = \"{field.Name}\" class=\"form-select\" formControlName=\"{field.Name}\"" +
                            $"[ngClass] = \"{{ 'is-invalid': isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')}}\">");
                        builder.AppendLine($"<option value = \"\" > Select </option >");
                        builder.AppendLine($" <option value=\"1\"> Option 1</option>");
                        builder.AppendLine($" <option value=\"2\"> Option 2</option>");
                        builder.AppendLine($" <option value=\"3\"> Option 3</option>");
                        builder.AppendLine($" <option value=\"4\"> Option 4</option>");
                        builder.AppendLine($"</select>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div >");
                    }
                    //Calendar
                    if (field.FieldType == "Calendar")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<label for= \"{field.Name}\" >{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">* :</span>");
                        }
                        builder.AppendLine($"</label>");
                        builder.AppendLine($"<input type=\"date\" id=\"{field.Name}\" name=\"{field.Name}\" formControlName=\"{field.Name}\"" +
                            $"[ngClass] = \"{{ 'is-invalid': isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')}}\"/>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div>");
                    }
                    //CheckBox
                    if (field.FieldType == "CheckBox")
                    {
                        builder.AppendLine($"<div class=\"col-12 col-sm-12 col-md-6 mb-3\">");
                        builder.AppendLine($"<div class=\"form-check\">");
                        builder.AppendLine($"<label class=\"form-check-label\">");
                        builder.AppendLine($"<input class=\"form-check-input\" type=\"checkbox\" name=\"{field.Name}\" formControlName=\"{field.Name}\">{field.Label}");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span class=\"error\">*</span>");
                        }
                        builder.AppendLine($" </label>");
                        builder.AppendLine($"</div>");
                        if (field.IsRequired == true)
                        {
                            builder.AppendLine($"<span *ngIf = \"isFormSubmitted && myForm.get('{field.Name}')?.hasError('required')\" class=\"error\">");
                            builder.AppendLine($"{field.Label} is required.");
                            builder.AppendLine($"</span>");
                        }
                        builder.AppendLine($"</div>");
                    }
                }
                //Submit Button
                builder.AppendLine($" <div class=\"col-12 mb-3\">");
                builder.AppendLine($"<button type=\"button\" class=\"btn btn-primary\" (click)=\"onSubmit()\">");
                builder.AppendLine($" Submit");
                builder.AppendLine($" </button>");
                builder.AppendLine($"</div>");

                builder.AppendLine($"</div>");
                builder.AppendLine($"</form>");
                builder.AppendLine("-------------------------------- End Add Form HTML Code ------------------------------------\n\n");

                builder.AppendLine("------------------------------Your Add Form Java Script's Code Starts Here-----------------------\n\n");

                builder.AppendLine(
                    "import {Component} from '@angular/core';\n" +
                    "import { FormControl, FormGroup, Validators }");
                builder.AppendLine("from '@angular/forms';\n\n\n");
                //variable isFormSubmitted
                builder.AppendLine($"public isFormSubmitted = false;\n");
                //Constructor Code Commented
                builder.AppendLine($"  constructor() {{\r\n    // private _loadingService: LoadingService,\r\n   " +
                    $" // private _toasterService: ToastrService,\r\n    // private _errorService: ErrorService,\r\n  " +
                    $"  // private _apiService: ApiService\r\n  }}");


                //ngOnInit function
                builder.AppendLine($"  ngOnInit() {{\r\n \r\n  }}");
                builder.AppendLine($"  myForm = new FormGroup({{");
                foreach (var field in Fields)
                {
                    builder.AppendLine($"{field.Name}: new FormControl('', [");
                    if (field.FieldType == "email")
                        builder.AppendLine($" Validators.email,");
                    if (field.IsRequired == true)
                    {
                        builder.AppendLine($"Validators.required");
                    }
                    builder.AppendLine($"]),");
                }
                builder.AppendLine($"}});");
                builder.AppendLine($"onSubmit() {{");
                builder.AppendLine($"this.isFormSubmitted = true;");
                builder.AppendLine($"if (this.myForm.invalid) {{\r\n      return;\r\n    }}");

                //API Code Commented
                builder.AppendLine($"    console.log(this.myForm.value);\r\n\r\n " +
                    $"   // this._loadingService.displayLoader(true);\r\n  " +
                    $"  // this._apiService.addFormData(this.myForm.value).subscribe(\r\n  " +
                    $"  //   (result: any) => {{\r\n    //     this._loadingService.displayLoader(false);\r\n  " +
                    $"  //     this.isFormSubmitted = false;\r\n    //     if (result.status == 200) {{\r\n  " +
                    $"  //       this._toasterService.success(result.body);\r\n    //  " +
                    $"   }}\r\n  " +
                    $"  //   }},\r\n  " +
                    $"  //   (error: any) => {{\r\n " +
                    $"   //     this._loadingService.displayLoader(false);\r\n " +
                    $"   //     this.isFormSubmitted = false;\r\n " +
                    $"   //     this._errorService.errorHandle(error);\r\n " +
                    $"   //   }}\r\n    // );");
                builder.AppendLine($"}}");
                builder.AppendLine("-------------------------------- End Add Form Java Script Code ------------------------------------\n\n");
                //Code for Update Form-------------
                builder.AppendLine("---------------------------Your Update Form HTML Code Starts Here -------------------------------\n\n");
                builder.AppendLine("----Update Form HTML Code same as Add Form Code because All fields are same ----\n\n");
                builder.AppendLine("------------------------------Your Update Form's Java Script's Code Starts Here-----------------------\n\n");
                builder.AppendLine(
                    "import {Component} from '@angular/core';\n" +
                    "import { FormControl, FormGroup, Validators }");
                builder.AppendLine("from '@angular/forms';\n\n\n");
                //variable isFormSubmitted
                builder.AppendLine($"public isFormSubmitted = false;\n");
                //Constructor Code Commented
                builder.AppendLine($"  constructor() {{\r\n    // private _loadingService: LoadingService,\r\n   " +
                    $" // private _toasterService: ToastrService,\r\n    // private _errorService: ErrorService,\r\n  " +
                    $"  // private _apiService: ApiService\r\n  }}");
                //ngOnInit() function
                builder.AppendLine($" ngOnInit() {{\r\n    this.getFormData();\r\n  }}");

                //myForm
                builder.AppendLine($"myForm = new FormGroup({{");
                foreach (var field in Fields)
                {
                    builder.AppendLine($"{field.Name}: new FormControl(");
                    if (field.IsDisable == true)
                    {
                        builder.AppendLine($" {{value:'', disabled: true }},[");
                    }
                    else
                    {
                        builder.AppendLine($"'',[");
                    }
                    if (field.FieldType == "email")
                        builder.AppendLine($" Validators.email,");
                    if (field.IsRequired == true)
                    {
                        builder.AppendLine($"Validators.required");
                    }
                    builder.AppendLine($"]),");
                }
                builder.AppendLine($"}});");

                //getFormData Function

                builder.AppendLine($"  getFormData(){{\r\n  " +
                    $"  // this._loadingService.displayLoader(true);\r\n  " +
                    $"  // this._apiService.getFormData(id).subscribe(\r\n  " +
                    $"  //   (result: any) => {{\r\n   " +
                    $" //     this._loadingService.displayLoader(false);\r\n " +
                    $"   //     this.isFormSubmited = false;\r\n   " +
                    $" //     if (result.status == 200) {{\r\n  " +
                    $"  //       this._toasterService.success(result.body);\r\n  " +
                    $"  //     }}\r\n    //   }},\r\n  " +
                    $"  //   (error: any) => {{\r\n  " +
                    $"  //     this._loadingService.displayLoader(false);\r\n " +
                    $"   //     this.isFormSubmited = false;\r\n " +
                    $"   //     this._errorService.errorHandle(error);\r\n " +
                    $"   //   }}\r\n " +
                    $"   // );\r\n  }}");
                builder.AppendLine($"onSubmit() {{");
                builder.AppendLine($"this.isFormSubmitted = true;");
                builder.AppendLine($"if (this.myForm.invalid) {{\r\n      return;\r\n    }}");
                //API Code Commented
                builder.AppendLine($"    console.log(this.myForm.value);\r\n\r\n " +
                    $"   // this._loadingService.displayLoader(true);\r\n  " +
                    $"  // this._apiService.addFormData(this.myForm.value).subscribe(\r\n  " +
                    $"  //   (result: any) => {{\r\n    //     this._loadingService.displayLoader(false);\r\n  " +
                    $"  //     this.isFormSubmitted = false;\r\n    //     if (result.status == 200) {{\r\n  " +
                    $"  //       this._toasterService.success(result.body);\r\n    //  " +
                    $"   }}\r\n  " +
                    $"  //   }},\r\n  " +
                    $"  //   (error: any) => {{\r\n " +
                    $"   //     this._loadingService.displayLoader(false);\r\n " +
                    $"   //     this.isFormSubmitted = false;\r\n " +
                    $"   //     this._errorService.errorHandle(error);\r\n " +
                    $"   //   }}\r\n    // );");
                builder.AppendLine($"}}");
                builder.AppendLine("-------------------------------- End Update form Java Script Code ------------------------------------\n\n");
                builder.AppendLine("--------------------------------Global CSS ------------------------------------\n\n");
                builder.AppendLine($".is-invalid " +
                    $"{{\r\n  border: 1px solid red;\r\n}}" +
                    $"\r\n\r\n.error " +
                    $"{{\r\n  color: #ff0000;\r\n}}");
                builder.AppendLine("-------------------------------------------- End Code --------------------------------------------------------\n\n");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //----Table Code Generater Function------
        private string generateTable()
        {
            StringBuilder table = new StringBuilder();
            try
            {
                //------HTML Code--------
                table.AppendLine($"--------------------Table HTML Code starts here----------------\n\n");
                table.AppendLine($"<div class=\"pt-2\">");
                table.AppendLine($"    <div class=\"card\">");
                table.AppendLine($"      <div class=\"card-header row\">");
                table.AppendLine($"        <div class=\"col-xs-12 col-sm-6 card-title\">{this.FormName} List</div>");
                table.AppendLine($"        <div class=\"col-xs-12 col-sm-6\">");
                table.AppendLine($"          <button type=\"button\" class=\"btn pull-right search\">");
                table.AppendLine($"            <span class=\"me-2\"><i class=\"fa fa-plus\"></i></span>Add {this.FormName}");
                table.AppendLine($"          </button>");
                table.AppendLine($"        </div>");
                table.AppendLine($"      </div>");
                table.AppendLine($"      <div class=\"card-body\">");
                table.AppendLine($"        <form name=\"searchForm\" id=\"searchForm\" autocomplete=\"off\" #searchForm=\"ngForm\">");
                table.AppendLine($"          <div class=\"row\">");
                table.AppendLine($"            <div class=\"w-100 ps-2\">");
                table.AppendLine($"              <div class=\"pb-2\">");
                table.AppendLine($"                <div class=\"input-group has-search\">");
                table.AppendLine($"                  <span class=\"fa fa-search form-control-search\"></span>");
                table.AppendLine($"                  <input #search=\"ngModel\" (keydown.enter)=\"onFilterGrid(searchForm)\" type=\"text\" [(ngModel)]=\"AutoGridPaging.SearchText\" placeholder=\"Search...\" name=\"search\" id=\"search\" class=\"form-control search-bar-box\">\r\n");
                table.AppendLine($"                  <span>");
                table.AppendLine($"                    <button type=\"button\" class=\"px-3 py-2 me-2 search\" (click)=\"onFilterGrid(searchForm)\">");
                table.AppendLine($"                      <i class=\"fa fa-search\"></i>");
                table.AppendLine($"                    </button>");
                table.AppendLine($"                    <button type=\"button\" class=\"px-3 py-2 me-2 search\" (click)=\"onRefreshFilterGrid()\">");
                table.AppendLine($"                      <i class=\"fa fa-refresh\"></i>");
                table.AppendLine($"                    </button>");
                table.AppendLine($"                  </span>");
                table.AppendLine($"                </div>");
                table.AppendLine($"              </div>");
                table.AppendLine($"            </div>");
                table.AppendLine($"          </div>");
                table.AppendLine($"        </form>");
                table.Append($"        <AutoGrid [Columns]=\"TableHeader\" [TotalPages]=\"AutoGridPaging.TotalPages\" [TotalRows]=\"AutoGridPaging.TotalRows\" [PageSize]=\"AutoGridPaging.PageSize\" [PageIndex]=\"AutoGridPaging.PageIndex\" [SortField]=\"AutoGridPaging.SortField\" [SortType]=\"AutoGridPaging.SortType\" [EditLink]=\"DataTableEditLink\"");
                if ((bool)chkAllowEdit.IsChecked)
                {
                    table.Append($" [EditAllow]=\"true\"");
                }
                else
                {
                    table.Append($" [EditAllow]=\"false\"");
                }
                if ((bool)chkAllowDelete.IsChecked)
                {
                    table.Append($" [DeleteAllow]=\"true\"");
                }
                else
                {
                    table.Append($" [DeleteAllow]=\"false\"");
                }
                table.Append($">\n");
                table.AppendLine($"        </AutoGrid>");
                table.AppendLine($"      </div>");
                table.AppendLine($"    </div>");
                table.AppendLine($"</div>\n\n");
                table.AppendLine($"--------------------Table HTML Code End here----------------\n\n\n");
                //------Java Script Code-------
                table.AppendLine($"--------------------Table Java Script Code Starts here----------------\n\n\n");
                table.AppendLine($"  @ViewChild(AutoGridComponent) private _AutoGrid!: AutoGridComponent;");
                table.AppendLine($" public AutoGridPaging: Paging = new Paging();");
                table.AppendLine($" public DataTableEditLink = '/user-edit';");
                table.AppendLine($"  public TableHeader: any = [");
                //---------generate columns----------
                foreach (var field in Fields)
                {
                    if (field.IsShow)
                    {
                        table.AppendLine($"    {{");
                        table.AppendLine($"      colName: '{field.Name}',");
                        if (field.IsSortable)
                            table.AppendLine($"      sortable: true,");
                        else
                            table.AppendLine($"      sortable: false,");
                        table.AppendLine($"      sortableFieldName: '{field.Name}',");
                        table.AppendLine($"      colDisplayName: '{field.Label}',");
                        table.AppendLine($"      visible: true,");
                        table.AppendLine($"    }},");
                    }
                }
                table.AppendLine($"  ];");
                table.AppendLine($"  public data = [];");
                table.AppendLine($"  constructor(private _toasterService: ToastrService) {{}}");
                //ngOnInit() function
                table.AppendLine($"  ngOnInit() {{\r\n    " +
                    $"this.onInitLoadData(\r\n      " +
                    $"this.AutoGridPaging.PageIndex,\r\n      " +
                    $"this.AutoGridPaging.PageSize,\r\n      " +
                    $"this.AutoGridPaging.SearchText,\r\n      " +
                    $"this.AutoGridPaging.SortField,\r\n      " +
                    $"this.AutoGridPaging.SortType\r\n    " +
                    $");\r\n  }}");
                //ngAfterViewInit() function
                table.AppendLine($"  ngAfterViewInit() {{\r\n    this._AutoGrid.RowDeleted$.subscribe((c) => {{\r\n      " +
                    $"// this._loadingService.displayLoader(true);\r\n      " +
                    $"// this._apiService.DeleteData(c.RowObject.Id).subscribe(\r\n      " +
                    $"//   (result: any) => {{\r\n      " +
                    $"//     this._loadingService.displayLoader(false);\r\n      " +
                    $"//     if (result.status == 200) {{\r\n      " +
                    $"//       this.data = result.body;\r\n      " +
                    $"//     }}\r\n      " +
                    $"//   }},\r\n      " +
                    $"//   (error: any) => {{\r\n      " +
                    $"//     this._loadingService.displayLoader(false);\r\n      " +
                    $"//     this._errorService.errorHandler(error);\r\n      " +
                    $"//   }}\r\n      " +
                    $"// );\r\n    }});");
                //---------this._AutoGrid.SortChanged$.subscribe((c)----------
                table.AppendLine($"    this._AutoGrid.SortChanged$.subscribe((c) => {{\r\n      " +
                    $"this.AutoGridPaging.SortField = c.SortField;\r\n      " +
                    $"this.AutoGridPaging.SortType = c.SortType;\r\n      " +
                    $"this.AutoGridPaging.PageIndex = 0;\r\n      " +
                    $"this.onInitLoadData(\r\n        " +
                    $"this.AutoGridPaging.PageIndex,\r\n        " +
                    $"this.AutoGridPaging.PageSize,\r\n        " +
                    $"this.AutoGridPaging.SearchText,\r\n        " +
                    $"this.AutoGridPaging.SortField,\r\n        " +
                    $"this.AutoGridPaging.SortType\r\n      " +
                    $");\r\n    " +
                    $"}});");
                //-------this._AutoGrid.PageIndexChanged$.subscribe((c)-------
                table.AppendLine($"    this._AutoGrid.PageIndexChanged$.subscribe((c) => {{\r\n      " +
                    $"if (c != this.AutoGridPaging.PageIndex - 1) {{\r\n        " +
                    $"this.AutoGridPaging.PageIndex = c;\r\n        " +
                    $"this.onInitLoadData(\r\n          " +
                    $"this.AutoGridPaging.PageIndex,\r\n          " +
                    $"this.AutoGridPaging.PageSize,\r\n          " +
                    $"this.AutoGridPaging.SearchText,\r\n          " +
                    $"this.AutoGridPaging.SortField,\r\n          " +
                    $"this.AutoGridPaging.SortType\r\n        " +
                    $");\r\n      " +
                    $"}} else this._AutoGrid.LoadData([]);\r\n    " +
                    $"}});");
                //--------this._AutoGrid.PageSizeChanged$.subscribe(()------
                table.AppendLine($"    this._AutoGrid.PageSizeChanged$.subscribe((c) => {{\r\n      " +
                    $"if (c != this.AutoGridPaging.PageSize) {{\r\n        " +
                    $"this.AutoGridPaging.PageSize = c;\r\n        " +
                    $"this.AutoGridPaging.PageIndex = 0;\r\n        " +
                    $"this.onInitLoadData(\r\n          " +
                    $"this.AutoGridPaging.PageIndex,\r\n          " +
                    $"this.AutoGridPaging.PageSize,\r\n          " +
                    $"this.AutoGridPaging.SearchText,\r\n          " +
                    $"this.AutoGridPaging.SortField,\r\n          " +
                    $"this.AutoGridPaging.SortType\r\n        " +
                    $");\r\n      " +
                    $"}} else this._AutoGrid.LoadData([]);\r\n    " +
                    $"}});\r\n  " +
                    $"}}");
                //-------onInitLoadData()--------
                table.AppendLine($"  onInitLoadData(\r\n    " +
                    $"PageIndex: number,\r\n    " +
                    $"PageSize: number,\r\n    " +
                    $"SearchText: string,\r\n    " +
                    $"SortField: string,\r\n    " +
                    $"SortType: number\r\n  ) " +
                    $"{{\r\n    " +
                    $"// this._loadingService.displayLoader(true);\r\n    " +
                    $"// PageIndex = PageIndex + 1;\r\n    " +
                    $"// this._apiService.GetData(AccountId, PageIndex, PageSize, SearchText, SortField, SortType).subscribe(\r\n    " +
                    $"//   (result: any) => {{\r\n    " +
                    $"//     this._loadingService.displayLoader(false);\r\n    " +
                    $"//     if (result.status == 200) {{\r\n    " +
                    $"//       this.handleData(result);\r\n    " +
                    $"//     }}\r\n    " +
                    $"//   }},\r\n    " +
                    $"//   (error: any) => {{\r\n    " +
                    $"//     this._loadingService.displayLoader(false);\r\n    " +
                    $"//     this._errorService.errorHandler(error);\r\n    " +
                    $"//   }}\r\n    " +
                    $"// );\r\n    " +
                    $"// this.AutoGridPaging.PageIndex = 0;\r\n    " +
                    $"// this.AutoGridPaging.PageSize = 10;\r\n    " +
                    $"// this.AutoGridPaging.TotalRows = this.data.length;\r\n    " +
                    $"// this.AutoGridPaging.TotalPages = Math.ceil(this.data.length / 10);\r\n    " +
                    $"setTimeout(() => {{\r\n      " +
                    $"this._AutoGrid.LoadData(this.data);\r\n    " +
                    $"}}, 2000);\r\n  " +
                    $"}}");
                //-------handleData()------
                table.AppendLine($"handleData(result: any) {{\r\n    " +
                    $"this.data = result.body.Data;\r\n    " +
                    $"if (this.data.length == 0) {{\r\n      " +
                    $"let searchText = this.AutoGridPaging.SearchText;\r\n      " +
                    $"this.AutoGridPaging = new Paging;\r\n      " +
                    $"this.AutoGridPaging.SearchText = searchText;\r\n      " +
                    $"this._AutoGrid.LoadData([]);\r\n      return;\r\n    " +
                    $"}}\r\n    if (result.body.ResponseParameter.PageIndex)\r\n      " +
                    $"this.AutoGridPaging.PageIndex = result.body.ResponseParameter.PageIndex;\r\n    " +
                    $"if (result.body.TotalPages)\r\n      " +
                    $"this.AutoGridPaging.TotalPages = result.body.TotalPages;\r\n    " +
                    $"if (result.body.TotalRows)\r\n      " +
                    $"this.AutoGridPaging.TotalRows = result.body.TotalRows;\r\n    " +
                    $"if (result.body.ResponseParameter.PageSize)\r\n      " +
                    $"this.AutoGridPaging.PageSize = result.body.ResponseParameter.PageSize;\r\n    " +
                    $"if (result.body.ResponseParameter.SortField)\r\n      " +
                    $"this.AutoGridPaging.SortField = result.body.ResponseParameter.SortField;\r\n    " +
                    $"if (result.body.ResponseParameter.SortType)\r\n      " +
                    $"this.AutoGridPaging.SortType = result.body.ResponseParameter.SortType;\r\n    " +
                    $"this._AutoGrid.LoadData(this.data);\r\n  " +
                    $"}}");
                //-----onFilterGrid()------
                table.AppendLine($" onFilterGrid(tenantForm: any) {{\r\n    " +
                    $"this.AutoGridPaging.SearchText = this.AutoGridPaging.SearchText.trim();\r\n    " +
                    $"if (tenantForm.invalid) return;\r\n    " +
                    $"if (this.AutoGridPaging.SearchText.length < 2) {{\r\n      " +
                    $"this._toasterService.warning('Enter minimum 2 letters');\r\n      " +
                    $"return;\r\n    }}\r\n \r\n    " +
                    $"this.AutoGridPaging.PageIndex = 0;\r\n    " +
                    $"this.onInitLoadData(\r\n      " +
                    $"this.AutoGridPaging.PageIndex,\r\n      " +
                    $"this.AutoGridPaging.PageSize,\r\n      " +
                    $"this.AutoGridPaging.SearchText,\r\n      " +
                    $"this.AutoGridPaging.SortField,\r\n      " +
                    $"this.AutoGridPaging.SortType\r\n    " +
                    $");\r\n  " +
                    $"}}");
                //-------onRefreshFilterGrid()-------
                table.AppendLine($"  onRefreshFilterGrid() {{\r\n    " +
                    $"this.AutoGridPaging.SearchText = '';\r\n    " +
                    $"this.AutoGridPaging.PageIndex = 0;\r\n    " +
                    $"this.onInitLoadData(\r\n      " +
                    $"this.AutoGridPaging.PageIndex,\r\n      " +
                    $"this.AutoGridPaging.PageSize,\r\n      " +
                    $"this.AutoGridPaging.SearchText,\r\n      " +
                    $"this.AutoGridPaging.SortField,\r\n      " +
                    $"this.AutoGridPaging.SortType\r\n    " +
                    $");\r\n  " +
                    $"}} \n\n\n");
                table.AppendLine($"--------------------Table Java Script Code End here----------------\n\n\n");
                return table.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
