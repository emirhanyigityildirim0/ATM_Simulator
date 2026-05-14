using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;

namespace ATMSimulator_1
{
    public partial class CultureBank : Form
    {
        SqlConnection conn;
        private string currentAccount = "";
        private string receiverAccount = "";
        private bool isTransferMode = false;
        private bool isWaitingForPassword = false;
        private bool isWaitingToAddNewUser = false;
        private bool isWaitingForReceiver = false;
        private bool isDepositMode = false;
        private bool isWithdrawMode = false;
        private TextBox currentInputBox = null;

        public CultureBank()
        {
            InitializeComponent();
            textBox1.KeyPress += SuppressTyping;
            textBoxUsername.KeyPress += SuppressTyping;
            textBoxPassword.KeyPress += SuppressTyping;
            textBoxTransferAmount.KeyPress += SuppressTyping;
            textBoxReceiverAccount.KeyPress += SuppressTyping;
            textBoxDeposit.KeyPress += SuppressTyping;
            textBoxWithdraw.KeyPress += SuppressTyping;

            textBox1.KeyDown += AllowBackspace;
            textBoxUsername.KeyDown += AllowBackspace;
            textBoxPassword.KeyDown += AllowBackspace;
            textBoxTransferAmount.KeyDown += AllowBackspace;
            textBoxReceiverAccount.KeyDown += AllowBackspace;
            textBoxDeposit.KeyDown += AllowBackspace;
            textBoxWithdraw.KeyDown += AllowBackspace;

            textBox1.Enter += (s, e) => currentInputBox = textBox1;
            textBoxUsername.Enter += (s, e) => currentInputBox = textBoxUsername;
            textBoxPassword.Enter += (s, e) => currentInputBox = textBoxPassword;
            string connectionString = @"Server=.; Database=CultureBankDB; Integrated Security=True; TrustServerCertificate=True;";
            conn = new SqlConnection(connectionString);

            try
            {
                conn.Open();
                MessageBox.Show("✅ Connection successful!");
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Connection failed: " + ex.Message);
            }
            currentInputBox = textBox1;
            button10.Enabled = false;
            textBox1.Focus();
        }

        private void AllowBackspace(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && currentInputBox != null && currentInputBox.Text.Length > 0)
            {
                currentInputBox.Text = currentInputBox.Text.Substring(0, currentInputBox.Text.Length - 1);
                currentInputBox.SelectionStart = currentInputBox.Text.Length;
                currentInputBox.SelectionLength = 0;
                e.SuppressKeyPress = true;
            }
        }
        private void SuppressTyping(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void NumberButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            TextBox activeBox = currentInputBox;

            if (activeBox != null)
            {
                activeBox.Text += btn.Text;

                activeBox.SelectionStart = activeBox.Text.Length;
                activeBox.SelectionLength = 0;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.NumPad0:
                    button0.PerformClick();
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    button1.PerformClick();
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    button2.PerformClick();
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    button3.PerformClick();
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    button4.PerformClick();
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    button5.PerformClick();
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    button6.PerformClick();
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    button7.PerformClick();
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    button8.PerformClick();
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    button9.PerformClick();
                    break;
            }
        }

        private void LogHistory(string operationType, decimal amount)
        {
            try
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                conn.Open();

                string insertHistory = "INSERT INTO History (ClientNo, Operation, Amount) VALUES (@clientNo, @op, @amt)";
                SqlCommand cmd = new SqlCommand(insertHistory, conn);

                cmd.Parameters.Add("@clientNo", SqlDbType.NVarChar).Value = currentAccount;
                cmd.Parameters.Add("@op", SqlDbType.NVarChar).Value = operationType;
                cmd.Parameters.Add("@amt", SqlDbType.Decimal).Value = amount;

                // LogTime is handled by GETDATE() in SQL Server, so no need to add it here!

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to log history: {ex.Message}");
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        private void CheckButton_Click(object sender, EventArgs e)
        {
            if (isWithdrawMode)
            {
                if (!decimal.TryParse(textBoxWithdraw.Text.Trim(), out decimal amountToWithdraw))
                {
                    MessageBox.Show("Please enter a valid amount to withdraw.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();

                    // 1. Fetch current balance using named parameters
                    string query = "SELECT Funds FROM Clients WHERE ClientNo = @clientNo";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@clientNo", currentAccount);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        decimal currentFunds = Convert.ToDecimal(result);
                        if (amountToWithdraw > currentFunds)
                        {
                            MessageBox.Show("Insufficient funds.");
                        }
                        else
                        {
                            decimal newFunds = currentFunds - amountToWithdraw;

                            // 2. Update balance using SQL Server syntax
                            string updateQuery = "UPDATE Clients SET Funds = @newFunds WHERE ClientNo = @clientNo";
                            SqlCommand updateCmd = new SqlCommand(updateQuery, conn);

                            // Use specific names to ensure the values go to the right columns
                            updateCmd.Parameters.AddWithValue("@newFunds", newFunds);
                            updateCmd.Parameters.AddWithValue("@clientNo", currentAccount);

                            updateCmd.ExecuteNonQuery();

                            MessageBox.Show($"Successfully withdrew ${amountToWithdraw:F2}.");
                            LogHistory("Withdraw", amountToWithdraw); // This calls the history log we fixed earlier
                            ResetToInitialState();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Withdraw error: {ex.Message}");
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            if (isDepositMode)
            {
                if (!decimal.TryParse(textBoxDeposit.Text.Trim(), out decimal amountToDeposit))
                {
                    MessageBox.Show("Please enter a valid deposit amount.");
                    return;
                }

                if (amountToDeposit > 2000)
                {
                    MessageBox.Show("Deposit limit exceeded. You can only deposit up to $2000 at once.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();

                    // 1. Fetch current balance using ClientNo and @parameters
                    string selectQuery = "SELECT Funds FROM Clients WHERE ClientNo = @clientNo";
                    SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                    selectCmd.Parameters.AddWithValue("@clientNo", currentAccount);
                    object result = selectCmd.ExecuteScalar();

                    if (result != null)
                    {
                        decimal currentFunds = Convert.ToDecimal(result);
                        decimal newFunds = currentFunds + amountToDeposit;

                        // 2. Update balance using SQL Server named parameters
                        string updateQuery = "UPDATE Clients SET Funds = @newFunds WHERE ClientNo = @clientNo";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@newFunds", newFunds);
                        updateCmd.Parameters.AddWithValue("@clientNo", currentAccount);
                        updateCmd.ExecuteNonQuery();

                        MessageBox.Show($"Successfully deposited ${amountToDeposit:F2}.");
                        LogHistory("Deposit", amountToDeposit);
                        ResetToInitialState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Deposit error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            if (isTransferMode && isWaitingForReceiver)
            {
                string receiverAccount = textBoxReceiverAccount.Text.Trim();
                if (string.IsNullOrEmpty(receiverAccount))
                {
                    MessageBox.Show("Please enter the receiver's account number.");
                    return;
                }
                if (!decimal.TryParse(textBoxTransferAmount.Text.Trim(), out decimal amountToTransfer))
                {
                    MessageBox.Show("Invalid amount.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();

                    // 1. Check if the receiver exists
                    string checkReceiverQuery = "SELECT Funds FROM Clients WHERE ClientNo = @receiverAcc";
                    SqlCommand checkedReceiverCmd = new SqlCommand(checkReceiverQuery, conn);
                    checkedReceiverCmd.Parameters.AddWithValue("@receiverAcc", receiverAccount);
                    object receiverFundsObj = checkedReceiverCmd.ExecuteScalar();

                    if (receiverFundsObj == null)
                    {
                        MessageBox.Show("Receiver account not found.");
                        return;
                    }

                    // 2. Get the sender's current balance
                    string getSenderQuery = "SELECT Funds FROM Clients WHERE ClientNo = @senderAcc";
                    SqlCommand getSenderCmd = new SqlCommand(getSenderQuery, conn);
                    getSenderCmd.Parameters.AddWithValue("@senderAcc", currentAccount);
                    decimal senderFunds = Convert.ToDecimal(getSenderCmd.ExecuteScalar());

                    if (amountToTransfer > senderFunds)
                    {
                        MessageBox.Show("Insufficient funds.");
                        return;
                    }

                    // Calculate new balances
                    decimal newSenderFunds = senderFunds - amountToTransfer;
                    decimal receiverFunds = Convert.ToDecimal(receiverFundsObj);
                    decimal newReceiverFunds = receiverFunds + amountToTransfer;

                    // 3. Update Sender's balance
                    string updateSender = "UPDATE Clients SET Funds = @newSenderFunds WHERE ClientNo = @senderAcc";
                    SqlCommand updateSenderCmd = new SqlCommand(updateSender, conn);
                    updateSenderCmd.Parameters.AddWithValue("@newSenderFunds", newSenderFunds);
                    updateSenderCmd.Parameters.AddWithValue("@senderAcc", currentAccount);
                    updateSenderCmd.ExecuteNonQuery();

                    // 4. Update Receiver's balance
                    string updateReceiver = "UPDATE Clients SET Funds = @newReceiverFunds WHERE ClientNo = @receiverAcc";
                    SqlCommand updateReceiverCmd = new SqlCommand(updateReceiver, conn);
                    updateReceiverCmd.Parameters.AddWithValue("@newReceiverFunds", newReceiverFunds);
                    updateReceiverCmd.Parameters.AddWithValue("@receiverAcc", receiverAccount);
                    updateReceiverCmd.ExecuteNonQuery();

                    MessageBox.Show($"Successfully transfered ${amountToTransfer:F2} to account {receiverAccount}");

                    // Log for the sender
                    LogHistory($"Transfer to {receiverAccount}", amountToTransfer);

                    // Log for the receiver
                    string originalSender = currentAccount;
                    currentAccount = receiverAccount;
                    LogHistory($"Received from {originalSender}", amountToTransfer);
                    currentAccount = originalSender;

                    ResetToInitialState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Transfer error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            else if (isTransferMode && !isWaitingForReceiver)
            {
                if (!decimal.TryParse(textBoxTransferAmount.Text.Trim(), out decimal amountToTransfer))
                {
                    MessageBox.Show("Please enter a valid amount.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();

                    conn.Open();

                    string query = "SELECT Funds FROM Clients WHERE ClientNo = @clientNo";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@clientNo", currentAccount);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        decimal currentFunds = Convert.ToDecimal(result);
                        if (amountToTransfer > currentFunds)
                        {
                            MessageBox.Show("Insufficient funds.");
                        }
                        else
                        {
                            MessageBox.Show("Enough funds to transfer.");

                            isWaitingForReceiver = true;
                            textBoxReceiverAccount.Visible = true;
                            labelReceiverAccount.Visible = true;
                            howMuchLabel.Visible = false;
                            textBoxTransferAmount.Visible = false;
                            textBoxReceiverAccount.Clear();
                            textBoxReceiverAccount.Focus();
                            currentInputBox = textBoxReceiverAccount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Transfer error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            if (isWaitingToAddNewUser)
            {
                string newUser = textBoxUsername.Text.Trim();
                string newPass = textBoxPassword.Text.Trim();

                if (string.IsNullOrEmpty(newUser) || string.IsNullOrEmpty(newPass))
                {
                    MessageBox.Show("Please enter both username and password.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();

                    // 1. Check if the account already exists
                    // Using ClientNo and @parameters for SQL Server
                    string checkQuery = "SELECT COUNT(*) FROM Clients WHERE ClientNo = @newUser";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@newUser", newUser);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        MessageBox.Show("Account already exists.");
                        return;
                    }

                    // 2. Insert the new user
                    string insertQuery = "INSERT INTO Clients (ClientNo, Funds, Password) VALUES (@clientNo, 0, @pass)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);

                    insertCmd.Parameters.AddWithValue("@clientNo", newUser);
                    insertCmd.Parameters.AddWithValue("@pass", newPass);

                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("User successfully registered");

                    ResetToInitialState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            if (!isWaitingForPassword)
            {
                string inputAccount = textBox1.Text.Trim();

                if (string.IsNullOrEmpty(inputAccount))
                {
                    MessageBox.Show("Please enter an account number.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close(); // Good practice to check state first
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM Clients WHERE ClientNo = @acc";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@acc", inputAccount);

                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Account exists.");
                        currentAccount = inputAccount;
                        isWaitingForPassword = true;

                        textBox1.Visible = false;
                        label1.Visible = false;

                        textBoxPassword.Visible = true;
                        labelPassword.Visible = true;
                        textBoxPassword.Clear();
                        textBoxPassword.Focus();
                        currentInputBox = textBoxPassword; // Set the focus for the UI
                    }
                    else
                    {
                        MessageBox.Show("Account not found");
                        textBox1.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
                return;
            }
            else
            {
                string pass = textBoxPassword.Text.Trim();
                if (string.IsNullOrEmpty(pass))
                {
                    MessageBox.Show("Please enter your password.");
                    return;
                }

                try
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Open();

                    // Updated for SQL Server: no brackets needed for ClientNo, using @acc parameter
                    string query = "SELECT Password FROM Clients WHERE ClientNo = @acc";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    // Named parameter for SQL Server
                    cmd.Parameters.AddWithValue("@acc", currentAccount);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result.ToString() == pass)
                    {
                        MessageBox.Show("Login successful. Welcome!");
                        ShowPostLoginOptions();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Incorrect password.");
                        textBoxPassword.Clear();
                        textBoxPassword.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                }
            }
        }

        private void ShowHistory()
        {
            try
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                conn.Open();

                // 1. Updated Table/Column names and parameter syntax (@acc)
                string query = "SELECT Operation, Amount, LogTime FROM History WHERE ClientNo = @acc ORDER BY LogTime DESC";
                SqlCommand cmd = new SqlCommand(query, conn);

                // 2. Named parameter for SQL Server
                cmd.Parameters.AddWithValue("@acc", currentAccount);

                SqlDataReader reader = cmd.ExecuteReader();
                StringBuilder sb = new StringBuilder();

                // Add a header for better readability
                sb.AppendLine($"{"Date",-20} | {"Operation",-15} | {"Amount"}");
                sb.AppendLine(new string('-', 50));

                while (reader.Read())
                {
                    // 3. LogTime formatting for a cleaner look
                    string logDate = reader["LogTime"].ToString();
                    sb.AppendLine($"{logDate,-20} | {reader["Operation"],-15} | {reader["Amount"]}$");
                }

                if (sb.Length < 60) // Check if only header exists
                {
                    MessageBox.Show("No history found for this account.");
                }
                else
                {
                    MessageBox.Show(sb.ToString(), "Transaction History");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}");
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        private void ShowPostLoginOptions()
        {
            buttonDeposit.Visible = true;
            buttonWithdraw.Visible = true;
            buttonTransfer.Visible = true;
            buttonHistory.Visible = true;
            button10.Enabled = true;
            button13.Enabled = false;

            textBoxPassword.Visible = false;
            labelPassword.Visible = false;

            label1.Visible = false;
            textBox1.Visible = false;

            textBoxUsername.Visible = false;
            labelUsername.Visible = false;

            addButton.Enabled = false;
            CheckButton.Enabled = false;

            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn.Open();

                string query = "SELECT Funds FROM Clients WHERE ClientNo = @acc";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@acc", currentAccount);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    decimal balance = Convert.ToDecimal(result);
                    // Display the balance with 2 decimal places
                    labelBalance.Text = $"Balance: ${balance:F2}";
                    labelBalance.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load balance. " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        private void ResetToInitialState()
        {
            currentAccount = "";
            receiverAccount = "";
            isWaitingForPassword = false;
            isWaitingToAddNewUser = false;
            isWaitingForReceiver = false;
            isTransferMode = false;
            isWithdrawMode = false;
            isDepositMode = false;

            textBox1.Clear();
            textBoxUsername.Clear();
            textBoxPassword.Clear();

            textBox1.Visible = true;
            label1.Visible = true;
            CheckButton.Visible = true;
            addButton.Enabled = true;
            button10.Enabled = false;

            textBoxUsername.Visible = false;
            textBoxPassword.Visible = false;
            labelUsername.Visible = false;
            labelPassword.Visible = false;
            labelBalance.Visible = false;

            buttonDeposit.Visible = false;
            buttonWithdraw.Visible = false;
            buttonTransfer.Visible = false;
            buttonHistory.Visible = false;

            labelWithdraw.Visible = false;
            textBoxWithdraw.Visible = false;

            howMuchLabel.Visible = false;
            labelReceiverAccount.Visible = false;
            textBoxReceiverAccount.Visible = false;

            textBoxTransferAmount.Visible = false;

            labelDeposit.Visible = false;
            textBoxDeposit.Visible = false;
            isDepositMode = false;

            CheckButton.Enabled = true;
            textBox1.Focus();
        }

        private void buttonBackspace_Click(object sender, EventArgs e)
        {
            if (currentInputBox != null && currentInputBox.Text.Length > 0)
            {
                currentInputBox.Text = currentInputBox.Text.Substring(0, currentInputBox.Text.Length - 1);
                currentInputBox.SelectionStart = currentInputBox.Text.Length;
                currentInputBox.SelectionLength = 0;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to go back to the entrance?",
                                                    "Confirm Exit",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ResetToInitialState();
            }
            else
            {
                return;
            }
        }

        private void addAccount_Click(object sender, EventArgs e)
        {
            isWaitingToAddNewUser = true;

            textBox1.Clear();
            addButton.Enabled = false;
            textBox1.Visible = false;
            textBoxUsername.Visible = true;
            textBoxPassword.Visible = true;
            labelUsername.Visible = true;
            labelPassword.Visible = true;
            label1.Visible = false;

            textBoxUsername.Clear();
            textBoxPassword.Clear();
            currentInputBox = textBoxPassword;
            textBoxUsername.Focus();

        }

        private void buttonTransfer_Click(object sender, EventArgs e)
        {
            buttonTransfer.Visible = false;
            labelBalance.Visible = false;
            buttonDeposit.Visible = false;
            buttonWithdraw.Visible = false;
            buttonHistory.Visible = false;
            button13.Enabled = true;
            howMuchLabel.Visible = true;
            textBoxTransferAmount.Visible = true;
            CheckButton.Enabled = true;
            currentInputBox = textBoxTransferAmount;
            textBoxTransferAmount.Clear();
            textBoxTransferAmount.Focus();
            isTransferMode = true;
        }

        private void buttonDeposit_Click(object sender, EventArgs e)
        {
            buttonTransfer.Visible = false;
            labelBalance.Visible = false;
            buttonDeposit.Visible = false;
            buttonWithdraw.Visible = false;
            buttonHistory.Visible = false;
            button13.Enabled = true;
            labelDeposit.Visible = true;
            textBoxDeposit.Visible = true;

            CheckButton.Enabled = true;
            currentInputBox = textBoxDeposit;
            textBoxDeposit.Clear();
            textBoxDeposit.Focus();
            isDepositMode = true;
        }

        private void buttonWithdraw_Click(object sender, EventArgs e)
        {
            buttonTransfer.Visible = false;
            labelBalance.Visible = false;
            buttonDeposit.Visible = false;
            buttonWithdraw.Visible = false;
            buttonHistory.Visible = false;
            button13.Enabled = true;

            labelWithdraw.Visible = true;
            textBoxWithdraw.Visible = true;

            CheckButton.Enabled = true;
            currentInputBox = textBoxWithdraw;
            textBoxWithdraw.Clear();
            textBoxWithdraw.Focus();
            isWithdrawMode = true;

        }

        private void buttonHistory_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        private void textBoxDeposit_TextChanged(object sender, EventArgs e)
        {

        }

        private void CultureBank_Load(object sender, EventArgs e)
        {

        }
    }
}