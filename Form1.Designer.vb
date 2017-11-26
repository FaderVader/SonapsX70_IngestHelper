<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.CopyBegin = New System.Windows.Forms.Label()
        Me.CloseApplication = New System.Windows.Forms.Button()
        Me.SelectDrive = New System.Windows.Forms.Button()
        Me.BeginOperation = New System.Windows.Forms.Button()
        Me.SelectSource = New System.Windows.Forms.ComboBox()
        Me.ListDrives = New System.Windows.Forms.ListBox()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.destDisplay = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'CopyBegin
        '
        Me.CopyBegin.AutoSize = True
        Me.CopyBegin.Location = New System.Drawing.Point(17, 197)
        Me.CopyBegin.Name = "CopyBegin"
        Me.CopyBegin.Size = New System.Drawing.Size(99, 13)
        Me.CopyBegin.TabIndex = 11
        Me.CopyBegin.Text = "Copying to backup!"
        Me.CopyBegin.Visible = False
        '
        'CloseApplication
        '
        Me.CloseApplication.BackColor = System.Drawing.Color.Crimson
        Me.CloseApplication.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CloseApplication.ForeColor = System.Drawing.Color.White
        Me.CloseApplication.Location = New System.Drawing.Point(191, 222)
        Me.CloseApplication.Name = "CloseApplication"
        Me.CloseApplication.Size = New System.Drawing.Size(75, 23)
        Me.CloseApplication.TabIndex = 10
        Me.CloseApplication.Text = "Exit"
        Me.CloseApplication.UseVisualStyleBackColor = False
        '
        'SelectDrive
        '
        Me.SelectDrive.Enabled = False
        Me.SelectDrive.Location = New System.Drawing.Point(191, 157)
        Me.SelectDrive.Name = "SelectDrive"
        Me.SelectDrive.Size = New System.Drawing.Size(75, 23)
        Me.SelectDrive.TabIndex = 9
        Me.SelectDrive.Text = "START!"
        Me.SelectDrive.UseVisualStyleBackColor = True
        '
        'BeginOperation
        '
        Me.BeginOperation.Location = New System.Drawing.Point(191, 17)
        Me.BeginOperation.Name = "BeginOperation"
        Me.BeginOperation.Size = New System.Drawing.Size(75, 37)
        Me.BeginOperation.TabIndex = 8
        Me.BeginOperation.Text = "Refresh Drives"
        Me.BeginOperation.UseVisualStyleBackColor = True
        '
        'SelectSource
        '
        Me.SelectSource.FormattingEnabled = True
        Me.SelectSource.Location = New System.Drawing.Point(20, 157)
        Me.SelectSource.Name = "SelectSource"
        Me.SelectSource.Size = New System.Drawing.Size(153, 21)
        Me.SelectSource.TabIndex = 7
        Me.SelectSource.Text = "Choose souce-drive"
        '
        'ListDrives
        '
        Me.ListDrives.FormattingEnabled = True
        Me.ListDrives.Location = New System.Drawing.Point(19, 17)
        Me.ListDrives.Name = "ListDrives"
        Me.ListDrives.Size = New System.Drawing.Size(154, 134)
        Me.ListDrives.TabIndex = 6
        '
        'BackgroundWorker1
        '
        '
        'destDisplay
        '
        Me.destDisplay.AutoSize = True
        Me.destDisplay.Location = New System.Drawing.Point(17, 227)
        Me.destDisplay.Name = "destDisplay"
        Me.destDisplay.Size = New System.Drawing.Size(94, 13)
        Me.destDisplay.TabIndex = 12
        Me.destDisplay.Text = "backupdestination"
        Me.destDisplay.Visible = False
        '
        'Form1
        '
        Me.AcceptButton = Me.BeginOperation
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.CloseApplication
        Me.ClientSize = New System.Drawing.Size(280, 257)
        Me.Controls.Add(Me.destDisplay)
        Me.Controls.Add(Me.CopyBegin)
        Me.Controls.Add(Me.CloseApplication)
        Me.Controls.Add(Me.SelectDrive)
        Me.Controls.Add(Me.BeginOperation)
        Me.Controls.Add(Me.SelectSource)
        Me.Controls.Add(Me.ListDrives)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SD Copy & Move 1.0"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents CopyBegin As Label
    Friend WithEvents CloseApplication As Button
    Friend WithEvents SelectDrive As Button
    Friend WithEvents BeginOperation As Button
    Friend WithEvents SelectSource As ComboBox
    Friend WithEvents ListDrives As ListBox
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents destDisplay As Label
End Class
