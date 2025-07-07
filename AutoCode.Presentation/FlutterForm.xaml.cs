using AutoCode.Presentation.Model;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AutoCode.Presentation
{
    public partial class FlutterForm : Window
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string FormName { get; set; }
        private string controllerName;
        string listName;
        private ObservableCollection<FieldProperties> Fields { get; set; }
        public List<string> FieldTypes { get; set; }
        public FlutterForm(List<string> flutterParamList)
        {
            try
            {
                log.Info("Enter into Flutter Form.xaml file.");
                InitializeComponent();
                if (flutterParamList.Count > 1)
                {
                    this.FormName = flutterParamList[0];
                    controllerName = FormName + "Controller";
                    listName = char.ToLower(FormName[0]) + FormName.Substring(1) + "List";
                    this.Fields = JsonConvert.DeserializeObject<ObservableCollection<FieldProperties>>(flutterParamList[1]);
                    setFieldValues();
                    dataGrid.ItemsSource = this.Fields;
                    this.FieldTypes = new List<string>
                {
                    "TextBox", "Dropdown", "RadioButton", "CheckBox", "Button", "Text", "Calendar", "Switch","Email","Number"
                };
                }
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                log.Error("Exception : " + ex.Message);
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
                log.Info("Clicked on Generate Button.");
                if (formCheckBox.IsChecked == true)
                {
                    txtBlockCode.Text = string.Empty;
                    //create form
                    txtBlockCode.Text += generateForm();
                }
                if (listCheckBox.IsChecked == true)
                {
                    txtBlockListCode.Text = string.Empty;
                    //create list
                        txtBlockListCode.Text += generateListUI();
                }
                if (notificationCheckBox.IsChecked == true)
                {
                    txtBlockCode.Text = string.Empty;
                    //create notification
                    txtBlockCode.Text += generateNotification();
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception on Generate Button clicked : " + ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        private void setFieldValues()
        {
            foreach (var field in this.Fields)
            {
                if (field.Type.IndexOf("string", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("long", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("double", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    field.FieldType = "TextBox";
                }
                else if (field.Type.IndexOf("bool", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "Switch";
                else if (field.Type.IndexOf("datetime", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0
                    || field.Type.IndexOf("timespan", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "Calender";
                else
                    field.FieldType = "TextBox";
                if (field.Name.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
                    field.FieldType = "Email";
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
        private string generateListUI()
        {
            StringBuilder list = new StringBuilder();
            try
            {
                list.AppendLine($"-------------------- Start UI Code for List ----------------------\n\n\n");
                //import packages
                list.AppendLine($"import 'package:flutter/material.dart';");
                list.AppendLine($"import 'package:flutter/services.dart';");
                list.AppendLine($"import 'package:get/get.dart';");
                list.AppendLine($"import 'dashboard_view_controller.dart';");
                //import commented classes
                list.AppendLine($"// import '../../const/app_color.dart';");
                list.AppendLine($"// import '../../navigation/pages.dart';");
                list.AppendLine($"// import '../../utility/local_db.dart';");
                list.AppendLine($"class {FormName} extends GetView<{controllerName}> {{");
                list.AppendLine($"  const {FormName}({{super.key}});");
                //@override
                list.AppendLine($" @override\r\n  " +
                    $"Widget build(BuildContext context) {{");
                list.AppendLine($"    Get.put({controllerName}());");  //get.put(controller)
                list.AppendLine($"return Scaffold(\r\n      " +
                    $"appBar: AppBar(),\r\n      " +
                    $"body:");
                //Obx code start from here
                list.AppendLine($"Obx\r\n       " +
                    $"(\r\n        " +
                    $"() =>");
                //loader code 
                list.AppendLine($"controller.isLoading.value\r\n              " +
                    $"? SizedBox(\r\n                  " +
                    $"height: MediaQuery.of(context).size.height - 100,\r\n                  " +
                    $"width: MediaQuery.of(context).size.width,\r\n                  " +
                    $"child: const Center(\r\n                    " +
                    $"child: CircularProgressIndicator.adaptive(\r\n                      " +
                    $"backgroundColor: Colors.red,\r\n                    " +
                    $"),\r\n                  " +
                    $"),\r\n                " +
                    $")");
                //SingleChildScrollView
                list.AppendLine($": SingleChildScrollView(\r\n                  " +
                    $"child: Column(\r\n                    " +
                    $"children: [");
                list.AppendLine($" Container(\r\n                        " +
                    $"color: Colors.pink,\r\n                        " +
                    $"height: Get.height / 12,\r\n                        " +
                    $"child: Row(\r\n                          " +
                    $"mainAxisAlignment: MainAxisAlignment.spaceEvenly,\r\n                          " +
                    $"children: [");
                //search box
                list.AppendLine($"Expanded(\r\n                              " +
                    $"child: TextFormField(\r\n                                " +
                    $"inputFormatters: [\r\n                                  " +
                    $"FilteringTextInputFormatter.deny(\r\n                                      " +
                    $"RegExp(r'[;%*\"]')),\r\n                                  " +
                    $"FilteringTextInputFormatter.deny(\r\n                                      " +
                    $"RegExp(r\"[']\")),\r\n                                " +
                    $"],\r\n                                " +
                    $"cursorHeight: 17,\r\n                                " +
                    $"cursorWidth: 1,\r\n                                " +
                    $"cursorColor: Colors.black,\r\n                                " +
                    $"controller: controller.searchController,\r\n                                " +
                    $"onFieldSubmitted: (value) => {{\r\n                                  " +
                    $"}},\r\n                                " +
                    $"style: const TextStyle(\r\n                                  " +
                    $"color: Colors.black,\r\n                                  " +
                    $"fontSize: 16.0,\r\n                                  " +
                    $"fontWeight: FontWeight.normal,\r\n                                " +
                    $"),");
                //style for searching tag
                list.AppendLine($"decoration: InputDecoration(\r\n                                  " +
                    $"contentPadding: const EdgeInsets.symmetric(\r\n                                      " +
                    $"vertical: 8, horizontal: 16),\r\n                                  " +
                    $"fillColor: Colors.white,\r\n                                  " +
                    $"filled: true,\r\n                                  " +
                    $"hintText: 'Search',\r\n                                  " +
                    $"hintStyle: const TextStyle(\r\n                                    " +
                    $"color: Colors.grey,\r\n                                    " +
                    $"fontSize: 16.0,\r\n                                  " +
                    $"),\r\n                                  " +
                    $"border: OutlineInputBorder(\r\n                                    " +
                    $"borderRadius: BorderRadius.circular(25.0),\r\n                                    " +
                    $"borderSide:\r\n                                        " +
                    $"const BorderSide(color: Colors.blue),\r\n                                  " +
                    $"),\r\n                                  " +
                    $"enabledBorder: OutlineInputBorder(\r\n                                    " +
                    $"borderRadius: BorderRadius.circular(25.0),\r\n                                    " +
                    $"borderSide:\r\n                                        " +
                    $"const BorderSide(color: Colors.grey),\r\n                                  " +
                    $"),\r\n                                  " +
                    $"focusedBorder: OutlineInputBorder(\r\n                                    " +
                    $"borderRadius: BorderRadius.circular(25.0),\r\n                                    " +
                    $"borderSide:\r\n                                        " +
                    $"const BorderSide(color: Colors.blue),\r\n                                  " +
                    $"),\r\n                                " +
                    $"),\r\n                                " +
                    $"onChanged: (value) {{\r\n                                  " +
                    $"// do something with the search value\r\n                                " +
                    $"}},\r\n                              " +  
                    $"),\r\n                            " + //closing tag of textformfield
                    $"),"); //closing tag of expanded
                list.AppendLine($"const SizedBox(\r\n                              " +
                    $"width: 12,\r\n                            " +
                    $"),");
                //sorting icon
                list.AppendLine($"InkWell(\r\n                              " +
                    $"onTap: () {{\r\n                                " +
                    $"print('onTappp');\r\n                              " +
                    $"}},\r\n                              " +
                    $"child: const Icon(\r\n                                " +
                    $"Icons.sort_rounded,\r\n                                " +
                    $"color: Colors.black,\r\n                              " +
                    $"),\r\n                            " +
                    $"),");
                list.AppendLine($"const SizedBox(\r\n                              " +
                    $"width: 6,\r\n                            " +
                    $"),");
                //filtering icon
                list.AppendLine($"InkWell(\r\n                              " +
                    $"onTap: () {{}},\r\n                              " +
                    $"child: const Icon(\r\n                                " +
                    $"Icons.filter_alt,\r\n                                " +
                    $"color: Colors.black,\r\n                              " +
                    $"),\r\n                            " +
                    $"),");
                list.AppendLine($"                          ],");//childern closing tag
                list.AppendLine($"                        ),");//row closing tag
                list.AppendLine($"                            ),"); //container closing tag
                //if list will empty
                list.AppendLine($"controller.{listName}.isEmpty\r\n                          " +
                    $"? const Center(\r\n                              " +
                    $"child: Text(\r\n                                " +
                    $"'No data found',\r\n                                " +
                    $"style: TextStyle(\r\n                                  " +
                    $"fontSize: 14,\r\n                                  " +
                    $"color: Colors.black,\r\n                                " +
                    $"),\r\n                              " +
                    $"),\r\n                            " +
                    $")");  //center closing tag
                //listView.Builder
                list.AppendLine($": ListView.builder(\r\n                              " +
                    $"padding: const EdgeInsets.fromLTRB(\r\n                                " +
                    $"0,\r\n                                " +
                    $"12,\r\n                                " +
                    $"0,\r\n                                " +
                    $"0,\r\n                              " +
                    $"),\r\n                              " +
                    $"shrinkWrap: true,\r\n                              " +
                    $"physics: const AlwaysScrollableScrollPhysics(),\r\n                              " +
                    $"controller: controller.scrollController,");
                list.AppendLine($"itemCount: controller.{listName}.length,");   //listLength
                //itemBuilder
                list.AppendLine($"itemBuilder: (\r\n                                " +
                    $"BuildContext context,\r\n                                " +
                    $"int index,\r\n                              " +
                    $") {{");
                //if condition inside itembuilder
                list.AppendLine($"if (index <= controller.{listName}.length ||\r\n                                    " +
                    $"controller.isPageLoader.value == false) {{");  //children 
                //return container of if block
                list.AppendLine($"return Container(\r\n                                    " +
                    $"margin: const EdgeInsets.only(\r\n                                      " +
                    $"bottom: 12,\r\n                                    " +
                    $"),");
                //decoratedBox
                list.AppendLine($"child: DecoratedBox(\r\n                                      " +
                    $"decoration: BoxDecoration(\r\n                                        " +
                    $"color: Colors.grey,\r\n                                        " +
                    $"borderRadius: BorderRadius.circular(12),\r\n                                      " +
                    $"),");
                //row inside decoratedBox 
                list.AppendLine($"child: Row(\r\n                                        " +
                    $"mainAxisAlignment:\r\n                                            " +
                    $"MainAxisAlignment.spaceBetween,\r\n                                        " +
                    $"crossAxisAlignment:\r\n                                            " +
                    $"CrossAxisAlignment.center,\r\n                                        " +
                    $"children: [");
                //container for show data in row
                list.AppendLine($"Container(\r\n                                            " +
                    $"width: Get.width * 0.60,\r\n                                            " +
                    $"margin: const EdgeInsets.only(\r\n                                              " +
                    $"left: 12,\r\n                                            " +
                    $"),\r\n                                            " +
                    $"child: Column(\r\n                                              " +
                    $"mainAxisAlignment:\r\n                                                  " +
                    $"MainAxisAlignment.start,\r\n                                              " +
                    $"crossAxisAlignment:\r\n                                                  " +
                    $"CrossAxisAlignment.start,\r\n                                              " +
                    $"children: [");
                //textbox for show data 
                foreach (var item in this.Fields)
                {
                    if (item.IsShow == true)
                    {
                        list.AppendLine($"Text(\r\n                                                  " +
                            $"controller.{listName}[index]['{item.Name}'].toString(),\r\n                                                  " +
                            $"style: const TextStyle(\r\n                                                    " +
                            $"fontSize: 16,\r\n                                                    " +
                            $"color: Colors.black,\r\n                                                    " +
                            $"fontStyle: FontStyle.normal,\r\n                                                    " +
                            $"fontWeight: FontWeight.w500,\r\n                                                  " +
                            $"),\r\n                                                " +
                            $"),"); //closing tag of text
                        list.AppendLine($"const SizedBox(\r\n                                                  " +
                            $"height: 4,\r\n                                                " +
                            $"),");
                    }
                }                
                list.AppendLine($"                                              ],");//closing tag of children
                list.AppendLine($"                                            ),");//closing tag of children
                list.AppendLine($"                                          ),");//closing tag of children
                //complete text here ^

                if ((bool)chkAllowEdit.IsChecked || (bool)chkAllowDelete.IsChecked)
                {
                    //create another inside container of show data
                    list.AppendLine($"Row(\r\n                                            " +
                        $"mainAxisAlignment:\r\n                                                " +
                        $"MainAxisAlignment.start,\r\n                                            " +
                        $"crossAxisAlignment:\r\n                                                " +
                        $"CrossAxisAlignment.center,\r\n                                            " +
                        $"children: [");
                    if ((bool)chkAllowEdit.IsChecked)
                    {
                        list.AppendLine($"InkWell(\r\n                                                " +
                            $"onTap: () {{}},\r\n                                                " +
                            $"child: const Icon(\r\n                                                  " +
                            $"Icons.edit,\r\n                                                  " +
                            $"color: Colors.blue,\r\n                                                " +
                            $"),\r\n                                              " +
                            $"),\r\n                                              " +
                            $"const SizedBox(\r\n                                                " +
                            $"width: 4,\r\n                                              " +
                            $"),");
                    }
                    if ((bool)chkAllowDelete.IsChecked)
                    {
                        list.AppendLine($"InkWell(\r\n                                                " +
                            $"onTap: () {{}},\r\n                                                " +
                            $"child: const Icon(\r\n                                                  " +
                            $"Icons.delete,\r\n                                                  " +
                            $"color: Colors.red,\r\n                                                " +
                            $"),\r\n                                              " +
                            $"),\n" +
                            $"const SizedBox(\r\n                                                " +
                            $"width: 4,\r\n                                              " +
                            $"),");
                    }
                    list.AppendLine($"                                            ],");//closing tag of children
                    list.AppendLine($"                                          ),");//closing tag of row
                    list.AppendLine($"");
                }
                list.AppendLine($"                                        ],");  //closing tag of children
                list.AppendLine($"                                      ),");    //closing tag of row
                list.AppendLine($"                                    ),");      //closing tag of decoratedBox
                list.AppendLine($"                                  );");        //closing tag of container
                list.AppendLine($"                                }}");   //closing tag of if block to check the condition of length

                //else Block for processing
                list.AppendLine($"else {{\r\n                                  " +
                    $"return SizedBox(\r\n                                    " +
                    $"height: 20,\r\n                                    " +
                    $"width: Get.width,\r\n                                    " +
                    $"child: const Center(\r\n                                      " +
                    $"child: Padding(\r\n                                        " +
                    $"padding: EdgeInsets.all(8.0),\r\n                                        " +
                    $"child: CircularProgressIndicator(\r\n                                          " +
                    $"strokeWidth: 2,\r\n                                          " +
                    $"color: Colors.blue,\r\n                                        " +
                    $"),\r\n                                      " +
                    $"),\r\n                                    " +
                    $"),\r\n                                  " +
                    $");\r\n                                " +
                    $"}}");
                //else block complete
                list.AppendLine($"                              }},");  //closing tag of {}
                list.AppendLine($"                            )");      //closing tag of listViewControl
                list.AppendLine($"                    ],");            //closing tag of children
                list.AppendLine($"                  ),");              //closing tag of column
                list.AppendLine($"                ),");                //closing tag of singleChildScrollviewer
                list.AppendLine($"      ),");                           //closing tag of Obx
                list.AppendLine($"    );");                           //closing tag of Scaffold
                list.AppendLine($"  }}");                              //closing tag of  Widget
                list.AppendLine($"}}\n\n\n");                          //closing tag of class DashboardView
                list.AppendLine($"---------------------- End UI Code of List -----------------------\n\n\n");
                list.AppendLine("-------------------------------- Start Model Code ------------------------------------\n\n");
                //import packages
                list.AppendLine($"import 'package:flutter/material.dart';\r\n" +
                    $"import 'package:get/get.dart';");
                //commented packages
                list.AppendLine($"// import '../../controller/api_controller.dart';\r\n" +
                    $"// import '../../controller/home_controller.dart';");
                //create controller class
                list.AppendLine($"class {controllerName} extends GetxController\r\n    " +
                    $"with WidgetsBindingObserver {{");
                //commented controllers
                list.AppendLine($"  // HomeController homeController = Get.put(HomeController());\r\n  " +
                    $"// ApiController apiController = Get.put(ApiController());");
                //scroll controller
                list.AppendLine($"  ScrollController scrollController = ScrollController();");
                //searching textField
                list.AppendLine($"  TextEditingController searchController = TextEditingController();");
                //isLoading and isPageLoading
                list.AppendLine($"  RxBool isLoading = true.obs;\r\n  " +
                    $"RxBool isPageLoader = false.obs;");
                //pageIndex AND pageSize 
                list.AppendLine($"  RxInt pageIndex = 1.obs;\r\n  " +
                    $"RxInt pageSize = 50.obs;");
                //generate list
                list.AppendLine($"RxList<dynamic> {listName} = [\r\n");
                //list code here
                list.AppendLine($"  ].obs;"); //closing tag of list
                //onInit()
                list.AppendLine($"@override\r\n  " +
                    $"void onInit() async {{\r\n    " +
                    $"WidgetsBinding.instance.addObserver(this);\r\n    " +
                    $"super.onInit();\r\n    " +
                    $"scrollController.addListener(pagination);\r\n    " +
                    $"Future.delayed(const Duration(seconds: 5)).then(\r\n      " +
                    $"(value) {{\r\n        " +
                    $"isLoading.value = false;\r\n      " +
                    $"}},\r\n    " +
                    $");\r\n    " +
                    $"getListApiCall(); // Api Call\r\n  " +
                    $"}}");  //closing tag of onInit()

                //method for pagination
                list.AppendLine($"pagination() async {{\r\n    " +
                    $"if (scrollController.position.pixels ==\r\n        " +
                    $"scrollController.position.maxScrollExtent) {{\r\n      " +
                    $"update();\r\n      " +
                    $"isPageLoader.value = true;\r\n      " +
                    $"update();\r\n      " +
                    $"if (pageIndex < 1) {{\r\n        " +
                    $"// if (pageIndex < apiResponse.data.dashboardList.totalPages) {{\r\n        " +
                    $"pageIndex = pageIndex + 1;\r\n        " +
                    $"// Api call\r\n        " +
                    $"getListApiCall();\r\n      " +
                    $"}}\r\n      " +
                    $"isPageLoader.value = false;\r\n      " +
                    $"update();\r\n    " +
                    $"}}\r\n  " +
                    $"}}");
                list.AppendLine($"  getListApiCall() async {{}}");
                list.AppendLine($"}}");  //closing tag for controller
                list.AppendLine("------------------------------- End Model Code ----------------------------------\n\n\n");
                return list.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string generateForm()
        {
            StringBuilder form = new StringBuilder();
            try
            {
                form.AppendLine($"------------------------Form Code Start Here----------------\n\n\n");
                form.AppendLine($"import 'package:flutter/material.dart';");
                form.AppendLine($"import 'package:flutter/services.dart';");
                form.AppendLine($"import 'package:get/get.dart';");
                form.AppendLine($"void main() => runApp({FormName}());\n");
                //class _TextFormFieldCustomState
                form.AppendLine($"class {FormName} extends GetView<{controllerName}> {{");
                form.AppendLine($"  const {FormName}({{super.key}});");        
                form.AppendLine($" @override\r\n  " +
                    $"Widget build(BuildContext context) {{\r\n    " +
                    $"    Get.put({controllerName}());" +
                    $"return MaterialApp(\r\n      " +
                    $"home: Scaffold(\r\n        " +
                    $"backgroundColor: const Color.fromARGB(255, 222, 221, 221),\r\n        " +
                    $"appBar: AppBar(\r\n          " +
                    $"centerTitle: true,\r\n          " +
                    $"title: const Text('{this.FormName}'),\r\n          " +
                    $"backgroundColor: Colors.blue,\r\n        " +
                    $"),");
                //body starts from here 
                form.AppendLine($"body: Obx(" +
                    $" () => SingleChildScrollView(\r\n          " +
                    $"padding: const EdgeInsets.all(16),\r\n          " +
                    $"child: Form(\r\n            " +
                    $"key: controller.formKey,\r\n            " +
                    $"child: Column(\r\n              " +
                    $"crossAxisAlignment: CrossAxisAlignment.start,\r\n              " +
                    $"children: [");
                foreach (var item in this.Fields)
                {
                    //"TextBox", "Dropdown", "RadioButton", "CheckBox", "Button", "Text", "Calendar", "Switch"
                    //------TextBox------
                    if (item.FieldType == "TextBox")
                    {
                        form.AppendLine($"const SizedBox(height: 10),");
                        form.AppendLine($"TextFormField(\r\n                  " +
                            $"controller: controller.{item.Name},\r\n                  " +
                            $"decoration: const InputDecoration(labelText: '{item.Label}'),");
                        //required validator
                        if (item.IsRequired == true)
                        {
                            form.AppendLine($"validator: (value) =>\r\n                      " +
                                $"value!.isEmpty ? '{item.Label} is required' : null,");
                        }
                        form.AppendLine($"),");   // closing tag of TextFormField
                    }
                    //------Email------
                    else if (item.FieldType == "Email")
                    {
                        form.AppendLine($"const SizedBox(height: 10),");
                        form.AppendLine($" TextFormField(\r\n                  " +
                            $"controller: controller.{item.Name},\r\n                  " +
                            $"decoration: const InputDecoration(labelText: '{item.Label}'),\r\n                  " +
                            $"keyboardType: TextInputType.emailAddress,");
                        form.AppendLine($"validator: (value) {{");
                        form.AppendLine($"String email = value.toString();");
                        //required validator
                        if (item.IsRequired == true)
                        {                            
                            form.AppendLine($"if (email.isEmpty) {{\r\n                      " +
                                $"return '{item.Label} is required';\r\n                    " +
                                $"}}");
                        }
                        form.AppendLine($"if (!controller.emailRegex.value.hasMatch(email)) {{return 'Enter valid {item.Label}';}}");
                        form.AppendLine($"return null;");
                        form.AppendLine($" }},");
                        form.AppendLine($"),");   // closing tag of TextFormField
                    }
                    //------Dropdown------
                    else if (item.FieldType == "Dropdown")
                    {
                        form.AppendLine($" const SizedBox(height: 20),");
                        form.AppendLine($" DropdownButtonFormField<String>(\r\n                  " +
                            $"value: controller.selectedValue.isEmpty ? null : controller.selectedValue,\r\n                  " +
                            $"decoration: InputDecoration(");
                        //label for dropdown
                        form.AppendLine($"labelText: '{item.Label}',");
                        form.AppendLine($"border: OutlineInputBorder(\r\n                        " +
                            $"borderRadius: BorderRadius.circular(12)),\r\n                    " +
                            $"focusedBorder: OutlineInputBorder(\r\n                        " +
                            $"borderSide: const BorderSide(color: Colors.blue)),\r\n                  " +
                            $"),");
                        //hint show on dropdown 
                        form.AppendLine($" hint: Text(controller.selectedValue.isEmpty ? controller.items[0] : controller.selectedValue),");
                        //Mapping item with dropdown
                        form.AppendLine($"items: controller.items.map((String item) {{\r\n                    " +
                            $"return DropdownMenuItem<String>(\r\n                      " +
                            $"value: item,\r\n                      " +
                            $"child: Text(item),\r\n                    " +
                            $");\r\n                  " +
                            $"}}).toList(),");
                        if (item.IsRequired == true)
                        {
                            form.AppendLine($" validator: (value) {{\r\n                    " +
                                $"if (value == null || value.isEmpty){{ return 'This field is required';}}\r\n                    " +
                                $"var comp = controller.items[0];\r\n                    " +
                                $"if (comp == value) return 'Select Valid Fruit';\r\n                    " +
                                $"return null;\r\n                  " +
                                $"}},");   
                        }
                        //onChanged()
                        form.AppendLine($"onChanged: (newValue) {{");
                        form.AppendLine($" if (newValue != null) {{");
                        form.AppendLine($" controller.selectedValue = newValue;}}");
                        form.AppendLine($"}}");  //closing tag of onChanged method
                        form.AppendLine($"),");   // closing tag of DropdownButtonFormField
                    }
                    //------RadioButton------
                    else if (item.FieldType == "RadioButton")
                    {
                        form.AppendLine($"const SizedBox(height: 20),");
                        //label for radio button
                        form.AppendLine($" const Text(\r\n                  " +
                            $"\'{item.Label}:\',\r\n                  " +
                            $"style: TextStyle(fontWeight: FontWeight.bold),\r\n                " +
                            $"),");
                        //Row()
                        form.AppendLine($"Row(\r\n                    " +
                            $"children: [");
                        //first radio button
                        form.AppendLine($"Radio(\r\n                        " +
                            $"value: 'Male',\r\n                        " +
                            $"groupValue: controller.{item.Name}.value,");
                        form.AppendLine($"onChanged: (value) {{\r\n                          " +
                            $"if (value != null) {{\r\n                            " +
                            $"controller.gender.value = value;\r\n                          " +
                            $"}}\r\n                        " +
                            $"}},");
                        form.AppendLine($"),"); //first radio button's closing bracket
                        form.AppendLine($"const Text('Male'),");   //label for first radio button
                        //second radio button
                        form.AppendLine($"Radio(\r\n                        " +
                            $"value: 'Female',\r\n                        " +
                            $"groupValue: controller.{item.Name}.value,");
                        form.AppendLine($"onChanged: (value) {{\r\n                          " +
                            $"if (value != null) {{\r\n                            " +
                            $"controller.{item.Name}.value = value;\r\n                          " +
                            $"}}\r\n                        " +
                            $"}},");
                        form.AppendLine($"),"); //second radio button's closing bracket
                        form.AppendLine($"const Text('Female'),");   //label for second radio button
                        if (item.IsRequired == true)
                        {
                            form.AppendLine($"if (controller.{item.Name}.isEmpty)\r\n                  " +
                                $"const Text(\"Please select {item.Label}\",\r\n                      " +
                                $"style: TextStyle(color: Colors.red, fontSize: 12)),");
                        }
                        form.AppendLine($"],");
                        form.AppendLine($"),");
                    }
                    //------CheckBox------
                    else if (item.FieldType == "CheckBox")
                    {
                        //Checkbox(
                        //  value: true,
                        //  onChanged: (value) { },
                        //);
                    }
                    //------Button------
                    else if (item.FieldType == "Button")
                    {
                        form.AppendLine($"const SizedBox(height: 30),");
                        form.AppendLine($" SizedBox(\r\n                  " +
                            $"width: double.infinity,\r\n                  " +
                            $"height: 50,\r\n                  " +
                            $"child: ElevatedButton(");
                        form.AppendLine($"onPressed: () => {{}},");
                        //style to button
                        form.AppendLine($"style: ElevatedButton.styleFrom(\r\n                      " +
                            $"backgroundColor: Colors.blue,\r\n                      " +
                            $"shape: RoundedRectangleBorder(\r\n                          " +
                            $"borderRadius: BorderRadius.circular(12)),\r\n                    " +
                            $"),");
                        //style to text of that button
                        form.AppendLine($"child: const Text('{item.Label}',\r\n                        " +
                            $"style: TextStyle(fontSize: 16, color: Colors.white)),");
                        form.AppendLine($"                  ),"); //button's closing tag
                        form.AppendLine($"                ),"); //sizedbox's closing tag
                    }
                    //------Text------
                    else if (item.FieldType == "Text")
                    {

                    }
                    //------Calendar------
                    else if (item.FieldType == "Calendar")
                    {

                    }
                    //------Switch------
                    else if (item.FieldType == "Switch")
                    {
                        form.AppendLine($"const SizedBox(height: 20),");
                        form.AppendLine($"Row(\r\n                  " +
                            $"mainAxisAlignment: MainAxisAlignment.spaceBetween,\r\n                  " +
                            $"children: [");
                        //Label form Switch
                        form.AppendLine($"const Text(\r\n                      " +
                            $"'{item.Label}',\r\n                      " +
                            $"style: TextStyle(fontSize: 16, color: Colors.black,fontWeight: FontWeight.bold),\r\n                    " +
                            $"),");
                        //Switch Tag
                        form.AppendLine($"Switch(\r\n                      " +
                            $"value:  controller.{item.Name}.value,\r\n");
                        //onChaged()
                        form.AppendLine($"onChanged: (value) {{");
                        form.AppendLine($" controller.{item.Name}.value =\r\n                              " +
                            $"!controller.{item.Name}.value;\r\n                        " +
                            $"}},\r\n                        " +
                            $"activeColor: Colors.green,\r\n                        " +
                            $"inactiveThumbColor: Colors.red,\r\n                      " +
                            $"),");  //switch closing tag
                        form.AppendLine($"],");  //children closing tage
                        form.AppendLine($"),");  //row's closing tag
                    }
                    //------Number------
                    else if (item.FieldType == "Number")
                    {
                        form.AppendLine($"const SizedBox(height: 10),");
                        form.AppendLine($" TextFormField(\r\n                  " +
                            $"controller: controller.{item.Name},\r\n                  " +
                            $"inputFormatters: <TextInputFormatter>[\r\n                    " +
                            $"FilteringTextInputFormatter.digitsOnly\r\n                  " +
                            $"],");
                        form.AppendLine($"decoration: const InputDecoration(labelText: '{item.Label}'),\n");
                        form.AppendLine($"keyboardType: TextInputType.number,\n");
                        if(item.IsRequired == true)
                        {
                            form.AppendLine($" validator: (value) {{\r\n                    " +
                                $"if (value!.isEmpty) {{\r\n                      " +
                                $"return '{item.Label} is required';\r\n                    " +
                                $"}}\r\n                    " +
                                $"return null;\r\n                  " +
                                $"}},");
                        }
                        form.AppendLine($"                ),\n");    //closing tag of TextFormField
                    }
                }
                //----Submit Button-----
                form.AppendLine($"const SizedBox(height: 30),");
                form.AppendLine($"SizedBox(\r\n                  " +
                    $"width: double.infinity,\r\n                  " +
                    $"height: 50,\r\n                  " +
                    $"child: ElevatedButton(\r\n                    " +
                    $"onPressed: () async {{");
                //if condition of button click ------"Success Message Show"-----
                form.AppendLine($"if (controller.formKey.currentState!.validate()) {{\r\n");
                form.AppendLine($"controller.{FormName}Api();");
                form.AppendLine($"}}");  //closing tag of if block
                form.AppendLine($"}},"); // closing tag of onPressed : () async {
                //style for button 
                form.AppendLine($" style: ElevatedButton.styleFrom(\r\n                      " +
                    $"backgroundColor: Colors.blue,\r\n                      " +
                    $"shape: RoundedRectangleBorder(\r\n                          " +
                    $"borderRadius: BorderRadius.circular(12)),\r\n                    " +
                    $"),\r\n                    " +
                    $"child: const Text('Submit',\r\n                        " +
                    $"style: TextStyle(fontSize: 16, color: Colors.white)),");
                form.AppendLine($"                  ),");  //button closing tag
                form.AppendLine($"                )"); //SizedBox closing tag                
                form.AppendLine($"              ],");//closing tage of children[]
                form.AppendLine($"            ),");// closing tag of Column
                form.AppendLine($"          ),");// closing tag of Form
                form.AppendLine($"        ),");// closing tag of sigleChildScrollView
                form.AppendLine($"       ),");// closing tag of Obx
                form.AppendLine($"      ),");// closing tag of Scaffold
                form.AppendLine($"    );");// closing tag of MaterialApp
                form.AppendLine($"  }}");// closing tag of   Widget build(BuildContext context)
                form.AppendLine($"}}");// closing tag of Class _TextFormFieldCustomState
                form.AppendLine($"------------------------Form Code End Here----------------\n\n\n");
                form.AppendLine($"------------------------API Code Starts Here----------------\n\n\n");
                form.AppendLine($"class {controllerName} extends GetxController {{");
                //commented controller
                form.AppendLine($"  // HomeController homeController = Get.put(HomeController());\r\n  " +
                    $"// ApiController apiController = Get.put(ApiController());");

                //variable define inside controller
                foreach (var item in this.Fields)
                {
                    if (item.FieldType == "TextBox")
                    {
                        //Controller for textFields
                        form.AppendLine($"final TextEditingController {item.Name} = TextEditingController();");
                    }
                    else if (item.FieldType == "Email")
                    {
                        //Controller for textFields
                        form.AppendLine($"final TextEditingController {item.Name} = TextEditingController();");
                        form.AppendLine($"final emailRegex = RegExp(r'^(([^<>()[\\]\\\\.,;:\\s@\\\"]+(\\.[^<>()[\\]\\\\.,;:\\s@\\\"]+)*)|(\\\".+\\\"))@((\\[[0-9]{{1,3}}\\.[0-9]{{1,3}}\\.[0-9]{{1,3}}\\.[0-9]{{1,3}}\\])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{{2,}}))$').obs;");
                    }
                    else if (item.FieldType == "Dropdown")
                    {
                        form.AppendLine($"String selectedValue = '';"); //selectedValue Variable for dropDown
                        form.AppendLine($"RxList<String> items =['Please select fruits', 'Apple', 'Banana', 'Watermelon'].obs;");  //dropdown values
                    }
                    else if (item.FieldType == "Number")
                    {
                        form.AppendLine($"final TextEditingController {item.Name} = TextEditingController();\n");
                    }
                    else if (item.FieldType == "RadioButton")
                        form.AppendLine($"RxString {item.Name} = 'Male'.obs;");  //variable for gender
                    else if (item.FieldType == "Switch")
                        form.AppendLine($"RxBool {item.Name} = false.obs;");  //variable for switch tag
                }

                //define formKey
                form.AppendLine($"final formKey = GlobalKey<FormState>();");
                //onInit()
                form.AppendLine($"@override\r\n  " +
                    $"void onInit() {{\r\n    " +
                    $"super.onInit();\r\n  " +
                    $"}}");
                //Api method
                form.AppendLine($"{FormName}Api() async {{");

                form.AppendLine($"print('Inside login api func');");
                // Access form data in console:
                foreach (var item in this.Fields)
                {
                    if (item.FieldType == "TextBox")
                    {
                        form.AppendLine($"print('{item.Label}: ${{{item.Name}.text}}');");
                    }
                    else if (item.FieldType == "Email")
                    {
                        form.AppendLine($"print('{item.Label}: ${{{item.Name}.text}}');");
                    }
                    else if (item.FieldType == "RadioButton")
                    {
                        form.AppendLine($"print('{item.Label}: $gender');");
                    }
                    else if (item.FieldType == "Switch")
                    {
                        form.AppendLine($"print('{item.Label}: ${item.Name}');");
                    }
                    else if (item.FieldType == "Dropdown")
                    {
                        form.AppendLine($"print('{item.Label}: $selectedValue');");
                    }
                    else if (item.FieldType == "Number")
                    {
                        form.AppendLine($"print('{item.Label}: ${{{item.Name}.text}}');");
                    }
                }
                form.AppendLine($"  }}");  // closing bracket of api method
                form.AppendLine($"}}");// closing bracket of controllerClass method
                form.AppendLine($"------------------------API Code Ends Here----------------\n\n\n");
                return form.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string generateNotification()
        {
            StringBuilder notification = new StringBuilder();
            try
            {
                notification.AppendLine($"------------------------Notification Code Start Here----------------\n\n\n");
                notification.AppendLine($"------------------------Notification Code End Here----------------\n\n\n");
                return notification.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void listCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            listCheckBoxs.Visibility = Visibility.Collapsed;
        }
        private void listCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            listCheckBoxs.Visibility = Visibility.Visible;
        }
    }
}
