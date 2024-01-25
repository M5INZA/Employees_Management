using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Data;
using System.IO;
using Microsoft.Win32;
using Microsoft.Reporting.WinForms;
using System.Drawing.Printing;
using System.Windows.Media.Animation;
using Microsoft.Data.Sqlite;

namespace Ahmed_Cp
{

    public partial class MainWindow : Window
    {
        //Mother fonction
        public MainWindow()
        {
            InitializeComponent();
        }

        // Mysql Connection
        SqliteConnection connection = new SqliteConnection
        ("Data Source=CompanyDatabase.db;");

        // one MysqlCommand i Will be Working in all script
        SqliteCommand Cmd = new SqliteCommand();

        //To show Information in DataGrid
        private void FillDatagrid()
        {
            try
            {
                SetCommand("SELECT * FROM employee;");
                DataTable dt = new DataTable();
                dt.Load(Cmd.ExecuteReader());
                SetInfo(dt, Emp_DtGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //to Clear Inputs
        private void ClearData()
        {
            SetCommand("Select max(Emp_Number)+1 from employee");
            //Input in Sql Cmd Is a table
            DataTable dt = new DataTable();
            dt.Load(Cmd.ExecuteReader());
            if (dt.Rows[0][0].ToString() == "")//if We Dont have any Employee information
            {
                txtEmployeeNumber.Text = "1";
            }
            else
            {
                txtEmployeeNumber.Text = dt.Rows[0][0].ToString();
            }
            //Clear Text Boxs Inputs
            txtEmployeeName.Clear();
            Male_Gndr.IsChecked = true;
            dpBirthDate.Text = "";
            txtAddress.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtSalary.Clear();
            //Bach tla3 image Bayda
            Emp_Image.Source = new BitmapImage();
            txtEmployeeName.Focus();
            //
            BtnAdd.IsEnabled = true;
            BtnAdd.Opacity = 1;
            BtnDelete.IsEnabled = false;
            BtnDelete.Opacity = 0.5;
            BtnEdit.IsEnabled = false;
            BtnEdit.Opacity = 0.5;
        }

        //Check inputs is valid
        private bool IsValid()
        {
            bool check = true;

            string message = "";

            // Check the Employee's Name
            if (string.IsNullOrEmpty(txtEmployeeName.Text) && St_Name.IsChecked == true)
            {
                message += "- Employee name cannot be empty!\n";
                check = false;
            }

            // Check Address
            if (string.IsNullOrEmpty(txtAddress.Text) && St_Address.IsChecked == true)
            {
                message += "- Address cannot be empty!\n";
                check = false;
            }

            // Check Birth Date
            if (string.IsNullOrEmpty(dpBirthDate.Text) && St_BirthDate.IsChecked == true)
            {
                message += "- Employee birth date cannot be empty!\n";
                check = false;
            }

            // Check Phone Number
            Regex phoneRegex = new Regex(@"^\+212[5-9](?:\d{8}|\d{1}-\d{2}-\d{2}-\d{2})$");

            if (!phoneRegex.IsMatch(txtPhone.Text) && St_Phone.IsChecked == true)
            {
                message += "- Invalid phone number format! Please use the format: +212699999999\n";
                check = false;
            }

            // Check Email
            Regex emailRegex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$");
            if (!emailRegex.IsMatch(txtEmail.Text) && St_Email.IsChecked == true)
            {
                message += "- Invalid email format!\n";
                check = false;
            }

            // Check Salary
            Regex salaryRegex = new Regex(@"^\d+(\.\d+)?$");
            if (!salaryRegex.IsMatch(txtSalary.Text) && St_Salary.IsChecked == true)
            {
                message += "- Invalid salary format! Please enter a valid number.\n";
                check = false;
            }

            // Check Image
            if (Emp_Image.Source == null && St_Image.IsChecked == true)
            {
                message += "- Employee image cannot be empty!\n";
                check = false;
            }

            if (!check && !string.IsNullOrEmpty(message))
            {
                MessageBox.Show($"Please fix the following issues:\n{message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return check;
        }

        //To Dont Replate Script of connection and Cmmand Text
        private void SetCommand(string SQL)
        {
            Cmd.Connection = connection;
            Cmd.CommandText = SQL;
        }

        // Insert Datatable rows into a DataGrid
        public void SetInfo(DataTable dt,DataGrid dg)
        {
            dt.Columns.Add("ImageString", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                byte[] image = dr["Emp_Image"] as byte[];
                dr["ImageString"] = (image != null && image.Length > 0) ? "Yes" : "No";
            }
            dg.ItemsSource = dt.DefaultView;
        }

        //Search in Table
        private DataTable Searching()
        {
            try
            {
                string SearchCommand = "SELECT * FROM Employee";
                //Hado Makay5admoch b Operation (Is Equale or Older Than.....)
                if (NameRb.IsChecked == true || PhoneRb.IsChecked == true || EmailRb.IsChecked == true || AddressRb.IsChecked == true)
                {
                    SearchCommand += " WHERE";

                    if (NameRb.IsChecked == true) SearchCommand += " Emp_Name";
                    else if (PhoneRb.IsChecked == true) SearchCommand += " Emp_Phone";
                    else if (EmailRb.IsChecked == true) SearchCommand += " Emp_Email";
                    else SearchCommand += " Emp_Address";

                    //'%Ex%' <-- bach ila kan Dakchi li kat9alab 3lih wast Information
                    SearchCommand += " Like '%" + SearchTextBox.Text + "%'";

                    if (MaleCb.IsChecked == true && FemaleCb.IsChecked == false) SearchCommand += " AND Emp_Gender='Male'";
                    else if (MaleCb.IsChecked == false && FemaleCb.IsChecked == true) SearchCommand += " AND Emp_Gender='Female'";

                    if (WhatsAppCb.IsChecked == true) SearchCommand += " AND Emp_Is_WhatsApp = 1";
                    else if (WhatsAppCb.IsChecked == false) SearchCommand += "AND Emp_Is_WhatsApp = 0";
                }
                //Hado kay5admo b Opertion
                if (NumberRb.IsChecked == true || BirthDateRb.IsChecked == true || SalaryRb.IsChecked == true)
                {
                    //Hitach ila makan Walo f search radi it5arba9 Command
                    if (SearchTextBox.Text != "" || Srch_BirthDate.Text != "")
                    {
                        SearchCommand += " WHERE";

                        if (NumberRb.IsChecked == true) SearchCommand += " Emp_Number";
                        else if (SalaryRb.IsChecked == true) SearchCommand += " Emp_Salary";
                        else SearchCommand += " Emp_Birth_Date";

                        if (Rb_Equals.IsChecked == true) SearchCommand += " = ";
                        else if (Rb_NotEquals.IsChecked == true) SearchCommand += " != ";
                        else if (Rb_Younger.IsChecked == true && BirthDateRb.IsChecked == false) SearchCommand += " < ";
                        else if (Rb_Older.IsChecked == true && BirthDateRb.IsChecked == false) SearchCommand += " > ";

                        //Hitach F Date Kaykoun Li 3ado L3am Lkbir howa sghir Like: 2006<1990
                        else if (Rb_Younger.IsChecked == true && BirthDateRb.IsChecked == true) SearchCommand += " > ";
                        else if (Rb_Older.IsChecked == true && BirthDateRb.IsChecked == true) SearchCommand += " < ";

                        if (Srch_BirthDate.Visibility == Visibility.Visible)
                        {
                            string Y = Srch_BirthDate.SelectedDate.Value.Year.ToString();
                            string M = Srch_BirthDate.SelectedDate.Value.Month.ToString();
                            string D = Srch_BirthDate.SelectedDate.Value.Day.ToString();
                            SearchCommand += $"'{Y}-{M}-{D}'";
                        }
                        else
                        {
                            // '' <- Dart Hado bach ila 9alabti 3la chi harf maytla3 walo hitach Salary o Number Gha Ar9am
                            SearchCommand += "'" + SearchTextBox.Text + "'";
                        }
                    }
                }
                SetCommand(SearchCommand);
                DataTable dt = new DataTable();
                dt.Load(Cmd.ExecuteReader());
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // For Moving Bitween Pages
        public void HiddenAll()
        {
            ListGrid.Visibility = Visibility.Hidden;
            ReportGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
        }
        
        private void GetBackupAction()
        {
            if(File.Exists("Backup Actions.txt"))
            {
                StreamReader sr = new StreamReader("Backup Actions.txt");
                BackupTextbox.Text = sr.ReadToEnd();
                sr.Close();
            }
            else
            {
                File.Create("Backup Actions.txt");
            }
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            HiddenAll();
            ListGrid.Visibility = Visibility.Visible;
        }

        //When Windows Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                connection.Open();
                //Set Information in Data Grid
                FillDatagrid();
                //Clear Inputs
                ClearData();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //When Backup Text box Loaded
        private void BackupTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            if (BackupTextbox.IsLoaded == true)
            {
                GetBackupAction();
            }
        }

        //Functions of Check Inputs in Text Box 3la Hasab Kola Wahad
        private void TxtEmployeeName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex Rx = new Regex("^[A-Za-z]$");
            e.Handled = !Rx.IsMatch(e.Text);
        }

        private void TxtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex Rx = new Regex(@"[^0-9\+]$");
            e.Handled = Rx.IsMatch(e.Text);
        }

        private void TxtEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex Rx = new Regex(@"^[0-9a-zA-Z_.@\-\+]$");
            e.Handled = !Rx.IsMatch(e.Text);
        }

        private void TxtSalary_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex Rx = new Regex(@"^[0-9.]+$");
            e.Handled = !Rx.IsMatch(e.Text);
        }

        //Hado li kay 5admo b operation
        private void NumberRb_Checked(object sender, RoutedEventArgs e)
        {
            if (Operation_Border != null)
            {
                Operation_Border.Visibility = Visibility.Visible;
            }

        }

        private void NumberRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Operation_Border != null)
            {
                Operation_Border.Visibility = Visibility.Hidden;
            }
        }

        private void BirthDateRb_Checked(object sender, RoutedEventArgs e)
        {
            Srch_BirthDate.Visibility = Visibility.Visible;
            Operation_Border.Visibility = Visibility.Visible;
            //Hadi Mohima l Searching Function
            SearchTextBox.Text = "";
        }

        private void BirthDateRb_Unchecked(object sender, RoutedEventArgs e)
        {
            Srch_BirthDate.Visibility = Visibility.Hidden;
            Operation_Border.Visibility = Visibility.Hidden;
        }

        private void SalaryRb_Checked(object sender, RoutedEventArgs e)
        {
            if (Operation_Border != null)
            {
                Operation_Border.Visibility = Visibility.Visible;
            }
        }

        private void SalaryRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Operation_Border != null)
            {
                Operation_Border.Visibility = Visibility.Hidden;
            }
        }

        //Export Image Click Function
        private void BtnImageExport_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenFileDialog Ofd = new OpenFileDialog();
                Ofd.Filter = "Image Files|*.jpg;*.png;*.jpeg;";
                bool? result = Ofd.ShowDialog(); // Result == true <-- Click Open.
                if (result == true)
                {
                    BitmapImage newImage = new BitmapImage(new Uri(Ofd.FileName));
                    Emp_Image.Source = newImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //New Button Click Function
        private void BtnNew_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Q_New.IsChecked == true)
                {
                    if (MessageBox.Show("Do you want a new process?", "Program", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        ClearData();
                        FillDatagrid();
                    }
                }
                else
                {
                    ClearData();
                    FillDatagrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Add Employee Function
        private void BtnAdd_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    // This line of Sql Add Command . Table Is |Number,Name,Gender,Address,BirthDate,Phone,IsWatsaap,Email,Salary,Image|
                    string AddCommand = "INSERT INTO employee VALUES (@EmployeeNumber, @EmployeeName, @Gender, @Address, @BirthDate, @Phone, @IsWhatsApp, @Email, @Salary, @Image)";

                    Cmd.Parameters.Clear();

                    //Dert Tag F Command -> @img <- bach ngol lih ila l9iti @img Hate blasteha Image As Bytes
                    Cmd.Parameters.AddWithValue("@EmployeeNumber", Convert.ToInt32(txtEmployeeNumber.Text));
                    Cmd.Parameters.AddWithValue("@EmployeeName", (txtEmployeeName.Text == "") ? (object)DBNull.Value : txtEmployeeName.Text);
                    Cmd.Parameters.AddWithValue("@Gender", (Male_Gndr.IsChecked == true) ? "Male" : "Female");
                    Cmd.Parameters.AddWithValue("@Address", (txtAddress.Text == "") ? (object)DBNull.Value : txtAddress.Text);
                    string Y = "";
                    string M = "";
                    string D = "";
                    if (dpBirthDate.Text != "")
                    {
                        Y = dpBirthDate.SelectedDate.Value.Year.ToString();
                        M = dpBirthDate.SelectedDate.Value.Month.ToString();
                        D = dpBirthDate.SelectedDate.Value.Day.ToString();
                        Cmd.Parameters.AddWithValue("@BirthDate", $"{Y}-{M}-{D}");
                    }
                    else
                    {
                        Cmd.Parameters.AddWithValue("@BirthDate", (object)DBNull.Value);
                    }
                    Cmd.Parameters.AddWithValue("@Phone", (txtPhone.Text == "") ? (object)DBNull.Value : txtPhone.Text);
                    Cmd.Parameters.AddWithValue("@IsWhatsApp", (chkIsWhatsApp.IsChecked == true) ? 1 : 0);
                    Cmd.Parameters.AddWithValue("@Email", (txtEmail.Text == "") ? (object)DBNull.Value : txtEmail.Text);
                    Cmd.Parameters.AddWithValue("@Salary", (txtSalary.Text == "") ? 0 : Convert.ToDouble(txtSalary.Text));
                    
                    //To Save Image We Want to Convert from Image to Byte(010100010) 
                    //So i Create a Memory to Save From My Image tool to Ms
                    if (Emp_Image.Source != null)
                    {
                        MemoryStream Ms = new MemoryStream();
                        BitmapSource Bs = Emp_Image.Source as BitmapSource;
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Bs));
                        encoder.Save(Ms);
                        Cmd.Parameters.AddWithValue("@Image", Ms.ToArray());

                    }
                    else
                    {
                        Cmd.Parameters.AddWithValue("@Image", (object)DBNull.Value);
                    }
                    SetCommand(AddCommand);
                    Cmd.ExecuteNonQuery();
                    //from settings
                    if (N_Add.IsChecked == true)
                    {
                        MessageBox.Show("Added successfully!!", "Program", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    FillDatagrid();
                    ClearData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //When Select in Employee Data Grid
        private void Emp_DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Emp_DtGrid.SelectedItem != null)
                {
                    //Employee Selected Radi Ithat f DataRowView Bach nthakm bih
                    DataRowView dr = Emp_DtGrid.SelectedItem as DataRowView;
                    //Add Number
                    txtEmployeeNumber.Text = dr["Emp_Number"].ToString();
                    //Add Name
                    txtEmployeeName.Text = dr["Emp_Name"].ToString();
                    //check Gender
                    if (dr["Emp_Gender"].ToString() == "Male")
                    {
                        Male_Gndr.IsChecked = true;
                    }
                    else
                    {
                        Female_Gndr.IsChecked = true;
                    }
                    //Add Address
                    txtAddress.Text = dr["Emp_Address"].ToString();
                    //Add birth date value
                    dpBirthDate.Text = dr["Emp_Birth_Date"].ToString();
                    //Add phone
                    txtPhone.Text = dr["Emp_Phone"].ToString();
                    //Check Is Watsaap
                    if (Convert.ToInt32(dr["Emp_Is_WhatsApp"]) == 1)
                    {
                        chkIsWhatsApp.IsChecked = true;
                    }
                    else
                    {
                        chkIsWhatsApp.IsChecked = false;
                    }
                    txtEmail.Text = dr["Emp_Email"].ToString();
                    //Add Salary
                    txtSalary.Text = dr["Emp_Salary"].ToString();
                    //Add Image
                    if (dr["Emp_Image"].ToString() != "")
                    {
                        byte[] imagedata = (byte[])dr["Emp_Image"];
                        BitmapImage image = new BitmapImage();
                        MemoryStream ms = new MemoryStream(imagedata);
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.EndInit();
                        Emp_Image.Source = image;
                    }
                    else
                    {
                        Emp_Image.Source = null;
                    }
                    //
                    BtnAdd.IsEnabled = false;
                    BtnAdd.Opacity = 0.5;
                    BtnDelete.IsEnabled = true;
                    BtnDelete.Opacity = 1;
                    BtnEdit.IsEnabled = true;
                    BtnEdit.Opacity = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Edit Button Click Function
        private void BtnEdit_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Emp_DtGrid.SelectedItem != null)
                {
                    if (IsValid())
                    {
                        string UpdateCommand = "UPDATE employee SET Emp_Name = @EmployeeName, Emp_Gender = @Gender, Emp_Address = @Address, Emp_Birth_Date = @BirthDate, Emp_Phone = @Phone, Emp_Is_WhatsApp = @IsWhatsApp, Emp_Email = @Email, Emp_Salary = @Salary , Emp_Image = @Image WHERE employee.Emp_Number = " + txtEmployeeNumber.Text + ";";

                        Cmd.Parameters.Clear();

                        //Dert Tag F Command -> @img <- bach ngol lih ila l9iti @img Hate blasteha Image As Bytes
                        Cmd.Parameters.AddWithValue("@EmployeeNumber", Convert.ToInt32(txtEmployeeNumber.Text));
                        Cmd.Parameters.AddWithValue("@EmployeeName", (txtEmployeeName.Text == "") ? (object)DBNull.Value : txtEmployeeName.Text);
                        Cmd.Parameters.AddWithValue("@Gender", (Male_Gndr.IsChecked == true) ? "Male" : "Female");
                        Cmd.Parameters.AddWithValue("@Address", (txtAddress.Text == "") ? (object)DBNull.Value : txtAddress.Text);
                        string Y = "";
                        string M = "";
                        string D = "";
                        if (dpBirthDate.Text != "")
                        {
                            Y = dpBirthDate.SelectedDate.Value.Year.ToString();
                            M = dpBirthDate.SelectedDate.Value.Month.ToString();
                            D = dpBirthDate.SelectedDate.Value.Day.ToString();
                            Cmd.Parameters.AddWithValue("@BirthDate", $"{Y}-{M}-{D}");
                        }
                        else
                        {
                            Cmd.Parameters.AddWithValue("@BirthDate", (object)DBNull.Value);
                        }
                        Cmd.Parameters.AddWithValue("@Phone", (txtPhone.Text == "") ? (object)DBNull.Value : txtPhone.Text);
                        Cmd.Parameters.AddWithValue("@IsWhatsApp", (chkIsWhatsApp.IsChecked == true) ? 1 : 0);
                        Cmd.Parameters.AddWithValue("@Email", (txtEmail.Text == "") ? (object)DBNull.Value : txtEmail.Text);
                        Cmd.Parameters.AddWithValue("@Salary", (txtSalary.Text == "") ? 0 : Convert.ToDouble(txtSalary.Text));

                        //To Save Image We Want to Convert from Image to Byte(010100010) 
                        //So i Create a Memory to Save From My Image tool to Ms
                        if (Emp_Image.Source != null)
                        {
                            MemoryStream Ms = new MemoryStream();
                            BitmapSource Bs = Emp_Image.Source as BitmapSource;
                            BitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(Bs));
                            encoder.Save(Ms);
                            Cmd.Parameters.AddWithValue("@Image", Ms.ToArray());

                        }
                        else
                        {
                            Cmd.Parameters.AddWithValue("@Image", (object)DBNull.Value);
                        }
                        //to not repeat code
                        //i creat a varible he scan if do code or not
                        bool do_it = true;

                        //if in settings Check Deletion Question
                        if (Q_Edit.IsChecked == true)
                        {
                            if (MessageBox.Show("Are you sure about this change?", "Program", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                do_it = false;
                            }
                        }
                        if (do_it)
                        {
                            //Do Command
                            SetCommand(UpdateCommand);
                            Cmd.ExecuteNonQuery();

                            if (N_Edit.IsChecked == true)
                            {
                                MessageBox.Show("Modified successfully!!", "Program", MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                            FillDatagrid();
                            ClearData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Delete Button Click Function
        private void BtnDelete_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Emp_DtGrid.SelectedItem != null)
                {
                    //to not repeat code
                    //i creat a varible he scan if do code or not
                    bool do_it = true;

                    //if in settings Check Deletion Question
                    if (Q_Delete.IsChecked == true)
                    {
                        //if press no
                        if (MessageBox.Show("Are you sure you deleted this employee?", "Program", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            do_it = false;
                        }
                    }

                    if (do_it)
                    {
                        string DeleteCommand = "DELETe FROM employee Where Emp_Number=" + txtEmployeeNumber.Text + ";";

                        SetCommand(DeleteCommand);
                        Cmd.ExecuteNonQuery();

                        FillDatagrid();
                        ClearData();

                        //if in settings Check Delete Notification
                        if (N_Delete.IsChecked == true)
                        {
                            MessageBox.Show("Deleted successfully!!", "Program", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //To Report Button Click Function
        private void BtnToReport_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Dg_Report.ItemsSource = Emp_DtGrid.SelectedItems;
                //Move to Report Grid
                HiddenAll();
                ReportGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Hado bach ikoun smouth Serching 
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetInfo(Searching(), Emp_DtGrid);
        }

        private void Srch_BirthDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetInfo(Searching(), Emp_DtGrid);
        }

        private void NameRb_Checked(object sender, RoutedEventArgs e)
        {
            if (NameRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void NameRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (NameRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void PhoneRb_Checked(object sender, RoutedEventArgs e)
        {
            if (PhoneRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void PhoneRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PhoneRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void AddressRb_Checked(object sender, RoutedEventArgs e)
        {
            if (AddressRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void AddressRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AddressRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void EmailRb_Checked(object sender, RoutedEventArgs e)
        {
            if (EmailRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void EmailRb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (EmailRb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void MaleCb_Checked(object sender, RoutedEventArgs e)
        {
            if (MaleCb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void MaleCb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MaleCb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void FemaleCb_Checked(object sender, RoutedEventArgs e)
        {
            if (FemaleCb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void FemaleCb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (FemaleCb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void WhatsAppCb_Click(object sender, RoutedEventArgs e)
        {
            if (WhatsAppCb.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Equals_Checked(object sender, RoutedEventArgs e)
        {
            if (Rb_Equals.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Equals_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Rb_Equals.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Younger_Click(object sender, RoutedEventArgs e)
        {
            if (Rb_Younger.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Younger_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Rb_Younger.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Older_Click(object sender, RoutedEventArgs e)
        {
            if (Rb_Older.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_Older_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Rb_Older.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_NotEquals_Click(object sender, RoutedEventArgs e)
        {
            if (Rb_NotEquals.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        private void Rb_NotEquals_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Rb_NotEquals.IsLoaded == true)
            {
                SetInfo(Searching(), Emp_DtGrid);
            }
        }

        //Is WhatsApp CheckBox Loaded Function
        private void WhatsAppCb_Loaded(object sender, RoutedEventArgs e)
        {
            //Bach Itla3 li 3adhom WhastApp O li ma3adhomch
            WhatsAppCb.IsChecked = null;
        }

        //Backup Methods
        private void Btn_TakeBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                //if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //{
                //    MySqlBackup msb = new MySqlBackup(Cmd);
                //    DateTime dt = DateTime.Now;
                //    string strname = $"Employee_{dt.Year}_{dt.Month}_{dt.Day}_{dt.Hour}_{dt.Minute}_{dt.Second}.sql";
                //    msb.ExportToFile(fbd.SelectedPath + "\\" + strname);

                //    StreamWriter sw = new StreamWriter("Backup Actions.txt", true);
                //    sw.WriteLine($"+Take Backup | {DateTime.Now.ToString()} | {fbd.SelectedPath + "\\" + strname}");
                //    sw.Close();
                //    GetBackupAction();

                //    MessageBox.Show("The backup was completed successfully!!", "Program", MessageBoxButton.OK, MessageBoxImage.Information);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (MessageBox.Show("Are you sure to restore a backup copy?", "Program", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //{
                //    OpenFileDialog ofd = new OpenFileDialog();
                //    ofd.Filter = "Sql Files|*.sql;";
                //    bool? result = ofd.ShowDialog();
                //    if (result == true)
                //    {
                //        MySqlBackup msb = new MySqlBackup(Cmd);
                //        msb.ImportFromFile(ofd.FileName);
                //        ClearData();
                //        FillDatagrid();

                //        StreamWriter sw = new StreamWriter("Backup Actions.txt", true);
                //        sw.WriteLine($"+Restor Backup | {DateTime.Now.ToString()} | {ofd.FileName}");
                //        sw.Close();
                //        GetBackupAction();

                //        MessageBox.Show("The backup was completed successfully!!", "Program", MessageBoxButton.OK, MessageBoxImage.Information);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        //Move betwen Pages
        private void AnimateVisibility(UIElement element, Visibility visibility, double durationSeconds)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = (visibility == Visibility.Visible) ? 0 : 1,
                To = (visibility == Visibility.Visible) ? 1 : 0,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            if (visibility == Visibility.Visible)
            {
                element.Visibility = Visibility.Visible;
            }
            else
            {
                // After the animation completes, set the visibility to Hidden
                animation.Completed += (sender, args) => element.Visibility = Visibility.Hidden;
            }
        }

        private void BtnListPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ListGrid.Visibility != Visibility.Visible)
            {
                HiddenAll();
                AnimateVisibility(ListGrid, Visibility.Visible, 0.5);
            }
        }

        private void BtnReportPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ReportGrid.Visibility != Visibility.Visible)
            {
                HiddenAll();
                AnimateVisibility(ReportGrid, Visibility.Visible, 0.5);
            }
        }

        private void BtnSettingsPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SettingsGrid.Visibility != Visibility.Visible)
            {
                HiddenAll();
                AnimateVisibility(SettingsGrid, Visibility.Visible, 0.5);
            }
        }


        private void BigReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportForm2 Rf = new ReportForm2();

                Rf.reportViewer2.Visible = false;
                Rf.reportViewer1.Visible = true;

                PageSettings ps = new PageSettings();
                ps.Landscape = true;
                ps.PaperSize.RawKind = (int)PaperKind.A4;
                Rf.reportViewer2.SetPageSettings(ps);

                ReportDataSource rds = new ReportDataSource("Employee2", Dg_Report.ItemsSource);
                Rf.reportViewer1.LocalReport.DataSources.Clear();
                Rf.reportViewer1.LocalReport.DataSources.Add(rds);
                Rf.reportViewer1.LocalReport.Refresh();

                Rf.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MiniReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportForm2 Rf = new ReportForm2();

                Rf.reportViewer2.Visible = true;
                Rf.reportViewer1.Visible = false;

                PageSettings ps = new PageSettings();
                ps.Landscape = true;
                ps.PaperSize.RawKind = (int)PaperKind.A4;
                Rf.reportViewer2.SetPageSettings(ps);

                ReportDataSource rds = new ReportDataSource("Employee1", Dg_Report.ItemsSource);
                Rf.reportViewer2.LocalReport.DataSources.Clear();
                Rf.reportViewer2.LocalReport.DataSources.Add(rds);
                Rf.reportViewer2.LocalReport.Refresh();

                Rf.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            DoubleAnimation animation = new DoubleAnimation(30, TimeSpan.FromSeconds(0.1));
            textBlock.BeginAnimation(TextBlock.FontSizeProperty, animation);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            DoubleAnimation animation = new DoubleAnimation(24, TimeSpan.FromSeconds(0.1));
            textBlock.BeginAnimation(TextBlock.FontSizeProperty, animation);
        }

        private void Programmer_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://linkr.bio/A7meed");
        }

    }
}
