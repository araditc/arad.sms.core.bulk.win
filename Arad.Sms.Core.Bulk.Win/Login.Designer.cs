
namespace Arad.Sms.Core.Bulk.Win
{
    partial class Login
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            txtUserName = new System.Windows.Forms.TextBox();
            txtPassword = new System.Windows.Forms.TextBox();
            btnLogin = new System.Windows.Forms.Button();
            btnExit = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            errorProvider1 = new System.Windows.Forms.ErrorProvider(components);
            cmbDomain = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(466, 324);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(99, 22);
            label1.TabIndex = 0;
            label1.Text = "نام کاربری :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(466, 378);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(81, 22);
            label2.TabIndex = 1;
            label2.Text = "گذر واژه :";
            // 
            // txtUserName
            // 
            txtUserName.Location = new System.Drawing.Point(18, 316);
            txtUserName.Margin = new System.Windows.Forms.Padding(4);
            txtUserName.Name = "txtUserName";
            txtUserName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            txtUserName.Size = new System.Drawing.Size(438, 29);
            txtUserName.TabIndex = 1;
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(18, 370);
            txtPassword.Margin = new System.Windows.Forms.Padding(4);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '#';
            txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            txtPassword.Size = new System.Drawing.Size(438, 29);
            txtPassword.TabIndex = 2;
            // 
            // btnLogin
            // 
            btnLogin.Location = new System.Drawing.Point(206, 477);
            btnLogin.Margin = new System.Windows.Forms.Padding(4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(150, 42);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "ورود";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new System.Drawing.Point(45, 477);
            btnExit.Margin = new System.Windows.Forms.Padding(4);
            btnExit.Name = "btnExit";
            btnExit.Size = new System.Drawing.Size(152, 42);
            btnExit.TabIndex = 3;
            btnExit.Text = "خروج";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.logod191b329_1d09_4fa2_b0e7_313879bd9a8b;
            pictureBox1.Location = new System.Drawing.Point(149, 13);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(264, 270);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            errorProvider1.RightToLeft = true;
            // 
            // cmbDomain
            // 
            cmbDomain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDomain.FormattingEnabled = true;
            cmbDomain.Items.AddRange(new object[] { "https://sms.jiring.ir:8085", "http://localhost:5000" });
            cmbDomain.Location = new System.Drawing.Point(18, 424);
            cmbDomain.Margin = new System.Windows.Forms.Padding(4);
            cmbDomain.Name = "cmbDomain";
            cmbDomain.RightToLeft = System.Windows.Forms.RightToLeft.No;
            cmbDomain.Size = new System.Drawing.Size(438, 30);
            cmbDomain.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(466, 432);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(59, 22);
            label3.TabIndex = 7;
            label3.Text = "دامنه :";
            // 
            // Login
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(576, 542);
            Controls.Add(btnExit);
            Controls.Add(btnLogin);
            Controls.Add(label3);
            Controls.Add(cmbDomain);
            Controls.Add(pictureBox1);
            Controls.Add(txtPassword);
            Controls.Add(txtUserName);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Login";
            RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ورود";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbDomain;
    }
}

