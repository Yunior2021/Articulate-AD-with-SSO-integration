Imports System.Configuration.Install

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()


        'Service process installer
        Me.ServiceProcessInstaller1 = New System.ServiceProcess.ServiceProcessInstaller()
        'Service installer
        'Me.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        'Me.ServiceProcessInstaller1.Password = ""
        'Me.ServiceProcessInstaller1.Username = ""
        Me.ServiceInstaller1 = New System.ServiceProcess.ServiceInstaller With {
            .Description = " Articulate Active Directory Integretion Service",
            .DisplayName = "ArticulateADService",
            .ServiceName = "ArticulateAD"
        }

    End Sub

    Private Sub ServiceProcessInstaller1_AfterInstall(sender As Object, e As InstallEventArgs) Handles ServiceProcessInstaller1.AfterInstall

    End Sub

    Private Sub ServiceInstaller1_AfterInstall(sender As Object, e As InstallEventArgs) Handles ServiceInstaller1.AfterInstall

    End Sub
End Class
