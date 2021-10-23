Imports System.Data.SqlClient
Imports ArticulateSSO.Articulate
Imports ArticulateSSO.SQL
Public Class Index
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim cmd As SqlCommand
        Dim sql_result As SqlDataReader
        Dim dad As SqlDataAdapter
        Dim dtb As New DataTable

        Using connection As SqlConnection = New SqlConnection(GetConnectionString)
            connection.Open()

            Dim rquery As String = "SELECT Email,Password,CustomerID FROM Users WHERE Username='" & Environment.UserName.ToString & "'"
            cmd = New SqlCommand(rquery, connection)
            sql_result = cmd.ExecuteReader()

            dad = New SqlDataAdapter(rquery, connection)

            If sql_result.HasRows Then

                dad.Fill(dtb)

                Dim ao As Articulate.ArticulateOnline = New Articulate.ArticulateOnline()
                ao.Url = "https://communitygrp.articulate-online.com/services/api/1.0/articulateonline.asmx"

                Dim request As AutoLoginRequest = New AutoLoginRequest()

                Dim c As Articulate.Credentials = New Articulate.Credentials()
                c.EmailAddress = dtb.Rows(0).Item("Email").ToString
                c.Password = dtb.Rows(0).Item("Password").ToString
                c.CustomerID = dtb.Rows(0).Item("CustomerID").ToString

                request.Credentials = c
                request.Url = "https://communitygrp.articulate-online.com/UserPortal/Content.aspx?Cust=" & dtb.Rows(0).Item("CustomerID").ToString

                Dim url As String = ao.GetAutoLoginUrl(request).Url


                'label1.Text = url
                'label1.Text = url.ToString
                Response.Redirect(url.ToString)
            Else
                ' User doesn't exits in articulate.
                label1.Text = "Account not found, please contact your Systems administrator."
            End If
            sql_result.Close()
            dtb.Dispose()
            connection.Close()
        End Using

    End Sub

End Class