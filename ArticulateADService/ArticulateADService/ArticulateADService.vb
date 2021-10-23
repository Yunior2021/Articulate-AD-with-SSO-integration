Imports ArticulateADService.ArticulateUsers
Public Class ArticulateAD
    Dim tmr As Timers.Timer
    Dim tmr2 As Timers.Timer
    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
        Me.EventLog.WriteEntry("ArticulateADService is started at. " + Date.Now)
        'ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf ServiceWorkerThread))
        tmr = New Timers.Timer()
        tmr.Interval = 300000
        AddHandler tmr.Elapsed, AddressOf MyTickhandler
        tmr.Enabled = True
        tmr2 = New Timers.Timer()
        tmr2.Interval = 480000
        AddHandler tmr2.Elapsed, AddressOf MyTickhandler2
        tmr2.Enabled = True
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        Me.EventLog.WriteEntry("ArticulateADService is Stopped. " + Date.Now)

        tmr.Enabled = False

    End Sub

    Private Sub MyTickhandler(obj As Object, e As EventArgs)
        ListADGroupMembers()
    End Sub
    Private Sub MyTickhandler2(obj As Object, e As EventArgs)
        Cleaner()
    End Sub
End Class
